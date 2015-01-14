using System;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.VersionControl.Server;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins
{
    public class CheckinPlugin : ISubscriber
    {
        public string Name
        {
            get { return "Checkin Event Handler"; }
        }

        public SubscriberPriority Priority
        {
            get { return SubscriberPriority.AboveNormal; }
        }

        public EventNotificationStatus ProcessEvent(TeamFoundationRequestContext requestContext, NotificationType notificationType,
            object notificationEventArgs, out int statusCode, out string statusMessage, out ExceptionPropertyCollection properties)
        {
            statusCode = 0;
            properties = null;
            statusMessage = String.Empty;

            var pluginSericeFactory = new TFPluginServiceFactory();

            var tfplugin = pluginSericeFactory.CreateWorkItemPlugin(notificationEventArgs);

            if (tfplugin == null) return EventNotificationStatus.ActionApproved;

            var pluginServiceProcessResponse = tfplugin.Process(requestContext, "PreUpdate");
            if (pluginServiceProcessResponse.IsValid) return EventNotificationStatus.ActionApproved;
            statusMessage = pluginServiceProcessResponse.Message;
            return EventNotificationStatus.ActionDenied;
        }
        public Type[] SubscribedTypes()
        {
            return new[] { typeof(CheckinNotification) };
        }
    }
}
