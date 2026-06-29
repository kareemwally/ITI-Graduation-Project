namespace BLL.AI
{
    /// <summary>Helpers for coercing model output into parseable JSON.</summary>
    public static class AiJson
    {
        /// <summary>
        /// Strips markdown code fences and any leading/trailing prose so the result can be
        /// deserialized even when a model ignores the "raw JSON only" instruction.
        /// </summary>
        public static string Clean(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "{}";

            var text = raw.Replace("```json", string.Empty)
                          .Replace("```", string.Empty)
                          .Trim();

            // Keep only the outermost JSON object if the model added stray text around it.
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                text = text.Substring(start, end - start + 1);

            return string.IsNullOrWhiteSpace(text) ? "{}" : text;
        }
    }
}
