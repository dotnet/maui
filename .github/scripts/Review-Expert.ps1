#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Trusted context, publication validation, and completion for /review expert-review.
.DESCRIPTION
    Never executes PR code. Analysis and GitHub publication run in separate jobs.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Gather', 'Snapshot', 'Prepare', 'Complete')]
    [string]$Stage,

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PRNumber,

    [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
    [string]$Repository = 'dotnet/maui',

    [string]$Actor = $env:GITHUB_ACTOR,
    [string]$OutputDirectory,
    [string]$ContextDirectory,
    [string]$ResultDirectory,
    [string]$AgentOutputPath
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/shared/Copy-BoundedDiagnosticFile.ps1"

function Write-ExpertJson {
    param([string]$Path, [AllowNull()][object]$Value)

    $parent = Split-Path -Parent $Path
    [void][IO.Directory]::CreateDirectory($parent)
    $json = ConvertTo-Json -InputObject $Value -Depth 50
    [IO.File]::WriteAllText($Path, "$json`n", [Text.UTF8Encoding]::new($false))
}

function Get-ExpertPublicationBody {
    param([string]$Body)

    # The native sanitizer requires an Actions logger; stdout is reserved for its JSON result.
    $script = @'
const fs = require("node:fs");
const log = message => process.stderr.write("[native-sanitizer] " + JSON.stringify(String(message)) + "\n");
global.core = { info: log, warning: log };
const { sanitizeContent } = require(process.argv[1]);
process.stdout.write(JSON.stringify(sanitizeContent(JSON.parse(fs.readFileSync(0, "utf8")).trim())));
'@
    $result = (ConvertTo-Json -InputObject $Body -Compress) |
        & node -e $script "$env:RUNNER_TEMP/gh-aw/actions/sanitize_content.cjs"
    if ($LASTEXITCODE -ne 0) { throw 'The pinned native publication sanitizer failed.' }
    $value = ConvertFrom-Json -InputObject ($result -join "`n") -NoEnumerate
    if ($value -isnot [string] -or [string]::IsNullOrWhiteSpace($value)) {
        throw 'The pinned native publication sanitizer returned an invalid finding.'
    }
    return $value
}

function Read-ExpertJson {
    param([string]$Path, [long]$MaxBytes = 16MB)

    $item = Get-Item -LiteralPath $Path
    if ($item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        $item.Length -gt $MaxBytes) {
        throw 'Review input must be a bounded regular JSON file.'
    }
    return ,(ConvertFrom-Json -InputObject ([IO.File]::ReadAllText($item.FullName)) -AsHashtable -NoEnumerate)
}

function Copy-ExpertInput {
    param([string]$Source, [string]$Destination, [long]$MaxBytes)

    $copy = Copy-BoundedDiagnosticFile -Source $Source -Destination $Destination -MaxBytes $MaxBytes
    if ($copy.Truncated) { throw 'Review input exceeded its publication limit; truncated JSON is not evidence.' }
    return $Destination
}

function Invoke-ExpertGh {
    param([string[]]$Arguments, [AllowNull()][object]$Body)

    $stderr = [IO.Path]::GetTempFileName()
    try {
        for ($attempt = 1; $attempt -le 3; $attempt++) {
            $raw = if ($null -ne $Body) {
                (ConvertTo-Json -InputObject $Body -Depth 50 -Compress) |
                    & gh api @Arguments --input - 2>$stderr
            } else {
                & gh api @Arguments 2>$stderr
            }
            $exitCode = $LASTEXITCODE
            if ($exitCode -eq 0) { break }
            $diagnostic = [IO.File]::ReadAllText($stderr)
            $http = [regex]::Match($diagnostic, '\(HTTP ([0-9]{3})\)')
            $httpStatus = if ($http.Success) { [int]$http.Groups[1].Value } else { 0 }
            $transport = -not $httpStatus -and $diagnostic -match
                '(?i)dial tcp|TLS handshake timeout|i/o timeout|connection reset by peer|unexpected EOF'
            $category = if ($httpStatus) {
                "HTTP $httpStatus"
            } elseif ($transport) { 'network transport failure' } else { 'unclassified CLI failure' }
            # Never echo raw API/CLI diagnostics: they can contain credentials or PR-controlled text.
            $retryable = $httpStatus -in @(502, 503, 504) -or $transport
            if ($null -eq $Body -and $retryable -and $attempt -lt 3) {
                Write-Warning "GitHub evidence read failed ($category; exit $exitCode); retrying attempt $($attempt + 1)/3."
                Start-Sleep -Seconds (2 * $attempt)
                continue
            }
            throw "GitHub API collection/publication failed for $($Arguments[0]) ($category; exit $exitCode; attempt $attempt)."
        }
        $text = $raw -join "`n"
        if ([string]::IsNullOrWhiteSpace($text)) { throw 'GitHub returned no JSON evidence.' }
        return ,(ConvertFrom-Json -InputObject $text -AsHashtable -NoEnumerate)
    } finally {
        Remove-Item -LiteralPath $stderr -Force
    }
}

function Get-ExpertPages {
    param([string]$Endpoint)

    $items = [Collections.Generic.List[object]]::new()
    for ($page = 1; $page -le 31; $page++) {
        $separator = if ($Endpoint.Contains('?')) { '&' } else { '?' }
        $batch = Invoke-ExpertGh -Arguments @("$Endpoint${separator}per_page=100&page=$page")
        if ($batch -isnot [array]) { throw 'GitHub returned an invalid paginated evidence shape.' }
        if ($page -eq 31 -and $batch.Count) {
            throw 'Review evidence exceeded the pagination limit; earlier feedback must not be silently omitted.'
        }
        foreach ($item in $batch) { $items.Add($item) }
        if ($batch.Count -lt 100) { return ,$items.ToArray() }
    }
    throw 'Review evidence exceeded the pagination limit; earlier feedback must not be silently omitted.'
}

function Assert-ExpertPermission {
    param([string]$Login)

    if ($Login -notmatch '^[A-Za-z0-9][A-Za-z0-9-]*$') { throw 'A human review requester is required.' }
    $permission = Invoke-ExpertGh -Arguments @("repos/$Repository/collaborators/$Login/permission")
    if ($permission.permission -notin @('admin', 'maintain', 'write')) {
        throw 'The expert-review requester does not have current repository write access.'
    }
}

function Invoke-ExpertGit {
    param([string[]]$Arguments)

    $raw = & git --no-pager -c core.hooksPath=/dev/null `
        -c filter.lfs.required=false -c filter.lfs.smudge= -c filter.lfs.process= @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Pinned review git operation failed: $($Arguments[0])." }
    return ($raw -join "`n")
}

function Assert-ExpertPath {
    param([object]$Path)

    if ($Path -isnot [string] -or [string]::IsNullOrWhiteSpace($Path) -or
        $Path -match '[\x00-\x1f\x7f\\]' -or $Path.StartsWith('/') -or
        $Path -match '^[A-Za-z]:' -or @($Path.Split('/') | Where-Object { $_ -in @('', '.', '..') }).Count) {
        throw 'A finding or source path is not a contained repository-relative path.'
    }
}

function Get-ExpertContext {
    $context = Read-ExpertJson -Path (Join-Path $ContextDirectory 'context.json')
    if ($context.repository -cne $Repository -or $context.pr_number -ne $PRNumber -or
        $context.schema_version -ne 1) { throw 'Review context does not match the trusted request.' }
    foreach ($key in @('head_sha', 'base_sha', 'merge_base_sha', 'infrastructure_sha')) {
        if ($context[$key] -notmatch '^[0-9a-f]{40}$') { throw 'Review context contains an invalid pinned revision.' }
    }
    if ($env:GITHUB_SHA -and $context.infrastructure_sha -cne $env:GITHUB_SHA) {
        if ($env:EXPERT_VERIFIED_EVIDENCE_SHA -cne $context.infrastructure_sha -or
            $env:EXPERT_VERIFIED_EVIDENCE_SHA -notmatch '^[0-9a-f]{40}$' -or
            $env:GITHUB_REPOSITORY -cne 'dotnet/maui' -or
            $env:GITHUB_REF -cne 'refs/heads/kubaflo-expert-live-canary-39332' -or
            $env:GITHUB_ACTOR -cne 'kubaflo' -or $env:GITHUB_EVENT_NAME -cne 'workflow_dispatch' -or
            $PRNumber -notin @(39502, 39501, 39500, 39498, 39497, 39495, 39493, 39491, 39489, 39487, 39486, 39484, 39483, 39482, 39480, 39479, 39475, 39474, 39473, 39472, 39470, 39469, 39462, 39460, 39458, 39457, 39453, 39452, 39451, 39449, 39441, 39439, 39433, 39430, 39429, 39427, 39426, 39425, 39422, 39421, 39419, 39415, 39412, 39411, 39409, 39406, 39403, 39401, 39400, 39398, 39396, 39384, 39378, 39377, 39375, 39366, 39365, 39361, 39346, 39345, 39344, 39325, 39314, 39289, 39281, 39272, 39255, 39240, 39233, 39218, 39195, 39180, 39179, 39176, 39059, 39052, 39013, 38915, 38911, 38822, 38807, 38782, 38780, 38735, 38733, 38638, 38625, 38611, 38603, 38585, 38441, 38111, 38067, 37953, 37874, 37836, 37690, 37572, 37337, 37319, 37137, 37032, 36762, 36702, 36581, 36571, 36226, 35973, 35640, 34837, 34740, 34357, 33611, 33561, 32898, 32382, 29952, 29755)) {
            throw 'Review infrastructure does not match the workflow or verified producer revision.'
        }
    }
    return $context
}

function Get-ExpertFeedback {
    return @{
        reviews = Get-ExpertPages "repos/$Repository/pulls/$PRNumber/reviews"
        inline = Get-ExpertPages "repos/$Repository/pulls/$PRNumber/comments"
        comments = Get-ExpertPages "repos/$Repository/issues/$PRNumber/comments"
    }
}

function Get-ExpertRequest {
    if (-not $env:GITHUB_EVENT_PATH) { throw 'A trusted GitHub event is required for request completion.' }
    if ($env:GITHUB_RUN_ATTEMPT -cne '1') { throw 'Use a fresh expert-review request, not a workflow rerun.' }
    $event = Read-ExpertJson -Path $env:GITHUB_EVENT_PATH
    $canary = $Repository -ceq 'dotnet/maui' -and
        $env:GITHUB_REF -ceq 'refs/heads/kubaflo-expert-live-canary-39332' -and
        $env:GITHUB_EVENT_NAME -ceq 'workflow_dispatch' -and
        $env:GITHUB_ACTOR -ceq 'kubaflo' -and
        $PRNumber -in @(39502, 39501, 39500, 39498, 39497, 39495, 39493, 39491, 39489, 39487, 39486, 39484, 39483, 39482, 39480, 39479, 39475, 39474, 39473, 39472, 39470, 39469, 39462, 39460, 39458, 39457, 39453, 39452, 39451, 39449, 39441, 39439, 39433, 39430, 39429, 39427, 39426, 39425, 39422, 39421, 39419, 39415, 39412, 39411, 39409, 39406, 39403, 39401, 39400, 39398, 39396, 39384, 39378, 39377, 39375, 39366, 39365, 39361, 39346, 39345, 39344, 39325, 39314, 39289, 39281, 39272, 39255, 39240, 39233, 39218, 39195, 39180, 39179, 39176, 39059, 39052, 39013, 38915, 38911, 38822, 38807, 38782, 38780, 38735, 38733, 38638, 38625, 38611, 38603, 38585, 38441, 38111, 38067, 37953, 37874, 37836, 37690, 37572, 37337, 37319, 37137, 37032, 36762, 36702, 36581, 36571, 36226, 35973, 35640, 34837, 34740, 34357, 33611, 33561, 32898, 32382, 29952, 29755)
    if ($event.repository.full_name -cne $Repository -or
        ($env:GITHUB_REF -cne "refs/heads/$($event.repository.default_branch)" -and -not $canary)) {
        throw 'Expert-review infrastructure must run from the repository default branch.'
    }
    if ($env:GITHUB_EVENT_NAME -eq 'issue_comment') {
        if ($event.action -cne 'created' -or -not $event.issue.pull_request -or
            $event.issue.number -ne $PRNumber -or $event.comment.user.type -cne 'User' -or
            $event.comment.body -cnotmatch '^/review\s+expert-review\s*$') {
            throw 'Not an exact newly created human expert-review PR command.'
        }
        $login = [string]$event.comment.user.login
    } elseif ($env:GITHUB_EVENT_NAME -eq 'workflow_dispatch') {
        if ([string]$event.inputs.pr_number -cne [string]$PRNumber) { throw 'Dispatch target does not match the request.' }
        $login = $env:GITHUB_ACTOR
    } else {
        throw 'Unsupported expert-review event.'
    }
    Assert-ExpertPermission -Login $login
    return @{ event = $event; actor = $login }
}

function Get-ExpertDiscussionContent {
    param([Collections.IDictionary]$Feedback)

    $content = [ordered]@{}
    foreach ($surface in @('reviews', 'inline', 'comments')) {
        $content[$surface] = @(
            foreach ($item in ($Feedback[$surface] | Sort-Object -Property id)) {
                $entry = [ordered]@{
                    author_id = $item.user.id
                    author = $item.user.login
                    author_type = $item.user.type
                }
                foreach ($field in @('id', 'body', 'state', 'commit_id', 'original_commit_id',
                    'path', 'line', 'original_line', 'start_line', 'original_start_line',
                    'side', 'start_side', 'diff_hunk', 'in_reply_to_id', 'pull_request_review_id',
                    'created_at', 'updated_at', 'submitted_at')) {
                    $entry[$field] = $item[$field]
                }
                $entry
            }
        )
    }
    return $content
}

function Get-ExpertRunUrl {
    if (-not $env:GITHUB_RUN_ID -or $env:GITHUB_RUN_ID -notmatch '^[1-9][0-9]*$') {
        throw 'A trusted GitHub run identity is required.'
    }
    return "$env:GITHUB_SERVER_URL/$Repository/actions/runs/$env:GITHUB_RUN_ID"
}

function Get-ExpertFindings {
    param([object]$Value, [Collections.Generic.HashSet[string]]$Paths)

    if ($Value -isnot [array] -or $Value.Count -gt 17) { throw 'Findings must be a JSON array containing at most 17 entries.' }
    $result = [Collections.Generic.List[object]]::new()
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($finding in $Value) {
        if ($finding -isnot [Collections.IDictionary] -or
            (($finding.Keys | Sort-Object) -join ',') -cne 'body,line,path') {
            throw 'Every finding must contain exactly path, line, and body.'
        }
        Assert-ExpertPath $finding.path
        if (-not $Paths.Contains($finding.path) -or
            ($finding.line -isnot [long] -and $finding.line -isnot [int]) -or
            $finding.line -lt 1 -or $finding.line -gt [int]::MaxValue -or
            $finding.body -isnot [string] -or $finding.body.Length -gt 16000 -or
            $finding.body -cnotmatch '^\*\*\[(critical|major|moderate|minor)\]\s+[^*]+\*\*') {
            throw 'A finding has an invalid diff path, line, or severity/dimension body.'
        }
        if ($finding.body -match '^\*\*\[(?!major\])[^\]]+\]\s+Input and Path Correctness\*\*') {
            throw 'Input and Path Correctness findings must retain their major severity cap.'
        }
        $entry = [ordered]@{ path = $finding.path; line = [int]$finding.line; body = $finding.body }
        $key = ConvertTo-Json -InputObject $entry -Compress
        if (-not $seen.Add($key)) { throw 'The expert output contains duplicate findings.' }
        $result.Add($entry)
    }
    return ,$result.ToArray()
}

function Test-ExpertJsonEqual {
    param([AllowNull()][object]$Left, [AllowNull()][object]$Right)

    if ($null -eq $Left -or $null -eq $Right) { return $null -eq $Left -and $null -eq $Right }
    if ($Left -is [Collections.IDictionary] -and $Right -is [Collections.IDictionary]) {
        if ($Left.Count -ne $Right.Count) { return $false }
        foreach ($key in $Left.Keys) {
            if (-not $Right.Contains($key) -or -not (Test-ExpertJsonEqual $Left[$key] $Right[$key])) { return $false }
        }
        return $true
    }
    if ($Left -is [array] -and $Right -is [array]) {
        if ($Left.Count -ne $Right.Count) { return $false }
        for ($i = 0; $i -lt $Left.Count; $i++) {
            if (-not (Test-ExpertJsonEqual $Left[$i] $Right[$i])) { return $false }
        }
        return $true
    }
    return $Left.GetType() -eq $Right.GetType() -and $Left -ceq $Right
}

function Get-ExpertDimensions {
    param([object]$Value)

    if ($Value -isnot [array] -or -not $Value.Count -or $Value.Count -gt 31) {
        throw 'The routed dimension list must be a nonempty bounded array.'
    }
    $ids = [Collections.Generic.HashSet[int]]::new()
    foreach ($id in $Value) {
        if (($id -isnot [long] -and $id -isnot [int]) -or $id -lt 1 -or $id -gt 31 -or -not $ids.Add($id)) {
            throw 'Routed dimensions must be distinct integers from 1 through 31.'
        }
    }
    foreach ($id in @(5, 6, 16)) {
        if (-not $ids.Contains($id)) { throw 'An always-active expert dimension was omitted.' }
    }
    return $ids
}

function Get-ExpertDimensionNames {
    $rulebook = [IO.File]::ReadAllText((Join-Path $PSScriptRoot '../agents/maui-expert-reviewer.md'))
    $names = @{}
    foreach ($heading in [regex]::Matches($rulebook, '(?m)^### ([1-9][0-9]?)\. (.+?) `\[(?:critical|major|moderate|minor)\]`\r?$')) {
        $id = [int]$heading.Groups[1].Value
        if ($id -gt 31 -or $names.ContainsKey($id)) {
            throw 'The trusted expert rulebook contains an invalid or duplicate dimension heading.'
        }
        $names.Add($id, $heading.Groups[2].Value)
    }
    if ($names.Count -ne 31) { throw 'The trusted expert rulebook must name all 31 dimensions.' }
    return $names
}

function Invoke-ExpertGather {
    if (-not $OutputDirectory) { throw 'Gather requires an output directory.' }
    if ($env:GITHUB_OUTPUT) { 'ready=false' >> $env:GITHUB_OUTPUT }
    Assert-ExpertPermission -Login $Actor
    [void][IO.Directory]::CreateDirectory($OutputDirectory)
    if (@(Get-ChildItem -LiteralPath $OutputDirectory -Force).Count) {
        throw 'Gather requires a new or empty frozen-context directory.'
    }
    $pr = Invoke-ExpertGh -Arguments @("repos/$Repository/pulls/$PRNumber")
    if ($pr.state -cne 'open' -or $pr.base.repo.full_name -cne $Repository) {
        throw 'Expert review requires an open PR in the requested repository.'
    }
    $head = [string]$pr.head.sha
    $base = [string]$pr.base.sha
    foreach ($sha in @($head, $base)) {
        if ($sha -notmatch '^[0-9a-f]{40}$') { throw 'GitHub did not return valid PR revisions.' }
    }
    $infrastructure = Invoke-ExpertGit -Arguments @('rev-parse', 'HEAD')
    if ($env:GITHUB_SHA -and $infrastructure -cne $env:GITHUB_SHA) {
        throw 'The collector checkout is not the trusted workflow revision.'
    }
    [void](Invoke-ExpertGit -Arguments @('fetch', '--no-tags', 'origin', $base, $head))
    $mergeBase = Invoke-ExpertGit -Arguments @('merge-base', $base, $head)
    $diff = Invoke-ExpertGit -Arguments @('diff', '--no-ext-diff', '--no-textconv', '--find-renames', $mergeBase, $head, '--')
    $paths = @((Invoke-ExpertGit -Arguments @('diff', '--name-only', '-z', $mergeBase, $head, '--')) -split "`0" |
        Where-Object { $_ })
    foreach ($path in $paths) { Assert-ExpertPath $path }
    Write-ExpertJson (Join-Path $OutputDirectory 'changed-paths.json') $paths
    [IO.File]::WriteAllText((Join-Path $OutputDirectory 'pr.diff'), $diff, [Text.UTF8Encoding]::new($false))
    $files = Get-ExpertPages "repos/$Repository/pulls/$PRNumber/files"
    $feedback = Get-ExpertFeedback
    Write-ExpertJson (Join-Path $OutputDirectory 'files.json') $files
    Write-ExpertJson (Join-Path $OutputDirectory 'prior-feedback.json') $feedback

    $gaps = [Collections.Generic.List[string]]::new()
    if ($files.Count -ne $paths.Count -or $files.Count -ne $pr.changed_files) {
        $gaps.Add('GitHub changed-file metadata is incomplete; use the full pinned git diff and source.')
    }
    $issues = [Collections.Generic.List[object]]::new()
    $pattern = '(?i)(?:fix(?:es|ed)?|close[sd]?|resolve[sd]?)\s+(?:' +
        [regex]::Escape("$env:GITHUB_SERVER_URL/$Repository/issues/") + '|#)([1-9][0-9]*)'
    $issueNumbers = @([regex]::Matches([string]$pr.body, $pattern) |
        ForEach-Object { [int]$_.Groups[1].Value } | Sort-Object -Unique)
    foreach ($number in $issueNumbers | Select-Object -First 20) {
        $issue = Invoke-ExpertGh -Arguments @("repos/$Repository/issues/$number")
        $issues.Add(@{ number = $number; url = $issue.html_url; title = $issue.title; body = $issue.body })
    }
    if ($issueNumbers.Count -gt 20) { $gaps.Add('Linked-issue collection exceeded the bounded 20-issue limit.') }
    Write-ExpertJson (Join-Path $OutputDirectory 'narrative.json') @{
        title = $pr.title; body = $pr.body; author = $pr.user.login; issues = $issues.ToArray()
    }

    $implementationPaths = @($paths | Where-Object {
        $_ -match '\.(cs|xaml)$' -and $_ -notmatch '(?i)(Tests|TestCases|tests|snapshots|samples)/' -and
        $_ -notmatch '\.(Designer|g)\.cs$'
    })
    $riskDirectory = Join-Path $OutputDirectory 'regression-check'
    if ($implementationPaths.Count) {
        & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Find-RegressionRisks.ps1') `
            -PRNumber $PRNumber -Repo $Repository -BaseBranch $base `
            -DiffFile (Join-Path $OutputDirectory 'pr.diff') -StrictEvidence `
            -ChangedPathsFile (Join-Path $OutputDirectory 'changed-paths.json') `
            -OutputDir $riskDirectory *> (Join-Path $OutputDirectory 'regression-collection.log')
        if ($LASTEXITCODE -ne 0) {
            $gaps.Add('Historical regression cross-reference failed; do not infer CLEAN from missing evidence.')
        } else {
            $risks = Read-ExpertJson (Join-Path $riskDirectory 'risks.json')
            if ($risks.pr_number -ne $PRNumber -or $risks.result -notin @('CLEAN', 'OVERLAP', 'REVERT')) {
                throw 'Regression cross-reference returned invalid evidence.'
            }
        }
    }
    $current = Invoke-ExpertGh -Arguments @("repos/$Repository/pulls/$PRNumber")
    Assert-ExpertPermission -Login $Actor
    if ($current.state -cne 'open' -or $current.head.sha -cne $head -or $current.base.sha -cne $base) {
        throw 'The PR changed during context collection; request a fresh expert review.'
    }
    $context = [ordered]@{
        schema_version = 1; repository = $Repository; pr_number = $PRNumber
        actor = $Actor; head_sha = $head; base_sha = $base; merge_base_sha = $mergeBase
        infrastructure_sha = $infrastructure; paths = $paths; gaps = $gaps.ToArray()
    }
    Write-ExpertJson (Join-Path $OutputDirectory 'context.json') $context
    foreach ($file in Get-ChildItem -LiteralPath $OutputDirectory -File) {
        if ($file.Length -gt 16MB) { throw 'A context file exceeds the bounded review evidence limit.' }
    }
    if ($env:GITHUB_OUTPUT) {
        "head_sha=$head" >> $env:GITHUB_OUTPUT
        "base_sha=$base" >> $env:GITHUB_OUTPUT
        'ready=true' >> $env:GITHUB_OUTPUT
    }
    Write-Host "Gathered frozen expert-review context for PR #$PRNumber."
}

function Invoke-ExpertSnapshot {
    $context = Get-ExpertContext
    if (-not $OutputDirectory -or (Test-Path -LiteralPath $OutputDirectory)) {
        throw 'Snapshot requires a new isolated source directory.'
    }
    if ((Invoke-ExpertGit -Arguments @('rev-parse', 'HEAD')) -cne $context.infrastructure_sha) {
        throw 'The expert adapter checkout is not the trusted infrastructure revision.'
    }
    [void](Invoke-ExpertGit -Arguments @('fetch', '--no-tags', 'origin', $context.base_sha, $context.head_sha))
    if ((Invoke-ExpertGit -Arguments @('merge-base', $context.base_sha, $context.head_sha)) -cne $context.merge_base_sha) {
        throw 'The source snapshot does not match the frozen diff.'
    }
    [void](Invoke-ExpertGit -Arguments @('worktree', 'add', '--detach', $OutputDirectory, $context.head_sha))
    Write-ExpertJson (Join-Path $ContextDirectory 'source-location.json') @{
        path = [IO.Path]::GetFullPath($OutputDirectory)
        head_sha = $context.head_sha
    }
    Write-Host "Prepared isolated pinned source for PR #$PRNumber; no PR code was executed."
}

function Invoke-ExpertPrepare {
    if ($env:GITHUB_RUN_ATTEMPT -cne '1') { throw 'Earlier-attempt artifacts cannot authorize expert-review publication.' }
    $context = Get-ExpertContext
    Assert-ExpertPermission -Login $context.actor
    if (-not $OutputDirectory -or -not $ResultDirectory -or -not $AgentOutputPath) {
        throw 'Publication requires context, result, agent-output, and validation directories.'
    }
    [void][IO.Directory]::CreateDirectory($OutputDirectory)
    $imported = Join-Path $OutputDirectory 'imported'
    $output = Read-ExpertJson (Copy-ExpertInput $AgentOutputPath (Join-Path $imported 'agent-output.json') 1MB) 1MB
    $rawFindings = Read-ExpertJson (Copy-ExpertInput (Join-Path $ResultDirectory 'inline-findings.json') `
        (Join-Path $imported 'inline-findings.json') 512KB) 512KB
    $state = Read-ExpertJson (Copy-ExpertInput (Join-Path $ResultDirectory 'review-state.json') `
        (Join-Path $imported 'review-state.json') 256KB) 256KB
    if ($output.items -isnot [array] -or $output.items.Count -ne 1 -or
        $output.items[0].type -cne 'submit_pull_request_review' -or
        $output.items[0].event -cne 'COMMENT' -or
        $output.items[0].data -isnot [Collections.IDictionary]) {
        throw 'The expert agent must submit exactly one structured COMMENT review.'
    }
    $message = $output.items[0]
    foreach ($key in @('item_number', 'pull_request_number', 'target')) {
        if ($message.Contains($key) -and [string]$message[$key] -cne [string]$PRNumber) {
            throw 'Expert output selected an unexpected publication target.'
        }
    }
    foreach ($key in @('target_repo', 'target-repo', 'repo', 'repository')) {
        if ($message.Contains($key) -and $message[$key] -cne $Repository -and
            -not ($env:EXPERT_VERIFIED_EVIDENCE_SHA -and $message[$key] -ceq 'kubaflo/maui')) {
            throw 'Expert output selected an unexpected publication repository.'
        }
    }
    if (-not (Test-ExpertJsonEqual $rawFindings $message.data.findings) -or
        -not (Test-ExpertJsonEqual $state $message.data.review_state)) {
        throw 'The staged output does not match the required persisted reviewer artifacts.'
    }
    if (($state.schema_version -isnot [long] -and $state.schema_version -isnot [int]) -or
        $state.schema_version -ne 1 -or $state.validation_complete -isnot [bool] -or
        $state.unplaced_findings -isnot [array] -or $state.unplaced_findings.Count -gt 17 -or
        $state.input_path_exceptions -isnot [array] -or $state.input_path_exceptions.Count -gt 2) {
        throw 'The reviewer completion metadata is invalid.'
    }
    $paths = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($path in $context.paths) { Assert-ExpertPath $path; [void]$paths.Add($path) }
    $findings = Get-ExpertFindings $rawFindings $paths
    $dimensions = Get-ExpertDimensions $state.activated_dimensions
    $dimensionNames = Get-ExpertDimensionNames
    $gaps = [Collections.Generic.List[string]]::new()
    $limitations = [Collections.Generic.List[string]]::new()
    foreach ($gap in $context.gaps) { $limitations.Add([string]$gap) }
    $completed = 0
    foreach ($id in $dimensions | Sort-Object) {
        $source = Join-Path $ResultDirectory "dimensions/$id.json"
        if (-not (Test-Path -LiteralPath $source)) {
            $gaps.Add("The $($dimensionNames[$id]) check did not return a saved result.")
            continue
        }
        $worker = Read-ExpertJson (Copy-ExpertInput $source (Join-Path $imported "dimensions/$id.json") 1MB) 1MB
        if ($worker -isnot [Collections.IDictionary] -or
            (($worker.Keys | Sort-Object) -join ',') -cne 'complete,dimension,findings' -or
            ($worker.dimension -isnot [long] -and $worker.dimension -isnot [int]) -or
            $worker.dimension -ne $id -or $worker.complete -isnot [bool]) {
            throw 'An independent dimension artifact has an invalid identity or completion flag.'
        }
        [void](Get-ExpertFindings $worker.findings $paths)
        if ($worker.complete) {
            $completed++
        } else {
            $gaps.Add("The $($dimensionNames[$id]) check did not finish.")
        }
    }
    if (-not $state.validation_complete) { $gaps.Add('The final check of potential findings did not finish.') }
    $exceptionKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($exception in $state.input_path_exceptions) {
        if ($exception.kind -notin @('committed-credential', 'archive-path-escape') -or
            -not $exceptionKeys.Add($exception.kind) -or
            -not @($findings | Where-Object {
                $_.path -ceq $exception.path -and $_.line -eq $exception.line -and
                $_.body -cmatch '^\*\*\[major\]\s+Input and Path Correctness\*\*'
            }).Count) {
            throw 'The reviewer finding-limit exception is not a retained dimension-31 finding.'
        }
    }
    if ($findings.Count -gt 15 + $exceptionKeys.Count) { throw 'The canonical expert finding limit was exceeded.' }
    $unplaced = [Collections.Generic.List[string]]::new()
    foreach ($body in $state.unplaced_findings) {
        if ($body -isnot [string] -or [string]::IsNullOrWhiteSpace($body) -or $body.Length -gt 4000) {
            throw 'Unplaceable expert feedback must be a bounded nonempty string.'
        }
        $unplaced.Add($body)
    }
    if ($findings.Count + $unplaced.Count -gt 15 + $exceptionKeys.Count) {
        throw 'Placed and unplaceable findings together exceed the canonical expert finding limit.'
    }

    $runUrl = Get-ExpertRunUrl
    $current = Invoke-ExpertGh -Arguments @("repos/$Repository/pulls/$PRNumber")
    $status = if ($gaps.Count) { 'incomplete' } else { 'complete' }
    $receipt = [ordered]@{
        schema_version = 1; repository = $Repository; pr_number = $PRNumber
        run_id = $env:GITHUB_RUN_ID; run_url = $runUrl; head_sha = $context.head_sha
        status = $status; inline_comments = @(); withheld_findings = @(); review_body = ''
    }
    if ($current.state -cne 'open') { throw 'The target PR closed before expert-review publication.' }
    if ($current.head.sha -cne $context.head_sha -or $current.base.sha -cne $context.base_sha) {
        $receipt.status = 'stale'
        Write-Warning 'The code snapshot changed; no stale inline findings will be posted.'
        $output.items = @(@{ type = 'noop'; message = 'The PR snapshot changed; no comments were published.' })
    } else {
        $feedback = Get-ExpertFeedback
        $frozenFeedback = Read-ExpertJson (Join-Path $ContextDirectory 'prior-feedback.json')
        if (-not (Test-ExpertJsonEqual (Get-ExpertDiscussionContent $frozenFeedback) (Get-ExpertDiscussionContent $feedback))) {
            $limitations.Add('Discussion changed during the review; findings assess the frozen code and discussion available at review start.')
        }
        $files = Read-ExpertJson (Join-Path $ContextDirectory 'files.json')
        $lineMap = @{}
        foreach ($file in $files) {
            $lines = [Collections.Generic.HashSet[int]]::new()
            $line = 0
            foreach ($patchLine in ([string]$file.patch -split "`n")) {
                if ($patchLine -match '^@@ -\d+(?:,\d+)? \+(\d+)(?:,\d+)? @@') {
                    $line = [int]$Matches[1]
                } elseif ($patchLine.StartsWith('+') -or $patchLine.StartsWith(' ')) {
                    [void]$lines.Add($line)
                    $line++
                }
            }
            $lineMap[$file.filename] = $lines
        }
        $findingCount = $findings.Count + $unplaced.Count
        $inline = [Collections.Generic.List[object]]::new()
        $expectedInline = [Collections.Generic.List[object]]::new()
        foreach ($finding in $findings) {
            $expectedBody = Get-ExpertPublicationBody $finding.body
            $alreadyPosted = @($feedback.inline | Where-Object {
                $_.path -ceq $finding.path -and $_.line -eq $finding.line -and
                $_.commit_id -ceq $context.head_sha -and
                ([string]$_.body).Contains($expectedBody, [StringComparison]::Ordinal)
            })
            if ($alreadyPosted.Count) {
                continue
            }
            if ($lineMap.ContainsKey($finding.path) -and $lineMap[$finding.path].Contains($finding.line)) {
                $inline.Add(@{
                    path = $finding.path; line = $finding.line
                    body = $finding.body
                })
                $expectedInline.Add(@{
                    path = $finding.path; line = $finding.line
                    body = $expectedBody; canonical_body = $finding.body
                })
            } else {
                $unplaced.Add("``$($finding.path):$($finding.line)`` - $($finding.body)")
            }
        }
        $receipt.inline_comments = $expectedInline.ToArray()
        $receipt.withheld_findings = $unplaced.ToArray()
        $receipt.coverage = "$completed/$($dimensions.Count)"
        $receipt.gaps = $gaps.ToArray()
        $receipt.limitations = $limitations.ToArray()
        if ($unplaced.Count) {
            Write-Warning "$($unplaced.Count) findings have no valid inline anchor; retained in the publication artifact, not posted elsewhere."
        }
        foreach ($gap in $gaps) { Write-Warning $gap }
        $items = [Collections.Generic.List[object]]::new()
        foreach ($finding in $inline) {
            $items.Add(@{
                type = 'create_pull_request_review_comment'
                path = $finding.path; line = $finding.line; body = $finding.body; side = 'RIGHT'
            })
        }
        $noun = if ($findingCount -eq 1) { 'finding' } else { 'findings' }
        $body = "## Expert Review $([char]0x2014) $findingCount $noun"
        $body += "`n`nReviewed commit [``$($context.head_sha.Substring(0, 7))``]($env:GITHUB_SERVER_URL/$Repository/commit/$($context.head_sha))."
        if ($gaps.Count) {
            $body += "`n`nSource assessment incomplete ($completed/$($dimensions.Count) dimensions); finding count is partial."
        }
        if ($inline.Count) { $body += "`n`nSee inline comments for details." }
        if ($findingCount -gt $inline.Count + $unplaced.Count) {
            $body += "`n`nPreviously reported findings were not duplicated."
        }
        if ($unplaced.Count) {
            $body += "`n`n$($unplaced.Count) findings could not be attached inline; retained in [run artifacts]($runUrl)."
        }
        $receipt.review_body = $body
        $receipt.finding_count = $findingCount
        $items.Add(@{ type = 'submit_pull_request_review'; event = 'COMMENT'; body = $body })
        $output.items = $items.ToArray()
    }
    Assert-ExpertPermission -Login $context.actor
    $latest = Invoke-ExpertGh -Arguments @("repos/$Repository/pulls/$PRNumber")
    if ($latest.state -cne 'open' -or $latest.head.sha -cne $current.head.sha -or
        $latest.base.sha -cne $current.base.sha) {
        throw 'The PR changed again during publication validation; request a fresh expert review.'
    }
    Write-ExpertJson (Join-Path $OutputDirectory 'publication.json') $receipt
    Write-ExpertJson $AgentOutputPath $output
    Write-Host "Validated inline-only publication: $($receipt.status); $($receipt.inline_comments.Count) new comments."
}

