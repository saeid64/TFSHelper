using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSHelper.Helper;
using TFSHelper.Helper.Model;
using TFSHelper.Helper.Utility;
using TFSHelper.Plugin.Core;
using TFSHelper.Plugin.Core.Helper;
using TFSHelper.Plugin.Core.Utility;
using TFSHelper.Plugins.Helper;
using TFSHelper.Plugins.Resources;

namespace TFSHelper.Plugins.Scrum
{
    public class GlobalHandler : IEventHandler
    {
        private WorkItemService _helper;

        public void Register(TFSEventAggregator aggregator)
        {

            //It applys as a AttributeRequestFilter to the Remainning Work's value to the Score value transformation
            var remainingToScore = new AttributeRequestFilter(item => 
                item.Fields[FieldNames.Type].NewValue.ToString() == FieldValues.WI_Task);

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

    }


}

