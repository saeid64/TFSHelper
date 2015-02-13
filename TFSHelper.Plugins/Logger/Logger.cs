using TFSHelper.Plugin.Core;

[assembly: log4net.Config.XmlConfigurator(ConfigFileExtension = "log4net", Watch = true, ConfigFile = "Log.config")]
namespace TFSHelper.Plugins.Logger
{
    internal static class Logger
    {
        static Logger()
        {

        }
        public static void Initialize(TFSEventArgs tfsEventArgs, string eventType)
        {
            ThreadContext.Properties["WorkItemId"] = tfsEventArgs.TFSEventItem.Id;
            ThreadContext.Properties["TeamProjectCollection"] = tfsEventArgs.TFSEventItem.TpcName;
            ThreadContext.Properties["TeamProject"] = tfsEventArgs.TFSEventItem.ProjectName;
            ThreadContext.Properties["WorkItemType"] = tfsEventArgs.TFSEventItem.Fields["System.WorkItemType"].NewValue;
            ThreadContext.Properties["UserId"] = tfsEventArgs.TFSEventItem.Changer;
            ThreadContext.Properties["PluginType"] = tfsEventArgs.TFSEventItem.ItemEventType.ToString();
            ThreadContext.Properties["EventType"] = eventType;
            ThreadContext.Properties["AreaPath"] = tfsEventArgs.TFSEventItem.Area;
        }
        public static void LogExtention(string dataToLog)
        {
            ThreadContext.Properties["Data"] = dataToLog;
        }
    }
}
