---
description: |
  Analyzes merged pull requests for documentation needs. When a PR is merged,
  this workflow reviews the changes and determines if documentation updates are
  required on dotnet/docs-maui. If so, it creates a tracking issue and comments
  on the PR with a link.

on:
  pull_request:
    types: [closed]
    branches:
      - main
      - net*.0
      - release/*
  workflow_dispatch:
    inputs:
      pr_number:
        description: "PR number to analyze"
        required: true
        type: string

if: >-
  (github.event.pull_request.merged == true || github.event_name == 'workflow_dispatch')
  && github.repository_owner == 'dotnet'

permissions:
  contents: read
  pull-requests: read
  issues: read

network:
  allowed:
    - defaults
    - github

tools:
  github:
    toolsets: [repos, issues, pull_requests]
    app:
      app-id: ${{ vars.DOCS_APP_ID }}
      private-key: ${{ secrets.DOCS_APP_PRIVATE_KEY }}
      owner: "dotnet"
      repositories: ["docs-maui"]
  web-fetch:

safe-outputs:
  create-issue:
    title-prefix: "[docs] "
    labels: [docs-from-code]
    target-repo: "dotnet/docs-maui"
    expires: false
  add-comment:
    hide-older-comments: true

timeout-minutes: 15
---

# PR Documentation Check

Analyze a merged pull request in `dotnet/maui` and determine whether documentation
updates are needed on the `dotnet/docs-maui` documentation site.

## Context

- **Repository**: `${{ github.repository }}`
- **PR Number**: `${{ github.event.pull_request.number || github.event.inputs.pr_number }}`
- **PR Title**: `${{ github.event.pull_request.title }}`

## Step 1: Gather PR Information

Use the GitHub tools to read the full pull request details for the PR number above,
including the title, description, author, base branch, and the full diff of changes.
Pay special attention to the **base branch** and the **PR author** username, as both
are needed in later steps.

If this was triggered via `workflow_dispatch`, use the `pr_number` input to look up
the PR details.

## Step 2: Quick Filter — Skip Obviously Non-Doc PRs

Before doing a deep analysis, check if this PR can be **skipped entirely**:

**Skip if ALL of these are true:**
- PR author is a bot (`dotnet-maestro[bot]`, `dependabot[bot]`, `github-actions[bot]`)
- OR PR has a `backport` label (or any label starting with `backport/`)

**Skip if ALL changed files** fall into these categories (no other files changed):
- Build infrastructure: `eng/`, `.github/`, `build.*`, `*.yml`, `*.yaml`,
  `*.props`, `*.targets`, `Directory.Build.*`, `NuGet.config`, `global.json`, `tools/`
- Tests only: paths containing `/tests/`, `/TestUtils/`, `/TestCases.`,
  `/DeviceTests/`, or files with `.Tests.`, `.UnitTests.`, `.DeviceTests.` in name
- Documentation/meta: `*.md`, `docs/`, `LICENSE*`, `THIRD-PARTY*`

If the PR is skipped, comment on the PR with a brief message confirming no
documentation updates are required and the reason (e.g., "bot PR", "test-only
changes"), then stop.

**Always proceed with analysis if:**
- Any `PublicAPI.Unshipped.txt` file was changed (strong signal of API changes)
- Any file under `src/Templates/` was changed (affects getting-started docs)

## Step 3: Analyze Changes for Documentation Needs

Review the PR diff and metadata for the following types of user-facing changes that
warrant documentation:

- **New public APIs**: new methods, classes, interfaces, bindable properties,
  attached properties, or XAML markup extensions
- **New features or capabilities**: new controls, layouts, platform behaviors,
  or Essentials APIs
- **Breaking changes**: removed or renamed APIs, behavioral changes, migration needs
- **New configuration options**: MSBuild properties, platform settings, or parameters
- **Platform-specific features**: new Android, iOS, macCatalyst, or Windows behaviors
- **Template changes**: changes to project templates affecting new project creation
- **Significant behavioral changes**: changes to navigation, layout, rendering,
  gestures, or lifecycle

Changes that do NOT typically need documentation:
- Internal refactoring with no public API surface changes
- Test-only changes
- Build/CI infrastructure changes
- Bug fixes that don't change documented behavior
- Dependency version bumps
- Code style or formatting changes

## Step 4: Check Existing Documentation

If you determine documentation may be needed, use the GitHub tools to browse the
`dotnet/docs-maui` repository to:

- Identify existing documentation pages that cover the affected feature area
- Determine whether existing pages need updates or new pages should be created
- Understand the current documentation structure and naming conventions

## Step 5: Take Action

### If documentation IS needed:

1. **Create an issue** on `dotnet/docs-maui` with:
   - A clear title describing the documentation work needed
   - A structured body that includes:
     - Link to the source PR in `dotnet/maui`
     - PR author mention
     - Whether to UPDATE existing pages or CREATE new pages
     - Specific file paths in the `docs-maui` repo that need updating (if applicable)
     - Detailed description of what sections need to be created or updated
     - Key points that must be covered
     - Suggested code examples (with placeholders if needed)
     - Platform-specific documentation needs (if applicable)
     - Cross-references to related documentation pages

2. **Comment on the PR** in `dotnet/maui` with:
   - A summary indicating documentation updates are needed
   - A link to the created docs issue
   - A brief description of what documentation changes are recommended

### If documentation is NOT needed:

1. **Comment on the PR** in `dotnet/maui` with:
   - A brief message confirming no documentation updates are required
   - A short explanation of why (e.g., "internal refactoring only",
     "bug fix with no behavioral change")
