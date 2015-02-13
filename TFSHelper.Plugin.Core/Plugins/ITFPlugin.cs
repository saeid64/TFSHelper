namespace TFSHelper.Plugin.Core.Plugins
{
    interface ITFPlugin
    {
        TFPluginProcessResponse Process(TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType);
        TFPluginProcessResponse ProcessAsync(TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType);

    }
}
