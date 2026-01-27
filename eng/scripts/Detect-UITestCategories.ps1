<#
.SYNOPSIS
    Detects which UI test categories should run based on changed files in a PR.

.DESCRIPTION
    This script analyzes the files changed in a PR and determines which UI test
    categories are relevant. It uses a mapping file to associate file patterns
    with test categories.

    The script outputs Azure DevOps pipeline variables that can be used to
    conditionally run only the relevant test categories.

.PARAMETER BaseBranch
    The target branch of the PR (e.g., "main", "release/8.0").

.PARAMETER MappingFile
    Path to the YAML mapping file that defines file pattern to category mappings.

.PARAMETER PRNumber
    Optional PR number. If provided, checks for bypass labels via GitHub API.

.PARAMETER GitHubToken
    Optional GitHub token for API access (checking labels).

.OUTPUTS
    Sets the following Azure DevOps pipeline variables:
    - DetectedCategories: Comma-separated list of categories to run
    - DetectedCategoryGroups: Comma-separated list of category groups to run
    - RunAllTests: "true" if all tests should run, "false" otherwise
    - DetectionReason: Human-readable explanation of why these tests were selected
    - SkippedCategoryGroups: Category groups that will be skipped

.EXAMPLE
    ./Detect-UITestCategories.ps1 -BaseBranch "main"

.EXAMPLE
    ./Detect-UITestCategories.ps1 -BaseBranch "main" -PRNumber 12345 -GitHubToken $env:GITHUB_TOKEN
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$BaseBranch = "main",

    [Parameter(Mandatory = $false)]
    [string]$MappingFile = "$PSScriptRoot/../pipelines/uitest-category-mapping.yml",

    [Parameter(Mandatory = $false)]
    [int]$PRNumber = 0,

    [Parameter(Mandatory = $false)]
    [string]$GitHubToken = "",

    [Parameter(Mandatory = $false)]
    [string]$Repository = "dotnet/maui"
)

$ErrorActionPreference = "Stop"

# =============================================================================
# Helper Functions
# =============================================================================

function Write-Banner {
    param([string]$Message)
    $line = "=" * 70
    Write-Host ""
    Write-Host $line -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Cyan
    Write-Host $line -ForegroundColor Cyan
    Write-Host ""
}

function Write-Section {
    param([string]$Message)
    Write-Host ""
    Write-Host "--- $Message ---" -ForegroundColor Yellow
}

function Set-PipelineVariable {
    param(
        [string]$Name,
        [string]$Value,
        [switch]$IsOutput
    )
    
    if ($env:TF_BUILD) {
        if ($IsOutput) {
            Write-Host "##vso[task.setvariable variable=$Name;isOutput=true]$Value"
        } else {
            Write-Host "##vso[task.setvariable variable=$Name]$Value"
        }
    }
    Write-Host "  $Name = $Value" -ForegroundColor Gray
}

function Test-FileMatchesPattern {
    param(
        [string]$FilePath,
        [string]$Pattern
    )
    
    # Normalize paths to forward slashes
    $normalizedFile = $FilePath -replace '\\', '/'
    $normalizedPattern = $Pattern -replace '\\', '/'
    
    # Convert glob pattern to regex
    # Order matters! Process more specific patterns first
    $regex = $normalizedPattern
    $regex = $regex -replace '\.', '\.'                    # Escape dots
    $regex = $regex -replace '\*\*/', '(.*?/)?'            # **/ matches zero or more path segments
    $regex = $regex -replace '/\*\*$', '/.*'               # /** at end matches everything after
    $regex = $regex -replace '\*\*', '.*'                  # ** alone matches anything
    $regex = $regex -replace '(?<!\.)(\*)', '[^/]*'        # * matches single segment (but not .*)
    $regex = "^$regex$"
    
    return $normalizedFile -match $regex
}

