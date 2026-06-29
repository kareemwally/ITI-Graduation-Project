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

## 2. AI factory verification (KYB)

- `POST /api/verification/factories/{factoryId}/ai-run` — analyzes the factory's uploaded
  documents, extracts the key fields, cross-checks them against the company's declared data,
  and stores an **`AIVerificationResult`** (extracted fields, confidence score, mismatches,
  approve/review/reject recommendation, model version) attached to a `VerificationCase`.
- `GET /api/verification/cases/{caseId}/ai-result` — returns the stored AI result for a case.

The verification result is a 1..1 with `VerificationCase`; re-running replaces the previous AI
result (delete-old/add-new) so the verification history per case is preserved.

## Notes

- AI/network failures are handled gracefully: smart search falls back to an empty filter set,
  verification returns a `502` with the provider error and never corrupts data.
- API keys are read from configuration / user-secrets — never commit real keys.
