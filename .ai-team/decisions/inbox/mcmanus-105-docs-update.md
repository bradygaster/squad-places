# Decision: Documentation path migration update pattern (#105)

**Author:** McManus  
**Date:** 2026-02-19  
**Status:** Completed  
**Related Issues:** #105, #101, #104, #108  

---

## Summary

Completed documentation and test suite updates for the `.ai-team/` → `.squad/` directory rename. Updated 6 core files (README.md, CONTRIBUTING.md, test/init-flow.test.js, test/plugin-marketplace.test.js) and created a comprehensive migration guide for users upgrading from v0.4.x to v0.5.0.

---

## What Changed

### Documentation Files
- **README.md**: Updated 7 references to `.squad/` (directory structure diagram, agent removal note, upgrade safety note, insider state note, label sync trigger, workflow table reference, server error recovery note)
- **CONTRIBUTING.md**: Updated 9 references (branch diagram, branch purpose table, protected files explanation, guard workflow description, PR fixing instructions, quick reference diagram, FAQ responses, summary section)

### Test Files
- **test/init-flow.test.js**: Updated 3 assertions to reference `.squad/` path patterns
- **test/plugin-marketplace.test.js**: Updated 2 assertions for `.squad/plugins/` state storage location

### New Documentation
- **docs/migration/v0.5.0-squad-rename.md**: Comprehensive 400+ line migration guide including:
  - What changed at a glance (table)
  - Pre-migration checklist
  - Step-by-step migration process (3 steps)
  - Email scrubbing details (what's removed vs. preserved)
  - Git history note (git filter-repo for full cleanup)
  - Backward compatibility (v0.5.0-v0.6.0 transition period)
  - Gradual migration strategy
  - Troubleshooting (5 Q&A sections)
  - Post-migration verification
  - Deprecation timeline (v0.4.x → v1.0.0)

---

## Reasoning

The `.squad/` rename is a breaking change for user repos. Documentation must simultaneously:

1. **Guide current users** — step-by-step migration without losing their work
2. **Preserve backward compatibility messaging** — assure v0.5.0 still works with `.ai-team/` (with deprecation warning)
3. **Establish a timeline** — users need to know when migration becomes required (v1.0.0)
4. **Explain the purpose** — email scrubbing + PII removal context (Baer's #108 discovery)
5. **Preempt concerns** — Q&A format addresses top 5 worries (can I undo? do I have to? what gets removed? when?)

The migration guide format is specifically designed for user-facing communication—not technical spec, not release notes, but "here's what's happening, why, and what you need to do."

---

## Testing

- All tests pass: 53/53 ✅
- init-flow tests verify `.squad/` is recognized by Init Mode
- plugin-marketplace tests confirm state storage at `.squad/plugins/marketplaces.json`
- Documentation changes reviewed for accuracy and completeness

---

## Key Decisions Made

1. **Migration guide is primary user communication** — not buried in upgrade command output, but published as discoverable docs
2. **Backward compat is explicit** — guide states v0.5.0-v0.6.0 support both; migration required in v1.0.0
3. **Email scrubbing is documented** — users know what PII is removed and why (privacy protection, not paranoia)
4. **Timeline is clear** — v0.4.x → v0.5.0+ (optional) → v0.6.0+ (encouraged) → v1.0.0 (required)
5. **Troubleshooting in Q&A format** — addresses emotional concerns ("will I lose data?") before technical ones

---

## Follow-up Actions

- None. PR #113 ready for review and merge to dev.
- Migration guide will be published with v0.5.0 release docs.
- Deprecation warning in CLI will link to migration guide.

---

## Related Decisions

- **#101 (Fenster)** — `squad upgrade --migrate-directory` command (implementation)
- **#104 (Fenster)** — `.ai-team-templates/` → `.squad/templates/` (template file organization)
- **#108 (Baer)** — Email scrubbing discovery + `git filter-repo` guidance
