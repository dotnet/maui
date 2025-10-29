#!/usr/bin/env pwsh
# Final comprehensive fix for all remaining xUnit issues

$ErrorActionPreference = "Continue"
$filesFixed = 0

Write-Host "Fixing all remaining xUnit issues..." -ForegroundColor Cyan

$targetPath = "src\Controls\tests\Xaml.UnitTests"
$files = Get-ChildItem -Path $targetPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\bin\*" }

foreach ($file in $files) {
    try {
      $content = Get-Content $file.FullName -Raw -ErrorAction Stop
    if ($null -eq $content) { continue }
 
        $originalContent = $content
        $changed = $false
  
        # Fix 1: Remove 'public public class'
        if ($content -match 'public public class') {
            $content = $content -replace 'public public class', 'public class'
            $changed = $true
      }
   
        # Fix 2: Fix Is.InstanceOf assertions
        if ($content -match 'Is\.InstanceOf') {
    $content = $content -replace 'Assert\.True\(([^,]+),\s*Is\.InstanceOf<([^>]+)>\(\)\);?', 'Assert.IsType<$2>($1);'
  $changed = $true
        }
  
 # Fix 3: Remove duplicate [Theory] attributes - handle [Theory][InlineData] on same line
    if ($content -match '\[Theory\]\s*\[InlineData') {
  $content = $content -replace '\[Theory\]\s*(\[InlineData)', '$1'
            $changed = $true
        }
   
        # Fix 4: Handle [InlineData][Theory] pattern (reverse order)
        if ($content -match '\[InlineData[^\]]*\]\s+\[Theory\]') {
 $lines = $content -split "`r?`n"
        $newLines = @()
   $skip = $false
       
  for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
    
       # If we find [InlineData] followed by [Theory], reorganize
              if ($line -match '^\s*\[InlineData' -and $i+1 -lt $lines.Count -and $lines[$i+1] -match '^\s*\[Theory\]') {
        # Add [Theory] first
                    $indent = $lines[$i+1] -replace '\[Theory\].*', ''
          $newLines += "$indent[Theory]"
          # Then add this [InlineData]
              $newLines += $line
         # Skip the next [Theory] line
          $i++
            $changed = $true
                }
 else {
        $newLines += $line
     }
     }
            
    if ($changed) {
            $content = $newLines -join "`n"
            }
        }
      
        # Fix 5: Remove standalone duplicate [Theory] before method
 $lines = $content -split "`r?`n"
        $newLines = @()
        $theoryCount = 0
        
     for ($i = 0; $i -lt $lines.Count; $i++) {
   $line = $lines[$i]
            
            if ($line -match '^\s*\[Theory\]\s*$') {
                $theoryCount++
      # Look ahead to see if there are more [Theory] or we're about to hit a method
         $lookahead = 1
          $foundMethod = $false
     $foundDuplicate = $false
           
       while ($i + $lookahead -lt $lines.Count -and $lookahead -lt 10) {
           $nextLine = $lines[$i + $lookahead]
       
            if ($nextLine -match '^\s*\[Theory\]\s*$') {
             $foundDuplicate = $true
               break
 }
          if ($nextLine -match '^\s*public\s+void') {
        $foundMethod = $true
            break
 }
                    if ($nextLine -match '^\s*\[InlineData') {
             break
  }
   $lookahead++
                }
          
                # Only keep if it's not a duplicate before method
         if (!$foundDuplicate -or $theoryCount -eq 1) {
          $newLines += $line
      }
      else {
    $changed = $true
      }
       }
            else {
     if ($line -match 'public\s+void') {
     $theoryCount = 0
       }
  $newLines += $line
            }
        }
 
        if ($changed) {
   $content = $newLines -join "`n"
        }
        
        # Only write if content changed
        if ($content -ne $originalContent) {
            Set-Content -Path $file.FullName -Value $content -NoNewline -ErrorAction Stop
    $filesFixed++
      Write-Host "Fixed: $($file.Name)" -ForegroundColor Green
      }
    }
    catch {
   Write-Host "Error processing $($file.FullName): $_" -ForegroundColor Yellow
    }
}

Write-Host "`nCompleted! Fixed $filesFixed files." -ForegroundColor Cyan
