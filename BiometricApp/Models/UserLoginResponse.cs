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
    }
}
