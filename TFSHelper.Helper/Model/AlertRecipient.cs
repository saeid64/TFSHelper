using System;

namespace TFSHelper.Helper.Model
{
    public class AlertRecipient
    {
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string DomainName { get; set; }
        public Guid TeamFoundationId { get; set; }
    }
}
