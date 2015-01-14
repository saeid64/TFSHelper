using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Tosan.TeamFoundation.Plugin.Core.Resources;
using Tosan.TFSHelper;
using Tosan.TFSHelper.Model;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins.Abstract
{
    abstract class TFRequestFilterPlugin : ITFPlugin
    {
        protected object ExtentionData;
        private static string _fieldRefernceName;
        protected TFSEventArgs ToTFSEventArgs(TFSItem item, TeamFoundationRequestContext requestContext,
            TFSEventArgs.ProcessActionResponse processActionResponse = null)
        {
            var tfsService = new WorkItemService(requestContext);
            tfsService.Init();

            try
            {
                var fieldsDef = tfsService.Store.FieldDefinitions;

                if (item.Id != 0)
                {
                    var wi = tfsService.GetWorkItem(item.Id);
                    //Type wiType;
                    //if (!RequestType.MapTFSWorkItem.TryGetValue(wi[FieldNames.Type].ToString(), out wiType) ||
                    //    wiType == null)
                    //    throw new TeamFoundationServerException("Cannot Find Related Type " + wi[FieldNames.Type]);
                    item.Fields.Add(new TFSField { Id = FieldNames.Type, NewValue = wi[FieldNames.Type] });

                    item.Area = wi.AreaPath;
                    item.ProjectName = wi.Project.Name;
                }
                else
                {
                    var tfsProject = tfsService.Store.Projects.OfType<Project>().Single(project => project.Guid == item.Fields["projectId"].NewValue.ToString());
                    item.ProjectName = tfsProject.Name;

                    if (item.Area == null)
                    {
                        var areas = tfsProject.AreaRootNodes.OfType<Node>().ToList();
                        if (item.Fields.Any(field => field.Id == FieldNames.AreaId) && areas.Any())
                        {
                            var area = areas.SingleOrDefault(node => node.Id == int.Parse(item.Fields[FieldNames.AreaId].NewValue.ToString()));
                            item.Area = area != null ? area.Name : item.ProjectName;
                        }
                        else
                            item.Area = item.ProjectName;
                    }
                }
                //if (!IsValidforAction(tfsProject.Uri.AbsoluteUri, tfsService)) return null;
                var collectedItems = item.Fields.Where(field => field.IsDirty).Select(field => field.Id).ToList();

                foreach (var fieldId in collectedItems)
                {
                    int id;
                    if (!int.TryParse(fieldId, out id)) continue;

                    if (fieldsDef.TryGetById(id) == null)
                    {
                        item.Fields.Remove(item.Fields[id.ToString()]);
                        continue;
                    }
                    var field = fieldsDef.GetById(id);
                    _fieldRefernceName = field.ReferenceName;
                    item.Fields[id.ToString()].Id = _fieldRefernceName;
                    //item.Fields[_fieldRefernceName].OldValue = wi[_fieldRefernceName];
                }

                item.TpcName = requestContext.ServiceHost.Name;
                item.ItemEventType = PluginType.WorkItemChangedEvent;
            }
            catch (Exception e)
            {
                throw new TeamFoundationServerException(e.Message);
            }

            return new TFSEventArgs { ContextTFSHelper = tfsService, TFSEventItem = item, ResponseAction = processActionResponse };
        }
        protected TFRequestFilterPlugin(object extentionData = null)
        {
            if (extentionData != null)
                ExtentionData = extentionData;
        }
        public abstract TFPluginProcessResponse Process(TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType);

        public TFPluginProcessResponse ProcessAsync(
            TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType)
        {
            var result =
                new TaskFactory().StartNew(() => Process(teamfoundationRequestContext, pluginExecutionType));
            return result.Result;
        }
    }
}
