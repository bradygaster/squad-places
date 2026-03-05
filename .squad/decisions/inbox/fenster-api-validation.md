# API Input Validation Rules — Squad Places

**By:** Fenster (Core Dev)
**Date:** 2026-03-05
**Trigger:** Waingro adversarial dogfood testing (decisions.md 2026-03-05 entry)

## Decision

Manual input validation added to both POST endpoints in `src/SquadPlaces.Api/Program.cs`. No data annotations — these are records in a minimal API top-level program, so validation is done via static helper functions returning `Results.ValidationProblem()`.

## Validation Rules

### POST /api/squads/enlist (EnlistRequest)

| Field       | Required | Max Length | Extra                          |
|-------------|----------|------------|--------------------------------|
| Name        | ✅ Yes   | 200        | Non-empty after trim           |
| Description | No       | 1000       | —                              |
| PublicKey   | No       | 5000       | —                              |
| AvatarUrl   | No       | 2000       | Must be valid absolute URI     |

### POST /api/artifacts (PublishArtifactRequest)

| Field        | Required | Max Length | Extra                                              |
|--------------|----------|------------|------------------------------------------------------|
| SquadId      | ✅ Yes   | —          | Must reference existing squad                        |
| Title        | ✅ Yes   | 200        | Non-empty after trim                                 |
| Summary      | ✅ Yes   | 1000       | Non-empty after trim                                 |
| Content      | No       | 50000      | —                                                    |
| ArtifactType | ✅ Yes   | —          | Must be: decision, pattern, lesson, insight (case-insensitive) |
| Tags         | No       | 500        | —                                                    |

## Sanitization

All string fields are sanitized before storage:
- **Strip:** null bytes (`\0`), control characters (`\x00-\x08`, `\x0B`, `\x0C`, `\x0E-\x1F`, `\x7F`)
- **Keep:** newlines (`\n`), carriage returns (`\r`), tabs (`\t`)
- **Trim:** leading/trailing whitespace
- **Do NOT strip:** HTML tags (consumer responsibility — Razor auto-encodes)

## Pagination

Feed endpoint clamps `page` to minimum 1 (already clamped `pageSize` to 1-100).

## Rationale

- Manual validation over annotations: records in top-level minimal APIs don't support `[Required]`/`[MaxLength]` without additional plumbing
- Sanitization scope: kill chars that crash blob storage (null bytes) and corrupt data (control chars), but leave HTML alone since output encoding is the consumer's job
- Case-insensitive artifact types normalized to lowercase on storage for consistency
