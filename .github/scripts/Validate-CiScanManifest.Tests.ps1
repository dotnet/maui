#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Validate-CiScanManifest.ps1'
    . $scriptPath

    $script:Net11Config = Get-CiScanScannerConfig -ScannerId 'ci-scan-net11'
    $script:MainConfig = Get-CiScanScannerConfig -ScannerId 'ci-scan'

    # The agent never supplies markers: gh-aw strips literal HTML comments out of the
    # compiled prompt, so a marker instruction in the prompt never reaches the model.
    # Every test body here is therefore marker-free, and the canonical markers are
    # asserted on the PUBLISHED body the validator returns.
    function New-TestBody {
        param(
            [string]$Pipeline = 'maui-pr',
            [Int64]$BuildId = 123456,
            [string]$Fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample scenario test|sample scenario assertion failed|windows'
        )

        @"
## Summary
Recurring sample failure.

## Build Information
- **Pipeline**: $Pipeline
- **Build**: https://dev.azure.com/example
- **Build ID**: $BuildId
- **Branch**: net11.0

## Error Message
``````
Sample scenario assertion failed
``````
"@
    }

    function New-TestSignature {
        param(
            [string]$Pipeline = 'maui-pr',
            [Int64]$BuildId = 123456,
            [string]$Fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample scenario test|sample scenario assertion failed|windows',
            [string]$Disposition = 'filed',
            [string]$Title = 'Sample test fails on Windows',
            [string]$SkipReason = '',
            [Int64]$IssueNumber = 0,
            [Int64[]]$SourceLogIds = @(1001),
            [string]$MatchPattern = 'Sample scenario assertion failed',
            [string]$Body = ''
        )

        if (-not $Body) {
            $Body = New-TestBody -Pipeline $Pipeline -BuildId $BuildId -Fingerprint $Fingerprint
        }

        $signature = [ordered]@{
            fingerprint = $Fingerprint
            disposition = $Disposition
            source_log_ids = $SourceLogIds
        }
        if ($Disposition -eq 'filed') {
            $signature.title = $Title
            $signature.body = $Body
            $signature.match_pattern = $MatchPattern
        } elseif ($Disposition -eq 'existing') {
            $signature.match_pattern = $MatchPattern
            $signature.issue_number = $IssueNumber
        } elseif ($Disposition -eq 'skipped') {
            $signature.skip_reason = $SkipReason
            $signature.match_pattern = $MatchPattern
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
            [Int64]$MainFailedRecordCount = 1,
            [Int64[]]$MainRequiredLogIds = @(1001),
            [AllowNull()][object]$MainFailedLeafLogIds = $null
        )

        # A failed-leaf log is a required log that actually failed. Absent an explicit
        # override, treat every required log of a non-successful build as a failed leaf,
        # which is the strict case the coverage gate has to hold under.
        if ($null -eq $MainFailedLeafLogIds) {
            $MainFailedLeafLogIds = if ($MainResult -eq 'succeeded') { @() } else { $MainRequiredLogIds }
        }

        @(
            [pscustomobject]@{
                name                = 'maui-pr'
                definition_id       = 302
                status              = 'scanned'
                build_id            = $MainBuildId
                result              = $MainResult
                failed_record_count = $MainFailedRecordCount
                required_log_ids    = $MainRequiredLogIds
                failed_leaf_log_ids = @($MainFailedLeafLogIds)
            }
            [pscustomobject]@{
                name                = 'maui-pr-devicetests'
                definition_id       = 314
                status              = 'scanned'
                build_id            = 123457
                result              = 'succeeded'
                failed_record_count = 0
                required_log_ids    = @()
                failed_leaf_log_ids = @()
            }
            [pscustomobject]@{
                name                = 'maui-pr-uitests'
                definition_id       = 313
                status              = 'scanned'
                build_id            = 123458
                result              = 'succeeded'
                failed_record_count = 0
                required_log_ids    = @()
                failed_leaf_log_ids = @()
            }
        )
    }

    function New-TestEvidence {
        param(
            [string]$Root,
            [string]$Pipeline = 'maui-pr',
            [Int64]$BuildId = 123456,
            [Int64]$LogId = 1001,
            [string[]]$Lines = @('Sample scenario assertion failed', 'Sample scenario assertion failed'),
            [string]$SegmentKind = 'azdo-log',
            [string]$SegmentSource = ''
        )

        if (-not $SegmentSource) {
            $SegmentSource = "$BuildId/$LogId"
        }
        $directory = Join-Path $Root $Pipeline
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
        Set-Content `
            -LiteralPath (Join-Path $directory "$BuildId-$LogId.log") `
            -Value $Lines
        [pscustomobject]@{
            schema_version = 1
            pipeline       = $Pipeline
            build_id       = $BuildId
            log_id         = $LogId
            segments       = @(
                [pscustomobject]@{
                    kind    = $SegmentKind
                    source  = $SegmentSource
                    content = $Lines -join "`n"
                }
            )
        } | ConvertTo-Json -Depth 6 | Set-Content `
            -LiteralPath (Join-Path $directory "$BuildId-$LogId.evidence.json")
    }

    # A filed payload's match count is recomputed from frozen evidence and injected by
    # the publisher, so a filed manifest can no longer be validated without evidence.
    # This builds the default evidence set that the default helpers above line up with.
    function New-DefaultEvidenceRoot {
        param(
            [Int64[]]$MainLogIds = @(1001),
            [string[]]$ExtraLines = @()
        )

        $root = Join-Path $TestDrive ('evidence-' + [guid]::NewGuid().ToString('n'))
        $lines = @('Sample scenario assertion failed', 'Sample scenario assertion failed') + $ExtraLines
        foreach ($logId in $MainLogIds) {
            New-TestEvidence -Root $root -Pipeline 'maui-pr' -BuildId 123456 -LogId $logId -Lines $lines
        }
        New-TestEvidence -Root $root -Pipeline 'maui-pr-devicetests' -BuildId 123457 -LogId 1001 -Lines $lines
        New-TestEvidence -Root $root -Pipeline 'maui-pr-uitests' -BuildId 123458 -LogId 1001 -Lines $lines

        return $root
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

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.pipelines.Count | Should -Be 3
        $plan.filed_count | Should -Be 1
        $plan.has_cap_skip | Should -BeFalse
    }

    It 'rejects fabricated all-clean coverage with untrusted build IDs' {
        $evidenceRoot = Join-Path $TestDrive 'untrusted-build-evidence'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds -MainBuildId 999999) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*build_id must match the trusted recent completed build*'
    }

    It 'rejects a trusted build inventory supplied without frozen evidence' {
        # Coverage is granted per disposition, and every disposition proof is evidence
        # backed. Accepting an inventory with no evidence path would make every
        # disposition cover its source logs with nothing proven.
        $manifest = New-CompleteManifest

        { Test-CiScanManifest -Manifest $manifest -ExpectedBuilds (New-ExpectedBuilds) } |
            Should -Throw '*trusted build inventory requires a trusted evidence path*'
    }

    It 'rejects empty coverage when the trusted build contains required logs' {
        $evidenceRoot = Join-Path $TestDrive 'empty-coverage-evidence'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*missing terminal coverage for trusted log IDs: 1001*'
    }

    It 'accepts empty coverage for a trusted successful build' {
        $evidenceRoot = Join-Path $TestDrive 'clean-build-evidence'
        New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
        $manifest = New-CompleteManifest

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds `
                -MainResult 'succeeded' `
                -MainFailedRecordCount 0 `
                -MainRequiredLogIds @()) `
            -TrustedEvidencePath $evidenceRoot

        $plan.pipelines[0].build_result | Should -Be 'succeeded'
        $plan.pipelines[0].failed_records | Should -Be 0
    }

    It 'accepts hidden failure coverage from a trusted successful build' {
        $evidenceRoot = Join-Path $TestDrive 'hidden-failure-evidence'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Disposition 'skipped' -SkipReason 'not-recurring')
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds `
                -MainResult 'succeeded' `
                -MainFailedRecordCount 0 `
                -MainRequiredLogIds @(1001)) `
            -TrustedEvidencePath $evidenceRoot

        $plan.pipelines[0].required_log_ids | Should -Be @(1001)
        $plan.pipelines[0].signatures[0].source_log_ids | Should -Be @(1001)
    }

    It 'rejects partial coverage of the trusted candidate logs' {
        $evidenceRoot = Join-Path $TestDrive 'partial-coverage-evidence'
        New-TestEvidence -Root $evidenceRoot -LogId 1001
        New-TestEvidence -Root $evidenceRoot -LogId 1002
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -SourceLogIds @(1001))
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds -MainFailedRecordCount 2 -MainRequiredLogIds @(1001, 1002)) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*missing terminal coverage for trusted log IDs: 1002*'
    }

    It 'accepts one deduplicated signature that covers multiple trusted logs' {
        $evidenceRoot = Join-Path $TestDrive 'dedup-coverage-evidence'
        New-TestEvidence -Root $evidenceRoot -LogId 1001
        New-TestEvidence -Root $evidenceRoot -LogId 1002
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -SourceLogIds @(1001, 1002) -Body (New-TestBody -MatchCount 4))
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds -MainFailedRecordCount 2 -MainRequiredLogIds @(1001, 1002)) `
            -TrustedEvidencePath $evidenceRoot

        $plan.pipelines[0].signatures[0].source_log_ids | Should -Be @(1001, 1002)
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
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -BuildId 123457 -Signatures @(
                        (New-TestSignature `
                                -Pipeline 'maui-pr-devicetests' `
                                -Fingerprint 'ci-scan-net11|net11.0|maui-pr-devicetests|device sample|assertion failed|android' `
                                -Disposition 'skipped' `
                                -SkipReason 'cap-reached')
                    ))
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -BuildId 123458)
            )
        }

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.filed_count | Should -Be 5
        $plan.has_cap_skip | Should -BeTrue
    }

    It 'rejects cap skips when fewer than five issues are filed' {
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -Signatures @(
                        (New-TestSignature)
                    ))
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -BuildId 123457 -Signatures @(
                        (New-TestSignature `
                                -Pipeline 'maui-pr-devicetests' `
                                -Fingerprint 'ci-scan-net11|net11.0|maui-pr-devicetests|device sample|assertion failed|android' `
                                -Disposition 'skipped' `
                                -SkipReason 'cap-reached')
                    ))
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -BuildId 123458)
            )
        }

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*exactly 5 issues are filed*'
    }

    It 'accepts a cap skip before five later filed entries' {
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

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.filed_count | Should -Be 5
        $plan.has_cap_skip | Should -BeTrue
    }

    It 'accepts a substantive skip after five filed entries' {
        $filed = for ($i = 1; $i -le 5; $i++) {
            $fingerprint = "ci-scan-net11|net11.0|maui-pr|filed sample $i|assertion failed|windows"
            New-TestSignature -Fingerprint $fingerprint -Body (
                New-TestBody -Fingerprint $fingerprint
            )
        }
        $substantiveSkip = New-TestSignature `
            -Fingerprint 'ci-scan-net11|net11.0|maui-pr|known infrastructure failure|assertion failed|windows' `
            -Disposition 'skipped' `
            -SkipReason 'infrastructure-noise'
        $manifest = New-CompleteManifest -MainSignatures (@($filed) + @($substantiveSkip))

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.filed_count | Should -Be 5
        $plan.has_cap_skip | Should -BeFalse
        $plan.pipelines[0].signatures[5].skip_reason | Should -BeExactly 'infrastructure-noise'
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
            Should -Throw '*unsafe characters*'
    }

    It 'canonicalizes the exact production uppercase fingerprint before publication' {
        $productionFingerprint = 'ci-scan|main|maui-pr|runoniOS_MauiReleaseTrimFull|ios-simulator-boot-timeout|ios-simulator-64'
        $canonicalFingerprint = 'ci-scan|main|maui-pr|runonios_mauireleasetrimfull|ios-simulator-boot-timeout|ios-simulator-64'
        $body = (New-TestBody).Replace('- **Branch**: net11.0', '- **Branch**: main')
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $productionFingerprint -Body $body)
        )
        $evidenceRoot = New-DefaultEvidenceRoot

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot `
            -ScannerId 'ci-scan'

        $plan.pipelines[0].signatures[0].fingerprint | Should -BeExactly $canonicalFingerprint
        $plan.issues[0].Fingerprint | Should -BeExactly $canonicalFingerprint
        $plan.issues[0].Body |
            Should -Match "(?m)^<!-- ci-scan-fingerprint: $([regex]::Escape($canonicalFingerprint)) -->$"
        $plan.issues[0].Body.Contains('runoniOS_MauiReleaseTrimFull') | Should -BeFalse
    }

    It 'rejects fingerprints that collide after trusted case canonicalization' {
        $productionFingerprint = 'ci-scan|main|maui-pr|runoniOS_MauiReleaseTrimFull|ios-simulator-boot-timeout|ios-simulator-64'
        $canonicalFingerprint = $productionFingerprint.ToLowerInvariant()
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $productionFingerprint -Disposition 'existing' -IssueNumber 36827),
            (New-TestSignature -Fingerprint $canonicalFingerprint -Disposition 'existing' -IssueNumber 36828)
        )

        { Test-CiScanManifest -Manifest $manifest -ScannerId 'ci-scan' } |
            Should -Throw "*Duplicate fingerprint '$canonicalFingerprint'*"
    }

    <#
        A fingerprint is embedded verbatim in the canonical marker, but the marker is
        matched AFTER ConvertTo-SafeIssueBody neutralizes the body. A GitHub issue/PR URL
        passes the character-class check (every character is in the allowed set), so
        without this gate the URL gets a zero-width space injected, the marker becomes
        unmatchable, and the run dies blaming the BODY for a defect in the FINGERPRINT.
        CI logs referencing a tracking issue by URL make this reachable in practice.
    #>
    It 'rejects a fingerprint containing a GitHub issue URL, naming the real cause' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|see https://github.com/dotnet/maui/issues/12345|assertion failed|windows'

        # Guard the premise: this really does survive the character-class check.
        $fingerprint | Should -CMatch '^[a-z0-9][a-z0-9 ._:/+()\-|]*$'

        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*rewritten by notification neutralization*'
    }

    It 'rejects any fingerprint neutralization would rewrite, not just URL shapes' {
        # Asserts the round-trip invariant itself, so a future neutralization rule is
        # covered without editing Assert-ValidFingerprint.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|see https://github.com/dotnet/maui/pull/999|assertion failed|windows'
        { Assert-ValidFingerprint `
                -Fingerprint $fingerprint `
                -PipelineName 'maui-pr' `
                -ScannerConfig $script:Net11Config } |
            Should -Throw '*rewritten by notification neutralization*'
    }

    It 'accepts a fingerprint that neutralization leaves untouched' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|see github.com/dotnet/maui issue 12345|assertion failed|windows'
        { Assert-ValidFingerprint `
                -Fingerprint $fingerprint `
                -PipelineName 'maui-pr' `
                -ScannerConfig $script:Net11Config } |
            Should -Not -Throw
    }

    It 'rejects a fingerprint for another scanner' {
        $fingerprint = 'ci-scan-main|main|maui-pr|sample test|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*does not match the ci-scan-net11 scanner*'
    }

    It 'rejects a fingerprint for another pipeline' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr-uitests|sample test|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*does not match the ci-scan-net11 scanner*'
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

    It 'canonicalizes the production em-dash title before publication' {
        $rawTitle = "Recurring Android device test failure $([char]0x2014) StatusBarThemeAppliesWhenHandlerConnects fails on Android CoreCLR and Mono (net11.0)"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Title $rawTitle)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $canonicalTitle = 'Recurring Android device test failure - StatusBarThemeAppliesWhenHandlerConnects fails on Android CoreCLR and Mono (net11.0)'
        $plan.pipelines[0].signatures[0].title | Should -BeExactly $canonicalTitle
        $plan.issues[0].Title | Should -BeExactly "[ci-scan-net11] $canonicalTitle"
    }

    It 'also canonicalizes an en-dash title separator' {
        $rawTitle = "Recurring sample failure $([char]0x2013) Windows"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Title $rawTitle)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.pipelines[0].signatures[0].title |
            Should -BeExactly 'Recurring sample failure - Windows'
    }

    It 'still rejects <Case> in a title rather than broadly normalizing Unicode' -ForEach @(
        @{ Case = 'curly quote'; Character = [char]0x2019 }
        @{ Case = 'non-breaking space'; Character = [char]0x00A0 }
        @{ Case = 'emoji'; Character = [char]::ConvertFromUtf32(0x1F600) }
    ) {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Title "Recurring sample $Character failure")
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must contain printable single-line ASCII only*'
    }

    <#
        Marker ownership.

        gh-aw strips literal HTML comments out of the compiled prompt, so a workflow that
        asks the agent to emit `<!-- ci-scan-fingerprint: ... -->` is telling the model
        something the model never receives. Production run 30413273824 filed five issues
        with neither marker for exactly that reason. The publisher therefore injects both
        markers itself, and refuses any agent body that carries marker-like content, in
        any spelling, so an injected marker is always the only marker.
    #>
    It 'injects exactly one canonical marker pair into a marker-free body' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $published = $plan.issues[0].Body
        $lines = $published -split "`r?`n"
        $lines[0] | Should -BeExactly "<!-- ci-scan-fingerprint: $fingerprint -->"
        $lines[1] | Should -BeExactly '<!-- ci-scan-match-count: 2 hits in failure.log -->'
        $lines[2] | Should -Match '^<!-- ci-scan-evidence-key: sha256:[0-9a-f]{64} -->$'
        $lines[3] | Should -BeExactly ''
        [regex]::Matches($published, '<!-- ci-scan-fingerprint:').Count | Should -Be 1
        [regex]::Matches($published, '<!-- ci-scan-match-count:').Count | Should -Be 1
        [regex]::Matches($published, '<!-- ci-scan-evidence-key:').Count | Should -Be 1
        # The agent body is preserved verbatim underneath the injected block.
        $published | Should -Match '(?m)^## Summary$'
    }

    It 'injects a distinct canonical marker for every filed manifest item' {
        $signatures = for ($i = 1; $i -le 3; $i++) {
            $fingerprint = "ci-scan-net11|net11.0|maui-pr|sample test $i|assertion failed|windows"
            New-TestSignature `
                -Fingerprint $fingerprint `
                -Title "Sample test $i fails on Windows" `
                -Body (New-TestBody -Fingerprint $fingerprint)
        }
        $manifest = New-CompleteManifest -MainSignatures @($signatures)

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.issues.Count | Should -Be 3
        foreach ($issue in $plan.issues) {
            $issue.Body | Should -Match "(?m)^<!-- ci-scan-fingerprint: $([regex]::Escape($issue.Fingerprint)) -->$"
        }
        @($plan.issues.Body | Select-Object -Unique).Count | Should -Be 3
    }

    It 'derives the injected fingerprint from the manifest, not from body content' {
        # A body that names a different fingerprint in plain text (no marker syntax) must
        # not influence what gets injected.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $decoy = 'ci-scan-net11|net11.0|maui-pr|attacker chosen|attacker error|linux'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nfingerprint claimed by log text: $decoy"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.issues[0].Body | Should -Match "(?m)^<!-- ci-scan-fingerprint: $([regex]::Escape($fingerprint)) -->$"
        $plan.issues[0].Body | Should -Not -Match "<!-- ci-scan-fingerprint: $([regex]::Escape($decoy)) -->"
    }

    It 'rejects a body that already carries the canonical fingerprint marker' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "<!-- ci-scan-fingerprint: $fingerprint -->`n$(New-TestBody -Fingerprint $fingerprint)"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must not contain scanner marker content*'
    }

    It 'rejects a body that carries a wrong fingerprint marker' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "<!-- ci-scan-fingerprint: ci-scan-net11|net11.0|maui-pr|other|other|linux -->`n$(New-TestBody -Fingerprint $fingerprint)"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must not contain scanner marker content*'
    }

    It 'rejects a body that carries duplicate fingerprint markers' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "<!-- ci-scan-fingerprint: $fingerprint -->`n<!-- ci-scan-fingerprint: $fingerprint -->`n$(New-TestBody -Fingerprint $fingerprint)"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must not contain scanner marker content*'
    }

    It 'rejects a body that carries a match-count marker' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "<!-- ci-scan-match-count: 9 hits in failure.log -->`n$(New-TestBody -Fingerprint $fingerprint)"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must not contain scanner marker content*'
    }

    It 'rejects every evasive spelling of a scanner marker' -ForEach @(
        @{ Case = 'no space after the comment open'; Marker = '<!--ci-scan-fingerprint: x -->' }
        @{ Case = 'uppercase token'; Marker = '<!-- CI-SCAN-FINGERPRINT: x -->' }
        @{ Case = 'mixed case count token'; Marker = '<!-- Ci-Scan-Match-Count: 4 hits in failure.log -->' }
        @{ Case = 'extra internal spacing'; Marker = '<!--   ci-scan-fingerprint  : x -->' }
        @{ Case = 'tab separated'; Marker = "<!--`tci-scan-fingerprint:`tx -->" }
        @{ Case = 'underscore separators'; Marker = '<!-- ci_scan_fingerprint: x -->' }
        @{ Case = 'space separators'; Marker = '<!-- ci scan fingerprint: x -->' }
        @{ Case = 'html entity encoded comment open'; Marker = '&lt;!-- ci-scan-fingerprint: x --&gt;' }
        @{ Case = 'soft hyphen inside the token'; Marker = "<!-- ci-scan-finger$([char]0x00AD)print: x -->" }
        @{ Case = 'bare token with no comment syntax'; Marker = 'ci-scan-match-count: 12 hits in failure.log' }
    ) {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n$Marker"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must not contain scanner marker content*'
    }

    It 'rejects a <Case> body carrying hidden or control content invisible to a reviewer' -ForEach @(
        @{ Case = 'ANSI/ESC escape'; Suffix = "$([char]0x1B)[31mred text"; Expect = 'C0 control character' }
        @{ Case = 'bare C0 control'; Suffix = "col$([char]0x07)umn"; Expect = 'C0 control character' }
        @{ Case = 'DEL character'; Suffix = "trailing$([char]0x7F)"; Expect = 'DEL control character' }
        @{ Case = 'C1 control (NEL)'; Suffix = "line$([char]0x0085)break"; Expect = 'C1 control character' }
        @{ Case = 'right-to-left override'; Suffix = "$([char]0x202E)dettimbus"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'zero-width non-joiner'; Suffix = "hid$([char]0x200C)den"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'byte order mark'; Suffix = "$([char]0xFEFF)prefixed"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'Arabic letter mark (bidi)'; Suffix = "sig$([char]0x061C)nal"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'Mongolian vowel separator'; Suffix = "gap$([char]0x180E)here"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'paragraph separator'; Suffix = "line$([char]0x2029)break"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'isolated variation selector-15'; Suffix = "glyph$([char]0xFE0E)"; Expect = 'isolated emoji presentation selector' }
        @{ Case = 'isolated variation selector-16'; Suffix = "glyph$([char]0xFE0F)"; Expect = 'isolated emoji presentation selector' }
        @{ Case = 'variation selector-15 after arbitrary symbol'; Suffix = ('cost $' + [char]0xFE0E); Expect = 'isolated emoji presentation selector' }
        @{ Case = 'variation selector-16 after arbitrary symbol'; Suffix = ('cost $' + [char]0xFE0F); Expect = 'isolated emoji presentation selector' }
        @{ Case = 'Mongolian free variation selector'; Suffix = "shape$([char]0x180B)here"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'combining grapheme joiner'; Suffix = "seam$([char]0x034F)less"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'Khmer inherent vowel'; Suffix = "gap$([char]0x17B4)here"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'Hangul filler'; Suffix = "blank$([char]0x3164)space"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'Hangul Choseong filler'; Suffix = "col$([char]0x115F)umn"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'Hangul Jungseong filler'; Suffix = "col$([char]0x1160)umn"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'halfwidth Hangul filler'; Suffix = "blank$([char]0xFFA0)word"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'astral tag character'; Suffix = "hidden$([char]::ConvertFromUtf32(0xE007F))"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'astral variation selector supplement'; Suffix = "mark$([char]::ConvertFromUtf32(0xE0100))"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'reserved default-ignorable Specials'; Suffix = "slot$([char]0xFFF0)here"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'unassigned default-ignorable tag-plane low'; Suffix = "hidden$([char]::ConvertFromUtf32(0xE0080))"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'unassigned default-ignorable tag-plane high'; Suffix = "hidden$([char]::ConvertFromUtf32(0xE0FFF))"; Expect = 'bidirectional or invisible format character' }
        @{ Case = 'musical format control'; Suffix = "beat$([char]::ConvertFromUtf32(0x1D173))here"; Expect = 'Unicode format character' }
        @{ Case = 'interlinear annotation anchor'; Suffix = "gloss$([char]0xFFF9)here"; Expect = 'Unicode format character' }
        @{ Case = 'shorthand format control'; Suffix = "steno$([char]::ConvertFromUtf32(0x1BCA0))here"; Expect = 'Unicode format character' }
        @{ Case = 'BMP noncharacter'; Suffix = "reserved$([char]0xFFFE)slot"; Expect = 'Unicode noncharacter' }
        @{ Case = 'astral noncharacter'; Suffix = "reserved$([char]::ConvertFromUtf32(0x1FFFE))slot"; Expect = 'Unicode noncharacter' }
        @{ Case = 'unpaired high surrogate'; Suffix = "dangling$([char]0xD800)"; Expect = 'unpaired high surrogate' }
        @{ Case = 'benign HTML comment open'; Suffix = '<!-- reviewer will not see this -->'; Expect = 'HTML comment sequence' }
        @{ Case = 'stray HTML comment close'; Suffix = 'looks fine --> but is not'; Expect = 'HTML comment sequence' }
    ) {
        # The manifest body no longer passes through gh-aw's sanitizeContent step,
        # so the trusted boundary must reject content that a human reviewer cannot
        # see rendered -- terminal escapes, invisible/bidirectional format chars,
        # and HTML comments -- rather than publish it verbatim into an issue.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n$Suffix"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw "*must not contain*$Expect*"
    }

    It 'still accepts a body whose only non-ASCII content is a legitimate tab or newline' {
        # Guardrail against over-rejection: real CI evidence routinely contains tabs
        # and newlines, and those must survive the hidden-content boundary.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n`tIndented follow-up line."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Not -Throw
    }

    It 'still accepts a body containing a legitimate astral-plane emoji' {
        # Guardrail against surrogate-pair over-rejection: an ordinary supplementary
        # -plane emoji (U+1F600) is encoded as a surrogate pair, and the scalar walk
        # must decode it to a harmless code point rather than mistaking either half
        # for an unpaired surrogate or a hidden format character.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nBuild smiled $([char]::ConvertFromUtf32(0x1F600)) at us."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Not -Throw
    }

    It 'still accepts a body containing an emoji presentation sequence' {
        # VS16 visibly selects emoji presentation for the preceding warning symbol.
        # It must not be treated like an isolated invisible selector and abort the
        # all-or-nothing publication batch.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nBuild emitted $([char]0x26A0)$([char]0xFE0F) during step 3."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Not -Throw
    }

    It 'accepts the production scanner task heading with emoji presentation' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n$([char]::ConvertFromUtf32(0x1F6E0))$([char]0xFE0F) Build Microsoft.Maui.sln"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Not -Throw
    }

    It 'still accepts a body containing a text presentation sequence' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nBuild emitted $([char]0x26A0)$([char]0xFE0E) during step 3."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Not -Throw
    }

    It 'still accepts a body with legitimate combining accents and CJK text' {
        # Guardrail against category over-rejection: the Format-category and
        # noncharacter checks must not swallow legitimate NonSpacingMark accents
        # (U+0301) or OtherLetter CJK (U+4E2D), which do appear in real evidence
        # (localized paths, author names, commit messages).
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nCafe$([char]0x0301) build for $([char]0x4E2D)$([char]0x6587) locale."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Not -Throw
    }

    It 'still accepts a body containing a visible U+FFFD replacement character' {
        # Guardrail against Specials over-rejection: the reserved default-ignorable
        # Specials reject (U+FFF0-FFF8) must stop short of U+FFFD, the replacement
        # character, which is visibly rendered and legitimately appears when CI logs
        # carry undecodable bytes. Rejecting it would refuse real evidence.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nGarbled byte $([char]0xFFFD) in log."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Not -Throw
    }

    It 'rejects a marker smuggled past NFKC folding with a noncharacter' {
        # A fullwidth-spelled marker embedded with a noncharacter (U+FFFE) makes
        # Test-MarkerLikeContent's NFKC normalization throw, so its raw-value fallback
        # never folds the fullwidth form onto the real token -- the marker gate passes.
        # The hidden-content gate must be the backstop: it rejects the U+FFFE outright,
        # so the smuggled marker can never reach publication.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $fullwidthMarker = -join ([int[]][char[]]'ciscanfingerprint' | ForEach-Object { [char]($_ + 0xFEE0) })
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n$fullwidthMarker$([char]0xFFFE)"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must not contain*Unicode noncharacter*'
    }

    It 'rejects a null body' {
        $signature = New-TestSignature
        $signature.body = $null
        $manifest = New-CompleteManifest -MainSignatures @($signature)

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw "*missing required property 'body'*"
    }

    It 'rejects an empty body' {
        $signature = New-TestSignature
        $signature.body = '   '
        $manifest = New-CompleteManifest -MainSignatures @($signature)

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*must not be empty*'
    }

    It 'rejects a non-string body' -ForEach @(
        @{ Case = 'number'; Value = 12345 }
        @{ Case = 'boolean'; Value = $true }
        @{ Case = 'object'; Value = [pscustomobject]@{ text = 'Assertion failed' } }
        @{ Case = 'array'; Value = @('Assertion failed', 'second line') }
    ) {
        $signature = New-TestSignature
        $signature.body = $Value
        $manifest = New-CompleteManifest -MainSignatures @($signature)

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*Body*must be a JSON string*'
    }

    It 'rejects a non-string title' {
        $signature = New-TestSignature
        $signature.title = [pscustomobject]@{ text = 'Sample test fails on Windows' }
        $manifest = New-CompleteManifest -MainSignatures @($signature)

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*Title*must be a JSON string*'
    }

    It 'accepts a valid canonical issue payload' {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.issues.Count | Should -Be 1
        $plan.issues[0].Title | Should -Be '[ci-scan-net11] Sample test fails on Windows'
        $plan.issues[0].MatchCount | Should -Be 2
    }

    It 'refuses to publish a filed payload without frozen evidence' {
        # The injected count has exactly one trusted source. With no frozen evidence
        # there is no count to inject, so the run must fail rather than invent one.
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*requires frozen trusted evidence to publish*'
    }

    It 'accepts a match count recomputed from frozen trusted evidence' {
        $evidenceRoot = Join-Path $TestDrive 'evidence'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot

        $plan.issues[0].MatchCount | Should -Be 2
        $plan.pipelines[0].signatures[0].match_pattern | Should -Be 'Sample scenario assertion failed'
        $plan.issues[0].EvidenceKey | Should -Match '^sha256:[0-9a-f]{64}$'
        @($plan.issues[0].EvidenceLineHashes).Count | Should -Be 1
    }

    It 'rejects a match found only in synthetic AzDO provenance framing' {
        $evidenceRoot = Join-Path $TestDrive 'header-only-evidence'
        New-TestEvidence -Root $evidenceRoot -Lines @('Different raw failure')
        Set-Content `
            -LiteralPath (Join-Path $evidenceRoot 'maui-pr/123456-1001.log') `
            -Value @('===== AzDO log 123456/1001 =====', 'Different raw failure')
        $header = '===== AzDO log 123456/1001 ====='
        $body = "$(New-TestBody)`n$header"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern '===== AzDO log' -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*must occur in trusted source log 1001*'
    }

    It 'accepts a real raw evidence line and binds it to a trusted evidence key' {
        $evidenceRoot = Join-Path $TestDrive 'raw-line-evidence'
        New-TestEvidence -Root $evidenceRoot -Lines @('Unique transport failure line')
        $body = "$(New-TestBody)`nUnique transport failure line"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern 'Unique transport failure' -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot

        $plan.issues[0].MatchCount | Should -Be 1
        $plan.issues[0].Body | Should -Match '(?m)^Unique transport failure line$'
        $plan.issues[0].Body | Should -Match '(?m)^<!-- ci-scan-evidence-key: sha256:[0-9a-f]{64} -->$'
    }

    It 'gives unrelated deadletter work items distinct trusted evidence proofs' {
        $url = 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt'
        $androidLine = "Helix work item android-emulator-boot was deadlettered: $url"
        $iosLine = "Helix work item ios-device-lost was deadlettered: $url"
        $androidRoot = Join-Path $TestDrive 'android-deadletter'
        $iosRoot = Join-Path $TestDrive 'ios-deadletter'
        New-TestEvidence `
            -Root $androidRoot `
            -Lines @($androidLine) `
            -SegmentKind 'helix-deadletter-uri' `
            -SegmentSource 'job-android/android-emulator-boot'
        New-TestEvidence `
            -Root $iosRoot `
            -Lines @($iosLine) `
            -SegmentKind 'helix-deadletter-uri' `
            -SegmentSource 'job-ios/ios-device-lost'

        $androidProof = Get-TrustedEvidenceMatchProof `
            -MatchPattern 'helix-workitem-deadletter.txt' `
            -PipelineName 'maui-pr' `
            -BuildId 123456 `
            -Fingerprint 'android-deadletter' `
            -SourceLogIds @(1001) `
            -TrustedEvidencePath $androidRoot
        $iosProof = Get-TrustedEvidenceMatchProof `
            -MatchPattern 'helix-workitem-deadletter.txt' `
            -PipelineName 'maui-pr' `
            -BuildId 123456 `
            -Fingerprint 'ios-deadletter' `
            -SourceLogIds @(1001) `
            -TrustedEvidencePath $iosRoot

        $androidProof.EvidenceKey | Should -Not -BeExactly $iosProof.EvidenceKey
        $androidProof.EvidenceLineHashes[0] | Should -Not -BeExactly $iosProof.EvidenceLineHashes[0]
    }

    It 'keeps real failure identity stable across builds' {
        $line = 'System.NullReferenceException in Microsoft.Maui.DeviceTests.ButtonTests'
        $firstRoot = Join-Path $TestDrive 'real-failure-first'
        $secondRoot = Join-Path $TestDrive 'real-failure-second'
        New-TestEvidence -Root $firstRoot -BuildId 900001 -Lines @($line)
        New-TestEvidence -Root $secondRoot -BuildId 900002 -Lines @($line)

        $firstProof = Get-TrustedEvidenceMatchProof `
            -MatchPattern 'NullReferenceException' `
            -PipelineName 'maui-pr' `
            -BuildId 900001 `
            -Fingerprint 'first-real-failure' `
            -SourceLogIds @(1001) `
            -TrustedEvidencePath $firstRoot
        $secondProof = Get-TrustedEvidenceMatchProof `
            -MatchPattern 'NullReferenceException' `
            -PipelineName 'maui-pr' `
            -BuildId 900002 `
            -Fingerprint 'second-real-failure' `
            -SourceLogIds @(1001) `
            -TrustedEvidencePath $secondRoot

        $firstProof.EvidenceKey | Should -BeExactly $secondProof.EvidenceKey
        $firstProof.EvidenceLineHashes | Should -BeExactly $secondProof.EvidenceLineHashes
    }

    It 'keeps AzDO task-command identity stable across transport timestamps' {
        $firstLine = '2026-07-20T18:34:13.9100750Z ##[error]Path does not exist: artifacts/bin'
        $secondLine = '2026-07-29T03:04:05.1234567Z ##[error]Path does not exist: artifacts/bin'
        $firstRoot = Join-Path $TestDrive 'timestamped-failure-first'
        $secondRoot = Join-Path $TestDrive 'timestamped-failure-second'
        New-TestEvidence -Root $firstRoot -BuildId 900001 -Lines @($firstLine)
        New-TestEvidence -Root $secondRoot -BuildId 900002 -Lines @($secondLine)

        $firstProof = Get-TrustedEvidenceMatchProof `
            -MatchPattern 'Path does not exist' `
            -PipelineName 'maui-pr' `
            -BuildId 900001 `
            -Fingerprint 'first-timestamped-failure' `
            -SourceLogIds @(1001) `
            -TrustedEvidencePath $firstRoot
        $secondProof = Get-TrustedEvidenceMatchProof `
            -MatchPattern 'Path does not exist' `
            -PipelineName 'maui-pr' `
            -BuildId 900002 `
            -Fingerprint 'second-timestamped-failure' `
            -SourceLogIds @(1001) `
            -TrustedEvidencePath $secondRoot

        $firstProof.EvidenceKey | Should -BeExactly $secondProof.EvidenceKey
        $firstProof.EvidenceLineHashes | Should -BeExactly $secondProof.EvidenceLineHashes
    }

    It 'binds a timestamped AzDO log to the timestamp-free issue excerpt' {
        $evidenceRoot = Join-Path $TestDrive 'timestamped-body-binding'
        $trustedLine = '2026-07-20T18:34:13.9100750Z ##[error]Path does not exist: artifacts/bin'
        $bodyLine = '##[error]Path does not exist: artifacts/bin'
        New-TestEvidence -Root $evidenceRoot -Lines @($trustedLine)
        $body = (New-TestBody).Replace('Sample scenario assertion failed', $bodyLine)
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern 'Path does not exist' -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot

        $plan.issues[0].MatchCount | Should -Be 1
        $plan.issues[0].Body | Should -Match "(?m)^$([regex]::Escape($bodyLine))$"
    }

    It 'preserves timestamps that are part of the failure message' {
        $message = '2026-07-20T18:34:13.9100750Z server clock skew exceeded threshold'

        ConvertTo-EvidenceIdentityLine -Value $message |
            Should -BeExactly $message.ToLowerInvariant()
        ConvertTo-EvidenceIdentityLine -Value $message -StripAzdoTransportTimestamp |
            Should -BeExactly 'server clock skew exceeded threshold'
    }

    It 'rejects more than 200 distinct matching evidence lines before publication' {
        $evidenceRoot = Join-Path $TestDrive 'excess-evidence-lines'
        $lines = 1..201 | ForEach-Object { "Unique failure line $_" }
        New-TestEvidence -Root $evidenceRoot -Lines $lines
        $body = "$(New-TestBody)`nUnique failure line 1"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern 'Unique failure line' -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*exceeds the 200 distinct evidence-line safety limit*'
    }

    It 'rejects marker-like match patterns before evidence lookup' -ForEach @(
        @{ Case = 'exact'; Pattern = 'ci-scan-fingerprint' }
        @{ Case = 'spacing'; Pattern = 'ci scan fingerprint' }
        @{ Case = 'case'; Pattern = 'CI-SCAN-FINGERPRINT' }
        @{ Case = 'zero width'; Pattern = "ci-scan-finger$([char]0x200B)print" }
        @{ Case = 'Unicode homoglyph'; Pattern = "c$([char]0x0456)-scan-finger$([char]0x0440)rint" }
        @{ Case = 'uppercase Unicode homoglyph'; Pattern = "$([char]0x0421)$([char]0x0406)-SCAN-FINGER$([char]0x0420)RINT" }
        @{ Case = 'evidence marker'; Pattern = 'ci-scan-evidence-key' }
    ) {
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern $Pattern)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*match_pattern*'
    }

    It 'injects the frozen evidence count, ignoring any count the agent implies' {
        # The agent cannot supply a count at all any more; whatever the body says in
        # prose, the injected marker must equal the trusted recount.
        $evidenceRoot = Join-Path $TestDrive 'single-hit-evidence'
        New-TestEvidence -Root $evidenceRoot -Lines @('Sample scenario assertion failed')
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample scenario test|sample scenario assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`
Observed 99 times according to the agent."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot

        $plan.issues[0].MatchCount | Should -Be 1
        $plan.issues[0].Body | Should -Match '(?m)^<!-- ci-scan-match-count: 1 hits in failure\.log -->$'
    }

    It 'rejects a source log that does not contain the filed match pattern' {
        $evidenceRoot = Join-Path $TestDrive 'multi-log-evidence'
        New-TestEvidence -Root $evidenceRoot -LogId 1001
        New-TestEvidence -Root $evidenceRoot -LogId 1002 -Lines @('Different failure')
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -SourceLogIds @(1001, 1002))
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds -MainRequiredLogIds @(1001, 1002)) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*must occur in trusted source log 1002*'
    }

    It 'rejects a match pattern absent from frozen trusted evidence' {
        $evidenceRoot = Join-Path $TestDrive 'evidence'
        New-TestEvidence -Root $evidenceRoot -Lines @('Different failure')
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*must occur in trusted source log 1001*'
    }

    It 'rejects a missing frozen evidence file' {
        $evidenceRoot = Join-Path $TestDrive 'missing-evidence'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*Trusted evidence file is missing*'
    }

    It 'injects an evidence-verified match pattern omitted from the body' {
        $evidenceRoot = Join-Path $TestDrive 'omitted-pattern-evidence'
        New-TestEvidence -Root $evidenceRoot -Lines @('Different transport failure')
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern 'Different transport failure')
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath $evidenceRoot

        $plan.issues[0].Body |
            Should -Match '(?ms)## Error Message\r?\n\r?\n    Different transport failure$'
    }

    It 'does not treat trusted state inside Error Message as historical failure evidence' {
        $pattern = '- **Pipeline**: maui-pr-devicetests'
        $body = "## Error Message`n$pattern"

        (Test-HistoricalErrorPattern -Body $body -MatchPattern $pattern) |
            Should -BeFalse
    }

    It 'treats case-variant state-like text as ordinary historical evidence' {
        $pattern = '- **pipeline**: maui-pr-devicetests'
        $body = "## Error Message`n$pattern"

        (Test-HistoricalErrorPattern -Body $body -MatchPattern $pattern) |
            Should -BeTrue
    }

    It 'requires a distinctive fingerprint before creating a canonical issue' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|test|failure|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw "*Fingerprint '$fingerprint' has no distinctive identity or failure-category tokens*"
    }

    It 'rejects a generic filed recurrence pattern before creating a canonical issue' {
        $pattern = 'Build FAILED.'
        $body = "$(New-TestBody)`n`n## Error Message`n`n    $pattern"
        $evidenceRoot = Join-Path $TestDrive 'generic-filed-pattern'
        New-TestEvidence -Root $evidenceRoot -Lines @($pattern)
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern $pattern -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*must contain at least two distinctive tokens or one token of at least 16 characters*'
    }

    It 'rejects a single common token as a filed recurrence pattern' {
        $fingerprint = 'ci-scan|main|maui-pr-devicetests|carouselviewdoesnotleakwithdefaultitemslayout|reference to microsoft.maui.controls.carouselview is still alive|maccatalyst'
        $pattern = 'CarouselView'
        $body = (New-TestBody -Pipeline 'maui-pr-devicetests' -Fingerprint $fingerprint).
            Replace('Sample scenario assertion failed', $pattern)
        $evidenceRoot = Join-Path $TestDrive 'single-token-filed-pattern'
        New-TestEvidence `
            -Root $evidenceRoot `
            -Pipeline 'maui-pr-devicetests' `
            -Lines @($pattern)
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -BuildId 123456)
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -BuildId 123456 -Signatures @(
                        (New-TestSignature `
                                -Pipeline 'maui-pr-devicetests' `
                                -Fingerprint $fingerprint `
                                -MatchPattern $pattern `
                                -Body $body)
                    ))
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -BuildId 123458)
            )
        }

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath $evidenceRoot `
                -ScannerId 'ci-scan' } |
            Should -Throw '*must contain at least two distinctive tokens or one token of at least 16 characters*'
    }

    It 'accepts one long distinctive token as a filed recurrence pattern' {
        $pattern = 'ArtifactAlreadyExists'
        $body = (New-TestBody).Replace('Sample scenario assertion failed', $pattern)
        $evidenceRoot = Join-Path $TestDrive 'long-token-filed-pattern'
        New-TestEvidence -Root $evidenceRoot -Lines @($pattern)
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern $pattern -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath $evidenceRoot

        $plan.pipelines[0].signatures[0].match_pattern | Should -BeExactly $pattern
    }

    It 'repairs the exact main production body and match_pattern mismatch' {
        $pipeline = 'maui-pr-devicetests'
        $buildId = 1540065
        $fingerprint = 'ci-scan|main|maui-pr-devicetests|carouselviewdoesnotleakwithdefaultitemslayout|reference to microsoft.maui.controls.carouselview is still alive|maccatalyst'
        $matchPattern = 'XHarness exit code: 1 (TESTS_FAILED)'
        $body = (New-TestBody -Pipeline $pipeline -BuildId $buildId -Fingerprint $fingerprint).
            Replace('Assertion failed', 'Reference to Microsoft.Maui.Controls.CarouselView is still alive')
        $signature = New-TestSignature `
            -Pipeline $pipeline `
            -BuildId $buildId `
            -Fingerprint $fingerprint `
            -Title 'DeviceTestsMacCatalyst Controls Tests fail: CarouselView memory leak' `
            -MatchPattern $matchPattern `
            -Body $body
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -BuildId 1540066)
                (New-TestPipeline -Name $pipeline -DefinitionId 314 -BuildId $buildId -Signatures @($signature))
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -BuildId 1540063)
            )
        }
        $evidenceRoot = Join-Path $TestDrive 'main-production-pattern-evidence'
        New-TestEvidence `
            -Root $evidenceRoot `
            -Pipeline $pipeline `
            -BuildId $buildId `
            -Lines @($matchPattern)

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath $evidenceRoot `
            -ScannerId 'ci-scan'

        $plan.issues[0].Body |
            Should -Match "(?ms)## Error Message\r?\n\r?\n    $([regex]::Escape($matchPattern))$"
        $plan.issues[0].MatchCount | Should -Be 1
    }

    It 'rejects trusted match-pattern injection that would exceed the body limit' {
        $body = (New-TestBody).Replace('Sample scenario assertion failed', 'Different transport failure')
        $body += 'a' * (59000 - $body.Length)
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Body $body)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw '*exceeds 59000 characters after trusted match_pattern injection*'
    }

    It 'requires the safely rendered match pattern in the published body' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $pattern = 'Failure for user@example'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n$pattern"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -MatchPattern $pattern -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot -ExtraLines @($pattern))

        $plan.issues[0].Body | Should -Not -Match 'user@example'
        $plan.issues[0].Body | Should -Match "user@$([char]0x200B)example"
    }

    It 'keeps the counted evidence line in a published body that neutralization rewrote' {
        # Regression: the evidence invariant used to be asserted against the raw
        # agent-supplied body, but the body that is actually filed is the neutralized
        # one. A native crash backtrace is the common real case - "#0 0x..." trips the
        # #ref rule - so the published body never carries the counted line verbatim.
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $pattern = '#0 0x00007fff9c3d1abc in maui_crash'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n$pattern"
        $evidenceRoot = Join-Path $TestDrive 'zwsp-evidence'
        New-TestEvidence -Root $evidenceRoot -Lines @($pattern, $pattern)
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body -MatchPattern $pattern)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot

        $published = $plan.issues[0].Body
        $zeroWidthSpace = [string][char]0x200B
        # Contract: neutralization DID rewrite the evidence line ...
        $published.Contains($pattern, [System.StringComparison]::Ordinal) | Should -BeFalse
        $published | Should -Match "#$([char]0x200B)0 0x00007fff9c3d1abc"
        # ... and it did so only by inserting zero-width spaces, so the line survives.
        $published.Replace($zeroWidthSpace, '').Contains($pattern, [System.StringComparison]::Ordinal) |
            Should -BeTrue
        $plan.issues[0].MatchCount | Should -Be 2
    }

    It 'rejects a match pattern that smuggles in a zero-width space' {
        # The body itself is clean, so this exercises the match_pattern guard rather
        # than the body guard.
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern "Assertion$([char]0x200B) failed")
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*match_pattern*must not contain zero-width spaces*'
    }

    It 'rejects hidden content in a match pattern before trusted injection' {
        $pattern = "Failure$([char]0x1B)[31m"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -MatchPattern $pattern)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot -ExtraLines @($pattern)) } |
            Should -Throw '*match_pattern*C0 control character*'
    }

    It 'rejects a body that smuggles in a zero-width space' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nSmuggled:$([char]0x200B) Assertion failed"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*Body*must not contain zero-width spaces*'
    }

    It 'neutralizes user and team mentions in the validated issue body' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nOwner: @octocat and @dotnet/maui."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.issues[0].Body | Should -Not -Match '@octocat|@dotnet/maui'
        $plan.issues[0].Body | Should -Match "@$([char]0x200B)octocat"
        $plan.issues[0].Body | Should -Match "@$([char]0x200B)dotnet/maui"
    }

    It 'neutralizes issue and pull-request cross references in the validated body' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $body = "$(New-TestBody -Fingerprint $fingerprint)`nRelated: #123, dotnet/maui#456, https://github.com/dotnet/maui/issues/789, https://github.com/dotnet/maui/pull/1011."
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -TrustedEvidencePath (New-DefaultEvidenceRoot)

        $plan.issues[0].Body | Should -Not -Match '(?<![\w/])#\d|dotnet/maui#\d|github\.com/dotnet/maui/(?:issues|pull)/\d'
    }

    It 'rejects a body that exceeds the limit after notification neutralization' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $mentions = -join (1..29000 | ForEach-Object { '@a' })
        $body = "$(New-TestBody -Fingerprint $fingerprint)`n$mentions"
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Fingerprint $fingerprint -Body $body)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*must be 20-59000 characters*'
    }

    It 'rejects a Build ID marker that differs from the pipeline build' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Fingerprint $fingerprint `
                    -Body (New-TestBody -Fingerprint $fingerprint -BuildId 999999))
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
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

    It 'rejects a non-distinctive recurrence pattern on an existing issue' {
        $pattern = 'Build FAILED.'
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'existing' `
                    -IssueNumber 36827 `
                    -MatchPattern $pattern)
        )

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw '*must contain at least two distinctive tokens or one token of at least 16 characters*'
    }

    It 'accepts an existing issue only when its pattern occurs in frozen evidence' {
        $evidenceRoot = Join-Path $TestDrive 'existing-evidence'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Disposition 'existing' -IssueNumber 36827)
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot

        $plan.pipelines[0].signatures[0].match_pattern | Should -Be 'Sample scenario assertion failed'
        $plan.pipelines[0].signatures[0].match_count | Should -Be 2
    }

    It 'rejects an existing issue pattern absent from frozen evidence' {
        $evidenceRoot = Join-Path $TestDrive 'existing-mismatch'
        New-TestEvidence -Root $evidenceRoot -Lines @('Different failure')
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature -Disposition 'existing' -IssueNumber 36827)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*must occur in trusted source log 1001*'
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
                @{ type = 'submit_ci_scan' },
                @{ type = 'noop'; body = 'alternate output' }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        { Assert-ScannerSubmissionFromAgentOutput -Path $path } |
            Should -Throw '*exactly one item of type submit_ci_scan and no alternate outputs*'
    }

    It 'rejects the production nested-string transport even when its JSON is valid' {
        $path = Join-Path $TestDrive 'nested-manifest.json'
        @{
            items = @(
                @{ type = 'submit_ci_scan'; manifest = '{"pipelines":[]}' }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        { Assert-ScannerSubmissionFromAgentOutput -Path $path } |
            Should -Throw '*authorization-only and must not contain manifest data or a path*'
    }

    It 'rejects agent selection of an arbitrary manifest path' {
        $path = Join-Path $TestDrive 'manifest-path.json'
        @{
            items = @(
                @{ type = 'submit_ci_scan'; manifest_path = '/tmp/gh-aw/agent/other.json' }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        { Assert-ScannerSubmissionFromAgentOutput -Path $path } |
            Should -Throw '*authorization-only and must not contain manifest data or a path*'
    }

    It 'accepts exactly one argument-free submission authorization' {
        $path = Join-Path $TestDrive 'authorization.json'
        @{
            items = @(
                @{ type = 'submit_ci_scan' }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        { Assert-ScannerSubmissionFromAgentOutput -Path $path } | Should -Not -Throw
    }

    It 'rejects a lone valid submission when the collector also recorded a rejected attempt' {
        # gh-aw's collector diverts a duplicate or argument-carrying submit_ci_scan
        # into a sibling `.errors` array while leaving one clean item in `.items`.
        # Inspecting only `.items` would let the run look successful, so a non-empty
        # `.errors` must fail the gate on its own.
        $path = Join-Path $TestDrive 'collector-errors.json'
        @{
            items  = @(
                @{ type = 'submit_ci_scan' }
            )
            errors = @(
                @{ type = 'submit_ci_scan'; message = 'rejected: exceeds max of 1' }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        { Assert-ScannerSubmissionFromAgentOutput -Path $path } |
            Should -Throw '*collector reported 1 rejected submission attempt*'
    }

    It 'accepts a submission when the collector errors array is present but empty' {
        # An empty `.errors` array is the normal shape and must not trip the gate.
        $path = Join-Path $TestDrive 'empty-errors.json'
        @{
            items  = @(
                @{ type = 'submit_ci_scan' }
            )
            errors = @()
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        { Assert-ScannerSubmissionFromAgentOutput -Path $path } | Should -Not -Throw
    }

    It 'reads multiline issue bodies from the fixed manifest file without nested JSON transport' {
        $path = Join-Path $TestDrive 'manifest_final.json'
        $body = "## Summary`nFirst line.`n`n## Error Message`nLiteral `"quoted`" line."
        @{
            pipelines = @(
                @{
                    name       = 'maui-pr'
                    signatures = @(@{ body = $body })
                }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path

        $result = Get-ScannerManifestFromFile -Path $path

        $result.pipelines[0].signatures[0].body | Should -BeExactly $body
    }

    It 'rejects a fixed manifest file over the byte limit' {
        $path = Join-Path $TestDrive 'oversized-manifest.json'
        Set-Content -LiteralPath $path -Value ('x' * 500001) -NoNewline

        { Get-ScannerManifestFromFile -Path $path } |
            Should -Throw '*exceeds the 500000 byte limit*'
    }

    It 'accepts a well-formed fixed manifest file' {
        $path = Join-Path $TestDrive 'good-manifest.json'
        Set-Content -LiteralPath $path -Value '{"pipelines":[]}'

        $result = Get-ScannerManifestFromFile -Path $path
        @($result.pipelines).Count | Should -Be 0
    }
}

