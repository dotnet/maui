#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the fixed iOS plus Mac Catalyst production-composite diagnostic.

.DESCRIPTION
    This is a report-only, trusted-code diagnostic. It runs one checked-in fixture
    against one immutable baseline product file and one immutable known-negative
    postimage. It never evaluates issue content, invokes a model, publishes an issue
    outcome, or certifies a product change.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-f]{40}$')]
    [string]$ExpectedSourceVersion,

    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [Parameter(Mandatory = $true)]
    [string]$TrustedRoot,

    [Parameter(Mandatory = $true)]
    [string]$TrustedTreeAttestation,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:CatalystProbeBaselineCommit =
'40590267d8057fd5c044e5bfea77a9dd31fef29f'
$script:CatalystProbeNegativeCommit =
'e456312886ee33fc0e69307e030c3028597ee32e'
$script:CatalystProbeProductPath =
'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs'
$script:CatalystProbeBaselineBlob =
'68393011a77138e20c05194e268e0751b5bb3198'
$script:CatalystProbeNegativeBlob =
'3e79d48f36a378f7618694d1472812288cbd82e0'
$script:CatalystProbeBaselineFileSha256 =
'bc69873cba98ef8aac4dda183439e18a8fe47973598f1d50fb9b40ce43c343f0'
$script:CatalystProbeNegativeFileSha256 =
'de13d4752e3e13b62d44932ae5a10f479fc798aca1e7fc4bd7d0f489934b4c08'
$script:CatalystProbePatchSha256 =
'07ae3f4da0a04ebcf22c2d395994b4dd68fb66c9ae38889c0dc5d782e2655775'
$script:CatalystProbeFixtureRelativePath =
'scripts/fixtures/ReplicationGesturePlatformManagerRegression.iOS.cs'
$script:CatalystProbeFixtureTargetRelativePath =
'src/Controls/tests/DeviceTests/ReplicationGesturePlatformManagerRegression.iOS.cs'
$script:CatalystProbeFixtureSha256 =
'9df820c9d684243f88dd4cc39c8090071299cba5e1731e8857e686aec66a0794'
$script:CatalystProbeClass =
'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression'
$script:CatalystProbeMethods = @(
    'SecondaryToBothCreatesNativeTap'
    'SecondaryToPrimaryCreatesNativeTap'
)
$script:CatalystProbePrimaryAnchorPath =
'src/Controls/tests/DeviceTests/Elements/Label/ReplicationProductionCompositeProbe.iOS.cs'
$script:CatalystProbePrimaryCategory = 'Label'
$script:CatalystProbePrimaryClass =
'Microsoft.Maui.DeviceTests.LabelTests'
$script:CatalystProbeProductionModuleName =
'Maui.CatalystProductionCompositeProbe'
$script:CatalystProbeExpectedCompanionRejection =
'The fixed Catalyst companion requires exactly two passing tests and retained raw XML.'
$script:CatalystProbeProjectPath =
'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
$script:CatalystProbeTargetFramework = 'net10.0-maccatalyst'
$script:CatalystProbeRuntimeIdentifier = 'maccatalyst-arm64'
$script:CatalystProbeXcodeVersion = '26.0.1'
$script:CatalystProbeIosRuntimeIdentifier =
'com.apple.CoreSimulator.SimRuntime.iOS-26-0'
$script:CatalystProbeIosDeviceType =
'com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro'
$script:CatalystProbeIosDeviceName =
'Maui Catalyst Production Composite Probe'
$script:CatalystProbeCycleBudgetSeconds = 480
$script:CatalystProbeCycleEvidenceBudgetSeconds = 30
$script:CatalystProbeVerifierBudgetSeconds =
$script:CatalystProbeCycleBudgetSeconds -
$script:CatalystProbeCycleEvidenceBudgetSeconds
$script:CatalystProbePrewarmBudgetSeconds = 600
$script:CatalystProbePatchApplyBudgetSeconds = 120
$script:CatalystProbeCoordinationBudgetSeconds = 180
$script:CatalystProbeOverallBudgetSeconds = 2040
$script:CatalystProbeTaskTimeoutSeconds = 2160
$script:CatalystProbeTaskTerminationReserveSeconds = 120
$script:CatalystProbeArtifactTailSeconds = 300
$script:CatalystProbeCleanupBudgetSeconds = 120
$script:CatalystProbeSummaryBudgetSeconds = 60
$script:CatalystProbeProcessTerminationSeconds = 10
$script:CatalystProbePrewarmCleanupReserveSeconds = 30
$script:CatalystProbeForbiddenEnvironmentNames = @(
    'GH_TOKEN'
    'GITHUB_TOKEN'
    'COPILOT_GITHUB_TOKEN'
    'SYSTEM_ACCESSTOKEN'
    'AZURE_DEVOPS_EXT_PAT'
    'SYSTEMVSSCONNECTION'
    'ENDPOINT_AUTH_SYSTEMVSSCONNECTION'
)

function Get-CatalystProbeXcodePath {
    return "/Applications/Xcode_$($script:CatalystProbeXcodeVersion).app"
}

function Assert-CatalystProbeXcodeVersion {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][string[]]$VersionOutput
    )

    if ($VersionOutput.Count -ne 2 -or
        $VersionOutput[0] -cne "Xcode $($script:CatalystProbeXcodeVersion)" -or
        $VersionOutput[1] -cnotmatch '^Build version [A-Za-z0-9]{1,32}$') {
        throw "Catalyst probe requires the fixed baseline Xcode $($script:CatalystProbeXcodeVersion)."
    }
}

function Get-CatalystProbeFileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.Length -le 0 -or $item.Length -gt 64MB) {
        throw "Catalyst probe requires a bounded regular file: $Path"
    }
    return (Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-CatalystProbeHostFacts {
    return [pscustomobject]@{
        IsMacOS = [OperatingSystem]::IsMacOS()
        Architecture = [Runtime.InteropServices.RuntimeInformation]::
        OSArchitecture.ToString().ToLowerInvariant()
    }
}

function Assert-CatalystProbeHostBoundary {
    $facts = Get-CatalystProbeHostFacts
    if (-not [bool]$facts.IsMacOS -or
        [string]$facts.Architecture -cne 'arm64') {
        throw 'The Catalyst gesture probe requires a fresh arm64 macOS Azure job.'
    }
    return $facts
}

function Assert-CatalystProbeClosedAzureIdentity {
    param(
        [Parameter(Mandatory = $true)][string]$ExpectedSourceVersion,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][string]$TrustedTreeAttestation,
        [Parameter(Mandatory = $true)][string]$OutputDirectory
    )

    $null = Assert-CatalystProbeHostBoundary
    if ([string]$env:TF_BUILD -ine 'true' -or
        $env:SYSTEM_DEFINITIONID -cne '27723' -or
        $env:BUILD_REPOSITORY_NAME -cne 'dotnet/maui' -or
        $env:BUILD_SOURCEBRANCH -cne
        'refs/heads/copilot/replicate-issues-pipeline' -or
        $env:CATALYST_PROBE_MODE -cne 'catalyst-gesture-probe') {
        throw 'The Catalyst probe requires its exact closed Azure pipeline identity.'
    }
    if ($ExpectedSourceVersion -cnotmatch '^[0-9a-f]{40}$' -or
        $env:BUILD_SOURCEVERSION -cne $ExpectedSourceVersion) {
        throw 'Catalyst probe source version is not the validated pipeline source.'
    }

    $agentTemp = [string]$env:AGENT_TEMPDIRECTORY
    $pipelineWorkspace = [string]$env:PIPELINE_WORKSPACE
    if ([string]::IsNullOrWhiteSpace($agentTemp) -or
        [string]::IsNullOrWhiteSpace($pipelineWorkspace)) {
        throw 'The Catalyst probe requires its closed Azure workspace roots.'
    }
    $agentTemp = [IO.Path]::GetFullPath($agentTemp)
    $pipelineWorkspace = [IO.Path]::GetFullPath($pipelineWorkspace)
    $expectedRuntime = Join-Path $agentTemp 'catalyst-gesture-runtime'
    foreach ($binding in @(
            @{ Actual = $RepositoryRoot; Environment = 'BUILD_SOURCESDIRECTORY' },
            @{ Actual = $TrustedRoot; Environment = 'CATALYST_PROBE_TRUSTED_ROOT' },
            @{ Actual = $TrustedTreeAttestation;
                Environment = 'CATALYST_PROBE_TRUSTED_ATTESTATION' },
            @{ Actual = $OutputDirectory;
                Environment = 'CATALYST_PROBE_OUTPUT_ROOT' },
            @{ Actual = $expectedRuntime;
                Environment = 'CATALYST_PROBE_RUNTIME_ROOT' })) {
        $bound = [string][Environment]::GetEnvironmentVariable(
            $binding.Environment)
        if ([string]::IsNullOrWhiteSpace($bound) -or
            [IO.Path]::GetFullPath([string]$binding.Actual) -cne
            [IO.Path]::GetFullPath($bound)) {
            throw "Catalyst probe path does not match $($binding.Environment)."
        }
    }
    foreach ($root in @(
            $agentTemp,
            $pipelineWorkspace,
            $RepositoryRoot,
            $TrustedRoot)) {
        $item = Get-Item -LiteralPath $root -Force -ErrorAction Stop
        if (-not $item.PSIsContainer -or
            $item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'The Catalyst probe requires regular closed Azure roots.'
        }
    }
    if (Test-Path -LiteralPath $OutputDirectory) {
        $output = Get-Item -LiteralPath $OutputDirectory -Force
        if (-not $output.PSIsContainer -or
            $output.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'The Catalyst probe requires a regular published output root.'
        }
    }
    $attestation = Get-Item -LiteralPath $TrustedTreeAttestation `
        -Force -ErrorAction Stop
    if ($attestation.PSIsContainer -or
        $attestation.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'The Catalyst probe requires a regular trusted-tree attestation.'
    }
    Assert-CatalystProbeTrustedTree -Context ([pscustomobject]@{
            TrustedRoot = [IO.Path]::GetFullPath($TrustedRoot)
            TrustedTreeAttestation =
            [IO.Path]::GetFullPath($TrustedTreeAttestation)
            ExpectedSourceVersion = $ExpectedSourceVersion
        })
}

function New-CatalystProbeTaskDeadline {
    $accountedSeconds =
    $script:CatalystProbeCoordinationBudgetSeconds +
    $script:CatalystProbePrewarmBudgetSeconds +
    (2 * $script:CatalystProbeCycleBudgetSeconds) +
    $script:CatalystProbePatchApplyBudgetSeconds +
    $script:CatalystProbeCleanupBudgetSeconds +
    $script:CatalystProbeSummaryBudgetSeconds
    if ($accountedSeconds -ne $script:CatalystProbeOverallBudgetSeconds -or
        $script:CatalystProbeOverallBudgetSeconds +
        $script:CatalystProbeTaskTerminationReserveSeconds -ne
        $script:CatalystProbeTaskTimeoutSeconds) {
        throw 'Catalyst probe internal task budget is inconsistent.'
    }
    $entryText = [Environment]::GetEnvironmentVariable(
        'CATALYST_PROBE_TASK_ENTRY_TIMESTAMP')
    $frequencyText = [Environment]::GetEnvironmentVariable(
        'CATALYST_PROBE_TASK_STOPWATCH_FREQUENCY')
    $budgetText = [Environment]::GetEnvironmentVariable(
        'CATALYST_PROBE_TASK_BUDGET_SECONDS')
    [long]$entryTimestamp = 0
    [long]$frequency = 0
    [int]$budgetSeconds = 0
    if (-not [long]::TryParse($entryText, [ref]$entryTimestamp) -or
        -not [long]::TryParse($frequencyText, [ref]$frequency) -or
        -not [int]::TryParse($budgetText, [ref]$budgetSeconds) -or
        $entryTimestamp -le 0 -or
        $frequency -ne [Diagnostics.Stopwatch]::Frequency -or
        $budgetSeconds -ne $script:CatalystProbeOverallBudgetSeconds) {
        throw 'Catalyst probe requires its fixed monotonic task-entry deadline.'
    }
    $now = [Diagnostics.Stopwatch]::GetTimestamp()
    if ($entryTimestamp -gt $now -or
        $now - $entryTimestamp -ge $frequency * $budgetSeconds) {
        throw 'Catalyst probe monotonic task-entry deadline is invalid or already expired.'
    }
    return [pscustomobject]@{
        EntryTimestamp = $entryTimestamp
        DeadlineTimestamp = $entryTimestamp + ($frequency * $budgetSeconds)
        Frequency = $frequency
        BudgetSeconds = $budgetSeconds
    }
}

function Get-CatalystProbeDeadlineRemainingSeconds {
    param([Parameter(Mandatory = $true)][pscustomobject]$Deadline)

    $remainingTicks =
    [long]$Deadline.DeadlineTimestamp -
    [Diagnostics.Stopwatch]::GetTimestamp()
    return [int][Math]::Floor(
        [double]$remainingTicks / [double]$Deadline.Frequency)
}

function Get-CatalystProbeDeadlineRemainingMilliseconds {
    param([Parameter(Mandatory = $true)][pscustomobject]$Deadline)

    return [int][Math]::Max(0, [Math]::Floor(
            ([long]$Deadline.DeadlineTimestamp -
            [Diagnostics.Stopwatch]::GetTimestamp()) * 1000.0 /
            [long]$Deadline.Frequency))
}

function Get-CatalystProbeProcessTimeoutSeconds {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Deadline,
        [Parameter(Mandatory = $true)]
        [ValidateRange(1, 2100)][int]$RequestedSeconds,
        [ValidateRange(0, 600)][int]$ReserveSeconds = 0,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $remainingSeconds =
    Get-CatalystProbeDeadlineRemainingSeconds -Deadline $Deadline
    $availableSeconds = $remainingSeconds - $ReserveSeconds
    if ($availableSeconds -lt 1) {
        throw ("Catalyst probe refuses '$Description': its monotonic task deadline " +
            "must retain $ReserveSeconds seconds for cleanup and summary.")
    }
    return [Math]::Min($RequestedSeconds, $availableSeconds)
}

function Assert-CatalystProbeTaskPhaseAdmission {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Deadline,
        [Parameter(Mandatory = $true)]
        [ValidateRange(1, 2100)][int]$RequiredSeconds,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $remainingSeconds =
    Get-CatalystProbeDeadlineRemainingSeconds -Deadline $Deadline
    if ($remainingSeconds -lt $RequiredSeconds) {
        throw "Catalyst probe has insufficient monotonic task time for $Description."
    }
    return $remainingSeconds
}

function Get-CatalystProbeTerminationWaitMilliseconds {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Deadline,
        [ValidateRange(0, 600)][int]$ReserveSeconds = 0,
        [Parameter(Mandatory = $true)]
        [ValidateRange(0, 10)][int]$MaximumWaitSeconds
    )

    $remainingSeconds =
    Get-CatalystProbeDeadlineRemainingSeconds -Deadline $Deadline
    $availableSeconds = [Math]::Max(0, $remainingSeconds - $ReserveSeconds)
    return [Math]::Min(
        $MaximumWaitSeconds * 1000,
        $availableSeconds * 1000)
}

function New-CatalystProbeCleanupDeadline {
    param([Parameter(Mandatory = $true)][pscustomobject]$TaskDeadline)

    $availableSeconds = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $TaskDeadline `
        -RequestedSeconds $script:CatalystProbeCleanupBudgetSeconds `
        -ReserveSeconds $script:CatalystProbeSummaryBudgetSeconds `
        -Description 'cleanup'
    $now = [Diagnostics.Stopwatch]::GetTimestamp()
    return [pscustomobject]@{
        EntryTimestamp = $now
        DeadlineTimestamp =
        $now + ([long]$availableSeconds * [long]$TaskDeadline.Frequency)
        Frequency = [long]$TaskDeadline.Frequency
        BudgetSeconds = $availableSeconds
    }
}

function New-CatalystProbePrewarmDeadline {
    param([Parameter(Mandatory = $true)][pscustomobject]$TaskDeadline)

    $downstreamReserveSeconds =
    (2 * $script:CatalystProbeCycleBudgetSeconds) +
    $script:CatalystProbePatchApplyBudgetSeconds +
    $script:CatalystProbeCleanupBudgetSeconds +
    $script:CatalystProbeSummaryBudgetSeconds
    $parentRemainingSeconds =
    Get-CatalystProbeDeadlineRemainingSeconds -Deadline $TaskDeadline
    $availableSeconds = [Math]::Min(
        $script:CatalystProbePrewarmBudgetSeconds,
        $parentRemainingSeconds - $downstreamReserveSeconds)
    if ($availableSeconds -lt 1) {
        throw ('Catalyst probe refuses dual-Apple prewarm: its monotonic task ' +
            "deadline must retain $downstreamReserveSeconds seconds downstream.")
    }
    $now = [Diagnostics.Stopwatch]::GetTimestamp()
    return [pscustomobject]@{
        EntryTimestamp = $now
        DeadlineTimestamp = [Math]::Min(
            [long]$TaskDeadline.DeadlineTimestamp -
            ([long]$downstreamReserveSeconds * [long]$TaskDeadline.Frequency),
            $now + ([long]$availableSeconds * [long]$TaskDeadline.Frequency))
        Frequency = [long]$TaskDeadline.Frequency
        BudgetSeconds = $availableSeconds
        ParentEntryTimestamp = [long]$TaskDeadline.EntryTimestamp
        ParentDeadlineTimestamp = [long]$TaskDeadline.DeadlineTimestamp
        DownstreamReserveSeconds = $downstreamReserveSeconds
    }
}

