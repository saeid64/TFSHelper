using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSHelper.Helper.Model;

namespace TFSHelper.Helper.Utility
{
    public class TFSFileHelper
    {
        public static TFSFile GeTFSFile(Attachment attachment)
        {
            var client = new WebClient();
            client.UseDefaultCredentials = true;
            return new TFSFile
            {
                Body = client.DownloadData(attachment.Uri),
                Name = attachment.Name,
                Comment = attachment.Comment,
                CreationTime = attachment.CreationTime,
                FileId = attachment.Id
            };
        }
       
    }
}
