#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the trusted failure-only verifier for an issue replication candidate.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$IssueNumber,

    [Parameter(Mandatory = $true)]
    [ValidateSet('android', 'ios', 'catalyst', 'windows')]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [ValidateSet('UITest', 'UnitTest', 'XamlUnitTest', 'DeviceTest')]
    [string]$TestType,

    [Parameter(Mandatory = $true)]
    [ValidateLength(1, 500)]
    [string]$TestFilter,

    [string]$TestProject = '',

    [string]$TestProjectPath = '',

    [Parameter(Mandatory = $true)]
    [ValidateLength(1, 500)]
    [string]$TestClass,

    [Parameter(Mandatory = $true)]
    [ValidateLength(1, 500)]
    [string]$TestMethod,

    [Parameter(Mandatory = $true)]
    [ValidateLength(3, 1000)]
    [string]$ExpectedFailureSignature,

    [Parameter(Mandatory = $true)]
    [string]$VerifierPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    # Reviewers reject a reproduction proved by a single execution, so the same
    # targeted test is run repeatedly and every run has to fail identically.
    [ValidateRange(1, 3)]
    [int]$RunCount = 1,

    # Runs the same targeted test as a negative control, where the reported
    # trigger has been removed and the unchanged oracle is therefore expected to
    # pass. A red test only shows that something is wrong; the control is what
    # shows the red depends on the reported behaviour rather than on something
    # incidental to the scenario.
    [switch]$ExpectPass
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

. (Join-Path $PSScriptRoot 'Get-ReplicationSignatureMatch.ps1')

function Get-ReplicationControlPassMarker {
    <#
        .SYNOPSIS
        The verifier banner that reports a targeted test which ran and passed.

        .DESCRIPTION
        The failure-only verifier has no success path for a passing test: it
        reports one as a rejection. A negative control wants exactly that
        rejection, so the two read the same banner rather than each carrying its
        own copy, which is how the tier-escalation detector previously drifted
        away from the producer and stopped firing.
    #>
    return 'test(s) PASSED but should FAIL'
}

