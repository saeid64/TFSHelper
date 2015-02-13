using System.Collections.Generic;

namespace TFSHelper.Plugin.Core.Utility
{
    public static class RequestType
    {
        public static Dictionary<string, RequestTypeEnum> PluginExecutionType = new Dictionary<string, RequestTypeEnum>
        {
          { "New", RequestTypeEnum.Created }, 
          { "Change",  RequestTypeEnum.Updated }, 
          { "PreUpdate",  RequestTypeEnum.PreUpdate },
        };

        public const string DevTask = "Dev Task";
        public const string Task = "Task";
        public const string TestTask = "Test Task";
        public const string Bug = "Bug";
        public const string Requirement = "Requirement";
        public const string AnalysisTask = "Analysis Task";
        public const string ProductBacklogItem = "Product Backlog Item";
        public const string Feature = "Feature";

    }

    public enum RequestTypeEnum
    {
        Created = 1, //Async Request
        Updated = 2, //Async Request
        PreUpdate = 3 //Sync Request
    }
}
