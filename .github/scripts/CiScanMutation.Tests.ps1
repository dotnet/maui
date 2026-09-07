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
        # The hidden/control-content rejection is a distinct trusted-boundary
        # layer from the marker check. Disabling it lets a body carrying an HTML
        # comment (which the canonical marker also is) or invisible content flow
        # to the deeper post-injection backstop instead of stopping at the edge.
        'no-hidden-content-rejection' = @{
            Find    = '    if ($hiddenReason) {'
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
        # The trusted boundary once again relies on the model to avoid typographic
        # dashes despite an explicit ASCII prompt.
        'title-dash-not-canonicalized' = @{
            Find    = '    return $Value.
        Replace([char]0x2013, [char]0x002D).
        Replace([char]0x2014, [char]0x002D).
        Trim()'
            Replace = '    return $Value.Trim()'
        }
        # The publisher once again requires the agent to duplicate match_pattern in
        # its issue body instead of repairing the evidence-verified handoff.
        'no-match-pattern-injection' = @{
            Find    = '    $body = Add-TrustedErrorMessagePattern `
        -Body $body `
        -MatchPattern $matchPattern'
            Replace = '    $body = $body'
        }
        # Canonical issues can again be created with patterns that cannot safely
        # prove a later recurrence.
        'no-recurrence-specificity' = @{
            Find    = '    Assert-CanonicalRecurrencePattern `
        -Fingerprint $Fingerprint `
        -MatchPattern $MatchPattern'
            Replace = ''
        }
        # The trusted existing disposition no longer enforces the same recurrence
        # specificity contract as newly filed issues.
        'no-existing-recurrence-specificity' = @{
            Find    = '                    Assert-CanonicalRecurrencePattern `
                        -Fingerprint $fingerprint `
                        -MatchPattern $matchPattern'
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

    function Test-FixedManifestHandoff {
        param([Parameter(Mandatory = $true)][string]$Source)

        return $Source -match 'CI_SCAN_MANIFEST_PATH: \$\{\{ runner\.temp \}\}/gh-aw/safe-jobs/agent/manifest_final\.json' -and
            $Source -match 'argument-free `submit_ci_scan`' -and
            $Source -notmatch '(?ms)^\s{6}inputs:\s*\r?\n\s{8}(?:manifest|manifest_path):' -and
            $Source -notmatch 'one `manifest` argument'
    }

    function Test-BoundedThreatDetectionStaging {
        param([Parameter(Mandatory = $true)][string]$Source)

        return $Source -match '\[ -L "\$manifest" \] \|\| \[ ! -f "\$manifest" \]' -and
            $Source -match '\[ "\$manifest_size" -eq 0 \] \|\| \[ "\$manifest_size" -gt 500000 \]' -and
            $Source -match 'cp --no-dereference -- "\$manifest" "\$staged"' -and
            $Source -match '\[ -L "\$staged" \] \|\| \[ ! -f "\$staged" \]' -and
            $Source -match '\[ "\$staged_size" -ne "\$manifest_size" \]'
    }

    function Test-TrustedEmojiSelectorPrompt {
        param(
            [Parameter(Mandatory = $true)][string]$Source,
            [Parameter(Mandatory = $true)][string]$ValidatorSource
        )

        $validatorMatch = [regex]::Match(
            $ValidatorSource,
            '(?s)\$isEmojiVariationBase = \$previousCode -in @\((?<bases>.*?)\r?\n\s+\)')
        $promptMatch = [regex]::Match(
            $Source,
            'Approved VS15/VS16 bases \(exactly\): (?<bases>U\+[0-9A-F]+(?:, U\+[0-9A-F]+)*)\.')
        if (-not $validatorMatch.Success -or -not $promptMatch.Success) {
            return $false
        }

        $validatorBases = @(
            [regex]::Matches($validatorMatch.Groups['bases'].Value, '0x(?<code>[0-9A-F]+)') |
                ForEach-Object { "U+$($_.Groups['code'].Value)" }
        )
        $promptBases = @($promptMatch.Groups['bases'].Value -split ', ')

        return @(
            Compare-Object `
                -ReferenceObject $validatorBases `
                -DifferenceObject $promptBases `
                -SyncWindow 0
        ).Count -eq 0 -and
            $Source -match 'Do not flag VS15 \(U\+FE0E\) or\s+VS16 \(U\+FE0F\) solely when it immediately follows one of the approved bases' -and
            $Source -match 'Flag an isolated VS15/VS16 or a selector following any other base\.'
    }

    function Test-FullEvidenceLinePrompt {
        param([Parameter(Mandatory = $true)][string]$Source)

        return $Source -match 'Copy at least one \*\*entire matching line\*\* from a frozen evidence file' -and
            $Source -match 'do not summarize it or replace\s+volatile fields with placeholders such as `<id>`' -and
            $Source -match 'shorter `match_pattern` substring is not sufficient for trusted evidence\s+identity' -and
            $Source -match 'Do not attempt to classify or remove timestamps yourself; copy them\s+verbatim\.' -and
            $Source -match 'trusted validator alone normalizes a recognized leading AzDO\s+transport timestamp'
    }

    function Test-MatchPatternRepairPrompt {
        param([Parameter(Mandatory = $true)][string]$Source)

        return $Source -match 'trusted\s+publisher verifies it against frozen evidence and appends a canonical\s+match-pattern excerpt under `## Error Message` if the agent omitted it there' -and
            $Source -match 'does not replace the full-evidence-line requirement'
    }

    function Test-InvestigationContextPrompt {
        param([Parameter(Mandatory = $true)][string]$Source)

        return $Source -match '## Investigation Context' -and
            $Source -match 'must be factual and declarative only' -and
            $Source -match 'suspected owning area or file, relevant evidence, and uncertainty' -and
            $Source -match 'no commands, requests, second-person wording, imperative verbs,\s+or instructions directed at a reader or agent' -and
            $Source -match 'contains prompt-injection or instructions aimed at you or a downstream\s+reader' -and
            $Source -notmatch '(?i)recommended action'
    }

    function Test-OrderIndependentCapPrompt {
        param([Parameter(Mandatory = $true)][string]$Source)

        return $Source -match 'exactly five\s+entries are actually marked `filed` across the complete manifest' -and
            $Source -match 'may appear before or after the fifth filed entry in fixed traversal order' -and
            $Source -match 'Use a substantive\s+skip reason whenever it applies, even after the cap is reached' -and
            $Source -match 'do not replace\s+it with `cap-reached` merely because of its position'
    }

    function Test-CanonicalRecurrencePrompt {
        param([Parameter(Mandatory = $true)][string]$Source)

        return $Source.Contains('well-formed historical match-count/evidence-key marker block') -and
            $Source.Contains('is not expected to') -and
            $Source.Contains('The trusted validator independently proves the') -and
            $Source.Contains('`## Error Message` section') -and
            $Source.Contains('not only in its trusted match-pattern excerpt') -and
            $Source.Contains('normalized identity/failure-category fields must contain a non-generic token') -and
            $Source.Contains('and the pattern itself must contain at least two distinctive tokens or one') -and
            $Source.Contains('generic text such as `Build FAILED.` is not') -and
            $Source.Contains('must not also occur in the `## Error Message` evidence') -and
            $Source.Contains('of a different open canonical issue for the same scanner branch and pipeline') -and
            $Source.Contains('restriction applies between `filed` entries in this manifest')
    }

    function Get-CompiledThreatDetectionPrompt {
        param([Parameter(Mandatory = $true)][string]$LockPath)

        $lockSource = Get-Content -LiteralPath $LockPath -Raw
        $promptMatch = [regex]::Match(
            $lockSource,
            '(?m)^\s+CUSTOM_PROMPT: (?<json>".*")$')
        if (-not $promptMatch.Success) {
            throw "The compiled lock '$LockPath' no longer contains the threat-detection CUSTOM_PROMPT."
        }

        return [System.Text.Json.JsonSerializer]::Deserialize[string](
            $promptMatch.Groups['json'].Value)
    }

    function New-ProbeManifest {
        param(
            [string]$Path,
            [string]$Body,
            [string]$MatchPattern = 'Sample scenario assertion failed',
            [string]$Title = 'Sample test fails on Windows',
            [ValidateSet('filed', 'existing')][string]$Disposition = 'filed'
        )

        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample scenario test|sample scenario assertion failed|windows'
        $signature = if ($Disposition -eq 'filed') {
            [pscustomobject]@{
                fingerprint    = $fingerprint
                disposition    = 'filed'
                source_log_ids = @(1001)
                title          = $Title
                match_pattern  = $MatchPattern
                body           = $Body
            }
        } else {
            [pscustomobject]@{
                fingerprint    = $fingerprint
                disposition    = 'existing'
                source_log_ids = @(1001)
                issue_number   = 36827
                match_pattern  = $MatchPattern
            }
        }
        $manifest = [pscustomobject]@{
            pipelines = @(
                [pscustomobject]@{
                    name          = 'maui-pr'
                    definition_id = 302
                    status        = 'scanned'
                    build_id      = 123456
                    signatures    = @($signature)
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
            [string]$Body = "## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nSample scenario assertion failed",
            [string]$MatchPattern = 'Sample scenario assertion failed',
            [string[]]$EvidenceLines = @('Sample scenario assertion failed', 'Sample scenario assertion failed'),
            [string]$Title = 'Sample test fails on Windows',
            [ValidateSet('filed', 'existing')][string]$Disposition = 'filed'
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
            -MatchPattern $MatchPattern `
            -Title $Title `
            -Disposition $Disposition
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
    $title = if (@($plan.issues).Count -gt 0) { $plan.issues[0].Title } else { '' }
    Write-Output ('RESULT ' + (ConvertTo-Json -Compress -InputObject @{ ok = $true; body = $body; title = $title }))
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

    $script:CanonicalMarker = '<!-- ci-scan-fingerprint: ci-scan-net11|net11.0|maui-pr|sample scenario test|sample scenario assertion failed|windows -->'
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

    It 'mutation "title-dash-not-canonicalized": the production title is rejected again' {
        $title = "Recurring Android device test failure $([char]0x2014) StatusBarThemeAppliesWhenHandlerConnects fails"
        $baseline = Invoke-ValidatorProbe -Title $title
        $mutated = Invoke-ValidatorProbe `
            -Mutation @('title-dash-not-canonicalized') `
            -Title $title

        $baseline.ok | Should -BeTrue
        $baseline.title | Should -BeExactly '[ci-scan-net11] Recurring Android device test failure - StatusBarThemeAppliesWhenHandlerConnects fails'
        $mutated.ok | Should -BeFalse
        $mutated.error | Should -Match 'must contain printable single-line ASCII only'
    }

    It 'mutation "no-match-pattern-injection": the main production mismatch is rejected again' {
        $pattern = 'XHarness exit code: 1 (TESTS_FAILED)'
        $body = "## Summary`nRecurring CarouselView leak.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nReference to Microsoft.Maui.Controls.CarouselView is still alive"
        $baseline = Invoke-ValidatorProbe `
            -Body $body `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern)
        $mutated = Invoke-ValidatorProbe `
            -Mutation @('no-match-pattern-injection') `
            -Body $body `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern)

        $baseline.ok | Should -BeTrue
        $baseline.body | Should -Match '(?ms)## Error Message\r?\n\r?\n    XHarness exit code: 1 \(TESTS_FAILED\)$'
        $mutated.ok | Should -BeFalse
        $mutated.error | Should -Match 'must contain match_pattern exactly'
    }

    It 'mutation "no-recurrence-specificity": generic canonical issues become publishable' {
        $pattern = 'Build FAILED.'
        $body = "## Summary`nGeneric failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`n$pattern"
        $baseline = Invoke-ValidatorProbe `
            -Body $body `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern)
        $mutated = Invoke-ValidatorProbe `
            -Mutation @('no-recurrence-specificity') `
            -Body $body `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern)

        $baseline.ok | Should -BeFalse
        $baseline.error | Should -Match 'must contain at least two distinctive tokens'
        $mutated.ok | Should -BeTrue
    }

    It 'mutation "no-existing-recurrence-specificity": generic existing coverage becomes trusted' {
        $pattern = 'Build FAILED.'
        $baseline = Invoke-ValidatorProbe `
            -Disposition 'existing' `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern)
        $mutated = Invoke-ValidatorProbe `
            -Mutation @('no-existing-recurrence-specificity') `
            -Disposition 'existing' `
            -MatchPattern $pattern `
            -EvidenceLines @($pattern)

        $baseline.ok | Should -BeFalse
        $baseline.error | Should -Match 'must contain at least two distinctive tokens'
        $mutated.ok | Should -BeTrue
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
        # Both edge-layer rejections (marker-like content and hidden/HTML-comment
        # content) are disabled so this proves the *post-injection* backstop is
        # independently load-bearing against duplicate markers.
        $result = Invoke-ValidatorProbe -Mutation @('no-duplicate-rejection', 'no-hidden-content-rejection') -Body $body

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*exactly one canonical fingerprint marker*'
    }

    It 'mutation "no-duplicate-rejection + no-post-injection-check": duplicate markers would ship' {
        $body = "$script:CanonicalMarker`n## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed"
        $result = Invoke-ValidatorProbe -Mutation @('no-duplicate-rejection', 'no-hidden-content-rejection', 'no-post-injection-check') -Body $body

        $result.ok | Should -BeTrue
        ([regex]::Matches($result.body, '<!-- ci-scan-fingerprint:')).Count | Should -Be 2
    }

    It 'baseline: the real validator rejects a body carrying hidden control content' {
        $body = "## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed$([char]0x1B)[31m"
        $result = Invoke-ValidatorProbe -Body $body

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*must not contain*C0 control character*'
    }

    It 'mutation "no-hidden-content-rejection": a hidden-content body slips past the boundary' {
        # A benign HTML comment carries no marker tokens, so only the new
        # hidden/control-content layer stands between it and publication.
        $body = "## Summary`nRecurring sample failure.`n`n## Build Information`n- **Pipeline**: maui-pr`n- **Build ID**: 123456`n`n## Error Message`nAssertion failed`n<!-- reviewer will not see this -->"
        $result = Invoke-ValidatorProbe -Mutation @('no-hidden-content-rejection') -Body $body

        # Prove the layer is load-bearing by asserting the concrete bypass: with the
        # guard disabled the body is fully published with the hidden comment intact,
        # not merely that some other error message differs.
        $result.ok | Should -BeTrue
        $result.error | Should -Not -BeLike '*HTML comment sequence*'
        $result.body | Should -BeLike '*<!-- reviewer will not see this -->*'
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
            -MatchPattern 'AzDO log 123456' `
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

    Describe 'CI scanner fixed manifest handoff mutation coverage' {
        BeforeAll {
            $script:WorkflowSources = @(
                Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/ci-status-main.md') -Raw
                Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/ci-status-net11.md') -Raw
            )
            $script:CompiledThreatPrompts = @(
                Get-CompiledThreatDetectionPrompt `
                    -LockPath (Join-Path $PSScriptRoot '../workflows/ci-status-main.lock.yml')
                Get-CompiledThreatDetectionPrompt `
                    -LockPath (Join-Path $PSScriptRoot '../workflows/ci-status-net11.lock.yml')
            )
            $script:SafeJobStepsNeedle = "      steps:`n        - name: Require successful agent submission gate"
        }

        It 'baseline: both twins use the fixed argument-free artifact handoff' {
            @($script:WorkflowSources | Where-Object { Test-FixedManifestHandoff -Source $_ }).Count |
                Should -Be 2
        }

        It 'baseline: both twins bound regular-file threat-detection staging' {
            @($script:WorkflowSources | Where-Object { Test-BoundedThreatDetectionStaging -Source $_ }).Count |
                Should -Be 2
        }

        It 'baseline: both twins mirror the trusted emoji-selector rule in threat detection' {
            @(
                $script:WorkflowSources |
                    Where-Object {
                        Test-TrustedEmojiSelectorPrompt `
                            -Source $_ `
                            -ValidatorSource $script:ValidatorSource
                    }
            ).Count | Should -Be 2
        }

        It 'baseline: both compiled twins execute the trusted emoji-selector rule' {
            @(
                $script:CompiledThreatPrompts |
                    Where-Object {
                        Test-TrustedEmojiSelectorPrompt `
                            -Source $_ `
                            -ValidatorSource $script:ValidatorSource
                    }
            ).Count | Should -Be 2
        }

        It 'baseline: both twins require a full frozen evidence line in filed bodies' {
            @($script:WorkflowSources | Where-Object { Test-FullEvidenceLinePrompt -Source $_ }).Count |
                Should -Be 2
        }

        It 'baseline: both twins describe trusted match-pattern repair' {
            @($script:WorkflowSources | Where-Object { Test-MatchPatternRepairPrompt -Source $_ }).Count |
                Should -Be 2
        }

        It 'baseline: both twins require factual investigation context' {
            @($script:WorkflowSources | Where-Object { Test-InvestigationContextPrompt -Source $_ }).Count |
                Should -Be 2
        }

        It 'baseline: both twins describe cap exhaustion independent of traversal order' {
            @($script:WorkflowSources | Where-Object { Test-OrderIndependentCapPrompt -Source $_ }).Count |
                Should -Be 2
        }

        It 'baseline: both twins separate canonical issue history from current recurrence proof' {
            @($script:WorkflowSources | Where-Object { Test-CanonicalRecurrencePrompt -Source $_ }).Count |
                Should -Be 2
        }

        It 'mutation "nested-string-transport": a manifest tool input fails the handoff invariant' {
            foreach ($source in $script:WorkflowSources) {
                $source.Contains($script:SafeJobStepsNeedle) | Should -BeTrue
                $mutated = $source.Replace(
                    $script:SafeJobStepsNeedle,
                    "      inputs:`n        manifest:`n          required: true`n          type: string`n$($script:SafeJobStepsNeedle)")

                $mutated | Should -Not -BeExactly $source
                (Test-FixedManifestHandoff -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "agent-selected-path": a manifest_path tool input fails the handoff invariant' {
            foreach ($source in $script:WorkflowSources) {
                $source.Contains($script:SafeJobStepsNeedle) | Should -BeTrue
                $mutated = $source.Replace(
                    $script:SafeJobStepsNeedle,
                    "      inputs:`n        manifest_path:`n          required: true`n          type: string`n$($script:SafeJobStepsNeedle)")

                $mutated | Should -Not -BeExactly $source
                (Test-FixedManifestHandoff -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "symlink-staging": removing the source symlink guard fails the staging invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    'if [ -L "$manifest" ] || [ ! -f "$manifest" ]; then',
                    'if [ ! -f "$manifest" ]; then')

                $mutated | Should -Not -BeExactly $source
                (Test-BoundedThreatDetectionStaging -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "unbounded-staging": removing the byte cap fails the staging invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    'if [ "$manifest_size" -eq 0 ] || [ "$manifest_size" -gt 500000 ]; then',
                    'if [ "$manifest_size" -eq 0 ]; then')

                $mutated | Should -Not -BeExactly $source
                (Test-BoundedThreatDetectionStaging -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "selector-carveout-removed": restoring generic selector rejection fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = [regex]::Replace(
                    $source,
                    '(?ms)\n      Apply this exact rule to variation selectors\..*?Flag an isolated VS15/VS16 or a selector following any other base\.\r?\n',
                    "`n      Flag variation selectors as hidden or invisible content.`n")

                $mutated | Should -Not -BeExactly $source
                (Test-TrustedEmojiSelectorPrompt `
                        -Source $mutated `
                        -ValidatorSource $script:ValidatorSource) |
                    Should -BeFalse
            }
        }

        It 'mutation "selector-carveout-widened": adding an untrusted base fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    'U+2764, U+1F6E0.',
                    'U+2764, U+1F600, U+1F6E0.')

                $mutated | Should -Not -BeExactly $source
                (Test-TrustedEmojiSelectorPrompt `
                        -Source $mutated `
                        -ValidatorSource $script:ValidatorSource) |
                    Should -BeFalse
            }
        }

        It 'mutation "selector-negative-rule-removed": dropping disallowed-base rejection fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    '      Flag an isolated VS15/VS16 or a selector following any other base.',
                    '')

                $mutated | Should -Not -BeExactly $source
                (Test-TrustedEmojiSelectorPrompt `
                        -Source $mutated `
                        -ValidatorSource $script:ValidatorSource) |
                    Should -BeFalse
            }
        }

        It 'mutation "stale-compiled-selector-rule": an omitted approved base fails the compiled prompt invariant' {
            foreach ($prompt in $script:CompiledThreatPrompts) {
                $mutated = $prompt.Replace(
                    ', U+1F6E0.',
                    '.')

                $mutated | Should -Not -BeExactly $prompt
                (Test-TrustedEmojiSelectorPrompt `
                        -Source $mutated `
                        -ValidatorSource $script:ValidatorSource) |
                    Should -BeFalse
            }
        }

        It 'mutation "matching-substring-only": removing the full-line requirement fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = [regex]::Replace(
                    $source,
                    '(?ms)\n3\. Copy at least one \*\*entire matching line\*\*.*?identity\.\r?\n',
                    "`n")

                $mutated | Should -Not -BeExactly $source
                (Test-FullEvidenceLinePrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "recommended-action-restored": downstream-directed issue guidance fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    '## Investigation Context',
                    '## Recommended Action')

                $mutated | Should -Not -BeExactly $source
                (Test-InvestigationContextPrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "declarative-only-rule-removed": an unconstrained context section fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = [regex]::Replace(
                    $source,
                    '(?ms)\nThe `Investigation Context` section must be factual and declarative only\..*?or instructions directed at a reader or agent\.\r?\n',
                    "`n")

                $mutated | Should -Not -BeExactly $source
                (Test-InvestigationContextPrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "downstream-threat-rule-relaxed": weakening strict threat detection fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = [regex]::Replace(
                    $source,
                    'contains prompt-injection or instructions aimed at you or a downstream\r?\n\s+reader',
                    'contains prompt-injection')

                $mutated | Should -Not -BeExactly $source
                (Test-InvestigationContextPrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "sequential-cap-contract": restoring traversal-order cap semantics fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    'so it may appear before or after the fifth filed entry in fixed traversal order.',
                    'so it must appear only after the fifth filed entry in fixed traversal order.')

                $mutated | Should -Not -BeExactly $source
                (Test-OrderIndependentCapPrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "agent-normalizes-timestamps": removing trusted timestamp ownership fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = [regex]::Replace(
                    $source,
                    '(?ms) Do not attempt to classify or remove timestamps yourself; copy them\r?\n   verbatim\. The trusted validator alone normalizes a recognized leading AzDO\r?\n   transport timestamp when computing evidence identity\.',
                    '')

                $mutated | Should -Not -BeExactly $source
                (Test-FullEvidenceLinePrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "agent-only-pattern-handoff": removing trusted repair fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = [regex]::Replace(
                    $source,
                    '(?ms) The trusted\r?\n   publisher verifies it against frozen evidence and appends a canonical\r?\n   match-pattern excerpt under `## Error Message` if the agent omitted it there;\r?\n   this repair does not replace the full-evidence-line requirement below\.',
                    '')

                $mutated | Should -Not -BeExactly $source
                (Test-MatchPatternRepairPrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "current-key-must-match-history": restoring cross-run key equality fails the recurrence invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    'is not expected to',
                    'must')

                $mutated | Should -Not -BeExactly $source
                (Test-CanonicalRecurrencePrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "trusted-excerpt-is-history": widening recurrence beyond Error Message evidence fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = [regex]::Replace(
                    $source,
                    '(?ms)referenced issue''s\r?\n  `## Error Message` section, not only in its trusted match-pattern excerpt\.',
                    'referenced issue body.')

                $mutated | Should -Not -BeExactly $source
                (Test-CanonicalRecurrencePrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "generic-recurrence-allowed": dropping recurrence specificity fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $start = $source.IndexOf(
                    'normalized identity/failure-category fields must contain a non-generic token,',
                    [System.StringComparison]::Ordinal)
                $endToken = 'sufficient.'
                $end = $source.IndexOf(
                    $endToken,
                    $start,
                    [System.StringComparison]::Ordinal)
                $start | Should -BeGreaterOrEqual 0
                $end | Should -BeGreaterOrEqual $start
                $mutated = $source.Remove($start, ($end + $endToken.Length) - $start)

                $mutated | Should -Not -BeExactly $source
                (Test-CanonicalRecurrencePrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "shared-pattern-allowed": dropping cross-issue attribution fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    'The pattern must not also occur in the `## Error Message` evidence',
                    'The pattern may also occur in the `## Error Message` evidence')

                $mutated | Should -Not -BeExactly $source
                (Test-CanonicalRecurrencePrompt -Source $mutated) | Should -BeFalse
            }
        }

        It 'mutation "same-run-shared-pattern-allowed": dropping intra-manifest attribution fails the prompt invariant' {
            foreach ($source in $script:WorkflowSources) {
                $mutated = $source.Replace(
                    'restriction applies between `filed` entries in this manifest',
                    'restriction does not apply between `filed` entries in this manifest')

                $mutated | Should -Not -BeExactly $source
                (Test-CanonicalRecurrencePrompt -Source $mutated) | Should -BeFalse
            }
        }
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
