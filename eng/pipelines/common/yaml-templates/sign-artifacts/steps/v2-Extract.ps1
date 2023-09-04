[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [String] $SourceDir,
    [Parameter(Mandatory=$true)]
    [String] $DestinationDir,
    [String] $ArchiveType = "nupkg"
)

$ErrorActionPreference = "Stop"

$pathsToExclude = @(
    "_rels",
    "package",
    "[Content_Types].xml",
    ".signature.p7s"
)

$filesToExtract = Get-ChildItem $SourceDir -Filter "*.$ArchiveType" -Recurse
if ($filesToExtract.Count -eq 0) {
    Write-Warning "Did not find any .$ArchiveType files to sign in '$SourceDir'."
    Write-Host ""
    exit 0
}

Write-Host "Found $($filesToExtract.Count) *.$ArchiveType file(s) in '$SourceDir'..."
foreach ($file in $filesToExtract) {
    Write-Host "    $($file.FullName)"
}
Write-Host ""

$grouped = $filesToExtract | Group-Object "Name" | Where-Object { $_.Count -ne 1 }
if ($grouped.Count -ne 0) {
    Write-Warning "Found $($grouped.Count) duplicate file name(s):"
    foreach ($g in $grouped) {
        Write-Warning "    $($g.Name)"
        foreach ($file in $g.Group) {
            Write-Warning "        $($file.FullName)"
        }
    }
    Write-Host ""
}

Write-Host "Extracting all archive files.."
foreach ($archive in $filesToExtract) {
    $dest = Join-Path $DestinationDir $archive.BaseName
    if (Test-Path $dest) {
        Write-Warning "    Skipping duplicate file: $($archive.FullName)"
        continue
    }
    Write-Host "    $($archive.FullName) => $($dest)"
    Copy-Item $archive.FullName "$($archive.FullName).zip"
    Expand-Archive "$($archive.FullName).zip" -DestinationPath $dest
    Remove-Item "$($archive.FullName).zip"

    # delete the bits that "nuget pack" adds
    foreach ($bad in $pathsToExclude) {
        $bad = Join-Path $dest $bad
        Remove-Item -LiteralPath $bad -Recurse -Force -ErrorAction Ignore
    }
}
Write-Host ""
