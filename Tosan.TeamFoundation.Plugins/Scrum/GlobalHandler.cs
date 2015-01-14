using System;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Xrm.Sdk;
using Tosan.TeamFoundation.Plugin.Core;
using Tosan.TeamFoundation.Plugin.Core.Helper;
using Tosan.TeamFoundation.Plugin.Core.Utility;
using Tosan.TeamFoundation.Plugins.Helper;
using Tosan.TeamFoundation.Plugins.Resources;
using System.Linq;
using Tosan.TFSHelper;
using Tosan.TFSHelper.Model;
using Microsoft.TeamFoundation.Framework.Common;
using System.Collections.Generic;
using Tosan.TFSHelper.Utility;

namespace Tosan.TeamFoundation.Plugins.Scrum
{
    public class GlobalHandler : IEventHandler
    {
        private WorkItemService _helper;

        public void Register(TFSEventAggregator aggregator)
        {
            //Filter on DevTask and  TestTask and AnalysisTask when state change to In Progress
            var updateStateFilter = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Task)
                && item.Fields[FieldNames.State] != null
                && item.Fields[FieldNames.State].IsDirty
                && item.Fields[FieldNames.State].NewValue.ToString() == "In Progress");
            //Filter on DevTask and TestTask and AnalysisTask when they are careted
            var tasksFilter = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask));
            //Filter on PBI and Feature when PRV Change
            var updatePRVFilterinBugandPBI = new AttributeRequestFilter(item =>
                 (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Bug
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_ProductBacklogItem)
                && item.Fields[FieldNames.PlannedReleaseVersion] != null && item.Fields[FieldNames.PlannedReleaseVersion].IsDirty);

            //Filter on DevTask and  AnalysisTask when state change 
            var TasksStateChangeValidator = new AttributeRequestFilter(item => (
               item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask)
                && item.Fields[FieldNames.State] != null
                && item.Fields[FieldNames.State].IsDirty);
            //Filter on DevTask and  TestTask and analysisTask when BaseLineWork change
            var updateBaselineChangeFilter = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
                 || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask)
               && item.Fields[FieldNames.BaseLineWork] != null
               && item.Fields[FieldNames.BaseLineWork].IsDirty);

            var inprogressTimeValidator = new AttributeRequestFilter(item => (
                item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
                 || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
                 || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
                 || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Task)
                 && item.Fields[FieldNames.State] != null
                 && item.Fields[FieldNames.State].IsDirty
                 && item.Fields[FieldNames.State].OldValue.ToString() == "In Progress");
            //Filter on DevTask and  TestTask when PRV change
            var updatePRVChangeFilterinDevandTest = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask)
               && item.Fields[FieldNames.PlannedReleaseVersion] != null
               && item.Fields[FieldNames.PlannedReleaseVersion].IsDirty);

            //Filter on DevTask and  TestTask  and AnalysisTask when Title change
            var updateTitleChangeFilterinDevandTestandAnalysis = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask)
               && item.Fields[FieldNames.Title] != null
               && item.Fields[FieldNames.Title].IsDirty);

            //Filter on DevTask and TestTask and AnalysisTask when Title change
            var updateDescriptionChangeFilterinDevandTestandAnalysis = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask)
               && item.Fields[FieldNames.Description] != null
               && item.Fields[FieldNames.Description].IsDirty);

            //Filter on DevTask and  TestTask  and AnalysisTask  when Assigned To change
            var updateAssignedToChangeFilter = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask)
               && item.Fields[FieldNames.AssignedTo] != null
               && item.Fields[FieldNames.AssignedTo].IsDirty);

            //Filter on DevTask and TestTask and AnalysisTask and Task when In Progress H change
            var inprogressTimeChange = new AttributeRequestFilter(item => (
              item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Task)
               && item.Fields[FieldNames.InProgressTimeHours] != null
               && item.Fields[FieldNames.InProgressTimeHours].IsDirty);

            //Filter on DevTask and  TestTask  and AnalysisTask when Link Change
            var LinkChange = new AttributeRequestFilter(item => (
            item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
             || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
             || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask)
             && item.TFSLinks != null && item.TFSLinks.Any());

            //Filter on DevTask and  TestTask  and AnalysisTask when Attachment Change
            var AttachmentsChange = new AttributeRequestFilter(item => (
            item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
             || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
             || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask)
             && item.TFSFiles != null && item.TFSFiles.Any());

            //Filter on DevTask and  TestTask  and AnalysisTask  when History To change
            var updateHistoryChangeFilter = new AttributeRequestFilter(item => (item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
               || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask)
               && item.Fields[FieldNames.History] != null
               && item.Fields[FieldNames.History].IsDirty);

            //Filter on PBI & Bug when connecting to the feature
            var pbiBugLinkChange = new AttributeRequestFilter(item => (
            item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_ProductBacklogItem
             || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Bug)
             && item.TFSLinks != null && item.TFSLinks.Any());

            var scoreTimeChange = new AttributeRequestFilter(item => (
                item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Task)
                                                                     && item.Fields[FieldNames.Score] != null
                                                                     && item.Fields[FieldNames.Score].IsDirty
                                                                     && (item.Fields[FieldNames.State].NewValue.ToString() !=
                                                                      FieldValues.State_Done ||
                                                                      item.Fields[FieldNames.State].NewValue.ToString() !=
                                                                      FieldValues.State_Removed));

            //It applys as a AttributeRequestFilter to the Remainning Work's value to the Score value transformation
            var remainingToScore = new AttributeRequestFilter(item => (
                item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
                || item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Task));

            aggregator.Subscribe(RequestTypeEnum.Updated, SetSingleWorkItemInProgress, updateStateFilter);
            aggregator.Subscribe(RequestTypeEnum.Created, GlobalTasksCreationHandler, tasksFilter);
            aggregator.Subscribe(RequestTypeEnum.Updated, UpdatePRVInFeature, updatePRVFilterinBugandPBI);
            aggregator.Subscribe(RequestTypeEnum.Updated, TasksInprogressTimeCalculator, inprogressTimeValidator);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateUpdateBaselineWork, updateBaselineChangeFilter);
            aggregator.Subscribe(RequestTypeEnum.Updated, UpdatePRVInParentwithMaxPrvinDevandTest, updatePRVChangeFilterinDevandTest);
            aggregator.Subscribe(RequestTypeEnum.Created, UpdatePRVInParentwithMaxPrvinDevandTest, updatePRVChangeFilterinDevandTest);
            aggregator.Subscribe(RequestTypeEnum.Updated, TfsCrmSyncTitle, updateTitleChangeFilterinDevandTestandAnalysis);
            aggregator.Subscribe(RequestTypeEnum.Updated, TfsCrmSyncDescription, updateDescriptionChangeFilterinDevandTestandAnalysis);
            aggregator.Subscribe(RequestTypeEnum.Updated, TfsCrmSyncAssignedTo, updateAssignedToChangeFilter);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateUpdateInProgressTimeSpan, inprogressTimeChange);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateChangeLink, LinkChange);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncAttachments, AttachmentsChange);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncHistory, updateHistoryChangeFilter);
            aggregator.Subscribe(RequestTypeEnum.Updated, TransferDataFromFeaturetoPbiBug, pbiBugLinkChange);
            aggregator.Subscribe(RequestTypeEnum.Updated, UpdateTasksScore, scoreTimeChange);
            aggregator.Subscribe(RequestTypeEnum.Created, UpdateTasksScore, scoreTimeChange);
            aggregator.Subscribe(RequestTypeEnum.Created, UpdateScoreValueByRemaining, remainingToScore);
        }

        /// <summary>
        /// Upadating Score Values by the transfering the value of the Remaining Time field to the Score Field (also called 'Estimated Time')
        /// </summary>
        /// <param name="workitemAccessories">TFS Event Argument includes changes</param>
        private void UpdateScoreValueByRemaining(TFSEventArgs workitemAccessories)
        {
            _helper = workitemAccessories.ContextTFSHelper as WorkItemService;
            var tfsItem = workitemAccessories.TFSEventItem;

            if (!tfsItem.Contains(FieldNames.RemainingWork) || tfsItem.Contains(FieldNames.Score)) return;

            var wi = _helper.FreeStore.GetWorkItem(tfsItem.Id);

            wi.Open();
            wi[FieldNames.Score] = tfsItem[FieldNames.RemainingWork].NewValue;
            wi.Save();
            wi.Close();
        }

        private void UpdateTasksScore(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var parentWi = _helper.GetParentWorkItem(eventArgs.TFSEventItem.Id, WorkItemLinkType.Topology.Tree);

            if (parentWi == null) return;

            var parentOldEffort = parentWi[FieldNames.Effort] ?? 0.0;

            var effortNew = eventArgs.TFSEventItem[FieldNames.Score].NewValue.ToString() == "" ? 0.0 : double.Parse(eventArgs.TFSEventItem[FieldNames.Score].NewValue.ToString());
            var effortOld = eventArgs.TFSEventItem[FieldNames.Score].OldValue.ToString() == "" ? 0.0 : double.Parse(eventArgs.TFSEventItem[FieldNames.Score].OldValue.ToString());

            var wiEffort = effortNew - effortOld;

            var effort = double.Parse(parentOldEffort.ToString()) + wiEffort;

            parentWi.Open();
            parentWi[FieldNames.Effort] = effort;
            parentWi.Save();
            parentWi.Close();
        }

        private void TasksInprogressTimeCalculator(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);

            var revisions = wi.Revisions.OfType<Revision>().Where(revision => revision.Fields["State"].Value.ToString() == "In Progress"
               && revision.Fields.Contains("State")
                && revision.Fields["State"].IsValid
                && revision.Fields["State"].Value != revision.Fields["State"].OriginalValue
                 && revision.Index < (int)eventArgs.TFSEventItem.Fields[FieldNames.Rev].NewValue)
                 .OrderByDescending(revision => revision.Index).ToList();

            if (!revisions.Any()) return;
            Double inprogressTime = 0;
            if (wi.Fields.Contains(FieldNames.InProgressTimeHours) && wi[FieldNames.InProgressTimeHours] != null)
                inprogressTime = double.Parse(wi[FieldNames.InProgressTimeHours].ToString());

            var lastInProgressRevision = revisions.First();
            if (!lastInProgressRevision.Fields.Contains("Changed Date")) return;
            // CultureInfo _cultureInfo = CultureInfo.CreateSpecificCulture("fa-ir");

            var newChangedTime = DateTime.Parse(lastInProgressRevision.Fields["Changed Date"].Value.ToString());

            var t = (DateTime.Parse(wi[FieldNames.ChangedDate].ToString()) - newChangedTime)
              .Add(new TimeSpan(0, 0, Convert.ToInt32(inprogressTime * 3600)));

            if (t.TotalHours < 0) throw new Exception(string.Format("For totalhours '{0}', The Time couldn't be NEGATIVE!", t.TotalHours));
            wi.Open();
            wi[FieldNames.InProgressTimeHours] = t.TotalHours.ToString("0.##");
            wi[FieldNames.InProgressTimeSpan] = t.ToString();
            wi.Save();
            wi.Close();
        }

        /// <summary>
        /// Actions to Take when the Tasks Were Created
        /// 1 - Converting newly created tasks to CRM's Activity
        /// </summary>
        /// <param name="eventArgs"></param>
        private void GlobalTasksCreationHandler(TFSEventArgs eventArgs)
        {
            CreateActivity(eventArgs);
        }

        private void CreateActivity(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var tfsEventItem = eventArgs.TFSEventItem;

            var crmHelper = new CrmService();
            var activityNumber = "";
            var activity = new Entity
            {
                LogicalName = CrmService.TasksActivityMapping[tfsEventItem[FieldNames.Type].NewValue.ToString()]
            };
            activity.Attributes["subject"] = tfsEventItem.Title;

            //Set Parent RFC as it's parent
            var parentWi = _helper.GetParentWorkItem(tfsEventItem.Id, WorkItemLinkType.Topology.Tree);
            if (parentWi == null) throw new Exception("The specified workitem doesn't have any associated parent.");
            if (parentWi[FieldNames.Type].ToString() != RequestType.Bug && parentWi[FieldNames.Type].ToString() != RequestType.ProductBacklogItem)
                parentWi = _helper.GetParentWorkItem(parentWi.Id, WorkItemLinkType.Topology.Tree);

            if (parentWi[FieldNames.RFCId] == null || string.IsNullOrEmpty(parentWi[FieldNames.RFCId].ToString())) return;

            activity.Attributes["regardingobjectid"] = new EntityReference("cmdb_changerequest",
                new Guid(parentWi[FieldNames.RFCId].ToString()));

            if (tfsEventItem[FieldNames.AssignedTo] != null)
            {
                string assignedTo = tfsEventItem[FieldNames.AssignedTo].NewValue.ToString();
                if (assignedTo.Contains(eventArgs.TFSEventItem.ProjectName))
                {
                    //GetFieldValue CRM Entity Mapping
                    var MappingEntity = crmHelper.GetEntityByField("cmdb_tfsmapping", "cmdb_fromfieldmappingvalue", assignedTo).FirstOrDefault();
                    //Set User as activity owener
                    if (MappingEntity != null)
                        activity.Attributes["ownerid"] = MappingEntity.Attributes["cmdb_relatedteam"];

                }
                else
                {
                    var identityService = new IdentityService(_helper.TeamProjectCollectionInstance);
                    var userDomainName = identityService.GetTFSIdentity(assignedTo,
                              IdentitySearchFactor.DisplayName);
                    //GetFieldValue CRM User from the user's domainName
                    var crmUserReference = crmHelper.GetCRMUserByDomainName(userDomainName.UniqueName);
                    //Set User as activity owener
                    activity.Attributes["ownerid"] = crmUserReference;
                }
            }

            if ((string)tfsEventItem[FieldNames.Type].NewValue == FieldValues.WI_DevTask)
            {
                if (tfsEventItem[FieldNames.ChangeType] != null)
                    activity.Attributes["cmdb_changetype"] =
                            new OptionSetValue(crmHelper.GetAttributeId("cmdb_changeactivity", "cmdb_changetype",
                                tfsEventItem[FieldNames.ChangeType].NewValue.ToString()));

                if (tfsEventItem.Contains(FieldNames.PlannedReleaseVersion))
                    activity.Attributes["new_releaseversion"] =
                        new OptionSetValue(crmHelper.GetAttributeId("cmdb_changeactivity", "new_releaseversion",
                            tfsEventItem[FieldNames.PlannedReleaseVersion].NewValue.ToString()));

                if (tfsEventItem.Contains(FieldNames.BaseLineWork))
                    activity.Attributes[FieldNames.CRM_Activity_ScheduleddurationH] = double.Parse(tfsEventItem[FieldNames.BaseLineWork].NewValue.ToString());

                if (tfsEventItem.Contains(FieldNames.NeginLiteBranchVersion))
                {
                    var neginLiteBranchVersionEntity = crmHelper.GetEntityByField(FieldNames.CRM_NeginliteVersion_EntitySchemaName, FieldNames.CRM_NeginliteVersion_Name, tfsEventItem[FieldNames.NeginLiteBranchVersion].NewValue.ToString()).FirstOrDefault();
                    activity.Attributes[FieldNames.CRM_ChangeActivity_NeginLiteBranchVersion] = new EntityReference(FieldNames.CRM_NeginliteVersion_EntitySchemaName, neginLiteBranchVersionEntity.Id);

                }
                if (tfsEventItem.Contains(FieldNames.NeginLiteTrunkVersion))
                {
                    var neginliteTrunkVersionEntity = crmHelper.GetEntityByField(FieldNames.CRM_NeginliteVersion_EntitySchemaName, FieldNames.CRM_NeginliteVersion_Name, tfsEventItem[FieldNames.NeginLiteTrunkVersion].NewValue.ToString()).FirstOrDefault();
                    activity.Attributes[FieldNames.CRM_ChangeActivity_NeginliteTrunkVersion] = new EntityReference(FieldNames.CRM_NeginliteVersion_EntitySchemaName, neginliteTrunkVersionEntity.Id);
                }

                if (tfsEventItem.Contains(FieldNames.Client))
                    activity.Attributes[FieldNames.CRM_ChangeActivity_Client] =
                        (tfsEventItem[FieldNames.Client].NewValue.ToString().ToLower() == "yes" ? new OptionSetValue(1) : new OptionSetValue(0));

                if (tfsEventItem.Contains(FieldNames.Server))
                    activity.Attributes[FieldNames.CRM_ChangeActivity_Server] =
                        (tfsEventItem[FieldNames.Server].NewValue.ToString().ToLower() == "yes" ? new OptionSetValue(1) : new OptionSetValue(0));

                if (tfsEventItem.Contains(FieldNames.WebConsole))
                    activity.Attributes[FieldNames.CRM_ChangeActivity_WebConsole] =
                        (tfsEventItem[FieldNames.WebConsole].NewValue.ToString().ToLower() == "yes" ? new OptionSetValue(1) : new OptionSetValue(0));

                if (tfsEventItem.Contains(FieldNames.ChangeNumber))
                    activity.Attributes[FieldNames.CRM_ChangeActivity_Changenumber] = tfsEventItem.Fields[FieldNames.ChangeNumber].NewValue.ToString();
            }
            else if ((string)tfsEventItem[FieldNames.Type].NewValue == FieldValues.WI_TestTask)
            {
                if (tfsEventItem[FieldNames.TestType] != null)
                    activity.Attributes["new_testtype"] =
                            new OptionSetValue(crmHelper.GetAttributeId("cmdb_testactivity", "new_testtype",
                                tfsEventItem[FieldNames.TestType].NewValue.ToString()));

                if (tfsEventItem.Contains(FieldNames.PlannedReleaseVersion))
                    activity.Attributes["cmdb_releaseversion"] =
                        new OptionSetValue(crmHelper.GetAttributeId("cmdb_testactivity", "cmdb_releaseversion",
                            tfsEventItem[FieldNames.PlannedReleaseVersion].NewValue.ToString()));
                if (tfsEventItem.Contains(FieldNames.BaseLineWork))
                    activity.Attributes[FieldNames.CRM_Activity_ScheduleddurationH] = double.Parse(tfsEventItem[FieldNames.BaseLineWork].NewValue.ToString());

                if (tfsEventItem[FieldNames.IsReopen] != null)
                    activity.Attributes["new_isreopened"] = tfsEventItem[FieldNames.IsReopen].NewValue.ToString() == "Yes";

                if (tfsEventItem[FieldNames.ChangeListNecessity] != null)
                    activity.Attributes[FieldNames.CRM_TestActivity_ChangeListNecessity] = tfsEventItem[FieldNames.ChangeListNecessity].NewValue.ToString() == "Yes";

                if (tfsEventItem[FieldNames.DevelopTest] != null)
                    activity.Attributes[FieldNames.CRM_TestActivity_DevelopTest] = tfsEventItem[FieldNames.DevelopTest].NewValue.ToString() == "Yes";

            }
            else if ((string)tfsEventItem[FieldNames.Type].NewValue == FieldValues.WI_AnalysisTask)
            {
                if (tfsEventItem.Contains(FieldNames.BaseLineWork))
                    activity.Attributes[FieldNames.CRM_AnalysisActivity_ScheduleddurationH] = double.Parse(tfsEventItem[FieldNames.BaseLineWork].NewValue.ToString());
            }
            //Add Workitem and Parent Ids to The Activity
            activity.Attributes["cmdb_workitemnumber"] = tfsEventItem.Id;
            activity.Attributes["cmdb_parentworkitemnumber"] = parentWi.Id;

            var activityId = crmHelper.CreateEntity(activity);

            //Set Activity Number
            if ((string)tfsEventItem[FieldNames.Type].NewValue == FieldValues.WI_DevTask)
            {
                var changeActivity = crmHelper.GetEntityByField(FieldNames.CRM_ChangeActivity_EntitySchemaName, "activityid", activityId.ToString()).FirstOrDefault();
                activityNumber = changeActivity.Attributes[FieldNames.CRM_ChangeActivity_ChangeActivityNumber].ToString();
            }
            else if ((string)tfsEventItem[FieldNames.Type].NewValue == FieldValues.WI_TestTask)
            {
                var testActivity = crmHelper.GetEntityByField(FieldNames.CRM_TestActivity_EntitySchemaName, "activityid", activityId.ToString()).FirstOrDefault();
                activityNumber = testActivity.Attributes[FieldNames.CRM_TestActivity_TestActivityNumber].ToString();
            }
            else if ((string)tfsEventItem[FieldNames.Type].NewValue == FieldValues.WI_AnalysisTask)
            {
                var analysisActivity = crmHelper.GetEntityByField(FieldNames.CRM_AnalysisActivity_EntitySchemaName.ToString(), "activityid", activityId.ToString()).FirstOrDefault();
                activityNumber = analysisActivity.Attributes[FieldNames.CRM_AnalysisActivity_AnalysisActivityNumber].ToString();
            }
            var wi = _helper.GetWorkItem(tfsEventItem.Id);
            wi.Open();
            wi[FieldNames.ActivityId] = activityId.ToString();
            wi[FieldNames.ActivityNumber] = activityNumber;
            wi[FieldNames.ReferenceNumber] = parentWi[FieldNames.RFCNumber];
            wi.Save();
            wi.Close();

        }
        /// <summary>
        /// به ازای هر نفر در یک Area 
        /// فقط یک تسک می تواند Inprogress  
        /// باشد
        /// </summary>
        /// <param name="eventArgs"></param>
        private void SetSingleWorkItemInProgress(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            var wiAssignTo = wi["Assigned To"];

            if (wiAssignTo == null || wiAssignTo.ToString().Contains(eventArgs.TFSEventItem.ProjectName)) //Check if Workitem 'Assign to' is a Team
                return;

            var wiAssignQuery = @"SELECT * FROM WorkItems WHERE" +
                                          "( [System.WorkItemType] = 'Dev Task'" +
                                          " or [System.WorkItemType] = 'Analysis Task'" +
                                          " or [System.WorkItemType] = 'Test Task' " +
                                          " or [System.WorkItemType] = 'Task' )" +
                                          " AND [System.State] = 'In Progress'" +
                                          " AND [System.AssignedTo] = '" + wiAssignTo +
                                          "' AND [System.Id] <> '" + wi.Id +
                                          "' AND [System.AreaId] = '" + wi.AreaId +
                                          "' ORDER BY [System.Id]";

            var workItems = _helper.GetWorkItemsByQuery(wiAssignQuery);

            foreach (var wItem in workItems)
            {
                wItem.Open();
                wItem["State"] = "Blocked";
                wItem.Save();
                wItem.Close();
            }
        }

        /// <summary>
        /// Update PRV by MaxPrv in PBIs & Bugs
        /// </summary>
        /// <param name="eventArgs"></param>
        private void UpdatePRVInFeature(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var feature = _helper.GetParentWorkItem(eventArgs.TFSEventItem.Id, WorkItemLinkType.Topology.Tree);
            var prvs = _helper.GetGlobalListValues(FieldNames.Global_PlannedReleaseVersion).ToList();
            if (feature == null) return;
            var PBIBugs = _helper.GetLinkedWorkItems(feature.Id, WorkItemLinkType.Topology.Tree).ToList();

            var isValid = PBIBugs.All(pbIorBug => pbIorBug.Fields[FieldNames.PlannedReleaseVersion].Value != null && !string.IsNullOrWhiteSpace(pbIorBug.Fields[FieldNames.PlannedReleaseVersion].Value.ToString()));
            if (!isValid) return;
            //find Max PRV
            var maxIndex = 0;
            foreach (var pbIorBug in PBIBugs)
            {
                var wiPRV = pbIorBug.Fields[FieldNames.PlannedReleaseVersion].Value.ToString();
                var selectedVersionIndex = prvs.FindIndex(s => s == wiPRV);
                maxIndex = maxIndex < selectedVersionIndex ? selectedVersionIndex : maxIndex;
            }
            //update PRV in feature


            feature.Open();
            feature[FieldNames.PlannedReleaseVersion] = prvs[maxIndex];
            feature.Save();
            feature.Close();
        }

        private void ScrumProcessTemplateUpdateBaselineWork(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmFieldName = string.Empty;
            crmFieldName = wi.Type.Name == FieldValues.WI_AnalysisTask ? FieldNames.CRM_AnalysisActivity_ScheduleddurationH : FieldNames.CRM_Activity_ScheduleddurationH;

            wi.SyncActivityTexualField(crmFieldName, FieldNames.BaseLineWork,
                CrmService.TasksActivityMapping[wi.Type.Name]);
        }

        private void UpdatePRVInParentwithMaxPrvinDevandTest(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var parentWi = _helper.GetParentWorkItem(eventArgs.TFSEventItem.Id, WorkItemLinkType.Topology.Tree);

            if (parentWi == null || (parentWi.Type.Name != FieldValues.WI_DevTask && parentWi.Type.Name != FieldValues.WI_TestTask && parentWi.Type.Name != FieldValues.WI_ProductBacklogItem && parentWi.Type.Name != FieldValues.WI_Bug)) return;
            var prvs = _helper.GetGlobalListValues(FieldNames.Global_PlannedReleaseVersion).ToList();

            var allLinkWorkItem = _helper.GetLinkedWorkItems(int.Parse(parentWi.Id.ToString()), WorkItemLinkType.Topology.Tree);
            var allDevandTestTasks = allLinkWorkItem.Where(item => (string)item[FieldNames.Type] == FieldValues.WI_DevTask || (string)item[FieldNames.Type] == FieldValues.WI_TestTask);


            //find Max PRV
            var maxIndex = -1;
            foreach (var devorTestTask in allDevandTestTasks)
            {
                var wiPRV = devorTestTask.Fields[FieldNames.PlannedReleaseVersion] != null ? devorTestTask.Fields[FieldNames.PlannedReleaseVersion].Value.ToString() : "";

                if (!string.IsNullOrWhiteSpace(wiPRV) && !((string)devorTestTask[FieldNames.Type] == FieldValues.WI_TestTask
                    && devorTestTask[FieldNames.TestType].ToString() == "Other Line Test"))
                {
                    var selectedVersionIndex = prvs.FindIndex(s => s == wiPRV);
                    maxIndex = maxIndex < selectedVersionIndex ? selectedVersionIndex : maxIndex;
                }
            }

            //update PRV in ParentWi
            if (maxIndex < 0) return;
            parentWi.Open();
            parentWi[FieldNames.PlannedReleaseVersion] = prvs[maxIndex];
            parentWi.Save();
            parentWi.Close();

        }

        private void TfsCrmSyncTitle(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;

            wi.SyncActivityTexualField(FieldNames.CRM_Activity_Subject, FieldNames.Title,
                CrmService.TasksActivityMapping[wi.Type.Name]);

        }
        private void TfsCrmSyncDescription(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var crmService = new CrmService();
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;

            var regex = new Regex(@"<[^>]+>|&nbsp;", RegexOptions.IgnoreCase);
            var des = wi.Description.Replace("</p>", "\n");
            var description = regex.Replace(des, "");

            crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), CrmService.TasksActivityMapping[wi[FieldNames.Type].ToString()], FieldNames.CRM_Activity_Description, description);
        }
        private void TfsCrmSyncAssignedTo(TFSEventArgs eventArgs)
        {
            var logData = string.Empty;
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmService = new CrmService();
            var crmAssigneeType = "systemuser";

            var assignedToEntityId = new Guid();

            var assignedTo = eventArgs.TFSEventItem.Fields[FieldNames.AssignedTo].NewValue.ToString();

            logData += string.Format("The Assignee is {0} |", assignedTo);
            if (assignedTo.Contains(wi.Project.Name))
            {
                crmAssigneeType = "team";
                //GetFieldValue CRM Entity Mapping
                var mappingEntity = crmService.GetEntityByField("cmdb_tfsmapping", "cmdb_fromfieldmappingvalue", assignedTo).FirstOrDefault();
                logData += string.Format("The Mapping Entity is {0} |", mappingEntity);
                //Set team as activity owener
                if (mappingEntity != null)
                    assignedToEntityId = ((EntityReference)mappingEntity.Attributes["cmdb_relatedteam"]).Id;
                logData += string.Format("The Related Entity is {0} |", ((EntityReference)mappingEntity.Attributes["cmdb_relatedteam"]).Name);
            }
            else
            {
                var identityService = new IdentityService(_helper.TeamProjectCollectionInstance);
                var userDomainName = identityService.GetTFSIdentity(assignedTo,
                          IdentitySearchFactor.DisplayName);
                //GetFieldValue CRM User from the user's domainName
                assignedToEntityId = crmService.GetCRMUserByDomainName(userDomainName.UniqueName).Id;
            }
            var activityEntitySchemaName = CrmService.TasksActivityMapping[wi[FieldNames.Type].ToString()];
            if (assignedToEntityId != new Guid())
            {
                crmService.ChangeEntityAssignee(assignedToEntityId, activityEntitySchemaName, new Guid(wi[FieldNames.ActivityId].ToString()), crmAssigneeType);
                logData += string.Format("Assign have done");
            }
            if (wi.State == FieldValues.State_InProgress)
            {
                wi.Open();
                wi.State = FieldValues.State_Blocked;
                wi.Save();
                wi.Close();
            }
            Logger.Logger.LogExtention(logData);
        }

        /// <summary>
        /// update InProgressTimeSpan when InProgressTimeH Changeed
        /// </summary>
        private void ScrumProcessTemplateUpdateInProgressTimeSpan(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            var inprogressTimeH = wi[FieldNames.InProgressTimeHours] != null
                ? double.Parse(wi[FieldNames.InProgressTimeHours].ToString())
                : 0;
            var t = new TimeSpan(0, 0, Convert.ToInt32(inprogressTimeH * 3600));
            wi.Open();
            wi[FieldNames.InProgressTimeSpan] = t.ToString();
            wi.Save();
            wi.Close();
        }

        private void ScrumProcessTemplateChangeLink(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (!string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            eventArgs.TFSEventItem.Fields.AddRange(wi.Fields.OfType<Field>()
                .Where(field => !eventArgs.TFSEventItem.Fields.Contains(field.ReferenceName))
                .Select(ea => new TFSField() { Id = ea.ReferenceName, NewValue = ea.Value }));
            CreateActivity(eventArgs);
        }
        /// <summary>
        /// add attachment to activity
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateSyncAttachments(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var crmService = new CrmService();
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);

            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            foreach (var tfsfile in eventArgs.TFSEventItem.TFSFiles)
                if (tfsfile.State == AtTimeState.Added)
                {
                    Attachment attachment = wi.Attachments.OfType<Attachment>().Single(attach => attach.Name == tfsfile.Name);
                    crmService.AddCRMNote(TFSFileHelper.GeTFSFile(attachment), new EntityReference(CrmService.TasksActivityMapping[wi[FieldNames.Type].ToString()], new Guid(wi[FieldNames.ActivityId].ToString())));
                }

        }

        private void ScrumProcessTemplateSyncHistory(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var crmService = new CrmService();
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;

            crmService.AddCRMNote(eventArgs.TFSEventItem[FieldNames.History].NewValue.ToString(), new EntityReference(CrmService.TasksActivityMapping[wi[FieldNames.Type].ToString()], new Guid(wi[FieldNames.ActivityId].ToString())), eventArgs.TFSEventItem["System.ChangedBy"].NewValue.ToString());

        }

        private void TransferDataFromFeaturetoPbiBug(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (!string.IsNullOrEmpty(wi[FieldNames.RFCId].ToString())) return;
            var parent = _helper.GetParentWorkItem(wi.Id, WorkItemLinkType.Topology.Tree);
            if (parent == null) return;
            if (parent.Type.Name != FieldValues.WI_Feature) return;

            wi.Open();
            wi[FieldNames.RFCNumber] = parent[FieldNames.RFCNumber];
            wi[FieldNames.RFCId] = parent[FieldNames.RFCId];
            wi[FieldNames.Customer] = parent[FieldNames.Customer];
            wi[FieldNames.PlannedReleaseVersion] = parent[FieldNames.PlannedReleaseVersion];
            wi[FieldNames.CRM_Link] = parent[FieldNames.CRM_Link];
            wi[FieldNames.Commited] = parent[FieldNames.Commited];
            wi.Save();
            wi.Close();
        }
    }


}

