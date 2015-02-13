using System;
using System.Collections.Generic;
using System.Linq;

namespace TFSHelper.Web.Service
{
    public class TFSBuildNotification
    {
       /// <summary>
      /// this function update buildNotifications table when build run
       /// </summary>
       /// <param name="labelTittle"></param>
       /// <param name="dir"></param>
       /// <param name="changeSets"></param>
        public static void UpdateBuildTabl(string labelTittle, string dir, List<TFSItem> changeSets)
        {
        
               var dbconnect = new TFSExtensionCon();

               if (dbconnect.bn_Label.Any(w => w.Label == labelTittle))
                   return;

               foreach (var changeSet in changeSets)
               {
                   var chLinks = changeSet.LinkItems;

                   if (chLinks == null)
                       continue;
                   var bnLabel = dbconnect.bn_Label.Add(new bn_Label { id = Guid.NewGuid(), Label = labelTittle, Dir = dir });
                   var bnChangeset = new bn_Changeset
                   {
                       id = Guid.NewGuid(),
                       ChangeSetId = changeSet.Id
                   };
                   foreach (var wi in chLinks)
                   {
                       bnChangeset.bn_WorkItems.Add(new bn_WorkItems
                       {
                           id = Guid.NewGuid(),
                           Area = wi.Item.Area,
                           Customer = GetFilds(wi, "Tosan.Customers"),
                           FixedVersion = GetFilds(wi, "Tosan.FixedVersion"),
                           RFCNumber = GetFilds(wi, "Tosan.RFCNumber"),
                           Severity = GetFilds(wi, "Microsoft.VSTS.Common.Severity"),
                           State = GetFilds(wi, "System.State"),
                           Title = wi.Item.Title
                       });
                   }
                   bnLabel.bn_Changeset.Add(bnChangeset);
                   dbconnect.SaveChanges();
               }

          

        }

        private static string GetFilds(TFSLinkField wi, string nameFild)
        {
            var fildContents = wi.Item.Fields.SingleOrDefault(field => field.Id == nameFild);
            return (fildContents != null ? fildContents.Value.ToString() : "");

        }


        /// <summary>
        /// update the work items and parent buildNumber when build run 
        /// </summary>
        /// <param name="labelTittle"></param>
        /// <param name="changeSets"></param>
        public static void UpdateWorkItemsandParentBuildNumbers(string labelTittle, List<TFSItem> changeSets)
        {
            var versionService = new WorkItemTracking();
            foreach (var chSet in changeSets)
            {
                var chLinks = chSet.LinkItems;
                if (chLinks == null)
                    continue;

                var changeSetLinks = chLinks.Where(chl => chl.Item != null && (chl.Item.Fields.SingleOrDefault(field => field.Id == "State" || field.Id == "System.State").Value.ToString() == "Resolved"));
                var parentList = new List<int>();
                var tfsLinkFields = changeSetLinks as IList<TFSLinkField> ?? changeSetLinks.ToList();
                var linkFields = tfsLinkFields.Where(field => field.Item.LinkItems != null
                    && field.Item.LinkItems.Any(linkField => linkField.LinkName == "Parent"))
                    .Select(chl => chl.Item.LinkItems.SingleOrDefault(linkItem => linkItem.LinkName == "Parent"));
                parentList.AddRange(linkFields.Where(field => field != null).Select(field => field.ID));
                //update Parent 
                foreach (var parentId in parentList)
                {
                    var parentItem = versionService.GetWorkItem(parentId, "Tosan");
                    var buildNumbers = parentItem.Fields.SingleOrDefault(field => field.Id == "Tosan.BuildNumbers");
                    var fieldList = new List<TFSField>
                         {
                        new TFSField {Id = "Tosan.BuildNumbers", Value = (buildNumbers == null ? string.Empty : buildNumbers.Value) + " \n\r " +labelTittle }
                         };
                    versionService.UpdateFields(parentId, fieldList, @"Tosanltd\TFS", "Tosan");
                }
                //update WorkItem
                foreach (var chLink in tfsLinkFields)
                {
                    var buildNumbers = chLink.Item.Fields.SingleOrDefault(field => field.Id == "Tosan.BuildNumbers");
                    var WIfieldList = new List<TFSField>
                         {
                      new TFSField {Id = "Tosan.BuildNumbers", Value = (buildNumbers == null ? string.Empty : buildNumbers.Value) + " \n\r " +labelTittle }
                         };

                    versionService.UpdateFields(chLink.Item.Id, WIfieldList, @"Tosanltd\TFS", "Tosan");

                }

            }
        }
    }
}