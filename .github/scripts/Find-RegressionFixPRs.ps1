#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds bounded, deterministic context for the regression-corpus scanner:
    recently merged regression-fix PRs whose introducing commit can seed a new
    code-review eval stimulus.

.DESCRIPTION
    Purely mechanical (no AI / LLM). The companion gh-aw workflow
    (regression-corpus-scanner.md) consumes the emitted candidates.json and asks
    the agent to author a hermetic `eval.vally.yaml` stimulus for each candidate.

    Pipeline:
      1. List PRs merged in the last -LookbackDays that carry `i/regression`
         (the definitive "this fixed a regression" signal on the fix PR).
      2. For each, gather the fix PR body + every linked issue (Fixes/Closes #N)
         with that issue's labels (to capture `regressed-in-*`) and a bounded
         number of issue comments.
      3. Regex-extract the *introducing* PR number from that text — the PR that
         shipped the regression ("regression from #N", "introduced by #N", etc.).
         That introducing PR's merge commit is the frozen `ref:` a Vally
         regression stimulus pins to (the reviewer is tested on the bad diff cold).
      4. Resolve the introducing PR's merge SHA + changed files via `gh`.
      5. Drop candidates already covered by the corpus (existing `regression_pr:`
         tags in any eval.vally.yaml) and those whose introducing PR cannot be
         resolved (flagged needsHumanAttribution — no SHA means no hermetic ref).
      6. Emit a bounded candidates.json.

    Hermeticity note: this scanner reads LIVE PR/issue data — that is expected and
    fine. Hermeticity applies to the eval it ultimately emits (frozen worktree, no
    PR/issue numbers in the stimulus prompt), not to the scanner itself.

.PARAMETER Owner
    Repository owner. Default 'dotnet'.

.PARAMETER Repo
    Repository name. Default 'maui'.

.PARAMETER LookbackDays
    How many days back to scan for merged regression-fix PRs. Default 14.

.PARAMETER MaxPRs
    Cap on candidates emitted (rate-limit / blast-radius guard). Default 5.

.PARAMETER CorpusGlob
    Glob for existing Vally eval specs used for dedup. Default
    '.github/skills/*/tests/*.vally.yaml'.

.PARAMETER OutputPath
    Where to write candidates.json.

.EXAMPLE
    pwsh .github/scripts/Find-RegressionFixPRs.ps1 -LookbackDays 14 -MaxPRs 5 `
        -OutputPath CustomAgentLogsTmp/RegressionCorpusScanner/candidates.json
#>

[CmdletBinding()]
param(
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [int]$LookbackDays = 14,
    [int]$MaxPRs = 5,
    [string]$CorpusGlob = '.github/skills/*/tests/*.vally.yaml',
    [string]$OutputPath = 'CustomAgentLogsTmp/RegressionCorpusScanner/candidates.json'
)

$ErrorActionPreference = 'Stop'

# ─── Pure helpers (unit-tested via AST extraction; no network/side effects) ────

function Test-IsRegressionLabel {
    # A label that marks a PR or issue as regression-related. `i/regression` is the
    # definitive fix-PR signal; `regressed-in-<version>` is the issue-side signal.
    param([string]$Label)
    return $Label -match '^(i/regression|regressed-in-[0-9][0-9A-Za-z.\-]*)$'
}

function Get-LinkedIssueNumbers {
    # Issues a PR closes (Fixes/Closes/Resolves #N or bullet-list references).
    # Regex mirrors Find-RegressionRisks.ps1 for consistency across the repo.
    param([string]$PRBody)
    if (-not $PRBody) { return @() }
    if ($PRBody -is [array]) { $PRBody = $PRBody -join "`n" }
    $normalized = $PRBody -replace "`r`n", "`n"
    $set = New-Object 'System.Collections.Generic.HashSet[int]'

    $patterns = @(
        '(?i)(?:Fix(?:es|ed)?|Close[sd]?|Resolve[sd]?)\s+(?:https://github\.com/[^/]+/[^/]+/issues/)?#?(\d+)',
        '(?m)^\s*-\s+#(\d+)\s*$',
        '(?m)^\s*-\s+https://github\.com/[^/]+/[^/]+/issues/(\d+)\s*$'
    )
    foreach ($pat in $patterns) {
        foreach ($m in [regex]::Matches($normalized, $pat)) {
            [void]$set.Add([int]$m.Groups[1].Value)
        }
    }
    return @($set)
}

