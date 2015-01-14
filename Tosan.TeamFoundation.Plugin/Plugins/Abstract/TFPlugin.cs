using System;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Framework.Server;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins.Abstract
{
    abstract class TFPlugin : ITFPlugin
    {
        protected object ExtentionData;
        
        protected abstract TFSEventArgs ConvertToTFEventArgs(TeamFoundationRequestContext requestContext,
            TFSEventArgs.ProcessActionResponse processActionResponse = null);

        protected TFPlugin(object extentionData = null)
        {
            if (extentionData != null)
                ExtentionData = extentionData;
        }

        public TFPluginProcessResponse Process(TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType)
        {
            
            var statusmsg = string.Empty;
            var isProcessValid = true;
            try
            {
                var aggregator = new TFSEventAggregator();
                aggregator.Init();

                aggregator.Publish(Utility.RequestType.PluginExecutionType[pluginExecutionType], ConvertToTFEventArgs(teamfoundationRequestContext,
                    delegate(bool isValid, string message)
                    {
                        statusmsg = message;
                        isProcessValid = isValid;
                    }
                    ));

                return new TFPluginProcessResponse() {IsValid = isProcessValid, Message = statusmsg};
            }
            catch (Exception e)
            {
                throw new TeamFoundationServerException(e.Message);
            }
        }

        public TFPluginProcessResponse ProcessAsync(TeamFoundationRequestContext teamfoundationRequestContext,
            string pluginExecutionType)
        {
            throw new NotImplementedException();
        }
    }
}