function Test-ReplicationExpectedFailureSignature {
    param(
        [AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Signature
    )

    if ([string]::IsNullOrWhiteSpace($Content) -or [string]::IsNullOrWhiteSpace($Signature)) {
        return $false
    }

    if ($Content.Contains($Signature, [StringComparison]::Ordinal)) {
        return $true
    }

    # Verifier output wraps and re-indents long assertion messages, so an exact
    # ordinal match rejects signatures that describe the very same failure.
    $normalizedContent = ConvertTo-NormalizedReplicationSignature -Value $Content
    $normalizedSignature = ConvertTo-NormalizedReplicationSignature -Value $Signature
    if ([string]::IsNullOrWhiteSpace($normalizedSignature)) {
        return $false
    }

    return $normalizedContent.Contains($normalizedSignature, [StringComparison]::OrdinalIgnoreCase)
}

function ConvertTo-NormalizedReplicationSignature {
    param([AllowEmptyString()][string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ''
    }

    return ([regex]::Replace($Value, '\s+', ' ')).Trim()
}

function Test-ReplicationInfrastructureFailure {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return $true
    }

    $patterns = @(
        # The verification harness states its own verdict. When it says it could
        # not run the test, believe that instead of re-deriving it from symptoms:
        # a run whose Appium session never started was reported as a genuine
        # verification failure, which blames a test that was never executed.
        '(?im)VERIFICATION INCONCLUSIVE',
        '(?im)\bENV ERROR\b',
        '(?im)\bEnvironment error persisted after\b',
        '(?im)^\s*Build FAILED\.?\s*$',
        '(?im)\berror (?:CS|MSB|NETSDK|XLS|APT|XA)\d{3,}\b',
        '(?im)\bNo test matches\b',
        '(?im)\bHandlerNotFoundException\b|\bUnable to find an? IElementHandler\b',
        '(?im)\b(?:test run|operation|command|task) timed out\b',
        '(?im)\b(?:snapshot|baseline).{0,80}\b(?:missing|not found|does not exist)\b',
        '(?im)\b(?:device|simulator|emulator).{0,80}\b(?:offline|unavailable|failed to boot)\b'
    )

    foreach ($pattern in $patterns) {
        if ($Content -match $pattern) {
            return $true
        }
    }
    return $false
}

function ConvertTo-AzdoSafeReplicationOutput {
    param([AllowEmptyString()][string]$Value)

    if ($null -eq $Value) {
        return ''
    }

    return $Value -replace '##vso\[[^\]]*\]', '' -replace '##\[[^\]]*\]', ''
}

function ConvertTo-BoundedVerificationFailureMessage {
    param(
        [AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Signature,
        [ValidateRange(1024, 4096)][int]$MaximumLength = 4096
    )

    if ($Content.Length -le $MaximumLength) {
        return $Content
    }

    $marker = "$([Environment]::NewLine)... verifier output truncated ...$([Environment]::NewLine)"
    $headLength = [Math]::Min(2800, $MaximumLength - $marker.Length)
    $head = $Content.Substring(0, $headLength)
    $remaining = $MaximumLength - $head.Length - $marker.Length

    if (
        $head.Contains($Signature, [StringComparison]::Ordinal) -or
        $remaining -le 0
    ) {
        return $head + $marker
    }

    $signatureIndex = $Content.IndexOf($Signature, [StringComparison]::Ordinal)
    if ($signatureIndex -lt 0) {
        return $Content.Substring(0, $MaximumLength - $marker.Length) + $marker
    }

    $tailLength = [Math]::Min($remaining, $Content.Length - $signatureIndex)
    return $head + $marker + $Content.Substring($signatureIndex, $tailLength)
}

if (-not (Test-Path -LiteralPath $VerifierPath -PathType Leaf)) {
    throw "Trusted failure-only verifier was not found: $VerifierPath"
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$resultPath = Join-Path $OutputDirectory 'verification-result.json'
$machineResultPath = Join-Path $OutputDirectory 'verifier-machine-result.json'
$repositoryRoot = git rev-parse --show-toplevel
$verificationRoot = Join-Path $repositoryRoot "CustomAgentLogsTmp/PRState/$IssueNumber/PRAgent/gate/verify-tests-fail"
$secretNames = @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')

function Invoke-SingleVerificationRun {
    param(
        [Parameter(Mandatory = $true)][int]$Run,
        [Parameter(Mandatory = $true)][string]$ConsoleLog
    )

    $savedSecrets = @{}
    try {
        foreach ($name in $secretNames) {
            $savedSecrets[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        $arguments = @(
            '-NoProfile',
            '-File', $VerifierPath,
            '-Platform', $Platform,
            '-TestType', $TestType,
            '-TestFilter', $TestFilter,
            '-TestClass', $TestClass,
            '-TestMethod', $TestMethod,
            '-MachineResultPath', $machineResultPath,
            '-PRNumber', [string]$IssueNumber,
            # Tell the verifier what it is being asked to prove. Without this it
            # prints "VERIFICATION PASSED / This proves the tests correctly
            # reproduce the bug" during a negative control, where a failing test
            # means the exact opposite. The decision logic never read that banner,
            # but a human reading the artifact did.
            '-Purpose', $(if ($ExpectPass) { 'NegativeControl' } else { 'Reproduction' })
        )
        if (-not [string]::IsNullOrWhiteSpace($TestProject)) {
            $arguments += @('-TestProject', $TestProject)
        }
        if (-not [string]::IsNullOrWhiteSpace($TestProjectPath)) {
            $arguments += @('-TestProjectPath', $TestProjectPath)
        }
        if ($TestType -eq 'DeviceTest') {
            $trustedSkillsRoot = Split-Path -Parent (
                Split-Path -Parent (
                    Split-Path -Parent $VerifierPath))
            $deviceTestScriptPath = Join-Path `
                $trustedSkillsRoot `
                'run-device-tests/scripts/Run-DeviceTests.ps1'
            if (-not (Test-Path -LiteralPath $deviceTestScriptPath -PathType Leaf)) {
                throw "Trusted device-test runner was not found: $deviceTestScriptPath"
            }
            $arguments += @('-DeviceTestScriptPath', $deviceTestScriptPath)
        }

        $output = @(& pwsh @arguments 2>&1)
        $exitCode = $LASTEXITCODE
        $outputText = ($output | ForEach-Object { [string]$_ }) -join [Environment]::NewLine
        $outputText | Set-Content -LiteralPath $ConsoleLog -Encoding utf8NoBOM
        Write-Host ("--- targeted test execution $Run of $RunCount ---")
        Write-Host (ConvertTo-AzdoSafeReplicationOutput -Value $outputText)
    }
    finally {
        foreach ($name in $secretNames) {
            [Environment]::SetEnvironmentVariable($name, $savedSecrets[$name])
        }
    }

    $runLogs = @($ConsoleLog)
    if (Test-Path -LiteralPath $verificationRoot) {
        $runLogs += @(Get-ChildItem -LiteralPath $verificationRoot -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -in @('verification-log.txt', 'test-without-fix.log', 'verification-report.md') } |
            ForEach-Object FullName)
    }

    $combined = ($runLogs | Sort-Object -Unique | ForEach-Object {
        if (Test-Path -LiteralPath $_ -PathType Leaf) {
            Get-Content -LiteralPath $_ -Raw -ErrorAction SilentlyContinue
        }
    }) -join [Environment]::NewLine

    $verifierPassed = $exitCode -eq 0 -and $combined -match 'VERIFICATION PASSED'
    $actualFailureMessage = ''
    $executedTestCount = -1
    $retainedResultFile = ''
    $expectedMachineFilter = if ($TestType -in @('UnitTest', 'XamlUnitTest')) {
        "FullyQualifiedName=$TestClass.$TestMethod"
    } else {
        $TestFilter
    }
    if (Test-Path -LiteralPath $machineResultPath -PathType Leaf) {
        try {
            $machineResultFile = Get-Item -LiteralPath $machineResultPath
            if ($machineResultFile.Length -gt 64KB) {
                throw 'Verifier machine result exceeds the trusted size limit.'
            }
            $machineResult = Get-Content -LiteralPath $machineResultPath -Raw |
                ConvertFrom-Json -ErrorAction Stop
            if (
                [int]$machineResult.schemaVersion -eq 1 -and
                $machineResult.failed -eq $true -and
                ([string]$machineResult.testType) -ceq $TestType -and
                ([string]$machineResult.testFilter) -ceq $expectedMachineFilter -and
                ([string]$machineResult.testProject) -ceq $TestProject -and
                ([string]$machineResult.testProjectPath) -ceq $TestProjectPath -and
                ([string]$machineResult.testClass) -ceq $TestClass -and
                ([string]$machineResult.testMethod) -ceq $TestMethod -and
                ([string]$machineResult.actualFailureMessage).Length -le 10000
            ) {
                $actualFailureMessage = [string]$machineResult.actualFailureMessage
                # Strict mode turns a missing property into a terminating error,
                # and the catch below would blank the failure message, so probe
                # for the optional count instead of dereferencing it.
                $countProperty = $machineResult.PSObject.Properties['executedTestCount']
                if ($countProperty -and $null -ne $countProperty.Value) {
                    $parsedCount = 0
                    if ([int]::TryParse(
                            [string]$countProperty.Value,
                            [ref]$parsedCount)) {
                        $executedTestCount = $parsedCount
                    }
                }
                $resultFileProperty = $machineResult.PSObject.Properties['resultFile']
                if ($resultFileProperty -and $resultFileProperty.Value) {
                    $retainedResultFile = [string]$resultFileProperty.Value
                }
            }
        } catch {
            $actualFailureMessage = ''
        } finally {
            # This intermediate file lives in the directory the credential-free
            # gate inspects, and that gate rejects any artifact it does not
            # expect. Removing it only on the success path would let a failed
            # run leave behind a file that destroys the next candidate.
            Remove-Item -LiteralPath $machineResultPath -Force -ErrorAction SilentlyContinue
        }
    }

    $signatureMatched = Test-ReplicationExpectedFailureSignature `
        -Content $actualFailureMessage `
        -Signature $ExpectedFailureSignature
    $signatureEquivalent = $signatureMatched
    if (-not $signatureMatched -and $verifierPassed) {
        $signatureEquivalent = Test-ReplicationSignatureEquivalent `
            -Declared $ExpectedFailureSignature `
            -Observed $actualFailureMessage
        if ($signatureEquivalent) {
            Write-Host ('The targeted test failed with the predicted defect but ' +
                'different wording, so the observed assertion is authoritative.')
        }
    }
    $infrastructureFailure = Test-ReplicationInfrastructureFailure -Content $combined

    # The evidence claim is "this one test fails". A contains-style filter can
    # select several tests, and a run where two tests executed cannot tell us
    # which one produced the failure, so the selection is not proof of anything.
    $selectionAmbiguous = $executedTestCount -gt 1
    if ($selectionAmbiguous) {
        Write-Host ("The targeted filter selected $executedTestCount tests, so the " +
            'failure cannot be attributed to the named test. Narrow the filter to ' +
            'select exactly one test.')
    }

    # A control run is only informative when the test actually executed and
    # reported a pass. An empty log, a crashed runner or a build break also
    # "did not fail", and treating those as a passing control would certify the
    # very reproductions that never ran.
    $testPassed = (-not $infrastructureFailure) -and
        $combined.Contains((Get-ReplicationControlPassMarker), [StringComparison]::Ordinal)

    return [pscustomobject]@{
        Run = $Run
        ExitCode = $exitCode
        VerifierPassed = $verifierPassed
        SignatureMatched = $signatureMatched
        SignatureEquivalent = $signatureEquivalent
        InfrastructureFailure = $infrastructureFailure
        SelectionAmbiguous = $selectionAmbiguous
        ExecutedTestCount = $executedTestCount
        RetainedResultFile = $retainedResultFile
        TestPassed = $testPassed
        ActualFailureMessage = $actualFailureMessage
        Passed = $verifierPassed -and $signatureEquivalent -and
            -not $infrastructureFailure -and -not $selectionAmbiguous
        LogFiles = @($runLogs | Sort-Object -Unique)
    }
}

function Get-ReplicationVolatileFreeMessage {
    <#
        .SYNOPSIS
        Removes the parts of a failure message that differ between identical runs.

        .DESCRIPTION
        Durations, hex object addresses, process and thread identifiers, and
        absolute paths change on every execution without saying anything about
        the defect, so comparing raw messages would call every reproduction
        unstable. Everything else, including the measured values the assertion
        reports, is the evidence being compared.
    #>
    param([AllowEmptyString()][string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ''
    }

    $text = $Value
    $text = [regex]::Replace($text, '(?i)0x[0-9a-f]{4,}', '<address>')
    $text = [regex]::Replace($text, '(?i)\b\d+(\.\d+)?\s*(ms|milliseconds|s|seconds)\b', '<duration>')
    $text = [regex]::Replace($text, '(?i)\b(emulator|device|udid|pid|tid)[-_ ]?[0-9a-f-]{2,}\b', '<device>')
    $text = [regex]::Replace($text, '(?i)[a-z]:\\[^\s"'']+|/(?:Users|home|private|var|tmp)/[^\s"'']+', '<path>')
    $text = [regex]::Replace($text, '(?i)\b\d{4}-\d{2}-\d{2}[t ]\d{2}:\d{2}:\d{2}\S*', '<timestamp>')

    return ConvertTo-NormalizedReplicationSignature -Value $text
}

function Test-ReplicationFailureMessagesAreStable {
    <#
        .SYNOPSIS
        Requires every run to fail with the same message, not merely a similar one.

        .DESCRIPTION
        Matching the declared signature only proves each run failed for the
        predicted reason somewhere. A reproduction whose reported values move
        between runs is measuring something noisy, and reviewers rejected that
        class repeatedly: a red that varies cannot be attributed confidently to
        the reported defect. Requiring one identical message across independent
        executions is what makes the evidence deterministic.
    #>
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        [AllowEmptyCollection()]
        [object[]]$Outcomes
    )

    if ($null -eq $Outcomes -or $Outcomes.Count -le 1) {
        return $true
    }

    $distinct = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($outcome in $Outcomes) {
        [void]$distinct.Add(
            (Get-ReplicationVolatileFreeMessage -Value ([string]$outcome.ActualFailureMessage)))
    }

    return $distinct.Count -eq 1
}

function Test-ReplicationControlFailureModeChanged {
    <#
        .SYNOPSIS
        Reports a negative control that failed for a reason the reproduction
        never observed.

        .DESCRIPTION
        A control only refutes a reproduction when it stays red for the *same*
        reason. If the edit meant to remove the trigger also removed the element
        the oracle locates, the control fails for an unrelated cause and says
        nothing about attribution.

        Reporting that as a refutation is the most expensive mistake available
        here, because it discards a reproduction whose device work and evidence
        are already paid for. When either side is unknown the answer is 'not
        changed', so a missing measurement never manufactures a verdict.
    #>
    param(
        [Parameter(Mandatory)][AllowNull()][AllowEmptyCollection()][AllowEmptyString()]
        [string[]]$ControlMessages,
        [Parameter(Mandatory)][AllowNull()][AllowEmptyCollection()][AllowEmptyString()]
        [string[]]$ReproductionMessages
    )

    $control = @(@($ControlMessages) | Where-Object { $_ })
    $reproduction = @(@($ReproductionMessages) | Where-Object { $_ })
    if ($control.Count -eq 0 -or $reproduction.Count -eq 0) {
        return $false
    }

    $shared = @($control | Where-Object { $reproduction -contains $_ })
    return $shared.Count -eq 0
}

$runOutcomes = New-Object 'System.Collections.Generic.List[object]'
for ($run = 1; $run -le $RunCount; $run++) {
    # The control shares the reproduction's output directory so the gate can
    # compare them, so it must never reuse the reproduction's console names or
    # it would overwrite the very evidence it is meant to corroborate.
    $consolePrefix = if ($ExpectPass) { 'negative-control-console' } else { 'verification-console' }
    $consoleLog = if ($run -eq 1) {
        Join-Path $OutputDirectory "$consolePrefix.log"
    } else {
        Join-Path $OutputDirectory "$consolePrefix-run-$run.log"
    }
    $outcome = Invoke-SingleVerificationRun -Run $run -ConsoleLog $consoleLog
    $runOutcomes.Add($outcome)
    $runSucceeded = if ($ExpectPass) { $outcome.TestPassed } else { $outcome.Passed }
    if (-not $runSucceeded -and -not $ExpectPass) {
        # Repeating a run that already failed only wastes device time; the
        # orchestrator repairs the test and verifies again from scratch.
        break
    }
    if (-not $runSucceeded) {
        # A red control is the one result this loop must NOT stop on. Stopping
        # here leaves a single sample, and that single sample is then used to
        # refute the reproduction, which is the most destructive verdict the
        # pipeline can reach: the device work and the recorded evidence are
        # already spent. A verdict that expensive has to be measured across
        # every requested run, so the control keeps going and lets the grading
        # below tell a repeatedly red control apart from a flaky one.
        continue
    }
}

if ($ExpectPass) {
    # The control shares the reproduction's oracle, so it must never be graded
    # on the expected failure signature: it is expected to produce no failure at
    # all. Report only how many control runs completed and how many observed the
    # test pass, and let the credential-free gate decide what that proves.
    $controlRuns = $runOutcomes.Count
    $controlPasses = @($runOutcomes | Where-Object { $_.TestPassed }).Count
    $controlInfrastructureFailure = @($runOutcomes |
        Where-Object { $_.InfrastructureFailure }).Count -gt 0

    $controlFailureMessages = @($runOutcomes |
        Where-Object { -not $_.TestPassed } |
        ForEach-Object { Get-ReplicationVolatileFreeMessage -Value ([string]$_.ActualFailureMessage) } |
        Where-Object { $_ } |
        Sort-Object -Unique)

    # A control that stays red only refutes the reproduction when it stays red
    # for the *same* reason. If removing the trigger also removed the element
    # the oracle looks for, or broke the scene some other way, the run failed
    # for a reason the reproduction never observed and says nothing about
    # attribution. Treating that as a refutation silently discards sound
    # reproductions, so the two cases are separated here.
    $reproductionMessages = @()
    $reproductionResultPath = Join-Path $OutputDirectory 'verification-result.json'
    if (Test-Path -LiteralPath $reproductionResultPath -PathType Leaf) {
        try {
            $reproductionResult = Get-Content -LiteralPath $reproductionResultPath -Raw |
                ConvertFrom-Json
            $reproductionMessages = @($reproductionResult.observedFailureMessages |
                Where-Object { $_ } |
                ForEach-Object { [string]$_ })
        } catch {
            $reproductionMessages = @()
        }
    }

    $failureModeChanged = Test-ReplicationControlFailureModeChanged `
        -ControlMessages $controlFailureMessages `
        -ReproductionMessages $reproductionMessages

    $controlResult = [ordered]@{
        schemaVersion = 1
        issueNumber = $IssueNumber
        platform = $Platform
        testType = $TestType
        testFilter = $TestFilter
        testClass = $TestClass
        testMethod = $TestMethod
        requestedRunCount = $RunCount
        runCount = $controlRuns
        passCount = $controlPasses
        infrastructureFailure = $controlInfrastructureFailure
        observedFailureMessages = @($controlFailureMessages)
        reproductionFailureMessages = @($reproductionMessages)
        failureModeChanged = $failureModeChanged
        logFiles = @($runOutcomes | ForEach-Object { $_.LogFiles } | Sort-Object -Unique)
    }
    $controlPath = Join-Path $OutputDirectory 'negative-control-result.json'
    $controlResult | ConvertTo-Json -Depth 10 |
        Set-Content -LiteralPath $controlPath -Encoding utf8NoBOM

    Write-Host ("Negative control observed the test pass in $controlPasses of " +
        "$controlRuns run(s); requested $RunCount.")

    if ($controlInfrastructureFailure) {
        throw ('The negative control failed for build or infrastructure reasons, so it cannot ' +
            'show that removing the reported trigger turns the test green.')
    }
    if ($failureModeChanged) {
        Write-Host ('Negative control failure mode: ' +
            ($controlFailureMessages -join ' | '))
        Write-Host ('Reproduction failure mode: ' + ($reproductionMessages -join ' | '))
        throw ('The negative control changed the failure mode instead of removing the trigger: ' +
            'it failed for a reason the reproduction never observed, so it cannot show whether ' +
            'the reproduction depends on the reported trigger.')
    }
    if ($controlRuns -ne $RunCount) {
        # Unreachable while the loop above runs every control run, and kept
        # deliberately: it is the net under that loop. If an early exit is ever
        # reintroduced, this turns the failure into a loud unmeasured verdict
        # instead of a silent refutation drawn from one sample, which is how 43
        # sound reproductions were destroyed in a single wave.
        throw ("The negative control completed only $controlRuns of $RunCount run(s), so how the " +
            'test behaves without the reported trigger was never measured. An unfinished control ' +
            'cannot show whether the reproduction depends on that trigger.')
    }
    if ($controlPasses -gt 0 -and $controlPasses -lt $RunCount) {
        throw ("The negative control is inconsistent: it passed in $controlPasses of " +
            "$controlRuns run(s). A control that changes its mind between identical runs " +
            'measures flakiness rather than attribution, so it can neither confirm nor ' +
            'refute the reproduction.')
    }
    if ($controlPasses -ne $RunCount) {
        if ($controlFailureMessages.Count -eq 0 -or $reproductionMessages.Count -eq 0) {
            # A control only refutes when it stayed red for the *same* reason,
            # and that comparison needs both sides. With either side missing -
            # an unsupported runner, an unparsed result document - the reason is
            # unknown, and the failure-mode check answers 'not changed' for a
            # comparison it could not make, which lands here. Refuting on that
            # is an absent measurement destroying a paid-for reproduction, so it
            # is reported as no comparable failure message instead.
            throw ("The negative control stayed red in all $controlRuns run(s), but " +
                'no comparable failure message was recorded on both sides, so whether it ' +
                'failed for the same reason as the reproduction was never measured. An ' +
                'unattributable control can neither confirm nor refute the reproduction.')
        }
        throw ("The negative control was expected to pass in all $RunCount run(s) but passed in " +
            "$controlPasses of $controlRuns. The reproduction's failure therefore does not depend " +
            'on the reported trigger alone.')
    }
    return
}

$firstOutcome = $runOutcomes[0]
$completedRuns = $runOutcomes.Count
$failedOutcomes = @($runOutcomes | Where-Object { -not $_.Passed })
$consistentRuns = $completedRuns -eq $RunCount -and $failedOutcomes.Count -eq 0
$verifierPassed = @($runOutcomes | Where-Object { -not $_.VerifierPassed }).Count -eq 0
$signatureMatched = @($runOutcomes | Where-Object { -not $_.SignatureMatched }).Count -eq 0
$signatureEquivalent = @($runOutcomes | Where-Object { -not $_.SignatureEquivalent }).Count -eq 0
$infrastructureFailure = @($runOutcomes | Where-Object { $_.InfrastructureFailure }).Count -gt 0
$selectionAmbiguous = @($runOutcomes | Where-Object { $_.SelectionAmbiguous }).Count -gt 0
$executedTestCounts = @($runOutcomes |
    Where-Object { $_.ExecutedTestCount -ge 0 } |
    ForEach-Object { [int]$_.ExecutedTestCount } |
    Sort-Object -Unique)
$retainedResultFiles = @($runOutcomes |
    ForEach-Object { [string]$_.RetainedResultFile } |
    Where-Object { $_ } |
    Sort-Object -Unique)
$nonZeroExitCodes = @($runOutcomes | Where-Object { $_.ExitCode -ne 0 })
$exitCode = if ($nonZeroExitCodes.Count -gt 0) { [int]$nonZeroExitCodes[0].ExitCode } else { 0 }
$actualFailureMessage = [string]$firstOutcome.ActualFailureMessage
$candidateLogs = @($runOutcomes | ForEach-Object { $_.LogFiles } | Sort-Object -Unique)

$boundedFailureMessage = ConvertTo-BoundedVerificationFailureMessage `
    -Content $actualFailureMessage `
    -Signature $ExpectedFailureSignature
$stableFailureMessage = Test-ReplicationFailureMessagesAreStable -Outcomes $runOutcomes.ToArray()
$verificationPassed = $verifierPassed -and
    $signatureEquivalent -and
    -not $infrastructureFailure -and
    -not $selectionAmbiguous -and
    $consistentRuns -and
    $stableFailureMessage

# The declared signature is the agent's prediction; the observed message came
# from a validated machine-readable verifier result. Publish what actually
# happened whenever the two differ but describe the same defect.
$effectiveFailureSignature = if ($signatureMatched) {
    $ExpectedFailureSignature
} else {
    ConvertTo-NormalizedReplicationSignature -Value $actualFailureMessage
}
if ($effectiveFailureSignature.Length -gt 400) {
    $effectiveFailureSignature = $effectiveFailureSignature.Substring(0, 400)
}

$result = [ordered]@{
    schemaVersion = 1
    issueNumber = $IssueNumber
    platform = $Platform
    testType = $TestType
    testFilter = $TestFilter
    testProject = $TestProject
    testProjectPath = $TestProjectPath
    testClass = $TestClass
    testMethod = $TestMethod
    expectedFailureSignature = $ExpectedFailureSignature
    actualFailureMessage = $boundedFailureMessage
    verifierExitCode = $exitCode
    verifierPassed = $verifierPassed
    signatureMatched = $signatureMatched
    signatureEquivalent = $signatureEquivalent
    effectiveFailureSignature = $effectiveFailureSignature
    infrastructureFailure = $infrastructureFailure
    selectionAmbiguous = $selectionAmbiguous
    executedTestCounts = @($executedTestCounts)
    retainedResultFiles = @($retainedResultFiles)
    verificationPassed = $verificationPassed
    requestedRunCount = $RunCount
    completedRunCount = $completedRuns
    consistentRuns = $consistentRuns
    stableFailureMessage = $stableFailureMessage
    observedFailureMessages = @($runOutcomes |
        ForEach-Object { Get-ReplicationVolatileFreeMessage -Value ([string]$_.ActualFailureMessage) } |
        Where-Object { $_ } |
        Sort-Object -Unique)
    logFiles = @($candidateLogs)
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $resultPath -Encoding utf8NoBOM

if (-not $verificationPassed) {
    if ($selectionAmbiguous) {
        # A summary of booleans would send the agent hunting for a build or
        # signature problem, so name the one thing that has to change.
        $rendered = if ($executedTestCounts.Count -gt 0) {
            ($executedTestCounts -join ' and ')
        } else {
            'several'
        }
        throw ("The run executed $rendered tests, so the failure cannot be " +
            'attributed to the named test. Give the reproduction test a unique ' +
            'name that no other test name contains so the runner selects ' +
            'exactly one test.')
    }
    if (-not $stableFailureMessage -and $verifierPassed -and $consistentRuns) {
        $observed = @($runOutcomes |
            ForEach-Object { Get-ReplicationVolatileFreeMessage -Value ([string]$_.ActualFailureMessage) } |
            Sort-Object -Unique)
        throw ("The test failed every run but not with the same message, so the reproduction is not " +
            "deterministic. Independent executions reported: " +
            (($observed | ForEach-Object { "'$_'" }) -join ' and ') +
            ". Assert on a value the defect fixes exactly rather than one that drifts between runs.")
    }
    throw ("Replication test verification failed (verifierPassed=$verifierPassed, " +
        "signatureMatched=$signatureMatched, signatureEquivalent=$signatureEquivalent, " +
        "infrastructureFailure=$infrastructureFailure, " +
        "consistentRuns=$consistentRuns, completedRuns=$completedRuns/$RunCount, " +
        "stableFailureMessage=$stableFailureMessage).")
}

Write-Host 'REPLICATION TEST VERIFICATION PASSED'
Write-Host ("The targeted test failed at the expected assertion in $completedRuns of $RunCount independent executions.")
Write-Host "Verification result: $resultPath"
