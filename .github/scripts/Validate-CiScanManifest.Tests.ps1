#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Validate-CiScanManifest.ps1'
    . $scriptPath

    function New-TestBody {
        param(
            [string]$Pipeline = 'maui-pr',
            [Int64]$BuildId = 123456,
            [string]$Fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows',
            [int]$MatchCount = 2
        )

        @"
<!-- ci-scan-fingerprint: $Fingerprint -->
<!-- ci-scan-match-count: $MatchCount hits in failure.log -->

## Summary
Recurring sample failure.

## Build Information
- **Pipeline**: $Pipeline
- **Build**: https://dev.azure.com/example
- **Build ID**: $BuildId
- **Branch**: net11.0

## Error Message
``````
Assertion failed
``````
"@
    }

    function New-TestSignature {
        param(
            [string]$Pipeline = 'maui-pr',
            [Int64]$BuildId = 123456,
            [string]$Fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows',
            [string]$Disposition = 'filed',
            [string]$Title = 'Sample test fails on Windows',
            [string]$SkipReason = '',
            [Int64]$IssueNumber = 0,
            [string]$Body = ''
        )

        if (-not $Body) {
            $Body = New-TestBody -Pipeline $Pipeline -BuildId $BuildId -Fingerprint $Fingerprint
        }

        $signature = [ordered]@{
            fingerprint = $Fingerprint
            disposition = $Disposition
        }
        if ($Disposition -eq 'filed') {
            $signature.title = $Title
            $signature.body = $Body
        } elseif ($Disposition -eq 'existing') {
            $signature.issue_number = $IssueNumber
        } elseif ($Disposition -eq 'skipped') {
            $signature.skip_reason = $SkipReason
        }

        return [pscustomobject]$signature
    }

    function New-TestPipeline {
        param(
            [string]$Name,
            [int]$DefinitionId,
            [string]$Status = 'scanned',
            [Int64]$BuildId = 123456,
            [object[]]$Signatures = @()
        )

        [pscustomobject]@{
            name          = $Name
            definition_id = $DefinitionId
            status        = $Status
            build_id      = $BuildId
            signatures    = @($Signatures)
        }
    }

    function New-CompleteManifest {
        param([object[]]$MainSignatures = @())

        [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -BuildId 123456 -Signatures $MainSignatures)
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -BuildId 123457)
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -BuildId 123458)
            )
        }
    }

    function New-ExpectedBuilds {
        param(
            [Int64]$MainBuildId = 123456,
            [string]$MainResult = 'failed',
            [Int64]$MainFailedRecordCount = 1
        )

        @(
            [pscustomobject]@{
                name                = 'maui-pr'
                definition_id       = 302
                status              = 'scanned'
                build_id            = $MainBuildId
                result              = $MainResult
                failed_record_count = $MainFailedRecordCount
            }
            [pscustomobject]@{
                name                = 'maui-pr-devicetests'
                definition_id       = 314
                status              = 'scanned'
                build_id            = 123457
                result              = 'succeeded'
                failed_record_count = 0
            }
            [pscustomobject]@{
                name                = 'maui-pr-uitests'
                definition_id       = 313
                status              = 'scanned'
                build_id            = 123458
                result              = 'succeeded'
                failed_record_count = 0
            }
        )
    }
}