Describe 'CI scanner workflow source invariants: <_>' -ForEach @('ci-status-main', 'ci-status-net11') {
    BeforeAll {
        # Both twins are held to the same source invariants; the main scanner used
        # to rely on the permissive built-in create-issue output and had none of
        # these guarantees.
        $workflowName = $_
        $workflowPath = Join-Path (Split-Path $PSScriptRoot -Parent) "workflows/$workflowName.md"
        $workflowSource = Get-Content -LiteralPath $workflowPath -Raw
    }

    It 'routes every issue write through the validating custom publisher' {
        $workflowSource | Should -Match '(?m)^\s+submit-ci-scan:$'
        $workflowSource | Should -Match 'Validate-CiScanManifest\.ps1'
        $workflowSource | Should -Match '(?m)^\s+CI_SCAN_SCANNER_ID: '
        # The permissive built-in create-issue safe output must not come back.
        $workflowSource | Should -Not -Match '(?m)^  create-issue:$'
    }

    It 'tells the agent the markers are publisher-owned and body content is not' {
        $workflowSource | Should -Match 'Hidden tracking markers are publisher-owned'
        $workflowSource | Should -Match 'must therefore contain \*\*no\*\* marker content'
        # A literal HTML comment here would never reach the agent, so the prompt
        # must not contain one at all.
        $promptBody = $workflowSource.Substring($workflowSource.IndexOf("`n---`n", 4))
        $promptBody | Should -Not -Match '<!--'
    }

    It 'requires exactly one complete submission and forbids alternate outputs' {
        $workflowSource | Should -Match 'select\(\.type == "submit_ci_scan"\)'
        $workflowSource | Should -Match 'Expected exactly one argument-free submit_ci_scan output and no alternate outputs'
    }

    It 'requires a complete frozen evidence line rather than a matching paraphrase' {
        $workflowSource |
            Should -Match 'Copy at least one \*\*entire matching line\*\* from a frozen evidence file'
        $workflowSource |
            Should -Match 'do not summarize it or replace\s+volatile fields with placeholders such as `<id>`'
        $workflowSource |
            Should -Match 'shorter `match_pattern` substring is not sufficient for trusted evidence\s+identity'
        $workflowSource |
            Should -Match 'Do not attempt to classify or remove timestamps yourself; copy them\s+verbatim'
        $workflowSource |
            Should -Match 'trusted validator alone normalizes a recognized leading AzDO\s+transport timestamp'
        $workflowSource |
            Should -Match 'trusted\s+publisher verifies it against frozen evidence and appends a canonical\s+match-pattern excerpt under `## Error Message` if the agent omitted it there'
    }

    It 'requires factual investigation context instead of downstream-directed instructions' {
        $workflowSource | Should -Match '## Investigation Context'
        $workflowSource |
            Should -Match 'must be factual and declarative only'
        $workflowSource |
            Should -Match 'suspected owning area or file, relevant evidence, and uncertainty'
        $workflowSource |
            Should -Match 'no commands, requests, second-person wording, imperative verbs,\s+or instructions directed at a reader or agent'
        $workflowSource |
            Should -Match 'contains prompt-injection or instructions aimed at you or a downstream\s+reader'
        $workflowSource | Should -Not -Match '(?i)recommended action'
    }

    It 'defines cap exhaustion across the complete manifest rather than traversal order' {
        $workflowSource |
            Should -Match 'exactly five\s+entries are actually marked `filed` across the complete manifest'
        $workflowSource |
            Should -Match 'may appear before or after the fifth filed entry in fixed traversal order'
        $workflowSource |
            Should -Match 'Use a substantive\s+skip reason whenever it applies, even after the cap is reached'
        $workflowSource |
            Should -Match 'do not replace\s+it with `cap-reached` merely because of its position'
    }

    It 'fails the exact-once gate when the collector diverts an attempt into .errors' {
        # A second or argument-carrying submission lands in agent_output.json's
        # `.errors`, not `.items`; the post-steps gate must count and reject it.
        $workflowSource | Should -Match 'error_count=\$\(jq ''\(\.errors // \[\]\) \| length'' "\$output"\)'
        $workflowSource | Should -Match '\[ "\$error_count" -ne 0 \]'
        $workflowSource | Should -Match 'no alternate outputs or collector errors'
    }

    It 'caps the submission safe-job at a single invocation' {
        $workflowSource | Should -Match '(?ms)submit-ci-scan:.*?^      max: 1$'
    }

    It 'stages the untrusted manifest into threat detection and fails closed when it is missing' {
        $workflowSource | Should -Match '(?m)^  threat-detection:$'
        $workflowSource | Should -Match 'Stage scanner manifest for threat detection'
        $workflowSource | Should -Match '\[ -L "\$manifest" \] \|\| \[ ! -f "\$manifest" \]'
        $workflowSource | Should -Match 'manifest_size=\$\(stat -c ''%s'' -- "\$manifest"\)'
        $workflowSource | Should -Match '\[ "\$manifest_size" -eq 0 \] \|\| \[ "\$manifest_size" -gt 500000 \]'
        $workflowSource | Should -Match 'cp --no-dereference -- "\$manifest" "\$staged"'
        $workflowSource | Should -Match '\[ -L "\$staged" \] \|\| \[ ! -f "\$staged" \]'
        $workflowSource | Should -Match '\[ "\$staged_size" -ne "\$manifest_size" \]'
        $workflowSource | Should -Match 'submit_ci_scan was authorized but manifest_final\.json is missing'
        # The fail-closed decision must key off the download-independent output_types
        # job output, not the continue-on-error agent_output.json download, so a
        # transient artifact-download failure cannot silently skip manifest scanning.
        $workflowSource | Should -Match 'OUTPUT_TYPES: \$\{\{ needs\.agent\.outputs\.output_types \}\}'
        $workflowSource | Should -Match '\$OUTPUT_TYPES.*==.*\*submit_ci_scan\*'
        $workflowSource | Should -Not -Match 'output=./tmp/gh-aw/agent_output\.json.\s*\r?\n\s*mkdir'
        # The detection prompt must name the staged file so the AI engine scans it.
        $workflowSource | Should -Match '/tmp/gh-aw/threat-detection/manifest_final\.json'
        # AI detection must stay enabled (no `engine: false` under threat-detection).
        $workflowSource | Should -Not -Match '(?ms)^  threat-detection:.*?^\s+engine: false'
    }

    It 'keeps threat detection aligned with the trusted variation-selector rule' {
        $validatorPath = Join-Path $PSScriptRoot 'Validate-CiScanManifest.ps1'
        $validatorSource = Get-Content -LiteralPath $validatorPath -Raw
        $validatorMatch = [regex]::Match(
            $validatorSource,
            '(?s)\$isEmojiVariationBase = \$previousCode -in @\((?<bases>.*?)\r?\n\s+\)')

        $validatorMatch.Success | Should -BeTrue

        $validatorBases = @(
            [regex]::Matches($validatorMatch.Groups['bases'].Value, '0x(?<code>[0-9A-F]+)') |
                ForEach-Object { "U+$($_.Groups['code'].Value)" }
        )

        $lockPath = Join-Path (Split-Path $PSScriptRoot -Parent) "workflows/$workflowName.lock.yml"
        $lockSource = Get-Content -LiteralPath $lockPath -Raw
        $compiledPromptMatch = [regex]::Match(
            $lockSource,
            '(?m)^\s+CUSTOM_PROMPT: (?<json>".*")$')
        $compiledPromptMatch.Success | Should -BeTrue
        $compiledPrompt = [System.Text.Json.JsonSerializer]::Deserialize[string](
            $compiledPromptMatch.Groups['json'].Value)

        foreach ($prompt in @($workflowSource, $compiledPrompt)) {
            $promptMatch = [regex]::Match(
                $prompt,
                'Approved VS15/VS16 bases \(exactly\): (?<bases>U\+[0-9A-F]+(?:, U\+[0-9A-F]+)*)\.')

            $promptMatch.Success | Should -BeTrue
            @($promptMatch.Groups['bases'].Value -split ', ') |
                Should -BeExactly $validatorBases
            $prompt |
                Should -Match 'Do not flag VS15 \(U\+FE0E\) or\s+VS16 \(U\+FE0F\) solely when it immediately follows one of the approved bases'
            $prompt |
                Should -Match 'Flag an isolated VS15/VS16 or a selector following any other base\.'
        }
    }

    It 'uses one bounded fixed same-run artifact file and no tool-selected transport' {
        $workflowSource |
            Should -Match 'CI_SCAN_MANIFEST_PATH: \$\{\{ runner\.temp \}\}/gh-aw/safe-jobs/agent/manifest_final\.json'
        $workflowSource | Should -Match '/tmp/gh-aw/agent/manifest_final\.json'
        $workflowSource | Should -Match 'same-run `agent` artifact'
        $workflowSource | Should -Match 'argument-free `submit_ci_scan`'
        $workflowSource | Should -Match 'jq -e \. /tmp/gh-aw/agent/manifest_final\.json >/dev/null'
        $workflowSource | Should -Not -Match '(?ms)^\s{6}inputs:\s*\r?\n\s{8}(?:manifest|manifest_path):'
        $workflowSource | Should -Not -Match 'one `manifest` argument'
    }

    It 'keeps custom publisher staging identical to framework staging' {
        $framework = [regex]::Match($workflowSource, '(?m)^  staged: (.+)$')
        $publisher = [regex]::Match($workflowSource, '(?m)^        GH_AW_SAFE_OUTPUTS_STAGED: (.+)$')

        $framework.Success | Should -BeTrue
        $publisher.Success | Should -BeTrue
        $publisher.Groups[1].Value | Should -BeExactly $framework.Groups[1].Value
    }

    It 'keeps retry adoption and artifact overwrite enabled' {
        $workflowSource | Should -Match 'retry_reused: true'
        [regex]::Matches($workflowSource, '(?m)^\s+overwrite: true$').Count | Should -Be 2
    }

    It 'points the agent at the actual trusted inventory path' {
        $workflowSource | Should -Match '/tmp/gh-aw/agent/trusted/expected-builds\.json'
        $workflowSource | Should -Not -Match '/tmp/gh-aw/agent/expected-builds\.json'
    }

    It 'freezes a trusted publisher SHA before the agent and checks it out exactly' {
        $workflowSource | Should -Match "const trustedPublisherRef = '\$\{\{ github\.workflow_sha \}\}'"
        $workflowSource | Should -Match 'trusted_publisher_ref: trustedPublisherRef'
        $workflowSource | Should -Match 'ref: \$\{\{ steps\.trusted_publisher_ref\.outputs\.ref \}\}'
        $workflowSource | Should -Not -Match '(?m)^\s+ref: main$'
        $workflowSource | Should -Not -Match "ref: 'heads/main'"
    }

    It 'fails closed on incomplete Helix work-item evidence' {
        $workflowSource | Should -Match 'attempt <= 6'
        $workflowSource | Should -Match 'items\.length >= finishedCount'
        $workflowSource | Should -Match 'const terminalItems = items\.every'
        $workflowSource | Should -Match 'waitingCount === 0'
        $workflowSource | Should -Match 'runningCount === 0'
        $workflowSource | Should -Not -Match 'unscheduledCount === 0'
        $workflowSource | Should -Match 'did not provide complete terminal work-item evidence'
        $workflowSource | Should -Match "state !== 'finished' && state !== 'failed'"
        $workflowSource | Should -Match 'workItem\.ExitCode !== null'
        $workflowSource | Should -Match 'Failed Helix work item .* has no console output'
    }
}

