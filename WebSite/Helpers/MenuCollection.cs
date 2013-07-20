using System.Collections.Generic;
using CommonClasses;
using CommonClasses.Helpers;
using System.Linq;

namespace WebSite.Helpers
{
    public static class MenuCollection
    {
        private static List<MenuLink> _menuLinks = new List<MenuLink>
                       {
                           new MenuLink { MenuLinkId = 1, Name = "Главная", Controller = "Home", Action = "Home", ParentId = null, ItemLevel = 1, Component = AccessComponent.Home},
                           new MenuLink { MenuLinkId = 2, Name = "Компания", Controller = "Instance", Action = "Instance", ParentId = 20, ItemLevel = 1, Component = AccessComponent.Instance},
                           new MenuLink { MenuLinkId = 3, Name = "Настройки", Controller = "", Action = "", ParentId = null, ItemLevel = 1, Component = AccessComponent.Settings, AutoRedirectedMenuId = 5},
                           new MenuLink { MenuLinkId = 4, Name = "Компания", Controller = "", Action = "", ParentId = 3, ItemLevel = 2, IsCategory = true, Component = AccessComponent.None}, 
                           new MenuLink { MenuLinkId = 5, Name = "Пользователи", Controller = "Users", Action = "Users", ParentId = 4, ItemLevel = 3, Component = AccessComponent.Users},
                           new MenuLink { MenuLinkId = 6, Name = "Роли", Controller = "Roles", Action = "Roles", ParentId = 4, ItemLevel = 3, Component = AccessComponent.Roles},
                           new MenuLink { MenuLinkId = 7, Name = "Инфраструктура", Controller = "", Action = "", ParentId = 3, ItemLevel = 2, IsCategory = true, Component = AccessComponent.None},
                       };

        public static List<MenuLink> MenuLinks
        {
            get
            {
                SetAccessToLinks();
                return _menuLinks;
            }
        }

        public static void SetAccessToLinks()
        {
            foreach (var menuLink in _menuLinks)
            {
                menuLink.HasAccess = menuLink.Component == AccessComponent.None || SessionHelper.Permissions.IsGranted(menuLink.Component, AccessLevel.Read);
            }

            foreach (var menuLink in _menuLinks.Where(m => m.AutoRedirectedMenuId != 0))
            {
                SetAvailableActionLink(menuLink);
            }
        }

        private static void SetAvailableActionLink(MenuLink menuLink)
        {
            if (menuLink.HasAccess)
            {
                MenuLink autoLink = GetAvailableActionLink(menuLink);
                if (autoLink != null)
                {
                    menuLink.Action = autoLink.Action;
                    menuLink.Controller = autoLink.Controller;
                }
            }
        }

        private static MenuLink GetAvailableActionLink(MenuLink menuLink)
        {
            MenuLink autoLink = _menuLinks.FirstOrDefault(m => menuLink.AutoRedirectedMenuId == m.MenuLinkId);
            if (autoLink != null && !autoLink.HasAccess)
                foreach (var dependedLink in _menuLinks.Where(m => m.ParentId == menuLink.MenuLinkId))
                {
                    if (dependedLink.IsCategory)
                        autoLink = _menuLinks.FirstOrDefault(m => m.ParentId == dependedLink.MenuLinkId && m.HasAccess);
                    else if (dependedLink.HasAccess)
                        autoLink = dependedLink;

                    if (autoLink != null)
                        break;
                }
            return autoLink;
        }
    }
}
