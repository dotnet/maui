#!/usr/bin/env pwsh
#
# Script to fix common xUnit migration issues in Xaml.UnitTests

param(
    [string]$Path = "."
)

$files = Get-ChildItem -Path $Path -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix 1: Make test classes public
    $content = $content -replace '(\s+)class Tests(\s+)\{', '$1public class Tests$2{'
    
  # Fix 2: Add [Theory] attribute to methods with [InlineData]
    $content = $content -replace '(\s+)\[InlineData([^\]]+)\]\s+public void (\w+)', '$1[Theory]$1[InlineData$2]$1public void $3'
    
    # Fix 3: Fix Assert.Pass() - remove it (xUnit doesn't have Assert.Pass)
    $content = $content -replace 'Assert\.Pass\(\);?', '// Test passed'
    
    # Fix 4: Fix Assume.That - convert to Skip
    $content = $content -replace 'Assume\.That\(([^,]+),\s*Is\.EqualTo\(([^\)]+)\)\);?', 'if ($1 != $2) { Assert.Skip("Assumption failed"); }'
    
    # Fix 5: Fix Is.TypeOf assertions
    $content = $content -replace 'Assert\.True\(([^,]+),\s*Is\.TypeOf<([^>]+)>\(\)\);?', 'Assert.IsType<$2>($1);'
    
    # Fix 6: Fix Assert.Equal with boolean - use Assert.True/False
    $content = $content -replace 'Assert\.Equal\(true,\s*([^\)]+)\);?', 'Assert.True($1);'
    $content = $content -replace 'Assert\.Equal\(false,\s*([^\)]+)\);?', 'Assert.False($1);'
    
    # Fix 7: Fix collection count assertions
    $content = $content -replace 'Assert\.Equal\(0,\s*([^\)]+)\.Count\);?', 'Assert.Empty($1);'
    $content = $content -replace 'Assert\.Equal\(1,\s*([^\)]+)\.Count\);?', 'Assert.Single($1);'
    
    # Fix 8: Add test data for Theory methods without it
    if ($content -match '(\s+)\[Theory\]\s+public void (\w+)\(bool useCompiledXaml\)') {
        if ($content -notmatch '\[InlineData\([^\)]*\)\]\s+public void ' + $matches[2]) {
    $content = $content -replace '(\s+)\[Theory\]\s+(public void ' + [regex]::Escape($matches[2]) + ')', '$1[Theory]$1[InlineData(false)]$1[InlineData(true)]$1$2'
        }
    }
    
    # Only write if content changed
    if ($content -ne $originalContent) {
    Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed: $($file.FullName)"
    }
}

Write-Host "Done!"
