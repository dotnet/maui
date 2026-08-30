#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Exports current feedback from open testing-fork reproduction PRs.
#>

[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$RepositoryOwner = 'kubaflo',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$RepositoryName = 'maui',

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

# Selector and quality normalization is shared with the clean validator.  The
# validator has an OutputPath parameter, so preserve this script's output path
# while importing its trusted function definitions.
$feedbackOutputPath = $OutputPath
$qualitySelectorValidatorPath = Join-Path $PSScriptRoot 'Validate-ReplicationCandidate.ps1'
if (Test-Path -LiteralPath $qualitySelectorValidatorPath -PathType Leaf) {
    . $qualitySelectorValidatorPath
}
$OutputPath = $feedbackOutputPath

function Invoke-FeedbackGhJson {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $json = & gh @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
    if ([string]::IsNullOrWhiteSpace([string]$json)) {
        return $null
    }
    return $json | ConvertFrom-Json -Depth 50
}

function ConvertTo-FeedbackText {
    param(
        [AllowNull()][object]$Value,
        [int]$MaximumLength = 400
    )

    if ($Value -isnot [string]) { return '' }
    $text = [string]$Value
    $text = $text -replace '##vso\[[^\]]*\]', ''
    $text = $text -replace '##\[[^\]]*\]', ''
    $text = $text -replace '::(?:set-output|add-mask|error|warning|notice)[^\s]*', ''
    $text = $text -replace '[\x00-\x1F\x7F]', ' '
    $text = $text -replace '\s+', ' '
    $text = $text.Trim()
    if ($text.Length -gt $MaximumLength) {
        $text = $text.Substring(0, [Math]::Max(1, $MaximumLength - 1)).TrimEnd() + '…'
    }
    return $text
}

function Get-FeedbackBodyLine {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][string]$Label,
        [int]$MaximumLength = 400
    )

    $match = [regex]::Match($Body, "(?im)^\s*-\s*$([regex]::Escape($Label)):\s*(?<value>.+)$")
    if (-not $match.Success) { return 'unknown' }
    $value = $match.Groups['value'].Value.Trim()
    $value = $value -replace '^`+|`+$', ''
    $safe = ConvertTo-FeedbackText -Value $value -MaximumLength $MaximumLength
    if ([string]::IsNullOrWhiteSpace($safe)) { return 'unknown' }
    return $safe
}

function Get-FeedbackSelectorDisclosure {
    param([AllowEmptyString()][string]$Body)

    $unknown = if (Get-Command New-ReplicationUnknownSelector -ErrorAction SilentlyContinue) {
        New-ReplicationUnknownSelector
    } else {
        [ordered]@{
            variant = 'unknown'; raw = 'unknown'; project = 'unknown'; projectPath = 'unknown'
            class = 'unknown'; method = 'unknown'; platform = 'unknown'
            discoveredCount = 0; executedCount = 0; fixture = 'unknown'
        }
    }
    $variant = Get-FeedbackBodyLine -Body $Body -Label 'Selector variant' -MaximumLength 48
    $raw = Get-FeedbackBodyLine -Body $Body -Label 'Raw runner selector' -MaximumLength 512
    $normalized = [regex]::Match(
        $Body,
        '(?im)^\s*-\s*Normalized selector:\s*project\s+`(?<project>[^`]+)`\s+\(`(?<path>[^`]+)`\),\s*class\s+`(?<class>[^`]+)`\s*,\s*method\s+`(?<method>[^`]+)`\s*,\s*platform\s+`(?<platform>[^`]+)`')
    $counts = [regex]::Match(
        $Body,
        '(?im)^\s*-\s*Trusted selector counts:\s*(?<discovered>\d+)\s+discovered\s*/\s*(?<executed>\d+)\s+executed')
    if (-not $normalized.Success -or -not $counts.Success) { return $unknown }

    $platform = ([string]$normalized.Groups['platform'].Value).Trim().ToLowerInvariant()
    if ($platform -eq 'maccatalyst') { $platform = 'catalyst' }
    if ($platform -notin @('android', 'ios', 'catalyst', 'windows')) { return $unknown }
    $variantMap = @{
        'ui-parameterized-fixture' = 'ui-parameterized-fixture'
        'device-category-only' = 'device-category-only'
        'fully-qualified-name' = 'fully-qualified-name'
    }
    if (-not $variantMap.ContainsKey($variant)) { return $unknown }

    $candidate = [ordered]@{
        variant = $variantMap[$variant]
        raw = $raw
        project = ConvertTo-FeedbackText $normalized.Groups['project'].Value 240
        projectPath = ConvertTo-FeedbackText $normalized.Groups['path'].Value 400
        class = ConvertTo-FeedbackText $normalized.Groups['class'].Value 400
        method = ConvertTo-FeedbackText $normalized.Groups['method'].Value 200
        platform = $platform
        discoveredCount = [int]$counts.Groups['discovered'].Value
        executedCount = [int]$counts.Groups['executed'].Value
        fixture = 'unknown'
    }
    if ($candidate.discoveredCount -ne 1 -or $candidate.executedCount -ne 1) {
        return $unknown
    }
    return $candidate
}

