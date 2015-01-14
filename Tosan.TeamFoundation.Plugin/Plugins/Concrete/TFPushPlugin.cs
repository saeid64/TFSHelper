using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Framework.Server;
using Tosan.TeamFoundation.Plugin.Core.Plugins.Abstract;

namespace Tosan.TeamFoundation.Plugin.Core.Plugins.Concrete
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
