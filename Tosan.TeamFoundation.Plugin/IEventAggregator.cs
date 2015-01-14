using System;
using Tosan.TeamFoundation.Plugin.Core.Utility;

namespace Tosan.TeamFoundation.Plugin.Core
{
    public interface IEventAggregator
    {
        void Subscribe(RequestTypeEnum requestType, Action<TFSEventArgs> action);
        void Subscribe(RequestTypeEnum requestType, Action<TFSEventArgs> action, IRequestFilter requestFilter, int priority = 0);
        void Publish(RequestTypeEnum tfSeventType, TFSEventArgs tfsEventArgs);
    }
}
