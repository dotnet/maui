#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [ValidateSet('Gather', 'Validate')][string]$Stage = 'Gather',
    [ValidateRange(1, 2147483647)][int]$PrNumber,
    [string]$Repository = 'dotnet/maui',
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$ContextDirectory,
    [string]$AgentOutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-SelectionGh {
    param([string[]]$Arguments)
    $result = & gh @Arguments
    if ($LASTEXITCODE -ne 0) { throw "GitHub evidence request failed (exit $LASTEXITCODE)." }
    return ($result -join "`n")
}

function Read-SelectionJson {
    param([string]$Path, [int]$MaximumBytes = 2097152)
    $file = Get-Item -LiteralPath $Path
    if ($file.PSIsContainer -or $file.LinkType -or $file.Length -gt $MaximumBytes) {
        throw 'Selection input must be a bounded regular file.'
    }
    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable -Depth 30
}

function Get-SelectionPlatforms {
    param([string]$Path)
    if ($Path -match '(?i)TestCases\.Android\.Tests/') { return @('android') }
    if ($Path -match '(?i)TestCases\.iOS\.Tests/') { return @('ios') }
    if ($Path -match '(?i)TestCases\.Mac\.Tests/') { return @('maccatalyst') }
    if ($Path -match '(?i)TestCases\.WinUI\.Tests/') { return @('windows') }
    if ($Path -match '(?i)(^|/)(Android|AndroidNative)(/|$)|\.android\.') { return @('android') }
    if ($Path -match '(?i)(^|/)(MacCatalyst)(/|$)|\.maccatalyst\.') { return @('maccatalyst') }
    if ($Path -match '(?i)(^|/)(iOS)(/|$)|\.ios\.') { return @('ios', 'maccatalyst') }
    if ($Path -match '(?i)(^|/)(Windows|WinUI)(/|$)|\.windows\.') { return @('windows') }
    return @('android', 'ios', 'maccatalyst', 'windows')
}

function Get-DeclaredCategories {
    param([string]$Content)
    $values = @{}
    foreach ($match in [regex]::Matches($Content, 'const\s+string\s+(\w+)\s*=\s*(?:"([^"]+)"|nameof\((\w+)\))')) {
        $values[$match.Groups[1].Value] = if ($match.Groups[2].Success) { $match.Groups[2].Value } else { $match.Groups[3].Value }
    }
    return $values
}

function Get-AttributeCategories {
    param([string]$Content, [hashtable]$Constants)
    $categories = @()
    foreach ($match in [regex]::Matches($Content, '\b(?:ShardedTest)?Category\(\s*(?:nameof\()?(?:UITestCategories|TestCategory)\.(\w+)')) {
        if ($Constants.ContainsKey($match.Groups[1].Value)) { $categories += $Constants[$match.Groups[1].Value] }
    }
    foreach ($match in [regex]::Matches($Content, '\bCategory\(\s*"([A-Za-z0-9_]+)"')) {
        $categories += $match.Groups[1].Value
    }
    return @($categories | Sort-Object -Unique)
}

function Get-ChangedTestCategories {
    param([string]$Patch, [hashtable]$Constants, [string]$Content)
    # Existing class/category attributes are often outside the changed hunk.
    return @(@(Get-AttributeCategories $Patch $Constants) + @(Get-AttributeCategories $Content $Constants) | Sort-Object -Unique)
}

function Get-HeadContent {
    param([string]$Path, [string]$HeadSha)
    $encodedPath = [uri]::EscapeDataString($Path)
    $content = Invoke-SelectionGh @('api', "-H", 'Accept: application/vnd.github.raw+json',
        "repos/dotnet/maui/contents/$encodedPath`?ref=$HeadSha")
    if ($content.Length -gt 180000) { throw 'Changed test source exceeds bounded category inspection.' }
    return [string]$content
}