function Get-FeedbackQualityDisclosure {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][object]$Selector
    )

    $quality = if (Get-Command New-ReplicationUnknownQualityContract -ErrorAction SilentlyContinue) {
        New-ReplicationUnknownQualityContract
    } else {
        [ordered]@{
            schemaVersion = 1
            userVisible = [ordered]@{ contract = 'unknown'; trigger = 'unknown' }
            oracle = [ordered]@{ primary = 'unknown'; independent = $null; independence = 'unknown'; rationale = 'unknown' }
            scenario = [ordered]@{ name = 'unknown'; precondition = 'unknown'; trigger = 'unknown'; transition = 'unknown'; observableIdentity = 'unknown'; affectedControl = $null }
            risk = [ordered]@{ adjacentStates = @(); lifecycleStates = @(); statelessApplicability = 'unknown' }
            semanticBlastRadius = [ordered]@{ affectedType = 'unknown'; affectedControl = 'unknown'; ownership = 'unknown'; sharedConsumers = @(); unchangedBehavior = 'unknown' }
            mediaAlignment = 'not-measured'
            review = [ordered]@{ findings = @() }
        }
    }
    $quality.userVisible.contract = Get-FeedbackBodyLine $Body 'User-visible contract' 500
    $quality.userVisible.trigger = Get-FeedbackBodyLine $Body 'User-visible trigger' 500
    $quality.oracle.primary = Get-FeedbackBodyLine $Body 'Primary oracle' 500
    $independenceLine = Get-FeedbackBodyLine $Body 'Oracle independence' 160
    $independenceMatch = [regex]::Match(
        $independenceLine,
        '(?i)^(?<value>independent|coupled|not-applicable|unknown)\b')
    $quality.oracle.independence = if ($independenceMatch.Success) {
        $independenceMatch.Groups['value'].Value.ToLowerInvariant()
    } else { 'unknown' }
    if ($independenceLine -match '\s+[—-]\s+(?<rationale>.+)$') {
        $quality.oracle.rationale = ConvertTo-FeedbackText $Matches['rationale'] 500
    }
    $quality.scenario.name = Get-FeedbackBodyLine $Body 'Scenario' 300
    $quality.scenario.precondition = Get-FeedbackBodyLine $Body 'Precondition' 500
    $quality.scenario.trigger = Get-FeedbackBodyLine $Body 'Trigger' 500
    $quality.scenario.transition = Get-FeedbackBodyLine $Body 'Transition' 500
    $quality.scenario.observableIdentity = Get-FeedbackBodyLine $Body 'Observable identity' 500
    $quality.semanticBlastRadius.affectedType = Get-FeedbackBodyLine $Body 'Semantic blast radius' 800
    $mediaMatch = [regex]::Match(
        $Body,
        '(?im)^\s*-\s*Media alignment:\s*`(?<value>verified|partial|not-measured)`')
    $quality.mediaAlignment = if ($mediaMatch.Success) {
        $mediaMatch.Groups['value'].Value.ToLowerInvariant()
    } else { 'not-measured' }
    return $quality
}

