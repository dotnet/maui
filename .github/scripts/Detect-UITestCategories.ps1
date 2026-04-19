<#
.SYNOPSIS
    Detects UI test and device test categories affected by a PR diff.

.DESCRIPTION
    Analyzes a PR diff (or a local diff) and produces the set of UI test and
    device test categories that should run. Categories are detected from two
    sources:

      1. [Category(...)] attributes in added/modified test code lines.
         Supports:
           - [Category(UITestCategories.X)]
           - [Category(nameof(UITestCategories.X))]
           - [Category("X")]
           - [Category(TestCategory.X)]  (device tests)
           - [Category(nameof(TestCategory.X))]

      2. Path-based inference from changed source files (when no test
         category attributes are present, e.g. a fix in src/Controls/src/Core/
         that ships without a new test).

    Output is plain text by default, or JSON with -Json. Designed to be
    runnable locally and from CI without any external dependencies beyond
    `git` (and `gh` if -PrNumber is used).

.PARAMETER PrNumber
    GitHub PR number to analyze. Requires `gh` CLI authenticated.
    Mutually exclusive with -DiffFile and -BaseRef.

.PARAMETER DiffFile
    Path to a unified diff file to analyze.
    Mutually exclusive with -PrNumber and -BaseRef.

.PARAMETER BaseRef
    Git ref to diff against (e.g. 'origin/main', 'main'). Defaults to
    'origin/main' when no other source is provided.

.PARAMETER Repository
    GitHub repository in 'owner/repo' form. Defaults to 'dotnet/maui'.
    Only used when -PrNumber is set.

.PARAMETER Json
    Emit results as JSON (single object) instead of human-readable text.

.PARAMETER OutputFile
    Optional path to also write the output to a file.

.EXAMPLE
    pwsh .github/scripts/Detect-UITestCategories.ps1 -PrNumber 34856

.EXAMPLE
    pwsh .github/scripts/Detect-UITestCategories.ps1 -BaseRef origin/main -Json

.EXAMPLE
    git diff origin/main > /tmp/pr.diff
    pwsh .github/scripts/Detect-UITestCategories.ps1 -DiffFile /tmp/pr.diff
#>
[CmdletBinding()]
param(
    [int]$PrNumber,
    [string]$DiffFile,
    [string]$BaseRef,
    [string]$Repository = 'dotnet/maui',
    [switch]$Json,
    [string]$OutputFile
)

$ErrorActionPreference = 'Stop'

# ── 1. Acquire the diff and the changed-files list ──────────────────────────
function Get-DiffAndFiles {
    param([int]$PrNumber, [string]$DiffFile, [string]$BaseRef, [string]$Repository)

    if ($PrNumber -gt 0) {
        if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
            throw "gh CLI is required for -PrNumber. Install from https://cli.github.com/"
        }
        $diff = gh pr diff $PrNumber --repo $Repository 2>$null
        if ($LASTEXITCODE -ne 0 -or -not $diff) {
            throw "Failed to fetch diff for PR #$PrNumber from $Repository"
        }
        $files = gh pr diff $PrNumber --repo $Repository --name-only 2>$null
        return @{ Diff = ($diff -join "`n"); Files = @($files | Where-Object { $_ }) }
    }

    if ($DiffFile) {
        if (-not (Test-Path $DiffFile)) { throw "Diff file not found: $DiffFile" }
        $diff = Get-Content -Raw $DiffFile
        $files = ($diff -split "`n") |
            Where-Object { $_ -match '^\+\+\+ b/(.+)$' } |
            ForEach-Object { $Matches[1] }
        return @{ Diff = $diff; Files = @($files) }
    }

    $ref = if ($BaseRef) { $BaseRef } else { 'origin/main' }
    $diff  = git diff $ref 2>$null
    if ($LASTEXITCODE -ne 0) { throw "git diff $ref failed" }
    $files = git diff $ref --name-only 2>$null
    return @{ Diff = ($diff -join "`n"); Files = @($files | Where-Object { $_ }) }
}

# ── 2. Extract [Category(...)] attributes from added lines ──────────────────
function Get-CategoriesFromDiff {
    param([string]$Diff, [string]$EnumName)

    $added = ($Diff -split "`n") | Where-Object { $_ -match '^\+[^+]' -and $_ -match '\[Category\(' }
    $found = New-Object System.Collections.Generic.HashSet[string]

    $patterns = @(
        "$EnumName\.([A-Za-z0-9_]+)",                  # UITestCategories.Button
        "nameof\(\s*$EnumName\.([A-Za-z0-9_]+)\s*\)"   # nameof(UITestCategories.Button)
    )

    foreach ($line in $added) {
        foreach ($pat in $patterns) {
            foreach ($m in [regex]::Matches($line, $pat)) {
                [void]$found.Add($m.Groups[1].Value)
            }
        }
        # Quoted form: [Category("Button")] — only used for UI categories
        if ($EnumName -eq 'UITestCategories') {
            foreach ($m in [regex]::Matches($line, '\[Category\("([A-Za-z0-9_]+)"\)\]')) {
                [void]$found.Add($m.Groups[1].Value)
            }
        }
    }
    return @($found | Sort-Object)
}

