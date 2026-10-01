#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Trusted orchestration for the gh-aw /review performance workflow.
.DESCRIPTION
    Gather pins the target and selects coverage without executing PR code. Measure
    runs in a separate disposable Linux job. Prepare imports bounded evidence on
    a fresh runner. Render replaces the agent's comment with a validated report
    immediately before gh-aw safe outputs publishes it.
#>
param(
    [Parameter(Mandatory)]
    [ValidateSet('Gather', 'Measure', 'Prepare', 'Render')]
    [string]$Stage,

    [Parameter(Mandatory)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PrNumber,

    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
    [string]$Repository,

    [Parameter(Mandatory)]
    [string]$OutputDirectory,

    [string]$ContextDirectory,
    [string]$MeasurementDirectory,
    [string]$AgentOutputPath
)

$ErrorActionPreference = 'Stop'
$scripts = Join-Path $PSScriptRoot '../skills/perf-analysis/scripts'
$policy = Join-Path $PSScriptRoot '../skills/perf-analysis/references/recommendation-policy.json'
. "$PSScriptRoot/shared/Copy-BoundedDiagnosticFile.ps1"
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path

function Invoke-Checked([string]$Command, [string[]]$Arguments) {
    $result = & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Command failed with exit code $LASTEXITCODE."
    }
    return $result
}

