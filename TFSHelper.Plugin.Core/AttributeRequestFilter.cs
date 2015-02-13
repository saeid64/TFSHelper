using System;
using System.Collections.Generic;
using System.Linq;
using TFSHelper.Helper.Model;

namespace TFSHelper.Plugin.Core
{
    public class AttributeRequestFilter : IRequestFilter
    {
        private static List<PluginKey> _pluginKeys;
        public AttributeRequestFilter(Func<TFSItem, bool> filterPredicate)
        {
            FilterPredicate = filterPredicate;
        }
        public AttributeRequestFilter(Func<TFSItem, bool> filterPredicate, string changedField)
        {
            FilterPredicate += filterPredicate;
            FilterPredicate += item => item.Fields[changedField] != null && item.Fields[changedField].IsDirty;
        }
        public AttributeRequestFilter(Func<TFSItem, bool> filterPredicate, string[] changedFields)
        {
            FilterPredicate += filterPredicate;
            foreach (var changedField in changedFields)
            {
                var field = changedField;
                FilterPredicate += item => item.Fields[field] != null && item.Fields[field].IsDirty;
            }
        }
        public Func<TFSItem, bool> FilterPredicate { get; set; }
        public bool IsValid(TFSEventArgs args)
        {
            return FilterPredicate(args.TFSEventItem);
        }

        public bool IsRegistered(TFSEventArgs args, string pluginKey)
        {
            return IsRegisteredPlugin(args.TFSEventItem.ProjectName, args.TFSEventItem.TpcName, pluginKey);
        }

        private static bool IsRegisteredPlugin(string projectName, string tpcName, string pluginKey)
        {
            if (_pluginKeys == null || !_pluginKeys.Any())
                _pluginKeys = Utility.PluginConfigurationParser.GetRegisteredPlugins(tpcName).ToList();
            return _pluginKeys.Any(pKey => pKey.ProjectName == projectName && pKey.KeyName == pluginKey);
        }
    }

    internal class PluginKey
    {
        internal string KeyName { get; set; }
        internal string ProjectName { get; set; }
        internal string AreaName { get; set; }
        internal string CollectionName { get; set; }
    }
}
