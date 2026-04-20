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



        public async Task<(bool Success, string Message)> SetupPostgresDatabase(DbConnectionModel dbModel)
        {
            try
            {
                if (!IsInternetAvailable())
                    return (false, "No network connection ❌");

                
                // 1. Connect to default DB
             
                var masterConnStr =
                    $"Host={dbModel.ServerName};Port=5432;" +
                    $"Username={dbModel.Username};Password={dbModel.Password};" +
                    $"Database=postgres";

                using (var masterConn = new NpgsqlConnection(masterConnStr))
                {
                    await masterConn.OpenAsync();

             
                    // 2. Check if DB exists
                   
                    var checkCmd = new NpgsqlCommand(
                        "SELECT 1 FROM pg_database WHERE datname = @db",
                        masterConn);

                    checkCmd.Parameters.AddWithValue("@db", dbModel.DatabaseName);

                    var exists = await checkCmd.ExecuteScalarAsync();

                   
                    // 3. Create DB if not exists
                 
                    if (exists == null)
                    {
                        var createCmd = new NpgsqlCommand(
                            $"CREATE DATABASE \"{dbModel.DatabaseName}\"",
                            masterConn);

                        await createCmd.ExecuteNonQueryAsync();
                    }
                }

               
                // 4. Connect to NEW database
               
                var connStr =
                    $"Host={dbModel.ServerName};Port=5432;" +
                    $"Username={dbModel.Username};Password={dbModel.Password};" +
                    $"Database={dbModel.DatabaseName}";

                using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

            
                // 5. Load SQL script
               
                using var stream = await FileSystem.OpenAppPackageFileAsync("scripts/CreateTablesPg.txt");
                using var reader = new StreamReader(stream);
                string script = await reader.ReadToEndAsync();

            
                // 6. Execute script safely
               
                using var cmd = new NpgsqlCommand(script, conn);
                await cmd.ExecuteNonQueryAsync();

                return (true, "PostgreSQL setup completed ✅");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
