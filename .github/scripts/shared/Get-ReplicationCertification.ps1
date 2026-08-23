#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

<#
    Grades how much a reproduction actually proves.

    A red test is not evidence of a defect. It is evidence that something
    failed, and reviewers rejected candidate after candidate that was red for a
    reason unrelated to the reported bug: a wrong oracle, a vacuous assertion, a
    scenario that fails identically on a build without the defect. Each of those
    was caught by hand, by removing the claimed trigger and re-running, and no
    static rule can substitute for that.

    So the pipeline states what it established rather than asserting a verdict:

      runtime-blocked        The platform runtime was unavailable, so nothing
                             was executed. Never presentable as a reproduction.
      candidate-scenario     The source compiles. Not empirically validated.
      observed-reproduction  The exact test repeatedly failed at the intended
                             assertion with an identical message.
      trigger-certified      Removing the reported trigger makes the same test
                             pass, so its failure is attributable to the trigger
                             rather than to an unrelated cause.
      certified-oracle       A minimal product fix additionally makes the test
                             pass, and taking that fix away makes it fail again,
                             so the test is a regression oracle for the defect.

    The name stops at 'trigger' deliberately. A full regression oracle also
    requires a minimal product fix to turn the test green and reverting that
    fix to turn it red again. When those arms did not run, naming this level
    'certified' outright would claim causal evidence the run never gathered.

    'certified-oracle' is the level that does claim it, and it is granted only
    when all four arms are satisfied: the test fails as reported, passes with
    the trigger removed, passes with a fix applied, and fails again when that
    fix is taken away.
#>

$script:ReplicationCertificationLevels = @(
    'runtime-blocked',
    'candidate-scenario',
    'observed-reproduction',
    'trigger-certified',
    # Rank is position in this array, not alphabetical order, so a stronger
    # level has to be appended rather than merely named.
    'certified-oracle'
)

function Get-ReplicationCertificationLevels {
    <#
        .SYNOPSIS
        Returns the certification levels, weakest first.
    #>
    return , @($script:ReplicationCertificationLevels)
}

function Get-ReplicationCertificationRank {
    <#
        .SYNOPSIS
        Returns a comparable rank for a certification level.

        .DESCRIPTION
        Callers compare strength constantly, and comparing the names as strings
        would order them alphabetically, which puts 'candidate-scenario' above
        'trigger-certified'.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string]$Level
    )

    $index = $script:ReplicationCertificationLevels.IndexOf([string]$Level)
    if ($index -lt 0) {
        throw "Unknown certification level '$Level'."
    }

    return $index
}

function Get-ReplicationControlOutcome {
    <#
        .SYNOPSIS
        Summarises one control arm of the certification matrix.

        .DESCRIPTION
        Every arm asks the same question in the same shape: of the runs that
        were attempted, how many produced the outcome the arm requires, and were
        there enough of them. 'Requested' is deliberately separate from
        'observed' so a control that simply did not run cannot be read as one
        that passed.
    #>
    [CmdletBinding()]
    param(
        [int]$Requested = 0,
        [int]$Observed = 0,
        [int]$Required = 3
    )

    $attempted = $Requested -gt 0
    $satisfied = $attempted -and $Requested -ge $Required -and $Observed -ge $Required

    return @{
        Attempted = $attempted
        Satisfied = [bool]$satisfied
        Requested = $Requested
        Observed  = $Observed
        Required  = $Required
    }
}

