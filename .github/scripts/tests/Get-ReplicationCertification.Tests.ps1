#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

BeforeAll {
    $script:ModulePath = Join-Path $PSScriptRoot '..' 'shared' 'Get-ReplicationCertification.ps1'
    . $script:ModulePath

    function New-Evidence {
        param([hashtable]$Override = @{})

        $evidence = @{
            runtimeAvailable       = $true
            baselineRuns           = 3
            baselineFailures       = 3
            stableFailureMessage   = $true
            exactlyOneTestExecuted = $true
            negativeControlRuns    = 0
            negativeControlPasses  = 0
            fixControlRuns         = 0
            fixControlPasses       = 0
            restorationRuns        = 0
            restorationFailures    = 0
        }
        foreach ($key in $Override.Keys) { $evidence[$key] = $Override[$key] }
        return $evidence
    }
}

Describe 'Get-ReplicationCertificationRank' {
    It 'orders levels by strength rather than alphabetically' {
        $blocked = Get-ReplicationCertificationRank -Level 'runtime-blocked'
        $candidate = Get-ReplicationCertificationRank -Level 'candidate-scenario'
        $observed = Get-ReplicationCertificationRank -Level 'observed-reproduction'
        $certified = Get-ReplicationCertificationRank -Level 'trigger-certified'

        $blocked | Should -BeLessThan $candidate
        $candidate | Should -BeLessThan $observed
        $observed | Should -BeLessThan $certified
    }

    It 'ranks certified above candidate even though it sorts earlier as text' {
        ('candidate-scenario' -lt 'trigger-certified') | Should -BeTrue
        (Get-ReplicationCertificationRank -Level 'trigger-certified') |
            Should -BeGreaterThan (Get-ReplicationCertificationRank -Level 'candidate-scenario')
    }

    It 'refuses an unknown level rather than defaulting it' {
        { Get-ReplicationCertificationRank -Level 'totally-certified' } | Should -Throw
    }
}

Describe 'Get-ReplicationCertification runtime availability' {
    It 'reports runtime-blocked when nothing could be executed' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{ runtimeAvailable = $false })

        $result.Level | Should -Be 'runtime-blocked'
    }

    It 'never claims a reproduction when the runtime was unavailable' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{ runtimeAvailable = $false })

        $result.ClaimsReproduction | Should -BeFalse
    }

    It 'still publishes a blocked run as an explicitly marked draft' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{ runtimeAvailable = $false })

        $result.Publish | Should -BeTrue
        $result.Reasons -join ' ' | Should -Match 'runtime was unavailable'
    }

    It 'ignores otherwise perfect evidence when the runtime was unavailable' {
        $evidence = New-Evidence @{
            runtimeAvailable      = $false
            negativeControlRuns   = 3
            negativeControlPasses = 3
        }

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'runtime-blocked'
    }
}

Describe 'Get-ReplicationCertification baseline requirements' {
    It 'withholds a scenario that failed fewer times than required' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{ baselineFailures = 2 })

        $result.Level | Should -Be 'candidate-scenario'
        $result.Publish | Should -BeFalse
    }

    It 'withholds a scenario whose failure message drifted between runs' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{ stableFailureMessage = $false })

        $result.Level | Should -Be 'candidate-scenario'
        $result.Reasons -join ' ' | Should -Match 'identical across runs'
    }

    It 'withholds a scenario that cannot prove exactly one test ran' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{ exactlyOneTestExecuted = $false })

        $result.Level | Should -Be 'candidate-scenario'
        $result.Reasons -join ' ' | Should -Match 'exactly one test'
    }

    It 'treats a control that never ran as unattempted rather than passing' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{ baselineRuns = 0; baselineFailures = 0 })

        $result.Level | Should -Be 'candidate-scenario'
        $result.Controls.Baseline.Attempted | Should -BeFalse
    }

    It 'honours a stricter required run count' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence) -RequiredRuns 5

        $result.Level | Should -Be 'candidate-scenario'
    }
}