# ── 3. Path-based inference (fallback when no Category attributes detected) ─
# Each rule: regex that must match the file path → array of categories.
$pathRules = @(
    @{ Pattern = 'CollectionView|/Items2?/'; UI = @('CollectionView','CarouselView'); Device = @('CollectionView','CarouselView') }
    @{ Pattern = 'CarouselView';       UI = @('CarouselView');     Device = @('CarouselView') }
    @{ Pattern = 'ListView';           UI = @('ListView');         Device = @('ListView') }
    @{ Pattern = '\bShell\b';          UI = @('Shell');            Device = @('Shell') }
    @{ Pattern = 'NavigationPage|/Navigation/'; UI = @('Navigation'); Device = @('NavigationPage') }
    @{ Pattern = 'TabbedPage';         UI = @('TabbedPage');       Device = @('TabbedPage') }
    @{ Pattern = 'FlyoutPage';         UI = @('FlyoutPage');       Device = @('FlyoutPage') }
    @{ Pattern = 'ImageButton';        UI = @('ImageButton');      Device = @('ImageButton') }
    @{ Pattern = '\bButton\b';         UI = @('Button');           Device = @('Button') }
    @{ Pattern = 'SearchBar';          UI = @('SearchBar');        Device = @('SearchBar') }
    @{ Pattern = 'DatePicker';         UI = @('DatePicker');       Device = @('DatePicker') }
    @{ Pattern = 'TimePicker';         UI = @('TimePicker');       Device = @('TimePicker') }
    @{ Pattern = '\bPicker\b';         UI = @('Picker');           Device = @('Picker') }
    @{ Pattern = 'CheckBox';           UI = @('CheckBox');         Device = @('CheckBox') }
    @{ Pattern = 'RadioButton';        UI = @('RadioButton');      Device = @('RadioButton') }
    @{ Pattern = 'RefreshView';        UI = @('RefreshView');      Device = @('RefreshView') }
    @{ Pattern = 'SwipeView';          UI = @('SwipeView');        Device = @('SwipeView') }
    @{ Pattern = 'IndicatorView';      UI = @('IndicatorView');    Device = @('IndicatorView') }
    @{ Pattern = 'ProgressBar';        UI = @('ProgressBar');      Device = @('ProgressBar') }
    @{ Pattern = 'ActivityIndicator';  UI = @('ActivityIndicator');Device = @('ActivityIndicator') }
    @{ Pattern = 'ScrollView';         UI = @('ScrollView');       Device = @('ScrollView') }
    @{ Pattern = '\bWebView\b';        UI = @('WebView');          Device = @('WebView') }
    @{ Pattern = 'BlazorWebView';      UI = @('WebView');          Device = @('BlazorWebView') }
    @{ Pattern = 'HybridWebView';      UI = @('WebView');          Device = @('HybridWebView') }
    @{ Pattern = '\bEntry\b';          UI = @('Entry');            Device = @('Entry') }
    @{ Pattern = '\bEditor\b';         UI = @('Editor');           Device = @('Editor') }
    @{ Pattern = '\bLabel\b';          UI = @('Label');            Device = @('Label') }
    @{ Pattern = '\bSwitch\b';         UI = @('Switch');           Device = @() }
    @{ Pattern = '\bSlider\b';         UI = @('Slider');           Device = @('Slider') }
    @{ Pattern = '\bStepper\b';        UI = @('Stepper');          Device = @('Stepper') }
    @{ Pattern = '\bBorder\b';         UI = @('Border');           Device = @('Border') }
    @{ Pattern = '\bFrame\b';          UI = @('Frame');            Device = @() }
    @{ Pattern = '\bBoxView\b';        UI = @('BoxView');          Device = @() }
    @{ Pattern = '\bShape\b|/Shapes/'; UI = @('Shape');            Device = @() }
    @{ Pattern = '\bShadow\b';         UI = @('Shadow');           Device = @() }
    @{ Pattern = '\bBrush\b';          UI = @('Brush');            Device = @() }
    @{ Pattern = 'ImageButton|\bImage\.cs|/Image/'; UI = @('Image'); Device = @('Image') }
    @{ Pattern = '\bGesture';          UI = @('Gestures');         Device = @('Gesture') }
    @{ Pattern = 'DragDrop|DragAndDrop'; UI = @('DragAndDrop');    Device = @() }
    @{ Pattern = 'Accessibility';      UI = @('Accessibility');    Device = @('Accessibility') }
    @{ Pattern = 'SafeArea';           UI = @('SafeAreaEdges');    Device = @() }
    @{ Pattern = '/Layout/|Layout\.cs'; UI = @('Layout');          Device = @('Layout') }
    @{ Pattern = '/Window/';           UI = @('Window');           Device = @('Window') }
    @{ Pattern = '/Page/';             UI = @('Page');             Device = @('Page') }
    @{ Pattern = '/Toolbar/';          UI = @('ToolbarItem');      Device = @('Toolbar') }
    @{ Pattern = '/Fonts/';            UI = @('Fonts');            Device = @('Fonts') }
    @{ Pattern = 'src/Graphics/';      UI = @('GraphicsView');     Device = @('Graphics') }
    @{ Pattern = 'src/Essentials/';    UI = @();                   Device = @('Essentials') }
)

