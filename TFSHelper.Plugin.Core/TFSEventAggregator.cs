using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TFSHelper.Plugin.Core.Utility;

namespace TFSHelper.Plugin.Core
{
    public class TFSEventAggregator : IEventAggregator
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<string, PluginSubscription> _subscriptions;
        public TFSEventAggregator()
        {
            _subscriptions = new Dictionary<string, PluginSubscription>();
        }

        public void Subscribe(RequestTypeEnum requestType, Action<TFSEventArgs> action)
        {
            Subscribe(requestType, action, null);
        }

        public void Subscribe(RequestTypeEnum requestType, Action<TFSEventArgs> action, IRequestFilter requestFilter, int priority = 0)
        {
            var key = requestType + action.Method.Name;
            _subscriptions.Add(key, new PluginSubscription { Action = action, RequestFilter = requestFilter, Priority = priority });
        }

        public void Publish(RequestTypeEnum eventType, TFSEventArgs tfsEventArgs)
        {
            if (!RequestType.PluginExecutionType.ContainsValue(eventType)) return;

            var subscriptions = new List<PluginSubscription>();
            foreach (var subscription in _subscriptions)
            {
                var key = eventType + subscription.Value.Action.Method.Name;
                if (key != subscription.Key) continue;

                var isRequestValid = subscription.Value.RequestFilter != null
                    && subscription.Value.RequestFilter.IsRegistered(tfsEventArgs, key)
                    && subscription.Value.RequestFilter.IsValid(tfsEventArgs);

                if (isRequestValid) subscriptions.Add(subscription.Value);
            }

            if (!subscriptions.Any()) return;

            Logger.Initialize(tfsEventArgs, eventType);
            if (eventType == RequestTypeEnum.PreUpdate) //Invokes Tasks Synchronously (PreUpdate Tasks)
            {
                //Prioritizing subscription methods to runs in order
                var prioritizedSubscriptions = subscriptions.OrderByDescending(subscription => subscription.Priority);
                foreach (var pluginSubscription in prioritizedSubscriptions)
                {
                    try
                    {
                        pluginSubscription.Action.Invoke(tfsEventArgs);
                        _logger.Info(pluginSubscription.Action.Method.Name);
                    }
                    catch (TFSHelperException exception)
                    {
                        ((log4net.Repository.Hierarchy.Logger)_logger.Logger).Log(exception.LogLevel,
                            exception.Message, new Exception(exception.StackTrace));
                        return;
                    }
                    catch (Exception e)
                    {
                        _logger.Fatal(e.Message, new Exception(e.StackTrace));
                        return;
                    }
                }
            }
            else //Invokes Tasks Asynchronously (Update/New Tasks)
                foreach (var pluginSubscription in subscriptions)
                {
                    var asyncResult = pluginSubscription.Action.BeginInvoke(tfsEventArgs, null, null);

                    try
                    {
                        pluginSubscription.Action.EndInvoke(asyncResult);
                        _logger.Info(pluginSubscription.Action.Method.Name);
                    }
                    catch (TFSHelperException exception)
                    {
                        ((log4net.Repository.Hierarchy.Logger)_logger.Logger).Log(exception.LogLevel,
                            exception.Message, new TFSHelperException(exception.StackTrace));
                    }
                    catch (Exception e)
                    {
                        _logger.Fatal(e.Message, new Exception(e.StackTrace));
                    }
                }
        }

        private class PluginSubscription
        {
            public IRequestFilter RequestFilter;
            public Action<TFSEventArgs> Action;
            public int Priority;
        }

        public void Init()
        {
            var assembly = Assembly.Load("TFSHelper.Plugins");
            var plugInType = typeof(IEventHandler);
            assembly.GetTypes().Where(t => plugInType.IsAssignableFrom(t) && t.IsAbstract == false)
                .Select(Activator.CreateInstance)
                .Where(x => x != null)
                .Cast<IEventHandler>()
                .ToList()
                .ForEach(p => p.Register(this));
        }
    }
}
