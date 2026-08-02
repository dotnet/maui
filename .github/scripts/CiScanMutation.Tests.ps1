#!/usr/bin/env pwsh
#Requires -Modules Pester

<#
    Mutation coverage for the CI scanner marker controls.

    Ordinary tests prove the validator and publisher behave correctly today. They
    do not prove the behaviour is *caused* by the controls we think it is — a
    suite can keep passing after a guard is deleted if nothing exercises it. That
    is exactly how the marker regression shipped: the prompt told the agent to
    emit markers, gh-aw never delivered that instruction, and nothing failed.

    Each test below takes the real source, applies one named mutation, and shows
    that the mutant either fails closed or produces the bad payload the control
    exists to prevent. Every mutation asserts that it actually changed the source
    first, so a rename cannot silently turn these into no-ops.
#>

BeforeAll {
    . (Join-Path $PSScriptRoot 'CiScanTwins.Helpers.ps1')

    $script:ValidatorPath = Join-Path $PSScriptRoot 'Validate-CiScanManifest.ps1'
    $script:ValidatorSource = Get-Content -LiteralPath $script:ValidatorPath -Raw

    $script:Mutations = @{
        # The publisher stops adding the canonical marker block.
        'no-injection'             = @{
            Find    = '$publishedBody = (New-CanonicalMarkerBlock `
            -Fingerprint $Fingerprint `
            -MatchCount $trustedEvidenceProof.MatchCount `
            -EvidenceKey $trustedEvidenceProof.EvidenceKey) + "`n`n" + $body'
            Replace = '$publishedBody = $body'
        }
        # The fingerprint marker is sourced from agent-controlled body text
        # instead of the validated manifest structure.
        'fingerprint-from-body'    = @{
            Find    = '$publishedBody = (New-CanonicalMarkerBlock `
            -Fingerprint $Fingerprint `
            -MatchCount $trustedEvidenceProof.MatchCount `
            -EvidenceKey $trustedEvidenceProof.EvidenceKey) + "`n`n" + $body'
            Replace = '$publishedBody = (New-CanonicalMarkerBlock `
            -Fingerprint ([regex]::Match($rawBody, ''(?m)^claimed-fingerprint: (.+)$'').Groups[1].Value) `
            -MatchCount $trustedEvidenceProof.MatchCount `
            -EvidenceKey $trustedEvidenceProof.EvidenceKey) + "`n`n" + $body'
        }
        # The count marker is no longer the frozen-evidence recount.
        'untrusted-count'          = @{
            Find    = '-MatchCount $trustedEvidenceProof.MatchCount `
            -EvidenceKey $trustedEvidenceProof.EvidenceKey) + "`n`n" + $body'
            Replace = '-MatchCount ($trustedEvidenceProof.MatchCount + 7) `
            -EvidenceKey $trustedEvidenceProof.EvidenceKey) + "`n`n" + $body'
        }
        # Validation happens only before injection: the post-injection assertion
        # over the exact published payload is removed.
        'no-post-injection-check'  = @{
            Find    = '    Assert-CanonicalPublishedBody `
        -Body $publishedBody `'
            Replace = '    Assert-NoOpPublishedBody `
        -Body $publishedBody `'
        }
        # Pre-existing / duplicate / evasive marker content is accepted from the agent.
        'no-duplicate-rejection'   = @{
            Find    = '    if (Test-MarkerLikeContent -Value $rawBody) {'
            Replace = '    if ($false) {'
        }
        # Marker-like match patterns can replay trusted publisher state.
        'no-marker-pattern-rejection' = @{
            Find    = '    if (Test-MarkerLikeContent -Value $matchPattern) {'
            Replace = '    if ($false) {'
        }
        # Synthetic framing is put back into the countable raw segment set.
        'synthetic-framing-counted' = @{
            Find    = '                -TrustedEvidencePath $TrustedEvidencePath)
        foreach ($segment in $segments) {'
            Replace = '                -TrustedEvidencePath $TrustedEvidencePath)
        $segments += [pscustomobject]@{ content = "===== AzDO log $BuildId/$sourceLogId =====" }
        foreach ($segment in $segments) {'
        }
        # Run-specific AzDO transport timestamps remain part of evidence identity.
        'timestamp-sensitive-identity' = @{
            Find    = '    if ($StripAzdoTransportTimestamp) {
        # Azure DevOps prepends a run-specific UTC timestamp to every stored log
        # line. Segment provenance decides whether it is transport framing; the
        # same timestamp in Helix or other evidence remains part of the message.
        $normalized = [regex]::Replace(
            $normalized,
            ''^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?Z[ \t]+'',
            ''''
        )
    }'
            Replace = ''
        }
    }

    function New-MutatedValidator {
        param(
            [Parameter(Mandatory = $true)][string[]]$Mutation,
            [Parameter(Mandatory = $true)][string]$Path
        )

        $source = $script:ValidatorSource
        foreach ($name in $Mutation) {
            $definition = $script:Mutations[$name]
            if (-not $definition) {
                throw "Unknown mutation '$name'."
            }
            if (-not $source.Contains($definition.Find)) {
                throw "Mutation '$name' no longer matches the validator source; update the mutation."
            }
            $source = $source.Replace($definition.Find, $definition.Replace)
        }

        # Stand-in for the removed post-injection assertion, so the mutant runs
        # instead of dying on a missing command.
        $source = $source.Replace(
            'function Assert-CanonicalPublishedBody {',
            "function Assert-NoOpPublishedBody { param(`$Body, `$Fingerprint, `$MatchCount, `$EvidenceKey, `$EvidenceLineHashes, `$MatchPattern, `$PipelineName, `$BuildId) }`n`nfunction Assert-CanonicalPublishedBody {")

        Set-Content -LiteralPath $Path -Value $source -Encoding utf8
        return $Path
    }

    function New-ProbeManifest {
        param(
            [string]$Path,
            [string]$Body,
            [string]$MatchPattern = 'Assertion failed'
        )

        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $manifest = [pscustomobject]@{
            pipelines = @(
                [pscustomobject]@{
                    name          = 'maui-pr'
                    definition_id = 302
                    status        = 'scanned'
                    build_id      = 123456
                    signatures    = @(
                        [pscustomobject]@{
                            fingerprint    = $fingerprint
                            disposition    = 'filed'
                            source_log_ids = @(1001)
                            title          = 'Sample test fails on Windows'
                            match_pattern  = $MatchPattern
                            body           = $Body
                        }
                    )
                }
                [pscustomobject]@{ name = 'maui-pr-devicetests'; definition_id = 314; status = 'scanned'; build_id = 123457; signatures = @() }
                [pscustomobject]@{ name = 'maui-pr-uitests'; definition_id = 313; status = 'scanned'; build_id = 123458; signatures = @() }
            )
        }

        Set-Content -LiteralPath $Path -Value ($manifest | ConvertTo-Json -Depth 12)
        return $Path
    }

    function New-ProbeEvidence {
        param(
            [string]$Root,
            [string[]]$Lines = @('Assertion failed', 'Assertion failed')
        )

        $directory = Join-Path $Root 'maui-pr'
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
        Set-Content `
            -LiteralPath (Join-Path $directory '123456-1001.log') `
            -Value $Lines
        [pscustomobject]@{
            schema_version = 1
            pipeline       = 'maui-pr'
            build_id       = 123456
            log_id         = 1001
            segments       = @(
                [pscustomobject]@{
                    kind    = 'azdo-log'
                    source  = '123456/1001'
                    content = $Lines -join "`n"
                }
            )
        } | ConvertTo-Json -Depth 6 | Set-Content `
            -LiteralPath (Join-Path $directory '123456-1001.evidence.json')
        return $Root
    }

    function Invoke-ValidatorProbe {
        param(
            [string[]]$Mutation = @(),
            [string]$Body = "## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed",
            [string]$MatchPattern = 'Assertion failed',
            [string[]]$EvidenceLines = @('Assertion failed', 'Assertion failed')
        )

        $work = Join-Path $TestDrive ('mutation-' + [guid]::NewGuid().ToString('n'))
        New-Item -ItemType Directory -Path $work -Force | Out-Null

        $validator = if ($Mutation.Count -eq 0) {
            $copy = Join-Path $work 'Validate-CiScanManifest.ps1'
            Set-Content -LiteralPath $copy -Value $script:ValidatorSource -Encoding utf8
            $copy
        } else {
            New-MutatedValidator -Mutation $Mutation -Path (Join-Path $work 'Validate-CiScanManifest.ps1')
        }

        $manifestPath = New-ProbeManifest `
            -Path (Join-Path $work 'manifest.json') `
            -Body $Body `
            -MatchPattern $MatchPattern
        $evidencePath = New-ProbeEvidence `
            -Root (Join-Path $work 'evidence') `
            -Lines $EvidenceLines
        $probePath = Join-Path $work 'probe.ps1'

        Set-Content -LiteralPath $probePath -Value @'
