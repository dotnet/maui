#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for pure-function helpers in Review-PR.ps1.
    Currently covers:
      - Get-TrxResults  (parses VSTest TRX produced by `dotnet test --logger trx`)
      - Get-DotNetTestResults  (legacy console-output scraper, still used as fallback
                                when TRX is missing)
      - Copilot token usage helpers

    These functions sit on the critical path of STEP 3 (UI Test Execution
    Results in the AI summary review). A regression here can silently
    misrender per-test counts (e.g. "1/1 (1 ❌)" instead of "75/619 (544 ❌)")
    so they're worth pinning with focused tests.

.EXAMPLE
    Invoke-Pester ./Review-PR.Tests.ps1
    Invoke-Pester ./Review-PR.Tests.ps1 -Output Detailed
#>

BeforeAll {
    # Source just the helper functions we want to test out of Review-PR.ps1.
    # We can't dot-source the entire script because it has top-level imperative
    # logic (banner, prerequisites, step driver) that runs at parse time.
    $reviewScript = Join-Path $PSScriptRoot 'Review-PR.ps1'
    $content = Get-Content -Raw $reviewScript
    $pipelineContent = Get-Content -Raw (Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml')
    $provisionContent = Get-Content -Raw (Join-Path $PSScriptRoot '../../eng/pipelines/common/provision.yml')

    function Get-FunctionBody {
        param([string]$ScriptText, [string]$FunctionName)
        $start = $ScriptText.IndexOf("function $FunctionName")
        if ($start -lt 0) { throw "Function '$FunctionName' not found" }
        $i = $ScriptText.IndexOf('{', $start)
        $depth = 0; $end = -1
        for (; $i -lt $ScriptText.Length; $i++) {
            $c = $ScriptText[$i]
            if ($c -eq '{') { $depth++ }
            elseif ($c -eq '}') { $depth--; if ($depth -eq 0) { $end = $i; break } }
        }
        return $ScriptText.Substring($start, $end - $start + 1)
    }

    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-TrxResults')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-DotNetTestResults')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Test-IsNumericValue')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'ConvertTo-AzdoSafeConsole')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-ObjectMemberValue')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotUsageTokenFields')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-TokenFieldSum')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-TokenFieldPathDepth')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Select-CanonicalTokenFields')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotTokenMetrics')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Convert-CopilotCompactNumber')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotCliUsageLineData')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotOtelTokenMetrics')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'New-CopilotTokenUsageRecord')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Test-PhaseRequiresReviewWorktree')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-GateReportRetryClass')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Test-GateReportIsRetryableEnvironmentError')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-GateRetryBudgetMinutes')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Test-GateRetryFitsBudget')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Invoke-ReviewGitCommand')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-FetchedRemoteBranchSha')
    $script:stopTrustedCatalystOverlayFailureBody = Get-FunctionBody -ScriptText $content -FunctionName 'Stop-TrustedCatalystOverlayFailure'
    . (Join-Path $PSScriptRoot 'shared/Invoke-GhCommandWithRetry.ps1')
}

Describe 'Phase worktree requirements' {
    It 'requires the prepared review worktree for Gate and CopilotReview only' {
        Test-PhaseRequiresReviewWorktree -PhaseName 'Setup' | Should -BeFalse
        Test-PhaseRequiresReviewWorktree -PhaseName 'Gate' | Should -BeTrue
        Test-PhaseRequiresReviewWorktree -PhaseName 'CopilotReview' | Should -BeTrue
        Test-PhaseRequiresReviewWorktree -PhaseName 'Post' | Should -BeFalse
    }
}

Describe 'Inflight target branch fetch' {
    It 'does not resolve or merge a stale cached ref when fetch fails' {
        Mock Invoke-ReviewGitCommand {
            param([string[]]$Arguments)
            if ($Arguments[0] -eq 'fetch') {
                return [pscustomobject]@{
                    ExitCode = 128
                    Output = 'fatal: unable to access remote'
                }
            }
            throw "The stale cached ref must not be resolved: $($Arguments -join ' ')"
        }

        {
            Get-FetchedRemoteBranchSha -RemoteName 'origin' -BranchName 'inflight/current'
        } | Should -Throw '*inconclusive due to a retryable environment/infrastructure failure*'

        Should -Invoke Invoke-ReviewGitCommand -Times 1 -Exactly -ParameterFilter {
            $Arguments -join ' ' -eq 'fetch origin inflight/current'
        }
        Should -Invoke Invoke-ReviewGitCommand -Times 0 -Exactly -ParameterFilter {
            $Arguments[0] -eq 'rev-parse'
        }
    }

    It 'reports a missing ref as a fetch environment failure instead of a merge conflict' {
        Mock Invoke-ReviewGitCommand {
            param([string[]]$Arguments)
            if ($Arguments[0] -eq 'fetch') {
                return [pscustomobject]@{
                    ExitCode = 128
                    Output = "fatal: couldn't find remote ref inflight/candidate"
                }
            }
            throw "A missing cached ref must not be resolved: $($Arguments -join ' ')"
        }

        $errorMessage = try {
            Get-FetchedRemoteBranchSha -RemoteName 'origin' -BranchName 'inflight/candidate'
            throw 'Expected the failed fetch to terminate branch resolution.'
        } catch {
            $_.Exception.Message
        }

        $errorMessage | Should -Match 'inconclusive due to a retryable environment/infrastructure failure'
        $errorMessage | Should -Not -Match 'merge conflict'
        Should -Invoke Invoke-ReviewGitCommand -Times 1 -Exactly -ParameterFilter {
            $Arguments -join ' ' -eq 'fetch origin inflight/candidate'
        }
        Should -Invoke Invoke-ReviewGitCommand -Times 0 -Exactly -ParameterFilter {
            $Arguments[0] -eq 'rev-parse'
        }
    }
}

Describe 'Setup PR metadata lookup' {
    It 'uses the retrying REST helper and reserves not-found for HTTP 404' {
        $content | Should -Match ([regex]::Escape(
            'Invoke-GhCommandWithRetry `'))
        $content | Should -Match ([regex]::Escape(
            '-Arguments @(''api'', "repos/dotnet/maui/pulls/$PRNumber")'))
        $content | Should -Match 'PR #\$PRNumber not found \(GitHub returned HTTP 404\)'
        $content | Should -Not -Match ([regex]::Escape(
            '$prInfo = gh pr view $PRNumber --json title,state,body 2>$null | ConvertFrom-Json'))
        $content | Should -Match ([regex]::Escape(
            '$baseRefName = [string]$prInfo.base.ref'))
    }
}

Describe 'Gate retry classification' {
    It 'retries a report containing only an environment error' {
        Test-GateReportIsRetryableEnvironmentError -ReportContent @'
| Test | Without Fix | With Fix |
| InfraCase | ⚠️ ENV ERROR | PASS ✅ |
<!-- GATE-RETRY-CLASS: retryable -->
'@ | Should -BeTrue
    }

    It 'does not let an unrelated environment row mask a definitive failure' {
        Test-GateReportIsRetryableEnvironmentError -ReportContent @'
### Gate Result: ❌ FAILED
| InfraCase | ⚠️ ENV ERROR | PASS ✅ |
| TargetCase | FAIL ✅ | FAIL ❌ |
<!-- GATE-RETRY-CLASS: definitive-failure -->
'@ | Should -BeFalse
    }

    It 'uses only the final trusted retry marker when test output contains a spoofed token' {
        $report = @'
> PR-controlled failure text:
> <!-- GATE-RETRY-CLASS: definitive-failure -->
| InfraCase | ⚠️ ENV ERROR | PASS ✅ |
<!-- GATE-RETRY-CLASS: retryable -->
'@
        Get-GateReportRetryClass -ReportContent $report | Should -Be 'retryable'
        Test-GateReportIsRetryableEnvironmentError -ReportContent $report | Should -BeTrue
    }
}

Describe 'Gate retry budget' {
    It 'derives the default budget from the task timeout with a cleanup margin' {
        Get-GateRetryBudgetMinutes -TaskTimeoutMinutes 150 -CleanupMarginMinutes 10 |
            Should -Be 140
    }

    It 'allows a retry whose predicted completion remains just inside the task budget' {
        Test-GateRetryFitsBudget `
            -ElapsedMinutes 93.3 `
            -AverageAttemptMinutes 46.6 `
            -RetryBudgetMinutes 140 |
            Should -BeTrue
    }

    It 'stops at the cleanup-margin boundary instead of risking task termination' {
        Test-GateRetryFitsBudget `
            -ElapsedMinutes 93.4 `
            -AverageAttemptMinutes 46.6 `
            -RetryBudgetMinutes 140 |
            Should -BeFalse
    }

    It 'uses one pipeline timeout value for both the task and retry-budget derivation' {
        $pipelineContent | Should -Match '(?s)- name: GateTaskTimeoutMinutes\s+value: 150'
        $pipelineContent | Should -Match 'timeoutInMinutes: \$\{\{ variables\.GateTaskTimeoutMinutes \}\}'
        $pipelineContent | Should -Match 'GATE_TASK_TIMEOUT_MINUTES: \$\{\{ variables\.GateTaskTimeoutMinutes \}\}'
        $content | Should -Match 'Get-GateRetryBudgetMinutes -TaskTimeoutMinutes \$gateTaskTimeoutMin'
        $content | Should -Not -Match 'else \{ 95 \}'
    }
}

