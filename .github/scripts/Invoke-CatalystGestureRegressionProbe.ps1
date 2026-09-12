#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the fixed Mac Catalyst gesture A/B infrastructure probe.

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
$script:CatalystProbeProjectPath =
    'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
$script:CatalystProbeTargetFramework = 'net10.0-maccatalyst'
$script:CatalystProbeRuntimeIdentifier = 'maccatalyst-arm64'
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
$script:CatalystProbeForbiddenEnvironmentNames = @(
    'GH_TOKEN'
    'GITHUB_TOKEN'
    'COPILOT_GITHUB_TOKEN'
    'SYSTEM_ACCESSTOKEN'
    'AZURE_DEVOPS_EXT_PAT'
    'SYSTEMVSSCONNECTION'
    'ENDPOINT_AUTH_SYSTEMVSSCONNECTION'
)

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
        $combined = ConvertTo-CatalystProbeLogData -Text (
            "stdout:`n$stdout`nstderr:`n$stderr")
        [IO.File]::WriteAllText(
            [IO.Path]::GetFullPath($LogPath),
            $combined,
            [Text.UTF8Encoding]::new($false))
        return [pscustomobject]@{
            ExitCode = if ($timedOut) { 124 } else { $process.ExitCode }
            TimedOut = $timedOut
            RequestedTimeoutSeconds = $TimeoutSeconds
            EffectiveTimeoutSeconds = $effectiveTotalSeconds
            EffectiveProcessSeconds = $effectiveProcessSeconds
            StartedUtc = $started.ToString('O')
            CompletedUtc = [DateTimeOffset]::UtcNow.ToString('O')
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

    $fixtureExpected = $State -cne 'setup'
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

    $null = Assert-CatalystProbeHostBoundary
    $tfBuild = [Environment]::GetEnvironmentVariable('TF_BUILD')
    if ([string]::IsNullOrWhiteSpace($tfBuild) -or
        $tfBuild.ToLowerInvariant() -cne 'true') {
        throw 'The Catalyst gesture probe may execute only inside its fresh Azure job.'
    }
    foreach ($binding in @(
            @{ Environment = 'BUILD_SOURCESDIRECTORY'; Value = $RepositoryRoot },
            @{ Environment = 'CATALYST_PROBE_TRUSTED_ROOT'; Value = $TrustedRoot },
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

    $platformProperties = @(
        '-p:IncludeMacCatalystTargetFrameworks=true'
        '-p:IncludeIosTargetFrameworks=false'
        '-p:IncludeAndroidTargetFrameworks=false'
        '-p:IncludeWindowsTargetFrameworks=false'
        '-p:IncludeMacOSTargetFrameworks=false'
        '-p:IncludeTizenTargetFrameworks=false'
    )
    $projectPath = Join-Path $Context.RepositoryRoot $script:CatalystProbeProjectPath
    $packages = $Context.RuntimeEnvironment['NUGET_PACKAGES']
    $commands = @(
        @{
            Name = 'restore-catalyst-graph.log'
            Arguments = @('restore', $projectPath, '--packages', $packages) +
                $platformProperties
            Timeout = 420
        },
        @{
            Name = 'restore-catalyst-top-rid.log'
            Arguments = @(
                'restore', $projectPath, '--packages', $packages,
                '--no-dependencies', '-r', $script:CatalystProbeRuntimeIdentifier) +
                $platformProperties
            Timeout = 180
        }
    )
    Assert-CatalystProbeRepositoryState -Context $Context -State setup
    try {
        foreach ($command in $commands) {
            Assert-CatalystProbeTrustedTree -Context $Context
            $result = Invoke-CatalystProbeBoundedProcess `
                -FileName 'dotnet' `
                -ArgumentList $command.Arguments `
                -WorkingDirectory $Context.RepositoryRoot `
                -Environment $Context.RuntimeEnvironment `
                -TimeoutSeconds $command.Timeout `
                -TaskDeadline $Context.ActiveDeadline `
                -ReserveSeconds $Context.ActiveReserveSeconds `
                -LogPath (Join-Path $Context.LogDirectory $command.Name)
            Assert-CatalystProbeTrustedTree -Context $Context
            if ($result.TimedOut -or $result.ExitCode -ne 0) {
                throw "Catalyst trusted prewarm failed; see $($command.Name)."
            }
        }
    } finally {
        Restore-CatalystProbeTrackedVerificationSideEffects `
            -Context $Context `
            -State setup
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
    $expectedStatus = " M $($script:CatalystProbeProductPath)`n?? $($script:CatalystProbeFixtureTargetRelativePath)"
    $expectedReverse = "?? $($script:CatalystProbeFixtureTargetRelativePath)`n M $($script:CatalystProbeProductPath)"
    if ($status -cne $expectedStatus -and $status -cne $expectedReverse) {
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

function New-CatalystProbeResult {
    param(
        [Parameter(Mandatory = $true)][string]$ExpectedSourceVersion,
        [Parameter(Mandatory = $true)][string]$TrustedTreeHash,
        [Parameter(Mandatory = $true)][string]$TrustedTreeAttestationSha256
    )

    return [ordered]@{
        schemaVersion = 1
        mode = 'catalyst-gesture-probe'
        reportOnly = $true
        outcome = 'inconclusive'
        reason = 'not-started'
        sourceVersion = $ExpectedSourceVersion
        trustedTreeHash = $TrustedTreeHash
        trustedTreeAttestationSha256 = $TrustedTreeAttestationSha256
        fixedInputs = [ordered]@{
            repository = 'https://github.com/dotnet/maui.git'
            baselineCommit = $script:CatalystProbeBaselineCommit
            negativeCommit = $script:CatalystProbeNegativeCommit
            productPath = $script:CatalystProbeProductPath
            baselineBlob = $script:CatalystProbeBaselineBlob
            negativeBlob = $script:CatalystProbeNegativeBlob
            baselineFileSha256 = $script:CatalystProbeBaselineFileSha256
            negativeFileSha256 = $script:CatalystProbeNegativeFileSha256
            productPatchSha256 = $script:CatalystProbePatchSha256
            fixtureSha256 = $script:CatalystProbeFixtureSha256
        }
        selector = [ordered]@{
            platform = 'catalyst'
            project = 'Controls'
            category = 'Gesture'
            class = $script:CatalystProbeClass
            methods = @($script:CatalystProbeMethods)
            expectedNativeTapCounts = 'public native UI Tap: 0 -> 1; NumberRequired: 2'
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
        baseline = $null
        negative = $null
        cleanup = [ordered]@{
            completed = $false
            fixtureRemoved = $false
            productRestored = $false
            repositoryStatusRestored = $false
            trustedTreeRestored = $false
        }
        assertions = [ordered]@{
            baseline = 'exact-two-pass'
            negative = 'exact-two-expected-Assert.Single-empty-failures'
        }
        exclusions = [ordered]@{
            certifiesIssue = $false
            certifiesProduct = $false
            publishesOutcome = $false
            mutatesPullRequest = $false
            invokesModel = $false
            activatesProductionCompanions = $false
            executesIssueLinkedCode = $false
        }
        profileBoundary = [ordered]@{
            requiresFreshAzureJob = $true
            initialContainerResultRequiredAbsent = $true
            deletesPreexistingContainerResult = $false
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

    $context = $null
    $probeError = $null
    $cleanupError = $null
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
        $result.budget.jobDeadlineUtc = $context.JobDeadlineUtc
        $null = Assert-CatalystProbeDeadlineAdmission `
            -DeadlineUtc $context.JobDeadlineUtc `
            -Phase 'complete report-only probe' `
            -PhaseBudgetSeconds $script:CatalystProbeOverallBudgetSeconds `
            -ArtifactTailSeconds $context.ArtifactTailSeconds
        $patchPath = New-CatalystProbeFixedPatch -Context $context
        Assert-CatalystProbeFixedPatchPolicy `
            -Context $context `
            -PatchPath $patchPath
        $null = Assert-CatalystProbeTaskPhaseAdmission `
            -Deadline $TaskDeadline `
            -RequiredSeconds $minimumRemainingWorkSeconds `
            -Description 'prewarm and both native cycles'
        Invoke-CatalystProbeTrustedRestore -Context $context
        Copy-CatalystProbeFixture -Context $context

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
        $result.baseline = Invoke-CatalystProbeCycle `
            -Kind 'baseline' `
            -Context $context
        $result.baseline | Add-Member -NotePropertyName containerResultSha256 `
            -NotePropertyValue (
                Assert-CatalystProbeOwnedContainerResult `
                    -Context $context `
                    -Cycle $result.baseline)

        $null = Assert-CatalystProbeTaskPhaseAdmission `
            -Deadline $TaskDeadline `
            -RequiredSeconds (
                $script:CatalystProbeCycleBudgetSeconds +
                $script:CatalystProbePatchApplyBudgetSeconds +
                $script:CatalystProbeCleanupBudgetSeconds +
                $script:CatalystProbeSummaryBudgetSeconds) `
            -Description 'fixed patch application and known-negative cycle'
        Enable-CatalystProbeKnownNegative `
            -Context $context `
            -PatchPath $patchPath
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
        $result.negative = Invoke-CatalystProbeCycle `
            -Kind 'negative' `
            -Context $context
        $result.negative | Add-Member -NotePropertyName containerResultSha256 `
            -NotePropertyValue (
                Assert-CatalystProbeOwnedContainerResult `
                    -Context $context `
                    -Cycle $result.negative)

        $baselineIds = @($result.baseline.identities | ForEach-Object {
            "$($_.type)`n$($_.method)`n$($_.displayName)"
        })
        $negativeIds = @($result.negative.identities | ForEach-Object {
            "$($_.type)`n$($_.method)`n$($_.displayName)"
        })
        if (($baselineIds -join "`n--identity--`n") -cne
            ($negativeIds -join "`n--identity--`n")) {
            throw 'Catalyst A/B cycles did not execute the same exact test identities.'
        }
        $result.outcome = 'baseline-pass/negative-regression-demonstrated'
        $result.reason = 'fixed-native-tap-regression-reproduced'
    } catch {
        $probeError = $_
        $result.outcome = 'inconclusive'
        $result.reason = 'probe-failed-closed'
        $result.failure = Get-CatalystProbeSafeError -Message $_.Exception.Message
    } finally {
        if ($context) {
            try {
                Restore-CatalystProbeRepository -Context $context
                $result.cleanup.completed = $true
                $result.cleanup.fixtureRemoved =
                    -not (Test-Path -LiteralPath $context.FixtureTargetPath)
                $result.cleanup.productRestored =
                    (Get-CatalystProbeFileSha256 -Path $context.ProductPath) -ceq
                        $script:CatalystProbeBaselineFileSha256
                $result.cleanup.repositoryStatusRestored = $true
                $result.cleanup.trustedTreeRestored = $true
            } catch {
                $cleanupError = $_
                $result.outcome = 'inconclusive'
                $result.reason = 'cleanup-failed-closed'
                $result.failure = Get-CatalystProbeSafeError `
                    -Message $_.Exception.Message
            }
        }
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
    }

    if ($cleanupError) {
        throw $cleanupError
    }
    if ($probeError) {
        throw $probeError
    }
    if ($result.outcome -cne
        'baseline-pass/negative-regression-demonstrated') {
        throw 'Catalyst gesture A/B probe did not demonstrate the fixed negative control.'
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
            'Assert-ReplicationAppleAppSandbox.ps1')) {
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