function Get-ReplicationFailureStatus {
    <#
        .SYNOPSIS
        Names, in one fixed phrase, what a draft is still missing.

        .DESCRIPTION
        A draft that is not certified has to say which gate stopped it, because
        "not certified" reads to a reviewer as "probably fine, just unlucky".
        The phrase is derived from the same evidence the grade is, in a fixed
        order, so the first unmet gate is the one reported and two runs that
        failed the same way always report the same phrase.
    #>
    param(
        [bool]$RuntimeAvailable,
        [int]$BaselineRuns,
        [bool]$ExactlyOneTestExecuted,
        [bool]$BaselineSatisfied,
        [bool]$StableFailureMessage,
        [bool]$ControlArmFailed,
        [bool]$NegativeControlAttempted
    )

    if (-not $RuntimeAvailable) { return 'RUNTIME UNAVAILABLE' }
    # The runtime was there and still nothing ran, so the candidate never
    # became a test that could be executed.
    if ($BaselineRuns -le 0) { return 'SOURCE CHANGES REQUIRED' }
    if (-not $ExactlyOneTestExecuted) { return 'ZERO TESTS SELECTED' }
    if (-not $BaselineSatisfied -or -not $StableFailureMessage) {
        return 'WRONG ASSERTION REACHED'
    }
    if ($ControlArmFailed) { return 'CAUSAL CONTROL FAILED' }
    if (-not $NegativeControlAttempted) { return 'EVIDENCE INCOMPLETE' }
    return 'CERTIFIED'
}