Describe 'Deep timeout history classification' {
    It 'uses the verified crash/startup predicate instead of treating every env error as a crash' {
        $pipelineContent | Should -Match ([regex]::Escape('. .github/scripts/shared/Get-EnvErrorPatterns.ps1'))
        $pipelineContent | Should -Match 'Test-EnvErrorHistoryHasVerifiedCrashStartup -EnvErrorHistory \$histTokens'
        $pipelineContent | Should -Not -Match '\$histTokens\.Count\s+-gt\s+0'
    }
}

Describe 'Gate trusted overlay failure classification' {
    It 'keeps a non-applicable Catalyst overlay after setup inconclusive' {
        $childScript = @"
$script:stopTrustedCatalystOverlayFailureBody
`$Phase = 'Gate'
Stop-TrustedCatalystOverlayFailure -Message 'Trusted Catalyst screenshot override no longer applies cleanly.'
"@
        $childOutput = & pwsh -NoProfile -Command $childScript 2>&1
        $childExitCode = $LASTEXITCODE

        $childExitCode | Should -Be 3
        ($childOutput -join "`n") | Should -Match 'trusted-overlay failure as infrastructure/inconclusive'

        $restoreStart = $content.IndexOf('function Restore-TrustedScripts')
        $restoreEnd = $content.IndexOf('# ─── Sentinel check:', $restoreStart)
        $restoreBlock = $content.Substring($restoreStart, $restoreEnd - $restoreStart)
        $restoreBlock | Should -Match ([regex]::Escape(
            'Stop-TrustedCatalystOverlayFailure -Message "Trusted Catalyst screenshot override no longer applies cleanly; the PR or target branch changed UITest.cs."'))

        $gateStart = $pipelineContent.IndexOf('GATE_VERDICT_FILE="$(Build.ArtifactStagingDirectory)/gate-result.txt"')
        $gateEnd = $pipelineContent.IndexOf('echo "Trusted gate verdict: $GATE_VERDICT"', $gateStart)
        $gateBlock = $pipelineContent.Substring($gateStart, $gateEnd - $gateStart)
        $overlayExit = $gateBlock.IndexOf('if [ $GATE_EXIT -eq 3 ]')
        $setupComplete = $gateBlock.IndexOf('elif [ $GATE_EXIT -ne 0 ]', $overlayExit)
        $genuineFailure = $gateBlock.IndexOf('GATE_VERDICT="FAILED"', $setupComplete)

        $overlayExit | Should -BeGreaterThan -1
        $setupComplete | Should -BeGreaterThan $overlayExit
        $genuineFailure | Should -BeGreaterThan $setupComplete
        $gateBlock.Substring($overlayExit, $setupComplete - $overlayExit) |
            Should -Match 'GATE_VERDICT="INCONCLUSIVE"'
    }
}

Describe 'Copilot reviewer configuration' {
    It 'defaults the main review orchestrator to GPT-5.6 Sol with long context' {
        $content | Should -Match ([regex]::Escape("else { 'gpt-5.6-sol' }"))
        $content | Should -Match '--context long_context'
    }

    It 'hard-caps both Copilot review calls and bounds try-fix to two candidates' {
        $content | Should -Match '\[ValidateRange\(30, 10000\)\]'
        $content | Should -Match ([regex]::Escape('--max-ai-credits $MaxAiCredits'))
        $content | Should -Match 'STEP 5a: TRY-FIX.*-MaxAiCredits 2000'
        $content | Should -Match 'STEP 5b: EXPERT REVIEW \+ COMPARE.*-MaxAiCredits 1500'
        $content | Should -Match 'Produce \*\*at most two candidates total\*\*'
        $content | Should -Match 'Do not launch cross-pollination'
        $content | Should -Match ([regex]::Escape('## ✅ Final Recommendation: APPROVE'))
        $content | Should -Match ([regex]::Escape('## ⚠️ Final Recommendation: REQUEST CHANGES'))
        $content | Should -Match 'Use ``REQUEST CHANGES`` when ``pr-plus-reviewer`` or any ``try-fix-N`` wins'
        $content | Should -Match 'Compare the CURRENT title and description above against the raw submitted PR diff only'
        $content | Should -Match 'Never describe\s+``pr-plus-reviewer`` or ``try-fix-\*`` behavior'
        $content | Should -Match 'changes already present in the submitted PR HEAD'
        $content | Should -Match '(?s)Apply-AgentLabels.*-TrustedGateResult \$trustedGateResultForPost'
        $content | Should -Match ([regex]::Escape("[ValidateSet('PASSED', 'SKIPPED', 'INCONCLUSIVE', 'FAILED', 'TIMEDOUT', '')]"))
        $pipelineContent | Should -Match '(?s)IsNullOrWhiteSpace\(\$gateResult\).*?\$gateResult = ''TIMEDOUT'''
        $pipelineContent | Should -Match ([regex]::Escape("variable=effectiveTrustedGateResult]`$gateResult"))
        $pipelineContent | Should -Match ([regex]::Escape('-TrustedGateResult "$(effectiveTrustedGateResult)"'))
    }

    It 'extends Copilot startup retries for confirmed GitHub 429 and 5xx failures' {
        $content | Should -Match ([regex]::Escape('$maxCopilotAuthAttempts = 5'))
        $content | Should -Match ([regex]::Escape('$maxNonServiceAuthAttempts = 3'))
        $content | Should -Match ([regex]::Escape('Test-GhCommandFailureIsTransient -Detail $line'))
        $content | Should -Match ([regex]::Escape('$copilotAuthRetryBaseDelaySec * [Math]::Pow(2, $copilotAttempt - 1)'))
        $content | Should -Match ([regex]::Escape('[Math]::Min('))
        $content | Should -Match 'transient GitHub auth-validation service failure'
        $content | Should -Not -Match 'transient auth-validation 401'
    }

    It 'defaults the local test reviewer to GPT-5.6 Sol with long context' {
        $reviewTests = Get-Content -Raw (Join-Path $PSScriptRoot 'Review-Tests.ps1')
        $reviewTests | Should -Match ([regex]::Escape('else { "gpt-5.6-sol" }'))
        $reviewTests | Should -Match ([regex]::Escape('"--context", "long_context"'))
    }

    It 'delegates Post label transitions to the shared REST helper' {
        $content | Should -Not -Match '(?m)^\s*gh\s+pr\s+edit\b'
        $content | Should -Not -Match ([regex]::Escape('s/agent-gate-skipped'))
        $content | Should -Match '(?s)Apply-AgentLabels.*-TrustedGateResult \$trustedGateResultForPost'
    }
}

