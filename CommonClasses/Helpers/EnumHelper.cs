using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace CommonClasses.Helpers
{
    public static class EnumHelper
    {
        public static string GetDescription<TEnum>(this TEnum en)
        {
            var type = en.GetType();
            var memInfo = type.GetMember(en.ToString());

            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, TEnum selectedItem)
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         select new { Id = e, Name = e.GetDescription() };
            return new SelectList(values, "Id", "Name", selectedItem);
        }
    }
}
