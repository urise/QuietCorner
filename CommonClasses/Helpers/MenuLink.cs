namespace CommonClasses.Helpers
{
    public class MenuLink
    {
        public int MenuLinkId { get; set; }
        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int? ParentId { get; set; }
        public bool IdDependant { get; set; }
        public int ItemLevel { get; set; }
        public bool IsCategory { get; set; }
        public AccessComponent Component { get; set; }
        public bool HasAccess { get; set; }
        public int AutoRedirectedMenuId { get; set; }
        public string TargetId { get; set; }
    }
}