namespace TFSHelper.Web.Service
{
    internal static class WorkItemQeury
    {
        internal static string GetWorkItemByActivityIdQuery(string activityId)
        {
            return string.Format(@"Select * from WorkItems
                                    Where [Tosan.ActivityId] = '{0}'
                                    AND ( [System.WorkItemType] = 'Dev Task' 
                                           OR [System.WorkItemType] = 'Analysis Task'
                                           OR [System.WorkItemType] = 'Task'
                                           OR [System.WorkItemType] = 'Test Task' )", activityId);
        }
    }
}