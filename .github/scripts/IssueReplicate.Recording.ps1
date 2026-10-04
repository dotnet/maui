function Read-IssueReplicateHttpResponse {
    param(
        [Parameter(Mandatory)][Net.Http.HttpResponseMessage]$Response,
        [Parameter(Mandatory)][ValidateRange(1, 1048576)][int]$MaxBytes,
        [Parameter(Mandatory)][Threading.CancellationToken]$CancellationToken,
        [switch]$AppiumErrorResponse
    )

    if (-not $AppiumErrorResponse -or $Response.IsSuccessStatusCode) {
        $Response.EnsureSuccessStatusCode() | Out-Null
    } else {
        $MaxBytes = [Math]::Min($MaxBytes, 16384)
    }
    if ($Response.Content.Headers.ContentLength -gt $MaxBytes) { throw 'The HTTP response exceeds its bound.' }
    $stream = $Response.Content.ReadAsStreamAsync($CancellationToken).GetAwaiter().GetResult()
    try {
        $bytes = [byte[]]::new($MaxBytes + 1)
        $count = 0
        while ($count -le $MaxBytes) {
            $read = $stream.ReadAsync($bytes, $count, $bytes.Length - $count, $CancellationToken).GetAwaiter().GetResult()
            if (-not $read) { break }
            $count += $read
        }
        if ($count -gt $MaxBytes) { throw 'The HTTP response exceeds its bound.' }
        $text = [Text.UTF8Encoding]::new($false, $true).GetString($bytes, 0, $count)
        if (-not $Response.IsSuccessStatusCode) {
            $errorResponse = $text | ConvertFrom-Json -Depth 8
            $status = [int]$Response.StatusCode
            if ($errorResponse.value.message -isnot [string] -or
                [string]::IsNullOrWhiteSpace($errorResponse.value.message)) {
                throw "Native Appium recording failed (HTTP $status) without an error message."
            }
            $message = $errorResponse.value.message -replace '##vso\[[^]]*\]', '' -replace '[\x00-\x1f\x7f]', ' '
            $message = $message.Substring(0, [Math]::Min(900, $message.Length))
            throw "Native Appium recording failed (HTTP $status): $message"
        }
        return $text
    } finally { $stream.Dispose() }
}

