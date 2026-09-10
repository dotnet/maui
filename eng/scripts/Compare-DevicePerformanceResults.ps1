#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compares parsed base/head MAUI device-performance results.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ResultsPath,

    [Parameter(Mandatory = $false)]
    [string]$MarkdownOut = "-",

    [Parameter(Mandatory = $false)]
    [string]$JsonOut,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedRepository,

    [Parameter(Mandatory = $true)]
    [int]$ExpectedPullRequestNumber,

    [Parameter(Mandatory = $false)]
    [ValidatePattern('\A(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?(?:\[bot\])?)?\z')]
    [string]$PullRequestAuthor = "",

    [Parameter(Mandatory = $true)]
    [string]$ExpectedBaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedHeadCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedHarnessSha,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedPlatform,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedScenario,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 10)]
    [int]$ExpectedVariantRuns = 2,

    [Parameter(Mandatory = $false)]
    [double]$TimePctTolerance = 15
)

$ErrorActionPreference = "Stop"

function Get-Median([double[]]$values) {
    $sorted = @($values | Sort-Object)
    $middle = [int][Math]::Floor($sorted.Count / 2)
    if ($sorted.Count % 2 -eq 1)
    {
        return [double]$sorted[$middle]
    }

    return ([double]$sorted[$middle - 1] + [double]$sorted[$middle]) / 2
}

function Get-Statistics($variantResults) {
    $measurements = @(
        $variantResults |
            ForEach-Object { $_.measurementsMilliseconds } |
            ForEach-Object { [double]$_ }
    )
    if ($measurements.Count -eq 0)
    {
        throw "A performance result variant contains no measurements."
    }

    return [PSCustomObject]@{
        Minimum = [double]($measurements | Measure-Object -Minimum).Minimum
        Maximum = [double]($measurements | Measure-Object -Maximum).Maximum
        Median = [double](Get-Median $measurements)
        Count = $measurements.Count
    }
}

function Get-Percent([double]$from, [double]$to) {
    if ($from -eq 0)
    {
        return $(if ($to -eq 0) { 0 } else { 100 })
    }

    return (($to - $from) / [Math]::Abs($from)) * 100
}

function Format-Milliseconds([double]$value) {
    return $value.ToString("N2", [Globalization.CultureInfo]::InvariantCulture) + " ms"
}

function ConvertTo-ReportText([string]$value) {
    $encoded = [Net.WebUtility]::HtmlEncode(($value -replace '\s+', ' '))
    return $encoded.Replace("|", "&#124;").Replace('`', "&#96;").Replace("[", "&#91;").Replace("]", "&#93;").Replace("@", "&#64;")
}

function Get-UniqueValues($items, [string]$propertyPath) {
    $values = foreach ($item in $items) {
        $value = $item
        foreach ($segment in $propertyPath.Split('.')) {
            if ($null -eq $value) {
                break
            }
            $property = $value.PSObject.Properties[$segment]
            $value = if ($null -ne $property) { $property.Value } else { $null }
        }
        if ($null -ne $value) {
            "$value"
        }
    }
    return @($values | Sort-Object -Unique)
}

if (-not (Test-Path $ResultsPath))
{
    throw "Results file does not exist: $ResultsPath"
}

$parsed = Get-Content $ResultsPath -Raw | ConvertFrom-Json
$results = @($parsed | ForEach-Object { $_ })
$grouped = @($results | Group-Object { "$($_.scenario)|$($_.platform)" })
$comparisons = New-Object System.Collections.Generic.List[object]

