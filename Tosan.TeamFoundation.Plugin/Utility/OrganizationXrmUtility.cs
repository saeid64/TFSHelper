using System;
using System.Configuration;
using System.Runtime.Caching;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;

namespace Tosan.TeamFoundation.Plugin.Core.Utility
{
    class OrganizationXrmUtility
    {
        internal static IOrganizationService GetOrganizationService()
        {
            try
            {
                var connection = new CrmConnection(new ConnectionStringSettings("xrm", @"Url=https://crm2011.tosanltd.com/Tosan;Domain=tosanltd.com; Username=tfs; Password=hsb_1234;"))
                    {
                        ServiceUri = new Uri(@"https://crm2011.tosanltd.com/Tosan/XRMServices/2011/Organization.svc")
                    };
                var myObjectCache = MemoryCache.Default;
                var myServiceCache = new OrganizationServiceCache(myObjectCache, connection);
                return new CachedOrganizationService(connection, myServiceCache);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
