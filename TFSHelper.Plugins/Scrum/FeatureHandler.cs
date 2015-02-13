using System;
using TFSHelper.Helper;
using TFSHelper.Plugin.Core;
using TFSHelper.Plugin.Core.Helper;
using TFSHelper.Plugin.Core.Utility;

namespace TFSHelper.Plugins.Scrum
{
    class FeatureHandler : IEventHandler
    {
        private WorkItemService _helper;

        public void Register(TFSEventAggregator aggregator)
        {
            //Filter  Effort change
            var updateEffortCangeFilter = new AttributeRequestFilter(item => item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Feature
             && item.Fields[FieldNames.Effort] != null
             && item.Fields[FieldNames.Effort].IsDirty);
            aggregator.Subscribe(RequestTypeEnum.Updated, ScrumProcessTemplateUpdateEffort, updateEffortCangeFilter);

        }


        /// <summary>
        /// Update TotalScheduledduration in RFC by Effort in Feature  
        /// </summary>
        /// <param name="eventArgs"></param>
        private void ScrumProcessTemplateUpdateEffort(TFSEventArgs eventArgs)
        {
            _helper = eventArgs.ContextTFSHelper as WorkItemService;

            var wi = _helper.GetWorkItem(eventArgs.TFSEventItem.Id);
            if (string.IsNullOrEmpty(wi[FieldNames.RFCId].ToString())) return;
            var crmService = new CrmService();
            crmService.UpdateField(new Guid(wi[FieldNames.RFCId].ToString()), FieldNames.CRM_RFC_EntitySchemaName, FieldNames.CRM_RFC_TotalScheduledduration, wi.Fields[FieldNames.Effort].Value);

        }
    }
}
