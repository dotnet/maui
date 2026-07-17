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
      2. For each, gather its linked issues (Fixes/Closes #N), their
         `regressed-in-*` labels, and a bounded number of comments. Only text
         authored by a repository maintainer is eligible for attribution.
      3. Regex-extract the *introducing* PR number from that trusted text — the
         PR that shipped the regression ("regression from #N", "introduced by
         #N", etc.).
         That introducing PR's merge commit is the frozen `ref:` a Vally
         regression stimulus pins to (the reviewer is tested on the bad diff cold).
      4. Resolve the introducing PR's merge SHA + changed files via `gh`.
      5. Drop candidates already covered by the corpus or pending scanner drafts
         (existing `regression_pr:` tags). Candidates whose introducing PR cannot
         be resolved are flagged needsHumanAttribution — no SHA means no hermetic ref.
      6. Emit bounded candidate context. Unresolved candidates do not consume the
         usable-candidate limit.

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
    Cap on usable candidates emitted (rate-limit / blast-radius guard). Unresolved
    candidates remain bounded by the source-query limit but do not consume this cap.
    Default 5.

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

function ConvertTo-GitHubNumber {
    # GitHub issue and PR numbers are positive Int32 values. Treat malformed or
    # out-of-range numeric-looking text as non-references rather than aborting a run.
    param([string]$Value)
    [void]([int]$number = 0)
    if ([int]::TryParse(
            $Value,
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$number) -and $number -gt 0) {
        return $number
    }
    return
}

function Test-IsRegressionLabel {
    # A label that marks a PR or issue as regression-related. `i/regression` is the
    # definitive fix-PR signal; `regressed-in-*` carries issue-side release context.
    param([string]$Label)
    return $Label -match '\A(?:i/regression|regressed-in-[0-9A-Za-z][0-9A-Za-z./\-]*)\z'
}

function Test-IsTrustedAssociation {
    # Attribution from editable text must come from a repository maintainer.
    param([string]$Association)
    if (-not $Association) { return $false }
    return $Association.Trim().ToUpperInvariant() -in @('OWNER', 'MEMBER', 'COLLABORATOR')
}

function Get-RegressedInLabels {
    # Keep the only label text emitted to the agent within the version-label grammar.
    param([string[]]$Labels)
    return @(
        $Labels |
            Where-Object {
                (Test-IsRegressionLabel $_) -and $_ -like 'regressed-in-*'
            }
    )
}

function Get-LinkedIssueNumbers {
    # Issues a PR closes in the scanned repository. Cap expansion before issue
    # contexts are fetched to keep the deterministic pre-pass bounded.
    param(
        [string]$PRBody,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui',
        [ValidateRange(1, 100)][int]$MaxIssues = 10
    )
    if (-not $PRBody) { return @() }
    if ($PRBody -is [array]) { $PRBody = $PRBody -join "`n" }
    $normalized = $PRBody -replace "`r`n", "`n"
    $set = New-Object 'System.Collections.Generic.HashSet[int]'
    $issueUrl = 'https://github\.com/{0}/{1}/issues/' -f
        [regex]::Escape($Owner), [regex]::Escape($Repo)

    $patterns = @(
        ('(?i)(?:Fix(?:es|ed)?|Close[sd]?|Resolve[sd]?)\s+(?:(?:{0})|#)(\d+)(?=$|[\s)\]\x7D.,;:!?#])' -f $issueUrl),
        '(?m)^\s*-\s+#(\d+)\s*$',
        ('(?m)^\s*-\s+{0}(\d+)\s*$' -f $issueUrl)
    )
    foreach ($pat in $patterns) {
        foreach ($m in [regex]::Matches($normalized, $pat)) {
            $number = ConvertTo-GitHubNumber $m.Groups[1].Value
            if ($null -ne $number) {
                [void]$set.Add($number)
            }
        }
    }
    return @($set | Sort-Object | Select-Object -First $MaxIssues)
}

function Get-IntroducingPrReferences {
    # Extracts the PR number(s) that INTRODUCED a regression from free text.
    # Anchored on regression-attribution phrasing so it does not match generic
    # "fixes #N" issue references. Returns distinct ints in first-seen order.
    param([string]$Text)
    if (-not $Text) { return @() }
    if ($Text -is [array]) { $Text = $Text -join "`n" }

    # Only canonical local pull URLs are attribution references. A Markdown PR
    # link must target the local repository, and the boundary rejects malformed
    # URLs before the number can be resolved locally.
    $localPullUrl = 'https://github\.com/dotnet/maui/pull/(?<number>\d+)(?=$|[/?#\s)\]\}.,;:])'
    $reference = '(?:(?:PR\s*#?|#)(?<number>\d+)|\[(?:PR\s*#?|#)\d+\]\(' +
        $localPullUrl + '|\[?' + $localPullUrl + ')'
    $patterns = @(
        ('(?i)regress(?:ion|ed)?\s+(?:from|in|introduced\s+in|caused\s+by)\s+{0}' -f $reference),
        ('(?i)introduced\s+(?:by|in)\s+{0}' -f $reference),
        ('(?i)caused\s+by\s+{0}' -f $reference),
        ('(?i)broke(?:n)?\s+(?:in|by)\s+{0}' -f $reference),
        ('(?i)regress(?:ed|ion)\s+(?:by)\s+{0}' -f $reference)
    )

    $matches = New-Object System.Collections.Generic.List[object]
    for ($patternIndex = 0; $patternIndex -lt $patterns.Count; $patternIndex++) {
        foreach ($m in [regex]::Matches($Text, $patterns[$patternIndex])) {
            $matches.Add([PSCustomObject]@{
                    Index        = $m.Index
                    PatternIndex = $patternIndex
                    Value        = $m.Groups['number'].Value
                }) | Out-Null
        }
    }

    $ordered = New-Object System.Collections.Generic.List[int]
    $seen = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($match in @($matches | Sort-Object Index, PatternIndex)) {
        $number = ConvertTo-GitHubNumber $match.Value
        if ($null -ne $number -and $seen.Add($number)) {
            [void]$ordered.Add($number)
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
        $number = ConvertTo-GitHubNumber $m.Groups[1].Value
        if ($null -ne $number) {
            [void]$set.Add($number)
        }
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

function Test-HumanAttributionCandidateIsNew {
    # Keep one human-attribution note per known introducing PR without preventing
    # a later fully usable candidate from adding that regression to the corpus.
    param(
        [Nullable[int]]$IntroducingPr,
        [int[]]$ExistingHumanAttributionNumbers
    )
    if ($null -eq $IntroducingPr) { return $true }
    return ($IntroducingPr -notin @($ExistingHumanAttributionNumbers))
}

function Get-UsableCandidateCount {
    param([System.Collections.IEnumerable]$Candidates)
    $count = 0
    foreach ($candidate in $Candidates) {
        if ($candidate -and -not $candidate.needsHumanAttribution) {
            $count++
        }
    }
    return $count
}

# ─── gh I/O wrappers (mocked in tests) ─────────────────────────────────────────

function Invoke-GhJson {
    param(
        [Parameter(Mandatory = $true)][string[]]$GhArgs,
        [switch]$AllowFailure
    )
    $raw = & gh @GhArgs 2>$null
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $message = "GitHub CLI command failed with exit code ${exitCode}: $($GhArgs[0])"
        if ($AllowFailure) {
            Write-Warning "$message; treating the requested GitHub data as unavailable."
            return $null
        }
        throw $message
    }
    if (-not $raw) { return $null }
    if ($raw -is [array]) { $raw = $raw -join "`n" }
    try {
        return $raw | ConvertFrom-Json
    }
    catch {
        $message = "GitHub CLI returned invalid JSON for $($GhArgs[0]): $($_.Exception.Message)"
        if ($AllowFailure) {
            Write-Warning "$message; treating the requested GitHub data as unavailable."
            return $null
        }
        throw $message
    }
}

function Get-MergedRegressionFixPRs {
    param([string]$Owner, [string]$Repo, [int]$LookbackDays, [int]$Limit)
    $since = (Get-Date).ToUniversalTime().AddDays(-[Math]::Abs($LookbackDays)).ToString('yyyy-MM-dd')
    $search = "is:merged label:i/regression merged:>=$since sort:created-asc"
    $prs = Invoke-GhJson -GhArgs @(
        'pr', 'list', '--repo', "$Owner/$Repo",
        '--state', 'merged', '--search', $search, '--limit', "$Limit",
        '--json', 'number,body,mergeCommit,mergedAt'
    )
    return @($prs | Where-Object { $null -ne $_ } | Sort-Object mergedAt, number)
}

function Get-IssueAuthorAssociation {
    # REST exposes author_association for both issues and PRs (which are issues).
    param([string]$Owner, [string]$Repo, [int]$Number)
    $issue = Invoke-GhJson -GhArgs @(
        'api', "repos/$Owner/$Repo/issues/$Number"
    ) -AllowFailure
    if (-not $issue) { return $null }
    return [string]$issue.author_association
}

function Get-IssueContext {
    param([string]$Owner, [string]$Repo, [int]$Number, [int]$MaxComments = 20)
    $issue = Invoke-GhJson -GhArgs @(
        'api', "repos/$Owner/$Repo/issues/$Number"
    ) -AllowFailure
    if (-not $issue) { return $null }
    $commentText = ''
    $comments = Invoke-GhJson -GhArgs @(
        'api', "repos/$Owner/$Repo/issues/$Number/comments?per_page=$MaxComments"
    ) -AllowFailure
    $trustedCommentBodies = @(
        $comments |
            Where-Object {
                $_ -and
                (Test-IsTrustedAssociation ([string]$_.author_association))
            } |
            ForEach-Object { [string]$_.body } |
            Where-Object { $_ }
    )
    if ($trustedCommentBodies) { $commentText = $trustedCommentBodies -join "`n" }
    return [PSCustomObject]@{
        Number      = $issue.number
        Labels      = @($issue.labels | ForEach-Object { $_.name } | Where-Object { $_ })
        Body        = [string]$issue.body
        CommentText = $commentText
        IsTrustedAttribution = Test-IsTrustedAssociation ([string]$issue.author_association)
    }
}

function Get-OpenRegressionCorpusPrTags {
    # Draft corpus PRs are not in main yet, so include their tags in deduplication.
    param([string]$Owner, [string]$Repo)
    $openPrs = Invoke-GhJson -GhArgs @(
        'pr', 'list', '--repo', "$Owner/$Repo",
        '--state', 'open', '--label', 'agentic-workflows', '--limit', '100',
        '--json', 'headRefName'
    )

    $tags = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($pr in @($openPrs | Where-Object { $_.headRefName -like 'regression-corpus/*' })) {
        $ref = [uri]::EscapeDataString([string]$pr.headRefName)
        $file = Invoke-GhJson -GhArgs @(
            'api',
            "repos/$Owner/$Repo/contents/.github/skills/code-review/tests/eval.vally.yaml?ref=$ref"
        )
        if (-not $file -or $file.encoding -ne 'base64' -or -not $file.content) {
            continue
        }

        $text = [Text.Encoding]::UTF8.GetString(
            [Convert]::FromBase64String(([string]$file.content -replace '\s', '')))
        foreach ($number in (Get-RegressionPrTagsFromText $text)) {
            [void]$tags.Add($number)
        }
    }
    return @($tags)
}

function Get-ExistingRegressionPrTags {
    # An unreadable existing corpus file must abort rather than silently allow a
    # duplicate draft. A glob with no matches remains a valid empty corpus.
    param([string]$CorpusGlob)
    $tags = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($file in (Get-ChildItem -Path $CorpusGlob -ErrorAction SilentlyContinue)) {
        $text = Get-Content -Raw -LiteralPath $file.FullName -ErrorAction Stop
        foreach ($number in (Get-RegressionPrTagsFromText $text)) {
            [void]$tags.Add($number)
        }
    }
    return @($tags)
}

function Get-IntroducingPrDetails {
    param([string]$Owner, [string]$Repo, [int]$Number)
    $pr = Invoke-GhJson -GhArgs @(
        'pr', 'view', "$Number", '--repo', "$Owner/$Repo",
        '--json', 'number,mergeCommit'
    ) -AllowFailure
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
    # Materialize the typed list to an array so the candidate has a stable, immutable
    # collection shape for JSON serialization.
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
        needsHumanAttribution    = $NeedsHumanAttribution -or $RegressionIssues.Count -eq 0
    }
}

# ─── Main ──────────────────────────────────────────────────────────────────────

$authCheck = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "::error::GitHub CLI not authenticated: $authCheck"
    throw 'gh auth required'
}

Write-Host "Scanning $Owner/$Repo for regression-fix PRs merged in the last $LookbackDays day(s)..."

# Dedup set: regression_pr tags already present in the corpus or pending scanner drafts.
$existingNumbers = New-Object 'System.Collections.Generic.HashSet[int]'
foreach ($n in (Get-ExistingRegressionPrTags -CorpusGlob $CorpusGlob)) {
    [void]$existingNumbers.Add($n)
}
foreach ($n in (Get-OpenRegressionCorpusPrTags -Owner $Owner -Repo $Repo)) {
    [void]$existingNumbers.Add($n)
}
Write-Host "Corpus already covers regression_pr: $(@($existingNumbers) -join ', ')"

$fixPRs = @(Get-MergedRegressionFixPRs -Owner $Owner -Repo $Repo -LookbackDays $LookbackDays -Limit ([Math]::Max($MaxPRs * 4, 20)))
Write-Host "Found $($fixPRs.Count) merged i/regression PR(s) in window."

$candidates = New-Object System.Collections.Generic.List[object]
$humanAttributionNumbers = New-Object 'System.Collections.Generic.HashSet[int]'

foreach ($pr in $fixPRs) {
    if ((Get-UsableCandidateCount -Candidates $candidates) -ge $MaxPRs) { break }

    $fixNumber = [int]$pr.number
    $fixPrAssociation = Get-IssueAuthorAssociation -Owner $Owner -Repo $Repo -Number $fixNumber
    $linkedIssues = Get-LinkedIssueNumbers -PRBody $pr.body -Owner $Owner -Repo $Repo

    # Keep only maintainer-authored editable bodies as attribution. The fix body
    # still drives linked-issue discovery above, regardless of author association.
    $commentSources = New-Object System.Collections.Generic.List[object]
    $bodySources = New-Object System.Collections.Generic.List[object]
    if (Test-IsTrustedAssociation $fixPrAssociation) {
        $bodySources.Add([PSCustomObject]@{ Source = 'pr-body'; Text = [string]$pr.body }) | Out-Null
    }

    $regressionIssues = New-Object System.Collections.Generic.List[object]
    foreach ($issueNum in $linkedIssues) {
        $ctx = Get-IssueContext -Owner $Owner -Repo $Repo -Number $issueNum
        if (-not $ctx) { continue }
        $regressedIn = @(Get-RegressedInLabels $ctx.Labels)
        $regressionIssues.Add([PSCustomObject]@{
            number            = $ctx.Number
            regressedInLabels = $regressedIn
        }) | Out-Null
        if ($ctx.CommentText) {
            $commentSources.Add([PSCustomObject]@{ Source = 'issue-comment'; Text = $ctx.CommentText }) | Out-Null
        }
        if ($ctx.IsTrustedAttribution) {
            $bodySources.Add([PSCustomObject]@{ Source = 'issue-body'; Text = $ctx.Body }) | Out-Null
        }
    }

    # An explicit trusted maintainer comment takes precedence over editable bodies.
    $attribSources = New-Object System.Collections.Generic.List[object]
    foreach ($source in $commentSources) {
        $attribSources.Add($source) | Out-Null
    }
    foreach ($source in $bodySources) {
        $attribSources.Add($source) | Out-Null
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

    $needsHuman = $regressionIssues.Count -eq 0 -or (-not $introDetails) -or (-not $introDetails.MergeCommit)
    if ($needsHuman -and -not (Test-HumanAttributionCandidateIsNew `
                -IntroducingPr $introducingPr `
                -ExistingHumanAttributionNumbers @($humanAttributionNumbers))) {
        Write-Host "  ⏭️ PR #$fixNumber → introducing #$introducingPr already pending human attribution; skipping."
        continue
    }

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
    if (-not $candidate.needsHumanAttribution -and $null -ne $candidate.introducingPr) {
        [void]$existingNumbers.Add([int]$candidate.introducingPr)
    }
    elseif ($null -ne $candidate.introducingPr) {
        [void]$humanAttributionNumbers.Add([int]$candidate.introducingPr)
    }

    Write-Host "  ✅ Candidate: fix #$fixNumber → introducing #$introducingPr (ref $($introDetails.MergeCommit)) needsHuman=$needsHuman"
}

$usableCandidateCount = Get-UsableCandidateCount -Candidates $candidates
$payload = [PSCustomObject]@{
    generatedAt  = (Get-Date).ToUniversalTime().ToString('o')
    owner        = $Owner
    repo         = $Repo
    lookbackDays = $LookbackDays
    count        = $candidates.Count
    usableCount  = $usableCandidateCount
    candidates   = $candidates.ToArray()
}

$dir = Split-Path -Parent $OutputPath
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
($payload | ConvertTo-Json -Depth 8) | Set-Content -LiteralPath $OutputPath -Encoding UTF8
Write-Host "Wrote $($candidates.Count) candidate(s) to $OutputPath"
