using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tosan.TFSHelper.Model
{
    public class AlertRecipient
    {
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string DomainName { get; set; }
        public Guid TeamFoundationId { get; set; }
    }
}
