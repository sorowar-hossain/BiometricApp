using BiometricApp.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

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


        public async Task<(bool Success, string Message)> ImportAllMembers(DbConnectionModel dbModel, UserLoginResponse user)
        {
            try
            {
                string rootPath = AppSettings.BaseFolder;

                string connStr =
                    $"Host={dbModel.ServerName};Database={dbModel.DatabaseName};Username={dbModel.Username};Password={dbModel.Password};Port=5432;";

                var folders = Directory.GetDirectories(rootPath);

                using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction(); // 🔥 START TRANSACTION

                try
                {
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

                        // ================= CHECK =================
                        string checkQuery = @"
                                            SELECT person_id 
                                            FROM demographics 
                                            WHERE person_unique_id = @id";

                        using var checkCmd = new NpgsqlCommand(checkQuery, conn, transaction);
                        checkCmd.Parameters.AddWithValue("@id", demo.PersonUniqueId);

                        var result = await checkCmd.ExecuteScalarAsync();

                        int personId;

                        // ================= INSERT / UPDATE =================
                        if (result == null)
                        {
                            string insertQuery = @"
                                            INSERT INTO demographics
                                            (
                                                user_id, org_id,
                                                first_name, last_name,
                                                marital_status,
                                                place_of_issue, place_of_birth,
                                                date_of_birth, gender,
                                                address, weight,
                                                father_name, mother_name,
                                                expiry_date,
                                                person_unique_id,
                                                created_on, created_by
                                            )
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
                                            )
                                            RETURNING person_id;
                                        ";

                            using var insertCmd = new NpgsqlCommand(insertQuery, conn, transaction);
                            AddParamsPg(insertCmd, demo, user.UserName, "");

                            personId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
                        }
                        else
                        {
                            personId = Convert.ToInt32(result);

                            string updateQuery = @"
                                            UPDATE demographics SET
                                                user_id = @UserId,
                                                org_id = @OrgId,
                                                first_name = @FirstName,
                                                last_name = @LastName,
                                                marital_status = @MaritalStatus,
                                                place_of_issue = @PlaceOfIssue,
                                                place_of_birth = @PlaceOfBirth,
                                                date_of_birth = @DateOfBirth,
                                                gender = @Gender,
                                                address = @Address,
                                                weight = @Weight,
                                                father_name = @FatherName,
                                                mother_name = @MotherName,
                                                expiry_date = @ExpiryDate,
                                                updated_on = NOW(),
                                                updated_by = @UpdatedBy
                                            WHERE person_id = @PersonId;
                                        ";

                            using var updateCmd = new NpgsqlCommand(updateQuery, conn, transaction);
                            AddParamsPg(updateCmd, demo, "", user.UserName);

                            updateCmd.Parameters.AddWithValue("@PersonId", personId);

                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        // ================= BIOMETRICS =================
                        await SaveBiometricsPg(conn, transaction, folder, personId, user);
                    }

                    transaction.Commit(); // 🔥 SUCCESS → SAVE ALL

                    return (true, "All Members Saved Successfully ✅");
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // 🔥 FAIL → UNDO EVERYTHING
                    return (false, ex.Message);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }



        public void AddParamsPg(NpgsqlCommand cmd, MemberModel demo, string createdBy, string updatedBy)
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

        public async Task SaveBiometricsPg(NpgsqlConnection conn, NpgsqlTransaction transaction, string folder, int personId, UserLoginResponse user)
        {
            // ================= CHECK EXISTENCE =================
            string checkQuery = "SELECT biometric_id FROM biometrics WHERE person_id=@PersonId";

            using var checkCmd = new NpgsqlCommand(checkQuery, conn, transaction);
            checkCmd.Parameters.AddWithValue("@PersonId", personId);

            var exists = await checkCmd.ExecuteScalarAsync();

            string query;

            // ================= UPDATE =================
            if (exists != null)
            {
                query = @"
                        UPDATE biometrics SET
                            left_thumb=@LeftThumb,
                            left_thumb_file_name=@LeftThumb_FileName,

                            left_index=@LeftIndex,
                            left_index_file_name=@LeftIndex_FileName,

                            left_middle=@LeftMiddle,
                            left_middle_file_name=@LeftMiddle_FileName,

                            left_ring=@LeftRing,
                            left_ring_file_name=@LeftRing_FileName,

                            left_little=@LeftLittle,
                            left_little_file_name=@LeftLittle_FileName,

                            right_thumb=@RightThumb,
                            right_thumb_file_name=@RightThumb_FileName,

                            right_index=@RightIndex,
                            right_index_file_name=@RightIndex_FileName,

                            right_middle=@RightMiddle,
                            right_middle_file_name=@RightMiddle_FileName,

                            right_ring=@RightRing,
                            right_ring_file_name=@RightRing_FileName,

                            right_little=@RightLittle,
                            right_little_file_name=@RightLittle_FileName,

                            left_iris=@LeftIris,
                            left_iris_file_name=@LeftIris_FileName,

                            right_iris=@RightIris,
                            right_iris_file_name=@RightIris_FileName,

                            face=@Face,
                            face_file_name=@Face_FileName,

                            updated_by=@UpdatedBy,
                            updated_on=NOW()

                        WHERE person_id=@PersonId";
            }
            // ================= INSERT =================
            else
            {
                query = @"
                        INSERT INTO biometrics
                        (
                            person_id,

                            left_thumb, left_thumb_file_name,
                            left_index, left_index_file_name,
                            left_middle, left_middle_file_name,
                            left_ring, left_ring_file_name,
                            left_little, left_little_file_name,

                            right_thumb, right_thumb_file_name,
                            right_index, right_index_file_name,
                            right_middle, right_middle_file_name,
                            right_ring, right_ring_file_name,
                            right_little, right_little_file_name,

                            left_iris, left_iris_file_name,
                            right_iris, right_iris_file_name,

                            face, face_file_name,

                            created_by,
                            created_on
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
                            NOW()
                        )";
            }

            using var cmd = new NpgsqlCommand(query, conn, transaction);

            cmd.Parameters.AddWithValue("@PersonId", personId);

            // ================= FILE HELPER =================
            byte[] GetFileBytes(string fileName)
            {
                string path = Path.Combine(folder, fileName);
                return File.Exists(path) ? File.ReadAllBytes(path) : null;
            }

            // ================= LEFT HAND =================
            cmd.Parameters.AddWithValue("@LeftThumb", (object?)GetFileBytes("l_Left_Thumb.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeftIndex", (object?)GetFileBytes("l_Left_Index.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeftMiddle", (object?)GetFileBytes("l_Left_Middle.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeftRing", (object?)GetFileBytes("l_Left_Ring.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeftLittle", (object?)GetFileBytes("l_Left_Little.bmp") ?? DBNull.Value);

            // ================= RIGHT HAND =================
            cmd.Parameters.AddWithValue("@RightThumb", (object?)GetFileBytes("r_Right_Thumb.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RightIndex", (object?)GetFileBytes("r_Right_Index.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RightMiddle", (object?)GetFileBytes("r_Right_Middle.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RightRing", (object?)GetFileBytes("r_Right_Ring.bmp") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RightLittle", (object?)GetFileBytes("r_Right_Little.bmp") ?? DBNull.Value);

            // ================= IRIS =================
            cmd.Parameters.AddWithValue("@LeftIris", (object?)GetFileBytes("left_iris.png") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RightIris", (object?)GetFileBytes("right_iris.png") ?? DBNull.Value);

            // ================= FACE =================
            cmd.Parameters.AddWithValue("@Face", (object?)GetFileBytes("face.png") ?? DBNull.Value);

            // ================= FILE NAMES =================
            cmd.Parameters.AddWithValue("@LeftThumb_FileName", "l_Left_Thumb.bmp");
            cmd.Parameters.AddWithValue("@LeftIndex_FileName", "l_Left_Index.bmp");
            cmd.Parameters.AddWithValue("@LeftMiddle_FileName", "l_Left_Middle.bmp");
            cmd.Parameters.AddWithValue("@LeftRing_FileName", "l_Left_Ring.bmp");
            cmd.Parameters.AddWithValue("@LeftLittle_FileName", "l_Left_Little.bmp");

            cmd.Parameters.AddWithValue("@RightThumb_FileName", "r_Right_Thumb.bmp");
            cmd.Parameters.AddWithValue("@RightIndex_FileName", "r_Right_Index.bmp");
            cmd.Parameters.AddWithValue("@RightMiddle_FileName", "r_Right_Middle.bmp");
            cmd.Parameters.AddWithValue("@RightRing_FileName", "r_Right_Ring.bmp");
            cmd.Parameters.AddWithValue("@RightLittle_FileName", "r_Right_Little.bmp");

            cmd.Parameters.AddWithValue("@LeftIris_FileName", "left_iris.png");
            cmd.Parameters.AddWithValue("@RightIris_FileName", "right_iris.png");

            cmd.Parameters.AddWithValue("@Face_FileName", "face.png");

            // ================= USERS =================
            cmd.Parameters.AddWithValue("@CreatedBy", user.UserName);
            cmd.Parameters.AddWithValue("@UpdatedBy", user.UserName);

            await cmd.ExecuteNonQueryAsync();
        }



    }
}
