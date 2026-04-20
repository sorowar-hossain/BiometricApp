using BiometricApp.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class PostgreSqlDatabaseService
    {
        private bool IsInternetAvailable()
        {
            var access = Connectivity.Current.NetworkAccess;
            return access == NetworkAccess.Internet;
        }

        public async Task<(bool Success, string Message)> TestConnection(DbConnectionModel dbModel)
        {
            try
            {
                if (!IsInternetAvailable())
                    return (false, "No internet connection ❌");

                if (string.IsNullOrWhiteSpace(dbModel.ServerName))
                    return (false, "Host cannot be empty");

                string connStr =
                    $"Host={dbModel.ServerName};" +
                    $"Port=5432;" +
                    $"Database={dbModel.DatabaseName};" +
                    $"Username={dbModel.Username};" +
                    $"Password={dbModel.Password};" +
                    $"Timeout=5;";

                using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

                return (true, "PostgreSQL connection successful ✅");
            }
            catch (NpgsqlException ex)
            {
                return (false, $"PostgreSQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
