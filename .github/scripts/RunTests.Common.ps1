# Shared helpers for `/review run-ui-tests`: AI-selected UI tests for a PR.
#
# The flow is:
#   1. Get-RunTestsContext.ps1   collects the PR diff and the UI test inventory (trusted).
#   2. The agent proposes platforms + categories (copilot-review-run-tests.md).
#   3. Invoke-RunTests.ps1       validates the proposal, pushes a dispatch commit (the PR's
#                                test merge plus the selection) to a run-tests/* branch, whose
#                                CI trigger runs maui-pr-uitests with only the selected tests,
#                                adds "skipped" check runs for everything that was not selected,
#                                a check that follows the run, and posts a comment.
#   4. Update-RunTestsResults.ps1 refreshes the comment when the build finishes.
#
# Everything here is pure (no network) so it can be unit tested.

$script:RunTestsPlatforms = [ordered]@{
    android     = 'Android'
    ios         = 'iOS'
    maccatalyst = 'MacCatalyst'
    windows     = 'Windows'
}

# Pipeline parameter values differ from the user-facing names for MacCatalyst only.
$script:RunTestsPlatformParameterSuffix = @{ android = 'Android'; ios = 'Ios'; maccatalyst = 'Catalyst'; windows = 'Windows' }
$script:RunTestsPipelinePlatform = @{
    android     = 'android'
    ios         = 'ios'
    maccatalyst = 'catalyst'
    windows     = 'windows'
}

# Umbrella categories accepted by eng/pipelines/common/ui-tests.yml, which expands them to CI shards.
$script:RunTestsUmbrellas = [ordered]@{
    CollectionView = 'CollectionView'
    Shell          = 'Shell'
}


# `/review run-ui-tests` selects UI tests (maui-pr-uitests); `/review run-device-tests` selects device
# tests (maui-pr-devicetests). Both share this flow; Set-RunTestsSuite switches the names, markers,
# branches and pipeline files below. Comment markers and check names are suite-specific, so a new
# request only supersedes (minimizes, retires) results of its own suite.
$script:RunTestsSuites = @{
    ui = [ordered]@{
        Command = 'run-ui-tests'; Pipeline = 'maui-pr-uitests'; Kind = 'UI tests'; DefinitionId = 313
        CommentMarker = '<!-- review-run-tests -->'; UnsupportedMarker = '<!-- review-run-tests-unsupported -->'
        DispatchBranchPrefix = 'run-tests/'; RequestPath = 'eng/pipelines/common/run-tests-request.yml'
        PipelinePath = 'eng/pipelines/ci-uitests.yml'
        # Trusted paths of the dispatch commit: the pipeline and the workflows.
        TrustedPaths = @('.github', 'eng/pipelines')
    }
    device = [ordered]@{
        Command = 'run-device-tests'; Pipeline = 'maui-pr-devicetests'; Kind = 'device tests'; DefinitionId = 314
        CommentMarker = '<!-- review-run-devicetests -->'; UnsupportedMarker = '<!-- review-run-devicetests-unsupported -->'
        DispatchBranchPrefix = 'run-devicetests/'; RequestPath = 'eng/pipelines/common/run-devicetests-request.yml'
        PipelinePath = 'eng/pipelines/ci-device-tests.yml'
        # The Helix project and Windows runner apply the selection, so they come from the trusted side too.
        TrustedPaths = @('.github', 'eng/pipelines', 'eng/devices', 'eng/helix_xharness.proj')
    }
}

# `/review run-tests` runs both suites and reports them in one comment, one section per suite.
$script:RunTestsCombined = [ordered]@{
    Command = 'run-tests'
    CommentMarker = '<!-- review-run-tests-all -->'
    UnsupportedMarker = '<!-- review-run-tests-all-unsupported -->'
}

function Set-RunTestsSuite {
    param(
        [Parameter(Mandatory)][ValidateSet('ui', 'device')][string]$Suite,
        # Part of `/review run-tests` (both suites): messages name that command, and a new comment
        # supersedes every earlier run-tests comment of either suite.
        [switch]$Combined
    )
    $profile = $script:RunTestsSuites[$Suite]
    $script:RunTestsSuite = $Suite
    $script:RunTestsCombinedRun = [bool]$Combined
    $script:RunTestsCommand = if ($Combined) { $script:RunTestsCombined.Command } else { $profile.Command }
    $script:RunTestsPipelineName = $profile.Pipeline
    $script:RunTestsKind = $profile.Kind
    $script:RunTestsDefinitionId = $profile.DefinitionId
    $script:RunTestsCommentMarker = $profile.CommentMarker
    $script:RunTestsUnsupportedMarker = $profile.UnsupportedMarker
    $script:RunTestsCheckPrefix = $profile.Pipeline
    $script:RunTestsSelectionCheckName = "$($profile.Pipeline) (test selection)"
    $script:RunTestsSkippedCheckPrefix = "$($profile.Pipeline) (skipped)"
    # Check run on the PR head that follows the dispatched build (whose own checks report on the
    # dispatch commit, not on the PR).
    $script:RunTestsRunCheckName = "$($profile.Pipeline) (selected run)"
    $script:RunTestsDispatchBranchPrefix = $profile.DispatchBranchPrefix
    $script:RunTestsRequestPath = $profile.RequestPath
    $script:RunTestsPipelinePath = $profile.PipelinePath
    $script:RunTestsTrustedPaths = $profile.TrustedPaths
    # Earlier bot comments of these kinds are minimized as outdated when a new comment is posted.
    $script:RunTestsOutdatedMarkers = if ($Combined) {
        @($script:RunTestsCombined.CommentMarker, $script:RunTestsCombined.UnsupportedMarker) +
            @($script:RunTestsSuites.Values | ForEach-Object { $_.CommentMarker, $_.UnsupportedMarker })
    } else {
        @($profile.CommentMarker, $profile.UnsupportedMarker)
    }
}

function Get-RunTestsCommentSuite {
    <#
    .SYNOPSIS
    The suite a bot comment belongs to ('ui', 'device', or 'all' for a `/review run-tests` comment
    with a section per suite), or $null for any other comment.
    #>
    param([AllowNull()][string]$Body)
    if (-not $Body) { return $null }
    if ($Body.StartsWith($script:RunTestsCombined.CommentMarker)) { return 'all' }
    foreach ($suite in 'ui', 'device') { if ($Body.StartsWith($script:RunTestsSuites[$suite].CommentMarker)) { return $suite } }
    return $null
}

function Select-RunTestsOutdatedComments {
    <#
    .SYNOPSIS
    The earlier `/review run-ui-tests` comments on a PR that a newly posted one supersedes.
    #>
    param([AllowNull()]$Comments, [Parameter(Mandatory)][long]$KeepId)
    $outdated = foreach ($comment in @($Comments | Where-Object { $_ })) {
        if ([long]$comment.id -eq $KeepId -or $comment.user.type -ne 'Bot' -or -not $comment.node_id -or -not $comment.body) { continue }
        $body = [string]$comment.body
        if (@($script:RunTestsOutdatedMarkers | Where-Object { $body.StartsWith($_) }).Count) { $comment }
    }
    return @($outdated)
}

function Hide-RunTestsOutdatedComments {
    <#
    .SYNOPSIS
    Minimizes, as outdated, the earlier `/review run-ui-tests` comments on a PR once a new one is posted.
    A failure only warns: the new comment is already up.
    #>
    param([Parameter(Mandatory)][string]$Repository, [Parameter(Mandatory)][string]$PrNumber, [Parameter(Mandatory)][long]$KeepId)
    $raw = & gh api --paginate --slurp "repos/$Repository/issues/$PrNumber/comments?per_page=100"
    if ($LASTEXITCODE -ne 0) { Write-Warning "Could not list the comments of #$PrNumber."; return }
    $comments = @(($raw -join "`n") | ConvertFrom-Json -Depth 20 | ForEach-Object { $_ })
    $query = 'mutation($id: ID!) { minimizeComment(input: { subjectId: $id, classifier: OUTDATED }) { minimizedComment { isMinimized } } }'
    foreach ($comment in Select-RunTestsOutdatedComments $comments -KeepId $KeepId) {
        & gh api graphql -f query=$query -f id=$($comment.node_id) | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Warning "Could not minimize $($comment.html_url)." }
        else { Write-Host "Minimized outdated $($comment.html_url)." }
    }
}

function Get-RunTestsPlatformIds { return @($script:RunTestsPlatforms.Keys) }

function Get-RunTestsPlatformName {
    param([Parameter(Mandatory)][string]$Platform)
    if (-not $script:RunTestsPlatforms.Contains($Platform)) { throw "Unknown platform '$Platform'." }
    return $script:RunTestsPlatforms[$Platform]
}

function Get-RunTestsInventory {
    <#
    .SYNOPSIS
    Reads the UI test categories CI can actually run from eng/pipelines/common/ui-tests.yml.
    Returns the individual categories in pipeline order, plus Material3 and the umbrellas.
    #>
    param([Parameter(Mandatory)][string]$PipelineYamlPath)

    $lines = Get-Content -LiteralPath $PipelineYamlPath
    $inGroups = $false
    $categories = [System.Collections.Generic.List[string]]::new()
    foreach ($line in $lines) {
        if ($line -match '^\s*-?\s*name:\s*categoryGroupsToTest\s*$' -or $line -match '^\s*categoryGroupsToTest:\s*$') {
            $inGroups = $true
            continue
        }
        if (-not $inGroups) { continue }
        if ($line -match '^\s*#' -or $line -match '^\s*(type|default):') { continue }
        if ($line -match "^\s*-\s*'([^']+)'\s*$") {
            foreach ($category in $Matches[1].Split(',')) {
                $trimmed = $category.Trim()
                if ($trimmed -and -not $categories.Contains($trimmed)) { $categories.Add($trimmed) }
            }
            continue
        }
        if ($line.Trim()) { break }
    }
    if ($categories.Count -lt 10) { throw "Could not read the UI test category inventory from '$PipelineYamlPath'." }

    return [pscustomobject]@{
        categories = @($categories)
        # Material3 has its own Android lane and is not part of categoryGroupsToTest.
        extraCategories = @('Material3')
        umbrellas = [ordered]@{
            CollectionView = @($categories | Where-Object { $_ -cmatch '^CollectionView\d+$' })
            Shell = @($categories | Where-Object { $_ -cmatch '^Shell\d+$' })
        }
    }
}

# Device test projects (eng/helix_xharness.proj MAUIScenario names) and their TestCategory sources.
# Controls and Core filter by category (TestFilter=SkipCategories=...); the others run whole.
$script:RunTestsDeviceProjects = [ordered]@{
    Controls      = @{ scenario = 'Controls'; categories = 'src/Controls/tests/DeviceTests/TestCategory.cs' }
    Core          = @{ scenario = 'Core'; categories = 'src/Core/tests/DeviceTests/TestCategory.cs' }
    Graphics      = @{ scenario = 'Graphics' }
    Essentials    = @{ scenario = 'Essentials' }
    BlazorWebView = @{ scenario = 'MauiBlazorWebView' }
}

function Get-RunTestsDeviceCategories {
    <#
    .SYNOPSIS
    Category values declared in a device test TestCategory.cs ("X" or nameof(X)).
    #>
    param([AllowNull()][AllowEmptyString()][string]$Content)
    if (-not $Content) { return @() }
    $values = foreach ($match in [regex]::Matches($Content, 'const\s+string\s+(\w+)\s*=\s*(?:"([^"]+)"|nameof\(\s*(\w+)\s*\))')) {
        $value = if ($match.Groups[2].Success) { $match.Groups[2].Value } else { $match.Groups[3].Value }
        # Category values end up in pipeline variables: identifiers only.
        if ($value -cmatch '^[A-Za-z0-9_]{1,64}$') { $value }
    }
    return @($values | Sort-Object -Unique)
}

