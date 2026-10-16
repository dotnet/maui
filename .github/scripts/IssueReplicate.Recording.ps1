function Read-IssueReplicateHttpResponse {
    param(
        [Parameter(Mandatory)][Net.Http.HttpResponseMessage]$Response,
        [Parameter(Mandatory)][ValidateRange(1, 1048576)][int]$MaxBytes,
        [Parameter(Mandatory)][Threading.CancellationToken]$CancellationToken,
        [switch]$AppiumErrorResponse
    )

    if (-not $AppiumErrorResponse -or $Response.IsSuccessStatusCode) {
        $Response.EnsureSuccessStatusCode() | Out-Null
    }
    else {
        $MaxBytes = [Math]::Min($MaxBytes, 16384)
    }
    if ($Response.Content.Headers.ContentLength -gt $MaxBytes) {
        throw "The HTTP response exceeds its bound: declared $($Response.Content.Headers.ContentLength) bytes, permitted $MaxBytes."
    }
    $stream = $Response.Content.ReadAsStreamAsync($CancellationToken).GetAwaiter().GetResult()
    try {
        $bytes = [byte[]]::new($MaxBytes + 1)
        $count = 0
        while ($count -le $MaxBytes) {
            $read = $stream.ReadAsync($bytes, $count, $bytes.Length - $count, $CancellationToken).GetAwaiter().GetResult()
            if (-not $read) { break }
            $count += $read
        }
        if ($count -gt $MaxBytes) { throw "The HTTP response exceeds its bound: read at least $count bytes, permitted $MaxBytes." }
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
    }
    finally { $stream.Dispose() }
}

