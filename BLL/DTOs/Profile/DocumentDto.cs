namespace BLL.DTOs.Profile
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