function Get-RunTestsDeviceInventory {
    <#
    .SYNOPSIS
    The device test units `/review run-device-tests` can select, in the same shape as the UI inventory:
    Controls.<Category> and Core.<Category> (umbrellas Controls and Core run all of a project), plus
    Graphics, Essentials and BlazorWebView, which run whole.
    .PARAMETER Categories
    Project name -> category values (Get-RunTestsDeviceCategories) for the projects that filter by category.
    #>
    param([Parameter(Mandatory)][System.Collections.IDictionary]$Categories)
    $units = [System.Collections.Generic.List[string]]::new()
    $umbrellas = [ordered]@{}
    $projects = [ordered]@{}
    foreach ($project in $script:RunTestsDeviceProjects.Keys) {
        if ($script:RunTestsDeviceProjects[$project].categories) {
            $names = @($Categories[$project] | Where-Object { $_ } | ForEach-Object { "$project.$_" })
            if ($names.Count -lt 5) { throw "Could not read the $project device test categories." }
            $umbrellas[$project] = $names
            $projects[$project] = $names
            foreach ($name in $names) { $units.Add($name) }
        } else {
            $projects[$project] = @($project)
            $units.Add($project)
        }
    }
    return [pscustomobject]@{ categories = @($units); extraCategories = @(); umbrellas = $umbrellas; projects = $projects }
}

function New-RunTestsDeviceRequestYaml {
    <#
    .SYNOPSIS
    Content of eng/pipelines/common/run-devicetests-request.yml for a dispatch commit.
    .DESCRIPTION
    Turns a validated device selection into the variables eng/pipelines/ci-device-tests.yml and
    eng/helix_xharness.proj read: platforms, the scenarios to run, and per filtered project the
    categories to skip (or, on Windows, to run). Every value is checked again because it is
    written into pipeline YAML and MSBuild properties.
    #>
    param(
        [Parameter(Mandatory)]$Selection,
        [Parameter(Mandatory)]$Inventory,
        [Parameter(Mandatory)][ValidateRange(1, 2147483647)][int]$PrNumber,
        [Parameter(Mandatory)][string]$HeadSha
    )
    if ($HeadSha -cnotmatch '^[0-9a-f]{40}$') { throw 'The head SHA must be a full commit SHA.' }
    $mode = [string]$Selection.mode
    if ($mode -cnotin @('selected', 'full')) { throw "A '$mode' selection is not dispatched." }
    $running = @($Selection.expandedCategories)
    $platformIds = if ($mode -eq 'full') { Get-RunTestsPlatformIds } else { @($Selection.platforms) }
    $platforms = @($platformIds | ForEach-Object { $script:RunTestsPipelinePlatform[$_] })
    $scenarios = [System.Collections.Generic.List[string]]::new()
    $skip = @{ Controls = @(); Core = @() }
    # On Windows the categories to run (the test script runs them one by one).
    $run = @{ Controls = @(); Core = @() }
    foreach ($project in $script:RunTestsDeviceProjects.Keys) {
        $units = @($Inventory.projects[$project])
        $on = if ($mode -eq 'full') { $units } else { @($units | Where-Object { $_ -cin $running }) }
        if (-not $on.Count) { continue }
        $scenarios.Add($script:RunTestsDeviceProjects[$project].scenario)
        if ($skip.Contains($project) -and $on.Count -lt $units.Count) {
            $skip[$project] = @($units | Where-Object { $_ -cnotin $on } | ForEach-Object { $_.Substring($project.Length + 1) })
            $run[$project] = @($on | ForEach-Object { $_.Substring($project.Length + 1) })
        }
    }
    if (-not $platforms.Count -or -not $scenarios.Count) { throw 'A device test run needs at least one platform and project.' }
    foreach ($platform in $platforms) { if ($platform -cnotin @($script:RunTestsPipelinePlatform.Values)) { throw "Unexpected platform '$platform'." } }
    foreach ($value in @($scenarios) + $skip.Controls + $skip.Core + $run.Controls + $run.Core) {
        if ($value -cnotmatch '^[A-Za-z0-9_]{1,64}$') { throw "Unexpected device test name '$value'." }
    }
    return @(
        "# Generated by ``/review $($script:RunTestsCommand)`` for pull request #$PrNumber at $HeadSha. Do not edit."
        '# See the checked-in eng/pipelines/common/run-devicetests-request.yml for the format.'
        'variables:'
        '  - name: RunTestsMode'
        "    value: '$mode'"
        '  - name: RunTestsPullRequest'
        "    value: '$PrNumber'"
        '  - name: RunTestsHeadSha'
        "    value: '$HeadSha'"
        '  - name: RunDeviceTestsPlatforms'
        "    value: '$($platforms -join ',')'"
        '  - name: RunDeviceTestsProjects'
        "    value: '$($scenarios -join '+')'"
        '  - name: RunDeviceTestsControlsSkip'
        "    value: '$($skip.Controls -join '+')'"
        '  - name: RunDeviceTestsCoreSkip'
        "    value: '$($skip.Core -join '+')'"
        '  - name: RunDeviceTestsControlsRun'
        "    value: '$($run.Controls -join '+')'"
        '  - name: RunDeviceTestsCoreRun'
        "    value: '$($run.Core -join '+')'"
        ''
    ) -join "`n"
}

function New-RunTestsSuiteRequestYaml {
    <#
    .SYNOPSIS
    The dispatch request file of the current suite.
    #>
    param([Parameter(Mandatory)]$Selection, [Parameter(Mandatory)]$Inventory, [Parameter(Mandatory)][int]$PrNumber, [Parameter(Mandatory)][string]$HeadSha)
    if ($script:RunTestsSuite -eq 'device') { return New-RunTestsDeviceRequestYaml -Selection $Selection -Inventory $Inventory -PrNumber $PrNumber -HeadSha $HeadSha }
    return New-RunTestsRequestYaml -Selection $Selection -PrNumber $PrNumber -HeadSha $HeadSha
}

function Get-RunTestsAllCategories {
    param([Parameter(Mandatory)]$Inventory)
    return @($Inventory.categories) + @($Inventory.extraCategories)
}

function ConvertFrom-RunTestsJsonArray {
    param([AllowNull()][object]$Value, [Parameter(Mandatory)][string]$Name)
    if ($null -eq $Value) { return @() }
    if ($Value -is [string]) {
        if ($Value.Length -gt 8000) { throw "$Name is too long." }
        if (-not $Value.Trim()) { return @() }
        try { $Value = ConvertFrom-Json -InputObject $Value -NoEnumerate -Depth 3 }
        catch { throw "$Name must be a JSON array of strings." }
    }
    $items = @($Value)
    foreach ($item in $items) {
        # Device test units are Project.Category (Controls.Button); UI categories are plain identifiers.
        if ($item -isnot [string] -or $item -cnotmatch '^[A-Za-z0-9_]{1,64}(\.[A-Za-z0-9_]{1,64})?$') {
            throw "$Name must contain only identifier strings."
        }
    }
    return $items
}

function Resolve-RunTestsSelection {
    <#
    .SYNOPSIS
    Validates the agent's proposal against the trusted inventory and returns a normalized selection.

    .OUTPUTS
    mode       : 'full' (everything, default pipeline), 'selected' or 'none'
    platforms  : platform ids that run (android, ios, maccatalyst, windows)
    categories : requested category ids (umbrellas kept); empty = all categories on those platforms
    expandedCategories : individual categories that run (umbrellas expanded)
    #>
    param(
        [Parameter(Mandatory)]$Proposal,
        [Parameter(Mandatory)]$Inventory,
        # platform id -> categories with enabled tests (Get-RunTestsRunnableCategories).
        [System.Collections.IDictionary]$Runnable
    )

    $get = {
        param($name)
        if ($Proposal -is [System.Collections.IDictionary]) { return $Proposal[$name] }
        $property = $Proposal.PSObject.Properties[$name]
        if ($property) { return $property.Value }
        return $null
    }

    $fullRun = & $get 'full_run'
    if ($fullRun -is [string]) { $fullRun = $fullRun -in @('true', 'True') }
    $fullRun = [bool]$fullRun

    $platforms = @(ConvertFrom-RunTestsJsonArray (& $get 'platforms') 'platforms' | ForEach-Object { $_.ToLowerInvariant() })
    $categories = @(ConvertFrom-RunTestsJsonArray (& $get 'categories') 'categories')
    $summary = [string](& $get 'summary')
    $rationale = [string](& $get 'rationale')

    $knownPlatforms = Get-RunTestsPlatformIds
    foreach ($platform in $platforms) {
        if ($platform -cnotin $knownPlatforms) { throw "Unknown platform '$platform'." }
    }

    $known = @(Get-RunTestsAllCategories $Inventory) + @($Inventory.umbrellas.Keys)
    $notes = [System.Collections.Generic.List[string]]::new()
    $validCategories = [System.Collections.Generic.List[string]]::new()
    foreach ($category in $categories) {
        # Accept case-insensitive matches but always emit the canonical spelling.
        $canonical = @($known | Where-Object { $_ -ieq $category } | Select-Object -First 1)
        if (-not $canonical) {
            $notes.Add("Ignored unknown category '$category'.")
            continue
        }
        if (-not $validCategories.Contains($canonical[0])) { $validCategories.Add($canonical[0]) }
    }

    $orderedPlatforms = @($knownPlatforms | Where-Object { $_ -in $platforms })

    if ($fullRun) {
        $mode = 'full'
        $orderedPlatforms = $knownPlatforms
        $validCategories.Clear()
    } elseif ($orderedPlatforms.Count -eq 0) {
        $mode = 'none'
        $validCategories.Clear()
    } elseif ($categories.Count -gt 0 -and $validCategories.Count -eq 0) {
        # Every requested category was unknown. Only run what the AI selected: nothing.
        $notes.Add('No requested category matched the inventory, so nothing from this selection runs.')
        $mode = 'none'
        $orderedPlatforms = @()
    } else {
        $mode = 'selected'
    }

    # Material3 only has an Android lane. Never add a platform the AI did not select.
    if ($mode -eq 'selected' -and $validCategories.Contains('Material3') -and 'android' -cnotin $orderedPlatforms) {
        $notes.Add('`Material3` only has Android tests and Android was not selected, so it does not run.')
        [void]$validCategories.Remove('Material3')
        if ($validCategories.Count -eq 0) { $mode = 'none'; $orderedPlatforms = @() }
    }

    $expanded = [System.Collections.Generic.List[string]]::new()
    $allCategories = Get-RunTestsAllCategories $Inventory
    if ($mode -eq 'full' -or ($mode -eq 'selected' -and $validCategories.Count -eq 0)) {
        foreach ($c in $allCategories) { $expanded.Add($c) }
    } elseif ($mode -eq 'selected') {
        foreach ($c in $validCategories) {
            if ($Inventory.umbrellas.Contains($c)) {
                foreach ($shard in $Inventory.umbrellas[$c]) { if (-not $expanded.Contains($shard)) { $expanded.Add($shard) } }
            } elseif (-not $expanded.Contains($c)) {
                $expanded.Add($c)
            }
        }
    }

    # Per-platform categories that will actually execute tests. Without a runnable map
    # (unit tests), every inventory category is assumed to have tests everywhere.
    $platformCategories = [ordered]@{}
    $pipelineCategories = [System.Collections.Generic.List[string]]::new()
    if ($mode -eq 'selected') {
        foreach ($platform in $orderedPlatforms) {
            $runnableHere = if ($Runnable) { @($Runnable[$platform]) } else { $allCategories }
            # The pipeline never runs WebView on Windows, and Material3 only has an Android lane.
            $platformCategories[$platform] = @($expanded | Where-Object {
                $_ -cin $runnableHere -and -not ($platform -eq 'windows' -and $_ -ceq 'WebView') -and
                -not ($platform -ne 'android' -and $_ -ceq 'Material3')
            })
        }
        if ($validCategories.Count -gt 0) {
            foreach ($c in @($expanded)) {
                if (-not @($orderedPlatforms | Where-Object { $c -cin $platformCategories[$_] }).Count) {
                    $notes.Add("``$c`` has no enabled $($script:RunTestsKind) on the selected platforms, so it does not run.")
                }
            }
            $kept = @($orderedPlatforms | Where-Object { $platformCategories[$_].Count -gt 0 })
            foreach ($platform in @($orderedPlatforms | Where-Object { $_ -notin $kept })) {
                $notes.Add("$(Get-RunTestsPlatformName $platform) was dropped: none of the selected categories have enabled tests there.")
                $platformCategories.Remove($platform)
            }
            $orderedPlatforms = $kept
            if ($orderedPlatforms.Count -eq 0) {
                $mode = 'none'
                $expanded.Clear()
                $validCategories.Clear()
            } else {
                $running = @($expanded | Where-Object { $c = $_; @($orderedPlatforms | Where-Object { $c -cin $platformCategories[$_] }).Count })
                $expanded.Clear(); foreach ($c in $running) { $expanded.Add($c) }
                foreach ($c in (ConvertTo-RunTestsPipelineCategories -Platforms $orderedPlatforms -PlatformCategories $platformCategories -Categories $running)) { $pipelineCategories.Add($c) }
            }
        }
    }

    return [pscustomobject]@{
        mode = $mode
        platforms = @($orderedPlatforms)
        categories = @($validCategories)
        expandedCategories = @($allCategories | Where-Object { $expanded.Contains($_) })
        platformCategories = $platformCategories
        pipelineCategories = @($pipelineCategories)
        summary = (Format-RunTestsText $summary 600)
        rationale = (Format-RunTestsText $rationale 4000 -Multiline)
        notes = @($notes)
    }
}

