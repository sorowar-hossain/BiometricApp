using System;
using System.IO;
using System.Threading.Tasks;

namespace BiometricApp.Services
{
    public class FaceService
    {

        public async Task<string> SaveFaceAsync(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image))
                throw new Exception("Image is empty");

            // remove base64 header
            var base64Data = base64Image.Split(',')[1];

            byte[] imageBytes = Convert.FromBase64String(base64Data);

            // default folder
            var folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "BiometricFaces"
            );

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // unique file name
            string fileName = $"face_{DateTime.Now:yyyyMMddHHmmss}.png";

            string fullPath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(fullPath, imageBytes);

            return fullPath;
        }
    }
}