param([string]$ValidatorPath, [string]$ManifestPath, [string]$EvidencePath)
$ErrorActionPreference = 'Stop'
. $ValidatorPath
try {
    $manifest = Get-Content -Raw -LiteralPath $ManifestPath | ConvertFrom-Json
    $plan = Test-CiScanManifest -Manifest $manifest -TrustedEvidencePath $EvidencePath
    $body = if (@($plan.issues).Count -gt 0) { $plan.issues[0].Body } else { '' }
    Write-Output ('RESULT ' + (ConvertTo-Json -Compress -InputObject @{ ok = $true; body = $body }))
} catch {
    Write-Output ('RESULT ' + (ConvertTo-Json -Compress -InputObject @{ ok = $false; error = "$($_.Exception.Message)" }))
}
'@

        # A child process keeps each mutant's function definitions out of the
        # test session, so one mutation cannot leak into the next assertion.
        $output = & pwsh -NoProfile -File $probePath $validator $manifestPath $evidencePath 2>&1
        $line = @($output | Where-Object { "$_" -like 'RESULT *' }) | Select-Object -Last 1
        if (-not $line) {
            throw "validator probe produced no result: $output"
        }

        return ("$line".Substring(7) | ConvertFrom-Json)
    }

    $script:CanonicalMarker = '<!-- ci-scan-fingerprint: ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows -->'
}

