using BiometricApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class LocalStorageService
    {
        private readonly string baseFolder;

        public LocalStorageService()
        {
            // Set base folder to C:\BiometricData\Members
            baseFolder = AppSettings.BaseFolder;

            // Ensure the folder exists
            Directory.CreateDirectory(baseFolder);
        }


        // Saves demographics data to a member folder.
       
        public async Task<string> SaveDemographicsAsync(MemberModel member)
        {
            // Each member gets their own folder
            string memberFolder = Path.Combine(baseFolder, member.PersonUniqueId);
            Directory.CreateDirectory(memberFolder);

            // Save demographics JSON
            string jsonPath = Path.Combine(memberFolder, "demographics.json");
            string json = JsonSerializer.Serialize(member, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, json);

            return memberFolder;
        }


        /// Loads demographics data for a member from folder
        public async Task<MemberModel?> LoadDemographicsAsync(string personUniqueId)
        {
            string memberFolder = Path.Combine(baseFolder, personUniqueId);
            string jsonPath = Path.Combine(memberFolder, "demographics.json");

            if (!File.Exists(jsonPath)) return null;

            string json = await File.ReadAllTextAsync(jsonPath);
            return JsonSerializer.Deserialize<MemberModel>(json);
        }


        /// Lists all saved member folders
        public string[] GetAllMemberFolders()
        {
            if (!Directory.Exists(baseFolder)) return Array.Empty<string>();
            return Directory.GetDirectories(baseFolder);
        }

 
        // Deletes a member folder after upload or cleanup
        public void DeleteMemberFolder(string personUniqueId)
        {
            string memberFolder = Path.Combine(baseFolder, personUniqueId);
            if (Directory.Exists(memberFolder))
            {
                Directory.Delete(memberFolder, true); // recursive delete
            }
        }
    }
}