#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compares repeated BenchmarkDotNet reports for a PR's base and head.

.DESCRIPTION
    Joins benchmarks by identity, compares allocation and timing ranges across repeated
    runs, and emits Markdown plus a machine-readable summary.

    Allocation regressions are confirmed only when head's best run is still worse than
    base's worst run. The reported delta is that non-overlapping gap, not a potentially
    exaggerated best-to-best difference.

    Timing remains advisory. A timing change is flagged only when the run-level ranges do
    not overlap and the median delta exceeds the configured percentage threshold.

    Missing reports, incomplete execution manifests, and disjoint benchmark sets are
    inconclusive. They can never produce a neutral/clean verdict.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BaseDir,

    [Parameter(Mandatory = $true)]
    [string]$HeadDir,

    [Parameter(Mandatory = $false)]
    [string]$RunManifestPath,

    [Parameter(Mandatory = $false)]
    [string]$MarkdownOut = "-",

    [Parameter(Mandatory = $false)]
    [string]$JsonOut,

    [Parameter(Mandatory = $false)]
    [double]$AllocAbsTol = 8,

    [Parameter(Mandatory = $false)]
    [double]$AllocPctTol = 0.5,

    [Parameter(Mandatory = $false)]
    [double]$TimePctTol = 15
)

$ErrorActionPreference = "Stop"

function Write-Info([string]$Message) {
    [Console]::Error.WriteLine($Message)
}

function Get-Median([double[]]$Values) {
    if (-not $Values -or $Values.Count -eq 0) {
        return $null
    }

    $sorted = @($Values | Sort-Object)
    $middle = [int][Math]::Floor($sorted.Count / 2)
    if (($sorted.Count % 2) -eq 1) {
        return [double]$sorted[$middle]
    }

    return ([double]$sorted[$middle - 1] + [double]$sorted[$middle]) / 2
}

function Get-SeriesStats([object[]]$Values) {
    $numbers = @($Values | Where-Object { $null -ne $_ } | ForEach-Object { [double]$_ })
    if ($numbers.Count -eq 0) {
        return $null
    }

    return [PSCustomObject]@{
        Min = [double]($numbers | Measure-Object -Minimum).Minimum
        Max = [double]($numbers | Measure-Object -Maximum).Maximum
        Median = [double](Get-Median $numbers)
        Count = $numbers.Count
    }
}

function Get-BenchmarkKey($Benchmark) {
    $fullName = [string]$Benchmark.FullName
    $parameters = [string]$Benchmark.Parameters

    if ($parameters -and $fullName -notlike "*$parameters*") {
        return "$fullName [$parameters]"
    }

    return $fullName
}

function Read-Benchmarks([string]$Directory) {
    $map = @{}
    if (-not (Test-Path $Directory)) {
        return $map
    }

    $files = Get-ChildItem -Path $Directory -Recurse -Filter '*-report-full*.json' -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        try {
            $document = Get-Content $file.FullName -Raw | ConvertFrom-Json
        }
        catch {
            Write-Info "WARN: invalid BenchmarkDotNet JSON: $($file.FullName)"
            continue
        }

        if (-not $document.Benchmarks) {
            continue
        }

        foreach ($benchmark in $document.Benchmarks) {
            $key = Get-BenchmarkKey $benchmark
            if (-not $map.ContainsKey($key)) {
                $map[$key] = [PSCustomObject]@{
                    Key = $key
                    Means = (New-Object System.Collections.Generic.List[double])
                    Allocs = (New-Object System.Collections.Generic.List[double])
                    Runs = 0
                    MissingStatisticsRuns = 0
                    MissingMemoryRuns = 0
                }
            }

            $entry = $map[$key]
            $entry.Runs++

            if ($benchmark.Statistics -and $null -ne $benchmark.Statistics.Mean) {
                $entry.Means.Add([double]$benchmark.Statistics.Mean)
            }
            else {
                $entry.MissingStatisticsRuns++
            }
            if ($benchmark.Memory -and $null -ne $benchmark.Memory.BytesAllocatedPerOperation) {
                $entry.Allocs.Add([double]$benchmark.Memory.BytesAllocatedPerOperation)
            }
            else {
                $entry.MissingMemoryRuns++
            }
        }
    }

    return $map
}

