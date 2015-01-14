using System;
using log4net.Core;
using Tosan.TeamFoundation.Plugin.Core;
using Tosan.TeamFoundation.Plugin.Core.Helper;
using Tosan.TeamFoundation.Plugin.Core.Utility;
using Tosan.TeamFoundation.Plugins.Resources;
using Tosan.TFSHelper;


namespace Tosan.TeamFoundation.Plugins.Scrum
{
    class AnalysisTaskHandler : IEventHandler
    {
        private WorkItemService _helper;

        public void Register(TFSEventAggregator aggregator)
        {
            var analysisUpdateFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
                && item.Fields[FieldNames.State] != null
                && item.Fields[FieldNames.State].IsDirty
                && !(item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_InProgress && item.Fields[FieldNames.State].OldValue.ToString() == FieldValues.State_Blocked)
                && !(item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Blocked && item.Fields[FieldNames.State].OldValue.ToString() == FieldValues.State_InProgress)
                && item.Fields[FieldNames.State].NewValue.ToString() != FieldValues.State_Done);


            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateAnalysisTaskStateChanged, analysisUpdateFilter);

            var analysisChangeStateToDoneFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_AnalysisTask
               && item.Fields[FieldNames.State] != null
               && item.Fields[FieldNames.State].IsDirty
               && item.Fields[FieldNames.State].NewValue.ToString() == FieldValues.State_Done);


            aggregator.Subscribe(RequestTypeEnum.PreUpdate, ScrumProcessTemplateAnalysisTaskStateChangedToDone, analysisChangeStateToDoneFilter);
        }

        /// <summary>
        /// for  Scrum Process Template Map State When Activity ID Exist
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateAnalysisTaskStateChanged(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
            var crmService = new CrmService();
            if (wi.State == FieldValues.State_Removed)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_AnalysisActivity_EntitySchemaName, (int)CRM_AnalysisactivityStateCode.Canceled, 2);

            else if (wi.State == FieldValues.State_Done && wi.Reason == FieldValues.Reason_WorkFinished)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_AnalysisActivity_EntitySchemaName, (int)CRM_AnalysisactivityStateCode.Completed_Confirmed, 1);
            else if (wi.State == FieldValues.State_Done && wi.Reason == FieldValues.Reason_WorkisCanceled)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_AnalysisActivity_EntitySchemaName, (int)CRM_AnalysisactivityStateCode.Canceled, 2);
            else if (wi.State == FieldValues.State_ToDo)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_AnalysisActivity_EntitySchemaName, (int)CRM_AnalysisactivityStateCode.Open_PendingPlanning, 0);

            else if (wi.State == FieldValues.State_InProgress)
                crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_AnalysisActivity_EntitySchemaName, (int)CRM_AnalysisactivityStateCode.Open_PendingImplementation, 0);

        }
        private void ScrumProcessTemplateAnalysisTaskStateChangedToDone(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;
            var state = eventArgs.TFSEventItem.Fields[FieldNames.State];
            var reason = eventArgs.TFSEventItem.Fields[FieldNames.Reason];
            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            try
            {
                if (string.IsNullOrEmpty(wi[FieldNames.ActivityId].ToString())) return;
                var crmService = new CrmService();

                if (state.NewValue.ToString() == FieldValues.State_Done && reason.NewValue.ToString() == FieldValues.Reason_WorkFinished)
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_AnalysisActivity_EntitySchemaName, (int)CRM_AnalysisactivityStateCode.Completed_Confirmed, 1);
                else if (state.NewValue.ToString() == FieldValues.State_Done && reason.NewValue.ToString() == FieldValues.Reason_WorkisCanceled)
                    crmService.ChangeEntityState(new Guid(wi[FieldNames.ActivityId].ToString()), FieldNames.CRM_AnalysisActivity_EntitySchemaName, (int)CRM_AnalysisactivityStateCode.Canceled, 2);
            }
            catch (Exception ex)
            {
                var message = string.Format(ErrorMessages.CRMError, ex.Message);
                eventArgs.ResponseAction(false, message);
                throw new TosanTeamFoundationException(message, Level.Warn);
            }

        }
    }
}
