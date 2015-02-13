namespace TFSHelper.Plugin.Core
{
    public interface IRequestFilter
    {
        bool IsValid(TFSEventArgs args);
        bool IsRegistered(TFSEventArgs args, string pluginKey);
    }
}
