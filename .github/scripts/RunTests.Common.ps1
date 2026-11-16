# Shared helpers for `/review run-tests`: AI-selected UI tests for a PR.
#
# The flow is:
#   1. Get-RunTestsContext.ps1   collects the PR diff and the UI test inventory (trusted).
#   2. The agent proposes platforms + categories (copilot-review-run-tests.md).
#   3. Invoke-RunTests.ps1       validates the proposal, queues maui-pr-uitests as a
#                                pull-request build (so Azure Pipelines reports native
#                                checks on the PR head), adds "skipped" check runs for
#                                everything that was not selected, and posts a comment.
#   4. Update-RunTestsResults.ps1 refreshes the comment when the build finishes.
#
# Everything here except Get-RunTestsAzdoToken is pure (no network) so it can be unit tested.

$script:RunTestsPlatforms = [ordered]@{
    android     = 'Android'
    ios         = 'iOS'
    maccatalyst = 'MacCatalyst'
    windows     = 'Windows'
}

# Pipeline parameter values differ from the user-facing names for MacCatalyst only.
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

$script:RunTestsMaterial3Controls = @('CheckBox', 'Editor', 'Entry', 'Image', 'Label', 'ProgressBar', 'SearchBar', 'Slider', 'Switch')

$script:RunTestsCommentMarker = '<!-- review-run-tests -->'
$script:RunTestsCheckPrefix = 'maui-pr-uitests'
$script:RunTestsSelectionCheckName = 'maui-pr-uitests (test selection)'
$script:RunTestsSkippedCheckPrefix = 'maui-pr-uitests (skipped)'

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
        if ($item -isnot [string] -or $item -cnotmatch '^[A-Za-z0-9_]{1,64}$') {
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
        [Parameter(Mandatory)]$Inventory
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
        # Every requested category was unknown: never risk a filter that runs zero tests.
        $notes.Add('No requested category matched the inventory; running all categories on the selected platforms.')
        $mode = 'selected'
    } else {
        $mode = 'selected'
    }

    # Material3 only exists on Android. Drop it rather than queue an empty lane elsewhere.
    if ($mode -eq 'selected' -and $validCategories.Count -gt 0 -and
        @($validCategories | Where-Object { $_ -cne 'Material3' }).Count -eq 0 -and 'android' -cnotin $orderedPlatforms) {
        $notes.Add('Material3 tests only run on Android; added Android.')
        $orderedPlatforms = @($knownPlatforms | Where-Object { $_ -in (@($orderedPlatforms) + 'android') })
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

    # eng/pipelines/common/ui-tests.yml also runs the Android Material3 lane for these controls.
    if ($mode -eq 'selected' -and 'android' -cin $orderedPlatforms -and -not $expanded.Contains('Material3') -and
        @($expanded | Where-Object { $_ -match 'Button|Picker' -or $_ -cin $script:RunTestsMaterial3Controls }).Count) {
        $expanded.Add('Material3')
        $notes.Add('Android Material3 tests also run because a Material3-styled control was selected.')
    }

    return [pscustomobject]@{
        mode = $mode
        platforms = @($orderedPlatforms)
        categories = @($validCategories)
        expandedCategories = @($allCategories | Where-Object { $expanded.Contains($_) })
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
    $platforms = @($Selection.platforms | ForEach-Object { $script:RunTestsPipelinePlatform[$_] })
    return [ordered]@{
        selectedRun        = 'true'
        selectedPlatforms  = (ConvertTo-Json -InputObject @($platforms) -Compress)
        selectedCategories = (ConvertTo-Json -InputObject @($Selection.categories) -Compress)
    }
}

function New-RunTestsQueueRequest {
    <#
    .SYNOPSIS
    Build API request that queues maui-pr-uitests as a pull-request build.

    .DESCRIPTION
    reason=pullRequest plus triggerInfo.pr.sourceSha makes Azure Pipelines report the
    build and every job as check runs on the PR head commit, so they show up in the
    PR's checks section exactly like an `/azp run` build. (The Runs API, or queuing a
    branch build, reports against a different commit and never appears on the PR.)
    #>
    param(
        [Parameter(Mandatory)]$PullRequest,
        [Parameter(Mandatory)]$Selection,
        [int]$DefinitionId = 313
    )
    $isFork = "$($PullRequest.head.repo.full_name -cne $PullRequest.base.repo.full_name)"
    $request = [ordered]@{
        definition    = [ordered]@{ id = $DefinitionId }
        reason        = 'pullRequest'
        sourceBranch  = "refs/pull/$($PullRequest.number)/merge"
        sourceVersion = $PullRequest.merge_commit_sha
        triggerInfo   = [ordered]@{
            'pr.sourceBranch' = $PullRequest.head.ref
            'pr.sourceSha'    = $PullRequest.head.sha
            'pr.targetBranch' = $PullRequest.base.ref
            'pr.id'           = "$($PullRequest.id)"
            'pr.title'        = $PullRequest.title
            'pr.number'       = "$($PullRequest.number)"
            'pr.isFork'       = $isFork
            'pr.draft'        = "$([bool]$PullRequest.draft)"
            'pr.providerId'   = 'github'
            'pr.autoCancel'   = 'false'
        }
    }
    $parameters = Get-RunTestsTemplateParameters $Selection
    if ($parameters.Count) { $request.templateParameters = $parameters }
    return $request
}

