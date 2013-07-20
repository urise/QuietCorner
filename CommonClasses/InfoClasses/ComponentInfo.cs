using CommonClasses.DbClasses;

namespace CommonClasses.InfoClasses
{
    public class ComponentInfo
    {
        public int ComponentId { get; set; }
        public string Name { get; set; }
        public bool IsReadOnlyAccess { get; set; }
        public int? ParentId { get; set; }

        public AccessLevel AccessLevel
        {
            get { return (AccessLevel)Access; }
            set { Access = (int)value; }
        }
        public int Access { get; set; }

        public bool Disabled { get; set; }

        public ComponentInfo() {}
        public ComponentInfo(Component component, AccessLevel access)
        {
            ComponentId = component.ComponentId;
            Name = component.ComponentName;
            IsReadOnlyAccess = component.IsReadOnlyAccess;
            AccessLevel = access;
        }

        public ComponentRole Update(ComponentRole componentsToRole)
        {
            componentsToRole.AccessLevel = AccessLevel;
            return componentsToRole;
        }
    }
}