function Invoke-IssueReplicateRecordingRequest {
    param(
        [Parameter(Mandatory)][ValidateSet('POST', 'GET')][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [hashtable]$Body = @{},
        [ValidateRange(1, 1048576)][int]$MaxBytes = 16384,
        [uri]$ServerUri = 'http://127.0.0.1:4723/wd/hub/',
        [ValidateRange(1, 20)][int]$TimeoutSeconds = 20
    )

    $recordingPath = $Path -cmatch '^session/[a-zA-Z0-9-]{1,100}/appium/(start|stop)_recording_screen$'
    $diagnosticPath = $Path -cmatch '^session/[a-zA-Z0-9-]{1,100}/(source|screenshot)$'
    if (($Method -ceq 'POST' -and -not $recordingPath) -or
        ($Method -ceq 'GET' -and -not $diagnosticPath)) {
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
    $deadline = [Threading.CancellationTokenSource]::new([TimeSpan]::FromSeconds($TimeoutSeconds))
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
        if ($result.value -isnot [string] -and $result.value.error) {
            throw 'Appium rejected the native recording request.'
        }
        return $result.value
    }
    finally {
        if ($response) { $response.Dispose() }
        $request.Dispose()
        $deadline.Dispose()
        $client.Dispose()
        $handler.Dispose()
    }
}

function Write-IssueReplicateDiagnosticBytes {
    param(
        [Parameter(Mandatory)][ValidateSet('TREE', 'SCREENSHOT', 'VIDEO')][string]$Kind,
        [Parameter(Mandatory)][ValidateSet('START', 'STOP')][string]$Phase,
        [Parameter(Mandatory)][byte[]]$Bytes
    )

    $limit = switch ($Kind) { 'TREE' { 32KB } 'SCREENSHOT' { 128KB } 'VIDEO' { 512KB } }
    if ($Bytes.Length -lt 1 -or $Bytes.Length -gt $limit) {
        throw "The diagnostic $Kind exceeds its fixed bounded evidence budget."
    }
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
    Write-Host "ISSUE_REPLICATE_DIAGNOSTIC_${Phase}_${Kind}_SHA256=$hash"
    Write-Host "ISSUE_REPLICATE_DIAGNOSTIC_${Phase}_${Kind}_BYTES=$($Bytes.Length)"
    for ($index = 0; $index * 4096 -lt $Bytes.Length; $index++) {
        $length = [Math]::Min(4096, $Bytes.Length - $index * 4096)
        Write-Host "ISSUE_REPLICATE_DIAGNOSTIC_${Phase}_${Kind}_$index=$([Convert]::ToBase64String($Bytes, $index * 4096, $length))"
    }
}

function Write-IssueReplicateNativeSnapshot {
    param(
        [Parameter(Mandatory)][string]$SessionId,
        [Parameter(Mandatory)][ValidateSet('START', 'STOP')][string]$Phase,
        [Collections.Generic.List[string]]$Observations,
        [switch]$GalleryOnly
    )

    $kinds = if ($GalleryOnly) { @('TREE') } else { @('TREE', 'SCREENSHOT') }
    foreach ($kind in $kinds) {
        try {
            $endpoint = if ($kind -ceq 'TREE') { 'source' } else { 'screenshot' }
            $value = Invoke-IssueReplicateRecordingRequest -Method GET -Path "session/$SessionId/$endpoint" `
                -MaxBytes 192KB -TimeoutSeconds 5
            if ($value -isnot [string] -or [string]::IsNullOrWhiteSpace($value)) {
                throw "Appium returned no diagnostic $kind."
            }
            if ($kind -ceq 'TREE') {
                try {
                    foreach ($observation in @(Get-IssueReplicateGalleryObservation -Source $value -Phase $Phase)) {
                        if ($null -ne $Observations) { $Observations.Add($observation) }
                        Write-Host $observation
                    }
                }
                catch {
                    $message = $_.Exception.Message -replace '##vso\[[^]]*\]', '' -replace '[\x00-\x1f\x7f]', ' '
                    Write-Warning "Unqualified gallery diagnostic unavailable: $($message.Substring(0, [Math]::Min(900, $message.Length)))"
                }
                if ($GalleryOnly) { continue }
                $length = [Math]::Min(8192, $value.Length)
                if ([char]::IsHighSurrogate($value[$length - 1])) { $length-- }
                $bytes = [Text.Encoding]::UTF8.GetBytes($value.Substring(0, $length))
                if ($length -lt $value.Length) { Write-Warning 'The diagnostic native tree was truncated; it is not a complete hierarchy.' }
            }
            else {
                if ($value -cnotmatch '^[A-Za-z0-9+/]+={0,2}$') { throw 'The diagnostic screenshot is not base64.' }
                $bytes = [Convert]::FromBase64String($value)
                if ($bytes.Length -lt 24 -or
                    [Convert]::ToHexString($bytes[0..7]) -cne '89504E470D0A1A0A') {
                    throw 'The diagnostic screenshot is not a PNG.'
                }
            }
            Write-IssueReplicateDiagnosticBytes -Kind $kind -Phase $Phase -Bytes $bytes
        }
        catch {
            $message = $_.Exception.Message -replace '##vso\[[^]]*\]', '' -replace '[\x00-\x1f\x7f]', ' '
            Write-Warning "Unqualified native $kind diagnostic unavailable: $($message.Substring(0, [Math]::Min(900, $message.Length)))"
        }
    }
}

function Get-IssueReplicateGalleryObservation {
    param(
        [Parameter(Mandatory)][string]$Source,
        [Parameter(Mandatory)][ValidateSet('START', 'STOP')][string]$Phase
    )

    if ($Source.Length -gt 192KB) { throw 'The native gallery source exceeds its existing diagnostic bound.' }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $settings.MaxCharactersInDocument = 192KB
    $text = [IO.StringReader]::new($Source)
    $reader = [Xml.XmlReader]::Create($text, $settings)
    try {
        $document = [Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
        $hash = [Convert]::ToHexString(
            [Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($Source))).ToLowerInvariant()
        foreach ($id in @('SearchBar', 'GoToTestButton')) {
            $nodes = @($document.SelectNodes(
                    "//*[@resource-id='com.microsoft.maui.uitests:id/$id' or @content-desc='$id' or @name='$id']"))
            foreach ($node in @($nodes | Select-Object -First 2)) {
                $attributes = [ordered]@{}
                $truncated = [Collections.Generic.List[string]]::new()
                foreach ($name in @('class', 'type', 'text', 'name', 'label', 'value', 'bounds',
                        'x', 'y', 'width', 'height', 'enabled', 'displayed', 'visible')) {
                    if (-not $node.HasAttribute($name)) { continue }
                    $value = $node.GetAttribute($name)
                    if ($value.Length -gt 512) {
                        $length = if ([char]::IsHighSurrogate($value[511])) { 511 } else { 512 }
                        $value = $value.Substring(0, $length)
                        $truncated.Add($name)
                    }
                    $attributes[$name] = $value
                }
                $record = [ordered]@{
                    qualified = $false; phase = $Phase; automationId = $id; matchingNodes = $nodes.Count
                    sourceSha256 = $hash; truncatedAttributes = @($truncated); attributes = $attributes
                } | ConvertTo-Json -Depth 5 -Compress
                $record = $record -replace '##vso\[[^]]*\]', ''
                if ([Text.Encoding]::UTF8.GetByteCount($record) -gt 4096) {
                    throw 'The native gallery observation exceeds its diagnostic line bound.'
                }
                "ISSUE_REPLICATE_GALLERY_DIAGNOSTIC=$record"
            }
        }
    }
    finally {
        $reader.Dispose()
        $text.Dispose()
    }
}

function Get-IssueReplicateRecordingSessionId {
    param([Parameter(Mandatory)][string]$LogPath)

    $log = Read-IssueReplicateNativeDiagnostic -Path $LogPath -MaxBytes 256KB -Tail -AppiumLog `
        -TimeoutSeconds 4
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
        $options.videoSize = '400x712'
    }
    else {
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

function Convert-IssueReplicateRecordingBudget {
    param(
        [Parameter(Mandatory)][byte[]]$Bytes,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][ValidateRange(24576, 524288)][int]$MaxBytes
    )

    Assert-IssueReplicateRecordingBytes -Bytes $Bytes
    $directory = Join-Path ([IO.Path]::GetTempPath()) "issue-recording-encode-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $directory -ErrorAction Stop | Out-Null
    try {
        $source = Join-Path $directory 'source.mp4'
        $encoded = Join-Path $directory 'transport.mp4'
        [IO.File]::WriteAllBytes($source, $Bytes)
        function Read-VideoIdentity {
            param([string]$Path)
            $ffprobe = Get-Command ffprobe -CommandType Application -ErrorAction Stop | Select-Object -First 1
            $probe = Invoke-IssueReplicateBoundedProcess -FilePath $ffprobe.Source -TimeoutSeconds 20 `
                -Arguments @('-v', 'error', '-select_streams', 'v', '-show_entries',
                'stream=width,height:format=duration', '-of', 'json', $Path) -MaxOutputBytes 8192
            $identity = $probe | ConvertFrom-Json
            if (@($identity.streams).Count -ne 1) { throw 'A recording must have one video stream.' }
            $duration = [double]::Parse([string]$identity.format.duration, [Globalization.CultureInfo]::InvariantCulture)
            if (-not [double]::IsFinite($duration) -or $duration -le 0 -or $duration -gt 31 -or
                $identity.streams[0].width -lt 1 -or $identity.streams[0].width -gt 4096 -or
                $identity.streams[0].height -lt 1 -or $identity.streams[0].height -gt 4096) {
                throw 'The recording duration or dimensions exceed the native capture contract.'
            }
            return @{ Duration = $duration; Width = [int]$identity.streams[0].width }
        }
        $original = Read-VideoIdentity -Path $source
        $width = [Math]::Min(480, $original.Width)
        $width -= $width % 2
        if ($width -lt 2) { throw 'The recording is too narrow for bounded encoding.' }
        $bitrate = [int][Math]::Floor(($MaxBytes - 16384) * 8 * 0.8 / $original.Duration)
        $arguments = @('-hide_banner', '-loglevel', 'error', '-nostdin', '-y', '-i', $source,
            '-an', '-sn', '-dn', '-vf', "fps=8,scale=${width}:-2:flags=lanczos", '-c:v', 'libx264',
            '-preset', 'veryfast', '-pix_fmt', 'yuv420p', '-b:v', "$bitrate",
            '-passlogfile', (Join-Path $directory 'encode'))
        foreach ($pass in 1..2) {
            $output = if ($pass -eq 1) { @('-f', 'null', '-') } else { @('-movflags', '+faststart', $encoded) }
            $ffmpeg = Get-Command ffmpeg -CommandType Application -ErrorAction Stop | Select-Object -First 1
            Invoke-IssueReplicateBoundedProcess -FilePath $ffmpeg.Source -TimeoutSeconds 90 `
                -Arguments ($arguments + @('-pass', "$pass") + $output) -MaxOutputBytes 8192 | Out-Null
        }
        $file = Get-Item -LiteralPath $encoded -ErrorAction Stop
        if ($file.PSIsContainer -or $file.Attributes -band [IO.FileAttributes]::ReparsePoint -or
            $file.Length -lt 24 -or $file.Length -gt $MaxBytes) {
            throw 'The full-duration recording still exceeds the fixed transport byte budget.'
        }
        $converted = Read-VideoIdentity -Path $encoded
        if ([Math]::Abs($converted.Duration - $original.Duration) -gt 0.25 -or
            $converted.Width -ne $width) {
            throw 'Recording encoding changed its duration or requested readable width.'
        }
        $result = [IO.File]::ReadAllBytes($file.FullName)
        Assert-IssueReplicateRecordingBytes -Bytes $result
        return @{
            Bytes = $result; Width = $width; SourceBytes = $Bytes.Length
            SourceSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
        }
    }
    finally {
        Remove-Item -LiteralPath $directory -Recurse -Force
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
    return , $bytes
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
    return , $bytes
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
    }
    finally {
        if ($response) { $response.Dispose() }
        $request.Dispose()
        $deadline.Dispose()
        $client.Dispose()
        $handler.Dispose()
        $token = $null
    }
}
