using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace TFSHelper.Plugin.Core.Utility
{
    public sealed class EventXmlSerializer
    {
        private static readonly XmlSerializer WorkItemChangedEventSerializer =
        new XmlSerializer(typeof(WorkItemChangedEvent));

        public XDocument Serialize<T>(object notificationEventArgs)
            where T : class
        {

            if (typeof(T) == typeof(WorkItemChangedEvent))
            {
                return GenerateWorkItemChangedEventXml(notificationEventArgs);
            }

            throw new NotSupportedException();
        }
        private Boolean IsValidField(XNode field)
        {
            var strField = field.ToString();
            if (strField.Contains("WEF"))
                return false;
            if (strField.Contains("GUID"))
                return false;
            return !strField.Contains(" Id");
        }
        private XDocument GenerateWorkItemChangedEventXml(object notificationEventArgs)
        {
            var eventArgs = notificationEventArgs as WorkItemChangedEvent;
            if (eventArgs == null)
            {
                return null;
            }

            // Serialize the event before manipulating since it is passed in by the TFS.
            XDocument eventDocument = SerializeToXml(notificationEventArgs);

            if (eventDocument.Root != null)
            {
                XElement titleNode = eventDocument.Root.Descendants("Title").Single();

                XElement customFieldsElement = RetrieveCustomFields(eventArgs);

                eventDocument.Root.Add(customFieldsElement);

                titleNode.Value = string.Format(CultureInfo.InvariantCulture, eventArgs.ChangeType ==
                    ChangeTypes.Change ? "{0} Work Item Changed: {1} {2} - {3}" : "{0} Work Item Created: {1} {2} - {3}", new object[]
                    {
                        eventArgs.PortfolioProject,
                        eventArgs.CoreFields.StringFields[0].NewValue,
                        eventArgs.CoreFields.IntegerFields[0].NewValue,
                        eventArgs.WorkItemTitle
                    });

                XElement coreFields = eventDocument.Root.Element("CoreFields");
                if (coreFields != null)
                {
                    //foreach (var intField in coreFields.Element("IntegerFields").Nodes().ToList())
                    foreach (var field in coreFields.Elements().Nodes().ToList())
                    {
                        if (!IsValidField(field))
                            field.Remove();
                    }
                }

                XElement changedFields = eventDocument.Root.Element("ChangedFields");
                if (changedFields != null)
                {
                    foreach (var field in changedFields.Elements().Nodes().ToList())
                    {
                        if (!IsValidField(field))
                            field.Remove();
                    }
                }

                XElement customFields = eventDocument.Root.Element("CustomFields");
                if (customFields != null)
                {
                    foreach (var field in customFields.Elements().Nodes().ToList())
                    {
                        if (!IsValidField(field))
                            field.Remove();
                    }
                }
            }
            return eventDocument;
        }
        public static XDocument SerializeToXml(object o)
        {
            var eventDocument = new XDocument();
            //var serializer = new XmlSerializer(typeof(WorkItemChangedEvent));
            using (var writer = eventDocument.CreateWriter())
            {
                WorkItemChangedEventSerializer.Serialize(writer, o);
            }

            return eventDocument;
        }
        private XElement RetrieveCustomFields(WorkItemChangedEvent eventArgs)
        {
            var integerFieldsElement = new XElement("IntegerFields");
            var stringFieldsElement = new XElement("StringFields");
            var customFieldsElement = new XElement("CustomFields", integerFieldsElement, stringFieldsElement);

            foreach (var fld in eventArgs.ChangedFields.IntegerFields)
            {
                //if (!fld.Name.StartsWith("WEF") && !fld.Name.Contains("GUID") && !fld.Name.Contains(" Id"))
                //{
                var fieldElement = new XElement("Field",
                                                new XElement("Name", fld.Name),
                                                new XElement("ReferenceName", fld.ReferenceName),
                                                new XElement("Value", fld.NewValue));

                integerFieldsElement.Add(fieldElement);
                //}
            }

            foreach (var fld in eventArgs.ChangedFields.StringFields)
            {
                //if (!fld.Name.StartsWith("WEF") && !fld.Name.Contains("GUID") && !fld.Name.Contains(" Id"))
                //{
                var fieldElement = new XElement("Field",
                                                new XElement("Name", fld.Name),
                                                new XElement("ReferenceName", fld.ReferenceName),
                                                new XElement("Value", fld.NewValue));

                stringFieldsElement.Add(fieldElement);
                //}
            }
            return customFieldsElement;
        }
    }

    static internal class XmlParser
    {
        static internal IEnumerable<string> GetElementsByXPathQeury(this XDocument document, string xpathQuery)
        {
            return document.XPathSelectElements(xpathQuery).Select(element => element.Attribute("Key").Value.ToLower());
        }
    }

    static internal class PluginConfigurationParser
    {
        private static readonly XDocument PluginConfiguration;
        static PluginConfigurationParser()
        {
            var pluginConfig = ConfigurationManager.AppSettings["PluginConfiguration"];
            if (pluginConfig == null) return;

            var appPath = SystemInfo.ApplicationBaseDirectory;
            PluginConfiguration = XDocument.Load(appPath + pluginConfig);
        }

        static internal IEnumerable<PluginKey> GetRegisteredPlugins(string collectionName)
        {
            if (PluginConfiguration == null) return null;

            var c = from element in PluginConfiguration.Descendants("Collection")
                    where element.Attribute("Name").Value == collectionName
                    select element;
            return (from i in c.Descendants("project")
                    where i.Attribute("Name") != null
                    let projectName = i.Attribute("Name").Value
                    from i2 in i.Descendants("plugin")
                    where i2.Attribute("Key") != null
                    let key = i2.Attribute("Key").Value
                    select new PluginKey() { CollectionName = collectionName, AreaName = "", ProjectName = projectName, KeyName = key }).ToList();
        }
    }
}
