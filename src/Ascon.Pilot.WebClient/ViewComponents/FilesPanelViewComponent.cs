using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Controllers;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    /// <summary>
    /// Компнент - панель управления файлом.
    /// </summary>
    public class FilesPanelViewComponent : ViewComponent
    {
        private ILogger<FilesController> _logger;

        public FilesPanelViewComponent(ILogger<FilesController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Вызвать компонент панели файлов
        /// </summary>
        /// <param name="folderId">Идентификатор текущего каталога</param>
        /// <param name="panelType">Тип отображения панели</param>
        /// <param name="onlySource">Отображать только исходные файлы</param>
        /// <returns>Представление панели управения файлом для каталога с идентификатором Id и итпом отбражения Type.</returns>
        public async Task<IViewComponentResult> InvokeAsync(Guid folderId, FilesPanelType panelType, bool onlySource)
        {
            return await Task.Run(() =>
            {
                {
                    List<FileViewModel> model = new List<FileViewModel>();
                    try
                    {
                        var types = HttpContext.Session.GetMetatypes();
                        var serverApi = HttpContext.GetServerApi();
                        var folder = serverApi.GetObjects(new[] { folderId }).First();

                        if (folder.Children?.Any() != true)
                            return View(panelType == FilesPanelType.List ? "List" : "Grid", new FileViewModel[] { });

                        var childrenIds = folder.Children.Select(x => x.ObjectId).ToArray();
                        var childrens = serverApi.GetObjects(childrenIds);

                        var folderType = types[folder.TypeId];
                        if (folderType.IsMountable && !(ViewBag.IsSource ?? false))
                            model.Add(new FileViewModel
                            {
                                IsFolder = true,
                                Id = folder.Id,
                                ObjectId = folder.Id,
                                ObjectName = "Исходные файлы",
                                ObjectTypeName = "Папка с исходными файлами",
                                ObjectTypeId = ApplicationConst.SourcefolderTypeid,
                                LastModifiedDate = folder.Created,
                                ChildrenCount = folder.Children.Count(x => types[x.TypeId].IsProjectFileOrFolder())
                            });

                        if (onlySource)
                        {
                            FillModelWithSource(childrens, types, model);
                        }
                        else
                        {
                            FillModel(childrens, types, model);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw new Exception(ex.Message);
                    }
                    return View(panelType == FilesPanelType.List ? "List" : "Grid", model);
                }
            });
        }

        private static void FillModel(List<DObject> childrens, IDictionary<int, MType> types, List<FileViewModel> model)
        {
            var childrenList = childrens.Where(x =>
            {
                var type = types[x.TypeId];
                return !type.IsService;
            });

            foreach (var dObject in childrenList)
            {
                var mType = types[dObject.TypeId];
                if (mType.Children.Any())
                    model.Add(new FileViewModel
                    {
                        Id = dObject.Id,
                        IsFolder = true,
                        ObjectId = dObject.Id,
                        ObjectTypeId = mType.Id,
                        ObjectTypeName = mType.Name,
                        ObjectName = dObject.GetTitle(mType),
                        FileName = dObject.GetTitle(mType),
                        LastModifiedDate = dObject.Created,
                        ChildrenCount = dObject.Children.Count(x => !types[x.TypeId].IsProjectFileOrFolder()),
                        IsMountable = mType.IsMountable
                    });
                else if (dObject.ActualFileSnapshot?.Files?.Any() == true)
                {
                    var file = dObject.ActualFileSnapshot.Files.First();
                    model.Add(new FileViewModel
                    {
                        Id = file.Body.Id,
                        IsFolder = false,
                        ObjectId = dObject.Id,
                        ObjectTypeId = mType.Id,
                        ObjectTypeName = mType.Name,
                        ObjectName = dObject.GetTitle(mType),
                        FileName = file.Name,
                        Size = (int) file.Body.Size,
                        LastModifiedDate = file.Body.Modified
                    });
                }
                else
                {
                    model.Add(new FileViewModel
                    {
                        IsFolder = true,
                        Id = dObject.Id,
                        ObjectName = dObject.GetTitle(mType),
                        ChildrenCount = dObject.Children.Count(x => !types[x.TypeId].IsProjectFileOrFolder()),
                        ObjectId = dObject.Id,
                        ObjectTypeId = mType.Id,
                        ObjectTypeName = mType.Name
                    });
                }
            }
        }

        private static void FillModelWithSource(List<DObject> childrens, IDictionary<int, MType> types, List<FileViewModel> model)
        {
            var projectChilds = childrens.Where(x => types[x.TypeId].IsProjectFileOrFolder());
            foreach (var dObject in projectChilds)
            {
                var mType = types[dObject.TypeId];
                if (mType.IsProjectFolder())
                    model.Add(new FileViewModel
                    {
                        Id = dObject.Id,
                        IsFolder = true,
                        ObjectId = dObject.Id,
                        ObjectTypeId = mType.Id,
                        ObjectTypeName = mType.Name,
                        ObjectName = dObject.GetTitle(mType),
                        FileName = dObject.GetTitle(mType),
                        LastModifiedDate = dObject.Created,
                        ChildrenCount = dObject.Children.Count,
                        IsMountable = mType.IsMountable
                    });
                else if (mType.IsProjectFile())
                {
                    var file = dObject.ActualFileSnapshot.Files.First();
                    model.Add(new FileViewModel
                    {
                        Id = file.Body.Id,
                        IsFolder = false,
                        ObjectId = dObject.Id,
                        ObjectTypeId = mType.Id,
                        ObjectTypeName = mType.Name,
                        ObjectName = dObject.GetTitle(mType),
                        FileName = file.Name,
                        Size = (int)file.Body.Size,
                        LastModifiedDate = file.Body.Modified
                    });
                }
            }
        }
    }
}