function Import-EvidenceFile([string]$Directory, [string]$Name, [long]$MaxBytes = 4MB) {
    $destination = Join-Path $OutputDirectory $Name
    $result = Copy-BoundedDiagnosticFile -Source (Join-Path $Directory $Name) `
        -Destination $destination -MaxBytes $MaxBytes
    if ($result.Truncated) {
        throw "Performance evidence '$Name' exceeds its $MaxBytes-byte limit."
    }
    return $destination
}

function Read-Metadata([string]$Path) {
    $metadata = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    if ($metadata.number -ne $PrNumber -or $metadata.repository -cne $Repository) {
        throw 'Performance evidence does not match the authorized target.'
    }
    foreach ($sha in @($metadata.mergeBaseOid, $metadata.headRefOid, $metadata.harnessSha)) {
        if ($sha -cnotmatch '^[0-9a-f]{40}$') {
            throw 'Performance evidence requires full immutable commit identities.'
        }
    }
    return $metadata
}

function Resolve-Decision {
    $arguments = @(
        '-NoProfile', '-File', "$scripts/Resolve-PerfDecision.ps1",
        '-SelectionPath', "$OutputDirectory/selection.json",
        '-PolicyPath', $policy,
        '-OutputPath', "$OutputDirectory/decision-baseline.json"
    )
    if (Test-Path -LiteralPath "$OutputDirectory/summary.json") {
        $arguments += @('-SummaryPath', "$OutputDirectory/summary.json")
    }
    Invoke-Checked pwsh $arguments | Out-Host
}

switch ($Stage) {
    'Gather' {
        $pr = (Invoke-Checked gh @('api', "repos/$Repository/pulls/$PrNumber") | Out-String) | ConvertFrom-Json
        if ($pr.state -ne 'open') {
            Write-Host "PR #$PrNumber is closed; no performance review is needed."
            if ($env:GITHUB_OUTPUT) { 'ready=false' >> $env:GITHUB_OUTPUT }
            exit 0
        }
        foreach ($sha in @($pr.base.sha, $pr.head.sha)) {
            if ($sha -cnotmatch '^[0-9a-f]{40}$') { throw 'GitHub returned an invalid commit identity.' }
        }
        Invoke-Checked git @('fetch', '--quiet', '--no-tags', 'origin', $pr.base.sha, $pr.head.sha) | Out-Host
        $mergeBase = (Invoke-Checked git @('merge-base', $pr.base.sha, $pr.head.sha) | Out-String).Trim()
        $harness = (Invoke-Checked git @('rev-parse', 'HEAD') | Out-String).Trim()
        $metadata = [ordered]@{
            repository = $Repository
            number = $PrNumber
            state = 'OPEN'
            baseRefName = $pr.base.ref
            baseRefOid = $pr.base.sha
            mergeBaseOid = $mergeBase
            headRefOid = $pr.head.sha
            harnessSha = $harness
            author = $pr.user.login
            url = $pr.html_url
        }
        $metadata | ConvertTo-Json | Set-Content -LiteralPath "$OutputDirectory/pr-resolved.json" -Encoding utf8
        $null = Read-Metadata "$OutputDirectory/pr-resolved.json"
        # Include both sides of renames so moving a shipping file cannot hide its old path.
        $files = Invoke-Checked git @('-c', 'core.quotePath=false', 'diff', '--no-ext-diff',
            '--no-textconv', '--no-renames', '--name-only', $mergeBase, $pr.head.sha, '--')
        $files | Set-Content -LiteralPath "$OutputDirectory/changed-files.txt" -Encoding utf8
        & pwsh -NoProfile -File "$scripts/Select-Benchmarks.ps1" `
            -ChangedFilesPath "$OutputDirectory/changed-files.txt" -OutputPath "$OutputDirectory/selection.json"
        if ($LASTEXITCODE -notin @(0, 3)) { throw "Benchmark selection failed: $LASTEXITCODE." }
        $selection = Get-Content -LiteralPath "$OutputDirectory/selection.json" -Raw | ConvertFrom-Json
        $ready = $selection.coverage.productFileCount -gt 0
        if ($env:GITHUB_OUTPUT) { "ready=$($ready.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT }
        if (-not $ready) {
            Write-Host 'No changed product files; skipping performance review.'
            exit 0
        }
        $diff = Invoke-Checked git @('diff', '--no-ext-diff', '--no-textconv',
            '--no-renames', '--unified=15', $mergeBase, $pr.head.sha, '--', 'src')
        $diff | Set-Content -LiteralPath "$OutputDirectory/pr.diff" -Encoding utf8
        if ((Get-Item -LiteralPath "$OutputDirectory/pr.diff").Length -gt 4MB) {
            throw 'The product diff exceeds the hosted review limit (4 MB); review locally.'
        }
    }
    'Measure' {
        $prPath = Import-EvidenceFile $ContextDirectory 'pr-resolved.json'
        $selectionPath = Import-EvidenceFile $ContextDirectory 'selection.json'
        $pr = Read-Metadata $prPath
        if (-not $IsLinux) { throw 'Hosted measurements require LinuxUsers isolation on Linux.' }
        Invoke-Checked git @('fetch', '--quiet', '--no-tags', 'origin', $pr.mergeBaseOid, $pr.headRefOid) | Out-Host
        & pwsh -NoProfile -File "$scripts/Invoke-PerfBenchmarks.ps1" `
            -PrNumber $PrNumber -SuitesPath $selectionPath -PrMetadataPath $prPath `
            -OutputRoot "$OutputDirectory/run" -IsolationMode LinuxUsers -RunsPerSide 2 -Job short
        $runnerExit = $LASTEXITCODE
        if ($runnerExit -ne 0) {
            Write-Warning "Managed measurement failed with exit code $runnerExit; preserving incomplete evidence."
        }
        $null = Import-EvidenceFile "$OutputDirectory/run" 'run-manifest.json'
        Invoke-Checked pwsh @('-NoProfile', '-File', "$scripts/Compare-BenchmarkResults.ps1",
            '-BaseDir', "$OutputDirectory/run/results/base", '-HeadDir', "$OutputDirectory/run/results/head",
            '-RunManifestPath', "$OutputDirectory/run-manifest.json",
            '-JsonOut', "$OutputDirectory/summary.json", '-MarkdownOut', "$OutputDirectory/table.md") | Out-Host
        if ($runnerExit -ne 0) { exit $runnerExit }
    }
    'Prepare' {
        foreach ($name in @('pr-resolved.json', 'selection.json', 'pr.diff')) {
            $null = Import-EvidenceFile $ContextDirectory $name
        }
        $pr = Read-Metadata "$OutputDirectory/pr-resolved.json"
        $selection = Get-Content -LiteralPath "$OutputDirectory/selection.json" -Raw | ConvertFrom-Json
        if (@($selection.suites).Count -gt 0) {
            $manifestPath = Join-Path $MeasurementDirectory 'run-manifest.json'
            $summaryPath = Join-Path $MeasurementDirectory 'summary.json'
            if ((Test-Path -LiteralPath $manifestPath) -and (Test-Path -LiteralPath $summaryPath)) {
                $manifest = Get-Content -LiteralPath (Import-EvidenceFile $MeasurementDirectory 'run-manifest.json') -Raw |
                    ConvertFrom-Json
                if ($manifest.prNumber -ne $PrNumber -or $manifest.baseSha -cne $pr.mergeBaseOid -or
                    $manifest.headSha -cne $pr.headRefOid -or $manifest.isolationMode -cne 'LinuxUsers' -or
                    $manifest.credentialsSanitized -ne $true -or $manifest.runsPerSide -ne 2) {
                    throw 'Managed evidence does not match the pinned isolated run.'
                }
                foreach ($name in @('summary.json', 'table.md')) {
                    $null = Import-EvidenceFile $MeasurementDirectory $name
                }
            } else {
                Write-Warning 'Managed evidence is missing; recording an inconclusive comparison.'
                Invoke-Checked pwsh @('-NoProfile', '-File', "$scripts/Compare-BenchmarkResults.ps1",
                    '-BaseDir', "$OutputDirectory/missing-base", '-HeadDir', "$OutputDirectory/missing-head",
                    '-JsonOut', "$OutputDirectory/summary.json", '-MarkdownOut', "$OutputDirectory/table.md") | Out-Host
            }
        }
        Resolve-Decision
    }
    'Render' {
        foreach ($name in @('pr-resolved.json', 'selection.json')) {
            $null = Import-EvidenceFile $ContextDirectory $name
        }
        foreach ($name in @('summary.json', 'table.md')) {
            if (Test-Path -LiteralPath (Join-Path $ContextDirectory $name)) {
                $null = Import-EvidenceFile $ContextDirectory $name
            }
        }
        $pr = Read-Metadata "$OutputDirectory/pr-resolved.json"
        $live = (Invoke-Checked gh @('api', "repos/$Repository/pulls/$PrNumber") | Out-String) | ConvertFrom-Json
        if ($live.state -ne 'open' -or $live.head.sha -cne $pr.headRefOid -or $live.base.sha -cne $pr.baseRefOid) {
            throw 'The PR was closed or its base/head changed; refusing to publish stale performance evidence.'
        }
        $payloadPath = Join-Path $OutputDirectory 'agent-output.json'
        $copy = Copy-BoundedDiagnosticFile -Source $AgentOutputPath -Destination $payloadPath -MaxBytes 256KB
        if ($copy.Truncated) { throw 'The agent response exceeds the performance report limit.' }
        $payload = Get-Content -LiteralPath $payloadPath -Raw | ConvertFrom-Json
        $items = @($payload.items)
        if ($items.Count -ne 1 -or $items[0].type -ne 'add_comment' -or $null -eq $items[0].data.narrative) {
            throw 'Expected exactly one add_comment carrying a performance narrative.'
        }
        $items[0].data.narrative | ConvertTo-Json -Depth 15 |
            Set-Content -LiteralPath "$OutputDirectory/narrative.json" -Encoding utf8
        Resolve-Decision
        $common = @('-SelectionPath', "$OutputDirectory/selection.json", '-PolicyPath', $policy,
            '-DecisionBaselinePath', "$OutputDirectory/decision-baseline.json",
            '-PrMetadataPath', "$OutputDirectory/pr-resolved.json")
        if (Test-Path -LiteralPath "$OutputDirectory/summary.json") {
            $common += @('-SummaryPath', "$OutputDirectory/summary.json")
        }
        $render = @('-NoProfile', '-File', "$scripts/New-PerformanceReport.ps1") + $common +
            @('-NarrativePath', "$OutputDirectory/narrative.json", '-OutputPath', "$OutputDirectory/report.md")
        if (Test-Path -LiteralPath "$OutputDirectory/table.md") {
            $render += @('-TablePath', "$OutputDirectory/table.md")
        }
        Invoke-Checked pwsh $render | Out-Host
        Invoke-Checked pwsh (@('-NoProfile', '-File', "$scripts/Validate-PerformanceReport.ps1") + $common +
            @('-ReportPath', "$OutputDirectory/report.md", '-JsonOut', "$OutputDirectory/report-validation.json")) | Out-Host
        $report = Get-Content -LiteralPath "$OutputDirectory/report.md" -Raw
        if ($report.Length -gt 60000) {
            throw 'The validated performance report exceeds the GitHub comment limit; inspect the workflow artifacts.'
        }
        $items[0].body = $report
        $items[0].PSObject.Properties.Remove('data')
        $payload | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $AgentOutputPath -Encoding utf8
    }
}