function Get-IntroducingPrReferences {
    # Extracts the PR number(s) that INTRODUCED a regression from free text.
    # Anchored on regression-attribution phrasing so it does not match generic
    # "fixes #N" issue references. Returns distinct ints in first-seen order.
    param([string]$Text)
    if (-not $Text) { return @() }
    if ($Text -is [array]) { $Text = $Text -join "`n" }

    $patterns = @(
        '(?i)regress(?:ion|ed)?\s+(?:from|in|introduced\s+in|caused\s+by)\s+(?:PR\s*#?|#)(\d+)',
        '(?i)introduced\s+(?:by|in)\s+(?:PR\s*#?|#)(\d+)',
        '(?i)caused\s+by\s+(?:PR\s*#?|#)(\d+)',
        '(?i)broke(?:n)?\s+(?:in|by)\s+(?:PR\s*#?|#)(\d+)',
        '(?i)regress(?:ed|ion)\s+(?:by)\s+(?:PR\s*#?|#)(\d+)'
    )

    $ordered = New-Object System.Collections.Generic.List[int]
    $seen = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($pat in $patterns) {
        foreach ($m in [regex]::Matches($Text, $pat)) {
            $n = [int]$m.Groups[1].Value
            if ($seen.Add($n)) { [void]$ordered.Add($n) }
        }
    }
    return @($ordered)
}

function Get-RegressionPrTagsFromText {
    # Extracts existing `regression_pr:` tag values from an eval.vally.yaml's text.
    # Used for dedup so the scanner never re-proposes a regression already covered.
    param([string]$Text)
    if (-not $Text) { return @() }
    if ($Text -is [array]) { $Text = $Text -join "`n" }
    $set = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($m in [regex]::Matches($Text, '(?im)^\s*regression_pr:\s*"?(\d+)"?')) {
        [void]$set.Add([int]$m.Groups[1].Value)
    }
    return @($set)
}

function Test-CandidateIsNew {
    # A candidate is new when its introducing PR is resolved and not already in the
    # corpus, and the fix PR itself is not already covered.
    param(
        [Nullable[int]]$IntroducingPr,
        [int]$FixPr,
        [int[]]$ExistingNumbers
    )
    $existing = @($ExistingNumbers)
    if ($FixPr -in $existing) { return $false }
    if ($null -eq $IntroducingPr) { return $true }
    return ($IntroducingPr -notin $existing)
}

# ─── gh I/O wrappers (mocked in tests) ─────────────────────────────────────────

function Invoke-GhJson {
    param([Parameter(Mandatory = $true)][string[]]$GhArgs)
    $raw = & gh @GhArgs 2>$null
    if (-not $raw) { return $null }
    if ($raw -is [array]) { $raw = $raw -join "`n" }
    try { return $raw | ConvertFrom-Json } catch { return $null }
}

function Get-MergedRegressionFixPRs {
    param([string]$Owner, [string]$Repo, [int]$LookbackDays, [int]$Limit)
    $since = (Get-Date).ToUniversalTime().AddDays(-[Math]::Abs($LookbackDays)).ToString('yyyy-MM-dd')
    $search = "is:merged label:i/regression merged:>=$since"
    $prs = Invoke-GhJson -GhArgs @(
        'pr', 'list', '--repo', "$Owner/$Repo",
        '--state', 'merged', '--search', $search, '--limit', "$Limit",
        '--json', 'number,body,mergeCommit'
    )
    return @($prs | Where-Object { $null -ne $_ })
}

