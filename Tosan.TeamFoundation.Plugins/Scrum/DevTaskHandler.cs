using System;
using System.Text;
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
    class DevTaskHandler : IEventHandler
    {

        private WorkItemService _helper;

        public void Register(TFSEventAggregator aggregator)
        {
            var filter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask);
            aggregator.Subscribe(RequestTypeEnum.Created, ScrumProcessTemplatePBIReopencountadd, filter);

            var filterChangeClient = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask && item.Fields[FieldNames.Client] != null && item.Fields[FieldNames.Client].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncClientField, filterChangeClient);

            var filterChangeServer = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask && item.Fields[FieldNames.Server] != null && item.Fields[FieldNames.Server].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncServerField, filterChangeServer);

            var filterChangeWebConsole = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask && item.Fields[FieldNames.WebConsole] != null && item.Fields[FieldNames.WebConsole].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncWebConsoleField, filterChangeWebConsole);

            var filterChangeChangeType = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask && item.Fields[FieldNames.ChangeType] != null && item.Fields[FieldNames.ChangeType].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncChangeTypeField, filterChangeChangeType);



            var devTaskPreUpdateStateFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
                && item.Fields[FieldNames.State] != null
                && item.Fields[FieldNames.State].IsDirty
                && item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Done);

            aggregator.Subscribe(RequestTypeEnum.PreUpdate, ScrumProcessTemplateDevTaskStateChangedValidation, devTaskPreUpdateStateFilter, 1);
            aggregator.Subscribe(RequestTypeEnum.PreUpdate, ScrumProcessTemplatePreDevTaskStateChangedToDone, devTaskPreUpdateStateFilter, 0);
            aggregator.Subscribe(RequestTypeEnum.PreUpdate, NeginLiteDevTaskStateChangedValidation, devTaskPreUpdateStateFilter, 1);


            var devTaskUpdateStateFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
              && item.Fields[FieldNames.State] != null
              && item.Fields[FieldNames.State].IsDirty
              && !(item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_InProgress && item.Fields[FieldNames.State].OldValue.ToString() == FieldValues.State_Blocked)
              && !(item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Blocked && item.Fields[FieldNames.State].OldValue.ToString() == FieldValues.State_InProgress)
              && item.Fields[FieldNames.State].NewValue.ToString() != FieldValues.State_Done);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateDevTaskStateChanged, devTaskUpdateStateFilter);

            var filterDevTaskUpdateChangeNumber = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask
                && item.Fields[FieldNames.ChangeNumber] != null
                && item.Fields[FieldNames.ChangeNumber].IsDirty);

            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncChangeNumber, filterDevTaskUpdateChangeNumber);

            var filterChangeNeginliteTrunkVersion = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask && item.Fields[FieldNames.NeginLiteTrunkVersion] != null && item.Fields[FieldNames.NeginLiteTrunkVersion].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, SyncNeginliteTrunkVersion, filterChangeNeginliteTrunkVersion);

            var filterChangeNeginLiteBranchVersion = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask && item.Fields[FieldNames.NeginLiteBranchVersion] != null && item.Fields[FieldNames.NeginLiteBranchVersion].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, SyncNeginLiteBranchVersion, filterChangeNeginLiteBranchVersion);

            var filterChangePRV = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_DevTask && item.Fields[FieldNames.PlannedReleaseVersion] != null && item.Fields[FieldNames.PlannedReleaseVersion].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateSyncPRVinCA, filterChangePRV);
        }

        /// <summary>
        ///PBI and bug Reopencount increment when DT added and feature.Reopencount>0
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplatePBIReopencountadd(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService as WorkItemService;
            var parentWi = _helper.GetParentWorkItem(eventArgs.TFSEventItem.Id, WorkItemLinkType.Topology.Tree);
            if (parentWi == null) return;
            if (parentWi[FieldNames.Type].ToString() != RequestType.Bug && parentWi[FieldNames.Type].ToString() != RequestType.ProductBacklogItem)
                parentWi = _helper.GetParentWorkItem(parentWi.Id, WorkItemLinkType.Topology.Tree);
            if (parentWi == null) return;
            var grandparent = _helper.GetParentWorkItem(parentWi.Id, WorkItemLinkType.Topology.Tree);
            if (grandparent == null || grandparent[FieldNames.ReopenCount] == null ||
                ((int)grandparent[FieldNames.ReopenCount]) <= 0) return;
            parentWi.Open();
            parentWi[FieldNames.ReopenCount] = parentWi[FieldNames.ReopenCount] != null ? ((int)parentWi[FieldNames.ReopenCount] + 1) : 1;
            parentWi.Save();
            parentWi.Close();
        }

        private void ScrumProcessTemplatePreDevTaskStateChangedToDone(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var state = eventArgs.TFSEventItem.Fields[FieldNames.State];
            var reason = eventArgs.TFSEventItem.Fields[FieldNames.Reason];
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            try
            {
                if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
                var crmService = new CrmService();
                if (state.NewValue.ToString() == FieldValues.State_Done && reason.NewValue.ToString() == FieldValues.Reason_WorkisCanceled)
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Canceled, 2);
                else if (state.NewValue.ToString() == FieldValues.State_Done && reason.NewValue.ToString() == FieldValues.Reason_WorkFinished)
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Completed, 1);
            }
            catch (Exception ex)
            {
                var message = string.Format(ErrorMessages.CRMError, ex.Message);
                eventArgs.ResponseAction(false, message);
                throw new TosanTeamFoundationException(message, Level.Warn);
            }
        }
        /// <summary>
        /// for Scrum process Template Map State 
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateDevTaskStateChanged(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);

            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmService = new CrmService();
            if (wi.State == FieldValues.State_Removed && wi.Reason == FieldValues.Reason_Removedfromthebacklog)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Canceled, 2);
            else if (wi.State == FieldValues.State_ToDo && wi.Reason == FieldValues.Reason_NewTask && !string.IsNullOrWhiteSpace(wi[FieldNames.BaseLineWork].ToString()))
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Open_Planned, 0);
            else if ((wi.State == FieldValues.State_InProgress || wi.State == FieldValues.State_Blocked) && (wi.Reason == FieldValues.Reason_WorkStarted || wi.Reason == FieldValues.Reason_TheTAskIsUnblocked || wi.Reason == FieldValues.Reason_TheTaskIsBlocked))
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Open_PendingImplementation, 0);

            else if (wi.State == FieldValues.State_ToDo && wi.Reason == FieldValues.Reason_NewTask && string.IsNullOrWhiteSpace(wi[FieldNames.BaseLineWork].ToString()))
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Open_PendingPlanning, 0);
            else if (wi.State == FieldValues.State_ToDo && wi.Reason == FieldValues.Reason_ReconsideringtheTask)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Open_PendingImplementation, 0);
            else if (wi.State == FieldValues.State_ToDo && wi.Reason == FieldValues.Reason_ClosedinError)
            {
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Open_PendingImplementation, 0);
                crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, FieldNames.CRM_ChangeActivity_IsReopened, true);
            }
            else if (wi.State == FieldValues.State_ToDo && wi.Reason == FieldValues.Reason_AdditionalWorkFound)
            {
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Open_PendingImplementation, 0);
                crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, FieldNames.CRM_ChangeActivity_IsReopened, true);
            }
            else if (wi.State == FieldValues.State_InProgress && wi.Reason == FieldValues.Reason_AdditionalWorkFound)
            {
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, (int)CRM_ChangeactivityStateCode.Open_PendingImplementation, 0);
                crmService.UpdateField(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_ChangeActivity_EntitySchemaName, FieldNames.CRM_ChangeActivity_IsReopened, true);
            }

        }

        /// <summary>
        ///  هنگام تغییر وضعیت ریزن workfinished,done
        ///  باید فیلدهای زیر مقدار داشته باشد
        ///  Client,Server,WebConsole,ChangeType,PRV
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateDevTaskStateChangedValidation(TFSEventArgs eventArgs)
        {
            var message = "";
            try
            {
                var state = eventArgs.TFSEventItem.Fields[FieldNames.State];
                var reason = eventArgs.TFSEventItem.Fields[FieldNames.Reason];
                _helper = eventArgs.ContextTFSHelper as WorkItemService;


                if ((string)state.NewValue == FieldValues.State_Done &&
                    (string)reason.NewValue == FieldValues.Reason_WorkFinished)
                {
                    var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);

                    var strFields = MessageBuilder.BuildFieldNamesMessage(wi, new[] { FieldNames.ChangeType, FieldNames.Client,
                        FieldNames.Server, FieldNames.PlannedReleaseVersion, FieldNames.WebConsole });

                    if (string.IsNullOrEmpty(strFields)) return;
                    message = string.Format(ErrorMessages.FieldRequireFilling, strFields);

                    throw new Exception();
                }
            }
            catch
                (Exception)
            {
                eventArgs.ResponseAction(false, message);
                Logger.Logger.LogExtention(message);

                throw new TosanTeamFoundationException(message, Level.Warn);
            }

        }


        /// <summary>
        /// در NeginLite
        ///  هنگام تغییر وضعیت ریزن workfinished,done
        ///  باید فیلدهای زیر مقدار داشته باشد
        ///  Client,Server,WebConsole,ChangeType,Trunk,Branch,PRV
        /// </summary>
        /// <param name="eventArgs"></param>
        private void NeginLiteDevTaskStateChangedValidation(TFSEventArgs eventArgs)
        {
            var message = "";
            try
            {
                var state = eventArgs.TFSEventItem.Fields[FieldNames.State];
                var reason = eventArgs.TFSEventItem.Fields[FieldNames.Reason];
                _helper = eventArgs.ContextTFSHelper as WorkItemService;

                if ((string)state.NewValue == FieldValues.State_Done && (string)reason.NewValue == FieldValues.Reason_WorkFinished)
                {
                    var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);

                    var strFields = MessageBuilder.BuildFieldNamesMessage(wi, new[] { FieldNames.ChangeType, FieldNames.Client,
                        FieldNames.Server, FieldNames.PlannedReleaseVersion, FieldNames.WebConsole,FieldNames.NeginLiteBranchVersion, FieldNames.NeginLiteTrunkVersion});

                    if (string.IsNullOrEmpty(strFields)) return;
                    message = string.Format(ErrorMessages.FieldRequireFilling, strFields);

                    throw new Exception();
                }
            }
            catch (Exception)
            {
                eventArgs.ResponseAction(false, message);
                Logger.Logger.LogExtention(message);
                throw new TosanTeamFoundationException(message, Level.Warn);
            }

        }

        /// <summary>
        /// Update WebConsoleField in CRM
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateSyncWebConsoleField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            wi.SyncActivityOptionSetValueField(FieldNames.CRM_ChangeActivity_WebConsole, FieldNames.WebConsole,
                FieldNames.CRM_ChangeActivity_EntitySchemaName);
        }
        /// <summary>
        ///  Update ServerField in CRM
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateSyncServerField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            wi.SyncActivityOptionSetValueField(FieldNames.CRM_ChangeActivity_Server, FieldNames.Server, FieldNames.CRM_ChangeActivity_EntitySchemaName);
        }
        /// <summary>
        /// Update ClientField in CRM
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateSyncClientField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            wi.SyncActivityOptionSetValueField(FieldNames.CRM_ChangeActivity_Client, FieldNames.Client, FieldNames.CRM_ChangeActivity_EntitySchemaName);
        }

        /// <summary>
        /// Update ChangeType in CRM
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateSyncChangeTypeField(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            wi.SyncActivityOptionSetValueField(FieldNames.CRM_ChangeActivity_ChangeType, FieldNames.ChangeType,
                FieldNames.CRM_ChangeActivity_EntitySchemaName);
        }
        /// <summary>
        ///  Update ChangeNumber in CRM
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateSyncChangeNumber(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            wi.SyncActivityTexualField(FieldNames.CRM_ChangeActivity_Changenumber, FieldNames.ChangeNumber, FieldNames.CRM_ChangeActivity_EntitySchemaName);

        }
        /// <summary>
        ///  Update NeginliteTrunkVersion in CRM for Team Negin
        /// </summary>
        /// <param name="eventArgs"></param>
        private void SyncNeginliteTrunkVersion(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi.Fields[FieldNames.NeginLiteBranchVersion].Value.ToString()))
                wi.SyncActivityEntityReferenceFieldEmpty(FieldNames.CRM_ChangeActivity_NeginliteTrunkVersion,
                FieldNames.NeginLiteTrunkVersion
                , FieldNames.CRM_ChangeActivity_EntitySchemaName);
            else
            wi.SyncActivityEntityReferenceField(FieldNames.CRM_ChangeActivity_NeginliteTrunkVersion,
                FieldNames.NeginLiteTrunkVersion
                , FieldNames.CRM_ChangeActivity_EntitySchemaName, FieldNames.CRM_NeginliteVersion_EntitySchemaName,
                FieldNames.CRM_NeginliteVersion_Name);
        }
        /// <summary>
        /// Update cNeginLiteBranchVersion in CRM for Team Negin
        /// </summary>
        /// <param name="eventArgs"></param>
        private void SyncNeginLiteBranchVersion(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi.Fields[FieldNames.NeginLiteBranchVersion].Value.ToString()))
                wi.SyncActivityEntityReferenceFieldEmpty(FieldNames.CRM_ChangeActivity_NeginLiteBranchVersion,
                FieldNames.NeginLiteBranchVersion
                , FieldNames.CRM_ChangeActivity_EntitySchemaName);
            else
            wi.SyncActivityEntityReferenceField(FieldNames.CRM_ChangeActivity_NeginLiteBranchVersion,
                FieldNames.NeginLiteBranchVersion
                , FieldNames.CRM_ChangeActivity_EntitySchemaName, FieldNames.CRM_NeginliteVersion_EntitySchemaName,
                FieldNames.CRM_NeginliteVersion_Name);
        }
        private void ScrumProcessTemplateSyncPRVinCA(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi.Fields[FieldNames.PlannedReleaseVersion].Value.ToString()))
                wi.SyncActivityEntityReferenceFieldEmpty(FieldNames.CRM_Activity_PlannedReleaseVersion,
                FieldNames.PlannedReleaseVersion
                , FieldNames.CRM_ChangeActivity_EntitySchemaName);
            else
            wi.SyncActivityEntityReferenceField(FieldNames.CRM_Activity_PlannedReleaseVersion,
                FieldNames.PlannedReleaseVersion
                , FieldNames.CRM_ChangeActivity_EntitySchemaName, FieldNames.CRM_PlannedReleaseVersion_EntitySchemaName,
                FieldNames.CRM_PlannedReleaseVersion_Name);
        }
    }
}