Describe 'CI scanner marker mutation coverage' {
    It 'baseline: the real validator injects exactly one canonical marker block' {
        $result = Invoke-ValidatorProbe

        $result.ok | Should -BeTrue
        ([regex]::Matches($result.body, '<!-- ci-scan-fingerprint:')).Count | Should -Be 1
        ([regex]::Matches($result.body, '<!-- ci-scan-match-count:')).Count | Should -Be 1
        ([regex]::Matches($result.body, '<!-- ci-scan-evidence-key:')).Count | Should -Be 1
        $result.body.StartsWith($script:CanonicalMarker) | Should -BeTrue
        $result.body | Should -Match '(?m)^<!-- ci-scan-match-count: 2 hits in failure\.log -->$'
    }

    It 'mutation "no-injection": removing injection cannot produce a marked issue' {
        $result = Invoke-ValidatorProbe -Mutation @('no-injection')

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not begin with the canonical marker block*'
    }

    It 'mutation "no-injection + no-post-injection-check": reproduces the unmarked-issue incident' {
        # This is the production failure mode, reconstructed: with both the
        # injection and the post-injection assertion gone, a perfectly valid-looking
        # run publishes an issue with neither marker and reports success.
        $result = Invoke-ValidatorProbe -Mutation @('no-injection', 'no-post-injection-check')

        $result.ok | Should -BeTrue
        $result.body | Should -Not -Match '<!-- ci-scan-fingerprint:'
        $result.body | Should -Not -Match '<!-- ci-scan-match-count:'
    }

    It 'mutation "fingerprint-from-body": untrusted fingerprint sourcing is rejected' {
        $body = "## Summary`nRecurring sample failure.`nclaimed-fingerprint: ci-scan-net11|net11.0|maui-pr|attacker chosen|attacker error|linux`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed"
        $result = Invoke-ValidatorProbe -Mutation @('fingerprint-from-body') -Body $body

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*canonical marker block*'
    }

    It 'mutation "fingerprint-from-body + no-post-injection-check": body content would own the marker' {
        # Shows the previous assertion is not vacuous: without the post-injection
        # check, the attacker-chosen fingerprint really does reach the marker.
        $body = "## Summary`nRecurring sample failure.`nclaimed-fingerprint: ci-scan-net11|net11.0|maui-pr|attacker chosen|attacker error|linux`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed"
        $result = Invoke-ValidatorProbe -Mutation @('fingerprint-from-body', 'no-post-injection-check') -Body $body

        $result.ok | Should -BeTrue
        $result.body | Should -Match 'attacker chosen'
        $result.body.StartsWith($script:CanonicalMarker) | Should -BeFalse
    }

    It 'mutation "untrusted-count": a count that is not the evidence recount is rejected' {
        $result = Invoke-ValidatorProbe -Mutation @('untrusted-count')

        $result.ok | Should -BeFalse
        # Post-injection validation rebuilds the expected block from the trusted
        # count, so an inflated count fails the exact-payload comparison.
        $result.error | Should -BeLike '*does not begin with the canonical marker block*'
    }

    It 'mutation "untrusted-count + no-post-injection-check": wrong count would be published' {
        $result = Invoke-ValidatorProbe -Mutation @('untrusted-count', 'no-post-injection-check')

        $result.ok | Should -BeTrue
        $result.body | Should -Match '(?m)^<!-- ci-scan-match-count: 9 hits in failure\.log -->$'
    }

    It 'mutation "no-duplicate-rejection": a pre-marked body is rejected downstream' {
        $body = "$script:CanonicalMarker`n## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed"
        $result = Invoke-ValidatorProbe -Mutation @('no-duplicate-rejection') -Body $body

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*exactly one canonical fingerprint marker*'
    }

    It 'mutation "no-duplicate-rejection + no-post-injection-check": duplicate markers would ship' {
        $body = "$script:CanonicalMarker`n## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed"
        $result = Invoke-ValidatorProbe -Mutation @('no-duplicate-rejection', 'no-post-injection-check') -Body $body

        $result.ok | Should -BeTrue
        ([regex]::Matches($result.body, '<!-- ci-scan-fingerprint:')).Count | Should -Be 2
    }

    It 'baseline: the real validator rejects that same pre-marked body outright' {
        $body = "$script:CanonicalMarker`n## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed"
        $result = Invoke-ValidatorProbe -Body $body

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*must not contain scanner marker content*'
    }

    It 'mutation "synthetic-framing-counted": a header-only pattern becomes false evidence' {
        $header = '===== AzDO log 123456/1001 ====='
        $result = Invoke-ValidatorProbe `
            -Mutation @('synthetic-framing-counted') `
            -MatchPattern '===== AzDO log' `
            -EvidenceLines @('Different raw failure') `
            -Body "## Summary`nHeader replay.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`n$header"

        $result.ok | Should -BeTrue
    }

    It 'baseline: header-only evidence fails closed when synthetic framing is not countable' {
        $header = '===== AzDO log 123456/1001 ====='
        $result = Invoke-ValidatorProbe `
            -MatchPattern '===== AzDO log' `
            -EvidenceLines @('Different raw failure') `
            -Body "## Summary`nHeader replay.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`n$header"

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*must occur in trusted source log 1001*'
    }

    It 'mutation "no-marker-pattern-rejection": marker state can become evidence when both marker gates are removed' {
        $pattern = 'ci-scan-fingerprint'
        $body = "## Summary`nMarker replay.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`n$pattern"
        $result = Invoke-ValidatorProbe `
            -Mutation @('no-marker-pattern-rejection', 'no-duplicate-rejection') `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern) `
            -Body $body

        $result.ok | Should -BeTrue
    }

    It 'baseline: marker-like match patterns are rejected before evidence can replay them' {
        $pattern = 'ci-scan-fingerprint'
        $result = Invoke-ValidatorProbe `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern) `
            -Body "## Summary`nMarker replay.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`n$pattern"

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*scanner marker content*'
    }

    It 'baseline: AzDO transport timestamps do not prevent trusted body binding' {
        $trusted = '2026-07-20T18:34:13.9100750Z ##[error]Path does not exist: artifacts/bin'
        $body = "## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`n##[error]Path does not exist: artifacts/bin"
        $result = Invoke-ValidatorProbe `
            -MatchPattern 'Path does not exist' `
            -EvidenceLines @($trusted) `
            -Body $body

        $result.ok | Should -BeTrue
    }

    It 'mutation "timestamp-sensitive-identity": realistic cross-build body binding fails' {
        $trusted = '2026-07-20T18:34:13.9100750Z ##[error]Path does not exist: artifacts/bin'
        $body = "## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`n##[error]Path does not exist: artifacts/bin"
        $result = Invoke-ValidatorProbe `
            -Mutation @('timestamp-sensitive-identity') `
            -MatchPattern 'Path does not exist' `
            -EvidenceLines @($trusted) `
            -Body $body

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*must contain a full trusted evidence line*'
    }
}