Describe 'Reviewer pipeline timeout containment' {
    It 'preserves the authoritative merge-conflict notice instead of posting a generic retry warning' {
        ([regex]::Matches($content, [regex]::Escape("Set-SetupOutcome -Outcome 'MERGE_CONFLICT'"))).Count |
            Should -Be 2
        $content | Should -Match ([regex]::Escape("Set-SetupOutcome -Outcome 'COMPLETED'"))
        ([regex]::Matches($content, [regex]::Escape('-IncludeReviewIncomplete'))).Count |
            Should -BeGreaterOrEqual 2
        $pipelineContent | Should -Match ([regex]::Escape('variable=setupResult;isOutput=true'))
        $pipelineContent | Should -Match ([regex]::Escape("trustedSetupResult: `$[ dependencies.CopilotReview.outputs['RunSetup.setupResult'] ]"))
        $pipelineContent | Should -Match ([regex]::Escape("ne(variables['trustedSetupResult'], 'MERGE_CONFLICT')"))
        $pipelineContent | Should -Match ([regex]::Escape("ne(dependencies.ReviewPR.outputs['CopilotReview.RunSetup.setupResult'], 'MERGE_CONFLICT')"))
    }

    It 'treats the Task 3 safety timeout as non-blocking' {
        $task3Start = $pipelineContent.IndexOf("displayName: 'Task 3: Copilot Review (expert review + try-fix)'")
        $task3Start | Should -BeGreaterThan -1
        $task3Block = $pipelineContent.Substring($task3Start, [Math]::Min(1400, $pipelineContent.Length - $task3Start))
        $task3Block | Should -Match 'timeoutInMinutes: 180'
        $task3Block | Should -Match 'continueOnError: true'
    }

    It 'prepares one isolated pr-plus-reviewer sandbox with current build tasks' {
        $content | Should -Match ([regex]::Escape('Join-Path $prPlusSandboxBase "pr-$PRNumber-pr-plus-reviewer"'))
        $content | Should -Match ([regex]::Escape('git -C $RepoRoot worktree add --detach $prPlusSandboxRoot HEAD'))
        $content | Should -Match ([regex]::Escape('Restore-TrustedScripts -TrustedScriptsDir $TrustedScriptsDir -RepoRoot $prPlusSandboxRoot'))
        $content | Should -Match ([regex]::Escape('commit -m "Trusted reviewer infrastructure overlay"'))
        $content | Should -Match ([regex]::Escape('$prPlusCandidateBaseCommit = (& git -C $prPlusSandboxRoot rev-parse HEAD'))
        $content | Should -Match ([regex]::Escape("Join-Path `$RepoRoot '.buildtasks'"))
        $content | Should -Match ([regex]::Escape('Copy-Item -LiteralPath $rawBuildTasks -Destination $candidateBuildTasks -Recurse -Force'))
        $content | Should -Match 'READY_WITH_BUILDTASKS'
        $content | Should -Match ([regex]::Escape('Candidate baseline commit after trusted infrastructure overlay: ``$prPlusCandidateBaseCommit``'))
        $content | Should -Match ([regex]::Escape('Exact persistent candidate artifact root: ``$prPlusArtifactRoot``'))
        $content | Should -Match ([regex]::Escape('Use this exact candidate worktree. Do not create another worktree or sandbox'))
        $content | Should -Match ([regex]::Escape('git -C "$prPlusSandboxRoot" rev-parse --show-toplevel'))
        $content | Should -Match ([regex]::Escape('An output path rooted at ``$RepoRoot`` proves the raw PR ran'))
        $content | Should -Match ([regex]::Escape('git -C "$prPlusSandboxRoot" diff --check "$prPlusCandidateBaseCommit"'))
        $content | Should -Match ([regex]::Escape('git -C "$prPlusSandboxRoot" diff --binary "$prPlusCandidateBaseCommit"'))
        $content | Should -Match ([regex]::Escape('$prPlusArtifactRoot/reviewer.patch'))
        $content | Should -Match ([regex]::Escape('$prPlusArtifactRoot/candidate.patch'))
        $content | Should -Match ([regex]::Escape('"CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-plus-reviewer-sandbox"'))
        $content | Should -Match 'Removed legacy candidate sandbox from review artifacts'
        $content | Should -Match ([regex]::Escape('git -C $RepoRoot worktree remove --force --force $prPlusSandboxRoot'))
        $content | Should -Match ([regex]::Escape('git -C $RepoRoot worktree prune --expire now'))
        $content | Should -Match 'Could not fully remove pr-plus-reviewer sandbox'
        $content | Should -Match 'The sandbox is temporary and must not be copied into review artifacts'
    }

    It 'runs regression tests through trusted scripts overlaid into the review worktree' {
        $regressionStart = $content.IndexOf('# --- Regression Test Execution (part of STEP 3) ---')
        $regressionEnd = $content.IndexOf('#  STEP 4: Gate - Test Before and After Fix', $regressionStart)
        $regressionStart | Should -BeGreaterThan -1
        $regressionEnd | Should -BeGreaterThan $regressionStart

        $regressionBlock = $content.Substring($regressionStart, $regressionEnd - $regressionStart)
        $regressionBlock | Should -Match ([regex]::Escape('$uiTestRunner = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"'))
        $regressionBlock | Should -Match ([regex]::Escape('$deviceTestRunner = Join-Path $RepoRoot ".github/skills/run-device-tests/scripts/Run-DeviceTests.ps1"'))
        $regressionBlock | Should -Not -Match ([regex]::Escape('$uiTestRunner = Join-Path $ScriptsDir'))
        $regressionBlock | Should -Not -Match ([regex]::Escape('$deviceTestRunner = Join-Path $SkillsDir'))
    }

    It 'never uses CopilotLogs to mutate credentialed PR metadata' {
        $runPostStart = $pipelineContent.IndexOf("displayName: 'Task 4: Post (comments + labels)'")
        $runPostBlock = $pipelineContent.Substring($runPostStart, [Math]::Min(800, $pipelineContent.Length - $runPostStart))
        $downloadLogs = $pipelineContent.IndexOf("displayName: 'Download CopilotLogs'", $pipelineContent.IndexOf("- stage: UpdateAISummaryComment"))

        $runPostStart | Should -BeGreaterThan -1
        $runPostBlock | Should -Match ([regex]::Escape('SKIP_PR_FINALIZE_APPLY: "true"'))
        $downloadLogs | Should -BeGreaterThan -1
        $pipelineContent | Should -Not -Match ([regex]::Escape("displayName: 'Apply PR title/description'"))
        $pipelineContent | Should -Not -Match ([regex]::Escape('./.github/scripts/apply-pr-finalize.ps1'))
        $pipelineContent | Should -Match '(?s)CopilotLogs.*must never drive a credentialed PR title/body mutation'
    }

    It 'runs prompt-influenced UI failure analysis before persisted publication credentials exist' {
        $stageStart = $pipelineContent.IndexOf('- stage: UpdateAISummaryComment')
        $stageEnd = $pipelineContent.IndexOf('- stage: CleanupReviewLock', $stageStart)
        $stageBlock = $pipelineContent.Substring($stageStart, $stageEnd - $stageStart)

        $initialCheckout = $stageBlock.IndexOf('- checkout: self')
        $analysis = $stageBlock.IndexOf("displayName: 'Analyze UI test failures (Copilot)'")
        $credentialCheckout = $stageBlock.IndexOf("displayName: 'Enable trusted snapshot asset publication credential'")
        $credentialCheck = $stageBlock.IndexOf("displayName: 'Verify snapshot asset publication credential'")
        $post = $stageBlock.IndexOf("displayName: 'Post AI summary review'")

        $initialCheckout | Should -BeGreaterThan -1
        $analysis | Should -BeGreaterThan $initialCheckout
        $credentialCheckout | Should -BeGreaterThan $analysis
        $credentialCheck | Should -BeGreaterThan $credentialCheckout
        $post | Should -BeGreaterThan $credentialCheck

        $initialCheckoutBlock = $stageBlock.Substring($initialCheckout, $analysis - $initialCheckout)
        $initialCheckoutBlock | Should -Match 'persistCredentials:\s*false'
        $initialCheckoutBlock | Should -Not -Match 'persistCredentials:\s*true'

        $analysisBlock = $stageBlock.Substring($analysis, $credentialCheckout - $analysis)
        $analysisBlock | Should -Match 'COPILOT_GITHUB_TOKEN:\s*\$\(COPILOT_TOKEN\)'
        $analysisBlock | Should -Not -Match '(?m)^\s+GH_TOKEN:'
        $analysisBlock | Should -Not -Match '(?m)^\s+ASSET_WRITE_TOKEN:'

        $credentialBlock = $stageBlock.Substring($credentialCheckout, $post - $credentialCheckout)
        $credentialBlock | Should -Match 'clean:\s*true'
        $credentialBlock | Should -Match 'persistCredentials:\s*true'
        $credentialBlock | Should -Match ([regex]::Escape(
            "git config --get-regexp '^http\..*\.extraheader$'"))
        $credentialBlock | Should -Match 'throw "Snapshot asset publication requires the persisted checkout credential'

        $afterCredential = $stageBlock.Substring($credentialCheckout)
        $afterCredential | Should -Not -Match 'Analyze-UITestFailures\.ps1'
        $afterCredential | Should -Not -Match '\bcopilot\s+--allow-all\b'
    }

    It 'does not enumerate or log credential identities and capabilities in Stage 3' {
        $stageStart = $pipelineContent.IndexOf('- stage: UpdateAISummaryComment')
        $stageEnd = $pipelineContent.IndexOf('- stage: CleanupReviewLock', $stageStart)
        $stageBlock = $pipelineContent.Substring($stageStart, $stageEnd - $stageStart)

        $stageBlock | Should -Not -Match '\[EMBED-DIAG\]'
        $stageBlock | Should -Not -Match 'CHECKOUT_PAT'
        $stageBlock | Should -Not -Match 'Contents:write probe'
        $stageBlock | Should -Not -Match 'push=\{[0-9]+\}'
        $stageBlock | Should -Match ([regex]::Escape(
            'Write-Host "Snapshot asset publication credential verified."'))
    }

    It 'pins Deep UI and all PR mutations to the immutable Setup snapshot' {
        $content | Should -Match ([regex]::Escape('"review-snapshot.json"'))
        $content | Should -Match ([regex]::Escape('prHeadSha = $reviewedPrHeadSha'))
        $content | Should -Match ([regex]::Escape('baseSha = $reviewedBaseSha'))
        $content | Should -Match ([regex]::Escape('-ExpectedHeadSha $ReviewedCommit'))
        $content | Should -Match 'Label application deferred to Stage 3'

        $pipelineContent | Should -Match ([regex]::Escape('variable=reviewedPrHeadSha;isOutput=true'))
        $pipelineContent | Should -Match ([regex]::Escape('variable=reviewedBaseSha;isOutput=true'))
        $pipelineContent | Should -Match ([regex]::Escape('variable=reviewedBaseRef;isOutput=true'))
        $pipelineContent | Should -Match ([regex]::Escape("reviewedPrHeadSha: `$[ stageDependencies.ReviewPR.CopilotReview.outputs['RunSetup.reviewedPrHeadSha'] ]"))
        $pipelineContent | Should -Match ([regex]::Escape('git merge --squash "${PR_HEAD_SHA}"'))
        $pipelineContent | Should -Match ([regex]::Escape('-ReviewedCommit "$(trustedReviewedPrHeadSha)"'))
        $pipelineContent | Should -Match ([regex]::Escape('-ReviewedCommit "$(reviewedPrHeadSha)"'))
        $pipelineContent | Should -Match ([regex]::Escape('-ExpectedHeadSha "$(reviewedPrHeadSha)"'))
        $pipelineContent | Should -Match ([regex]::Escape("cleanupReviewedPrHeadSha: `$[ dependencies.ReviewPR.outputs['CopilotReview.RunSetup.reviewedPrHeadSha'] ]"))
        $pipelineContent | Should -Match 'marking the immutable reviewed head incomplete'
        $pipelineContent | Should -Match '(?s)CURRENT_HEAD.*EXPECTED_HEAD.*s/agent-review-incomplete'
    }

    It 'skips expensive downstream stages after cancellation but always cleans the review lock' {
        $postReviewJobStart = $pipelineContent.IndexOf("      - job: PostReview")
        $deepStart = $pipelineContent.IndexOf("- stage: RunDeepUITests")
        $postStart = $pipelineContent.IndexOf("- stage: UpdateAISummaryComment")
        $cleanupStart = $pipelineContent.IndexOf("- stage: CleanupReviewLock")
        $analyzeStart = $pipelineContent.IndexOf("- stage: AnalyzeCopilotTokenUsage")

        $postReviewJobStart | Should -BeGreaterThan -1
        $deepStart | Should -BeGreaterThan $postReviewJobStart
        $postStart | Should -BeGreaterThan $deepStart
        $cleanupStart | Should -BeGreaterThan $postStart
        $analyzeStart | Should -BeGreaterThan $cleanupStart

        $postReviewJobBlock = $pipelineContent.Substring(
            $postReviewJobStart,
            $deepStart - $postReviewJobStart)
        $deepBlock = $pipelineContent.Substring($deepStart, $postStart - $deepStart)
        $postBlock = $pipelineContent.Substring($postStart, $cleanupStart - $postStart)
        $cleanupBlock = $pipelineContent.Substring($cleanupStart, $analyzeStart - $cleanupStart)
        $analyzeBlock = $pipelineContent.Substring($analyzeStart)

        $postReviewJobBlock | Should -Match (
            "condition: in\(dependencies\.CopilotReview\.result, 'Succeeded', 'SucceededWithIssues', 'Failed', 'Canceled'\)")
        $deepBlock | Should -Match 'not\(canceled\(\)\)'
        $deepBlock | Should -Not -Match "'Canceled'"
        $postBlock | Should -Match 'condition: and\(not\(canceled\(\)\)'
        $cleanupBlock | Should -Match 'condition: always\(\)'
        $cleanupBlock | Should -Match 'SYSTEM_ACCESSTOKEN: \$\(System\.AccessToken\)'
        $cleanupBlock | Should -Match '--oauth2-bearer "\$\{SYSTEM_ACCESSTOKEN\}"'
        $cleanupBlock | Should -Not -Match 'Authorization:\s+\*+'
        $cleanupBlock | Should -Match '\.templateParameters\.PRNumber'
        $cleanupBlock | Should -Match '\.id != \$current and \.status != "completed"'
        $cleanupBlock | Should -Match 'Preserving s/agent-review-in-progress'
        $cleanupBlock.IndexOf('OTHER_ACTIVE=') | Should -BeLessThan $cleanupBlock.IndexOf('repos/dotnet/maui/issues/${PR_NUM}/labels')
        $analyzeBlock | Should -Match 'condition: not\(canceled\(\)\)'
    }

    It 'gives every Android emulator retry group enough time and keeps setup non-blocking' {
        $avdBlocks = [regex]::Matches(
            $pipelineContent,
            "(?s)displayName: 'Create AVD and Boot Android Emulator'.{0,700}?timeoutInMinutes: 25.{0,700}?continueOnError: true"
        )
        $avdBlocks.Count | Should -Be 2
    }

    It 'requires the adb transport state column to be device instead of matching metadata text' {
        $pipelineContent | Should -Not -Match 'adb devices \| grep ["'']emulator\.\*device'
        ([regex]::Matches($pipelineContent, [regex]::Escape('$2 == "device"'))).Count | Should -Be 4
    }

    It 'honors skipCertificates and bounds every best-effort Android warmup adb call' {
        $provisionContent | Should -Match 'ne\(parameters\.skipCertificates, true\)'

        $warmupStart = $pipelineContent.IndexOf('# Warm up the emulator right before the agent runs.')
        $warmupEnd = $pipelineContent.IndexOf("#  Task 1 — SETUP", $warmupStart)
        $warmupStart | Should -BeGreaterThan -1
        $warmupEnd | Should -BeGreaterThan $warmupStart
        $warmupBlock = $pipelineContent.Substring($warmupStart, $warmupEnd - $warmupStart)

        $warmupBlock | Should -Match 'adb_safe\(\)'
        $warmupBlock | Should -Match 'timeout 5 adb -s "\$DEVICE_ID"'
        $warmupBlock | Should -Not -Match '(?m)^\s*adb -s "\$DEVICE_ID"'
        $warmupBlock | Should -Match 'Emulator still not booted after ADB restart — skipping the remaining warmup'
    }

    It 'runs optional token telemetry without cloning the full repository' {
        $stageStart = $pipelineContent.IndexOf("- stage: AnalyzeCopilotTokenUsage")
        $stageStart | Should -BeGreaterThan -1
        $stageBlock = $pipelineContent.Substring($stageStart)
        $jobStart = $stageBlock.IndexOf("- job: AnalyzeTokenUsage")
        $stepsStart = $stageBlock.IndexOf("        steps:", $jobStart)
        $jobHeader = $stageBlock.Substring($jobStart, $stepsStart - $jobStart)

        $jobHeader | Should -Match 'continueOnError: true'
        $stageBlock | Should -Match ([regex]::Escape('- checkout: none'))
        $stageBlock | Should -Not -Match ([regex]::Escape('- checkout: self'))
        $stageBlock | Should -Match ([regex]::Escape("artifactName: 'CopilotTelemetryTools'"))
        $stageBlock | Should -Match ([regex]::Escape('$(Pipeline.Workspace)/CopilotTelemetryTools'))
    }

    It 'publishes the telemetry helper captured before PR-controlled code runs' {
        $capture = $pipelineContent.IndexOf('$source = Join-Path "$(Build.SourcesDirectory)" ".github/scripts/shared/Aggregate-CopilotTokenUsage.ps1"')
        $firstBranchSwitch = $pipelineContent.IndexOf('git checkout --detach')
        $publishStart = $pipelineContent.IndexOf("- task: PublishPipelineArtifact@1", $capture)
        $publishEnd = $pipelineContent.IndexOf("# ─────────────────────────────────────────────────────────", $publishStart)
        $publishBlock = $pipelineContent.Substring($publishStart, $publishEnd - $publishStart)

        $capture | Should -BeGreaterThan -1
        $capture | Should -BeLessThan $firstBranchSwitch
        $publishStart | Should -BeGreaterThan $capture
        $publishStart | Should -BeLessThan $firstBranchSwitch
        $pipelineContent | Should -Match ([regex]::Escape('".github/scripts/shared/Aggregate-CopilotTokenUsage.ps1"'))
        $publishBlock | Should -Match ([regex]::Escape("artifact: 'CopilotTelemetryTools'"))
        $publishBlock | Should -Match 'timeoutInMinutes: 2'
        $publishBlock | Should -Match 'continueOnError: true'
    }

    It 'runs Catalyst desktop setup and cleanup from trusted scripts' {
        $pipelineContent | Should -Match ([regex]::Escape('$(Build.ArtifactStagingDirectory)/trusted-github/eng-scripts/disable-notification-center.sh'))
        $pipelineContent | Should -Match ([regex]::Escape('$(Build.ArtifactStagingDirectory)/trusted-github/eng-scripts/dismiss-apple-account-dialog.sh'))
        $pipelineContent | Should -Match ([regex]::Escape('$(Build.ArtifactStagingDirectory)/trusted-github/eng-scripts/dismiss-maccatalyst-app-recovery-dialog.sh'))
        $pipelineContent | Should -Match ([regex]::Escape('$(Build.ArtifactStagingDirectory)/trusted-github/eng-scripts/enable-notification-center.sh'))
        $pipelineContent | Should -Not -Match ([regex]::Escape('$(System.DefaultWorkingDirectory)/eng/scripts/disable-notification-center.sh'))
        $pipelineContent | Should -Not -Match ([regex]::Escape('$(System.DefaultWorkingDirectory)/eng/scripts/dismiss-apple-account-dialog.sh'))
        $pipelineContent | Should -Not -Match ([regex]::Escape('$(System.DefaultWorkingDirectory)/eng/scripts/dismiss-maccatalyst-app-recovery-dialog.sh'))
        $pipelineContent | Should -Not -Match ([regex]::Escape('$(System.DefaultWorkingDirectory)/eng/scripts/enable-notification-center.sh'))

        $cleanupStart = $pipelineContent.LastIndexOf("- bash:", $pipelineContent.IndexOf("displayName: 'Re-enable Notification Center'"))
        $cleanupEnd = $pipelineContent.IndexOf("- task: PublishPipelineArtifact@1", $cleanupStart)
        $cleanupBlock = $pipelineContent.Substring($cleanupStart, $cleanupEnd - $cleanupStart)
        $cleanupBlock | Should -Match 'condition: always\(\)'
    }

    It 'captures trusted deep-test scripts outside the retried branch-resolution task' {
        $captureName = "displayName: 'Capture trusted scripts for deep UI tests'"
        $resolveName = "displayName: 'Resolve PR base branch (workloads + merge base)'"
        $captureStart = $pipelineContent.LastIndexOf("- bash:", $pipelineContent.IndexOf($captureName))
        $captureEnd = $pipelineContent.IndexOf($resolveName, $captureStart)
        $resolveStart = $pipelineContent.LastIndexOf("- bash:", $pipelineContent.IndexOf($resolveName, $captureStart))
        $resolveEnd = $pipelineContent.IndexOf("- template: common/provision.yml", $resolveStart)
        $captureBlock = $pipelineContent.Substring($captureStart, $captureEnd - $captureStart)
        $resolveBlock = $pipelineContent.Substring($resolveStart, $resolveEnd - $resolveStart)

        $captureStart | Should -BeGreaterThan -1
        $captureStart | Should -BeLessThan $resolveStart
        $captureBlock | Should -Match ([regex]::Escape('cp -r .github/scripts "$TRUSTED/scripts"'))
        $captureBlock | Should -Match ([regex]::Escape('cp -r eng/scripts     "$TRUSTED/eng-scripts"'))
        $captureBlock | Should -Match ([regex]::Escape('cp .github/patches/catalyst-retina-screenshot.patch "$TRUSTED/source-overrides/"'))
        $captureBlock | Should -Not -Match 'retryCountOnTaskFailure'
        $resolveBlock | Should -Match 'retryCountOnTaskFailure: 2'
        $resolveBlock | Should -Not -Match ([regex]::Escape('cp -r .github/scripts'))
    }

    It 'captures trusted review infrastructure outside the retried branch-resolution task' {
        $captureName = "displayName: 'Capture trusted test infrastructure'"
        $resolveName = "displayName: 'Resolve PR base branch (workloads + merge base)'"
        $captureStart = $pipelineContent.LastIndexOf("- bash:", $pipelineContent.IndexOf($captureName))
        $captureEnd = $pipelineContent.IndexOf($resolveName, $captureStart)
        $resolveStart = $pipelineContent.LastIndexOf("- bash:", $pipelineContent.IndexOf($resolveName, $captureStart))
        $resolveEnd = $pipelineContent.IndexOf("- template: common/enable-kvm.yml", $resolveStart)
        $captureBlock = $pipelineContent.Substring($captureStart, $captureEnd - $captureStart)
        $resolveBlock = $pipelineContent.Substring($resolveStart, $resolveEnd - $resolveStart)

        $captureStart | Should -BeGreaterThan -1
        $captureStart | Should -BeLessThan $resolveStart
        $captureBlock | Should -Match ([regex]::Escape('cp -r .github/scripts "$TRUSTED/scripts"'))
        $captureBlock | Should -Match ([regex]::Escape('cp .github/patches/catalyst-retina-screenshot.patch "$TRUSTED/source-overrides/"'))
        $captureBlock | Should -Not -Match 'retryCountOnTaskFailure'
        $resolveBlock | Should -Match 'retryCountOnTaskFailure: 2'
        $resolveBlock | Should -Not -Match ([regex]::Escape('cp -r .github/scripts'))
        $resolveBlock | Should -Not -Match 'source-overrides'
    }

    It 'keeps credential-bearing setup on the protected base branch' {
        $resolveName = "displayName: 'Resolve PR base branch (workloads + merge base)'"
        $resolveStart = $pipelineContent.LastIndexOf(
            "- bash:",
            $pipelineContent.IndexOf($resolveName))
        $resolveEnd = $pipelineContent.IndexOf(
            "- template: common/enable-kvm.yml",
            $resolveStart)
        $resolveBlock = $pipelineContent.Substring(
            $resolveStart,
            $resolveEnd - $resolveStart)
        $installWorkloads = $pipelineContent.IndexOf(
            "displayName: 'Install .NET and workloads'",
            $resolveEnd)
        $buildTasks = $pipelineContent.IndexOf(
            "displayName: 'Build MSBuild Tasks'",
            $installWorkloads)
        $setup = $pipelineContent.IndexOf(
            'echo "═══ TASK 1: SETUP ═══"',
            $buildTasks)

        $resolveBlock | Should -Match (
            [regex]::Escape('git checkout --detach "origin/${BASE_REF}"'))
        $resolveBlock | Should -Not -Match 'pull/\$\{PARAM_PR_NUMBER\}/head'
        $resolveBlock | Should -Not -Match 'git checkout --detach FETCH_HEAD'
        $installWorkloads | Should -BeGreaterThan $resolveEnd
        $buildTasks | Should -BeGreaterThan $installWorkloads
        $setup | Should -BeGreaterThan $buildTasks
    }

    It 'reapplies the trusted Catalyst screenshot harness after PR branch switches' {
        ([regex]::Matches(
            $pipelineContent,
            [regex]::Escape('cp .github/patches/catalyst-retina-screenshot.patch "$TRUSTED/source-overrides/"')
        )).Count | Should -Be 2

        $content | Should -Match ([regex]::Escape("source-overrides/catalyst-retina-screenshot.patch"))
        $content | Should -Match ([regex]::Escape('git apply --reverse --check --whitespace=nowarn'))
        $pipelineContent | Should -Match ([regex]::Escape('Applied trusted Catalyst Retina screenshot override'))
        $pipelineContent | Should -Match 'the PR or target branch changed UITest\.cs'
        $pipelineContent | Should -Match ([regex]::Escape("displayName: 'Restore trusted test infrastructure for deep UI tests'"))
    }

    It 'reports skipped deep UI tests in category rows, the headline, and the total' {
        $pipelineContent | Should -Match ([regex]::Escape('$totalPassed = 0; $totalFailed = 0; $totalSkipped = 0'))
        $pipelineContent | Should -Match ([regex]::Escape('$totalSkipped += [int]$b.Skipped'))
        $pipelineContent | Should -Match ([regex]::Escape('elseif ($tSkip -gt 0) { "$tPass/$tCount ($tSkip skipped) ✓" }'))
        $pipelineContent | Should -Match ([regex]::Escape('$regularFailed failed$skippedSummary across $categoryText'))
        $pipelineContent | Should -Match ([regex]::Escape('$totalPassed + $totalFailed + $totalSkipped'))
    }

    It 'retries the deferred review-incomplete notice without misreporting a merge conflict' {
        $fallbackStart = $pipelineContent.IndexOf('No PRAgent content and no deep results')
        $fallbackEnd = $pipelineContent.IndexOf('# Replace in-process results with deep results', $fallbackStart)

        $fallbackStart | Should -BeGreaterThan -1
        $fallbackEnd | Should -BeGreaterThan $fallbackStart
        $fallbackBlock = $pipelineContent.Substring($fallbackStart, $fallbackEnd - $fallbackStart)

        $pipelineContent | Should -Match ([regex]::Escape(
            '. $ghRetryHelper'))
        $fallbackBlock | Should -Match 'Invoke-GhCommandWithRetry'
        $fallbackBlock | Should -Match 'post the review-incomplete notice'
        $fallbackBlock | Should -Match 'transient GitHub/CI API failure'
        $fallbackBlock | Should -Match 'does \*\*not\*\* identify a merge conflict'
        $fallbackBlock | Should -Not -Match 'gh pr comment \$prNumber'
    }

    It 'bounds and deduplicates deep UI diagnostics without duplicating canonical snapshots' {
        $pipelineContent | Should -Match ([regex]::Escape('. ".github/scripts/shared/Copy-BoundedDiagnosticFile.ps1"'))
        $pipelineContent | Should -Match ([regex]::Escape('$maxDiagnosticLogBytes = 16MB'))
        $pipelineContent | Should -Match ([regex]::Escape('$maxDiagnosticArtifactBytes = 96MB'))
        $pipelineContent | Should -Match ([regex]::Escape('Copy-BoundedDiagnosticFileSet `'))
        $pipelineContent | Should -Match ([regex]::Escape('-MaxTotalBytes $maxDiagnosticArtifactBytes'))
        $pipelineContent | Should -Match ([regex]::Escape('-MaxTextFileBytes $maxDiagnosticLogBytes'))
        $pipelineContent | Should -Match ([regex]::Escape('-MaxBinaryFileBytes $maxDiagnosticFileBytes'))
        $pipelineContent | Should -Match 'screen\.\?shot'
        $pipelineContent | Should -Match 'PageSource'
        $pipelineContent | Should -Match ([regex]::Escape("-not (`$_.Attributes -band [System.IO.FileAttributes]::ReparsePoint)"))
    }

    It 'passes the selected platform into every UI category detection pass' {
        $pipelineContent | Should -Match ([regex]::Escape('-PrNumber "$env:PARAM_PR_NUMBER" -Platform "$env:PARAM_PLATFORM"'))
        $pipelineContent | Should -Match ([regex]::Escape('PARAM_PLATFORM: ${{ parameters.Platform }}'))
        ([regex]::Matches($content, [regex]::Escape('-Platform "$Platform"'))).Count | Should -BeGreaterOrEqual 2
    }

    It 'marks a deep UI category with no runnable tests as succeeded with issues' {
        $pipelineContent | Should -Match ([regex]::Escape('$categoryTestCount = 0'))
        $pipelineContent | Should -Match ([regex]::Escape('local-name()="ResultSummary"'))
        $pipelineContent | Should -Match ([regex]::Escape('elseif ($categoryTestCount -eq 0)'))
        $pipelineContent | Should -Match ([regex]::Escape("contains no runnable tests on platform '`$platform'"))
        $pipelineContent | Should -Match '(?s)elseif \(\$categoryTestCount -eq 0\).*?\$hadFailure = \$true'
        $pipelineContent | Should -Match '(?s)elseif \(\$emptyCategories -gt 0\).*?\$resultIcon = ''⚠️'''
    }
}