foreach ($group in $grouped)
{
    $baseResults = @($group.Group | Where-Object { $_.variant -eq "base" })
    $headResults = @($group.Group | Where-Object { $_.variant -eq "head" })
    $parts = $group.Name.Split('|', 2)
    $provenanceErrors = New-Object System.Collections.Generic.List[string]
    $correctnessErrors = New-Object System.Collections.Generic.List[string]

    if ($baseResults.Count -ne $ExpectedVariantRuns) {
        $provenanceErrors.Add("Expected $ExpectedVariantRuns base results, found $($baseResults.Count).")
    }
    if ($headResults.Count -ne $ExpectedVariantRuns) {
        $provenanceErrors.Add("Expected $ExpectedVariantRuns head results, found $($headResults.Count).")
    }

    $baseCommits = @($baseResults.commitSha | Sort-Object -Unique)
    $headCommits = @($headResults.commitSha | Sort-Object -Unique)
    if ($baseCommits.Count -ne 1 -or $baseCommits[0] -ne $ExpectedBaseCommitSha) {
        $provenanceErrors.Add("Base results do not match expected commit '$ExpectedBaseCommitSha'.")
    }
    if ($headCommits.Count -ne 1 -or $headCommits[0] -ne $ExpectedHeadCommitSha) {
        $provenanceErrors.Add("Head results do not match expected commit '$ExpectedHeadCommitSha'.")
    }

    $allResults = @($baseResults + $headResults)
    $expectedProperties = @(
        @{ Path = "repository"; Expected = $ExpectedRepository; Name = "repository" },
        @{ Path = "pullRequestNumber"; Expected = "$ExpectedPullRequestNumber"; Name = "PR number" },
        @{ Path = "harnessSha"; Expected = $ExpectedHarnessSha; Name = "harness SHA" },
        @{ Path = "platform"; Expected = $ExpectedPlatform; Name = "platform" },
        @{ Path = "scenario"; Expected = $ExpectedScenario; Name = "scenario" },
        @{ Path = "expectedVariantRuns"; Expected = "$ExpectedVariantRuns"; Name = "expected run count" }
    )
    foreach ($expectedProperty in $expectedProperties) {
        $values = @(Get-UniqueValues $allResults $expectedProperty.Path)
        if ($values.Count -ne 1 -or $values[0] -ne $expectedProperty.Expected) {
            $provenanceErrors.Add("Results do not match expected $($expectedProperty.Name) '$($expectedProperty.Expected)'.")
        }
    }

    foreach ($variant in @(
        @{ Name = "base"; Results = $baseResults },
        @{ Name = "head"; Results = $headResults }
    )) {
        $ordinals = @($variant.Results.runOrdinal | ForEach-Object { [int]$_ } | Sort-Object -Unique)
        $expectedOrdinals = @(1..$ExpectedVariantRuns)
        if (($ordinals -join ",") -ne ($expectedOrdinals -join ",")) {
            $provenanceErrors.Add("$($variant.Name) run ordinals must be 1..$ExpectedVariantRuns.")
        }
    }

    if (@($allResults | Where-Object { $_.correctness.passed -ne $true }).Count -gt 0) {
        $correctnessErrors.Add("One or more device results failed operation-level correctness validation.")
    }

    if ($parts[0] -eq "collectionview-grouped-scrollto-makevisible") {
        $positionCountersComplete = @(
            $allResults | Where-Object {
                $null -eq $_.counters.PSObject.Properties["targetPositionSpread"] -or
                $null -eq $_.counters.PSObject.Properties["positionsOutsideTolerance"]
            }
        ).Count -eq 0

        if (-not $positionCountersComplete) {
            $correctnessErrors.Add("Grouped ScrollTo results are missing final-position consistency counters.")
        } elseif (@(
            $headResults | Where-Object {
                [double]$_.counters.positionsOutsideTolerance -ne 0
            }
        ).Count -gt 0) {
            $correctnessErrors.Add("The head did not produce a consistent grouped ScrollTo final position.")
        }
    }

    if ($parts[0] -eq "collectionview-keepitemsinview-update") {
        $itemUpdateCountersComplete = @(
            $allResults | Where-Object {
                $null -eq $_.counters.PSObject.Properties["lastFirstVisiblePosition"] -or
                $null -eq $_.counters.PSObject.Properties["lastExpectedFirstVisiblePosition"] -or
                $null -eq $_.counters.PSObject.Properties["updatesPreservingFirstVisibleItem"]
            }
        ).Count -eq 0

        if (-not $itemUpdateCountersComplete) {
            $correctnessErrors.Add("KeepItemsInView results are missing final-position counters.")
        } elseif (@(
            $headResults | Where-Object {
                [double]$_.counters.lastFirstVisiblePosition -ne
                    [double]$_.counters.lastExpectedFirstVisiblePosition -or
                [double]$_.counters.updatesPreservingFirstVisibleItem -ne
                    ([double]$_.warmupCount + @($_.measurementsMilliseconds).Count)
            }
        ).Count -gt 0) {
            $correctnessErrors.Add("The head did not preserve the first visible item through every measured update.")
        }
    }

    if ($parts[0] -eq "carouselview-swipe-disabled") {
        if ($parts[1] -eq "android") {
            $touchCountersComplete = @(
                $allResults | Where-Object {
                    $null -eq $_.counters.PSObject.Properties["interceptedTouchEventCount"] -or
                    $null -eq $_.counters.PSObject.Properties["finalPosition"]
                }
            ).Count -eq 0

            if (-not $touchCountersComplete) {
                $correctnessErrors.Add("Android CarouselView results are missing touch-interception counters.")
            } elseif (@(
                $headResults | Where-Object {
                    [double]$_.counters.interceptedTouchEventCount -ne 0 -or
                    [double]$_.counters.finalPosition -ne 0
                }
            ).Count -gt 0) {
                $correctnessErrors.Add("The Android head intercepted a disabled-swipe touch or changed position.")
            }
        } else {
            $layoutCountersComplete = @(
                $allResults | Where-Object {
                    $null -eq $_.counters.PSObject.Properties["embeddedScrollViewCount"] -or
                    $null -eq $_.counters.PSObject.Properties["stateReapplicationFailures"]
                }
            ).Count -eq 0

            if (-not $layoutCountersComplete) {
                $correctnessErrors.Add("Apple CarouselView results are missing layout-state counters.")
            } elseif (@(
                $headResults | Where-Object {
                    [double]$_.counters.embeddedScrollViewCount -le 0 -or
                    [double]$_.counters.stateReapplicationFailures -ne 0
                }
            ).Count -gt 0) {
                $correctnessErrors.Add("The Apple head did not reapply disabled swipe/bounce state.")
            }
        }
    }

    if ($parts[0] -eq "carouselview-wheel-snap-windows") {
        $wheelCountersComplete = @(
            $allResults | Where-Object {
                $null -eq $_.counters.PSObject.Properties["maximumCenterError"] -or
                $null -eq $_.counters.PSObject.Properties["positionsOutsideTolerance"] -or
                $null -eq $_.counters.PSObject.Properties["positionMismatchCount"]
            }
        ).Count -eq 0

        if (-not $wheelCountersComplete) {
            $correctnessErrors.Add("Windows CarouselView results are missing centering counters.")
        } elseif (@(
            $headResults | Where-Object {
                [double]$_.counters.maximumCenterError -gt 1 -or
                [double]$_.counters.positionsOutsideTolerance -ne 0 -or
                [double]$_.counters.positionMismatchCount -ne 0
            }
        ).Count -gt 0) {
            $correctnessErrors.Add("The Windows head did not consistently center wheel-scroll results.")
        }
    }

    if ($parts[0] -eq "handler-property-update-batch") {
        $handlerCountersComplete = @(
            $allResults | Where-Object {
                $null -eq $_.counters.PSObject.Properties["completedUpdateBatches"] -or
                $null -eq $_.counters.PSObject.Properties["nativeValueMismatchCount"]
            }
        ).Count -eq 0

        if (-not $handlerCountersComplete) {
            $correctnessErrors.Add("Handler property-update results are missing correctness counters.")
        } elseif (@(
            $headResults | Where-Object {
                [double]$_.counters.nativeValueMismatchCount -ne 0 -or
                [double]$_.counters.completedUpdateBatches -ne
                    ([double]$_.warmupCount + @($_.measurementsMilliseconds).Count)
            }
        ).Count -gt 0) {
            $correctnessErrors.Add("The head did not complete every handler update with matching native values.")
        }
    }

    $environmentPaths = @(
        "environment.executionKind",
        "environment.deviceModel",
        "environment.osVersion",
        "environment.runtimeFramework",
        "environment.processArchitecture",
        "environment.runtimeVariant",
        "environment.sdkVersion",
        "build.azdoBuildId",
        "build.azdoBuildUrl",
        "build.helixJobId",
        "build.helixWorkItem"
    )
    foreach ($environmentPath in $environmentPaths) {
        $values = @(Get-UniqueValues $allResults $environmentPath)
        if ($values.Count -ne 1 -or [string]::IsNullOrWhiteSpace($values[0])) {
            $provenanceErrors.Add("'$environmentPath' must be present and identical for all results.")
        }
    }

    if ($provenanceErrors.Count -gt 0 -or $correctnessErrors.Count -gt 0) {
        $allErrors = @(
            $provenanceErrors | ForEach-Object { $_ }
            $correctnessErrors | ForEach-Object { $_ }
        )
        $comparisons.Add([PSCustomObject]@{
            Scenario = $parts[0]
            Platform = $parts[1]
            Complete = $false
            ProvenanceValidated = $provenanceErrors.Count -eq 0
            CorrectnessPassed = $correctnessErrors.Count -eq 0
            Flag = "inconclusive"
            Reason = $allErrors -join " "
        })
        continue
    }

    $base = Get-Statistics $baseResults
    $head = Get-Statistics $headResults
    $medianDeltaPct = Get-Percent $base.Median $head.Median
    $rangesDoNotOverlap = $head.Minimum -gt $base.Maximum -or $head.Maximum -lt $base.Minimum

    $flag = "neutral"
    if ($rangesDoNotOverlap -and $medianDeltaPct -ge $TimePctTolerance)
    {
        $flag = "time-regression-advisory"
    }
    elseif ($rangesDoNotOverlap -and $medianDeltaPct -le -$TimePctTolerance)
    {
        $flag = "time-improvement-advisory"
    }

    $counterNames = @(
        @($baseResults | ForEach-Object { $_.counters.PSObject.Properties.Name }) +
        @($headResults | ForEach-Object { $_.counters.PSObject.Properties.Name }) |
            Sort-Object -Unique
    )
    $counters = @(
        foreach ($name in $counterNames)
        {
            $baseValues = @(
                $baseResults |
                    ForEach-Object {
                        $property = $_.counters.PSObject.Properties[$name]
                        if ($null -ne $property) { $property.Value }
                    } |
                    Where-Object { $null -ne $_ } |
                    ForEach-Object { [double]$_ }
            )
            $headValues = @(
                $headResults |
                    ForEach-Object {
                        $property = $_.counters.PSObject.Properties[$name]
                        if ($null -ne $property) { $property.Value }
                    } |
                    Where-Object { $null -ne $_ } |
                    ForEach-Object { [double]$_ }
            )
            $baseValue = if ($baseValues.Count -gt 0) { ($baseValues | Measure-Object -Average).Average } else { $null }
            $headValue = if ($headValues.Count -gt 0) { ($headValues | Measure-Object -Average).Average } else { $null }
            [PSCustomObject]@{
                Name = $name
                Base = if ($null -ne $baseValue) { [double]$baseValue } else { $null }
                Head = if ($null -ne $headValue) { [double]$headValue } else { $null }
                Delta = if ($null -ne $baseValue -and $null -ne $headValue) {
                    [double]$headValue - [double]$baseValue
                } else {
                    $null
                }
            }
        }
    )

    $comparisons.Add([PSCustomObject]@{
        Scenario = $baseResults[0].scenario
        Platform = $baseResults[0].platform
        Complete = $true
        ProvenanceValidated = $true
        CorrectnessPassed = $true
        BaseCommit = $baseCommits[0]
        HeadCommit = $headCommits[0]
        BaseResultCount = $baseResults.Count
        HeadResultCount = $headResults.Count
        Build = [PSCustomObject]@{
            azdoBuildId = $baseResults[0].build.azdoBuildId
            azdoBuildUrl = $baseResults[0].build.azdoBuildUrl
            helixJobId = $baseResults[0].build.helixJobId
            helixWorkItem = $baseResults[0].build.helixWorkItem
        }
        Environment = $baseResults[0].environment
        Base = $base
        Head = $head
        MedianDeltaPct = $medianDeltaPct
        RangesDoNotOverlap = $rangesDoNotOverlap
        Counters = $counters
        Flag = $flag
        Reason = $null
    })
}