function Invoke-IssueReplicateRecordingRequest {
    param(
        [Parameter(Mandatory)][ValidateSet('POST')][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [hashtable]$Body = @{},
        [ValidateRange(1, 1048576)][int]$MaxBytes = 16384,
        [uri]$ServerUri = 'http://127.0.0.1:4723/wd/hub/'
    )

    if ($Path -cnotmatch '^session/[a-zA-Z0-9-]{1,100}/appium/(start|stop)_recording_screen$') {
        throw 'Unexpected native recording endpoint.'
    }
    if ($ServerUri.Scheme -cne 'http' -or $ServerUri.Host -cne '127.0.0.1' -or
        $ServerUri.AbsolutePath -cne '/wd/hub/' -or $ServerUri.UserInfo -or $ServerUri.Query -or $ServerUri.Fragment) {
        throw 'Native recording requires the loopback Appium endpoint.'
    }
    $handler = [Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $handler.UseProxy = $false
    $client = [Net.Http.HttpClient]::new($handler)
    $deadline = [Threading.CancellationTokenSource]::new([TimeSpan]::FromSeconds(20))
    $request = [Net.Http.HttpRequestMessage]::new(
        [Net.Http.HttpMethod]::new($Method), [uri]::new($ServerUri, $Path))
    $response = $null
    try {
        if ($Method -eq 'POST') {
            $request.Content = [Net.Http.StringContent]::new(
                ($Body | ConvertTo-Json -Depth 5 -Compress), [Text.Encoding]::UTF8, 'application/json')
        }
        $response = $client.SendAsync($request, [Net.Http.HttpCompletionOption]::ResponseHeadersRead,
            $deadline.Token).GetAwaiter().GetResult()
        $result = Read-IssueReplicateHttpResponse -Response $response -MaxBytes $MaxBytes `
            -CancellationToken $deadline.Token -AppiumErrorResponse |
        ConvertFrom-Json -Depth 8
        if ('value' -cnotin @($result.PSObject.Properties.Name)) {
            throw 'Appium did not return a recording response envelope.'
        }
        if ($result.value.error) { throw 'Appium rejected the native recording request.' }
        return $result.value
    } finally {
        if ($response) { $response.Dispose() }
        $request.Dispose()
        $deadline.Dispose()
        $client.Dispose()
        $handler.Dispose()
    }
}

function Get-IssueReplicateRecordingSessionId {
    param([Parameter(Mandatory)][string]$LogPath)

    $log = Read-IssueReplicateNativeDiagnostic -Path $LogPath -MaxBytes 256KB -Tail
    $matches = [regex]::Matches($log.Text,
        '(?:\[HTTP\]\s+-->\s+(?:GET|POST|DELETE)\s+/(?:wd/hub/)?session/|\[AppiumDriver(?:@[^\]\r\n]+)?\]\s+New [^\r\n]+ session created successfully, session )([a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12})')
    if (-not $matches.Count) { throw 'The live Appium log contains no bounded frontend session identifier.' }
    return $matches[$matches.Count - 1].Groups[1].Value
}

function Start-IssueReplicateRecording {
    param(
        [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform,
        [Parameter(Mandatory)][string]$LogPath
    )

    $id = Get-IssueReplicateRecordingSessionId -LogPath $LogPath
    $options = @{ timeLimit = 30; forceRestart = $true }
    if ($Platform -eq 'android') {
        $options.bitRate = 100000
        $options.videoSize = '480x854'
    } else {
        $options.videoType = 'libx264'
        $options.videoFps = 10
        $options.videoScale = '480:-2'
        $options.videoQuality = 'low'
    }
    Invoke-IssueReplicateRecordingRequest -Method POST -Path "session/$id/appium/start_recording_screen" `
        -Body @{ options = $options } | Out-Null
    return $id
}

function Assert-IssueReplicateRecordingBytes {
    param([Parameter(Mandatory)][byte[]]$Bytes)

    if ($Bytes.Length -lt 24 -or $Bytes.Length -gt 512KB -or
        [Text.Encoding]::ASCII.GetString($Bytes, 4, 4) -cne 'ftyp') {
        throw 'The native recording is not a bounded MP4 file.'
    }
}

function Stop-IssueReplicateRecording {
    param([Parameter(Mandatory)][string]$SessionId)

    $encoded = Invoke-IssueReplicateRecordingRequest -Method POST `
        -Path "session/$SessionId/appium/stop_recording_screen" -MaxBytes 720KB
    if ($encoded -isnot [string] -or $encoded.Length -lt 1 -or
        $encoded.Length -gt 4 * [Math]::Ceiling(512KB / 3) -or
        $encoded -cnotmatch '^[A-Za-z0-9+/]+={0,2}$') {
        throw 'Appium did not return a bounded native recording.'
    }
    $bytes = [Convert]::FromBase64String($encoded)
    Assert-IssueReplicateRecordingBytes -Bytes $bytes
    return ,$bytes
}

function Assert-IssueReplicateRecording {
    param([Parameter(Mandatory)]$Recording)

    if ($Recording.status -isnot [string] -or $Recording.status -cnotin @('available', 'failed', 'not-started')) {
        throw 'The recording status is invalid.'
    }
    if ($Recording.status -eq 'available' -and
        ($Recording.sha256 -isnot [string] -or $Recording.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
        ($Recording.bytes -isnot [long] -and $Recording.bytes -isnot [int]) -or
        $Recording.bytes -lt 24 -or $Recording.bytes -gt 512KB)) {
        throw 'The recording identity or size is invalid.'
    }
    if ($Recording.diagnostic -isnot [string] -or $Recording.diagnostic.Length -gt 1000) {
        throw 'The recording diagnostic is invalid.'
    }
    if ($Recording.status -ne 'available' -and [string]::IsNullOrWhiteSpace($Recording.diagnostic)) {
        throw 'An unavailable recording must include its diagnostic.'
    }
}

function Assert-IssueReplicateResultRecording {
    param([Parameter(Mandatory)]$Result)

    if ($Result.recording) {
        if ($Result.testKind -cne 'ui') { throw 'Only a UI result can contain a native recording.' }
        Assert-IssueReplicateRecording -Recording $Result.recording
    }
}

function Export-IssueReplicateRecording {
    param(
        [Parameter(Mandatory)][byte[]]$Bytes,
        [Parameter(Mandatory)]$Recording,
        [ValidateSet('None', 'Azure', 'GitHub')][string]$Provider
    )

    Assert-IssueReplicateRecording -Recording $Recording
    Assert-IssueReplicateRecordingBytes -Bytes $Bytes
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
    if ($Recording.status -cne 'available' -or $Recording.bytes -ne $Bytes.Length -or $Recording.sha256 -cne $hash) {
        throw 'The recording bytes do not match the completed result.'
    }
    if ($Provider -eq 'GitHub' -and $Bytes.Length -gt 256KB) {
        throw 'The recording exceeds the GitHub job-output byte budget.'
    }
    for ($index = 0; $index * 64KB -lt $Bytes.Length; $index++) {
        $length = [Math]::Min(64KB, $Bytes.Length - $index * 64KB)
        $encoded = [Convert]::ToBase64String($Bytes, $index * 64KB, $length)
        switch ($Provider) {
            'Azure' { Write-Host "##vso[task.setvariable variable=recording$index;isOutput=true]$encoded" }
            'GitHub' {
                if (-not $env:GITHUB_OUTPUT) { throw 'The GitHub job output file is unavailable.' }
                [IO.File]::AppendAllText($env:GITHUB_OUTPUT, "recording$index=$encoded`n")
            }
            default { Write-Output $encoded }
        }
    }
}