Describe 'CI scanner twin discovery mutation coverage' {
    It 'baseline: discovery finds both compiled twins' {
        @(Get-CiScanTwin).Count | Should -Be 2
    }

    It 'mutation "one-twin-omitted": discovery reports a single twin' {
        # Proves the anti-vacuity assertion in Validate-CiScanPublisher.Tests.ps1
        # is load-bearing: dropping a twin changes what discovery returns, so the
        # "exactly two" assertion fails rather than silently testing one scanner.
        $root = Join-Path $TestDrive 'one-twin'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        Copy-Item `
            -LiteralPath (Join-Path $PSScriptRoot '../workflows/ci-status-net11.lock.yml') `
            -Destination $root

        $twins = @(Get-CiScanTwin -WorkflowRoot $root)

        $twins.Count | Should -Be 1
        { $twins.Count | Should -Be 2 } | Should -Throw
    }

    It 'mutation "discovery-empty": an empty discovery fails the anti-vacuity gate' {
        $root = Join-Path $TestDrive 'no-twins'
        New-Item -ItemType Directory -Path $root -Force | Out-Null

        $twins = @(Get-CiScanTwin -WorkflowRoot $root)

        $twins.Count | Should -Be 0
        { $twins.Count | Should -Be 2 } | Should -Throw
    }

    It 'mutation "publisher-step-renamed": extraction fails instead of testing nothing' {
        $root = Join-Path $TestDrive 'renamed-step'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        $lockPath = Join-Path $root 'ci-status-net11.lock.yml'
        (Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/ci-status-net11.lock.yml') -Raw).Replace(
            'Preflight references and publish validated issues',
            'Publish issues') | Set-Content -LiteralPath $lockPath

        @(Get-CiScanTwin -WorkflowRoot $root).Count | Should -Be 0
        { Get-CiScanPublisherScript -LockPath $lockPath } |
            Should -Throw '*no longer contains the publisher step*'
    }
}
