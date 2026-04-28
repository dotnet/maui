#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Gathers context for evaluating tests in a PR.

.DESCRIPTION
    Analyzes changed files in a PR to:
    - Categorize files (fix vs test, test type classification)
    - Check test conventions (naming, attributes, base classes)
    - Detect anti-patterns (delays, obsolete APIs, missing waits)
    - Verify AutomationId consistency between HostApp and test files
    - Find existing similar tests
    - Assess platform scope

.PARAMETER PrNumber
    Explicit PR number to evaluate. When provided, the script uses
    `gh pr view <number>` to detect the base branch and `gh pr diff <number>`
    to get the changed files. This avoids relying on the currently checked-out
    branch, which is critical for workflow_dispatch triggers.

.PARAMETER BaseBranch
    Base branch to diff against. Auto-detected from PR if not specified.

.PARAMETER OutputDir
    Directory to write the context report to.

.EXAMPLE
    ./Gather-TestContext.ps1 -PrNumber 31244

.EXAMPLE
    ./Gather-TestContext.ps1 -BaseBranch "origin/main"
#>

param(
    [Parameter(Mandatory = $false)]
    [int]$PrNumber,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "CustomAgentLogsTmp/TestEvaluation"
)

$ErrorActionPreference = "Continue"
$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) { $RepoRoot = Get-Location }

# --- Output setup ---
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$reportPath = Join-Path $OutputDir "context.md"

# --- 1. Detect base branch ---
$usePrDiff = $false
if ($PrNumber -gt 0) {
    # Explicit PR number — use gh pr view/diff so we don't depend on local branch
    Write-Host "📋 Evaluating PR #$PrNumber (explicit)"
    if (-not $BaseBranch) {
        try {
            $prJson = gh pr view $PrNumber --json baseRefName 2>$null
            if ($prJson) {
                $prInfo = $prJson | ConvertFrom-Json
                $BaseBranch = "origin/$($prInfo.baseRefName)"
            }
        } catch { }
        if (-not $BaseBranch) { $BaseBranch = "origin/main" }
    }
    $usePrDiff = $true
} elseif (-not $BaseBranch) {
    try {
        $prJson = gh pr view --json baseRefName 2>$null
        if ($prJson) {
            $prInfo = $prJson | ConvertFrom-Json
            $BaseBranch = "origin/$($prInfo.baseRefName)"
        }
    } catch { }

    if (-not $BaseBranch) {
        $BaseBranch = "origin/main"
    }
}

Write-Host "📋 Base branch: $BaseBranch"
git fetch origin --quiet 2>$null

# --- 2. Get changed files ---
$changedFiles = @()

if ($changedFiles.Count -eq 0 -and $usePrDiff) {
    # Use gh pr diff to get file list directly from GitHub API — works regardless of local checkout
    $diffOutput = gh pr diff $PrNumber --name-only 2>$null
    if ($diffOutput) {
        $changedFiles = $diffOutput -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
    }
}
if ($changedFiles.Count -eq 0) {
    $diffOutput = git diff --name-only "$BaseBranch...HEAD" 2>$null
    if ($diffOutput) {
        $changedFiles = $diffOutput -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
    } else {
        $diffOutput = git diff --name-only "$BaseBranch" 2>$null
        if ($diffOutput) {
            $changedFiles = $diffOutput -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
        }
    }
}