Describe 'Get-ReplicationCertification observed reproduction' {
    It 'reaches observed-reproduction on a repeatable identical failure' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence)

        $result.Level | Should -Be 'observed-reproduction'
        $result.Publish | Should -BeTrue
    }

    It 'does not promote a repeatable failure that was never controlled' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence)

        $result.Level | Should -Not -Be 'trigger-certified'
        $result.Reasons -join ' ' | Should -Match 'No negative control'
    }
}

Describe 'Get-ReplicationCertification causal controls' {
    It 'certifies when removing the trigger makes the test pass' {
        $evidence = New-Evidence @{ negativeControlRuns = 3; negativeControlPasses = 3 }

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'trigger-certified'
    }

    It 'refuses to certify when the test stays red without the trigger' {
        $evidence = New-Evidence @{ negativeControlRuns = 3; negativeControlPasses = 0 }
        $result = Get-ReplicationCertification -Evidence $evidence

        $result.Level | Should -Be 'observed-reproduction'
        $result.Reasons -join ' ' | Should -Match 'unrelated to the report'
    }

    It 'refuses to certify when the negative control passed only intermittently' {
        $evidence = New-Evidence @{ negativeControlRuns = 3; negativeControlPasses = 2 }

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'observed-reproduction'
    }

    It 'certifies the full matrix when fix and restoration arms agree' {
        $evidence = New-Evidence @{
            negativeControlRuns   = 3
            negativeControlPasses = 3
            fixControlRuns        = 3
            fixControlPasses      = 3
            restorationRuns       = 3
            restorationFailures   = 3
        }

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'trigger-certified'
    }

    It 'treats a fix that does not turn the test green as decisive against the test' {
        $evidence = New-Evidence @{
            negativeControlRuns   = 3
            negativeControlPasses = 3
            fixControlRuns        = 3
            fixControlPasses      = 0
        }
        $result = Get-ReplicationCertification -Evidence $evidence

        $result.Level | Should -Be 'observed-reproduction'
        $result.Reasons -join ' ' | Should -Match 'not measuring the defect'
    }

    It 'treats a reverted fix that stays green as decisive against the test' {
        $evidence = New-Evidence @{
            negativeControlRuns   = 3
            negativeControlPasses = 3
            fixControlRuns        = 3
            fixControlPasses      = 3
            restorationRuns       = 3
            restorationFailures   = 0
        }
        $result = Get-ReplicationCertification -Evidence $evidence

        $result.Level | Should -Be 'observed-reproduction'
        $result.Reasons -join ' ' | Should -Match 'not attributable to the defect'
    }

    It 'does not penalise a run that simply skipped the optional fix arms' {
        $evidence = New-Evidence @{ negativeControlRuns = 3; negativeControlPasses = 3 }
        $result = Get-ReplicationCertification -Evidence $evidence

        $result.Reasons | Should -BeNullOrEmpty
        $result.Controls.Fix.Attempted | Should -BeFalse
    }
}

Describe 'Get-ReplicationCertification input shapes' {
    It 'reads evidence supplied as a deserialised JSON object' {
        $evidence = New-Evidence @{ negativeControlRuns = 3; negativeControlPasses = 3 } |
            ConvertTo-Json | ConvertFrom-Json

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'trigger-certified'
    }

    It 'treats absent fields as absent evidence rather than as success' {
        $result = Get-ReplicationCertification -Evidence ([pscustomobject]@{ runtimeAvailable = $true })

        $result.Level | Should -Be 'candidate-scenario'
    }
}

