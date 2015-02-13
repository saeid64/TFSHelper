using System;
using TFSHelper.Helper;
using TFSHelper.Helper.Model;

namespace TFSHelper.Plugin.Core
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
