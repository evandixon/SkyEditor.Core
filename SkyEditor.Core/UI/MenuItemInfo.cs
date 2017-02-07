using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// Container for information used to make menu items (<see cref="ActionMenuItem"/>) from menu actions (<see cref="MenuAction"/>)
    /// </summary>
    public class MenuItemInfo
    {
        public MenuItemInfo()
        {
            this.ActionTypes = new List<TypeInfo>();
            this.Children = new List<MenuItemInfo>();
            this.SortOrder = 0;
        }
        public string Header { get; set; }
        public List<TypeInfo> ActionTypes { get; set; }
        public List<MenuItemInfo> Children { get; set; }
        public decimal SortOrder { get; set; }
    }
}
