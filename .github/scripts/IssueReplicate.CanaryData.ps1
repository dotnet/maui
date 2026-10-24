. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
. (Join-Path $PSScriptRoot 'IssueReplicate.Recording.ps1')

function Export-IssueReplicateCanaryPublicationData {
    param(
        [Parameter(Mandatory)][hashtable]$Directories,
        [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$BuildId,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string]$PipelineSha,
        [Parameter(Mandatory)]$Recording
    )

    $kinds = @('Input', 'Sample', 'Candidate', 'Verified')
    if ($Directories.Count -ne $kinds.Count -or @($Directories.Keys | Where-Object { $_ -cnotin $kinds }).Count) {
        throw 'Unexpected canary publication data directories.'
    }
    $bytes = Import-IssueReplicateRecording -Recording $Recording -Prefix REPRO_VIDEO_
    $metadata = @{ schemaVersion = 1; buildId = $BuildId; pipelineSha = $PipelineSha } |
    ConvertTo-Json -Compress
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('CANARY_PUBLICATION_Header=' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($metadata)))
    foreach ($kind in $kinds) {
        $encoded = & (Join-Path $PSScriptRoot 'IssueReplicate.Transport.ps1') -Mode Export -Kind $kind `
            -Directory $Directories[$kind] -Provider None
        $lines.Add("CANARY_PUBLICATION_$kind=$encoded")
    }
    $chunks = @(Export-IssueReplicateRecording -Bytes $bytes -Recording $Recording -Provider None)
    for ($index = 0; $index -lt $chunks.Count; $index++) {
        $lines.Add("CANARY_PUBLICATION_Recording$index=$($chunks[$index])")
    }
    return $lines.ToArray()
}

function Import-IssueReplicateCanaryPublicationData {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Directory,
        [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$BuildId,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string]$PipelineSha
    )

    if ([Text.Encoding]::UTF8.GetByteCount($Text) -gt 2MB) { throw 'The canary publication log exceeds its bound.' }
    if (Test-Path -LiteralPath $Directory) { throw 'Canary publication import cannot overwrite an existing directory.' }
    $packet = @{}
    foreach ($line in $Text -split "`n") {
        $line = $line.TrimEnd("`r")
        $line = $line -replace '^\d{4}-\d{2}-\d{2}T[0-9:.+-]+Z ', ''
        if (-not $line.StartsWith('CANARY_PUBLICATION_', [StringComparison]::Ordinal)) { continue }
        if ($line -cnotmatch '^CANARY_PUBLICATION_(Header|Input|Sample|Candidate|Verified|Recording[0-7])=([A-Za-z0-9+/]+={0,2})$') {
            throw 'The canary publication packet contains a malformed field.'
        }
        $name = $Matches[1]
        $encoded = $Matches[2]
        $limit = if ($name -eq 'Header') { 1024 } elseif ($name.StartsWith('Recording')) { 87384 } else { 120KB }
        if ($packet.ContainsKey($name) -or $encoded.Length -gt $limit) {
            throw 'The canary publication packet contains duplicate or oversized data.'
        }
        $packet[$name] = $encoded
    }
    foreach ($name in @('Header', 'Input', 'Sample', 'Candidate', 'Verified')) {
        if (-not $packet.ContainsKey($name)) { throw "The canary publication packet is missing $name." }
    }
    $metadata = [Text.UTF8Encoding]::new($false, $true).GetString([Convert]::FromBase64String($packet.Header)) |
    ConvertFrom-Json -Depth 4
    if ($metadata.schemaVersion -ne 1 -or $metadata.buildId -ne $BuildId -or $metadata.pipelineSha -cne $PipelineSha) {
        throw 'The canary publication packet does not match the selected build and source.'
    }
    foreach ($kind in @('Input', 'Sample', 'Candidate', 'Verified')) {
        & (Join-Path $PSScriptRoot 'IssueReplicate.Transport.ps1') -Mode Import -Kind $kind `
            -Directory (Join-Path $Directory $kind) -Encoded $packet[$kind]
    }
    $result = Get-Content -LiteralPath (Join-Path $Directory 'Verified/result.json') -Raw | ConvertFrom-Json -Depth 8
    Assert-IssueReplicateResultRecording -Result $result
    $saved = @{}
    try {
        for ($index = 0; $index -lt 8; $index++) {
            $name = "REPRO_VIDEO_$index"
            $saved[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $packet["Recording$index"])
        }
        $bytes = Import-IssueReplicateRecording -Recording $result.recording -Prefix REPRO_VIDEO_
    } finally {
        foreach ($name in $saved.Keys) { [Environment]::SetEnvironmentVariable($name, $saved[$name]) }
    }
    [IO.File]::WriteAllBytes((Join-Path $Directory 'recording.mp4'), $bytes)
    return @{ Directory = $Directory; RecordingBytes = $bytes; Packet = $packet }
}
