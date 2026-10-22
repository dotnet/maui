function Test-RegressionPriorReport {
    param([AllowEmptyString()][string]$Body)

    return $Body -match '<!-- (?:Issue Regression Trace -->|issue-replicate-result:|issue-replicate-report-complete)|(?m)^## (?:Regression Trace|MauiBot AI Summary)\b'
}

function Get-RegressionDiagnosticText {
    param([Parameter(Mandatory)]$Context)

    [pscustomobject]@{
        url = $Context.issue.url; body = $Context.issue.body
        author = $Context.issue.author; updatedAt = $Context.issue.updatedAt
    }
    $Context.comments | Where-Object {
        $_.authorType -ceq 'User' -and -not (Test-RegressionPriorReport -Body ([string]$_.body))
    }
}

function Get-RegressionSourceExcerpt {
    param(
        [Parameter(Mandatory)][string]$Content,
        [Parameter(Mandatory)][string[]]$Patterns
    )

    $lines = $Content -split '\r?\n'
    $ranges = [System.Collections.Generic.List[object]]::new()
    foreach ($pattern in $Patterns) {
        for ($index = 0; $index -lt $lines.Count; $index++) {
            if ($lines[$index] -cmatch $pattern) {
                $ranges.Add([pscustomobject]@{
                        start = [Math]::Max(0, $index - 12)
                        end   = [Math]::Min($lines.Count - 1, $index + 65)
                    })
                break
            }
        }
    }
    $merged = [System.Collections.Generic.List[object]]::new()
    foreach ($range in @($ranges | Sort-Object start)) {
        if ($merged.Count -gt 0 -and $range.start -le $merged[-1].end + 1) {
            $merged[-1].end = [Math]::Max($merged[-1].end, $range.end)
        }
        else {
            $merged.Add($range)
        }
    }
    $remainingLines = 240
    $remainingBytes = 16KB
    $excerpts = @(
        foreach ($range in $merged) {
            $selected = [System.Collections.Generic.List[string]]::new()
            for ($index = $range.start; $index -le $range.end -and $remainingLines -gt 0; $index++) {
                $length = [Text.Encoding]::UTF8.GetByteCount($lines[$index]) + 1
                if ($length -gt $remainingBytes) { break }
                $selected.Add($lines[$index])
                $remainingBytes -= $length
                $remainingLines--
            }
            if ($selected.Count -gt 0) {
                [pscustomobject]@{
                    startLine = $range.start + 1; endLine = $range.start + $selected.Count
                    content = $selected -join "`n"
                }
            }
        }
    )
    return $excerpts
}

