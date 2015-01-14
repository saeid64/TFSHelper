using System;
using Microsoft.TeamFoundation.WorkItemTracking.Server;
using Tosan.TFSHelper;
using Tosan.TFSHelper.Model;

namespace Tosan.TeamFoundation.Plugin.Core
{
    public class TFSEventArgs : EventArgs
    {
        private TFSService _tfsService;
        public delegate void ProcessActionResponse(bool isValid, string message);
        public TFSService ContextTFSHelper
        {
            get
            {
                if (!_tfsService.IsInitiated)
                    _tfsService.Init();
                return _tfsService;
            }
            set { _tfsService = value; }
        }
        public TFSItem TFSEventItem { get; set; }
        public ProcessActionResponse ResponseAction { get; set; }
        public WorkItemChangedEvent NotificationEventArgs { get; set; }
    }
}
