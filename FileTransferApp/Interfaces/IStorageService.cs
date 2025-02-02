using Microsoft.AspNetCore.Http;

namespace FileTransferApp.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string fileName);
        Task<Stream> DownloadFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        bool FileExists(string filePath);
    }
} 