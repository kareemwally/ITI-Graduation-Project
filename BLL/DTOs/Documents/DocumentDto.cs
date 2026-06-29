namespace BLL.DTOs.Documents
{
    /// <summary>API view of a stored document (KYB file or order document).</summary>
    public class DocumentDto
    {
        public int Id { get; set; }
        public int? FactoryId { get; set; }
        public int? OrderId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