function Get-FeedbackReviewDisclosure {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][object]$Quality
    )

    $findings = [Collections.Generic.List[object]]::new()
    foreach ($finding in @($Quality.review.findings | Select-Object -First 8)) {
        [void]$findings.Add($finding)
    }
    foreach ($pattern in @(
            '(?im)^\s*-\s*\*\*[A-Za-z-]+\.\*\*\s+.+?\s+\(`(?<category>grounded-product-defect|missing-evidence-coverage|advisory-hardening|unsupported-speculative)`;\s*grounding\s*`(?<grounding>[^`]+)`,\s*confidence\s*`(?<confidence>[^`]+)`,\s*corroboration\s*`(?<corroboration>[^`]+)`;\s*advisory\)',
            '(?im)^\s*-\s*\*\*(?<category>grounded-product-defect|missing-evidence-coverage|advisory-hardening|unsupported-speculative)\*\*\s+\(`(?<grounding>[^`]+)`,\s*`(?<confidence>[^`]+)`,\s*`(?<corroboration>[^`]+)`\):'
        )) {
        foreach ($match in [regex]::Matches($Body, $pattern)) {
            if ($findings.Count -ge 8) { break }
            [void]$findings.Add([ordered]@{
                category = $match.Groups['category'].Value
                grounding = ConvertTo-FeedbackText $match.Groups['grounding'].Value 48
                confidence = ConvertTo-FeedbackText $match.Groups['confidence'].Value 16
                corroboration = ConvertTo-FeedbackText $match.Groups['corroboration'].Value 24
                detail = 'See the bounded advisory finding in the pull request body.'
            })
        }
        if ($findings.Count -ge 8) { break }
    }
    return [ordered]@{ findings = @($findings.ToArray() | Select-Object -First 8) }
}

function Get-FeedbackEvidenceDisclosure {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][object]$Quality
    )

    return [ordered]@{
        mediaAlignment = [string]$Quality.mediaAlignment
        recordingLinked = [bool]($Body -match '(?i)repro\.mp4')
        previewLinked = [bool]($Body -match '(?i)preview\.(?:gif|png)')
        manifestLinked = [bool]($Body -match '(?i)evidence manifest')
    }
}

if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
    throw 'GH_TOKEN is required to export reproduction PR feedback.'
}

$authenticatedLogin = (& gh api user --jq '.login').Trim()
if ($LASTEXITCODE -ne 0 -or
    -not $authenticatedLogin.Equals('MauiBot', [StringComparison]::OrdinalIgnoreCase)) {
    throw "GH_TOKEN must authenticate as 'MauiBot'."
}

$repository = "$RepositoryOwner/$RepositoryName"
$markerPattern = '<!--\s*MAUI_COPILOT_REPLICATION\s+issue=(\d+)\s+platform=([a-z0-9-]+)\s*-->'
$pulls = @(
    Invoke-FeedbackGhJson `
        -Arguments @('api', "repos/$repository/pulls?state=open&per_page=100") `
        -Description "Listing open pull requests in $repository"
) | Where-Object {
    [regex]::IsMatch([string]$_.body, $markerPattern)
}