function Format-RunTestsText {
    <#
    .SYNOPSIS
    Neutralizes agent-authored text before it is rendered in a comment or check run.
    #>
    param([AllowNull()][AllowEmptyString()][string]$Text, [int]$MaxLength = 600, [switch]$Multiline)
    if (-not $Text) { return '' }
    $clean = $Text -replace '<!--', '&lt;!--' -replace '<', '&lt;' -replace '>', '&gt;'
    # Avoid pinging people or teams from AI-written text.
    $clean = $clean -replace '@(?=[A-Za-z0-9])', '@&#8203;'
    if (-not $Multiline) { $clean = $clean -replace '[\r\n]+', ' ' }
    $clean = $clean.Trim()
    if ($clean.Length -gt $MaxLength) { $clean = $clean.Substring(0, $MaxLength).TrimEnd() + '…' }
    return $clean
}

function Get-RunTestsTemplateParameters {
    <#
    .SYNOPSIS
    Azure DevOps runtime parameters for maui-pr-uitests. Full runs use the pipeline defaults.
    #>
    param([Parameter(Mandatory)]$Selection)
    if ($Selection.mode -ne 'selected') { return [ordered]@{} }
    # Only booleans and fixed-value strings: Azure DevOps lets an identity with just 'Queue builds'
    # set those, while free-text or object parameters need 'Edit queue build configuration'.
    $parameters = [ordered]@{ selectedRun = 'true' }
    foreach ($platform in Get-RunTestsPlatformIds) {
        $parameters["uitestPlatform$($script:RunTestsPlatformParameterSuffix[$platform])"] = $(if ($platform -cin @($Selection.platforms)) { 'true' } else { 'false' })
    }
    foreach ($category in @($Selection.expandedCategories)) {
        $on = @(foreach ($platform in Get-RunTestsPlatformIds) {
                if ($Selection.platformCategories.Contains($platform) -and $category -cin @($Selection.platformCategories[$platform])) {
                    $script:RunTestsPipelinePlatform[$platform]
                }
            })
        if ($on.Count) { $parameters["uitest_$category"] = $on -join '+' }
    }
    return $parameters
}

function Get-RunTestsDispatchBranch {
    <#
    .SYNOPSIS
    Branch name of one `/review run-ui-tests` dispatch: unique per request, one path segment under run-tests/.
    #>
    param([Parameter(Mandatory)][ValidateRange(1, 2147483647)][int]$PrNumber, [Parameter(Mandatory)][string]$RunId)
    if ($RunId -cnotmatch '^[1-9][0-9]{0,19}$') { throw "Invalid run id '$RunId'." }
    return "$($script:RunTestsDispatchBranchPrefix)pr$PrNumber-$RunId"
}

function New-RunTestsRequestYaml {
    <#
    .SYNOPSIS
    Content of eng/pipelines/common/run-tests-request.yml for a dispatch commit.

    .DESCRIPTION
    Carries exactly what the uitestPlatform* / uitest_<Category> parameters would: the platforms
    that run anything, and every selection as <platform>:<Category>. eng/pipelines/ci-uitests.yml
    turns these into the selectedPlatforms / selectedCategories of eng/pipelines/common/ui-tests.yml.
    Every value comes from the validated selection and is checked again here, because it is
    written into pipeline YAML.
    #>
    param(
        [Parameter(Mandatory)]$Selection,
        [Parameter(Mandatory)][ValidateRange(1, 2147483647)][int]$PrNumber,
        [Parameter(Mandatory)][string]$HeadSha
    )
    if ($HeadSha -cnotmatch '^[0-9a-f]{40}$') { throw 'The head SHA must be a full commit SHA.' }
    $mode = [string]$Selection.mode
    if ($mode -cnotin @('selected', 'full')) { throw "A '$mode' selection is not dispatched." }
    $platforms = @()
    $categories = @()
    if ($mode -eq 'selected') {
        $parameters = Get-RunTestsTemplateParameters $Selection
        foreach ($platform in Get-RunTestsPlatformIds) {
            if ($parameters["uitestPlatform$($script:RunTestsPlatformParameterSuffix[$platform])"] -eq 'true') {
                $platforms += $script:RunTestsPipelinePlatform[$platform]
            }
        }
        foreach ($key in @($parameters.Keys | Where-Object { $_ -like 'uitest_*' })) {
            foreach ($platform in ($parameters[$key] -split '\+')) { $categories += "$($platform):$($key.Substring(7))" }
        }
        if (-not $platforms.Count -or -not $categories.Count) { throw 'A selected run needs at least one platform and category.' }
    } else {
        $platforms = @(Get-RunTestsPlatformIds | ForEach-Object { $script:RunTestsPipelinePlatform[$_] })
    }
    $known = @($script:RunTestsPipelinePlatform.Values)
    foreach ($platform in $platforms) { if ($platform -cnotin $known) { throw "Unexpected platform '$platform'." } }
    foreach ($category in $categories) {
        if ($category -cnotmatch '^(android|ios|catalyst|windows):[A-Za-z0-9_]{1,64}$') { throw "Unexpected category '$category'." }
    }
    return @(
        "# Generated by ``/review $($script:RunTestsCommand)`` for pull request #$PrNumber at $HeadSha. Do not edit."
        '# See the checked-in eng/pipelines/common/run-tests-request.yml for the format.'
        'variables:'
        '  - name: RunTestsMode'
        "    value: '$mode'"
        '  - name: RunTestsPullRequest'
        "    value: '$PrNumber'"
        '  - name: RunTestsHeadSha'
        "    value: '$HeadSha'"
        '  - name: RunTestsPlatforms'
        "    value: '$($platforms -join ',')'"
        '  - name: RunTestsCategories'
        "    value: '$($categories -join ',')'"
        ''
    ) -join "`n"
}

function Test-RunTestsPipelineSupportsDispatch {
    <#
    .SYNOPSIS
    True when the suite's pipeline (ci-uitests.yml, ci-device-tests.yml) builds its dispatch
    branches and reads the dispatch request.
    #>
    param([AllowNull()][AllowEmptyString()][string]$Content)
    if (-not $Content) { return $false }
    $prefix = [regex]::Escape($script:RunTestsDispatchBranchPrefix)
    return ($Content -match "(?m)^\s*-\s*$($prefix)\*\s*$") -and $Content.Contains("/$($script:RunTestsRequestPath)")
}

function New-RunTestsCheckPlan {
    <#
    .SYNOPSIS
    The GitHub check runs that describe what did NOT run.

    .DESCRIPTION
    Azure Pipelines only reports jobs that exist in the run, so anything filtered out by the
    selection would otherwise be invisible. One check per platform that does not run at all,
    and one per category that does not run on any selected platform, all with conclusion
    'skipped' so they render as "Skipped" in the PR's checks list.
    #>
    param([Parameter(Mandatory)]$Selection, [Parameter(Mandatory)]$Inventory)

    $checks = [System.Collections.Generic.List[object]]::new()
    if ($Selection.mode -eq 'full') { return @() }

    foreach ($platform in Get-RunTestsPlatformIds) {
        if ($platform -in $Selection.platforms) { continue }
        $name = Get-RunTestsPlatformName $platform
        $checks.Add([pscustomobject]@{
            name = "$($script:RunTestsSkippedCheckPrefix) $name $(if ($script:RunTestsSuite -eq 'device') { 'DeviceTests' } else { 'UITests' })"
            title = "$name $($script:RunTestsKind) were not selected"
            kind = 'platform'
            key = $platform
        })
    }
    if ($Selection.mode -eq 'selected' -and $script:RunTestsSuite -eq 'device') {
        # One check per device test project that runs nothing (per-category checks would be ~100).
        foreach ($project in $Inventory.projects.Keys) {
            if (@($Inventory.projects[$project] | Where-Object { $_ -cin @($Selection.expandedCategories) }).Count) { continue }
            $checks.Add([pscustomobject]@{
                name = "$($script:RunTestsSkippedCheckPrefix) $project.DeviceTests"
                title = "$project device tests were not selected"
                kind = 'category'
                key = $project
            })
        }
    } elseif ($Selection.mode -eq 'selected') {
        $running = $Selection.expandedCategories
        foreach ($category in Get-RunTestsAllCategories $Inventory) {
            if ($category -in $running) { continue }
            $checks.Add([pscustomobject]@{
                name = "$($script:RunTestsSkippedCheckPrefix) $category"
                title = "$category $($script:RunTestsKind) were not selected"
                kind = 'category'
                key = $category
            })
        }
    }
    return @($checks)
}

function Get-RunTestsBadge {
    param([Parameter(Mandatory)][string]$Label, [Parameter(Mandatory)][string]$Message, [string]$Color = '1f6feb')
    $l = [uri]::EscapeDataString($Label).Replace('-', '--')
    $m = [uri]::EscapeDataString($Message).Replace('-', '--')
    return "  <img alt=`"$Label $Message`" src=`"https://img.shields.io/badge/$l-$m-$($Color)?labelColor=30363d&amp;style=flat-square`">"
}

