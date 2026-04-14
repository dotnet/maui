#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detects all tests added/modified in a PR or git diff and classifies them by type.

.DESCRIPTION
    Analyzes changed files to identify each individual test and its type (UITest, UnitTest,
    XamlUnitTest, DeviceTest). Returns structured data for each test including the filter
    needed to run it and which runner to use.

    Can take a PR number (fetches file list from GitHub) or use the local git diff.

.PARAMETER PRNumber
    GitHub PR number to analyze. Uses `gh` CLI to fetch file list.

.PARAMETER BaseBranch
    Base branch for git diff comparison. If omitted, auto-detected from PR or uses HEAD~1.

.PARAMETER ChangedFiles
    Explicit list of changed file paths (skips PR/git detection).

.OUTPUTS
    Array of hashtables, each with:
    - Type:        UITest | UnitTest | XamlUnitTest | DeviceTest
    - TestName:    Human-readable test name (class name or method name)
    - Filter:      dotnet test --filter value
    - Project:     Project key for device tests (Controls, Core, etc.)
    - ProjectPath: Relative .csproj path for unit tests
    - Runner:      Which script runs it (BuildAndRunHostApp, dotnet-test, Run-DeviceTests)
    - Platform:    Whether -Platform is required
    - Files:       List of test files

.EXAMPLE
    # Detect tests from a PR
    ./Detect-TestsInDiff.ps1 -PRNumber 25129

.EXAMPLE
    # Detect tests from local git diff
    ./Detect-TestsInDiff.ps1 -BaseBranch main

.EXAMPLE
    # Pipe explicit file list
    ./Detect-TestsInDiff.ps1 -ChangedFiles @("src/Controls/tests/DeviceTests/Editor/EditorTests.iOS.cs")
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [string[]]$ChangedFiles
)

$ErrorActionPreference = "Stop"

# ============================================================
# Test type classification patterns (ordered by specificity)
# ============================================================

$TestTypeRules = @(
    @{
        Type = "UITest"
        PathPattern = "TestCases\.(Shared\.Tests|HostApp)"
        Runner = "BuildAndRunHostApp"
        NeedsPlatform = $true
        # UI test files come in pairs (HostApp + Shared.Tests). Group by class name.
    }
    @{
        Type = "XamlUnitTest"
        PathPattern = "Xaml\.UnitTests/"
        Runner = "dotnet-test"
        NeedsPlatform = $false
        ProjectPath = "src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj"
    }
    @{
        Type = "DeviceTest"
        PathPattern = "DeviceTests/"
        Runner = "Run-DeviceTests"
        NeedsPlatform = $true
    }
    @{
        Type = "UnitTest"
        PathPattern = "(?<!\w)UnitTests/|Graphics\.Tests/"
        Runner = "dotnet-test"
        NeedsPlatform = $false
    }
)

# Device test project detection
$DeviceTestProjects = @{
    "Controls"     = "src/Controls/tests/DeviceTests/"
    "Core"         = "src/Core/tests/DeviceTests/"
    "Essentials"   = "src/Essentials/test/DeviceTests/"
    "Graphics"     = "src/Graphics/tests/DeviceTests/"
    "BlazorWebView"= "src/BlazorWebView/tests/DeviceTests/"
    "AI"           = "src/AI/tests/Essentials.AI.DeviceTests/"
}

# Unit test project detection
$UnitTestProjects = @{
    "Controls.Core.UnitTests"          = "src/Controls/tests/Core.UnitTests/"
    "Controls.Xaml.UnitTests"          = "src/Controls/tests/Xaml.UnitTests/"
    "Controls.BindingSourceGen.UnitTests" = "src/Controls/tests/BindingSourceGen.UnitTests/"
    "SourceGen.UnitTests"              = "src/Controls/tests/SourceGen.UnitTests/"
    "Core.UnitTests"                   = "src/Core/tests/UnitTests/"
    "Essentials.UnitTests"             = "src/Essentials/test/UnitTests/"
    "Graphics.Tests"                   = "src/Graphics/tests/Graphics.Tests/"
    "Resizetizer.UnitTests"            = "src/SingleProject/Resizetizer/test/UnitTests/"
    "Compatibility.Core.UnitTests"     = "src/Compatibility/Core/tests/Compatibility.UnitTests/"
    "Essentials.AI.UnitTests"          = "src/AI/tests/Essentials.AI.UnitTests/"
}

