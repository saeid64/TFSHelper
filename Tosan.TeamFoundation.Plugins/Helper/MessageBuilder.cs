using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Tosan.TeamFoundation.Plugin.Core.Utility;

namespace Tosan.TeamFoundation.Plugins.Helper
{
    class MessageBuilder
    {
        public static string BuildFieldNamesMessage(WorkItem workItem, string[] fieldNames)
        {
            var sBuilder = new StringBuilder();
            foreach (var fieldName in fieldNames.Select(workItem.GetWiFilledFieldName).Where(s => !string.IsNullOrEmpty(s)))
                sBuilder.Append(string.Format("'{0}', ", fieldName));

            return sBuilder.ToString();
        }
    }
}