function Format-Ns($Nanoseconds) {
    if ($null -eq $Nanoseconds) {
        return "n/a"
    }

    $value = [double]$Nanoseconds
    if ($value -lt 1000) {
        return ("{0:N1} ns" -f $value)
    }
    if ($value -lt 1000000) {
        return ("{0:N2} us" -f ($value / 1000))
    }
    if ($value -lt 1000000000) {
        return ("{0:N2} ms" -f ($value / 1000000))
    }

    return ("{0:N2} s" -f ($value / 1000000000))
}

function Format-Bytes($Bytes) {
    if ($null -eq $Bytes) {
        return "n/a"
    }

    $value = [double]$Bytes
    if ($value -lt 1024) {
        return ("{0:N0} B" -f $value)
    }
    if ($value -lt 1048576) {
        return ("{0:N2} KB" -f ($value / 1024))
    }

    return ("{0:N2} MB" -f ($value / 1048576))
}

function Format-Range($Stats, [scriptblock]$Formatter) {
    if ($null -eq $Stats) {
        return "n/a"
    }

    $minimum = & $Formatter $Stats.Min
    if ($Stats.Min -eq $Stats.Max) {
        return $minimum
    }

    $maximum = & $Formatter $Stats.Max
    return "$minimum - $maximum"
}

function Get-Percent([double]$From, [double]$To) {
    if ($From -eq 0) {
        if ($To -eq 0) {
            return 0
        }

        return 100
    }

    return (($To - $From) / [Math]::Abs($From)) * 100
}

$base = Read-Benchmarks $BaseDir
$head = Read-Benchmarks $HeadDir
Write-Info "Base benchmarks: $($base.Count) | Head benchmarks: $($head.Count)"

$baseRuns = if ($base.Count) {
    (@($base.Values) | Measure-Object -Property Runs -Minimum).Minimum
}
else {
    0
}
$headRuns = if ($head.Count) {
    (@($head.Values) | Measure-Object -Property Runs -Minimum).Minimum
}
else {
    0
}
$executionStatus = "not-provided"
$executionComplete = $false
$manifest = $null
$expectedRunsPerSide = 2
if ($RunManifestPath) {
    if (Test-Path $RunManifestPath) {
        try {
            $manifest = Get-Content $RunManifestPath -Raw | ConvertFrom-Json
        }
        catch {
            Write-Info "WARN: invalid benchmark run manifest: $RunManifestPath"
            $executionStatus = "invalid"
        }

        if ($null -ne $manifest) {
            $executionStatus = [string]$manifest.status
            $executionComplete = ($executionStatus -eq "complete")
            if (-not $executionComplete -and
                $null -ne $manifest.PSObject.Properties["suites"] -and
                @($manifest.suites).Count -gt 0) {
                $unexpectedIncompleteSuites = @(
                    $manifest.suites | Where-Object {
                        -not $_.complete -and
                        [string]$_.skipReason -ne "benchmark inputs changed"
                    }
                )
                $completedSuites = @($manifest.suites | Where-Object { $_.complete })
                if ($unexpectedIncompleteSuites.Count -eq 0 -and $completedSuites.Count -gt 0) {
                    $executionComplete = $true
                    $executionStatus = "partial-coverage"
                }
            }
            if ($manifest.runsPerSide) {
                $expectedRunsPerSide = [int]$manifest.runsPerSide
            }
        }
    }
    else {
        $executionStatus = "missing"
        $executionComplete = $false
    }
}

