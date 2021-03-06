﻿using System;
using System.Configuration;
using System.Runtime.Caching;

namespace TFSHelper.Plugin.Core.Utility
{
    class OrganizationXrmUtility
    {
        internal static IOrganizationService GetOrganizationService()
        {
            try
            {
                var connection = new CrmConnection(new ConnectionStringSettings("xrm", @""))
                    {
                        ServiceUri = new Uri(@"")
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
