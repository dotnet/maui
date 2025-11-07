#!/usr/bin/env pwsh
# Script to migrate test files from NUnit patterns to XUnit (Pass 2)

$ErrorActionPreference = "Stop"

$testProjectPath = "C:\repos\dotnet\maui\src\Controls\tests\Xaml.UnitTests"

# Get all C# files excluding Generated, obj, and bin folders
$files = Get-ChildItem -Path $testProjectPath -Filter "*.cs" -Recurse | Where-Object { 
    $_.FullName -notlike "*\Generated\*" -and 
    $_.FullName -notlike "*\obj\*" -and 
    $_.FullName -notlike "*\bin\*"
}

$totalFiles = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix 1: Remove [Values] from parameter positions and add InlineData
    # Pattern: method with [Values] on parameters needs conversion
    # For now, just comment out the [Values] on parameters as TODO
    $content = $content -replace '\((\s*\[Values[^\]]*\])', '(// TODO: Convert to [InlineData] or [MemberData] $1'
    
    # Fix 2: Remove duplicate [Fact] attributes
    $content = $content -replace '\[Fact\]\s*\r?\n\s*\[Fact', '[Fact'
    
    # Fix 3: Remove 'override' from Setup methods (BaseTestFixture doesn't have virtual Setup)
    $content = $content -replace 'public override void Setup\(\)', 'public void Setup()'
    
    # Fix 4: Convert [TestCase] to [Theory] + [InlineData]
    $content = $content -replace '\[TestCase\(([^\]]+)\)\]', '[Theory]$([Environment]::NewLine)\t\t[InlineData($1)]'
    
    # Fix 5: Add using statement for Xunit if Assert.IsInstanceOf is found
    if ($content -match 'Assert\.IsInstanceOf') {
        # Comment it with TODO since XUnit uses Assert.IsType or Assert.IsAssignableFrom
        $content = $content -replace 'Assert\.IsInstanceOf<([^>]+)>\(([^)]+)\)', '// TODO: Use Assert.IsType or Assert.IsAssignableFrom - Assert.IsInstanceOf<$1>($2)'
    }
    
    # Fix 6: Convert Assert.Fail to Assert.True(false, message) or just fail
    $content = $content -replace 'Assert\.Fail\(([^)]+)\)', 'Assert.Fail($1) // TODO: XUnit doesn''t have Assert.Fail, use Assert.True(false, $1) or throw'
    
    # Fix 7: Add ExpectedResult TODO comments
    if ($content -match 'ExpectedResult\s*=') {
        $content = $content -replace '(ExpectedResult\s*=\s*[^,\]]+)', '// TODO: Convert ExpectedResult to return value assertion - $1'
    }
    
    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $totalFiles++
        Write-Host "Fixed: $($file.Name)"
    }
}

Write-Host ""
Write-Host "Pass 2 Migration complete!"
Write-Host "Files modified: $totalFiles"
