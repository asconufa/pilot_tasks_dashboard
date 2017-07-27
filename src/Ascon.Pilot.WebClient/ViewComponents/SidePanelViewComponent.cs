using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    /// <summary>
    /// Компонент - боковая панель.
    /// </summary>
    public class SidePanelViewComponent : ViewComponent
    {
        /// <summary>
        /// Вызов боковой панели.
        /// </summary>
        /// <param name="id">Id папки.</param>
        /// <returns>Боковая панель для папки с идентификатором Id.</returns>
        public async Task<IViewComponentResult> InvokeAsync(Guid? id)
        {
            return await Task.Run(() => GetSidePanel(id)) ;
        }


        /// <summary>
        /// Отображение боковой панели.
        /// </summary>
        /// <param name="id">Уникальный Id папки, для которой запрашивается представление.</param>
        /// <returns>Представление боковой панели для папки с идентификатором Id.</returns>
        public IViewComponentResult GetSidePanel(Guid? id)
        {
            id = id ?? DObject.RootId;

            var serverApi = HttpContext.GetServerApi();
            var rootObject = serverApi.GetObjects(new[] { id.Value }).First();
            var bRootObject = serverApi.GetObjects(new[] { DObject.RootId }).First();
            var mTypes = HttpContext.Session.GetMetatypes();
            var model = new SidePanelViewModel
            {
                ObjectId = id.Value,
                Types = mTypes,
                Items = new List<SidePanelItem>()
            };

            var prevId = rootObject.Id;
            var parentId = rootObject.Id;
            do
            {
                var parentObject = serverApi.GetObjects(new[] { parentId }).First();
                var parentChildsIds = parentObject.Children
                                        .Where(x => mTypes[x.TypeId].IsService == false)
                                        .Select(x => x.ObjectId).ToArray();
                if (parentChildsIds.Length != 0)
                {
                    var parentChilds = serverApi.GetObjects(parentChildsIds);
                    var subtree = model.Items;
                    model.Items = new List<SidePanelItem>(parentChilds.Count);
                    foreach (var parentChild in parentChilds)
                    {
                        model.Items.Add(new SidePanelItem
                        {
                            Type = mTypes[parentChild.TypeId],
                            DObject = parentChild,
                            SubItems = parentChild.Id == prevId ? subtree : null,
                            Selected = parentChild.Id == id
                        });
                    }
                }

                prevId = parentId;
                parentId = parentObject.ParentId;
            } while (parentId != bRootObject.ParentId);
            return View(model);
        }
    }
}
