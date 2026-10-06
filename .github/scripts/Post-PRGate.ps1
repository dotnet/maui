#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes only the existing-PR-test gate, without submitting a PR review.
.DESCRIPTION
    Runs on a fresh trusted agent. The pipeline verdict and commit identities
    are authoritative; the bounded report is diagnostic data, never executable.
    Publication is idempotent per Azure run and only edits the publisher's own
    comments. A stale run cannot change result labels or hide newer reports.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PRNumber,

    [Parameter(Mandatory)]
    [ValidateRange(1, [long]::MaxValue)]
    [long]$RunId,

    [Parameter(Mandatory)]
    [ValidateSet('PASSED', 'FAILED', 'SKIPPED', 'INCONCLUSIVE', 'TIMEDOUT')]
    [string]$TrustedGateResult,

    [Parameter(Mandatory)]
    [ValidateSet('android', 'ios', 'catalyst', 'windows')]
    [string]$Platform,

    [ValidatePattern('^$|^[0-9a-fA-F]{40}$')]
    [string]$ReviewedCommit = '',

    [ValidatePattern('^$|^[0-9a-fA-F]{40}$')]
    [string]$BaseCommit = '',

    [ValidateSet('COMPLETED', 'MERGE_CONFLICT', 'FAILED', 'NOT_STARTED')]
    [string]$SetupResult = 'NOT_STARTED',

    [string]$ReportFile,

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'shared/Invoke-GhCommandWithRetry.ps1')
. (Join-Path $PSScriptRoot 'shared/Update-AgentLabels.ps1')
. (Join-Path $PSScriptRoot 'shared/Remove-StaleMauiBotComments.ps1')

