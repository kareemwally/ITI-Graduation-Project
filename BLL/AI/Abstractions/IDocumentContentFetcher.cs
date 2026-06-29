namespace BLL.AI.Abstractions
{
    /// <summary>
    /// Downloads a stored document (image/PDF) from its URL and turns it into an
    /// <see cref="AiAttachment"/> the model can read. Best-effort: returns null on any failure
    /// or for unsupported content types.
    /// </summary>
    public interface IDocumentContentFetcher
    {
        Task<AiAttachment?> FetchAsync(string fileUrl, CancellationToken cancellationToken = default);
    }
}
