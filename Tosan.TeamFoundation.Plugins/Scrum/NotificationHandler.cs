//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Net;
//using System.Net.Mail;
//using System.Text;
//using System.Xml;
//using System.Xml.Linq;
//using System.Xml.Xsl;
//using Microsoft.TeamFoundation.Framework.Common;
//using Microsoft.TeamFoundation.WorkItemTracking.Client;
//using Microsoft.TeamFoundation.WorkItemTracking.Server;
//using Tosan.TeamFoundation.Plugin.Core;
//using Tosan.TeamFoundation.Plugin.Core.Utility;
//using Tosan.TeamFoundation.Plugins.Scrum.Resources;
//using WorkItemService = Tosan.TeamFoundation.Plugin.Core.WorkItemService;

//namespace Tosan.TeamFoundation.Plugins.Scrum
//{
//    class NotificationHandler : IEventHandler
//    {

//        class EmailComparer : IEqualityComparer<AlertRecipient>
//        {
//            public int Compare(AlertRecipient x, AlertRecipient y)
//            {
//                return System.String.Compare(x.EmailAddress, y.EmailAddress, System.StringComparison.OrdinalIgnoreCase);
//            }

//            public bool Equals(AlertRecipient x, AlertRecipient y)
//            {
//                return System.String.Equals(x.EmailAddress, y.EmailAddress);
//            }

//            public int GetHashCode(AlertRecipient obj)
//            {
//                return obj.EmailAddress.GetHashCode();
//            }
//        }

//        public void Register(TFSEventAggregator aggregator)
//        {
//        }



//        private void wiCreated(TFSEventArgs eventArgs)
//        {

//            var recipients = new HashSet<AlertRecipient>(new EmailComparer());

//            recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient((String)eventArgs.TFSEventItem.Fields["System.CreatedBy"].NewValue, IdentitySearchFactor.DisplayName));
//            recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient((String)eventArgs.TFSEventItem.Fields["System.AssignedTo"].NewValue, IdentitySearchFactor.DisplayName));

//            if (eventArgs.TFSEventItem.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Bug &&
//               (eventArgs.TFSEventItem.Fields[FieldNames.Severity].NewValue.ToString() == FieldValues.Severity_Blocker ||
//                eventArgs.TFSEventItem.Fields[FieldNames.Severity].NewValue.ToString() == FieldValues.Severity_Critical))

//            if (recipients.Count > 0)
//                SendMail(eventArgs, recipients);
//        }

//        private void wiUpdated(TFSEventArgs eventArgs)
//        {
//            var recipients = new HashSet<AlertRecipient>(new EmailComparer());
//            if (eventArgs.TFSEventItem.Fields[FieldNames.AssignedTo].IsDirty)
//            {
//                recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient((String)eventArgs.TFSEventItem.Fields[FieldNames.CreatedBy].NewValue, IdentitySearchFactor.DisplayName));
//                recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient(eventArgs.TFSEventItem.Changer, IdentitySearchFactor.Identifier));
//                recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient((String)eventArgs.TFSEventItem.Fields[FieldNames.AssignedTo].OldValue, IdentitySearchFactor.DisplayName));
//                recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient((String)eventArgs.TFSEventItem.Fields[FieldNames.AssignedTo].NewValue, IdentitySearchFactor.DisplayName));
//            }
//            if (eventArgs.TFSEventItem.Fields[FieldNames.State].IsDirty)
//            {
//                recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient((String)eventArgs.TFSEventItem.Fields[FieldNames.CreatedBy].NewValue, IdentitySearchFactor.DisplayName));
//                recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient((String)eventArgs.TFSEventItem.Fields[FieldNames.AssignedTo].NewValue, IdentitySearchFactor.DisplayName));
//                recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient(eventArgs.TFSEventItem.Changer, IdentitySearchFactor.Identifier));

//                if (eventArgs.TFSEventItem.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Tested)
//                {
//                    WorkItemService _helper = eventArgs.ContextTFSHelper;
//                    var parentWi = _helper.GetParentWorkItem(eventArgs.TFSEventItem.Id, WorkItemLinkType.Topology.Tree);

//                    if (parentWi != null)
//                    {
//                        var allDevTasks = _helper.GetLinkedWorkItems(parentWi.Id, WorkItemLinkType.Topology.Tree, FieldValues.WI_DevTask);
//                        foreach (var devTask in allDevTasks)
//                            recipients.Add(eventArgs.ContextTFSHelper.GetUserRecipient(devTask.Fields[FieldNames.AssignedTo].Value.ToString(), IdentitySearchFactor.DisplayName));
//                    }
//                }
//            }

//            if (recipients.Count > 0)
//                SendMail(eventArgs, recipients);

//        }

//        void SendMail(TFSEventArgs eventArgs, HashSet<AlertRecipient> recipients)
//        {
//            var serializer = new EventXmlSerializer();

//            XDocument eventDocument = serializer.Serialize<WorkItemChangedEvent>(eventArgs.NotificationEventArgs);
//            SendMailNotification(eventArgs, eventDocument, recipients);
//        }


//        public void SendMailNotification(TFSEventArgs eventArgs, XDocument eventDocument, HashSet<AlertRecipient> recipients)
//        {
//            var message = new MailMessage
//            {

//                Body = Format(eventDocument), //"Hi this is a test message",
//                BodyEncoding = Encoding.UTF8,
//                IsBodyHtml = true,
//                Subject = "TFS Notification",
//                SubjectEncoding = Encoding.UTF8,
//                From = new MailAddress("tfs@kishware.com")
//            };

//            foreach (var person in recipients)
//            {
//                message.To.Add(new MailAddress(person.EmailAddress, person.DisplayName));
//            }


//            using (var client = new SmtpClient("mail.tosanltd.com"))
//            {
//                client.Credentials = new NetworkCredential("tfs@kishware.com", "hsb_1234");
//                client.Send(message);
//            }

//        }


//        private string Format(XDocument eventDocument)
//        {
//            const string path = @"C:\Program Files\Microsoft Team Foundation Server 11.0\Application Tier\TFSJobAgent\Transforms\1033\WorkItemChangedEvent.xsl"; //Path.Combine(_xslSearchPath, eventType + "." + xslExtension);

//            var transform = new XslCompiledTransform();
//            transform.Load(path);
//            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
//            {
//                using (var xmlWriter = new XmlTextWriter(writer))
//                {
//                    using (var reader = eventDocument.CreateReader())
//                    {
//                        transform.Transform(reader, null, xmlWriter);
//                        return writer.ToString();
//                    }
//                }
//            }
//        }

//    }
//}