function Import-IssueReplicateRecording {
    param(
        [Parameter(Mandatory)]$Recording,
        [Parameter(Mandatory)][ValidatePattern('^REPRO_(FIRST_|CONFIRM_|VERIFIED1_|VERIFIED2_)?VIDEO_$')]
        [string]$Prefix
    )

    Assert-IssueReplicateRecording -Recording $Recording
    if ($Recording.status -cne 'available') { throw 'No completed recording is available.' }
    $bytes = [byte[]]::new([int]$Recording.bytes)
    $count = [int][Math]::Ceiling($bytes.Length / 64KB)
    for ($index = 0; $index -lt 8; $index++) {
        $encoded = [Environment]::GetEnvironmentVariable("$Prefix$index")
        if ($index -ge $count) {
            if ($encoded -and $encoded -cnotmatch '^\$\([A-Za-z0-9_]+\)$') { throw 'Unexpected recording fragment.' }
            continue
        }
        $length = [Math]::Min(64KB, $bytes.Length - $index * 64KB)
        if (-not $encoded -or $encoded.Length -ne 4 * [Math]::Ceiling($length / 3) -or
            $encoded -cnotmatch '^[A-Za-z0-9+/]+={0,2}$') { throw 'A recording fragment is missing or malformed.' }
        $chunk = [Convert]::FromBase64String($encoded)
        if ($chunk.Length -ne $length) { throw 'A recording fragment has an invalid size.' }
        [Array]::Copy($chunk, 0, $bytes, $index * 64KB, $chunk.Length)
    }
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    if ($hash -cne $Recording.sha256) { throw 'Recording fragments failed their integrity check.' }
    Assert-IssueReplicateRecordingBytes -Bytes $bytes
    return ,$bytes
}