$baseKeys = @($base.Keys | Sort-Object)
$headKeys = @($head.Keys | Sort-Object)
$commonKeys = @($baseKeys | Where-Object { $head.ContainsKey($_) })
$baseOnlyKeys = @($baseKeys | Where-Object { -not $head.ContainsKey($_) })
$headOnlyKeys = @($headKeys | Where-Object { -not $base.ContainsKey($_) })
$benchmarkSetsMatch = ($commonKeys.Count -gt 0) -and ($baseOnlyKeys.Count -eq 0) -and ($headOnlyKeys.Count -eq 0)
$noData = ($base.Count -eq 0) -or ($head.Count -eq 0)
$incompleteBenchmarkData = New-Object System.Collections.Generic.List[object]
foreach ($key in $commonKeys) {
    $baseEntry = $base[$key]
    $headEntry = $head[$key]

    $baseComplete =
        $baseEntry.Runs -eq $expectedRunsPerSide -and
        $baseEntry.Means.Count -eq $expectedRunsPerSide -and
        $baseEntry.Allocs.Count -eq $expectedRunsPerSide -and
        $baseEntry.MissingStatisticsRuns -eq 0 -and
        $baseEntry.MissingMemoryRuns -eq 0
    $headComplete =
        $headEntry.Runs -eq $expectedRunsPerSide -and
        $headEntry.Means.Count -eq $expectedRunsPerSide -and
        $headEntry.Allocs.Count -eq $expectedRunsPerSide -and
        $headEntry.MissingStatisticsRuns -eq 0 -and
        $headEntry.MissingMemoryRuns -eq 0

    if (-not $baseComplete -or -not $headComplete) {
        $incompleteBenchmarkData.Add([PSCustomObject]@{
            name = $key
            expectedRunsPerSide = $expectedRunsPerSide
            baseRuns = $baseEntry.Runs
            baseMeanRuns = $baseEntry.Means.Count
            baseAllocationRuns = $baseEntry.Allocs.Count
            headRuns = $headEntry.Runs
            headMeanRuns = $headEntry.Means.Count
            headAllocationRuns = $headEntry.Allocs.Count
        })
    }
}

$benchmarkDataComplete = ($commonKeys.Count -gt 0) -and ($incompleteBenchmarkData.Count -eq 0)
$allocConfirmed =
    $benchmarkDataComplete -and
    ($baseRuns -ge 2) -and
    ($headRuns -ge 2)
$coverageComplete =
    (-not $noData) -and
    $executionComplete -and
    ($null -eq $manifest -or [string]$manifest.status -eq "complete") -and
    $benchmarkSetsMatch -and
    $benchmarkDataComplete

$allKeys = @($baseKeys + $headKeys | Sort-Object -Unique)
$rows = New-Object System.Collections.Generic.List[object]

