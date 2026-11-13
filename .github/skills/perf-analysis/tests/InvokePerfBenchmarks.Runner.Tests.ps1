#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$skillRoot = Split-Path -Parent $PSScriptRoot
$runner = Join-Path $skillRoot "scripts\Invoke-PerfBenchmarks.ps1"
$comparator = Join-Path $skillRoot "scripts\Compare-BenchmarkResults.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-perf-runner-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Invoke-Git([string[]]$arguments) {
    & git @arguments *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "git $($arguments -join ' ') failed."
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try {
    $repo = Join-Path $testRoot "repo"
    $output = Join-Path $testRoot "output"
    $fakeBin = Join-Path $testRoot "bin"
    $orderLog = Join-Path $testRoot "order.log"
    $prerequisiteLog = Join-Path $testRoot "prerequisite.log"
    New-Item -ItemType Directory -Force -Path $repo, $fakeBin | Out-Null

    Push-Location $repo
    try {
        Invoke-Git @("init", "--initial-branch=main")
        Invoke-Git @("config", "user.name", "Perf Runner Test")
        Invoke-Git @("config", "user.email", "perf-runner@example.invalid")
        Invoke-Git @("remote", "add", "origin", "https://github.com/dotnet/maui.git")

        "<Project />" | Set-Content "fixture.csproj" -Encoding UTF8
        "base" | Set-Content "source.txt" -Encoding UTF8
        Invoke-Git @("add", ".")
        Invoke-Git @("commit", "-m", "base")
        $baseSha = (& git rev-parse HEAD).Trim()

        "head" | Set-Content "source.txt" -Encoding UTF8
        Invoke-Git @("add", ".")
        Invoke-Git @("commit", "-m", "head")
        $headSha = (& git rev-parse HEAD).Trim()
    }
    finally {
        Pop-Location
    }

    $selection = [PSCustomObject]@{
        suites = @([PSCustomObject]@{
            project = "Xaml"
            csproj = "fixture.csproj"
            filters = @("*Fixture*", "*HistoricalName*")
            matchedFiles = @("source.txt")
            benchmarkInputsChanged = $false
            changedBenchmarkInputFiles = @()
            trustedBenchmarkFiles = @(
                "src/Core/tests/Benchmarks/Benchmarks/LayoutExtensionsBenchmarker.cs"
            )
        })
    }
    $selection | ConvertTo-Json -Depth 8 |
        Set-Content (Join-Path $testRoot "selection.json") -Encoding UTF8

    [PSCustomObject]@{
        number = 42
        title = "Fixture"
        state = "OPEN"
        baseRefName = "main"
        headRefOid = $headSha
        mergeBaseOid = $baseSha
        url = "https://github.com/dotnet/maui/pull/42"
    } | ConvertTo-Json -Depth 6 |
        Set-Content (Join-Path $testRoot "pr.json") -Encoding UTF8

@'
@echo off
if not exist "src\Core\tests\Benchmarks\Benchmarks\LayoutExtensionsBenchmarker.cs" exit /b 9
if "%~1"=="build" (
  if "%~2"=="Microsoft.Maui.BuildTasks.slnf" (
    for %%I in ("%CD%") do echo %%~nxI>>"%PERF_FAKE_PREREQUISITE_LOG%"
  )
  exit /b 0
)
if not "%~1"=="run" exit /b 1
:parse
if "%~1"=="" exit /b 1
if "%~1"=="--artifacts" goto found
shift
goto parse
:found
set "ARTIFACTS=%~2"
if not exist "%ARTIFACTS%" mkdir "%ARTIFACTS%"
for %%I in ("%CD%") do set "SIDE=%%~nxI"
for %%I in ("%ARTIFACTS%") do set "RUN=%%~nxI"
echo %SIDE%:%RUN%>>"%PERF_FAKE_ORDER_LOG%"
if "%PERF_FAKE_ASYMMETRIC%"=="1" if "%SIDE%"=="base" (
  echo {"Benchmarks":[{"FullName":"Fixture.Benchmark","Parameters":"","Statistics":{"Mean":100,"StandardError":1},"Memory":{"BytesAllocatedPerOperation":0}},{"FullName":"Fixture.BaseOnly","Parameters":"","Statistics":{"Mean":100,"StandardError":1},"Memory":{"BytesAllocatedPerOperation":0}}]}>"%ARTIFACTS%\Fixture-report-full.json"
  exit /b 0
)
echo {"Benchmarks":[{"FullName":"Fixture.Benchmark","Parameters":"","Statistics":{"Mean":100,"StandardError":1},"Memory":{"BytesAllocatedPerOperation":0}}]}>"%ARTIFACTS%\Fixture-report-full.json"
exit /b 0
'@ | Set-Content (Join-Path $fakeBin "dotnet.cmd") -Encoding ASCII

@'
#!/usr/bin/env sh
test -f "src/Core/tests/Benchmarks/Benchmarks/LayoutExtensionsBenchmarker.cs" || exit 9
if [ "$1" = "build" ]; then
  if [ "$2" = "Microsoft.Maui.BuildTasks.slnf" ]; then
    basename "$PWD" >> "$PERF_FAKE_PREREQUISITE_LOG"
  fi
  exit 0
fi
if [ "$1" != "run" ]; then
  exit 1
fi
while [ "$#" -gt 0 ]; do
  if [ "$1" = "--artifacts" ]; then
    artifacts="$2"
    break
  fi
  shift
done
test -n "$artifacts" || exit 1
mkdir -p "$artifacts"
side="$(basename "$PWD")"
run="$(basename "$artifacts")"
printf '%s:%s\n' "$side" "$run" >> "$PERF_FAKE_ORDER_LOG"
if [ "$PERF_FAKE_ASYMMETRIC" = "1" ] && [ "$side" = "base" ]; then
  printf '%s\n' '{"Benchmarks":[{"FullName":"Fixture.Benchmark","Parameters":"","Statistics":{"Mean":100,"StandardError":1},"Memory":{"BytesAllocatedPerOperation":0}},{"FullName":"Fixture.BaseOnly","Parameters":"","Statistics":{"Mean":100,"StandardError":1},"Memory":{"BytesAllocatedPerOperation":0}}]}' > "$artifacts/Fixture-report-full.json"
else
  printf '%s\n' '{"Benchmarks":[{"FullName":"Fixture.Benchmark","Parameters":"","Statistics":{"Mean":100,"StandardError":1},"Memory":{"BytesAllocatedPerOperation":0}}]}' > "$artifacts/Fixture-report-full.json"
fi
exit 0
'@ | Set-Content (Join-Path $fakeBin "dotnet") -Encoding UTF8
    if (-not $IsWindows) {
        & chmod +x (Join-Path $fakeBin "dotnet")
    }

    $savedPath = $env:PATH
    $savedOrderLog = $env:PERF_FAKE_ORDER_LOG
    $savedPrerequisiteLog = $env:PERF_FAKE_PREREQUISITE_LOG
    $env:PATH = "$fakeBin$([IO.Path]::PathSeparator)$savedPath"
    $env:PERF_FAKE_ORDER_LOG = $orderLog
    $env:PERF_FAKE_PREREQUISITE_LOG = $prerequisiteLog
    try {
        Push-Location $repo
        try {
            & $runner `
                -PrNumber 42 `
                -SuitesPath (Join-Path $testRoot "selection.json") `
                -OutputRoot $output `
                -RunsPerSide 2 `
                -Job short `
                -IsolationMode None `
                -PrMetadataPath (Join-Path $testRoot "pr.json")
            Assert-Equal 0 $LASTEXITCODE "Runner should complete"
        }
        finally {
            Pop-Location
        }
    }
    finally {
        $env:PATH = $savedPath
        $env:PERF_FAKE_ORDER_LOG = $savedOrderLog
        $env:PERF_FAKE_PREREQUISITE_LOG = $savedPrerequisiteLog
    }

    $manifest = Get-Content (Join-Path $output "run-manifest.json") -Raw | ConvertFrom-Json
    if ($manifest.status -ne "complete") {
        Get-ChildItem (Join-Path $output "results") -Recurse -Filter "*.log" |
            ForEach-Object {
                Write-Host "=== $($_.FullName) ==="
                Get-Content $_.FullName
            }
    }
    Assert-Equal "complete" $manifest.status "Runner manifest status"
    Assert-Equal 2 $manifest.runsPerSide "Runner repetitions"
    Assert-Equal "None" $manifest.isolationMode "Runner isolation mode"
    Assert-Equal $baseSha $manifest.baseSha "Runner base SHA"
    Assert-Equal $headSha $manifest.headSha "Runner head SHA"
    Assert-Equal $true $manifest.credentialsSanitized "Runner credential sanitation"
    Assert-Equal $true $manifest.prerequisites.buildTasks.base.succeeded "Base build-task prerequisite"
    Assert-Equal $true $manifest.prerequisites.buildTasks.head.succeeded "Head build-task prerequisite"
    Assert-Equal 1 @($manifest.suites[0].notApplicableFilters).Count "Symmetrically absent filter count"
    Assert-Equal "*HistoricalName*" $manifest.suites[0].notApplicableFilters[0] "Symmetrically absent filter"
    Assert-Equal 0 @($manifest.suites[0].asymmetricMissingFilters).Count "No asymmetric filter drift"
    Assert-Equal "base" $manifest.runOrder[0].order[0] "First ABBA side"
    Assert-Equal "head" $manifest.runOrder[0].order[1] "Second ABBA side"
    Assert-Equal "head" $manifest.runOrder[1].order[0] "Third ABBA side"
    Assert-Equal "base" $manifest.runOrder[1].order[1] "Fourth ABBA side"

    $observedOrder = @(Get-Content $orderLog)
    Assert-Equal "base:run1" $observedOrder[0] "Observed first run"
    Assert-Equal "head:run1" $observedOrder[1] "Observed second run"
    Assert-Equal "head:run2" $observedOrder[2] "Observed third run"
    Assert-Equal "base:run2" $observedOrder[3] "Observed fourth run"
    $prerequisiteOrder = @(Get-Content $prerequisiteLog)
    Assert-Equal "base" $prerequisiteOrder[0] "Base build-task invocation"
    Assert-Equal "head" $prerequisiteOrder[1] "Head build-task invocation"

    $partialOutput = Join-Path $testRoot "partial-output"
    $partialSelection = [PSCustomObject]@{
        suites = @([PSCustomObject]@{
            project = "Xaml"
            csproj = "fixture.csproj"
            filters = @("*Fixture*", "*ChangedBenchmark*")
            runnableFilters = @("*Fixture*")
            changedFilters = @("*ChangedBenchmark*")
            matchedFiles = @("source.txt")
            benchmarkInputsChanged = $false
            changedBenchmarkInputFiles = @("ChangedBenchmark.cs")
            trustedBenchmarkFiles = @(
                "src/Core/tests/Benchmarks/Benchmarks/LayoutExtensionsBenchmarker.cs"
            )
        })
    }
    $partialSelection | ConvertTo-Json -Depth 8 |
        Set-Content (Join-Path $testRoot "partial-selection.json") -Encoding UTF8
    $env:PATH = "$fakeBin$([IO.Path]::PathSeparator)$savedPath"
    $env:PERF_FAKE_ORDER_LOG = $orderLog
    $env:PERF_FAKE_PREREQUISITE_LOG = $prerequisiteLog
    try {
        Push-Location $repo
        try {
            & $runner `
                -PrNumber 42 `
                -SuitesPath (Join-Path $testRoot "partial-selection.json") `
                -OutputRoot $partialOutput `
                -RunsPerSide 2 `
                -Job short `
                -IsolationMode None `
                -PrMetadataPath (Join-Path $testRoot "pr.json")
            Assert-Equal 0 $LASTEXITCODE "Partial filter runner should complete"
        }
        finally {
            Pop-Location
        }
    }
    finally {
        $env:PATH = $savedPath
        $env:PERF_FAKE_ORDER_LOG = $savedOrderLog
        $env:PERF_FAKE_PREREQUISITE_LOG = $savedPrerequisiteLog
    }
    $partialManifest = Get-Content (Join-Path $partialOutput "run-manifest.json") -Raw | ConvertFrom-Json
    Assert-Equal "partial" $partialManifest.status "Partial filter manifest status"
    Assert-Equal "*ChangedBenchmark*" $partialManifest.suites[0].skippedFilters[0] "Skipped changed filter"
    Assert-Equal $true $partialManifest.suites[0].complete "Runnable subset should complete"

    $asymmetricOutput = Join-Path $testRoot "asymmetric-output"
    $selection.suites[0].filters = @("*BaseOnly*")
    $selection | ConvertTo-Json -Depth 8 |
        Set-Content (Join-Path $testRoot "asymmetric-selection.json") -Encoding UTF8
    $env:PATH = "$fakeBin$([IO.Path]::PathSeparator)$savedPath"
    $env:PERF_FAKE_ORDER_LOG = $orderLog
    $env:PERF_FAKE_PREREQUISITE_LOG = $prerequisiteLog
    $env:PERF_FAKE_ASYMMETRIC = "1"
    try {
        Push-Location $repo
        try {
            & $runner `
                -PrNumber 42 `
                -SuitesPath (Join-Path $testRoot "asymmetric-selection.json") `
                -OutputRoot $asymmetricOutput `
                -RunsPerSide 2 `
                -Job short `
                -IsolationMode None `
                -PrMetadataPath (Join-Path $testRoot "pr.json")
            Assert-Equal 0 $LASTEXITCODE "Runner should emit an incomplete manifest for asymmetric filter drift"
        }
        finally {
            Pop-Location
        }
    }
    finally {
        $env:PATH = $savedPath
        $env:PERF_FAKE_ORDER_LOG = $savedOrderLog
        $env:PERF_FAKE_PREREQUISITE_LOG = $savedPrerequisiteLog
        Remove-Item Env:PERF_FAKE_ASYMMETRIC -ErrorAction SilentlyContinue
    }
    $asymmetricManifest = Get-Content (Join-Path $asymmetricOutput "run-manifest.json") -Raw | ConvertFrom-Json
    Assert-Equal "incomplete" $asymmetricManifest.status "Asymmetric filter manifest status"
    Assert-Equal "*BaseOnly*" $asymmetricManifest.suites[0].asymmetricMissingFilters[0] "Asymmetric filter identity"

    & $comparator `
        -BaseDir (Join-Path $output "results\base") `
        -HeadDir (Join-Path $output "results\head") `
        -RunManifestPath (Join-Path $output "run-manifest.json") `
        -MarkdownOut (Join-Path $testRoot "table.md") `
        -JsonOut (Join-Path $testRoot "summary.json")
    Assert-Equal 0 $LASTEXITCODE "Comparator should consume runner output"

    $summary = Get-Content (Join-Path $testRoot "summary.json") -Raw | ConvertFrom-Json
    Assert-Equal "neutral" $summary.verdict "Runner-to-comparator verdict"
    Assert-Equal $true $summary.executionComplete "Comparator execution completeness"
    Assert-Equal $true $summary.benchmarkDataComplete "Comparator benchmark completeness"

    Assert-Equal $false (Test-Path (Join-Path $output "worktrees\base")) "Base worktree cleanup"
    Assert-Equal $false (Test-Path (Join-Path $output "worktrees\head")) "Head worktree cleanup"

    Write-Host "Performance benchmark runner end-to-end test passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
