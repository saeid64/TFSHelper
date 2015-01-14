using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Xrm.Sdk;
using Tosan.TeamFoundation.Plugin.Core.Helper;
using Tosan.TeamFoundation.Plugins.Resources;

namespace Tosan.TeamFoundation.Plugins.Helper
{
    public static class WorkItemExtension
    {
        public static bool Contains(this WorkItem workItem, string fieldName)
        {
            return workItem.Fields.Contains(fieldName) && workItem[fieldName] != null && !string.IsNullOrEmpty(workItem[fieldName].ToString());
        }

        public static string GetWiFilledFieldName(this WorkItem workItem, string fieldName)
        {
            if (!workItem.Fields.Contains(fieldName) || workItem.Fields[fieldName].Value == null ||
                string.IsNullOrEmpty(workItem.Fields[fieldName].Value.ToString()))
                return workItem.Fields[fieldName].Name;
            return string.Empty;
        }

        public static void SyncActivityOptionSetValueField(this WorkItem workItem, string crmFieldName, string tfsFieldName, string entitySchemaName)
        {
            var crmService = new CrmService();
            if (workItem.Contains(FieldNames.ActivityId))
                crmService.UpdateField(new Guid(workItem[FieldNames.ActivityId].ToString()),
                    entitySchemaName, crmFieldName,
                    workItem.Contains(tfsFieldName)
                        ? new OptionSetValue(crmService.GetAttributeId(entitySchemaName, crmFieldName, workItem[tfsFieldName].ToString())) : null);
        }

        public static void SyncActivityTexualField(this WorkItem workItem, string crmFieldName, string tfsFieldName, string entitySchemaName)
        {
            var crmService = new CrmService();
            if (workItem.Contains(FieldNames.ActivityId))
                crmService.UpdateField(new Guid(workItem[FieldNames.ActivityId].ToString()),
                    entitySchemaName,
                    crmFieldName, workItem.Fields[tfsFieldName].Value);
        }

        public static void SyncActivityEntityReferenceField(this WorkItem workItem, string crmFieldName, string tfsFieldName, string entitySchemaName, string refernceEntitySchemaName, string referenceFieldName)
        {
            var crmService = new CrmService();
            var entity = crmService.GetEntityByField(refernceEntitySchemaName, referenceFieldName, workItem.Fields[tfsFieldName].Value.ToString()).FirstOrDefault();
            if (entity == null) throw new Exception("Couldn't find any particular entity in CRM with specified id.");
            if (!workItem.Contains(FieldNames.ActivityId)) return;
            var activityId = workItem.Fields[FieldNames.ActivityId].Value.ToString();
            crmService.UpdateField(new Guid(activityId), entitySchemaName, crmFieldName, new EntityReference(refernceEntitySchemaName, entity.Id));

        }
        public static void SyncActivityEntityReferenceFieldEmpty(this WorkItem workItem, string crmFieldName, string tfsFieldName, string entitySchemaName)
        {
            var crmService = new CrmService();
            if (!workItem.Contains(FieldNames.ActivityId)) return;
            var activityId = workItem.Fields[FieldNames.ActivityId].Value.ToString();
            crmService.UpdateField(new Guid(activityId), entitySchemaName, crmFieldName, null);

        }
    }
}