function New-CatalystProbeCoordinationDeadline {
    param([Parameter(Mandatory = $true)][pscustomobject]$TaskDeadline)

    $downstreamReserveSeconds =
    $script:CatalystProbePrewarmBudgetSeconds +
    (2 * $script:CatalystProbeCycleBudgetSeconds) +
    $script:CatalystProbePatchApplyBudgetSeconds +
    $script:CatalystProbeCleanupBudgetSeconds +
    $script:CatalystProbeSummaryBudgetSeconds
    $deadlineTimestamp = [Math]::Min(
        [long]$TaskDeadline.EntryTimestamp +
        ([long]$script:CatalystProbeCoordinationBudgetSeconds *
        [long]$TaskDeadline.Frequency),
        [long]$TaskDeadline.DeadlineTimestamp -
        ([long]$downstreamReserveSeconds * [long]$TaskDeadline.Frequency))
    if ($deadlineTimestamp -le [Diagnostics.Stopwatch]::GetTimestamp()) {
        throw 'Catalyst probe exhausted coordination before production composite setup completed.'
    }
    return [pscustomobject]@{
        EntryTimestamp = [long]$TaskDeadline.EntryTimestamp
        DeadlineTimestamp = $deadlineTimestamp
        Frequency = [long]$TaskDeadline.Frequency
        BudgetSeconds = $script:CatalystProbeCoordinationBudgetSeconds
        DownstreamReserveSeconds = $downstreamReserveSeconds
    }
}

function New-CatalystProbeFixedPhaseDeadline {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$TaskDeadline,
        [Parameter(Mandatory = $true)][ValidateRange(1, 600)]
        [int]$BudgetSeconds,
        [Parameter(Mandatory = $true)][ValidateRange(0, 1800)]
        [int]$DownstreamReserveSeconds,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $parentRemainingSeconds =
    Get-CatalystProbeDeadlineRemainingSeconds -Deadline $TaskDeadline
    $availableSeconds = [Math]::Min(
        $BudgetSeconds,
        $parentRemainingSeconds - $DownstreamReserveSeconds)
    if ($availableSeconds -lt 1) {
        throw (
            "Catalyst probe refuses $Description because its monotonic task " +
            "deadline must retain $DownstreamReserveSeconds seconds downstream.")
    }
    $now = [Diagnostics.Stopwatch]::GetTimestamp()
    return [pscustomobject]@{
        EntryTimestamp = $now
        DeadlineTimestamp = [Math]::Min(
            [long]$TaskDeadline.DeadlineTimestamp -
            ([long]$DownstreamReserveSeconds * [long]$TaskDeadline.Frequency),
            $now + ([long]$availableSeconds * [long]$TaskDeadline.Frequency))
        Frequency = [long]$TaskDeadline.Frequency
        BudgetSeconds = $availableSeconds
        DownstreamReserveSeconds = $DownstreamReserveSeconds
    }
}

function ConvertTo-CatalystProbeLogData {
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string]$Text
    )

    if ($null -eq $Text) {
        return ''
    }
    return (($Text -replace '##vso\[[^\]]*\]', '') -replace
        '##\[(?:command|debug|error|group|section|warning|endgroup)\]', '')
}

function Get-CatalystProbeSafeError {
    param([Parameter(Mandatory = $true)][string]$Message)

    $safe = (ConvertTo-CatalystProbeLogData -Text $Message) -replace '[\r\n]+', ' '
    if ($safe.Length -gt 1024) {
        $safe = $safe.Substring(0, 1024)
    }
    return $safe
}

function Assert-CatalystProbePathUnderRoot {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $fullPath = [IO.Path]::GetFullPath($Path)
    $fullRoot = [IO.Path]::GetFullPath($Root).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    if (-not ($fullPath -ceq $fullRoot) -and
        -not $fullPath.StartsWith(
            "$fullRoot$([IO.Path]::DirectorySeparatorChar)",
            [StringComparison]::Ordinal)) {
        throw "$Description escapes its fixed root."
    }
    return $fullPath
}

function Test-CatalystProbePathOverlap {
    param(
        [Parameter(Mandatory = $true)][string]$First,
        [Parameter(Mandatory = $true)][string]$Second
    )

    $firstPath = [IO.Path]::GetFullPath($First).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $secondPath = [IO.Path]::GetFullPath($Second).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $separator = [IO.Path]::DirectorySeparatorChar
    return $firstPath -ceq $secondPath -or
    $firstPath.StartsWith("$secondPath$separator", [StringComparison]::Ordinal) -or
    $secondPath.StartsWith("$firstPath$separator", [StringComparison]::Ordinal)
}

function Resolve-CatalystProbePrivateRuntimeRoot {
    param(
        [Parameter(Mandatory = $true)][string]$RuntimeRoot,
        [Parameter(Mandatory = $true)][string]$AgentTempDirectory,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$TrustedRoot
    )

    $agentTemp = (Resolve-Path -LiteralPath $AgentTempDirectory).Path
    $expected = [IO.Path]::GetFullPath(
        (Join-Path $agentTemp 'catalyst-gesture-runtime'))
    $runtime = [IO.Path]::GetFullPath($RuntimeRoot)
    if ($runtime -cne $expected) {
        throw 'Catalyst probe runtime cache must use its fixed Agent.TempDirectory root.'
    }
    foreach ($publishedOrProtectedRoot in @(
            $OutputDirectory, $RepositoryRoot, $TrustedRoot)) {
        if (Test-CatalystProbePathOverlap `
                -First $runtime `
                -Second $publishedOrProtectedRoot) {
            throw 'Catalyst probe private runtime cache overlaps a published or protected root.'
        }
    }
    if (Test-Path -LiteralPath $runtime) {
        throw 'Catalyst probe refuses to reuse its private runtime cache.'
    }
    New-Item -ItemType Directory -Path $runtime | Out-Null
    $item = Get-Item -LiteralPath $runtime -Force
    if (-not $item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Catalyst probe private runtime cache must be a regular directory.'
    }
    return $item.FullName
}

function Assert-CatalystProbeBudgetSchedule {
    param(
        [Parameter(Mandatory = $true)][object[]]$Phases,
        [Parameter(Mandatory = $true)]
        [ValidateRange(1, 86400)][int]$AvailableSeconds,
        [Parameter(Mandatory = $true)]
        [ValidateRange(1, 3600)][int]$ArtifactTailSeconds
    )

    $names = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    [long]$phaseSeconds = 0
    foreach ($phase in $Phases) {
        $name = [string]$phase.name
        $maximum = [int]$phase.maximumSeconds
        $retries = [int]$phase.retryCount
        if ($name -cnotmatch '^[a-z][a-z0-9-]{0,63}$' -or
            -not $names.Add($name) -or
            $maximum -lt 1 -or $maximum -gt 7200 -or
            $retries -lt 0 -or $retries -gt 3) {
            throw 'Catalyst probe budget contains an invalid phase.'
        }
        $phaseSeconds += [long]$maximum * ([long]$retries + 1)
    }
    $requiredSeconds = $phaseSeconds + $ArtifactTailSeconds
    if ($requiredSeconds -gt $AvailableSeconds) {
        throw ("Catalyst probe worst-case phase and retry envelope requires " +
            "$requiredSeconds seconds but only $AvailableSeconds are available.")
    }
    return [pscustomobject]@{
        phaseSeconds = $phaseSeconds
        artifactTailSeconds = $ArtifactTailSeconds
        requiredSeconds = $requiredSeconds
        availableSeconds = $AvailableSeconds
        slackSeconds = $AvailableSeconds - $requiredSeconds
    }
}

function Get-CatalystProbeFixedPipelineBudget {
    param(
        [Parameter(Mandatory = $true)][DateTimeOffset]$StartedUtc
    )

    $phases = @(
        [ordered]@{ name = 'trusted-capture'; maximumSeconds = 120; retryCount = 0 }
        [ordered]@{ name = 'trusted-attestation'; maximumSeconds = 180; retryCount = 0 }
        [ordered]@{ name = 'public-object-fetch'; maximumSeconds = 240; retryCount = 0 }
        [ordered]@{ name = 'xcode-selection'; maximumSeconds = 180; retryCount = 0 }
        [ordered]@{ name = 'tool-restore'; maximumSeconds = 240; retryCount = 0 }
        [ordered]@{ name = 'workload-bootstrap'; maximumSeconds = 900; retryCount = 0 }
        [ordered]@{ name = 'build-tasks'; maximumSeconds = 600; retryCount = 0 }
        [ordered]@{ name = 'report-only-probe'; maximumSeconds = 2160; retryCount = 0 }
    )
    $availableSeconds = 5400
    $evaluation = Assert-CatalystProbeBudgetSchedule `
        -Phases $phases `
        -AvailableSeconds $availableSeconds `
        -ArtifactTailSeconds $script:CatalystProbeArtifactTailSeconds
    return [ordered]@{
        schemaVersion = 1
        capturedUtc = $StartedUtc.ToUniversalTime().ToString('O')
        safeJobDeadlineUtc = $StartedUtc.AddSeconds(
            $availableSeconds).ToUniversalTime().ToString('O')
        jobTimeoutSeconds = 6000
        checkoutTimeoutSeconds = 300
        validationTimeoutSeconds = 120
        availableAfterValidationSeconds = $availableSeconds
        configuredTaskEnvelopeSeconds = 5220
        phases = $phases
        artifactTailSeconds = $script:CatalystProbeArtifactTailSeconds
        requiredSeconds = $evaluation.requiredSeconds
        slackSeconds = $evaluation.slackSeconds
        configuredTaskSlackSeconds = $availableSeconds - 5220
    }
}

function Assert-CatalystProbeDeadlineAdmission {
    param(
        [Parameter(Mandatory = $true)][string]$DeadlineUtc,
        [Parameter(Mandatory = $true)][string]$Phase,
        [Parameter(Mandatory = $true)]
        [ValidateRange(1, 7200)][int]$PhaseBudgetSeconds,
        [ValidateRange(0, 7200)][int]$RemainingPhaseBudgetSeconds = 0,
        [ValidateRange(1, 3600)][int]$ArtifactTailSeconds =
        $script:CatalystProbeArtifactTailSeconds,
        [DateTimeOffset]$NowUtc = [DateTimeOffset]::UtcNow
    )

    $deadline = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParse(
            $DeadlineUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind,
            [ref]$deadline)) {
        throw 'Catalyst probe job deadline is not a valid immutable timestamp.'
    }
    $requiredSeconds =
    $PhaseBudgetSeconds + $RemainingPhaseBudgetSeconds + $ArtifactTailSeconds
    $remainingSeconds = [Math]::Floor(
        ($deadline.ToUniversalTime() - $NowUtc.ToUniversalTime()).TotalSeconds)
    if ($remainingSeconds -lt $requiredSeconds) {
        throw ("Catalyst probe refuses '$Phase': $remainingSeconds seconds remain, " +
            "but $requiredSeconds are reserved for this phase, later phases, and " +
            'the evidence-publication tail.')
    }
    return [pscustomobject]@{
        phase = $Phase
        admittedUtc = $NowUtc.ToUniversalTime().ToString('O')
        deadlineUtc = $deadline.ToUniversalTime().ToString('O')
        remainingSeconds = $remainingSeconds
        requiredSeconds = $requiredSeconds
        artifactTailSeconds = $ArtifactTailSeconds
    }
}

function Invoke-WithoutReplicationSecrets {
    param([Parameter(Mandatory = $true)][scriptblock]$Action)

    $saved = @{}
    try {
        $forbiddenNames = @(
            $script:CatalystProbeForbiddenEnvironmentNames
            Get-ReplicationForbiddenEnvironmentNames
        ) | Sort-Object -CaseSensitive -Unique
        foreach ($name in $forbiddenNames) {
            $saved[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        & $Action
    } finally {
        foreach ($name in $saved.Keys) {
            [Environment]::SetEnvironmentVariable($name, $saved[$name])
        }
    }
}

function Get-CatalystProbeRuntimeEnvironment {
    param([Parameter(Mandatory = $true)][string]$RuntimeRoot)

    $runtimeRoot = [IO.Path]::GetFullPath($RuntimeRoot)
    $nugetPackages = Join-Path $runtimeRoot 'nuget-packages'
    $dotnetHome = Join-Path $runtimeRoot 'dotnet-home'
    foreach ($directory in @($runtimeRoot, $nugetPackages, $dotnetHome)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $allowed = Get-ReplicationExecutionEnvironment -Additional @{
        NUGET_PACKAGES = $nugetPackages
        DOTNET_CLI_HOME = $dotnetHome
        DOTNET_NOLOGO = '1'
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        CI = 'true'
    }
    $environment = [Collections.Generic.Dictionary[string, string]]::new(
        [StringComparer]::Ordinal)
    foreach ($entry in $allowed.GetEnumerator()) {
        $environment[[string]$entry.Key] = [string]$entry.Value
    }

    $null = Assert-ReplicationExecutionEnvironment -Environment $environment
    return $environment
}

function Invoke-CatalystProbeBoundedProcess {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$ArgumentList,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, string]]$Environment,
        [Parameter(Mandatory = $true)][ValidateRange(1, 2100)][int]$TimeoutSeconds,
        [Parameter(Mandatory = $true)][pscustomobject]$TaskDeadline,
        [ValidateRange(0, 600)][int]$ReserveSeconds = 0,
        [Parameter(Mandatory = $true)][string]$LogPath
    )

    $null = Assert-ReplicationExecutionEnvironment -Environment $Environment
    $effectiveTotalSeconds = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $TaskDeadline `
        -RequestedSeconds $TimeoutSeconds `
        -ReserveSeconds $ReserveSeconds `
        -Description ([IO.Path]::GetFileName($LogPath))
    $terminationSeconds = [Math]::Min(
        $script:CatalystProbeProcessTerminationSeconds,
        [Math]::Max(0, $effectiveTotalSeconds - 1))
    $effectiveProcessSeconds = $effectiveTotalSeconds - $terminationSeconds
    $operationStart = [Diagnostics.Stopwatch]::GetTimestamp()
    $operationDeadline = [pscustomobject]@{
        EntryTimestamp = $operationStart
        DeadlineTimestamp = [Math]::Min(
            [long]$TaskDeadline.DeadlineTimestamp -
            ([long]$ReserveSeconds * [long]$TaskDeadline.Frequency),
            $operationStart +
            ([long]$effectiveTotalSeconds * [long]$TaskDeadline.Frequency))
        Frequency = [long]$TaskDeadline.Frequency
        BudgetSeconds = $effectiveTotalSeconds
    }
    $logParent = Split-Path -Parent $LogPath
    if (-not (Test-Path -LiteralPath $logParent -PathType Container)) {
        New-Item -ItemType Directory -Path $logParent -Force | Out-Null
    }
    if (Test-Path -LiteralPath $LogPath) {
        throw "Catalyst probe refuses to replace an existing process log: $LogPath"
    }

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.Environment.Clear()
    foreach ($entry in $Environment.GetEnumerator()) {
        $startInfo.Environment[$entry.Key] = $entry.Value
    }
    foreach ($argument in $ArgumentList) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $started = [DateTimeOffset]::UtcNow
    $timedOut = $false
    $processStarted = $false
    $terminationWaitAttempted = $false
    $readCancellation = [Threading.CancellationTokenSource]::new()
    try {
        if (-not $process.Start()) {
            throw "Catalyst probe could not start fixed command '$FileName'."
        }
        $processStarted = $true
        $readMilliseconds = Get-CatalystProbeDeadlineRemainingMilliseconds `
            -Deadline $operationDeadline
        $readCancellation.CancelAfter($readMilliseconds)
        $stdoutTask = $process.StandardOutput.ReadToEndAsync($readCancellation.Token)
        $stderrTask = $process.StandardError.ReadToEndAsync($readCancellation.Token)
        $processWaitMilliseconds = [Math]::Max(0, [Math]::Min(
                $effectiveProcessSeconds * 1000,
                (Get-CatalystProbeDeadlineRemainingMilliseconds -Deadline $operationDeadline) -
                ($terminationSeconds * 1000)))
        if (-not $process.WaitForExit([int]$processWaitMilliseconds)) {
            $timedOut = $true
            $process.Kill($true)
            $terminationWaitAttempted = $true
            $terminationWaitMilliseconds =
            Get-CatalystProbeTerminationWaitMilliseconds `
                -Deadline $operationDeadline `
                -MaximumWaitSeconds $terminationSeconds
            if (-not $process.WaitForExit($terminationWaitMilliseconds)) {
                throw "Catalyst probe could not terminate timed-out command '$FileName'."
            }
        }
        $streams = [Threading.Tasks.Task]::WhenAll(
            [Threading.Tasks.Task[]]@($stdoutTask, $stderrTask))
        $drainMilliseconds = Get-CatalystProbeDeadlineRemainingMilliseconds `
            -Deadline $operationDeadline
        try {
            $drained = $streams.Wait([int]$drainMilliseconds)
        } catch [AggregateException] {
            if ($readCancellation.IsCancellationRequested) {
                throw "Catalyst probe redirected streams exceeded the command deadline for '$FileName'."
            }
            throw
        }
        if (-not $drained -or -not $stdoutTask.IsCompletedSuccessfully -or
            -not $stderrTask.IsCompletedSuccessfully) {
            throw "Catalyst probe redirected streams exceeded the command deadline for '$FileName'."
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        $exitCode = if ($timedOut) { 124 } else { $process.ExitCode }
        $completed = [DateTimeOffset]::UtcNow
        $combined = ConvertTo-CatalystProbeLogData -Text (
            "exitCode: $exitCode`ntimedOut: $timedOut`n" +
            "requestedTimeoutSeconds: $TimeoutSeconds`n" +
            "effectiveTimeoutSeconds: $effectiveTotalSeconds`n" +
            "effectiveProcessSeconds: $effectiveProcessSeconds`n" +
            "startedUtc: $($started.ToString('O'))`n" +
            "completedUtc: $($completed.ToString('O'))`n" +
            "stdout:`n$stdout`nstderr:`n$stderr")
        [IO.File]::WriteAllText(
            [IO.Path]::GetFullPath($LogPath),
            $combined,
            [Text.UTF8Encoding]::new($false))
        return [pscustomobject]@{
            ExitCode = $exitCode
            TimedOut = $timedOut
            RequestedTimeoutSeconds = $TimeoutSeconds
            EffectiveTimeoutSeconds = $effectiveTotalSeconds
            EffectiveProcessSeconds = $effectiveProcessSeconds
            StartedUtc = $started.ToString('O')
            CompletedUtc = $completed.ToString('O')
            Stdout = $stdout
            Stderr = $stderr
            LogSha256 = Get-CatalystProbeFileSha256 -Path $LogPath
        }
    } finally {
        $readCancellation.Cancel()
        if ($processStarted -and -not $process.HasExited) {
            $process.Kill($true)
            if (-not $terminationWaitAttempted) {
                $terminationWaitMilliseconds =
                Get-CatalystProbeTerminationWaitMilliseconds `
                    -Deadline $operationDeadline `
                    -MaximumWaitSeconds $terminationSeconds
                if ($terminationWaitMilliseconds -gt 0) {
                    [void]$process.WaitForExit($terminationWaitMilliseconds)
                }
            }
        }
        $process.Dispose()
        $readCancellation.Dispose()
    }
}

function Invoke-CatalystProbeGitText {
    param(
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][string]$LogName
    )

    $result = Invoke-CatalystProbeBoundedProcess `
        -FileName 'git' `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds 120 `
        -TaskDeadline $Context.ActiveDeadline `
        -ReserveSeconds $Context.ActiveReserveSeconds `
        -LogPath (Join-Path $Context.LogDirectory $LogName)
    if ($result.TimedOut -or $result.ExitCode -ne 0) {
        throw "Fixed local Git inspection failed; see $LogName."
    }
    return $result.Stdout.TrimEnd("`r", "`n")
}

function Assert-CatalystProbeTrustedTree {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    $null = Assert-TrustedTreeAttestation `
        -TrustedRoot $Context.TrustedRoot `
        -Attestation $Context.TrustedTreeAttestation `
        -ExpectedSourceVersion $Context.ExpectedSourceVersion `
        -Context 'Catalyst gesture probe boundary'
}

function Get-CatalystProbeRepositoryStatus {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    return Invoke-CatalystProbeGitText `
        -ArgumentList @('status', '--porcelain=v1', '--untracked-files=all') `
        -Context $Context `
        -LogName "git-status-$([guid]::NewGuid().ToString('N')).log"
}