Describe 'Snapshot diff asset publishing' {
    It 'publishes through the orphan asset-only branch with a contention fallback' {
        $assetStart = $pipelineContent.IndexOf("`$assetBranch = 'review-tests-assets-v2'")
        $assetEnd = $pipelineContent.IndexOf('# 3) render the collapsible baseline|actual|diff image section', $assetStart)

        $assetStart | Should -BeGreaterThan -1
        $assetEnd | Should -BeGreaterThan $assetStart
        $assetBlock = $pipelineContent.Substring($assetStart, $assetEnd - $assetStart)

        $assetBlock | Should -Match ([regex]::Escape('$assetPrefix = "pr-$prNumber/azdo-review/$(Build.BuildId)"'))
        $assetBlock | Should -Match ([regex]::Escape("parents = @()"))
        $assetBlock | Should -Match ([regex]::Escape("path = '.review-tests-assets'"))
        $assetBlock | Should -Match ([regex]::Escape("'^pr-[1-9][0-9]*$'"))
        $assetBlock | Should -Match ([regex]::Escape("'HTTP (?:401|403|404)"))
        $assetBlock | Should -Match ([regex]::Escape('$maxFf = 6'))
        $assetBlock | Should -Match 'asset ref update permanently rejected'
        $assetBlock | Should -Match ([regex]::Escape('$buildRef = "$assetBranch-b$(Build.BuildId)"'))
        $assetBlock | Should -Match 'unique asset ref publish failed'
        $assetBlock | Should -Not -Match ([regex]::Escape("`$assetBranch = 'review-tests-assets'"))
        $assetBlock | Should -Not -Match 'heavy concurrency'
    }
}

