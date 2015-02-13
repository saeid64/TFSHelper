namespace TFSHelper.Plugin.Core.Plugins
{
    class RequestFilterPlugin : ITeamFoundationRequestFilter
    {

        public void BeginRequest(TeamFoundationRequestContext requestContext)
        {

        }

        public void RequestReady(TeamFoundationRequestContext requestContext)
        {
        }

        public void EnterMethod(TeamFoundationRequestContext requestContext)
        {
            var pluginSericeFactory = new TFPluginServiceFactory();
            var requestContextNethodName = requestContext.Method.Name.ToLower();
            ITFPlugin tfplugin = null;

            if (requestContextNethodName == "apiwit.updateworkitems")
                tfplugin = pluginSericeFactory.CreateRequestFilterWorkItemUpdatePlugin(null);
            else if (requestContextNethodName == "bulkupdate")
                tfplugin = pluginSericeFactory.CreateRequestFilterBulkUpdatePlugin(null);

            if (tfplugin == null) return;
            var pluginServiceProcessResponse = tfplugin.Process(requestContext, "PreUpdate");
            if (!pluginServiceProcessResponse.IsValid)
                requestContext.Cancel(pluginServiceProcessResponse.Message);

        }

        public void LeaveMethod(TeamFoundationRequestContext requestContext)
        {
        }

        public void EndRequest(TeamFoundationRequestContext requestContext)
        {
        }
    }
}
