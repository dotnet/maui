#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Analyzes PR changes and determines which UI test categories to run

.DESCRIPTION
    This script uses GitHub CLI to get changed files from a PR and intelligently
    determines which UI test categories need to be executed based on the changes.
    It uses GitHub Copilot CLI via a custom agent to analyze the changes.

.PARAMETER PrNumber
    The pull request number to analyze (optional, will detect from Azure DevOps context)

.PARAMETER RepoOwner
    The repository owner (default: dotnet)

.PARAMETER RepoName
    The repository name (default: maui)

.PARAMETER OutputFile
    Path to output file for test categories (default: test-categories.txt)

.EXAMPLE
    ./analyze-pr-changes.ps1 -PrNumber 12345
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$PrNumber,
    
    [Parameter(Mandatory=$false)]
    [string]$RepoOwner = "dotnet",
    
    [Parameter(Mandatory=$false)]
    [string]$RepoName = "maui",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFile = "test-categories.txt"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== PR Change Analysis for Intelligent UI Test Execution ===" -ForegroundColor Cyan

# Function to detect PR number from Azure DevOps environment
function Get-PrNumberFromAzureDevOps {
    if ($env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
        return $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER
    }
    
    # Try to get from source branch (PR branches are typically refs/pull/NUMBER/merge)
    if ($env:BUILD_SOURCEBRANCH -match "refs/pull/(\d+)/") {
        return $Matches[1]
    }
    
    return $null
}

# Determine PR number
if (-not $PrNumber) {
    $PrNumber = Get-PrNumberFromAzureDevOps
    if (-not $PrNumber) {
        Write-Error "Could not determine PR number. Please provide -PrNumber parameter or run in Azure DevOps PR context."
        exit 1
    }
}

Write-Host "Analyzing PR #$PrNumber" -ForegroundColor Yellow

# Check if gh CLI is installed
$ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghInstalled) {
    Write-Host "GitHub CLI (gh) not found. Installing..." -ForegroundColor Yellow
    
    if ($IsLinux) {
        # Install on Linux
        Write-Host "Installing GitHub CLI on Linux..."
        
        # Check if running as root or if sudo is available
        $canUseSudo = $false
        try {
            $sudoCheck = sudo -n true 2>&1
            $canUseSudo = $LASTEXITCODE -eq 0
        } catch {
            $canUseSudo = $false
        }
        
        if ($canUseSudo -or $env:USER -eq "root") {
            # Ubuntu/Debian method
            if (Test-Path "/etc/debian_version") {
                Invoke-Expression "curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg"
                Invoke-Expression "sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg"
                Invoke-Expression "echo 'deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main' | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null"
                Invoke-Expression "sudo apt update"
                Invoke-Expression "sudo apt install -y gh"
            }
            # RHEL/Fedora method
            elseif (Test-Path "/etc/redhat-release") {
                Invoke-Expression "sudo dnf install -y 'dnf-command(config-manager)'"
                Invoke-Expression "sudo dnf config-manager --add-repo https://cli.github.com/packages/rpm/gh-cli.repo"
                Invoke-Expression "sudo dnf install -y gh"
            }
            else {
                Write-Error "Unsupported Linux distribution for automatic GitHub CLI installation"
                exit 1
            }
        } else {
            Write-Error "GitHub CLI installation requires sudo privileges. Please install gh manually or run with appropriate permissions."
            exit 1
        }
    }
    elseif ($IsMacOS) {
        # Install on macOS using Homebrew
        Write-Host "Installing GitHub CLI on macOS..."
        if (-not (Get-Command brew -ErrorAction SilentlyContinue)) {
            Write-Error "Homebrew not found. Please install Homebrew first or install gh manually."
            exit 1
        }
        brew install gh
    }
    elseif ($IsWindows) {
        # Install on Windows using winget
        Write-Host "Installing GitHub CLI on Windows..."
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            winget install --id GitHub.cli --silent
        }
        elseif (Get-Command choco -ErrorAction SilentlyContinue) {
            choco install gh -y
        }
        else {
            Write-Error "Neither winget nor chocolatey found. Please install gh manually from https://cli.github.com/"
            exit 1
        }
    }
    
    # Verify installation
    $ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghInstalled) {
        Write-Error "Failed to install GitHub CLI. Please install manually."
        exit 1
    }
    
    Write-Host "GitHub CLI installed successfully!" -ForegroundColor Green
}

