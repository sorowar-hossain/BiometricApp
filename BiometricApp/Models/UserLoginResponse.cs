using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Models
{
    public class UserLoginResponse
    {
        public int UserId { get; set; }
        public int OrgId { get; set; }
        public string OrgCode { get; set; }
        public string UserName { get; set; }
        public bool Success { get; set; }
        public string Message { get; internal set; }
        public string OrganizationName { get; internal set; }
        public string OrganizationCode { get; internal set; }

        public string PersonUniqueId { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
