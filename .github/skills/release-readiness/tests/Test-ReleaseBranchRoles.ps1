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
    $null = Invoke-FixtureGit branch release/11.0.1xx $ga
    $null = Invoke-FixtureGit tag 11.0.0
    foreach ($prerelease in @(
        @{ Branch = 'release/11.0.1xx-preview1'; Tag = '11.0.0-preview.1.26001.1' }
        @{ Branch = 'release/11.0.1xx-preview7'; Tag = '11.0.100-preview.7.26410.2' }
        @{ Branch = 'release/11.0.1xx-rc1'; Tag = '11.0.100-rc.1.26458.5' }
    )) {
        $null = Invoke-FixtureGit branch $prerelease.Branch
        $null = Invoke-FixtureGit tag $prerelease.Tag
    }
    $null = Invoke-FixtureGit branch net11.0 release/11.0.1xx-preview7
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
    $liveNightly = Get-ReadinessNightlyParameters -Roles $roles -Major 11 -RepositoryPath $fixture `
        -VersionContent '<Project><MajorVersion>11</MajorVersion><PatchVersion>21</PatchVersion></Project>'
    Assert-Role '^11\.0\.21-ci\.inflight\.' $liveNightly.InflightPrefixRegex 'live content overrides stale local patch20'
    Assert-Role 0 (Get-ReadinessNightlyParameters -Roles (Resolve-ReadinessBranchRoles -Major 12 -RepositoryPath $fixture) `
        -Major 12 -RepositoryPath $fixture).Count 'unconfigured major has no selected nightly constraints'
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
    Assert-Role $ga $sr1.candidateBaselineRef 'SR1 pins the GA release branch without a tag dependency'
    Assert-Role release/11.0.1xx $sr1.baselineBranch 'SR1 baseline is the owner-designated GA branch'
    Assert-Role $true $sr1.tagIndependent 'configured progress does not infer lifecycle from tags'
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
            -MainBranch alternate-fixes -SurveyRef net11 -BaselineRef $ga `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $overrideOut
        $overridden = Get-Content (Join-Path $overrideOut 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role alternate-fixes $overridden.metadata.sourceBranch 'report honors explicit source override'
        Assert-Role net11 $overridden.metadata.srBranch 'report honors explicit content override independently'
        Assert-Role 1 @($overridden.srContents.sourcePrs | Where-Object { $_ -eq 9102 }).Count 'explicit survey override changes actual inventory'
        $previewOut = Join-Path $fixture 'legacy-preview-report'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-preview7 `
            -MainBranch net11.0 -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $previewOut
        $legacyPreview = Get-Content (Join-Path $previewOut 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role release/11.0.1xx-preview7 $legacyPreview.metadata.srBranch 'legacy prerelease survey still executes through entrypoint'

        $null = Invoke-FixtureGit branch release/11.0.1xx-sr1 net11
        $null = Invoke-FixtureGit fetch --quiet origin
        $cutOut = Join-Path $fixture 'cut-report'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $cutOut
        $cut = Get-Content (Join-Path $cutOut 'release-readiness.json') -Raw | ConvertFrom-Json
        $advance = @($cut.shipChecks | Where-Object Area -eq 'net11 bumped to SR2 cycle')
        Assert-Role BLOCKED $advance[0].Status 'feature main12 cannot satisfy net11 next-cycle advance'
        Assert-Role $true ($advance[0].NextAction -match 'net11') 'version-advance recommendation targets actual fixes source'
        $null = Invoke-FixtureGit tag 11.0.10 release/11.0.1xx-sr1
        & (Join-Path $scripts 'Find-ReleaseReadinessTrackers.ps1') -Repo $fixture -NoFetch -OutputJson $detectionPath
        $taggedCut = Get-Content $detectionPath -Raw | ConvertFrom-Json
        Assert-Role in-flight @($taggedCut.trackers |
            Where-Object canonicalKey -eq net11-sr1)[0].mode 'creating a manual SR tag neither retires progress nor marks it shipped'
        Assert-Role 2 @($cut.srContents.sourcePrs | Where-Object { $_ -in @(9101, 9102) }).Count 'cut inventory includes integrated fixes against the pinned GA branch'
        Assert-Role $true ((Get-Content (Join-Path $cutOut 'release-readiness.md') -Raw) -match 'release-readiness-snapshot:') 'manual lifecycle is anchored to source snapshots, not tags'
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
        Assert-Role $ga @($bootstrap.trackers | Where-Object canonicalKey -eq net11-sr1)[0].candidateBaselineRef 'deleting a tag does not change the branch baseline'
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $out
        $withoutTag = Get-Content (Join-Path $out 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role 1 @($withoutTag.srContents.sourcePrs | Where-Object { $_ -eq 9101 }).Count 'tag absence does not block measured contents'
        $null = Invoke-FixtureGit tag 11.0.0 main
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $out
        $wrongTag = Get-Content (Join-Path $out 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role $ga $wrongTag.metadata.priorSrRef 'an unrelated manual tag cannot substitute feature-main as the baseline'
        Assert-Role candidate $wrongTag.metadata.mode 'creating a tag cannot mark the candidate shipped'
        $null = Invoke-FixtureGit branch -D release/11.0.1xx
        $null = Invoke-FixtureGit fetch --quiet --prune origin
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $out
        $missingBaseline = Get-Content (Join-Path $out 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role $false ($missingBaseline.verdict.label -eq 'Ready') 'missing source evidence is explicit, never CLEAN'
        Assert-Role UNKNOWN @($missingBaseline.shipChecks |
            Where-Object Area -eq 'Candidate inventory baseline')[0].Status 'missing branch evidence is diagnostic, not a tag gate'
        Assert-Role not-run $missingBaseline.inventory.status 'absent baseline never produces a measured inventory'
        Assert-Role $true $missingBaseline.surveyIncomplete 'missing inventory keeps the overall survey incomplete'
        Assert-Role $false ($missingBaseline.PSObject.Properties.Name -contains 'srContents') 'unrun commit/source-PR counts are absent'
        Assert-Role $false ($missingBaseline.PSObject.Properties.Name -contains 'regressions') 'unrun regressions are not reported as zero findings'
        Assert-Role $false (Test-Path (Join-Path $out 'sr-source-prs.txt')) 'a prior source-PR inventory cannot survive a missing-baseline report'
        Assert-Role $false (Test-Path (Join-Path $out 'sr-commits.json')) 'a prior commit inventory cannot survive a missing-baseline report'
        $missingText = Get-Content (Join-Path $out 'release-readiness.md') -Raw
        Assert-Role $true ($missingText -match 'Release contents.*NOT RUN') 'missing-baseline Markdown visibly labels the unrun contents'
        Assert-Role $false ($missingText -match 'No blocking regressions|No risk-tier regressions|What.s New in SR.*0 commits') 'no clean/zero inventory is inferred from missing evidence'
        $failedClosed = $false
        try {
            & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
                -BaselineRef origin/nonexistent-source -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks `
                -OutputDir (Join-Path $fixture 'invalid-baseline')
        } catch { $failedClosed = $_.Exception.Message -match 'not found' }
        Assert-Role $true $failedClosed 'an explicitly supplied nonexistent baseline remains a hard error'
        $null = Invoke-FixtureGit branch release/11.0.1xx $ga
        $null = Invoke-FixtureGit fetch --quiet origin
        & (Join-Path $scripts 'Get-ReleaseReadiness.ps1') -SrBranch release/11.0.1xx-sr1 -Candidate `
            -Phase commits -NoFetch -SkipMilestoneChecks -SkipMaestroChecks -OutputDir $out
        $restoredBaseline = Get-Content (Join-Path $out 'release-readiness.json') -Raw | ConvertFrom-Json
        Assert-Role 1 @($restoredBaseline.srContents.sourcePrs |
            Where-Object { $_ -eq 9101 }).Count 'restoring the GA branch restores measured contents despite the unrelated tag'
        Assert-Role 0 @($restoredBaseline.shipChecks |
            Where-Object Area -eq 'Candidate inventory baseline').Count 'the missing-baseline blocker disappears only after verified evidence returns'
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
        foreach ($parameterName in @('MainBranch', 'SurveyRef', 'BaselineRef', 'ExcludeBranches')) {
            $unsafeParameters = @{
                SrBranch = 'release/11.0.1xx-sr1'; Repo = 'dotnet/maui'; Candidate = $true; NoFetch = $true
            }
            $unsafeParameters[$parameterName] = "candidate/11.0.1xx-sr1 --output=$fixture/injected.txt"
            $rejectedArguments = $false
            try { $null = Resolve-Context @unsafeParameters }
            catch { $rejectedArguments = $_.Exception.Message -match 'one safe Git revision' }
            Assert-Role $true $rejectedArguments "$parameterName rejects injected Git arguments before querying history"
        }
        Assert-Role $false (Test-Path (Join-Path $fixture 'injected.txt')) 'revision validation cannot redirect Git log output'
        $context = Resolve-Context -SrBranch release/11.0.1xx-sr1 -Repo dotnet/maui -Candidate -NoFetch
        $pinnedContext = Resolve-Context -SrBranch release/11.0.1xx-sr1 -Repo dotnet/maui -Candidate `
            -SurveyRef $context.srHeadSha -NoFetch
        Assert-Role $context.srHeadSha $pinnedContext.srHeadSha 'one immutable commit remains a supported survey override'
        $contents = Get-SrCommits -Ctx $context
        $classified = Classify-RegressionCandidate -Issue @{ number = 42 } -CandidatePrs @() -Ctx $context -SrContents $contents
        Assert-Role 'merged-non-main-only' $classified.classification 'unintegrated staged fix is not falsely on net11'
        Assert-Role $false $classified.candidateFixPrs[0].onMain 'compatibility onMain field checks actual fixes ancestry'
        Assert-Role candidate/11.0.1xx-sr1 $classified.candidateFixPrs[0].baseRef 'staged fix evidence names selected candidate'
        Assert-Role $true ($classified.recommendedAction -match 'into net11') 'staged fix recommendation integrates into net11, not main12'
        $sharedContents = $contents.Clone()
        $sharedContents.commits = @(@{
            sha = (Invoke-FixtureGit rev-parse 'origin/candidate/11.0.1xx-sr1~1').Trim()
            sourcePr = 9101; sourcePrs = @(9101); backportPr = $null; fixedIssues = @(44)
        })
        $shared = Classify-RegressionCandidate -Issue @{ number = 44 } -CandidatePrs @() -Ctx $context -SrContents $sharedContents
        Assert-Role merged-on-main-no-backport $shared.classification 'shared fixing commit is on source despite candidate-only child'
        Assert-Role $true $shared.candidateFixPrs[0].onMain 'source ancestry belongs to fixing commit, not candidate tip'
        $mixedContents = $sharedContents.Clone()
        $mixedContents.commits = @($sharedContents.commits) + @(@{
            sha = $context.srHeadSha; sourcePr = 9103; sourcePrs = @(9103); backportPr = $null; fixedIssues = @(44)
        })
        $mixed = Classify-RegressionCandidate -Issue @{ number = 44 } -CandidatePrs @() -Ctx $context -SrContents $mixedContents
        Assert-Role $true @($mixed.candidateFixPrs | Where-Object number -eq 9101)[0].onMain 'mixed fixes retain source-integrated PR evidence'
        Assert-Role $false @($mixed.candidateFixPrs | Where-Object number -eq 9103)[0].onMain 'mixed fixes do not relabel staged-only PR evidence'

        $originalGh = (Get-Command Invoke-Gh).ScriptBlock
        try {
            $managedPr = @{
                number = 9911; title = '[release] SR1 candidate'; headRefName = $context.srBranch
                author = @{ login = 'github-actions[bot]' }
            }
            $botProofJson = @{
                user = @{ login = 'github-actions[bot]'; type = 'Bot' }
                head = @{ ref = $context.srBranch; repo = @{ full_name = 'dotnet/maui' } }
                base = @{ ref = 'net11'; repo = @{ full_name = 'dotnet/maui' } }
                body = '<!-- maui-release-tracking:production:candidate -->'
            } | ConvertTo-Json -Depth 10
            $botProof = $botProofJson | ConvertFrom-Json -AsHashtable
            $proofReadUnavailable = $false
            Set-Item Function:Invoke-Gh -Value {
                param([string[]]$GhArgs, [switch]$Quiet)
                if ($GhArgs[0] -eq 'pr') { return ConvertTo-Json -InputObject @($managedPr) -Depth 10 }
                if ($GhArgs -contains '--jq') { return 'CONTRIBUTOR' }
                if ($proofReadUnavailable) { return $null }
                return $botProof | ConvertTo-Json -Depth 10
            }
            $acceptedBot = Get-CandidatePrResolution -Ctx $context
            Assert-Role 1 @($acceptedBot.candidates).Count 'verified managed bot candidate is accepted'
            foreach ($tamper in @(
                { param($proof) $proof.body = '' }
                { param($proof) $proof.user.type = 'User' }
                { param($proof) $proof.user.login = 'unrelated-bot' }
                { param($proof) $proof.head.repo.full_name = 'fork/maui' }
                { param($proof) $proof.base.ref = 'main' }
                { param($proof) $proof.head.ref = 'unrelated' }
                { param($proof) $proof.head.repo = $null }
            )) {
                $botProof = $botProofJson | ConvertFrom-Json -AsHashtable
                & $tamper $botProof
                $rejectedBot = Get-CandidatePrResolution -Ctx $context
                Assert-Role 0 @($rejectedBot.candidates).Count 'unverified bot identity, route, or marker is rejected'
            }
            $proofReadUnavailable = $true
            $unverifiableBot = Get-CandidatePrResolution -Ctx $context
            Assert-Role 0 @($unverifiableBot.candidates).Count 'missing bot proof fails closed'
            Assert-Role 1 $unverifiableBot.unverifiable 'missing bot proof is not mislabeled as confirmed spoofing'
        } finally { Set-Item Function:Invoke-Gh -Value $originalGh }
        $originalPrInfo = (Get-Command Get-PrInfo).ScriptBlock
        $originalBackports = (Get-Command Get-BackportPrsForSr).ScriptBlock
        try {
            $routingFix = @{
                number = 9110; title = 'Reviewed fix'; body = 'Fixes #43'
                state = 'OPEN'; baseRefName = 'net11'; mergedAt = $null; mergeCommit = $null
                files = @(@{ path = 'src/Core/src/Fixture.cs' })
            }
            Set-Item Function:Get-PrInfo -Value { param($Repo, $PrNumber) $routingFix }
            Set-Item Function:Get-BackportPrsForSr -Value { param($Repo, $SrBranch, $SourcePrNumber) @() }
            $reviewed = Classify-RegressionCandidate -Issue @{ number = 43; state = 'OPEN' } -CandidatePrs @(9110) -Ctx $context -SrContents $contents
            Assert-Role open-on-main $reviewed.classification 'initial net11 review keeps compatibility classification'
            Assert-Role $true ($reviewed.recommendedAction -match 'After approval, a maintainer manually retargets.*inflight/11.0.1xx-sr2') 'approved fix is manually retargeted to active inflight'
            Assert-Role $false ($reviewed.recommendedAction -match 'Wait for.*net11 merge') 'readiness never requests a direct net11 fix merge'
            $routingFix.state = 'MERGED'
            $routingFix.baseRefName = 'inflight/11.0.1xx-sr2'
            $routingFix.mergedAt = '2026-11-06T00:00:00Z'
            $routingFix.mergeCommit = @{ oid = (Invoke-FixtureGit rev-parse origin/inflight/11.0.1xx-sr2).Trim() }
            $stagedFix = Classify-RegressionCandidate -Issue @{ number = 43; state = 'OPEN' } -CandidatePrs @(9110) -Ctx $context -SrContents $contents
            Assert-Role merged-non-main-only $stagedFix.classification 'merged inflight fix remains staged, not falsely released'
            Assert-Role $true ($stagedFix.recommendedAction -match 'already staged.*through candidate integration') 'inflight fix follows candidate promotion rather than direct net11 flow'
        } finally {
            Set-Item Function:Get-PrInfo -Value $originalPrInfo
            Set-Item Function:Get-BackportPrsForSr -Value $originalBackports
        }
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
    Write-FixtureState
    $null = Invoke-FixtureGit checkout --quiet -b release/11.0.1xx-sr1 net11
    Set-FixtureVersion 11 20
    Commit-Fixture 'Incorrect SR1 release metadata'
    $null = Invoke-FixtureGit fetch --quiet origin
    $invalidReleaseFailed = $false
    try {
        & (Join-Path $scripts 'Find-ReleaseReadinessTrackers.ps1') -Repo $fixture -MajorVersion 11 -NoFetch -OutputJson $detectionPath
    } catch { $invalidReleaseFailed = $_.Exception.Message -match "Configured release.*patch 20.*outside SR1" }
    Assert-Role $true $invalidReleaseFailed 'rejected configured release cannot silently remove both release and candidate'
    $null = Invoke-FixtureGit checkout --quiet main
    $null = Invoke-FixtureGit branch -D release/11.0.1xx-sr1
    $null = Invoke-FixtureGit checkout --quiet --orphan release/10.0.1xx-sr13 $ga
    Set-FixtureVersion 10 130 'servicing'
    $savedAuthorDate = $env:GIT_AUTHOR_DATE
    $savedCommitterDate = $env:GIT_COMMITTER_DATE
    try {
        $env:GIT_AUTHOR_DATE = $env:GIT_COMMITTER_DATE = '2020-01-01T00:00:00Z'
        Commit-Fixture 'Idle historical SR13'
    } finally {
        $env:GIT_AUTHOR_DATE = $savedAuthorDate
        $env:GIT_COMMITTER_DATE = $savedCommitterDate
    }
    $null = Invoke-FixtureGit tag 10.0.130
    $null = Invoke-FixtureGit checkout --quiet net10.0
    Set-FixtureVersion 10 140
    Commit-Fixture 'Fresh historical SR14 fixes'
    $null = Invoke-FixtureGit checkout --quiet main
    Write-FixtureState
    $null = Invoke-FixtureGit fetch --quiet --prune origin
    & (Join-Path $scripts 'Find-ReleaseReadinessTrackers.ps1') -Repo $fixture -AllActiveMajors -NoFetch -OutputJson $detectionPath
    $historicalDetection = Get-Content $detectionPath -Raw | ConvertFrom-Json
    Assert-Role 1 @($historicalDetection.majors | Where-Object majorVersion -eq 10).Count 'fresh fixes keep the historical servicing major active'
    $historicalTrackers = @($historicalDetection.majors | Where-Object majorVersion -eq 10).trackers
    Assert-Role 1 @($historicalTrackers | Where-Object canonicalKey -eq net10-sr14).Count 'fresh historical fixes retain SR14 despite an idle shipped branch'
    Assert-Role 1 @($historicalTrackers | Where-Object { $_.canonicalKey -eq 'net10-sr13' -and $_.mode -eq 'shipped' }).Count 'fresh historical fixes retain the shipped SR13 follow-up'
    Write-Host "Passed: $passed; Failed: 0"
} finally {
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