function Get-IssueContext {
    param([string]$Owner, [string]$Repo, [int]$Number, [int]$MaxComments = 20)
    $issue = Invoke-GhJson -GhArgs @(
        'issue', 'view', "$Number", '--repo', "$Owner/$Repo",
        '--json', 'number,body,labels'
    )
    if (-not $issue) { return $null }
    $commentText = ''
    $comments = Invoke-GhJson -GhArgs @(
        'api', "repos/$Owner/$Repo/issues/$Number/comments?per_page=$MaxComments",
        '--jq', '[.[].body]'
    )
    if ($comments) { $commentText = (@($comments) -join "`n") }
    return [PSCustomObject]@{
        Number      = $issue.number
        Labels      = @($issue.labels | ForEach-Object { $_.name } | Where-Object { $_ })
        Body        = [string]$issue.body
        CommentText = $commentText
    }
}

function Get-IntroducingPrDetails {
    param([string]$Owner, [string]$Repo, [int]$Number)
    $pr = Invoke-GhJson -GhArgs @(
        'pr', 'view', "$Number", '--repo', "$Owner/$Repo",
        '--json', 'number,mergeCommit'
    )
    if (-not $pr) { return $null }
    $sha = if ($pr.mergeCommit -and $pr.mergeCommit.oid) { $pr.mergeCommit.oid } else { $null }
    return [PSCustomObject]@{
        Number      = $pr.number
        MergeCommit = $sha
    }
}

function New-RegressionCandidate {
    # Assembles normalized, structural candidate context for the agent. Fetched
    # titles and file paths are deliberately excluded so the prompt never embeds
    # untrusted prose or path text; the agent can inspect the introducing PR by ID.
    # IMPORTANT: regressionIssues is materialized with .ToArray(). Wrapping a generic
    # List[object] with @(...) inside a [PSCustomObject] literal throws "Argument
    # types do not match", so the naive @($regressionIssues) form is a trap — keep
    # .ToArray() here (see Find-RegressionFixPRs.Tests.ps1 for the regression test).
    param(
        [int]$FixPr,
        $FixPrMergeCommit,
        [System.Collections.Generic.List[object]]$RegressionIssues = (New-Object 'System.Collections.Generic.List[object]'),
        $IntroducingPr,
        $IntroDetails,
        $AttributionSource,
        [bool]$NeedsHumanAttribution
    )
    return [PSCustomObject]@{
        fixPr                    = $FixPr
        fixPrMergeCommit         = $FixPrMergeCommit
        regressionIssues         = $RegressionIssues.ToArray()
        introducingPr            = $IntroducingPr
        introducingPrMergeCommit = if ($IntroDetails) { $IntroDetails.MergeCommit } else { $null }
        attributionSource        = $AttributionSource
        needsHumanAttribution    = $NeedsHumanAttribution
    }
}

# ─── Main ──────────────────────────────────────────────────────────────────────

$authCheck = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "::error::GitHub CLI not authenticated: $authCheck"
    throw 'gh auth required'
}

Write-Host "Scanning $Owner/$Repo for regression-fix PRs merged in the last $LookbackDays day(s)..."

# Dedup set: regression_pr tags already present in the corpus.
$existingNumbers = New-Object 'System.Collections.Generic.HashSet[int]'
foreach ($file in (Get-ChildItem -Path $CorpusGlob -ErrorAction SilentlyContinue)) {
    $text = Get-Content -Raw -LiteralPath $file.FullName -ErrorAction SilentlyContinue
    foreach ($n in (Get-RegressionPrTagsFromText $text)) { [void]$existingNumbers.Add($n) }
}
Write-Host "Corpus already covers regression_pr: $(@($existingNumbers) -join ', ')"

