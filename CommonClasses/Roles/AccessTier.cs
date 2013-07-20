using System;

namespace CommonClasses.Roles
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AccessTier: Attribute
    {
        public AccessComponent Component { get; set; }
        public AccessLevel Level { get; set; }

        public AccessTier() {}

        public AccessTier(AccessComponent component, AccessLevel level)
        {
            Component = component;
            Level = level;
        }
    }
}