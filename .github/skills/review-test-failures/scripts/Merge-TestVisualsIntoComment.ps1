#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Merges trusted visual comparisons into the single /review tests comment.

.DESCRIPTION
    Reads visual asset metadata produced by Publish-TestVisualAssets.ps1 and inserts
    bounded, expandable comparison panels into an existing analysis comment. The
    merger enforces gh-aw's final comment limits before changing any output.

    In AgentOutput mode, the script atomically updates the add_comment item in
    /tmp/gh-aw/agent_output.json. In CommentBody mode, it atomically updates a local
    Markdown comment file.
#>

[CmdletBinding(DefaultParameterSetName = "AgentOutput")]
param(
    [Parameter(Mandatory = $true)]
    [int]$PrNumber,

    [Parameter(Mandatory = $true)]
    [string]$ContextJsonPath,

    [Parameter(Mandatory = $false)]
    [string]$Repository = $env:GITHUB_REPOSITORY,

    [Parameter(Mandatory = $true, ParameterSetName = "AgentOutput")]
    [string]$AgentOutputPath,

    [Parameter(Mandatory = $true, ParameterSetName = "CommentBody")]
    [string]$CommentBodyPath,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 49)]
    [int]$MaxCommentUrls = 45,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 10)]
    [int]$MaxCommentMentions = 10,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1000, 65000)]
    [int]$MaxCommentCharacters = 60000
)

$ErrorActionPreference = "Stop"

function Get-InlineVisualStartMarker {
    return "<!-- Tests Failure Visuals Inline Start -->"
}

function Get-InlineVisualEndMarker {
    return "<!-- Tests Failure Visuals Inline End -->"
}

function Get-InlineVisualPlaceholder {
    return "<!-- GH_AW_TRUSTED_VISUALS -->"
}

function Get-BoundedText {
    param(
        [string]$Value,
        [int]$MaximumLength
    )

    if ([string]::IsNullOrEmpty($Value) -or $Value.Length -le $MaximumLength) {
        return $Value
    }
    if ($MaximumLength -le 3) {
        return $Value.Substring(0, $MaximumLength)
    }
    return $Value.Substring(0, $MaximumLength - 3) + "..."
}

function Escape-VisualText {
    param(
        [string]$Value,
        [int]$MaximumLength = 240
    )

    if ($null -eq $Value) {
        return ""
    }

    $bounded = Get-BoundedText -Value $Value -MaximumLength $MaximumLength
    return [System.Net.WebUtility]::HtmlEncode($bounded).Replace("@", "&#64;")
}