$UnitTestProjectPaths = @{
    "Controls.Core.UnitTests"          = "src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj"
    "Controls.Xaml.UnitTests"          = "src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj"
    "Controls.BindingSourceGen.UnitTests" = "src/Controls/tests/BindingSourceGen.UnitTests/Controls.BindingSourceGen.UnitTests.csproj"
    "SourceGen.UnitTests"              = "src/Controls/tests/SourceGen.UnitTests/SourceGen.UnitTests.csproj"
    "Core.UnitTests"                   = "src/Core/tests/UnitTests/Core.UnitTests.csproj"
    "Essentials.UnitTests"             = "src/Essentials/test/UnitTests/Essentials.UnitTests.csproj"
    "Graphics.Tests"                   = "src/Graphics/tests/Graphics.Tests/Graphics.Tests.csproj"
    "Resizetizer.UnitTests"            = "src/SingleProject/Resizetizer/test/UnitTests/Resizetizer.UnitTests.csproj"
    "Compatibility.Core.UnitTests"     = "src/Compatibility/Core/tests/Compatibility.UnitTests/Compatibility.Core.UnitTests.csproj"
    "Essentials.AI.UnitTests"          = "src/AI/tests/Essentials.AI.UnitTests/Essentials.AI.UnitTests.csproj"
}

# ============================================================
# Step 1: Get changed files
# ============================================================

if (-not $ChangedFiles -or $ChangedFiles.Count -eq 0) {
    if ($PRNumber) {
        # Fetch from GitHub
        # Use paginated API to handle PRs with >30 changed files
        $prFiles = gh api "repos/dotnet/maui/pulls/$PRNumber/files" --paginate --jq '.[].filename' 2>$null
        if ($LASTEXITCODE -ne 0 -or -not $prFiles) {
            $prFiles = gh pr view $PRNumber --json files --jq '.files[].path' 2>$null
        }
        if ($LASTEXITCODE -ne 0 -or -not $prFiles) {
            $prFiles = gh pr diff $PRNumber --name-only 2>$null
        }
        $ChangedFiles = $prFiles -split "`n" | Where-Object { $_ }
    } else {
        # Use git diff
        $mergeBase = $null
        if ($BaseBranch) {
            $mergeBase = git merge-base HEAD "origin/$BaseBranch" 2>$null
            if (-not $mergeBase) {
                $mergeBase = git merge-base HEAD -- "$BaseBranch" 2>$null
            }
        }
        if (-not $mergeBase) {
            # Try to detect from PR metadata
            try {
                $prInfo = gh pr view --json baseRefName --jq '.baseRefName' 2>$null
                if ($prInfo) {
                    $mergeBase = git merge-base HEAD "origin/$prInfo" 2>$null
                }
            } catch {}
        }
        if (-not $mergeBase) {
            $mergeBase = "HEAD~1"
        }
        $ChangedFiles = git diff $mergeBase HEAD --name-only 2>$null
    }
}

if (-not $ChangedFiles -or $ChangedFiles.Count -eq 0) {
    Write-Host "No changed files detected." -ForegroundColor Yellow
    return @()
}

# ============================================================
# Step 2: Classify each file and group into test entries
# ============================================================

# Infrastructure files to ignore even when in test directories
$IgnoredFileNames = @(
    "MauiProgram", "Startup", "TestOptions", "TestCategory",
    "AssemblyInfo", "GlobalUsings", "Usings"
)

# Intermediate: collect test files grouped by type + test name
$testGroups = @{}  # Key: "Type:TestName" → Value: hashtable

