using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using TFSHelper.Helper.Model;
using TFSHelper.Plugin.Core.Plugins.Abstract;

namespace TFSHelper.Plugin.Core.Plugins.Concrete
{
    class TFRequestFilterUpdateWorkItemsPlugin : TFRequestFilterPlugin
    {
        public TFRequestFilterUpdateWorkItemsPlugin(Object extentionData) : base(extentionData) { }

        public override TFPluginProcessResponse Process(TeamFoundationRequestContext teamfoundationRequestContext,
            string pluginExecutionType)
        {
            var statusmsg = string.Empty;
            var isProcessValid = true;
            if (HttpContext.Current.Request.Form.Count == 0 || string.IsNullOrEmpty(HttpContext.Current.Request.Form[0]))
                return null;
            var jRequestMessage = HttpContext.Current.Request.Form[0];
            dynamic parsedRequestMessages = string.Empty;
            try
            {
                parsedRequestMessages = JArray.Parse(jRequestMessage);
            }
            catch (Exception)
            {
                //Log
            }
            foreach (var parsedRequestMessage in parsedRequestMessages)
            {

                if (string.IsNullOrEmpty(parsedRequestMessage.ToString())) return null;
                var fields = parsedRequestMessage.fields;
                var dic = new Dictionary<string, string>();
                var tfsItem = new TFSItem();

                foreach (var field in fields)
                {
                    if (field.Value == null) continue;
                    if (field.Value.Value != null)
                        dic.Add(field.Name, field.Value.Value.ToString());
                }

                if (parsedRequestMessage.links != null)
                {
                    var tfsLinkFields = new List<TFSLinkField>();
                    var totalLinks = (JObject)parsedRequestMessage.links;
                    JToken deletedLinkValue;
                    var deletedLinks = totalLinks.TryGetValue("deletedLinks", out deletedLinkValue);
                    JToken addedLinkValue;
                    var addedLinks = totalLinks.TryGetValue("addedLinks", out addedLinkValue);

                    if (deletedLinks)
                        tfsLinkFields.AddRange(deletedLinkValue.ToObject<List<TFSLinkField>>().Select(field => new TFSLinkField
                                        {
                                            Id = field.Id,
                                            LinkType = field.LinkType,
                                            State = AtTimeState.Deleted
                                        }));
                    if (addedLinks)
                        tfsLinkFields.AddRange(addedLinkValue.ToObject<List<TFSLinkField>>().Select(field => new TFSLinkField
                                        {
                                            Id = field.Id,
                                            LinkType = field.LinkType,
                                            State = AtTimeState.Added
                                        }));

                    tfsItem.TFSLinks = tfsLinkFields;
                }

                tfsItem.Fields = new TFSFieldList
                    {
                        new TFSField() {Id = "rev", NewValue = parsedRequestMessage.rev.Value},
                        new TFSField() {Id = "System.Id", NewValue = parsedRequestMessage.id.Value},
                        new TFSField() {Id = "projectId", NewValue = parsedRequestMessage.projectId.Value}
                    };

                foreach (var item in dic)
                {
                    tfsItem.Fields.Add(new TFSField { Id = item.Key, NewValue = item.Value, IsDirty = true });
                }

                var aggregator = new TFSEventAggregator();
                aggregator.Init();


                var tfsEventArgs = ToTFSEventArgs(tfsItem, teamfoundationRequestContext,
                    delegate(bool isValid, string message)
                    {
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
                if (tfsEventArgs == null) return null;
                aggregator.Publish(Utility.RequestType.PluginExecutionType[pluginExecutionType], tfsEventArgs);
            }
            return new TFPluginProcessResponse() { IsValid = isProcessValid, Message = statusmsg };
        }
    }
}
