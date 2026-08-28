[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Path
)

$root = (Resolve-Path -LiteralPath $Path).Path.TrimEnd('\', '/')
$files = @(Get-ChildItem -LiteralPath $root -File -Recurse -Force)
if ($files.Count -eq 0) {
    throw "Directory '$root' contains no files."
}

[string[]] $entries = $files |
    ForEach-Object {
        $relativePath = $_.FullName.Substring($root.Length).TrimStart('\', '/').Replace('\', '/')
        $fileHash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        "$relativePath`t$fileHash"
    }
[Array]::Sort($entries, [StringComparer]::Ordinal)

$manifest = [string]::Join("`n", $entries)
$bytes = [System.Text.Encoding]::UTF8.GetBytes($manifest)
$hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
[Convert]::ToHexString($hash).ToLowerInvariant()
