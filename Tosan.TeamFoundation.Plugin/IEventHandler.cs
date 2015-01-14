namespace Tosan.TeamFoundation.Plugin.Core
{
    public interface IEventHandler
    {
        void Register(TFSEventAggregator aggregator);
    }
}
