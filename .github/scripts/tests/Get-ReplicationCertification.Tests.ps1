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
        $certified = Get-ReplicationCertificationRank -Level 'certified-oracle'

        $blocked | Should -BeLessThan $candidate
        $candidate | Should -BeLessThan $observed
        $observed | Should -BeLessThan $certified
    }

    It 'ranks certified above candidate even though it sorts earlier as text' {
        ('candidate-scenario' -lt 'certified-oracle') | Should -BeTrue
        (Get-ReplicationCertificationRank -Level 'certified-oracle') |
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

        $result.Level | Should -Not -Be 'certified-oracle'
        $result.Reasons -join ' ' | Should -Match 'No negative control'
    }
}

Describe 'Get-ReplicationCertification causal controls' {
    It 'certifies when removing the trigger makes the test pass' {
        $evidence = New-Evidence @{ negativeControlRuns = 3; negativeControlPasses = 3 }

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'certified-oracle'
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

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'certified-oracle'
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

        (Get-ReplicationCertification -Evidence $evidence).Level | Should -Be 'certified-oracle'
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