function Test-RunTestsQueuedSelection {
    <#
    .SYNOPSIS
    True when Azure DevOps applied the requested runtime parameters to the queued build.
    #>
    param([Parameter(Mandatory)]$Build, [Parameter(Mandatory)]$Selection)
    $expected = Get-RunTestsTemplateParameters $Selection
    if (-not $expected.Count) { return $true }
    $actual = $Build.templateParameters
    if ($null -eq $actual) { return $false }
    foreach ($key in $expected.Keys) {
        $value = if ($actual -is [System.Collections.IDictionary]) { $actual[$key] } else { $actual.PSObject.Properties[$key]?.Value }
        if ($null -eq $value) { return $false }
        if ($key -eq 'selectedRun') {
            if ("$value" -notin @('true', 'True')) { return $false }
            continue
        }
        $normalized = ConvertTo-Json -InputObject @(ConvertFrom-Json -InputObject "$value") -Compress
        if ($normalized -cne $expected[$key]) { return $false }
    }
    return $true
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
            name = "$($script:RunTestsSkippedCheckPrefix) $name UITests"
            title = "$name UI tests were not selected"
            kind = 'platform'
            key = $platform
        })
    }
    if ($Selection.mode -eq 'selected') {
        $running = $Selection.expandedCategories
        foreach ($category in Get-RunTestsAllCategories $Inventory) {
            if ($category -in $running) { continue }
            $checks.Add([pscustomobject]@{
                name = "$($script:RunTestsSkippedCheckPrefix) $category"
                title = "$category UI tests were not selected"
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

function New-RunTestsComment {
    <#
    .SYNOPSIS
    Renders the PR comment, in the same style as the other `/review` reports.
    #>
    param(
        [Parameter(Mandatory)]$Selection,
        [Parameter(Mandatory)]$Inventory,
        [Parameter(Mandatory)]$PullRequest,
        [Parameter(Mandatory)][string]$WorkflowRunUrl,
        [AllowNull()]$Build,
        [string]$QueueError,
        [string]$Requester,
        [AllowNull()]$Results,
        [switch]$SelectionOnly
    )

    $sha = [string]$PullRequest.head.sha
    $short = $sha.Substring(0, 8)
    $repo = [string]$PullRequest.base.repo.full_name
    $allCategories = Get-RunTestsAllCategories $Inventory
    $runCount = if ($Selection.mode -eq 'none') { 0 } else { $Selection.expandedCategories.Count }
    $skippedCategories = @($allCategories | Where-Object { $_ -notin $Selection.expandedCategories })
    $skippedPlatforms = @(Get-RunTestsPlatformIds | Where-Object { $_ -notin $Selection.platforms })

    $scope = switch ($Selection.mode) {
        'full' { 'Full suite' }
        'none' { 'No UI tests' }
        default { 'Targeted' }
    }
    $scopeColor = switch ($Selection.mode) { 'full' { '9a6700' } 'none' { '6e7681' } default { '1f6feb' } }
    $platformText = if ($Selection.platforms.Count) { ($Selection.platforms | ForEach-Object { Get-RunTestsPlatformName $_ }) -join ', ' } else { 'none' }

    $status = Get-RunTestsStatus -Selection $Selection -Build $Build -QueueError $QueueError -Results $Results
    if ($SelectionOnly -and -not $QueueError -and $Selection.mode -ne 'none') {
        $status = @{ state = 'selection-only'; badge = 'SELECTION ONLY'; color = '6e7681'; line = '&#x2139;&#xFE0F; Selection only &#x2014; Azure DevOps dispatch is not available in this repository, so `maui-pr-uitests` was not queued.' }
    }
    $who = if ($Requester -cmatch '^[A-Za-z0-9][A-Za-z0-9-]{0,38}$') { "@$Requester" } else { 'Requested' }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add($script:RunTestsCommentMarker)
    $lines.Add("<!-- review-run-tests-head: $sha -->")
    if ($Build) { $lines.Add("<!-- review-run-tests-build: $($Build.id) -->") }
    $lines.Add("<!-- review-run-tests-state: $($status.state) -->")
    $lines.Add('')
    $lines.Add('## Selected UI Tests')
    $lines.Add('')
    $lines.Add("> $who &#x2014; AI-selected UI tests for commit [``$short``](https://github.com/$repo/commit/$sha).")
    $lines.Add('')
    $lines.Add('<p align="left">')
    $lines.Add((Get-RunTestsBadge 'Result' $status.badge $status.color))
    $lines.Add((Get-RunTestsBadge 'Scope' $scope $scopeColor))
    $lines.Add((Get-RunTestsBadge 'Platforms' $platformText))
    $lines.Add((Get-RunTestsBadge 'Categories' "$runCount of $($allCategories.Count)"))
    $lines.Add((Get-RunTestsBadge 'Commit' $short))
    $lines.Add('</p>')
    $lines.Add('')
    if ($Selection.summary) { $lines.Add($Selection.summary); $lines.Add('') }
    $lines.Add('<!-- run-tests-status -->')
    $lines.Add($status.line)
    $lines.Add('<!-- /run-tests-status -->')
    $lines.Add('')
    $lines.Add('---')
    $lines.Add('')

    # What runs
    $lines.Add('<details>')
    $lines.Add("<summary><strong>&#x2705; Running</strong> &#x2014; $(Format-RunTestsCount $runCount 'category' 'categories') on $platformText</summary>")
    $lines.Add('<br/>')
    $lines.Add('')
    switch ($Selection.mode) {
        'full' { $lines.Add('The complete `maui-pr-uitests` suite runs on every platform (same as `/azp run`).') }
        'none' { $lines.Add('No UI tests run for this PR.') }
        default {
            $lines.Add('| Platform | Categories |')
            $lines.Add('| --- | --- |')
            foreach ($platform in $Selection.platforms) {
                $categoriesForPlatform = @($Selection.expandedCategories | Where-Object { $_ -cne 'Material3' -or $platform -eq 'android' })
                $text = if ($Selection.categories.Count -eq 0) { '_all categories_' } else { Format-RunTestsCategoryList $categoriesForPlatform }
                $lines.Add("| $(Get-RunTestsPlatformName $platform) | $text |")
            }
        }
    }
    $lines.Add('')
    $lines.Add('</details>')
    $lines.Add('')

    # What was skipped
    if ($Selection.mode -ne 'full') {
        $skipCount = if ($Selection.mode -eq 'none') { $allCategories.Count } else { $skippedCategories.Count }
        $lines.Add('<details>')
        $lines.Add("<summary><strong>&#x23ED;&#xFE0F; Skipped</strong> &#x2014; $(Format-RunTestsCount $skipCount 'category' 'categories'), $(Format-RunTestsCount $skippedPlatforms.Count 'platform' 'platforms')</summary>")
        $lines.Add('<br/>')
        $lines.Add('')
        if ($skippedPlatforms.Count) {
            $lines.Add("**Platforms not run:** $(($skippedPlatforms | ForEach-Object { Get-RunTestsPlatformName $_ }) -join ', ')")
            $lines.Add('')
        }
        if ($Selection.mode -eq 'selected' -and $skippedCategories.Count) {
            $lines.Add("**Categories not run:** $(Format-RunTestsCategoryList $skippedCategories)")
            $lines.Add('')
        }
        $lines.Add('Each skipped platform and category is listed as a **Skipped** `maui-pr-uitests (skipped) …` check on this commit. Use `/azp run maui-pr-uitests` for the full suite.')
        $lines.Add('')
        $lines.Add('</details>')
        $lines.Add('')
    }

    $lines.Add('<!-- run-tests-results -->')
    $lines.Add((Format-RunTestsResultsBlock $Results))
    $lines.Add('<!-- /run-tests-results -->')
    # Why
    $lines.Add('<details>')
    $lines.Add('<summary><strong>&#x1F916; Why these tests</strong> &#x2014; AI rationale</summary>')
    $lines.Add('<br/>')
    $lines.Add('')
    $lines.Add($(if ($Selection.rationale) { $Selection.rationale } else { '_No rationale provided._' }))
    foreach ($note in $Selection.notes) { $lines.Add(''); $lines.Add("> [!NOTE]`n> $(Format-RunTestsText $note 300)") }
    $lines.Add('')
    $lines.Add('</details>')
    $lines.Add('')
    $lines.Add("<sub>Generated by [/review run-tests]($WorkflowRunUrl). A targeted run is a fast signal, not full-suite certification.</sub>")
    return ($lines -join "`n")
}

function Format-RunTestsResultsBlock {
    param([AllowNull()]$Results)
    if (-not $Results) { return '' }
    $lines = [System.Collections.Generic.List[string]]::new()
    $open = if ($Results.completed -and $Results.result -ne 'succeeded') { ' open' } else { '' }
    $lines.Add("<details$open>")
    $lines.Add("<summary><strong>&#x1F4CA; Results</strong> &#x2014; $($Results.headline)</summary>")
    $lines.Add('<br/>')
    $lines.Add('')
    $lines.Add('| Job | Result |')
    $lines.Add('| --- | --- |')
    foreach ($job in $Results.jobs) {
        $icon = switch ($job.result) { 'succeeded' { '&#x2705;' } 'failed' { '&#x274C;' } 'canceled' { '&#x26AA;' } 'succeededWithIssues' { '&#x26A0;&#xFE0F;' } default { '&#x23F3;' } }
        $lines.Add("| $($job.name -replace '\|', '/') | $icon $($job.result) |")
    }
    $lines.Add('')
    $lines.Add('</details>')
    $lines.Add('')
    return ($lines -join "`n")
}

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
    $updated = [regex]::Replace($updated, '(?s)<!-- run-tests-status -->.*?<!-- /run-tests-status -->',
        { param($m) "<!-- run-tests-status -->`n$($status.line)`n<!-- /run-tests-status -->" })
    $block = Format-RunTestsResultsBlock $Results
    $updated = [regex]::Replace($updated, '(?s)<!-- run-tests-results -->.*?<!-- /run-tests-results -->',
        { param($m) "<!-- run-tests-results -->`n$block`n<!-- /run-tests-results -->" })
    return $updated
}

function Get-RunTestsStatus {
    param($Selection, $Build, [string]$QueueError, $Results)
    if ($QueueError) {
        return @{ state = 'error'; badge = 'NOT QUEUED'; color = 'cf222e'; line = "&#x274C; **Could not queue `maui-pr-uitests`:** $(Format-RunTestsText $QueueError 400)" }
    }
    if ($Selection.mode -eq 'none') {
        return @{ state = 'done'; badge = 'NOTHING TO RUN'; color = '6e7681'; line = '&#x2139;&#xFE0F; The AI found no UI-test-relevant changes, so no `maui-pr-uitests` run was queued.' }
    }
    if (-not $Build) {
        return @{ state = 'dry-run'; badge = 'DRY RUN'; color = '6e7681'; line = '&#x2139;&#xFE0F; Dry run &#x2014; nothing was queued.' }
    }
    $link = "[maui-pr-uitests #$($Build.id)]($($Build._links.web.href))"
    if ($Results -and $Results.completed) {
        switch ($Results.result) {
            'succeeded' { return @{ state = 'done'; badge = 'PASSED'; color = '1a7f37'; line = "&#x2705; $link passed." } }
            'canceled' { return @{ state = 'done'; badge = 'CANCELED'; color = '6e7681'; line = "&#x26AA; $link was canceled." } }
            default { return @{ state = 'done'; badge = 'FAILED'; color = 'cf222e'; line = "&#x274C; $link $($Results.result). See the failing checks below or the run for details." } }
        }
    }
    return @{ state = 'running'; badge = 'RUNNING'; color = '9a6700'; line = "&#x23F3; $link is running. Its jobs appear as `maui-pr-uitests (…)` checks on this PR; this comment updates when it finishes." }
}

function ConvertTo-RunTestsResults {
    <#
    .SYNOPSIS
    Summarizes an Azure DevOps build + timeline into the comment's results block.
    #>
    param([Parameter(Mandatory)]$Build, [AllowNull()]$Timeline)
    $jobs = @()
    if ($Timeline -and $Timeline.records) {
        $jobs = @($Timeline.records | Where-Object { $_.type -eq 'Job' -and $_.result -ne 'skipped' } |
            Sort-Object { $_.name } | ForEach-Object {
                [pscustomobject]@{ name = [string]$_.name; result = $(if ($_.result) { [string]$_.result } else { [string]$_.state }) }
            })
    }
    $completed = $Build.status -eq 'completed'
    $failed = @($jobs | Where-Object { $_.result -eq 'failed' }).Count
    $passed = @($jobs | Where-Object { $_.result -eq 'succeeded' }).Count
    $headline = if ($completed) { "$($Build.result): $passed passed, $failed failed of $($jobs.Count) jobs" } else { "running: $passed passed, $failed failed so far" }
    return [pscustomobject]@{
        completed = $completed
        result = [string]$Build.result
        jobs = $jobs
        headline = $headline
    }
}

function Get-RunTestsAzdoToken {
    <#
    .SYNOPSIS
    Exchanges the workflow's GitHub OIDC token for an Azure DevOps bearer token (no PAT).
    Uses the same federated identity as review-trigger.yml and the CI-fix dispatcher.
    #>
    $tenantId = $env:AZDO_TRIGGER_TENANT_ID
    $clientId = $env:AZDO_TRIGGER_CLIENT_ID
    if (-not $tenantId -or -not $clientId) { throw 'AZDO_TRIGGER_TENANT_ID and AZDO_TRIGGER_CLIENT_ID must be set.' }
    if (-not $env:ACTIONS_ID_TOKEN_REQUEST_URL -or -not $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN) {
        throw 'GitHub OIDC is unavailable; the job needs id-token: write.'
    }
    $oidc = Invoke-RestMethod -Uri "$($env:ACTIONS_ID_TOKEN_REQUEST_URL)&audience=api://AzureADTokenExchange" `
        -Headers @{ Authorization = "Bearer $($env:ACTIONS_ID_TOKEN_REQUEST_TOKEN)" } -TimeoutSec 30
    Write-Host "::add-mask::$($oidc.value)"
    $token = Invoke-RestMethod -Method Post -Uri "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token" `
        -ContentType 'application/x-www-form-urlencoded' -TimeoutSec 30 -Body @{
            grant_type = 'client_credentials'
            client_id = $clientId
            client_assertion_type = 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'
            client_assertion = $oidc.value
            scope = '499b84ac-1321-427f-aa17-267ca6975798/.default'
        }
    Write-Host "::add-mask::$($token.access_token)"
    return $token.access_token
}
