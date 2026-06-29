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

### Switching providers

In `appsettings.json`:

```jsonc
"Ai": {
  "Provider": "Gemini",        // change to "Claude" (or any registered provider) to switch
  "Gemini": { "ApiKey": "...", "Model": "gemini-1.5-flash" },
  "Claude": { "ApiKey": "...", "Model": "claude-sonnet-4-6" }
}
```

Flip `Ai:Provider` to `Claude`, set its key, restart — everything (smart search + verification)
now runs on Claude.

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
