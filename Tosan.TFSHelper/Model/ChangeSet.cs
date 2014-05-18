using System.Collections.Generic;

namespace Tosan.TFSHelper.Model
{
    class ChangeSet : TFSItem
    {
        public List<string> SourcesPath { get; set; }
        public string WorkSpace { get; set; }
        public string WorkSpaceOwner { get; set; }
        public string ChangeSetOwner { get; set; }
        public string ChangeSetOwnerDisplayName { get; set; }
        public bool IsPolicyOverrided { get; set; }
    }
}
