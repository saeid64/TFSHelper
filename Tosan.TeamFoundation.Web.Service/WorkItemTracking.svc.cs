using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TosanTFS.Web.Service
{
    public class WorkItemTracking : IWorkItemTracking
    {
        #region Private Variables

        private TFSHelper _helper;
        private string _noteSignature;
        private const string TempFolder = @"\Tempfiles\";

        #endregion

        #region WorkItem Tracking Service Methods

        public void Init(string uri)
        {
            _helper = new TFSHelper(uri);
        }

        public int CreateWorkItem(TFSItem item, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var proj = _helper.Store.Projects[item.ProjectName];

            var wi = new WorkItem(proj.WorkItemTypes[item.WorkItemType]);

            foreach (var field in item.Fields.Where(field => field.Value != null))
                wi[field.Id] = field.Value;

            var validation = wi.Validate();
            wi.Save();
            return wi.Id;
        }

        public int CreateImpersonateWorkItem(TFSItem item, string collectionName, string impersonateUsername)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName, impersonateUsername);
            var proj = _helper.Store.Projects[item.ProjectName];

            var wi = new WorkItem(proj.WorkItemTypes[item.WorkItemType]);

            foreach (var field in item.Fields.Where(field => field.Value != null))
                wi[field.Id] = field.Value;

            var validation = wi.Validate();
            if (validation != null && validation.ToArray().Any())
                throw new Exception(string.Format("In field {0} and for value {1} can't save WorkItem",
                                                  ((Field)validation[0]).Id, ((Field)validation[0]).Value));
            wi.Save();
            return wi.Id;
        }

        public void ChangeWorkItemState(int itemId, string state, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var wi = _helper.GetWorkItem(itemId);
            var oldState = wi["System.State"];

            if (Equals(oldState, state)) return;

            wi.Open();
            wi["System.State"] = state;

            var validated = wi.Validate();
            if (validated != null && validated.ToArray().Any())
                throw new Exception("The State is Not valid for change!");

            wi.Save();
            wi.Close();
        }

        public void LinkWorkItems(int sourceId, int targetId, string linkType, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var wi = _helper.GetWorkItem(sourceId);

            var linkTypeEnd = _helper.Store.WorkItemLinkTypes.LinkTypeEnds[linkType];
            wi.WorkItemLinks.Add(new WorkItemLink(linkTypeEnd, targetId));
            wi.Save();
        }

        public List<string> GetIterations(string collectionName, string projectName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var proj = _helper.Store.Projects[projectName];

            var lstIterations = new List<string>();
            foreach (Node node in proj.IterationRootNodes)
            {
                lstIterations.Add(node.Name);
                lstIterations.AddRange(from Node item in node.ChildNodes select item.Name);
            }
            return lstIterations;
        }

        public void UpdateFields(int workItemId, List<TFSField> fields, string impersonateUsername,
                                 string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName, impersonateUsername);

            var wi = _helper.GetWorkItem(workItemId);
            wi.Open();
            foreach (var field in fields.Where(field => field != null && field.Value != null && wi[field.Id] != field.Value))
                wi[field.Id] = field.Value;

            var validatedFields = wi.Validate();
            if (validatedFields != null && validatedFields.ToArray().Any())
                throw new Exception(string.Format("In field {0} and for value {1} can't save WorkItem",
                                                  ((Field)validatedFields[0]).Id, ((Field)validatedFields[0]).Value));

            if (!wi.Fields.OfType<Field>().Any(field => field.IsDirty)) return;
            wi.Save();
            wi.Close();
        }

        /// <summary>
        /// Warning: Be Aware of Using this Method! 
        /// </summary>
        public void UpdateFieldsBypassRules(int workItemId, List<TFSField> fields, string impersonateUsername,
                                            string collectionName)
        {
            try
            {
                if (_helper == null)
                    _helper = new TFSHelper(collectionName);

                var wi = _helper.GetWorkItem(workItemId, WorkItemStoreFlags.BypassRules);

                var flds =
                    new List<TFSField>(
                        fields.Where(field => field != null && field.Value != null && wi[field.Id].ToString() != field.Value.ToString()));
                if (!flds.Any()) return;

                wi.Open();

                foreach (var field in flds)
                    wi[field.Id] = field.Value;

                if (!wi.Fields.OfType<Field>().Any(field => field.IsDirty)) return;
                wi.Save();
                if (fields.Any(field => (field.Id == "System.State" || field.Id == "State") && field.Value != null))
                    BypassRulesFieldConfig(wi, GetUserDisplayName(impersonateUsername, collectionName));
                wi.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public List<int> RunWIQL(string query, string collectionName)
        {
            _helper = new TFSHelper(collectionName);

            var workItemsByQuery = _helper.GetWorkItemsByQuery(query);

            return workItemsByQuery.Count != 0
                       ? workItemsByQuery.OfType<WorkItem>().Select(item => item.Id).ToList()
                       : null;
        }

        public TFSItem GetWorkItem(int wiId, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);
            return _helper.GetWorkItem(wiId).ToTfsItem();
        }

        public List<int> GetLinkedWorkItem(int wiId, WorkItemLinkTypeTopology topology, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var _topology = WorkItemLinkType.Topology.Tree;

            if (topology == WorkItemLinkTypeTopology.Network)
                _topology = WorkItemLinkType.Topology.Network;
            else if (topology == WorkItemLinkTypeTopology.Tree)
                _topology = WorkItemLinkType.Topology.Tree;

            var wi = _helper.GetWorkItem(wiId);

            return (from l in wi.WorkItemLinks.OfType<WorkItemLink>()
                    where l.LinkTypeEnd.LinkType.LinkTopology == _topology
                    select l.TargetId).ToList();
        }

        public void AddImpersonateHistoryComment(int wiId, string comment, string createdBy, string createdOn,
                                                 string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var workitem = _helper.GetWorkItem(wiId);
            _noteSignature = string.Format(" <b> By {0}, At {1}</b>", createdBy, createdOn);

            if (
                workitem.Revisions.OfType<Revision>()
                        .Any(revision => revision.Fields["History"].Value.ToString().Contains(_noteSignature)))
                return;

            workitem.Open();

            workitem["History"] = comment + _noteSignature;
            workitem.Save();
            workitem.Close();
        }

        public void AddImpersonateAttachment(int wiId, string fileComment, string fileName, byte[] fileByteArray,
                                             string impersonateUsername, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName, impersonateUsername);
            var workitem = _helper.GetWorkItem(wiId);
            if (workitem.Attachments.Count != 0)
                if (workitem.Attachments.OfType<Attachment>().Any(attachment => attachment.Name == fileName))
                    return;

            var tempfolder = Directory.GetCurrentDirectory() + TempFolder;
            var fileAddress = tempfolder + fileName;
            if (!File.Exists(fileAddress))
            {
                if (!Directory.Exists(tempfolder))
                {
                    var securityRules = new DirectorySecurity();
                    securityRules.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.Read,
                                                                         AccessControlType.Allow));
                    securityRules.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl,
                                                                         AccessControlType.Allow));

                    Directory.CreateDirectory(tempfolder, securityRules);
                }
                File.WriteAllBytes(fileAddress, fileByteArray);
            }

            workitem.Open();
            workitem.Attachments.Add(new Attachment(fileAddress, fileComment));
            workitem.Save();
            workitem.Close();

            File.Delete(fileAddress);
        }

        public void AddHistoryComment(int wiId, string comment, string createdBy, string createdOn,
                                      string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var workitem = _helper.GetWorkItem(wiId);
            _noteSignature = string.Format(" - ثبت شده توسط <b> {0}</b>, در <b>{1}</b>", createdBy, createdOn);

            if (
                workitem.Revisions.OfType<Revision>()
                        .Any(revision => revision.Fields["History"].Value.ToString().Contains(_noteSignature)))
                return;

            workitem.Open();

            workitem["History"] = comment + _noteSignature;
            workitem.Save();
            workitem.Close();
        }

        public void AddAttachment(int wiId, string fileComment, string fileName, byte[] fileByteArray, string createdBy,
                                  string createdOn, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);
            var workitem = _helper.GetWorkItem(wiId);
            if (workitem.Attachments.Count != 0)
                if (workitem.Attachments.OfType<Attachment>().Any(attachment => attachment.Name == fileName))
                    return;

            var tempfolder = Directory.GetCurrentDirectory() + TempFolder;
            var fileAddress = tempfolder + fileName;
            if (!File.Exists(fileAddress))
            {
                if (!Directory.Exists(tempfolder))
                {
                    var securityRules = new DirectorySecurity();
                    securityRules.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.Read,
                                                                         AccessControlType.Allow));
                    securityRules.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl,
                                                                         AccessControlType.Allow));

                    Directory.CreateDirectory(tempfolder, securityRules);
                }
                File.WriteAllBytes(fileAddress, fileByteArray);
            }

            workitem.Open();
            workitem.Attachments.Add(new Attachment(fileAddress, string.Format("Added By {0}", createdBy)));
            workitem.Save();
            workitem.Close();

            if (!string.IsNullOrEmpty(fileComment))
                AddHistoryComment(wiId, string.Format(@"توضیحات فایل <b>{0}: </b><br \>", fileName) + fileComment,
                                  createdBy, createdOn, collectionName);


            File.Delete(fileAddress);
        }

        public void DestoryWi(int wiId, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            _helper.Store.DestroyWorkItems(new[] { wiId });
        }

        public string GetUserDisplayName(string userDomainName, string collectionName)
        {
            TeamFoundationIdentity teamFoundationIdentity = null;
            try
            {
                if (_helper == null)
                    _helper = new TFSHelper(collectionName);

                teamFoundationIdentity = _helper.GetTFSIdentity(userDomainName, IdentitySearchFactor.AccountName);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return teamFoundationIdentity == null
                       ? GetUserDisplayName("Tosanltd\\tfs", collectionName)
                       : teamFoundationIdentity.DisplayName;
        }

        public List<int> GetWorkItemsByFieldMatching(List<TFSField> fieldsValue, string collectionName)
        {
            _helper = new TFSHelper(collectionName);
            var query = new StringBuilder();

            query.Append(@"Select * From WorkItems Where ");

            for (int i = 0; i < fieldsValue.Count; i++)
            {
                query.Append(string.Format("[{0}] = '{1}'", fieldsValue.ElementAt(i).Id, fieldsValue.ElementAt(i).Value));
                if (i + 1 != fieldsValue.Count)
                    query.Append(" AND ");
            }
            query.Append(" ORDER BY [System.Id]");
            var s = query.ToString();
            var workItemsByQuery = _helper.GetWorkItemsByQuery(s);
            return workItemsByQuery.Count != 0
                       ? workItemsByQuery.OfType<WorkItem>().Select(item => item.Id).ToList()
                       : new List<int>();
        }

        public void ImportGlobalList(string globalListName, string value, string collectionName)
        {
            _helper = new TFSHelper(collectionName);
            var globalXml = _helper.Store.ExportGlobalLists();

            var xPath = string.Format("*/GLOBALLIST[@name='{0}']", globalListName);

            var xmlNodeList = globalXml.SelectNodes(xPath);

            var element = globalXml.CreateElement("LISTITEM");
            element.SetAttribute("value", value);
            xmlNodeList[0].AppendChild(element);

            _helper.Store.ImportGlobalLists(globalXml.InnerXml);

        }

        public bool IsUserInProject(string userDomainName, string projectName, string collectionName)
        {
            _helper = new TFSHelper(collectionName);
            var users = _helper.GetProjectValidUsers(projectName);
            return users.Contains(userDomainName.ToLower());
        }

        public int[] GetWorkItemByActivityId(string activityId, string collectionName)
        {
            _helper = new TFSHelper(collectionName);

            var wis = _helper.GetWorkItemsByQuery(WorkItemQeury.GetWorkItemByActivityIdQuery(activityId)).OfType<WorkItem>().ToList();

            return wis.Any() ? wis.Select(w => w.Id).ToArray() : new int[0];
        }
        #endregion

        #region Version Control Service Methods

        public List<string> GetGlobalListsValues(string listName, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            return _helper.GetGlobalListValues(listName).ToList();
        }

        public List<string> GetBranchLabels(string dirName, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var versionService = _helper.GetCheckInService();
            var labels = versionService.QueryLabels(null, null, null, false, dirName, VersionSpec.Latest);

            return labels.OrderBy(label => label.LastModifiedDate).Select(label => label.Name).ToList();
        }

        public List<int> GetBranchChangSets(string dirName, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var versionService = _helper.GetCheckInService();

            var changeSets = versionService.QueryHistory(dirName, RecursionType.Full);

            return changeSets.Select(changeset => changeset.ChangesetId).ToList();
        }

        public List<TFSItem> GetChangeSetsWorkItems(int changeSetId, string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var versionService = _helper.GetCheckInService();

            var changeSet = versionService.GetChangeset(changeSetId);

            return changeSet.WorkItems.Select(item => item.ToTfsItem()).ToList();
        }

        public List<TFSItem> GetLabelScopedChangeSets(string dirName, string baseLabel, string targetLabel,
                                                              string collectionName)
        {
            if (_helper == null)
                _helper = new TFSHelper(collectionName);

            var versionService = _helper.GetCheckInService();

            var sourceSpec = new LabelVersionSpec(baseLabel, dirName);
            var targetSpec = new LabelVersionSpec(targetLabel, dirName);

            var changeSets =
                versionService.QueryHistory(dirName, targetSpec, 0, RecursionType.Full, string.Empty, sourceSpec,
                                            targetSpec, int.MaxValue, true, false, false, true)
                              .OfType<Changeset>()
                              .ToList();

            return changeSets.Select(changeset => changeset.ToTfsItem()).ToList();

        }

        #endregion

        #region Build Service Methods

        public bool BuildNotify(string label, string dir, string reportDir, string collectionName)
        {

            try
            {
                if (dir != null && label != null && collectionName != null)
                {
                    var labelList = GetBranchLabels(dir, collectionName);
                    var labelIndex = labelList.FindIndex(s => s == label);
                    if (labelIndex > 0)
                    {
                        var fromLabel = labelList[labelIndex - 1];
                        var changesets = GetLabelScopedChangeSets(dir, fromLabel, label, collectionName);
                        TFSBuildNotification.UpdateWorkItemsandParentBuildNumbers(label, changesets);
                        TFSBuildNotification.UpdateBuildTabl(label, dir, changesets);
                    }

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }

        #endregion

        #region Private Methods

        private void BypassRulesFieldConfig(WorkItem wi, string userDisplayName)
        {
            var state = wi["System.State"].ToString();

            var states = StateMappingFields.Where(pair => pair.Key != state);

            foreach (var mappingField in states)
            {
                wi[mappingField.Value + " Date"] = null;
                wi[mappingField.Value + " By"] = null;
            }

            if (StateMappingFields.ContainsKey(state))
            {
                wi[StateMappingFields[state] + " Date"] = DateTime.Now;
                wi[StateMappingFields[state] + " By"] = userDisplayName;
            }
            wi.Save();
        }
        private static readonly Dictionary<string, string> StateMappingFields = new Dictionary<string, string>()
            {
                {"Tested","Resolved"},
                {"Active","Activated"},
                {"Closed","Closed"},
                {"Resolved","Resolved"},
            };


        #endregion

    }
}