function Get-CategoriesFromPaths {
    param([string[]]$Files)
    $ui     = New-Object System.Collections.Generic.HashSet[string]
    $device = New-Object System.Collections.Generic.HashSet[string]
    foreach ($file in $Files) {
        foreach ($rule in $pathRules) {
            if ($file -match $rule.Pattern) {
                foreach ($c in $rule.UI)     { [void]$ui.Add($c) }
                foreach ($c in $rule.Device) { [void]$device.Add($c) }
            }
        }
    }
    return @{
        UI     = @($ui     | Sort-Object)
        Device = @($device | Sort-Object)
    }
}

# ── 4. Run ──────────────────────────────────────────────────────────────────
$result = Get-DiffAndFiles -PrNumber $PrNumber -DiffFile $DiffFile -BaseRef $BaseRef -Repository $Repository
$diff   = $result.Diff
$files  = @($result.Files)

$uiFromAttr     = Get-CategoriesFromDiff -Diff $diff -EnumName 'UITestCategories'
$deviceFromAttr = Get-CategoriesFromDiff -Diff $diff -EnumName 'TestCategory'
$inferred       = Get-CategoriesFromPaths -Files $files

# Attribute-detected categories take priority. If none, fall back to path inference.
$uiCategories     = if ($uiFromAttr.Count -gt 0)     { $uiFromAttr }     else { $inferred.UI }
$deviceCategories = if ($deviceFromAttr.Count -gt 0) { $deviceFromAttr } else { $inferred.Device }

$uiSource     = if ($uiFromAttr.Count -gt 0)     { 'attribute' } elseif ($uiCategories.Count     -gt 0) { 'path-inference' } else { 'none' }
$deviceSource = if ($deviceFromAttr.Count -gt 0) { 'attribute' } elseif ($deviceCategories.Count -gt 0) { 'path-inference' } else { 'none' }

$testFiles   = @($files | Where-Object { $_ -match '(?i)tests?/|TestCases|UnitTests|DeviceTests' -and $_ -match '\.(cs|xaml)$' })
$sourceFiles = @($files | Where-Object { $_ -match '\.(cs|xaml)$' -and $_ -notmatch '(?i)tests?/|TestCases|UnitTests|DeviceTests' })

# ── 5. Format output ────────────────────────────────────────────────────────
if ($Json) {
    $output = [pscustomobject]@{
        prNumber           = $PrNumber
        uiCategories       = $uiCategories
        uiCategoriesSource = $uiSource
        uiCategoriesFilter = ($uiCategories -join ',')
        deviceCategories   = $deviceCategories
        deviceCategoriesSource = $deviceSource
        deviceCategoriesFilter = ($deviceCategories -join ';')
        testFiles          = $testFiles
        sourceFiles        = $sourceFiles
        totalChangedFiles  = $files.Count
    } | ConvertTo-Json -Depth 5
} else {
    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine("UI test categories  ($uiSource):     " + ($(if ($uiCategories.Count)     { $uiCategories     -join ', ' } else { '(none)' })))
    [void]$sb.AppendLine("Device test categories ($deviceSource): " + ($(if ($deviceCategories.Count) { $deviceCategories -join ', ' } else { '(none)' })))
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("Pipeline parameters:")
    [void]$sb.AppendLine("  uiTestCategories     = $($uiCategories -join ',')")
    [void]$sb.AppendLine("  deviceTestCategories = $($deviceCategories -join ';')")
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("Changed files: $($files.Count) total ($($testFiles.Count) test, $($sourceFiles.Count) source)")
    $output = $sb.ToString().TrimEnd()
}

Write-Output $output
if ($OutputFile) {
    $output | Set-Content -Path $OutputFile -Encoding utf8
    Write-Verbose "Wrote output to $OutputFile"
}