foreach ($file in $ChangedFiles) {
    # Skip non-code files
    if ($file -notmatch "\.(cs|xaml)$") { continue }
    # Skip snapshot files
    if ($file -match "snapshots/") { continue }
    # Skip infrastructure files (MauiProgram.cs, Startup.cs, etc.)
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file) -replace '\.(iOS|Android|Windows|MacCatalyst)$', ''
    if ($baseName -in $IgnoredFileNames) { continue }

    foreach ($rule in $TestTypeRules) {
        if ($file -match $rule.PathPattern) {
            $testType = $rule.Type
            $testName = $null
            $project = $null
            $projectPath = $null
            $filter = $null

            switch ($testType) {
                "UITest" {
                    # Only Shared.Tests files define actual test classes.
                    # HostApp files are UI pages associated with tests but aren't tests themselves.
                    if ($file -match "TestCases\.Shared\.Tests") {
                        if ($file -match "[/\\]([^/\\]+)\.cs$") {
                            $testName = $matches[1]
                        }
                        $filter = $testName
                    } elseif ($file -match "TestCases\.HostApp") {
                        # HostApp pages: extract name and associate with Shared.Tests entry
                        if ($file -match "[/\\]([^/\\]+)\.(cs|xaml)$") {
                            $testName = $matches[1]
                            $testName = $testName -replace '\.xaml$', ''
                        }
                        # Mark as companion file — will be merged with Shared.Tests entry if one exists
                        $filter = $testName
                    }
                }

                "XamlUnitTest" {
                    if ($file -match "[/\\]([^/\\]+)\.(cs|xaml)$") {
                        $testName = $matches[1]
                        $testName = $testName -replace '\.(rt|rtsg|rtxc|xaml)$', ''
                    }
                    $projectPath = $rule.ProjectPath
                    $filter = $testName
                }

                "DeviceTest" {
                    if ($file -match "[/\\]([^/\\]+)\.cs$") {
                        $className = $matches[1]
                        # Strip platform suffix: EditorTests.iOS → EditorTests
                        $className = $className -replace '\.(iOS|Android|Windows|MacCatalyst)$', ''
                        $testName = $className
                    }

                    # Detect which device test project
                    foreach ($projKey in $DeviceTestProjects.Keys) {
                        if ($file -like "*$($DeviceTestProjects[$projKey])*") {
                            $project = $projKey
                            break
                        }
                    }
                    if (-not $project) {
                        Write-Warning "Device test file '$file' did not match any known project — skipping."
                        $testName = $null
                    }

                    # Filter will be set to Method=X after method extraction in Step 4
                    $filter = $testName
                }

                "UnitTest" {
                    if ($file -match "[/\\]([^/\\]+)\.cs$") {
                        $testName = $matches[1]
                    }

                    # Detect which unit test project
                    foreach ($projName in $UnitTestProjects.Keys) {
                        if ($file -like "*$($UnitTestProjects[$projName])*") {
                            $project = $projName
                            $projectPath = $UnitTestProjectPaths[$projName]
                            break
                        }
                    }
                    $filter = $testName
                }
            }

            if ($testName) {
                $groupKey = "${testType}:${testName}"
                if (-not $testGroups.ContainsKey($groupKey)) {
                    $testGroups[$groupKey] = @{
                        Type = $testType
                        TestName = $testName
                        Filter = $filter
                        Project = $project
                        ProjectPath = $projectPath
                        Runner = $rule.Runner
                        NeedsPlatform = $rule.NeedsPlatform
                        Files = @()
                    }
                }
                $testGroups[$groupKey].Files += $file
            }

            break  # File matched a rule, don't check further rules
        }
    }
}

# ============================================================
# Step 3: Post-process — remove HostApp-only UI test entries (no test class)
# ============================================================

# For UITest entries, verify at least one file is from TestCases.Shared.Tests
foreach ($key in @($testGroups.Keys)) {
    $group = $testGroups[$key]
    if ($group.Type -ne "UITest") { continue }

    $hasTestClass = $group.Files | Where-Object { $_ -match "TestCases\.Shared\.Tests" }
    if (-not $hasTestClass) {
        $testGroups.Remove($key)
    }
}

# ============================================================
# Step 4: For device tests, extract specific test method names from the diff
#         for display purposes, but keep the category-based filter
# ============================================================

