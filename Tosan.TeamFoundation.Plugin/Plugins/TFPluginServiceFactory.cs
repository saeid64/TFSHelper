using Tosan.TeamFoundation.Plugin.Core.Plugins.Concrete;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins
{
    internal class TFPluginServiceFactory
    {
        public ITFPlugin CreateWorkItemPlugin(object extensionData)
        {
            return new TFWorkItemPlugin(extensionData);
        }

        public ITFPlugin CreateRequestFilterWorkItemUpdatePlugin(object extensionData)
        {
            return new TFRequestFilterUpdateWorkItemsPlugin(extensionData);
        }
        public ITFPlugin CreateCheckInPlugin(object extensionData)
        {
            return new TFCheckInPlugin(extensionData);
        }
        public ITFPlugin CreateRequestFilterBulkUpdatePlugin( object extensionData)
        {
            return new TFRequestFilterBulkUpdatePlugin(extensionData);
        }
        public ITFPlugin CreatePushPlugin(object extensionData)
        {
            return new TFPushPlugin(extensionData);
        }
    }
}