function Get-PRLabels {
    param(
        [int]$PRNumber,
        [string]$Repository,
        [string]$Token
    )
    
    if ($PRNumber -eq 0 -or [string]::IsNullOrEmpty($Token)) {
        return @()
    }
    
    try {
        $headers = @{
            "Authorization" = "Bearer $Token"
            "Accept" = "application/vnd.github.v3+json"
            "User-Agent" = "MAUI-UITest-Detection"
        }
        
        $url = "https://api.github.com/repos/$Repository/pulls/$PRNumber"
        $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
        
        return $response.labels | ForEach-Object { $_.name }
    }
    catch {
        Write-Warning "Failed to fetch PR labels: $_"
        return @()
    }
}

# =============================================================================
# Main Script
# =============================================================================

Write-Banner "UI Test Category Detection"

# Resolve mapping file path
if (-not [System.IO.Path]::IsPathRooted($MappingFile)) {
    $MappingFile = Join-Path $PSScriptRoot $MappingFile
}
$MappingFile = [System.IO.Path]::GetFullPath($MappingFile)

Write-Host "Configuration:" -ForegroundColor White
Write-Host "  Base Branch: $BaseBranch"
Write-Host "  Mapping File: $MappingFile"
Write-Host "  PR Number: $(if ($PRNumber -gt 0) { $PRNumber } else { 'N/A' })"

# Check if mapping file exists
if (-not (Test-Path $MappingFile)) {
    Write-Warning "Mapping file not found: $MappingFile"
    Write-Host "Falling back to running all tests."
    Set-PipelineVariable -Name "RunAllTests" -Value "true" -IsOutput
    Set-PipelineVariable -Name "DetectionReason" -Value "Mapping file not found" -IsOutput
    exit 0
}

# Install powershell-yaml if needed
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Write-Host "Installing powershell-yaml module..."
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser
}
Import-Module powershell-yaml

# Load mapping configuration
Write-Section "Loading Category Mapping"
$mappingContent = Get-Content $MappingFile -Raw
$mapping = ConvertFrom-Yaml $mappingContent

$bypassLabel = $mapping.bypassLabel
$maxFiles = $mapping.maxFilesBeforeFullRun
$categoryGroups = $mapping.categoryGroups
$fileMappings = $mapping.mappings

Write-Host "  Bypass Label: $bypassLabel"
Write-Host "  Max Files Threshold: $maxFiles"
Write-Host "  Category Groups: $($categoryGroups.Count)"
Write-Host "  Mapping Rules: $($fileMappings.Count)"

# Check for bypass label
Write-Section "Checking Bypass Conditions"

if ($PRNumber -gt 0 -and -not [string]::IsNullOrEmpty($GitHubToken)) {
    $labels = Get-PRLabels -PRNumber $PRNumber -Repository $Repository -Token $GitHubToken
    Write-Host "  PR Labels: $($labels -join ', ')"
    
    if ($labels -contains $bypassLabel) {
        Write-Host "  ⚠️  Bypass label '$bypassLabel' detected!" -ForegroundColor Yellow
        Set-PipelineVariable -Name "RunAllTests" -Value "true" -IsOutput
        Set-PipelineVariable -Name "DetectionReason" -Value "Bypass label '$bypassLabel' present on PR" -IsOutput
        Set-PipelineVariable -Name "DetectedCategoryGroups" -Value ($categoryGroups -join ',') -IsOutput
        exit 0
    }
}

# Get changed files
Write-Section "Detecting Changed Files"

# Fetch the base branch to ensure we have it
Write-Host "  Fetching origin/$BaseBranch..."
$fetchResult = git fetch origin $BaseBranch 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to fetch base branch: $fetchResult"
}

# Get the merge base
$mergeBase = git merge-base "origin/$BaseBranch" HEAD 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to find merge base, using origin/$BaseBranch directly"
    $mergeBase = "origin/$BaseBranch"
}