foreach ($key in $allKeys) {
    $baseEntry = $base[$key]
    $headEntry = $head[$key]
    $shortName = $key -replace '^Microsoft\.Maui\.(Controls\.Xaml\.|Controls\.|)?Benchmarks\.', ''

    if (-not $baseEntry) {
        $headAlloc = Get-SeriesStats @($headEntry.Allocs)
        $headMean = Get-SeriesStats @($headEntry.Means)
        $rows.Add([PSCustomObject]@{
            Name = $shortName
            Kind = "added"
            AllocBase = $null
            AllocHead = $headAlloc
            MeanBase = $null
            MeanHead = $headMean
            ConfirmedAllocDelta = $null
            ConfirmedAllocPct = $null
            MeanPct = $null
            Flag = "new"
            Sort = 5
        })
        continue
    }

    if (-not $headEntry) {
        $baseAlloc = Get-SeriesStats @($baseEntry.Allocs)
        $baseMean = Get-SeriesStats @($baseEntry.Means)
        $rows.Add([PSCustomObject]@{
            Name = $shortName
            Kind = "removed"
            AllocBase = $baseAlloc
            AllocHead = $null
            MeanBase = $baseMean
            MeanHead = $null
            ConfirmedAllocDelta = $null
            ConfirmedAllocPct = $null
            MeanPct = $null
            Flag = "removed"
            Sort = 5
        })
        continue
    }

    $baseAlloc = Get-SeriesStats @($baseEntry.Allocs)
    $headAlloc = Get-SeriesStats @($headEntry.Allocs)
    $baseMean = Get-SeriesStats @($baseEntry.Means)
    $headMean = Get-SeriesStats @($headEntry.Means)

    $allocFlag = "neutral"
    $confirmedAllocDelta = $null
    $confirmedAllocPct = $null

    if ($baseAlloc -and $headAlloc) {
        $regressionGap = $headAlloc.Min - $baseAlloc.Max
        $improvementGap = $baseAlloc.Min - $headAlloc.Max

        if ($regressionGap -ge $AllocAbsTol) {
            $regressionPct = if ($baseAlloc.Max -eq 0) { 100 } else { ($regressionGap / [Math]::Abs($baseAlloc.Max)) * 100 }
            if ($regressionPct -ge $AllocPctTol) {
                $allocFlag = "alloc-regression"
                $confirmedAllocDelta = $regressionGap
                $confirmedAllocPct = $regressionPct
            }
        }
        elseif ($improvementGap -ge $AllocAbsTol) {
            $improvementPct = if ($baseAlloc.Min -eq 0) { 100 } else { ($improvementGap / [Math]::Abs($baseAlloc.Min)) * 100 }
            if ($improvementPct -ge $AllocPctTol) {
                $allocFlag = "alloc-improvement"
                $confirmedAllocDelta = -$improvementGap
                $confirmedAllocPct = -$improvementPct
            }
        }
    }

    $timeFlag = "neutral"
    $meanPct = $null
    if ($baseMean -and $headMean) {
        $meanPct = Get-Percent $baseMean.Median $headMean.Median
        $rangesDoNotOverlap = ($headMean.Min -gt $baseMean.Max) -or ($headMean.Max -lt $baseMean.Min)

        if ($rangesDoNotOverlap -and $meanPct -ge $TimePctTol) {
            $timeFlag = "time-regression"
        }
        elseif ($rangesDoNotOverlap -and $meanPct -le -$TimePctTol) {
            $timeFlag = "time-improvement"
        }
    }

    $flag = "neutral"
    $sort = 4
    if ($allocFlag -eq "alloc-regression") {
        $flag = "alloc-regression"
        $sort = 0
    }
    elseif ($timeFlag -eq "time-regression") {
        $flag = "time-regression"
        $sort = 1
    }
    elseif ($allocFlag -eq "alloc-improvement") {
        $flag = "alloc-improvement"
        $sort = 2
    }
    elseif ($timeFlag -eq "time-improvement") {
        $flag = "time-improvement"
        $sort = 3
    }

    $rows.Add([PSCustomObject]@{
        Name = $shortName
        Kind = "compared"
        AllocBase = $baseAlloc
        AllocHead = $headAlloc
        MeanBase = $baseMean
        MeanHead = $headMean
        ConfirmedAllocDelta = $confirmedAllocDelta
        ConfirmedAllocPct = $confirmedAllocPct
        MeanPct = $meanPct
        Flag = $flag
        Sort = $sort
    })
}

$rows = @($rows | Sort-Object Sort, Name)
$allocRegressions = @($rows | Where-Object { $_.Flag -eq "alloc-regression" })
$timeRegressions = @($rows | Where-Object { $_.Flag -eq "time-regression" })
$improvements = @($rows | Where-Object { $_.Flag -like "*improvement" })
$allocImprovements = @($rows | Where-Object { $_.Flag -eq "alloc-improvement" })
$timeImprovements = @($rows | Where-Object { $_.Flag -eq "time-improvement" })
$confirmedAllocRegression = ($allocRegressions.Count -gt 0) -and $allocConfirmed
$cleanEvidenceComplete = $coverageComplete -and $allocConfirmed
$canClaimClean =
    $cleanEvidenceComplete -and
    $allocRegressions.Count -eq 0 -and
    $timeRegressions.Count -eq 0