Describe 'CI scanner skip disposition evidence proof' {
    It 'rejects an all-skipped manifest that never reads the trusted evidence' {
        # Every real failure marked not-actionable used to satisfy terminal coverage
        # with zero filed issues and no evidence opened.
        $evidenceRoot = Join-Path $TestDrive 'skip-fabricated'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'skipped' `
                    -SkipReason 'not-actionable' `
                    -MatchPattern 'Totally invented failure text')
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*must occur in trusted source log 1001*'
    }

    It 'requires a match_pattern on skipped signatures' {
        $signature = New-TestSignature -Disposition 'skipped' -SkipReason 'not-actionable'
        $signature.PSObject.Properties.Remove('match_pattern')
        $manifest = New-CompleteManifest -MainSignatures @($signature)

        { Test-CiScanManifest -Manifest $manifest } |
            Should -Throw "*missing required property 'match_pattern'*"
    }

    It 'accepts a skip proven against the trusted evidence' {
        $evidenceRoot = Join-Path $TestDrive 'skip-proven'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'skipped' `
                    -SkipReason 'infrastructure-noise' `
                    -MatchPattern 'Sample scenario assertion failed')
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds) `
            -TrustedEvidencePath $evidenceRoot

        $plan.filed_count | Should -Be 0
        $plan.pipelines[0].signatures[0].match_count | Should -Be 2
    }

    It 'rejects signature-not-in-fetched-log contradicted by the trusted evidence' {
        $evidenceRoot = Join-Path $TestDrive 'skip-contradicted'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'skipped' `
                    -SkipReason 'signature-not-in-fetched-log' `
                    -MatchPattern 'Sample scenario assertion failed')
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds -MainFailedLeafLogIds @()) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*is contradicted by trusted source log 1001*'
    }

    It 'accepts signature-not-in-fetched-log for a required log that did not fail' {
        # The legitimate case: a required log that is not a failed leaf (e.g. a green
        # Helix submission task that still has to be inspected). An absence proof is
        # the only proof available there, because there is no failure to cite.
        $evidenceRoot = Join-Path $TestDrive 'skip-absent'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'skipped' `
                    -SkipReason 'signature-not-in-fetched-log' `
                    -MatchPattern 'No usable failure signature')
        )

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds (New-ExpectedBuilds -MainFailedLeafLogIds @()) `
            -TrustedEvidencePath $evidenceRoot

        $plan.pipelines[0].signatures[0].match_count | Should -Be 0
    }

    It 'rejects signature-not-in-fetched-log that dismisses a genuinely failed log' {
        # Regression: an absence proof is satisfiable by any fabricated pattern, so it
        # establishes nothing about the failure. Allowing it on a failed-leaf log let an
        # agent dismiss a real required-log failure with a strawman pattern and still
        # satisfy the complete-coverage gate having filed zero issues.
        $evidenceRoot = Join-Path $TestDrive 'skip-absent-failed-leaf'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'skipped' `
                    -SkipReason 'signature-not-in-fetched-log' `
                    -MatchPattern 'zzzzzzzz-not-present-anywhere')
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds -MainFailedLeafLogIds @(1001)) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*cannot use signature-not-in-fetched-log for failed log IDs: 1001*'
    }

    It 'rejects signature-not-in-fetched-log on a failed leaf mixed with a clean required log' {
        $evidenceRoot = Join-Path $TestDrive 'skip-absent-mixed'
        New-TestEvidence -Root $evidenceRoot -LogId 1001 -Lines @('Green submission log')
        New-TestEvidence -Root $evidenceRoot -LogId 1002 -Lines @('Green submission log')
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'skipped' `
                    -SkipReason 'signature-not-in-fetched-log' `
                    -SourceLogIds @(1001, 1002) `
                    -MatchPattern 'zzzzzzzz-not-present-anywhere')
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds `
                    -MainFailedRecordCount 2 `
                    -MainRequiredLogIds @(1001, 1002) `
                    -MainFailedLeafLogIds @(1002)) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*cannot use signature-not-in-fetched-log for failed log IDs: 1002*'
    }

    It 'rejects a trusted failed_leaf_log_ids entry outside required_log_ids' {
        $evidenceRoot = Join-Path $TestDrive 'failed-leaf-out-of-range'
        New-TestEvidence -Root $evidenceRoot
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature)
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds -MainFailedLeafLogIds @(1001, 9999)) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*failed_leaf_log_ids entry 9999 is not in required_log_ids*'
    }

    It 'rejects a skip whose trusted evidence file is missing entirely' {
        $evidenceRoot = Join-Path $TestDrive 'skip-missing-evidence'
        New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
        $manifest = New-CompleteManifest -MainSignatures @(
            (New-TestSignature `
                    -Disposition 'skipped' `
                    -SkipReason 'not-recurring' `
                    -MatchPattern 'Assertion failed')
        )

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds (New-ExpectedBuilds) `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*Trusted evidence file is missing*'
    }
}

Describe 'CI scanner issue cap limits filing, not scanning' {
    It 'rejects skipped-cap-reached as a pipeline status' {
        $signatures = for ($i = 1; $i -le 5; $i++) {
            $fingerprint = "ci-scan-net11|net11.0|maui-pr|sample test $i|assertion failed|windows"
            New-TestSignature -Fingerprint $fingerprint -Body (New-TestBody -Fingerprint $fingerprint)
        }
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -Signatures $signatures)
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -Status 'skipped-cap-reached')
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -Status 'skipped-cap-reached')
            )
        }

        { Test-CiScanManifest `
                -Manifest $manifest `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) } |
            Should -Throw "*has invalid status 'skipped-cap-reached'*"
    }

    It 'still enforces terminal coverage for later pipelines after the cap is reached' {
        # The cap used to let device/UI failures go unscanned while the run stayed green.
        $evidenceRoot = Join-Path $TestDrive 'cap-coverage'
        for ($i = 1; $i -le 5; $i++) {
            New-TestEvidence -Root $evidenceRoot -LogId (1000 + $i)
        }
        $signatures = for ($i = 1; $i -le 5; $i++) {
            $fingerprint = "ci-scan-net11|net11.0|maui-pr|sample test $i|assertion failed|windows"
            New-TestSignature `
                -Fingerprint $fingerprint `
                -SourceLogIds @(1000 + $i) `
                -Body (New-TestBody -Fingerprint $fingerprint)
        }
        $expected = New-ExpectedBuilds -MainRequiredLogIds @(1001, 1002, 1003, 1004, 1005)
        $expected[1].result = 'failed'
        $expected[1].failed_record_count = 1
        $expected[1].required_log_ids = @(2001)
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -Signatures $signatures)
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -BuildId 123457)
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -BuildId 123458)
            )
        }

        { Test-CiScanManifest `
                -Manifest $manifest `
                -ExpectedBuilds $expected `
                -TrustedEvidencePath $evidenceRoot } |
            Should -Throw '*missing terminal coverage for trusted log IDs: 2001*'
    }

    It 'accepts post-cap coverage recorded as cap-reached skips' {
        $evidenceRoot = Join-Path $TestDrive 'cap-covered'
        for ($i = 1; $i -le 5; $i++) {
            New-TestEvidence -Root $evidenceRoot -LogId (1000 + $i)
        }
        New-TestEvidence `
            -Root $evidenceRoot `
            -Pipeline 'maui-pr-devicetests' `
            -BuildId 123457 `
            -LogId 2001 `
            -Lines @('Device harness crashed')
        $signatures = for ($i = 1; $i -le 5; $i++) {
            $fingerprint = "ci-scan-net11|net11.0|maui-pr|sample test $i|assertion failed|windows"
            New-TestSignature `
                -Fingerprint $fingerprint `
                -SourceLogIds @(1000 + $i) `
                -Body (New-TestBody -Fingerprint $fingerprint)
        }
        $expected = New-ExpectedBuilds -MainRequiredLogIds @(1001, 1002, 1003, 1004, 1005)
        $expected[1].result = 'failed'
        $expected[1].failed_record_count = 1
        $expected[1].required_log_ids = @(2001)
        $manifest = [pscustomobject]@{
            pipelines = @(
                (New-TestPipeline -Name 'maui-pr' -DefinitionId 302 -Signatures $signatures)
                (New-TestPipeline -Name 'maui-pr-devicetests' -DefinitionId 314 -BuildId 123457 -Signatures @(
                        (New-TestSignature `
                                -Pipeline 'maui-pr-devicetests' `
                                -BuildId 123457 `
                                -Fingerprint 'ci-scan-net11|net11.0|maui-pr-devicetests|device harness|crashed|android' `
                                -Disposition 'skipped' `
                                -SkipReason 'cap-reached' `
                                -SourceLogIds @(2001) `
                                -MatchPattern 'Device harness crashed')
                    ))
                (New-TestPipeline -Name 'maui-pr-uitests' -DefinitionId 313 -BuildId 123458)
            )
        }

        $plan = Test-CiScanManifest `
            -Manifest $manifest `
            -ExpectedBuilds $expected `
            -TrustedEvidencePath $evidenceRoot

        $plan.filed_count | Should -Be 5
        $plan.has_cap_skip | Should -BeTrue
    }
}

Describe 'CI scanner twin configuration' {
    BeforeAll {
        function New-ScannerManifest {
            param(
                [Parameter(Mandatory = $true)][string]$Fingerprint,
                [string]$Title = 'Sample test fails on Windows'
            )

            New-CompleteManifest -MainSignatures @(
                (New-TestSignature `
                        -Fingerprint $Fingerprint `
                        -Title $Title `
                        -Body (New-TestBody -Fingerprint $Fingerprint))
            )
        }
    }

    It 'resolves the trusted configuration for each scanner twin' -ForEach @(
        @{ ScannerId = 'ci-scan'; Branch = 'main'; Label = 'ci-scan'; TitlePrefix = '[ci-scan] ' }
        @{ ScannerId = 'ci-scan-net11'; Branch = 'net11.0'; Label = 'ci-scan-net11'; TitlePrefix = '[ci-scan-net11] ' }
    ) {
        $config = Get-CiScanScannerConfig -ScannerId $ScannerId

        $config.ScannerId | Should -BeExactly $ScannerId
        $config.Branch | Should -BeExactly $Branch
        $config.Label | Should -BeExactly $Label
        $config.TitlePrefix | Should -BeExactly $TitlePrefix
    }

    It 'rejects an unknown scanner id' -ForEach @(
        @{ ScannerId = 'ci-scan-net12' }
        @{ ScannerId = 'CI-SCAN' }
        @{ ScannerId = '' }
    ) {
        { Get-CiScanScannerConfig -ScannerId $ScannerId } | Should -Throw '*Unknown CI scanner id*'
    }

    It 'publishes main-scanner payloads with the main identity' {
        $fingerprint = 'ci-scan|main|maui-pr|sample test|assertion failed|windows'
        $plan = Test-CiScanManifest `
            -Manifest (New-ScannerManifest -Fingerprint $fingerprint) `
            -TrustedEvidencePath (New-DefaultEvidenceRoot) `
            -ScannerId 'ci-scan'

        $plan.scanner_id | Should -BeExactly 'ci-scan'
        $plan.branch | Should -BeExactly 'main'
        $plan.label | Should -BeExactly 'ci-scan'
        $plan.title_prefix | Should -BeExactly '[ci-scan] '
        $plan.issues[0].Title | Should -BeExactly '[ci-scan] Sample test fails on Windows'
        $plan.issues[0].Body | Should -Match "(?m)^<!-- ci-scan-fingerprint: $([regex]::Escape($fingerprint)) -->$"
        $plan.issues[0].Body | Should -Match '(?m)^<!-- ci-scan-match-count: 2 hits in failure\.log -->$'
    }

    It 'publishes net11-scanner payloads with the net11 identity' {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $plan = Test-CiScanManifest `
            -Manifest (New-ScannerManifest -Fingerprint $fingerprint) `
            -TrustedEvidencePath (New-DefaultEvidenceRoot) `
            -ScannerId 'ci-scan-net11'

        $plan.scanner_id | Should -BeExactly 'ci-scan-net11'
        $plan.branch | Should -BeExactly 'net11.0'
        $plan.label | Should -BeExactly 'ci-scan-net11'
        $plan.issues[0].Title | Should -BeExactly '[ci-scan-net11] Sample test fails on Windows'
        $plan.issues[0].Body | Should -Match "(?m)^<!-- ci-scan-fingerprint: $([regex]::Escape($fingerprint)) -->$"
    }

    It 'refuses a fingerprint minted for the other twin' -ForEach @(
        @{ ScannerId = 'ci-scan'; Fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows' }
        @{ ScannerId = 'ci-scan-net11'; Fingerprint = 'ci-scan|main|maui-pr|sample test|assertion failed|windows' }
    ) {
        { Test-CiScanManifest `
                -Manifest (New-ScannerManifest -Fingerprint $Fingerprint) `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) `
                -ScannerId $ScannerId } |
            Should -Throw "*does not match the $ScannerId scanner*"
    }

    It 'refuses a fingerprint whose branch field does not match the twin' {
        # Same scanner id, wrong branch: the marker must never claim coverage on a
        # branch the scanner does not own.
        { Test-CiScanManifest `
                -Manifest (New-ScannerManifest -Fingerprint 'ci-scan|net11.0|maui-pr|sample test|assertion failed|windows') `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) `
                -ScannerId 'ci-scan' } |
            Should -Throw '*does not match the ci-scan scanner*'
    }

    It 'refuses a title that already carries the twin prefix' {
        { Test-CiScanManifest `
                -Manifest (New-ScannerManifest `
                    -Fingerprint 'ci-scan|main|maui-pr|sample test|assertion failed|windows' `
                    -Title '[ci-scan] Sample test fails on Windows') `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) `
                -ScannerId 'ci-scan' } |
            Should -Throw '*must omit the prefix added by the publisher*'
    }

    It 'still enforces the five-issue cap on the main twin' {
        $signatures = for ($i = 1; $i -le 6; $i++) {
            $fingerprint = "ci-scan|main|maui-pr|sample test $i|assertion failed|windows"
            New-TestSignature `
                -Fingerprint $fingerprint `
                -Title "Sample test $i fails on Windows" `
                -Body (New-TestBody -Fingerprint $fingerprint)
        }

        { Test-CiScanManifest `
                -Manifest (New-CompleteManifest -MainSignatures @($signatures)) `
                -TrustedEvidencePath (New-DefaultEvidenceRoot) `
                -ScannerId 'ci-scan' } |
            Should -Throw '*exceeding the cap of 5*'
    }
}