foreach ($key in @($testGroups.Keys)) {
    $group = $testGroups[$key]
    if ($group.Type -ne "DeviceTest") { continue }

    # Try to find added [Fact] or [Test] methods from the diff
    $addedMethods = @()
    # Cache PR files API response once before the inner loop
    if ($PRNumber -and -not $script:_cachedPRFiles) {
        $script:_cachedPRFiles = gh api "repos/dotnet/maui/pulls/$PRNumber/files" --paginate 2>$null | ConvertFrom-Json
        if (-not $script:_cachedPRFiles) { $script:_cachedPRFiles = @() }
    }
    $effectiveMergeBase = if ($mergeBase) { $mergeBase } else { "HEAD~1" }
    foreach ($file in $group.Files) {
        $patch = $null
        if ($PRNumber -and $script:_cachedPRFiles) {
            # Look up patch from cached API response
            $fileEntry = $script:_cachedPRFiles | Where-Object { $_.filename -eq $file } | Select-Object -First 1
            $patch = if ($fileEntry) { $fileEntry.patch } else { $null }
        } elseif (-not $PRNumber) {
            # Try from git diff
            $patch = git diff $effectiveMergeBase HEAD -- $file 2>$null
        }

        if ($patch) {
            $addedLines = $patch -split "`n" | Where-Object { $_ -match "^\+" }
            foreach ($line in $addedLines) {
                if ($line -match "public\s+async\s+Task\s+(\w+)\s*\(" -or
                    $line -match "public\s+void\s+(\w+)\s*\(") {
                    $methodName = $matches[1]
                    if ($methodName -ne "Dispose" -and $methodName -ne "Setup" -and
                        $addedMethods -notcontains $methodName) {
                        $addedMethods += $methodName
                    }
                }
            }
        }
    }

    # Extract method names for display, use Category= filter for the device test runner
    if ($addedMethods.Count -gt 0) {
        $group.TestName = "$($group.TestName) ($($addedMethods -join ', '))"
        $group.Methods = $addedMethods

        # Find [Category] attribute from the main (non-platform) test class file
        $baseClassName = ($group.TestName -split ' \(')[0]
        $repoRoot = git rev-parse --show-toplevel 2>$null
        $categoryFilter = $null

        foreach ($file in $group.Files) {
            if ($file -match "\.cs$") {
                # Try the main class file (without platform suffix)
                $testDir = [System.IO.Path]::GetDirectoryName($file)
                $mainFile = if ($repoRoot) { Join-Path $repoRoot "$testDir/$baseClassName.cs" } else { $null }
                if ($mainFile -and (Test-Path $mainFile)) {
                    $content = Get-Content $mainFile -Raw -ErrorAction SilentlyContinue
                    # Match [Category(TestCategory.X)] or [Category("X")]
                    if ($content -match '\[Category\(TestCategory\.(\w+)\)\]') {
                        $categoryFilter = "Category=$($matches[1])"
                        break
                    } elseif ($content -match '\[Category\("([^"]+)"\)\]') {
                        $categoryFilter = "Category=$($matches[1])"
                        break
                    }
                }
                # Also check the changed file itself
                $fullPath = if ($repoRoot) { Join-Path $repoRoot $file } else { $file }
                if (Test-Path $fullPath) {
                    $content = Get-Content $fullPath -Raw -ErrorAction SilentlyContinue
                    if ($content -match '\[Category\(TestCategory\.(\w+)\)\]') {
                        $categoryFilter = "Category=$($matches[1])"
                        break
                    } elseif ($content -match '\[Category\("([^"]+)"\)\]') {
                        $categoryFilter = "Category=$($matches[1])"
                        break
                    }
                }
            }
        }

        # Use Category filter if found, otherwise fall back to class name
        $group.Filter = if ($categoryFilter) { $categoryFilter } else { $baseClassName }
    }
}

# ============================================================
# Step 5: Output results
# ============================================================

$results = @($testGroups.Values | Sort-Object { $_.Type }, { $_.TestName })

if ($results.Count -eq 0) {
    Write-Host "No tests detected in changed files." -ForegroundColor Yellow
    return @()
}

# Display summary
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║        Detected Tests in PR                               ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  Found $($results.Count) test(s) across $($results | Select-Object -ExpandProperty Type -Unique | Measure-Object | Select-Object -ExpandProperty Count) type(s)                              ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$i = 0
foreach ($test in $results) {
    $i++
    $platformNote = if ($test.NeedsPlatform) { "(requires -Platform)" } else { "" }
    $icon = switch ($test.Type) {
        "UITest"       { "🖥️" }
        "DeviceTest"   { "📱" }
        "UnitTest"     { "🧪" }
        "XamlUnitTest" { "📄" }
    }

    Write-Host "  $icon $i. [$($test.Type)] $($test.TestName) $platformNote" -ForegroundColor White
    Write-Host "     Filter:  $($test.Filter)" -ForegroundColor Gray
    if ($test.Project) {
        Write-Host "     Project: $($test.Project)" -ForegroundColor Gray
    }
    if ($test.ProjectPath) {
        Write-Host "     Path:    $($test.ProjectPath)" -ForegroundColor Gray
    }
    Write-Host "     Runner:  $($test.Runner)" -ForegroundColor Gray
    Write-Host "     Files:   $($test.Files -join ', ')" -ForegroundColor DarkGray
    Write-Host ""
}

return $results