function Get-CatalystProbeRepositoryStatusEntries {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    $status = Invoke-CatalystProbeGitText `
        -ArgumentList @('status', '--porcelain=v1', '-z', '--untracked-files=all') `
        -Context $Context `
        -LogName "git-status-z-$([guid]::NewGuid().ToString('N')).log"
    if ([string]::IsNullOrEmpty($status)) {
        return @()
    }

    $entries = [Collections.Generic.List[object]]::new()
    foreach ($record in $status.Split(
            [char]0,
            [StringSplitOptions]::RemoveEmptyEntries)) {
        if ($record.Length -lt 4 -or $record[2] -cne ' ') {
            throw 'Catalyst probe received malformed bounded Git status output.'
        }
        $entries.Add([pscustomobject]@{
                Status = $record.Substring(0, 2)
                Path = $record.Substring(3).Replace('\', '/')
            })
    }
    return @($entries)
}

function Get-CatalystProbeWorkingProductBlob {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][string]$LogName
    )

    return Invoke-CatalystProbeGitText `
        -ArgumentList @(
        'hash-object',
        "--path=$($script:CatalystProbeProductPath)",
        '--',
        $script:CatalystProbeProductPath) `
        -Context $Context `
        -LogName $LogName
}

function Get-CatalystProbeTrackedVerificationSideEffects {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)]
        [ValidateSet('setup', 'baseline', 'negative')][string]$State
    )

    Assert-CatalystProbeTrustedTree -Context $Context
    $expectedProductSha256 = if ($State -ceq 'negative') {
        $script:CatalystProbeNegativeFileSha256
    } else {
        $script:CatalystProbeBaselineFileSha256
    }
    $expectedProductBlob = if ($State -ceq 'negative') {
        $script:CatalystProbeNegativeBlob
    } else {
        $script:CatalystProbeBaselineBlob
    }
    if ((Get-CatalystProbeFileSha256 -Path $Context.ProductPath) -cne
        $expectedProductSha256) {
        throw "Catalyst probe $State product bytes changed outside the fixed contract."
    }
    if ((Get-CatalystProbeWorkingProductBlob `
                -Context $Context `
                -LogName "git-$State-protected-product-$([guid]::NewGuid().ToString('N')).log") -cne
        $expectedProductBlob) {
        throw "Catalyst probe $State product blob changed outside the fixed contract."
    }

    $productionComposite = $Context.PSObject.Properties['ProductionComposite'] -and
    [bool]$Context.ProductionComposite
    $fixtureExpected = -not $productionComposite -and $State -cne 'setup'
    if ($fixtureExpected) {
        if (-not (Test-Path -LiteralPath $Context.FixtureTargetPath -PathType Leaf) -or
            (Get-CatalystProbeFileSha256 -Path $Context.FixtureTargetPath) -cne
            $script:CatalystProbeFixtureSha256) {
            throw "Catalyst probe $State fixture bytes changed outside the fixed contract."
        }
    } elseif (Test-Path -LiteralPath $Context.FixtureTargetPath) {
        throw 'Catalyst probe setup unexpectedly contains the fixed fixture target.'
    }

    $restorePaths = [Collections.Generic.List[string]]::new()
    $fixtureSeen = $false
    $productSeen = $false
    foreach ($entry in @(Get-CatalystProbeRepositoryStatusEntries -Context $Context)) {
        $status = [string]$entry.Status
        $path = [string]$entry.Path
        if ($status[0] -cne ' ' -and $status[0] -cne '?') {
            throw "Catalyst probe refuses staged repository path: $path"
        }
        if ($status -ceq '??') {
            if ($fixtureExpected -and
                $path -ceq $script:CatalystProbeFixtureTargetRelativePath -and
                -not $fixtureSeen) {
                $fixtureSeen = $true
                continue
            }
            throw "Catalyst probe found an unexpected untracked repository path: $path"
        }
        if ($status -cnotin @(' M', ' D', ' T')) {
            throw "Catalyst probe refuses unexpected tracked status '$status' for path: $path"
        }
        if ($path -ceq $script:CatalystProbeProductPath) {
            if ($State -cne 'negative' -or $status -cne ' M' -or $productSeen) {
                throw 'Catalyst probe protected product has an unexpected repository state.'
            }
            $productSeen = $true
            continue
        }
        [void]$restorePaths.Add($path)
    }
    if ($fixtureExpected -ne $fixtureSeen -or
        (($State -ceq 'negative') -ne $productSeen)) {
        throw "Catalyst probe repository does not match its closed $State state."
    }
    return @($restorePaths)
}

function Assert-CatalystProbeRepositoryState {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)]
        [ValidateSet('setup', 'baseline', 'negative')][string]$State
    )

    $unexpected = @(
        Get-CatalystProbeTrackedVerificationSideEffects `
            -Context $Context `
            -State $State)
    if ($unexpected.Count -ne 0) {
        throw "Catalyst probe $State scope contains tracked verification output: $($unexpected[0])"
    }
}

function Restore-CatalystProbeTrackedVerificationSideEffects {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)]
        [ValidateSet('setup', 'baseline', 'negative')][string]$State
    )

    $restorePaths = @(
        Get-CatalystProbeTrackedVerificationSideEffects `
            -Context $Context `
            -State $State)
    if ($restorePaths.Count -gt 0) {
        $restore = Invoke-CatalystProbeBoundedProcess `
            -FileName 'git' `
            -ArgumentList (@(
                'restore', '--source', $script:CatalystProbeBaselineCommit,
                '--worktree', '--') + $restorePaths) `
            -WorkingDirectory $Context.RepositoryRoot `
            -Environment $Context.RuntimeEnvironment `
            -TimeoutSeconds 120 `
            -TaskDeadline $Context.ActiveDeadline `
            -ReserveSeconds $Context.ActiveReserveSeconds `
            -LogPath (Join-Path $Context.LogDirectory (
                "git-restore-tracked-output-$State-$([guid]::NewGuid().ToString('N')).log"))
        if ($restore.TimedOut) {
            throw 'Catalyst probe tracked verification-output restoration timed out.'
        }
        if ($restore.ExitCode -ne 0) {
            throw 'Catalyst probe tracked verification-output restoration failed.'
        }
    }
    Assert-CatalystProbeRepositoryState -Context $Context -State $State
}

function Get-CatalystProbeContainerResultPath {
    $userProfile = [Environment]::GetFolderPath(
        [Environment+SpecialFolder]::UserProfile)
    if ([string]::IsNullOrWhiteSpace($userProfile)) {
        throw 'Catalyst probe cannot resolve its fresh Azure user profile.'
    }
    return Join-Path $userProfile (
        'Library/Containers/com.microsoft.maui.controls.devicetests/' +
        'Data/Documents/.config/TestResults.xUnit.xml')
}

function Initialize-CatalystGestureProbeContext {
    param(
        [Parameter(Mandatory = $true)][string]$ExpectedSourceVersion,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][string]$TrustedTreeAttestation,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [Parameter(Mandatory = $true)][pscustomobject]$TaskDeadline
    )

    Assert-CatalystProbeClosedAzureIdentity `
        -ExpectedSourceVersion $ExpectedSourceVersion `
        -RepositoryRoot $RepositoryRoot `
        -TrustedRoot $TrustedRoot `
        -TrustedTreeAttestation $TrustedTreeAttestation `
        -OutputDirectory $OutputDirectory
    $tfBuild = [Environment]::GetEnvironmentVariable('TF_BUILD')
    if ([string]::IsNullOrWhiteSpace($tfBuild) -or
        $tfBuild.ToLowerInvariant() -cne 'true') {
        throw 'The Catalyst gesture probe may execute only inside its fresh Azure job.'
    }
    foreach ($binding in @(
            @{ Environment = 'BUILD_SOURCESDIRECTORY'; Value = $RepositoryRoot },
            @{ Environment = 'CATALYST_PROBE_TRUSTED_ROOT'; Value = $TrustedRoot },
            @{ Environment = 'CATALYST_PROBE_TRUSTED_ATTESTATION'; Value =
                $TrustedTreeAttestation },
            @{ Environment = 'CATALYST_PROBE_OUTPUT_ROOT'; Value = $OutputDirectory })) {
        $bound = [Environment]::GetEnvironmentVariable($binding.Environment)
        if ([string]::IsNullOrWhiteSpace($bound) -or
            [IO.Path]::GetFullPath($bound) -cne [IO.Path]::GetFullPath($binding.Value)) {
            throw "Catalyst probe path does not match $($binding.Environment)."
        }
    }
    if ([Environment]::GetEnvironmentVariable('BUILD_SOURCEVERSION') -cne
        $ExpectedSourceVersion) {
        throw 'Catalyst probe source version is not the validated pipeline source.'
    }

    $repositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $trustedRoot = (Resolve-Path -LiteralPath $TrustedRoot).Path
    $trustedTreeAttestation = (Resolve-Path -LiteralPath $TrustedTreeAttestation).Path
    $pipelineWorkspace = [Environment]::GetEnvironmentVariable('PIPELINE_WORKSPACE')
    if ([string]::IsNullOrWhiteSpace($pipelineWorkspace)) {
        throw 'Catalyst probe requires the fresh pipeline workspace boundary.'
    }
    $agentTempDirectory =
    [Environment]::GetEnvironmentVariable('AGENT_TEMPDIRECTORY')
    $requestedRuntimeRoot =
    [Environment]::GetEnvironmentVariable('CATALYST_PROBE_RUNTIME_ROOT')
    if ([string]::IsNullOrWhiteSpace($agentTempDirectory) -or
        [string]::IsNullOrWhiteSpace($requestedRuntimeRoot)) {
        throw 'Catalyst probe requires its fixed private Agent.TempDirectory cache root.'
    }
    $jobDeadlineUtc =
    [Environment]::GetEnvironmentVariable('CATALYST_GESTURE_JOB_DEADLINE_UTC')
    $artifactTailText =
    [Environment]::GetEnvironmentVariable('CATALYST_GESTURE_ARTIFACT_TAIL_SECONDS')
    $artifactTailSeconds = 0
    $parsedJobDeadline = [DateTimeOffset]::MinValue
    if ([string]::IsNullOrWhiteSpace($jobDeadlineUtc) -or
        -not [DateTimeOffset]::TryParse(
            $jobDeadlineUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind,
            [ref]$parsedJobDeadline) -or
        $parsedJobDeadline.ToUniversalTime() -gt
        [DateTimeOffset]::UtcNow.AddSeconds(5400) -or
        -not [int]::TryParse($artifactTailText, [ref]$artifactTailSeconds) -or
        $artifactTailSeconds -ne $script:CatalystProbeArtifactTailSeconds) {
        throw 'Catalyst probe requires its fixed job deadline and evidence tail.'
    }
    $outputDirectory = Assert-CatalystProbePathUnderRoot `
        -Path $OutputDirectory `
        -Root $pipelineWorkspace `
        -Description 'Catalyst probe output'
    $repositoryPrefix = $repositoryRoot.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
    if ($outputDirectory -ceq $repositoryRoot -or
        $outputDirectory.StartsWith($repositoryPrefix, [StringComparison]::Ordinal) -or
        $trustedRoot -ceq $repositoryRoot -or
        $trustedRoot.StartsWith($repositoryPrefix, [StringComparison]::Ordinal)) {
        throw 'Catalyst probe trusted and output roots must stay outside the product checkout.'
    }
    if (-not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }
    $outputItem = Get-Item -LiteralPath $outputDirectory -Force
    if (-not $outputItem.PSIsContainer -or
        $outputItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Catalyst probe output must be a regular directory.'
    }
    $runtimeRoot = Resolve-CatalystProbePrivateRuntimeRoot `
        -RuntimeRoot $requestedRuntimeRoot `
        -AgentTempDirectory $agentTempDirectory `
        -OutputDirectory $outputDirectory `
        -RepositoryRoot $repositoryRoot `
        -TrustedRoot $trustedRoot
    $resultPath = Join-Path $outputDirectory 'catalyst-gesture-probe.json'
    if (Test-Path -LiteralPath $resultPath) {
        throw 'Catalyst probe refuses to replace an existing result document.'
    }

    $logDirectory = Join-Path $outputDirectory 'logs'
    if (Test-Path -LiteralPath $logDirectory) {
        throw "Catalyst probe refuses to reuse owned directory: $logDirectory"
    }
    New-Item -ItemType Directory -Path $logDirectory | Out-Null

    $trustedFixturePath = Join-Path $trustedRoot $script:CatalystProbeFixtureRelativePath
    $fixtureTargetPath = Join-Path $repositoryRoot $script:CatalystProbeFixtureTargetRelativePath
    $productPath = Join-Path $repositoryRoot $script:CatalystProbeProductPath
    $containerResultPath = Get-CatalystProbeContainerResultPath
    if (Test-Path -LiteralPath $containerResultPath) {
        throw ('Catalyst probe requires an initially absent container result and ' +
            'will not delete a result it did not create in this fresh Azure job.')
    }
    $context = [pscustomobject]@{
        ExpectedSourceVersion = $ExpectedSourceVersion
        RepositoryRoot = $repositoryRoot
        TrustedRoot = $trustedRoot
        TrustedTreeAttestation = $trustedTreeAttestation
        OutputDirectory = $outputDirectory
        ResultPath = $resultPath
        LogDirectory = $logDirectory
        RuntimeRoot = $runtimeRoot
        RuntimeEnvironment = $null
        TaskDeadline = $TaskDeadline
        ActiveDeadline = $TaskDeadline
        ActiveReserveSeconds =
        $script:CatalystProbeCleanupBudgetSeconds +
        $script:CatalystProbeSummaryBudgetSeconds
        TrustedFixturePath = $trustedFixturePath
        FixtureTargetPath = $fixtureTargetPath
        ProductPath = $productPath
        ContainerResultPath = $containerResultPath
        JobDeadlineUtc = $parsedJobDeadline.ToUniversalTime().ToString('O')
        ArtifactTailSeconds = $artifactTailSeconds
        InitialRepositoryStatus = $null
        RepositoryState = 'setup'
        ProductionComposite = $true
        OwnedIosSimulator = $null
    }
    $context.RuntimeEnvironment =
    Get-CatalystProbeRuntimeEnvironment -RuntimeRoot $runtimeRoot

    Assert-CatalystProbeTrustedTree -Context $context
    if ((Get-CatalystProbeFileSha256 -Path $trustedFixturePath) -cne
        $script:CatalystProbeFixtureSha256) {
        throw 'The attested Catalyst gesture fixture does not match its fixed digest.'
    }
    if (Test-Path -LiteralPath $fixtureTargetPath) {
        throw 'Catalyst probe refuses to replace an existing fixture target.'
    }
    $head = Invoke-CatalystProbeGitText `
        -ArgumentList @('rev-parse', 'HEAD') `
        -Context $context `
        -LogName 'git-head.log'
    if ($head -cne $script:CatalystProbeBaselineCommit) {
        throw 'Catalyst probe repository is not detached at the immutable baseline.'
    }
    foreach ($object in @(
            "$($script:CatalystProbeBaselineCommit)^{commit}",
            "$($script:CatalystProbeNegativeCommit)^{commit}",
            $script:CatalystProbeBaselineBlob,
            $script:CatalystProbeNegativeBlob)) {
        $actualType = Invoke-CatalystProbeGitText `
            -ArgumentList @('cat-file', '-t', $object) `
            -Context $context `
            -LogName "git-object-$([guid]::NewGuid().ToString('N')).log"
        if ($actualType -cnotin @('commit', 'blob')) {
            throw 'Catalyst probe is missing a fixed immutable Git object.'
        }
    }
    $baselineBlob = Invoke-CatalystProbeGitText `
        -ArgumentList @('rev-parse', "$($script:CatalystProbeBaselineCommit):$($script:CatalystProbeProductPath)") `
        -Context $context `
        -LogName 'git-baseline-blob.log'
    $negativeBlob = Invoke-CatalystProbeGitText `
        -ArgumentList @('rev-parse', "$($script:CatalystProbeNegativeCommit):$($script:CatalystProbeProductPath)") `
        -Context $context `
        -LogName 'git-negative-blob.log'
    if ($baselineBlob -cne $script:CatalystProbeBaselineBlob -or
        $negativeBlob -cne $script:CatalystProbeNegativeBlob) {
        throw 'Catalyst probe product blobs do not match the immutable contract.'
    }
    $pathDiff = Invoke-CatalystProbeGitText `
        -ArgumentList @(
        'diff', '--name-status', $script:CatalystProbeBaselineCommit,
        $script:CatalystProbeNegativeCommit, '--', $script:CatalystProbeProductPath) `
        -Context $context `
        -LogName 'git-product-name-status.log'
    if ($pathDiff -cne "M`t$($script:CatalystProbeProductPath)") {
        throw 'Catalyst probe requires one modification-only product-path diff.'
    }
    if ((Get-CatalystProbeFileSha256 -Path $productPath) -cne
        $script:CatalystProbeBaselineFileSha256) {
        throw 'Catalyst probe baseline product file does not match its fixed digest.'
    }
    if ((Get-CatalystProbeWorkingProductBlob `
                -Context $context `
                -LogName 'git-baseline-working-blob.log') -cne
        $script:CatalystProbeBaselineBlob) {
        throw 'Catalyst probe baseline product working blob is not the fixed preimage.'
    }
    $observedStatus = Get-CatalystProbeRepositoryStatus -Context $context
    if (-not [string]::IsNullOrEmpty($observedStatus)) {
        Restore-CatalystProbeTrackedVerificationSideEffects `
            -Context $context `
            -State setup
    }
    $context.InitialRepositoryStatus = ''
    return $context
}