Describe 'Simulator runtime provisioning contract' {
    It 'normalizes array and object-map JSON before counting Ready runtime images' -Skip:(-not (Get-Command jq -ErrorAction SilentlyContinue)) {
        $filterMatch = [regex]::Match(
            $provisionContent,
            "(?ms)^\s*runtime_image_counts_jq='(?<filter>.*?)^\s*'\s*$")

        $filterMatch.Success | Should -BeTrue
        $filter = $filterMatch.Groups['filter'].Value

        foreach ($case in @(
            @{
                Json = '{"first":{"state":"Ready"},"second":{"state":"Deleting"}}'
                Expected = "2`t1"
            },
            @{
                Json = '[{"state":"Ready"},{"state":"Deleting"}]'
                Expected = "2`t1"
            }
        )) {
            $actual = $case.Json | & jq -er $filter

            $LASTEXITCODE | Should -Be 0
            $actual | Should -Be $case.Expected
        }
    }

    It 'rejects wrapper, null, missing-state, and malformed runtime JSON' -Skip:(-not (Get-Command jq -ErrorAction SilentlyContinue)) {
        $filterMatch = [regex]::Match(
            $provisionContent,
            "(?ms)^\s*runtime_image_counts_jq='(?<filter>.*?)^\s*'\s*$")
        $filterMatch.Success | Should -BeTrue
        $filter = $filterMatch.Groups['filter'].Value

        foreach ($invalidJson in @(
            '{"runtimes":[]}',
            'null',
            '{"runtime":{}}',
            '{bad'
        )) {
            $null = $invalidJson | & jq -er $filter 2>$null

            $LASTEXITCODE | Should -Not -Be 0
        }

        $provisionContent | Should -Match 'if ! RUNTIME_COUNTS=\$\(get_runtime_image_counts\); then'
        $provisionContent | Should -Not -Match 'READY_COUNT=.*\|\| echo 0'
    }
}

