# USAGE:
#  - To mark all APIs as shipped:
#    .\eng\scripts\mark-shipped.ps1
#  - To mark APIs as shipped/unshipped based on changes between two branches:
#    .\eng\scripts\mark-shipped.ps1 -BaselineBranch <branch>

[CmdletBinding(PositionalBinding = $false)]
param (
    [string]$BaselineBranch
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

$removedPrefix = "*REMOVED*";

function Read-ApiFiles([string]$dir) {
    $shipped = @()
    $removed = @()
    $added = @()

    $shippedFilePath = Join-Path $dir "PublicAPI.Shipped.txt"
    $shippedContents = Get-Content $shippedFilePath
    foreach ($item in $shippedContents) {
        $item = $item.Trim()
        if ($item.Length -gt 0) {
            $shipped += $item
        }
    }

    $unshippedFilePath = Join-Path $dir "PublicAPI.Unshipped.txt"
    $unshippedContents = Get-Content $unshippedFilePath
    foreach ($item in $unshippedContents) {
        $item = $item.Trim()
        if ($item.Length -gt 0) {
            if ($item.StartsWith($removedPrefix)) {
                $item = $item.Substring($removedPrefix.Length).Trim()
                $removed += $item
            }
            else {
                $added += $item
            }
        }
    }

    $filtered = ($shipped + $added) | Where-Object { -not $removed.Contains($_) } | Sort-Object -Unique

    return @{
        Shipped  = $shipped
        Added    = $added
        Removed  = $removed
        Filtered = $filtered
    }
}

function Move-Shipped([string]$dir) {
    Write-Host "- Processing $dir" -NoNewline
    
    $results = Read-ApiFiles $dir
    $s = $results.Shipped.Count
    $a = $results.Added.Count
    $r = $results.Removed.Count
    $f = $results.Filtered.Count

    Write-Host " => $s + $a - $r = $f"

    $unshippedFilePath = Join-Path $dir "PublicAPI.Unshipped.txt"
    $shippedFilePath = Join-Path $dir "PublicAPI.Shipped.txt"

    $results.Filtered | Out-File $shippedFilePath -Encoding Ascii -Force
    "#nullable enable" | Out-File $unshippedFilePath -Encoding Ascii -Force
}

function Get-ShippedApiFiles() {
    Write-Host "Looking for PublicAPI files..."
    $files = Get-ChildItem "src" -Recurse -Filter "PublicApi.Shipped.txt" -Exclude "artifacts"
    Write-Host "Found $($files.Count) files."
    return $files
}

function Read-AllApiFiles() {
    $files = Get-ShippedApiFiles
    $results = @{}
    $counter = 0
    Write-Host "Processing [$counter/$($files.Count)]..." -NoNewline
    foreach ($file in $files) {
        $counter += 1
        Write-Host "`rProcessing $file [$counter/$($files.Count)]...                                                                        `r" -NoNewline
        $results[$file.FullName] = Read-ApiFiles (Split-Path -Parent $file)
    }
    Write-Host "`rDone.                                                                                                                                 `r"
    return $results
}

if ($BaselineBranch) {
    $CurrentBranch = git rev-parse --abbrev-ref HEAD
    Write-Host "Regenerating unshipped PublicAPI files for changes between $BaselineBranch and $CurrentBranch..."

    # get the APIs for the current branch
    Write-Host "Reading PubilicAPI files for $CurrentBranch..."
    $currentChanges = Read-AllApiFiles

    # get the APIs for the baseline branch
    Write-Host "Reading PubilicAPI files for $BaselineBranch..."
    git checkout $BaselineBranch
    $baselineChanges = Read-AllApiFiles

    # switch back to the current branch
    Write-Host "Generating unshipped PublicAPI files..."
    git checkout $CurrentBranch

    # process the differences between the branches
    foreach ($file in $currentChanges.Keys) {
        $dir = (Split-Path -Parent $file)
        Write-Host "- Processing $dir" -NoNewline
    
        # get the changes for this file for each branch
        $currentFile = $currentChanges[$file]
        $baselineFile = $baselineChanges[$file]

        # skip any files that don't exist in both branches
        if (-not $currentFile) {
            Write-Host " => does not exist in the current branch"
            continue
        }
        if (-not $baselineFile) {
            Write-Host " => does not exist in the baseline branch"
            continue
        }

        # get the APIs for the current and baseline branches
        $current = $currentFile.Filtered
        $baseline = $baselineFile.Filtered

        # calculate the changes between the branches
        $added = $current | Where-Object { -not $baseline.Contains($_) }
        $removed = $baseline | Where-Object { -not $current.Contains($_) }
        $both = ($added + $removed) | Sort-Object -Unique

        # generate the unshipped file contents
        $unshipped = @()
        $unshipped += "#nullable enable"
        foreach ($item in $both) {
            if ($added -contains $item) {
                $unshipped += $item
            }
            else {
                $unshipped += "$removedPrefix$item"
            }
        }

        $unshippedFilePath = Join-Path $dir "PublicAPI.Unshipped.txt"
        $shippedFilePath = Join-Path $dir "PublicAPI.Shipped.txt"

        $baseline | Out-File $shippedFilePath -Encoding Ascii -Force
        $unshipped | Out-File $unshippedFilePath -Encoding Ascii -Force

        Write-Host " => Done"
    }
}
else {
    try {
        $files = Get-ShippedApiFiles
        Write-Host "Processing files..."
        foreach ($file in $files) {
            Move-Shipped (Split-Path -Parent $file)
        }
        Write-Host "Processing complete."
    }
    catch {
        Write-Error $_
        Write-Error $_.Exception
        exit 1
    }
}