function New-CatalystProbeFixedPatch {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    $patchPath = Join-Path $Context.OutputDirectory 'known-negative-product.patch'
    if (Test-Path -LiteralPath $patchPath) {
        throw 'Catalyst probe refuses to replace its fixed patch.'
    }
    $patch = Invoke-CatalystProbeGitText `
        -ArgumentList @(
        'diff', '--binary', '--full-index', '--no-color',
        $script:CatalystProbeBaselineCommit,
        $script:CatalystProbeNegativeCommit,
        '--', $script:CatalystProbeProductPath) `
        -Context $Context `
        -LogName 'git-product-patch.log'
    [IO.File]::WriteAllText(
        $patchPath,
        "$patch`n",
        [Text.UTF8Encoding]::new($false))
    if ((Get-CatalystProbeFileSha256 -Path $patchPath) -cne
        $script:CatalystProbePatchSha256) {
        throw 'Catalyst probe fixed product patch has an unexpected digest.'
    }
    return $patchPath
}

function Copy-CatalystProbeProductionValidationPatch {
    param(
        [Parameter(Mandatory = $true)][string]$PatchPath,
        [Parameter(Mandatory = $true)][string]$EvidenceArtifactRoot
    )

    $evidenceRoot = [IO.Path]::GetFullPath($EvidenceArtifactRoot)
    if ([IO.Path]::GetFileName($evidenceRoot) -cne 'production-composite') {
        throw 'Catalyst probe production validation requires its fixed evidence root.'
    }
    $outputRoot = Split-Path -Parent $evidenceRoot
    $expectedSource = Join-Path $outputRoot 'known-negative-product.patch'
    if ([IO.Path]::GetFullPath($PatchPath) -cne
        [IO.Path]::GetFullPath($expectedSource)) {
        throw 'Catalyst probe production validation requires its canonical fixed patch.'
    }
    $rootItem = Get-Item -LiteralPath $evidenceRoot -Force -ErrorAction Stop
    if (-not $rootItem.PSIsContainer -or
        $rootItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Catalyst probe production validation requires a regular evidence root.'
    }
    if ((Get-CatalystProbeFileSha256 -Path $PatchPath) -cne
        $script:CatalystProbePatchSha256) {
        throw 'Catalyst probe canonical patch does not match its immutable digest.'
    }

    $targetPath = Join-Path $evidenceRoot 'fix.patch'
    if (Test-Path -LiteralPath $targetPath) {
        throw 'Catalyst probe refuses to replace its production validation patch.'
    }
    [IO.File]::Copy(
        [IO.Path]::GetFullPath($PatchPath),
        [IO.Path]::GetFullPath($targetPath),
        $false)
    try {
        if ((Get-CatalystProbeFileSha256 -Path $targetPath) -cne
            $script:CatalystProbePatchSha256 -or
            (Get-CatalystProbeFileSha256 -Path $PatchPath) -cne
            $script:CatalystProbePatchSha256) {
            throw 'Catalyst probe staged patch does not match its immutable digest.'
        }
    } catch {
        if (Test-Path -LiteralPath $targetPath -PathType Leaf) {
            Remove-Item -LiteralPath $targetPath -Force
        }
        throw
    }
    return $targetPath
}

function Copy-CatalystProbeFixture {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    Assert-CatalystProbeTrustedTree -Context $Context
    Assert-CatalystProbeRepositoryState -Context $Context -State setup
    if (Test-Path -LiteralPath $Context.FixtureTargetPath) {
        throw 'Catalyst probe refuses to replace an existing fixture target.'
    }
    Copy-Item -LiteralPath $Context.TrustedFixturePath -Destination $Context.FixtureTargetPath
    if ((Get-CatalystProbeFileSha256 -Path $Context.FixtureTargetPath) -cne
        $script:CatalystProbeFixtureSha256) {
        throw 'Copied Catalyst probe fixture failed its immutable digest check.'
    }
    $Context.RepositoryState = 'baseline'
    Assert-CatalystProbeRepositoryState -Context $Context -State baseline
}

function Invoke-CatalystProbeTrustedRestore {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    $phaseContext = $Context.PSObject.Copy()
    $phaseContext.ActiveDeadline =
    New-CatalystProbePrewarmDeadline -TaskDeadline $Context.TaskDeadline
    $phaseContext.ActiveReserveSeconds =
    $script:CatalystProbePrewarmCleanupReserveSeconds
    # The preguard is deliberately outside the admitted phase's finally. Rejected
    # input belongs to the caller and must never be "cleaned" as build output.
    Assert-CatalystProbeRepositoryState -Context $phaseContext -State setup
    $plan = Get-ReplicationAppleCompanionPrewarmPlan `
        -RepositoryRoot $Context.RepositoryRoot `
        -TrustedRoot $Context.TrustedRoot `
        -PackagesPath $Context.RuntimeEnvironment['NUGET_PACKAGES']
    $assets = $null
    $restoreIdentity = $null
    $buildIdentities = [Collections.Generic.List[object]]::new()
    $validationCount = 0
    $cleanupCompleted = $false
    try {
        foreach ($command in @($plan.Commands)) {
            Assert-CatalystProbeTrustedTree -Context $phaseContext
            $result = Invoke-CatalystProbeBoundedProcess `
                -FileName 'dotnet' `
                -ArgumentList $command.Arguments `
                -WorkingDirectory $phaseContext.RepositoryRoot `
                -Environment $phaseContext.RuntimeEnvironment `
                -TimeoutSeconds $command.TimeoutSeconds `
                -TaskDeadline $phaseContext.ActiveDeadline `
                -ReserveSeconds $phaseContext.ActiveReserveSeconds `
                -LogPath (Join-Path $phaseContext.LogDirectory $command.LogName)
            Assert-CatalystProbeTrustedTree -Context $phaseContext
            $null = @(
                Get-CatalystProbeTrackedVerificationSideEffects `
                    -Context $phaseContext `
                    -State setup)
            if ($result.TimedOut -or $result.ExitCode -ne 0) {
                throw "Catalyst trusted dual-Apple prewarm failed; see $($command.LogName)."
            }
            if ($command.Kind -ceq 'restore') {
                $assets = Assert-ReplicationAppleCompanionAssets `
                    -RepositoryRoot $phaseContext.RepositoryRoot
                $validationCount++
                $restoreIdentity = [ordered]@{
                    name = [string]$command.Name
                    logName = [string]$command.LogName
                    logSha256 = [string]$result.LogSha256
                }
            } else {
                if ($null -eq $assets) {
                    throw 'Catalyst trusted dual-Apple prewarm build preceded asset validation.'
                }
                $postBuildAssets = Assert-ReplicationAppleCompanionAssets `
                    -RepositoryRoot $phaseContext.RepositoryRoot `
                    -ExpectedSha256 $assets.AssetsSha256
                $validationCount++
                $buildIdentities.Add([ordered]@{
                        targetFramework = [string]$command.TargetFramework
                        runtimeIdentifier = [string]$command.RuntimeIdentifier
                        configuration = 'Debug'
                        noRestore = $true
                        logName = [string]$command.LogName
                        logSha256 = [string]$result.LogSha256
                        assetsSha256 = [string]$postBuildAssets.AssetsSha256
                    })
            }
        }
    } finally {
        $phaseContext.ActiveReserveSeconds = 0
        Restore-CatalystProbeTrackedVerificationSideEffects `
            -Context $phaseContext `
            -State setup
        $cleanupCompleted = $true
    }
    if (-not $cleanupCompleted -or $null -eq $restoreIdentity -or
        $buildIdentities.Count -ne 2 -or $validationCount -ne 3) {
        throw 'Catalyst trusted dual-Apple prewarm did not complete its closed proof.'
    }
    return [pscustomobject][ordered]@{
        ready = $true
        phaseBudgetSeconds = [int]$phaseContext.ActiveDeadline.BudgetSeconds
        cleanupCompleted = $cleanupCompleted
        restore = $restoreIdentity
        assets = [ordered]@{
            sha256 = [string]$assets.AssetsSha256
            targetPairs = @($assets.TargetPairs)
            originalTargetFrameworks = @($assets.OriginalTargetFrameworks)
            frameworks = @($assets.Frameworks)
            validationCount = $validationCount
            unchangedThroughBuilds = $true
        }
        builds = @($buildIdentities)
    }
}

function Assert-CatalystProbeFixedPatchPolicy {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][string]$PatchPath
    )

    Assert-CatalystProbeTrustedTree -Context $Context
    $null = Invoke-WithoutReplicationSecrets {
        $null = Assert-ReplicationFixSources `
            -RepositoryRoot $Context.RepositoryRoot `
            -Paths @($script:CatalystProbeProductPath) `
            -PatchPath $PatchPath
    }
    Assert-CatalystProbeTrustedTree -Context $Context
    if ((Get-CatalystProbeFileSha256 -Path $Context.ProductPath) -cne
        $script:CatalystProbeBaselineFileSha256) {
        throw 'Complete-postimage policy did not restore the fixed baseline product.'
    }
    if (-not [string]::IsNullOrEmpty(
            (Get-CatalystProbeRepositoryStatus -Context $Context))) {
        throw 'Complete-postimage policy did not restore the clean trusted checkout.'
    }
}

function Invoke-CatalystProbeCleanupCommand {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $result = Invoke-CatalystProbeBoundedProcess `
        -FileName $FileName `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds 30 `
        -TaskDeadline $Context.ActiveDeadline `
        -ReserveSeconds $Context.ActiveReserveSeconds `
        -LogPath (Join-Path $Context.LogDirectory (
            "$Name-$([guid]::NewGuid().ToString('N')).log"))
    if ($result.TimedOut -or $result.ExitCode -ne 0) {
        throw "Catalyst probe owned-app cleanup command failed: $Name."
    }
    return $result
}

function Stop-CatalystProbeOwnedApplication {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    $appRoot = Join-Path $Context.RepositoryRoot (
        'artifacts/bin/Controls.DeviceTests/Debug/' +
        "$($script:CatalystProbeTargetFramework)/$($script:CatalystProbeRuntimeIdentifier)")
    if (-not (Test-Path -LiteralPath $appRoot -PathType Container)) {
        return 0
    }
    $apps = @(Get-ChildItem -LiteralPath $appRoot -Directory -Filter '*.app')
    if ($apps.Count -gt 1) {
        throw 'Catalyst probe cleanup found more than one owned app bundle.'
    }
    if ($apps.Count -eq 0) {
        return 0
    }
    $app = $apps[0]
    if ($app.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Catalyst probe cleanup refuses a linked app bundle.'
    }
    $infoPlist = Join-Path $app.FullName 'Contents/Info.plist'
    $info = Get-Item -LiteralPath $infoPlist -Force -ErrorAction Stop
    if ($info.PSIsContainer -or $info.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Catalyst probe cleanup Info.plist is missing or linked.'
    }
    $null = Invoke-CatalystProbeCleanupCommand -Context $Context `
        -FileName '/usr/bin/codesign' `
        -ArgumentList @('--verify', '--deep', '--strict', $app.FullName) `
        -Name 'cleanup-signature'
    $signed = Invoke-CatalystProbeCleanupCommand -Context $Context `
        -FileName '/usr/bin/codesign' `
        -ArgumentList @('-d', '--entitlements', ':-', $app.FullName) `
        -Name 'cleanup-entitlements'
    $signedText = "$($signed.Stdout)`n$($signed.Stderr)"
    $plistStart = $signedText.IndexOf('<?xml', [StringComparison]::Ordinal)
    if ($plistStart -lt 0) {
        $plistStart = $signedText.IndexOf('<plist', [StringComparison]::Ordinal)
    }
    $plistEnd = $signedText.LastIndexOf('</plist>', [StringComparison]::Ordinal)
    if ($plistStart -lt 0 -or $plistEnd -lt $plistStart) {
        throw 'Catalyst probe cleanup could not read signed app entitlements.'
    }
    $entitlements = Read-ReplicationApplePlist `
        -Content $signedText.Substring($plistStart, $plistEnd + 8 - $plistStart) `
        -Description 'Catalyst cleanup signed entitlements'
    $null = Assert-ReplicationMacCatalystEntitlementDocument `
        -Document $entitlements -Description 'Catalyst cleanup signed entitlements'

    $executableResult = Invoke-CatalystProbeCleanupCommand -Context $Context `
        -FileName '/usr/libexec/PlistBuddy' `
        -ArgumentList @('-c', 'Print :CFBundleExecutable', $infoPlist) `
        -Name 'cleanup-executable'
    $bundleResult = Invoke-CatalystProbeCleanupCommand -Context $Context `
        -FileName '/usr/libexec/PlistBuddy' `
        -ArgumentList @('-c', 'Print :CFBundleIdentifier', $infoPlist) `
        -Name 'cleanup-bundle'
    $executableName = $executableResult.Stdout.Trim()
    $bundleIdentifier = $bundleResult.Stdout.Trim()
    if ($executableName -cne 'Microsoft.Maui.Controls.DeviceTests' -or
        $bundleIdentifier -cne 'com.microsoft.maui.controls.devicetests') {
        throw 'Catalyst probe cleanup found an unexpected app identity.'
    }
    $executablePath = Resolve-ReplicationAppleExistingPath `
        -Path (Join-Path $app.FullName "Contents/MacOS/$executableName")
    $executable = Get-Item -LiteralPath $executablePath -Force -ErrorAction Stop
    if ($executable.PSIsContainer -or
        $executable.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Catalyst probe cleanup executable is missing or linked.'
    }
    $stopped = 0
    foreach ($process in @(Get-Process -Name $executableName -ErrorAction SilentlyContinue)) {
        try {
            $null = Get-CatalystProbeProcessTimeoutSeconds `
                -Deadline $Context.ActiveDeadline -RequestedSeconds 10 `
                -ReserveSeconds $Context.ActiveReserveSeconds `
                -Description 'owned app termination'
            if (-not $process.HasExited -and
                (Resolve-ReplicationAppleExistingPath -Path $process.Path) -ceq $executablePath) {
                $creationTime = $process.StartTime.ToUniversalTime()
                if ($process.HasExited -or
                    $process.StartTime.ToUniversalTime() -ne $creationTime) {
                    throw 'Catalyst probe owned process changed before termination.'
                }
                $process.Kill($true)
                $waitMilliseconds = Get-CatalystProbeTerminationWaitMilliseconds `
                    -Deadline $Context.ActiveDeadline `
                    -ReserveSeconds $Context.ActiveReserveSeconds `
                    -MaximumWaitSeconds 10
                if (-not $process.WaitForExit($waitMilliseconds)) {
                    throw 'Catalyst probe owned app did not exit within its cleanup deadline.'
                }
                $stopped++
            }
        } finally {
            $process.Dispose()
        }
    }
    return $stopped
}

function Read-CatalystProbeFailureDetails {
    param(
        [Parameter(Mandatory = $true)][string]$StrictEvidencePath,
        [Parameter(Mandatory = $true)][object]$Document
    )

    $details = @{}
    foreach ($source in @($Document.resultFiles)) {
        $path = Join-Path (Split-Path -Parent $StrictEvidencePath) ([string]$source.name)
        $settings = [Xml.XmlReaderSettings]::new()
        $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $settings.MaxCharactersInDocument = 10MB
        $reader = $null
        try {
            $reader = [Xml.XmlReader]::Create($path, $settings)
            $xml = [Xml.XmlDocument]::new()
            $xml.XmlResolver = $null
            $xml.Load($reader)
        } finally {
            if ($reader) { $reader.Dispose() }
        }
        foreach ($test in @($xml.SelectNodes('/assemblies/assembly//test'))) {
            $failure = $test.SelectSingleNode('./failure')
            if ($failure) {
                $message = $failure.SelectSingleNode('./message')
                $stack = $failure.SelectSingleNode('./stack-trace')
                $details[[string]$test.GetAttribute('method')] = [pscustomobject]@{
                    ExceptionType = [string]$failure.GetAttribute('exception-type')
                    Message = if ($message) { [string]$message.InnerText } else { '' }
                    Stack = if ($stack) { [string]$stack.InnerText } else { '' }
                }
            }
        }
    }
    return $details
}

