using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using TFSHelper.Helper.Model;
using TFSHelper.Plugin.Core.Plugins.Abstract;

namespace TFSHelper.Plugin.Core.Plugins.Concrete
{
    class TFRequestFilterBulkUpdatePlugin : TFRequestFilterPlugin
    {
        public TFRequestFilterBulkUpdatePlugin(Object extentionData) : base(extentionData) { }
        public override TFPluginProcessResponse Process(TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType)
        {
            var statusmsg = string.Empty;
            var isProcessValid = true;
            var rollingBackTfsItems = new List<TFSItem>();
            try
            {
                var inputStream = HttpContext.Current.Request.InputStream;
                inputStream.Position = 0;

                var xDocument = XDocument.Load(inputStream);

                var tfsItems = xDocument.Descendants().Where(element => element.Name == "UpdateWorkItem").Select(ProduceTFSItem);
                Parallel.ForEach(tfsItems, tfsItem =>
                {
                    var aggregator = new TFSEventAggregator();
                    aggregator.Init();

                    var isInProcessValid = true;
                    var tfsEventArgs = ToTFSEventArgs(tfsItem, teamfoundationRequestContext,
                        delegate(bool isValid, string message)
                        {
                            isInProcessValid = isValid;
                            isProcessValid = isProcessValid && isValid;
                            statusmsg += message + Environment.NewLine;

                            #region TFS 2012 Cancellation Message Bug Fix

                            //var contexts = new List<TeamFoundationRequestContext>();
                            //var systemContextsField = typeof(TeamFoundationRequestContext).GetField("m_systemRequestContexts", BindingFlags.NonPublic | BindingFlags.Instance);
                            //var userContextsField = typeof(TeamFoundationRequestContext).GetField("m_userRequestContexts", BindingFlags.NonPublic | BindingFlags.Instance);
                            //var cancellationReasonField = typeof(TeamFoundationRequestContext).GetField("m_cancellationReason", BindingFlags.NonPublic | BindingFlags.Instance);
                            //var systemRequestContexts = (Dictionary<Guid, TeamFoundationRequestContext>)systemContextsField.GetValue(requestContext);
                            //var userRequestContexts = (Dictionary<Guid, TeamFoundationRequestContext>)userContextsField.GetValue(requestContext);
                            //contexts.AddRange(systemRequestContexts.Values);
                            //contexts.AddRange(userRequestContexts.Values);
                            //requestContext.Cancel(result["Message"].ToString());
                            //foreach (var teamFoundationRequestContext in contexts)
                            //{
                            //    cancellationReasonField.SetValue(teamFoundationRequestContext, requestContext.Status.Message);
                            //}

                            #endregion
                        });
                    if (tfsEventArgs == null) return;
                    aggregator.Publish(Utility.RequestType.PluginExecutionType[pluginExecutionType], tfsEventArgs);

                    if (!isInProcessValid)
                        rollingBackTfsItems.Add(tfsItem);
                });
                if (!rollingBackTfsItems.Any()) return new TFPluginProcessResponse { IsValid = true, Message = statusmsg };

                //Rollbacks all the Changes by a TFSJob
                var jobService = teamfoundationRequestContext.GetService<TeamFoundationJobService>();
                jobService.QueueOneTimeJob(teamfoundationRequestContext, "WorkItem Rollback Job", "TFSHelper.Job.RollBackJob", GetXml(rollingBackTfsItems), true);
                return new TFPluginProcessResponse { IsValid = true, Message = statusmsg };
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static TFSItem ProduceTFSItem(XElement element)
        {
            var tfsItem = new TFSItem
            {
                Id = int.Parse(element.Attribute("WorkItemID").Value),
                Revsion = int.Parse(element.Attribute("Revision").Value)
            };
            foreach (var xElement in element.Descendants("Column").Select(xElement => xElement).Where(xElement => !xElement.IsEmpty))
            {
                tfsItem.Fields.Add(new TFSField { Id = xElement.Attribute("Column").Value, NewValue = xElement.Value, IsDirty = true });
            }
            return tfsItem;
        }
        private static XmlDocument GetXml(IEnumerable<TFSItem> tfsItems)
        {
            var xml = new XmlDocument();
            var workitemsElement = xml.CreateElement("WorkItems");

            foreach (var item in tfsItems)
            {
                var workitemElement = xml.CreateElement("WorkItem");
                var idElement = xml.CreateElement("Id");
                idElement.InnerText = item.Id.ToString();
                var revisionElement = xml.CreateElement("Revision");
                revisionElement.InnerText = item.Revsion.ToString();

                workitemElement.AppendChild(idElement);
                workitemElement.AppendChild(revisionElement);
                workitemsElement.AppendChild(workitemElement);
            }

            xml.AppendChild(workitemsElement);
            return xml;


        }
    }
}
