using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models;
using Ascon.Pilot.WebClient.ViewComponents;
using Ascon.Pilot.WebClient.ViewModels;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

#if DNX451
using MuPDFLib;
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace Ascon.Pilot.WebClient.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private ILogger<FilesController> _logger;
        private IHostingEnvironment _environment;

        public FilesController(ILogger<FilesController> logger, IHostingEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult ChangeFilesPanelType(string returnUrl, FilesPanelType type)
        {
            HttpContext.Session.SetSessionValues(SessionKeys.FilesPanelType, type);
            return Redirect(returnUrl);
        }

        public IActionResult GetBreabcrumbs(Guid id)
        {
            return ViewComponent(typeof(BreadcrumbsViewComponent), id);
        }

        public IActionResult Index(Guid? id, bool isSource = false)
        {
            var model = new UserPositionViewModel();

                id = id ?? DObject.RootId;
                FilesPanelType type = HttpContext.Session.GetSessionValues<FilesPanelType>(SessionKeys.FilesPanelType);
                model.CurrentFolderId = id.Value;
                model.FilesPanelType = type;
                ViewBag.FilesPanelType = type;
                ViewBag.IsSource = isSource;

            return View(model);
        }
        
        public async Task<IActionResult> GetNodeChilds(Guid id)
        {
            return await Task.Run(() =>
            {
                dynamic[] childNodes;

                    var serverApi = HttpContext.GetServerApi();
                    var node = serverApi.GetObjects(new[] { id }).First();

                    var types = HttpContext.Session.GetMetatypes();

                    var childIds = node.Children?
                                        .Where(x => types[x.TypeId].Children?.Any() == true)
                                        .Select(child => child.ObjectId).ToArray();
                    var nodeChilds = serverApi.GetObjects(childIds);

                    childNodes = nodeChilds.Where(t =>
                    {
                        var mType = types[t.TypeId];
                        return !mType.IsService;

                    }).Select(x =>
                        {
                            var mType = types[x.TypeId];


                            var sidePanelItem = new SidePanelItem
                            {
                                DObject = x,
                                Type = mType,
                                SubItems = x.Children.Any(y => types[y.TypeId].Children.Any()) ? new List<SidePanelItem>() : null
                            };

                            return sidePanelItem.GetDynamic(id, types);
                        })
                        .ToArray();
                return Json(childNodes);
            });
        }

        public IActionResult SidePanel(Guid? id)
        {
            return ViewComponent(typeof (SidePanelViewComponent), id);
        }

        public IActionResult GetObject(Guid id, bool isSource = false)
        {
            var filesPanelType = HttpContext.Session.GetSessionValues<FilesPanelType>(SessionKeys.FilesPanelType);

            return ViewComponent(typeof(FilesPanelViewComponent), new { folderId = id, panelType = filesPanelType, onlySource = isSource });
        }

        public IActionResult GetSource(Guid id)
        {
            return GetObject(id, true);
        }

        public IActionResult Preview(Guid id, int size, string name)
        {
            ViewBag.Url = Url.Action("DownloadPdf", new { id, size, name });
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax) return PartialView();
            return View();
        }

        public IActionResult DownloadPdf(Guid id, int size, string name)
        {
            byte[] fileChunk;

                var serverApi = HttpContext.GetServerApi();
                fileChunk = serverApi.GetFileChunk(id, 0, size);
                var fileDownloadName = string.IsNullOrWhiteSpace(name) ? id.ToString() : name;
                if (Response.Headers.ContainsKey("Content-Disposition"))
                    Response.Headers.Remove("Content-Disposition");
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileDownloadName}");
            return new FileContentResult(fileChunk, "application/pdf");
        }

        public async Task<IActionResult> Download(Guid id, int size, string name)
        {
            return await Task.Run(() =>
            {
                {
                    var serverApi = HttpContext.GetServerApi();
                    var fileChunk = serverApi.GetFileChunk(id, 0, size);
                    return new FileContentResult(fileChunk, "application/octet-stream")
                    {
                        FileDownloadName = string.IsNullOrWhiteSpace(name) ? id.ToString() : name
                    };
                }
            });
        }

        public IActionResult DownloadArchive(Guid[] objectsIds)
        {
            if (objectsIds.Length == 0)
                return NotFound();
            byte[] mstData;

                var serverApi = HttpContext.GetServerApi();
                var objects = serverApi.GetObjects(objectsIds);

                var types = HttpContext.Session.GetMetatypes();

                using (var compressedFileStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, true))
                    {
                        AddObjectsToArchive(serverApi, objects, zipArchive, types, "");
                    }
                    mstData = compressedFileStream.ToArray();
                }
            return new FileContentResult(mstData, "application/zip") { FileDownloadName = "archive.zip" };
        }

        private void AddObjectsToArchive(IServerApi serverApi, List<DObject> objects, ZipArchive archive, IDictionary<int, MType> types, string currentPath)
        {
            foreach (var obj in objects)
            {
                if (!types[obj.TypeId].Children.Any())
                {
                    var dFile = obj.ActualFileSnapshot.Files.FirstOrDefault();
                    if (dFile == null)
                        continue;

                    var fileId = dFile.Body.Id;
                    var fileSize = dFile.Body.Size;
                    var fileBody = serverApi.GetFileChunk(fileId, 0, (int)fileSize);

                    if (archive.Entries.Any(x => x.Name == dFile.Name))
                        dFile.Name += " Conflicted";
                    var zipEntry = archive.CreateEntry(Path.Combine(currentPath, dFile.Name), CompressionLevel.NoCompression);

                    //Get the stream of the attachment
                    using (var originalFileStream = new MemoryStream(fileBody))
                    using (var zipEntryStream = zipEntry.Open())
                    {
                        //Copy the attachment stream to the zip entry stream
                        originalFileStream.CopyTo(zipEntryStream);
                    }
                }
                else
                {
                    var name = obj.GetTitle(types[obj.TypeId]);
                    var directoryPath = Path.Combine(currentPath, name);
                    var objChildrenIds = obj.Children.Select(x => x.ObjectId).ToArray();
                    if (!objChildrenIds.Any())
                        continue;

                    var objChildren = serverApi.GetObjects(objChildrenIds);
                    AddObjectsToArchive(serverApi, objChildren, archive, types, directoryPath);
                }
            }
        }

        public IActionResult Thumbnail(Guid id, int size, string extension, int typeId)
        {
            return RedirectToAction("GetTypeIcon", "Home", new { id = typeId });
        }

        public IActionResult Image(Guid id, int size, string extension)
        {
            const string pngContentType = "image/png";
            const string svgContentType = "image/svg+xml";
            var virtualFileResult = File(Url.Content("~/images/file.svg"), svgContentType);
#if DNX451
            if (size >= 10 * 1024 * 1024)
                return virtualFileResult;

            var serverApi = HttpContext.GetServerApi();
            var file = serverApi.GetFileChunk(id, 0, size);
            try
            {
                if (file != null)
                {
                    int page = 1;
                    int dpi = 150;
                    RenderType RenderType = RenderType.RGB;
                    bool rotateAuto = false;
                    string password = "";

                    var fileName = $"tmp/{id}{extension}";
                    using (var fileStream = System.IO.File.Create(fileName))
                        fileStream.Write(file, 0, file.Length);

                    byte[] thumbnailContent;
                    using (MuPDF pdfDoc = new MuPDF(fileName, password))
                    {
                        pdfDoc.Page = page;
                        Bitmap bm = pdfDoc.GetBitmap(0, 0, dpi, dpi, 0, RenderType, rotateAuto, false, 0);
                        HttpContext.Response.ContentType = pngContentType;
                        using (var ms = new MemoryStream())
                        {
                            bm.Save(ms, ImageFormat.Png);
                            thumbnailContent = ms.ToArray();
                        }
                    }

                    System.IO.File.Delete(fileName);

                    return File(thumbnailContent, pngContentType, $"{id}.png");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(1, "Unable to generate thumbnail for file", ex);
            }
#endif
            return virtualFileResult;
        }

        [HttpPost]
        public ActionResult Rename(Guid idToRename, string newName, Guid renameRootId)
        {

                var api = HttpContext.GetServerApi();
                var objectToRename = api.GetObjects(new[] { idToRename })[0];
                var newObject = objectToRename.Clone();

                /*api.Change(new DChangesetData()
                {
                    Changes = new List<DChange>
                    {
                        new DChange()
                        {
                            New = newObject,
                            Old = objectToRename
                        }
                    }
                });*/
            return RedirectToAction("Index", new {id = renameRootId });
        }

        [HttpPost]
        public ActionResult Remove(Guid idToRemove, Guid removeRootId)
        {
            return RedirectToAction("Index", new { id = removeRootId });
        }
        
        //[HttpPost]
        //public async Task<RedirectToActionResult> Upload(Guid folderId, IFormFile file)
        //{
        //    try
        //    {
        //        if (file.Length == 0)
        //            throw new ArgumentNullException(nameof(file));

        //        string fileName = file.GetFileName();
        //        var pathToSave = Path.Combine(_environment.WebRootPath, fileName);
        //        await file.SaveAsAsync(pathToSave);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning(1, "Unable to upload file", ex);
        //    }
        //    return RedirectToAction("Index", new { id = folderId });
        //}
    }
}