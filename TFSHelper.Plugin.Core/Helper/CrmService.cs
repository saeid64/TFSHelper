using System;
using System.Collections.Generic;
using TFSHelper.Helper.Model;
using TFSHelper.Plugin.Core.Utility;

namespace TFSHelper.Plugin.Core.Helper
{
    public class CrmService
    {
        private readonly IOrganizationService _orgService;
        private readonly CrmOrganizationServiceContext _serviceContext;

        public static IDictionary<string, string> TasksActivityMapping = new Dictionary<string, string>()
        {
            {"Dev Task", "cmdb_changeactivity"},
            {"Test Task", "cmdb_testactivity"},
            {"Analysis Task", "cmdb_analysisactivity"}
        };

        public CrmService()
        {
            _orgService = OrganizationXrmUtility.GetOrganizationService();
            _serviceContext = new CrmOrganizationServiceContext(_orgService);
        }

        public IEnumerable<Entity> GetEntityByField(string entityLogicalName, string fieldName, string fieldContent)
        {
            return _serviceContext.CreateQuery(entityLogicalName).Where(attr => (string)attr[fieldName] == fieldContent);
        }

        public void ChangeEntityState(Guid id, string entityName, int status, int state)
        {
            var closeRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference(entityName, id),
                    State = new OptionSetValue(state),
                    Status = new OptionSetValue(status),

                };
            _orgService.Execute(closeRequest);
        }
        public void ChangeEntityAssignee(Guid assigneeId, string entityName, Guid entityId, string crmAssigneeType)
        {
            var assignRequest = new AssignRequest()
            {
                Assignee = new EntityReference(crmAssigneeType, assigneeId),
                Target = new EntityReference(entityName, entityId)
            };
            _orgService.Execute(assignRequest);
        }
        public Entity RetrieveEntity(Guid id, string entityName)
        {
            return _orgService.Retrieve(entityName, id, new ColumnSet(true));
        }

        public void UpdateField(Guid id, string entityName, string fieldName, object fieldValue)
        {
            try
            {
                var entity = _orgService.Retrieve(entityName, id, new ColumnSet(fieldName));
                entity[fieldName] = fieldValue;
                _orgService.Update(entity);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Guid CreateEntity(Entity entity)
        {
            try
            {
                return _serviceContext.Create(entity);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public EntityReference GetCRMUserByDomainName(string domainName)
        {
            var systemUser = _serviceContext.CreateQuery("systemuser")
                .SingleOrDefault(u => (string)u["domainname"] == domainName
                    && ((bool)u["isdisabled"]) == false);
            return systemUser != null ? new EntityReference("systemuser", systemUser.Id) : null;
        }

        public int GetAttributeId(string entityName, string attributeName, string optionSetLabel)
        {
            var optionSetValue = 0;
            var retrieveAttributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityName,
                    LogicalName = attributeName,
                    RetrieveAsIfPublished = true
                };

            var retrieveAttributeResponse =
              (RetrieveAttributeResponse)_orgService.Execute(retrieveAttributeRequest);
            var picklistAttributeMetadata =
              (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            var optionsetMetadata = picklistAttributeMetadata.OptionSet;

            foreach (var optionMetadata in optionsetMetadata.Options
                .Where(optionMetadata => optionMetadata.Label.UserLocalizedLabel.Label.ToLower() == optionSetLabel.ToLower()))
            {
                optionSetValue = optionMetadata.Value.Value;
                return optionSetValue;
            }
            return optionSetValue;
        }

        public void AddCRMNote(TFSFile tfsFile, EntityReference regardingEntityReference)
        {
            var crmAnnotation = new Entity("annotation");
            var encodedData = Convert.ToBase64String(tfsFile.Body);
            crmAnnotation.Attributes["objectid"] = regardingEntityReference;
            crmAnnotation.Attributes["subject"] = "From TFS By " + tfsFile.Creator + " @" + tfsFile.CreationTime;
            crmAnnotation.Attributes["documentbody"] = encodedData;
            crmAnnotation.Attributes["mimetype"] = @"text/plain";
            crmAnnotation.Attributes["notetext"] = tfsFile.Comment;
            crmAnnotation.Attributes["filename"] = tfsFile.Name;
            _orgService.Create(crmAnnotation);
        }
     
        public void AddCRMNote(string text, EntityReference regardingEntityReference, string user)
        {
            var crmAnnotation = new Entity("annotation");
            crmAnnotation.Attributes["objectid"] = regardingEntityReference;
            crmAnnotation.Attributes["subject"] = " From TFS By  " + user + "  @" + DateTime.Now;
            crmAnnotation.Attributes["notetext"] = text;
            _orgService.Create(crmAnnotation);
        }
    }

    public enum CRM_RFCState
    {
        Planned_Tested = 281240008,
        Planned_Maintanance = 281240006
    }
    public enum CRM_ChangeactivityChangeType
    {
        Code_Review = 281240004,
        Applications = 281240000,
        Hardware = 281240001,
        Configuration = 281240002,
        Change_Model = 281240003,
        Infrastructure = 281240005
    }
    public enum CRM_ChangeactivityStateCode
    {
        Canceled = 3,
        Completed = 2,
        Open_Planned = 100000004,
        Open_PendingImplementation = 100000001,
        Open_PendingPlanning = 1

    }
    public enum CRM_TestactivityStateCode
    {
        Canceled = 3,
        Completed_Confirmed = 2,
        Completed_Rejected = 281240001,
        Open_Planned = 281240002,
        Open_PendingPlanning = 281240000,
        Open_PendingTest = 281240004

    }
    public enum CRM_TestactivityTestType
    {
        Functional_Test = 100000000,
        Acceptance_Test = 100000001,
        Integration_Test = 100000002,
        Performance_Test = 100000003,
        Security_Test = 100000004,
        Usabilty_Test = 100000005,
        Unit_Test = 100000006,
        Suit_Test = 100000007,
        Test_Case = 100000008,
        Static_Test = 100000009
    }
    public enum CRM_AnalysisactivityStateCode
    {
        Canceled = 3,
        Completed_Confirmed = 2,
        Open_PendingImplementation = 281240001,
        Open_PendingPlanning = 1

    }

}
