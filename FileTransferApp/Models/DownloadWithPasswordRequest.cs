using System;

namespace FileTransferApp.Models
{
    public class DownloadWithPasswordRequest
    {
        public Guid Id { get; set; }
        public string Password { get; set; }
    }
} 