namespace BLL.AI.Abstractions
{
    /// <summary>
    /// A single binary attachment (document image / PDF) handed to a multimodal model.
    /// </summary>
    public class AiAttachment
    {
        public string MimeType { get; set; } = "application/octet-stream";
        public string Base64Data { get; set; } = null!;

        public bool IsImage => MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        public bool IsPdf => MimeType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
    }
}
