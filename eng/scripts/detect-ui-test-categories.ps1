[CmdletBinding()]
param(
    [string]$TargetBranch,
    [string]$TestRoot = "src/Controls/tests/TestCases.Shared.Tests"
)

$buildReason = $env:BUILD_REASON
if ([string]::IsNullOrWhiteSpace($buildReason)) {
    $buildReason = $env:SYSTEM_REASON
}

if ($buildReason -ne 'PullRequest') {
    Write-Host "Build reason '$buildReason' is not PullRequest. Skipping category detection." -ForegroundColor Cyan
    return
}

if ([string]::IsNullOrWhiteSpace($TargetBranch)) {
    $TargetBranch = $env:SYSTEM_PULLREQUEST_TARGETBRANCH
}

if ([string]::IsNullOrWhiteSpace($TargetBranch)) {
    Write-Warning "Unable to determine target branch for comparison. Falling back to running all categories."
    return
}

$targetBranch = $TargetBranch -replace '^refs/heads/', ''

Write-Host "Fetching target branch 'origin/${targetBranch}' for diff analysis..." -ForegroundColor Cyan
try {
    git fetch origin "${targetBranch}" --no-tags --prune --depth=200 | Out-Null
} catch {
    Write-Warning "Failed to fetch origin/${targetBranch}: $($_.Exception.Message). Falling back to running all categories."
    return
}

$mergeBase = $null
try {
    $mergeBase = (git merge-base HEAD "origin/${targetBranch}").Trim()
} catch {
    Write-Warning "Could not determine merge base with origin/${targetBranch}: $($_.Exception.Message). Falling back to running all categories."
    return
}

if ([string]::IsNullOrWhiteSpace($mergeBase)) {
    Write-Warning "Merge base calculation returned empty result. Falling back to running all categories."
    return
}

Write-Host "Calculating diff between $mergeBase and HEAD limited to '$TestRoot'..." -ForegroundColor Cyan
$diff = git diff --diff-filter=AMR --unified=0 $mergeBase HEAD -- "$TestRoot"
if ([string]::IsNullOrWhiteSpace($diff)) {
    Write-Host "No changes detected under '$TestRoot'. Falling back to default category matrix." -ForegroundColor Cyan
    return
}

$categoryPattern = '^[+]{1}\s*\[Category\((?<value>[^\)]*)\)\]'
$addedCategories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

foreach ($line in $diff -split "`n") {
    if ($line -match $categoryPattern) {
        $rawValue = $Matches['value'].Trim()
        if ([string]::IsNullOrWhiteSpace($rawValue)) {
            continue
        }

        # Normalize value: UITestCategories.XYZ => XYZ, quoted strings => trimmed text
        if ($rawValue -match '^UITestCategories\.(?<name>[A-Za-z0-9_]+)$') {
            $category = $Matches['name']
        } elseif ($rawValue -match '^["''](?<name>[A-Za-z0-9_ -]+)["'']$') {
            $category = $Matches['name']
        } else {
            # Attempt to evaluate nameof-style constructs or fallback to raw value
            if ($rawValue -match 'nameof\(UITestCategories\.(?<name>[A-Za-z0-9_]+)\)') {
                $category = $Matches['name']
            } else {
                Write-Warning "Unrecognized category expression '$rawValue'. Falling back to running all categories."
                return
            }
        }

        $category = $category.Trim()
        if (-not [string]::IsNullOrWhiteSpace($category)) {
            $addedCategories.Add($category) | Out-Null
        }
    }
}

if ($addedCategories.Count -eq 0) {
    Write-Host "No new Category attributes detected in diff. Using default category matrix." -ForegroundColor Cyan
    return
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