using System;

namespace FileTransferApp.Models
{
    public class FileResponseModel
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool HasPassword { get; set; }
        public bool IsPublic { get; set; }
        public string UserId { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
    }
} 