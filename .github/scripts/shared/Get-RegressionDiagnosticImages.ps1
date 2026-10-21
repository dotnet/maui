function Get-RegressionDiagnosticImages {
    param(
        [Parameter(Mandatory)]$Inventory,
        [Parameter(Mandatory)][string]$Directory
    )

    $result = [ordered]@{
        images = @()
        gaps = [System.Collections.Generic.List[string]]::new()
        limits = @{ images = 2; bytesPerImage = 512KB; pixels = 4000000; redirects = 2 }
    }
    $eligible = @($Inventory.attachments | Where-Object {
        $_.url -cmatch '\Ahttps://github\.com/user-attachments/assets/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\z'
    } | Sort-Object url -Unique)
    # Keep an early diagnostic and the latest correction, not every attachment.
    $ordered = @($Inventory.attachments | Where-Object { $_.url -cin $eligible.url })
    $selected = @(@($ordered | Select-Object -First 1) + @($ordered | Select-Object -Last 1) |
        Sort-Object url -Unique)
    if ($selected.Count -eq 0) { return [pscustomobject]$result }
    New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    $directoryItem = Get-Item -LiteralPath $Directory
    if ($directoryItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Diagnostic image directory must not be a reparse point.'
    }
    $handler = [Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $handler.UseCookies = $false
    $handler.UseDefaultCredentials = $false
    $client = [Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(20)
    try {
        foreach ($attachment in $selected) {
            $response = $null
            $deadline = [Threading.CancellationTokenSource]::new([TimeSpan]::FromSeconds(20))
            try {
                $uri = [uri]$attachment.url
                for ($redirects = 0; ; $redirects++) {
                    $response = $client.GetAsync($uri,
                        [Net.Http.HttpCompletionOption]::ResponseHeadersRead,
                        $deadline.Token).GetAwaiter().GetResult()
                    if ([int]$response.StatusCode -notin @(301, 302, 303, 307, 308)) { break }
                    if ($redirects -ge 2 -or $null -eq $response.Headers.Location) {
                        throw 'Diagnostic redirect limit exceeded.'
                    }
                    $next = [uri]::new($uri, $response.Headers.Location)
                    if ($next.Scheme -cne 'https' -or -not [string]::IsNullOrEmpty($next.UserInfo) -or
                        -not $next.IsDefaultPort -or
                        $next.Host -cnotmatch '\Agithub-production-user-asset-[0-9a-f]{6}\.s3\.amazonaws\.com\z') {
                        throw 'Diagnostic redirect destination is not an approved GitHub image CDN.'
                    }
                    $response.Dispose()
                    $response = $null
                    $uri = $next
                }
                if (-not $response.IsSuccessStatusCode -or
                    $response.Content.Headers.ContentType.MediaType -cne 'image/png' -or
                    $response.Content.Headers.ContentLength -gt 512KB) {
                    throw 'Diagnostic response is unavailable, oversized, or not PNG.'
                }
                $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
                $buffer = [byte[]]::new(512KB + 1)
                $length = 0
                try {
                    while ($length -lt $buffer.Length) {
                        $count = $stream.ReadAsync($buffer, $length, $buffer.Length - $length,
                            $deadline.Token).GetAwaiter().GetResult()
                        if ($count -eq 0) { break }
                        $length += $count
                    }
                } finally { $stream.Dispose() }
                if ($length -gt 512KB -or $length -lt 24 -or
                    [Convert]::ToHexString($buffer[0..7]) -cne '89504E470D0A1A0A' -or
                    [Text.Encoding]::ASCII.GetString($buffer, 12, 4) -cne 'IHDR') {
                    throw 'Diagnostic PNG signature or byte limit is invalid.'
                }
                $width = [long]$buffer[16] * 16777216 + [long]$buffer[17] * 65536 +
                    [long]$buffer[18] * 256 + $buffer[19]
                $height = [long]$buffer[20] * 16777216 + [long]$buffer[21] * 65536 +
                    [long]$buffer[22] * 256 + $buffer[23]
                if ($width -le 0 -or $height -le 0 -or $width -gt 4096 -or $height -gt 4096 -or
                    $width * $height -gt 4000000) { throw 'Diagnostic image dimensions exceed the limit.' }
                [byte[]]$bytes = $buffer[0..($length - 1)]
                $name = "asset-$($attachment.url.Split('/')[-1]).png"
                $path = Join-Path $Directory $name
                $file = [IO.File]::Open($path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
                try { $file.Write($bytes, 0, $bytes.Length) } finally { $file.Dispose() }
                $result.images += [pscustomobject]@{
                    url = $attachment.url; mentionedAt = $attachment.mentionedAt
                    path = "diagnostics/$name"; bytes = $length; width = $width; height = $height
                    sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
                    status = 'available-not-yet-interpreted'
                }
            } catch {
                $reason = if ($_.Exception.Message -cin @(
                    'Diagnostic redirect limit exceeded.',
                    'Diagnostic redirect destination is not an approved GitHub image CDN.',
                    'Diagnostic response is unavailable, oversized, or not PNG.',
                    'Diagnostic PNG signature or byte limit is invalid.',
                    'Diagnostic image dimensions exceed the limit.'
                )) { $_.Exception.Message } else {
                    "PNG transfer/storage failed ($($_.Exception.GetType().Name)); signed redirect URLs are not retained."
                }
                $result.gaps.Add("Static PNG unavailable: $($attachment.url). $reason")
                Write-Warning $reason
            } finally {
                if ($null -ne $response) { $response.Dispose() }
                $deadline.Dispose()
            }
        }
    } finally { $client.Dispose() }
    return [pscustomobject]$result
}