# Get changed files
$changedFiles = git diff --name-only $mergeBase HEAD 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to get changed files: $changedFiles"
    Write-Host "Falling back to running all tests."
    Set-PipelineVariable -Name "RunAllTests" -Value "true" -IsOutput
    Set-PipelineVariable -Name "DetectionReason" -Value "Failed to detect changed files" -IsOutput
    Set-PipelineVariable -Name "DetectedCategoryGroups" -Value ($categoryGroups -join ',') -IsOutput
    exit 0
}

$changedFiles = $changedFiles -split "`n" | Where-Object { $_ -ne "" }
$fileCount = $changedFiles.Count

Write-Host "  Changed files: $fileCount"

if ($fileCount -eq 0) {
    Write-Host "  No changed files detected."
    Set-PipelineVariable -Name "RunAllTests" -Value "false" -IsOutput
    Set-PipelineVariable -Name "DetectionReason" -Value "No changed files detected" -IsOutput
    Set-PipelineVariable -Name "DetectedCategories" -Value "" -IsOutput
    Set-PipelineVariable -Name "DetectedCategoryGroups" -Value "" -IsOutput
    exit 0
}

# Check file count threshold
if ($fileCount -gt $maxFiles) {
    Write-Host "  ⚠️  File count ($fileCount) exceeds threshold ($maxFiles)" -ForegroundColor Yellow
    Set-PipelineVariable -Name "RunAllTests" -Value "true" -IsOutput
    Set-PipelineVariable -Name "DetectionReason" -Value "Changed file count ($fileCount) exceeds threshold ($maxFiles)" -IsOutput
    Set-PipelineVariable -Name "DetectedCategoryGroups" -Value ($categoryGroups -join ',') -IsOutput
    exit 0
}

# Show changed files (limit to first 20 for readability)
$displayFiles = $changedFiles | Select-Object -First 20
foreach ($file in $displayFiles) {
    Write-Host "    - $file" -ForegroundColor Gray
}
if ($fileCount -gt 20) {
    Write-Host "    ... and $($fileCount - 20) more files" -ForegroundColor Gray
}

# Match files to categories
Write-Section "Matching Files to Categories"

$detectedCategories = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$runAllTests = $false
$matchedFiles = @{}

foreach ($file in $changedFiles) {
    $matched = $false
    
    foreach ($rule in $fileMappings) {
        foreach ($pattern in $rule.patterns) {
            if (Test-FileMatchesPattern -FilePath $file -Pattern $pattern) {
                $matched = $true
                $categories = $rule.categories
                
                foreach ($category in $categories) {
                    if ($category -eq "*") {
                        $runAllTests = $true
                        Write-Host "  ⚠️  File '$file' matches catch-all pattern '$pattern'" -ForegroundColor Yellow
                    } else {
                        [void]$detectedCategories.Add($category)
                        if (-not $matchedFiles.ContainsKey($file)) {
                            $matchedFiles[$file] = @()
                        }
                        $matchedFiles[$file] += $category
                    }
                }
                
                break  # Stop checking patterns for this rule
            }
        }
        
        if ($matched -and $runAllTests) {
            break  # Stop checking rules if we're running all tests
        }
    }
    
    if (-not $matched) {
        Write-Host "  ℹ️  No mapping for: $file" -ForegroundColor Gray
    }
}

# Handle run-all-tests case
if ($runAllTests) {
    Write-Host ""
    Write-Host "  Result: Running ALL tests (core/infrastructure change detected)" -ForegroundColor Yellow
    Set-PipelineVariable -Name "RunAllTests" -Value "true" -IsOutput
    Set-PipelineVariable -Name "DetectionReason" -Value "Core or infrastructure files changed - running all tests" -IsOutput
    Set-PipelineVariable -Name "DetectedCategoryGroups" -Value ($categoryGroups -join ',') -IsOutput
    exit 0
}

