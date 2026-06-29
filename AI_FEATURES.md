# Fayed AI Features

Two AI capabilities, both built on a **provider-agnostic abstraction** so the underlying
model can be changed at any time from configuration — no code changes.

## Provider abstraction (`BLL/AI`)

```
IAiProvider            ← one implementation per vendor (GeminiAiProvider, ClaudeAiProvider, ...)
IAiProviderResolver    ← picks the active provider from config (Ai:Provider)
AiCompletionRequest    ← provider-neutral request (system prompt, user prompt, attachments, JSON hint)
IDocumentContentFetcher← downloads KYB documents (image/PDF) for multimodal analysis
```

Adding a new provider = add one class implementing `IAiProvider`, register it, done
(Open/Closed + Dependency Inversion). Business code only ever talks to `IAiProviderResolver`.

Registered providers: `ItiGateway` (default), `Gemini`, `Claude`.

### Switching providers

In `appsettings.json`:

```jsonc
"Ai": {
  "Provider": "ItiGateway",    // change to "Gemini" / "Claude" to switch — no code changes
  "ItiGateway": {
    "BaseUrl": "http://apiaccess.iti.net.eg/student", // OpenAI-compatible Chat Completions gateway
    "ApiKey": "<your ITI key>",
    "Model": "anthropic.claude-sonnet-4-6"
  },
  "Gemini": { "ApiKey": "...", "Model": "gemini-1.5-flash" },
  "Claude": { "ApiKey": "...", "Model": "claude-sonnet-4-6" }
}
```

Flip `Ai:Provider`, set the key, restart — everything (smart search + verification) runs on the
new provider.

### Model choice (ITI gateway)

The gateway exposes Bedrock models behind one key. Recommended:

| Feature | Model | Why |
|---|---|---|
| Verification (KYB) | `anthropic.claude-sonnet-4-6` | Multimodal — reads document images/PDFs — and very reliable structured JSON |
| Smart search | `anthropic.claude-sonnet-4-6` (or `anthropic.claude-haiku-4-5-20251001-v1:0` for lower cost/latency) | Text → JSON filters |

Verification **requires a vision-capable model**; text-only models (DeepSeek, gpt-oss, Llama)
cannot read the documents. A per-call `ModelOverride` on `AiCompletionRequest` lets a cheaper
model be used for search while verification stays on Sonnet, all through the same gateway.

## 1. Smart search

`POST /api/listings/smart-search` — body `{ "query": "عايز حديد خردة في القاهرة تحت ١٠ آلاف" }`

Flow: `AiSearchService` asks the active provider to turn the free-text prompt into structured
filters → `ListingManager.SearchListingsAsync` runs the query → `SmartSearchManager` records the
attempt in **`AISearchLogs`** (prompt, extracted filters, result count, top listing IDs, model
version) for analytics.

> Fixes the previous bug where the Gemini request URL was a malformed markdown link, which made
> every smart search silently return empty results.

## 2. AI factory verification (KYB) — end to end

Flow (store-first, then verify — documents are kept regardless of the AI outcome so a human
officer can review them):

```
1. Upload    POST /api/documents/factories/{factoryId}      (multipart: file + documentType)
             → Cloudinary → Document row saved
2. Verify    POST /api/verification/factories/{factoryId}/ai-run
             → downloads the docs → base64 → AI gateway (vision model) → extract + cross-check
             → AIVerificationResult saved on a VerificationCase (Status = UnderReview)
3. Read      GET  /api/verification/cases/{caseId}/ai-result
   List docs GET  /api/documents/factories/{factoryId}
4. Decide    POST /api/verification/cases/{caseId}/decision   (officer: Approved/Rejected + notes)
             → closes the case, sets Factory.VerificationStatus, writes an AuditLog
```

- `AIVerificationResult` holds extracted fields, confidence score, mismatches, and an
  approve/review/reject recommendation + model version.
- The recommendation is **advisory**: it moves the case to `UnderReview`; the final
  approve/reject decision is a human officer action (the AI never auto-approves a factory).
- The result is 1..1 with `VerificationCase`; re-running replaces the previous AI result
  (delete-old/add-new) so each verification attempt's history is preserved.

## Notes

- AI/network failures are handled gracefully: smart search falls back to an empty filter set,
  verification returns a `502` with the provider error and never corrupts data.
- API keys are read from configuration / user-secrets — never commit real keys.