$prJson = Invoke-GhCommandWithRetry `
    -Arguments @('api', "repos/dotnet/maui/pulls/$PRNumber") `
    -Description "read gate publication target #$PRNumber" `
    -RequireOutput
$pr = $prJson | ConvertFrom-Json
if ($pr.state -ne 'open' -or $pr.number -ne $PRNumber -or
    $pr.user.login -notmatch '^[A-Za-z0-9][A-Za-z0-9_\[\]-]{0,99}$' -or
    $pr.head.sha -notmatch '^[0-9a-fA-F]{40}$') {
    throw 'Gate publication requires valid metadata for the expected open PR.'
}

$report = ''
if (-not [string]::IsNullOrWhiteSpace($ReportFile)) {
    $item = Get-Item -LiteralPath $ReportFile -ErrorAction Stop
    if ($item -isnot [IO.FileInfo] -or
        ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        $item.Length -gt 16MB) {
        throw 'Gate report must be a bounded regular file.'
    }
    $report = [IO.File]::ReadAllText($item.FullName, [Text.UTF8Encoding]::new($false, $true))
}
if ($TrustedGateResult -eq 'PASSED' -and
    ([string]::IsNullOrWhiteSpace($report) -or -not $ReviewedCommit -or -not $BaseCommit)) {
    throw 'Cannot publish PASSED without diagnostic evidence and immutable commit identities.'
}

$failureOnly = $report -match '(?m)^## Gate: Test Verification \(Failure-Only Mode\)\s*$'
$isCurrent = $ReviewedCommit -and $pr.head.sha -ceq $ReviewedCommit
$verdict = switch ($TrustedGateResult) {
    'PASSED' {
        if ($failureOnly) {
            '&#x2705; **Verification: PASSED (failure-only).** The selected tests failed without a product fix. No with-fix result was verified.'
        } else {
            '&#x2705; **Verification: PASSED.** The selected tests met the verifier''s expected outcomes. See the diagnostics for the before/after evidence; a test-only change does not imply a with-fix run.'
        }
    }
    'FAILED' {
        '&#x274C; **Verification: FAILED.** The selected tests did not meet the expected outcomes. Inspect the evidence before changing the test or fix.'
    }
    'SKIPPED' {
        '&#x26A0; **Verification: SKIPPED.** No runnable PR tests were selected. The fix has not been verified by this gate.'
    }
    'INCONCLUSIVE' {
        if ($SetupResult -eq 'MERGE_CONFLICT') {
            '&#x26A0; **Verification: INCONCLUSIVE.** The PR could not be merged with its target branch. Resolve the merge conflicts before retrying; no tests ran.'
        } else {
            '&#x26A0; **Verification: INCONCLUSIVE.** Setup, build, or execution did not produce a conclusive result. This is not evidence that the fix is incorrect.'
        }
    }
    'TIMEDOUT' {
        '&#x26A0; **Verification: TIMEDOUT.** The gate stopped before publishing its verdict. Partial diagnostics do not verify the fix.'
    }
}

$nextStep = switch ($TrustedGateResult) {
    'PASSED' { 'Review whether the selected tests cover the reported issue. A passing gate is not a code review or merge recommendation.' }
    'FAILED' { 'Check the unexpected before/after outcome and the assertion diagnostics, then correct the test or fix.' }
    'SKIPPED' { 'Add runnable regression tests to the PR. This command does not generate missing tests.' }
    default { 'Resolve the reported setup, build, or environment blocker before retrying. An inconclusive run does not invalidate the PR.' }
}
$commitNote = if ($ReviewedCommit) {
    "PR head: [$($ReviewedCommit.Substring(0, 8))](https://github.com/dotnet/maui/commit/$ReviewedCommit)."
} else {
    'Setup did not capture a verified PR head; no commit is claimed as tested.'
}
$baseNote = if ($BaseCommit) {
    "Without-fix baseline: [$($BaseCommit.Substring(0, 8))](https://github.com/dotnet/maui/commit/$BaseCommit)."
} else {
    'No immutable without-fix baseline was prepared.'
}
$staleNote = if ($ReviewedCommit -and -not $isCurrent) {
    '**Outdated snapshot:** the PR head changed while this gate ran. This report is for the pinned commit above; result labels are not updated.'
} else { '' }

$reportNote = ''
if ($report.Length -gt 8000) {
    $report = $report.Substring(0, 4000) + "`n... diagnostic excerpt omitted ...`n" +
        $report.Substring($report.Length - 4000)
    $reportNote = 'The diagnostic excerpt is shortened. The complete bounded report and execution logs remain in the GateLogs pipeline artifact.'
}
$diagnostics = if (-not [string]::IsNullOrWhiteSpace($report)) {
    '<details><summary>Test results and diagnostics</summary>' + "`n`n" +
        '**Diagnostic transcript only; the verification verdict above is authoritative.**' + "`n`n" +
        '<pre>' + [Net.WebUtility]::HtmlEncode($report) + '</pre>' + "`n`n" +
        $reportNote + "`n`n</details>"
} else {
    'No diagnostic report was produced. No successful test execution is inferred.'
}

$marker = '<!-- AI Gate -->'
$runMarker = "<!-- pr-gate-run:$RunId -->"
$prefix = "$marker`n$runMarker"
$body = @(
    $prefix, '## PR gate', '',
    "@$($pr.user.login), this report verifies existing tests only.", '',
    '---', '',
    '<details>',
    '<summary><strong>&#x1F9EA; Gate analysis</strong> &#x2014; click to expand</summary>',
    '<br/>', '',
    $verdict, '',
    "**Platform:** $Platform", '',
    $commitNote, '', $baseNote, '', $staleNote, '',
    '**Scope:** Selected PR unit, XAML, device, and UI tests. Native coverage remains bounded by the verifier; unselected tests are not implied to pass.', '',
    $diagnostics, '',
    '</details>', '', '---', '',
    '<details>',
    '<summary><strong>&#x1F9ED; Follow-up</strong> &#x2014; actions and refresh</summary>',
    '<br/>', '',
    $nextStep, '',
    'No expert code review, alternative fix, title/description edit, approval, or full-category UI sweep runs in this command.', '',
    "Refresh with ``/review gate --platform $Platform`` after updating the PR or resolving the blocker.", '',
    "<sub>Gate execution $RunId. Existing tests only; no generated test candidate or native recording is claimed.</sub>",
    '</details>'
) -join "`n"
if ([Text.Encoding]::UTF8.GetByteCount($body) -gt 60000) {
    throw 'Gate comment exceeds the bounded publication size.'
}
if ($DryRun) {
    $body
    return
}

