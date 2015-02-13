using System;

namespace TFSHelper.Plugin.Core.Utility
{
    public class TFSHelperException : Exception
    {
        public Level LogLevel { get; set; }
        public TFSHelperException(string message) : base(message)
        {
        }

        public TFSHelperException(string message, Level logLevel):base(message)
        {
            LogLevel = logLevel;
        }
    }
}