$fixPRs = @(Get-MergedRegressionFixPRs -Owner $Owner -Repo $Repo -LookbackDays $LookbackDays -Limit ([Math]::Max($MaxPRs * 4, 20)))
Write-Host "Found $($fixPRs.Count) merged i/regression PR(s) in window."

$candidates = New-Object System.Collections.Generic.List[object]

foreach ($pr in $fixPRs) {
    if ($candidates.Count -ge $MaxPRs) { break }

    $fixNumber = [int]$pr.number
    $linkedIssues = Get-LinkedIssueNumbers $pr.body

    # Accumulate attribution text: fix PR body + linked issue bodies/comments.
    $attribSources = New-Object System.Collections.Generic.List[object]
    $attribSources.Add([PSCustomObject]@{ Source = 'pr-body'; Text = [string]$pr.body }) | Out-Null

    $regressionIssues = New-Object System.Collections.Generic.List[object]
    foreach ($issueNum in $linkedIssues) {
        $ctx = Get-IssueContext -Owner $Owner -Repo $Repo -Number $issueNum
        if (-not $ctx) { continue }
        $regressedIn = @($ctx.Labels | Where-Object { $_ -match '^regressed-in-' })
        $regressionIssues.Add([PSCustomObject]@{
            number            = $ctx.Number
            regressedInLabels = $regressedIn
        }) | Out-Null
        $attribSources.Add([PSCustomObject]@{ Source = 'issue-body'; Text = $ctx.Body }) | Out-Null
        $attribSources.Add([PSCustomObject]@{ Source = 'issue-comment'; Text = $ctx.CommentText }) | Out-Null
    }

    # Find the introducing PR reference, excluding the fix PR and its linked issues.
    $introducingPr = $null
    $attributionSource = $null
    foreach ($entry in $attribSources) {
        $refs = @(Get-IntroducingPrReferences $entry.Text | Where-Object {
            $_ -ne $fixNumber -and $_ -notin $linkedIssues
        })
        if ($refs.Count -gt 0) {
            $introducingPr = [int]$refs[0]
            $attributionSource = $entry.Source
            break
        }
    }

    if (-not (Test-CandidateIsNew -IntroducingPr $introducingPr -FixPr $fixNumber -ExistingNumbers @($existingNumbers))) {
        Write-Host "  ⏭️ PR #$fixNumber → introducing #$introducingPr already in corpus; skipping."
        continue
    }

    $introDetails = $null
    if ($introducingPr) {
        $introDetails = Get-IntroducingPrDetails -Owner $Owner -Repo $Repo -Number $introducingPr
    }

    $needsHuman = (-not $introDetails) -or (-not $introDetails.MergeCommit)

    $fixMergeOid = if ($pr.mergeCommit) { $pr.mergeCommit.oid } else { $null }
    $candidate = New-RegressionCandidate `
        -FixPr $fixNumber `
        -FixPrMergeCommit $fixMergeOid `
        -RegressionIssues $regressionIssues `
        -IntroducingPr $introducingPr `
        -IntroDetails $introDetails `
        -AttributionSource $attributionSource `
        -NeedsHumanAttribution $needsHuman
    $candidates.Add($candidate) | Out-Null

    Write-Host "  ✅ Candidate: fix #$fixNumber → introducing #$introducingPr (ref $($introDetails.MergeCommit)) needsHuman=$needsHuman"
}

$payload = [PSCustomObject]@{
    generatedAt  = (Get-Date).ToUniversalTime().ToString('o')
    owner        = $Owner
    repo         = $Repo
    lookbackDays = $LookbackDays
    count        = $candidates.Count
    candidates   = $candidates.ToArray()
}

$dir = Split-Path -Parent $OutputPath
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
($payload | ConvertTo-Json -Depth 8) | Set-Content -LiteralPath $OutputPath -Encoding UTF8
Write-Host "Wrote $($candidates.Count) candidate(s) to $OutputPath"