Describe 'CI scanner pipeline coverage gate' {
    It 'rejects an early stop after one pipeline when the cap is unused' {
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -Signatures @(
                        (New-TestSignature)
                    ))
            )
        }

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*exactly 3 pipelines*'
    }

    It 'accepts complete three-pipeline coverage with an unused cap' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        $plan = Test-CiScanManifest -Manifest $manifest

        $plan.pipelines.Count | Should -Be 3
        $plan.filed_count | Should -Be 1
        $plan.has_cap_skip | Should -BeFalse
    }

    It 'rejects fabricated all-clean coverage with untrusted build IDs' {
        $manifest = New-CompleteManifest

        { Test-CiScanManifest -Manifest $manifest -ExpectedBuilds (New-ExpectedBuilds -MainBuildId 999999) } |
            Should -Throw '*build_id must match the trusted latest completed build*'
    }

    It 'rejects empty coverage when the trusted build contains failed records' {
        $manifest = New-CompleteManifest

        { Test-CiScanManifest -Manifest $manifest -ExpectedBuilds (New-ExpectedBuilds) } |
            Should -Throw '*must record at least one signature because the trusted build has failed records*'
    }

    It 'accepts empty coverage for a trusted successful build' {
        $manifest = New-CompleteManifest

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds -MainResult 'succeeded' -MainFailedRecordCount 0)

        $plan.pipelines[0].build_result | Should -Be 'succeeded'
        $plan.pipelines[0].failed_records | Should -Be 0
    }

    It 'accepts actual cap exhaustion with explicit remaining-pipeline skips' {
        $signatures = for ($i = 1; $i -le 5; $i++) {
            $fingerprint = "ci-scan-net11|net11.0|maui-pr|sample test $i|assertion failed|windows"
            New-TestSignature -Fingerprint $fingerprint -Body (
                New-TestBody -Fingerprint $fingerprint
            )
        }
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -Signatures $signatures)
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -Status 'skipped-cap-reached')
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -Status 'skipped-cap-reached')
            )
        }

        $plan = Test-CiScanManifest -Manifest $manifest

        $plan.filed_count | Should -Be 5
        $plan.has_cap_skip | Should -BeTrue
    }

    It 'rejects cap skips when fewer than five issues are filed' {
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -Signatures @(
                        (New-TestSignature)
                    ))
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -Status 'skipped-cap-reached')
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -Status 'skipped-cap-reached')
            )
        }

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*exactly 5 issues are filed*'
    }

    It 'rejects a cap skip that appears before five later filed entries' {
        $earlySkip = New-TestSignature `
            -Fingerprint 'ci-scan-net11|net11.0|maui-pr|early sample|assertion failed|windows' `
            -Disposition 'skipped' `
            -SkipReason 'cap-reached'
        $filed = for ($i = 1; $i -le 5; $i++) {
            $fingerprint = "ci-scan-net11|net11.0|maui-pr|later sample $i|assertion failed|windows"
            New-TestSignature -Fingerprint $fingerprint -Body (
                New-TestBody -Fingerprint $fingerprint
            )
        }
        $manifest = New-CompleteManifest -MainSignatures (@($earlySkip) + @($filed))

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*cannot use cap-reached before exactly 5 issues are filed*'
    }

    It 'rejects reordered configured pipelines' {
        $manifest = New-CompleteManifest
        $manifest.pipelines = @(
            $manifest.pipelines[1],
            $manifest.pipelines[0],
            $manifest.pipelines[2]
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*must be maui-pr definition 302 in configured order*'
    }

    It 'rejects a skipped pipeline that still contains signatures' {
        $manifest = New-CompleteManifest
        $manifest.pipelines[1].status = 'skipped-no-recent-build'
        $manifest.pipelines[1].signatures = @(
            (New-TestSignature `
                    -Pipeline 'maui-pr-devicetests' `
                    -BuildId 123457 `
                    -Fingerprint 'ci-scan-net11|net11.0|maui-pr-devicetests|device sample|timeout|android')
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*must have an empty signatures array*'
    }
}

