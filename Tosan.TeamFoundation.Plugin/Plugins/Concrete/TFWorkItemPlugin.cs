using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using Tosan.TeamFoundation.Plugin.Core.Plugins.Abstract;
using Tosan.TeamFoundation.Plugin.Core.Utility;
using Tosan.TFSHelper;
using Tosan.TFSHelper.Model;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins.Concrete
{
    class TFWorkItemPlugin : TFPlugin
    {
        internal TFWorkItemPlugin(Object extentionData) : base(extentionData) { }
        protected override TFSEventArgs ConvertToTFEventArgs(TeamFoundationRequestContext requestContext, TFSEventArgs.ProcessActionResponse processActionResponse = null)
        {
            var itemChangedEvent = base.ExtentionData as WorkItemChangedEvent;
            try
            {
                var tfsItem = new TFSItem();
                var tfsService = new WorkItemService(requestContext);

                tfsItem.Fields = new TFSFieldList();
                tfsItem.Fields.AddRange(TFWorkItemHelper.GetFieldList(itemChangedEvent.CoreFields));
                if (itemChangedEvent.TextFields != null)
                    tfsItem.Fields.AddRange(itemChangedEvent.TextFields.Select(field => new TFSField { Id = field.ReferenceName, IsDirty = true, NewValue = field.Value, Type = TFSType.String }));
                if (itemChangedEvent.ChangedFields != null)
                {
                    var changedFields = TFWorkItemHelper.GetFieldList(new CoreFieldsType { IntegerFields = itemChangedEvent.ChangedFields.IntegerFields, StringFields = itemChangedEvent.ChangedFields.StringFields });

                    foreach (var changedField in changedFields.Where(changedField => tfsItem.Fields.All(field => field.Id != changedField.Id)))
                        tfsItem.Fields.Add(changedField);

                    foreach (var field in tfsItem.Fields.Where(field => changedFields.Any(tfsField => tfsField.Id == field.Id)))
                        field.IsDirty = true;

                }
                tfsItem.ProjectName = itemChangedEvent.PortfolioProject;
                tfsItem.TpcName = requestContext.ServiceHost.Name;
                tfsItem.Changer = itemChangedEvent.ChangerSid;
                if (itemChangedEvent.AddedRelations != null && itemChangedEvent.AddedRelations.Any())
                {
                    tfsItem.TFSLinks = new List<TFSLinkField>();
                    tfsItem.TFSLinks.AddRange(itemChangedEvent.AddedRelations.Select(
                        relation => new TFSLinkField { Id = int.Parse(relation.WorkItemId), LinkName = relation.LinkName, State = AtTimeState.Added }));
                }
                if (itemChangedEvent.DeletedRelations != null && itemChangedEvent.DeletedRelations.Any())
                {
                    tfsItem.TFSLinks = new List<TFSLinkField>();
                    tfsItem.TFSLinks.AddRange(itemChangedEvent.DeletedRelations.Select(
                        relation => new TFSLinkField { Id = int.Parse(relation.WorkItemId), LinkName = relation.LinkName, State = AtTimeState.Deleted }));
                }
                if (itemChangedEvent.AddedFiles != null && itemChangedEvent.AddedFiles.Any())
                {
                    tfsItem.TFSFiles = new List<TFSFile>();
                    tfsItem.TFSFiles.AddRange(itemChangedEvent.AddedFiles.Select(
                        attachment => new TFSFile { Name = attachment.Name, State = AtTimeState.Added }));
                }
                if (itemChangedEvent.DeletedFiles != null && itemChangedEvent.DeletedFiles.Any())
                {
                    tfsItem.TFSFiles = new List<TFSFile>();
                    tfsItem.TFSFiles.AddRange(itemChangedEvent.DeletedFiles.Select(
                        attachment => new TFSFile { FileId = int.Parse(attachment.FileId), State = AtTimeState.Deleted }));
                }
                tfsItem.ItemEventType = PluginType.WorkItemChangedEvent;

                return new TFSEventArgs { ContextTFSHelper = tfsService, TFSEventItem = tfsItem, NotificationEventArgs = itemChangedEvent };

            }
            catch (Exception e)
            {
                throw new TeamFoundationServerException(e.Message);
            }
        }
    }
}