function Get-ReplicationCertification {
    <#
        .SYNOPSIS
        Grades a reproduction from the evidence actually collected.

        .DESCRIPTION
        The grade is deliberately derived, never declared: an agent that could
        name its own level would name the highest one. Each promotion requires
        strictly more evidence than the last, and any missing arm caps the grade
        rather than being assumed to have passed.

        The minimum certification matrix, always on one exact test, is

            baseline red N/N
              -> trigger-removed green N/N
              -> product-fix green N/N
              -> restored baseline red N/N

        The fix and restoration arms are optional because authoring a minimal
        product fix is out of scope for a reproduction. When they are absent the
        run can still reach 'trigger-certified' on the negative control alone,
        which is the arm that rules out a test that is red for an unrelated
        reason. When they are present and disagree, they are decisive: a fix
        that does not turn the test green means the test is not measuring what
        the fix repairs.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$Evidence,

        [ValidateRange(1, 10)]
        [int]$RequiredRuns = 3
    )

    function Get-Field {
        param($Source, [string]$Name, $Default)

        if ($null -eq $Source) { return $Default }
        if ($Source -is [System.Collections.IDictionary]) {
            if ($Source.Contains($Name)) { return $Source[$Name] }
            return $Default
        }
        $property = $Source.PSObject.Properties[$Name]
        if ($property) { return $property.Value }
        return $Default
    }

    $reasons = New-Object 'System.Collections.Generic.List[string]'

    $runtimeAvailable = [bool](Get-Field $Evidence 'runtimeAvailable' $false)
    if (-not $runtimeAvailable) {
        # Item 6: a blocked runtime still produces a draft, because the authored
        # scenario is worth keeping, but it must never read as a reproduction.
        return @{
            Level              = 'runtime-blocked'
            Publish            = $true
            ClaimsReproduction = $false
            Reasons            = @('The platform runtime was unavailable, so no test was executed.')
            Status             = (Get-ReplicationFailureStatus -RuntimeAvailable $false `
                    -BaselineRuns 0 -ExactlyOneTestExecuted $false -BaselineSatisfied $false `
                    -StableFailureMessage $false -ControlArmFailed $false `
                    -NegativeControlAttempted $false)
            Controls           = @{}
        }
    }

    $baseline = Get-ReplicationControlOutcome `
        -Requested ([int](Get-Field $Evidence 'baselineRuns' 0)) `
        -Observed ([int](Get-Field $Evidence 'baselineFailures' 0)) `
        -Required $RequiredRuns
    $negative = Get-ReplicationControlOutcome `
        -Requested ([int](Get-Field $Evidence 'negativeControlRuns' 0)) `
        -Observed ([int](Get-Field $Evidence 'negativeControlPasses' 0)) `
        -Required $RequiredRuns
    $fix = Get-ReplicationControlOutcome `
        -Requested ([int](Get-Field $Evidence 'fixControlRuns' 0)) `
        -Observed ([int](Get-Field $Evidence 'fixControlPasses' 0)) `
        -Required $RequiredRuns
    $restoration = Get-ReplicationControlOutcome `
        -Requested ([int](Get-Field $Evidence 'restorationRuns' 0)) `
        -Observed ([int](Get-Field $Evidence 'restorationFailures' 0)) `
        -Required $RequiredRuns

    $controls = @{
        Baseline    = $baseline
        Negative    = $negative
        Fix         = $fix
        Restoration = $restoration
    }

    $stableMessage = [bool](Get-Field $Evidence 'stableFailureMessage' $false)
    $exactlyOne = [bool](Get-Field $Evidence 'exactlyOneTestExecuted' $false)

    if (-not $baseline.Satisfied) {
        $reasons.Add(
            "The test failed $($baseline.Observed) of $($baseline.Requested) runs; " +
            "$RequiredRuns identical failures are required.")
    }
    if (-not $stableMessage) {
        $reasons.Add('The failure message was not identical across runs.')
    }
    if (-not $exactlyOne) {
        $reasons.Add('The run did not prove that exactly one test was selected and executed.')
    }

    if ($reasons.Count -gt 0) {
        # The weak-test case the reviewers kept rejecting: plausible, red, and
        # unvalidated. Withheld rather than published.
        return @{
            Level              = 'candidate-scenario'
            Publish            = $false
            ClaimsReproduction = $false
            Reasons            = @($reasons)
            Status             = (Get-ReplicationFailureStatus `
                    -RuntimeAvailable $runtimeAvailable `
                    -BaselineRuns $baseline.Requested `
                    -ExactlyOneTestExecuted $exactlyOne `
                    -BaselineSatisfied $baseline.Satisfied `
                    -StableFailureMessage $stableMessage `
                    -ControlArmFailed $false `
                    -NegativeControlAttempted $negative.Attempted)
            Controls           = $controls
        }
    }

    # A fix arm that was attempted and did not turn the test green is decisive
    # against the test, not merely absent evidence: it says the test does not
    # measure what the fix repairs.
    if ($fix.Attempted -and -not $fix.Satisfied) {
        $reasons.Add('A minimal product fix did not make the test pass, so the test is not measuring the defect the fix repairs.')
    }
    if ($restoration.Attempted -and -not $restoration.Satisfied) {
        $reasons.Add('Reverting the fix did not make the test fail again, so its failure is not attributable to the defect.')
    }
    if ($negative.Attempted -and -not $negative.Satisfied) {
        $reasons.Add('Removing the reported trigger did not make the test pass, so it is red for a reason unrelated to the report.')
    }

    if ($reasons.Count -gt 0) {
        return @{
            Level              = 'observed-reproduction'
            Publish            = $true
            ClaimsReproduction = $true
            Reasons            = @($reasons)
            Status             = (Get-ReplicationFailureStatus `
                    -RuntimeAvailable $runtimeAvailable `
                    -BaselineRuns $baseline.Requested `
                    -ExactlyOneTestExecuted $exactlyOne `
                    -BaselineSatisfied $baseline.Satisfied `
                    -StableFailureMessage $stableMessage `
                    -ControlArmFailed $true `
                    -NegativeControlAttempted $true)
            Controls           = $controls
        }
    }

    if (-not $negative.Attempted) {
        return @{
            Level              = 'observed-reproduction'
            Publish            = $true
            ClaimsReproduction = $true
            Reasons            = @('No negative control was run, so the failure has not been tied to the reported trigger.')
            Status             = (Get-ReplicationFailureStatus `
                    -RuntimeAvailable $runtimeAvailable `
                    -BaselineRuns $baseline.Requested `
                    -ExactlyOneTestExecuted $exactlyOne `
                    -BaselineSatisfied $baseline.Satisfied `
                    -StableFailureMessage $stableMessage `
                    -ControlArmFailed $false `
                    -NegativeControlAttempted $false)
            Controls           = $controls
        }
    }

    # Every arm the run attempted was satisfied, or it would have returned
    # above. So the only question left is whether the fix arms ran at all.
    $fullyCertified = $fix.Attempted -and $fix.Satisfied -and
        $restoration.Attempted -and $restoration.Satisfied

    return @{
        Level              = $(if ($fullyCertified) { 'certified-oracle' } else { 'trigger-certified' })
        Publish            = $true
        ClaimsReproduction = $true
        Reasons            = @()
        Status             = (Get-ReplicationFailureStatus `
                -RuntimeAvailable $runtimeAvailable `
                -BaselineRuns $baseline.Requested `
                -ExactlyOneTestExecuted $exactlyOne `
                -BaselineSatisfied $baseline.Satisfied `
                -StableFailureMessage $stableMessage `
                -ControlArmFailed $false `
                -NegativeControlAttempted $true)
        Controls           = $controls
    }
}

function Get-ReplicationCertificationSummary {
    <#
        .SYNOPSIS
        Renders the certification matrix for a pull request body.

        .DESCRIPTION
        Reviewers repeatedly had to reconstruct by hand which arms had actually
        run, so the table states every arm including the ones that did not run.
        An arm that was skipped reads as skipped rather than as absent.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$Certification
    )

    $level = [string]$Certification.Level
    $lines = New-Object 'System.Collections.Generic.List[string]'
    $lines.Add("**Evidence level: ``$level``**")
    # A draft that is not certified has to name the gate that stopped it, in
    # the same fixed phrase every time, so a reviewer never has to infer from
    # prose whether the run fell short of proof or merely of polish.
    $status = ''
    if ($Certification -is [System.Collections.IDictionary]) {
        if ($Certification.Contains('Status')) { $status = [string]$Certification['Status'] }
    } elseif ($Certification.PSObject.Properties['Status']) {
        $status = [string]$Certification.PSObject.Properties['Status'].Value
    }
    if (-not [string]::IsNullOrWhiteSpace($status) -and $status -ne 'CERTIFIED') {
        $lines.Add("**Status: ``$status``**")
    }
    $lines.Add('')

    $descriptions = @{
        'runtime-blocked'       = 'The platform runtime was unavailable. Nothing was executed and this is not a reproduction.'
        'candidate-scenario'    = 'The source compiles. It has not been empirically validated.'
        'observed-reproduction' = 'The exact test repeatedly failed at the intended assertion.'
        'trigger-certified'     = 'Removing the reported trigger makes the same test pass, so the failure is attributable to the trigger. A minimal product fix was not authored, so this is not a full regression oracle.'
        'certified-oracle'      = 'All four arms agree: the test fails as reported, passes with the reported trigger removed, passes with a minimal product fix applied, and fails again once that fix is taken away. Its failure is caused by the defect the fix repairs.'
    }
    if ($descriptions.ContainsKey($level)) {
        $lines.Add($descriptions[$level])
        $lines.Add('')
    }

    $controls = $Certification.Controls
    if ($controls -and $controls.Count -gt 0) {
        $lines.Add('| Control | Expected | Result |')
        $lines.Add('| --- | --- | --- |')

        # An arm that was never going to run must not read like an arm that
        # failed to. The fix arms are skipped by policy, not by accident, and a
        # bare 'not run' next to two green arms invites a reviewer to read the
        # matrix as incomplete evidence rather than as bounded scope.
        $arms = @(
            @{ Key = 'Baseline';    Name = 'Baseline';               Expected = 'fails';  OutOfScope = $false },
            @{ Key = 'Negative';    Name = 'Trigger removed';        Expected = 'passes'; OutOfScope = $false },
            @{ Key = 'Fix';         Name = 'Minimal product fix';    Expected = 'passes'; OutOfScope = $true },
            @{ Key = 'Restoration'; Name = 'Fix reverted';           Expected = 'fails';  OutOfScope = $true }
        )

        foreach ($arm in $arms) {
            $outcome = $controls[$arm.Key]
            $result = if (-not $outcome -or -not $outcome.Attempted) {
                if ($arm.OutOfScope) { 'not run (out of scope)' } else { 'not run' }
            } elseif ($outcome.Satisfied) {
                "$($outcome.Observed)/$($outcome.Requested) ✅"
            } else {
                "$($outcome.Observed)/$($outcome.Requested) ❌"
            }
            $lines.Add("| $($arm.Name) | $($arm.Expected) | $result |")
        }
        $lines.Add('')
    }

    $reasons = @($Certification.Reasons)
    if ($reasons.Count -gt 0) {
        $lines.Add('Limitations:')
        foreach ($reason in $reasons) {
            $lines.Add("- $reason")
        }
    }

    return ($lines -join "`n")
}
