#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Applies a validated reproduction patch and opens a draft pull request.

.DESCRIPTION
    Run only from a clean trusted checkout after candidate and evidence
    validation. The GitHub token must be provided through GH_TOKEN.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ValidatedCandidatePath,

    [Parameter(Mandatory = $true)]
    [string]$PublishedEvidencePath,

    [Parameter(Mandatory = $true)]
    [string]$IssueContextPath,

    [Parameter(Mandatory = $true)]
    [string]$PatchPath,

    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$IssueOwner = 'dotnet',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$IssueRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$TargetOwner = 'kubaflo',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$TargetRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9._/-]+$')]
    [string]$BaseBranch = 'main',

    [string]$BuildUrl = '',

    [string]$OutputPath = '',

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

. (Join-Path $PSScriptRoot 'Get-ReplicationGitHubLogin.ps1')

function ConvertTo-ReplicationSingleLine {
    param(
        [AllowEmptyString()][string]$Value,
        [int]$MaximumLength = 160
    )

    if ($null -eq $Value) {
        return ''
    }

    $safe = $Value -replace '[\x00-\x1F\x7F]', ' '
    $safe = $safe -replace '##vso\[[^\]]*\]', ''
    $safe = $safe -replace '##\[[^\]]*\]', ''
    $safe = $safe -replace '::(?:set-output|add-mask|error|warning|notice)[^\s]*', ''
    $safe = $safe -replace '\s+', ' '
    $safe = $safe.Trim()
    if ($safe.Length -gt $MaximumLength) {
        $safe = $safe.Substring(0, $MaximumLength).TrimEnd()
    }
    return $safe
}

function ConvertTo-ReplicationInlineCode {
    param([AllowEmptyString()][string]$Value)
    return (ConvertTo-ReplicationSingleLine -Value $Value -MaximumLength 500).Replace('`', "'")
}

function Get-ReplicationPullRequestMarker {
    param(
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Platform
    )

    return "<!-- MAUI_COPILOT_REPLICATION issue=$IssueNumber platform=$($Platform.ToLowerInvariant()) -->"
}

function New-ReplicationBranchName {
    param(
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Platform,
        [Parameter(Mandatory = $true)][string]$BuildId
    )

    $safePlatform = $Platform.ToLowerInvariant() -replace '[^a-z0-9-]', '-'
    $safeBuildId = $BuildId -replace '[^A-Za-z0-9._-]', '-'
    return "copilot/reproduce-$IssueNumber-$safePlatform-$safeBuildId"
}

