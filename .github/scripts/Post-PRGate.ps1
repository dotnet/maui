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

function ConvertTo-GatePhaseCell {
    param([string]$Value, [switch]$WithoutFix)

    $status = [regex]::Match($Value, '^[^A-Za-z]*(BUILD ERROR|ENV ERROR|NO MATCH|NEW SNAPSHOT|PASS|FAIL)\b',
        [Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $duration = [regex]::Match($Value, '\b([0-9]{1,7})s\b')
    $suffix = if ($duration.Success) { " ($($duration.Groups[1].Value)s)" } else { '' }
    switch ($status.Groups[1].Value) {
        'BUILD ERROR' { return '&#x26A0; Build blocked' }
        'ENV ERROR' { return '&#x26A0; Environment blocked' }
        'NO MATCH' { return '&#x26A0; No matching tests' }
        'NEW SNAPSHOT' { return '&#x26A0; Snapshot baseline missing' }
        'FAIL' {
            if ($WithoutFix) { return "&#x2705; Failed as expected$suffix" }
            return "&#x274C; Failed$suffix"
        }
        'PASS' {
            if ($WithoutFix) { return "&#x26A0; Passed without the fix$suffix" }
            return "&#x2705; Passed$suffix"
        }
        default {
            Write-Warning 'An unrecognized diagnostic phase outcome is displayed as not verified.'
            return 'Not verified'
        }
    }
}

function ConvertTo-GateTestName {
    param([string]$Name)

    $text = ($Name -replace '[\x00-\x1F\x7F]', ' ').Trim()
    if ($text.Length -gt 120) { $text = $text.Substring(0, 117) + '...' }
    $text = $text.Replace('|', '\|')
    $delimiter = '`'
    while ($text.Contains($delimiter)) { $delimiter += '`' }
    return "$delimiter $text $delimiter"
}

function Get-GateDiagnosticSummary {
    param([string]$Report, [switch]$FailureOnly)

    # Rebuild only known report fields; artifact-owned Markdown is never rendered.
    $scan = $Report.Substring(0, [Math]::Min($Report.Length, 256KB))
    $rows = [Collections.Generic.List[object]]::new()
    $logs = [Collections.Generic.List[object]]::new()
    $table = $false
    $insideLog = $false
    $phase = 'Execution'
    $logTest = ''
    $log = [Text.StringBuilder]::new()
    $totalGroups = 0
    $invalidRow = $false
    foreach ($line in ($scan -split '\r?\n')) {
        if ($line -match '^```(?:[A-Za-z0-9_-]+)?\s*$') {
            if ($insideLog -and $log.Length -gt 0 -and $logs.Count -lt 4) {
                $logs.Add([pscustomobject]@{ Phase = $phase; Test = $logTest; Text = $log.ToString().TrimEnd() })
            }
            $insideLog = -not $insideLog
            [void]$log.Clear()
            continue
        }
        if ($insideLog) {
            if ($log.Length -lt 2000 -and $logs.Count -lt 4) {
                [void]$log.AppendLine($line.Substring(0, [Math]::Min($line.Length, 2000 - $log.Length)))
            }
            continue
        }
        if ($line -match '^<summary>.*<strong>(Without fix|With fix)</strong>') {
            $phase = $Matches[1]
            $logTest = ''
            foreach ($candidate in ($rows | Sort-Object { $_.PlainName.Length } -Descending)) {
                if ($line.Contains("$($candidate.PlainName):")) {
                    $logTest = $candidate.Name
                    break
                }
            }
        }
        if ($FailureOnly -and $logs.Count -lt 4 -and
            $line -match '^-\s+\*\*(?<test>[^*\r\n]{1,200})\*\*:\s+`(?<error>[^\r\n]{1,2048})`$') {
            $logs.Add([pscustomobject]@{
                Phase = 'Without fix'
                Test = ConvertTo-GateTestName -Name $Matches['test']
                Text = $Matches['error']
            })
        }
        if ($line.Trim() -in @(
            '| Test | Without Fix (expect FAIL) | With Fix (expect PASS) |',
            '| Test | Type | Outcome |'
        )) {
            $table = $true
            continue
        }
        if (-not $table) { continue }
        if ($line -match '^\|[-| ]+\|$') { continue }
        if (-not $line.StartsWith('|')) {
            $table = $false
            continue
        }
        $row = [regex]::Match($line,
            '^\|\s*(?<test>.{1,2048}?)\s*\|\s*(?<before>[^|\r\n]{1,256})\s*\|\s*(?<after>[^|\r\n]{1,256})\s*\|\s*$')
        if (-not $row.Success) {
            $invalidRow = $true
            continue
        }
        $test = $row.Groups['test'].Value.Trim()
        $name = if ($FailureOnly) {
            [regex]::Match($test, '^`(?<name>[^`\r\n]+)`$')
        } else {
            [regex]::Match($test, '\*\*(?<name>[^*\r\n]+)\*\*')
        }
        if (-not $name.Success) {
            $invalidRow = $true
            continue
        }
        $totalGroups++
        if ($rows.Count -ge 20) { continue }
        $type = 'Other'
        if ($FailureOnly) {
            if ($row.Groups['before'].Value.Trim() -in @('UnitTest', 'XamlUnitTest', 'DeviceTest', 'UITest')) {
                $type = $row.Groups['before'].Value.Trim()
            }
            $before = ConvertTo-GatePhaseCell -Value $row.Groups['after'].Value -WithoutFix
            $after = 'Not run (test-only change)'
        } else {
            foreach ($entry in @(
                @{ Type = 'UnitTest'; Icon = 0x1F9EA },
                @{ Type = 'XamlUnitTest'; Icon = 0x1F4C4 },
                @{ Type = 'DeviceTest'; Icon = 0x1F4F1 },
                @{ Type = 'UITest'; Icon = 0x1F5A5 }
            )) {
                if ($test.StartsWith([char]::ConvertFromUtf32($entry.Icon))) {
                    $type = $entry.Type
                    break
                }
            }
            $before = ConvertTo-GatePhaseCell -Value $row.Groups['before'].Value -WithoutFix
            $after = ConvertTo-GatePhaseCell -Value $row.Groups['after'].Value
        }
        $rows.Add([pscustomobject]@{
            Type = $type
            PlainName = $name.Groups['name'].Value
            Name = ConvertTo-GateTestName -Name $name.Groups['name'].Value
            Before = $before
            After = $after
        })
    }
    if ($invalidRow) {
        Write-Warning 'Some diagnostic rows could not be parsed; the run link retains the complete evidence.'
    }
    if ($insideLog -and $log.Length -gt 0 -and $logs.Count -lt 4) {
        $logs.Add([pscustomobject]@{ Phase = $phase; Test = $logTest; Text = $log.ToString().TrimEnd() })
    }
    return [pscustomobject]@{
        Rows = $rows.ToArray()
        Logs = $logs.ToArray()
        Truncated = $scan.Length -lt $Report.Length -or $totalGroups -gt $rows.Count
    }
}

$firstLine = ($report -split '\r?\n', 2)[0].Trim()
$failureOnly = $firstLine -eq '## Gate: Test Verification (Failure-Only Mode)'
$tableStart = $report.IndexOf('| Test |', [StringComparison]::Ordinal)
$preambleLength = if ($tableStart -ge 0) { $tableStart } else { $report.Length }
$preamble = $report.Substring(0, [Math]::Min($preambleLength, 8192))
$compileCoupled = $preamble -match '(?m)^[^A-Za-z\r\n]*\*\*Verified \(new API / feature\)\*\*'
$summary = Get-GateDiagnosticSummary -Report $report -FailureOnly:$failureOnly
$isCurrent = $ReviewedCommit -and $pr.head.sha -ceq $ReviewedCommit
$verdict = switch ($TrustedGateResult) {
    'PASSED' {
        if ($failureOnly) {
            '&#x2705; **Passed (failure-only).** The selected tests failed without a product fix. No with-fix result was verified.'
        } elseif ($compileCoupled) {
            '&#x2705; **Passed (compilation-dependent baseline).** The tests require API added by the fix, so the without-fix baseline could not compile. The with-fix run passed; no runtime failure-to-pass reproduction is claimed.'
        } else {
            '&#x2705; **Passed.** The selected verification satisfied the gate''s passing conditions. Unmatched, skipped, or inconclusive groups are not passing evidence.'
        }
    }
    'FAILED' {
        '&#x274C; **Failed.** The selected tests did not meet the expected outcomes. Inspect the evidence before changing the test or fix.'
    }
    'SKIPPED' {
        '&#x26A0; **Skipped.** No runnable PR tests were selected. The fix has not been verified by this gate.'
    }
    'INCONCLUSIVE' {
        if ($SetupResult -eq 'MERGE_CONFLICT') {
            '&#x26A0; **Inconclusive.** The PR could not be merged with its target branch; no tests ran.'
        } else {
            '&#x26A0; **Inconclusive.** Setup, build, or execution did not produce a conclusive result. This does not prove the fix is incorrect.'
        }
    }
    'TIMEDOUT' {
        '&#x26A0; **Timed out.** The gate stopped before publishing its verdict. Partial diagnostics do not verify the fix.'
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

$analysis = [Collections.Generic.List[string]]::new()
foreach ($group in @(
    @{ Type = 'UnitTest'; Title = '&#x1F9EA; Unit tests' },
    @{ Type = 'XamlUnitTest'; Title = '&#x1F4C4; XAML unit tests' },
    @{ Type = 'DeviceTest'; Title = '&#x1F4F1; Device tests' },
    @{ Type = 'UITest'; Title = '&#x1F5A5; UI tests' },
    @{ Type = 'Other'; Title = 'Selected tests' }
)) {
    $rows = @($summary.Rows | Where-Object { $_.Type -eq $group.Type })
    if ($rows.Count -eq 0) { continue }
    $analysis.Add('<details>')
    $analysis.Add("<summary><strong>$($group.Title)</strong></summary>")
    $analysis.Add('<br/>')
    $analysis.Add('')
    $analysis.Add('| Test group | Without fix | With fix |')
    $analysis.Add('|---|---|---|')
    foreach ($row in $rows) {
        $analysis.Add("| $($row.Name) | $($row.Before) | $($row.After) |")
    }
    $analysis.Add('')
    $analysis.Add('</details>')
    $analysis.Add('')
}
if ($summary.Rows.Count -eq 0) {
    $analysis.Add('No interpretable test-group summary was produced. No additional test execution is inferred.')
}
if ($summary.Truncated) {
    $analysis.Add('Only a bounded selection of test groups is shown; the run retains the complete report.')
}
if ($report -match '(?m)^#### .*Gate coverage limitations') {
    $analysis.Add('**Coverage gap:** some detected groups were omitted by the verifier. Inspect the complete report before treating this as full coverage.')
}
if ($summary.Logs.Count -gt 0) {
    $analysis.Add('<details>')
    $analysis.Add('<summary><strong>Execution log excerpts</strong></summary>')
    $analysis.Add('<br/>')
    $analysis.Add('')
    foreach ($log in $summary.Logs) {
        $analysis.Add("**$($log.Phase)** $($log.Test)")
        $analysis.Add('')
        $analysis.Add('<pre><code>' + [Net.WebUtility]::HtmlEncode($log.Text) + '</code></pre>')
        $analysis.Add('')
    }
    $analysis.Add('</details>')
}
$platformName = switch ($Platform) {
    'android' { 'Android' }
    'ios' { 'iOS' }
    'catalyst' { 'MacCatalyst' }
    'windows' { 'Windows' }
}
$resultColor = switch ($TrustedGateResult) {
    'PASSED' { '2da44e' }
    'FAILED' { 'cf222e' }
    default { '9a6700' }
}
$intro = if ($ReviewedCommit) {
    "> @$($pr.user.login) &#x2014; existing-test verification for commit [``$($ReviewedCommit.Substring(0, 8))``](https://github.com/dotnet/maui/commit/$ReviewedCommit)."
} else {
    "> @$($pr.user.login) &#x2014; existing-test verification; Setup did not capture a tested commit."
}
$commitBadge = if ($ReviewedCommit) {
    "  <img alt=`"Commit $($ReviewedCommit.Substring(0, 8))`" src=`"https://img.shields.io/badge/Commit-$($ReviewedCommit.Substring(0, 8))-1f6feb?labelColor=30363d&amp;style=flat-square`">"
} else { '' }

$marker = '<!-- AI Gate -->'
$runMarker = "<!-- pr-gate-run:$RunId -->"
$prefix = "$marker`n$runMarker"
$body = @(
    $prefix, '', '## PR Test Gate', '',
    $intro, '',
    '<p align="left">',
    '  <img alt="Scope Existing PR tests" src="https://img.shields.io/badge/Scope-Existing%20PR%20tests-1f6feb?labelColor=30363d&amp;style=flat-square">',
    "  <img alt=`"Result $TrustedGateResult`" src=`"https://img.shields.io/badge/Result-$TrustedGateResult-$resultColor`?labelColor=30363d&amp;style=flat-square`">",
    "  <img alt=`"Platform $platformName`" src=`"https://img.shields.io/badge/Platform-$platformName-1f6feb?labelColor=30363d&amp;style=flat-square`">",
    $commitBadge,
    '</p>', '',
    '---', '',
    '<details>',
    '<summary><strong>&#x1F9EA; Gate analysis</strong> &#x2014; click to expand</summary>',
    '<br/>', '',
    $verdict, '',
    $staleNote, '',
    ($analysis -join "`n"), '',
    $commitNote, '', $baseNote, '',
    '**Scope:** Selected PR tests only, with bounded native coverage. The table summarizes diagnostic evidence; the gate verdict above is authoritative.', '',
    "[Complete report and execution logs](https://dev.azure.com/DevDiv/DevDiv/_build/results?buildId=$RunId) &#x2014; ``GateLogs`` and ``BuildLogs`` artifacts.", '',
    '</details>', '', '---', '',
    '<details>',
    '<summary><strong>&#x1F9ED; Follow-up</strong> &#x2014; actions and refresh</summary>',
    '<br/>', '',
    "**Next action:** $nextStep", '',
    "> Maintainers: comment ``/review gate --platform $Platform`` to refresh this report.", '',
    'This is test verification, not a code review or merge recommendation. No alternative fix, generated test, native recording, approval, or PR metadata edit is claimed.',
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
    $labelName = switch ($TrustedGateResult) {
        'PASSED' { 's/agent-gate-passed' }
        'FAILED' { 's/agent-gate-failed' }
        default { '' }
    }
    if ($labelName) {
        # Publishing a gate signal must not edit repository-wide label definitions.
        Invoke-GhCommandWithRetry `
            -Arguments @('api', "repos/dotnet/maui/labels/$([uri]::EscapeDataString($labelName))") `
            -Description "read existing gate signal '$labelName'" -RequireOutput | Out-Null
    }
    foreach ($label in @('s/agent-gate-passed', 's/agent-gate-failed')) {
        if ($label -eq $labelName) { continue }
        if (-not (Remove-Label -PRNumber $PRNumber -LabelName $label)) {
            throw "Gate comment was published, but clearing signal '$label' failed."
        }
    }
    if ($labelName -and -not (Add-Label -PRNumber $PRNumber -LabelName $labelName)) {
        throw "Gate comment was published, but applying signal '$labelName' failed."
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

# Removing an already-absent signal accepts HTTP 404 but leaves gh's native exit code.
exit 0
