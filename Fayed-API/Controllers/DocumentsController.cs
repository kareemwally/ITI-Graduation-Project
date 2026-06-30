using BLL.DTOs.Documents;
using BLL.Managers.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentManager _documentManager;

        public DocumentsController(IDocumentManager documentManager)
        {
            _documentManager = documentManager;
        }

        // Upload a single KYB document for a factory (multipart/form-data: file + documentType).
        // Documents are stored first; verification reads them afterwards.
        [HttpPost("factories/{factoryId:int}")]
        [RequestSizeLimit(20_000_000)] // 20 MB
        public async Task<IActionResult> UploadFactoryDocument(
            int factoryId, [FromForm] UploadFactoryDocumentRequest request)
        {
            var response = await _documentManager.UploadFactoryDocumentAsync(
                factoryId, request.File, request.DocumentType);
            return StatusCode(response.StatusCode, response);
        }

        // List the documents already uploaded for a factory.
        [HttpGet("factories/{factoryId:int}")]
        public async Task<IActionResult> GetFactoryDocuments(int factoryId)
            => Ok(await _documentManager.GetFactoryDocumentsAsync(factoryId));
    }
}
