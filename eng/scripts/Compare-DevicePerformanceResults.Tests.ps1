#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Compare-DevicePerformanceResults.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-perf-compare-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual)
    {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Assert-CommentFormat([string]$path) {
    $markdown = Get-Content $path -Raw
    Assert-Equal $true $markdown.StartsWith("## Performance Review Summary") "Comment heading"
    Assert-Equal 3 ([regex]::Matches($markdown, '<img ').Count) "Summary badge count"
    Assert-Equal 2 ([regex]::Matches($markdown, '(?m)^<details>\r?$').Count) "Exactly two closed sections"
    Assert-Equal 2 ([regex]::Matches($markdown, '(?m)^</details>\r?$').Count) "Balanced comment sections"
    Assert-Equal $false ($markdown -match '<details\s') "Sections must not open by default"
    Assert-Equal $true ($markdown -match '<summary><strong>.*Performance Results</strong>') "Performance section title"
    Assert-Equal $true ($markdown -match '<summary><strong>.*Findings &amp; Follow-up</strong>') "Findings section title"
    Assert-Equal $false ($markdown -match 'Test Setup|Review Sessions|Next Steps') "No setup or generic session sections"
    $visible = [regex]::Replace($markdown, '(?s)<details>.*?</details>', '')
    Assert-Equal $false ($visible -match '(?m)^\|') "Results tables must be collapsed"
    Assert-Equal $true ($visible -match 'performance review results are available based on commit') "Visible result notification"
    Assert-Equal $true $visible.Contains('https://github.com/dotnet/maui/commit/def456') "Pinned head link"
    return $markdown
}

function Write-Results([string]$path, [object[]]$results) {
    ConvertTo-Json -InputObject $results -Depth 10 |
        Set-Content -Path $path -Encoding UTF8
}

function New-Result(
    [string]$variant,
    [double[]]$measurements,
    [int]$runOrdinal = 1,
    [string]$harnessSha = "harness123",
    [bool]$correctnessPassed = $true
) {
    return [PSCustomObject]@{
        schemaVersion = 2
        repository = "dotnet/maui"
        pullRequestNumber = 42
        scenario = "collectionview-scroll"
        platform = "ios"
        variant = $variant
        commitSha = if ($variant -eq "base") { "abc123" } else { "def456" }
        harnessSha = $harnessSha
        runOrdinal = $runOrdinal
        expectedVariantRuns = 2
        build = [PSCustomObject]@{
            azdoBuildId = "100"
            azdoBuildUrl = "https://build/100"
            helixJobId = "job"
            helixWorkItem = "work"
        }
        environment = [PSCustomObject]@{
            executionKind = "simulator"
            deviceModel = "iPhone"
            osVersion = "18.5"
            runtimeFramework = ".NET 10"
            processArchitecture = "Arm64"
            runtimeVariant = "mono"
            sdkVersion = "10.0"
        }
        correctness = [PSCustomObject]@{
            passed = $correctnessPassed
            accessibilityStatus = "not-assessed"
        }
        warmupCount = 2
        measurementsMilliseconds = $measurements
        statistics = [PSCustomObject]@{}
        counters = [PSCustomObject]@{ layoutPasses = if ($variant -eq "base") { 10 } else { 5 } }
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try
{
    $resultsPath = Join-Path $testRoot "results.json"
    $summaryPath = Join-Path $testRoot "summary.json"
    $markdownPath = Join-Path $testRoot "summary.md"

    Write-Results $resultsPath @(
        (New-Result "base" @(100, 102, 104, 106) 1),
        (New-Result "head" @(70, 72, 74, 76) 1),
        (New-Result "head" @(71, 73, 75, 77) 2),
        (New-Result "base" @(101, 103, 105, 107) 2)
    )

    $comparisonArguments = @{
        ResultsPath = $resultsPath
        JsonOut = $summaryPath
        MarkdownOut = $markdownPath
        ExpectedRepository = "dotnet/maui"
        ExpectedPullRequestNumber = 42
        ExpectedBaseCommitSha = "abc123"
        ExpectedHeadCommitSha = "def456"
        ExpectedHarnessSha = "harness123"
        ExpectedPlatform = "ios"
        ExpectedScenario = "collectionview-scroll"
        ExpectedVariantRuns = 2
    }
    & $script @comparisonArguments
    Assert-Equal 0 $LASTEXITCODE "Comparator should succeed"

    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "time-improvement-advisory" $summary.verdict "Improvement verdict"
    Assert-Equal -5 $summary.comparisons[0].counters[0].delta "Counter delta"
    Assert-Equal 2 $summary.comparisons[0].baseResultCount "Base ABBA result count"
    Assert-Equal 2 $summary.comparisons[0].headResultCount "Head ABBA result count"
    Assert-Equal $true $summary.provenanceValidated "Provenance validation"
    Assert-Equal $true $summary.correctnessPassed "Correctness validation"
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("Timing-Improvement%20advisory-") "Improvement badge"
    Assert-Equal $true $markdown.Contains("Correctness-Passed-") "Correctness badge"
    Assert-Equal $true $markdown.Contains("PR #42") "Offline report identifies the PR without inventing an author"
    Assert-Equal $true $markdown.Contains("103.50 ms") "Base median is displayed"
    Assert-Equal $true $markdown.Contains("73.50 ms") "Head median is displayed"

    $authorArguments = $comparisonArguments.Clone()
    $authorArguments.PullRequestAuthor = "perf-author"
    & $script @authorArguments
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("> @perf-author &mdash;") "Author result notification"
    $stdoutArguments = $authorArguments.Clone()
    $stdoutArguments.Remove("MarkdownOut")
    $stdoutMarkdown = & $script @stdoutArguments 6>$null
    Assert-Equal $markdown.Trim() ([string]$stdoutMarkdown).Trim() "Stdout and file output use the same format"

    $authorArguments.PullRequestAuthor = "dependabot[bot]"
    & $script @authorArguments
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("> @dependabot[bot] &mdash;") "Bot author notification"
    $authorArguments.PullRequestAuthor = "author`n<details open>"
    $invalidAuthorRejected = $false
    try {
        & $script @authorArguments
    }
    catch [Management.Automation.ParameterBindingException] {
        $invalidAuthorRejected = $true
    }
    Assert-Equal $true $invalidAuthorRejected "Unsafe author metadata must be rejected"

    Write-Results $resultsPath @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "head" @(105, 115) 1),
        (New-Result "head" @(106, 116) 2),
        (New-Result "base" @(101, 111) 2)
    )

    & $script @comparisonArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "neutral" $summary.verdict "Overlapping timing ranges should be neutral"
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("Timing-Neutral-") "Neutral badge"
    Assert-Equal $true $markdown.Contains("No timing regression was demonstrated") "Neutral follow-up"

    Write-Results $resultsPath @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "base" @(101, 111) 2),
        (New-Result "head" @(105, 115) 1)
    )
    & $script @comparisonArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Missing head result should be inconclusive"
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("Timing-Inconclusive-") "Incomplete result badge"
    Assert-Equal $true $markdown.Contains("Expected 2 head results, found 1.") "Incomplete diagnostics remain available"
    Assert-Equal $false $markdown.Contains("Correctness-Passed-") "Unvalidated results must not claim correctness passed"

    Write-Results $resultsPath @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "base" @(101, 111) 2),
        (New-Result "head" @(105, 115) 1 "wrong-harness"),
        (New-Result "head" @(106, 116) 2 "wrong-harness")
    )
    & $script @comparisonArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Wrong harness should be inconclusive"
    Assert-Equal $false $summary.provenanceValidated "Wrong harness provenance"

    Write-Results $resultsPath @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "base" @(101, 111) 2),
        (New-Result "head" @(105, 115) 1 "harness123" $false),
        (New-Result "head" @(106, 116) 2)
    )
    & $script @comparisonArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Correctness failure should be inconclusive"
    Assert-Equal $false $summary.correctnessPassed "Correctness failure status"
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("Correctness-Failed-") "Correctness failure badge"
    Assert-Equal $true $markdown.Contains("failed operation-level correctness") "Correctness diagnostics remain available"

    $wrongCommitResults = @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "base" @(101, 111) 2),
        (New-Result "head" @(105, 115) 1),
        (New-Result "head" @(106, 116) 2)
    )
    $wrongCommitResults[2].commitSha = "stale-head"
    Write-Results $resultsPath $wrongCommitResults
    & $script @comparisonArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Wrong head SHA should be inconclusive"

    $wrongScenarioResults = @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "base" @(101, 111) 2),
        (New-Result "head" @(105, 115) 1),
        (New-Result "head" @(106, 116) 2)
    )
    $wrongScenarioResults | ForEach-Object { $_.scenario = "unrelated-scenario" }
    Write-Results $resultsPath $wrongScenarioResults
    & $script @comparisonArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Wrong scenario should be inconclusive"

    $mixedEnvironmentResults = @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "base" @(101, 111) 2),
        (New-Result "head" @(105, 115) 1),
        (New-Result "head" @(106, 116) 2)
    )
    $mixedEnvironmentResults[2].environment.sdkVersion = "11.0"
    $mixedEnvironmentResults[3].environment.sdkVersion = "11.0"
    Write-Results $resultsPath $mixedEnvironmentResults
    & $script @comparisonArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Mixed build SDKs should be inconclusive"

    $groupedResults = @(
        (New-Result "base" @(500, 520) 1),
        (New-Result "head" @(490, 510) 1),
        (New-Result "head" @(495, 515) 2),
        (New-Result "base" @(505, 525) 2)
    )
    foreach ($result in $groupedResults) {
        $result.scenario = "collectionview-grouped-scrollto-makevisible"
        $result.counters = [PSCustomObject]@{
            targetPositionSpread = if ($result.variant -eq "base") { 80 } else { 5 }
            positionsOutsideTolerance = if ($result.variant -eq "base") { 3 } else { 0 }
        }
    }
    $groupedArguments = $comparisonArguments.Clone()
    $groupedArguments.ExpectedScenario = "collectionview-grouped-scrollto-makevisible"
    Write-Results $resultsPath $groupedResults
    & $script @groupedArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "neutral" $summary.verdict "Correct grouped ScrollTo head should compare"
    Assert-Equal $true $summary.correctnessPassed "Grouped ScrollTo head correctness"

    $groupedResults[1].counters.positionsOutsideTolerance = 1
    Write-Results $resultsPath $groupedResults
    & $script @groupedArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Inconsistent grouped ScrollTo head should be inconclusive"
    Assert-Equal $true $summary.provenanceValidated "Grouped ScrollTo provenance remains valid"
    Assert-Equal $false $summary.correctnessPassed "Grouped ScrollTo correctness failure"

    $itemUpdateResults = @(
        (New-Result "base" @(300, 320) 1),
        (New-Result "head" @(390, 410) 1),
        (New-Result "head" @(395, 415) 2),
        (New-Result "base" @(305, 325) 2)
    )
    foreach ($result in $itemUpdateResults) {
        $result.scenario = "collectionview-keepitemsinview-update"
        $result.counters = [PSCustomObject]@{
            lastExpectedFirstVisiblePosition = 51
            lastFirstVisiblePosition = 51
            updatesPreservingFirstVisibleItem = if ($result.variant -eq "base") { 0 } else { 4 }
        }
    }
    $itemUpdateArguments = $comparisonArguments.Clone()
    $itemUpdateArguments.ExpectedScenario = "collectionview-keepitemsinview-update"
    Write-Results $resultsPath $itemUpdateResults
    & $script @itemUpdateArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "time-regression-advisory" $summary.verdict "Correct item update head should compare"
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("Timing-Regression%20advisory-") "Regression badge"
    Assert-Equal $true $markdown.Contains("Investigate the timing increase") "Regression follow-up"
    Assert-Equal $true $summary.correctnessPassed "Item update head correctness"

    $itemUpdateResults[1].counters.updatesPreservingFirstVisibleItem = 3
    Write-Results $resultsPath $itemUpdateResults
    & $script @itemUpdateArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Incorrect item update head should be inconclusive"
    Assert-Equal $false $summary.correctnessPassed "Item update correctness failure"

    $itemUpdateResults[1].counters.updatesPreservingFirstVisibleItem = 4
    $itemUpdateResults[1].counters.lastFirstVisiblePosition = 0
    Write-Results $resultsPath $itemUpdateResults
    & $script @itemUpdateArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Jumping to the first item should be inconclusive"
    Assert-Equal $false $summary.correctnessPassed "First-item jump correctness failure"

    $windowsCarouselResults = @(
        (New-Result "base" @(100, 105) 1),
        (New-Result "head" @(90, 95) 1),
        (New-Result "head" @(91, 96) 2),
        (New-Result "base" @(101, 106) 2)
    )
    foreach ($result in $windowsCarouselResults) {
        $result.scenario = "carouselview-wheel-snap-windows"
        $result.platform = "windows"
        $result.environment.executionKind = "virtual"
        $result.environment.deviceModel = "Helix Windows VM"
        $result.environment.processArchitecture = "X64"
        $result.environment.runtimeVariant = "coreclr"
        $result.counters = [PSCustomObject]@{
            maximumCenterError = if ($result.variant -eq "base") { 20 } else { 0.5 }
            positionsOutsideTolerance = if ($result.variant -eq "base") { 4 } else { 0 }
            positionMismatchCount = 0
        }
    }
    $windowsCarouselArguments = $comparisonArguments.Clone()
    $windowsCarouselArguments.ExpectedPlatform = "windows"
    $windowsCarouselArguments.ExpectedScenario = "carouselview-wheel-snap-windows"
    Write-Results $resultsPath $windowsCarouselResults
    & $script @windowsCarouselArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal $true $summary.correctnessPassed "Windows CarouselView head correctness"

    $windowsCarouselResults[1].counters.maximumCenterError = 2
    Write-Results $resultsPath $windowsCarouselResults
    Assert-Equal 2 (Get-Content $resultsPath -Raw | ConvertFrom-Json)[1].counters.maximumCenterError "Windows off-center fixture"
    & $script @windowsCarouselArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Off-center Windows CarouselView head should be inconclusive"

    $appleCarouselResults = @(
        (New-Result "base" @(100, 105) 1),
        (New-Result "head" @(110, 115) 1),
        (New-Result "head" @(111, 116) 2),
        (New-Result "base" @(101, 106) 2)
    )
    foreach ($result in $appleCarouselResults) {
        $result.scenario = "carouselview-swipe-disabled"
        $result.counters = [PSCustomObject]@{
            embeddedScrollViewCount = 1
            stateReapplicationFailures = if ($result.variant -eq "base") { 50 } else { 0 }
        }
    }
    $appleCarouselArguments = $comparisonArguments.Clone()
    $appleCarouselArguments.ExpectedScenario = "carouselview-swipe-disabled"
    Write-Results $resultsPath $appleCarouselResults
    & $script @appleCarouselArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "neutral" $summary.verdict "Correct Apple CarouselView head should compare"
    Assert-Equal $true $summary.correctnessPassed "Apple CarouselView correctness"

    $appleCarouselResults[1].counters.stateReapplicationFailures = 1
    Write-Results $resultsPath $appleCarouselResults
    & $script @appleCarouselArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Incorrect Apple CarouselView head should be inconclusive"
    Assert-Equal $false $summary.correctnessPassed "Apple CarouselView correctness failure"

    $androidCarouselResults = @(
        (New-Result "base" @(100, 105) 1),
        (New-Result "head" @(10, 15) 1),
        (New-Result "head" @(11, 16) 2),
        (New-Result "base" @(101, 106) 2)
    )
    foreach ($result in $androidCarouselResults) {
        $result.scenario = "carouselview-swipe-disabled"
        $result.platform = "android"
        $result.counters = [PSCustomObject]@{
            interceptedTouchEventCount = if ($result.variant -eq "base") { 300 } else { 0 }
            finalPosition = if ($result.variant -eq "base") { 1 } else { 0 }
        }
    }
    $androidCarouselArguments = $comparisonArguments.Clone()
    $androidCarouselArguments.ExpectedScenario = "carouselview-swipe-disabled"
    $androidCarouselArguments.ExpectedPlatform = "android"
    Write-Results $resultsPath $androidCarouselResults
    & $script @androidCarouselArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "time-improvement-advisory" $summary.verdict "Correct Android CarouselView head should compare"
    Assert-Equal $true $summary.correctnessPassed "Android CarouselView correctness"

    $androidCarouselResults[1].counters.interceptedTouchEventCount = 1
    Write-Results $resultsPath $androidCarouselResults
    & $script @androidCarouselArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Incorrect Android CarouselView head should be inconclusive"
    Assert-Equal $false $summary.correctnessPassed "Android CarouselView correctness failure"

    $handlerResults = @(
        (New-Result "base" @(10, 11) 1),
        (New-Result "head" @(9, 10) 1),
        (New-Result "head" @(9, 10) 2),
        (New-Result "base" @(10, 11) 2)
    )
    foreach ($result in $handlerResults) {
        $result.scenario = "handler-property-update-batch"
        $result.platform = "windows"
        $result.environment.executionKind = "native-host"
        $result.environment.deviceModel = "Windows"
        $result.environment.processArchitecture = "X64"
        $result.environment.runtimeVariant = "coreclr"
        $result.counters = [PSCustomObject]@{
            completedUpdateBatches = 4
            nativeValueMismatchCount = 0
        }
    }
    $handlerArguments = $comparisonArguments.Clone()
    $handlerArguments.ExpectedScenario = "handler-property-update-batch"
    $handlerArguments.ExpectedPlatform = "windows"
    Write-Results $resultsPath $handlerResults
    & $script @handlerArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal $true $summary.correctnessPassed "Handler property-update correctness"

    $handlerResults[1].counters.nativeValueMismatchCount = 1
    Write-Results $resultsPath $handlerResults
    & $script @handlerArguments
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $summary.verdict "Handler mismatch should be inconclusive"
    Assert-Equal $false $summary.correctnessPassed "Handler mismatch correctness failure"

    Write-Results $resultsPath @()
    & $script @comparisonArguments
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("Timing-Inconclusive-") "Empty result badge"
    Assert-Equal $true $markdown.Contains("Correctness-Not%20assessed-") "Empty results must not claim a correctness failure"
    Assert-Equal $true $markdown.Contains("No device results were supplied") "Empty result explanation"

    $unsafeScenario = "<details open>injected</details>|@unexpected"
    $unsafeResults = @(
        (New-Result "base" @(100, 110) 1),
        (New-Result "head" @(105, 115) 1),
        (New-Result "head" @(106, 116) 2),
        (New-Result "base" @(101, 111) 2)
    )
    $unsafeResults | ForEach-Object { $_.scenario = $unsafeScenario }
    $unsafeArguments = $comparisonArguments.Clone()
    $unsafeArguments.ExpectedScenario = $unsafeScenario
    Write-Results $resultsPath $unsafeResults
    & $script @unsafeArguments
    $markdown = Assert-CommentFormat $markdownPath
    Assert-Equal $true $markdown.Contains("&lt;details open&gt;") "Scenario HTML must be escaped"
    Assert-Equal $true $markdown.Contains("&#124;") "Scenario must not add table columns"
    Assert-Equal $false $markdown.Contains("@unexpected") "Scenario must not add author mentions"

    Write-Host "All device performance comparator tests passed."
}
finally
{
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
