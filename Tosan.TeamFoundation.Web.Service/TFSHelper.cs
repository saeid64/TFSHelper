using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TeamFoundationIdentity = Microsoft.TeamFoundation.Framework.Client.TeamFoundationIdentity;

namespace TosanTFS.Web.Service
{
    public class TFSHelper
    {
        public string TfsUri { get { return new System.Configuration.AppSettingsReader().GetValue("TFSUri", typeof(string)).ToString(); } }
        public TfsTeamProjectCollection TfsInstance { get; set; }
        private WorkItemStore _store;
        private WorkItemStore _freeStore;
        public WorkItemStore Store
        {
            get
            {
                if (_store != null)
                    return _store;
                _store = TfsInstance.GetService<WorkItemStore>();

                return _store;
            }
            set { _store = value; }
        }
        public WorkItemStore FreeStore
        {
            get
            {
                if (_freeStore != null)
                    return _freeStore;
                _freeStore = new WorkItemStore(TfsInstance, WorkItemStoreFlags.BypassRules);

                return _freeStore;
            }
            set { _freeStore = value; }
        }
        public TFSHelper()
        {
            // TODO: Complete member initialization
            CreateTFSInstance("Tosan", string.Empty);
        }
        public TFSHelper(string collectionName)
        {
            CreateTFSInstance(collectionName, string.Empty);
        }
        public TFSHelper(string collectionName, string impersonateUser)
        {
            CreateTFSInstance(collectionName, impersonateUser);
        }
        private void CreateTFSInstance(string collectionName, string impersonateUser)
        {
            ////TfsInstance.EnsureAuthenticated();
            var cred = new NetworkCredential("AccountUserName", "AccountPassword", "CompanyDomainName");
            var address = TfsUri + collectionName;
            TfsInstance = string.IsNullOrEmpty(impersonateUser)
                ? new TfsTeamProjectCollection(new Uri(address), cred)
                : GetImpersonatedCollection(new Uri(address), impersonateUser, cred);

        }
        public TeamFoundationIdentity GetTFSIdentity(string searchValue, IdentitySearchFactor identitySearchFactor)
        {

            // Get the TFS Identity Management Service
            var identityManagementService =
                TfsInstance.GetService<IIdentityManagementService>();

            // Look up the user that we want to impersonate
            return identityManagementService.ReadIdentity(identitySearchFactor,
                                                       searchValue, MembershipQuery.None, ReadIdentityOptions.None);
        }
        public static TfsTeamProjectCollection GetImpersonatedCollection(Uri collectionToUse, string userToImpersonate, NetworkCredential cred)
        {
            var currentCollection =
               new TfsTeamProjectCollection(collectionToUse, cred);

            // Get the TFS Identity Management Service
            var identityManagementService =
               currentCollection.GetService<IIdentityManagementService>();

            // Look up the user that we want to impersonate
            var identity =
               identityManagementService.ReadIdentity(IdentitySearchFactor.AccountName,
                  userToImpersonate, MembershipQuery.None, ReadIdentityOptions.None);

            var impersonatedCollection =
               new TfsTeamProjectCollection(collectionToUse, new TfsClientCredentials(new WindowsCredential(cred)),
               identity.Descriptor);

            return impersonatedCollection;
        }
        public List<string> GetProjectValidUsers(string projectName)
        {
            var groupSecurityService = TfsInstance.GetService<IIdentityManagementService2>();

            var contributors = groupSecurityService.ReadIdentity(IdentitySearchFactor.AccountName, string.Format("[{0}]\\Contributors", projectName), MembershipQuery.Expanded, ReadIdentityOptions.None);

            var identites = groupSecurityService.ReadIdentities(IdentitySearchFactor.Identifier, contributors.Members.Select(descriptor => descriptor.Identifier).ToArray(), MembershipQuery.Direct, ReadIdentityOptions.ExtendedProperties);
            return identites.Select(identities => identites)
                    .ElementAt(0)
                    .Select(teamFoundationIdentity => teamFoundationIdentity[0].UniqueName.ToLower()).ToList();
        }
        public WorkItemStore GetService()
        {
            if (TfsInstance == null)
                throw new TeamFoundationServerException("The Service Instance in Null.");
            return TfsInstance.GetService<WorkItemStore>();
        }
        public VersionControlServer GetCheckInService()
        {
            if (TfsInstance == null)
                throw new TeamFoundationServerException("The Service Instance in Null.");
            return TfsInstance.GetService<VersionControlServer>();
        }
        public ILinking GetLinkingService()
        {
            if (TfsInstance == null)
                throw new TeamFoundationServerException("The Service Instance in Null.");
            return TfsInstance.GetService<ILinking>();
        }
        public WorkItem GetWorkItem(int id)
        {
            return Store.GetWorkItem(id);
        }
        public WorkItem GetWorkItem(int id, WorkItemStoreFlags itemStoreFlag)
        {
            return itemStoreFlag == WorkItemStoreFlags.BypassRules ? FreeStore.GetWorkItem(id) : Store.GetWorkItem(id);
        }
        public WorkItemCollection GetWorkItemsByQuery(string query)
        {
            return Store.Query(query);
        }
        public WorkItemLinkInfo[] GetWorkItemLinkQuery(string query)
        {
            var q = new Query(Store, query);
            return q.RunLinkQuery();
        }
        public WorkItem GetParentWorkItem(int id, WorkItemLinkType.Topology topology)
        {
            // Get the work item with the specified id
            var workItem = GetWorkItem(id);

            // Get the link to the parent work item through the work item links
            var q = from l in workItem.WorkItemLinks.OfType<WorkItemLink>()
                    where l.LinkTypeEnd.LinkType.LinkTopology == topology
                          && !l.LinkTypeEnd.IsForwardLink
                    select l.TargetId;

            // If there is a link with a parent work item
            if (q.Any())
            {
                // Return that one
                return GetWorkItem(q.ElementAt(0));
            }
            else
            {
                return null;
            }

        }
        public IEnumerable<string> GetGlobalListValues(string listName)
        {
            var xmlGlobalList = Store.ExportGlobalLists();

            var xPath = string.Format("*/GLOBALLIST[@name='{0}']/LISTITEM/@value", listName);

            var xmlNodeList = xmlGlobalList.SelectNodes(xPath);

            return xmlNodeList != null ? xmlNodeList.OfType<XmlNode>().Select(node => node.Value).ToList() : null;
        }
        /// <summary>
        /// get List of Collection Name 
        /// </summary>
        /// <returns></returns>
        public List<string> GetCollectionsName()
        {
            Uri configurationServerUri = new Uri(TfsUri);
            TfsConfigurationServer configurationServer =
                    TfsConfigurationServerFactory.GetConfigurationServer(configurationServerUri);

            ITeamProjectCollectionService tpcService = configurationServer.GetService<ITeamProjectCollectionService>();
            return tpcService.GetCollections().Select(clollection => clollection.Name).ToList();

        }


    }
}
