---
name: maui-release-notes
description: Generate or update .NET MAUI workload release notes documentation. Creates well-formatted markdown with workload versions, platform dependencies (Xcode, JDK, Android SDK), and installation instructions. Use when asked to generate/create/update MAUI release notes, document current workload versions, check for new workload releases, or understand the release notes format.
---

# MAUI Release Notes Generator

Generate formatted markdown for .NET MAUI workload releases using live NuGet data from the `dotnet-workload-info` skill.

## File Structure

```
release-notes/
├── maui-release-notes.md              # Index page with all releases
└── maui-release-notes-YYYYMMDD.md     # Dated release notes (one per update)
```

## Workflow

### Step 1: Gather workload data
Use `dotnet-workload-info` skill to fetch live data for the **two most recent .NET versions** (e.g., .NET 10 and .NET 9):
- Workload set versions (NuGet + CLI format)
- Individual workload versions (MAUI, iOS, Mac Catalyst, Android, tvOS, macOS)
- MAUI NuGet package versions (implicit from workload vs latest on NuGet)
- Apple dependencies (Xcode version, SDK)
- Android dependencies (JDK, SDK packages)

### Step 2: Check for version changes
Compare fetched data against most recent `maui-release-notes-YYYYMMDD.md`:

```bash
ls -1 release-notes/maui-release-notes-*.md | sort -r | head -1
```

Key versions to compare per .NET version:
- Workload set version
- MAUI version
- iOS version  
- Android version

**If any changed → proceed. If unchanged → report "versions are up to date".**

### Step 3: Generate dated release notes
Create `release-notes/maui-release-notes-{YYYYMMDD}.md` using templates from [references/templates.md](references/templates.md).

For link construction (NuGet URLs, GitHub release tags), see [references/links.md](references/links.md).

### Step 4: Update index page
Add new entry at **top** of `release-notes/maui-release-notes.md`:
- Only include .NET version sections that had actual changes
- Update "Last Updated" date

## Index Entry Format

Each release entry includes:
1. Date heading (e.g., `### January 19, 2026`)
2. Link to full notes
3. Per-.NET version summary (only for versions with changes):
   - Workload set version
   - Workload versions table with Requirements column
   - MAUI NuGet packages (implicit vs latest)

**Requirements column:**
- Apple platforms: `Xcode ≥ {version}`
- Android: `API {level}, JDK {version}`
- MAUI: (empty)

## Detailed Templates

See [references/templates.md](references/templates.md) for:
- Complete index page template
- Full dated release notes template with all sections

## Link Construction

See [references/links.md](references/links.md) for:
- NuGet package URL patterns
- GitHub release tag formats (including Apple platform tag algorithm)
- Formatting conventions

## Inputs

| Parameter | Required | Default |
|-----------|----------|---------|
| dotnetVersions | no | Two most recent stable versions |
| includePrerelease | no | false |

## Output

**Versions changed:**
1. New `release-notes/maui-release-notes-{YYYYMMDD}.md`
2. Updated `release-notes/maui-release-notes.md` with new entry at top

**Versions current:** Informational message only

## Dependencies

Requires `dotnet-workload-info` skill for live NuGet data. Never use cached/hardcoded versions.
