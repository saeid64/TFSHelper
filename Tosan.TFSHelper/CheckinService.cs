using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Server;

namespace Tosan.TFSHelper
{
    class CheckinService : TFSService
    {
        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public CheckinService()
            : base()
        {
        }

        /// <summary>
        /// Initilizing TFS Service via 'Team Foundation Request Conxtext' whcich usually is passing through tfs plugins.
        /// </summary>
        /// <param name="requestContext">Team Foundation Request Conxtext</param>
        public CheckinService(TeamFoundationRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// If you Initilized the service before and you have the projectcollection you can use this constructor.
        /// </summary>
        /// <param name="projectCollection">Team Foundation Project Collection</param>
        public CheckinService(TfsTeamProjectCollection projectCollection)
            : base(projectCollection)
        {
        }

        /// <summary>
        /// Initilizing TFS Service.
        /// </summary>

        #endregion
    }
}
