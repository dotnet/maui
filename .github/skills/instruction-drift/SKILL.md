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
2. **Snapshot sources** — Run `Check-Staleness.ps1` to capture current source status (issue states, page content hashes, latest releases with release notes)
3. **Crawl indexes** — Fetch doc site index pages (reference/, patterns/, guides/) and identify pages not yet tracked in the manifest
4. **Discover issues** — Find recently closed issues (last 90 days) in tracked repos that aren't in the manifest
5. **Flag drift signals** — Identify closed issues, fetch errors, content hash changes, untracked pages, untracked issues, and coverage gaps
6. **Report** — Present a prioritized drift report with actionable items for human review

## Running the Skill

### Step 1: Run the staleness check script

```bash
pwsh .github/skills/instruction-drift/scripts/Check-Staleness.ps1
```

The script outputs a JSON report to stdout with:
- Current state of tracked issues (open/closed) and whether resolution was expected
- Content hashes of reference pages (compare across runs to detect changes)
- Latest release tag and release notes (truncated to 2000 chars) for tracked repos
- **Untracked pages** — new doc pages discovered by crawling index pages that aren't in the manifest
- **Untracked closed issues** — recently closed issues (last 90 days) not yet tracked in the manifest
- Coverage gaps declared in the manifest (features from a page not yet documented locally)
- Whether any sources failed to fetch (404, timeout, etc.)
- `changes_detected` flag — true when any actionable signal is found

### Step 2: Analyze the report

Read the JSON output and the target instruction/skill files. For each detected signal:

1. **Untracked pages** (highest priority) — New doc pages the manifest doesn't track. Fetch each page, determine if it documents features relevant to our skill, and if so: (a) add the URL to the manifest, (b) extract the key features, (c) update the skill files.
2. **Untracked closed issues** — Recently closed issues not in the manifest. Check if any represent feature ships or bug fixes that affect our documented guidance. Add relevant ones to the manifest with `resolution_expected: true`.
3. **Coverage gaps** — Features from a tracked page that we know we haven't documented. Fetch the page, extract the missing features, and update the skill files.
4. **Issue state change** — If a tracked issue closed, look for workarounds or "upstream issue" references that may now be obsolete.
5. **Page content change** — A hash changed but we don't know what specifically. Fetch the page and compare against what we document. Check `coverage_gaps` first — the change may have added features we already know we're missing.
6. **New release** — Read the release notes (included in the report) for features or breaking changes. Cross-reference against our anti-patterns table and feature documentation.
7. **Fetch failure** — A source URL returned 404 or timed out. The manifest may need updating (URL moved or docs restructured).

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
  - url: https://github.github.com/gh-aw/reference/integrity/
    sections: ["Security Boundaries"]
    coverage_gaps:  # Features from this page NOT yet documented locally
      - "endorsement-reactions (v0.68.2+)"
      - "approval-labels for promoting items"

  # Tracked GitHub issues (check open/closed state)
  - issue: github/gh-aw#18481
    resolution_expected: true

  # GitHub releases (check for new versions)
  - releases: github/gh-aw

style: |
  Match existing section structure. Use tables for feature comparisons.
```

### Source fields

| Field | Required | Description |
|-------|----------|-------------|
| `url` | Yes (for web sources) | Reference page URL — script hashes content to detect changes |
| `sections` | No | Which local sections this source informs — helps the agent scope updates |
| `coverage_gaps` | No | Features from this page NOT yet documented locally — the agent should check these first when the page hash changes |
| `issue` | Yes (for issue sources) | Format: `owner/repo#number` — script checks open/closed state |
| `resolution_expected` | No | When `true` and issue closes, `changes_detected` is flagged |
| `releases` | Yes (for release sources) | Format: `owner/repo` — script checks latest release tag |

## Important Constraints

- **Never auto-edit instruction files.** Always produce a drift report for human review.
- **Respect `divergence:` sections.** These contain hard-won operational knowledge that doesn't come from upstream docs. Never suggest removing this content.
- **Report fetch failures loudly.** A 404 on a tracked URL is a signal that the manifest needs updating, not that nothing changed.
- **Distinguish mechanical vs. judgment changes.** A closed issue is a mechanical fact. A restructured docs page requires human judgment about what to update.
