using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class ImageService : IImageService
    {
        private string? _defaultImage = "/img/DefaultImage.jpg";
        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, string? imgEntity)
        {
            try
            {
                if (!string.IsNullOrEmpty(imgEntity))
                {
                    _defaultImage = $"/img/default{imgEntity}.jpg";
                }

                if (fileData == null || fileData.Length == 0)
                {
                    //show default

                    return _defaultImage;
                }

                string? imageBase64Data = Convert.ToBase64String(fileData);
                imageBase64Data = string.Format($"data:{extension};base64, {imageBase64Data}");
                return imageBase64Data;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                if (file != null)
                {
                    using MemoryStream memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    byte[] byteFile = memoryStream.ToArray();
                    memoryStream.Close();
                    return byteFile;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}