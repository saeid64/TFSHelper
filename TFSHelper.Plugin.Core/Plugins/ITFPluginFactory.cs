namespace TFSHelper.Plugin.Core.Plugins
{
    interface ITFPluginFactory
    {
        ITFPlugin CreateTFPlugin(TeamFoundationRequestContext teamfoundationRequestContext, object extensionData);
    }
}
