function Get-RegressionSourceEvidence {
    param(
        [Parameter(Mandatory)]$Context
    )

    # Issue text selects fixed paths, never an API endpoint, ref, or executable.
    $text = "$($Context.issue.title)`n$($Context.issue.body)"
    $hybridWebView = $text -match 'HybridWebView|EvaluateJavaScriptAsync'
    $groups = [ordered]@{
        'HybridWebView|EvaluateJavaScriptAsync' = @(
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.cs',
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.Android.cs',
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.iOS.cs',
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.Windows.cs'
        )
        'Material3|MauiMaterial|ThemeOverlay' = @(
            'src/Core/src/Platform/Android/MauiAppCompatActivity.cs',
            'src/Core/src/Platform/Android/Resources/values/styles-material3.xml',
            'src/Core/src/RuntimeFeature.cs'
        )
        'SwipeView' = @(
            'src/Core/src/Platform/Windows/SwipeViewExtensions.cs',
            'src/Core/src/Handlers/SwipeView/SwipeViewHandler.Windows.cs'
        )
        'OnScrollViewerFound|RemainingItemsThresholdReached|ItemsViewHandler\.Windows' = @(
            'src/Controls/src/Core/Handlers/Items/ItemsViewHandler.Windows.cs'
        )
        'SourceGen|TypedBinding|compiled binding' = @(
            'src/Controls/src/SourceGen/CompiledBindingMarkup.cs',
            'src/Controls/src/Core/TypedBinding.cs'
        )
        'RefreshView|AlwaysBounceVertical' = @(
            'src/Core/src/Platform/iOS/MauiRefreshView.cs',
            'src/Core/src/Handlers/RefreshView/RefreshViewHandler.iOS.cs'
        )
        'WebView' = @(
            'src/Core/src/Handlers/WebView/WebViewHandler.cs',
            'src/Core/src/Handlers/WebView/WebViewHandler.Android.cs',
            'src/Core/src/Handlers/WebView/WebViewHandler.iOS.cs',
            'src/Core/src/Handlers/WebView/WebViewHandler.Windows.cs',
            'src/Core/src/Platform/Android/MauiWebView.cs'
        )
        'TitleBar' = @('src/Controls/src/Core/TitleBar/TitleBar.Windows.cs')
    }
    $selected = @(
        foreach ($pattern in $groups.Keys) {
            if ($hybridWebView -and $pattern -eq 'WebView') { continue }
            if ($text -match $pattern) { $groups[$pattern] }
        }
    ) | Select-Object -Unique
    $paths = @($selected | Select-Object -First 6)
    $evidence = [ordered]@{
        repository = 'dotnet/maui'
        collector = 'trusted-pre-activation'
        paths = $paths
        pathsTruncated = @($selected).Count -gt 6
        limits = @{ paths = 6; sourceBytes = 64KB; historyPerPath = 10; diffs = 6; patchCharacters = 8000 }
        sources = @()
        history = @()
        diffs = @()
        gaps = [System.Collections.Generic.List[string]]::new()
    }
    if ($Context.preflight.mode -eq 'boundary-only') { return [pscustomobject]$evidence }
    if ($paths.Count -eq 0) {
        $evidence.gaps.Add('No fixed source-path group matched; scoped MCP reads may still be available.')
        return [pscustomobject]$evidence
    }

    foreach ($boundaryName in @('reportedGood', 'reportedBad')) {
        $boundary = $Context.boundaries[$boundaryName]
        if ($boundary.status -ne 'resolved') { continue }
        $sha = [string]$boundary.sha
        if ($sha -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Invalid evidence revision.' }
        foreach ($path in $paths) {
            $endpoint = "repos/dotnet/maui/contents/${path}?ref=$sha"
            try {
                $json = Invoke-GhCommandWithRetry -Arguments @('api', $endpoint) `
                    -Description 'collect fixed-path release source' -AllowNotFound -RequireOutput
                if ($null -eq $json) {
                    $evidence.gaps.Add("Source absent at ${sha}: $path")
                    continue
                }
                $file = $json | ConvertFrom-Json
                if ($file.type -cne 'file' -or $file.path -cne $path -or
                    $file.encoding -cne 'base64' -or $file.sha -cnotmatch '\A[0-9a-f]{40}\z') {
                    throw 'Unexpected source response.'
                }
                if ($file.size -gt 64KB) {
                    $evidence.gaps.Add("Source exceeds 65536-byte limit at ${sha}: $path")
                    continue
                }
                $bytes = [Convert]::FromBase64String($file.content)
                if ($bytes.Length -gt 64KB -or $bytes.Length -ne $file.size) {
                    throw 'Source size does not match bounded metadata.'
                }
                $evidence.sources += [pscustomobject]@{
                    boundary = $boundaryName; revision = $sha; path = $path; blob = $file.sha
                    endpoint = $endpoint; url = "https://github.com/dotnet/maui/blob/$sha/$path"
                    content = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
                }
            } catch {
                $evidence.gaps.Add("Source unavailable at ${sha}: $path ($($_.Exception.Message))")
                Write-Warning "Bounded regression source unavailable: $path"
            }
        }
    }

    $bad = $Context.boundaries.reportedBad
    if ($bad.status -ne 'resolved') { return [pscustomobject]$evidence }
    foreach ($path in $paths) {
        $endpoint = "repos/dotnet/maui/commits?sha=$($bad.sha)&path=$path&per_page=11&page=1"
        try {
            $json = Invoke-GhCommandWithRetry -Arguments @('api', $endpoint) `
                -Description 'collect fixed-path release history' -RequireOutput
            $commits = $json | ConvertFrom-Json -NoEnumerate
            if ($commits -isnot [array]) { throw 'Invalid path history.' }
            $entries = @($commits | Select-Object -First 10 | ForEach-Object {
                if ($_.sha -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Invalid history revision.' }
                [pscustomobject]@{
                    sha = $_.sha; url = "https://github.com/dotnet/maui/commit/$($_.sha)"
                    subject = ($_.commit.message -split '\r?\n')[0]
                    parents = @($_.parents | ForEach-Object {
                        if ($_.sha -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Invalid history parent.' }
                        $_.sha
                    })
                }
            })
            $evidence.history += [pscustomobject]@{
                revision = $bad.sha; path = $path; endpoint = $endpoint
                truncated = $commits.Count -gt 10; followsRenames = $false; commits = $entries
            }
        } catch {
            $evidence.gaps.Add("History unavailable: $path ($($_.Exception.Message))")
            Write-Warning "Bounded regression history unavailable: $path"
        }
    }

    $candidates = @($evidence.history | ForEach-Object { $_.commits } |
        Select-Object -ExpandProperty sha -Unique | Select-Object -First 6)
    foreach ($sha in $candidates) {
        try {
            $endpoint = "repos/dotnet/maui/commits/${sha}?per_page=100&page=1"
            $json = Invoke-GhCommandWithRetry -Arguments @('api', $endpoint) `
                -Description 'collect bounded path commit diff' -RequireOutput
            $commit = $json | ConvertFrom-Json
            if ($commit.sha -cne $sha -or $commit.files -isnot [array]) { throw 'Invalid commit diff.' }
            $evidence.diffs += [pscustomobject]@{
                sha = $sha; endpoint = $endpoint
                filesPossiblyTruncated = $commit.files.Count -ge 100
                files = @($commit.files | Where-Object { $_.filename -cin $paths } | ForEach-Object {
                    $patch = [string]$_.patch
                    [pscustomobject]@{
                        path = $_.filename; previousPath = $_.previous_filename; status = $_.status
                        patchAvailable = $null -ne $_.patch
                        patchTruncated = $patch.Length -gt 8000
                        patch = $patch.Substring(0, [Math]::Min(8000, $patch.Length))
                    }
                })
            }
        } catch {
            $evidence.gaps.Add("Commit diff unavailable: $sha ($($_.Exception.Message))")
            Write-Warning 'A bounded regression commit diff was unavailable.'
        }
    }
    return [pscustomobject]$evidence
}

function Get-RegressionDiagnosticInventory {
    param([Parameter(Mandatory)]$Context)

    $entries = [System.Collections.Generic.List[object]]::new()
    $versions = [System.Collections.Generic.List[object]]::new()
    $texts = @([pscustomobject]@{ url = $Context.issue.url; body = $Context.issue.body }) +
        @($Context.comments | Where-Object { $_.authorType -ceq 'User' })
    $truncated = $false
    foreach ($text in $texts) {
        foreach ($match in [regex]::Matches([string]$text.body,
            '(?<![0-9A-Za-z.])v?\d+\.\d+\.\d+(?:-[0-9A-Za-z][0-9A-Za-z-]*(?:\.[0-9A-Za-z-]+)*)?(?![0-9A-Za-z]|\.[0-9A-Za-z])')) {
            if ($versions.Count -ge 20) { $truncated = $true; break }
            $versions.Add([pscustomobject]@{
                value = $match.Value; mentionedAt = $text.url; status = 'supplemental-unverified'
            })
        }
        # Inventory only public GitHub attachment locations; never fetch a URL.
        foreach ($match in [regex]::Matches([string]$text.body,
            'https://(?:github\.com/(?:user-attachments/(?:assets|files)|dotnet/maui/files)/|user-images\.githubusercontent\.com/)[A-Za-z0-9/_.-]+')) {
            if ($entries.Count -ge 20) {
                $truncated = $true
                # Preserve the earliest ten and latest ten mentions, including corrections.
                $entries.RemoveAt(10)
            }
            $entries.Add([pscustomobject]@{
                url = $match.Value.TrimEnd('.'); mentionedAt = $text.url; status = 'linked-not-downloaded'
            })
        }
    }
    return [pscustomobject]@{
        attachments = @($entries); supplementalVersions = @($versions); truncated = $truncated
    }
}