function Format-RunTestsCount {
    param([int]$Count, [string]$Singular, [string]$Plural)
    return "$Count $(if ($Count -eq 1) { $Singular } else { $Plural })"
}

function Format-RunTestsCategoryList {
    param([string[]]$Categories)
    if (-not $Categories -or $Categories.Count -eq 0) { return '_none_' }
    return (@($Categories | ForEach-Object { "``$_``" }) -join ', ')
}

function Get-RunTestsCommentStatus {
    # The status of one suite's run, as shown in its comment.
    param($Selection, $Build, [string]$QueueError, $Results, [switch]$SelectionOnly)
    $status = Get-RunTestsStatus -Selection $Selection -Build $Build -QueueError $QueueError -Results $Results
    if ($SelectionOnly -and -not $QueueError -and $Selection.mode -ne 'none') {
        $status = @{ state = 'selection-only'; badge = 'SELECTION ONLY'; color = '6e7681'; line = "&#x2139;&#xFE0F; Selection only &#x2014; Azure DevOps dispatch is not available in this repository, so ``$($script:RunTestsPipelineName)`` was not queued." }
    }
    return $status
}

function Get-RunTestsPlatformText {
    param($Selection)
    if ($Selection.mode -eq 'none' -or -not $Selection.platforms.Count) { return 'none' }
    return (($Selection.platforms | ForEach-Object { Get-RunTestsPlatformName $_ }) -join ', ')
}

function Get-RunTestsRequesterText {
    param([string]$Requester)
    return $(if ($Requester -cmatch '^[A-Za-z0-9][A-Za-z0-9-]{0,38}$') { "@$Requester" } else { 'Requested' })
}

function Get-RunTestsSuiteMarkers {
    # Hidden markers of one suite's run: Update-RunTestsResults.ps1 follows the build they name.
    param($Status, $PullRequest, $Build, $Dispatch)
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("<!-- review-run-tests-head: $([string]$PullRequest.head.sha) -->")
    if ($Build) { $lines.Add("<!-- review-run-tests-build: $($Build.id) -->") }
    if ($Dispatch) {
        $lines.Add("<!-- review-run-tests-dispatch: $($Dispatch.sha) -->")
        if ($Dispatch.checkRunId) { $lines.Add("<!-- review-run-tests-check: $($Dispatch.checkRunId) -->") }
    }
    $lines.Add("<!-- review-run-tests-state: $($Status.state) -->")
    return $lines
}

function Format-RunTestsRunning {
    <#
    .SYNOPSIS
    The status line, what runs on each platform and the results block of one suite.
    #>
    param($Selection, $Status, $Results)
    $runCount = if ($Selection.mode -eq 'none') { 0 } else { $Selection.expandedCategories.Count }
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<!-- run-tests-status -->')
    $lines.Add($Status.line)
    $lines.Add('<!-- /run-tests-status -->')
    $lines.Add('')
    if ($Selection.summary) { $lines.Add($Selection.summary); $lines.Add('') }
    $runningText = if ($Selection.mode -eq 'none') { 'nothing' } else { "$(Format-RunTestsCount $runCount 'category' 'categories') on $(Get-RunTestsPlatformText $Selection)" }
    $lines.Add("#### <strong>$($script:RunTestsRunHeading['running'])</strong> &#x2014; $runningText")
    $lines.Add('')
    switch ($Selection.mode) {
        'full' { $lines.Add("The complete ``$($script:RunTestsPipelineName)`` suite runs on every platform (same as ``/azp run``).") }
        'none' { $lines.Add("No $($script:RunTestsKind) run for this PR.") }
        default {
            $lines.Add('| Platform | Categories |')
            $lines.Add('| --- | --- |')
            foreach ($platform in $Selection.platforms) {
                $categoriesForPlatform = if ($Selection.platformCategories -and $Selection.platformCategories.Contains($platform)) {
                    @($Selection.platformCategories[$platform])
                } else {
                    @($Selection.expandedCategories | Where-Object { $_ -cne 'Material3' -or $platform -eq 'android' })
                }
                $text = if ($Selection.categories.Count -eq 0) { '_all categories_' } else { Format-RunTestsCategoryList $categoriesForPlatform }
                $lines.Add("| $(Get-RunTestsPlatformName $platform) | $text |")
            }
        }
    }
    $lines.Add('')
    $lines.Add('<!-- run-tests-results -->')
    $lines.Add((Format-RunTestsResultsBlock $Results))
    $lines.Add('<!-- /run-tests-results -->')
    return $lines
}

function Format-RunTestsSkipped {
    <#
    .SYNOPSIS
    What one suite skips: { summary; lines }, or $null for a full run.
    #>
    param($Selection, $Inventory)
    if ($Selection.mode -eq 'full') { return $null }
    $allCategories = Get-RunTestsAllCategories $Inventory
    $skippedCategories = @($allCategories | Where-Object { $_ -notin $Selection.expandedCategories })
    $skippedPlatforms = @(Get-RunTestsPlatformIds | Where-Object { $_ -notin $Selection.platforms })
    $skipCount = if ($Selection.mode -eq 'none') { $allCategories.Count } else { $skippedCategories.Count }
    $lines = [System.Collections.Generic.List[string]]::new()
    if ($skippedPlatforms.Count) {
        $lines.Add("**Platforms not run:** $(($skippedPlatforms | ForEach-Object { Get-RunTestsPlatformName $_ }) -join ', ')")
        $lines.Add('')
    }
    if ($Selection.mode -eq 'selected' -and $skippedCategories.Count) {
        $lines.Add("**Categories not run:** $(Format-RunTestsCategoryList $skippedCategories)")
        $lines.Add('')
    }
    $lines.Add("Each skipped platform and $(if ($script:RunTestsSuite -eq 'device') { 'project' } else { 'category' }) is listed as a **Skipped** ``$($script:RunTestsSkippedCheckPrefix) …`` check on this commit. Use ``/azp run $($script:RunTestsPipelineName)`` for the full suite.")
    return @{
        count = $skipCount
        summary = "$(Format-RunTestsCount $skipCount 'category' 'categories'), $(Format-RunTestsCount $skippedPlatforms.Count 'platform' 'platforms')"
        lines = $lines
    }
}

function Format-RunTestsWhy {
    # Each model's choice and rationale for one suite.
    param($Selection)
    $lines = [System.Collections.Generic.List[string]]::new()
    $models = @($Selection.PSObject.Properties['models']?.Value | Where-Object { $_ })
    if ($models.Count -gt 1) {
        $lines.Add('Each model chose independently; every test that runs was selected by at least one of them.')
        $lines.Add('')
        $lines.Add("| Platform | $(($models | ForEach-Object { $_.displayName }) -join ' | ') |")
        $lines.Add("| --- |$(' --- |' * $models.Count)")
        foreach ($platform in Get-RunTestsPlatformIds) {
            $cells = foreach ($model in $models) {
                if ($model.mode -eq 'full') { '_all categories_' }
                elseif ($model.platformCategories -and $model.platformCategories.Contains($platform) -and @($model.platformCategories[$platform]).Count) { Format-RunTestsCategoryList @($model.platformCategories[$platform]) }
                else { '&#x2014;' }
            }
            $lines.Add("| $(Get-RunTestsPlatformName $platform) | $($cells -join ' | ') |")
        }
        foreach ($model in $models) {
            $lines.Add('')
            $lines.Add("**$($model.displayName)** &#x2014; $(if ($model.summary) { $model.summary } else { '_no summary_' })")
            $lines.Add('')
            $lines.Add($(if ($model.rationale) { $model.rationale } else { '_No rationale provided._' }))
        }
    } else {
        $lines.Add($(if ($Selection.rationale) { $Selection.rationale } else { '_No rationale provided._' }))
    }
    foreach ($note in $Selection.notes) { $lines.Add(''); $lines.Add("> [!NOTE]`n> $(Format-RunTestsText $note 300)") }
    return $lines
}

function New-RunTestsComment {
    <#
    .SYNOPSIS
    Renders the `/review run-ui-tests` or `/review run-device-tests` PR comment, in the same style as
    the other `/review` reports: title, requester line and badges up front; everything else in one
    collapsed section.
    #>
    param(
        [Parameter(Mandatory)]$Selection,
        [Parameter(Mandatory)]$Inventory,
        [Parameter(Mandatory)]$PullRequest,
        [Parameter(Mandatory)][string]$WorkflowRunUrl,
        [AllowNull()]$Build,
        [string]$QueueError,
        [string]$Requester,
        # maui-expert-autopilot runs: the loop's own comments say who started it.
        [switch]$OmitRequester,
        [AllowNull()]$Results,
        # The dispatch commit ({ sha; checkRunId }) maui-pr-uitests builds for this request.
        [AllowNull()]$Dispatch,
        [switch]$SelectionOnly
    )

    $sha = [string]$PullRequest.head.sha
    $short = $sha.Substring(0, 8)
    $repo = [string]$PullRequest.base.repo.full_name
    $allCategories = Get-RunTestsAllCategories $Inventory
    $runCount = if ($Selection.mode -eq 'none') { 0 } else { $Selection.expandedCategories.Count }
    $scopeColor = switch ($Selection.mode) { 'full' { '9a6700' } 'none' { '6e7681' } default { '1f6feb' } }
    $status = Get-RunTestsCommentStatus -Selection $Selection -Build $Build -QueueError $QueueError -Results $Results -SelectionOnly:$SelectionOnly

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add($script:RunTestsCommentMarker)
    foreach ($line in Get-RunTestsSuiteMarkers -Status $status -PullRequest $PullRequest -Build $Build -Dispatch $Dispatch) { $lines.Add($line) }
    $lines.Add('')
    $lines.Add($(if ($script:RunTestsSuite -eq 'device') { '## Selected Device Tests' } else { '## Selected UI Tests' }))
    $lines.Add('')
    if (-not $OmitRequester) {
        $lines.Add("> $(Get-RunTestsRequesterText $Requester) &#x2014; $(Get-RunTestsSelectedByText $Selection) for commit [``$short``](https://github.com/$repo/commit/$sha).")
        $lines.Add('')
    }
    $lines.Add('<p align="left">')
    $lines.Add((Get-RunTestsBadge 'Result' $status.badge $status.color))
    $lines.Add((Get-RunTestsBadge 'Platforms' (Get-RunTestsPlatformText $Selection)))
    $categoryBadge = if ($Selection.mode -eq 'full') { "All $($allCategories.Count)" } else { "$runCount of $($allCategories.Count)" }
    $lines.Add((Get-RunTestsBadge 'Categories' $categoryBadge $scopeColor))
    $lines.Add((Get-RunTestsBadge 'Commit' $short))
    $lines.Add('</p>')
    $lines.Add('')
    $lines.Add('---')
    $lines.Add('')
    $lines.Add('<details>')
    $lines.Add('<summary><strong>&#x1F9EA; Test selection</strong> &#x2014; click to expand</summary>')
    $lines.Add('<br/>')
    $lines.Add('')
    foreach ($line in Format-RunTestsRunning -Selection $Selection -Status $status -Results $Results) { $lines.Add($line) }

    $skipped = Format-RunTestsSkipped -Selection $Selection -Inventory $Inventory
    if ($skipped) {
        $lines.Add('<details>')
        $lines.Add("<summary><strong>&#x23ED;&#xFE0F; Skipped</strong> &#x2014; $($skipped.summary)</summary>")
        $lines.Add('<br/>')
        $lines.Add('')
        foreach ($line in $skipped.lines) { $lines.Add($line) }
        $lines.Add('')
        $lines.Add('</details>')
        $lines.Add('')
    }

    $lines.Add('<details>')
    $lines.Add("<summary><strong>&#x1F916; Why these tests</strong> &#x2014; $(Get-RunTestsSelectedByText $Selection -Short)</summary>")
    $lines.Add('<br/>')
    $lines.Add('')
    foreach ($line in Format-RunTestsWhy $Selection) { $lines.Add($line) }
    $lines.Add('')
    $lines.Add('</details>')
    $lines.Add('')
    $lines.Add("<sub>Generated by [/review $($script:RunTestsCommand)]($WorkflowRunUrl). A targeted run is a fast signal, not full-suite certification.</sub>")
    $lines.Add('')
    $lines.Add('</details>')
    return ($lines -join "`n")
}

