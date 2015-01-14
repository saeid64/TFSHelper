using System;
using log4net.Core;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Tosan.TeamFoundation.Plugin.Core;
using Tosan.TeamFoundation.Plugin.Core.Helper;
using Tosan.TeamFoundation.Plugin.Core.Utility;
using Tosan.TeamFoundation.Plugins.Helper;
using Tosan.TeamFoundation.Plugins.Resources;
using Microsoft.Xrm.Sdk;
using System.Linq;
using Tosan.TFSHelper;

namespace Tosan.TeamFoundation.Plugins.Scrum
{
    // comment for test
    class TestTaskHandler : IEventHandler
    {
        private WorkItemService _helper;

        public void Register(TFSEventAggregator aggregator)
        {
            var testTaskUpdateStateFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
                && item.Fields[FieldNames.State] != null
                && item.Fields[FieldNames.State].IsDirty
                && !(item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_InProgress && item.Fields[FieldNames.State].OldValue.ToString() == FieldValues.State_Blocked)
                && !(item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Blocked && item.Fields[FieldNames.State].OldValue.ToString() == FieldValues.State_InProgress)
               && item.Fields[FieldNames.State].NewValue.ToString() != FieldValues.State_Done);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateTestTaskStateChanged, testTaskUpdateStateFilter);

            var testTaskUpdateStateToDoneFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
               && item.Fields[FieldNames.State] != null
               && item.Fields[FieldNames.State].IsDirty
               && item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Done);

            aggregator.Subscribe(RequestTypeEnum.PreUpdate, ScrumProcessTemplateTestTaskStateChangedToDone, testTaskUpdateStateToDoneFilter, 0);

            var testTaskUpdateStateToDoneRejectedFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
              && item.Fields[FieldNames.State] != null
              && item.Fields[FieldNames.State].IsDirty
              && (item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Done
              && item.Fields[FieldNames.Reason].NewValue.ToString() == FieldValues.Reason_Rejected));

            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateTestTaskCreated, testTaskUpdateStateToDoneRejectedFilter);

            var pretestTaskUpdateStateFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
              && item.Fields[FieldNames.State] != null
              && item.Fields[FieldNames.State].IsDirty
              && item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Done);

            aggregator.Subscribe(RequestTypeEnum.PreUpdate, ScrumProcessTemplateTestTaskStateChangedValidation, pretestTaskUpdateStateFilter, 1);

            var testTaskStateChangeToDoneRejectedFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask
               && item.Fields[FieldNames.State] != null
               && item.Fields[FieldNames.State].IsDirty
               && item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Done
               && item.Fields[FieldNames.Reason].NewValue.ToString() == FieldValues.Reason_Rejected);

            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateReopencounInc, testTaskStateChangeToDoneRejectedFilter);


            var filterChangeTestType = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.TestType);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncTestTypeField, filterChangeTestType);

            var filterChangeList = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.ChangeList);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncChangeListField, filterChangeList);

            var filterChangeListNecessity = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.ChangeListNecessity);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncChangeListNecessityField, filterChangeListNecessity);

            var filterChangeRelatedTestCases = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.RelatedTestCases);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncRelatedTestCasesField, filterChangeRelatedTestCases);

            var filterTestResultforEachPath = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.TestResultforEachPath);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncTestResultforEachPathField, filterTestResultforEachPath);

            var filterChangeTestPaths = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.TestPaths);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncTestPathsField, filterChangeTestPaths);

            var filterChangeReferencePath = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.ReferencePath);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncReferencePathField, filterChangeReferencePath);

            var filterChangeTestEnviromentConfiguration = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.TestEnviromentConfiguration);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncTestEnviromentConfigurationField, filterChangeTestEnviromentConfiguration);

            var filterChangePRV = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask && item.Fields[FieldNames.PlannedReleaseVersion] != null && item.Fields[FieldNames.PlannedReleaseVersion].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncPRVinTA, filterChangePRV);

            var filterDevelopTest = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_TestTask, FieldNames.DevelopTest);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncDevelopTestField, filterDevelopTest);

        }

        private void ScrumProcessTemplateSyncChangeListNecessityField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmService = new CrmService();
            if (wi[FieldNames.ChangeListNecessity] != null && !string.IsNullOrEmpty(wi[FieldNames.ChangeListNecessity].ToString()))
                crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName,
                    FieldNames.CRM_TestActivity_ChangeListNecessity, wi.Fields[FieldNames.ChangeListNecessity].Value.ToString() == "Yes" ? true : false);

            else
                crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, 
                    FieldNames.CRM_TestActivity_ChangeListNecessity, null);
        }

        #region CRM Synchorinization Fields Methods
        private void ScrumProcessTemplateSyncTestEnviromentConfigurationField(TFSEventArgs eventArgs)
        {
            UpdateCRMStrFieldsValue(eventArgs, FieldNames.CRM_TestActivity_TestEnviromentConfiguration, FieldNames.TestEnviromentConfiguration);
        }

        private void ScrumProcessTemplateSyncReferencePathField(TFSEventArgs eventArgs)
        {
            UpdateCRMStrFieldsValue(eventArgs, FieldNames.CRM_TestActivity_ReferencePath, FieldNames.ReferencePath);
        }

        private void ScrumProcessTemplateSyncTestPathsField(TFSEventArgs eventArgs)
        {
            UpdateCRMStrFieldsValue(eventArgs, FieldNames.CRM_TestActivity_TestPath, FieldNames.TestPaths);
        }

        private void ScrumProcessTemplateSyncTestResultforEachPathField(TFSEventArgs eventArgs)
        {
            UpdateCRMStrFieldsValue(eventArgs, FieldNames.CRM_TestActivity_TestResultforEachPath, FieldNames.TestResultforEachPath);
        }

        private void ScrumProcessTemplateSyncRelatedTestCasesField(TFSEventArgs eventArgs)
        {
            UpdateCRMStrFieldsValue(eventArgs, FieldNames.CRM_TestActivity_RelatedTestCases, FieldNames.RelatedTestCases);
        }

        private void UpdateCRMStrFieldsValue(TFSEventArgs eventArgs, string crmFieldName, string fieldName)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var crmService = new CrmService();
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            wi.SyncActivityTexualField(crmFieldName, fieldName, FieldNames.CRM_TestActivity_EntitySchemaName);
        }
        #endregion
        //12  Test Task Program Fields Changed
        /// <summary>
        /// for  Scrum Process Template
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateTestTaskStateChanged(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmService = new CrmService();

            if (wi.State == FieldValues.State_Removed)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, (int)CRM_TestactivityStateCode.Canceled, 2);

            else if (wi.State == FieldValues.State_ToDo)
            {
                if (!wi.Fields.Contains(FieldNames.PlannedReleaseVersion) ||
                    wi.Fields[FieldNames.PlannedReleaseVersion].Value == null ||
                   string.IsNullOrEmpty(wi.Fields[FieldNames.PlannedReleaseVersion].Value.ToString()))
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, (int)CRM_TestactivityStateCode.Open_PendingPlanning, 0);
                else crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, (int)CRM_TestactivityStateCode.Open_Planned, 0);

                if (wi.Reason == FieldValues.Reason_Rejectedfromtest)
                    crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, FieldNames.CRM_TestActivity_IsReopened, true);
            }
            if (wi.State == FieldValues.State_InProgress)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, (int)CRM_TestactivityStateCode.Open_PendingTest, 0);

        }
        private void ScrumProcessTemplateTestTaskStateChangedToDone(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var state = eventArgs.TFSEventItem.Fields[FieldNames.State];
            var reason = eventArgs.TFSEventItem.Fields[FieldNames.Reason];
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmService = new CrmService();

            try
            {
                if (state.NewValue.ToString() == FieldValues.State_Done && reason.NewValue.ToString() == FieldValues.Reason_Accepted)
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, (int)CRM_TestactivityStateCode.Completed_Confirmed, 1);
                else if (state.NewValue.ToString() == FieldValues.State_Done && reason.NewValue.ToString() == FieldValues.Reason_Rejected)
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, (int)CRM_TestactivityStateCode.Completed_Rejected, 1);
                else if (state.NewValue.ToString() == FieldValues.State_Done && reason.NewValue.ToString() == FieldValues.Reason_Canceled)
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, (int)CRM_TestactivityStateCode.Canceled, 2);

            }
            catch (Exception ex)
            {
                var message = string.Format(ErrorMessages.CRMError, ex.Message);
                eventArgs.ResponseAction(false, message);
                throw new TosanTeamFoundationException(message, Level.Warn);
            }
        }
        private void ScrumProcessTemplateTestTaskCreated(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (wi[FieldNames.CreateNewTestTaskAfterRejected].ToString() == "Yes")
            {
                CreateTestTask(wi);
                wi.Open();
                wi[FieldNames.CreateNewTestTaskAfterRejected] = "No";
                wi.Save();
                wi.Close();
            }
        }
        /// <summary>
        ///   در این وضعیت feature در Reopencount
        ///       یک عدد اضافه شود
        ///     ؛ در اینصورت is rejected from test=Yes
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateReopencounInc(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            // WorkItem wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            //   if (wi.State == FieldValues.State_Done && wi.Reason == FieldValues.Reason_Rejected) return;
            var crmService = new CrmService();
            var parentWi = _helper.GetParentWorkItem(eventArgs.TFSEventItem.Id, WorkItemLinkType.Topology.Tree);
            if (parentWi == null) return;
            if (parentWi[FieldNames.Type].ToString() != RequestType.Bug && parentWi[FieldNames.Type].ToString() != RequestType.ProductBacklogItem)
                parentWi = _helper.GetParentWorkItem(parentWi.Id, WorkItemLinkType.Topology.Tree);
            if (parentWi == null) return;
            var grandparent = _helper.GetParentWorkItem(parentWi.Id, WorkItemLinkType.Topology.Tree);
            if (grandparent == null) return;
            grandparent.Open();
            grandparent[FieldNames.ReopenCount] = (grandparent[FieldNames.ReopenCount] == null) ? 1 : ((int)grandparent[FieldNames.ReopenCount] + 1);
            grandparent.Save();
            if (!(grandparent[FieldNames.RFCId] == null || string.IsNullOrEmpty(grandparent[FieldNames.RFCId].ToString())))
                crmService.UpdateField(new Guid(grandparent[FieldNames.RFCId].ToString()), FieldNames.CRM_RFC_EntitySchemaName, FieldNames.CRM_RFC_IsRejectedfromTest, true);
            grandparent.Close();
        }

        /// <summary>
        ///  هنگام تغییر وضعیت ریزن Accepted,done
        ///  باید فیلدهای زیر مقدار داشته باشد
        ///  TestType
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateTestTaskStateChangedValidation(TFSEventArgs eventArgs)
        {
            var message = string.Empty;
            try
            {
                _helper = eventArgs.ContextTFSHelper as WorkItemService;
                var state = eventArgs.TFSEventItem.Fields[FieldNames.State];
                var reason = eventArgs.TFSEventItem.Fields[FieldNames.Reason];
                var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
                var strFields = "";
                if ((string)state.NewValue == FieldValues.State_Done &&
                    (string)reason.NewValue == FieldValues.Reason_Accepted)
                {
                    var changeListValid = wi.Fields[FieldNames.ChangeListNecessity].Value.ToString() == "Yes";
                    strFields = MessageBuilder.BuildFieldNamesMessage(wi, changeListValid
                        ? new[] { FieldNames.PlannedReleaseVersion, FieldNames.TestType, FieldNames.ChangeList }
                        : new[] { FieldNames.PlannedReleaseVersion, FieldNames.TestType });
                    if (string.IsNullOrEmpty(strFields)) return;
                    message = string.Format(ErrorMessages.FieldRequireFilling, strFields);
                    throw new Exception();
                }
                else if ((string)state.NewValue == FieldValues.State_Done &&
                         (string)reason.NewValue == FieldValues.Reason_Rejected)
                {
                    strFields = MessageBuilder.BuildFieldNamesMessage(wi, new[] { FieldNames.PlannedReleaseVersion, FieldNames.TestType, FieldNames.BaselineWorkforNewTestTask });

                    if (string.IsNullOrEmpty(strFields)) return;
                    message = string.Format(ErrorMessages.FieldRequireFilling, strFields);

                    throw new Exception();
                }
            }

            catch (Exception)
            {
                eventArgs.ResponseAction(false, message);
                throw new TosanTeamFoundationException(message, Level.Warn);
            }
        }


        /// <summary>
        ///  Update TestTypeField in CRM
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateSyncTestTypeField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            wi.SyncActivityOptionSetValueField(FieldNames.CRM_TestActivity_TestType, FieldNames.TestType, FieldNames.CRM_TestActivity_EntitySchemaName);
        }

        private void ScrumProcessTemplateSyncChangeListField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wiAParent = _helper.GetParentWorkItem(eventArgs.TFSEventItem.Id, WorkItemLinkType.Topology.Tree);
            if (wiAParent == null || !wiAParent.Fields.Contains(FieldNames.RFCId)) return;
            var fieldValue = eventArgs.TFSEventItem[FieldNames.ChangeList] == null || string.IsNullOrEmpty(eventArgs.TFSEventItem[FieldNames.ChangeList].NewValue.ToString()) ? null
                : eventArgs.TFSEventItem[FieldNames.ChangeList].NewValue + " - " + eventArgs.TFSEventItem[FieldNames.AssignedTo].NewValue;
            var crmService = new CrmService();
            var changeRequestId = new Guid(wiAParent[FieldNames.RFCId].ToString());
            var changeRequest = crmService.RetrieveEntity(changeRequestId, FieldNames.CRM_RFC_EntitySchemaName);
            var changeListValue = changeRequest.Contains(FieldNames.CRM_RFC_ChangeList) ? changeRequest[FieldNames.CRM_RFC_ChangeList] + Environment.NewLine + fieldValue : fieldValue;
            crmService.UpdateField(changeRequestId, FieldNames.CRM_RFC_EntitySchemaName, FieldNames.CRM_RFC_ChangeList, changeListValue);
        }

        private void ScrumProcessTemplateSyncPRVinTA(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            var activityId = wi.Fields[FieldNames.ActivityId].Value.ToString();
            if (string.IsNullOrEmpty(activityId)) return;
            if (string.IsNullOrEmpty(wi.Fields[FieldNames.PlannedReleaseVersion].Value.ToString()))
                wi.SyncActivityEntityReferenceFieldEmpty(FieldNames.CRM_Activity_PlannedReleaseVersion, FieldNames.PlannedReleaseVersion, FieldNames.CRM_TestActivity_EntitySchemaName);
            else
                wi.SyncActivityEntityReferenceField(FieldNames.CRM_Activity_PlannedReleaseVersion,
                    FieldNames.PlannedReleaseVersion, FieldNames.CRM_TestActivity_EntitySchemaName,
                    FieldNames.CRM_PlannedReleaseVersion_EntitySchemaName, FieldNames.CRM_PlannedReleaseVersion_Name);
          
            if (wi.State == FieldValues.State_ToDo)
            {
                var crmService = new CrmService();
                var planedReleaseVersionOLd = eventArgs.TFSEventItem.Fields[FieldNames.PlannedReleaseVersion].OldValue.ToString();
                var planedReleaseVersionNew = eventArgs.TFSEventItem.Fields[FieldNames.PlannedReleaseVersion].NewValue.ToString();
                if (!string.IsNullOrEmpty(planedReleaseVersionOLd) && string.IsNullOrEmpty(planedReleaseVersionNew))
                {
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()),
                        FieldNames.CRM_TestActivity_EntitySchemaName,
                        (int) CRM_TestactivityStateCode.Open_PendingPlanning, 0);
                }

                else if (string.IsNullOrEmpty(planedReleaseVersionOLd) && !string.IsNullOrEmpty(planedReleaseVersionNew))
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()),
                    FieldNames.CRM_TestActivity_EntitySchemaName, (int) CRM_TestactivityStateCode.Open_Planned, 0);
        }
        }
        private WorkItem CreateTestTask(WorkItem taRejected)
        {
            var wi = new WorkItem(taRejected.Type);
            wi[FieldNames.Title] = "عدم تاییدی – " + taRejected[FieldNames.Title];
            wi[FieldNames.BaseLineWork] = taRejected[FieldNames.BaselineWorkforNewTestTask];
            wi[FieldNames.RemainingWork] = taRejected[FieldNames.BaselineWorkforNewTestTask];
            wi[FieldNames.Score] = taRejected[FieldNames.BaselineWorkforNewTestTask];
            wi[FieldNames.PlannedReleaseVersion] = taRejected[FieldNames.PlannedReleaseVersion];
            wi[FieldNames.AssignedTo] = taRejected[FieldNames.AssignedTo];
            wi[FieldNames.TestType] = taRejected[FieldNames.TestType];
            wi[FieldNames.State] = FieldValues.State_ToDo;
            wi[FieldNames.Reason] = FieldValues.Reason_Rejectedfromtest;
            wi[FieldNames.Activity] = "Incident";
            wi[FieldNames.AreaId] = taRejected[FieldNames.AreaId];
            wi[FieldNames.IsReopen] = "Yes";
            wi[FieldNames.ChangeListNecessity] = taRejected[FieldNames.ChangeListNecessity];
            wi[FieldNames.DevelopTest] = taRejected[FieldNames.DevelopTest];

            var relatedLink = new RelatedLink(taRejected.Id);
            wi.Links.Add(relatedLink);
            var parent = _helper.GetParentWorkItem(taRejected.Id, WorkItemLinkType.Topology.Tree);
            if (parent != null)
            {
                var linkTypeEnd = _helper.Store.WorkItemLinkTypes.LinkTypeEnds["Parent"];
                wi.WorkItemLinks.Add(new WorkItemLink(linkTypeEnd, parent.Id));
            }


            var validation = wi.Validate();
            wi.Save();

            return wi;
        }

        private void ScrumProcessTemplateSyncDevelopTestField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmService = new CrmService();
            if (wi[FieldNames.DevelopTest] != null && !string.IsNullOrEmpty(wi[FieldNames.DevelopTest].ToString()))
                crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, FieldNames.CRM_TestActivity_DevelopTest, wi.Fields[FieldNames.DevelopTest].Value.ToString() == "Yes" ? true : false);

            else
                crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_TestActivity_EntitySchemaName, FieldNames.CRM_TestActivity_DevelopTest, null);


        }

    }
}
