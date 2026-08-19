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

.PARAMETER DiffBase
    Commit used as the local diff base. When provided, changed files and added
    device-test methods are read from DiffBase..HEAD instead of the live PR.

.PARAMETER Platform
    Optional device-test platform used to exclude methods from partial files that do not
    compile for that target (for example, Android methods from a Windows Gate run).

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
    [string[]]$ChangedFiles,

    [Parameter(Mandatory = $false)]
    [string]$DiffBase,

    [Parameter(Mandatory = $false)]
    [string]$Platform
)

$ErrorActionPreference = "Stop"

if (-not [string]::IsNullOrWhiteSpace($DiffBase)) {
    $resolvedDiffBase = git rev-parse --verify "$DiffBase^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($resolvedDiffBase)) {
        throw "Diff base '$DiffBase' is not a valid commit."
    }
    $DiffBase = $resolvedDiffBase.Trim()
}

function Test-DeviceTestFileAppliesToPlatform {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$TargetPlatform
    )

    if ([string]::IsNullOrWhiteSpace($TargetPlatform)) {
        return $true
    }

    $normalizedPlatform = $TargetPlatform.Trim().ToLowerInvariant()
    if ($normalizedPlatform -eq 'catalyst') {
        $normalizedPlatform = 'maccatalyst'
    } elseif ($normalizedPlatform -in @('win', 'winui')) {
        $normalizedPlatform = 'windows'
    }

    $normalizedPath = $Path.Replace('\', '/')
    $fileName = [System.IO.Path]::GetFileName($normalizedPath)

    if ($fileName -match '(?i)\.android\.cs$') {
        return $normalizedPlatform -eq 'android'
    }
    if ($fileName -match '(?i)\.windows\.cs$') {
        return $normalizedPlatform -eq 'windows'
    }
    if ($fileName -match '(?i)\.ios\.cs$') {
        return $normalizedPlatform -in @('ios', 'maccatalyst')
    }
    if ($fileName -match '(?i)\.maccatalyst\.cs$') {
        return $normalizedPlatform -eq 'maccatalyst'
    }

    if ($normalizedPath -match '(?i)/(?:Platforms?/)?Android/') {
        return $normalizedPlatform -eq 'android'
    }
    if ($normalizedPath -match '(?i)/(?:Platforms?/)?Windows/') {
        return $normalizedPlatform -eq 'windows'
    }
    if ($normalizedPath -match '(?i)/(?:Platforms?/)?iOS/') {
        return $normalizedPlatform -in @('ios', 'maccatalyst')
    }
    if ($normalizedPath -match '(?i)/(?:Platforms?/)?MacCatalyst/') {
        return $normalizedPlatform -eq 'maccatalyst'
    }

    return $true
}

function Test-DeviceTestPlatformPartial {
    param([Parameter(Mandatory = $true)][string]$Path)

    $normalizedPath = $Path.Replace('\', '/')
    if ($normalizedPath -notmatch '(?i)(?:^|/)DeviceTests/') {
        return $false
    }

    $fileName = [System.IO.Path]::GetFileName($normalizedPath)
    return (
        $fileName -match '(?i)\.(?:android|windows|ios|maccatalyst)\.cs$' -or
        $normalizedPath -match '(?i)/(?:Platforms?/)?(?:Android|Windows|iOS|MacCatalyst)/'
    )
}

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
}

# ============================================================
# Step 1: Get changed files
# ============================================================

