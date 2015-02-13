using System;

namespace TFSHelper.Plugin.Core.Utility
{
    public class TosanTeamFoundationException : Exception
    {
        public Level LogLevel { get; set; }
        public TosanTeamFoundationException(string message) : base(message)
        {
        }

        public TosanTeamFoundationException(string message, Level logLevel):base(message)
        {
            LogLevel = logLevel;
        }
    }
}