function Sync-IssueReplicateRecordingPublication {
    param(
        [Parameter(Mandatory)][byte[]]$Bytes,
        [Parameter(Mandatory)]$Recording,
        [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
        [AllowEmptyString()][string]$ExistingBody = '',
        [Parameter(Mandatory)][scriptblock]$SaveCheckpoint
    )

    Export-IssueReplicateRecording -Bytes $Bytes -Recording $Recording -Provider None | Out-Null
    if ([Text.Encoding]::UTF8.GetByteCount($ExistingBody) -gt 65000) {
        throw 'The prior recording report exceeds its reconciliation bound.'
    }
    $mediaMarker = "<!-- issue-replicate-recording:$($Recording.sha256) -->"
    $match = [regex]::Match($ExistingBody,
        [regex]::Escape($mediaMarker) + '\s+(https://github\.com/user-attachments/assets/[a-fA-F0-9-]{36})(?:\s|$)')
    if ($match.Success) {
        $url = $match.Groups[1].Value
        return @{ Url = $url; Checkpoint = "$mediaMarker`n$url" }
    }
    if ($ExistingBody.Contains('<!-- issue-replicate-recording-pending:') -or
        $ExistingBody.Contains('<!-- issue-replicate-recording:')) {
        throw 'A prior recording upload is unreconciled or mismatched. Reconcile its attachment receipt manually; an automatic retry will not upload again.'
    }

    & $SaveCheckpoint "<!-- issue-replicate-recording-pending:$($Recording.sha256) -->" | Out-Null
    $url = Publish-IssueReplicateRecording -Bytes $Bytes -IssueNumber $IssueNumber
    if ($url -cnotmatch '^https://github\.com/user-attachments/assets/[a-fA-F0-9-]{36}$') {
        throw 'The upload returned no approved recording receipt; reconcile the pending report before retrying.'
    }
    $checkpoint = "$mediaMarker`n$url"
    Write-Host "Native recording upload receipt ($($Recording.sha256)): $url"
    & $SaveCheckpoint $checkpoint | Out-Null
    return @{ Url = $url; Checkpoint = $checkpoint }
}

function Publish-IssueReplicateRecording {
    param(
        [Parameter(Mandatory)][byte[]]$Bytes,
        [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber
    )

    Assert-IssueReplicateRecordingBytes -Bytes $Bytes
    $repoId = gh api repos/dotnet/maui --jq .id
    if ($LASTEXITCODE -ne 0 -or "$repoId" -cnotmatch '^[1-9][0-9]*$') {
        throw 'Could not resolve the recording destination repository.'
    }
    $token = if ($env:GH_TOKEN) { $env:GH_TOKEN } else { $env:GITHUB_TOKEN }
    if (-not $token) {
        $token = gh auth token --hostname github.com
        if ($LASTEXITCODE -ne 0 -or -not $token) { throw 'The media publisher is not authenticated.' }
    }
    $handler = [Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $client = [Net.Http.HttpClient]::new($handler)
    $deadline = [Threading.CancellationTokenSource]::new([TimeSpan]::FromSeconds(60))
    $response = $null
    $request = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::Post,
        "https://uploads.github.com/user-attachments/assets?name=issue-$IssueNumber.mp4&content_type=video%2Fmp4&repository_id=$repoId")
    try {
        $request.Headers.Authorization = [Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $token.Trim())
        $request.Headers.Add('X-GitHub-Api-Version', '2022-11-28')
        $request.Headers.Add('User-Agent', 'maui-issue-replicate')
        $request.Content = [Net.Http.ByteArrayContent]::new($Bytes)
        $request.Content.Headers.ContentType = [Net.Http.Headers.MediaTypeHeaderValue]::new('application/octet-stream')
        $response = $client.SendAsync($request, [Net.Http.HttpCompletionOption]::ResponseHeadersRead,
            $deadline.Token).GetAwaiter().GetResult()
        $text = Read-IssueReplicateHttpResponse -Response $response -MaxBytes 16384 `
            -CancellationToken $deadline.Token
        $receipt = $text | ConvertFrom-Json -Depth 5
        if ($receipt.url -cnotmatch '^https://github\.com/user-attachments/assets/[a-fA-F0-9-]{36}$') {
            throw 'GitHub did not return an approved recording URL.'
        }
        return [string]$receipt.url
    } finally {
        if ($response) { $response.Dispose() }
        $request.Dispose()
        $deadline.Dispose()
        $client.Dispose()
        $handler.Dispose()
        $token = $null
    }
}
