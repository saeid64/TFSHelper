using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSHelper.Helper.Model;

namespace TFSHelper.Helper
{
    public class WorkItemService : TFSService
    {
        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public WorkItemService()
            : base()
        {
        }

        /// <summary>
        /// Initilizing TFS Service via 'Team Foundation Request Conxtext' whcich usually is passing through tfs plugins.
        /// </summary>
        /// <param name="requestContext">Team Foundation Request Conxtext</param>
        public WorkItemService(TeamFoundationRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// If you Initilized the service before and you have the projectcollection you can use this constructor.
        /// </summary>
        /// <param name="projectCollection">Team Foundation Project Collection</param>
        public WorkItemService(TfsTeamProjectCollection projectCollection)
            : base(projectCollection)
        {
        }

        /// <summary>
        /// Initilizing TFS Service.
        /// </summary>

        #endregion

        #region Public Service Methods
        public WorkItem GetWorkItem(int id)
        {
            return Store.GetWorkItem(id);
        }

        /// <summary>
        /// GetFieldValue workitem by id, normally or as bypassed Rule workitem
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemStoreFlag">You can use BypassRules flag to get workitems, just be careful if you using this feature be aware of their consequences</param>
        /// <returns></returns>
        public WorkItem GetWorkItem(int id, WorkItemStoreFlags itemStoreFlag)
        {
            return itemStoreFlag == WorkItemStoreFlags.BypassRules ? FreeStore.GetWorkItem(id) : Store.GetWorkItem(id);
        }

        /// <summary>
        /// Qury workitems with TFS WIQL query
        /// </summary>
        /// <param name="query">Query should be from WorkItems</param>
        /// <returns>Return workitems as a WorkItemCollection Type</returns>
        public IEnumerable<WorkItem> GetWorkItemsByQuery(string query)
        {
            return Store.Query(query).OfType<WorkItem>();
        }

        /// <summary>
        /// Qury workitem's links with TFS WIQL query
        /// </summary>
        /// <param name="query">Query should be from WorkItemLinks</param>
        /// <returns>Return workitems as a array of WorkItemsLink info</returns>
        public IEnumerable<WorkItemLinkInfo> GetWorkItemLinksByQuery(string query)
        {
            Query q = new Query(Store, query);
            return q.RunLinkQuery();
        }

        /// <summary>
        /// GetFieldValue related workitem by type of the TFS link topology
        /// </summary>
        /// <param name="id">workitem id</param>
        /// <param name="linkTopology">Type of TFS link's linkTopology</param>
        /// <returns></returns>
        public IEnumerable<int> GetLinkedWorkItemIds(int id, WorkItemLinkType.Topology linkTopology)
        {
            WorkItem workItem = GetWorkItem(id);
            return from l in workItem.WorkItemLinks.OfType<WorkItemLink>()
                   where l.LinkTypeEnd.LinkType.LinkTopology == linkTopology
                   select l.TargetId;
        }

        /// <summary>
        /// GetFieldValue related workitem by type of the TFS link topology and its workitem type
        /// </summary>
        /// <param name="id">workitem id</param>
        /// <param name="linkTopology">Type of TFS link's linkTopology</param>
        /// <param name="wiType">Type of the Linked workitem which should be returned</param>
        /// <returns></returns>
        public IEnumerable<WorkItem> GetLinkedWorkItems(int id, WorkItemLinkType.Topology linkTopology, string wiType)
        {
            WorkItem workItem = GetWorkItem(id);
            IEnumerable<int> targets = from l in workItem.WorkItemLinks.OfType<WorkItemLink>()
                          where l.LinkTypeEnd.LinkType.LinkTopology == linkTopology
                          select l.TargetId;

            int[] targetIds = targets as int[] ?? targets.ToArray();
            if (!targetIds.Any()) return new List<WorkItem>();

            IEnumerable<WorkItem> wiCollection = targetIds.Select(GetWorkItem);

            return wiCollection.Where(item => (string)item[TFFieldName.Type] == wiType);
        }

        /// <summary>
        /// GetFieldValue related workitem by type of the TFS link topology
        /// </summary>
        /// <param name="id">workitem id</param>
        /// <param name="linkTopology">Type of TFS link's linkTopology</param>
        /// <returns></returns>
        public IEnumerable<WorkItem> GetLinkedWorkItems(int id, WorkItemLinkType.Topology linkTopology)
        {
            WorkItem workItem = GetWorkItem(id);
            IEnumerable<int> targets = from l in workItem.WorkItemLinks.OfType<WorkItemLink>()
                          where l.LinkTypeEnd.LinkType.LinkTopology == linkTopology
                          select l.TargetId;

            int[] targetIds = targets as int[] ?? targets.ToArray();
            if (!targetIds.Any()) return new List<WorkItem>();

            IEnumerable<WorkItem> wiCollection = targetIds.Select(GetWorkItem);

            return wiCollection;
        }

        /// <summary>
        /// GetFieldValue parent workItem of specified workitem
        /// </summary>
        /// <param name="id">workitem id</param>
        /// <param name="topology">Type of TFS link's linkTopology; just pass 'Tree' linkTopology for parent/child relationship.</param>
        /// <returns>returns null if there isn't any parent workitem</returns>
        public WorkItem GetParentWorkItem(int id, WorkItemLinkType.Topology topology)
        {
            // GetFieldValue the work item with the specified id
            WorkItem workItem = GetWorkItem(id);

            // GetFieldValue the link to the parent work item through the work item links
            IEnumerable<int> q = from l in workItem.WorkItemLinks.OfType<WorkItemLink>()
                    where l.LinkTypeEnd.LinkType.LinkTopology == topology
                          && !l.LinkTypeEnd.IsForwardLink
                    select l.TargetId;

            // If there is a link with a parent work item
            return q.Any() ? GetWorkItem(q.ElementAt(0)) : null;

        }

        /// <summary>
        /// GetFieldValue global value list of specified list name.
        /// </summary>
        /// <param name="listName">Which global list should be retrieved</param>
        /// <returns></returns>
        public IEnumerable<string> GetGlobalListValues(string listName)
        {
            XmlDocument xmlGlobalList = Store.ExportGlobalLists();

            string xPath = string.Format("*/GLOBALLIST[@name='{0}']/LISTITEM/@value", listName);

            XmlNodeList xmlNodeList = xmlGlobalList.SelectNodes(xPath);

            return xmlNodeList != null ? xmlNodeList.OfType<XmlNode>().Select(node => node.Value).ToList() : null;
        }

        /// <summary>
        /// get List of Collection Names
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCollectionsName()
        {
            ITeamProjectCollectionService tpcService = ConfigurationServer.GetService<ITeamProjectCollectionService>();
            return tpcService.GetCollections().Select(clollection => clollection.Name).ToList();
        }

        /// <summary>
        /// Set a workitem's state
        /// </summary>
        /// <param name="workItem">The workitem which its state should be changed</param>
        /// <param name="state">the new state of the workitem</param>
        public void SetWorkItemState(WorkItem workItem, string state)
        {
            try
            {
                string oldState = (string)workItem[TFFieldName.State];
                if (string.Equals(oldState, state)) return;
                workItem.Open();
                workItem[TFFieldName.State] = state;
                ArrayList notValid = workItem.Validate();
                workItem.Save();
                workItem.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Set a workitem's state with a reason
        /// </summary>
        /// <param name="workItem">The workitem which its state should be changed</param>
        /// <param name="state">the new state of the workitem</param>
        /// <param name="reason">the new reason of the workitem</param>
        public void SetWorkItemState(WorkItem workItem, string state, string reason)
        {
            try
            {
                string oldState = (string)workItem[TFFieldName.State];
                if (string.Equals(oldState, state)) return;

                workItem.Open();
                workItem[TFFieldName.State] = state;
                workItem[TFFieldName.Reason] = reason;
                ArrayList notValid = workItem.Validate();
                workItem.Save();
                workItem.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// GetFieldValue TFS Item from workItem
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns>Returns TFSItem as Consistent Custom Type which is a equivalent to workitem</returns>
        public TFSItem GetTFSItem(WorkItem workItem)
        {
            TFSItem tfsItem = new TFSItem { ProjectName = workItem.Project.Name };
            foreach (Field field in workItem.Fields)
            {
                tfsItem.Fields.Add(new TFSField { Id = field.ReferenceName, NewValue = field.Value });
            }
            if (workItem.Links == null || workItem.Links.Count == 0) return tfsItem;
            foreach (RelatedLink lnk in from Link link in workItem.Links select link as RelatedLink)
            {
                tfsItem.TFSLinks.Add(new TFSLinkField() { Id = lnk.RelatedWorkItemId, LinkName = lnk.LinkTypeEnd.Name, LinkImmutableName = lnk.LinkTypeEnd.ImmutableName, LinkType = TFSLinkField.Type.RelatedLink });
            }
            foreach (ExternalLink lnk in from Link link in workItem.Links select link as ExternalLink)
            {
                //Todo: Implement 
            }
            return tfsItem;
        }

        /// <summary>
        /// RollBacks the workitem to the specified Revision
        /// </summary>
        /// <param name="wi">The workitem wi which should be reollback</param>
        /// <param name="revision">The Revision number should workitem return to that state</param>
        public void RollbackWorkItem(WorkItem wi, int revision)
        {
           // var wi = Store.GetWorkItem(id);
                          
            var revFields =
                wi.Revisions[revision - 1].Fields.OfType<Field>().Where(field => field.IsChangedInRevision && !field.IsComputed && field.FieldDefinition.IsIndexed).ToList();

            if (!revFields.Any()) return ;

            wi.Open();
            foreach (var field in revFields)
            {
                wi[field.ReferenceName] = field.Value;
            }
            wi.Save();
        }
        public List<Field> GetChangedFieldsList(WorkItem wi, int revision)
        {
            List<Field> revFields =
                wi.Revisions[revision - 1].Fields.OfType<Field>().Where(field => field.IsChangedInRevision && !field.IsComputed && field.FieldDefinition.IsIndexed).ToList();

            return revFields;

        }
        //public string GetChangedField(WorkItem wi ,int revision)
        //{
       
        //    var identityService = new IdentityService(TeamProjectCollectionInstance);
        //    string msg = "WorkItem Id= " + wi.Id.ToString() + "\n ";
        //    var revFields =
        //        wi.Revisions[revision - 1].Fields.OfType<Field>().Where(field => field.IsChangedInRevision && !field.IsComputed && field.FieldDefinition.IsIndexed).ToList();

        //    if (!revFields.Any()) return msg;
            
        //    foreach (var field in revFields)
        //    {
        //        wi[field.ReferenceName] = field.Value;
        //        msg += field.ReferenceName.ToString() + " Changed To " + field.Value.ToString() + "\n ";
        //    }
          
        //    return msg;

        //}
        #endregion
    }
}