$identityJson = Invoke-GhCommandWithRetry `
    -Arguments @('api', 'user') -Description 'identify the gate publisher' -RequireOutput
$identity = $identityJson | ConvertFrom-Json
if ($identity.id -notmatch '^[1-9][0-9]*$') {
    throw 'Could not identify the authenticated gate publisher.'
}
$query = ".[] | select(.user.id == $($identity.id) and (.body | startswith(`"$marker`"))) | {id, node_id, body: .body[0:200]} | @json"
function Get-OwnedGateComments {
    $commentJson = Invoke-GhCommandWithRetry `
        -Arguments @('api', '--paginate', "repos/dotnet/maui/issues/$PRNumber/comments?per_page=100", '--jq', $query) `
        -Description 'read owned gate comments'
    if (-not [string]::IsNullOrWhiteSpace($commentJson)) {
        $commentJson -split '\r?\n' | Where-Object { $_ } | ForEach-Object { $_ | ConvertFrom-Json }
    }
}
$comments = @(Get-OwnedGateComments)
$existing = @($comments | Where-Object { $_.body.StartsWith($prefix, [StringComparison]::Ordinal) })
if ($existing.Count -gt 1) {
    throw 'Multiple owned gate comments exist for the same run; refusing ambiguous publication.'
}
$endpoint = if ($existing.Count -eq 1) {
    "repos/dotnet/maui/issues/comments/$($existing[0].id)"
} else { "repos/dotnet/maui/issues/$PRNumber/comments" }
$method = if ($existing.Count -eq 1) { 'PATCH' } else { 'POST' }
$postedJson = $body | gh api $endpoint --method $method -F body=@-
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace([string]$postedJson)) {
    throw 'Gate comment publication failed; retry the same run to reconcile its owned comment.'
}
$posted = $postedJson | ConvertFrom-Json
if ($posted.id -notmatch '^[1-9][0-9]*$' -or
    $posted.html_url -notmatch "^https://github\.com/dotnet/maui/(pull|issues)/$PRNumber#issuecomment-[1-9][0-9]*$") {
    throw 'Gate publication returned an unexpected receipt.'
}
Write-Host "Published gate comment: $($posted.html_url)"
if ($env:TF_BUILD) {
    Write-Host "##vso[task.setvariable variable=gateCommentId;isOutput=true]$($posted.id)"
}

# Recheck publication ordering and the live head before changing result labels.
$comments = @(Get-OwnedGateComments)
$newerReport = @($comments | Where-Object {
    $_.body -match '<!-- pr-gate-run:([1-9][0-9]{0,18}) -->' -and
    [decimal]$Matches[1] -gt $RunId
})
$currentPrJson = Invoke-GhCommandWithRetry `
    -Arguments @('api', "repos/dotnet/maui/pulls/$PRNumber") `
    -Description 'recheck the gate result commit' -RequireOutput
$currentPr = $currentPrJson | ConvertFrom-Json
if ($ReviewedCommit -and $currentPr.state -eq 'open' -and
    $currentPr.head.sha -ceq $ReviewedCommit -and $newerReport.Count -eq 0) {
    foreach ($label in @('s/agent-gate-passed', 's/agent-gate-failed')) {
        Remove-Label -PRNumber $PRNumber -LabelName $label | Out-Null
    }
    $labelName = switch ($TrustedGateResult) {
        'PASSED' { 's/agent-gate-passed' }
        'FAILED' { 's/agent-gate-failed' }
        default { '' }
    }
    if ($labelName) {
        $definition = $script:SignalLabels[$labelName]
        Ensure-LabelExists -LabelName $labelName -Description $definition.Description -Color $definition.Color
        Add-Label -PRNumber $PRNumber -LabelName $labelName | Out-Null
    }
    foreach ($comment in $comments) {
        if ($comment.id -eq $posted.id) { continue }
        if ($comment.body -notmatch '<!-- pr-gate-run:([1-9][0-9]{0,18}) -->' -or
            [decimal]$Matches[1] -ge $RunId) { continue }
        Invoke-GitHubMinimizeComment -SubjectNodeId $comment.node_id `
            -Reason "superseded gate report $($comment.id)" | Out-Null
    }
} else {
    Write-Warning 'The PR snapshot is unverified, closed, outdated, or superseded by a newer run; result labels and other reports were left unchanged.'
}
