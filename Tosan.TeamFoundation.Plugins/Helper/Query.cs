using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tosan.TeamFoundation.Plugin.Core.Helper
{
    internal static class Query
    {
        public static string GetTaskByParentTriage(int parentId, string triage, string linkType)
        {
            return string.Format(@"SELECT [System.ID],
                                [System.WorkItemType],
                                [System.Title],
                                [System.AssignedTo],
                                [System.State],
                                [Microsoft.VSTS.Common.Triage]
                                FROM WorkItemLinks 
                                WHERE ([Source].[System.WorkItemType] = 'Requirement'  
                                AND  [Source].[System.RelatedLinkCount] > 0  
                                AND  [Source].[System.ID] = {0}) 
                                AND ([System.Links.LinkType] = '{1}')
                                AND [Target].[Microsoft.VSTS.Common.Triage] <> '{2}'
                                 ORDER BY [System.ID] mode(Recursive)", parentId, linkType, triage);
        }
        public static string GetWorkItemRestrictionCheckInPolicy(string project, string userDisplayName)
        {
            return string.Format(@"SELECT [System.Id]
                                FROM WorkItems WHERE 
                                [System.WorkItemType] = 'Dev Task'  
                                AND  [System.State] = 'Active'  
                                AND  [System.AssignedTo] = '{1}'
                                ORDER BY [System.Id]", project, userDisplayName);
        }
    }
}
     