#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Offline release-role, detection, inventory, and handoff fixtures.
    No GitHub writes, remote services, or workflow configuration assertions.
#>
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scripts = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../scripts'))
. (Join-Path $scripts 'ReleaseBranchRoles.ps1')
. (Join-Path $scripts 'NightlyFeed.ps1')
. (Join-Path $scripts 'InternalOfficialBuild.ps1')
. (Join-Path $scripts 'New-ReleaseHandoff.ps1') -ReadinessJson unused
$fixture = Join-Path (Get-Location).Path ".artifacts/readiness-roles-$([guid]::NewGuid().ToString('N'))"
$passed = 0
function Assert-Role($Expected, $Actual, [string]$Label) {
    if ($Expected -cne $Actual) { throw "$Label — expected '$Expected', got '$Actual'." }
    $script:passed++
    Write-Host "PASS $Label"
}
function Invoke-FixtureGit {
    $output = & git -C $fixture @args 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git $args failed: $output" }
    return $output
}
function Set-FixtureVersion([int]$Major, [int]$Patch, [string]$Label = 'ci.main') {
    @"
<Project><PropertyGroup>
<MajorVersion>$Major</MajorVersion><MinorVersion>0</MinorVersion><PatchVersion>$Patch</PatchVersion>
<PreReleaseVersionLabel>$Label</PreReleaseVersionLabel>
<PreReleaseVersionIteration>1</PreReleaseVersionIteration>
<StabilizePackageVersion>false</StabilizePackageVersion>
</PropertyGroup></Project>
"@ | Set-Content (Join-Path $fixture 'eng/Versions.props')
}
function Write-FixtureState([int]$Sr = 1, [string]$NightlyMode = 'versioned') {
    @{
        schemaVersion = 1; repository = $fixture; releaseBand = '11.0.1xx'
        currentSr = $Sr; fixBranch = 'net11'; featureBranch = 'main'
        candidateNamingPolicy = 'unresolved'; nightlyMode = $NightlyMode
    } | ConvertTo-Json | Set-Content (Join-Path $fixture 'eng/release-rollover.json')
}
function Commit-Fixture([string]$Message) {
    $null = Invoke-FixtureGit add .
    $null = Invoke-FixtureGit commit --quiet -m $Message
}

