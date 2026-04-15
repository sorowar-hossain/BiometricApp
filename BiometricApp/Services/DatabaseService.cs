using BiometricApp.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public async Task<(bool Success, string Message)> TestConnectionLocal(DbConnectionModel dbModel)
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

        public async Task<(bool Success, string Message)> SetupDatabase(DbConnectionModel dbModel)
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
                            var res = await CreateTables(dbModel);
                            return res;
                        }
                    }

                    // Step 3: Create database if not exists
                    string createDbQuery = $"CREATE DATABASE [{dbModel.DatabaseName}]";

                    using (SqlCommand cmd = new SqlCommand(createDbQuery, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    var result = await CreateTables(dbModel);
                    return result;
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

        public async Task<(bool Success, string Message)> SetupDatabaseLocal(DbConnectionModel dbModel)
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
                            //return (true, $"Database '{dbModel.DatabaseName}' already exists ✅ (Local)");
                            var result = await CreateTablesLocal(dbModel);
                            return result;
                        }
                    }

                    // Step 4: Create database if not exists
                    string createDbQuery =
                        $"CREATE DATABASE [{dbModel.DatabaseName}]";

                    using (SqlCommand cmd = new SqlCommand(createDbQuery, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    //return (true, $"Database '{dbModel.DatabaseName}' created successfully 🏗️ (Local)");
                    var res = await CreateTablesLocal(dbModel);
                    return res;
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

        public async Task<(bool Success, string Message)> CreateTables(DbConnectionModel dbModel)
        {
            try
            {
                string connStr =
                    $"Server={dbModel.ServerName};Database={dbModel.DatabaseName};" +
                    $"User Id={dbModel.Username};Password={dbModel.Password};" +
                    $"TrustServerCertificate=True;Connection Timeout=5;";

                using var stream = await FileSystem.OpenAppPackageFileAsync("Scripts/CreateTables.sql");

                using var reader = new StreamReader(stream);
                string script = await reader.ReadToEndAsync();

                using SqlConnection conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                using SqlCommand cmd = new SqlCommand(script, conn);
                await cmd.ExecuteNonQueryAsync();

                return (true, "Database setup successfully ✅");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> CreateTablesLocal(DbConnectionModel dbModel)
        {
            try
            {
                string connStr =
                            $"Server={dbModel.ServerName};" +
                            $"Database={dbModel.DatabaseName};" +
                            $"Trusted_Connection=True;" +
                            $"TrustServerCertificate=True;" +
                            $"Connection Timeout=5;";

                using var stream = await FileSystem.OpenAppPackageFileAsync("Scripts/CreateTables.sql");

                using var reader = new StreamReader(stream);
                string script = await reader.ReadToEndAsync();

                using SqlConnection conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                using SqlCommand cmd = new SqlCommand(script, conn);
                await cmd.ExecuteNonQueryAsync();

                return (true, "Database setup successfully ✅");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }


        public async Task<(bool Success, string Message)> ImportAllMembers(DbConnectionModel dbModel, UserLoginResponse user)
        {
            try
            {
                string rootPath = AppSettings.BaseFolder;

                string connStr =
                    $"Server={dbModel.ServerName};" +
                    $"Database={dbModel.DatabaseName};" +
                    $"Trusted_Connection=True;" +
                    $"TrustServerCertificate=True;" +
                    $"Connection Timeout=30;";

                var folders = Directory.GetDirectories(rootPath);

                using SqlConnection conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                foreach (var folder in folders)
                {
                    string demoPath = Path.Combine(folder, "demographics.json");

                    if (!File.Exists(demoPath))
                        continue;

                    var demo = JsonSerializer.Deserialize<MemberModel>(
                        await File.ReadAllTextAsync(demoPath)
                    );

                    if (demo == null || string.IsNullOrWhiteSpace(demo.PersonUniqueId))
                        continue;

                    // =========================
                    // CHECK EXISTING
                    // =========================
                    string checkQuery = @"
                                        SELECT PersonId 
                                        FROM Demographics 
                                        WHERE PersonUniqueId = @PersonUniqueId";

                    using SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@PersonUniqueId", demo.PersonUniqueId);

                    object result = await checkCmd.ExecuteScalarAsync();

                    int personId;

                    // =========================
                    // INSERT OR UPDATE
                    // =========================
                    if (result == null)
                    {
                        string insertQuery = @"
                        INSERT INTO Demographics
                        (
                            UserId, OrgId,
                            FirstName, LastName,
                            MaritalStatus,
                            PlaceOfIssue, PlaceOfBirth,
                            DateOfBirth, Gender,
                            Address, Weight,
                            FatherName, MotherName,
                            ExpiryDate,
                            PersonUniqueId,
                            CreatedOn, CreatedBy
                        )
                        OUTPUT INSERTED.PersonId
                        VALUES
                        (
                            @UserId, @OrgId,
                            @FirstName, @LastName,
                            @MaritalStatus,
                            @PlaceOfIssue, @PlaceOfBirth,
                            @DateOfBirth, @Gender,
                            @Address, @Weight,
                            @FatherName, @MotherName,
                            @ExpiryDate,
                            @PersonUniqueId,
                            @CreatedOn, @CreatedBy
                        )";

                        using SqlCommand insertCmd = new SqlCommand(insertQuery, conn);

                        AddParams(insertCmd, demo, user.UserName,"");

                        personId = (int)await insertCmd.ExecuteScalarAsync();
                    }
                    else
                    {
                        personId = Convert.ToInt32(result);

                        string updateQuery = @"
                        UPDATE Demographics
                        SET
                            UserId = @UserId,
                            OrgId = @OrgId,
                            FirstName = @FirstName,
                            LastName = @LastName,
                            MaritalStatus = @MaritalStatus,
                            PlaceOfIssue = @PlaceOfIssue,
                            PlaceOfBirth = @PlaceOfBirth,
                            DateOfBirth = @DateOfBirth,
                            Gender = @Gender,
                            Address = @Address,
                            Weight = @Weight,
                            FatherName = @FatherName,
                            MotherName = @MotherName,
                            ExpiryDate = @ExpiryDate,
                            UpdatedOn = GETDATE(),
                            UpdatedBy = @UpdatedBy
                        WHERE PersonId = @PersonId";

                        using SqlCommand updateCmd = new SqlCommand(updateQuery, conn);

                        AddParams(updateCmd, demo, "",user.UserName);
                        updateCmd.Parameters.AddWithValue("@PersonId", personId);

                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    // =========================
                    // BIOMETRICS
                    // =========================
                    await SaveBiometrics(conn, folder, personId, user);
                }

                return (true, "All Members Saved Successfully ✅");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void AddParams(SqlCommand cmd, MemberModel demo, string createdBy, string updatedBy)
        {
            cmd.Parameters.AddWithValue("@UserId", demo.UserId);
            cmd.Parameters.AddWithValue("@OrgId", demo.OrgId);

            cmd.Parameters.AddWithValue("@FirstName", demo.FirstName);
            cmd.Parameters.AddWithValue("@LastName", demo.LastName);
            cmd.Parameters.AddWithValue("@MaritalStatus", demo.MaritalStatus);

            cmd.Parameters.AddWithValue("@PlaceOfIssue", (object?)demo.PlaceOfIssue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PlaceOfBirth", (object?)demo.PlaceOfBirth ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@DateOfBirth", (object?)demo.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", demo.Gender);

            cmd.Parameters.AddWithValue("@Address", demo.Address);

            cmd.Parameters.AddWithValue("@Weight", (object?)demo.Weight ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@FatherName", demo.FatherName);
            cmd.Parameters.AddWithValue("@MotherName", demo.MotherName);

            cmd.Parameters.AddWithValue("@ExpiryDate", (object?)demo.ExpiryDate ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@PersonUniqueId", demo.PersonUniqueId);

            cmd.Parameters.AddWithValue("@CreatedOn", demo.CreatedOn);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
        }

        public async Task SaveBiometrics(SqlConnection conn, string folder, int personId, UserLoginResponse user)
        {
            string query = @"
                    IF EXISTS (SELECT 1 FROM Biometrics WHERE PersonId=@PersonId)
                    BEGIN
                        UPDATE Biometrics SET
                            LeftThumb=@LeftThumb,
                            LeftThumb_FileName=@LeftThumb_FileName,

                            LeftIndex=@LeftIndex,
                            LeftIndex_FileName=@LeftIndex_FileName,

                            LeftMiddle=@LeftMiddle,
                            LeftMiddle_FileName=@LeftMiddle_FileName,

                            LeftRing=@LeftRing,
                            LeftRing_FileName=@LeftRing_FileName,

                            LeftLittle=@LeftLittle,
                            LeftLittle_FileName=@LeftLittle_FileName,

                            RightThumb=@RightThumb,
                            RightThumb_FileName=@RightThumb_FileName,

                            RightIndex=@RightIndex,
                            RightIndex_FileName=@RightIndex_FileName,

                            RightMiddle=@RightMiddle,
                            RightMiddle_FileName=@RightMiddle_FileName,

                            RightRing=@RightRing,
                            RightRing_FileName=@RightRing_FileName,

                            RightLittle=@RightLittle,
                            RightLittle_FileName=@RightLittle_FileName,

                            LeftIris=@LeftIris,
                            LeftIris_FileName=@LeftIris_FileName,

                            RightIris=@RightIris,
                            RightIris_FileName=@RightIris_FileName,

                            Face=@Face,
                            Face_FileName=@Face_FileName,

                            UpdatedBy=@UpdatedBy,
                            UpdatedOn=GETDATE()

                        WHERE PersonId=@PersonId
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Biometrics
                        (
                            PersonId,

                            LeftThumb, LeftThumb_FileName,
                            LeftIndex, LeftIndex_FileName,
                            LeftMiddle, LeftMiddle_FileName,
                            LeftRing, LeftRing_FileName,
                            LeftLittle, LeftLittle_FileName,

                            RightThumb, RightThumb_FileName,
                            RightIndex, RightIndex_FileName,
                            RightMiddle, RightMiddle_FileName,
                            RightRing, RightRing_FileName,
                            RightLittle, RightLittle_FileName,

                            LeftIris, LeftIris_FileName,
                            RightIris, RightIris_FileName,

                            Face, Face_FileName,

                            CreatedBy,
                            CreatedOn
                          
                        )
                        VALUES
                        (
                            @PersonId,

                            @LeftThumb, @LeftThumb_FileName,
                            @LeftIndex, @LeftIndex_FileName,
                            @LeftMiddle, @LeftMiddle_FileName,
                            @LeftRing, @LeftRing_FileName,
                            @LeftLittle, @LeftLittle_FileName,

                            @RightThumb, @RightThumb_FileName,
                            @RightIndex, @RightIndex_FileName,
                            @RightMiddle, @RightMiddle_FileName,
                            @RightRing, @RightRing_FileName,
                            @RightLittle, @RightLittle_FileName,

                            @LeftIris, @LeftIris_FileName,
                            @RightIris, @RightIris_FileName,

                            @Face, @Face_FileName,

                            @CreatedBy,
                            GETDATE()
                        )
                    END";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PersonId", personId);


            // Helper function
            byte[] GetFileBytes(string fileName, string folder)
            {
                string path = Path.Combine(folder, fileName);

                if (!File.Exists(path))
                    return null;

                return File.ReadAllBytes(path);
            }


            // LEFT HAND
            cmd.Parameters.Add("@LeftThumb", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("l_Left_Thumb.bmp", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@LeftIndex", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("l_Left_Index.bmp", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@LeftMiddle", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("l_Left_Middle.bmp", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@LeftRing", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("l_Left_Ring.bmp", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@LeftLittle", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("l_Left_Little.bmp", folder) ?? DBNull.Value;


            // RIGHT HAND

            cmd.Parameters.Add("@RightThumb", SqlDbType.VarBinary).Value =
                   (object?)GetFileBytes("r_Right_Thumb.bmp", folder) ?? DBNull.Value;


            cmd.Parameters.Add("@RightIndex", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("r_Right_Index.bmp", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@RightMiddle", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("r_Right_Middle.bmp", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@RightRing", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("r_Right_Ring.bmp", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@RightLittle", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("r_Right_Little.bmp", folder) ?? DBNull.Value;


            // IRIS

            cmd.Parameters.Add("@LeftIris", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("left_iris.png", folder) ?? DBNull.Value;

            cmd.Parameters.Add("@RightIris", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("right_iris.png", folder) ?? DBNull.Value;

            // FACE
            cmd.Parameters.Add("@Face", SqlDbType.VarBinary).Value =
                (object?)GetFileBytes("face.png", folder) ?? DBNull.Value;

            // LEFT HAND FileNames
            cmd.Parameters.Add("@LeftThumb_FileName", SqlDbType.NVarChar, 200).Value = "l_Left_Thumb.bmp";
            cmd.Parameters.Add("@LeftIndex_FileName", SqlDbType.NVarChar, 200).Value = "l_Left_Index.bmp";
            cmd.Parameters.Add("@LeftMiddle_FileName", SqlDbType.NVarChar, 200).Value = "l_Left_Middle.bmp";
            cmd.Parameters.Add("@LeftRing_FileName", SqlDbType.NVarChar, 200).Value = "l_Left_Ring.bmp";
            cmd.Parameters.Add("@LeftLittle_FileName", SqlDbType.NVarChar, 200).Value = "l_Left_Little.bmp";

            // RIGHT HAND FileNames
            cmd.Parameters.Add("@RightThumb_FileName", SqlDbType.NVarChar, 200).Value = "r_Right_Thumb.bmp";
            cmd.Parameters.Add("@RightIndex_FileName", SqlDbType.NVarChar, 200).Value = "r_Right_Index.bmp";
            cmd.Parameters.Add("@RightMiddle_FileName", SqlDbType.NVarChar, 200).Value = "r_Right_Middle.bmp";
            cmd.Parameters.Add("@RightRing_FileName", SqlDbType.NVarChar, 200).Value = "r_Right_Ring.bmp";
            cmd.Parameters.Add("@RightLittle_FileName", SqlDbType.NVarChar, 200).Value = "r_Right_Little.bmp";

            // IRIS FileNames
            cmd.Parameters.Add("@LeftIris_FileName", SqlDbType.NVarChar, 200).Value = "left_iris.png";
            cmd.Parameters.Add("@RightIris_FileName", SqlDbType.NVarChar, 200).Value = "right_iris.png";

            // FACE FileName
            cmd.Parameters.Add("@Face_FileName", SqlDbType.NVarChar, 200).Value = "face.png";

        

            cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = user.UserName;
            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 100).Value = user.UserName;

            await cmd.ExecuteNonQueryAsync();
        }
    }

}




