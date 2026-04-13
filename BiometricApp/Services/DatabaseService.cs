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
                if (string.IsNullOrWhiteSpace(dbModel.ServerName))
                {
                    return (false, "Server cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(dbModel.DatabaseName))
                {
                    return (false, "Database cannot be empty");
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
        public async Task<(bool Success, string Message)> EnsureDatabaseExists(DbConnectionModel dbModel)
        {
            try
            {
                if (!IsInternetAvailable())
                    return (false, "No network connection ❌");

                // Step 1: Connect to master DB
                string masterConnStr =
                    $"Server={dbModel.ServerName};Database=master;" +
                    $"User Id={dbModel.Username};Password={dbModel.Password};" +
                    $"TrustServerCertificate=True;Connection Timeout=5;";

                using (SqlConnection conn = new SqlConnection(masterConnStr))
                {
                    await conn.OpenAsync();

                    // Step 2: Check if database exists
                    string checkDbQuery =
                        "SELECT COUNT(*) FROM sys.databases WHERE name = @db";

                    using (SqlCommand cmd = new SqlCommand(checkDbQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@db", dbModel.DatabaseName);

                        int exists = (int)await cmd.ExecuteScalarAsync();

                        if (exists > 0)
                        {
                            return (true, $"Database '{dbModel.DatabaseName}' already exists ✅");
                        }
                    }

                    // Step 3: Create database if not exists
                    string createDbQuery = $"CREATE DATABASE [{dbModel.DatabaseName}]";

                    using (SqlCommand cmd = new SqlCommand(createDbQuery, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return (true, $"Database '{dbModel.DatabaseName}' created successfully 🏗️");
                }
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

        public async Task<(bool Success, string Message)> TestLocalConnection(DbConnectionModel dbModel)
        {
            try
            {
                // Step 1: Check Internet (optional for local, you can remove if not needed)
                if (!IsInternetAvailable())
                {
                    return (false, "No network connection ❌");
                }

                // Step 2: Validate inputs
                if (string.IsNullOrWhiteSpace(dbModel.ServerName))
                {
                    return (false, "Server cannot be empty");
                }

                // Step 3: Windows Authentication connection string
                string connStr =
                    $"Server={dbModel.ServerName};" +
                    $"Database={dbModel.DatabaseName};" +
                    $"Trusted_Connection=True;" +
                    $"TrustServerCertificate=True;" +
                    $"Connection Timeout=5;";

                // Step 4: Try connecting
                using SqlConnection conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                return (true, "Local connection successful (Windows Auth) ✅");
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

        public async Task<(bool Success, string Message)> EnsureDatabaseExistsLocal(DbConnectionModel dbModel)
        {
            try
            {
                // Step 1: Check network (optional for local, you can remove if needed)
                if (!IsInternetAvailable())
                    return (false, "No network connection ❌");

                // Step 2: Build connection string (Windows Authentication)
                string masterConnStr =
                    $"Server={dbModel.ServerName};Database=master;" +
                    $"Trusted_Connection=True;" +
                    $"TrustServerCertificate=True;" +
                    $"Connection Timeout=5;";

                using (SqlConnection conn = new SqlConnection(masterConnStr))
                {
                    await conn.OpenAsync();

                    // Step 3: Check if database exists
                    string checkDbQuery =
                        "SELECT COUNT(*) FROM sys.databases WHERE name = @db";

                    using (SqlCommand cmd = new SqlCommand(checkDbQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@db", dbModel.DatabaseName);

                        int exists = (int)await cmd.ExecuteScalarAsync();

                        if (exists > 0)
                        {
                            return (true, $"Database '{dbModel.DatabaseName}' already exists ✅ (Local)");
                        }
                    }

                    // Step 4: Create database if not exists
                    string createDbQuery =
                        $"CREATE DATABASE [{dbModel.DatabaseName}]";

                    using (SqlCommand cmd = new SqlCommand(createDbQuery, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return (true, $"Database '{dbModel.DatabaseName}' created successfully 🏗️ (Local)");
                }
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