$mergeBase = $null
if (-not $ChangedFiles -or $ChangedFiles.Count -eq 0) {
    if ($DiffBase) {
        # The review worktree is a committed snapshot. Prefer its exact diff so a
        # force-push or new PR commit during a Gate retry cannot change selection.
        $ChangedFiles = git diff $DiffBase HEAD --name-only 2>$null
    } elseif ($PRNumber) {
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

# ─── Reliability fix #7: parse the actual class name from the .cs file ───
# The previous logic derived the dotnet test filter from the *filename basename*,
# but maui's test repo uses a "category-prefix" file-naming convention where the
# filename includes a logical bucket dot the class name (e.g.
# `CarouselViewUITests.ProgrammaticPositionBounceBack.cs` containing the class
# `CarouselViewProgrammaticPositionBounceBack`). Filtering by the filename
# yielded `--filter FullyQualifiedName~CarouselViewUITests.ProgrammaticPositionBounceBack`
# which matched zero tests, and the gate marked the PR FAILED purely because of
# our auto-detection mistake. Reading the actual `public class` declaration
# from the file content is more reliable. Falls back to the filename basename
# when the file can't be read (e.g., file deleted, path unresolvable).
$RepoRootForRead = git rev-parse --show-toplevel 2>$null
function Get-ClassNameFromFile {
    param([string]$RelativePath)
    $candidates = @($RelativePath)
    if ($RepoRootForRead) {
        $candidates += (Join-Path $RepoRootForRead $RelativePath)
    }
    foreach ($p in $candidates) {
        if (Test-Path $p) {
            try {
                $content = Get-Content $p -Raw -ErrorAction Stop
            } catch { continue }
            # A test file can declare concrete helper classes before its actual test
            # class. Prefer the first concrete class that owns a test method
            # attribute instead of blindly selecting the first public class.
            # This keeps XHarness class isolation on the class that owns the tests
            # (for example ShellHandlerTests_Shell, not StartupTrackingShellHandler).
            $classMatches = @([regex]::Matches(
                $content,
                '(?m)^\s*public(?<modifiers>(?:\s+(?:partial|sealed|abstract|static))*)\s+class\s+(?<name>\w+)'
            ))
            $concreteClasses = @($classMatches | Where-Object {
                $_.Groups['modifiers'].Value -notmatch '\b(?:abstract|static)\b'
            })
            if ($concreteClasses.Count -eq 0) { continue }

            $testAttributes = @([regex]::Matches(
                $content,
                '(?m)^\s*\[\s*(?:(?:\w+)\.)*(Fact|Theory|Test|TestCase|TestCaseSource|TestMethod)\b'
            ))
            foreach ($testAttribute in $testAttributes) {
                $testClass = $classMatches |
                    Where-Object { $_.Index -lt $testAttribute.Index } |
                    Select-Object -Last 1
                if ($testClass -and
                    $testClass.Groups['modifiers'].Value -notmatch '\b(?:abstract|static)\b') {
                    return $testClass.Groups['name'].Value
                }
            }

            return $concreteClasses[0].Groups['name'].Value
        }
    }
    return $null
}

function Test-CsFileHasTestMethods {
    <#
    .SYNOPSIS
        Returns $true only if a .cs file actually declares test methods.
    .DESCRIPTION
        Test-support files (helpers, base classes, fixtures, data builders) live under the
        same test projects but contain NO [Fact]/[Test] methods. Detecting one as a "test"
        (e.g. VisualStateTestHelpers.cs) makes the gate run a filter that matches zero tests;
        the empty run is then scored as a failure and drags the whole gate to FAILED even when
        the PR's real tests pass FAIL→PASS. Requiring at least one test-method attribute keeps
        those support files out of the detected-test set.
    #>
    param([string]$RelativePath)
    $candidates = @($RelativePath)
    if ($RepoRootForRead) { $candidates += (Join-Path $RepoRootForRead $RelativePath) }
    foreach ($p in $candidates) {
        if (Test-Path $p) {
            try { $content = Get-Content $p -Raw -ErrorAction Stop } catch { continue }
            # xUnit: [Fact] [Theory]; NUnit: [Test] [TestCase] [TestCaseSource]; MSTest: [TestMethod]
            return ($content -match '(?m)\[\s*(Fact|Theory|Test|TestCase|TestCaseSource|TestMethod)\b')
        }
    }
    # File unreadable (deleted/unresolvable) — don't over-filter; let existing fallbacks handle it.
    return $true
}

function Get-AddedDeviceTestMethodsFromPatch {
    <#
    .SYNOPSIS
        Returns only added methods that are explicitly marked as tests.
    .DESCRIPTION
        Device-test files often add public helper methods alongside [Fact]/[Test]
        methods. Treating every added public void/Task as a test makes result
        validation demand helpers that the runner will never execute, turning a
        clean with-fix run into a false environment error.
    #>
    param([string]$Patch)

    if ([string]::IsNullOrWhiteSpace($Patch)) {
        return @()
    }

    $methods = [System.Collections.Generic.List[string]]::new()
    $pendingTestAttribute = $false

    foreach ($rawLine in ($Patch -split "`n")) {
        if ($rawLine -notmatch '^\+(?!\+\+)') {
            continue
        }

        $line = $rawLine.Substring(1).TrimEnd("`r")
        if ($line -match '\[\s*(?:(?:\w+)\.)*(Fact|Theory|Test|TestCase|TestCaseSource|TestMethod)\b') {
            $pendingTestAttribute = $true
        }

        # Attribute-only, comment, preprocessor, and blank lines may legitimately
        # sit between the test attribute and method declaration.
        $declaration = $line -replace '^\s*(?:\[[^\]]+\]\s*)+', ''
        if ([string]::IsNullOrWhiteSpace($declaration) -or
            $declaration -match '^\s*(?://|/\*|\*|#)') {
            continue
        }

        if ($pendingTestAttribute -and
            $declaration -match '^\s*public\s+(?:(?:static|async|virtual|override|new)\s+)*(?:Task(?:<[^>]+>)?|ValueTask(?:<[^>]+>)?|void)\s+(\w+)\s*\(') {
            $methodName = $matches[1]
            if ($methods -notcontains $methodName) {
                $methods.Add($methodName)
            }
            $pendingTestAttribute = $false
            continue
        }

        # A non-attribute declaration consumed the pending marker without defining
        # a test method; do not let it leak to a later public helper.
        if ($pendingTestAttribute -and $declaration -notmatch '^\s*\[') {
            $pendingTestAttribute = $false
        }
    }

    return @($methods)
}

foreach ($file in $ChangedFiles) {
    # Skip non-code files
    if ($file -notmatch "\.(cs|xaml)$") { continue }
    # Skip snapshot files
    if ($file -match "snapshots/") { continue }
    # Skip infrastructure files (MauiProgram.cs, Startup.cs, etc.)
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file) -replace '\.(iOS|Android|Windows|MacCatalyst)$', ''
    if ($baseName -in $IgnoredFileNames) { continue }

    $isDeviceTestFile = $file.Replace('\', '/') -match '(?i)(?:^|/)DeviceTests/'
    if ($isDeviceTestFile -and -not (Test-DeviceTestFileAppliesToPlatform -Path $file -TargetPlatform $Platform)) {
        continue
    }
    $isDeviceTestPlatformPartial = $isDeviceTestFile -and (Test-DeviceTestPlatformPartial -Path $file)

    # Skip test-support .cs files that contain NO test methods (helpers, base classes,
    # fixtures, data builders). Detecting e.g. VisualStateTestHelpers.cs as a "test" makes
    # the gate run a filter that matches nothing; that empty run is scored as a failure and
    # drags the whole gate to FAILED even when the PR's real tests pass. HostApp companion
    # pages, .xaml files, and platform partials whose shared DeviceTests class owns the test
    # methods legitimately have no test attributes, so exempt them here.
    if ($file -match '\.cs$' -and
        $file -notmatch 'TestCases\.HostApp' -and
        -not $isDeviceTestPlatformPartial) {
        if (-not (Test-CsFileHasTestMethods -RelativePath $file)) { continue }
    }

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
                        # Prefer the class name parsed from the file content over
                        # the filename basename. maui's test repo often uses a
                        # category-prefix in the filename that does NOT match the
                        # actual class name (e.g. CarouselViewUITests.X.cs → class X).
                        $parsedClass = Get-ClassNameFromFile -RelativePath $file
                        if ($parsedClass) {
                            $testName = $parsedClass
                        } elseif ($file -match "[/\\]([^/\\]+)\.cs$") {
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
                    $parsedClass = Get-ClassNameFromFile -RelativePath $file
                    if ($parsedClass) {
                        $testName = $parsedClass
                    } elseif ($file -match "[/\\]([^/\\]+)\.(cs|xaml)$") {
                        $testName = $matches[1]
                        $testName = $testName -replace '\.(rt|rtsg|rtxc|xaml)$', ''
                    }
                    $projectPath = $rule.ProjectPath
                    $filter = $testName
                }

                "DeviceTest" {
                    $parsedClass = Get-ClassNameFromFile -RelativePath $file
                    if ($parsedClass) {
                        $testName = $parsedClass
                    } elseif ($file -match "[/\\]([^/\\]+)\.cs$") {
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
                    $parsedClass = Get-ClassNameFromFile -RelativePath $file
                    if ($parsedClass) {
                        $testName = $parsedClass
                    } elseif ($file -match "[/\\]([^/\\]+)\.cs$") {
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
#         for display and result scoping, but keep the category-based filter
# ============================================================

foreach ($key in @($testGroups.Keys)) {
    $group = $testGroups[$key]
    if ($group.Type -ne "DeviceTest") { continue }

    # Find added test methods from the diff. Never include public helpers: a
    # missing helper result is otherwise misclassified as an environment error
    # after all real tests pass.
    $addedMethods = @()
    # Cache PR files API response once before the inner loop.
    # A failure here must NEVER abort the gate. The script runs under
    # $ErrorActionPreference='Stop', so an unguarded parse error is terminating: `gh api`
    # can return an HTML error page (rate-limit / transient 5xx) — as seen on PR #36572,
    # where "ConvertFrom-Json: parsing value: <" crashed the gate to exit 3 / INCONCLUSIVE —
    # and `--paginate` alone emits multiple concatenated JSON arrays for >30-file PRs, which
    # also breaks ConvertFrom-Json. Fetch defensively: --slurp yields one well-formed array
    # of pages, validate it's JSON, flatten one level, and swallow any error (degrading to
    # no method-name display; the category-based filter is unaffected).
    # `$script:_cachedPRFiles` alone cannot gate the fetch: an empty result is `@()`,
    # and `-not @()` is `$true`, so every later device-test group would retry the same
    # failing call. Track the fetch attempt with a separate sentinel.
    if ($PRNumber -and -not $DiffBase -and -not $script:_prFilesFetchAttempted) {
        $script:_prFilesFetchAttempted = $true
        try {
            $rawPRFiles = (gh api "repos/dotnet/maui/pulls/$PRNumber/files" --paginate --slurp 2>$null | Out-String).Trim()
            if ($rawPRFiles.StartsWith('[')) {
                # --slurp wraps each page as one element ([[file,...],[file,...]]) — flatten a level.
                $script:_cachedPRFiles = @(($rawPRFiles | ConvertFrom-Json) | ForEach-Object { $_ })
            }
        } catch {
            Write-Host "  ℹ️  PR files fetch failed (non-fatal; skipping method-name display): $($_.Exception.Message)"
        }
        if (-not $script:_cachedPRFiles) { $script:_cachedPRFiles = @() }
    }
    $effectiveMergeBase = if ($DiffBase) { $DiffBase } elseif ($mergeBase) { $mergeBase } else { "HEAD~1" }
    foreach ($file in $group.Files) {
        if (-not (Test-DeviceTestFileAppliesToPlatform -Path $file -TargetPlatform $Platform)) {
            continue
        }

        $patch = $null
        if ($DiffBase) {
            $patch = ((git diff $effectiveMergeBase HEAD -- $file 2>$null) -join "`n")
        } elseif ($PRNumber -and $script:_cachedPRFiles) {
            # Look up patch from cached API response
            $fileEntry = $script:_cachedPRFiles | Where-Object { $_.filename -eq $file } | Select-Object -First 1
            $patch = if ($fileEntry) { $fileEntry.patch } else { $null }
        } elseif (-not $PRNumber) {
            # Try from git diff
            $patch = ((git diff $effectiveMergeBase HEAD -- $file 2>$null) -join "`n")
        }

        if ($patch) {
            foreach ($methodName in @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch)) {
                if ($addedMethods -notcontains $methodName) {
                    $addedMethods += $methodName
                }
            }
        }
    }

    # Method names are optional display metadata and provide narrower result scoping when available.
    if ($addedMethods.Count -gt 0) {
        $group.TestName = "$($group.TestName) ($($addedMethods -join ', '))"
        $group.Methods = $addedMethods
    }

    # Find [Category] attribute (and the namespace, for a fully-qualified class filter)
    # from the main (non-platform) test class file. This is independent of whether the
    # diff adds a method: modified existing device-test bodies still require filtering.
    $baseClassName = ($group.TestName -split ' \(')[0]
    $repoRoot = git rev-parse --show-toplevel 2>$null
    $categoryFilter = $null
    $classNamespace = $null

    foreach ($file in $group.Files) {
        if ($file -notmatch "\.cs$") { continue }

        # Probe the main class file (without platform suffix) first, then the changed file.
        $testDir = [System.IO.Path]::GetDirectoryName($file)
        $candidates = @()
        if ($repoRoot) { $candidates += (Join-Path $repoRoot "$testDir/$baseClassName.cs") }
        $candidates += $(if ($repoRoot) { Join-Path $repoRoot $file } else { $file })

        foreach ($candidate in $candidates) {
            if (-not ($candidate -and (Test-Path $candidate))) { continue }
            $content = Get-Content $candidate -Raw -ErrorAction SilentlyContinue
            if (-not $content) { continue }

            # Capture the namespace (block-scoped or file-scoped) once. $matches is read
            # immediately, before the [Category] match below can overwrite it.
            if (-not $classNamespace -and $content -match '(?m)^\s*namespace\s+([A-Za-z_][\w.]*)') {
                $classNamespace = $matches[1]
            }

            # Match [Category(TestCategory.X)] or [Category("X")] once.
            if (-not $categoryFilter) {
                if ($content -match '\[Category\(TestCategory\.(\w+)\)\]') {
                    $categoryFilter = "Category=$($matches[1])"
                } elseif ($content -match '\[Category\("([^"]+)"\)\]') {
                    $categoryFilter = "Category=$($matches[1])"
                }
            }
        }

        if ($categoryFilter -and $classNamespace) { break }
    }

    # Use Category filter if found, otherwise fall back to class name.
    $group.Filter = if ($categoryFilter) { $categoryFilter } else { $baseClassName }

    # For device tests, also emit a fully-qualified class name so the gate can run ONLY the
    # PR's test class (XHarness SkipClass include filter) instead of the whole Category. A
    # single unrelated crashing test in the same category otherwise APP_CRASHes the run and
    # turns the verdict INCONCLUSIVE (e.g. dotnet/maui#36616). Additive: $group.Filter still
    # carries the whole-Category value for Windows + fallback, so existing behaviour is kept.
    if ($group.Type -eq "DeviceTest" -and $baseClassName) {
        $group.ClassFilter = if ($classNamespace) { "$classNamespace.$baseClassName" } else { $baseClassName }
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
