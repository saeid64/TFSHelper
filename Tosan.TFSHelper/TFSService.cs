using System;
using System.Configuration;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Tosan.TFSHelper
{
    public class TFSService
    {
        #region Private Fields

        private static string TFSConfiguredUri
        {
            get
            {
                return new AppSettingsReader().GetValue("TFSServerUri", typeof(string)).ToString();
            }
        }

        internal string TfsConfiguredCollectionName
        {
            get { return new AppSettingsReader().GetValue("TFSCollectionName", typeof(string)).ToString(); }
        }

        private WorkItemStore _store;
        private WorkItemStore _freeStore;
        private string _uri;
        private string _collectionName;
        private TfsConfigurationServer _tfsConfigurationServer;
        private TeamFoundationJobService _teamFoundationJobService;

        #endregion

        #region Public Property

        public bool IsInitiated = false;

        public TfsTeamProjectCollection TeamProjectCollectionInstance { get; private set; }
        public WorkItemStore Store
        {
            get
            {
                if (_store != null)
                    return _store;
                _store = TeamProjectCollectionInstance.GetService<WorkItemStore>();
                if (_store == null) throw new Exception("The workitem store is null!");
                return _store;
            }
        }
        /// <summary>
        /// for bypassing all the tfs rules, be careful when using this kind of WorkItemStore!
        /// </summary>
        public WorkItemStore FreeStore
        {
            get
            {
                if (_freeStore != null)
                    return _freeStore;
                _freeStore = new WorkItemStore(TeamProjectCollectionInstance, WorkItemStoreFlags.BypassRules);

                return _freeStore;
            }
        }

        public TfsConfigurationServer ConfigurationServer
        {
            get
            {
                if (_tfsConfigurationServer == null)
                    GetTFSConfigurationServer();
                return _tfsConfigurationServer;

            }
        }
        public TeamFoundationJobService TeamFoundationJobService
        {
            get
            {
                if (_teamFoundationJobService == null)
                    GetTFSJobService();
                return _teamFoundationJobService;

            }
        }

        /// <summary>
        /// The default name for the TFS Team Project Collection, if you haven't set this whithin your app config (as a key value pair, 'TFSCollectionName' in appsettings) you should set it implicitly.
        /// </summary>
        ///
        public string DefaultCollectionName
        {
            get { return string.IsNullOrEmpty(_collectionName) ? TfsConfiguredCollectionName : _collectionName; }
            set { _collectionName = value; }
        }
        /// <summary>
        /// The default Uri for TFS Server, if you haven't set this whithin your app config (as a key value pair, 'TFSServerUri' in appsettings) you should set it implicitly.
        /// </summary>
        public string TFSServerUri
        {
            get { return string.IsNullOrEmpty(_uri) ? TFSConfiguredUri : _uri; }
            set { _uri = value; }
        }
        public Uri TFSCollectionUri
        {
            get
            {
                var serverUri = TFSServerUri.EndsWith("/") ? TFSServerUri : TFSServerUri + "/";
                return new Uri(serverUri + DefaultCollectionName);
            }
        }
        /// <summary>
        /// if been set, credential will use to initiating tfs service with if not the service gonna initiate with default user.
        /// </summary>
        public ICredentials Credential { get; set; }
        /// <summary>
        /// if been set, TFS Service is impersonated using this User Domain Name thus every action through this service gonna happen by the of this user.
        /// </summary>
        public string ImpersonateeDomainName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TFSService()
        {

        }

        /// <summary>
        /// Initilizing TFS Service via 'Team Foundation Request Conxtext' whcich usually is passing through tfs plugins.
        /// </summary>
        /// <param name="requestContext">Team Foundation Request Conxtext</param>
        public TFSService(TeamFoundationRequestContext requestContext)
        {
            //Extracting TFS Uri from it's request context
            var locationService = requestContext.GetService<TeamFoundationLocationService>();
            _uri = locationService.GetServerAccessMapping(requestContext).AccessPoint;
            _collectionName = requestContext.ServiceHost.Name;
        }

        /// <summary>
        /// If you Initilized the service before and you have the projectcollection you can use this constructor.
        /// </summary>
        /// <param name="projectCollection">Team Foundation Project Collection</param>
        public TFSService(TfsTeamProjectCollection projectCollection)
        {
            TeamProjectCollectionInstance = projectCollection;
        }

        /// <summary>
        /// Initilizing TFS Service.
        /// </summary>
        public void Init()
        {
            EmployTFSServices(ImpersonateeDomainName, Credential);
            IsInitiated = true;
        }

        #endregion

        #region Private Methods

        private void EmployTFSServices(string impersonateeDomainName, ICredentials credential)
        {
            if (string.IsNullOrEmpty(DefaultCollectionName))
                throw new Exception("There is no Collection Name was set for initiating tfs service.");
            if (string.IsNullOrEmpty(TFSServerUri))
                throw new Exception("There is no Server URI was set for initiating tfs service.");

            TeamProjectCollectionInstance = string.IsNullOrEmpty(impersonateeDomainName)
                ? GetTFSCollection(credential)
                : GetImpersonatedCollection(impersonateeDomainName, credential);

            TeamProjectCollectionInstance.Authenticate();



        }
        private TfsTeamProjectCollection GetImpersonatedCollection(string impersonateeDomainName,
            ICredentials credential)
        {
            var currentCollection = GetTFSCollection(credential);

            var identityService = new IdentityService(currentCollection);
            var identity = identityService.GetTFSIdentity(impersonateeDomainName, IdentitySearchFactor.AccountName);

            return credential != null
                ? new TfsTeamProjectCollection(TFSCollectionUri,
                    new TfsClientCredentials(new WindowsCredential(credential)), identity.Descriptor)
                : new TfsTeamProjectCollection(TFSCollectionUri, identity.Descriptor);
        }
        private TfsTeamProjectCollection GetTFSCollection(ICredentials credential)
        {
            return credential != null
                ? new TfsTeamProjectCollection(TFSCollectionUri, credential)
                : new TfsTeamProjectCollection(TFSCollectionUri);
        }

        private void GetTFSConfigurationServer()
        {
            _tfsConfigurationServer = new TfsConfigurationServer(new Uri(TFSServerUri), Credential);
        }
        private void GetTFSJobService()
        {
            _teamFoundationJobService = ConfigurationServer.GetService<TeamFoundationJobService>();
        }
        #endregion

    }
}
