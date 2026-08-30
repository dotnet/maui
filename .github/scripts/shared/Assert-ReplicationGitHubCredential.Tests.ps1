#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Assert-ReplicationGitHubCredential.ps1')

    function New-CredentialResponse {
        param(
            [int]$StatusCode = 200,
            [string]$Login = 'MauiBot',
            [string]$Message = '',
            [hashtable]$Headers = @{}
        )

        return @{
            StatusCode = $StatusCode
            Login      = $Login
            Message    = $Message
            Headers    = $Headers
        }
    }
}

Describe 'Assert-ReplicationGitHubCredential' {
    It 'uses the supplied token only in the authenticated request header' {
        $script:authorization = ''
        Mock Invoke-WebRequest {
            param($Uri, $Headers, $Method, $TimeoutSec, $SkipHttpErrorCheck)
            $script:authorization = [string]$Headers.Authorization
            [pscustomobject]@{
                StatusCode = 200
                Content = '{"login":"MauiBot"}'
                Headers = @{}
            }
        }

        $result = Invoke-ReplicationCredentialRequest `
            -Token 'credential-value' `
            -ApiBase 'https://api.github.com'

        $script:authorization | Should -BeExactly 'Bearer credential-value'
        $result.Login | Should -BeExactly 'MauiBot'
    }

    It 'returns the authenticated login when GitHub accepts the token' {
        $login = Assert-ReplicationGitHubCredential `
            -Token 'valid' `
            -Requester { New-CredentialResponse -Login 'MauiBot' } `
            -RetryDelaysSeconds @(0)

        $login | Should -Be 'MauiBot'
    }

    It 'refuses an empty token without asking GitHub' {
        $called = $false
        {
            Assert-ReplicationGitHubCredential `
                -Token '  ' `
                -Requester { $script:called = $true; New-CredentialResponse }
        } | Should -Throw '*is empty*'

        $called | Should -BeFalse
    }

    It 'names the pipeline variable that has to be rotated on a 401' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'dead' `
                -TokenVariableName 'GH_COMMENT_TOKEN' `
                -Requester { New-CredentialResponse -StatusCode 401 } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw "*GH_COMMENT_TOKEN*rotated*"
    }

    It 'does not retry a rejected credential' {
        $script:attempts = 0
        {
            Assert-ReplicationGitHubCredential `
                -Token 'dead' `
                -MaximumAttempts 5 `
                -Requester {
                    $script:attempts++
                    New-CredentialResponse -StatusCode 401
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw

        $script:attempts | Should -Be 1
    }

    It 'reports a spent quota as a rate limit rather than a dead credential' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester {
                    New-CredentialResponse `
                        -StatusCode 403 `
                        -Headers @{ 'x-ratelimit-remaining' = '0' }
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*rate limited*'
    }

    It 'does not tell anyone to rotate a token that is only rate limited' {
        $message = ''
        try {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester {
                    New-CredentialResponse `
                        -StatusCode 403 `
                        -Headers @{ 'X-RateLimit-Remaining' = '0' }
                } `
                -RetryDelaysSeconds @(0)
        } catch {
            $message = [string]$_.Exception.Message
        }

        $message | Should -Not -BeNullOrEmpty
        $message | Should -Not -Match 'rotate'
    }

    It 'reports a 403 with quota remaining as a missing permission' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester {
                    New-CredentialResponse `
                        -StatusCode 403 `
                        -Headers @{ 'x-ratelimit-remaining' = '4999' }
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*missing a permission*'
    }

    It 'retries a GitHub outage and succeeds when the service returns' {
        $script:attempts = 0
        $login = Assert-ReplicationGitHubCredential `
            -Token 'valid' `
            -Requester {
                $script:attempts++
                if ($script:attempts -lt 3) {
                    return New-CredentialResponse -StatusCode 503
                }

                return New-CredentialResponse -Login 'MauiBot'
            } `
            -RetryDelaysSeconds @(0)

        $login | Should -Be 'MauiBot'
        $script:attempts | Should -Be 3
    }

    It 'retries a network failure that never reached GitHub' {
        $script:attempts = 0
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -MaximumAttempts 3 `
                -Requester {
                    $script:attempts++
                    New-CredentialResponse `
                        -StatusCode 0 `
                        -Message 'The operation timed out'
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*timed out*'

        $script:attempts | Should -Be 3
    }

    It 'rejects a token that authenticates as the wrong account' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -ExpectedLogin 'MauiBot' `
                -Requester { New-CredentialResponse -Login 'someone-else' } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw "*authenticates as 'someone-else'*"
    }

    It 'accepts the expected login regardless of case' {
        $login = Assert-ReplicationGitHubCredential `
            -Token 'valid' `
            -ExpectedLogin 'mauibot' `
            -Requester { New-CredentialResponse -Login 'MauiBot' } `
            -RetryDelaysSeconds @(0)

        $login | Should -Be 'MauiBot'
    }

    It 'fails when GitHub accepts the token but returns no login' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester { New-CredentialResponse -Login '' } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*did not*return a login*'
    }

    It 'never repeats the token in a failure message' {
        $secret = 'ghp_examplesecretvalue'
        $message = ''
        try {
            Assert-ReplicationGitHubCredential `
                -Token $secret `
                -Requester { New-CredentialResponse -StatusCode 401 } `
                -RetryDelaysSeconds @(0)
        } catch {
            $message = [string]$_.Exception.Message
        }

        $message | Should -Not -Match ([regex]::Escape($secret))
    }
}

Describe 'Get-ReplicationCredentialFailureKind' {
    It 'treats 429 as a rate limit' {
        Get-ReplicationCredentialFailureKind -StatusCode 429 |
            Should -Be 'ratelimited'
    }

    It 'treats a secondary rate limit message as a rate limit' {
        Get-ReplicationCredentialFailureKind `
            -StatusCode 403 `
            -Message 'You have exceeded a secondary rate limit' |
            Should -Be 'ratelimited'
    }

    It 'treats a server error as transient' {
        Get-ReplicationCredentialFailureKind -StatusCode 502 |
            Should -Be 'transient'
    }

    It 'does not treat a rejected credential as transient' {
        Get-ReplicationCredentialFailureKind -StatusCode 401 |
            Should -Be 'invalid'
    }
}

Describe 'Replication credential pre-flight wiring' {
    BeforeAll {
        $script:PipelinePath = Join-Path `
            (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) `
            'pipelines/ci-copilot.yml'
        if (-not (Test-Path -LiteralPath $script:PipelinePath)) {
            $script:PipelinePath = Join-Path `
                (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))) `
                'eng/pipelines/ci-copilot.yml'
        }
        $script:Pipeline = Get-Content -LiteralPath $script:PipelinePath -Raw

        function Get-CredentialPreflightStep {
            # Slice the whole step rather than a fixed window, so the tests keep
            # asserting about the step and not about how long it happens to be.
            $marker = "displayName: 'Verify the GitHub credential before provisioning'"
            $end = $script:Pipeline.IndexOf($marker)
            if ($end -lt 0) { throw 'The credential pre-flight step is missing.' }
            $start = $script:Pipeline.LastIndexOf('- pwsh:', $end)
            if ($start -lt 0) { throw 'Could not find the start of the pre-flight step.' }
            return $script:Pipeline.Substring($start, ($end - $start) + $marker.Length + 200)
        }
    }

    It 'checks the credential before the expensive setup steps' {
        $probeIndex = $script:Pipeline.IndexOf('Verify the GitHub credential')
        $probeIndex | Should -BeGreaterThan 0

        foreach ($expensive in @(
            'Capture trusted test infrastructure',
            'Install reproduction recording tools',
            'Install GitHub CLI',
            'Prepare sanitized issue context')) {
            $index = $script:Pipeline.IndexOf($expensive)
            $index | Should -BeGreaterThan 0
            $probeIndex | Should -BeLessThan $index
        }
    }

    It 'only runs the pre-flight for replicate runs' {
        $probeIndex = $script:Pipeline.IndexOf('Verify the GitHub credential')
        $window = $script:Pipeline.Substring($probeIndex, 700)
        $window | Should -Match "eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\)"
    }

    It 'tells an unprovided secret apart from an expired one' {
        # Azure Pipelines leaves '$(Name)' in place when Name is not defined for
        # the run, so the step receives that literal text and GitHub answers 401
        # exactly as it would for a revoked token. The remedies are opposite:
        # one needs the secret rotated, the other needs it made available to the
        # ref being built, so reporting "rotate the token" for an undefined
        # variable sends whoever reads it to the wrong place entirely.
        Test-ReplicationUnexpandedVariable -Value '$(GH_COMMENT_TOKEN)' | Should -BeTrue
        Test-ReplicationUnexpandedVariable -Value '  $(GH_COMMENT_TOKEN)  ' | Should -BeTrue
        Test-ReplicationUnexpandedVariable -Value 'ghp_realtokenvalue' | Should -BeFalse
        Test-ReplicationUnexpandedVariable -Value '' | Should -BeFalse
        Test-ReplicationUnexpandedVariable -Value '$(not a variable)' | Should -BeFalse

        $verdict = Get-ReplicationGitHubCredentialVerdict -Token '$(GH_COMMENT_TOKEN)' -Requester {
            param($t, $b) throw 'GitHub must not be called for an unsubstituted variable.'
        }
        $verdict.Kind | Should -Be 'undefined'
        $verdict.Usable | Should -BeFalse
        $verdict.Message | Should -Match 'not being provided to the ref'
        $verdict.Message | Should -Not -Match 'rotated'

        (Get-ReplicationCredentialDecision -Kind 'undefined').Tag |
            Should -Be 'credential-not-provided'
    }

    It 'fails only on the failures a later run could recover from' {
        # A rate limit resets and an unreachable GitHub comes back, so those are
        # worth failing on and running again. A wrong login is a
        # misconfiguration nobody should work around.
        foreach ($kind in @('ratelimited', 'unreachable', 'wronglogin')) {
            (Get-ReplicationCredentialDecision -Kind $kind).Action |
                Should -Be 'fail' -Because "$kind is recoverable or needs a person"
        }
    }

    It 'keeps reproducing when only the publishing credential is dead' {
        # dotnet/maui issues are public. Reproducing on a device, recording the
        # evidence, and authoring a failing test need no credential, so an
        # expired publishing secret must not discard all of that work: it is
        # exactly the evidence that shows whether replication itself is sound.
        foreach ($kind in @('invalid', 'empty', 'forbidden')) {
            (Get-ReplicationCredentialDecision -Kind $kind).Action |
                Should -Be 'degrade' -Because "$kind cannot be fixed by rerunning"
        }

        (Get-ReplicationCredentialDecision -Kind 'ok').Action | Should -Be 'continue'
    }

    It 'names every unusable credential on the run summary' {
        # When it dies the definition fills with runs whose cause is only
        # readable by opening a log.
        (Get-ReplicationCredentialDecision -Kind 'invalid').Tag | Should -Be 'credential-expired'
        (Get-ReplicationCredentialDecision -Kind 'empty').Tag | Should -Be 'credential-missing'
        (Get-ReplicationCredentialDecision -Kind 'forbidden').Tag | Should -Be 'credential-forbidden'
        (Get-ReplicationCredentialDecision -Kind 'ratelimited').Tag | Should -Be 'github-rate-limited'
        (Get-ReplicationCredentialDecision -Kind 'ok').Tag | Should -BeNullOrEmpty
    }

    It 'never silently continues on a credential it could not classify' {
        # An unknown kind must not be read as success and publish with a
        # credential nobody checked.
        (Get-ReplicationCredentialDecision -Kind 'something-new').Action |
            Should -Not -Be 'continue'
    }

    It 'wires the pipeline step to the decision rather than repeating it' {
        $step = Get-CredentialPreflightStep
        $step | Should -Match 'Get-ReplicationCredentialDecision'
        $step | Should -Match 'build\.addbuildtag\]\$\(\$decision\.Tag\)'
        $step | Should -Match 'throw \$verdict\.Message'
        $step | Should -Match 'REPLICATION_EVIDENCE_ONLY\]true'
        $step | Should -Match 'replicationEvidenceOnly;isOutput=true\]true'
        $step | Should -Match 'build\.addbuildtag\]evidence-only'
        # An unhandled action must not fall through as success.
        $step | Should -Match 'Unhandled credential decision'
    }

    It 'refuses to publish from a run that read the issue anonymously' {
        # An evidence-only run has no credential that can write, and a draft PR
        # that silently never appears is worse than one the summary says was
        # skipped.
        $script:Pipeline | Should -Match `
            "ne\(dependencies\.ReviewPR\.outputs\['CopilotReview\.ReplicationCredentialCheck\.replicationEvidenceOnly'\], 'true'\)"

        # The output variable only exists if the step is named.
        $step = Get-CredentialPreflightStep
        $step | Should -Match 'name: ReplicationCredentialCheck'
    }

    It 'only reads anonymously when the run has already given up publishing' {
        # Reading anonymously is safe; doing it on a run that still intends to
        # publish would hide a dying credential until the publish step.
        $contextIndex = $script:Pipeline.IndexOf('Prepare sanitized issue context')
        $contextIndex | Should -BeGreaterThan 0
        $contextStart = $script:Pipeline.LastIndexOf('- pwsh:', $contextIndex)
        $contextStart | Should -BeGreaterThan 0
        $contextStep = $script:Pipeline.Substring($contextStart, $contextIndex - $contextStart)
        $contextStep | Should -Match 'AllowAnonymousFallback'
        $contextStep | Should -Match "REPLICATION_EVIDENCE_ONLY -eq 'true'"
    }
}