function Get-RegressionSourceEvidence {
    param(
        [Parameter(Mandatory)]$Context
    )

    # Issue text selects fixed paths, never an API endpoint, ref, or executable.
    $text = "$($Context.issue.title)`n" + (@(Get-RegressionDiagnosticText -Context $Context |
                Select-Object -ExpandProperty body) -join "`n")
    $platformText = "$($Context.issue.fields['Affected platforms'])`n$($Context.issue.labels -join ' ')`n$($Context.issue.title)"
    $platforms = @(
        if ($platformText -match '\bAndroid\b') { 'Android' }
        if ($platformText -match '\biOS\b|\bMac\s*Catalyst\b|\bMacCatalyst\b|\bmacOS\b') { 'Apple' }
        if ($platformText -match '\bWindows\b|\bWinUI\b') { 'Windows' }
    )
    $hybridWebView = $text -match 'HybridWebView'
    $resourceSourceGen = $text -match 'SourceGen|MauiXamlInflator' -and
    $text -match 'DynamicResource|StaticResource|ResourceDictionary|lazy resource'
    $groups = [ordered]@{
        'Resizetizer|Svg\.Skia|SkiaSharp|\bSVG\b'                                                                                = @(
            'eng/Versions.props',
            'eng/Version.Details.xml',
            'src/SingleProject/Resizetizer/src/Resizetizer.csproj',
            'src/SingleProject/Resizetizer/src/SkiaSharpSvgTools.cs',
            'src/SingleProject/Resizetizer/src/SkiaSharpTools.cs'
        )
        'TitleView|NavigationPage'                                                                                               = @(
            'src/Controls/src/Core/Compatibility/Handlers/NavigationPage/iOS/NavigationRenderer.cs'
        )
        'SafeAreaEdges|ApplyCellSafeAreaOverride'                                                                                = @(
            'src/Core/src/Platform/iOS/MauiView.cs',
            'src/Controls/src/Core/Handlers/Items2/iOS/TemplatedCell2.cs',
            'src/Controls/src/Core/Handlers/Items/iOS/TemplatedCell.cs',
            'src/Core/src/Platform/iOS/WrapperView.cs',
            'src/Core/src/Platform/ElementExtensions.cs',
            'src/Core/src/Handlers/View/ViewHandler.cs'
        )
        'CollectionView|CarouselView|SelectionMode|OnScrollViewerFound|RemainingItemsThresholdReached|ItemsViewHandler\.Windows' = @(
            'src/Controls/src/Core/Handlers/Items/SelectableItemsViewHandler.Windows.cs',
            'src/Controls/src/Core/Handlers/Items/ItemsViewHandler.Windows.cs'
        )
        'HybridWebView'                                                                                                          = @(
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.cs',
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.iOS.cs',
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.Android.cs',
            'src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.Windows.cs'
        )
        'Material3|MauiMaterial|ThemeOverlay'                                                                                    = @(
            'src/Core/src/Platform/Android/MauiAppCompatActivity.cs',
            'src/Core/src/Platform/Android/Resources/values/styles-material3.xml',
            'src/Core/src/RuntimeFeature.cs'
        )
        'SwipeView'                                                                                                              = @(
            'src/Core/src/Platform/Android/MauiSwipeView.cs',
            'src/Core/src/Handlers/SwipeView/SwipeViewHandler.Android.cs',
            'src/Core/src/Platform/iOS/MauiSwipeView.cs',
            'src/Core/src/Handlers/SwipeView/SwipeViewHandler.iOS.cs',
            'src/Core/src/Platform/iOS/SwipeViewExtensions.cs',
            'src/Core/src/Platform/Windows/SwipeViewExtensions.cs',
            'src/Core/src/Handlers/SwipeView/SwipeViewHandler.Windows.cs',
            'src/Core/src/Handlers/SwipeView/SwipeViewHandler.cs'
        )
        'SourceGen resources'                                                                                                    = @(
            'src/Controls/src/Core/ResourceDictionary.cs',
            'src/Controls/src/SourceGen/SetPropertyHelpers.cs',
            'src/Controls/src/SourceGen/NodeSGExtensions.cs',
            'src/Controls/src/SourceGen/Visitors/SetResourcesVisitor.cs',
            'src/Controls/src/SourceGen/Visitors/SetPropertiesVisitor.cs',
            'src/Controls/src/Xaml/MarkupExtensions/DynamicResourceExtension.cs'
        )
        'SourceGen|TypedBinding|compiled binding'                                                                                = @(
            'src/Controls/src/SourceGen/CompiledBindingMarkup.cs',
            'src/Controls/src/Core/TypedBinding.cs'
        )
        'RefreshView|AlwaysBounceVertical'                                                                                       = @(
            'src/Core/src/Platform/iOS/MauiRefreshView.cs',
            'src/Core/src/Handlers/RefreshView/RefreshViewHandler.iOS.cs',
            'src/Core/src/Handlers/RefreshView/RefreshViewHandler.Android.cs',
            'src/Core/src/Handlers/RefreshView/RefreshViewHandler.Windows.cs',
            'src/Core/src/Handlers/RefreshView/RefreshViewHandler.cs'
        )
        'WebView'                                                                                                                = @(
            'src/Core/src/Platform/Android/MauiWebView.cs',
            'src/Core/src/Handlers/WebView/WebViewHandler.Android.cs',
            'src/Core/src/Platform/Android/WebViewExtensions.cs',
            'src/Core/src/Handlers/WebView/WebViewHandler.iOS.cs',
            'src/Core/src/Handlers/WebView/WebViewHandler.Windows.cs',
            'src/Core/src/Handlers/WebView/WebViewHandler.cs'
        )
        'TitleBar'                                                                                                               = @('src/Controls/src/Core/TitleBar/TitleBar.Windows.cs')
    }
    $patterns = @($groups.Keys | Sort-Object -Stable -Property @{
            Expression = {
                if ($_ -eq 'SourceGen resources') {
                    $resourceSourceGen -and $Context.issue.title -match 'SourceGen|DynamicResource|ResourceDictionary'
                }
                else { $Context.issue.title -match $_ }
            }
            Descending = $true
        })
    $selected = @(
        foreach ($pattern in $patterns) {
            if ($hybridWebView -and $pattern -eq 'WebView') { continue }
            if ($resourceSourceGen -and $pattern -eq 'SourceGen|TypedBinding|compiled binding') { continue }
            if (($pattern -eq 'SourceGen resources' -and $resourceSourceGen) -or $text -match $pattern) {
                foreach ($path in $groups[$pattern]) {
                    $platform = if ($path -match '/Android/|\.Android\.cs\z') { 'Android' }
                    elseif ($path -match '/iOS/|\.iOS\.cs\z') { 'Apple' }
                    elseif ($path -match '/Windows/|\.Windows\.cs\z') { 'Windows' }
                    else { $null }
                    if ($null -eq $platform -or $platforms.Count -eq 0 -or $platform -in $platforms) {
                        $path
                    }
                }
            }
        }
    ) | Select-Object -Unique
    $symbolPatterns = @{
        'src/Controls/src/Core/Compatibility/Handlers/NavigationPage/iOS/NavigationRenderer.cs' = @(
            '^\s*Container CreateTitleViewContainer\(',
            '^\s*class Container\b',
            '^\s*void InitializeContainer\(',
            '^\s*nfloat ToolbarHeight\b'
        )
    }
    $paths = @($selected | Select-Object -First 6)
    $evidence = [ordered]@{
        repository        = 'dotnet/maui'
        collector         = 'trusted-pre-activation'
        platforms         = $platforms
        boundarySelection = $Context.investigation.selection
        paths             = $paths
        pathsTruncated    = @($selected).Count -gt 6
        limits            = @{
            paths = 6; sourceBytes = 64KB; excerptInputBytes = 256KB; excerptBytes = 16KB
            excerptLines = 240; historyPerPath = 10; diffs = 6; patchCharacters = 8000
        }
        sources           = @()
        history           = @()
        diffs             = @()
        gaps              = [System.Collections.Generic.List[string]]::new()
    }
    if ($Context.preflight.mode -eq 'boundary-only') { return [pscustomobject]$evidence }
    if ($paths.Count -eq 0) {
        $evidence.gaps.Add('No fixed source-path group matched; scoped MCP reads may still be available.')
        return [pscustomobject]$evidence
    }

    foreach ($boundaryName in @('good', 'bad')) {
        $boundary = $Context.investigation.$boundaryName
        if ($boundary.status -notin @('resolved', 'mapped-source')) { continue }
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
                $size = 0L
                if (-not [long]::TryParse([string]$file.size, [ref]$size) -or $size -lt 0) {
                    throw 'Invalid source size.'
                }
                $hasExcerpts = $symbolPatterns.ContainsKey($path)
                $inputLimit = if ($hasExcerpts) { 256KB } else { 64KB }
                if ($size -gt $inputLimit) {
                    $evidence.gaps.Add("Source exceeds $inputLimit-byte input limit at ${sha}: $path")
                    continue
                }
                $bytes = [Convert]::FromBase64String($file.content)
                if ($bytes.Length -gt $inputLimit -or $bytes.Length -ne $size) {
                    throw 'Source size does not match bounded metadata.'
                }
                $content = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
                $excerpts = @()
                if ($size -gt 64KB) {
                    $excerpts = @(Get-RegressionSourceExcerpt -Content $content -Patterns $symbolPatterns[$path])
                    if ($excerpts.Count -eq 0) { throw 'No trusted source symbols matched the oversized file.' }
                    $evidence.gaps.Add("Only bounded symbol excerpts captured at ${sha}: $path; the full file is not included.")
                }
                $evidence.sources += [pscustomobject]@{
                    boundary = $boundaryName; revision = $sha; path = $path; blob = $file.sha
                    boundaryStatus = $boundary.status; origin = $boundary.origin; sizeBytes = $size
                    endpoint = $endpoint; url = "https://github.com/dotnet/maui/blob/$sha/$path"
                    representation = if ($size -le 64KB) { 'complete-file' } else { 'symbol-excerpts' }
                    complete       = $size -le 64KB
                    content        = if ($size -le 64KB) { $content } else { $null }
                    excerpts       = $excerpts
                }
            }
            catch {
                $evidence.gaps.Add("Source unavailable at ${sha}: $path ($($_.Exception.Message))")
                Write-Warning "Bounded regression source unavailable: $path"
            }
        }
    }

    $bad = $Context.investigation.bad
    if ($bad.status -notin @('resolved', 'mapped-source')) { return [pscustomobject]$evidence }
    $comparison = $Context.investigation.comparison
    $comparedCommits = @{}
    $hasBadSideComparison = $null -ne $comparison -and $comparison.status -in @('ahead', 'diverged')
    if ($hasBadSideComparison) {
        foreach ($commit in $comparison.commits) { $comparedCommits[[string]$commit.sha] = $true }
    }
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
                        inComparedBadHistory = $comparedCommits.ContainsKey([string]$_.sha)
                        inForwardComparison  = $null -ne $comparison -and $comparison.isForwardRange -and
                        $comparedCommits.ContainsKey([string]$_.sha)
                        subject              = ($_.commit.message -split '\r?\n')[0]
                        parents              = @($_.parents | ForEach-Object {
                                if ($_.sha -cnotmatch '\A[0-9a-f]{40}\z') { throw 'Invalid history parent.' }
                                $_.sha
                            })
                    }
                })
            $evidence.history += [pscustomobject]@{
                revision = $bad.sha; path = $path; endpoint = $endpoint
                truncated = $commits.Count -gt 10; followsRenames = $false; commits = $entries
            }
        }
        catch {
            $evidence.gaps.Add("History unavailable: $path ($($_.Exception.Message))")
            Write-Warning "Bounded regression history unavailable: $path"
        }
    }

    $candidates = [System.Collections.Generic.List[string]]::new()
    $passes = if ($hasBadSideComparison) { @($true, $false) } else { @($false) }
    foreach ($inRange in $passes) {
        if (-not $inRange -and $null -ne $comparison -and $comparison.isForwardRange -and
            -not $comparison.commitsTruncated) { continue }
        $queues = @($evidence.history | ForEach-Object {
                , @($_.commits | Where-Object { $_.inComparedBadHistory -eq $inRange })
            })
        for ($index = 0; $index -lt 10 -and $candidates.Count -lt 6; $index++) {
            foreach ($queue in $queues) {
                if ($index -lt $queue.Count -and -not $candidates.Contains($queue[$index].sha)) {
                    $candidates.Add($queue[$index].sha)
                    if ($candidates.Count -eq 6) { break }
                }
            }
        }
    }
    $evidence['diffSelection'] = if ($null -ne $comparison -and $comparison.isForwardRange) {
        'visible-forward-range-first; round-robin across paths'
    }
    elseif ($hasBadSideComparison) {
        'visible-compared-bad-history-first; not a forward interval; round-robin across paths'
    }
    else { 'round-robin across paths; no verified forward range' }
    $evidence['diffsTruncated'] = @($evidence.history | ForEach-Object { $_.commits } | Where-Object {
            $null -eq $comparison -or -not $comparison.isForwardRange -or
            $comparison.commitsTruncated -or $_.inForwardComparison
        } |
            Select-Object -ExpandProperty sha -Unique).Count -gt $candidates.Count
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
                files                  = @($commit.files | Where-Object {
                        $_.filename -cin $paths -or $_.previous_filename -cin $paths
                    } | ForEach-Object {
                        $patch = [string]$_.patch
                        [pscustomobject]@{
                            path = $_.filename; previousPath = $_.previous_filename; status = $_.status
                            patchAvailable = $null -ne $_.patch
                            patchTruncated = $patch.Length -gt 8000
                            patch          = $patch.Substring(0, [Math]::Min(8000, $patch.Length))
                        }
                    })
            }
        }
        catch {
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
    $observations = [System.Collections.Generic.List[object]]::new()
    $texts = @(Get-RegressionDiagnosticText -Context $Context)
    $truncated = $false
    $observationsTruncated = $false
    foreach ($text in $texts) {
        foreach ($line in @(Get-RegressionUnfencedLine -Body ([string]$text.body))) {
            $cells = @($line.Value -split '\|' | ForEach-Object { [regex]::Split($_, '(?<=[.!?])\s+') })
            foreach ($cell in $cells) {
                if ($cell.Length -gt 1024) {
                    $observationsTruncated = $true
                    continue
                }
                $numbers = [regex]::Matches($cell,
                    '(?<![0-9A-Za-z.])v?\d+\.\d+\.\d+(?:-[0-9A-Za-z][0-9A-Za-z-]*(?:\.[0-9A-Za-z-]+)*)?(?![0-9A-Za-z]|\.[0-9A-Za-z])')
                if ($numbers.Count -ne 1 -or
                    $cell.Substring(0, $numbers[0].Index) -match '(?:SDK|Xcode|iOS|Android|macOS|Windows|Visual Studio(?: Code)?)\s*(?:version\s*)?\z') {
                    continue
                }
                # An inability to reproduce is not an observed failing or passing outcome.
                if ($cell -match "\b(?:fail(?:ed|s|ing)?\s+to|unable\s+to|not\s+able\s+to|cannot|can't|could\s+not|couldn't)\s+reproduc(?:e(?:d+|s)?|ing)\b") {
                    continue
                }
                $notWorking = "\b(?:(?:(?:do|does|did)\s+not|don't|doesn't|didn't)\s+work|not\s+working)\b"
                $notReproduced = "\b(?:not\s+reproduc(?:ed+|ible)|(?:(?:do|does|did)\s+not|don't|doesn't|didn't)\s+reproduce)\b"
                $goodText = $cell -replace $notWorking, ''
                $badText = $cell -replace $notReproduced, ''
                $good = $cell -match $notReproduced -or
                $goodText -match '\b(?:works?|worked|pass(?:es|ed)?|last (?:working|good))\b'
                $bad = $cell -match $notWorking -or
                $badText -match '\b(?:reproduced+|reproduces?|reproducible|broken|fail(?:s|ed)?|first (?:bad|failing))\b'
                if ($good -eq $bad) { continue }
                if ($observations.Count -ge 20) {
                    $observationsTruncated = $true
                    $observations.RemoveAt(10)
                }
                $observations.Add([pscustomobject]@{
                        value = $numbers[0].Value.TrimStart('v'); role = if ($good) { 'good' } else { 'bad' }
                        firstObservedBad = $bad -and $cell -match (
                            '\b(?:from|since|starting (?:with|in)|first (?:bad|failing))\s+(?:version\s+)?' +
                            [regex]::Escape($numbers[0].Value) + '\b'
                        )
                        mentionedAt = $text.url; author = $text.author; updatedAt = $text.updatedAt
                        excerpt = $cell.Trim(); status = 'human-reported-unverified'
                    })
            }
        }
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
        versionObservations = @($observations); versionObservationsTruncated = $observationsTruncated
    }
}