function Assert-CatalystProbeCycleEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][string]$StrictEvidencePath
    )

    $evidence = Read-ReplicationRegressionRunEvidence `
        -Path $StrictEvidencePath `
        -ExpectedPlatform 'catalyst' `
        -ExpectedProject 'Controls' `
        -ExpectedCategory 'Gesture' `
        -ExpectedClass $script:CatalystProbeClass
    $document = $evidence.Document
    $records = @($document.records)
    if ([int]$document.total -ne 2 -or $records.Count -ne 2 -or
        [int]$document.errors -ne 0 -or [int]$document.skipped -ne 0) {
        throw 'Catalyst probe evidence must contain exactly two completed, non-skipped facts.'
    }
    $methods = @($records | ForEach-Object { [string]$_.method } |
            Sort-Object -CaseSensitive)
    if (($methods -join "`n") -cne
        ($script:CatalystProbeMethods -join "`n") -or
        @($methods | Select-Object -Unique).Count -ne 2) {
        throw 'Catalyst probe evidence contains the wrong test identities.'
    }

    if ($Kind -ceq 'baseline') {
        if ([int]$document.passed -ne 2 -or [int]$document.failed -ne 0 -or
            @($records | Where-Object { $_.outcome -cne 'Pass' }).Count -ne 0) {
            throw 'Catalyst baseline did not pass both fixed gesture facts.'
        }
    } elseif ($Kind -ceq 'negative') {
        if ([int]$document.passed -ne 0 -or [int]$document.failed -ne 2 -or
            @($records | Where-Object { $_.outcome -cne 'Fail' }).Count -ne 0) {
            throw 'Catalyst known-negative did not fail both fixed gesture facts.'
        }
        $failureDetails = Read-CatalystProbeFailureDetails `
            -StrictEvidencePath $StrictEvidencePath `
            -Document $document
        foreach ($method in $script:CatalystProbeMethods) {
            if (-not $failureDetails.ContainsKey($method)) {
                throw 'Catalyst known-negative lacks expected assertion failure details.'
            }
            $failure = $failureDetails[$method]
            if ($failure.ExceptionType -cnotmatch '(?i)(SingleException|XunitException)' -or
                $failure.Message -cnotmatch '(?is)Assert\.Single.*collection was empty' -or
                -not $failure.Stack.Contains(
                    $script:CatalystProbeClass,
                    [StringComparison]::Ordinal) -or
                -not $failure.Stack.Contains(
                    $method,
                    [StringComparison]::Ordinal)) {
                throw 'Catalyst known-negative failed outside the expected native tap mapping assertion.'
            }
        }
    } else {
        throw "Unknown Catalyst probe cycle kind '$Kind'."
    }

    return [pscustomobject]@{
        StrictEvidenceSha256 = [string]$evidence.Digest
        ResultFiles = @($document.resultFiles | ForEach-Object {
                [ordered]@{
                    name = [string]$_.name
                    sha256 = [string]$_.sha256
                }
            })
        Identities = @($records | Sort-Object method -CaseSensitive |
                ForEach-Object {
                    [ordered]@{
                        type = [string]$_.type
                        method = [string]$_.method
                        displayName = [string]$_.displayName
                        outcome = [string]$_.outcome
                        failureSignature = [string]$_.failureSignature
                    }
                })
        Total = [int]$document.total
        Passed = [int]$document.passed
        Failed = [int]$document.failed
        Skipped = [int]$document.skipped
        Errors = [int]$document.errors
    }
}

function Assert-CatalystProbeOwnedContainerResult {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][pscustomobject]$Cycle
    )

    $containerDigest =
    Get-CatalystProbeFileSha256 -Path $Context.ContainerResultPath
    $retainedDigests = @($Cycle.resultFiles | ForEach-Object {
            [string]$_.sha256
        })
    if ($retainedDigests.Count -ne 1 -or
        $retainedDigests[0] -cne $containerDigest) {
        throw 'Catalyst probe container result is not the exact retained XML from this cycle.'
    }
    return $containerDigest
}

function Invoke-CatalystProbeCycle {
    param(
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][pscustomobject]$Context
    )

    $cycleRoot = Join-Path $Context.OutputDirectory $Kind
    if (Test-Path -LiteralPath $cycleRoot) {
        throw "Catalyst probe refuses to reuse the $Kind cycle directory."
    }
    $strictEvidencePath = Join-Path $cycleRoot 'strict-test-evidence.json'
    $verificationScriptPath = Join-Path $Context.TrustedRoot (
        'scripts/shared/Invoke-ReplicationTestVerification.ps1')
    $verifierPath = Join-Path $Context.TrustedRoot (
        'skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1')
    # The generic verifier binder requires a positive issue number, but the
    # RegressionEvidence branch never reads issue content. Keep a fixed sentinel
    # rather than accepting the pipeline's report-only IssueNumber=0 as input.
    $arguments = @(
        '-IssueNumber', '1',
        '-BaseSha', $script:CatalystProbeBaselineCommit,
        '-Platform', 'catalyst',
        '-TestType', 'DeviceTest',
        '-TestFilter', 'Category=Gesture',
        '-TestProject', 'Controls',
        '-TestProjectPath', $script:CatalystProbeProjectPath,
        '-TestClass', $script:CatalystProbeClass,
        '-ExpectedFailureSignature', 'regression-evidence',
        '-VerifierPath', $verifierPath,
        '-OutputDirectory', $cycleRoot,
        '-RunCount', '1',
        '-RegressionEvidence'
    )
    $command = Get-ReplicationAppleIsolatedCommand `
        -Platform 'catalyst' `
        -TrustedRoot $Context.TrustedRoot `
        -ScriptPath $verificationScriptPath `
        -Arguments $arguments `
        -Environment $Context.RuntimeEnvironment
    $expectedState = if ($Kind -ceq 'baseline') { 'baseline' } else { 'negative' }
    if ([string]$Context.RepositoryState -cne $expectedState) {
        throw "Catalyst probe $Kind cycle was requested outside its closed repository state."
    }
    Assert-CatalystProbeTrustedTree -Context $Context
    Assert-CatalystProbeRepositoryState -Context $Context -State $expectedState
    $cycleSeconds = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $Context.ActiveDeadline `
        -RequestedSeconds $script:CatalystProbeCycleBudgetSeconds `
        -ReserveSeconds $Context.ActiveReserveSeconds `
        -Description "$Kind complete native cycle"
    $cycleStart = [Diagnostics.Stopwatch]::GetTimestamp()
    $cycleContext = $Context.PSObject.Copy()
    $cycleContext.ActiveDeadline = [pscustomobject]@{
        EntryTimestamp = $cycleStart
        DeadlineTimestamp = [Math]::Min(
            [long]$Context.ActiveDeadline.DeadlineTimestamp -
            ([long]$Context.ActiveReserveSeconds * [long]$Context.ActiveDeadline.Frequency),
            $cycleStart + ([long]$cycleSeconds * [long]$Context.ActiveDeadline.Frequency))
        Frequency = [long]$Context.ActiveDeadline.Frequency
        BudgetSeconds = $cycleSeconds
    }
    $cycleContext.ActiveReserveSeconds = $script:CatalystProbeCycleEvidenceBudgetSeconds
    $cleanupStoppedCount = 0
    try {
        $processResult = Invoke-CatalystProbeBoundedProcess `
            -FileName $command.FilePath `
            -ArgumentList $command.Arguments `
            -WorkingDirectory $Context.RepositoryRoot `
            -Environment $command.Environment `
            -TimeoutSeconds $script:CatalystProbeVerifierBudgetSeconds `
            -TaskDeadline $cycleContext.ActiveDeadline `
            -ReserveSeconds $cycleContext.ActiveReserveSeconds `
            -LogPath (Join-Path $Context.LogDirectory "$Kind-verifier.log")
    } finally {
        $cycleContext.ActiveReserveSeconds = 10
        $cycleCleanupErrors = [Collections.Generic.List[string]]::new()
        try {
            $cleanupStoppedCount =
            Stop-CatalystProbeOwnedApplication -Context $cycleContext
        } catch {
            $cycleCleanupErrors.Add("owned process cleanup: $($_.Exception.Message)")
        }
        try {
            Assert-CatalystProbeTrustedTree -Context $Context
        } catch {
            $cycleCleanupErrors.Add("trusted-tree cleanup: $($_.Exception.Message)")
        }
        try {
            Restore-CatalystProbeTrackedVerificationSideEffects `
                -Context $cycleContext `
                -State $expectedState
        } catch {
            $cycleCleanupErrors.Add(
                "tracked verification-output cleanup: $($_.Exception.Message)")
        }
        if ($cycleCleanupErrors.Count -ne 0) {
            throw "Catalyst $Kind cycle cleanup failed: $($cycleCleanupErrors -join '; ')"
        }
    }
    if ($processResult.TimedOut) {
        throw "Catalyst $Kind cycle exceeded its fixed eight-minute budget."
    }
    if ($processResult.ExitCode -ne 0) {
        throw "Catalyst $Kind verifier failed before complete strict evidence was available."
    }
    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $cycleContext.ActiveDeadline -RequestedSeconds 10 `
        -Description "$Kind strict evidence"
    $cycle = Assert-CatalystProbeCycleEvidence `
        -Kind $Kind `
        -StrictEvidencePath $strictEvidencePath
    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $cycleContext.ActiveDeadline -RequestedSeconds 1 `
        -Description "$Kind completed evidence"
    return [pscustomobject]@{
        verifierExitCode = $processResult.ExitCode
        timedOut = [bool]$processResult.TimedOut
        startedUtc = $processResult.StartedUtc
        completedUtc = $processResult.CompletedUtc
        verifierLogSha256 = $processResult.LogSha256
        strictEvidenceSha256 = $cycle.StrictEvidenceSha256
        resultFiles = $cycle.ResultFiles
        identities = $cycle.Identities
        total = $cycle.Total
        passed = $cycle.Passed
        failed = $cycle.Failed
        skipped = $cycle.Skipped
        errors = $cycle.Errors
        cleanupStoppedProcessCount = $cleanupStoppedCount
    }
}

function Enable-CatalystProbeKnownNegative {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][string]$PatchPath
    )

    Assert-CatalystProbeTrustedTree -Context $Context
    if ([string]$Context.RepositoryState -cne 'baseline') {
        throw 'Catalyst probe known-negative apply requires the closed baseline state.'
    }
    Assert-CatalystProbeRepositoryState -Context $Context -State baseline

    $apply = Invoke-CatalystProbeBoundedProcess `
        -FileName 'git' `
        -ArgumentList @('apply', '--whitespace=nowarn', '--', $PatchPath) `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds 120 `
        -TaskDeadline $Context.ActiveDeadline `
        -ReserveSeconds $Context.ActiveReserveSeconds `
        -LogPath (Join-Path $Context.LogDirectory 'git-apply-known-negative.log')
    if ($apply.TimedOut -or $apply.ExitCode -ne 0) {
        throw 'Catalyst probe could not apply the fixed known-negative postimage.'
    }
    if ((Get-CatalystProbeFileSha256 -Path $Context.ProductPath) -cne
        $script:CatalystProbeNegativeFileSha256) {
        throw 'Catalyst probe applied product file does not match the fixed negative postimage.'
    }
    if ((Get-CatalystProbeWorkingProductBlob `
                -Context $Context `
                -LogName 'git-negative-working-blob.log') -cne
        $script:CatalystProbeNegativeBlob) {
        throw 'Catalyst probe applied product working blob is not the fixed postimage.'
    }
    $status = Get-CatalystProbeRepositoryStatus -Context $Context
    $productionComposite = $Context.PSObject.Properties['ProductionComposite'] -and
    [bool]$Context.ProductionComposite
    $expectedStatus = if ($productionComposite) {
        " M $($script:CatalystProbeProductPath)"
    } else {
        " M $($script:CatalystProbeProductPath)`n?? $($script:CatalystProbeFixtureTargetRelativePath)"
    }
    $expectedReverse = "?? $($script:CatalystProbeFixtureTargetRelativePath)`n M $($script:CatalystProbeProductPath)"
    if ($status -cne $expectedStatus -and
        (-not $productionComposite -and $status -cne $expectedReverse)) {
        throw 'Catalyst probe found repository changes outside its fixture and product path.'
    }
    $Context.RepositoryState = 'negative'
    Assert-CatalystProbeRepositoryState -Context $Context -State negative
}

function Restore-CatalystProbeRepository {
    param([Parameter(Mandatory = $true)][pscustomobject]$Context)

    $Context.ActiveDeadline =
    New-CatalystProbeCleanupDeadline -TaskDeadline $Context.TaskDeadline
    $Context.ActiveReserveSeconds = 0
    $errors = [Collections.Generic.List[string]]::new()
    try {
        $null = Stop-CatalystProbeOwnedApplication -Context $Context
    } catch {
        $errors.Add("owned process cleanup: $($_.Exception.Message)")
    }
    $scopeValidated = $false
    try {
        # Admitted build phases own their output cleanup. Do not erase a change
        # that their pre-execution scope guard rejected.
        Assert-CatalystProbeRepositoryState `
            -Context $Context `
            -State ([string]$Context.RepositoryState)
        $scopeValidated = $true
    } catch {
        $errors.Add("repository scope: $($_.Exception.Message)")
    }
    if ($scopeValidated) {
        try {
            if (Test-Path -LiteralPath $Context.FixtureTargetPath) {
                Remove-Item -LiteralPath $Context.FixtureTargetPath -Force
            }
        } catch {
            $errors.Add("fixture cleanup: $($_.Exception.Message)")
        }
        try {
            $restore = Invoke-CatalystProbeBoundedProcess `
                -FileName 'git' `
                -ArgumentList @(
                'restore', '--source', $script:CatalystProbeBaselineCommit,
                '--', $script:CatalystProbeProductPath) `
                -WorkingDirectory $Context.RepositoryRoot `
                -Environment $Context.RuntimeEnvironment `
                -TimeoutSeconds 120 `
                -TaskDeadline $Context.ActiveDeadline `
                -ReserveSeconds 0 `
                -LogPath (Join-Path $Context.LogDirectory 'git-restore-product.log')
            if ($restore.TimedOut -or $restore.ExitCode -ne 0) {
                throw 'fixed product restore failed'
            }
            if ((Get-CatalystProbeFileSha256 -Path $Context.ProductPath) -cne
                $script:CatalystProbeBaselineFileSha256) {
                throw 'restored product digest is incorrect'
            }
            if ((Get-CatalystProbeWorkingProductBlob `
                        -Context $Context `
                        -LogName 'git-restored-working-blob.log') -cne
                $script:CatalystProbeBaselineBlob) {
                throw 'restored product blob is incorrect'
            }
        } catch {
            $errors.Add("product cleanup: $($_.Exception.Message)")
        }
        try {
            Assert-CatalystProbeTrustedTree -Context $Context
        } catch {
            $errors.Add("trusted-tree cleanup: $($_.Exception.Message)")
        }
        try {
            $finalStatus = Get-CatalystProbeRepositoryStatus -Context $Context
            if ($finalStatus -cne $Context.InitialRepositoryStatus) {
                throw 'repository status differs from the observed pre-probe status'
            }
        } catch {
            $errors.Add("repository cleanup: $($_.Exception.Message)")
        }
    }
    if ($errors.Count -ne 0) {
        throw "Catalyst probe cleanup failed: $($errors -join '; ')"
    }
}

