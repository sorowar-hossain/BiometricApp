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
            try
            {
                await File.WriteAllTextAsync(jsonPath, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

            return memberFolder;
        }


        public async Task<bool> UpdateDemographicsAsync(MemberModel member)
        {
            string memberFolder = Path.Combine(baseFolder, member.PersonUniqueId);
            string jsonPath = Path.Combine(memberFolder, "demographics.json");

            // Check if file exists
            if (!File.Exists(jsonPath))
                return false;

            string json = JsonSerializer.Serialize(member, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(jsonPath, json);

            return true;
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


        public void DeleteAllMemberFolders()
        {
            if (Directory.Exists(baseFolder))
            {
                var directories = Directory.GetDirectories(baseFolder);

                foreach (var dir in directories)
                {
                    Directory.Delete(dir, true); // recursive delete
                }
            }
        }

        public async Task<List<MemberDropdown>> GetMembersForDropdownAsync()
        {
            var result = new List<MemberDropdown>();

            if (!Directory.Exists(baseFolder))
                return result;

            var folders = Directory.GetDirectories(baseFolder);

            foreach (var folder in folders)
            {
                var jsonPath = Path.Combine(folder, "demographics.json");

                if (File.Exists(jsonPath))
                {
                    var json = await File.ReadAllTextAsync(jsonPath);
                    var member = JsonSerializer.Deserialize<MemberModel>(json);

                    if (member != null)
                    {
                        result.Add(new MemberDropdown
                        {
                            PersonUniqueId = member.PersonUniqueId,
                            FullName = $"{member.FirstName} {member.LastName} ({member.CreatedOn.ToString("HH:mm")})"
                        });
                    }
                }
            }

            return result.OrderBy(x => x.FullName).ToList(); 
        }
    }
}