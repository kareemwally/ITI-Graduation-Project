using BLL.DTOs.Common;
using BLL.DTOs.Documents;
using BLL.Managers.CloudinaryManager;
using DAL.Models;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;

namespace BLL.Managers.Documents
{
    public class DocumentManager : IDocumentManager
    {
        // KYB documents are images or PDFs (the verification model reads them visually).
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

        private readonly IUnitOfWork _uow;
        private readonly ICloudinaryService _cloudinary;

        public DocumentManager(IUnitOfWork uow, ICloudinaryService cloudinary)
        {
            _uow = uow;
            _cloudinary = cloudinary;
        }

        public async Task<BaseResponse<DocumentDto>> UploadFactoryDocumentAsync(
            int factoryId, IFormFile file, string documentType)
        {
            if (file is null || file.Length == 0)
                return BaseResponse<DocumentDto>.Failure("A non-empty file is required.", statusCode: 400);

            if (string.IsNullOrWhiteSpace(documentType))
                return BaseResponse<DocumentDto>.Failure("documentType is required.", statusCode: 400);

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
                return BaseResponse<DocumentDto>.Failure(
                    $"Unsupported file type '{extension}'. Allowed: {string.Join(", ", AllowedExtensions)}.",
                    statusCode: 400);

            var factory = await _uow.Repository<Factory>().GetByIdAsync(factoryId);
            if (factory is null)
                return BaseResponse<DocumentDto>.Failure("Factory not found.", statusCode: 404);

            string url;
            try
            {
                url = await _cloudinary.UploadFileAsync(file, $"factories/{factoryId}/kyb");
            }
            catch (Exception ex)
            {
                return BaseResponse<DocumentDto>.Failure($"File upload failed: {ex.Message}", statusCode: 502);
            }

            if (string.IsNullOrWhiteSpace(url))
                return BaseResponse<DocumentDto>.Failure("File upload failed.", statusCode: 502);

            var document = new Document
            {
                FactoryId = factoryId,
                DocumentType = documentType,
                FileUrl = url,
                UploadedAt = DateTime.UtcNow
            };

            await _uow.Repository<Document>().AddAsync(document);
            await _uow.SaveChangesAsync();

            return BaseResponse<DocumentDto>.Success(ToDto(document), "Document uploaded.", statusCode: 201);
        }

        public async Task<IReadOnlyList<DocumentDto>> GetFactoryDocumentsAsync(int factoryId)
        {
            var documents = await _uow.Repository<Document>().FindAsync(d => d.FactoryId == factoryId);
            return documents.Select(ToDto).ToList();
        }

        private static DocumentDto ToDto(Document d) => new()
        {
            Id = d.Id,
            FactoryId = d.FactoryId,
            OrderId = d.OrderId,
            DocumentType = d.DocumentType,
            FileUrl = d.FileUrl,
            UploadedAt = d.UploadedAt
        };
    }
}