$incomplete = @($comparisons | Where-Object { -not $_.Complete })
$regressions = @($comparisons | Where-Object { $_.Flag -eq "time-regression-advisory" })
$improvements = @($comparisons | Where-Object { $_.Flag -eq "time-improvement-advisory" })

$verdict = if ($results.Count -eq 0 -or $incomplete.Count -gt 0) {
    "inconclusive"
} elseif ($regressions.Count -gt 0) {
    "time-regression-advisory"
} elseif ($improvements.Count -gt 0) {
    "time-improvement-advisory"
} else {
    "neutral"
}

$provenanceValidated = $results.Count -gt 0 -and @($comparisons | Where-Object { -not $_.ProvenanceValidated }).Count -eq 0
$correctnessPassed = $results.Count -gt 0 -and @($comparisons | Where-Object { -not $_.CorrectnessPassed }).Count -eq 0
$timingLabels = @{
    "neutral" = "Neutral"
    "time-regression-advisory" = "Regression advisory"
    "time-improvement-advisory" = "Improvement advisory"
    "inconclusive" = "Inconclusive"
}
$timingColor = switch ($verdict) {
    "time-regression-advisory" { "d1242f" }
    "time-improvement-advisory" { "1a7f37" }
    default { "bf8700" }
}
$correctnessLabel = if (-not $provenanceValidated) { "Not assessed" } elseif ($correctnessPassed) { "Passed" } else { "Failed" }
$correctnessColor = if (-not $provenanceValidated) { "6e7781" } elseif ($correctnessPassed) { "1a7f37" } else { "d1242f" }
$platformLabel = switch ($ExpectedPlatform) {
    "ios" { "iOS" }
    "maccatalyst" { "MacCatalyst" }
    "android" { "Android" }
    "windows" { "Windows" }
    default { $ExpectedPlatform }
}
$commitUrl = [Net.WebUtility]::HtmlEncode("https://github.com/$ExpectedRepository/commit/$ExpectedHeadCommitSha")
$shortHead = ConvertTo-ReportText $ExpectedHeadCommitSha.Substring(0, [Math]::Min(7, $ExpectedHeadCommitSha.Length))
$recipient = if ($PullRequestAuthor) {
    "@$PullRequestAuthor"
} else {
    $prUrl = [Net.WebUtility]::HtmlEncode("https://github.com/$ExpectedRepository/pull/$ExpectedPullRequestNumber")
    "<a href=`"$prUrl`">PR #$ExpectedPullRequestNumber</a>"
}

$builder = New-Object System.Text.StringBuilder
[void]$builder.AppendLine("## Performance Review Summary")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("> $recipient &mdash; performance review results are available based on commit <a href=`"$commitUrl`"><code>$shortHead</code></a>.")
[void]$builder.AppendLine("")
[void]$builder.AppendLine('<p align="left">')
foreach ($badge in @(
    @{ Label = "Timing"; Value = $timingLabels[$verdict]; Color = $timingColor },
    @{ Label = "Correctness"; Value = $correctnessLabel; Color = $correctnessColor },
    @{ Label = "Platform"; Value = $platformLabel; Color = "1f6feb" }
)) {
    $valueSegment = [Uri]::EscapeDataString($badge.Value.Replace("-", "--").Replace("_", "__"))
    $alt = ConvertTo-ReportText "$($badge.Label) $($badge.Value)"
    [void]$builder.AppendLine("  <img alt=`"$alt`" src=`"https://img.shields.io/badge/$($badge.Label)-$valueSegment-$($badge.Color)?labelColor=30363d&amp;style=flat-square`">")
}
[void]$builder.AppendLine("</p>")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("---")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("<details>")
[void]$builder.AppendLine("<summary><strong>&#128202; Performance Results</strong> &mdash; $($timingLabels[$verdict].ToLowerInvariant())</summary>")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("| Scenario | Base median | Head median | Result |")
[void]$builder.AppendLine("|---|---:|---:|---|")

foreach ($comparison in $comparisons)
{
    $scenario = ConvertTo-ReportText $comparison.Scenario
    if (-not $comparison.Complete)
    {
        [void]$builder.AppendLine("| <code>$scenario</code> | n/a | n/a | Inconclusive |")
        continue
    }

    $baseMedian = Format-Milliseconds $comparison.Base.Median
    $headMedian = Format-Milliseconds $comparison.Head.Median
    $deltaSign = if ($comparison.MedianDeltaPct -gt 0) { "+" } else { "" }
    $delta = $comparison.MedianDeltaPct.ToString("N1", [Globalization.CultureInfo]::InvariantCulture)
    [void]$builder.AppendLine("| <code>$scenario</code> | $baseMedian | $headMedian | $($timingLabels[$comparison.Flag]) ($deltaSign$delta%) |")
}
if ($results.Count -eq 0) {
    [void]$builder.AppendLine("| No device results | n/a | n/a | Inconclusive |")
}

[void]$builder.AppendLine("")
[void]$builder.AppendLine("*Timing is advisory: repeated ranges must not overlap and the median change must reach $TimePctTolerance% to be flagged. Allocation and accessibility coverage are not inferred.*")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("</details>")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("---")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("<details>")
$followUpLabel = if ($verdict -eq "inconclusive") { "investigation required" } elseif ($regressions.Count -gt 0) { "timing increase to investigate" } else { "advisory results" }
[void]$builder.AppendLine("<summary><strong>&#128295; Findings &amp; Follow-up</strong> &mdash; $followUpLabel</summary>")
[void]$builder.AppendLine("")
if ($results.Count -eq 0) {
    [void]$builder.AppendLine("No device results were supplied. Run both variants before drawing conclusions.")
} elseif ($incomplete.Count -gt 0) {
    [void]$builder.AppendLine("**Comparison is inconclusive.** Resolve the reported provenance or correctness failures and rerun:")
    [void]$builder.AppendLine("")
    foreach ($comparison in $incomplete) {
        [void]$builder.AppendLine("- $(ConvertTo-ReportText $comparison.Reason)")
    }
} elseif ($regressions.Count -gt 0) {
    [void]$builder.AppendLine("Investigate the timing increase in the measured scenario before accepting the change.")
} elseif ($improvements.Count -gt 0) {
    [void]$builder.AppendLine("A timing improvement was observed in the measured scenario; it remains advisory.")
} else {
    [void]$builder.AppendLine("No timing regression was demonstrated in the measured scenario.")
}
[void]$builder.AppendLine("")
[void]$builder.AppendLine("These results are not whole-PR merge clearance. Full ranges, counters, and provenance remain in <code>comparison-summary.json</code>.")
[void]$builder.AppendLine("")
[void]$builder.AppendLine("</details>")

$markdown = $builder.ToString()
if ($MarkdownOut -eq "-")
{
    Write-Output $markdown
}
else
{
    $directory = Split-Path -Parent $MarkdownOut
    if ($directory -and -not (Test-Path $directory))
    {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    Set-Content -Path $MarkdownOut -Value $markdown -Encoding UTF8
}

$summary = [PSCustomObject]@{
    schemaVersion = 2
    verdict = $verdict
    timePctTolerance = $TimePctTolerance
    expected = [PSCustomObject]@{
        repository = $ExpectedRepository
        pullRequestNumber = $ExpectedPullRequestNumber
        baseCommitSha = $ExpectedBaseCommitSha
        headCommitSha = $ExpectedHeadCommitSha
        harnessSha = $ExpectedHarnessSha
        platform = $ExpectedPlatform
        scenario = $ExpectedScenario
        variantRuns = $ExpectedVariantRuns
    }
    provenanceValidated = $provenanceValidated
    correctnessPassed = $correctnessPassed
    accessibilityStatuses = @($results.correctness.accessibilityStatus | Sort-Object -Unique)
    comparisons = @($comparisons | ForEach-Object { $_ })
}

if ($JsonOut)
{
    $directory = Split-Path -Parent $JsonOut
    if ($directory -and -not (Test-Path $directory))
    {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    ConvertTo-Json -InputObject $summary -Depth 12 |
        Set-Content -Path $JsonOut -Encoding UTF8
}

Write-Host "Verdict: $verdict"
exit 0
