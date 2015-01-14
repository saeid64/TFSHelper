using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TosanTFS.Web.Service
{
    public static class WorkItemHelper
    {
        public static TFSItem ToTfsItem(this WorkItem workItem)
        {
            var tfsItem = new TFSItem
                {
                    Fields = new List<TFSField>(),
                    Id = workItem.Id,
                    Title = workItem.Title,
                    WorkItemType = workItem["System.WorkItemType"].ToString(),
                    ProjectName = workItem.Project.Name,
                    Area = workItem.AreaPath
                };

            tfsItem.Fields.AddRange(new LinkedList<TFSField>(workItem.Fields.OfType<Field>().Select(field => new TFSField
                {
                    Id = field.Name,
                    Value = field.Value
                })));

            if (workItem.WorkItemLinks != null && workItem.WorkItemLinks.OfType<WorkItemLink>().Any())
            {
                tfsItem.LinkItems = new List<TFSLinkField>();
                tfsItem.LinkItems.AddRange(workItem.WorkItemLinks.OfType<WorkItemLink>().Select(
                    relation => new TFSLinkField { ID = relation.TargetId, LinkName = relation.LinkTypeEnd.Name, LinkType = relation.LinkTypeEnd.LinkType.ReferenceName }));
            }

            return tfsItem;
        }
        public static TFSItem ToTfsItem(this Changeset changeset)
        {
            var tfsItem = new TFSItem
            {
                Fields = new List<TFSField>(),
                Id = changeset.ChangesetId
            };

            if (changeset.WorkItems != null && changeset.WorkItems.Any())
            {
                tfsItem.LinkItems = new List<TFSLinkField>();
                tfsItem.LinkItems.AddRange(changeset.WorkItems.Select(
                    relation => new TFSLinkField { ID = relation.Id, LinkName = "workItems", Item = relation.ToTfsItem() }));
            }

            return tfsItem;
        }
    }
}