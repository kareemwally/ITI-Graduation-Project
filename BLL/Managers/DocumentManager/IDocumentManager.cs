using BLL.DTOs.Common;
using BLL.DTOs.Documents;
using Microsoft.AspNetCore.Http;

namespace BLL.Managers.Documents
{
    /// <summary>
    /// Handles KYB document uploads: stores the file (Cloudinary) and persists a Document row so the
    /// verification flow can later read it. Documents are kept regardless of the AI outcome so a
    /// human officer can review them.
    /// </summary>
    public interface IDocumentManager
    {
        Task<BaseResponse<DocumentDto>> UploadFactoryDocumentAsync(int factoryId, IFormFile file, string documentType);
        Task<IReadOnlyList<DocumentDto>> GetFactoryDocumentsAsync(int factoryId);
    }
}
