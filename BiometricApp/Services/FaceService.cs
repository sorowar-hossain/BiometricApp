using System;
using System.IO;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class FaceService
    {
        private readonly string baseFolder;

        public FaceService()
        {
            // Set base folder to C:\BiometricData\Members
            baseFolder = AppSettings.BaseFolder;
        }

        public async Task<string> SaveFaceAsync(string base64Image,string personUniqueId)
        {
            if (string.IsNullOrEmpty(base64Image))
                throw new Exception("Image is empty");

            // remove base64 header
            var base64Data = base64Image.Split(',')[1];

            byte[] imageBytes = Convert.FromBase64String(base64Data);

            // default folder

            string memberFolder = Path.Combine(baseFolder, personUniqueId);

            //var folderPath = Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            //    "BiometricFaces"
            //);

            if (!Directory.Exists(memberFolder))
                Directory.CreateDirectory(memberFolder);

            // unique file name
            string fileName = $"face.png";

            string fullPath = Path.Combine(memberFolder, fileName);

            await File.WriteAllBytesAsync(fullPath, imageBytes);

            return fullPath;
        }
    }
}