Describe 'Get-ReplicationCertificationSummary' {
    It 'states the level it is reporting' {
        $certification = Get-ReplicationCertification -Evidence (New-Evidence)

        Get-ReplicationCertificationSummary -Certification $certification |
            Should -Match 'Evidence level: `observed-reproduction`'
    }

    It 'shows a skipped arm as not run rather than omitting it' {
        $certification = Get-ReplicationCertification -Evidence (New-Evidence)
        $summary = Get-ReplicationCertificationSummary -Certification $certification

        $summary | Should -Match 'Minimal product fix \| passes \| not run'
    }

    It 'reports every arm of the matrix' {
        $certification = Get-ReplicationCertification -Evidence (New-Evidence)
        $summary = Get-ReplicationCertificationSummary -Certification $certification

        foreach ($arm in @('Baseline', 'Trigger removed', 'Minimal product fix', 'Fix reverted')) {
            $summary | Should -Match ([regex]::Escape($arm))
        }
    }

    It 'marks a satisfied arm distinctly from a failed one' {
        $passing = Get-ReplicationCertification -Evidence (New-Evidence @{
                negativeControlRuns = 3; negativeControlPasses = 3
            })
        $failing = Get-ReplicationCertification -Evidence (New-Evidence @{
                negativeControlRuns = 3; negativeControlPasses = 0
            })

        (Get-ReplicationCertificationSummary -Certification $passing) | Should -Match 'Trigger removed \| passes \| 3/3 ✅'
        (Get-ReplicationCertificationSummary -Certification $failing) | Should -Match 'Trigger removed \| passes \| 0/3 ❌'
    }

    It 'surfaces the limitations that blocked certification' {
        $certification = Get-ReplicationCertification -Evidence (New-Evidence @{
                negativeControlRuns = 3; negativeControlPasses = 0
            })

        Get-ReplicationCertificationSummary -Certification $certification | Should -Match 'Limitations:'
    }

    It 'does not describe a blocked run as a reproduction' {
        $certification = Get-ReplicationCertification -Evidence (New-Evidence @{ runtimeAvailable = $false })
        $summary = Get-ReplicationCertificationSummary -Certification $certification

        $summary | Should -Match 'not a reproduction'
    }
}

Describe 'Get-ReplicationCertification failure status' {
    It 'names the unmet gate rather than leaving a reviewer to infer it' {
        $cases = @(
            @{ Status = 'RUNTIME UNAVAILABLE'     ; Evidence = @{ runtimeAvailable = $false } },
            @{ Status = 'SOURCE CHANGES REQUIRED' ; Evidence = @{ baselineRuns = 0; baselineFailures = 0 } },
            @{ Status = 'ZERO TESTS SELECTED'     ; Evidence = @{ exactlyOneTestExecuted = $false } },
            @{ Status = 'WRONG ASSERTION REACHED' ; Evidence = @{ stableFailureMessage = $false } },
            @{ Status = 'EVIDENCE INCOMPLETE'     ; Evidence = @{} },
            @{ Status = 'CAUSAL CONTROL FAILED'   ; Evidence = @{ negativeControlRuns = 3; negativeControlPasses = 1 } },
            @{ Status = 'CERTIFIED'               ; Evidence = @{ negativeControlRuns = 3; negativeControlPasses = 3 } }
        )

        foreach ($case in $cases) {
            $result = Get-ReplicationCertification -Evidence (New-Evidence $case.Evidence)
            $result.Status | Should -Be $case.Status
        }
    }

    It 'reports the first unmet gate when several are unmet at once' {
        # A run with no runtime also has no selection and no control, and it
        # has to report the reason that made all the others unknowable.
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{
                runtimeAvailable       = $false
                exactlyOneTestExecuted = $false
                stableFailureMessage   = $false
            })

        $result.Status | Should -Be 'RUNTIME UNAVAILABLE'
    }

    It 'separates a missing control from a failed one' {
        # Both are uncertified, but one was never asked and the other answered
        # no, and a reviewer acts differently on each.
        $missing = Get-ReplicationCertification -Evidence (New-Evidence @{})
        $failed = Get-ReplicationCertification -Evidence (New-Evidence @{
                negativeControlRuns = 3; negativeControlPasses = 0
            })

        $missing.Status | Should -Be 'EVIDENCE INCOMPLETE'
        $failed.Status | Should -Be 'CAUSAL CONTROL FAILED'
        $missing.Status | Should -Not -Be $failed.Status
    }

    It 'prints the status in the pull request body unless the run certified' {
        $uncertified = Get-ReplicationCertification -Evidence (New-Evidence @{ exactlyOneTestExecuted = $false })
        $certified = Get-ReplicationCertification -Evidence (New-Evidence @{
                negativeControlRuns = 3; negativeControlPasses = 3
                fixControlRuns = 3; fixControlPasses = 3
                restorationRuns = 3; restorationFailures = 3
            })

        (Get-ReplicationCertificationSummary -Certification $uncertified) |
            Should -Match 'Status:.*ZERO TESTS SELECTED'
        (Get-ReplicationCertificationSummary -Certification $certified) |
            Should -Not -Match 'Status:'
    }

    It 'uses only the agreed vocabulary so the phrase can be matched exactly' {
        $allowed = @(
            'RUNTIME UNAVAILABLE', 'SOURCE CHANGES REQUIRED', 'ZERO TESTS SELECTED',
            'WRONG ASSERTION REACHED', 'CAUSAL CONTROL FAILED', 'EVIDENCE INCOMPLETE',
            'CERTIFIED')

        foreach ($runtime in @($true, $false)) {
            foreach ($runs in @(0, 3)) {
                foreach ($exact in @($true, $false)) {
                    foreach ($stable in @($true, $false)) {
                        foreach ($passes in @(0, 1, 3)) {
                            $result = Get-ReplicationCertification -Evidence (New-Evidence @{
                                    runtimeAvailable       = $runtime
                                    baselineRuns           = $runs
                                    baselineFailures       = $runs
                                    exactlyOneTestExecuted = $exact
                                    stableFailureMessage   = $stable
                                    negativeControlRuns    = $passes
                                    negativeControlPasses  = $passes
                                })
                            $allowed | Should -Contain $result.Status
                        }
                    }
                }
            }
        }
    }
}