function New-CatalystProbeProductionCompositeModule {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][pscustomobject]$CoordinationDeadline,
        [Parameter(Mandatory = $true)][pscustomobject]$PreparedSimulator
    )

    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $CoordinationDeadline `
        -RequestedSeconds $script:CatalystProbeCoordinationBudgetSeconds `
        -Description 'production composite module loading'
    Assert-CatalystProbeTrustedTree -Context $Context
    $preparedUdid = [string]$PreparedSimulator.udid
    $contextPreparedUdid = if ($null -ne $Context.OwnedIosSimulator) {
        [string]$Context.OwnedIosSimulator.udid
    } else { '' }
    if (-not [bool]$PreparedSimulator.installedOnly -or
        $preparedUdid -cnotmatch
        '^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$' -or
        $contextPreparedUdid -cne $preparedUdid -or
        @($PreparedSimulator.installedRuntimeIdentifiers).Count -ne 1 -or
        [string]$PreparedSimulator.installedRuntimeIdentifiers[0] -cne
        $script:CatalystProbeIosRuntimeIdentifier) {
        throw (
            'The production composite private module requires the exact ' +
            'installed prepared iOS simulator identity.')
    }
    $orchestratorPath = Join-Path $Context.TrustedRoot 'scripts/Replicate-Issue.ps1'
    $item = Get-Item -LiteralPath $orchestratorPath -Force -ErrorAction Stop
    if ($item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'The production composite orchestrator must be an attested regular file.'
    }
    $agentTemp = [IO.Path]::GetFullPath(
        [Environment]::GetEnvironmentVariable('AGENT_TEMPDIRECTORY'))
    $privateArtifactRoot = Join-Path $agentTemp (
        'catalyst-production-composite-private')
    $privateRuntimeRoot = Join-Path $agentTemp (
        'catalyst-production-composite-runtime')
    foreach ($path in @($privateArtifactRoot, $privateRuntimeRoot)) {
        if (Test-Path -LiteralPath $path) {
            throw 'The production composite private module refuses to reuse its private roots.'
        }
    }
    New-Item -ItemType Directory -Path $privateArtifactRoot | Out-Null
    $contextPath = Join-Path $privateArtifactRoot 'unused-report-only-context'
    $priorPreparedUdid =
    [Environment]::GetEnvironmentVariable('CATALYST_PROBE_PREPARED_IOS_UDID')
    [Environment]::SetEnvironmentVariable(
        'CATALYST_PROBE_PREPARED_IOS_UDID', $preparedUdid)
    try {
        $module = New-Module `
            -Name $script:CatalystProbeProductionModuleName `
            -ScriptBlock {
            param($Path, $Parameters)
            . $Path @Parameters
        } `
            -ArgumentList $orchestratorPath, @{
            IssueNumber = 1
            Platform = 'ios'
            BaseSha = $script:CatalystProbeBaselineCommit
            ContextPath = $contextPath
            TrustedRoot = $Context.TrustedRoot
            ArtifactRoot = $privateArtifactRoot
            TrustedTreeAttestationPath = $Context.TrustedTreeAttestation
            TrustedSourceVersion = $Context.ExpectedSourceVersion
            DeviceUdid = $preparedUdid
            StepTimeoutMinutes = 0
            Model = ''
            FixedCatalystCompositeProbeLibraryOnly = $true
            RequirePreparedIosSimulator = $true
        }
    } catch {
        if (-not (Get-ChildItem -LiteralPath $privateArtifactRoot -Force |
                    Select-Object -First 1)) {
            Remove-Item -LiteralPath $privateArtifactRoot -Force
        }
        throw
    } finally {
        [Environment]::SetEnvironmentVariable(
            'CATALYST_PROBE_PREPARED_IOS_UDID', $priorPreparedUdid)
    }
    if ($null -eq $module -or
        [string]$module.Name -cne $script:CatalystProbeProductionModuleName) {
        throw 'The fixed production-composite private module did not load.'
    }
    $Context | Add-Member -NotePropertyName ProductionPrivateArtifactRoot `
        -NotePropertyValue $privateArtifactRoot -Force
    $Context | Add-Member -NotePropertyName ProductionPrivateRuntimeRoot `
        -NotePropertyValue $privateRuntimeRoot -Force
    Assert-CatalystProbeTrustedTree -Context $Context
    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $CoordinationDeadline `
        -RequestedSeconds 1 `
        -Description 'production composite module loading'
    return $module
}

function Invoke-CatalystProbePrepareIosSimulator {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)][pscustomobject]$CoordinationDeadline
    )

    $developer = Invoke-CatalystProbeBoundedProcess `
        -FileName '/usr/bin/xcode-select' `
        -ArgumentList @('-p') `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds 20 `
        -TaskDeadline $CoordinationDeadline `
        -LogPath (Join-Path $Context.LogDirectory 'ios-xcode-select.log')
    $expectedDeveloper = "$(Get-CatalystProbeXcodePath)/Contents/Developer"
    if ($developer.TimedOut -or $developer.ExitCode -ne 0 -or
        $developer.Stdout.Trim() -cne $expectedDeveloper) {
        throw 'Production composite iOS preparation is not using fixed Xcode 26.0.1.'
    }
    # Discovery can initialize CoreSimulator. All reads spend the same existing
    # coordination allowance, including process termination, without retries.
    $runtimes = Invoke-CatalystProbeBoundedProcess `
        -FileName '/usr/bin/xcrun' `
        -ArgumentList @('simctl', 'list', 'runtimes', '--json') `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds $script:CatalystProbeCoordinationBudgetSeconds `
        -TaskDeadline $CoordinationDeadline `
        -LogPath (Join-Path $Context.LogDirectory 'ios-installed-runtimes.log')
    if ($runtimes.TimedOut -or $runtimes.ExitCode -ne 0) {
        throw ("Production composite iOS preparation could not inspect installed runtimes " +
            "(exitCode=$($runtimes.ExitCode), timedOut=$($runtimes.TimedOut)).")
    }
    $runtimeDocument = $runtimes.Stdout | ConvertFrom-Json -Depth 8
    $installedIos = @($runtimeDocument.runtimes | Where-Object {
            [string]$_.identifier -ceq
            $script:CatalystProbeIosRuntimeIdentifier -and
            [bool]$_.isAvailable
        })
    if ($installedIos.Count -ne 1) {
        throw ('Production composite iOS preparation requires the exact ' +
            'already-installed iOS 26.0 runtime.')
    }
    $deviceTypes = Invoke-CatalystProbeBoundedProcess `
        -FileName '/usr/bin/xcrun' `
        -ArgumentList @('simctl', 'list', 'devicetypes', '--json') `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds $script:CatalystProbeCoordinationBudgetSeconds `
        -TaskDeadline $CoordinationDeadline `
        -LogPath (Join-Path $Context.LogDirectory 'ios-installed-devicetypes.log')
    if ($deviceTypes.TimedOut -or $deviceTypes.ExitCode -ne 0) {
        throw ("Production composite iOS preparation could not inspect installed device types " +
            "(exitCode=$($deviceTypes.ExitCode), timedOut=$($deviceTypes.TimedOut)).")
    }
    $deviceTypeDocument = $deviceTypes.Stdout | ConvertFrom-Json -Depth 8
    $matchingTypes = @($deviceTypeDocument.devicetypes | Where-Object {
            [string]$_.identifier -ceq $script:CatalystProbeIosDeviceType -and
            [string]$_.name -ceq 'iPhone 11 Pro'
        })
    if ($matchingTypes.Count -ne 1) {
        throw ('Production composite iOS preparation requires the fixed installed ' +
            'iPhone 11 Pro device type.')
    }

    $devices = Invoke-CatalystProbeBoundedProcess `
        -FileName '/usr/bin/xcrun' `
        -ArgumentList @('simctl', 'list', 'devices', '--json') `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds $script:CatalystProbeCoordinationBudgetSeconds `
        -TaskDeadline $CoordinationDeadline `
        -LogPath (Join-Path $Context.LogDirectory 'ios-installed-devices.log')
    if ($devices.TimedOut -or $devices.ExitCode -ne 0) {
        throw ("Production composite iOS preparation could not inspect installed devices " +
            "(exitCode=$($devices.ExitCode), timedOut=$($devices.TimedOut)).")
    }
    $deviceDocument = $devices.Stdout | ConvertFrom-Json -Depth 12
    $runtimeProperty = $deviceDocument.devices.PSObject.Properties[
    $script:CatalystProbeIosRuntimeIdentifier]
    if ($null -eq $runtimeProperty) {
        throw 'Production composite iOS preparation could not bind devices to its runtime.'
    }
    $bootedMatchingDevices = @($runtimeProperty.Value | Where-Object {
            [bool]$_.isAvailable -and
            [string]$_.state -ceq 'Booted' -and
            [string]$_.deviceTypeIdentifier -ceq
            $script:CatalystProbeIosDeviceType -and
            [string]$_.udid -cmatch
            '^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$'
        })
    if ($bootedMatchingDevices.Count -gt 1) {
        throw 'Production composite iOS preparation found ambiguous booted simulators.'
    }
    $selected = $null
    if ($bootedMatchingDevices.Count -eq 1) {
        $selected = [pscustomobject]@{
            udid = [string]$bootedMatchingDevices[0].udid
            installedRuntimeIdentifiers = @(
                $script:CatalystProbeIosRuntimeIdentifier)
            xcodeDeveloperPath = $expectedDeveloper
            installedOnly = $true
            createdByProbe = $false
            bootedByProbe = $false
            bootAttemptedByProbe = $false
        }
        $Context.OwnedIosSimulator = $selected
    }

    if ($null -eq $selected) {
        $created = Invoke-CatalystProbeBoundedProcess `
            -FileName '/usr/bin/xcrun' `
            -ArgumentList @(
            'simctl', 'create',
            $script:CatalystProbeIosDeviceName,
            $script:CatalystProbeIosDeviceType,
            $script:CatalystProbeIosRuntimeIdentifier) `
            -WorkingDirectory $Context.RepositoryRoot `
            -Environment $Context.RuntimeEnvironment `
            -TimeoutSeconds 30 `
            -TaskDeadline $CoordinationDeadline `
            -LogPath (Join-Path $Context.LogDirectory 'ios-simulator-create.log')
        $udid = $created.Stdout.Trim().ToUpperInvariant()
        if ($created.TimedOut -or $created.ExitCode -ne 0 -or
            $udid -cnotmatch
            '^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$') {
            throw 'Production composite iOS preparation could not create its owned simulator.'
        }
        $selected = [pscustomobject]@{
            udid = $udid
            installedRuntimeIdentifiers = @($script:CatalystProbeIosRuntimeIdentifier)
            xcodeDeveloperPath = $expectedDeveloper
            installedOnly = $true
            createdByProbe = $true
            bootedByProbe = $false
            bootAttemptedByProbe = $true
        }
        $Context.OwnedIosSimulator = $selected
        $boot = Invoke-CatalystProbeBoundedProcess `
            -FileName '/usr/bin/xcrun' `
            -ArgumentList @('simctl', 'boot', $udid) `
            -WorkingDirectory $Context.RepositoryRoot `
            -Environment $Context.RuntimeEnvironment `
            -TimeoutSeconds 30 `
            -TaskDeadline $CoordinationDeadline `
            -LogPath (Join-Path $Context.LogDirectory 'ios-simulator-boot.log')
        if ($boot.TimedOut -or $boot.ExitCode -ne 0) {
            throw 'Production composite iOS preparation could not boot its owned simulator.'
        }
        $selected.bootedByProbe = $true
    }
    $ready = Invoke-CatalystProbeBoundedProcess `
        -FileName '/usr/bin/xcrun' `
        -ArgumentList @('simctl', 'bootstatus', ([string]$selected.udid), '-b') `
        -WorkingDirectory $Context.RepositoryRoot `
        -Environment $Context.RuntimeEnvironment `
        -TimeoutSeconds $script:CatalystProbeCoordinationBudgetSeconds `
        -TaskDeadline $CoordinationDeadline `
        -LogPath (Join-Path $Context.LogDirectory 'ios-simulator-bootstatus.log')
    if ($ready.TimedOut -or $ready.ExitCode -ne 0) {
        throw 'Production composite iOS simulator did not become ready.'
    }
    return $selected
}

function Remove-CatalystProbeOwnedIosSimulator {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [AllowNull()][pscustomobject]$Simulator,
        [Parameter(Mandatory = $true)][pscustomobject]$CleanupDeadline
    )

    if ($null -eq $Simulator -or -not [bool]$Simulator.createdByProbe) {
        return
    }
    $udid = [string]$Simulator.udid
    if ($udid -cnotmatch
        '^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$') {
        throw 'Catalyst probe refuses to clean a simulator with an invalid identity.'
    }
    $errors = [Collections.Generic.List[string]]::new()
    $bootWasAttempted = if (
        $Simulator.PSObject.Properties['bootAttemptedByProbe']) {
        [bool]$Simulator.bootAttemptedByProbe
    } else {
        [bool]$Simulator.bootedByProbe
    }
    if ($bootWasAttempted) {
        try {
            $shutdown = Invoke-CatalystProbeBoundedProcess `
                -FileName '/usr/bin/xcrun' `
                -ArgumentList @('simctl', 'shutdown', $udid) `
                -WorkingDirectory $Context.RepositoryRoot `
                -Environment $Context.RuntimeEnvironment `
                -TimeoutSeconds 30 `
                -TaskDeadline $CleanupDeadline `
                -LogPath (Join-Path $Context.LogDirectory 'ios-simulator-shutdown.log')
            if ($shutdown.TimedOut -or $shutdown.ExitCode -ne 0) {
                throw 'owned simulator shutdown failed'
            }
        } catch {
            $errors.Add($_.Exception.Message)
        }
    }
    try {
        $delete = Invoke-CatalystProbeBoundedProcess `
            -FileName '/usr/bin/xcrun' `
            -ArgumentList @('simctl', 'delete', $udid) `
            -WorkingDirectory $Context.RepositoryRoot `
            -Environment $Context.RuntimeEnvironment `
            -TimeoutSeconds 30 `
            -TaskDeadline $CleanupDeadline `
            -LogPath (Join-Path $Context.LogDirectory 'ios-simulator-delete.log')
        if ($delete.TimedOut -or $delete.ExitCode -ne 0) {
            throw 'owned simulator deletion failed'
        }
    } catch {
        $errors.Add($_.Exception.Message)
    }
    if ($errors.Count -ne 0) {
        throw "Catalyst probe owned simulator cleanup failed: $($errors -join '; ')"
    }
}

function Get-CatalystProbeProductionCompositeSelection {
    param(
        [Parameter(Mandatory = $true)][System.Management.Automation.PSModuleInfo]$Module,
        [Parameter(Mandatory = $true)][pscustomobject]$Context
    )

    if (Test-Path -LiteralPath (
            Join-Path $Context.RepositoryRoot $script:CatalystProbePrimaryAnchorPath)) {
        throw 'The fixed Label metadata anchor must remain absent from the worktree.'
    }
    $values = & $Module {
        param($RepositoryRoot, $BaselineSha, $AnchorPath, $ProductPath)
        $selection = Get-ReplicationRegressionLaneSelection `
            -TestPath $AnchorPath `
            -RepositoryRoot $RepositoryRoot `
            -BaselineSha $BaselineSha `
            -Platform 'ios'
        $requirement = Get-ReplicationFixedCompanionRequirement `
            -Platform 'ios' `
            -FixPaths @($ProductPath)
        return [pscustomobject]@{
            Selection = $selection
            Requirement = $requirement
        }
    } $Context.RepositoryRoot $script:CatalystProbeBaselineCommit `
        $script:CatalystProbePrimaryAnchorPath $script:CatalystProbeProductPath
    if ($null -eq $values.Selection -or
        [string]$values.Selection.Category -cne
        $script:CatalystProbePrimaryCategory -or
        [string]$values.Selection.TestClass -cne $script:CatalystProbePrimaryClass -or
        [string]$values.Selection.Platform -cne 'ios' -or
        [string]$values.Selection.BaselineSha -cne
        $script:CatalystProbeBaselineCommit -or
        [string]$values.Selection.GeneratedTestPath -cne
        $script:CatalystProbePrimaryAnchorPath -or
        $null -eq $values.Requirement -or
        [string]$values.Requirement.Id -cne
        'gesture-platform-manager-catalyst-v1') {
        throw 'The immutable baseline did not derive the fixed Label plus Catalyst composite.'
    }
    return $values
}

function Invoke-CatalystProbeProductionPrewarm {
    param(
        [Parameter(Mandatory = $true)][System.Management.Automation.PSModuleInfo]$Module,
        [Parameter(Mandatory = $true)][pscustomobject]$Deadline
    )

    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $Deadline `
        -RequestedSeconds $script:CatalystProbePrewarmBudgetSeconds `
        -Description 'production dual-Apple prewarm'
    $preparedUdid = & $Module { [string]$DeviceUdid }
    if ($preparedUdid -cnotmatch
        '^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$') {
        throw 'The production prewarm module lost its prepared simulator binding.'
    }
    $priorRuntimeUdid =
    [Environment]::GetEnvironmentVariable('MAUI_REPLICATION_DEVICE_UDID')
    [Environment]::SetEnvironmentVariable(
        'MAUI_REPLICATION_DEVICE_UDID', $preparedUdid)
    try {
        return & $Module {
            param($DeadlineTimestamp)
            Invoke-ReplicationFixedCatalystCompositePrewarm `
                -DeadlineTimestamp $DeadlineTimestamp
        } ([long]$Deadline.DeadlineTimestamp)
    } finally {
        [Environment]::SetEnvironmentVariable(
            'MAUI_REPLICATION_DEVICE_UDID', $priorRuntimeUdid)
    }
}

function Assert-CatalystProbeProductionAssetsRetained {
    param([Parameter(Mandatory = $true)]
        [System.Management.Automation.PSModuleInfo]$Module)

    return & $Module { Assert-ReplicationAppleCompanionPrewarmRetained }
}

