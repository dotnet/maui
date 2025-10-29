#!/usr/bin/env pwsh
# Cleanup script to fix issues from the first pass

$ErrorActionPreference = "Continue"
$filesFixed = 0

Write-Host "Cleaning up duplicate attributes and modifiers..." -ForegroundColor Cyan

$files = Get-ChildItem -Path "." -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Stop
        if ($null -eq $content) { continue }
        
        $originalContent = $content
    $changed = $false
        
        # Fix 1: Remove duplicate [Theory] attributes
    if ($content -match '\[Theory\]\s+\[Theory\]') {
            $content = $content -replace '(\[Theory\]\s+)+\[Theory\]', '[Theory]'
            $changed = $true
        }
  
        # Fix 2: Remove duplicate public modifiers
        if ($content -match 'public public class') {
            $content = $content -replace 'public public class', 'public class'
        $changed = $true
        }
        
    # Fix 3: Fix Is.InstanceOf assertions (NUnit to xUnit)
        if ($content -match 'Is\.InstanceOf') {
    $content = $content -replace 'Assert\.True\(([^,]+),\s*Is\.InstanceOf<([^>]+)>\(\)\);?', 'Assert.IsType<$2>($1);'
            $changed = $true
        }
     
        # Fix 4: Remove duplicate [Theory] that appears right before [InlineData]
        $lines = $content -split "`r?`n"
    $newLines = @()
    for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            
            # Skip [Theory] if the next line is also [Theory] or [InlineData]
       if ($line -match '^\s*\[Theory\]\s*$' -and $i+1 -lt $lines.Count) {
        $nextLine = $lines[$i+1]
      if ($nextLine -match '^\s*\[Theory\]\s*$' -or $nextLine -match '^\s*\[InlineData') {
  continue
       }
    }
    
            $newLines += $line
        }
        
        if ($newLines.Count -gt 0 -and ($newLines.Count -ne $lines.Count)) {
        $content = $newLines -join "`n"
     $changed = $true
        }
        
        # Only write if content changed
     if ($changed -and $content -ne $originalContent) {
            Set-Content -Path $file.FullName -Value $content -NoNewline -ErrorAction Stop
            $filesFixed++
      Write-Host "Fixed: $($file.FullName)" -ForegroundColor Green
     }
    }
    catch {
        Write-Host "Error processing $($file.FullName): $_" -ForegroundColor Yellow
    }
}

Write-Host "`nCompleted! Fixed $filesFixed files." -ForegroundColor Cyan