function Get-UiTestSymbols {
    $projects = [ordered]@{ android = 'Android'; ios = 'iOS'; maccatalyst = 'Mac'; windows = 'WinUI' }
    $symbols = [ordered]@{}
    foreach ($platform in $projects.Keys) {
        $name = $projects[$platform]
        $csproj = Get-Content "src/Controls/tests/TestCases.$name.Tests/Controls.TestCases.$name.Tests.csproj" -Raw
        $match = [regex]::Match($csproj, '<DefineConstants>\$\(DefineConstants\);([^<]+)</DefineConstants>')
        if (-not $match.Success) { throw "Unable to read UI test symbols for $platform." }
        $symbols[$platform] = @($match.Groups[1].Value -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }
    return $symbols
}

function Test-PreprocessorExpression {
    param([string]$Expression, [string[]]$Defined, [string[]]$Known)
    $tokens = @([regex]::Matches($Expression, '\s*(&&|\|\||!|\(|\)|[A-Za-z_]\w*)\s*') | ForEach-Object { $_.Groups[1].Value })
    if (($tokens -join '') -cne ($Expression -replace '\s', '')) { return $null }
    $script:guardTokens = $tokens
    $script:guardIndex = 0
    $parseOr = $null; $parseAnd = $null; $parseUnary = $null
    $parseUnary = {
        if ($script:guardIndex -ge $script:guardTokens.Count) { throw 'incomplete' }
        $token = $script:guardTokens[$script:guardIndex++]
        if ($token -eq '!') { return -not (& $parseUnary) }
        if ($token -eq '(') {
            $value = & $parseOr
            if ($script:guardIndex -ge $script:guardTokens.Count -or $script:guardTokens[$script:guardIndex++] -ne ')') { throw 'unbalanced' }
            return $value
        }
        if ($token -cin @('true', 'false')) { return $token -ceq 'true' }
        if ($token -notmatch '^[A-Za-z_]\w*$' -or $token -cnotin $Known) { throw 'unknown symbol' }
        return $token -cin $Defined
    }
    $parseAnd = {
        $value = & $parseUnary
        while ($script:guardIndex -lt $script:guardTokens.Count -and $script:guardTokens[$script:guardIndex] -eq '&&') {
            $script:guardIndex++; $right = & $parseUnary; $value = $value -and $right
        }
        return $value
    }
    $parseOr = {
        $value = & $parseAnd
        while ($script:guardIndex -lt $script:guardTokens.Count -and $script:guardTokens[$script:guardIndex] -eq '||') {
            $script:guardIndex++; $right = & $parseAnd; $value = $value -or $right
        }
        return $value
    }
    try {
        $result = & $parseOr
        if ($script:guardIndex -ne $script:guardTokens.Count) { return $null }
        return [bool]$result
    } catch { return $null }
}

function Get-GuardedTestPlatforms {
    # Returns the platforms compiling a shared UI test wrapped in one file-wide #if, or $null when not provable.
    param([string]$Content, [System.Collections.Specialized.OrderedDictionary]$Symbols)
    $lines = @($Content -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($lines.Count -lt 2 -or $lines[0] -notmatch '^#if\s+(.+?)\s*(//.*)?$') { return $null }
    $expression = $Matches[1]
    $depth = 0
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^#if\b') { $depth++ }
        elseif ($lines[$i] -match '^#(else|elif)\b' -and $depth -eq 1) { return $null }
        elseif ($lines[$i] -match '^#endif\b') {
            $depth--
            if ($depth -eq 0 -and $i -ne $lines.Count - 1) { return $null }
        }
    }
    if ($depth -ne 0) { return $null }
    $known = @($Symbols.Values | ForEach-Object { $_ } | Sort-Object -Unique)
    $platforms = @()
    foreach ($platform in $Symbols.Keys) {
        $compiled = Test-PreprocessorExpression $expression $Symbols[$platform] $known
        if ($null -eq $compiled) { return $null }
        if ($compiled) { $platforms += $platform }
    }
    return , $platforms
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
if ($Stage -eq 'Gather') {
    if ($Repository -cne 'dotnet/maui') { throw 'This selector is scoped to dotnet/maui.' }
    $pr = Invoke-SelectionGh @('api', "repos/$Repository/pulls/$PrNumber") | ConvertFrom-Json
    if ($pr.state -ne 'open' -or -not $pr.head.repo -or $pr.mergeable -ne $true) {
        throw 'Selection requires an open, mergeable PR with an available source repository. Retry if GitHub is still computing mergeability.'
    }
    $files = @(Invoke-SelectionGh @('api', '--paginate', '--slurp', "repos/$Repository/pulls/$PrNumber/files?per_page=100") |
        ConvertFrom-Json | ForEach-Object { $_ | ForEach-Object { $_ } })
    if ($files.Count -ne $pr.changed_files -or $files.Count -gt 3000) { throw 'PR file evidence is incomplete.' }
    $merge = Invoke-SelectionGh @('api', "repos/$Repository/git/commits/$($pr.merge_commit_sha)") | ConvertFrom-Json
    # pulls.base.sha is the base tip at the PR's last sync; GitHub rebuilds the test-merge on the current base tip.
    if ($merge.parents.Count -ne 2 -or $merge.parents[1].sha -cne $pr.head.sha) {
        throw 'GitHub test-merge does not match the frozen head.'
    }
    $mergeBaseSha = $merge.parents[0].sha
    $baseProgress = Invoke-SelectionGh @('api', "repos/$Repository/compare/$($pr.base.sha)...$mergeBaseSha") | ConvertFrom-Json
    if ($baseProgress.status -notin @('identical', 'ahead')) {
        throw "GitHub's test-merge was built on a base commit outside the PR base history, so CI would test a stale merge. Push to the PR (or rebase it) so GitHub rebuilds the test-merge, then rerun."
    }
    $uiRoot = 'src/Controls/tests/TestCases.Shared.Tests'
    $uiConstants = Get-DeclaredCategories (Get-Content "$uiRoot/UITestCategories.cs" -Raw)
    $uiSymbols = Get-UiTestSymbols
    $projectRoots = [ordered]@{
        'Core.DeviceTests' = 'src/Core/tests/DeviceTests'
        'Controls.DeviceTests' = 'src/Controls/tests/DeviceTests'
        'Graphics.DeviceTests' = 'src/Graphics/tests/DeviceTests'
        'Essentials.DeviceTests' = 'src/Essentials/test/DeviceTests'
        'MauiBlazorWebView.DeviceTests' = 'src/BlazorWebView/tests/DeviceTests'
    }
    $deviceInventories = @{}
    foreach ($project in $projectRoots.Keys) {
        $categoryFile = Join-Path $projectRoots[$project] 'TestCategory.cs'
        $constants = if (Test-Path $categoryFile) { Get-DeclaredCategories (Get-Content $categoryFile -Raw) } else { @{} }
        $deviceInventories[$project] = $constants
    }
    # Known leaf controls are a coverage floor, not permission for AI to omit shared infrastructure.
    $controls = @('ActivityIndicator','Border','BoxView','Button','CheckBox','DatePicker','Editor','Entry','Image','ImageButton',
        'IndicatorView','Label','Picker','ProgressBar','RadioButton','RefreshView','ScrollView','SearchBar','Slider','Stepper',
        'Switch','SwipeView','TimePicker','WebView')
    $evidence = @()
    $floorPlatforms = @()
    $floorUi = @()
    $floorDevice = @()
    $floorProjects = @()
    $broad = $false
    $reasons = @()
    $patchBudget = 180000
    foreach ($file in $files) {
        $paths = @($file.filename)
        if ($file.PSObject.Properties['previous_filename']) { $paths += $file.previous_filename }
        $patch = if ($file.PSObject.Properties['patch']) { [string]$file.patch } else { '' }
        $patchUnavailable = -not $patch -and $file.changes -gt 0
        if ($patch.Length -gt $patchBudget) {
            $patch = ''
            $broad = $true
            $reasons += 'Diff exceeds bounded AI context; broader coverage retained.'
        } else { $patchBudget -= $patch.Length }
        $evidence += @{ paths = $paths; status = $file.status; patch = $patch }
        foreach ($path in $paths) {
            if ($path -match '^(docs/|\.github/docs/)|(?i)\.(md|txt)$') { continue }
            if ($patchUnavailable) {
                $broad = $true
                $reasons += "Patch unavailable for changed content: $path"
            }
            $pathPlatforms = @(Get-SelectionPlatforms $path)
            $current = $file.status -ne 'removed' -and $path -eq $file.filename
            $control = $null
            foreach ($candidate in $controls) {
                if ($path -match "(?i)^src/(Core/src/(Handlers/$candidate/|I$candidate\.|Platform/[^/]+/(Maui)?$candidate\.)|Controls/src/Core/$candidate(?:/|\.))") {
                    $control = $candidate
                    break
                }
            }
            if ($control) {
                $floorUi += $control
                $floorProjects += @('Core.DeviceTests', 'Controls.DeviceTests')
                $floorDevice += $control
                foreach ($project in @('Core.DeviceTests', 'Controls.DeviceTests')) {
                    $testFiles = @(Get-ChildItem -LiteralPath $projectRoots[$project] -Recurse -File -Filter '*.cs' |
                        Where-Object { $_.FullName -match "(?i)/$control/" })
                    foreach ($testFile in $testFiles) {
                        $floorDevice += Get-AttributeCategories (Get-Content $testFile.FullName -Raw) $deviceInventories[$project]
                    }
                }
            } elseif ($current -and $path -match '^src/Controls/tests/TestCases\.(Android|iOS|Mac|WinUI)\.Tests/snapshots/.+/(\w+)\.png$') {
                # A screenshot baseline belongs to the UI test method with the same name.
                $method = $Matches[2]
                $owners = @(Get-ChildItem -LiteralPath $uiRoot -Recurse -File -Filter '*.cs' |
                    Where-Object { (Get-Content $_.FullName -Raw) -match "\bvoid\s+$method\s*\(" } |
                    ForEach-Object { [IO.Path]::GetRelativePath((Get-Location).Path, $_.FullName) -replace '\\', '/' })
                foreach ($changed in @($files | Where-Object { $_.status -ne 'removed' -and $_.filename -match "^$([regex]::Escape($uiRoot))/.+\.cs$" })) {
                    if ($changed.patch -match "\bvoid\s+$method\s*\(") { $owners += $changed.filename }
                }
                $owners = @($owners | Sort-Object -Unique)
                $cats = @()
                if ($owners.Count -eq 1) {
                    try { $cats = @(Get-AttributeCategories (Get-HeadContent $owners[0] $pr.head.sha) $uiConstants) } catch { $cats = @() }
                }
                if ($cats.Count -eq 0) { $broad = $true; $reasons += "Unresolved UI snapshot coverage: $path" }
                $floorUi += $cats
            } elseif ($path -match '^src/Controls/tests/TestCases\.(Shared|Android|iOS|Mac|WinUI)') {
                $cats = @(Get-AttributeCategories $patch $uiConstants)
                if ($current -and $path.EndsWith('.cs', [StringComparison]::OrdinalIgnoreCase)) {
                    $content = Get-HeadContent $path $pr.head.sha
                    $cats = @(Get-ChangedTestCategories $patch $uiConstants $content)
                    if ($path.StartsWith("$uiRoot/")) {
                        $guarded = Get-GuardedTestPlatforms $content $uiSymbols
                        if ($null -ne $guarded) { $pathPlatforms = @($guarded) }
                    }
                }
                $cats = @($cats | Where-Object { $_ })
                if ($cats.Count -eq 0) { $broad = $true; $reasons += "Unresolved UI test coverage: $path" }
                $floorUi += $cats
            } elseif ($current -and $path -match '^src/Controls/tests/TestCases\.HostApp/Issues/') {
                # An issue page only serves the UI test with the same name; reuse that test's categories and platform guard.
                $issueName = ([IO.Path]::GetFileName($path) -split '\.')[0]
                $pairPattern = "^$([regex]::Escape($uiRoot))/Tests/(?:.+/)?$([regex]::Escape($issueName))\.cs$"
                $removedPaths = @($files | Where-Object { $_.status -eq 'removed' } | ForEach-Object { $_.filename })
                $pairs = @(@($files | ForEach-Object { $_.filename }) +
                    @(Get-ChildItem -LiteralPath "$uiRoot/Tests" -Recurse -File -Filter "$issueName.cs" |
                        ForEach-Object { [IO.Path]::GetRelativePath((Get-Location).Path, $_.FullName) -replace '\\', '/' }) |
                    Where-Object { $_ -match $pairPattern -and $_ -notin $removedPaths } | Sort-Object -Unique)
                $cats = @()
                if ($issueName -match '^\w+$' -and $pairs.Count -eq 1) {
                    try {
                        $content = Get-HeadContent $pairs[0] $pr.head.sha
                        $cats = @(Get-AttributeCategories $content $uiConstants)
                        $guarded = Get-GuardedTestPlatforms $content $uiSymbols
                        if ($cats.Count -gt 0 -and $null -ne $guarded) { $pathPlatforms = @($guarded) }
                    } catch { $cats = @() }
                }
                if ($cats.Count -eq 0) { $broad = $true; $reasons += "Unresolved HostApp issue page coverage: $path" }
                $floorUi += $cats
            } elseif ($path -match '^src/(Core|Controls|Graphics|Essentials|BlazorWebView)/tests?/DeviceTests/') {
                $project = @($projectRoots.Keys | Where-Object { $path.StartsWith($projectRoots[$_] + '/') })
                if ($project.Count -ne 1) {
                    $broad = $true
                    $reasons += "Unknown device test project: $path"
                    $floorPlatforms += $pathPlatforms
                    continue
                }
                $floorProjects += $project[0]
                $cats = @(Get-AttributeCategories $patch $deviceInventories[$project[0]])
                if ($current -and $path.EndsWith('.cs', [StringComparison]::OrdinalIgnoreCase)) {
                    $cats = @(Get-ChangedTestCategories $patch $deviceInventories[$project[0]] (Get-HeadContent $path $pr.head.sha))
                }
                $cats = @($cats | Where-Object { $_ })
                foreach ($candidate in $controls) {
                    if ($path -match "(?i)/$candidate/") {
                        if ($candidate -cin $deviceInventories[$project[0]].Values) { $cats += $candidate }
                    }
                }
                if ($cats.Count -eq 0) { $broad = $true; $reasons += "Unresolved device test coverage: $path" }
                $floorDevice += $cats
            } elseif ($path -match '^src/.*/(?:UnitTests|Core\.UnitTests|Xaml\.UnitTests|SourceGen\.UnitTests)/') {
                # These suites remain in the unmodified maui-pr run.
                continue
            } else {
                $broad = $true
                $reasons += "Shared or unmapped runtime/build change: $path"
            }
            $floorPlatforms += $pathPlatforms
        }
    }
    $latest = Invoke-SelectionGh @('api', "repos/$Repository/pulls/$PrNumber") | ConvertFrom-Json
    if ($latest.head.sha -cne $pr.head.sha -or $latest.merge_commit_sha -cne $pr.merge_commit_sha) { throw 'PR changed during evidence collection.' }
    $context = [ordered]@{
        schemaVersion = 1; repository = $Repository; prNumber = $PrNumber
        headSha = $pr.head.sha; baseSha = $mergeBaseSha; mergeSha = $pr.merge_commit_sha
        headRef = $pr.head.ref; baseRef = $pr.base.ref; headRepository = $pr.head.repo.full_name
        floor = @{
            platforms = @($floorPlatforms | Sort-Object -Unique)
            uiCategories = @($floorUi | Sort-Object -Unique)
            deviceProjects = @($floorProjects | Sort-Object -Unique)
            deviceCategories = @($floorDevice | Sort-Object -Unique)
            broad = $broad; reasons = @($reasons | Sort-Object -Unique)
        }
        uiCategories = @($uiConstants.Values | Sort-Object -Unique)
        deviceCategories = @{}
        files = $evidence
    }
    foreach ($project in $projectRoots.Keys) { $context.deviceCategories[$project] = @($deviceInventories[$project].Values | Sort-Object -Unique) }
    $context | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath "$OutputDirectory/context.json" -Encoding utf8
    if ($env:GITHUB_OUTPUT) { Add-Content $env:GITHUB_OUTPUT 'ready=true' }
    return
}

$context = Read-SelectionJson "$ContextDirectory/context.json"
if ($context.repository -cne 'dotnet/maui' -or $context.schemaVersion -ne 1) { throw 'Invalid selection context.' }
$proposal = $null
$warnings = @()
if ($AgentOutputPath -and (Test-Path -LiteralPath $AgentOutputPath)) {
    $output = Read-SelectionJson $AgentOutputPath
    $items = @($output.items | Where-Object { $_.type -eq 'queue_selected_tests' })
    if ($items.Count -ne 1) { throw 'Expected exactly one queue-selected-tests proposal.' }
    $proposal = $items[0]
} else {
    $warnings += 'AI output unavailable; using conservative evidence-based selection.'
}
$floor = $context.floor
$platforms = @($floor.platforms)
$ui = @($floor.uiCategories)
$projects = @($floor.deviceProjects)
$device = @($floor.deviceCategories)
$broad = [bool]$floor.broad
if ($proposal) {
    if ($proposal.ContainsKey('broader') -and $proposal.broader -isnot [bool]) { throw 'broader must be a boolean.' }
    foreach ($key in @('platforms', 'ui_categories', 'device_projects', 'device_categories', 'reason')) {
        if (-not $proposal.ContainsKey($key)) { throw "Proposal is missing $key." }
    }
    foreach ($key in @('platforms', 'ui_categories', 'device_projects', 'device_categories')) {
        if ($proposal[$key] -isnot [string] -or $proposal[$key].Length -gt 16000) { throw 'Expected a bounded JSON-array string.' }
        $values = ConvertFrom-Json -InputObject $proposal[$key] -NoEnumerate -Depth 3
        if ($values -isnot [array] -or @($values | Where-Object { $_ -isnot [string] -or $_ -cnotmatch '^[A-Za-z0-9_.]+$' }).Count) {
            throw 'Selection lists must contain only identifier strings.'
        }
        $proposal[$key] = @($values)
    }
    if ($proposal.platforms.Count -gt 4 -or $proposal.ui_categories.Count -gt 100 -or
        $proposal.device_projects.Count -gt 5 -or $proposal.device_categories.Count -gt 100 -or
        $proposal.reason -isnot [string] -or $proposal.reason.Length -gt 4000) { throw 'Oversized or malformed proposal.' }
    foreach ($platform in $proposal.platforms) {
        if ($platform -cnotin @('android', 'ios', 'maccatalyst', 'windows')) { throw 'Invalid proposed platform.' }
    }
    foreach ($category in $proposal.ui_categories) {
        if ($category -cnotin $context.uiCategories) { throw 'Invalid proposed UI category.' }
    }
    foreach ($project in $proposal.device_projects) {
        if (-not $context.deviceCategories.ContainsKey($project)) { throw 'Invalid proposed device project.' }
    }
    foreach ($category in $proposal.device_categories) {
        if ($category -cnotin @($context.deviceCategories.Values | ForEach-Object { $_ })) { throw 'Invalid proposed device category.' }
    }
    $platforms += $proposal.platforms
    $ui += $proposal.ui_categories
    $projects += $proposal.device_projects
    $device += $proposal.device_categories
    if ($proposal.ContainsKey('broader') -and $proposal.broader -eq $true) { $broad = $true }
    $warnings += 'AI proposal validated and unioned with mandatory coverage; confidence is not a coverage guarantee.'
}
$platforms = @($platforms | Sort-Object -Unique)
$ui = @($ui | Sort-Object -Unique)
$projects = @($projects | Sort-Object -Unique)
$device = @($device | Sort-Object -Unique)
if (@($ui | Where-Object { $_ -cnotin $context.uiCategories }).Count) {
    $broad = $true
    $warnings += 'Changed test categories are absent from the trusted inventory; broader coverage retained.'
}
if ($broad -and $platforms.Count -gt 0) {
    $ui = @()
    $projects = @($context.deviceCategories.Keys | Sort-Object)
    $device = @()
}
foreach ($project in $projects) {
    if (@($device | Where-Object { $_ -cnotin $context.deviceCategories[$project] }).Count -gt 0) {
        $device = @()
        $warnings += 'Device category mismatch between selected projects: running selected projects without category restriction.'
        break
    }
}
$selection = [ordered]@{
    schemaVersion = 1; repository = $context.repository; prNumber = $context.prNumber
    headSha = $context.headSha; baseSha = $context.baseSha; mergeSha = $context.mergeSha
    platforms = $platforms; uiCategories = $ui; deviceProjects = $projects; deviceCategories = $device
    runUi = ($platforms.Count -gt 0 -and ($broad -or $ui.Count -gt 0))
    runDevice = ($platforms.Count -gt 0 -and $projects.Count -gt 0)
    broader = $broad; reasons = @($floor.reasons) + $warnings
}
$selection | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath "$OutputDirectory/selection.json" -Encoding utf8
