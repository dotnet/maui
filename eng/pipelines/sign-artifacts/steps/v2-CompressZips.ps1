[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [String] $SourceDir,
    [Parameter(Mandatory=$true)]
    [String] $DestinationDir
)

$ErrorActionPreference = "Stop"

New-Item -Type Directory $DestinationDir -Force | Out-Null

Write-Host "Compressing all the directories(s) in '$SourceDir'..."
$folders = Get-ChildItem $SourceDir -Directory

foreach ($dir in $folders) {
    $name = $dir.Name
    $src = $dir.FullName
    $dest = "$DestinationDir/$name.zip"

    Write-Host "Compressing '$src' to '$dest'..."
    Compress-Archive -Path "$src/*" -DestinationPath $dest -Force
}
