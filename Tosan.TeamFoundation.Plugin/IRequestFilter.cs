namespace Tosan.TeamFoundation.Plugin.Core
{
    public interface IRequestFilter
    {
        bool IsValid(TFSEventArgs args);
        bool IsRegistered(TFSEventArgs args, string pluginKey);
    }
}
