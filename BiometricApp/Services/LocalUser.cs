using BiometricApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class LocalUser
    {
        private SaveUserResult resultObj = new();

        public async Task<SaveUserResult> SaveUser(AppUser user)
        {

            resultObj.Message = string.Empty;
            try
            {
                string orgFolder = Path.Combine(AppSettings.UserFolder, user.OrganizationCode);

                if (!Directory.Exists(orgFolder))
                    Directory.CreateDirectory(orgFolder);

                string filePath = Path.Combine(orgFolder, "users.json");

                List<AppUser> users = new();

                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    users = JsonSerializer.Deserialize<List<AppUser>>(json) ?? new();
                }

                // Check for duplicate username
                if (users.Any(u => u.Username == user.Username))
                {
                    resultObj.Message = "Username already exists!";
                    resultObj.Success = false; // operation failed
                    return resultObj;
                }

                users.Add(user);

                string newJson = JsonSerializer.Serialize(users, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(filePath, newJson);

                resultObj.Message = "User saved successfully.";
                resultObj.Success = true; // operation succeeded
                return resultObj;
            }
            catch (Exception ex)
            {
                resultObj.Message = "Error: " + ex.Message;
                resultObj.Success = false;
                return resultObj;
            }

           
        }

    }
}