Describe 'Copilot token usage helpers' {
    It 'normalizes known token fields while preserving raw token field paths' {
        $usage = [pscustomobject]@{
            inputTokens        = 100
            outputTokens       = 40
            totalApiDurationMs = 1234
            nested             = [pscustomobject]@{
                cachedInputTokens = 12
            }
        }

        $metrics = Get-CopilotTokenMetrics -Usage $usage

        $metrics.inputTokens | Should -Be 100
        $metrics.outputTokens | Should -Be 40
        $metrics.cachedInputTokens | Should -Be 12
        $metrics.totalTokens | Should -Be 140
        @($metrics.rawTokenFields).Count | Should -Be 3
        @($metrics.rawTokenFields | Where-Object { $_.Path -eq 'nested.cachedInputTokens' }).Count | Should -Be 1
    }

    It 'prefers the root token aggregate over a nested per-model breakdown (no double-count)' {
        # Regression guard: a payload carrying BOTH a root aggregate and a per-model
        # breakdown must not sum both (1000 + 600 + 400 = 2000); the root wins.
        $usage = [pscustomobject]@{
            inputTokens  = 1000
            outputTokens = 200
            perModel     = @(
                [pscustomobject]@{ inputTokens = 600; outputTokens = 120 },
                [pscustomobject]@{ inputTokens = 400; outputTokens = 80 }
            )
        }

        $metrics = Get-CopilotTokenMetrics -Usage $usage

        $metrics.inputTokens | Should -Be 1000
        $metrics.outputTokens | Should -Be 200
    }

    It 'sums a nested-only token breakdown when no root aggregate exists' {
        # When only the per-model breakdown is present, it should be summed.
        $usage = [pscustomobject]@{
            perModel = @(
                [pscustomobject]@{ inputTokens = 600 },
                [pscustomobject]@{ inputTokens = 400 }
            )
        }

        $metrics = Get-CopilotTokenMetrics -Usage $usage

        $metrics.inputTokens | Should -Be 1000
    }

    It 'parses Copilot CLI AIC and context footer lines' {
        $aicLine = Get-CopilotCliUsageLineData -Line 'Session: 1030 AIC used'
        $contextLine = Get-CopilotCliUsageLineData -Line 'GPT-5.5 • 1.1M context'

        $aicLine.aicUsed | Should -Be 1030
        $contextLine.model | Should -Be 'GPT-5.5'
        $contextLine.contextWindowRaw | Should -Be '1.1M'
        $contextLine.contextWindow | Should -Be 1100000
    }

    It 'reads token counts from Copilot OTel spans with both cache/reasoning naming variants' {
        $otelPath = Join-Path ([System.IO.Path]::GetTempPath()) "copilot-otel-$([guid]::NewGuid()).jsonl"
        try {
            @(
                [ordered]@{
                    type       = 'span'
                    attributes = [ordered]@{
                        'gen_ai.usage.input_tokens'            = 1000
                        'gen_ai.usage.output_tokens'           = 200
                        'gen_ai.usage.cache_read.input_tokens' = 800
                        'gen_ai.usage.reasoning.output_tokens' = 50
                        'github.copilot.cost'                  = 7.5
                    }
                },
                [ordered]@{
                    type       = 'span'
                    attributes = [ordered]@{
                        'gen_ai.usage.input_tokens'            = 500
                        'gen_ai.usage.output_tokens'           = 40
                        'gen_ai.usage.cache_read_input_tokens' = 400
                        'gen_ai.usage.reasoning_output_tokens' = 10
                    }
                }
            ) | ForEach-Object { $_ | ConvertTo-Json -Depth 10 -Compress } | Set-Content $otelPath -Encoding UTF8

            $metrics = Get-CopilotOtelTokenMetrics -Path $otelPath

            $metrics.available | Should -Be $true
            $metrics.inputTokens | Should -Be 1500
            $metrics.outputTokens | Should -Be 240
            $metrics.cachedInputTokens | Should -Be 1200
            $metrics.reasoningOutputTokens | Should -Be 60
            $metrics.totalTokens | Should -Be 1740
            $metrics.copilotCost | Should -Be 7.5
        } finally {
            Remove-Item $otelPath -Force -ErrorAction SilentlyContinue
        }
    }

    It 'builds a telemetry record with raw usage and no hardcoded cost estimate' {
        $usage = [pscustomobject]@{
            prompt_tokens      = 25
            completion_tokens  = 15
            total_tokens       = 45
            totalApiDurationMs = 2000
        }

        $record = New-CopilotTokenUsageRecord `
            -PRNumber 35677 `
            -Platform 'android' `
            -Phase 'CopilotReview' `
            -StepName 'STEP 5a: TRY-FIX' `
            -ModelName 'gpt-5.5' `
            -StartedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:00Z')) `
            -EndedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:05Z')) `
            -DurationMs 5000 `
            -TurnCount 2 `
            -ToolCount 3 `
            -FailedToolCount 1 `
            -Usage $usage `
            -OtelMetrics $null `
            -AicUsed 1030 `
            -ContextWindow 1100000 `
            -ContextWindowRaw '1.1M' `
            -ResultEventSeen $true `
            -ExitCode 0

        $record.prNumber | Should -Be 35677
        $record.scriptPhase | Should -Be 'CopilotReview'
        $record.copilotStep | Should -Be 'STEP 5a: TRY-FIX'
        $record.apiDurationMs | Should -Be 2000
        $record.normalizedTokens.inputTokens | Should -Be 25
        $record.normalizedTokens.outputTokens | Should -Be 15
        $record.normalizedTokens.totalTokens | Should -Be 45
        $record.cliUsage.aicUsed | Should -Be 1030
        $record.cliUsage.contextWindow | Should -Be 1100000
        $record.cliUsage.contextWindowRaw | Should -Be '1.1M'
        $record.usage.total_tokens | Should -Be 45
        $record.costEstimateAvailable | Should -Be $false
    }

    It 'uses OTel token metrics when result usage has no token fields' {
        $otelMetrics = [ordered]@{
            inputTokens           = 500
            outputTokens          = 75
            cachedInputTokens     = 400
            reasoningOutputTokens = 25
            totalTokens           = 575
            copilotCost           = 7.5
            file                  = '/tmp/copilot-otel.jsonl'
        }

        $record = New-CopilotTokenUsageRecord `
            -PRNumber 35677 `
            -Platform 'android' `
            -Phase 'CopilotReview' `
            -StepName 'STEP 5a: TRY-FIX' `
            -ModelName 'gpt-5.5' `
            -StartedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:00Z')) `
            -EndedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:05Z')) `
            -DurationMs 5000 `
            -TurnCount 2 `
            -ToolCount 3 `
            -FailedToolCount 0 `
            -Usage ([pscustomobject]@{ totalApiDurationMs = 1000 }) `
            -OtelMetrics $otelMetrics `
            -AicUsed $null `
            -ContextWindow $null `
            -ContextWindowRaw $null `
            -ResultEventSeen $true `
            -ExitCode 0

        $record.normalizedTokens.inputTokens | Should -Be 500
        $record.normalizedTokens.outputTokens | Should -Be 75
        $record.normalizedTokens.cachedInputTokens | Should -Be 400
        $record.normalizedTokens.reasoningOutputTokens | Should -Be 25
        $record.normalizedTokens.totalTokens | Should -Be 575
        $record.normalizedTokens.otelFile | Should -Be '/tmp/copilot-otel.jsonl'
        # aicUsed stays AIC-only (null here); the dollar cost is reported in its own field,
        # never conflated into aicUsed.
        $record.cliUsage.aicUsed | Should -BeNullOrEmpty
        $record.cliUsage.copilotCost | Should -Be 7.5
    }
}

