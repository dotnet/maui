#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Export', 'Import', 'FetchSample')][string]$Mode,
    [ValidateSet('Input', 'Sample', 'Candidate', 'Verified')][string]$Kind,
    [Parameter(Mandatory)][string]$Directory,
    [string]$Encoded = '',
    [ValidateSet('None', 'Azure', 'GitHub')][string]$Provider = 'None'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$utf8 = [Text.UTF8Encoding]::new($false, $true)
$limits = switch ($Kind) {
    'Input' { @{ 'manifest.json' = 50000 } }
    'Sample' { @{ 'sample-result.json' = 16384 } }
    'Candidate' { @{ 'candidate.json' = 80000 } }
    'Verified' { @{ 'result.json' = 16384; 'test.patch' = 102400; 'feedback.txt' = 4096 } }
}

function Read-RegularBytes {
    param([string]$Path, [int]$MaxBytes)
    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -lt 1 -or $file.Length -gt $MaxBytes -or
        $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Job data must be a bounded regular file.'
    }
    return ,[IO.File]::ReadAllBytes($file.FullName)
}

if ($Mode -eq 'FetchSample') {
    $manifest = $utf8.GetString((Read-RegularBytes (Join-Path $Directory 'manifest.json') 50000)) |
        ConvertFrom-Json -Depth 6
    if ($manifest.schemaVersion -ne 1 -or $manifest.targetSha -cnotmatch '^[0-9a-f]{40}$' -or
        $manifest.sampleSha256 -cnotmatch '^[0-9a-f]{64}$') {
        throw 'Cannot fetch a sample without an immutable issue snapshot.'
    }
    $source = Get-IssueReplicateSource -AuthorTexts @("[repro.zip]($($manifest.sourceUrl))")
    if ($source.Type -cne $manifest.sourceType -or $source.Url -cne $manifest.sourceUrl) {
        throw 'The sample source does not match an approved GitHub URL.'
    }
    $url = $source.Url
    if ($source.Type -eq 'repository') {
        if ($manifest.sourceCommit -cnotmatch '^[0-9a-f]{40}$') { throw 'The sample revision is not pinned.' }
        $repo = ([uri]$source.Url).AbsolutePath.TrimStart('/')
        $url = "https://api.github.com/repos/$repo/zipball/$($manifest.sourceCommit)"
    }
    $bytes = Get-IssueReplicateDownload -Url ([uri]$url) -MaxBytes 10MB
    if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne
        $manifest.sampleSha256) { throw 'The re-fetched sample differs from the pinned intake hash.' }
    $path = Join-Path $Directory 'sample.zip'
    [IO.File]::WriteAllBytes($path, $bytes)
    Assert-IssueReplicateZip -Path $path
    return
}

if (-not $Kind) { throw 'A job data kind is required.' }
$required = switch ($Kind) {
    'Input' { 'manifest.json' }
    'Sample' { 'sample-result.json' }
    'Candidate' { 'candidate.json' }
    'Verified' { 'result.json' }
}
if ($Mode -eq 'Export') {
    if ($Kind -eq 'Verified' -and (Test-Path -LiteralPath (Join-Path $Directory 'test.log') -PathType Leaf)) {
        $feedback = Get-IssueReplicateFeedback -Path (Join-Path $Directory 'test.log')
        if (-not [string]::IsNullOrWhiteSpace($feedback)) {
            [IO.File]::WriteAllText((Join-Path $Directory 'feedback.txt'), $feedback, $utf8)
        }
    }
    $files = @()
    foreach ($name in @($limits.Keys | Sort-Object)) {
        $path = Join-Path $Directory $name
        if (-not (Test-Path -LiteralPath $path)) {
            if ($name -ceq $required) { throw "Required job data is missing: $name." }
            continue
        }
        $bytes = Read-RegularBytes -Path $path -MaxBytes $limits[$name]
        $files += @{
            name = $name
            sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
            content = [Convert]::ToBase64String($bytes)
        }
    }
    $json = @{ schemaVersion = 1; kind = $Kind; files = $files } | ConvertTo-Json -Depth 5 -Compress
    $bytes = $utf8.GetBytes($json)
    if ($bytes.Length -gt 320KB) { throw 'Job data exceeds the transfer limit.' }
    $output = [IO.MemoryStream]::new()
    try {
        $gzip = [IO.Compression.GZipStream]::new($output, [IO.Compression.CompressionLevel]::Optimal, $true)
        try { $gzip.Write($bytes, 0, $bytes.Length) } finally { $gzip.Dispose() }
        $Encoded = [Convert]::ToBase64String($output.ToArray())
    } finally { $output.Dispose() }
    if ($Encoded.Length -gt 120KB) { throw 'Compressed job data exceeds the environment-safe output limit.' }
    switch ($Provider) {
        'Azure' { Write-Host "##vso[task.setvariable variable=payload;isOutput=true]$Encoded" }
        'GitHub' {
            if (-not $env:GITHUB_OUTPUT) { throw 'The GitHub job output file is unavailable.' }
            [IO.File]::AppendAllText($env:GITHUB_OUTPUT, "payload=$Encoded`n", $utf8)
        }
        default { return $Encoded }
    }
    return
}

if ($Encoded.Length -lt 1 -or $Encoded.Length -gt 120KB -or
    $Encoded -cnotmatch '^[A-Za-z0-9+/]+={0,2}$') { throw 'Job data is missing or malformed.' }
$inputStream = [IO.MemoryStream]::new([Convert]::FromBase64String($Encoded))
$output = [IO.MemoryStream]::new()
try {
    $gzip = [IO.Compression.GZipStream]::new($inputStream, [IO.Compression.CompressionMode]::Decompress)
    try {
        $buffer = [byte[]]::new(8192)
        while (($count = $gzip.Read($buffer, 0, $buffer.Length)) -gt 0) {
            if ($output.Length + $count -gt 320KB) { throw 'Expanded job data exceeds the transfer limit.' }
            $output.Write($buffer, 0, $count)
        }
    } finally { $gzip.Dispose() }
    $envelope = $utf8.GetString($output.ToArray()) | ConvertFrom-Json -Depth 6
} finally {
    $output.Dispose()
    $inputStream.Dispose()
}
if ($envelope.schemaVersion -ne 1 -or $envelope.kind -cne $Kind -or
    @($envelope.files).Count -lt 1 -or @($envelope.files).Count -gt $limits.Count) {
    throw 'Unexpected job data contract.'
}
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$validated = @()
foreach ($file in @($envelope.files)) {
    $name = [string]$file.name
    if ($name -cnotin @($limits.Keys) -or -not $seen.Add($name) -or
        $file.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $file.content -isnot [string] -or
        $file.content.Length -gt 4 * [Math]::Ceiling($limits[$name] / 3) -or
        $file.content -cnotmatch '^[A-Za-z0-9+/]+={0,2}$') { throw 'Unexpected job data file.' }
    $bytes = [Convert]::FromBase64String($file.content)
    if ($bytes.Length -lt 1 -or $bytes.Length -gt $limits[$name] -or
        [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() -cne
        $file.sha256) { throw 'Job data failed its size or integrity check.' }
    $validated += @{ name = $name; bytes = $bytes }
}
if (-not $seen.Contains($required)) { throw 'Required job data is missing.' }
New-Item -ItemType Directory -Path $Directory -Force | Out-Null
foreach ($file in $validated) {
    $path = Join-Path $Directory $file.name
    if (Test-Path -LiteralPath $path) { throw 'Job data cannot overwrite an existing file.' }
    [IO.File]::WriteAllBytes($path, $file.bytes)
}
