using System;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Common;

namespace TFSHelper.Plugin.Core.Plugins
{
    public class WorkItemPlugin : ISubscriber
    {

        public string Name
        {
            get { return "WorkItem Changed Event Handler"; }
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
            try
            {
                if (notificationType != NotificationType.Notification) return EventNotificationStatus.ActionDenied;

                var changedEventArgs = notificationEventArgs as WorkItemChangedEvent;
                //If Changer is Service User 'tosanltd\tfs' Cancel and return Plugin
                if (changedEventArgs.ChangerTeamFoundationId.ToLower() == "efc74553-5a42-4b24-b794-e571d266caa6") return EventNotificationStatus.ActionApproved;

                var pluginSericeFactory = new TFPluginServiceFactory();

                var tfplugin = pluginSericeFactory.CreateWorkItemPlugin(notificationEventArgs);

                if (tfplugin == null) return EventNotificationStatus.ActionApproved;

                var pluginServiceProcessResponse = tfplugin.Process(requestContext, changedEventArgs.ChangeType.ToString());
                if (pluginServiceProcessResponse.IsValid) return EventNotificationStatus.ActionApproved;
                statusMessage = pluginServiceProcessResponse.Message;
                return EventNotificationStatus.ActionDenied;
            }
            catch (Exception e)
            {
                throw new TeamFoundationServerException(e.Message);
            }
        }
        public Type[] SubscribedTypes()
        {
            return new[] { typeof(WorkItemChangedEvent) };
        }
    }
}