try {
    $null = New-Item -ItemType Directory -Force (Join-Path $fixture 'eng')
    $null = New-Item -ItemType Directory -Force (Join-Path $fixture '.github/ISSUE_TEMPLATE')
    $null = Invoke-FixtureGit init --quiet -b main
    $null = Invoke-FixtureGit config user.name 'Readiness fixture'
    $null = Invoke-FixtureGit config user.email 'fixture@example.invalid'
    @"
- type: dropdown
  id: version-with-bug
  attributes:
    options:
      - 11.0.10
      - 10.0.80
"@ | Set-Content (Join-Path $fixture '.github/ISSUE_TEMPLATE/bug-report.yml')
    Set-FixtureVersion 11 0
    Commit-Fixture 'GA baseline'
    $ga = (Invoke-FixtureGit rev-parse HEAD).Trim()
    $null = Invoke-FixtureGit tag 11.0.0
    foreach ($prerelease in @(
        @{ Branch = 'release/11.0.1xx-preview1'; Tag = '11.0.0-preview.1.26001.1' }
        @{ Branch = 'release/11.0.1xx-preview7'; Tag = '11.0.100-preview.7.26410.2' }
        @{ Branch = 'release/11.0.1xx-rc1'; Tag = '11.0.100-rc.1.26458.5' }
    )) {
        $null = Invoke-FixtureGit branch $prerelease.Branch
        $null = Invoke-FixtureGit tag $prerelease.Tag
    }
    $null = Invoke-FixtureGit branch net11
    $null = Invoke-FixtureGit checkout --quiet net11
    Set-FixtureVersion 11 10
    Commit-Fixture 'Staged fix (#9101)'
    $null = Invoke-FixtureGit branch candidate/11.0.1xx-sr1
    'fix' | Set-Content (Join-Path $fixture 'fix.txt')
    Commit-Fixture 'Later stable fix (#9102)'
    $null = Invoke-FixtureGit checkout --quiet -b inflight/11.0.1xx-sr2
    Set-FixtureVersion 11 20
    Commit-Fixture 'Future SR2'
    $null = Invoke-FixtureGit checkout --quiet -b inflight/current
    Set-FixtureVersion 11 90
    Commit-Fixture 'Retained legacy nightly'
    $null = Invoke-FixtureGit checkout --quiet main
    Set-FixtureVersion 12 0 'preview'
    Commit-Fixture 'Next-major feature (#9201)'
    $null = Invoke-FixtureGit checkout --quiet -b net10.0 $ga
    Set-FixtureVersion 10 0
    Commit-Fixture 'Historical GA'
    $null = Invoke-FixtureGit tag 10.0.0
    Set-FixtureVersion 10 70
    Commit-Fixture 'Historical SR7'
    $null = Invoke-FixtureGit tag 10.0.70
    $null = Invoke-FixtureGit checkout --quiet -b release/10.0.1xx-sr8
    Set-FixtureVersion 10 80 'servicing'
    Commit-Fixture 'Historical SR8 fix (#9001)'
    $null = Invoke-FixtureGit tag 10.0.80
    $null = Invoke-FixtureGit checkout --quiet net10.0
    Set-FixtureVersion 10 90
    Commit-Fixture 'Historical SR9 candidate'
    $null = Invoke-FixtureGit checkout --quiet main
    Write-FixtureState
    $null = Invoke-FixtureGit remote add origin $fixture
    $null = Invoke-FixtureGit fetch --quiet origin

    $roles = Resolve-ReadinessBranchRoles -Major 11 -RepositoryPath $fixture
    Assert-Role net11 $roles.sourceBranch 'main12 does not own .NET11 fixes'
    Assert-Role $true $roles.configurationOnly 'role names do not certify protections or signed builds'
    Assert-Role candidate/11.0.1xx-sr1 $roles.candidateBranch 'state selects exact candidate'
    Assert-Role inflight/11.0.1xx-sr2 $roles.nightlyBranch 'versioned nightly follows future SR2'
    Assert-Role net10.0 (Resolve-ReadinessBranchRoles -Major 10 -RepositoryPath $fixture).sourceBranch 'historical major remains independent'
    Assert-Role main (Resolve-ReadinessBranchRoles -Major 12 -RepositoryPath $fixture).sourceBranch 'new-major preview source remains main'
    Assert-Role custom-source (Resolve-ReadinessBranchRoles -Major 11 -RepositoryPath $fixture -SourceBranch custom-source).sourceBranch 'explicit source override is honored'
    $nightly = Get-ReadinessNightlyParameters -Roles $roles -Major 11 -RepositoryPath $fixture
    Assert-Role '^11\.0\.20-ci\.inflight\.' $nightly.InflightPrefixRegex 'nightly version prefix comes from selected lane'
    $fetcher = {
        param($Url)
        if ($Url -like '*/v3/index.json') {
            return @{ resources = @(@{ '@type' = 'RegistrationsBaseUrl/3.6.0'; '@id' = 'https://fixture.invalid/registration' }) }
        }
        @{ items = @(@{ items = @(
            @{ catalogEntry = @{ version = '11.0.90-ci.inflight.1'; published = '2026-11-05T00:00:00Z' } }
            @{ catalogEntry = @{ version = '11.0.20-ci.main.1'; published = '2026-11-05T00:00:00Z' } }
            @{ catalogEntry = @{ version = '11.0.20-ci.inflight.1'; published = '2026-10-01T00:00:00Z' } }
        ) }) }
    }
    $freshness = Resolve-NightlyDogfoodFreshness -Feed dotnet11 -BandPrefixRegex '^11\.0\.10-' @nightly -Fetcher $fetcher
    Assert-Role '11.0.20-ci.inflight.1' $freshness.version 'fresh retained inflight/main cannot mask stale active nightly'
    Assert-Role stale (Get-NightlyFeedTier -Freshness $freshness -Now ([datetime]'2026-11-05')) 'active nightly staleness survives'
    $missing = Resolve-NightlyDogfoodFreshness -Feed dotnet11 -BandPrefixRegex '^11\.0\.20-' `
        -InflightPrefixRegex '^11\.0\.30-ci\.inflight\.' -RequireInflight $true -Fetcher $fetcher
    Assert-Role $false $missing.matched 'missing configured nightly cannot fall back to another band'
    $branches = @(Get-InternalOfficialBuildBranches -MajorVersion 11 -ReleaseBranch release/11.0.1xx-sr1 `
        -ReleaseBranchExists $false -SourceBranch $roles.nightlyBranch)
    Assert-Role refs/heads/inflight/11.0.1xx-sr2 ($branches -join ',') 'signed-build discovery uses configured nightly only'
    Write-FixtureState -NightlyMode legacy
    Assert-Role inflight/current (Resolve-ReadinessBranchRoles -Major 11 -RepositoryPath $fixture).nightlyBranch 'legacy nightly compatibility'
    Write-FixtureState

    $detectionPath = Join-Path $fixture 'trackers.json'
    & (Join-Path $scripts 'Find-ReleaseReadinessTrackers.ps1') -Repo $fixture -AllActiveMajors -NoFetch -OutputJson $detectionPath
    $detection = Get-Content $detectionPath -Raw | ConvertFrom-Json
    $sr1 = @($detection.majors | Where-Object majorVersion -eq 11).trackers | Where-Object canonicalKey -eq net11-sr1
    Assert-Role candidate/11.0.1xx-sr1 $sr1.surveyRef 'detector surveys staged SR1, not main12'
    Assert-Role net11 $sr1.sourceBranch 'detector exposes fix source'
    Assert-Role 0 @(@($detection.majors | Where-Object majorVersion -eq 11).trackers |
        Where-Object branchType -in @('preview', 'rc')).Count 'both historical .0 and SDK-band .100 shipped prerelease tags retire their lanes'
    Assert-Role $true ([string]::IsNullOrEmpty($sr1.priorSrBranch)) 'SR1 never fabricates SR0'
    Assert-Role 11.0.0 $sr1.candidateBaselineRef 'SR1 uses real immutable GA anchor'
    Assert-Role release/11.0.1xx-sr1 $sr1.candidateTargetBranch 'candidate target stays distinct from staged ref'
    Assert-Role $false $sr1.branchExists 'staged candidate does not imply release cut'
    Assert-Role 1 @($detection.majors | Where-Object majorVersion -eq 10).Count 'historical servicing major survives main rollover'
    $sr9 = @($detection.majors | Where-Object majorVersion -eq 10).trackers | Where-Object canonicalKey -eq net10-sr9
    Assert-Role net10.0 $sr9.surveyRef 'legacy SR candidate keeps historical source'
    Assert-Role 1 @(@($detection.majors | Where-Object majorVersion -eq 12).trackers | Where-Object canonicalKey -eq net12-preview1).Count 'new-major preview remains detected'

    $bin = Join-Path $fixture 'bin'
    $null = New-Item -ItemType Directory $bin
    "#!/bin/sh`nprintf '[]\n'`n" | Set-Content (Join-Path $bin 'gh')
    & chmod +x (Join-Path $bin 'gh')
    $priorPath = $env:PATH
    $env:PATH = "$bin$([IO.Path]::PathSeparator)$priorPath"
    Push-Location $fixture
    try {
        $out = Join-Path $fixture 'report'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
            -RegressionLabels regressed-in-11.0.0 -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $out
        $report = Get-Content (Join-Path $out 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role net11 $report.metadata.mainBranch 'report automatically resolves stable fixes'
        Assert-Role candidate/11.0.1xx-sr1 $report.metadata.srBranch 'report content coordinate is selected staged ref'
        Assert-Role 1 @($report.srContents.sourcePrs | Where-Object { $_ -eq 9101 }).Count 'selected candidate fix appears in source inventory'
        Assert-Role 0 @($report.srContents.sourcePrs | Where-Object { $_ -in @(9102, 9201) }).Count 'later fixes and main12 features excluded from inventory'
        Assert-Role source-ahead $report.metadata.candidateIntegration.status 'candidate/source relationship is verified'
        Assert-Role 1 $report.metadata.candidateIntegration.sourceOnlyCommitCount 'source fixes absent from candidate remain explicit'
        Assert-Role WATCH @($report.shipChecks | Where-Object Area -eq 'Candidate/source synchronization')[0].Status 'candidate-only inventory cannot certify a larger prospective cut'
        Assert-Role $true $report.surveyIncomplete 'partial fixture cannot claim full Ready'
        $handoff = New-ReleaseHandoffModel -Readiness $report
        Assert-Role release/11.0.1xx-sr1 $handoff.Branch 'handoff identifies SR1 without a prior SR'
        Assert-Role candidate/11.0.1xx-sr1 $handoff.SurveyRef 'handoff preserves staged evidence ref'
        Assert-Role net11 $handoff.SourceBranch 'handoff preserves actual fixes source'
        $text = Get-Content (Join-Path $out 'release-readiness.md') -Raw
        Assert-Role $false ($text -match '== main|SR0|awaiting main merge') 'candidate prose never relabels net11 as main or invents SR0'
        Assert-Role $true ($text -match 'not the full prospective cut') 'candidate report prominently warns about unrepresented source fixes'
        $null = Invoke-FixtureGit branch alternate-fixes net11
        $null = Invoke-FixtureGit fetch --quiet origin
        $overrideOut = Join-Path $fixture 'override-report'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
            -MainBranch alternate-fixes -SurveyRef net11 -BaselineRef 11.0.0 `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $overrideOut
        $overridden = Get-Content (Join-Path $overrideOut 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role alternate-fixes $overridden.metadata.sourceBranch 'report honors explicit source override'
        Assert-Role net11 $overridden.metadata.srBranch 'report honors explicit content override independently'
        Assert-Role 1 @($overridden.srContents.sourcePrs | Where-Object { $_ -eq 9102 }).Count 'explicit survey override changes actual inventory'

        $null = Invoke-FixtureGit branch release/11.0.1xx-sr1 net11
        $null = Invoke-FixtureGit fetch --quiet origin
        $cutOut = Join-Path $fixture 'cut-report'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $cutOut
        $cut = Get-Content (Join-Path $cutOut 'release-readiness.json') -Raw | ConvertFrom-Json
        $advance = @($cut.shipChecks | Where-Object Area -eq 'net11 bumped to SR2 cycle')
        Assert-Role BLOCKED $advance[0].Status 'feature main12 cannot satisfy net11 next-cycle advance'
        Assert-Role $true ($advance[0].NextAction -match 'net11') 'version-advance recommendation targets actual fixes source'
        $null = Invoke-FixtureGit branch -D release/11.0.1xx-sr1
        $null = Invoke-FixtureGit fetch --quiet --prune origin

        $historicalOut = Join-Path $fixture 'historical-report'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/10.0.1xx-sr8 -Candidate `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $historicalOut
        $historical = Get-Content (Join-Path $historicalOut 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role net10.0 $historical.metadata.srBranch 'legacy candidate interface resolves historical source automatically'
        Assert-Role release/10.0.1xx-sr9 $historical.metadata.targetSrBranch 'legacy prior-SR interface still selects next SR'

        $null = Invoke-FixtureGit tag -d 11.0.0
        & (Join-Path $scripts 'Find-ReleaseReadinessTrackers.ps1') -Repo $fixture -NoFetch -OutputJson $detectionPath
        $bootstrap = Get-Content $detectionPath -Raw | ConvertFrom-Json
        Assert-Role 11 $bootstrap.majorVersion 'default detector uses configured major after main12 rollover'
        Assert-Role 1 @($bootstrap.trackers | Where-Object canonicalKey -eq net11-sr1).Count 'missing GA evidence never silently omits configured current candidate'
        $failedClosed = $false
        try {
            & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
                -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir (Join-Path $fixture 'missing-baseline')
        } catch { $failedClosed = $_.Exception.Message -match 'no verified GA/prior stable-tag baseline' }
        Assert-Role $true $failedClosed 'missing bootstrap baseline fails explicitly, never CLEAN'
        $null = Invoke-FixtureGit tag 11.0.0 $ga
    } finally { Pop-Location; $env:PATH = $priorPath }

    $null = Invoke-FixtureGit checkout --quiet inflight/11.0.1xx-sr2
    Set-FixtureVersion 11 10 'ci.inflight'
    Commit-Fixture 'Unadvanced seeded nightly band'
    $null = Invoke-FixtureGit fetch --quiet origin
    $mismatch = $false
    try { $null = Get-ReadinessNightlyParameters -Roles $roles -Major 11 -RepositoryPath $fixture }
    catch { $mismatch = $_.Exception.Message -match 'PatchVersion 10; expected 20..29' }
    Assert-Role $true $mismatch 'SR2 branch suffix with SR1 package metadata is explicitly UNKNOWN'
    Push-Location $fixture
    $env:PATH = "$bin$([IO.Path]::PathSeparator)$priorPath"
    try {
        $mismatchOut = Join-Path $fixture 'mismatched-nightly-report'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $mismatchOut
        $mismatchedReport = Get-Content (Join-Path $mismatchOut 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role UNKNOWN @($mismatchedReport.shipChecks |
            Where-Object Area -eq 'Configured nightly lane metadata')[0].Status 'nightly version gap remains explicit in report JSON'
        Assert-Role $false ($mismatchedReport.verdict.label -eq 'Ready') 'missing nightly coherence cannot certify Ready'
    } finally { Pop-Location; $env:PATH = $priorPath }
    $null = Invoke-FixtureGit checkout --quiet candidate/11.0.1xx-sr1
    'staged only' | Set-Content (Join-Path $fixture 'staged-only.txt')
    Commit-Fixture "Staged-only fix (#9103)`n`nFixes #42"
    $null = Invoke-FixtureGit fetch --quiet origin
    Write-FixtureState
    Push-Location $fixture
    $priorTestMode = $env:GET_RELEASE_READINESS_TEST_MODE
    try {
        $env:GET_RELEASE_READINESS_TEST_MODE = '1'
        . (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1
        $context = Resolve-Context -SrBranch release/11.0.1xx-sr1 -Repo dotnet/maui -Candidate -NoFetch
        $contents = Get-SrCommits -Ctx $context
        $classified = Classify-RegressionCandidate -Issue @{ number = 42 } -CandidatePrs @() -Ctx $context -SrContents $contents
        Assert-Role 'merged-non-main-only' $classified.classification 'unintegrated staged fix is not falsely on net11'
        Assert-Role $false $classified.candidateFixPrs[0].onMain 'compatibility onMain field checks actual fixes ancestry'
        Assert-Role candidate/11.0.1xx-sr1 $classified.candidateFixPrs[0].baseRef 'staged fix evidence names selected candidate'
        Assert-Role $true ($classified.recommendedAction -match 'into net11') 'staged fix recommendation integrates into net11, not main12'
    } finally { Pop-Location; $env:GET_RELEASE_READINESS_TEST_MODE = $priorTestMode }
    $null = Invoke-FixtureGit checkout --quiet main
    Write-FixtureState -Sr 2
    $rotated = Resolve-ReadinessBranchRoles -Major 11 -RepositoryPath $fixture
    Assert-Role candidate/11.0.1xx-sr2 $rotated.candidateBranch 'state rotation automatically selects SR2'
    Assert-Role inflight/11.0.1xx-sr3 $rotated.nightlyBranch 'state rotation automatically selects SR3 nightly'
    $missingLaneFailed = $false
    try { $null = Get-ReadinessNightlyParameters -Roles $rotated -Major 11 -RepositoryPath $fixture }
    catch { $missingLaneFailed = $_.Exception.Message -match 'UNKNOWN' }
    Assert-Role $true $missingLaneFailed 'missing rotated lane cannot borrow retained older inflight'
    Write-Host "Passed: $passed; Failed: 0"
} finally {
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
