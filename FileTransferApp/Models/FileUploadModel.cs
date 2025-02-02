using System;

namespace FileTransferApp.Models
{
    public class FileUploadModel
    {
        public DateTime? ExpiryDate { get; set; }
        public string Password { get; set; }
        public bool IsPublic { get; set; } = true;
        public string SenderEmail { get; set; }
        public string RecipientEmail { get; set; }
        public string Message { get; set; }
    }
} 