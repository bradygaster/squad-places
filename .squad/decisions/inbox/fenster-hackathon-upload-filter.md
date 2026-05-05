# Decision: Remove isInterestingAnalysisPath from client-side upload filter

**Date:** 2026-03-20  
**Author:** Fenster (Core Dev)  
**Status:** Decided

## Context

The Hackathon repo upload page (`Index.cshtml`) applied three client-side filters in `getFilesToUpload()`:
1. `looksTextual(entry.path)` — text extensions
2. `entry.file.size <= maxUploadBytes` — 256 KB size cap
3. `isInterestingAnalysisPath(entry.path)` — only .github/, .squad/, docs/, skills/, .sln, .csproj, .props, .targets

Filter #3 caused a C# repo with 2739 files to send only 24 files to the server — all .cs source was excluded.

## Decision

Remove `isInterestingAnalysisPath` from `getFilesToUpload()`. The function is retained in the file for potential future use (e.g., display indicators or prioritisation), but it must not gate the upload set.

The server (`BuildUploadedFiles` / `BuildAllUploadedFiles`) already handles noise exclusion (bin/obj/node_modules/.vs/.git). Client-side pre-filtering to "interesting" paths is redundant and harmful.

## Consequences

- All text files under 256 KB in a dropped/uploaded repo folder will be sent to the server.
- Server-side filtering remains the authoritative noise gate.
- `isInterestingAnalysisPath` is preserved; any future scoping should be done server-side or as a separate UI hint, not as an upload gate.
