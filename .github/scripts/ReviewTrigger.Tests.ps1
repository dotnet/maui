#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $workflowPath = Join-Path $PSScriptRoot '..' 'workflows' 'review-trigger.yml' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $workflow = Get-Content -Raw -LiteralPath $workflowPath

    $matchJob = [regex]::Match(
        $workflow,
        '(?ms)^  match:\s.*?(?=^  trigger-review:)')
    if (-not $matchJob.Success) {
        throw 'Could not find the match job in review-trigger.yml.'
    }

    $triggerJobPattern = '(?ms)^  trigger-review:\s.*?(?=^  [A-Za-z0-9_-]+:[ \t]*\r?$|\z)'
    $triggerJob = [regex]::Match($workflow, $triggerJobPattern)
    if (-not $triggerJob.Success) {
        throw 'Could not find the trigger-review job in review-trigger.yml.'
    }

    $script:Workflow = $workflow
    $script:MatchJob = $matchJob.Value
    $script:TriggerJobPattern = $triggerJobPattern
    $script:TriggerJob = $triggerJob.Value
}

Describe '/review command matching' {
    It 'rejects the removed rerun subcommand before the generic review matcher' {
        $rerunGuard = $script:MatchJob.IndexOf(
            'if [[ "${TRIMMED_BODY}" =~ ^[[:space:]]*/review[[:space:]]+rerun([[:space:]]|$) ]]')
        $genericReviewMatcher = $script:MatchJob.IndexOf(
            'elif [[ "${COMMENT_BODY}" =~ ^[[:space:]]*/review([[:space:]]|$) ]]')
        # Locate the guard's closing `fi` tolerant of CRLF line endings and any leading
        # whitespace, so a non-functional reformat (Windows CRLF / re-indent) of the
        # workflow doesn't break this regression test.
        $rerunGuardMatch = [regex]::Match(
            $script:MatchJob.Substring($rerunGuard), '\r?\n[ \t]*fi(?=[\r\n]|$)')
        $rerunGuardEnd = if ($rerunGuardMatch.Success) { $rerunGuard + $rerunGuardMatch.Index } else { -1 }

        $rerunGuard | Should -BeGreaterOrEqual 0
        $rerunGuardEnd | Should -BeGreaterThan $rerunGuard
        $genericReviewMatcher | Should -BeGreaterThan $rerunGuard
        $rerunBlock = $script:MatchJob.Substring(
            $rerunGuard,
            $rerunGuardEnd - $rerunGuard)

        $rerunBlock | Should -Match 'echo "matched=false" >> "\$GITHUB_OUTPUT"'
        $rerunBlock | Should -Match 'exit 0'
        $rerunBlock | Should -Not -Match 'echo "matched=true"'
        $script:MatchJob | Should -Not -Match 'echo "command='
    }

    It 'does not contain the removed rerun job' {
        $script:Workflow | Should -Not -Match '(?m)^  mark-rerun-ready:'
        $script:Workflow | Should -Not -Match 'Resolve-RerunEligibility\.ps1'
    }

    It 'rejects a leading-newline "/review rerun" when the matcher is executed (behavioral)' {
        # sed strips leading whitespace PER LINE, so a leading blank line survives in
        # TRIMMED_BODY. If the guard anchored `^/review` (no leading-ws tolerance) while
        # the generic matcher allows `^[[:space:]]*/review`, a pasted `\n/review rerun`
        # would evade the guard and trigger a full review. Execute the real match script
        # to prove the guard rejects it while normal `/review` still matches.
        if (-not (Get-Command bash -ErrorAction SilentlyContinue)) {
            Set-ItResult -Skipped -Because 'bash is not available on this host'
            return
        }

        # Extract only the match step's `run:` block, then dedent the YAML block.
        $runMatch = [regex]::Match(
            $script:MatchJob,
            '(?ms)^        run: \|\r?\n(.*?)(?=^      - name:|\z)')
        $runMatch.Success | Should -BeTrue
        $rawScript = $runMatch.Groups[1].Value
        $scriptBody = (($rawScript -split "`n") | ForEach-Object { $_ -replace '^          ', '' }) -join "`n"

        $scriptFile = Join-Path ([System.IO.Path]::GetTempPath()) ("review-match-$([guid]::NewGuid().ToString('n')).sh")
        Set-Content -LiteralPath $scriptFile -Value $scriptBody -NoNewline

        $invoke = {
            param([string]$Body)
            $outFile = Join-Path ([System.IO.Path]::GetTempPath()) ("gho-$([guid]::NewGuid().ToString('n'))")
            try {
                $env:COMMENT_BODY = $Body
                $env:EVENT_NAME = 'issue_comment'
                $env:GITHUB_OUTPUT = $outFile
                & bash $scriptFile | Out-Null
                $line = Get-Content -LiteralPath $outFile -ErrorAction SilentlyContinue |
                    Where-Object { $_ -like 'matched=*' } | Select-Object -Last 1
                return ($line -replace '^matched=', '')
            } finally {
                Remove-Item -LiteralPath $outFile -ErrorAction SilentlyContinue
                Remove-Item Env:\COMMENT_BODY -ErrorAction SilentlyContinue
                Remove-Item Env:\EVENT_NAME -ErrorAction SilentlyContinue
                Remove-Item Env:\GITHUB_OUTPUT -ErrorAction SilentlyContinue
            }
        }

        try {
            (& $invoke '/review')            | Should -Be 'true'
            (& $invoke '/review rerun')      | Should -Be 'false'
            (& $invoke '   /review rerun')   | Should -Be 'false'
            (& $invoke "`n/review rerun")    | Should -Be 'false'  # the regression
            (& $invoke "`n/review")          | Should -Be 'true'
            (& $invoke '/review tests')      | Should -Be 'false'
        } finally {
            Remove-Item -LiteralPath $scriptFile -ErrorAction SilentlyContinue
        }
    }

    It 'continues to route workflow dispatch to a normal review' {
        $script:MatchJob | Should -Match 'github\.event_name.*workflow_dispatch'
        $script:MatchJob | Should -Match 'echo "matched=true" >> "\$GITHUB_OUTPUT"'
    }
}

