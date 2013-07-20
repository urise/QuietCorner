using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CommonClasses.Helpers;

namespace WebSite.Helpers
{
    public static class MenuHelper
    {
        public static bool CheckForRelationship(List<MenuLink> collection, MenuLink link, MenuLink requestedLink)
        {
            if (link == null || requestedLink == null || collection == null) return false;
            if (link.MenuLinkId == requestedLink.MenuLinkId
                || (requestedLink.Controller == link.Controller && requestedLink.Action == link.Action && string.IsNullOrEmpty(link.TargetId))) return true;

            var tempLink = requestedLink;
            while (collection.Any(x => x.MenuLinkId == tempLink.ParentId))
            {
                tempLink = collection.First(x => x.MenuLinkId == tempLink.ParentId);
                if (tempLink.MenuLinkId == link.MenuLinkId) return true;
            }
            return false;
        }

        public static int? GetParentIdOfRelatedToRequestLevelOrNull(List<MenuLink> collection, int requestedLevel, MenuLink requestedLink)
        {
            if (requestedLink.ParentId == null || requestedLevel == 1) return null;
            var levelItem = requestedLink;
            while (requestedLink.ParentId != null)
            {
                if (levelItem.ItemLevel == requestedLevel) return levelItem.ParentId;
                levelItem = collection.First(x => x.MenuLinkId == levelItem.ParentId);
            }
            return null;
        }

        public static MvcHtmlString CategoryMenuContent(this HtmlHelper helper, List<MenuLink> collection, int categoryItemId, MenuLink requestedItem)
        {
            var sb = new StringBuilder();
            sb.Append("<ul class=\"sub-menu-block\">");
            foreach (var menuLink in collection.Where(x => x.ParentId == categoryItemId))
            {
                GenerateLinkItem(collection, requestedItem, sb, menuLink);
            }
            sb.Append("</ul>");
            return new MvcHtmlString(sb.ToString());
        }

        private static void GenerateLinkItem(List<MenuLink> collection, MenuLink requestedItem, StringBuilder sb, MenuLink menuLink)
        {
            var linkClass = CheckForRelationship(collection, menuLink, requestedItem) ? "active" : string.Empty;
            linkClass += menuLink.HasAccess ? string.Empty : " noAccess";
            sb.AppendFormat("<li><a href={0} class=\"{1}\"><span>{2}</span></a>", GetLinkUrl(menuLink), linkClass, menuLink.Name);
            if (collection.Any(x => x.ParentId == menuLink.MenuLinkId))
                CreateSubLevel(ref sb, collection, menuLink.MenuLinkId, requestedItem);
            sb.Append("</li>");
        }

        static void CreateSubLevel(ref StringBuilder sb, List<MenuLink> collection, int categoryItemId, MenuLink requestedItem)
        {
            sb.Append("<ul class=\"sublevel\">");
            foreach (var subItem in collection.Where(x => x.ParentId == categoryItemId))
            {
                GenerateLinkItem(collection, requestedItem, sb, subItem);
            }
            sb.Append("</ul>");
        }

        static string GetLinkUrl(MenuLink link)
        {
            return UrlHelper.Action(link.Action, link.Controller, new { target = link.TargetId });
        }

        private static UrlHelper UrlHelper
        {
            get
            {
                return new UrlHelper(HttpContext.Current.Request.RequestContext);
            }
        }
    }
}