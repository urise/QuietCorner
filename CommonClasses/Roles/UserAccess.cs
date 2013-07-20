using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonClasses.Roles
{
    public class UserAccess
    {
        public Dictionary<AccessComponent, AccessLevel> AccessDict = 
            new Dictionary<AccessComponent, AccessLevel>();

        public void Add(AccessComponent component, AccessLevel level)
        {
            AccessDict[component] = level;
        }

        public bool IsGranted(AccessComponent component, AccessLevel requiredLevel)
        {
            AccessLevel storedLevel;
            if (!AccessDict.TryGetValue(component, out storedLevel)) return false;

            return storedLevel >= requiredLevel;
        }
    }
}