function Test-VisualAssetUrl {
    param(
        [string]$Url,
        [string]$Repository,
        [int]$PrNumber,
        [string]$AssetCommit
    )

    if ([string]::IsNullOrWhiteSpace($Url) -or $Url.Length -gt 2048) {
        return $false
    }
    if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$' -or
        $AssetCommit -notmatch '^[0-9a-fA-F]{40}$') {
        return $false
    }

    $uri = $null
    if (-not [Uri]::TryCreate($Url, [UriKind]::Absolute, [ref]$uri)) {
        return $false
    }
    if ($uri.Scheme -ne "https" -or
        $uri.Host -ne "raw.githubusercontent.com" -or
        -not $uri.IsDefaultPort -or
        $uri.UserInfo -or
        $uri.Query -or
        $uri.Fragment) {
        return $false
    }

    try {
        $path = [Uri]::UnescapeDataString($uri.AbsolutePath)
    }
    catch {
        return $false
    }

    $repositoryParts = $Repository.Split("/")
    $expectedPrefix = "/$($repositoryParts[0])/$($repositoryParts[1])/$AssetCommit/pr-$PrNumber/"
    if (-not $path.StartsWith($expectedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        return $false
    }

    $relativePath = $path.Substring($expectedPrefix.Length)
    if ([string]::IsNullOrWhiteSpace($relativePath) -or
        $relativePath.Contains("..") -or
        $relativePath.Contains("//") -or
        $relativePath.Contains("\") -or
        $relativePath -notmatch '^[A-Za-z0-9._/-]+\.png$') {
        return $false
    }

    return @($relativePath.Split("/") | Where-Object { $_ -eq "." -or $_ -eq ".." }).Count -eq 0
}

function Get-CommentLimitCounts {
    param([string]$Body)

    if ($null -eq $Body) {
        $Body = ""
    }

    return [pscustomobject]@{
        urls = [regex]::Matches($Body, 'https?://[^\s]+').Count
        # Count mentions with a permissive pattern that captures whole GitHub tokens: usernames may
        # contain hyphens (@test-user) and team mentions carry a slash (@org/team). The narrower
        # '@\w+' clipped those to their first segment (@test / @org). For the raw count each '@'
        # still anchors one match, but the permissive form keeps counting conservative (it can only
        # ever match >= as many tokens, never fewer) and captures the true token, which is the
        # correct, robust basis for the mention-budget guard.
        mentions = [regex]::Matches($Body, '@[\w-]+(?:/[\w-]+)?').Count
        characters = $Body.Length
    }
}

function Remove-InlineVisualSection {
    param([string]$Body)

    if ([string]::IsNullOrEmpty($Body)) {
        return $Body
    }

    $pattern = [regex]::Escape((Get-InlineVisualStartMarker)) +
        '(?<content>.*?)' +
        [regex]::Escape((Get-InlineVisualEndMarker))
    $regex = [regex]::new(
        $pattern,
        [System.Text.RegularExpressions.RegexOptions]::Singleline,
        [TimeSpan]::FromSeconds(1))
    $evaluator = [System.Text.RegularExpressions.MatchEvaluator] {
        param($match)
        $content = $match.Groups['content'].Value
        if ([regex]::IsMatch(
                $content,
                '^\s*### Visual failure comparisons(?:\r?\n|$)',
                [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            return ""
        }
        # Marker text in fresh agent output is untrusted. Neutralize the marker comments but preserve
        # arbitrary analysis between them; only a section with the trusted merger's exact heading is
        # eligible for idempotent replacement.
        return $content
    }
    return $regex.Replace($Body, $evaluator)
}

function Test-VisualSnapshotPathMatchesPlatform {
    param(
        [string]$Path,
        [string]$Platform
    )

    $normalizedPath = "/" + (($Path -replace '\\', '/').TrimStart('/'))
    $normalizedPlatform = if ($null -eq $Platform) { "" } else { $Platform.ToLowerInvariant() }
    switch ($normalizedPlatform) {
        "android" { return $normalizedPath -match '/snapshots/android(?:-[^/]+)?/' }
        "ios" { return $normalizedPath -match '/snapshots/ios(?:-[^/]+)?/' }
        "macos" { return $normalizedPath -match '/snapshots/mac/' }
        "maccatalyst" { return $normalizedPath -match '/snapshots/mac/' }
        "windows" { return $normalizedPath -match '/snapshots/windows/' }
        default { return $false }
    }
}

function Test-VisualComparisonChanged {
    param(
        [object]$Comparison,
        [object]$Context
    )

    $changedFiles = @($Context.scope.changedFiles | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })
    if ($changedFiles.Count -eq 0) {
        return $false
    }

    $baselineRepositoryPath = [string]$Comparison.baselineRepositoryPath
    if (-not [string]::IsNullOrWhiteSpace($baselineRepositoryPath)) {
        $normalizedBaselinePath = ($baselineRepositoryPath -replace '\\', '/').TrimStart('/')
        foreach ($changedFile in $changedFiles) {
            $normalizedChangedFile = ([string]$changedFile -replace '\\', '/').TrimStart('/')
            if ([string]::Equals(
                    $normalizedChangedFile,
                    $normalizedBaselinePath,
                    [StringComparison]::OrdinalIgnoreCase)) {
                return $true
            }
        }
    }
    else {
        $snapshotFileName = [System.IO.Path]::GetFileName([string]$Comparison.snapshotFileName)
        if (-not [string]::IsNullOrWhiteSpace($snapshotFileName)) {
            foreach ($changedFile in $changedFiles) {
                $matchingFileName = [string]::Equals(
                    [System.IO.Path]::GetFileName([string]$changedFile),
                    $snapshotFileName,
                    [StringComparison]::OrdinalIgnoreCase)
                $matchingPlatform = Test-VisualSnapshotPathMatchesPlatform `
                    -Path ([string]$changedFile) `
                    -Platform ([string]$Comparison.platform)
                if ($matchingFileName -and $matchingPlatform) {
                    return $true
                }
            }
        }
    }

    $automatedTestName = [string]$Comparison.automatedTestName
    $classMatch = [regex]::Match(
        $automatedTestName,
        '\.(?<class>[A-Za-z_][A-Za-z0-9_]*)(?:\([^)]*\))?\.[^.]+$')
    if ($classMatch.Success) {
        $testFileName = $classMatch.Groups['class'].Value + ".cs"
        foreach ($changedFile in $changedFiles) {
            if ([string]::Equals(
                    [System.IO.Path]::GetFileName([string]$changedFile),
                    $testFileName,
                    [StringComparison]::OrdinalIgnoreCase)) {
                return $true
            }
        }
    }

    return $false
}

function Get-VisualRelationship {
    param(
        [object]$Comparison,
        [object]$Context
    )

    $testName = [string]$Comparison.testName
    $platform = [string]$Comparison.platform
    $failure = $null
    foreach ($candidate in @($Context.failures.unique | Where-Object { $null -ne $_ })) {
        if ([string]::Equals(
                [string]$candidate.testName,
                $testName,
                [StringComparison]::OrdinalIgnoreCase) -and
            [string]::Equals(
                [string]$candidate.platform,
                $platform,
                [StringComparison]::OrdinalIgnoreCase)) {
            $failure = $candidate
            break
        }
    }

    $attribution = if ($null -ne $failure) {
        [string]$failure.deterministicAttribution
    }
    else {
        ""
    }
    if ($attribution -eq "regressed-vs-base") {
        return [pscustomobject]@{
            label = "Likely PR-caused"
            detail = "The same leg was green on the sampled base build and red on this PR."
        }
    }
    $comparisonChanged = Test-VisualComparisonChanged -Comparison $Comparison -Context $Context
    if ($comparisonChanged) {
        return [pscustomobject]@{
            label = "Likely PR-caused"
            detail = "This PR changes the exact snapshot or visual test."
        }
    }
    if ($null -eq $failure) {
        return [pscustomobject]@{
            label = "Needs human investigation"
            detail = "No decisive exact test-and-platform baseline attribution was available."
        }
    }

    switch ($attribution) {
        "pre-existing-on-base" {
            return [pscustomobject]@{
                label = "Likely unrelated"
                detail = "The exact test and platform also failed on the base branch."
            }
        }
        "known-issue" {
            return [pscustomobject]@{
                label = "Likely unrelated"
                detail = "The exact test and platform also failed on base and matched a known issue."
            }
        }
        default {
            $detail = if ([bool]$failure.alsoFailsOnBaseline -or [bool]$failure.legAlsoFailsOnBase) {
                "Base-branch evidence exists, but it was not strong enough to dismiss this exact failure."
            }
            else {
                "No decisive exact test-and-platform baseline attribution was available."
            }
            return [pscustomobject]@{
                label = "Needs human investigation"
                detail = $detail
            }
        }
    }
}

function New-InlineVisualPanel {
    param(
        [object]$Comparison,
        [object]$Relationship,
        [string]$BaselineUrl,
        [string]$ActualUrl,
        [string]$DiffUrl
    )

    $testName = Escape-VisualText -Value ([string]$Comparison.testName) -MaximumLength 180
    $platform = Escape-VisualText -Value ([string]$Comparison.platform) -MaximumLength 40
    $description = Escape-VisualText -Value ([string]$Comparison.description) -MaximumLength 240
    $baselineStatus = Escape-VisualText -Value ([string]$Comparison.baselineStatus) -MaximumLength 180
    $baselineAlt = Escape-VisualText -Value "$([string]$Comparison.testName) baseline" -MaximumLength 220
    $actualAlt = Escape-VisualText -Value "$([string]$Comparison.testName) actual" -MaximumLength 220
    $diffAlt = Escape-VisualText -Value "$([string]$Comparison.testName) diff" -MaximumLength 220
    $safeActualUrl = Escape-VisualText -Value $ActualUrl -MaximumLength 2048
    $relationshipLabel = Escape-VisualText -Value ([string]$Relationship.label) -MaximumLength 80
    $relationshipDetail = Escape-VisualText -Value ([string]$Relationship.detail) -MaximumLength 240
    $buildId = [int]$Comparison.buildId

    $baselineCell = if ($BaselineUrl) {
        $safeBaselineUrl = Escape-VisualText -Value $BaselineUrl -MaximumLength 2048
        "<img alt=""$baselineAlt"" width=""260"" src=""$safeBaselineUrl"">"
    }
    else {
        "<em>Baseline unavailable: $baselineStatus</em>"
    }
    $diffCell = if ($DiffUrl) {
        $safeDiffUrl = Escape-VisualText -Value $DiffUrl -MaximumLength 2048
        "<img alt=""$diffAlt"" width=""260"" src=""$safeDiffUrl"">"
    }
    else {
        "<em>CI diff was not generated.</em>"
    }
    $descriptionLine = if ($description) {
        "CI reported <code>$description</code> in build <code>$buildId</code>."
    }
    else {
        "CI reported a visual snapshot failure in build <code>$buildId</code>."
    }

    return @"
<details>
<summary><code>$testName</code> - $platform - <strong>$relationshipLabel</strong> - visual comparison</summary>

$descriptionLine

**Relationship to PR:** **$relationshipLabel** - $relationshipDetail

<table><tr><th>CI baseline</th><th>Fresh PR actual</th><th>CI diff</th></tr><tr>
<td>$baselineCell</td>
<td><img alt="$actualAlt" width="260" src="$safeActualUrl"></td>
<td>$diffCell</td>
</tr></table>
</details>

"@
}

function New-InlineVisualSection {
    param(
        [string[]]$Panels,
        # Comparisons dropped by the PUBLISHER or by asset validation (dedup, the MaxComparisons cap,
        # the discovery/publish time budget, or actual/baseline URLs that failed validation). These
        # were never candidates for this comment, so they must NOT be described as comment-safety drops.
        [int]$PublisherOmittedCount = 0,
        # Valid, ready-to-render panels dropped ONLY to keep the comment within its URL / mention /
        # character budget.
        [int]$CommentSafetyOmittedCount = 0,
        [int]$PreparationFailureCount = 0,
        # A Git/API failure occurred AFTER images were prepared, so nothing could be published. Render
        # a fixed, trusted notice (never the untrusted raw exception text) so the reader can tell a
        # publish failure apart from a run that produced no visual evidence.
        [bool]$PublicationFailed = $false
    )

    $builder = [System.Text.StringBuilder]::new()
    [void]$builder.AppendLine((Get-InlineVisualStartMarker))
    [void]$builder.AppendLine("### Visual failure comparisons")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("Full-resolution CI baseline, actual, and diff images are embedded below. They supplement the failure classification and do not change the deterministic verdict ceiling.")
    [void]$builder.AppendLine("Relationship labels use deterministic exact test-and-platform baseline evidence plus exact changed snapshot/test scope; missing or mixed evidence remains **Needs human investigation**.")
    [void]$builder.AppendLine()

    foreach ($panel in @($Panels)) {
        [void]$builder.Append($panel)
    }

    if ($PublicationFailed) {
        [void]$builder.AppendLine("Visual comparisons were prepared but could not be published to the asset branch because a Git or API error occurred after image preparation. The deterministic verdict is unaffected; no images are shown for this run.")
        [void]$builder.AppendLine()
    }
    if (@($Panels).Count -eq 0 -and $CommentSafetyOmittedCount -gt 0) {
        [void]$builder.AppendLine("Visual comparisons were detected, but none fit within the comment safety limits.")
        [void]$builder.AppendLine()
    }
    if (@($Panels).Count -eq 0 -and $PublisherOmittedCount -gt 0 -and -not $PublicationFailed -and $PreparationFailureCount -eq 0 -and $CommentSafetyOmittedCount -eq 0) {
        [void]$builder.AppendLine("Visual comparisons were detected, but none could be published within the publisher bounds.")
        [void]$builder.AppendLine()
    }
    if ($CommentSafetyOmittedCount -gt 0) {
        [void]$builder.AppendLine("Visual output was bounded for comment safety; $CommentSafetyOmittedCount additional comparison(s) were omitted.")
    }
    if ($PublisherOmittedCount -gt 0) {
        [void]$builder.AppendLine("$PublisherOmittedCount additional visual comparison(s) were omitted by publisher bounds (deduplication, the comparison cap, the discovery/publish time budget, or assets that failed validation).")
    }
    if ($PreparationFailureCount -gt 0) {
        [void]$builder.AppendLine("$PreparationFailureCount visual comparison(s) could not be prepared from CI artifacts and are not shown.")
    }

    [void]$builder.AppendLine((Get-InlineVisualEndMarker))
    return $builder.ToString()
}

function Insert-InlineVisualSection {
    param(
        [string]$Body,
        [string]$Section
    )

    $placeholder = Get-InlineVisualPlaceholder
    $placeholderIndex = $Body.IndexOf($placeholder, [StringComparison]::Ordinal)
    if ($placeholderIndex -ge 0) {
        $prefix = $Body.Substring(0, $placeholderIndex)
        $suffix = $Body.Substring($placeholderIndex + $placeholder.Length).Replace($placeholder, "")
        return $prefix + $Section + $suffix
    }

    $withoutPlaceholders = $Body.Replace($placeholder, "")
    $closingDetailsIndex = $withoutPlaceholders.LastIndexOf("</details>", [StringComparison]::OrdinalIgnoreCase)
    if ($closingDetailsIndex -ge 0) {
        return $withoutPlaceholders.Insert($closingDetailsIndex, "$Section`n")
    }

    return $withoutPlaceholders.TrimEnd() + "`n`n" + $Section
}

function Test-CommentWithinLimits {
    param(
        [string]$Body,
        [int]$MaxCommentUrls,
        [int]$MaxCommentMentions,
        [int]$MaxCommentCharacters
    )

    $counts = Get-CommentLimitCounts -Body $Body
    return $counts.urls -le $MaxCommentUrls -and
        $counts.mentions -le $MaxCommentMentions -and
        $counts.characters -le $MaxCommentCharacters
}

function Insert-LimitSafeInlineVisualSection {
    param(
        [string]$Body,
        [string]$Section,
        [int]$MaxCommentUrls,
        [int]$MaxCommentMentions,
        [int]$MaxCommentCharacters
    )

    $mergedBody = Insert-InlineVisualSection -Body $Body -Section $Section
    if (Test-CommentWithinLimits `
            -Body $mergedBody `
            -MaxCommentUrls $MaxCommentUrls `
            -MaxCommentMentions $MaxCommentMentions `
            -MaxCommentCharacters $MaxCommentCharacters) {
        return $mergedBody
    }

    return $Body.Replace((Get-InlineVisualPlaceholder), "")
}

function Merge-VisualsIntoBody {
    param(
        [string]$Body,
        [object]$Context,
        [string]$Repository,
        [int]$PrNumber,
        [int]$MaxCommentUrls,
        [int]$MaxCommentMentions,
        [int]$MaxCommentCharacters
    )

    if ($null -eq $Body) {
        $Body = ""
    }

    $baseBody = Remove-InlineVisualSection -Body $Body
    $placeholder = Get-InlineVisualPlaceholder
    if (-not $Context.visualAssets -or -not [bool]$Context.visualAssets.published) {
        # Publishing produced no panels. Distinguish the sub-cases so a real failure is never rendered
        # as "no visual evidence at all", in priority order:
        #   1. a post-preparation Git/API *publication* failure (explicit publicationFailed flag) --
        #      the most severe; it also carries any surviving omission/preparation counts
        #   2. every prepared comparison failed *preparation* (preparationFailureCount > 0)
        #   3. comparisons were bounded away by publisher budget/dedup/cap (omittedCount > 0)
        #   4. genuinely no visual evidence -> strip the placeholder
        $prepFailures = if ($Context.visualAssets -and $Context.visualAssets.preparationFailureCount) {
            [Math]::Max(0, [int]$Context.visualAssets.preparationFailureCount)
        }
        else {
            0
        }
        # Publisher/validation omissions (the MaxComparisons cap / dedup) are real even when nothing
        # was published, so surface them here too rather than hardcoding zero.
        $publisherOmitted = if ($Context.visualAssets -and $Context.visualAssets.omittedCount) {
            [Math]::Max(0, [int]$Context.visualAssets.omittedCount)
        }
        else {
            0
        }
        # Post-preparation publication failure is signalled by an EXPLICIT trusted flag set only in the
        # Publish-GitAssets catch block. Inferring it from a non-empty error list (the prior heuristic)
        # misfired: the pre-publication path also emits published=false with a budget/omission error
        # when the publish budget expires before the first comparison is prepared, which is an omission
        # -- not a Git/API publication failure. Check it FIRST so a publish failure that ALSO had some
        # preparation failures or omissions still shows the publish notice (never the raw exception
        # text, which is untrusted CI output), plus the surviving caveats.
        $publicationFailed = [bool]($Context.visualAssets -and $Context.visualAssets.publicationFailed)
        if ($publicationFailed) {
            $failureSection = New-InlineVisualSection -Panels @() -PublisherOmittedCount $publisherOmitted -CommentSafetyOmittedCount 0 -PreparationFailureCount $prepFailures -PublicationFailed $true
            return Insert-LimitSafeInlineVisualSection `
                -Body $baseBody `
                -Section $failureSection `
                -MaxCommentUrls $MaxCommentUrls `
                -MaxCommentMentions $MaxCommentMentions `
                -MaxCommentCharacters $MaxCommentCharacters
        }
        if ($prepFailures -gt 0) {
            $failureSection = New-InlineVisualSection -Panels @() -PublisherOmittedCount $publisherOmitted -CommentSafetyOmittedCount 0 -PreparationFailureCount $prepFailures
            return Insert-LimitSafeInlineVisualSection `
                -Body $baseBody `
                -Section $failureSection `
                -MaxCommentUrls $MaxCommentUrls `
                -MaxCommentMentions $MaxCommentMentions `
                -MaxCommentCharacters $MaxCommentCharacters
        }
        # No panels were published and no explicit publication or preparation failure occurred, but
        # comparisons were detected and then bounded away by publisher budget/dedup/cap (e.g. the
        # publish budget expiring before the first comparison). Surface that caveat instead of stripping
        # the placeholder, so a non-empty-but-unpublished scan is never rendered as a genuinely clean run.
        if ($publisherOmitted -gt 0) {
            $failureSection = New-InlineVisualSection -Panels @() -PublisherOmittedCount $publisherOmitted -CommentSafetyOmittedCount 0 -PreparationFailureCount 0
            return Insert-LimitSafeInlineVisualSection `
                -Body $baseBody `
                -Section $failureSection `
                -MaxCommentUrls $MaxCommentUrls `
                -MaxCommentMentions $MaxCommentMentions `
                -MaxCommentCharacters $MaxCommentCharacters
        }
        return $baseBody.Replace($placeholder, "")
    }

    $assetCommit = [string]$Context.visualAssets.commit
    if ($assetCommit -notmatch '^[0-9a-fA-F]{40}$') {
        return $baseBody.Replace($placeholder, "")
    }

    $comparisons = @($Context.visualAssets.comparisons | Where-Object { $null -ne $_ })
    if ($comparisons.Count -eq 0) {
        return $baseBody.Replace($placeholder, "")
    }

    $validPanels = New-Object System.Collections.Generic.List[string]
    $invalidCount = 0
    foreach ($comparison in $comparisons) {
        $actualUrl = [string]$comparison.actualUrl
        if (-not (Test-VisualAssetUrl `
                -Url $actualUrl `
                -Repository $Repository `
                -PrNumber $PrNumber `
                -AssetCommit $assetCommit)) {
            $invalidCount++
            continue
        }

        $baselineUrl = [string]$comparison.baselineUrl
        if ($baselineUrl -and -not (Test-VisualAssetUrl `
                -Url $baselineUrl `
                -Repository $Repository `
                -PrNumber $PrNumber `
                -AssetCommit $assetCommit)) {
            $baselineUrl = $null
        }

        $diffUrl = [string]$comparison.diffUrl
        if ($diffUrl -and -not (Test-VisualAssetUrl `
                -Url $diffUrl `
                -Repository $Repository `
                -PrNumber $PrNumber `
                -AssetCommit $assetCommit)) {
            $diffUrl = $null
        }

        $relationship = Get-VisualRelationship -Comparison $comparison -Context $Context
        $validPanels.Add((New-InlineVisualPanel `
                    -Comparison $comparison `
                    -Relationship $relationship `
                    -BaselineUrl $baselineUrl `
                    -ActualUrl $actualUrl `
                    -DiffUrl $diffUrl))
    }

    $publisherOmitted = if ($Context.visualAssets.omittedCount) {
        [Math]::Max(0, [int]$Context.visualAssets.omittedCount)
    }
    else {
        0
    }
    $preparationFailureCount = if ($Context.visualAssets.preparationFailureCount) {
        [Math]::Max(0, [int]$Context.visualAssets.preparationFailureCount)
    }
    else {
        0
    }
    $selectedPanels = New-Object System.Collections.Generic.List[string]
    foreach ($panel in $validPanels) {
        $trialPanels = @($selectedPanels.ToArray()) + @($panel)
        $trialSection = New-InlineVisualSection -Panels $trialPanels `
            -PublisherOmittedCount ($publisherOmitted + $invalidCount) `
            -CommentSafetyOmittedCount ($validPanels.Count - $trialPanels.Count) `
            -PreparationFailureCount $preparationFailureCount
        $trialBody = Insert-InlineVisualSection -Body $baseBody -Section $trialSection
        if (Test-CommentWithinLimits `
                -Body $trialBody `
                -MaxCommentUrls $MaxCommentUrls `
                -MaxCommentMentions $MaxCommentMentions `
                -MaxCommentCharacters $MaxCommentCharacters) {
            $selectedPanels.Add($panel)
        }
    }

    $section = New-InlineVisualSection -Panels $selectedPanels.ToArray() `
        -PublisherOmittedCount ($publisherOmitted + $invalidCount) `
        -CommentSafetyOmittedCount ($validPanels.Count - $selectedPanels.Count) `
        -PreparationFailureCount $preparationFailureCount
    $mergedBody = Insert-InlineVisualSection -Body $baseBody -Section $section
    if (-not (Test-CommentWithinLimits `
            -Body $mergedBody `
            -MaxCommentUrls $MaxCommentUrls `
            -MaxCommentMentions $MaxCommentMentions `
            -MaxCommentCharacters $MaxCommentCharacters)) {
        return $baseBody.Replace($placeholder, "")
    }

    return $mergedBody
}

function Write-AtomicUtf8Text {
    param(
        [string]$Path,
        [string]$Content
    )

    $directory = Split-Path -Parent $Path
    if (-not $directory) {
        $directory = (Get-Location).Path
    }
    $temporaryPath = Join-Path $directory ".$([System.IO.Path]::GetFileName($Path)).$([Guid]::NewGuid().ToString('N')).tmp"
    try {
        [System.IO.File]::WriteAllText(
            $temporaryPath,
            $Content,
            [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::Move($temporaryPath, $Path, $true)
    }
    finally {
        Remove-Item -LiteralPath $temporaryPath -Force -ErrorAction SilentlyContinue
    }
}

function Update-AgentOutputFile {
    param(
        [string]$Path,
        [object]$Context,
        [string]$Repository,
        [int]$PrNumber,
        [int]$MaxCommentUrls,
        [int]$MaxCommentMentions,
        [int]$MaxCommentCharacters
    )

    $originalJson = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $agentOutput = $originalJson | ConvertFrom-Json
    if ($null -eq $agentOutput -or $null -eq $agentOutput.items) {
        return [pscustomobject]@{ changed = $false; mergedComments = 0 }
    }

    $mergedComments = 0
    foreach ($item in @($agentOutput.items)) {
        if ([string]$item.type -ne "add_comment" -or
            [int]$item.item_number -ne $PrNumber -or
            $null -eq $item.body) {
            continue
        }

        $originalBody = [string]$item.body
        $mergedBody = Merge-VisualsIntoBody `
            -Body $originalBody `
            -Context $Context `
            -Repository $Repository `
            -PrNumber $PrNumber `
            -MaxCommentUrls $MaxCommentUrls `
            -MaxCommentMentions $MaxCommentMentions `
            -MaxCommentCharacters $MaxCommentCharacters
        if ($mergedBody -ne $originalBody) {
            $item.body = $mergedBody
            $mergedComments++
        }
    }

    if ($mergedComments -eq 0) {
        return [pscustomobject]@{ changed = $false; mergedComments = 0 }
    }

    $updatedJson = $agentOutput | ConvertTo-Json -Depth 100 -Compress
    $roundTripped = $updatedJson | ConvertFrom-Json
    if ($null -eq $roundTripped -or
        @($roundTripped.items).Count -ne @($agentOutput.items).Count) {
        throw "Updated agent output did not preserve the item schema."
    }
    foreach ($item in @($roundTripped.items)) {
        if ([string]$item.type -eq "add_comment" -and [int]$item.item_number -eq $PrNumber) {
            if (-not (Test-CommentWithinLimits `
                    -Body ([string]$item.body) `
                    -MaxCommentUrls $MaxCommentUrls `
                    -MaxCommentMentions $MaxCommentMentions `
                    -MaxCommentCharacters $MaxCommentCharacters)) {
                throw "Merged add_comment body exceeded the configured safety limits."
            }
        }
    }

    Write-AtomicUtf8Text -Path $Path -Content $updatedJson
    return [pscustomobject]@{ changed = $true; mergedComments = $mergedComments }
}

function Update-CommentBodyFile {
    param(
        [string]$Path,
        [object]$Context,
        [string]$Repository,
        [int]$PrNumber,
        [int]$MaxCommentUrls,
        [int]$MaxCommentMentions,
        [int]$MaxCommentCharacters
    )

    $originalBody = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $mergedBody = Merge-VisualsIntoBody `
        -Body $originalBody `
        -Context $Context `
        -Repository $Repository `
        -PrNumber $PrNumber `
        -MaxCommentUrls $MaxCommentUrls `
        -MaxCommentMentions $MaxCommentMentions `
        -MaxCommentCharacters $MaxCommentCharacters
    if ($mergedBody -eq $originalBody) {
        return [pscustomobject]@{ changed = $false; mergedComments = 0 }
    }

    Write-AtomicUtf8Text -Path $Path -Content $mergedBody
    return [pscustomobject]@{ changed = $true; mergedComments = 1 }
}

if ([string]::IsNullOrWhiteSpace($Repository)) {
    $Repository = "dotnet/maui"
}
if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') {
    throw "Repository must be an owner/name pair."
}
if (-not (Test-Path -LiteralPath $ContextJsonPath)) {
    throw "Context JSON was not found: $ContextJsonPath"
}

$context = Get-Content -LiteralPath $ContextJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($null -eq $context) {
    throw "Context JSON was empty."
}
if ([string]$context.repository -ne $Repository) {
    throw "Context repository '$($context.repository)' did not match trusted repository '$Repository'."
}
if ([int]$context.pr.number -ne $PrNumber) {
    throw "Context PR '$($context.pr.number)' did not match trusted PR '$PrNumber'."
}

if ($PSCmdlet.ParameterSetName -eq "AgentOutput") {
    if (-not (Test-Path -LiteralPath $AgentOutputPath)) {
        Write-Host "Agent output was not found; leaving the ordinary analysis unchanged."
        exit 0
    }
    $result = Update-AgentOutputFile `
        -Path $AgentOutputPath `
        -Context $context `
        -Repository $Repository `
        -PrNumber $PrNumber `
        -MaxCommentUrls $MaxCommentUrls `
        -MaxCommentMentions $MaxCommentMentions `
        -MaxCommentCharacters $MaxCommentCharacters
}
else {
    if (-not (Test-Path -LiteralPath $CommentBodyPath)) {
        throw "Comment body was not found: $CommentBodyPath"
    }
    $result = Update-CommentBodyFile `
        -Path $CommentBodyPath `
        -Context $context `
        -Repository $Repository `
        -PrNumber $PrNumber `
        -MaxCommentUrls $MaxCommentUrls `
        -MaxCommentMentions $MaxCommentMentions `
        -MaxCommentCharacters $MaxCommentCharacters
}

if ($result.changed) {
    Write-Host "Merged trusted visual comparisons into $($result.mergedComments) analysis comment payload(s)."
}
else {
    Write-Host "No visual comparison merge was needed; the ordinary analysis remains unchanged."
}
