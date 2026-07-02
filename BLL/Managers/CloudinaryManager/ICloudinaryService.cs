using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace BLL.Managers.CloudinaryManager
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task<string> UploadPdfAsync(byte[] pdfBytes, string fileName, string folderName);
        Task<bool> DeleteFileAsync(string fileUrl, string resourceType = "image");
    }
}