$verdict = if ($confirmedAllocRegression) {
    "alloc-regression"
}
elseif (-not $coverageComplete) {
    "inconclusive"
}
elseif ($allocRegressions.Count -gt 0 -and -not $allocConfirmed) {
    "inconclusive"
}
elseif ($timeRegressions.Count -gt 0) {
    "time-regression-advisory"
}
elseif ($allocImprovements.Count -gt 0 -and $cleanEvidenceComplete) {
    "improvement"
}
elseif ($timeImprovements.Count -gt 0 -and $cleanEvidenceComplete) {
    "time-improvement-advisory"
}
elseif ($canClaimClean) {
    "neutral"
}
else {
    "inconclusive"
}

$labels = @{
    "alloc-regression" = "ALLOC+ regression"
    "time-regression" = "time+ regression*"
    "alloc-improvement" = "ALLOC- improved"
    "time-improvement" = "time- improved*"
    "neutral" = "ok"
    "new" = "new"
    "removed" = "gone"
}

$markdownBuilder = New-Object System.Text.StringBuilder
[void]$markdownBuilder.AppendLine("| Benchmark | Alloc base -> head | Mean base -> head | Flag |")
[void]$markdownBuilder.AppendLine("|---|---|---|---|")

foreach ($row in $rows) {
    $allocCell = "{0} -> {1}" -f
        (Format-Range $row.AllocBase ${function:Format-Bytes}),
        (Format-Range $row.AllocHead ${function:Format-Bytes})

    if ($null -ne $row.ConfirmedAllocDelta) {
        $sign = if ($row.ConfirmedAllocDelta -gt 0) { "+" } else { "-" }
        $allocCell += (" (confirmed {0}{1}, {0}{2:N1}%)" -f
            $sign,
            (Format-Bytes ([Math]::Abs($row.ConfirmedAllocDelta))),
            [Math]::Abs($row.ConfirmedAllocPct))
    }

    $meanCell = "{0} -> {1}" -f
        (Format-Range $row.MeanBase ${function:Format-Ns}),
        (Format-Range $row.MeanHead ${function:Format-Ns})

    if ($null -ne $row.MeanPct) {
        $sign = if ($row.MeanPct -gt 0) { "+" } else { "" }
        $meanCell += (" (median {0}{1:N1}%)" -f $sign, $row.MeanPct)
    }

    [void]$markdownBuilder.AppendLine("| ``$($row.Name)`` | $allocCell | $meanCell | $($labels[$row.Flag]) |")
}

[void]$markdownBuilder.AppendLine("")
[void]$markdownBuilder.AppendLine("Allocation ranges contain every repeated run. A confirmed delta is only the non-overlapping gap between base and head. Timing uses run-level median/ranges and remains advisory.")

if (-not $executionComplete) {
    [void]$markdownBuilder.AppendLine("")
    [void]$markdownBuilder.AppendLine("Execution manifest is incomplete (`$executionStatus`); a clean verdict is not permitted.")
}
if (-not $benchmarkSetsMatch) {
    [void]$markdownBuilder.AppendLine("")
    [void]$markdownBuilder.AppendLine("Benchmark sets differ or have no common rows; a clean verdict is not permitted.")
}
if (-not $benchmarkDataComplete) {
    [void]$markdownBuilder.AppendLine("")
    [void]$markdownBuilder.AppendLine("One or more benchmarks are missing statistics, allocation data, or an expected repeated run; a clean verdict is not permitted.")
}
if (-not $allocConfirmed) {
    [void]$markdownBuilder.AppendLine("")
    [void]$markdownBuilder.AppendLine("Fewer than two reports per side were available; allocation deltas are not confirmed.")
}

$markdown = $markdownBuilder.ToString()
if ($MarkdownOut -eq "-") {
    Write-Output $markdown
}
else {
    $directory = Split-Path -Parent $MarkdownOut
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    Set-Content -Path $MarkdownOut -Value $markdown -Encoding UTF8
    Write-Info "Wrote $MarkdownOut"
}