function Get-CatalystProbeProductionPrewarmLogs {
    param([Parameter(Mandatory = $true)][string]$PrewarmRoot)

    $root = [IO.Path]::GetFullPath($PrewarmRoot)
    $rootItem = Get-Item -LiteralPath $root -Force -ErrorAction Stop
    if (-not $rootItem.PSIsContainer -or
        $rootItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Production composite prewarm evidence root is not a regular directory.'
    }
    $expected = @(
        'prewarm-build-ios-simulator-no-restore.log'
        'prewarm-build-maccatalyst-no-restore.log'
        'prewarm-restore-dual-apple-graph.log'
        'tool-restore.log'
        'xharness-command-probe/xharness-help-tail.log'
        'xharness-command-probe/xharness-help.log'
        'xharness-preflight-child.log'
        'xharness-preflight/xharness-help-tail.log'
        'xharness-preflight/xharness-help.log'
        'xharness-preflight/xharness-preflight.log'
    )
    $entries = @(Get-ChildItem -LiteralPath $root -Recurse -Force)
    if (@($entries | Where-Object {
                $_.Attributes -band [IO.FileAttributes]::ReparsePoint
            }).Count -ne 0) {
        throw 'Production composite prewarm evidence contains a linked entry.'
    }
    $files = @($entries | Where-Object { -not $_.PSIsContainer })
    if (@($files | Where-Object {
                $_.Length -lt 1 -or $_.Length -gt 256KB
            }).Count -ne 0) {
        throw 'Production composite prewarm evidence contains an empty or oversized log.'
    }
    $relativeFiles = @($files | ForEach-Object {
            [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/')
        } | Sort-Object)
    if ($relativeFiles.Count -ne $expected.Count -or
        (Compare-Object -ReferenceObject $expected -DifferenceObject $relativeFiles)) {
        throw 'Production composite prewarm did not retain its exact bounded command logs.'
    }
    return @($files | Sort-Object {
            [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/')
        } | ForEach-Object {
            [ordered]@{
                name = ([IO.Path]::GetRelativePath(
                        $root, $_.FullName)).Replace('\', '/')
                sha256 = Get-CatalystProbeFileSha256 -Path $_.FullName
            }
        })
}

function Invoke-CatalystProbeProductionComposite {
    param(
        [Parameter(Mandatory = $true)][System.Management.Automation.PSModuleInfo]$Module,
        [Parameter(Mandatory = $true)]$Selection,
        [Parameter(Mandatory = $true)]$Requirement,
        [Parameter(Mandatory = $true)][string]$PrimaryOutputDirectory,
        [Parameter(Mandatory = $true)][string]$CompanionOutputDirectory,
        [Parameter(Mandatory = $true)][pscustomobject]$Deadline,
        [Parameter(Mandatory = $true)][pscustomobject]$Context,
        [Parameter(Mandatory = $true)]
        [ValidateSet('baseline', 'negative')][string]$State
    )

    $preparedUdid = & $Module { [string]$DeviceUdid }
    if ($preparedUdid -cnotmatch
        '^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$') {
        throw 'The production composite module lost its prepared simulator binding.'
    }
    if (-not $Context.PSObject.Properties['ProductionComposite'] -or
        -not [bool]$Context.ProductionComposite -or
        [string]$Context.RepositoryState -cne $State -or
        [int]$Deadline.BudgetSeconds -ne $script:CatalystProbeCycleBudgetSeconds) {
        throw 'The production composite wrapper is outside its admitted closed phase.'
    }
    $phaseContext = $Context.PSObject.Copy()
    $phaseContext.ActiveDeadline = $Deadline
    $phaseContext.ActiveReserveSeconds = 0
    Assert-CatalystProbeTrustedTree -Context $phaseContext
    Assert-CatalystProbeRepositoryState `
        -Context $phaseContext `
        -State $State
    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $Deadline `
        -RequestedSeconds $script:CatalystProbeCycleBudgetSeconds `
        -Description "$State production composite"

    $priorRuntimeUdid =
    [Environment]::GetEnvironmentVariable('MAUI_REPLICATION_DEVICE_UDID')
    [Environment]::SetEnvironmentVariable(
        'MAUI_REPLICATION_DEVICE_UDID', $preparedUdid)
    try {
        try {
            & $Module {
                param(
                    $Selection,
                    $Requirement,
                    $PrimaryOutputDirectory,
                    $CompanionOutputDirectory,
                    $TimeoutSeconds,
                    $DeadlineTimestamp)
                Invoke-ReplicationRegressionCompositeRun `
                    -Selection $Selection `
                    -CompanionRequirement $Requirement `
                    -PrimaryOutputDirectory $PrimaryOutputDirectory `
                    -CompanionOutputDirectory $CompanionOutputDirectory `
                    -TrustedScriptRoot $trustedScripts `
                    -TimeoutSeconds $TimeoutSeconds `
                    -DeadlineTimestamp $DeadlineTimestamp `
                    -DeviceUdid $DeviceUdid `
                    -RequirePreparedIosSimulator:$RequirePreparedIosSimulator
            } $Selection $Requirement $PrimaryOutputDirectory `
                $CompanionOutputDirectory $script:CatalystProbeCycleBudgetSeconds `
            ([long]$Deadline.DeadlineTimestamp)
        } finally {
            Restore-CatalystProbeTrackedVerificationSideEffects `
                -Context $phaseContext `
                -State $State
            $null = Get-CatalystProbeProcessTimeoutSeconds `
                -Deadline $Deadline `
                -RequestedSeconds 1 `
                -Description "$State production composite restoration"
        }
    } finally {
        [Environment]::SetEnvironmentVariable(
            'MAUI_REPLICATION_DEVICE_UDID', $priorRuntimeUdid)
    }
}

function Assert-CatalystProbePrimaryRun {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Selection
    )

    $run = Read-ReplicationRegressionRunEvidence `
        -Path $Path `
        -ExpectedPlatform 'ios' `
        -ExpectedProject ([string]$Selection.Project) `
        -ExpectedCategory $script:CatalystProbePrimaryCategory `
        -ExpectedClass $script:CatalystProbePrimaryClass
    if ([int]$run.Document.failed -ne 0 -or
        [int]$run.Document.errors -ne 0 -or
        [int]$run.Document.passed -lt 1) {
        throw 'The production composite Label primary lane did not remain non-regressing.'
    }
    return $run
}

function Confirm-CatalystProbeExpectedProductionRejection {
    param([Parameter(Mandatory = $true)]
        [Management.Automation.ErrorRecord]$ErrorRecord)

    if ($ErrorRecord.Exception.Message -cne
        $script:CatalystProbeExpectedCompanionRejection) {
        throw $ErrorRecord
    }
    return $ErrorRecord.Exception.Message
}

function Invoke-CatalystProbeProductionV2Validation {
    param(
        [Parameter(Mandatory = $true)][System.Management.Automation.PSModuleInfo]$Module,
        [Parameter(Mandatory = $true)]$Selection,
        [Parameter(Mandatory = $true)]$Requirement,
        [Parameter(Mandatory = $true)][string]$EvidenceArtifactRoot,
        [Parameter(Mandatory = $true)][string]$PatchPath,
        [Parameter(Mandatory = $true)][string]$TrustedFixturePath,
        [Parameter(Mandatory = $true)][pscustomobject]$Deadline
    )

    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $Deadline `
        -RequestedSeconds 1 `
        -Description 'production v2 patch staging'
    $null = Copy-CatalystProbeProductionValidationPatch `
        -PatchPath $PatchPath `
        -EvidenceArtifactRoot $EvidenceArtifactRoot
    $result = $null
    $validationError = $null
    try {
        $result = & $Module {
            param(
                $Selection,
                $Requirement,
                $EvidenceArtifactRoot,
                $PatchPath,
                $TrustedFixturePath,
                $ProductPath)
            $path = Write-ReplicationRegressionEvidenceDocument `
                -Selection $Selection `
                -CompanionRequirement $Requirement `
                -EvidenceArtifactRoot $EvidenceArtifactRoot `
                -ProductPatchPath $PatchPath
            $null = Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $EvidenceArtifactRoot `
                -ExpectedBaselineSha ([string]$Selection.BaselineSha) `
                -ExpectedPlatform 'ios' `
                -ExpectedCategory ([string]$Selection.Category) `
                -ExpectedProject ([string]$Selection.Project) `
                -ExpectedProjectPath ([string]$Selection.ProjectPath) `
                -ExpectedClass ([string]$Selection.TestClass) `
                -ExpectedGeneratedTestPath ([string]$Selection.GeneratedTestPath) `
                -ExpectedFixPaths @($ProductPath) `
                -TrustedFixturePath $TrustedFixturePath
            return $path
        } $Selection $Requirement $EvidenceArtifactRoot $PatchPath `
            $TrustedFixturePath $script:CatalystProbeProductPath
    } catch {
        $validationError = $_
    }
    $null = Get-CatalystProbeProcessTimeoutSeconds `
        -Deadline $Deadline `
        -RequestedSeconds 1 `
        -Description 'production v2 evidence validation'
    if ($validationError) {
        throw $validationError
    }
    return $result
}

function Remove-CatalystProbeProductionCompositeModule {
    param(
        [AllowNull()][System.Management.Automation.PSModuleInfo]$Module,
        [Parameter(Mandatory = $true)][pscustomobject]$Context
    )

    $moduleCleanupError = $null
    if ($Module) {
        try {
            & $Module {
                param($DeadlineTimestamp)
                Remove-ReplicationRuntimeCache -DeadlineTimestamp $DeadlineTimestamp
            } ([long]$Context.ActiveDeadline.DeadlineTimestamp)
        } catch {
            $moduleCleanupError = $_
        } finally {
            Remove-Module -ModuleInfo $Module -Force
        }
    }
    if ($Context.PSObject.Properties['ProductionPrivateArtifactRoot']) {
        $privateRoot = [string]$Context.ProductionPrivateArtifactRoot
        $remaining = @(Get-ChildItem -LiteralPath $privateRoot -Force `
                -ErrorAction SilentlyContinue)
        if ($remaining.Count -ne 0) {
            throw 'The production composite private artifact root contains unexpected bytes.'
        }
        Remove-Item -LiteralPath $privateRoot -Force
    }
    if ($Context.PSObject.Properties['ProductionPrivateRuntimeRoot'] -and
        (Test-Path -LiteralPath (
            [string]$Context.ProductionPrivateRuntimeRoot))) {
        throw 'The production composite private runtime cache was not removed.'
    }
    if ($moduleCleanupError) {
        throw $moduleCleanupError
    }
}

function New-CatalystProbeResult {
    param(
        [Parameter(Mandatory = $true)][string]$ExpectedSourceVersion,
        [Parameter(Mandatory = $true)][string]$TrustedTreeHash,
        [Parameter(Mandatory = $true)][string]$TrustedTreeAttestationSha256
    )

    return [ordered]@{
        schemaVersion = 2
        mode = 'catalyst-gesture-probe'
        reportOnly = $true
        outcome = 'inconclusive'
        reason = 'not-started'
        sourceVersion = $ExpectedSourceVersion
        trustedTreeHash = $TrustedTreeHash
        trustedTreeAttestationSha256 = $TrustedTreeAttestationSha256
        trustedImplementation = $null
        fixedInputs = [ordered]@{
            repository = 'https://github.com/dotnet/maui.git'
            baselineCommit = $script:CatalystProbeBaselineCommit
            negativeCommit = $script:CatalystProbeNegativeCommit
            xcodeVersion = $script:CatalystProbeXcodeVersion
            productPath = $script:CatalystProbeProductPath
            baselineBlob = $script:CatalystProbeBaselineBlob
            negativeBlob = $script:CatalystProbeNegativeBlob
            baselineFileSha256 = $script:CatalystProbeBaselineFileSha256
            negativeFileSha256 = $script:CatalystProbeNegativeFileSha256
            productPatchSha256 = $script:CatalystProbePatchSha256
            fixtureSha256 = $script:CatalystProbeFixtureSha256
        }
        scope = 'known-bad-postimage-replay-only'
        selectors = [ordered]@{
            primary = [ordered]@{
                platform = 'ios'
                project = 'Controls'
                category = $script:CatalystProbePrimaryCategory
                class = $script:CatalystProbePrimaryClass
                metadataAnchor = $script:CatalystProbePrimaryAnchorPath
                metadataAnchorIsGeneratedTest = $false
            }
            companion = [ordered]@{
                platform = 'catalyst'
                project = 'Controls'
                category = 'Gesture'
                class = $script:CatalystProbeClass
                methods = @($script:CatalystProbeMethods)
                expectedNativeTapCounts =
                'public native UI Tap: 0 -> 1; NumberRequired: 2'
            }
        }
        budget = [ordered]@{
            cycleCountMaximum = 2
            cycleSecondsEach = $script:CatalystProbeCycleBudgetSeconds
            cycleVerifierSeconds = $script:CatalystProbeVerifierBudgetSeconds
            prewarmSeconds = $script:CatalystProbePrewarmBudgetSeconds
            patchApplySeconds = $script:CatalystProbePatchApplyBudgetSeconds
            coordinationSeconds = $script:CatalystProbeCoordinationBudgetSeconds
            overallScriptSeconds = $script:CatalystProbeOverallBudgetSeconds
            cleanupSeconds = $script:CatalystProbeCleanupBudgetSeconds
            summarySeconds = $script:CatalystProbeSummaryBudgetSeconds
            azureTaskSeconds = $script:CatalystProbeTaskTimeoutSeconds
            azureTerminationReserveSeconds =
            $script:CatalystProbeTaskTerminationReserveSeconds
            artifactTailSeconds = $script:CatalystProbeArtifactTailSeconds
            jobDeadlineUtc = $null
            taskRemainingAtSummarySeconds = $null
            retries = 0
        }
        prewarm = $null
        simulator = $null
        baseline = $null
        negative = $null
        expectedV2Rejection = $null
        cleanup = [ordered]@{
            completed = $false
            simulatorRemoved = $false
            fixtureRemoved = $false
            productRestored = $false
            repositoryStatusRestored = $false
            trustedTreeRestored = $false
        }
        assertions = [ordered]@{
            baselinePrimary = 'fresh strict iOS Label multiset with no failures'
            baselineCompanion = 'exact-two-pass'
            negativePrimary = 'fresh strict iOS Label multiset unchanged from baseline'
            negativeCompanion =
            'exact-two-expected-Assert.Single-empty-failures'
            productionV2 = 'must-reject-known-negative-Catalyst-result'
        }
        exclusions = [ordered]@{
            certifiesIssue = $false
            certifiesProduct = $false
            publishesOutcome = $false
            mutatesPullRequest = $false
            invokesModel = $false
            executesProductionComposite = $true
            executesIssueLinkedCode = $false
            generatedIssueTest = $false
        }
        profileBoundary = [ordered]@{
            requiresFreshAzureJob = $true
            initialContainerResultRequiredAbsent = $true
            deletesPreexistingContainerResult = $false
            ios = 'ios-review-host-no-network-isolation'
            iosIndependentOutboundIsolation = $false
            catalyst = 'signed-app-sandbox-live-outbound-deny'
        }
        limitations = [ordered]@{
            assetsFileRetained = $false
            assetsContinuity =
            'validated by attested production code before and after native execution'
        }
        failure = $null
    }
}

function Write-CatalystProbeResult {
    param(
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Result,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $json = $Result | ConvertTo-Json -Depth 12
    if ([Text.Encoding]::UTF8.GetByteCount($json) -gt 262144) {
        throw 'Catalyst probe result exceeded its fixed report bound.'
    }
    [IO.File]::WriteAllText(
        [IO.Path]::GetFullPath($Path),
        "$json`n",
        [Text.UTF8Encoding]::new($false))
}

function Invoke-CatalystGestureRegressionProbeCore {
    param(
        [Parameter(Mandatory = $true)][string]$ExpectedSourceVersion,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][string]$TrustedTreeAttestation,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [Parameter(Mandatory = $true)][pscustomobject]$TaskDeadline
    )

    Assert-CatalystProbeClosedAzureIdentity `
        -ExpectedSourceVersion $ExpectedSourceVersion `
        -RepositoryRoot $RepositoryRoot `
        -TrustedRoot $TrustedRoot `
        -TrustedTreeAttestation $TrustedTreeAttestation `
        -OutputDirectory $OutputDirectory
    $context = $null
    $productionModule = $null
    $probeError = $null
    $cleanupError = $null
    $summaryError = $null
    $result = New-CatalystProbeResult `
        -ExpectedSourceVersion $ExpectedSourceVersion `
        -TrustedTreeHash 'unavailable' `
        -TrustedTreeAttestationSha256 'unavailable'
    $started = [DateTimeOffset]::UtcNow
    try {
        $minimumRemainingWorkSeconds =
        $script:CatalystProbePrewarmBudgetSeconds +
        (2 * $script:CatalystProbeCycleBudgetSeconds) +
        $script:CatalystProbePatchApplyBudgetSeconds +
        $script:CatalystProbeCleanupBudgetSeconds +
        $script:CatalystProbeSummaryBudgetSeconds
        $null = Assert-CatalystProbeTaskPhaseAdmission `
            -Deadline $TaskDeadline `
            -RequiredSeconds $minimumRemainingWorkSeconds `
            -Description 'initialization, patch preparation, and both cycles'
        $attestationDocument =
        Read-TrustedTreeAttestation -Path $TrustedTreeAttestation
        $result.trustedTreeHash = [string]$attestationDocument.treeHash
        $result.trustedTreeAttestationSha256 =
        Get-CatalystProbeFileSha256 -Path $TrustedTreeAttestation
        $context = Initialize-CatalystGestureProbeContext `
            -ExpectedSourceVersion $ExpectedSourceVersion `
            -RepositoryRoot $RepositoryRoot `
            -TrustedRoot $TrustedRoot `
            -TrustedTreeAttestation $TrustedTreeAttestation `
            -OutputDirectory $OutputDirectory `
            -TaskDeadline $TaskDeadline
        $result.trustedImplementation = [ordered]@{}
        foreach ($relativePath in @(
                'scripts/Replicate-Issue.ps1',
                'scripts/Invoke-CatalystGestureRegressionProbe.ps1',
                'scripts/shared/Assert-ReplicationCertificationBinding.ps1',
                'scripts/shared/Replication-AppleCompanionPrewarm.ps1')) {
            $result.trustedImplementation[$relativePath] =
            Get-CatalystProbeFileSha256 -Path (
                Join-Path $context.TrustedRoot $relativePath)
        }
        $result.budget.jobDeadlineUtc = $context.JobDeadlineUtc
        $null = Assert-CatalystProbeDeadlineAdmission `
            -DeadlineUtc $context.JobDeadlineUtc `
            -Phase 'complete report-only probe' `
            -PhaseBudgetSeconds $script:CatalystProbeOverallBudgetSeconds `
            -ArtifactTailSeconds $context.ArtifactTailSeconds
        $coordinationDeadline =
        New-CatalystProbeCoordinationDeadline -TaskDeadline $TaskDeadline
        $context.ActiveDeadline = $coordinationDeadline
        $context.ActiveReserveSeconds = 0
        $patchPath = New-CatalystProbeFixedPatch -Context $context
        Assert-CatalystProbeFixedPatchPolicy `
            -Context $context `
            -PatchPath $patchPath
        $result.simulator = Invoke-CatalystProbePrepareIosSimulator `
            -Context $context `
            -CoordinationDeadline $coordinationDeadline
        $productionModule = New-CatalystProbeProductionCompositeModule `
            -Context $context `
            -CoordinationDeadline $coordinationDeadline `
            -PreparedSimulator $result.simulator
        $productionInputs = Get-CatalystProbeProductionCompositeSelection `
            -Module $productionModule `
            -Context $context
        $selection = $productionInputs.Selection
        $requirement = $productionInputs.Requirement
        $null = Get-CatalystProbeProcessTimeoutSeconds `
            -Deadline $coordinationDeadline `
            -RequestedSeconds 1 `
            -Description 'production composite coordination'
        $null = Assert-CatalystProbeTaskPhaseAdmission `
            -Deadline $TaskDeadline `
            -RequiredSeconds $minimumRemainingWorkSeconds `
            -Description 'prewarm and both native cycles'
        $prewarmDeadline =
        New-CatalystProbePrewarmDeadline -TaskDeadline $TaskDeadline
        $context.ActiveDeadline = $prewarmDeadline
        $context.ActiveReserveSeconds = 0
        $prewarm = Invoke-CatalystProbeProductionPrewarm `
            -Module $productionModule `
            -Deadline $prewarmDeadline
        $retainedAssets =
        Assert-CatalystProbeProductionAssetsRetained -Module $productionModule
        $prewarmLogRoot = Join-Path $context.OutputDirectory (
            'production-composite/prewarm')
        $prewarmLogs = @(Get-CatalystProbeProductionPrewarmLogs `
                -PrewarmRoot $prewarmLogRoot)
        $null = Get-CatalystProbeProcessTimeoutSeconds `
            -Deadline $prewarmDeadline `
            -RequestedSeconds 1 `
            -Description 'production dual-Apple prewarm evidence'
        $result.prewarm = [ordered]@{
            ready = $true
            phaseBudgetSeconds = [int]$prewarmDeadline.BudgetSeconds
            assetsSha256 = [string]$retainedAssets.AssetsSha256
            targetPairs = @($retainedAssets.TargetPairs)
            commandLogs = $prewarmLogs
            privateToolRestore = $true
            xharnessCommandPreflight = $true
            fullIosBuild = $true
            fullCatalystBuild = $true
            noRestoreBuilds = $true
        }
        $context.RepositoryState = 'baseline'
        Assert-CatalystProbeRepositoryState -Context $context -State baseline

        $null = Assert-CatalystProbeTaskPhaseAdmission `
            -Deadline $TaskDeadline `
            -RequiredSeconds (
            (2 * $script:CatalystProbeCycleBudgetSeconds) +
            $script:CatalystProbePatchApplyBudgetSeconds +
            $script:CatalystProbeCleanupBudgetSeconds +
            $script:CatalystProbeSummaryBudgetSeconds) `
            -Description 'both native cycles'
        $null = Assert-CatalystProbeDeadlineAdmission `
            -DeadlineUtc $context.JobDeadlineUtc `
            -Phase 'baseline and negative cycles' `
            -PhaseBudgetSeconds $script:CatalystProbeCycleBudgetSeconds `
            -RemainingPhaseBudgetSeconds (
            $script:CatalystProbeCycleBudgetSeconds +
            $script:CatalystProbeCleanupBudgetSeconds) `
            -ArtifactTailSeconds $context.ArtifactTailSeconds
        $productionRoot = Join-Path $context.OutputDirectory 'production-composite'
        $regressionRoot = Join-Path $productionRoot 'regression'
        $baselinePrimaryRoot = Join-Path $regressionRoot 'baseline'
        $baselineCompanionRoot = Join-Path $regressionRoot 'catalyst-baseline'
        $baselineDeadline = New-CatalystProbeFixedPhaseDeadline `
            -TaskDeadline $TaskDeadline `
            -BudgetSeconds $script:CatalystProbeCycleBudgetSeconds `
            -DownstreamReserveSeconds (
            $script:CatalystProbePatchApplyBudgetSeconds +
            $script:CatalystProbeCycleBudgetSeconds +
            $script:CatalystProbeCleanupBudgetSeconds +
            $script:CatalystProbeSummaryBudgetSeconds) `
            -Description 'baseline production composite'
        $context.ActiveDeadline = $baselineDeadline
        $context.ActiveReserveSeconds = 0
        Invoke-CatalystProbeProductionComposite `
            -Module $productionModule `
            -Selection $selection `
            -Requirement $requirement `
            -PrimaryOutputDirectory $baselinePrimaryRoot `
            -CompanionOutputDirectory $baselineCompanionRoot `
            -Deadline $baselineDeadline `
            -Context $context `
            -State baseline
        $baselinePrimary = Assert-CatalystProbePrimaryRun `
            -Path (Join-Path $baselinePrimaryRoot 'strict-test-evidence.json') `
            -Selection $selection
        $baselineCompanion = Assert-CatalystProbeCycleEvidence `
            -Kind baseline `
            -StrictEvidencePath (
            Join-Path $baselineCompanionRoot 'strict-test-evidence.json')
        $null = Assert-CatalystProbeProductionAssetsRetained `
            -Module $productionModule
        if (Test-Path -LiteralPath $context.FixtureTargetPath) {
            throw 'Production composite baseline left its transient Catalyst fixture behind.'
        }
        Assert-CatalystProbeRepositoryState -Context $context -State baseline
        $null = Get-CatalystProbeProcessTimeoutSeconds `
            -Deadline $baselineDeadline `
            -RequestedSeconds 1 `
            -Description 'baseline production composite evidence'
        $result.baseline = [ordered]@{
            primary = [ordered]@{
                strictEvidenceSha256 = [string]$baselinePrimary.Digest
                total = [int]$baselinePrimary.Document.total
                passed = [int]$baselinePrimary.Document.passed
                skipped = [int]$baselinePrimary.Document.skipped
                failed = [int]$baselinePrimary.Document.failed
                rawXml = @($baselinePrimary.Document.resultFiles |
                        ForEach-Object {
                            [ordered]@{
                                name = [string]$_.name
                                sha256 = [string]$_.sha256
                            }
                        })
            }
            companion = [ordered]@{
                strictEvidenceSha256 =
                [string]$baselineCompanion.StrictEvidenceSha256
                identities = @($baselineCompanion.Identities)
                total = [int]$baselineCompanion.Total
                passed = [int]$baselineCompanion.Passed
                skipped = [int]$baselineCompanion.Skipped
                failed = [int]$baselineCompanion.Failed
                rawXml = @($baselineCompanion.ResultFiles)
            }
        }

        $null = Assert-CatalystProbeTaskPhaseAdmission `
            -Deadline $TaskDeadline `
            -RequiredSeconds (
            $script:CatalystProbeCycleBudgetSeconds +
            $script:CatalystProbePatchApplyBudgetSeconds +
            $script:CatalystProbeCleanupBudgetSeconds +
            $script:CatalystProbeSummaryBudgetSeconds) `
            -Description 'fixed patch application and known-negative cycle'
        $patchDeadline = New-CatalystProbeFixedPhaseDeadline `
            -TaskDeadline $TaskDeadline `
            -BudgetSeconds $script:CatalystProbePatchApplyBudgetSeconds `
            -DownstreamReserveSeconds (
            $script:CatalystProbeCycleBudgetSeconds +
            $script:CatalystProbeCleanupBudgetSeconds +
            $script:CatalystProbeSummaryBudgetSeconds) `
            -Description 'fixed known-negative patch'
        $context.ActiveDeadline = $patchDeadline
        $context.ActiveReserveSeconds = 0
        Enable-CatalystProbeKnownNegative `
            -Context $context `
            -PatchPath $patchPath
        $null = Get-CatalystProbeProcessTimeoutSeconds `
            -Deadline $patchDeadline `
            -RequestedSeconds 1 `
            -Description 'fixed known-negative patch'
        $null = Assert-CatalystProbeTaskPhaseAdmission `
            -Deadline $TaskDeadline `
            -RequiredSeconds (
            $script:CatalystProbeCycleBudgetSeconds +
            $script:CatalystProbeCleanupBudgetSeconds +
            $script:CatalystProbeSummaryBudgetSeconds) `
            -Description 'known-negative cycle'
        $null = Assert-CatalystProbeDeadlineAdmission `
            -DeadlineUtc $context.JobDeadlineUtc `
            -Phase 'known-negative cycle' `
            -PhaseBudgetSeconds $script:CatalystProbeCycleBudgetSeconds `
            -RemainingPhaseBudgetSeconds $script:CatalystProbeCleanupBudgetSeconds `
            -ArtifactTailSeconds $context.ArtifactTailSeconds
        $negativePrimaryRoot = Join-Path $regressionRoot 'fix'
        $negativeCompanionRoot = Join-Path $regressionRoot 'catalyst-fix'
        $negativeDeadline = New-CatalystProbeFixedPhaseDeadline `
            -TaskDeadline $TaskDeadline `
            -BudgetSeconds $script:CatalystProbeCycleBudgetSeconds `
            -DownstreamReserveSeconds (
            $script:CatalystProbeCleanupBudgetSeconds +
            $script:CatalystProbeSummaryBudgetSeconds) `
            -Description 'known-negative production composite'
        $context.ActiveDeadline = $negativeDeadline
        $context.ActiveReserveSeconds = 0
        $productionRejection = $null
        try {
            Invoke-CatalystProbeProductionComposite `
                -Module $productionModule `
                -Selection $selection `
                -Requirement $requirement `
                -PrimaryOutputDirectory $negativePrimaryRoot `
                -CompanionOutputDirectory $negativeCompanionRoot `
                -Deadline $negativeDeadline `
                -Context $context `
                -State negative
            throw 'The production composite unexpectedly accepted the fixed known-negative postimage.'
        } catch {
            $productionRejection =
            Confirm-CatalystProbeExpectedProductionRejection -ErrorRecord $_
        }
        $negativePrimary = Assert-CatalystProbePrimaryRun `
            -Path (Join-Path $negativePrimaryRoot 'strict-test-evidence.json') `
            -Selection $selection
        $negativeCompanion = Assert-CatalystProbeCycleEvidence `
            -Kind negative `
            -StrictEvidencePath (
            Join-Path $negativeCompanionRoot 'strict-test-evidence.json')
        $null = Assert-CatalystProbeProductionAssetsRetained `
            -Module $productionModule
        if (Test-Path -LiteralPath $context.FixtureTargetPath) {
            throw 'Production composite negative run left its transient Catalyst fixture behind.'
        }
        Assert-CatalystProbeRepositoryState -Context $context -State negative
        $result.negative = [ordered]@{
            primary = [ordered]@{
                strictEvidenceSha256 = [string]$negativePrimary.Digest
                total = [int]$negativePrimary.Document.total
                passed = [int]$negativePrimary.Document.passed
                skipped = [int]$negativePrimary.Document.skipped
                failed = [int]$negativePrimary.Document.failed
                rawXml = @($negativePrimary.Document.resultFiles |
                        ForEach-Object {
                            [ordered]@{
                                name = [string]$_.name
                                sha256 = [string]$_.sha256
                            }
                        })
            }
            companion = [ordered]@{
                strictEvidenceSha256 =
                [string]$negativeCompanion.StrictEvidenceSha256
                identities = @($negativeCompanion.Identities)
                total = [int]$negativeCompanion.Total
                passed = [int]$negativeCompanion.Passed
                skipped = [int]$negativeCompanion.Skipped
                failed = [int]$negativeCompanion.Failed
                rawXml = @($negativeCompanion.ResultFiles)
                productionRejection = $productionRejection
            }
        }

        $baselineIds = @($result.baseline.companion.identities | ForEach-Object {
                "$($_.type)`n$($_.method)`n$($_.displayName)"
            })
        $negativeIds = @($result.negative.companion.identities | ForEach-Object {
                "$($_.type)`n$($_.method)`n$($_.displayName)"
            })
        if (($baselineIds -join "`n--identity--`n") -cne
            ($negativeIds -join "`n--identity--`n")) {
            throw 'Production Catalyst composite runs did not execute the same exact identities.'
        }

        $v2Rejection = $null
        try {
            $null = Invoke-CatalystProbeProductionV2Validation `
                -Module $productionModule `
                -Selection $selection `
                -Requirement $requirement `
                -EvidenceArtifactRoot $productionRoot `
                -PatchPath $patchPath `
                -TrustedFixturePath $context.TrustedFixturePath `
                -Deadline $negativeDeadline
            throw 'Production v2 evidence unexpectedly accepted the known-negative Catalyst result.'
        } catch {
            $v2Rejection =
            Confirm-CatalystProbeExpectedProductionRejection -ErrorRecord $_
        }
        $rejectedV2Path = Join-Path $regressionRoot 'regression-evidence.json'
        if (-not (Test-Path -LiteralPath $rejectedV2Path -PathType Leaf)) {
            throw 'Production v2 rejection did not retain its rejected evidence document.'
        }
        $result.expectedV2Rejection = [ordered]@{
            rejected = $true
            reason = $v2Rejection
            document = 'production-composite/regression/regression-evidence.json'
            documentSha256 =
            Get-CatalystProbeFileSha256 -Path $rejectedV2Path
            certifiable = $false
        }
        $null = Get-CatalystProbeProcessTimeoutSeconds `
            -Deadline $negativeDeadline `
            -RequestedSeconds 1 `
            -Description 'known-negative production composite evidence'
        $result.outcome =
        'production-composite-baseline-pass/known-negative-rejected'
        $result.reason = 'fixed-production-composite-rejection-demonstrated'
    } catch {
        $probeError = $_
        $result.outcome = 'inconclusive'
        $result.reason = 'probe-failed-closed'
        $result.failure = Get-CatalystProbeSafeError -Message $_.Exception.Message
    } finally {
        if ($context) {
            $cleanupFailures = [Collections.Generic.List[string]]::new()
            $cleanupDeadline = $null
            try {
                $cleanupDeadline = New-CatalystProbeFixedPhaseDeadline `
                    -TaskDeadline $TaskDeadline `
                    -BudgetSeconds $script:CatalystProbeCleanupBudgetSeconds `
                    -DownstreamReserveSeconds $script:CatalystProbeSummaryBudgetSeconds `
                    -Description 'production composite cleanup'
                $context.ActiveDeadline = $cleanupDeadline
                $context.ActiveReserveSeconds = 0
            } catch {
                $cleanupFailures.Add("cleanup admission: $($_.Exception.Message)")
            }
            if ($cleanupDeadline) {
                try {
                    $ownedSimulator = if (
                        $context.PSObject.Properties['OwnedIosSimulator']) {
                        $context.OwnedIosSimulator
                    } else {
                        $null
                    }
                    Remove-CatalystProbeOwnedIosSimulator `
                        -Context $context `
                        -Simulator $ownedSimulator `
                        -CleanupDeadline $cleanupDeadline
                    $result.cleanup.simulatorRemoved = $true
                } catch {
                    $cleanupFailures.Add(
                        "simulator cleanup: $($_.Exception.Message)")
                }
                try {
                    Restore-CatalystProbeRepository -Context $context
                } catch {
                    $cleanupFailures.Add(
                        "repository cleanup: $($_.Exception.Message)")
                }
                try {
                    Remove-CatalystProbeProductionCompositeModule `
                        -Module $productionModule `
                        -Context $context
                    $productionModule = $null
                } catch {
                    $cleanupFailures.Add(
                        "production module cleanup: $($_.Exception.Message)")
                }
                try {
                    $null = Get-CatalystProbeProcessTimeoutSeconds `
                        -Deadline $cleanupDeadline `
                        -RequestedSeconds 1 `
                        -Description 'production composite cleanup'
                } catch {
                    $cleanupFailures.Add(
                        "cleanup deadline: $($_.Exception.Message)")
                }
            }
            if ($cleanupFailures.Count -eq 0) {
                $result.cleanup.completed = $true
                $result.cleanup.fixtureRemoved =
                -not (Test-Path -LiteralPath $context.FixtureTargetPath)
                $result.cleanup.productRestored =
                (Get-CatalystProbeFileSha256 -Path $context.ProductPath) -ceq
                $script:CatalystProbeBaselineFileSha256
                $result.cleanup.repositoryStatusRestored = $true
                $result.cleanup.trustedTreeRestored = $true
            } else {
                $cleanupError = [Management.Automation.ErrorRecord]::new(
                    [InvalidOperationException]::new(
                        "Catalyst probe cleanup failed: $($cleanupFailures -join '; ')"),
                    'CatalystProbeCleanupFailed',
                    [Management.Automation.ErrorCategory]::InvalidOperation,
                    $context)
                $result.outcome = 'inconclusive'
                $result.reason = 'cleanup-failed-closed'
                $result.failure = Get-CatalystProbeSafeError `
                    -Message $cleanupError.Exception.Message
            }
        }
        $summaryDeadline = New-CatalystProbeFixedPhaseDeadline `
            -TaskDeadline $TaskDeadline `
            -BudgetSeconds $script:CatalystProbeSummaryBudgetSeconds `
            -DownstreamReserveSeconds 0 `
            -Description 'production composite summary'
        $result['startedUtc'] = $started.ToString('O')
        $result['completedUtc'] = [DateTimeOffset]::UtcNow.ToString('O')
        $result.budget.taskRemainingAtSummarySeconds =
        Get-CatalystProbeDeadlineRemainingSeconds -Deadline $TaskDeadline
        $resultPath = if ($context) {
            $context.ResultPath
        } else {
            Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) (
                'catalyst-gesture-probe.json')
        }
        if (-not (Test-Path -LiteralPath (Split-Path -Parent $resultPath))) {
            New-Item -ItemType Directory -Path (Split-Path -Parent $resultPath) `
                -Force | Out-Null
        }
        Write-CatalystProbeResult -Result $result -Path $resultPath
        try {
            $null = Get-CatalystProbeProcessTimeoutSeconds `
                -Deadline $summaryDeadline `
                -RequestedSeconds 1 `
                -Description 'production composite summary'
        } catch {
            $summaryError = $_
            $result.outcome = 'inconclusive'
            $result.reason = 'summary-deadline-exhausted'
            $result.failure = Get-CatalystProbeSafeError `
                -Message $_.Exception.Message
            Write-CatalystProbeResult -Result $result -Path $resultPath
        }
    }

    if ($summaryError) {
        throw $summaryError
    }
    if ($cleanupError) {
        throw $cleanupError
    }
    if ($probeError) {
        throw $probeError
    }
    if ($result.outcome -cne
        'production-composite-baseline-pass/known-negative-rejected') {
        throw 'Catalyst gesture probe did not demonstrate the production composite rejection.'
    }
    return [pscustomobject]$result
}

if ($MyInvocation.InvocationName -cne '.') {
    $taskDeadline = New-CatalystProbeTaskDeadline
    $sharedRoot = Join-Path $TrustedRoot 'scripts/shared'
    foreach ($helper in @(
            'Assert-TrustedTreeAttestation.ps1',
            'Assert-ReplicationExecutionEnvironment.ps1',
            'Assert-ReplicationTestGuard.ps1',
            'Assert-ReplicationCertificationBinding.ps1',
            'Assert-ReplicationAppleAppSandbox.ps1',
            'Replication-AppleCompanionPrewarm.ps1')) {
        . (Join-Path $sharedRoot $helper)
    }
    $null = Invoke-CatalystGestureRegressionProbeCore `
        -ExpectedSourceVersion $ExpectedSourceVersion `
        -RepositoryRoot $RepositoryRoot `
        -TrustedRoot $TrustedRoot `
        -TrustedTreeAttestation $TrustedTreeAttestation `
        -OutputDirectory $OutputDirectory `
        -TaskDeadline $taskDeadline
}
