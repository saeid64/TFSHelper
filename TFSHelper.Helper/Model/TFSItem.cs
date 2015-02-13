using System;
using System.Collections.Generic;
using System.Linq;

namespace TFSHelper.Helper.Model
{
    public class TFSItem
    {
        public TFSItem()
        {
            Fields = new TFSFieldList();
            TFSLinks = new List<TFSLinkField>();
            TFSFiles = new List<TFSFile>();
        }
        public TFSFieldList Fields { get; set; }
        public string Title { get { return GetFieldValue("System.Title"); } set { Add("System.Title", value); } }
        public int Id { get { return GetFieldValue("System.Id")!= null ? int.Parse(GetFieldValue("System.Id")) : 0; } set { Add("System.Id", value); } }
        public string Area { get { return GetFieldValue("System.AreaPath"); } set { Add("System.AreaPath", value); } }
        public string Type { get { return GetFieldValue("System.WorkItemType"); } set { Add("System.WorkItemType", value); } }
        public int Revsion
        {
            get
            {
                var rev = GetFieldValue("System.Rev");
                return rev != null && int.Parse(rev) != 0
                    ? int.Parse(rev)
                    : 0;
            }
            set { Add("System.Rev", value); }
        }
        public string Changer { get; set; }
        public List<TFSLinkField> TFSLinks { get; set; }
        public List<TFSFile> TFSFiles { get; set; }
        public string ProjectName { get; set; }
        public string TpcName { get; set; }
        public PluginType ItemEventType { get; set; }

        public string GetFieldValue(string id)
        {
            return !Contains(id) ? null : this[id].NewValue.ToString();
        }
        public bool Add(string id, object value, bool isDirty = false, object oldValue = null, TFSType type = TFSType.String)
        {
            if (Contains(id)) return false;
            Fields.Add(new TFSField { Id = id, NewValue = value, IsDirty = isDirty, OldValue = oldValue, Type = type });
            return true;
        }
        public bool Contains(string fieldId)
        {
            return Fields[fieldId] != null && Fields[fieldId].NewValue != null && !string.IsNullOrEmpty(Fields[fieldId].NewValue.ToString());
        }
        /// <summary>
        /// GetFieldValue Fields Value by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TFSField this[string id]
        {
            get
            {
                return Fields.SingleOrDefault(field => field.Id == id);
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this[id] = value;
            }
        }
    }
    public class TFSFieldList : List<TFSField>
    {
        public TFSField this[string id]
        {
            get
            {
                return this.SingleOrDefault(field => field.Id == id);
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this[id] = value;
            }
        }
    }

    public static class TFSFieldListExtension
    {
        public static bool Contains(this TFSFieldList tfsFieldList, string tfsItemId)
        {
            return (tfsFieldList[tfsItemId] != null && tfsFieldList[tfsItemId].NewValue != null);
        }
    }
    public class TFSField
    {
        public string Id { get; set; }
        public object NewValue { get; set; }
        public object OldValue { get; set; }
        public TFSType Type { get; set; }
        public bool IsDirty { get; set; }

        public bool IsNullorEmpty()
        {
            return NewValue == null || string.IsNullOrEmpty(NewValue.ToString());
        }
    }

    public enum TFSType
    {
        Integer = 1,
        String = 2
    }
    public class TFSLinkField
    {
        public int Id { get; set; }
        public string LinkName { get; set; }
        public string LinkImmutableName { get; set; }
        public Type LinkType { get; set; }
        public AtTimeState State { get; set; }

        public enum Type
        {
            RelatedLink = 1,
            ExternalLink = 2
        }
    }
    public class TFSFile
    {
        public byte[] Body { get; set; }
        public string Name { get; set; }
        public string MIME { get; set; }
        public string Comment { get; set; }
        public DateTime CreationTime { get; set; }
        public string Creator { get; set; }
        public AtTimeState State { get; set; }
        public int FileId { get; set; }
    }
    public enum PluginType
    {
        WorkItemChangedEvent = 0,
        CheckinNotification = 1
    }
    public enum AtTimeState
    {
        Added = 1,
        Deleted = 2
    }
}
