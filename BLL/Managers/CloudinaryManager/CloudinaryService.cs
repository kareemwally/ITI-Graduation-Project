using BLL.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DAL.Models.ExceptionModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IO;

namespace BLL.Managers.CloudinaryManager
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> settings)
        {
            var account = new Account(
                settings.Value.CloudName,
                settings.Value.ApiKey,
                settings.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<bool> DeleteFileAsync(string fileUrl, string resourceType = "image")
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return false;

            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;

                var publicIdWithExtension = string.Join("", segments.SkipWhile(whileAndIncluding => !whileAndIncluding.Contains("upload/")).Skip(1));

                var publicId = Path.ChangeExtension(publicIdWithExtension, null);

                DeletionParams deletionParams;

                if (resourceType.ToLower() == "video")
                {
                    deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Video };
                }
                else if (resourceType.ToLower() == "raw")
                {
                    deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
                }
                else
                {
                    deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
                }

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result.ToLower() != "ok")
                {
                    throw new CloudinaryUploadException("فشل حذف الملف من سيرفر الميديا. السبب");
                }

                return true;
            }

            catch
            {
                throw new CloudinaryUploadException($"حدث خطأ غير متوقع أثناء محاولة حذف الملف");
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var extension = Path.GetExtension(file.FileName).ToLower();

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".jfif", ".webp" };
            var videoExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm" };

            UploadResult uploadResult;

            if (imageExtensions.Contains(extension))
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folderName
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            else if (videoExtensions.Contains(extension))
            {
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folderName
                };
                uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
            }
            else
            {
                var uploadParams = new RawUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folderName
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null)
            {
                throw new CloudinaryUploadException($"فشل رفع الملف '{file.FileName}' إلى السيرفر");
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string> UploadPdfAsync(byte[] pdfBytes, string fileName, string folderName)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                return string.Empty;

            using var stream = new MemoryStream(pdfBytes);

            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(fileName, stream),
                Folder = folderName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new CloudinaryUploadException($"فشل رفع ملف PDF '{fileName}' إلى السيرفر");
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}
