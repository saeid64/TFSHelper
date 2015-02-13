using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.TeamFoundation.Framework.Server;
using TFSHelper.Helper;

namespace TFSHelper.Job
{
    public class RollBackJob : ITeamFoundationJobExtension
    {
        private static readonly object JobLock = new object();
        public TeamFoundationJobExecutionResult Run(TeamFoundationRequestContext requestContext,
            TeamFoundationJobDefinition jobDefinition, DateTime queueTime, out string resultMessage)
        {
            resultMessage = "";
            try
            {
                lock (JobLock)
                {
                    var data = jobDefinition.Data;
                    var doc = XDocument.Parse(data.OuterXml);
                    var items = doc.Descendants("WorkItem")
                        .Select(element => new { Id = int.Parse(element.Element("Id").Value), rev = int.Parse(element.Element("Revision").Value) });

                    if (!items.Any()) return TeamFoundationJobExecutionResult.Succeeded;

                    var tfsHelper = new TFSHelper.WorkItemService(requestContext);
                    tfsHelper.Init();
                    IList wiList=new ArrayList();
                    foreach (var item in items)
                    {
                      var wi = tfsHelper.GetWorkItem(item.Id);
                      tfsHelper.RollbackWorkItem(wi, item.rev);
                      wiList.Add(wi);
                    }
                    var notificationService = new NotificationService(tfsHelper.TeamProjectCollectionInstance);
                    notificationService.NotifyThisItems(wiList);
                 
                    TeamFoundationApplicationCore.Log(requestContext, string.Format("TFS Rollback Job: RollBacked {0} items.", items.Count()), 0, EventLogEntryType.Information);
                    return TeamFoundationJobExecutionResult.Succeeded;
                }

            }
            catch (Exception e)
            {
                TeamFoundationApplicationCore.Log(requestContext, e.Message, 0, EventLogEntryType.Error);
            }
            return TeamFoundationJobExecutionResult.Succeeded;
        }


    }
}