Describe 'The top level claims only the arms that ran' {
    # The published matrix showed 'Baseline 3/3 OK | Trigger removed 3/3 OK |
    # Minimal product fix: not run | Fix reverted: not run' under a level named
    # 'certified-oracle'. Nothing in the table was false, but the name asserted
    # causal certification the run never gathered, because a regression oracle
    # is certified by the fix and restoration arms and those never execute.
    It 'does not offer a level whose name claims a fix arm that never runs' {
        Get-ReplicationCertificationLevels | Should -Not -Contain 'certified-oracle'
    }

    It 'names the top level for the trigger it actually controlled' {
        $result = Get-ReplicationCertification -Evidence (New-Evidence @{
                negativeControlRuns   = 3
                negativeControlPasses = 3
            })
        $result.Level | Should -Be 'trigger-certified'
        $result.Status | Should -Be 'CERTIFIED'
    }

    It 'keeps the top level strongest so the rename did not reorder strength' {
        (Get-ReplicationCertificationRank -Level 'trigger-certified') |
            Should -BeGreaterThan (Get-ReplicationCertificationRank -Level 'observed-reproduction')
    }

    It 'says a skipped fix arm was out of scope, not merely absent' {
        $summary = Get-ReplicationCertificationSummary -Certification (
            Get-ReplicationCertification -Evidence (New-Evidence @{
                    negativeControlRuns   = 3
                    negativeControlPasses = 3
                }))
        $text = $summary -join "`n"
        $text | Should -Match 'Minimal product fix \| passes \| not run \(out of scope\)'
        $text | Should -Match 'Fix reverted \| fails \| not run \(out of scope\)'
    }

    It 'still reports a genuinely missing baseline as plain "not run"' {
        # Only the fix arms are skipped by policy. A baseline or negative
        # control that did not run is a gap, and labelling it out of scope
        # would excuse exactly the evidence this grader exists to demand.
        $summary = Get-ReplicationCertificationSummary -Certification (
            Get-ReplicationCertification -Evidence (New-Evidence @{
                    negativeControlRuns   = 0
                    negativeControlPasses = 0
                }))
        $text = $summary -join "`n"
        $text | Should -Match 'Trigger removed \| passes \| not run$|Trigger removed \| passes \| not run\s'
        $text | Should -Not -Match 'Trigger removed \| passes \| not run \(out of scope\)'
    }

    It 'tells the reader in prose that this is not a full regression oracle' {
        $summary = Get-ReplicationCertificationSummary -Certification (
            Get-ReplicationCertification -Evidence (New-Evidence @{
                    negativeControlRuns   = 3
                    negativeControlPasses = 3
                }))
        ($summary -join "`n") | Should -Match 'not a full regression oracle'
    }
}