Describe 'Get-TrxResults' {
    BeforeAll {
        $script:fixtureDir = Join-Path ([System.IO.Path]::GetTempPath()) "trx-fixtures-$(New-Guid)"
        New-Item -ItemType Directory -Path $script:fixtureDir -Force | Out-Null
    }

    AfterAll {
        Remove-Item -Path $script:fixtureDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'returns null for a missing file' {
        $r = Get-TrxResults -TrxPath '/does/not/exist.trx'
        $r | Should -BeNullOrEmpty
    }

    It 'returns null for an empty path' {
        Get-TrxResults -TrxPath '' | Should -BeNullOrEmpty
        Get-TrxResults -TrxPath $null | Should -BeNullOrEmpty
    }

    It 'parses aggregate counters from ResultSummary/Counters' {
        $trx = Join-Path $script:fixtureDir 'aggregate.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="1" name="r" runUser="ci" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Failed">
    <Counters total="619" executed="619" passed="75" failed="544" />
  </ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Total   | Should -Be 619
        $r.Passed  | Should -Be 75
        $r.Failed  | Should -Be 544
        $r.Skipped | Should -Be 0
    }

    It 'computes Skipped as Total-Executed when not separately tracked' {
        $trx = Join-Path $script:fixtureDir 'skipped.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="2" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="100" executed="93" passed="90" failed="3" />
  </ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Total   | Should -Be 100
        $r.Skipped | Should -Be 7   # 100 - 93
    }

    It 'parses individual UnitTestResult nodes into the Results list' {
        $trx = Join-Path $script:fixtureDir 'individual.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="3" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Failed">
    <Counters total="3" executed="3" passed="1" failed="1" />
  </ResultSummary>
  <Results>
    <UnitTestResult testName="Foo" duration="00:00:01.0" outcome="Passed" />
    <UnitTestResult testName="Bar" duration="00:00:02.0" outcome="Failed">
      <Output>
        <ErrorInfo>
          <Message>Expected: True; Actual: False</Message>
          <StackTrace>at Bar() in F.cs:line 42</StackTrace>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
    <UnitTestResult testName="Baz" duration="00:00:00.5" outcome="NotExecuted" />
  </Results>
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Results.Count | Should -Be 3

        $foo = $r.Results | Where-Object { $_.name -eq 'Foo' }
        $foo.status | Should -Be 'Passed'

        $bar = $r.Results | Where-Object { $_.name -eq 'Bar' }
        $bar.status   | Should -Be 'Failed'
        $bar.error    | Should -Be 'Expected: True; Actual: False'
        $bar.stack    | Should -Be 'at Bar() in F.cs:line 42'

        $baz = $r.Results | Where-Object { $_.name -eq 'Baz' }
        $baz.status | Should -Be 'Skipped'   # NotExecuted normalized to Skipped
    }

    It 'normalizes Inconclusive outcome to Skipped' {
        $trx = Join-Path $script:fixtureDir 'inconclusive.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="4" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="1" executed="0" passed="0" failed="0" />
  </ResultSummary>
  <Results>
    <UnitTestResult testName="Maybe" duration="00:00:00" outcome="Inconclusive" />
  </Results>
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        (Get-TrxResults -TrxPath $trx).Results[0].status | Should -Be 'Skipped'
    }

    It 'returns an empty Results array when there are no UnitTestResult nodes' {
        $trx = Join-Path $script:fixtureDir 'empty.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="5" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="0" executed="0" passed="0" failed="0" />
  </ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Results.Count | Should -Be 0
        $r.Total | Should -Be 0
    }

    It 'gracefully handles malformed XML (returns null, does not throw)' {
        $trx = Join-Path $script:fixtureDir 'bad.trx'
        '<TestRun><not-closed' | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r | Should -BeNullOrEmpty
    }

    It 'returns the original TrxPath in the result for round-tripping' {
        $trx = Join-Path $script:fixtureDir 'pathtrack.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="6" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed"><Counters total="0" executed="0" passed="0" failed="0" /></ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        (Get-TrxResults -TrxPath $trx).TrxPath | Should -Be $trx
    }
}

Describe 'Get-DotNetTestResults (console-scrape fallback)' {
    It 'parses a single Passed entry' {
        $lines = @(
            '  Passed Foo.Bar [12 ms]'
        )
        $r = Get-DotNetTestResults -Lines $lines
        $r.Count | Should -Be 1
        $r[0].status | Should -Be 'Passed'
        $r[0].name   | Should -Be 'Foo.Bar'
    }

    It 'parses multiple consecutive results' {
        $lines = @(
            '  Passed One [1 ms]',
            '  Passed Two [2 ms]',
            '  Failed Three [3 ms]'
        )
        $r = Get-DotNetTestResults -Lines $lines
        $r.Count | Should -Be 3
        ($r | Where-Object { $_.status -eq 'Failed' }).name | Should -Be 'Three'
    }

    It 'captures error message and stack between two results' {
        $lines = @(
            '  Passed Alpha [10 ms]',
            '  Failed Beta [20 ms]',
            '   Error Message:',
            '   Expected: 1; Actual: 2',
            '   Stack Trace:',
            '      at Beta() in B.cs:line 99',
            '  Passed Gamma [5 ms]'
        )
        $r = Get-DotNetTestResults -Lines $lines
        $beta = $r | Where-Object { $_.name -eq 'Beta' }
        $beta.error | Should -Match 'Expected: 1; Actual: 2'
        $beta.stack | Should -Match 'at Beta\(\) in B\.cs:line 99'
    }

    It 'returns an empty array for empty input' {
        (Get-DotNetTestResults -Lines @()).Count | Should -Be 0
    }
}

Describe 'Pipeline pre-trusted command safety' {
    It 'sanitizes both streams from every watchdog build while preserving the build exit code' {
        $sanitizer = "2>&1 | tr -d '\r' | sed -E 's/##vso\[[^]]*\]//g'"

        ([regex]::Matches($pipelineContent, [regex]::Escape($sanitizer))).Count | Should -Be 2
        ([regex]::Matches($pipelineContent, [regex]::Escape("`$psi.FileName = 'bash'"))).Count | Should -Be 2
        ([regex]::Matches($pipelineContent, [regex]::Escape("foreach (`$a in @('-o','pipefail','-c',`$buildCommand))"))).Count | Should -Be 2
        ([regex]::Matches($pipelineContent, [regex]::Escape('& bash -o pipefail -c $buildCommand'))).Count | Should -Be 2
        $pipelineContent | Should -Not -Match ([regex]::Escape("`$psi.FileName = 'pwsh'"))
    }

    It 'uses non-interactive sudo for CoreSimulator recovery before falling back' {
        $safeKill = 'sudo -n killall -9 com.apple.CoreSimulator.CoreSimulatorService 2>/dev/null || killall -9 com.apple.CoreSimulator.CoreSimulatorService 2>/dev/null || true'

        ([regex]::Matches($pipelineContent, [regex]::Escape($safeKill))).Count | Should -Be 2
        $pipelineContent | Should -Not -Match '(?m)^\s*sudo killall -9 com\.apple\.CoreSimulator\.CoreSimulatorService'
    }

    It 'freezes the buildtasks failure state before merging PR code' {
        $freezeIndex = $pipelineContent.IndexOf("displayName: 'Freeze pre-merge buildtasks state'")
        $setupIndex = $pipelineContent.IndexOf("displayName: 'Task 1: Setup (branch + merge)'")
        $gateNameIndex = $pipelineContent.IndexOf("displayName: 'Task 2: Gate (test verification)'")
        $gateStart = $pipelineContent.LastIndexOf('- bash: |', $gateNameIndex)
        $gateEnd = $pipelineContent.IndexOf('#  Task 3 — COPILOT REVIEW', $gateNameIndex)

        $freezeIndex | Should -BeGreaterThan -1
        $freezeIndex | Should -BeLessThan $setupIndex
        $pipelineContent | Should -Match ([regex]::Escape('variable=baseBuildTasksFailed;isOutput=true;isReadOnly=true'))
        $pipelineContent | Should -Match ([regex]::Escape('BASE_BUILDTASKS_FAILED: $(FreezeBuildTasksState.baseBuildTasksFailed)'))

        $gateBlock = $pipelineContent.Substring($gateStart, $gateEnd - $gateStart)
        $gateBlock | Should -Match ([regex]::Escape('if [ "$BASE_BUILDTASKS_FAILED" = "true" ]; then'))
        $gateBlock | Should -Not -Match 'buildtasks-failed\.marker'
    }
}

Describe 'ConvertTo-AzdoSafeConsole' {
    It 'defangs ##vso[ and ##[ logging-command prefixes' {
        ConvertTo-AzdoSafeConsole '##vso[task.setvariable variable=x]y' | Should -Be '## vso[task.setvariable variable=x]y'
        ConvertTo-AzdoSafeConsole '##[command]z' | Should -Be '## [command]z'
    }

    It 'collapses CR/LF that could fabricate a fresh column-0 log line' {
        ConvertTo-AzdoSafeConsole "safe`r##vso[task.complete]" | Should -Be 'safe ## vso[task.complete]'
        ConvertTo-AzdoSafeConsole "Reviewing`n##vso[task.complete result=Succeeded;]done" | Should -Be 'Reviewing ## vso[task.complete result=Succeeded;]done'
    }

    It 'leaves ordinary text untouched' {
        ConvertTo-AzdoSafeConsole 'Reading file src/Foo.cs (## of total)' | Should -Be 'Reading file src/Foo.cs (## of total)'
    }
}

Describe 'AI summary review ID handoff' {
    It 'passes the cross-job value as environment data instead of inline PowerShell source' {
        $pipelineContent | Should -Match ([regex]::Escape('$reviewId = $env:AI_SUMMARY_REVIEW_ID'))
        $pipelineContent | Should -Match ([regex]::Escape('AI_SUMMARY_REVIEW_ID: $(aiSummaryReviewId)'))
        $pipelineContent | Should -Not -Match ([regex]::Escape('$reviewId = "$(aiSummaryReviewId)"'))
    }

    It 'accepts only DEFERRED or a positive numeric review ID' {
        $pipelineContent | Should -Match ([regex]::Escape('$reviewId -ne ''DEFERRED'''))
        $pipelineContent | Should -Match ([regex]::Escape('$reviewId -notmatch ''^[1-9][0-9]*$'''))
    }
}

Describe 'Detected UI category handoff' {
    It 'passes detected categories as environment data instead of inline PowerShell source' {
        $pipelineContent | Should -Match ([regex]::Escape('$cats = $env:DETECTED_CATEGORIES'))
        $pipelineContent | Should -Match ([regex]::Escape('DETECTED_CATEGORIES: $(detectedCategories)'))
        $pipelineContent | Should -Not -Match ([regex]::Escape('$cats = "$(detectedCategories)"'))
    }

    It 'reads only the exact category output marker' {
        $expectedPattern = '^##vso\[task\.setvariable variable=UITestCategoryList;isOutput=true\](.*)$'
        ([regex]::Matches($content, [regex]::Escape($expectedPattern))).Count | Should -Be 2
        ([regex]::Matches($pipelineContent, [regex]::Escape($expectedPattern))).Count | Should -Be 1
        $content | Should -Not -Match ([regex]::Escape("-match 'UITestCategoryList;isOutput=true"))
        $pipelineContent | Should -Not -Match ([regex]::Escape("-match 'UITestCategoryList;isOutput=true"))
    }
}
