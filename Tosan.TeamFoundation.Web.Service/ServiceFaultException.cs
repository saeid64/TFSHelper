using System;
using System.Collections.Generic;
using log4net;

namespace TosanTFS.Web.Service
{
    public class ServiceFaultException : Exception
    {
        private readonly ILog _logger = LogManager.GetLogger("ServiceFaultLogger");
        public ServiceFaultException(IEnumerable<KeyValuePair<string, string>> logValueData, ServiceType serviceType, string message)
            : base(message)
        {
            foreach (var logValue in logValueData)
                ThreadContext.Properties[logValue.Key] = logValue.Value;

            ThreadContext.Properties["PluginType"] = serviceType.ToString();

            _logger.Fatal(message, this);
        }
    }
    public enum ServiceType
    {
        WorkItemTrackingService = 0,
        VersionControlService = 1
    }
}