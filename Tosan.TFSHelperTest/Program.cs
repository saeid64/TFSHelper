using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Tosan.TFSHelper;

namespace Tosan.TFSHelperTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //creating instance of WorkItem Service
            var workItemService = new WorkItemService()
            {
                //Optional Parameters:
                //Credential = new NetworkCredential("SomeUser","Password","CompanyDomain"),
                //ImpersonateeDomainName = "SomeUserDomainName",
                //TFSServerUri = "http://YourTFSServer:8080/tfs", //like this project can be specified in application configuration as 'TFSServerUri' in appSettings section
                //DefaultCollectionName = "YourDefaultProjectCollectionName" //like this project can be specified in application configuration as 'TFSCollectionName' in appSettings section
            };
            //Initiating Services
            workItemService.Init();

            //Now Calling some extension services

            var globalListValues = workItemService.GetGlobalListValues("GlobalListName");

            var workItem = workItemService.GetWorkItem(40579);

            var tfsItem = workItemService.GetTFSItem(workItem);

            var parentWorkItem = workItemService.GetParentWorkItem(40579, WorkItemLinkType.Topology.Tree);

            var collections = workItemService.GetCollectionsName();

        }
    }
}
