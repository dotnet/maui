---
name: instruction-drift
description: >-
  Detect drift in instruction files and skills against upstream documentation sources.
  Checks when tracked GitHub issues close, reference pages change, or new platform features
  ship that aren't reflected locally. Use when asked to "check for drift", "are instructions up to date",
  "instruction drift", "check if gh-aw docs are current", "update instructions from upstream",
  "what changed upstream", or "have our instructions drifted". Also use proactively before major
  instruction file edits to ensure you're working with current information. Produces a drift
  report — never auto-edits files.
---

# Instruction Drift Skill

Detect when instruction files or skills have drifted from their upstream documentation sources. This skill **only detects and reports** — it never auto-edits instruction files.

## When to Use

- Before editing any instruction file that tracks external documentation
- When asked if instructions are up to date
- When upstream platforms (gh-aw, Helix, Android SDK) ship new releases
- Periodically (monthly) to catch drift

## How It Works

1. **Discover targets** — Find all `.sync.yaml` manifest files in the repository
2. **Check sources** — Run `Check-Staleness.ps1` to fetch source status (issue states, page hashes, releases)
3. **Compare** — Identify what changed since last check
4. **Report** — Present a prioritized staleness report

## Running the Skill

### Step 1: Run the staleness check script

```bash
pwsh .github/skills/instruction-drift/scripts/Check-Staleness.ps1
```

The script outputs a JSON report to stdout with:
- Which tracked issues changed state (open → closed or vice versa)
- Which reference page content hashes changed
- Which tracked repos have new releases
- Whether any sources failed to fetch (404, timeout, etc.)

### Step 2: Analyze the report

Read the JSON output and the target instruction file. For each detected change:

1. **Issue state change** — Check if the instruction file references the issue. If the issue closed, look for workarounds or "upstream issue" references that may now be obsolete.
2. **Page content change** — The page changed but we don't know what specifically. Fetch the page content and compare against what the instruction file documents.
3. **New release** — Check release notes for features or breaking changes relevant to the instruction file.
4. **Fetch failure** — A source URL returned 404 or timed out. The manifest may need updating (URL moved or docs restructured).

### Step 3: Classify changes

| Priority | Description | Action |
|----------|-------------|--------|
| **P0** | Factually wrong content (closed issue still referenced as open, deprecated feature documented as current) | Flag for immediate update |
| **P1** | Missing security-relevant changes (new anti-patterns, protection mechanisms) | Flag for prompt update |
| **P2** | Missing new features or capabilities | Flag for update when convenient |
| **P3** | Nice-to-have updates (new examples, reorganization) | Flag for future consideration |

### Step 4: Output the report

Present findings as a structured report. If invoked manually, show the report in the conversation. If invoked by a workflow, the report can be posted as a GitHub issue.

## Manifest Format

Each instruction file that tracks external sources has a `.sync.yaml` manifest co-located with it:

```yaml
# .github/instructions/gh-aw-workflows.sync.yaml
target: gh-aw-workflows.instructions.md

# Sections that contain our own operational knowledge (not from upstream).
# The agent should NEVER remove or rewrite these — only append new content.
divergence:
  - section: "Security Boundaries"
    reason: "Our operational analysis and defense-in-depth assessment"
  - section: "Safe Pattern: Checkout + Restore"
    reason: "Documents our custom Checkout-GhAwPr.ps1 script"

sources:
  # Reference documentation pages
  - url: https://github.github.com/gh-aw/reference/triggers/
    sections: ["Anti-Patterns", "Common Patterns", "Limitations"]
  - url: https://github.github.com/gh-aw/reference/frontmatter/
    sections: ["Anti-Patterns", "Architecture", "Additional Frontmatter Features"]
  - url: https://github.github.com/gh-aw/reference/safe-outputs/
    sections: ["Anti-Patterns", "Safe Outputs Quick Reference"]

  # Tracked GitHub issues (check open/closed state)
  - issue: github/gh-aw#18481
    resolution_expected: true
  - issue: github/gh-aw#25439
    resolution_expected: true

  # GitHub releases (check for new versions)
  - releases: github/gh-aw

style: |
  Match existing section structure. Use tables for feature comparisons.
  Include code examples for common patterns. Mark items as anti-patterns
  when a manual reimplementation should use a built-in feature instead.
  Keep security guidance precise — never simplify away nuance.
```

## Important Constraints

- **Never auto-edit instruction files.** Always produce a drift report for human review.
- **Respect `divergence:` sections.** These contain hard-won operational knowledge that doesn't come from upstream docs. Never suggest removing this content.
- **Report fetch failures loudly.** A 404 on a tracked URL is a signal that the manifest needs updating, not that nothing changed.
- **Distinguish mechanical vs. judgment changes.** A closed issue is a mechanical fact. A restructured docs page requires human judgment about what to update.
