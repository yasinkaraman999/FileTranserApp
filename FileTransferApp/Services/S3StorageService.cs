using Amazon.S3;
using Amazon.S3.Model;
using FileTransferApp.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FileTransferApp.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration.GetValue<string>("AWS:BucketName") ?? throw new ArgumentNullException("AWS:BucketName configuration is missing");
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);
            return fileName;
        }

        public async Task<Stream> DownloadFileAsync(string filePath)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };

                var response = await _s3Client.GetObjectAsync(request);
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException("File not found in S3.", filePath);
            }
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath
            };

            await _s3Client.DeleteObjectAsync(request);
        }

        public bool FileExists(string filePath)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };

                _s3Client.GetObjectMetadataAsync(request).Wait();
                return true;
            }
            catch (AggregateException ex) when (ex.InnerException is AmazonS3Exception s3Ex && 
                                              s3Ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
} 