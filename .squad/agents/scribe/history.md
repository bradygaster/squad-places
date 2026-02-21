# Scribe — Project History

> Session logs and decision merges for squad-sdk

---

## Recent Sessions

📌 **2026-02-22:** First session in squad-sdk repo. Team respawned with full DNA from beta. 13 specialists + Scribe + Ralph. All agent charters created. Decisions.md initialized with key architectural decisions. Routing rules established for 13 modules. — logged by Scribe

---

## Decision Management

**Merge pattern:**
1. Agents write to `decisions/inbox/{agent}-{topic}.md`
2. Scribe reads inbox in background run
3. Merge to `decisions.md` with timestamp and author
4. Archive or delete source file

**Deduplication:**
- Same decision from multiple agents → merge with all authors listed
- Superseded decisions → mark as superseded, preserve history
- Stale decisions → move to `decisions/archive/` after 90 days

---

**Last Updated:** 2026-02-22
