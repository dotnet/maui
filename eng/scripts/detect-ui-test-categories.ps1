[CmdletBinding()]
param(
    [string]$TargetBranch,
    [string]$PrNumber,
    [string]$Categories,
    [string]$AiCategories,
    [string]$TestRoot = "src/Controls/tests/TestCases.Shared.Tests"
)

# Normalize PrNumber: strip whitespace; AzDO often passes a placeholder space when the parameter is unset.
if (-not [string]::IsNullOrWhiteSpace($PrNumber)) {
    $PrNumber = $PrNumber.Trim()
} else {
    $PrNumber = $null
}

# Normalize Categories parameter
if (-not [string]::IsNullOrWhiteSpace($Categories)) {
    $Categories = $Categories.Trim()
} else {
    $Categories = $null
}

# Normalize AiCategories parameter (from AI reasoning during pre-flight)
if (-not [string]::IsNullOrWhiteSpace($AiCategories)) {
    $AiCategories = $AiCategories.Trim()
} else {
    $AiCategories = $null
}

$buildReason = $env:BUILD_REASON
if ([string]::IsNullOrWhiteSpace($buildReason)) {
    $buildReason = $env:SYSTEM_REASON
}

$isManualPrTest = -not [string]::IsNullOrWhiteSpace($PrNumber)

# Track manual-PR-mode git state mutations so they can be undone on exit.
# This script is invoked as a separate `pwsh` process by Review-PR.ps1, but the
# git working tree is shared on disk — so a stray detached HEAD or leftover
# `_detect_base` / `_detect_head` remote will be visible to subsequent steps
# (e.g., the gate's `git diff`). Always clean up before returning.
$script:detectOriginalRef = $null
$script:detectHeadMutated = $false

