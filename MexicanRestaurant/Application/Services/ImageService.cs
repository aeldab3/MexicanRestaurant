using MexicanRestaurant.Core.Interfaces;


namespace MexicanRestaurant.Application.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _env = webHostEnvironment;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile, string folderName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid image format. Allowed formats are: " + string.Join(", ", allowedExtensions));

            long maxFileSize = 1 * 1024 * 1024; // 1MB
            if (imageFile.Length > maxFileSize)
                throw new InvalidOperationException("Image size exceeds the maximum limit of 1MB.");

            var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string guidPart = Guid.NewGuid().ToString().Substring(0, 8);
            var uniqueName = $"{fileName}_{timeStamp}_{guidPart}{extension}";
            var uploadPath = Path.Combine(_env.WebRootPath, folderName);

            Directory.CreateDirectory(uploadPath); // Ensure the directory exists (if not, create it)

            var fileFullPath = Path.Combine(uploadPath, uniqueName);

            using (var stream = new FileStream(fileFullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return uniqueName; // to save it in the database
        }

        public async Task DeleteImageAsync(string imageName, string folderName)
        {
            if (string.IsNullOrEmpty(imageName)) return;
            var imagePath = Path.Combine(_env.WebRootPath, folderName, imageName);
            if (File.Exists(imagePath))
                await Task.Run(() => File.Delete(imagePath));
        }
    }
}
