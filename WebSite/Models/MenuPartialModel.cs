using System.Collections.Generic;
using CommonClasses.Helpers;
using WebSite.Helpers;

namespace WebSite.Models
{
    public class MenuPartialModel
    {
        public List<MenuLink> Collection { get { return MenuCollection.MenuLinks; } }
        public int LevelToRender { get; set; }
        public string MenuUlIdString { get; set; }
    }
}