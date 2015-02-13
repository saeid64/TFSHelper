using System.Linq;
using TFSHelper.Plugin.Core.Utility;

[assembly: log4net.Config.XmlConfigurator(ConfigFileExtension = "log4net", Watch = true, ConfigFile = "TosanTFSLog.config")]
namespace TFSHelper.Plugin.Core
{
    internal static class Logger
    {
        static Logger()
        {

        }
        public static void Initialize(TFSEventArgs tfsEventArgs, RequestTypeEnum eventType)
        {
            ThreadContext.Properties["WorkItemId"] = tfsEventArgs.TFSEventItem.Id;
            ThreadContext.Properties["TeamProjectCollection"] = tfsEventArgs.TFSEventItem.TpcName;
            ThreadContext.Properties["TeamProject"] = tfsEventArgs.TFSEventItem.ProjectName;
            ThreadContext.Properties["WorkItemType"] = tfsEventArgs.TFSEventItem.Fields[FieldNames.Type].NewValue;
            ThreadContext.Properties["UserId"] = tfsEventArgs.TFSEventItem.Changer;
            ThreadContext.Properties["PluginType"] = tfsEventArgs.TFSEventItem.ItemEventType.ToString();
            ThreadContext.Properties["EventType"] = RequestType.PluginExecutionType.Single(s => s.Value == eventType).Key;
            ThreadContext.Properties["AreaPath"] = tfsEventArgs.TFSEventItem.Area;
        }
        public static void LogExtention(string dataToLog)
        {
            ThreadContext.Properties["Data"] = dataToLog;
        }
    }
}
