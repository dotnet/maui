#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Adds trusted performance categories without replacing a measured revision's categories.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$TrustedCategoryPath,

    [Parameter(Mandatory = $true)]
    [string]$TargetCategoryPath
)

$ErrorActionPreference = "Stop"

$trustedSource = Get-Content -LiteralPath $TrustedCategoryPath -Raw
$targetSource = Get-Content -LiteralPath $TargetCategoryPath -Raw
$declarations = [regex]::Matches(
    $trustedSource,
    'public\s+const\s+string\s+(?<name>Performance\w*)\s*=\s*nameof\(\s*\k<name>\s*\)\s*;')
if ($declarations.Count -eq 0) {
    throw "The trusted harness contains no performance category declarations."
}

$classes = [regex]::Matches(
    $targetSource,
    '(?m)^(?<indent>[ \t]*)public\s+static\s+(?:partial\s+)?class\s+TestCategory\s*\{')
if ($classes.Count -ne 1) {
    throw "Expected exactly one TestCategory class in the measured revision."
}

$additions = [Collections.Generic.List[string]]::new()
$names = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($declaration in $declarations) {
    $name = $declaration.Groups["name"].Value
    if (-not $names.Add($name)) {
        throw "Duplicate trusted performance category '$name'."
    }

    $escapedName = [regex]::Escape($name)
    $existing = [regex]::Matches(
        $targetSource,
        "public\s+const\s+string\s+$escapedName\s*=\s*(?<value>[^;]+);")
    if ($existing.Count -gt 0) {
        $expectedValue = '^(?:nameof\(\s*' + $escapedName + '\s*\)|"' + $escapedName + '")$'
        if ($existing.Count -ne 1 -or $existing[0].Groups["value"].Value.Trim() -cnotmatch $expectedValue) {
            throw "Measured revision has a conflicting performance category '$name'."
        }
        continue
    }

    $additions.Add("$($classes[0].Groups['indent'].Value)`tpublic const string $name = nameof($name);")
}

if ($additions.Count -gt 0) {
    $newLine = if ($targetSource.Contains("`r`n")) { "`r`n" } else { "`n" }
    $insert = $newLine + ($additions -join $newLine)
    $mergedSource = $targetSource.Insert($classes[0].Index + $classes[0].Length, $insert)
    $bytes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $TargetCategoryPath).Path)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    [IO.File]::WriteAllText(
        (Resolve-Path -LiteralPath $TargetCategoryPath).Path,
        $mergedSource,
        [Text.UTF8Encoding]::new($hasBom))
}

Write-Host "Added $($additions.Count) trusted performance category declaration(s)."