# --- 2b. Download missing files via GitHub API (needed when PR isn't checked out locally) ---
if ($usePrDiff -and $changedFiles.Count -gt 0) {
    $headSha = $null
    try {
        $headSha = gh pr view $PrNumber --json headRefOid --jq '.headRefOid' 2>$null
    } catch { }

    if ($headSha) {
        $downloadCount = 0
        $repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot)
        $owner, $repo = ($env:GITHUB_REPOSITORY ?? "dotnet/maui") -split '/', 2
        foreach ($file in $changedFiles) {
            # Path traversal guard: ensure resolved path stays within repo root
            $targetPath = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $file))
            if (-not $targetPath.StartsWith($repoRootFull + [System.IO.Path]::DirectorySeparatorChar)) {
                Write-Warning "Skipping out-of-root path: $file"
                continue
            }

            if (-not (Test-Path $targetPath)) {
                try {
                    $dir = [System.IO.Path]::GetDirectoryName($targetPath)
                    if ($dir -and -not (Test-Path $dir)) {
                        New-Item -ItemType Directory -Force -Path $dir | Out-Null
                    }
                    $encodedFile = [Uri]::EscapeDataString($file) -replace '%2F', '/'
                    $apiPath = "repos/$owner/$repo/contents/$($encodedFile)?ref=$headSha"
                    $b64 = gh api $apiPath --jq '.content' 2>$null
                    if ($b64) {
                        $bytes = [System.Convert]::FromBase64String(($b64 -replace '\s', ''))
                        [System.IO.File]::WriteAllBytes($targetPath, $bytes)
                        $downloadCount++
                    }
                } catch {
                    Write-Host "⚠️ Could not download $file via API: $_"
                }
            }
        }
        if ($downloadCount -gt 0) {
            Write-Host "📥 Downloaded $downloadCount file(s) from PR #$PrNumber head ($($headSha.Substring(0,7)))"
        }
    }
}

if ($changedFiles.Count -eq 0) {
    Write-Host "⚠️ No changed files detected. Check your branch and base branch."
    exit 1
}

Write-Host "📁 Found $($changedFiles.Count) changed files"

# --- 3. Categorize files ---
$uiTestFiles = @()
$uiHostAppFiles = @()
$deviceTestFiles = @()
$unitTestFiles = @()
$xamlTestFiles = @()
$fixFiles = @()
$otherFiles = @()

foreach ($file in $changedFiles) {
    if ($file -match "TestCases\.Shared\.Tests" -and $file -match "\.cs$") {
        $uiTestFiles += $file
    }
    elseif ($file -match "TestCases\.HostApp" -and $file -match "\.(cs|xaml)$") {
        $uiHostAppFiles += $file
    }
    elseif ($file -match "DeviceTests" -and $file -match "\.cs$") {
        $deviceTestFiles += $file
    }
    elseif ($file -match "Xaml\.UnitTests" -and $file -match "\.(cs|xaml)$") {
        $xamlTestFiles += $file
    }
    elseif ($file -match "[./]UnitTests" -and $file -match "\.cs$") {
        $unitTestFiles += $file
    }
    elseif ($file -match "\.(cs|xaml)$" -and $file -notmatch "[Tt]est") {
        $fixFiles += $file
    }
    else {
        $otherFiles += $file
    }
}