$exportedPulls = [Collections.Generic.List[object]]::new()
foreach ($pull in $pulls) {
    $marker = [regex]::Match([string]$pull.body, $markerPattern)
    $number = [int]$pull.number
    $pullBody = ConvertTo-FeedbackText -Value ([string]$pull.body) -MaximumLength 20000
    $selector = Get-FeedbackSelectorDisclosure -Body ([string]$pull.body)
    $quality = Get-FeedbackQualityDisclosure -Body ([string]$pull.body) -Selector $selector
    $review = Get-FeedbackReviewDisclosure -Body ([string]$pull.body) -Quality $quality
    $evidence = Get-FeedbackEvidenceDisclosure -Body ([string]$pull.body) -Quality $quality
    $exportedPulls.Add([ordered]@{
        number = $number
        url = ConvertTo-FeedbackText -Value ([string]$pull.html_url) -MaximumLength 500
        title = ConvertTo-FeedbackText -Value ([string]$pull.title) -MaximumLength 300
        issueNumber = [int]$marker.Groups[1].Value
        platform = ConvertTo-FeedbackText -Value $marker.Groups[2].Value -MaximumLength 32
        draft = [bool]$pull.draft
        updatedAt = ConvertTo-FeedbackText -Value ([string]$pull.updated_at) -MaximumLength 64
        headSha = ConvertTo-FeedbackText -Value ([string]$pull.head.sha) -MaximumLength 64
        headRepository = ConvertTo-FeedbackText -Value ([string]$pull.head.repo.full_name) -MaximumLength 200
        selector = $selector
        quality = $quality
        qualityContract = $quality
        evidence = $evidence
        review = $review
        body = $pullBody
        discussionComments = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/issues/$number/comments?per_page=100") `
                -Description "Listing discussion comments for $repository#$number"
        ) | Select-Object -First 50 | ForEach-Object {
            [ordered]@{
                author = ConvertTo-FeedbackText -Value ([string]$_.user.login) -MaximumLength 80
                createdAt = ConvertTo-FeedbackText -Value ([string]$_.created_at) -MaximumLength 64
                body = ConvertTo-FeedbackText -Value ([string]$_.body) -MaximumLength 2000
            }
        }
        reviews = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/pulls/$number/reviews?per_page=100") `
                -Description "Listing reviews for $repository#$number"
        ) | Select-Object -First 50 | ForEach-Object {
            [ordered]@{
                author = ConvertTo-FeedbackText -Value ([string]$_.user.login) -MaximumLength 80
                state = ConvertTo-FeedbackText -Value ([string]$_.state) -MaximumLength 32
                submittedAt = ConvertTo-FeedbackText -Value ([string]$_.submitted_at) -MaximumLength 64
                body = ConvertTo-FeedbackText -Value ([string]$_.body) -MaximumLength 2000
            }
        }
        inlineComments = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/pulls/$number/comments?per_page=100") `
                -Description "Listing inline review comments for $repository#$number"
        ) | Select-Object -First 100 | ForEach-Object {
            [ordered]@{
                author = ConvertTo-FeedbackText -Value ([string]$_.user.login) -MaximumLength 80
                path = ConvertTo-FeedbackText -Value ([string]$_.path) -MaximumLength 400
                line = if ($_.line -is [int] -or $_.line -is [long]) { [int]$_.line } else { $null }
                body = ConvertTo-FeedbackText -Value ([string]$_.body) -MaximumLength 2000
            }
        }
        commits = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/pulls/$number/commits?per_page=100") `
                -Description "Listing commits for $repository#$number"
        ) | ForEach-Object {
            [ordered]@{
                sha = ConvertTo-FeedbackText -Value ([string]$_.sha) -MaximumLength 64
                author = ConvertTo-FeedbackText -Value ([string]$_.commit.author.name) -MaximumLength 160
                authoredAt = ConvertTo-FeedbackText -Value ([string]$_.commit.author.date) -MaximumLength 64
                message = ConvertTo-FeedbackText -Value ([string]$_.commit.message) -MaximumLength 400
            }
        } | Select-Object -First 100
    })
}

$output = [ordered]@{
    repository = $repository
    generatedAt = [DateTimeOffset]::UtcNow.ToString('O')
    pullRequestCount = $exportedPulls.Count
    pullRequests = @($exportedPulls)
}
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$outputJson = $output | ConvertTo-Json -Depth 20
if ([Text.Encoding]::UTF8.GetByteCount($outputJson) -gt 4MB) {
    throw 'Feedback export exceeds the bounded output size.'
}
[System.IO.File]::WriteAllText(
    $OutputPath,
    $outputJson + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Write-Host "Exported feedback for $($exportedPulls.Count) reproduction PR(s) to $OutputPath."
