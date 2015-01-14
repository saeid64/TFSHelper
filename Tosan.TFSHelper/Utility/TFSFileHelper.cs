using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Tosan.TFSHelper.Model;

namespace Tosan.TFSHelper.Utility
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
