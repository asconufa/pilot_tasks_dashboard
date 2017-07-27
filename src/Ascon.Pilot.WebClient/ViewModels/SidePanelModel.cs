using System;
using System.Collections.Generic;
using System.Linq;
using Ascon.Pilot.Core;

namespace Ascon.Pilot.WebClient.ViewModels
{
    /// <summary>
    /// Модель боковой панели
    /// </summary>
    public class SidePanelViewModel
    {
        /// <summary>
        /// Словарь с типами файлов кнопок
        /// </summary>
        public IDictionary<int, MType> Types { get; set; }
        /// <summary>
        /// Уникальный идентификатор объекта 
        /// </summary>
        public Guid ObjectId { get; set; }
        /// <summary>
        /// Список кнопок панели
        /// </summary>
        public List<SidePanelItem> Items { get; set; }
        
        /// <summary>
        /// Добавление кнопки в боковую панель.
        /// </summary>
        /// <returns>Массив кнопок боковой панели.</returns>
        public dynamic[] ToDynamic()
        {
            var items = new List<dynamic>(Items.Count);
            foreach (var sidePanelItem in Items)
            {
                items.Add(sidePanelItem.GetDynamic(ObjectId, Types));
            }
            var result = new List<dynamic>
            {
                new
                {
                    id = DObject.RootId,
                    text = "Начало",
                    nodes = items.ToArray(),
                    state = new
                    {
                        selected = DObject.RootId == ObjectId,
                        expanded = true
                    },
                    tags = new[] {items.Count.ToString()}
                }
            };
            return result.ToArray();
        }
    }

    /// <summary>
    /// Модель пункта боковой панели
    /// </summary>
    public class SidePanelItem
    {
        /// <summary>
        /// Отображаемое имя пункта
        /// </summary>
        public string Name {
            get
            {
                if (Type == null || DObject == null)
                    return "не определено";
                return DObject.GetTitle(Type);
            }
        }
        /// <summary>
        /// Тип пункта
        /// </summary>
        public MType Type { get; set; }
        /// <summary>
        /// Объект действия пункта
        /// </summary>
        public DObject DObject { get; set; }
        /// <summary>
        /// Подпункты для данного пункта
        /// </summary>
        public List<SidePanelItem> SubItems { get; set; }
        /// <summary>
        /// Принимает значение true,  если пункт выбран
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Принимает значение true, если пункт является флажком, или имеет подпункты
        /// </summary>
        private bool Expanded {
            get { return Selected || SubItems?.Any(x => x.Expanded) == true; }
        }
            
        /// <summary>
        /// Создание пункта боковой панели во время выполнения
        /// </summary>
        /// <param name="id">Уникальный идентификатор объекта, под окторый создаётся пункт</param>
        /// <param name="types">Словарь типов, для которого будет действовать данный пункт.</param>
        /// <returns>Представление пункта боковой панели</returns>
        public dynamic GetDynamic(Guid id, IDictionary<int, MType> types)
        {
            var nodes = new List<dynamic>();
            if (SubItems != null)
                foreach (var sidePanelItem in SubItems)
                {
                    nodes.Add(sidePanelItem.GetDynamic(id, types));
                }
            var mType = types[DObject.TypeId];
            string icon;
            var tags = new List<string>(2){DObject.Children.Count(x => !types[x.TypeId].IsProjectFileOrFolder()).ToString()};
            if (mType.IsMountable)
                tags.Add("ИФ");
            return new
            {
                id = DObject.Id,
                text = Name,
                icon = ApplicationConst.TypesGlyphiconDictionary.TryGetValue(mType.Name, out icon) ? icon : "",
                state = new {
                    selected = Selected,
                    expanded = Expanded
                },
                tags = tags.ToArray(),
                nodes = nodes.Any() || DObject.Children.Any(y => types[y.TypeId].Children.Any()) ? nodes.ToArray() : null
            };
        }
    }
}
