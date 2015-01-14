using Microsoft.TeamFoundation.Framework.Server;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins
{
    interface ITFPluginFactory
    {
        ITFPlugin CreateTFPlugin(TeamFoundationRequestContext teamfoundationRequestContext, object extensionData);
    }
}