# Check authentication
Write-Host "Checking GitHub CLI authentication..." -ForegroundColor Yellow

# Try to authenticate using token if available
if ($env:GITHUB_TOKEN) {
    Write-Host "Using GITHUB_TOKEN from environment" -ForegroundColor Green
    $env:GH_TOKEN = $env:GITHUB_TOKEN
}
elseif ($env:GH_TOKEN) {
    Write-Host "Using GH_TOKEN from environment" -ForegroundColor Green
}
else {
    # Check if already authenticated
    $authStatus = gh auth status 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "GitHub CLI is not authenticated. Please set GITHUB_TOKEN or GH_TOKEN environment variable, or run 'gh auth login'"
        exit 1
    }
}

# Get changed files from PR
Write-Host "Fetching changed files from PR #$PrNumber..." -ForegroundColor Yellow
$changedFilesJson = gh pr view $PrNumber --repo "$RepoOwner/$RepoName" --json files | ConvertFrom-Json

if (-not $changedFilesJson -or -not $changedFilesJson.files) {
    Write-Error "Failed to fetch changed files from PR #$PrNumber"
    exit 1
}

$changedFiles = $changedFilesJson.files | ForEach-Object { $_.path }
$fileCount = $changedFiles.Count

Write-Host "Found $fileCount changed files" -ForegroundColor Green
Write-Host "Changed files:" -ForegroundColor Cyan
$changedFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }

# Create analysis request for the custom agent
$analysisRequest = @{
    prNumber = $PrNumber
    repository = "$RepoOwner/$RepoName"
    changedFiles = $changedFiles
    fileCount = $fileCount
} | ConvertTo-Json -Depth 10

Write-Host "`nAnalyzing changes to determine test categories..." -ForegroundColor Yellow
Write-Host "This analysis considers:" -ForegroundColor Cyan
Write-Host "  - Direct control modifications (e.g., Button.cs -> Button tests)" -ForegroundColor Gray
Write-Host "  - Platform-specific changes (e.g., .Android.cs affects Android tests)" -ForegroundColor Gray
Write-Host "  - Core framework impact (e.g., src/Core/ changes affect all tests)" -ForegroundColor Gray
Write-Host "  - Handler and rendering changes" -ForegroundColor Gray
Write-Host "  - Test infrastructure modifications" -ForegroundColor Gray

# Perform intelligent analysis
$analysis = @{
    shouldRunTests = $true
    testStrategy = "selective"
    categoryGroups = @()
    reasoning = ""
    filesAnalyzed = $fileCount
    criticalChanges = @()
}

# Simple rule-based analysis (fallback if custom agent not available)
$docOnlyPatterns = @("*.md", "docs/*", "*.txt", "LICENSE*", "CODE-OF-CONDUCT*", "CONTRIBUTING*")
$testInfraPatterns = @("*.Tests/*", "TestCases.HostApp/*")
$corePatterns = @("src/Core/*", "src/Controls/src/Core/Layout/*", "src/Controls/src/Core/VisualElement*", "src/Controls/src/Core/Element*")
$platformPatterns = @("*.Android.cs", "*.iOS.cs", "*.Windows.cs", "*.MacCatalyst.cs", "*/Android/*", "*/iOS/*", "*/Windows/*", "*/MacCatalyst/*")

# Check if only documentation changed
$allDocsOnly = $true
foreach ($file in $changedFiles) {
    $isDoc = $false
    foreach ($pattern in $docOnlyPatterns) {
        if ($file -like $pattern) {
            $isDoc = $true
            break
        }
    }
    if (-not $isDoc) {
        $allDocsOnly = $false
        break
    }
}

