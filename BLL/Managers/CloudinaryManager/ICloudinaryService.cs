using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Managers.CloudinaryManager
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName );
    }
}