# How a suite's result reads in the overview table of a `/review run-tests` comment, and which
# result the comment's one Result badge shows: the first of these that any suite has.
$script:RunTestsResultOrder = [ordered]@{
    'FAILED'         = @{ icon = '&#x274C;'; color = 'cf222e' }
    'NOT QUEUED'     = @{ icon = '&#x274C;'; color = 'cf222e' }
    'RUNNING'        = @{ icon = '&#x23F3;'; color = '9a6700' }
    'CANCELED'       = @{ icon = '&#x26AA;'; color = '6e7681' }
    'DRY RUN'        = @{ icon = '&#x2139;&#xFE0F;'; color = '6e7681' }
    'SELECTION ONLY' = @{ icon = '&#x2139;&#xFE0F;'; color = '6e7681' }
    'PASSED'         = @{ icon = '&#x2705;'; color = '2da44e' }
    'NOTHING TO RUN' = @{ icon = '&#x2796;'; color = '6e7681' }
}

function New-RunTestsCombinedComment {
    <#
    .SYNOPSIS
    The `/review run-tests` comment, in the standard `/review` layout: title, requester line, one
    badge row, then one collapsed section with an overview table of both suites and a collapsible
    block per suite. Each suite's block sits between `<!-- run-tests-section: <suite> -->` markers
    with its own build markers, so Update-RunTestsResults.ps1 refreshes each build on its own and
    then Update-RunTestsCombinedOverview recomputes the Result badge and the overview table.
    .PARAMETER Runs
    Ordered suite id ('ui', 'device') -> { selection; inventory; build; queueError; dispatch }.
    .PARAMETER Notes
    Lines shown in the collapsed section, e.g. a suite that could not run on this PR's target branch.
    #>
    param(
        [Parameter(Mandatory)][System.Collections.IDictionary]$Runs,
        [Parameter(Mandatory)]$PullRequest,
        [Parameter(Mandatory)][string]$WorkflowRunUrl,
        [string[]]$ModelNames,
        [string]$Requester,
        [switch]$OmitRequester,
        [string[]]$Notes,
        [switch]$SelectionOnly
    )
    $sha = [string]$PullRequest.head.sha
    $short = $sha.Substring(0, 8)
    $repo = [string]$PullRequest.base.repo.full_name
    $kinds = @($Runs.Keys | ForEach-Object { $script:RunTestsSuites[$_].Kind })
    $by = if ($ModelNames.Count) { " ($($ModelNames -join ' + '))" } else { '' }

    # Header badges: the union of platforms and each suite's count; the Result badge is filled in
    # from the sections by Update-RunTestsCombinedOverview.
    $platforms = @(Get-RunTestsPlatformIds | Where-Object { $p = $_; @($Runs.Values | Where-Object { $_.selection.mode -ne 'none' -and $p -in $_.selection.platforms }).Count })
    $platformText = if ($platforms.Count) { ($platforms | ForEach-Object { Get-RunTestsPlatformName $_ }) -join ', ' } else { 'none' }
    $testsText = @(foreach ($suite in $Runs.Keys) {
            $selection = $Runs[$suite].selection
            $name = if ($suite -eq 'device') { 'device' } else { 'UI' }
            switch ($selection.mode) { 'full' { "all $name" } 'none' { "0 $name" } default { "$($selection.expandedCategories.Count) $name" } }
        }) -join ', '

    $sections = [System.Collections.Generic.List[string]]::new()
    $skippedBlocks = [System.Collections.Generic.List[string]]::new()
    $whyBlocks = [System.Collections.Generic.List[string]]::new()
    foreach ($suite in $Runs.Keys) {
        $run = $Runs[$suite]
        Set-RunTestsSuite $suite -Combined
        $selection = $run.selection
        $allCategories = Get-RunTestsAllCategories $run.inventory
        $unit = if ($suite -eq 'device') { 'units' } else { 'categories' }
        $title = if ($suite -eq 'device') { 'Device tests' } else { 'UI tests' }
        $status = Get-RunTestsCommentStatus -Selection $selection -Build $run.build -QueueError $run.queueError -SelectionOnly:$SelectionOnly
        $selected = switch ($selection.mode) { 'full' { "all $($allCategories.Count) $unit" } 'none' { 'none' } default { "$($selection.expandedCategories.Count) of $($allCategories.Count) $unit" } }
        $buildCell = if ($run.build) { "[$($script:RunTestsPipelineName) #$($run.build.id)]($($run.build._links.web.href))" } else { '&#x2014;' }
        $summaryText = switch ($selection.mode) {
            'full' { "the full suite on every platform" }
            'none' { 'nothing to run' }
            default {
                # GitHub does not render markdown inside <summary>.
                $shown = @($selection.expandedCategories | Select-Object -First 4 | ForEach-Object { "<code>$_</code>" }) -join ', '
                $more = $selection.expandedCategories.Count - 4
                "$shown$(if ($more -gt 0) { " +$more" }) on $(Get-RunTestsPlatformText $selection)"
            }
        }
        $sections.Add("<!-- run-tests-section: $suite -->")
        foreach ($line in Get-RunTestsSuiteMarkers -Status $status -PullRequest $PullRequest -Build $run.build -Dispatch $run.dispatch) { $sections.Add($line) }
        $sections.Add("<!-- run-tests-result: $($status.badge) -->")
        $sections.Add("<!-- run-tests-overview: $title|$selected|$buildCell -->")
        $sections.Add('<details>')
        $sections.Add("<summary><strong>&#x1F9EA; $title</strong> &#x2014; $summaryText</summary>")
        $sections.Add('<br/>')
        $sections.Add('')
        foreach ($line in Format-RunTestsRunning -Selection $selection -Status $status -Results $null) { $sections.Add($line) }
        $sections.Add('</details>')
        $sections.Add("<!-- /run-tests-section: $suite -->")
        $sections.Add('')

        $skipped = Format-RunTestsSkipped -Selection $selection -Inventory $run.inventory
        if ($skipped) {
            $skippedBlocks.Add("**$title** &#x2014; $($skipped.summary)")
            $skippedBlocks.Add('')
            foreach ($line in $skipped.lines) { $skippedBlocks.Add($line) }
            $skippedBlocks.Add('')
        }
        $whyBlocks.Add("#### $title")
        $whyBlocks.Add('')
        foreach ($line in Format-RunTestsWhy $selection) { $whyBlocks.Add($line) }
        $whyBlocks.Add('')
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add($script:RunTestsCombined.CommentMarker)
    $lines.Add("<!-- review-run-tests-head: $sha -->")
    $lines.Add('')
    $lines.Add('## Selected Tests')
    $lines.Add('')
    if (-not $OmitRequester) {
        $lines.Add("> $(Get-RunTestsRequesterText $Requester) &#x2014; AI-selected $($kinds -join ' and ')$by for commit [``$short``](https://github.com/$repo/commit/$sha).")
        $lines.Add('')
    }
    $lines.Add('<p align="left">')
    $lines.Add((Get-RunTestsBadge 'Result' 'RUNNING' '9a6700'))
    $lines.Add((Get-RunTestsBadge 'Platforms' $platformText))
    $lines.Add((Get-RunTestsBadge 'Tests' $testsText))
    $lines.Add((Get-RunTestsBadge 'Commit' $short))
    $lines.Add('</p>')
    $lines.Add('')
    $lines.Add('---')
    $lines.Add('')
    $lines.Add('<details>')
    $lines.Add('<summary><strong>&#x1F9EA; Test selection</strong> &#x2014; click to expand</summary>')
    $lines.Add('<br/>')
    $lines.Add('')
    foreach ($note in @($Notes | Where-Object { $_ })) { $lines.Add("> [!NOTE]`n> $(Format-RunTestsText $note 400)"); $lines.Add('') }
    $lines.Add('<!-- run-tests-overview -->')
    $lines.Add('<!-- /run-tests-overview -->')
    $lines.Add('')
    foreach ($line in $sections) { $lines.Add($line) }
    if ($skippedBlocks.Count) {
        $lines.Add('<details>')
        $lines.Add('<summary><strong>&#x23ED;&#xFE0F; Skipped</strong> &#x2014; what neither suite runs</summary>')
        $lines.Add('<br/>')
        $lines.Add('')
        foreach ($line in $skippedBlocks) { $lines.Add($line) }
        $lines.Add('</details>')
        $lines.Add('')
    }
    $lines.Add('<details>')
    $lines.Add("<summary><strong>&#x1F916; Why these tests</strong> &#x2014; AI rationale$by</summary>")
    $lines.Add('<br/>')
    $lines.Add('')
    foreach ($line in $whyBlocks) { $lines.Add($line) }
    $lines.Add('</details>')
    $lines.Add('')
    $lines.Add("<sub>Generated by [/review $($script:RunTestsCombined.Command)]($WorkflowRunUrl). A targeted run is a fast signal, not full-suite certification.</sub>")
    $lines.Add('')
    $lines.Add('</details>')
    return (Update-RunTestsCombinedOverview ($lines -join "`n"))
}

function Update-RunTestsCombinedOverview {
    <#
    .SYNOPSIS
    Recomputes the Result badge and the overview table of a `/review run-tests` comment from its
    suite sections (their `run-tests-result` and `run-tests-overview` markers).
    #>
    param([Parameter(Mandatory)][string]$Body)
    # Comments posted before the overview table existed keep a Result badge per section; leave them.
    if (-not $Body.Contains('<!-- run-tests-overview -->')) { return $Body }
    $rows = [System.Collections.Generic.List[string]]::new()
    $results = [System.Collections.Generic.List[string]]::new()
    foreach ($section in Get-RunTestsCommentSections $Body) {
        $result = if ($section.body -match '<!-- run-tests-result: ([A-Z ]+) -->') { $Matches[1] } else { 'RUNNING' }
        if (-not $script:RunTestsResultOrder.Contains($result)) { $result = 'RUNNING' }
        $results.Add($result)
        if ($section.body -notmatch '<!-- run-tests-overview: ([^|]*)\|([^|]*)\|(.*?) -->') { continue }
        $rows.Add("| $($Matches[1]) | $($script:RunTestsResultOrder[$result].icon) $($result.ToLowerInvariant()) | $($Matches[2]) | $($Matches[3]) |")
    }
    $overall = @($script:RunTestsResultOrder.Keys | Where-Object { $_ -in $results })[0] ?? 'RUNNING'
    $table = @('| Suite | Result | Selected | Build |', '| --- | --- | --- | --- |') + $rows
    $badge = Get-RunTestsBadge 'Result' $overall $script:RunTestsResultOrder[$overall].color
    $updated = [regex]::Replace($Body, '(?m)^  <img alt="Result [^"]*"[^\n]*$', { param($m) $badge }, 'None', [TimeSpan]::FromSeconds(5))
    return [regex]::Replace($updated, '(?s)<!-- run-tests-overview -->.*?<!-- /run-tests-overview -->',
        { param($m) "<!-- run-tests-overview -->`n$($table -join "`n")`n<!-- /run-tests-overview -->" })
}

function Get-RunTestsCommentSections {
    <#
    .SYNOPSIS
    The suite sections of a run-tests comment: one { suite; body } per section of a combined
    `/review run-tests` comment, or the whole body as one section for a single-suite comment.
    #>
    param([Parameter(Mandatory)][string]$Body)
    $suite = Get-RunTestsCommentSuite $Body
    if ($suite -ne 'all') { return @(if ($suite) { [pscustomobject]@{ suite = $suite; body = $Body } }) }
    return @([regex]::Matches($Body, '(?s)<!-- run-tests-section: (ui|device) -->\n(.*?)\n<!-- /run-tests-section: \1 -->') |
        ForEach-Object { [pscustomobject]@{ suite = $_.Groups[1].Value; body = $_.Groups[2].Value } })
}

function Set-RunTestsCommentSection {
    <#
    .SYNOPSIS
    Replaces one suite's section of a combined comment (or the whole body of a single-suite comment).
    #>
    param([Parameter(Mandatory)][string]$Body, [Parameter(Mandatory)][string]$Suite, [Parameter(Mandatory)][string]$Section)
    if ((Get-RunTestsCommentSuite $Body) -ne 'all') { return $Section }
    $pattern = "(?s)(<!-- run-tests-section: $Suite -->\n).*?(\n<!-- /run-tests-section: $Suite -->)"
    return [regex]::Replace($Body, $pattern, { param($m) $m.Groups[1].Value + $Section + $m.Groups[2].Value })
}

function Format-RunTestsFailedTestPlatforms {
    param([Parameter(Mandatory)]$Test)
    return (@($Test.platforms | ForEach-Object {
                $url = if ($Test.PSObject.Properties['links'] -and $Test.links) { $Test.links[$_] } else { $null }
                if ($url) { "[$_]($url)" } else { $_ }
            }) -join ', ')
}

function Format-RunTestsResultsBlock {
    param([AllowNull()]$Results)
    if (-not $Results) { return '' }
    $buildUrl = [string]$Results.PSObject.Properties['buildUrl']?.Value
    $lines = [System.Collections.Generic.List[string]]::new()
    $open = if ($Results.completed -and $Results.result -ne 'succeeded') { ' open' } else { '' }
    $lines.Add("<details$open>")
    $lines.Add("<summary><strong>&#x1F4CA; Results</strong> &#x2014; $($Results.headline)</summary>")
    $lines.Add('<br/>')
    $lines.Add('')
    $failedTests = @($Results.PSObject.Properties['failedTests']?.Value | Where-Object { $_ })
    if ($failedTests.Count -gt 0) {
        $lines.Add("**&#x274C; Failed tests ($($failedTests.Count))**")
        $lines.Add('')
        $lines.Add('| Test | Failed on | Error |')
        $lines.Add('| --- | --- | --- |')
        foreach ($test in @($failedTests | Select-Object -First $script:RunTestsMaxFailedTests)) {
            $errorText = (Format-RunTestsText ([string]$test.PSObject.Properties['error']?.Value) 160) -replace '\|', '\|'
            $lines.Add("| ``$(Format-RunTestsText $test.name)`` | $(Format-RunTestsFailedTestPlatforms $test) | $errorText |")
        }
        if ($failedTests.Count -gt $script:RunTestsMaxFailedTests) {
            $lines.Add('')
            $lines.Add("...and $($failedTests.Count - $script:RunTestsMaxFailedTests) more in the Tests tab.")
        }
        if ($Results.PSObject.Properties['rerun']?.Value) {
            $lines.Add('')
            $lines.Add('Some jobs were re-run, so this list can include failures from earlier attempts that later passed.')
        }
        $lines.Add('')
    }
    if ($buildUrl) {
        $lines.Add("&#x1F517; [All test results]($(Get-RunTestsResultUrl $buildUrl)) &#xB7; [Build logs]($buildUrl&view=logs)")
        $lines.Add('')
    }
    $lines.Add('| Job | Result |')
    $lines.Add('| --- | --- |')
    foreach ($job in $Results.jobs) {
        $icon = switch ($job.result) { 'succeeded' { '&#x2705;' } 'failed' { '&#x274C;' } 'canceled' { '&#x26AA;' } 'succeededWithIssues' { '&#x26A0;&#xFE0F;' } default { '&#x23F3;' } }
        $jobName = $job.name -replace '\|', '/'
        $jobUrl = [string]$job.PSObject.Properties['url']?.Value
        if ($jobUrl) { $jobName = "[$jobName]($jobUrl)" }
        $lines.Add("| $jobName | $icon $($job.result) |")
    }
    $lines.Add('')
    $lines.Add('</details>')
    $lines.Add('')
    return ($lines -join "`n")
}

$script:RunTestsMaxFailedTests = 25
$script:RunTestsTestAssemblyPlatforms = [ordered]@{ android = 'Android'; ios = 'iOS'; mac = 'MacCatalyst'; winui = 'Windows' }

function Get-RunTestsResultUrl {
    <#
    .SYNOPSIS
    Link to a build's Tests tab, or to one test result in it.
    #>
    param([Parameter(Mandatory)][string]$BuildUrl, [string]$RunId, [string]$ResultId)
    $url = "$BuildUrl&view=ms.vss-test-web.build-test-results-tab"
    if ($RunId -match '^\d+$' -and $ResultId -match '^\d+$') { $url += "&runId=$RunId&resultId=$ResultId&paneView=debug" }
    return $url
}

function ConvertTo-RunTestsFailedTests {
    <#
    .SYNOPSIS
    Groups failed Azure DevOps test results by test: the platforms it failed on, a link to each
    platform's result and the first error message.
    .PARAMETER TestResults
    Test results as returned by test/Runs/{runId}/results/{id} (testRun.id, id, errorMessage), or the
    lighter testresults/resultsbybuild entries (runId, id).
    #>
    param([AllowNull()]$TestResults, [string]$BuildUrl)
    $groups = [ordered]@{}
    foreach ($result in @($TestResults | Where-Object { $_ })) {
        $title = [string]$result.testCaseTitle
        $full = [string]$result.automatedTestName
        # "Namespace.Fixture(Args).Method" or just "Namespace.Fixture"; show "Fixture.Method".
        $fixture = if ($title -and $full.EndsWith(".$title")) { $full.Substring(0, $full.Length - $title.Length - 1) } else { $full }
        $class = (($fixture -replace '\([^()]*\)$', '') -split '\.')[-1]
        $name = if ($class -and $title -and $class -ne $title) { "$class.$title" } elseif ($title) { $title } else { $class }
        if (-not $name) { continue }
        $runName = [string]$(if ($result.PSObject.Properties['testRun']) { $result.testRun.name } else { '' })
        $platform = if ([string]$result.automatedTestStorage -match 'testcases\.(\w+)\.tests') { $script:RunTestsTestAssemblyPlatforms[$Matches[1]] ?? $Matches[1] }
        # Device test runs are named DeviceTests<Platform>_<Runtime>_<queue>... (eng/pipelines/arcade/stage-device-tests.yml).
        elseif ($runName -match '^DeviceTests(Android|IOS|MacCatalyst|Windows)(?:_(CoreCLR|Mono))?') {
            $platformName = @{ Android = 'Android'; IOS = 'iOS'; MacCatalyst = 'MacCatalyst'; Windows = 'Windows' }[$Matches[1]]
            if ($Matches[2] -eq 'Mono') { "$platformName (Mono)" } else { $platformName }
        } else { 'Unknown' }
        if (-not $groups.Contains($name)) { $groups[$name] = [ordered]@{ links = [System.Collections.Generic.SortedDictionary[string, string]]::new(); error = '' } }
        $runId = [string]$(if ($result.PSObject.Properties['testRun']) { $result.testRun.id } else { $result.PSObject.Properties['runId']?.Value })
        # Later results (re-runs) replace earlier ones for the same platform.
        $groups[$name].links[$platform] = if ($BuildUrl) { Get-RunTestsResultUrl $BuildUrl $runId ([string]$result.id) } else { '' }
        $message = [string]$result.PSObject.Properties['errorMessage']?.Value
        if ($message -and -not $groups[$name].error) { $groups[$name].error = (($message -split '\r?\n') | Where-Object { $_.Trim() } | Select-Object -First 1) }
    }
    return @($groups.Keys | Sort-Object | ForEach-Object {
            [pscustomobject]@{ name = $_; platforms = @($groups[$_].links.Keys); links = $groups[$_].links; error = $groups[$_].error }
        })
}

# Heading of the section listing what runs. A neutral icon, so a failed build never shows a green tick there.
$script:RunTestsRunHeading = @{ running = '&#x1F9EA; Running'; done = '&#x1F9EA; Ran' }

function Update-RunTestsComment {
    <#
    .SYNOPSIS
    Rewrites the status line, Result badge, state marker and results block of an existing comment.
    #>
    param([Parameter(Mandatory)][string]$Body, [Parameter(Mandatory)]$Build, [Parameter(Mandatory)]$Results)
    $status = Get-RunTestsStatus -Selection ([pscustomobject]@{ mode = 'selected' }) -Build $Build -Results $Results
    $badge = Get-RunTestsBadge 'Result' $status.badge $status.color
    $updated = [regex]::Replace($Body, '(?m)^  <img alt="Result [^"]*"[^\n]*$', { param($m) $badge })
    $updated = [regex]::Replace($updated, '<!-- review-run-tests-state: [a-z-]+ -->', "<!-- review-run-tests-state: $($status.state) -->")
    # A section of a `/review run-tests` comment also records its result for the overview table.
    $updated = [regex]::Replace($updated, '<!-- run-tests-result: [A-Z ]+ -->', "<!-- run-tests-result: $($status.badge) -->")
    $updated = [regex]::Replace($updated, '(?s)<!-- run-tests-status -->.*?<!-- /run-tests-status -->',
        { param($m) "<!-- run-tests-status -->`n$($status.line)`n<!-- /run-tests-status -->" })
    # "Running" while the build is in progress (or re-running), "Ran" once it has finished.
    $heading = $script:RunTestsRunHeading[$(if ($Results -and $Results.completed) { 'done' } else { 'running' })]
    $updated = [regex]::Replace($updated, '<strong>(?:&#x2705;|&#x1F9EA;) (?:Running|Ran)</strong>', { param($m) "<strong>$heading</strong>" })
    $block = Format-RunTestsResultsBlock $Results
    $updated = [regex]::Replace($updated, '(?s)<!-- run-tests-results -->.*?<!-- /run-tests-results -->',
        { param($m) "<!-- run-tests-results -->`n$block`n<!-- /run-tests-results -->" })
    return $updated
}

function New-RunTestsRunCheckUpdate {
    <#
    .SYNOPSIS
    PATCH body for the "maui-pr-uitests (selected run)" check from the build's current state.
    #>
    param([Parameter(Mandatory)]$Build, [Parameter(Mandatory)]$Results)
    $title = "$($script:RunTestsPipelineName) #$($Build.id): $($Results.headline)"
    $webUrl = [string]$Build._links.web.href
    $failed = @($Results.jobs | Where-Object { $_.result -eq 'failed' } | ForEach-Object {
            $url = [string]$_.PSObject.Properties['url']?.Value
            if ($url) { "- [$($_.name)]($url)" } else { "- $($_.name)" } })
    $tests = @($Results.PSObject.Properties['failedTests']?.Value | Where-Object { $_ } | Select-Object -First $script:RunTestsMaxFailedTests |
        ForEach-Object { "- ``$(Format-RunTestsText $_.name)`` on $(Format-RunTestsFailedTestPlatforms $_)" })
    $summary = @("[$($script:RunTestsPipelineName) #$($Build.id)]($webUrl) &#xB7; [All test results]($(Get-RunTestsResultUrl $webUrl))", '')
    if ($tests.Count) { $summary += @('Failed tests:') + $tests + @('') }
    if ($failed.Count) { $summary += @('Failed jobs:') + $failed }
    $update = [ordered]@{ output = [ordered]@{ title = (Format-RunTestsText $title 200); summary = ($summary -join "`n") } }
    if ($Results.completed) {
        $update.status = 'completed'
        $update.conclusion = switch ($Results.result) { 'succeeded' { 'success' } 'canceled' { 'cancelled' } 'partiallySucceeded' { 'failure' } default { 'failure' } }
        $update.completed_at = (Get-Date).ToUniversalTime().ToString('o')
    } else {
        $update.status = 'in_progress'
    }
    return $update
}

function New-RunTestsJobChecks {
    <#
    .SYNOPSIS
    One check run per job of the dispatched build, for the PR head.

    .DESCRIPTION
    The build runs on a run-tests/* dispatch commit, so Azure Pipelines reports its jobs there and
    the PR's checks panel never shows them. These mirror each job ("maui-pr-uitests (Android UITests
    ... Shell1)") with its state and a link to its log. external_id ("job:<build id>") marks them
    as job checks, so a later build on the same commit can retire the ones it does not run.
    #>
    param([Parameter(Mandatory)]$Build, [Parameter(Mandatory)]$Results)
    $buildUrl = [string]$Build._links.web.href
    return @($Results.jobs | ForEach-Object {
            $name = Format-RunTestsText "$($script:RunTestsCheckPrefix) ($($_.name))" 180
            $url = if ($_.url) { [string]$_.url } else { $buildUrl }
            $check = [ordered]@{
                name = $name; details_url = $url; external_id = "job:$($Build.id)"
                output = [ordered]@{
                    title = "$($_.name): $($_.result)"
                    summary = "[$($_.name)]($url) in [$($script:RunTestsPipelineName) #$($Build.id)]($buildUrl), the run selected by ``/review`` for this commit."
                }
            }
            switch ($_.result) {
                { $_ -in 'succeeded', 'succeededWithIssues' } { $check.status = 'completed'; $check.conclusion = 'success' }
                'failed' { $check.status = 'completed'; $check.conclusion = 'failure' }
                { $_ -in 'canceled', 'abandoned' } { $check.status = 'completed'; $check.conclusion = 'cancelled' }
                'pending' { $check.status = 'queued' }
                default { $check.status = 'in_progress' }
            }
            [pscustomobject]$check
        })
}

function Get-RunTestsStatus {
    param($Selection, $Build, [string]$QueueError, $Results)
    if ($QueueError) {
        return @{ state = 'error'; badge = 'NOT QUEUED'; color = 'cf222e'; line = "&#x274C; **Could not queue ``$($script:RunTestsPipelineName)``:** $(Format-RunTestsText $QueueError 400)" }
    }
    if ($Selection.mode -eq 'none') {
        return @{ state = 'done'; badge = 'NOTHING TO RUN'; color = '6e7681'; line = "&#x2139;&#xFE0F; The AI found no changes relevant to $($script:RunTestsKind), so no ``$($script:RunTestsPipelineName)`` run was queued." }
    }
    if (-not $Build) {
        return @{ state = 'dry-run'; badge = 'DRY RUN'; color = '6e7681'; line = '&#x2139;&#xFE0F; Dry run &#x2014; nothing was queued.' }
    }
    $link = "[$($script:RunTestsPipelineName) #$($Build.id)]($($Build._links.web.href))"
    if ($Results -and $Results.completed) {
        switch ($Results.result) {
            'succeeded' { return @{ state = 'done'; badge = 'PASSED'; color = '2da44e'; line = "&#x2705; $link passed." } }
            'canceled' { return @{ state = 'done'; badge = 'CANCELED'; color = '6e7681'; line = "&#x26AA; $link was canceled." } }
            default {
                $tests = @($Results.PSObject.Properties['failedTests']?.Value | Where-Object { $_ })
                $jobs = @($Results.PSObject.Properties['jobs']?.Value | Where-Object { $_.result -eq 'failed' })
                $what = if ($tests.Count) {
                    $platforms = @($tests | ForEach-Object { $_.platforms } | Sort-Object -Unique) -join ', '
                    " &#x2014; $(Format-RunTestsCount $tests.Count 'test' 'tests') failed on $platforms. See **Failed tests** below."
                } elseif ($jobs.Count) { " &#x2014; $(Format-RunTestsCount $jobs.Count 'job' 'jobs') failed. See **Results** below." } else { '.' }
                return @{ state = 'done'; badge = 'FAILED'; color = 'cf222e'; line = "&#x274C; $link $($Results.result)$what" }
            }
        }
    }
    return @{ state = 'running'; badge = 'RUNNING'; color = '9a6700'; line = "&#x23F3; $link is running. The ``$($script:RunTestsRunCheckName)`` check on this PR follows it, and this comment updates when it finishes." }
}

function ConvertTo-RunTestsResults {
    <#
    .SYNOPSIS
    Summarizes an Azure DevOps build + timeline into the comment's results block.
    #>
    param([Parameter(Mandatory)]$Build, [AllowNull()]$Timeline, [AllowNull()]$FailedTests)
    $jobs = @()
    $webUrl = [string]$Build.PSObject.Properties['_links']?.Value.web.href
    if ($Timeline -and $Timeline.records) {
        $byId = @{}
        foreach ($record in $Timeline.records) { if ($record.id) { $byId[[string]$record.id] = $record } }
        $jobs = @($Timeline.records | Where-Object { $_.type -eq 'Job' -and $_.result -ne 'skipped' } | ForEach-Object {
                # Job -> Phase -> Stage; show "Stage Job" like the Azure Pipelines check names.
                $stage = $null
                $parent = $byId[[string]$_.parentId]
                while ($parent -and $parent.type -ne 'Stage') { $parent = $byId[[string]$parent.parentId] }
                if ($parent) { $stage = [string]$parent.name }
                $name = if ($stage) { "$stage $($_.name)" } else { [string]$_.name }
                $url = if ($webUrl -and $_.id) { "$webUrl&view=logs&j=$($_.id)" } else { $null }
                [pscustomobject]@{ name = $name; result = $(if ($_.result) { [string]$_.result } else { [string]$_.state }); url = $url }
            } | Sort-Object @{ Expression = { $_.result -ne 'failed' } }, name)
    }
    $completed = $Build.status -eq 'completed'
    $failed = @($jobs | Where-Object { $_.result -eq 'failed' }).Count
    $passed = @($jobs | Where-Object { $_.result -eq 'succeeded' }).Count
    $headline = if ($completed) { "$($Build.result): $passed passed, $failed failed of $($jobs.Count) jobs" } else { "running: $passed passed, $failed failed so far" }
    return [pscustomobject]@{
        completed = $completed
        result = [string]$Build.result
        jobs = $jobs
        buildUrl = $webUrl
        failedTests = @($FailedTests | Where-Object { $_ })
        rerun = [bool]@($Timeline.records | Where-Object { $_.type -eq 'Job' -and [int]($_.attempt ?? 1) -gt 1 }).Count
        headline = $headline
    }
}

# ---------------------------------------------------------------------------
# Which categories have at least one enabled UI test on each platform.
# Shared UI tests are compiled into every platform project; each project defines the
# TEST_FAILS_ON_* symbols of the *other* platforms, so `#if TEST_FAILS_ON_WINDOWS`
# compiles a test everywhere except Windows.
# ---------------------------------------------------------------------------

$script:RunTestsPlatformSymbols = @{
    android     = @('ANDROID', 'TEST_FAILS_ON_IOS', 'TEST_FAILS_ON_CATALYST', 'TEST_FAILS_ON_WINDOWS')
    ios         = @('IOS', 'IOSUITEST', 'TEST_FAILS_ON_ANDROID', 'TEST_FAILS_ON_WINDOWS', 'TEST_FAILS_ON_CATALYST')
    maccatalyst = @('MACCATALYST', 'MACUITEST', 'TEST_FAILS_ON_ANDROID', 'TEST_FAILS_ON_WINDOWS', 'TEST_FAILS_ON_IOS')
    windows     = @('WINDOWS', 'WINTEST', 'TEST_FAILS_ON_ANDROID', 'TEST_FAILS_ON_CATALYST', 'TEST_FAILS_ON_IOS')
}

$script:RunTestsPlatformProjects = @{
    android     = 'TestCases.Android.Tests'
    ios         = 'TestCases.iOS.Tests'
    maccatalyst = 'TestCases.Mac.Tests'
    windows     = 'TestCases.WinUI.Tests'
}


function Test-RunTestsPreprocessorExpression {
    <#
    .SYNOPSIS
    Evaluates a C# `#if` expression (identifiers, true/false, !, &&, ||, parentheses).
    Undefined symbols are false, as in C#.
    #>
    param([Parameter(Mandatory)][string]$Expression, [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Symbols)
    $tokens = @([regex]::Matches(($Expression -replace '//.*$', ''), '&&|\|\||!|\(|\)|[A-Za-z_][A-Za-z0-9_]*') | ForEach-Object Value)
    $state = @{ i = 0 }
    $peek = { if ($state.i -lt $tokens.Count) { $tokens[$state.i] } else { $null } }
    $parseOr = $null; $parseAnd = $null; $parseUnary = $null
    $parseUnary = {
        $t = & $peek
        if ($t -eq '!') { $state.i++; return -not (& $parseUnary) }
        if ($t -eq '(') { $state.i++; $v = & $parseOr; if ((& $peek) -eq ')') { $state.i++ }; return $v }
        $state.i++
        if ($t -ceq 'true') { return $true }
        if ($t -ceq 'false') { return $false }
        return ($t -cin $Symbols)
    }
    $parseAnd = {
        $v = & $parseUnary
        while ((& $peek) -eq '&&') { $state.i++; $r = & $parseUnary; $v = $v -and $r }
        return $v
    }
    $parseOr = {
        $v = & $parseAnd
        while ((& $peek) -eq '||') { $state.i++; $r = & $parseAnd; $v = $v -or $r }
        return $v
    }
    return [bool](& $parseOr)
}

function Get-RunTestsFileCategories {
    <#
    .SYNOPSIS
    Category values of the tests that are compiled in a source file for the given symbols.
    #>
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Symbols,
        [Parameter(Mandatory)][hashtable]$CategoryConstants
    )
    $found = [System.Collections.Generic.HashSet[string]]::new()
    # Each frame: parent active, any branch taken, this branch active.
    $stack = [System.Collections.Generic.Stack[object]]::new()
    $active = $true
    foreach ($line in $Content -split "`n") {
        $trim = $line.Trim()
        if ($trim -match '^#\s*if\s+(.+)$') {
            $value = Test-RunTestsPreprocessorExpression $Matches[1] $Symbols
            $stack.Push(@{ parent = $active; taken = $value })
            $active = $active -and $value
            continue
        }
        if ($trim -match '^#\s*elif\s+(.+)$' -and $stack.Count) {
            $frame = $stack.Peek()
            $value = (-not $frame.taken) -and (Test-RunTestsPreprocessorExpression $Matches[1] $Symbols)
            if ($value) { $frame.taken = $true }
            $active = $frame.parent -and $value
            continue
        }
        if ($trim -match '^#\s*else\b' -and $stack.Count) {
            $frame = $stack.Peek()
            $active = $frame.parent -and -not $frame.taken
            $frame.taken = $true
            continue
        }
        if ($trim -match '^#\s*endif\b' -and $stack.Count) {
            $active = $stack.Pop().parent
            continue
        }
        if (-not $active -or $trim.StartsWith('//')) { continue }
        foreach ($m in [regex]::Matches($trim, '(?:\[|,)\s*(?:NUnit\.Framework\.)?(ShardedTestCategory|Category)\s*\(\s*UITestCategories\.(\w+)(?:\s*,\s*(?:shard\s*:\s*)?(\d+))?')) {
            $name = $m.Groups[2].Value
            $value = if ($CategoryConstants.ContainsKey($name)) { $CategoryConstants[$name] } else { $name }
            [void]$found.Add($value)
            if ($m.Groups[1].Value -eq 'ShardedTestCategory') {
                $shard = if ($m.Groups[3].Success) { $m.Groups[3].Value } else { '1' }
                [void]$found.Add("$value$shard")
            }
        }
    }
    return @($found)
}

function Get-RunTestsCategoryConstants {
    param([Parameter(Mandatory)][string]$TestsRoot)
    $constants = @{}
    $source = Join-Path $TestsRoot 'TestCases.Shared.Tests/UITestCategories.cs'
    foreach ($match in [regex]::Matches((Get-Content -LiteralPath $source -Raw), 'const\s+string\s+(\w+)\s*=\s*"([^"]+)"')) {
        $constants[$match.Groups[1].Value] = $match.Groups[2].Value
    }
    return $constants
}

function Get-RunTestsRunnableCategories {
    <#
    .SYNOPSIS
    Map of platform id -> sorted list of categories with at least one enabled UI test.
    .PARAMETER TestsRoot
    src/Controls/tests
    #>
    param([Parameter(Mandatory)][string]$TestsRoot)
    $constants = Get-RunTestsCategoryConstants $TestsRoot
    $shared = @(Get-ChildItem -LiteralPath (Join-Path $TestsRoot 'TestCases.Shared.Tests') -Recurse -Filter '*.cs' -File |
        Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' } | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw })
    $result = [ordered]@{}
    foreach ($platform in Get-RunTestsPlatformIds) {
        $symbols = $script:RunTestsPlatformSymbols[$platform]
        $set = [System.Collections.Generic.HashSet[string]]::new()
        $own = Join-Path $TestsRoot $script:RunTestsPlatformProjects[$platform]
        $files = @($shared)
        if (Test-Path -LiteralPath $own) {
            $files += @(Get-ChildItem -LiteralPath $own -Recurse -Filter '*.cs' -File |
                Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' } | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw })
        }
        foreach ($content in $files) {
            if (-not $content -or $content -notmatch 'UITestCategories\.') { continue }
            foreach ($c in Get-RunTestsFileCategories -Content $content -Symbols $symbols -CategoryConstants $constants) { [void]$set.Add($c) }
        }
        $result[$platform] = @($set | Sort-Object)
    }
    return $result
}


# Models allowed to select tests, with the names shown in comments.
$script:RunTestsModels = [ordered]@{ 'gpt-6.2-sol' = 'GPT-6.2 Sol'; 'mai-code-2' = 'MAI Code 2' }

function Get-RunTestsSelectedByText {
    <#
    .SYNOPSIS
    "AI-selected UI tests (GPT-6.2 Sol + MAI Code 2)" for the comment header; -Short for the rationale heading.
    #>
    param([Parameter(Mandatory)]$Selection, [switch]$Short)
    $names = @($Selection.PSObject.Properties['models']?.Value | Where-Object { $_ } | ForEach-Object { $_.displayName })
    if ($Short) { return $(if ($names.Count) { "AI rationale ($($names -join ' + '))" } else { 'AI rationale' }) }
    return $(if ($names.Count) { "AI-selected $($script:RunTestsKind) ($($names -join ' + '))" } else { "AI-selected $($script:RunTestsKind)" })
}

function ConvertTo-RunTestsPipelineCategories {
    <#
    .SYNOPSIS
    Encodes a per-platform selection as the pipeline's selectedCategories list.
    .DESCRIPTION
    When every selected platform runs the same categories, the list holds plain names. Otherwise
    EVERY entry is platform-qualified (android:Button, ios:Label, android:Material3). The template
    guards in eng/pipelines/common/ui-tests.yml rely on a list never mixing the two forms: plain
    names apply to every selected platform, and in a qualified list each stage looks only at its
    own platform's entries.
    #>
    param([string[]]$Platforms, $PlatformCategories, [string[]]$Categories)
    $uniform = $true
    foreach ($c in $Categories) {
        foreach ($platform in $Platforms) {
            if ($c -cnotin @($PlatformCategories[$platform])) { $uniform = $false }
        }
    }
    if ($uniform) { return @($Categories) }
    $qualified = foreach ($c in $Categories) {
        foreach ($platform in $Platforms) {
            if ($c -cin @($PlatformCategories[$platform])) { "$($script:RunTestsPipelinePlatform[$platform]):$c" }
        }
    }
    return @($qualified)
}


function Get-RunTestsMergeState {
    <#
    .SYNOPSIS
    Decides whether GitHub's test-merge commit can be built for the selected head.
    Returns 'ready' (it merges exactly that head), 'wait' (GitHub has not recomputed it yet, e.g.
    it still merges the previous head) or 'moved' (the PR closed, moved, was retargeted or conflicts).
    #>
    param([Parameter(Mandatory)]$PullRequest, [AllowNull()]$MergeCommit, [Parameter(Mandatory)][string]$HeadSha, [string]$BaseRef)
    if ($PullRequest.state -ne 'open' -or $PullRequest.head.sha -cne $HeadSha) { return 'moved' }
    if ($BaseRef -and $PullRequest.base.ref -cne $BaseRef) { return 'moved' }
    if ($PullRequest.mergeable -eq $false) { return 'moved' }
    if (-not $PullRequest.merge_commit_sha -or -not $MergeCommit) { return 'wait' }
    if ($MergeCommit.sha -and $MergeCommit.sha -cne $PullRequest.merge_commit_sha) { return 'wait' }
    $parents = @($MergeCommit.parents)
    if ($parents.Count -eq 2 -and $parents[1].sha -ceq $HeadSha) { return 'ready' }
    return 'wait'
}

# The fields of a model's run_selected_ui_tests proposal.
$script:RunTestsProposalKeys = 'platforms', 'categories', 'full_run', 'summary', 'rationale'

function Get-RunTestsSuiteProposal {
    <#
    .SYNOPSIS
    One suite's part of a model's proposal. In a `/review run-tests` request the UI selection is in
    platforms/categories/full_run and the device selection in device_platforms/device_categories/
    device_full_run; a model that left the device half out selected no device tests.
    #>
    param([System.Collections.IDictionary]$Proposal, [string]$Suite, [switch]$Combined)
    if (-not $Combined -or $Suite -eq 'ui') {
        $selected = @{}
        foreach ($key in $script:RunTestsProposalKeys) { if ($Proposal.Contains($key)) { $selected[$key] = $Proposal[$key] } }
        return $selected
    }
    $selected = @{ platforms = '[]'; categories = '[]'; full_run = $false }
    foreach ($key in $script:RunTestsProposalKeys) { if ($Proposal.Contains("device_$key")) { $selected[$key] = $Proposal["device_$key"] } }
    # Without its own summary or rationale the device half shares the request's.
    foreach ($key in 'summary', 'rationale') { if (-not $selected.Contains($key) -and $Proposal.Contains($key)) { $selected[$key] = $Proposal[$key] } }
    return $selected
}

function Merge-RunTestsSelections {
    <#
    .SYNOPSIS
    Combines the selections of several models: each platform runs the union of the categories the
    models chose for it. A test runs only if at least one model selected it.

    .PARAMETER Selections
    Ordered dictionary of model id -> selection from Resolve-RunTestsSelection.
    #>
    param([Parameter(Mandatory)][System.Collections.IDictionary]$Selections, [Parameter(Mandatory)]$Inventory)
    $models = @(foreach ($id in $Selections.Keys) {
            $s = $Selections[$id]
            [pscustomobject]@{
                id = $id; displayName = $(if ($script:RunTestsModels.Contains($id)) { $script:RunTestsModels[$id] } else { $id })
                mode = $s.mode; platformCategories = $s.platformCategories; summary = $s.summary; rationale = $s.rationale
            }
        })
    $all = @($Selections.Values)
    $notes = [System.Collections.Generic.List[string]]::new()
    foreach ($id in $Selections.Keys) {
        foreach ($note in $Selections[$id].notes) { $notes.Add("$($script:RunTestsModels[$id] ?? $id): $note") }
    }
    $allCategories = Get-RunTestsAllCategories $Inventory
    $summary = (@($models | ForEach-Object { "**$($_.displayName):** $($_.summary)" }) -join '<br/>')
    $base = [ordered]@{
        mode = 'none'; platforms = @(); categories = @(); expandedCategories = @(); platformCategories = [ordered]@{}
        pipelineCategories = @(); summary = $summary; rationale = $(if ($models.Count -eq 1) { $models[0].rationale } else { '' }); notes = @($notes); models = $models
    }
    if (@($all | Where-Object { $_.mode -eq 'full' }).Count) {
        $full = @($all | Where-Object { $_.mode -eq 'full' })[0]
        $base.mode = 'full'; $base.platforms = @($full.platforms); $base.expandedCategories = @($full.expandedCategories)
        return [pscustomobject]$base
    }
    $active = @($all | Where-Object { $_.mode -eq 'selected' })
    if (-not $active.Count) { return [pscustomobject]$base }

    $map = [ordered]@{}
    foreach ($platform in Get-RunTestsPlatformIds) {
        $set = @($active | ForEach-Object { if ($_.platformCategories -and $_.platformCategories.Contains($platform)) { @($_.platformCategories[$platform]) } } | Where-Object { $_ })
        if ($set.Count) { $map[$platform] = @($allCategories | Where-Object { $_ -cin $set }) }
    }
    $platforms = @($map.Keys)
    $expanded = @($allCategories | Where-Object { $c = $_; @($platforms | Where-Object { $c -cin $map[$_] }).Count })
    $pipeline = [System.Collections.Generic.List[string]]::new()
    $everythingEverywhere = -not @($active | Where-Object { $_.categories.Count -gt 0 }).Count
    if (-not $everythingEverywhere) {
        foreach ($c in (ConvertTo-RunTestsPipelineCategories -Platforms $platforms -PlatformCategories $map -Categories $expanded)) { $pipeline.Add($c) }
    }
    $base.mode = 'selected'
    $base.platforms = $platforms
    $base.categories = $(if ($everythingEverywhere) { @() } else { $expanded })
    $base.expandedCategories = $expanded
    $base.platformCategories = $map
    $base.pipelineCategories = @($pipeline)
    return [pscustomobject]$base
}

Set-RunTestsSuite 'ui'
