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
    }
}
