using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace CommonClasses.Helpers
{
    public class Utf8StringWriter: StringWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }

    public static class XmlHelper
    {
        public static string GetObjectXml(object obj, string reason = null)
        {
            var objectType = obj.GetType();
            using (var stringWriter = new Utf8StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement(objectType.Name);
                    if (!string.IsNullOrEmpty(reason))
                        xmlWriter.WriteAttributeString("Reason", reason);

                    IEnumerable<PropertyInfo> propertyInfos = objectType.GetProperties().OrderBy(r => r.Name);
                    foreach(PropertyInfo propertyInfo in propertyInfos)
                    {
                        if (!propertyInfo.PropertyType.IsValueType && propertyInfo.PropertyType != typeof(string) || propertyInfo.PropertyType.IsEnum)
                            continue;
                        xmlWriter.WriteElementString(propertyInfo.Name, ConversionHelper.ObjToString(propertyInfo.GetValue(obj, null)));
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                }
                return stringWriter.ToString().RemoveXmlHeader();
            }
        }

        public static string GetDifferenceXml(object oldObj, object newObj)
        {
            var objectType = newObj.GetType();

            using (var stringWriter = new Utf8StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement(objectType.Name);

                    IEnumerable<PropertyInfo> propertyInfos = objectType.GetProperties().OrderBy(r => r.Name);
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        if (!propertyInfo.PropertyType.IsValueType && propertyInfo.PropertyType != typeof(string) || propertyInfo.PropertyType.IsEnum)
                            continue;
                        object newValue = propertyInfo.GetValue(newObj, null);
                        var propertyInfo2 = oldObj.GetType().GetProperty(propertyInfo.Name);
                        if (propertyInfo2 == null) continue; //todo: fix it in another way
                        object oldValue = propertyInfo2.GetValue(oldObj, null);
                        if (!ReflectionHelper.ValuesAreEqual(oldValue, newValue, propertyInfo.PropertyType.FullName))
                        {
                            xmlWriter.WriteStartElement(propertyInfo.Name);
                            xmlWriter.WriteElementString("Old", ConversionHelper.ObjToString(oldValue));
                            xmlWriter.WriteElementString("New", ConversionHelper.ObjToString(newValue));
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                }
                return stringWriter.ToString().RemoveXmlHeader();
            }
        }

        private static string RemoveXmlHeader(this string xml)
        {
            int n = xml.IndexOf("?>");
            if (n == -1) return xml;
            return xml.Substring(n + 2);
        }

        public static void GetElementsValues(string xml, string operation, Dictionary<string, string> properties)
        {
            bool isUpdate = operation == "U";
            bool excludeEmpty = operation == "I";
            using (var xmlReader = XmlReader.Create(new StringReader(xml)))
            {
                xmlReader.MoveToContent();
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        var value = string.Empty;
                        if (isUpdate)
                        {
                            var inner = xmlReader.ReadSubtree();
                            while (inner.Read())
                            {
                                if (inner.Name.Equals("New"))
                                {
                                    value = inner.ReadString();
                                }
                            }
                        }
                        else
                        {
                            value = xmlReader.ReadString();
                            if(excludeEmpty && string.IsNullOrEmpty(value))
                                continue;
                        }

                        properties[xmlReader.Name] = value;
                    }
                }
            }
        }
    }
}