Describe 'review trigger hardening' {
    It 'authorizes in the pre-flight job before provisioning trigger-review' {
        $script:MatchJob | Should -Match '(?m)^      proceed: \$\{\{ steps\.gate\.outputs\.proceed \}\}$'
        $script:MatchJob | Should -Match '(?m)^      - name: Check actor permission$'
        $script:MatchJob | Should -Match 'ACTOR: \$\{\{ github\.actor \}\}'
        $script:MatchJob | Should -Match 'REPO: \$\{\{ github\.repository \}\}'
        $script:MatchJob | Should -Match 'Permission lookup failed.*treating the caller as unauthorized'
        $script:MatchJob | Should -Match 'PERMISSION="none"'
        $script:TriggerJob | Should -Match "(?m)^    if: needs\.match\.outputs\.proceed == 'true'$"
        $script:TriggerJob | Should -Not -Match '(?m)^        id: auth$'
    }

    It 'stops trigger-review extraction at the next top-level job' {
        $workflowWithFutureJob = $script:Workflow.TrimEnd() + @'

  future-job:
    runs-on: ubuntu-latest
'@
        $triggerJob = [regex]::Match($workflowWithFutureJob, $script:TriggerJobPattern)

        $triggerJob.Success | Should -BeTrue
        $triggerJob.Value | Should -Not -Match '(?m)^  future-job:'
    }

    It 'does not interpolate actor or repository directly into shell commands' {
        $script:Workflow | Should -Not -Match 'gh api repos/\$\{\{ github\.repository \}\}'
        $script:Workflow | Should -Not -Match 'collaborators/\$\{\{ github\.actor \}\}'
        $script:Workflow | Should -Not -Match 'User \$\{\{ github\.actor \}\}'
    }

    It 'keeps OIDC and AzDO tokens shell-local in one trigger step' {
        $script:TriggerJob | Should -Match '(?ms)- name: Trigger maui-copilot pipeline.*# 1\. Get GitHub OIDC token.*# 2\. Exchange OIDC token for AzDO access token.*# 3\. Trigger the pipeline'
        $script:TriggerJob | Should -Not -Match 'oidc_token=.*GITHUB_OUTPUT'
        $script:TriggerJob | Should -Not -Match 'azdo_token=.*GITHUB_OUTPUT'
        $script:TriggerJob | Should -Not -Match 'steps\.(oidc|token)\.outputs'
        $script:TriggerJob | Should -Match 'unset OIDC_TOKEN'
        $script:TriggerJob | Should -Match 'unset AZDO_TOKEN'
    }

    It 'only hides commands after the pre-flight authorization gate succeeded' {
        $script:TriggerJob | Should -Not -Match 'steps\.auth'
        $script:TriggerJob | Should -Match "(?m)^        if: \$\{\{ !cancelled\(\) && github\.event_name == 'issue_comment' \}\}$"
    }
}
