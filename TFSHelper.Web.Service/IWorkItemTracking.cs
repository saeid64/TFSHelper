using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace TFSHelper.Web.Service
{
    [ServiceContract]
    public interface IWorkItemTracking
    {
        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int CreateWorkItem(TFSItem item, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int CreateImpersonateWorkItem(TFSItem item, string collectionName, string impersonateUsername);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ChangeWorkItemState(int workItemId, string state, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void LinkWorkItems(int sourceId, int targetId, string linkType, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Init(string uri);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<string> GetIterations(string collectionName, string projectName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateFields(int workItemId, List<TFSField> fields, string impersonateUsername, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateFieldsBypassRules(int workItemId, List<TFSField> fields, string impersonateUsername, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<int> RunWIQL(string query, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        TFSItem GetWorkItem(int wiId, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<int> GetLinkedWorkItem(int wiId, WorkItemLinkTypeTopology topology, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddImpersonateHistoryComment(int wiId, string comment, string createdBy, string createdOn, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddImpersonateAttachment(int wiId, string filecomment, string fileName, byte[] file, string impersonateUsername, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddHistoryComment(int wiId, string comment, string createdBy, string createdOn, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddAttachment(int wiId, string filecomment, string fileName, byte[] file, string createdBy, string createdOn, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DestoryWi(int wiId, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetUserDisplayName(string userDomainName, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<int> GetWorkItemsByFieldMatching(List<TFSField> tfsFields, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<string> GetGlobalListsValues(string listName, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<string> GetBranchLabels(string dirName, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<int> GetBranchChangSets(string dirName, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<TFSItem> GetChangeSetsWorkItems(int changeSetId, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<TFSItem> GetLabelScopedChangeSets(string dirName, string baseLabel, string targetLabel, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        bool BuildNotify(string label, string dir, string reportDir, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ImportGlobalList(string globalListName, string value, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        bool IsUserInProject(string userDomainName, string projectName, string collectionName);

        [OperationContract]
        [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int[] GetWorkItemByActivityId(string activityId, string collectionName);
    }


    //[DataContract]
    //public class TFSFieldList : List<TFSField>
    //{
    //    [DataMember]
    //    public TFSField this[string id]
    //    {
    //        get
    //        {
    //            return this.SingleOrDefault(field => field.Id == id);
    //        }
    //        set
    //        {
    //            if (value == null) throw new ArgumentNullException("value");
    //            this[id] = value;
    //        }
    //    }
    //}
    [DataContract]
    public class TFSField
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public object Value { get; set; }
        [DataMember]
        public string Type { get; set; }
    }
    [DataContract]
    public class TFSItem
    {
        [DataMember]
        public List<TFSField> Fields { get; set; }

        [DataMember]
        public List<TFSLinkField> LinkItems { get; set; }

        [DataMember]
        public string Title
        {
            get
            {
                var tfsField = Fields.SingleOrDefault(field => field.Id == "System.Title");
                return tfsField != null ? tfsField.Value.ToString() : null;
            }
            set { Fields.Add(new TFSField { Id = "System.Title", Value = value }); }
        }

        [DataMember]
        public string WorkItemType
        {
            get
            {
                var tfsField = Fields.SingleOrDefault(field => field.Id == "System.WorkItemType");
                return tfsField != null ? tfsField.Value.ToString() : null;
            }
            set { Fields.Add(new TFSField { Id = "System.WorkItemType", Value = value }); }
        }
        [DataMember]
        public int Id
        {
            get
            {
                var tfsField = Fields.SingleOrDefault(field => field.Id == "System.Id");
                return tfsField != null ? int.Parse(tfsField.Value.ToString()) : 0;
            }
            set { Fields.Add(new TFSField { Id = "System.Id", Value = value }); }
        }
        [DataMember]
        public string Area
        {
            get
            {
                var tfsField = Fields.SingleOrDefault(field => field.Id == "System.AreaPath");
                return tfsField != null ? tfsField.Value.ToString() : null;
            }
            set { Fields.Add(new TFSField { Id = "System.AreaPath", Value = value }); }
        }

        [DataMember]
        public string CRMId
        {
            get
            {
                TFSField tfsField = null;
                switch (WorkItemTypeCode)
                {
                    case 1:
                        tfsField = Fields.SingleOrDefault(field => field.Id == "RFCId");
                        break;
                    case 2:
                        tfsField = Fields.SingleOrDefault(field => field.Id == "ActivityId");
                        break;
                }
                return tfsField != null ? tfsField.ToString() : null;
            }
            set { } ////Just Some Hacking for WCF Recognition
        }

        /// <summary>
        /// 1 is Requesrement, 2 is Task and 0 is Unknown Type.
        /// </summary>
        [DataMember]
        public int WorkItemTypeCode
        {
            get
            {
                if (RequirementTypes.Contains(WorkItemType))
                    return 1;
                return TaskTypes.Contains(WorkItemType) ? 2 : 0;
            }
            set { } //Just Some Hacking for WCF Recognition
        }
        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string[] RequirementTypes = new[] { "Bug", "Product Backlog Item" };

        [DataMember]
        public string[] TaskTypes = new[] { "Dev Task", "Test Task", "Analysis Task", "Task" };
    }

    [DataContract]
    public class TFSLinkField
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string LinkName { get; set; }
        [DataMember]
        public string LinkType { get; set; }
        [DataMember]
        public TFSItem Item { get; set; }

    }

    [DataContract]
    public enum WorkItemLinkTypeTopology
    {
        [EnumMember]
        Tree = 0,
        [EnumMember]
        Network = 1
    }

    //[DataContract]
    //public class TFSTypes
    //{

    //}
}
