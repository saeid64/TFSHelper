using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;

namespace Tosan.TFSHelper
{
    public class IdentityService
    {
        public IIdentityManagementService IdentityManagementService { get; set; }
        private readonly TfsTeamProjectCollection _projectCollection;

        public IdentityService(TfsTeamProjectCollection projectCollection)
        {
            _projectCollection = projectCollection;
            // Get the TFS Identity Management Service
            IdentityManagementService =
               projectCollection.GetService<IIdentityManagementService>();
        }
        public TeamFoundationIdentity GetTFSIdentity(string searchValue, IdentitySearchFactor identitySearchFactor)
        {
            // Look up the user that we want to impersonate
            return IdentityManagementService.ReadIdentity(identitySearchFactor,
                                                       searchValue, MembershipQuery.None, ReadIdentityOptions.None);
        }
    }
}
