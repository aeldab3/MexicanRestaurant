namespace MexicanRestaurant.Core.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile imageFile, string folderName);
        Task DeleteImageAsync(string imageName, string folderName);
    }
}
