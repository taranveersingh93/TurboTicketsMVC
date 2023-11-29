using TurboTicketsMVC.Models.Enums;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTFileService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string ConvertByteArrayToFile(byte[] fileData, string extension,  DefaultImage fileType);

        public string GetFileIcon(string file);

        public string FormatFileSize(long bytes);

    }
}