# If no categories detected but files changed, run all tests (safety net)
if ($detectedCategories.Count -eq 0 -and $fileCount -gt 0) {
    Write-Host ""
    Write-Host "  ⚠️  No categories matched but files were changed - running all tests" -ForegroundColor Yellow
    Set-PipelineVariable -Name "RunAllTests" -Value "true" -IsOutput
    Set-PipelineVariable -Name "DetectionReason" -Value "No category mappings matched changed files - running all tests for safety" -IsOutput
    Set-PipelineVariable -Name "DetectedCategoryGroups" -Value ($categoryGroups -join ',') -IsOutput
    exit 0
}

# Determine which category groups to run
Write-Section "Determining Category Groups"

$detectedCategoryGroups = @()
$skippedCategoryGroups = @()

foreach ($group in $categoryGroups) {
    $groupCategories = $group -split ','
    $shouldRun = $false
    
    foreach ($cat in $groupCategories) {
        if ($detectedCategories.Contains($cat.Trim())) {
            $shouldRun = $true
            break
        }
    }
    
    if ($shouldRun) {
        $detectedCategoryGroups += $group
    } else {
        $skippedCategoryGroups += $group
    }
}

# Output results
Write-Section "Detection Results"

$categoriesList = ($detectedCategories | Sort-Object) -join ', '
$categoryGroupsList = $detectedCategoryGroups -join ';'
$skippedGroupsList = $skippedCategoryGroups -join ';'

Write-Host ""
Write-Host "  Detected Categories ($($detectedCategories.Count)):" -ForegroundColor Green
foreach ($cat in ($detectedCategories | Sort-Object)) {
    Write-Host "    ✓ $cat" -ForegroundColor Green
}

Write-Host ""
Write-Host "  Category Groups to Run ($($detectedCategoryGroups.Count) of $($categoryGroups.Count)):" -ForegroundColor Green
foreach ($group in $detectedCategoryGroups) {
    Write-Host "    ✓ $group" -ForegroundColor Green
}

Write-Host ""
Write-Host "  Category Groups to Skip ($($skippedCategoryGroups.Count)):" -ForegroundColor Gray
foreach ($group in $skippedCategoryGroups) {
    Write-Host "    ✗ $group" -ForegroundColor Gray
}

# Calculate savings
$totalJobs = $categoryGroups.Count * 4  # Approximate: groups × platforms
$runningJobs = $detectedCategoryGroups.Count * 4
$savedJobs = $totalJobs - $runningJobs
$savingsPercent = [math]::Round(($savedJobs / $totalJobs) * 100)

Write-Host ""
Write-Host "  📊 Estimated Savings:" -ForegroundColor Cyan
Write-Host "     Running: ~$runningJobs jobs (was ~$totalJobs)"
Write-Host "     Saving: ~$savedJobs jobs ($savingsPercent% reduction)"

# Set pipeline variables
Write-Section "Setting Pipeline Variables"

Set-PipelineVariable -Name "RunAllTests" -Value "false" -IsOutput
Set-PipelineVariable -Name "DetectedCategories" -Value $categoriesList -IsOutput
Set-PipelineVariable -Name "DetectedCategoryGroups" -Value $categoryGroupsList -IsOutput
Set-PipelineVariable -Name "SkippedCategoryGroups" -Value $skippedGroupsList -IsOutput
Set-PipelineVariable -Name "DetectionReason" -Value "Detected $($detectedCategories.Count) categories from $fileCount changed files" -IsOutput
Set-PipelineVariable -Name "DetectedCategoryCount" -Value $detectedCategories.Count -IsOutput
Set-PipelineVariable -Name "DetectedGroupCount" -Value $detectedCategoryGroups.Count -IsOutput

Write-Banner "Detection Complete"

Write-Host "Summary:" -ForegroundColor White
Write-Host "  Changed Files: $fileCount"
Write-Host "  Detected Categories: $($detectedCategories.Count)"
Write-Host "  Category Groups to Run: $($detectedCategoryGroups.Count) of $($categoryGroups.Count)"
Write-Host "  Estimated Job Reduction: $savingsPercent%"
Write-Host ""
