using Amazon.S3;
using Amazon.S3.Model;

namespace StressTracker5001Server.Services
{
    public interface IFileStorageService
    {
        Task<bool> UploadFileAsync(Guid attachmentId, Stream fileStream, string fileName, string contentType);
        Task<bool> DeleteFileAsync(Guid attachmentId);
        string GetFileUrl(Guid attachmentId);
    }

    public class CloudflareFileStorageService : IFileStorageService
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;
        private readonly string _publicUrl;

        public CloudflareFileStorageService(IConfiguration configuration)
        {
            var config = configuration.GetSection("Cloudflare");
            _bucketName = config["BucketName"]!;
            _publicUrl = config["PublicUrl"]!;

            var s3Config = new AmazonS3Config
            {
                ServiceURL = $"https://{config["AccountId"]}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(
                config["AccessKeyId"],
                config["AccessKeySecret"],
                s3Config
            );
        }

        public async Task<bool> UploadFileAsync(Guid attachmentId, Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = $"uploads/{attachmentId}",
                    InputStream = fileStream,
                    ContentType = contentType
                };
                putRequest.Metadata.Add("original-filename", fileName);

                await _s3Client.PutObjectAsync(putRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFileAsync(Guid attachmentId)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = $"uploads/{attachmentId}"
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetFileUrl(Guid attachmentId)
        {
            return $"{_publicUrl}/uploads/{attachmentId}";
        }
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _storagePath;
        private readonly string _publicUrl;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(_storagePath);
            _publicUrl = configuration["LocalStorage:PublicUrl"]!;
        }

        public async Task<bool> UploadFileAsync(Guid attachmentId, Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var filePath = Path.Combine(_storagePath, attachmentId.ToString());
                using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
                await fileStream.CopyToAsync(fileStreamOutput);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> DeleteFileAsync(Guid attachmentId)
        {
            try
            {
                var filePath = Path.Combine(_storagePath, attachmentId.ToString());
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public string GetFileUrl(Guid attachmentId)
        {
            return $"{_publicUrl}/uploads/{attachmentId}";
        }
    }
}