function New-ReplicationPullRequestBody {
    param(
        [Parameter(Mandatory = $true)]$Candidate,
        [Parameter(Mandatory = $true)]$Evidence,
        [Parameter(Mandatory = $true)][string]$IssueTitle,
        [Parameter(Mandatory = $true)][string]$IssueOwner,
        [Parameter(Mandatory = $true)][string]$IssueRepository,
        [AllowEmptyString()][string]$BuildUrl
    )

    $issueNumber = [int]$Candidate.issueNumber
    $platform = ConvertTo-ReplicationSingleLine -Value ([string]$Candidate.platform) -MaximumLength 40
    $testType = ConvertTo-ReplicationInlineCode -Value ([string]$Candidate.testType)
    $testFilter = ConvertTo-ReplicationInlineCode -Value ([string]$Candidate.testFilter)
    $rawFailureSignature = [string]$Candidate.expectedFailureSignature
    $actualFailureMessage = [string]$Candidate.actualFailureMessage
    if ([string]::IsNullOrWhiteSpace($rawFailureSignature) -or
        [string]::IsNullOrWhiteSpace($actualFailureMessage) -or
        -not $actualFailureMessage.Contains(
            $rawFailureSignature,
            [StringComparison]::Ordinal)) {
        throw 'Validated candidate targeted failure message does not contain the expected failure signature.'
    }
    $failureSignature = ConvertTo-ReplicationInlineCode -Value $rawFailureSignature
    $baseSha = ConvertTo-ReplicationInlineCode -Value ([string]$Candidate.baseSha
    )
    $marker = Get-ReplicationPullRequestMarker -IssueNumber $issueNumber -Platform $platform
    $safeTitle = ConvertTo-ReplicationSingleLine -Value $IssueTitle -MaximumLength 180

    $steps = @()
    foreach ($step in @($Candidate.reproductionSteps)) {
        $safeStep = ConvertTo-ReplicationSingleLine -Value ([string]$step) -MaximumLength 300
        if ($safeStep) {
            $steps += "1. $safeStep"
        }
    }
    if ($steps.Count -eq 0) {
        $steps = @('1. Run the issue-specific scenario described in the linked issue.')
    }

    $buildLine = if ($BuildUrl) { "- Pipeline run: $BuildUrl" } else { '- Pipeline run: unavailable' }
    $issueUrl = "https://github.com/$IssueOwner/$IssueRepository/issues/$issueNumber"

    return @"
$marker

> [!IMPORTANT]
> This is AI-generated **reproduction evidence**, not a merge-ready product fix. The added test intentionally fails on the unfixed baseline. A product fix should make the test pass before this PR is considered for merge.

## Reproduced issue

- Issue: [$IssueOwner/$IssueRepository#$issueNumber — $safeTitle]($issueUrl)
- Platform: **$platform**
- Validated on baseline commit: ``$baseSha`` — the trusted device reproduction and the failing-test verification both ran against this commit
- Base branch: the reproduction commit is applied directly onto the current tip of the pull request base branch, so this diff contains only the added reproduction test. That tip, not the baseline above, is the parent of the commit in this pull request.
- Test type: **$testType**
- Targeted filter: ``$testFilter``
- Expected failing assertion: ``$failureSignature``
$buildLine

## Device evidence

[![Reproduction preview]($($Evidence.blobs.preview))]($($Evidence.blobs.video))

[Open the full MP4 recording]($($Evidence.blobs.video)) · [Evidence manifest]($($Evidence.blobs.manifest))

The authoritative proof is the trusted targeted test failing with the expected assertion above. The recording corroborates that; for defects with no visible symptom it may show only the app-reported verdict rather than the defect itself.

This recording is of the trusted Sandbox reproduction app that established the behavior on-device, not of the committed test executing. Its on-screen text therefore comes from that Sandbox app and will not match the assertion payload emitted by the committed test. Treat it as corroboration of the symptom, not as exact-head evidence for the commit in this pull request.

## Reproduction steps

$($steps -join [Environment]::NewLine)

## Safety and validation

- The pipeline reconstructed the scenario from issue text, inline snippets, and allowed raster screenshots.
- No linked repository, archive, binary, script, package, or arbitrary external file was downloaded.
- A trusted runner reproduced the behavior on-device and matched the expected targeted test failure.
- The published patch is add-only and restricted to approved MAUI test locations.
"@
}

function Get-ValidatedCandidateFiles {
    param([Parameter(Mandatory = $true)]$Candidate)

    $property = $Candidate.PSObject.Properties['files']
    if (-not $property) {
        $property = $Candidate.PSObject.Properties['addedFiles']
    }
    if (-not $property) {
        throw 'Validated candidate does not contain a files or addedFiles property.'
    }

    $files = @($property.Value | ForEach-Object { ([string]$_).Replace('\', '/') })
    if ($files.Count -eq 0) {
        throw 'Validated candidate does not list any added files.'
    }
    return $files
}

function Invoke-ReplicationExternalCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

function Resolve-ReplicationSourceRepository {
    param(
        [Parameter(Mandatory = $true)][string]$ParentOwner,
        [Parameter(Mandatory = $true)][string]$ParentRepository
    )

    $query = @'
query {
  viewer {
    login
    repositories(
      first: 100
      affiliations: [OWNER, ORGANIZATION_MEMBER]
    ) {
      nodes {
        nameWithOwner
        isFork
        viewerPermission
        parent {
          nameWithOwner
        }
      }
    }
  }
}
'@
    $responseJson = & gh api graphql -f "query=$query"
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to inspect repositories available to the publication token.'
    }
    $response = $responseJson | ConvertFrom-Json -Depth 20
    $expectedParent = "$ParentOwner/$ParentRepository"
    $matches = @($response.data.viewer.repositories.nodes) |
        Where-Object {
            $_.isFork -eq $true -and
            [string]$_.parent.nameWithOwner -eq $expectedParent -and
            [string]$_.viewerPermission -in @('WRITE', 'MAINTAIN', 'ADMIN')
        }
    if ($matches.Count -eq 0) {
        $createdForkJson = & gh api `
            -X POST `
            "repos/$ParentOwner/$ParentRepository/forks"
        if ($LASTEXITCODE -ne 0) {
            throw "MauiBot has no writable fork of $expectedParent and creating one failed."
        }
        $createdFork = $createdForkJson | ConvertFrom-Json -Depth 20
        $createdFullName = [string]$createdFork.full_name
        if ($createdFullName -notmatch '^[A-Za-z0-9-]+/[A-Za-z0-9._-]+$') {
            throw 'GitHub returned an invalid name for the newly created MauiBot fork.'
        }

        for ($attempt = 1; $attempt -le 12; $attempt++) {
            Start-Sleep -Seconds 5
            $forkJson = & gh api "repos/$createdFullName" 2>$null
            if ($LASTEXITCODE -eq 0) {
                $fork = $forkJson | ConvertFrom-Json -Depth 20
                if ($fork.fork -eq $true -and
                    [string]$fork.parent.full_name -eq $expectedParent -and
                    $fork.permissions.push -eq $true) {
                    return [pscustomobject]@{
                        Owner = ($createdFullName -split '/', 2)[0]
                        Repository = ($createdFullName -split '/', 2)[1]
                    }
                }
            }
        }
        throw 'The newly created MauiBot fork did not become writable within 60 seconds.'
    }
    if ($matches.Count -ne 1) {
        throw "Expected exactly one writable fork of $expectedParent; found $($matches.Count)."
    }

    $parts = ([string]$matches[0].nameWithOwner) -split '/', 2
    if ($parts.Count -ne 2 -or
        $parts[0] -notmatch '^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$' -or
        $parts[1] -notmatch '^[A-Za-z0-9._-]+$') {
        throw 'Resolved reproduction fork has an invalid repository name.'
    }
    return [pscustomobject]@{
        Owner = $parts[0]
        Repository = $parts[1]
    }
}

$candidate = Get-Content -LiteralPath $ValidatedCandidatePath -Raw | ConvertFrom-Json -Depth 50
if ($candidate.validationPassed -ne $true) {
    throw 'Candidate validation did not pass; a pull request will not be created.'
}

$evidence = Get-Content -LiteralPath $PublishedEvidencePath -Raw | ConvertFrom-Json -Depth 20
$context = Get-Content -LiteralPath $IssueContextPath -Raw | ConvertFrom-Json -Depth 20
$issueNumber = [int]$candidate.issueNumber
$platform = ConvertTo-ReplicationSingleLine -Value ([string]$candidate.platform) -MaximumLength 40
if ([int]$evidence.issueNumber -ne $issueNumber -or [string]$evidence.platform -ne [string]$candidate.platform) {
    throw 'Published evidence does not match the validated issue and platform.'
}

$issueTitle = if ($context.PSObject.Properties['title']) { [string]$context.title } else { "Issue #$issueNumber" }
$buildId = if ($env:BUILD_BUILDID) {
    if ($env:SYSTEM_JOBATTEMPT -match '^[1-9]\d*$') {
        "$($env:BUILD_BUILDID)-$($env:SYSTEM_JOBATTEMPT)"
    } else {
        $env:BUILD_BUILDID
    }
} else {
    [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
}
$branchName = New-ReplicationBranchName -IssueNumber $issueNumber -Platform $platform -BuildId $buildId
$marker = Get-ReplicationPullRequestMarker -IssueNumber $issueNumber -Platform $platform
$prTitle = "[$platform] Add failing reproduction for #$issueNumber"
$prBody = New-ReplicationPullRequestBody `
    -Candidate $candidate `
    -Evidence $evidence `
    -IssueTitle $issueTitle `
    -IssueOwner $IssueOwner `
    -IssueRepository $IssueRepository `
    -BuildUrl $BuildUrl

$plan = [ordered]@{
    issueNumber = $issueNumber
    platform = $platform
    branch = $branchName
    title = $prTitle
    body = $prBody
    marker = $marker
    files = @(Get-ValidatedCandidateFiles -Candidate $candidate)
    url = $null
}

if (-not $DryRun) {
    if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
        throw 'GH_TOKEN is required to publish the reproduction pull request.'
    }

    $authenticatedLogin = Get-ReplicationGitHubLogin
    if (-not $authenticatedLogin.Equals('MauiBot', [StringComparison]::OrdinalIgnoreCase)) {
        throw "GH_TOKEN must authenticate as 'MauiBot'."
    }
    $source = Resolve-ReplicationSourceRepository `
        -ParentOwner $IssueOwner `
        -ParentRepository $IssueRepository
    $sourceOwner = [string]$source.Owner
    $sourceRepository = [string]$source.Repository

    Push-Location $RepositoryRoot
    try {
        $status = & git status --porcelain
        if ($LASTEXITCODE -ne 0 -or $status) {
            throw 'The trusted publishing checkout must be clean before applying the reproduction patch.'
        }

        $openPullsJson = & gh pr list `
            --repo "$TargetOwner/$TargetRepository" `
            --state open `
            --limit 200 `
            --json 'number,body,url'
        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to query existing reproduction pull requests.'
        }

        $duplicate = @($openPullsJson | ConvertFrom-Json) | Where-Object {
            [string]$_.body -like "*$marker*"
        } | Select-Object -First 1
        if ($duplicate) {
            throw "An open reproduction pull request already exists for this issue/platform: $($duplicate.url)"
        }

        # The target repository's default branch is not the validated baseline,
        # so committing the patch onto the baseline makes the pull request diff
        # include every unrelated commit between them. Build the branch on the
        # actual pull request base instead, which leaves the diff equal to the
        # add-only patch. MauiBot cannot push a base branch into the target
        # repository, so the base branch itself must stay untouched; the
        # validated baseline remains recorded in the pull request body.
        $targetRemote = 'replication-target'
        & git remote remove $targetRemote 2>$null
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('remote', 'add', $targetRemote, "https://github.com/$TargetOwner/$TargetRepository.git") `
            -Description 'Configuring reproduction target'
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('fetch', '--no-tags', '--depth', '1', $targetRemote, $BaseBranch) `
            -Description 'Fetching the pull request base branch'

        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('checkout', '--detach', 'FETCH_HEAD') -Description 'Checking out the pull request base'
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('switch', '-c', $branchName) -Description 'Creating reproduction branch'
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('apply', '--index', '--whitespace=nowarn', $PatchPath) -Description 'Applying validated reproduction patch'

        $staged = @(& git diff --cached --name-status --diff-filter=ACDMRTUXB)
        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to inspect the staged reproduction patch.'
        }
        $actualFiles = @()
        foreach ($line in $staged) {
            if ($line -notmatch '^A\s+(.+)$') {
                throw "The staged patch is not add-only: $line"
            }
            $actualFiles += $Matches[1].Replace('\', '/')
        }

        $expectedFiles = @($plan.files | Sort-Object -Unique)
        $actualFiles = @($actualFiles | Sort-Object -Unique)
        if (($expectedFiles -join "`n") -ne ($actualFiles -join "`n")) {
            throw 'The staged files do not exactly match the validated candidate manifest.'
        }

        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('config', 'user.name', 'maui-copilot-replication') -Description 'Configuring git author'
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('config', 'user.email', '223556219+Copilot@users.noreply.github.com') -Description 'Configuring git email'
        Invoke-ReplicationExternalCommand -FilePath 'gh' -Arguments @('auth', 'setup-git') -Description 'Configuring bot Git authentication'

        $sourceRemote = 'replication-fork'
        & git remote remove $sourceRemote 2>$null
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('remote', 'add', $sourceRemote, "https://github.com/$sourceOwner/$sourceRepository.git") `
            -Description 'Configuring reproduction fork'

        $commitMessage = @"
Add failing reproduction for #$issueNumber on $platform

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
Copilot-Session: 735ac9a2-7bec-4baa-ad19-c298e5bc795a
"@
        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('commit', '-m', $commitMessage) -Description 'Committing reproduction test'
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('push', $sourceRemote, "HEAD:refs/heads/$branchName") `
            -Description 'Pushing reproduction branch'

        $bodyPath = Join-Path ([IO.Path]::GetTempPath()) "maui-replication-pr-$issueNumber-$buildId.md"
        try {
            $prBody | Set-Content -LiteralPath $bodyPath -Encoding utf8NoBOM
            $prUrl = & gh pr create `
                --repo "$TargetOwner/$TargetRepository" `
                --head "$sourceOwner`:$branchName" `
                --base $BaseBranch `
                --title $prTitle `
                --body-file $bodyPath `
                --draft
            if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace([string]$prUrl)) {
                throw 'Creating the draft reproduction pull request failed.'
            }
            $plan.url = ([string]$prUrl).Trim()
        }
        finally {
            Remove-Item -LiteralPath $bodyPath -Force -ErrorAction SilentlyContinue
        }
    }
    finally {
        Pop-Location
    }
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path (Split-Path -Parent $PublishedEvidencePath) 'published-pr.json'
}
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
Write-Host "Replication pull request publication manifest: $OutputPath"
