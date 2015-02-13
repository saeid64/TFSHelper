using System;
using TFSHelper.Plugin.Core.Plugins.Abstract;

namespace TFSHelper.Plugin.Core.Plugins.Concrete
{
    class TFPushPlugin : TFPlugin
    {
        internal TFPushPlugin(Object extentionData) : base(extentionData) { }
        protected override TFSEventArgs ConvertToTFEventArgs(TeamFoundationRequestContext requestContext,
            TFSEventArgs.ProcessActionResponse processActionResponse = null)
        {
            throw new NotImplementedException();
        }
    }
}
