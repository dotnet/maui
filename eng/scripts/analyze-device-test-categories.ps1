#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Analyzes PR changes to determine which Device Test categories to run

.DESCRIPTION
    Reads the changed files from analyze-pr-changes.ps1 output and maps them
    to device test categories based on the TestCategory constants.

.PARAMETER InputFile
    Path to file containing test categories from PR analysis

.PARAMETER OutputFile
    Path to output file for device test categories

.EXAMPLE
    ./analyze-device-test-categories.ps1
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$InputFile = "test-categories.txt",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFile = "device-test-categories.txt"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Device Test Category Mapping ===" -ForegroundColor Cyan

# Read UI test categories
if (-not (Test-Path $InputFile)) {
    Write-Error "Input file not found: $InputFile. Run analyze-pr-changes.ps1 first."
    exit 1
}

$uiCategories = @(Get-Content $InputFile | Where-Object { $_.Trim() -ne "" })

if ($uiCategories.Count -eq 0) {
    Write-Host "No UI test categories found - no device tests needed" -ForegroundColor Yellow
    "" | Out-File -FilePath $OutputFile -Encoding UTF8
    exit 0
}

Write-Host "UI Test Categories:" -ForegroundColor Cyan
$uiCategories | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }

# Device test category mapping
# Based on src/Core/tests/DeviceTests/TestCategory.cs and src/Controls/tests/DeviceTests/TestCategory.cs
$deviceTestCategories = @{
    # Direct mappings (same name in both UI and Device tests)
    "ActivityIndicator" = "ActivityIndicator"
    "Border" = "Border"
    "Button" = "Button"
    "CheckBox" = "CheckBox"
    "CarouselView" = "CarouselView"
    "CollectionView" = "CollectionView"
    "DatePicker" = "DatePicker"
    "Dispatcher" = "Dispatcher"
    "Editor" = "Editor"
    "Entry" = "Entry"
    "Frame" = "Frame"
    "Image" = "Image"
    "ImageButton" = "ImageButton"
    "Label" = "Label"
    "Layout" = "Layout"
    "ListView" = "ListView"
    "NavigationPage" = "NavigationPage"
    "Page" = "Page"
    "Picker" = "Picker"
    "ProgressBar" = "ProgressBar"
    "RadioButton" = "RadioButton"
    "RefreshView" = "RefreshView"
    "ScrollView" = "ScrollView"
    "SearchBar" = "SearchBar"
    "Shell" = "Shell"
    "Slider" = "Slider"
    "Stepper" = "Stepper"
    "SwipeView" = "SwipeView"
    "Switch" = "Switch"
    "TabbedPage" = "TabbedPage"
    "WebView" = "WebView"
    "Window" = "Window"
    
    # UI test categories that map to device test categories
    "BoxView" = "BoxView"
    "ContentView" = "ContentView"
    "Element" = "Element"
    "Fonts" = "Fonts"
    "Graphics" = "Graphics"
    "GraphicsView" = "GraphicsView"
    "IndicatorView" = "IndicatorView"
    "Shape" = "Shape"
    "ShapeView" = "ShapeView"
    "View" = "View"
    "VisualElement" = "VisualElement"
    "FlyoutPage" = "FlyoutPage"
    "Lifecycle" = "Lifecycle"
    "Accessibility" = "Accessibility"
    "WindowOverlay" = "WindowOverlay"
    
    # UI test categories without direct device test equivalent (still test in device tests as View/VisualElement)
    "Animation" = "View"
    "Brush" = "View"
    "Gestures" = "View"
    "InputTransparent" = "View"
    "IsEnabled" = "View"
    "IsVisible" = "View"
    "Shadow" = "View"
    "ViewBaseTests" = "View"
    "Visual" = "VisualElement"
}

# Parse categories and map to device test categories
$mappedCategories = @{}

foreach ($categoryGroup in $uiCategories) {
    $categories = $categoryGroup.Split(",") | ForEach-Object { $_.Trim() }
    
    foreach ($category in $categories) {
        if ($deviceTestCategories.ContainsKey($category)) {
            $deviceCategory = $deviceTestCategories[$category]
            $mappedCategories[$deviceCategory] = $true
            Write-Host "  Mapped: $category -> $deviceCategory" -ForegroundColor Green
        }
        else {
            Write-Host "  No device test mapping for: $category" -ForegroundColor Yellow
        }
    }
}

# If no device categories mapped, skip device tests
if ($mappedCategories.Count -eq 0) {
    Write-Host "`nNo device test categories mapped - skipping device tests" -ForegroundColor Yellow
    "" | Out-File -FilePath $OutputFile -Encoding UTF8
    exit 0
}

# Output device test categories (one per line for simplicity)
$deviceCategories = $mappedCategories.Keys | Sort-Object
Write-Host "`nDevice Test Categories to Run:" -ForegroundColor Cyan
$deviceCategories | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }

$deviceCategories | Out-File -FilePath $OutputFile -Encoding UTF8

Write-Host "`nDevice test categories written to: $OutputFile" -ForegroundColor Green

# Set Azure DevOps variable if in pipeline
if ($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI) {
    $categoryList = $deviceCategories -join ","
    Write-Host "##vso[task.setVariable variable=DeviceTestCategories;isOutput=true]$categoryList"
    Write-Host "##vso[task.setVariable variable=ShouldRunDeviceTests;isOutput=true]true"
}

Write-Host "`n=== Device Test Category Mapping Complete ===" -ForegroundColor Cyan
exit 0
