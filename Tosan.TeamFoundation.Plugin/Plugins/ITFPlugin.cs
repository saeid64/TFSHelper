using Microsoft.TeamFoundation.Framework.Server;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins
{
    interface ITFPlugin
    {
        TFPluginProcessResponse Process(TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType);
        TFPluginProcessResponse ProcessAsync(TeamFoundationRequestContext teamfoundationRequestContext, string pluginExecutionType);

    }
}