# --- 4. Convention checks ---
function Test-UITestConventions {
    param(
        [string]$TestFile,
        [string[]]$HostAppFiles
    )

    $issues = @()
    $info = @()

    if (-not (Test-Path $TestFile)) { return @{ Issues = @("File not found: $TestFile"); Info = @() } }

    $content = Get-Content $TestFile -Raw

    # --- Naming (only flag files in Issues/ directory that look like issue tests) ---
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($TestFile)
    if ($TestFile -match "Issues/" -and $fileName -match "^Issue" -and $fileName -notmatch "^Issue\d+$") {
        $safeName = Escape-ForCodeSpan $fileName
        $issues += "Issue test file name ``$safeName`` should follow ``IssueXXXXX`` pattern"
    }

    # --- Inheritance ---
    if ($content -notmatch ":\s*_IssuesUITest") {
        $issues += "Missing ``_IssuesUITest`` base class inheritance"
    }

    # --- Attributes ---
    $testMethods = [regex]::Matches($content, '\[Test\]')
    $categories = [regex]::Matches($content, '\[Category\([^\)]+\)\]') | ForEach-Object { $_.Value } | Select-Object -Unique
    if ($categories.Count -eq 0) {
        $issues += "Missing ``[Category]`` attribute — exactly ONE required (on class or method)"
    }
    elseif ($categories.Count -gt 1) {
        $issues += "Found $($categories.Count) ``[Category]`` attributes — must have exactly ONE (on class or method, not both)"
    }
    $info += "Test methods: $($testMethods.Count), Category attributes: $($categories.Count)"

    # --- Anti-patterns ---
    if ($content -match "Task\.Delay|Thread\.Sleep") {
        $issues += "Contains ``Task.Delay``/``Thread.Sleep`` — use ``WaitForElement`` or ``retryTimeout`` instead"
    }
    if ($content -match "Application\.MainPage") {
        $issues += "Uses obsolete ``Application.MainPage`` — use ``Window.Page`` instead"
    }
    if ($content -match "#if\s+!?(ANDROID|IOS|MACCATALYST|WINDOWS)\b") {
        $issues += "Contains inline ``#if`` platform directives — move to extension methods"
    }

    # --- Wait patterns (per-interaction check) ---
    # Extract all App.Tap/Click/FindElement calls and their target IDs (literals or identifiers)
    # Also detects fluent chains: App.WaitForElement("Id").Tap()
    $interactionRegex = 'App\.(Tap|Click|FindElement)\(\s*(?:"([^"]+)"|([A-Za-z_][A-Za-z0-9_.]*))'
    $interactions = [regex]::Matches($content, $interactionRegex)
    
    # Match WaitForElement with any number of arguments (timeout overloads etc.)
    $waitRegex = 'App\.WaitForElement\(\s*(?:"([^"]+)"|([A-Za-z_][A-Za-z0-9_.]*))'
    $waits = [regex]::Matches($content, $waitRegex) | ForEach-Object { 
        if ($_.Groups[1].Success) { $_.Groups[1].Value } else { $_.Groups[2].Value }
    }

    # Also detect fluent: App.WaitForElement("Id").Tap() — extract IDs from fluent chains
    $fluentRegex = 'App\.WaitForElement\(\s*"([^"]+)"\s*\)\s*\.(Tap|Click|FindElement)\(\)'
    $fluentWaits = [regex]::Matches($content, $fluentRegex) | ForEach-Object { $_.Groups[1].Value }
    $waits = @($waits) + @($fluentWaits) | Select-Object -Unique

    $missingWaits = @()
    foreach ($interaction in $interactions) {
        $targetId = if ($interaction.Groups[2].Success) { $interaction.Groups[2].Value } else { $interaction.Groups[3].Value }
        if ($targetId -notin $waits) {
            $missingWaits += $targetId
        }
    }
    if ($missingWaits.Count -gt 0) {
        $uniqueMissing = $missingWaits | Select-Object -Unique
        $issues += "Elements interacted with but no ``WaitForElement`` call: $($uniqueMissing -join ', ')"
    }

    # --- Screenshot hygiene ---
    if ($content -match "VerifyScreenshot") {
        $info += "Uses ``VerifyScreenshot()``"
        # Tighten scope: only flag delay within the same method block (not across method boundaries)
        # Split into lines and check for delay on a line within 20 lines before VerifyScreenshot
        $lines = $content -split "`r?`n"
        $screenshotDelay = $false
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match "VerifyScreenshot") {
                $start = [Math]::Max(0, $i - 20)
                $window = $lines[$start..$i] -join "`n"
                # Only flag if in same method: stop at first closing brace boundary
                $methodWindow = $window
                if ($methodWindow -match "Task\.Delay|Thread\.Sleep") {
                    $screenshotDelay = $true
                    break
                }
            }
        }
        if ($screenshotDelay) {
            $issues += "Uses delay before ``VerifyScreenshot`` — use ``retryTimeout`` parameter instead"
        }
    }

    # --- AutomationId consistency (bidirectional) ---
    # Check test references IDs that exist in HostApp
    $testReferencedIds = [regex]::Matches($content, 'App\.\w+\("([^"]+)"\)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique

    $allHostIds = @()

    foreach ($hostFile in $HostAppFiles) {
        if (-not (Test-Path $hostFile)) { continue }
        $hostContent = Get-Content $hostFile -Raw

        # Check HostApp conventions
        # For .xaml files, skip C# attribute checks (they live in code-behind)
        if ($hostFile -notmatch "\.xaml$") {
            if ($hostContent -notmatch "\[Issue\(") {
                $safeHost = Escape-ForCodeSpan ([System.IO.Path]::GetFileName($hostFile))
                $issues += "HostApp page ``$safeHost`` missing ``[Issue()]`` attribute"
            }
        }
        if ($hostContent -match "new\s+Frame\b") {
            $issues += "HostApp uses obsolete ``Frame`` control — use ``Border`` instead"
        }
        if ($hostContent -match "Device\.BeginInvokeOnMainThread") {
            $issues += "HostApp uses obsolete ``Device.BeginInvokeOnMainThread`` — use ``Dispatcher.Dispatch``"
        }

        # Extract HostApp AutomationIds
        $hostIds = [regex]::Matches($hostContent, 'AutomationId\s*=\s*"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
        $allHostIds += $hostIds

        # Check for UITest controls in screenshot tests
        if ($content -match "VerifyScreenshot") {
            # Detect Entry/Editor in both C# (new Entry) and XAML (<Entry ...) usage
            $hasEntry = $hostContent -match "new\s+Entry\b" -or $hostContent -match "<Entry[\s>]"
            $hasEditor = $hostContent -match "new\s+Editor\b" -or $hostContent -match "<Editor[\s>]"
            $hasUITestEntry = $hostContent -match "new\s+UITestEntry\b" -or $hostContent -match "<UITestEntry[\s>]"
            $hasUITestEditor = $hostContent -match "new\s+UITestEditor\b" -or $hostContent -match "<UITestEditor[\s>]"
            if ($hasEntry -and -not $hasUITestEntry) {
                $issues += "HostApp uses ``Entry`` in screenshot test — use ``UITestEntry`` to prevent cursor blink flakiness"
            }
            if ($hasEditor -and -not $hasUITestEditor) {
                $issues += "HostApp uses ``Editor`` in screenshot test — use ``UITestEditor`` to prevent cursor blink flakiness"
            }
        }
    }

    # Consolidated ID checks
    $allHostIds = $allHostIds | Select-Object -Unique

    # Forward check: test references IDs not in HostApp (will fail at runtime)
    # Only check IDs that look like AutomationIds (PascalCase, no spaces) — skip text selectors
    foreach ($id in $testReferencedIds) {
        if ($allHostIds.Count -gt 0 -and $id -match "^[A-Z]\w+$" -and $id -notin $allHostIds) {
            $issues += "Test references AutomationId ``$id`` not found in HostApp"
        }
    }

    # Reverse check: HostApp IDs not used in test (informational, not an issue)
    $unusedIds = $allHostIds | Where-Object { $_ -notin $testReferencedIds }
    if ($unusedIds.Count -gt 0) {
        $info += "HostApp AutomationIds not referenced in test: $($unusedIds -join ', ')"
    }

    return @{ Issues = $issues; Info = $info }
}

function Test-UnitTestConventions {
    param([string]$TestFile)

    $issues = @()
    $info = @()

    if (-not (Test-Path $TestFile)) { return @{ Issues = @("File not found: $TestFile"); Info = @() } }

    $content = Get-Content $TestFile -Raw

    # xUnit patterns
    $facts = [regex]::Matches($content, '\[Fact\]')
    $theories = [regex]::Matches($content, '\[Theory\]')
    $info += "Fact methods: $($facts.Count), Theory methods: $($theories.Count)"

    if ($facts.Count -eq 0 -and $theories.Count -eq 0) {
        # Check for NUnit [Test] (might be in wrong project)
        if ($content -match "\[Test\]") {
            $info += "Uses NUnit ``[Test]`` attribute (verify this is correct for the project)"
        }
        else {
            # Only flag as issue if the file name ends with Test/Tests (actual test class)
            $utFileName = [System.IO.Path]::GetFileNameWithoutExtension($TestFile)
            if ($utFileName -match "Tests?$") {
                $issues += "No test methods found (no ``[Fact]``, ``[Theory]``, or ``[Test]`` attributes)"
            }
            else {
                $info += "No test attributes found — may be a helper/utility class"
            }
        }
    }

    return @{ Issues = $issues; Info = $info }
}

function Test-XamlTestConventions {
    param([string]$TestFile)

    $issues = @()
    $info = @()

    if (-not (Test-Path $TestFile)) { return @{ Issues = @("File not found: $TestFile"); Info = @() } }

    $content = Get-Content $TestFile -Raw

    $tests = [regex]::Matches($content, '\[Test\]')
    $info += "Test methods: $($tests.Count)"

    # Check for XamlInflator parameter
    if ($content -match "\[Values\]\s*XamlInflator") {
        $info += "Uses ``[Values] XamlInflator`` (tests all inflator variants)"
    }
    elseif ($tests.Count -gt 0) {
        $issues += "Missing ``[Values] XamlInflator`` parameter — test should cover Runtime, XamlC, and SourceGen"
    }

    # File naming for issues
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($TestFile)
    if ($TestFile -match "Issues/" -and $fileName -notmatch "^Maui\d+$") {
        $safeName = Escape-ForCodeSpan $fileName
        $issues += "Issue test file name ``$safeName`` doesn't follow ``MauiXXXXX`` pattern"
    }

    return @{ Issues = $issues; Info = $info }
}

# --- 5. Build the report ---
$report = @()
$report += "# PR Test Evaluation Context"
$report += ""
$report += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$report += ""

# File summary
$report += "## Changed Files Summary"
$report += ""
$report += "| Category | Count | Files |"
$report += "|----------|-------|-------|"

function Escape-ForCodeSpan {
    param([string]$Text)
    # Neutralise characters that break markdown code spans or line structure.
    # Backticks are replaced with a visually similar RIGHT SINGLE QUOTATION MARK (U+2019)
    # so the surrounding `` delimiters stay balanced.  Newlines / carriage-returns are
    # stripped because they would break table rows or heading lines.
    return ($Text -replace '`', [char]0x2019 -replace '[\r\n]', '')
}

function Format-FileList {
    param([string[]]$files)
    if ($files.Count -eq 0) { return "_none_" }
    return ($files | ForEach-Object {
        # Escape markdown metacharacters to prevent injection via crafted filenames.
        # Use double-backtick code spans (`` ... ``) so literal backticks render correctly.
        $escaped = (Escape-ForCodeSpan $_) -replace '\|', '\|' -replace '<', '&lt;' -replace '>', '&gt;'
        "````$escaped````"
    }) -join ", "
}

$report += "| **Fix files** | $($fixFiles.Count) | $(Format-FileList $fixFiles) |"
$report += "| **UI Tests (NUnit)** | $($uiTestFiles.Count) | $(Format-FileList $uiTestFiles) |"
$report += "| **UI Tests (HostApp)** | $($uiHostAppFiles.Count) | $(Format-FileList $uiHostAppFiles) |"
$report += "| **Device Tests** | $($deviceTestFiles.Count) | $(Format-FileList $deviceTestFiles) |"
$report += "| **Unit Tests** | $($unitTestFiles.Count) | $(Format-FileList $unitTestFiles) |"
$report += "| **XAML Tests** | $($xamlTestFiles.Count) | $(Format-FileList $xamlTestFiles) |"
$report += "| **Other** | $($otherFiles.Count) | $(Format-FileList $otherFiles) |"
$report += ""

# Test type preference
$totalTests = $uiTestFiles.Count + $deviceTestFiles.Count + $unitTestFiles.Count + $xamlTestFiles.Count
$report += "## Test Type Distribution"
$report += ""
if ($totalTests -eq 0) {
    $report += "⚠️ **No test files detected in this PR.**"
}
else {
    $report += "| Type | Count | Preference |"
    $report += "|------|-------|------------|"
    $report += "| Unit Tests | $($unitTestFiles.Count) | ⭐ Most preferred (fast, isolated) |"
    $report += "| XAML Tests | $($xamlTestFiles.Count) | ⭐ Most preferred (compile-time) |"
    $report += "| Device Tests | $($deviceTestFiles.Count) | ⭐⭐ Good (platform-specific) |"
    $report += "| UI Tests | $($uiTestFiles.Count) | ⭐⭐⭐ When needed (slow, Appium) |"
}
$report += ""

# Convention checks
$conventionIssueCount = 0

if ($uiTestFiles.Count -gt 0) {
    $report += "## UI Test Convention Checks"
    $report += ""
    foreach ($testFile in $uiTestFiles) {
        $baseName = [System.IO.Path]::GetFileNameWithoutExtension($testFile)
        $matchingHostFiles = $uiHostAppFiles | Where-Object {
            # Handle .xaml.cs: GetFileNameWithoutExtension("Issue12345.xaml.cs") → "Issue12345.xaml"
            $hostName = [System.IO.Path]::GetFileNameWithoutExtension($_)
            $hostName = $hostName -replace '\.xaml$', ''
            $hostName -eq $baseName
        }
        $result = Test-UITestConventions -TestFile $testFile -HostAppFiles $matchingHostFiles
        $safeBase = Escape-ForCodeSpan $baseName
        $report += "### ``$safeBase``"
        if ($result.Info.Count -gt 0) {
            foreach ($i in $result.Info) { $report += "- ℹ️ $i" }
        }
        if ($result.Issues.Count -eq 0) {
            $report += "- ✅ No convention issues found"
        }
        else {
            foreach ($issue in $result.Issues) {
                $report += "- ⚠️ $issue"
                $conventionIssueCount++
            }
        }
        $report += ""
    }
}

if ($unitTestFiles.Count -gt 0) {
    $report += "## Unit Test Convention Checks"
    $report += ""
    foreach ($testFile in $unitTestFiles) {
        $baseName = [System.IO.Path]::GetFileNameWithoutExtension($testFile)
        $result = Test-UnitTestConventions -TestFile $testFile
        $safeBase = Escape-ForCodeSpan $baseName
        $report += "### ``$safeBase``"
        if ($result.Info.Count -gt 0) {
            foreach ($i in $result.Info) { $report += "- ℹ️ $i" }
        }
        if ($result.Issues.Count -eq 0) {
            $report += "- ✅ No convention issues found"
        }
        else {
            foreach ($issue in $result.Issues) {
                $report += "- ⚠️ $issue"
                $conventionIssueCount++
            }
        }
        $report += ""
    }
}

if ($xamlTestFiles.Count -gt 0) {
    $report += "## XAML Test Convention Checks"
    $report += ""
    foreach ($testFile in ($xamlTestFiles | Where-Object { $_ -match "\.cs$" })) {
        $baseName = [System.IO.Path]::GetFileNameWithoutExtension($testFile)
        $result = Test-XamlTestConventions -TestFile $testFile
        $safeBase = Escape-ForCodeSpan $baseName
        $report += "### ``$safeBase``"
        if ($result.Info.Count -gt 0) {
            foreach ($i in $result.Info) { $report += "- ℹ️ $i" }
        }
        if ($result.Issues.Count -eq 0) {
            $report += "- ✅ No convention issues found"
        }
        else {
            foreach ($issue in $result.Issues) {
                $report += "- ⚠️ $issue"
                $conventionIssueCount++
            }
        }
        $report += ""
    }
}

# --- 6. Find existing similar tests ---
$report += "## Existing Similar Tests"
$report += ""

# Extract control/feature names from fix files
$controlNames = @()
foreach ($file in $fixFiles) {
    # Extract from path patterns like Controls/src/Core/Button/ or Handlers/Entry/
    if ($file -match "Controls/src/Core/(\w+)/" ) { $controlNames += $Matches[1] }
    if ($file -match "Handlers/(\w+)/" ) { $controlNames += $Matches[1] }
    if ($file -match "Platform/\w+/(\w+)\." ) { $controlNames += $Matches[1] }

    # Extract from file name (e.g., ButtonHandler.Android.cs → Button)
    $stem = [System.IO.Path]::GetFileNameWithoutExtension($file)
    $stem = $stem -replace "\.(Android|iOS|Windows|MacCatalyst|Tizen|Standard)$", ""
    if ($stem -match "^(\w+?)Handler$") { $controlNames += $Matches[1] }
}
$controlNames = $controlNames | Select-Object -Unique | Where-Object { $_.Length -gt 2 }

if ($controlNames.Count -gt 0) {
    foreach ($control in $controlNames) {
        $existing = @()
        # Search in UI tests
        $uiMatches = Get-ChildItem -Path "$RepoRoot/src/Controls/tests/TestCases.Shared.Tests/Tests" -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue |
            Select-String -Pattern "\b$control\b" -List -ErrorAction SilentlyContinue |
            Select-Object -ExpandProperty Path -First 5
        if ($uiMatches) { $existing += $uiMatches | ForEach-Object { $_ -replace [regex]::Escape("$RepoRoot/"), "" } }

        # Search in unit tests
        $unitMatches = Get-ChildItem -Path "$RepoRoot/src/Controls/tests/Core.UnitTests" -Recurse -Filter "*$control*" -ErrorAction SilentlyContinue |
            Select-Object -ExpandProperty FullName -First 5
        if ($unitMatches) { $existing += $unitMatches | ForEach-Object { $_ -replace [regex]::Escape("$RepoRoot/"), "" } }

        if ($existing.Count -gt 0) {
            $report += "### Existing tests for ``$control``"
            foreach ($f in $existing) {
                $report += "- ``$f``"
            }
            $report += ""
        }
    }
}
else {
    $report += "_Could not detect control/feature names from fix files._"
    $report += ""
}

# --- 7. Platform scope analysis ---
$report += "## Platform Scope Analysis"
$report += ""

$platformIndicators = @{
    "Android"      = ($fixFiles | Where-Object { $_ -match "Android|\.android\." }).Count
    "iOS"          = ($fixFiles | Where-Object { $_ -match "\.ios\." -or ($_ -match "/iOS/" -and $_ -notmatch "MacCatalyst") }).Count
    "MacCatalyst"  = ($fixFiles | Where-Object { $_ -match "MacCatalyst|\.maccatalyst\." -or $_ -match "\.ios\." }).Count
    "Windows"      = ($fixFiles | Where-Object { $_ -match "Windows|\.windows\." }).Count
    "Cross-platform" = ($fixFiles | Where-Object { $_ -notmatch "Android|\.android\.|\.ios\.|/iOS/|MacCatalyst|\.maccatalyst\.|Windows|\.windows\." }).Count
}

$report += "| Platform | Fix Files Touching |"
$report += "|----------|--------------------|"
foreach ($kv in $platformIndicators.GetEnumerator()) {
    $indicator = if ($kv.Value -gt 0) { "✅ $($kv.Value) file(s)" } else { "—" }
    $report += "| $($kv.Key) | $indicator |"
}
$report += ""

if ($platformIndicators["Cross-platform"] -gt 0) {
    $report += "ℹ️ Cross-platform fix files detected — tests should ideally run on **all platforms**."
    $report += ""
}

# --- 8. Summary ---
$report += "## Summary"
$report += ""
$report += "| Metric | Value |"
$report += "|--------|-------|"
$report += "| Total changed files | $($changedFiles.Count) |"
$report += "| Fix files | $($fixFiles.Count) |"
$report += "| Test files | $totalTests |"
$report += "| Convention issues | $conventionIssueCount |"

$testRatio = if ($fixFiles.Count -gt 0 -and $totalTests -gt 0) { "✅ Tests present" } elseif ($fixFiles.Count -gt 0) { "⚠️ No tests for fix" } else { "ℹ️ No fix files" }
$report += "| Test coverage | $testRatio |"
$report += ""

# --- Write output ---
$report | Out-File -FilePath $reportPath -Encoding utf8

Write-Host ""
Write-Host "═══════════════════════════════════════════════════"
Write-Host "  Context report: $reportPath"
Write-Host "  Convention issues: $conventionIssueCount"
Write-Host "  Test files: $totalTests | Fix files: $($fixFiles.Count)"
Write-Host "═══════════════════════════════════════════════════"
Write-Host ""

# Also print the report to stdout for agent consumption
$report | ForEach-Object { Write-Host $_ }
