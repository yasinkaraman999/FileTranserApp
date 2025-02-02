using System;

namespace FileTransferApp.Data.Models
{
    public class FileEntity
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Password { get; set; }
        public bool IsPublic { get; set; }
        public string? UserId { get; set; }
        public string? SenderEmail { get; set; }
        public string? RecipientEmail { get; set; }
        public string? Message { get; set; }

        // Navigation property
        public virtual ApplicationUser? User { get; set; }
    }
} 