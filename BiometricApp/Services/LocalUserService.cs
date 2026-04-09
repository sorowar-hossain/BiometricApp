using BiometricApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class LocalUserService
    {
        private SaveUserResult resultObj = new();
        private UserLoginResponse loginObj = new(); 
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


        public async Task<UserLoginResponse> UserValidation(LoginModel user)  
        {

            loginObj.Message = string.Empty;
            try
            {
                string orgFolder = Path.Combine(AppSettings.UserFolder, AppSettings.OrganizationCode);

                if (!Directory.Exists(orgFolder))
                {
                    loginObj.Message = "Invalid username or password. Please try again.";
                    loginObj.Success = false; 
                    return loginObj;
                }
                   

                string filePath = Path.Combine(orgFolder, "users.json");

                List<AppUser> users = new();

                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    users = JsonSerializer.Deserialize<List<AppUser>>(json) ?? new();
                }

                // Check for Valid username
                if (users.Any(u => u.Username == user.Username && u.PasswordHash == HashPassword(user.Password)))
                {
                    loginObj.Message = "Valid User";
                    loginObj.Success = true;
                    loginObj.OrganizationName = AppSettings.OrganizationName;
                    loginObj.OrganizationCode = AppSettings.OrganizationCode;  
                    return loginObj;
                }

                else
                {
                    loginObj.Message = "Invalid username or password. Please try again.";
                    loginObj.Success = false; 
                    return loginObj;
                }

               
            }
            catch (Exception ex)
            {
                loginObj.Message = "Error: " + ex.Message;
                loginObj.Success = false;
                return loginObj;
            }


        }

        public string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

    }
}
