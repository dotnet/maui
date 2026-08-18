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

function Get-ReplicationCandidateText {
    <#
        .SYNOPSIS
        Reads an optional validated-candidate property without tripping StrictMode.
    #>
    param(
        [Parameter(Mandatory = $true)]$Candidate,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $property = $Candidate.PSObject.Properties[$Name]
    if (-not $property -or $null -eq $property.Value) {
        return ''
    }

    return [string]$property.Value
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
    # A validated document deserialised from JSON is a PSCustomObject, and
    # reading a property it does not carry throws under StrictMode. Build
    # 14999470 produced a ready candidate and then failed the whole publication
    # on exactly that, so these descriptive names are read defensively.
    $candidateTestClass = Get-ReplicationCandidateText -Candidate $Candidate -Name 'testClassName'
    $candidateTestMethod = Get-ReplicationCandidateText -Candidate $Candidate -Name 'testMethodName'
    $exactTestName = if ($candidateTestClass -and $candidateTestMethod) {
        ConvertTo-ReplicationInlineCode `
            -Value ("{0}.{1}" -f $candidateTestClass, $candidateTestMethod)
    } else {
        $testFilter
    }

    # Reviewers rejected evidence that called a simulator or emulator run
    # "on-device". Name the surface that actually ran the reproduction.
    $recordingSurface = switch ([string]$Candidate.platform) {
        'android' { 'Android emulator' }
        'ios' { 'iOS Simulator' }
        'windows' { 'Windows host' }
        'catalyst' { 'Mac Catalyst host' }
        default { "$platform host" }
    }
    $recordedDevice = if ($Evidence.PSObject.Properties['device']) {
        ConvertTo-ReplicationSingleLine -Value ([string]$Evidence.device) -MaximumLength 80
    } else {
        ''
    }
    if ($recordedDevice) {
        $recordingSurface = "$recordingSurface ``$recordedDevice``"
    }

    # Reviewers repeatedly read the platform above as a claim that the committed
    # test ran on that surface. Unit and XAML tests execute on the build host, so
    # the recording is evidence of the issue rather than of the test.
    $testHostDescription = switch ([string]$Candidate.testType) {
        'UnitTest' { "the **build host**, not the $recordingSurface. The recording below is evidence of the reported issue, not of this test executing." }
        'XamlUnitTest' { "the **build host**, not the $recordingSurface. The recording below is evidence of the reported issue, not of this test executing." }
        default { "the **$recordingSurface** used for the run above." }
    }
    # Publish what the test actually reported rather than what the agent
    # predicted; the validator already proved the two describe one defect.
    $rawFailureSignature = Get-ReplicationCandidateText `
        -Candidate $Candidate `
        -Name 'observedFailureSignature'
    if ([string]::IsNullOrWhiteSpace($rawFailureSignature)) {
        $rawFailureSignature = [string]$Candidate.expectedFailureSignature
    }
    $actualFailureMessage = [string]$Candidate.actualFailureMessage
    $normalizedActualMessage = ([regex]::Replace($actualFailureMessage, '\s+', ' ')).Trim()
    $normalizedSignature = ([regex]::Replace($rawFailureSignature, '\s+', ' ')).Trim()
    if ([string]::IsNullOrWhiteSpace($normalizedSignature) -or
        [string]::IsNullOrWhiteSpace($normalizedActualMessage) -or
        -not $normalizedActualMessage.Contains(
            $normalizedSignature,
            [StringComparison]::Ordinal)) {
        throw 'Validated candidate targeted failure message does not contain the expected failure signature.'
    }
    $verificationRunCount = 0
    $runCountProperty = $Candidate.PSObject.Properties['verificationRunCount']
    if ($runCountProperty) {
        $verificationRunCount = [int]$runCountProperty.Value
    }
    if ($verificationRunCount -lt 2) {
        throw 'Validated candidate does not prove the test failed in repeated independent runs.'
    }
    # Reviewers read a single sentence covering both the recording and the test
    # as a claim that the committed test ran on the recorded surface. A test in
    # a non-platform target framework would behave identically on a machine with
    # no platform SDK installed, so say which surface established which fact.
    $platformNeutralTestTypes = @('UnitTest', 'XamlUnitTest')
    $reproductionClaim = if ($platformNeutralTestTypes -contains [string]$Candidate.testType) {
        "- A trusted runner reproduced the behavior on the $recordingSurface. The committed test is platform-neutral: it ran on the build host and failed in $verificationRunCount consecutive executions, so it corroborates the same defect in cross-platform code rather than proving the $recordingSurface behavior itself."
    } else {
        "- A trusted runner reproduced the behavior on the $recordingSurface and matched the expected targeted test failure in $verificationRunCount consecutive executions."
    }
    $determinismLine = "- Determinism: the exact test above was executed **$verificationRunCount " +
        'independent times** on this baseline and failed at the same assertion every time'
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
- Base branch: the reproduction commit sits directly on the baseline above, so the first parent of the commit in this pull request is exactly the commit the device reproduction and the failing-test verification ran against, and this diff contains only the added reproduction test.
- Test type: **$testType**
- Test execution host: $testHostDescription
- Exact test: ``$exactTestName``
- Targeted filter: ``$testFilter`` — an issue-keyed class token; use the exact test above when a runner needs a precise selector
- Expected failing assertion: ``$failureSignature``
$determinismLine
$buildLine

## Recorded evidence ($recordingSurface)

[![Reproduction preview]($($Evidence.blobs.preview))]($($Evidence.blobs.video))

[Open the full MP4 recording]($($Evidence.blobs.video)) · [Evidence manifest]($($Evidence.blobs.manifest))

The authoritative proof is the trusted targeted test failing with the expected assertion above. The recording corroborates that; for defects with no visible symptom it may show only the app-reported verdict rather than the defect itself.

This recording is of the trusted Sandbox reproduction app that established the behavior on the $recordingSurface, not of the committed test executing. Its on-screen text therefore comes from that Sandbox app and will not match the assertion payload emitted by the committed test. Treat it as corroboration of the symptom, not as exact-head evidence for the commit in this pull request.

## Reproduction steps

$($steps -join [Environment]::NewLine)

## Safety and validation

- The pipeline reconstructed the scenario from issue text, inline snippets, and allowed raster screenshots.
- No linked repository, archive, binary, script, package, or arbitrary external file was downloaded.
$reproductionClaim
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

function Write-ReplicationPublicationManifest {
    param([Parameter(Mandatory)]$Plan)

    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        $script:OutputPath = Join-Path (Split-Path -Parent $PublishedEvidencePath) 'published-pr.json'
    }
    $directory = Split-Path -Parent $OutputPath
    if ($directory) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $Plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
    Write-Host "Replication pull request publication manifest: $OutputPath"
}

$plan = [ordered]@{
    issueNumber = $issueNumber
    platform = $platform
    branch = $branchName
    title = $prTitle
    body = $prBody
    marker = $marker
    files = @(Get-ValidatedCandidateFiles -Candidate $candidate)
    url = $null
    duplicateOf = $null
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
            # Build 15001510 reproduced issue 37151 and authored its test while
            # an earlier run was publishing the same issue and platform. The
            # second run is redundant, not broken, so it reports what already
            # covers the issue instead of failing the build.
            $plan.duplicateOf = [string]$duplicate.url
            Write-Host ("An open reproduction pull request already covers this issue and platform: " +
                "$($duplicate.url)")
            Write-ReplicationPublicationManifest -Plan $plan
            Pop-Location
            exit 0
        }

        # Reviewers verify the reproduction against the pull request's first
        # parent. Opening against a moving branch made that parent a different
        # commit from the one the device run and the failing test were verified
        # on, and three independent reviews reported it as a provenance defect.
        # Commit onto the verified baseline itself so the first parent is
        # exactly the commit the evidence describes. That keeps the diff equal
        # to the add-only patch only while the baseline is an ancestor of the
        # base branch, so prove that rather than assume it.
        $baselineSha = [string]$Candidate.baseSha
        if ($baselineSha -cnotmatch '^[0-9a-f]{40}$') {
            throw 'Validated candidate baseline commit is not a full lowercase SHA.'
        }
        & git cat-file -e "$baselineSha^{commit}" 2>$null
        if ($LASTEXITCODE -ne 0) {
            throw 'The validated baseline commit is missing from the publisher checkout.'
        }

        $targetRemote = 'replication-target'
        & git remote remove $targetRemote 2>$null
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('remote', 'add', $targetRemote, "https://github.com/$TargetOwner/$TargetRepository.git") `
            -Description 'Configuring reproduction target'
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('fetch', '--no-tags', $targetRemote, $BaseBranch) `
            -Description 'Fetching the pull request base branch'
        & git merge-base --is-ancestor $baselineSha FETCH_HEAD
        if ($LASTEXITCODE -ne 0) {
            throw ('The validated baseline is not contained in the pull request ' +
                'base branch, so the reproduction diff would carry unrelated commits.')
        }

        Invoke-ReplicationExternalCommand -FilePath 'git' -Arguments @('checkout', '--detach', $baselineSha) -Description 'Checking out the verified baseline'
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

Write-ReplicationPublicationManifest -Plan $plan
