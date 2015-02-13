[assembly: log4net.Config.XmlConfigurator(ConfigFileExtension = "log4net", Watch = true, ConfigFile = "TosanTFSLog.config")]
namespace TFSHelper.Web.Service
{
    internal static class Logger
    {

        static Logger()
        {

        }
        public static void LogExtention(string dataToLog)
        {
            ThreadContext.Properties["Data"] = dataToLog;
        }
    }
}
