using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CommonClasses.Helpers
{
    public static class ConversionHelper
    {
        public static decimal ToDecimal(this string str)
        {
            return Decimal.Parse(str.FixDecimalSeparator(), CultureInfo.CurrentCulture);
        }

        public static string FixDecimalSeparator(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            return str.Replace(".", separator).Replace(",", separator);
        }

        public static string ObjToString(object obj)
        {
            if (obj == null) return String.Empty;

            switch (obj.GetType().FullName)
            {
                case "System.DateTime":
                    {
                        var dt = (DateTime)obj;
                        if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0)
                            return dt.ToString("yyyy.MM.dd");
                        return dt.ToString("yyyy.MM.dd hh:mm:ss");
                    }
                case "System.Single":
                    return ((Single)obj).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case "System.Double":
                    return ((Double)obj).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case "System.Decimal":
                    return ((Decimal)obj).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case "System.Boolean":
                    return (bool) obj ? "1" : "0";
            }
            return obj.ToString();
        }

        public static int ToIntSafe(this string str)
        {
            int result;
            if (!int.TryParse(str, out result)) return 0;
            return result;
        }

        public static string ToStringFull(this DateTime dt)
        {
            return dt.ToString("yyyy.MM.dd HH:mm:ss:ffff");
        }

        public static DateTime ToDateTimeFull(this string str)
        {
            return DateTime.ParseExact(str, "yyyy.MM.dd HH:mm:ss:ffff", CultureInfo.InvariantCulture);
        }

        public static string ToCurrencyString(this decimal value, string currencyCode)
        {
            return string.Format("{0} {1}", value, currencyCode);
        }

        public static string ToPeriodName(this DateTime value)
        {
            return value.ToString("Y", CultureInfo.CreateSpecificCulture("ru-Ru"));
        }

        public static string ToStringLocal(this DateTime dt)
        {
            return dt.ToString("dd.MM.yyyy");
        }
    }
}
