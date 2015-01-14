using System;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Git.Server;
using System.Threading.Tasks;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins
{
    public class PushPlugin : ISubscriber
    {
        public string Name
        {
            get { return "Push Event Handler"; }
        }

        public SubscriberPriority Priority
        {
            get { return SubscriberPriority.AboveNormal; }
        }

        public EventNotificationStatus ProcessEvent(TeamFoundationRequestContext requestContext, NotificationType notificationType,
            object notificationEventArgs, out int statusCode, out string statusMessage, out ExceptionPropertyCollection properties)
        {
            var s = new TaskFactory();
            statusCode = 0;
            properties = null;
            statusMessage = String.Empty;
            var statusmsg = string.Empty;
            var isValid = true;
            try
            {
                var pushNotification = notificationEventArgs as PushNotification;
                //var notification = new GitService(requestContext, pushNotification);
                //pushNotification.RefUpdateResults
            }
            catch (Exception e)
            {
                statusMessage = e.Message;
                return EventNotificationStatus.ActionDenied;
            }
            return EventNotificationStatus.ActionApproved;
        }
        public Type[] SubscribedTypes()
        {
            return new[] { typeof(PushNotification) };
        }
    }
}
