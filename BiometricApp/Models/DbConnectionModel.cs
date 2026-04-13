using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Models
{
    public class DbConnectionModel
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public int DatabaseType { get; set; } 
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