$summary = [PSCustomObject]@{
    schemaVersion = 2
    verdict = $verdict
    canClaimClean = $canClaimClean
    cleanEvidenceComplete = $cleanEvidenceComplete
    coverageComplete = $coverageComplete
    executionComplete = $executionComplete
    executionStatus = $executionStatus
    benchmarkSetsMatch = $benchmarkSetsMatch
    benchmarkDataComplete = $benchmarkDataComplete
    expectedRunsPerSide = $expectedRunsPerSide
    incompleteBenchmarkData = @($incompleteBenchmarkData | ForEach-Object { $_ })
    noData = $noData
    baseCount = $base.Count
    headCount = $head.Count
    commonCount = $commonKeys.Count
    baseOnlyBenchmarks = @($baseOnlyKeys)
    headOnlyBenchmarks = @($headOnlyKeys)
    baseRuns = $baseRuns
    headRuns = $headRuns
    allocConfirmed = $allocConfirmed
    comparedCount = @($rows | Where-Object { $_.Kind -eq "compared" }).Count
    benchmarks = @(
        $rows | Where-Object { $_.Kind -eq "compared" } | ForEach-Object {
            [PSCustomObject]@{
                name = $_.Name
                flag = $_.Flag
                allocation = [PSCustomObject]@{
                    baseMin = $_.AllocBase.Min
                    baseMedian = $_.AllocBase.Median
                    baseMax = $_.AllocBase.Max
                    headMin = $_.AllocHead.Min
                    headMedian = $_.AllocHead.Median
                    headMax = $_.AllocHead.Max
                }
                time = [PSCustomObject]@{
                    baseMinNs = $_.MeanBase.Min
                    baseMedianNs = $_.MeanBase.Median
                    baseMaxNs = $_.MeanBase.Max
                    headMinNs = $_.MeanHead.Min
                    headMedianNs = $_.MeanHead.Median
                    headMaxNs = $_.MeanHead.Max
                }
            }
        }
    )
    allocRegressions = @(
        $allocRegressions | ForEach-Object {
            [PSCustomObject]@{
                name = $_.Name
                confirmedDeltaBytes = $_.ConfirmedAllocDelta
                confirmedDeltaPct = [Math]::Round($_.ConfirmedAllocPct, 2)
                baseMinBytes = $_.AllocBase.Min
                baseMaxBytes = $_.AllocBase.Max
                headMinBytes = $_.AllocHead.Min
                headMaxBytes = $_.AllocHead.Max
                confirmed = $allocConfirmed
            }
        }
    )
    timeRegressions = @(
        $timeRegressions | ForEach-Object {
            [PSCustomObject]@{
                name = $_.Name
                medianDeltaPct = [Math]::Round($_.MeanPct, 2)
                baseMinNs = $_.MeanBase.Min
                baseMaxNs = $_.MeanBase.Max
                headMinNs = $_.MeanHead.Min
                headMaxNs = $_.MeanHead.Max
                advisory = $true
            }
        }
    )
    improvements = @(
        $improvements | ForEach-Object {
            [PSCustomObject]@{
                name = $_.Name
                flag = $_.Flag
                confirmedDeltaBytes = $_.ConfirmedAllocDelta
                confirmedDeltaPct = if ($null -ne $_.ConfirmedAllocPct) { [Math]::Round($_.ConfirmedAllocPct, 2) } else { $null }
                medianDeltaPct = if ($null -ne $_.MeanPct) { [Math]::Round($_.MeanPct, 2) } else { $null }
            }
        }
    )
    thresholds = [PSCustomObject]@{
        allocAbsTol = $AllocAbsTol
        allocPctTol = $AllocPctTol
        timePctTol = $TimePctTol
    }
}

if ($JsonOut) {
    $directory = Split-Path -Parent $JsonOut
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $summary | ConvertTo-Json -Depth 10 | Set-Content -Path $JsonOut -Encoding UTF8
    Write-Info "Wrote $JsonOut"
}

Write-Info "Verdict: $verdict (coverageComplete=$coverageComplete allocConfirmed=$allocConfirmed)"