function Invoke-ExpertComplete {
    $request = Get-ExpertRequest
    if ($env:GITHUB_EVENT_NAME -ne 'issue_comment') { return }
    $context = Get-ExpertContext
    $receipt = Read-ExpertJson (Join-Path $ResultDirectory 'publication.json') 256KB
    $runUrl = Get-ExpertRunUrl
    if ($receipt.repository -cne $Repository -or $receipt.pr_number -ne $PRNumber -or
        [string]$receipt.run_id -cne $env:GITHUB_RUN_ID -or $receipt.run_url -cne $runUrl -or
        $receipt.head_sha -cne $context.head_sha -or $receipt.status -cne 'complete') {
        Write-Host 'No completed publication receipt; leaving the command visible.'
        return
    }
    if ($receipt.inline_comments -isnot [array] -or $receipt.inline_comments.Count -gt 17) {
        throw 'The inline publication receipt is invalid.'
    }
    if ([string]::IsNullOrWhiteSpace($receipt.review_body)) { throw 'The review receipt body is missing.' }
    $reviews = Get-ExpertPages "repos/$Repository/pulls/$PRNumber/reviews"
    $postedReview = @($reviews | Where-Object {
        $_.user.login -ceq 'github-actions[bot]' -and $_.commit_id -ceq $context.head_sha -and
        $_.state -ceq 'COMMENTED' -and $_.body -ceq $receipt.review_body
    })
    if (-not $postedReview.Count) { throw 'GitHub has no matching completed expert-review receipt.' }
    if ($receipt.inline_comments.Count) {
        $comments = Get-ExpertPages "repos/$Repository/pulls/$PRNumber/comments"
        foreach ($expected in $receipt.inline_comments) {
            $published = @($comments | Where-Object {
                $_.user.login -ceq 'github-actions[bot]' -and
                $_.commit_id -ceq $context.head_sha -and $_.path -ceq $expected.path -and
                $_.line -eq $expected.line -and $_.body -ceq $expected.body
            })
            if (-not $published.Count) { throw 'GitHub has no matching published inline finding.' }
        }
    }
    $pr = Invoke-ExpertGh -Arguments @("repos/$Repository/pulls/$PRNumber")
    if ($pr.state -cne 'open' -or $pr.head.sha -cne $context.head_sha -or $pr.base.sha -cne $context.base_sha) {
        Write-Host 'The PR advanced after publication; leaving the request visible.'
        return
    }
    $comment = Invoke-ExpertGh -Arguments @("repos/$Repository/issues/comments/$($request.event.comment.id)")
    $apiUrl = if ($env:GITHUB_API_URL) { $env:GITHUB_API_URL } else { 'https://api.github.com' }
    if ($comment.issue_url -cne "$apiUrl/repos/$Repository/issues/$PRNumber" -or
        $comment.node_id -cne $request.event.comment.node_id -or
        $comment.user.login -cne $request.actor -or $comment.body -cnotmatch '^/review\s+expert-review\s*$') {
        throw 'The command identity changed before completion.'
    }
    Assert-ExpertPermission -Login $request.actor
    $minimized = Invoke-ExpertGh -Arguments @('graphql') -Body @{
        query = 'mutation($id:ID!){minimizeComment(input:{subjectId:$id,classifier:RESOLVED}){minimizedComment{isMinimized}}}'
        variables = @{ id = $comment.node_id }
    }
    if ($minimized.errors -or $minimized.data.minimizeComment.minimizedComment.isMinimized -ne $true) {
        throw 'GitHub did not confirm command minimization.'
    }
    Write-Host 'Verified the expert-review receipt and inline publication, then minimized its command.'
}

switch ($Stage) {
    'Gather' { Invoke-ExpertGather }
    'Snapshot' { Invoke-ExpertSnapshot }
    'Prepare' { Invoke-ExpertPrepare }
    'Complete' { Invoke-ExpertComplete }
}
