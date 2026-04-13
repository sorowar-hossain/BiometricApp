using BiometricApp.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class DatabaseService
    {
        // ✅ Check Internet Connection
        private bool IsInternetAvailable()
        {
            var access = Connectivity.Current.NetworkAccess;
            return access == NetworkAccess.Internet;
        }

        // ✅ Test SQL Connection (with network check first)
        public async Task<(bool Success, string Message)> TestConnection(DbConnectionModel dbModel)
        {
            try
            {
                // Step 1: Check Internet
                if (!IsInternetAvailable())
                {
                    return (false, "No internet connection ❌");
                }

                // Step 2: Validate inputs (optional but useful)
                if (string.IsNullOrWhiteSpace(dbModel.ServerName) || string.IsNullOrWhiteSpace(dbModel.ServerName))
                {
                    return (false, "Server or Database cannot be empty");
                }

                // Step 3: Build connection string
                string connStr =
                    $"Server={dbModel.ServerName};Database={dbModel.DatabaseName};User Id={dbModel.Username};Password={dbModel.Password};TrustServerCertificate=True;Connection Timeout=5;";

                // Step 4: Try connecting
                using SqlConnection conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                return (true, "Connection successful ✅");
            }
            catch (SqlException ex)
            {
                return (false, $"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