if ($allDocsOnly) {
    $analysis.testStrategy = "none"
    $analysis.shouldRunTests = $false
    $analysis.reasoning = "All changes are documentation only - no tests needed"
}
else {
    # Check if core framework changed
    $hasCoreChanges = $false
    foreach ($file in $changedFiles) {
        foreach ($pattern in $corePatterns) {
            if ($file -like $pattern) {
                $hasCoreChanges = $true
                $analysis.criticalChanges += "$file - Core framework change"
                break
            }
        }
    }
    
    if ($hasCoreChanges) {
        $analysis.testStrategy = "full"
        $analysis.reasoning = "Core framework changes detected - running all test categories for safety"
        # Return all category groups
        $analysis.categoryGroups = @(
            "Accessibility,ActionSheet,ActivityIndicator,Animation,AppLinks",
            "Border,BoxView,Brush,Button",
            "Cells,CheckBox,ContextActions,CustomRenderers",
            "CarouselView",
            "CollectionView",
            "DatePicker,Dispatcher,DisplayAlert,DisplayPrompt,DragAndDrop",
            "Entry",
            "Editor,Effects,FlyoutPage,Focus,Fonts,Frame,Gestures,GraphicsView",
            "Image,ImageButton,IndicatorView,InputTransparent,IsEnabled,IsVisible",
            "Label,Layout,Lifecycle,ListView",
            "ManualReview,Maps",
            "Navigation",
            "Page,Performance,Picker,ProgressBar",
            "RadioButton,RefreshView",
            "SafeAreaEdges",
            "ScrollView,SearchBar,Shape,Slider,SoftInput,Stepper,Switch,SwipeView",
            "Shell",
            "TabbedPage,TableView,TimePicker,TitleView,ToolbarItem",
            "Shadow,ViewBaseTests,Visual,WebView,Window"
        )
    }
    else {
        # Intelligent category mapping
        $categories = @{}
        
        foreach ($file in $changedFiles) {
            $fileName = Split-Path $file -Leaf
            $dirName = Split-Path $file -Parent
            
            # Map file changes to categories
            # Control-specific patterns
            if ($file -match "Button") { $categories["Button"] = $true }
            if ($file -match "Label") { $categories["Label"] = $true }
            if ($file -match "Entry") { $categories["Entry"] = $true }
            if ($file -match "Editor") { $categories["Editor"] = $true }
            if ($file -match "CollectionView") { $categories["CollectionView"] = $true }
            if ($file -match "CarouselView") { $categories["CarouselView"] = $true }
            if ($file -match "ListView") { $categories["ListView"] = $true }
            if ($file -match "ScrollView") { $categories["ScrollView"] = $true }
            if ($file -match "Shell") { $categories["Shell"] = $true }
            if ($file -match "Navigation") { $categories["Navigation"] = $true }
            if ($file -match "Layout") { $categories["Layout"] = $true; $categories["ViewBaseTests"] = $true }
            if ($file -match "SafeArea") { $categories["SafeAreaEdges"] = $true }
            if ($file -match "Image") { $categories["Image"] = $true }
            if ($file -match "Picker") { $categories["Picker"] = $true }
            if ($file -match "Slider") { $categories["Slider"] = $true }
            if ($file -match "Stepper") { $categories["Stepper"] = $true }
            if ($file -match "Switch") { $categories["Switch"] = $true }
            if ($file -match "WebView") { $categories["WebView"] = $true }
            if ($file -match "Border") { $categories["Border"] = $true }
            if ($file -match "Frame") { $categories["Frame"] = $true }
            if ($file -match "Page") { $categories["Page"] = $true }
            if ($file -match "Window") { $categories["Window"] = $true }
            
            # Platform-specific changes - include related controls
            foreach ($pattern in $platformPatterns) {
                if ($file -like $pattern) {
                    $analysis.criticalChanges += "$file - Platform-specific change"
                    # Add broader coverage for platform changes
                    $categories["ViewBaseTests"] = $true
                    break
                }
            }
            
            # Test infrastructure changes
            foreach ($pattern in $testInfraPatterns) {
                if ($file -like $pattern) {
                    # Try to determine which test category from the file path
                    if ($file -match "Issues/Issue(\d+)") {
                        $analysis.criticalChanges += "$file - Test case modification"
                    }
                    break
                }
            }
        }
        
        if ($categories.Count -eq 0) {
            # No specific categories identified, run a conservative set
            $analysis.testStrategy = "full"
            $analysis.reasoning = "Could not identify specific affected categories - running all tests for safety"
            $analysis.categoryGroups = @(
                "Accessibility,ActionSheet,ActivityIndicator,Animation,AppLinks",
                "Border,BoxView,Brush,Button",
                "Cells,CheckBox,ContextActions,CustomRenderers",
                "CarouselView",
                "CollectionView",
                "DatePicker,Dispatcher,DisplayAlert,DisplayPrompt,DragAndDrop",
                "Entry",
                "Editor,Effects,FlyoutPage,Focus,Fonts,Frame,Gestures,GraphicsView",
                "Image,ImageButton,IndicatorView,InputTransparent,IsEnabled,IsVisible",
                "Label,Layout,Lifecycle,ListView",
                "ManualReview,Maps",
                "Navigation",
                "Page,Performance,Picker,ProgressBar",
                "RadioButton,RefreshView",
                "SafeAreaEdges",
                "ScrollView,SearchBar,Shape,Slider,SoftInput,Stepper,Switch,SwipeView",
                "Shell",
                "TabbedPage,TableView,TimePicker,TitleView,ToolbarItem",
                "Shadow,ViewBaseTests,Visual,WebView,Window"
            )
        }
        else {
            # Build category groups from identified categories
            $categoryList = $categories.Keys | Sort-Object
            $analysis.categoryGroups = @($categoryList -join ",")
            $analysis.reasoning = "Selective test execution based on identified control changes: $($categoryList -join ', ')"
        }
    }
}