# When categories are explicitly provided, skip auto-detection entirely.
# This lets triage/maintainers run specific categories via manual queue.
if (-not [string]::IsNullOrWhiteSpace($Categories)) {
    $catList = @($Categories -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    Write-Host "##[section]Categories provided via parameter: $($catList -join ', '). Skipping auto-detection." -ForegroundColor Green
    $matrix = [ordered]@{}
    $index = 0
    foreach ($cat in ($catList | Sort-Object)) {
        $matrix["Category_$index"] = @{ CATEGORYGROUP = $cat }
        $index++
    }
    $matrixJson = $matrix | ConvertTo-Json -Depth 5
    Write-Host "##vso[task.setvariable variable=UITestCategoryMatrix;isOutput=true]$matrixJson"
    Write-Host "##vso[task.setvariable variable=UITestCategoryList;isOutput=true]$($catList -join ',')"
    return
}

if ($buildReason -ne 'PullRequest' -and -not $isManualPrTest) {
    Write-Host "Build reason '$buildReason' is not PullRequest and no -PrNumber override was provided. Running all categories." -ForegroundColor Cyan
    return
}

if ([string]::IsNullOrWhiteSpace($TargetBranch)) {
    $TargetBranch = $env:SYSTEM_PULLREQUEST_TARGETBRANCH
}

# Determine the PR number for label / API lookups.
$prNumberForLookup = $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER
if ([string]::IsNullOrWhiteSpace($prNumberForLookup) -and $isManualPrTest) {
    $prNumberForLookup = $PrNumber
}
$repoName = $env:BUILD_REPOSITORY_NAME
if ([string]::IsNullOrWhiteSpace($repoName)) {
    $repoName = 'dotnet/maui'
}

# Helper: build authenticated GitHub API headers.
function Get-GitHubHeaders {
    $h = @{ 'User-Agent' = 'maui-ui-test-detector' }
    if (-not [string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
        $h['Authorization'] = "Bearer $env:GH_TOKEN"
    } else {
        Write-Host "⚠️ No GitHub token available (set GH_TOKEN env var)" -ForegroundColor Yellow
    }
    return $h
}

# Helper: invoke a REST call with retries for transient failures.
function Invoke-WithRetry {
    param([string]$Uri, [hashtable]$Headers, [int]$MaxRetries = 3, [int]$DelaySeconds = 10)
    for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
        try {
            return Invoke-RestMethod -Uri $Uri -Headers $Headers -Method Get -TimeoutSec 30
        } catch {
            Write-Host "##[warning]Attempt $attempt/$MaxRetries failed: $($_.Exception.Message)"
            if ($attempt -lt $MaxRetries) {
                Write-Host "Retrying in ${DelaySeconds}s..."
                Start-Sleep -Seconds $DelaySeconds
            } else {
                throw
            }
        }
    }
}

# Manual-test override: when -PrNumber is provided, fetch the PR's base/head from GitHub
# and replay the same diff that a normal PR build would see.
if ($isManualPrTest) {
    try {
        $prUrl = "https://api.github.com/repos/$repoName/pulls/$PrNumber"
        Write-Host "##[section]Manual PR test mode (PrNumber=$PrNumber). Fetching PR metadata from $prUrl" -ForegroundColor Yellow
        $pr = Invoke-WithRetry -Uri $prUrl -Headers (Get-GitHubHeaders)

        # Guard against fork-deletion / partial responses that would leave critical
        # fields null and produce confusing `git remote add ""` errors below.
        if ($null -eq $pr -or $null -eq $pr.head -or $null -eq $pr.head.repo -or $null -eq $pr.base -or $null -eq $pr.base.repo) {
            Write-Host "##[warning]Incomplete PR API response (the source fork may have been deleted). Falling back to running ALL categories."
            return
        }
        $TargetBranch = $pr.base.ref
        $headRef = $pr.head.ref
        $headSha = $pr.head.sha
        $baseRepoCloneUrl = $pr.base.repo.clone_url
        $headRepoCloneUrl = $pr.head.repo.clone_url
        if ([string]::IsNullOrWhiteSpace($headSha) -or [string]::IsNullOrWhiteSpace($headRepoCloneUrl) -or [string]::IsNullOrWhiteSpace($baseRepoCloneUrl)) {
            Write-Host "##[warning]PR API response missing SHA or clone URL. Falling back to running ALL categories."
            return
        }
        Write-Host "PR #$PrNumber : $($pr.head.repo.full_name)/$headRef ($headSha) -> $($pr.base.repo.full_name)/$TargetBranch" -ForegroundColor Cyan

        # Capture the original HEAD so we can restore it before exiting.
        # symbolic-ref returns the branch name; if HEAD is already detached, fall back to the SHA.
        $script:detectOriginalRef = (& git symbolic-ref --short HEAD 2>$null)
        if ([string]::IsNullOrWhiteSpace($script:detectOriginalRef)) {
            $script:detectOriginalRef = (& git rev-parse HEAD 2>$null)
        }

        try {
            # Fetch base branch from the base repo.
            git remote remove _detect_base 2>$null | Out-Null
            git remote add _detect_base $baseRepoCloneUrl
            git fetch _detect_base "$TargetBranch" --no-tags --prune --depth=200 | Out-Null
            git update-ref refs/remotes/origin/$TargetBranch _detect_base/$TargetBranch | Out-Null

            # Fetch head commit (works for forks too) and check it out so the diff reflects the PR changes.
            git remote remove _detect_head 2>$null | Out-Null
            git remote add _detect_head $headRepoCloneUrl
            git fetch _detect_head "$headSha" --no-tags --depth=200 | Out-Null
            git checkout --quiet $headSha | Out-Null
            $script:detectHeadMutated = $true
        } finally {
            # Temp remotes were only needed for the fetch — drop them whether or not setup succeeded
            # so we don't leave stray entries in `.git/config` for future runs.
            git remote remove _detect_base 2>$null | Out-Null
            git remote remove _detect_head 2>$null | Out-Null
        }
    } catch {
        Write-Host "##[warning]Manual PR test setup failed: $($_.Exception.Message). Falling back to running ALL categories."
        return
    }
}

# Wrap the rest of the script so the manual-PR HEAD checkout is always reverted
# before exit, regardless of which `return` path or unhandled error is taken.
try {

# Escape hatch: PR label "run-all-uitests" forces the full category matrix to run.
if (-not [string]::IsNullOrWhiteSpace($prNumberForLookup)) {
    try {
        $labelsUrl = "https://api.github.com/repos/$repoName/issues/$prNumberForLookup/labels"
        Write-Host "Checking PR labels at $labelsUrl" -ForegroundColor Cyan
        $labels = Invoke-WithRetry -Uri $labelsUrl -Headers (Get-GitHubHeaders)
        $labelNames = @($labels | ForEach-Object { $_.name })
        Write-Host "PR labels: $([string]::Join(', ', $labelNames))" -ForegroundColor Cyan
        if ($labelNames -contains 'run-all-uitests') {
            Write-Host "##[section]Label 'run-all-uitests' present. Running ALL UI test categories (detection bypassed)." -ForegroundColor Yellow
            return
        }
    } catch {
        Write-Host "##[warning]Failed to query PR labels: $($_.Exception.Message). Continuing with category detection."
    }
}

if ([string]::IsNullOrWhiteSpace($TargetBranch)) {
    Write-Host "##[warning]Unable to determine target branch for comparison."
    Write-Host "##[section]FALLBACK: All UI test categories will run for this PR."
    return
}

$targetBranch = $TargetBranch -replace '^refs/heads/', ''

Write-Host "Fetching target branch 'origin/${targetBranch}' for diff analysis..." -ForegroundColor Cyan
try {
    git fetch origin "${targetBranch}" --no-tags --prune --depth=200 | Out-Null
} catch {
    Write-Host "##[warning]Failed to fetch origin/${targetBranch}: $($_.Exception.Message)"
    Write-Host "##[section]FALLBACK: All UI test categories will run for this PR."
    return
}

$mergeBase = $null
try {
    $mergeBase = (git merge-base HEAD "origin/${targetBranch}").Trim()
} catch {
    Write-Host "##[warning]Could not determine merge base with origin/${targetBranch}: $($_.Exception.Message)"
    Write-Host "##[section]FALLBACK: All UI test categories will run for this PR."
    return
}

if ([string]::IsNullOrWhiteSpace($mergeBase)) {
    Write-Host "##[warning]Merge base calculation returned empty result."
    Write-Host "##[section]FALLBACK: All UI test categories will run for this PR."
    return
}

Write-Host "Calculating diff between $mergeBase and HEAD limited to '$TestRoot'..." -ForegroundColor Cyan
$diff = git diff --diff-filter=AMR --unified=0 $mergeBase HEAD -- "$TestRoot"

# Also get the full file list for Tier 2 source-path mapping
$allChangedFiles = @(git diff --diff-filter=AMR --name-only $mergeBase HEAD)
Write-Host "Total changed files in PR: $($allChangedFiles.Count)" -ForegroundColor Cyan

# ============================================================================
# TIER 2: Source-path-to-category mapping table
# Maps product code paths to UI test categories by naming conventions.
# ============================================================================

$pathToCategoryMap = @(
    @{ Pattern = 'src/Controls/src/Core/Button';              Category = 'Button' }
    @{ Pattern = 'src/Controls/src/Core/Border';              Category = 'Border' }
    @{ Pattern = 'src/Controls/src/Core/BoxView';             Category = 'BoxView' }
    @{ Pattern = 'src/Controls/src/Core/CarouselView';        Category = 'CarouselView' }
    @{ Pattern = 'src/Controls/src/Core/Handlers/Items';      Category = 'CollectionView' }
    @{ Pattern = 'src/Controls/src/Core/Handlers/Items2';     Category = 'CollectionView' }
    @{ Pattern = 'src/Controls/src/Core/CheckBox';            Category = 'CheckBox' }
    @{ Pattern = 'src/Controls/src/Core/DatePicker';          Category = 'DatePicker' }
    @{ Pattern = 'src/Controls/src/Core/Editor';              Category = 'Editor' }
    @{ Pattern = 'src/Controls/src/Core/Entry';               Category = 'Entry' }
    @{ Pattern = 'src/Controls/src/Core/FlyoutPage';          Category = 'FlyoutPage' }
    @{ Pattern = 'src/Controls/src/Core/Frame';               Category = 'Frame' }
    @{ Pattern = 'src/Controls/src/Core/GestureRecognizer';   Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/PanGesture';          Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/PinchGesture';        Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/SwipeGesture';        Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/TapGesture';          Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/PointerGesture';      Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/DragGesture';         Category = 'DragAndDrop' }
    @{ Pattern = 'src/Controls/src/Core/DragGesture';         Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/DropGesture';         Category = 'DragAndDrop' }
    @{ Pattern = 'src/Controls/src/Core/DropGesture';         Category = 'Gestures' }
    @{ Pattern = 'src/Controls/src/Core/Image.';              Category = 'Image' }
    @{ Pattern = 'src/Controls/src/Core/ImageButton';         Category = 'ImageButton' }
    @{ Pattern = 'src/Controls/src/Core/IndicatorView';       Category = 'IndicatorView' }
    @{ Pattern = 'src/Controls/src/Core/Label';               Category = 'Label' }
    @{ Pattern = 'src/Controls/src/Core/Layout';              Category = 'Layout' }
    @{ Pattern = 'src/Controls/src/Core/Grid';                Category = 'Layout' }
    @{ Pattern = 'src/Controls/src/Core/StackLayout';         Category = 'Layout' }
    @{ Pattern = 'src/Controls/src/Core/FlexLayout';          Category = 'Layout' }
    @{ Pattern = 'src/Controls/src/Core/AbsoluteLayout';      Category = 'Layout' }
    @{ Pattern = 'src/Controls/src/Core/ListView';            Category = 'ListView' }
    @{ Pattern = 'src/Controls/src/Core/NavigationPage';      Category = 'Navigation' }
    @{ Pattern = 'src/Controls/src/Core/Page';                Category = 'Page' }
    @{ Pattern = 'src/Controls/src/Core/ContentPage';         Category = 'Page' }
    @{ Pattern = 'src/Controls/src/Core/Picker';              Category = 'Picker' }
    @{ Pattern = 'src/Controls/src/Core/ProgressBar';         Category = 'ProgressBar' }
    @{ Pattern = 'src/Controls/src/Core/RadioButton';         Category = 'RadioButton' }
    @{ Pattern = 'src/Controls/src/Core/RefreshView';         Category = 'RefreshView' }
    @{ Pattern = 'src/Controls/src/Core/ScrollView';          Category = 'ScrollView' }
    @{ Pattern = 'src/Controls/src/Core/SearchBar';           Category = 'SearchBar' }
    @{ Pattern = 'src/Controls/src/Core/Handlers/Shapes';     Category = 'Shape' }
    @{ Pattern = 'src/Controls/src/Core/Shapes';              Category = 'Shape' }
    @{ Pattern = 'src/Controls/src/Core/Shell';               Category = 'Shell' }
    @{ Pattern = 'src/Controls/src/Core/Handlers/Shell';      Category = 'Shell' }
    @{ Pattern = 'src/Controls/src/Core/Slider';              Category = 'Slider' }
    @{ Pattern = 'src/Controls/src/Core/Stepper';             Category = 'Stepper' }
    @{ Pattern = 'src/Controls/src/Core/Switch';              Category = 'Switch' }
    @{ Pattern = 'src/Controls/src/Core/SwipeView';           Category = 'SwipeView' }
    @{ Pattern = 'src/Controls/src/Core/TabbedPage';          Category = 'TabbedPage' }
    @{ Pattern = 'src/Controls/src/Core/TableView';           Category = 'TableView' }
    @{ Pattern = 'src/Controls/src/Core/TimePicker';          Category = 'TimePicker' }
    @{ Pattern = 'src/Controls/src/Core/ToolbarItem';         Category = 'ToolbarItem' }
    @{ Pattern = 'src/Controls/src/Core/WebView';             Category = 'WebView' }
    @{ Pattern = 'src/Controls/src/Core/Window';              Category = 'Window' }
    @{ Pattern = 'src/Controls/src/Core/VisualStateManager';  Category = 'VisualStateManager' }
    @{ Pattern = 'src/Controls/src/Core/Shadow';              Category = 'Shadow' }
    @{ Pattern = 'src/Controls/src/Core/Brush';               Category = 'Brush' }
    @{ Pattern = 'src/Core/src/Handlers/HybridWebView';       Category = 'WebView' }
    @{ Pattern = 'src/Core/src/Platform/';                    Category = 'ViewBaseTests' }
    @{ Pattern = 'src/Core/src/Handlers/';                    Category = 'ViewBaseTests' }
    @{ Pattern = 'src/Essentials/';                           Category = 'Essentials' }
    @{ Pattern = 'src/Controls/src/Core/Handlers/FlyoutPage'; Category = 'FlyoutPage' }
    @{ Pattern = 'src/Controls/src/Core/Handlers/TabbedPage'; Category = 'TabbedPage' }
    @{ Pattern = 'src/Controls/src/Core/Handlers/NavigationPage'; Category = 'Navigation' }
    @{ Pattern = 'src/Controls/src/Core/SafeArea';            Category = 'SafeAreaEdges' }
    @{ Pattern = 'src/Controls/src/Core/Platform/iOS/Extensions/SafeArea'; Category = 'SafeAreaEdges' }
)

if ([string]::IsNullOrWhiteSpace($diff)) {
    Write-Host "No changes detected under '$TestRoot'." -ForegroundColor Cyan
} else {
    Write-Host "Test file changes detected under '$TestRoot'." -ForegroundColor Green
}

$categoryPattern = '^\+\s*\[Category\((?<value>[^\)]*)\)'
$addedCategories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

# ============================================================================
# TIER 1: Scan test file diffs for [Category] attributes
# ============================================================================

if (-not [string]::IsNullOrWhiteSpace($diff)) {
    foreach ($line in $diff -split "`n") {
        if ($line -match $categoryPattern) {
            $rawValue = $Matches['value'].Trim()
            if ([string]::IsNullOrWhiteSpace($rawValue)) {
                continue
            }

            if ($rawValue -match '^UITestCategories\.(?<name>[A-Za-z0-9_]+)$') {
                $category = $Matches['name']
            } elseif ($rawValue -match '^["''](?<name>[A-Za-z0-9_ -]+)["'']$') {
                $category = $Matches['name']
            } else {
                if ($rawValue -match 'nameof\(UITestCategories\.(?<name>[A-Za-z0-9_]+)\)') {
                    $category = $Matches['name']
                } else {
                    # Unrecognized format (e.g., a constant from another class, string concat, interpolation).
                    # Log a warning and continue — Tier 2 (source paths) and Tier 3 (AI) can still fill in categories.
                    # Throwing here would abort detection entirely and silently fall back to "run all".
                    Write-Host "##[warning]Unrecognized category expression '$rawValue'. Expected formats: UITestCategories.<Name>, nameof(UITestCategories.<Name>), or a quoted string. Skipping this entry."
                    continue
                }
            }

            $category = $category.Trim()
            if (-not [string]::IsNullOrWhiteSpace($category)) {
                $addedCategories.Add($category) | Out-Null
            }
        }
    }

    if ($addedCategories.Count -eq 0) {
        # Scan modified test files for existing [Category] attributes
        Write-Host "No new Category attributes in diff. Scanning modified files for existing categories..." -ForegroundColor Cyan
        $modifiedFiles = @(git diff --diff-filter=AMR --name-only $mergeBase HEAD -- "$TestRoot" | Where-Object { $_ -match '\.cs$' })
        foreach ($file in $modifiedFiles) {
            if (-not (Test-Path $file)) { continue }
            $content = Get-Content $file -Raw
            $fileMatches = [regex]::Matches($content, '\[Category\(([^\)]*)\)')
            foreach ($m in $fileMatches) {
                $rawValue = $m.Groups[1].Value.Trim()
                if ([string]::IsNullOrWhiteSpace($rawValue)) { continue }
                if ($rawValue -match '^UITestCategories\.(?<name>[A-Za-z0-9_]+)$') {
                    $cat = $Matches['name']
                } elseif ($rawValue -match '^["''](?<name>[A-Za-z0-9_ -]+)["'']$') {
                    $cat = $Matches['name']
                } elseif ($rawValue -match 'nameof\(UITestCategories\.(?<name>[A-Za-z0-9_]+)\)') {
                    $cat = $Matches['name']
                } else { continue }
                $cat = $cat.Trim()
                if (-not [string]::IsNullOrWhiteSpace($cat)) {
                    $addedCategories.Add($cat) | Out-Null
                }
            }
        }
    }

    if ($addedCategories.Count -gt 0) {
        Write-Host "Tier 1 (test files): $([string]::Join(', ', $addedCategories))" -ForegroundColor Green
    }
}

# ============================================================================
# TIER 2: Source-path heuristic mapping (product code → categories)
# ============================================================================

$tier2Categories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$touchesControls = $false

foreach ($file in $allChangedFiles) {
    if ($file -like 'src/Controls/*' -or $file -like 'src/Core/*' -or $file -like 'src/Essentials/*') {
        $touchesControls = $true
    }
    foreach ($mapping in $pathToCategoryMap) {
        if ($file -like "$($mapping.Pattern)*") {
            $tier2Categories.Add($mapping.Category) | Out-Null
        }
    }
}

if ($tier2Categories.Count -gt 0) {
    Write-Host "Tier 2 (source paths): $([string]::Join(', ', $tier2Categories))" -ForegroundColor Green
    foreach ($c in $tier2Categories) { $addedCategories.Add($c) | Out-Null }
}

# ============================================================================
# TIER 3: AI-provided categories (from pre-flight reasoning)
# ============================================================================

if (-not [string]::IsNullOrWhiteSpace($AiCategories)) {
    $aiCatList = @($AiCategories -split '[,\n]' | ForEach-Object { ($_ -replace '\s*[-—].*$', '').Trim() } | Where-Object { $_ -and $_ -ne 'NONE' })
    if ($aiCatList.Count -gt 0) {
        # Build a set of valid categories from UITestCategories.cs so we can drop AI hallucinations.
        # An invalid category would otherwise create a matrix job that runs zero tests.
        $validCategories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
        $uiTestCategoriesFile = Join-Path $TestRoot 'UITestCategories.cs'
        if (Test-Path $uiTestCategoriesFile) {
            $uiTestCategoriesContent = Get-Content $uiTestCategoriesFile -Raw
            $constMatches = [regex]::Matches($uiTestCategoriesContent, 'public\s+const\s+string\s+\w+\s*=\s*"(?<value>[^"]+)"')
            foreach ($m in $constMatches) {
                $validCategories.Add($m.Groups['value'].Value) | Out-Null
            }
        }

        Write-Host "Tier 3 (AI reasoning): $([string]::Join(', ', $aiCatList))" -ForegroundColor Green
        foreach ($c in $aiCatList) {
            if ($validCategories.Count -gt 0 -and -not $validCategories.Contains($c)) {
                Write-Host "##[warning]AI suggested category '$c' is not defined in UITestCategories.cs. Skipping to avoid creating an empty matrix job."
                continue
            }
            $addedCategories.Add($c) | Out-Null
        }
    }
}

# ============================================================================
# FINAL DECISION
# ============================================================================

if ($addedCategories.Count -eq 0) {
    if ($touchesControls) {
        # Changed files under src/Controls/ but couldn't map to specific categories — run all
        Write-Host "Changed files touch Controls/Core/Essentials but no specific categories identified. Running all." -ForegroundColor Yellow
        return
    } else {
        # No UI-relevant changes at all
        Write-Host "No UI-relevant changes detected. No UI test categories to run." -ForegroundColor Cyan
        Write-Host "##vso[task.setvariable variable=UITestCategoryList;isOutput=true]NONE"
        return
    }
}

Write-Host "Detected categories from PR changes: $([string]::Join(', ', $addedCategories))" -ForegroundColor Green

# Build matrix JSON expected by Azure Pipelines strategy matrix (CATEGORYGROUP values)
$matrix = [ordered]@{}
$index = 0
foreach ($category in ($addedCategories | Sort-Object)) {
    $key = "Category_$index"
    $matrix[$key] = @{ CATEGORYGROUP = $category }
    $index++
}

$matrixJson = $matrix | ConvertTo-Json -Depth 5

Write-Host "##vso[task.setvariable variable=UITestCategoryMatrix;isOutput=true]$matrixJson"
Write-Host "##vso[task.setvariable variable=UITestCategoryList;isOutput=true]$([string]::Join(',', ($addedCategories | Sort-Object)))"

} finally {
    # Restore the working tree to its pre-detection state so subsequent steps
    # in Review-PR.ps1 (e.g., the gate's `git diff`) don't see a detached HEAD.
    if ($script:detectHeadMutated -and -not [string]::IsNullOrWhiteSpace($script:detectOriginalRef)) {
        Write-Host "Restoring HEAD to '$script:detectOriginalRef' (was checked out for category detection)" -ForegroundColor DarkGray
        git checkout --quiet $script:detectOriginalRef 2>$null | Out-Null
    }
}