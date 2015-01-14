using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Client.Internal;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Tosan.TFSHelper.Model;
using System.Xml.Linq;

namespace Tosan.TFSHelper
{
    public class EmailComparer : IEqualityComparer<AlertRecipient>
    {
        public int Compare(AlertRecipient x, AlertRecipient y)
        {
            return System.String.Compare(x.EmailAddress, y.EmailAddress, System.StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(AlertRecipient x, AlertRecipient y)
        {
            return System.String.Equals(x.EmailAddress, y.EmailAddress);
        }

        public int GetHashCode(AlertRecipient obj)
        {
            return obj.EmailAddress.GetHashCode();
        }
    }
    public class NotificationService : TFSService
    {
        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public NotificationService()
            : base()
        {
        }

        /// <summary>
        /// Initilizing TFS Service via 'Team Foundation Request Conxtext' whcich usually is passing through tfs plugins.
        /// </summary>
        /// <param name="requestContext">Team Foundation Request Conxtext</param>
        public NotificationService(TeamFoundationRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// If you Initilized the service before and you have the projectcollection you can use this constructor.
        /// </summary>
        /// <param name="projectCollection">Team Foundation Project Collection</param>
        public NotificationService(TfsTeamProjectCollection projectCollection)
            : base(projectCollection)
        {
        }

        /// <summary>
        /// Initilizing TFS Service.
        /// </summary>

        #endregion Constructors
        #region Method
        public void NotifyThisItems(IList items)
        {
            string content = "";
            var recipients = new HashSet<AlertRecipient>(new EmailComparer());
            IdentityService identityService = new IdentityService(TeamProjectCollectionInstance);
            WorkItemService wiItemService = new WorkItemService();
            foreach (WorkItem wi in items)
            {

                recipients.Add(identityService.GetUserRecipient(identityService.GetTFSIdentity(wi.Revisions[wi.Rev -1].Fields["System.ChangedBy"].OriginalValue.ToString(), IdentitySearchFactor.DisplayName)));
                var revFields = wiItemService.GetChangedFieldsList(wi, (wi.Rev));
                content += "<p>"+"WorkItemId=" + wi.Id.ToString() + "</p>";
                content += " <table border=1>";
                content += "<tr> <td> FieldName </td> <td>OldValue</td> <td> NewValue </td></tr> ";
                  
                foreach (Field revField in revFields)
                {
                    content += " <tr> ";
                    content += " <td> " + revField.Name + "  </td> <td> " + revField.OriginalValue.ToString() + "  </td> <td> " + revField.Value.ToString() + "  </td> ";
                    content += " </tr> ";
                }
                content += " </table> ";
            }
            SendMail(content, recipients);

        }
        public void SendMail(string content, HashSet<AlertRecipient> recipients)
        {
              const string path = @"C:\Program Files\Microsoft Team Foundation Server 12.0\Application Tier\TFSJobAgent\Transforms\1033\WorkItemChangedEvent-RollBack.html";

            var sr = new StreamReader(path,
                    System.Text.Encoding.UTF8);
            var body = string.Format(sr.ReadToEnd(), content);
         
            var message = new MailMessage
            {

                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Subject = "TFS Notification",
                SubjectEncoding = Encoding.UTF8,
                From = new MailAddress("tfs@kishware.com")
            };

            foreach (var person in recipients)
            {
                message.To.Add(new MailAddress(person.EmailAddress, person.DisplayName));
            }
            using (var client = new SmtpClient("mail.tosanltd.com"))
            {
                client.Credentials = new NetworkCredential("TFS", "hsb_1234", "tosanltd.com");
                client.Send(message);
            }
        }
       #endregion Method

    }
}