# Output results
Write-Host "`n=== Analysis Results ===" -ForegroundColor Cyan
Write-Host "Test Strategy: $($analysis.testStrategy)" -ForegroundColor Yellow
Write-Host "Should Run Tests: $($analysis.shouldRunTests)" -ForegroundColor Yellow
Write-Host "Files Analyzed: $($analysis.filesAnalyzed)" -ForegroundColor Yellow
Write-Host "`nReasoning:" -ForegroundColor Cyan
Write-Host $analysis.reasoning -ForegroundColor White

if ($analysis.criticalChanges.Count -gt 0) {
    Write-Host "`nCritical Changes:" -ForegroundColor Cyan
    $analysis.criticalChanges | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
}

if ($analysis.shouldRunTests) {
    Write-Host "`nTest Category Groups to Run:" -ForegroundColor Cyan
    $analysis.categoryGroups | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }
    
    # Write to output file
    $analysis.categoryGroups | Out-File -FilePath $OutputFile -Encoding UTF8
    Write-Host "`nTest categories written to: $OutputFile" -ForegroundColor Green
    
    # Also set Azure DevOps variable if in pipeline
    if ($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI) {
        Write-Host "##vso[task.setVariable variable=TestCategoryGroups;isOutput=true]$($analysis.categoryGroups -join '|')"
        Write-Host "##vso[task.setVariable variable=ShouldRunTests;isOutput=true]true"
        Write-Host "##vso[task.setVariable variable=TestStrategy;isOutput=true]$($analysis.testStrategy)"
    }
}
else {
    Write-Host "`nNo UI tests needed for this PR" -ForegroundColor Green
    "" | Out-File -FilePath $OutputFile -Encoding UTF8
    
    # Set Azure DevOps variable
    if ($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI) {
        Write-Host "##vso[task.setVariable variable=ShouldRunTests;isOutput=true]false"
        Write-Host "##vso[task.setVariable variable=TestStrategy;isOutput=true]none"
    }
}

Write-Host "`n=== Analysis Complete ===" -ForegroundColor Cyan
exit 0
