using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation;
using TFSHelper.Helper;
using TFSHelper.Helper.Model;
using TFSHelper.Plugin.Core.Plugins.Abstract;

namespace TFSHelper.Plugin.Core.Plugins.Concrete
{
    class TFCheckInPlugin : TFPlugin
    {
        internal TFCheckInPlugin(object extentionData) : base(extentionData) { }

        protected override TFSEventArgs ConvertToTFEventArgs(TeamFoundationRequestContext requestContext,
            TFSEventArgs.ProcessActionResponse processActionResponse = null)
        {
            var itemChangedEvent = base.ExtentionData as CheckinNotification;
            try
            {
                var tfsService = new WorkItemService(requestContext);
                var changeSet = new ChangeSet
                {
                    Fields = new TFSFieldList(),
                    Id = itemChangedEvent.Changeset,
                    SourcesPath = new List<string>(itemChangedEvent.GetSubmittedItems(requestContext)),
                    Changer = itemChangedEvent.ChangesetOwner.Descriptor.Identifier,
                    WorkSpace = itemChangedEvent.WorkspaceName,
                    WorkSpaceOwner = itemChangedEvent.WorkspaceOwner.UniqueName,
                    ChangeSetOwnerDisplayName = itemChangedEvent.ChangesetOwner.DisplayName,
                    TpcName = requestContext.ServiceHost.Name,

                };
                changeSet.Fields.Add(new TFSField { Id = FieldNames.Type, NewValue = "ChangeSet" });
                changeSet.IsPolicyOverrided = itemChangedEvent.PolicyOverrideInfo.PolicyFailures != null;
                if (itemChangedEvent.NotificationInfo.WorkItemInfo != null)
                {
                    changeSet.TFSLinks = new List<TFSLinkField>();
                    foreach (var itemInfo in itemChangedEvent.NotificationInfo.WorkItemInfo)
                    {
                        changeSet.TFSLinks.Add(new TFSLinkField()
                        {
                            Id = itemInfo.Id,
                            LinkName = itemInfo.CheckinAction.ToString()
                        });
                    }
                }
                changeSet.ItemEventType = PluginType.CheckinNotification;
                return new TFSEventArgs { ContextTFSHelper = tfsService, TFSEventItem = changeSet, NotificationEventArgs = null, ResponseAction = processActionResponse };
            }
            catch (Exception e)
            {
                throw new TeamFoundationServerException(e.Message);
            }
        }
    }
}
