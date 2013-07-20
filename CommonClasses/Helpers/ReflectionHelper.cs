#region Directievs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

#endregion

namespace CommonClasses.Helpers
{
    /// <summary>
    /// Helper that provides data for controls on main page
    /// </summary>
    public class ReflectionHelper
    {
        //public static void SetObjectStateProperties(ObjectStateInfo obj, Dictionary<string, string> properties, bool excludeDefaults, IEncryptor encryptor = null)
        //{
        //    foreach (KeyValuePair<string, string> pair in properties)
        //    {
        //        PropertyInfo propertyInfo = obj.ObjectType.GetProperty(pair.Key);
        //        if (propertyInfo != null)
        //        {
        //            var type = propertyInfo.PropertyType;
        //            if (!TypeCanBeRestoredFromString(type)) continue;

        //            string value = PropertyIsEncrypted(obj.ObjectType, pair.Key) && encryptor != null
        //                               ? encryptor.Decrypt(pair.Value)
        //                               : pair.Value;
        //            object safeValue;
        //            if (IsNullable(type))
        //                safeValue = string.IsNullOrEmpty(value) ? null : ConvertToType(value, type.GetGenericArguments()[0]);
        //            else
        //                safeValue = ConvertToType(value, type);
        //            propertyInfo.SetValue(obj.ObjectState, safeValue, null);

        //            if (excludeDefaults && obj.ChangedProperties.Contains(pair.Key))
        //            {
        //                var defaultValue = GetDefaultValue(type);
        //                if (safeValue == null || (defaultValue != null && defaultValue.Equals(safeValue)))
        //                    obj.ChangedProperties.Remove(pair.Key);
        //            }
        //        }
        //    }
        //}

//        private static object GetDefaultValue(Type t)
//        {
//            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
//            {
//                return Activator.CreateInstance(t);
//            }
//            return null;
//        }

        public static bool TypeCanBeRestoredFromString(Type type)
        {
            // we ckeck for "entitystate" property - cause it accures on some machines
            return (type.IsValueType && !type.Name.Equals("entitystate", StringComparison.CurrentCultureIgnoreCase))
                    || IsNullable(type) || type.IsPrimitive ||
                   type.Name.Equals("string", StringComparison.CurrentCultureIgnoreCase) ||
                   type.Name.Equals("datetime", StringComparison.CurrentCultureIgnoreCase);
        }

        public static void CopyAllProperties(object src, object dest)
        {
            CopyAllProperties(src, dest, new List<string>());
        }

        public static void CopyAllProperties(object src, object dest, List<String> notCopyList)
        {
            var destProperties = dest.GetType().GetProperties().Where(p=>p.CanWrite).ToDictionary(p => p.Name, p => p);
            var propertyInfos = src.GetType().GetProperties().Where(
                p => !notCopyList.Contains(p.Name)
                    && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                    && destProperties.Keys.Contains(p.Name)
                    && destProperties[p.Name].PropertyType == p.PropertyType
                );

            foreach (var propertyInfo in propertyInfos)
            {
                destProperties[propertyInfo.Name].SetValue(dest, propertyInfo.GetValue(src, null), null);
            }
        }

        public static bool PropertiesAreEqual(object obj1, object obj2, PropertyInfo propertyInfo)
        {
            object value1 = propertyInfo.GetValue(obj1, null);
            object value2 = propertyInfo.GetValue(obj2, null);
            return ValuesAreEqual(value1, value2, propertyInfo.PropertyType.FullName);
        }

        public static bool ValuesAreEqual(object value1, object value2, string typeFullName)
        {
            if (typeFullName == "System.String")
            {
                return (String.IsNullOrEmpty((string)value1) && String.IsNullOrEmpty((string)value2) ||
                        String.Compare((string)value1, (string)value2) == 0);
            }

            if (value1 == null)
                return value2 == null;

            return value1.Equals(value2);
        }

        public static bool CompareAllProperties(object src, object dest)
        {
            if (src.GetType() != dest.GetType())
                throw new Exception("src and dest should have the same type");

            PropertyInfo[] propertyInfos;
            propertyInfos = src.GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (!PropertiesAreEqual(src, dest, propertyInfo))
                    return false;
            }
            return true;
        }

        //public static ObjectStateInfo GetObjectStateInfo(IEnumerable<IDataLogDb> logs, Type type, int? transactionNumber = null, IEncryptor encryptor = null)
        //{
        //    var objectStateInfo = new ObjectStateInfo { ObjectState = Activator.CreateInstance(type), ChangedProperties = new List<string>(), ObjectType = type };
        //    if (logs.Any())
        //    {
        //        var properties = new Dictionary<string, string>();
        //        foreach (var logDb in logs)
        //        {
        //            XmlHelper.GetElementsValues(logDb.Details, logDb.Operation, properties);
        //        }

        //        bool excludeDefaults = false;
        //        var changedProperties = new Dictionary<string, string>();
        //        var lastLog = transactionNumber.HasValue
        //                          ? logs.FirstOrDefault(l => l.TransactionNumber == transactionNumber)
        //                          : logs.Last();
        //        if (lastLog != null && lastLog.Operation != "D")
        //        {
        //            XmlHelper.GetElementsValues(lastLog.Details, lastLog.Operation, changedProperties);
        //            excludeDefaults = lastLog.Operation == "I";
        //        }

        //        objectStateInfo.ChangedProperties = changedProperties.Keys.ToList();
        //        SetObjectStateProperties(objectStateInfo, properties, excludeDefaults, encryptor);
        //    }
        //    return objectStateInfo;
        //}

        //public static ObjectStateInfo GetObjectStateInfo(DataLogDb log, Type type, int? transactionNumber = null, IEncryptor encryptor = null)
        //{
        //    return GetObjectStateInfo(new List<IDataLogDb> {log}, type, transactionNumber, encryptor);
        //}

        public static bool IsNullable(Type type)
        {
            return type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static object ConvertToType(object value, Type type)
        {
            if (type == typeof(bool))
            {
                value = value.Equals("1");
            }
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        //public static bool HasProperties(IEnumerable<IDataLogDb> logs, IEnumerable<string> pnames)
        //{
        //    var properties = new Dictionary<string, string>();
        //    foreach (var logDb in logs)
        //    {
        //        XmlHelper.GetElementsValues(logDb.Details, logDb.Operation, properties);
        //    }
        //    return properties.Any(p => pnames.Contains(p.Key));
        //}

        //public static 
    }
}