Describe 'CI scanner issue payload gate' {
    It 'rejects unsafe characters in a fingerprint' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample "test"|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*non-normalized or unsafe characters*'
    }

    It 'rejects a fingerprint for another scanner' {
        $fingerprint = 'ci-scan-main|main|maui-pr|sample test|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*does not match the net11 scanner*'
    }

    It 'rejects a fingerprint for another pipeline' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr-uitests|sample test|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*does not match the net11 scanner*'
    }

    It 'rejects a fingerprint with the wrong field count' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*exactly six non-empty pipe-delimited fields*'
    }

    It 'rejects the literal truncation placeholder in a title' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Title 'Sample failure [Content truncated due to length]')
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*forbidden truncation placeholder*'
    }

    It 'rejects a missing fingerprint marker' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = (New-TestBody -Fingerprint $fingerprint) -replace '(?m)^<!-- ci-scan-fingerprint:.*\r?\n', ''
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*exactly one canonical fingerprint marker*'
    }

    It 'rejects duplicate fingerprint markers' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n<!-- ci-scan-fingerprint: $fingerprint -->"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*exactly one canonical fingerprint marker*'
    }

    It 'rejects a missing match-count marker' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = (New-TestBody -Fingerprint $fingerprint) -replace '(?m)^<!-- ci-scan-match-count:.*\r?\n', ''
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*exactly one canonical positive match-count marker*'
    }

    It 'rejects duplicate match-count markers' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n<!-- ci-scan-match-count: 3 hits in failure.log -->"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*exactly one canonical positive match-count marker*'
    }

    It 'accepts a valid canonical issue payload' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        $plan = Test-CiScanManifest -Manifest $manifest

        $plan.issues.Count | Should -Be 1
        $plan.issues[0].Title | Should -Be '[ci-scan-net11] Sample test fails on Windows'
        $plan.issues[0].MatchCount | Should -Be 2
    }

    It 'neutralizes user and team mentions in the validated issue body' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nOwner: @octocat and @dotnet/maui."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        $plan = Test-CiScanManifest -Manifest $manifest

        $plan.issues[0].Body | Should -Not -Match '@octocat|@dotnet/maui'
        $plan.issues[0].Body | Should -Match "@$([char]0x200B)octocat"
        $plan.issues[0].Body | Should -Match "@$([char]0x200B)dotnet/maui"
    }

    It 'rejects a Build ID marker that differs from the pipeline build' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Fingerprint $fingerprint `
                    -Body (New-TestBody -Fingerprint $fingerprint -BuildId 999999))
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*Build ID line matching 123456*'
    }
}

Describe 'CI scanner terminal dispositions' {
    It 'accepts a valid existing issue reference' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Disposition 'existing' -IssueNumber 36827)
        )

        $plan = Test-CiScanManifest -Manifest $manifest

        $plan.filed_count | Should -Be 0
        $plan.pipelines[0].signatures[0].issue_number | Should -Be 36827
    }

    It 'rejects a non-positive existing issue reference' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Disposition 'existing' -IssueNumber 0)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*issue_number must be a positive integer*'
    }

    It 'accepts an allowed non-cap skip reason' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Disposition 'skipped' -SkipReason 'not-recurring')
        )

        $plan = Test-CiScanManifest -Manifest $manifest

        $plan.pipelines[0].signatures[0].skip_reason | Should -Be 'not-recurring'
    }

    It 'rejects an unknown skip reason' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Disposition 'skipped' -SkipReason 'free-form reason')
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*invalid skip_reason*'
    }

    It 'rejects duplicate fingerprints in one pipeline' {
        $signature = New-TestSignature -Disposition 'existing' -IssueNumber 36827
        $manifest = New-CompleteManifest -MainSignatures @($signature, $signature)

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*Duplicate fingerprint*'
    }
}

Describe 'CI scanner agent output gate' {
    It 'rejects alternate outputs alongside the scanner submission' {
        $path = Join-Path $TestDrive 'agent-output.json'
        @{
            items = @(
                @{ type = 'submit_ci_scan'; manifest = '{}' },
                @{ type = 'noop'; body = 'alternate output' }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        { Get-ScannerManifestFromAgentOutput -Path $path } |
            Should -Throw '*exactly one item of type submit_ci_scan and no alternate outputs*'
    }
}
