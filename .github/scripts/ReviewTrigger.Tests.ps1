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
    $script:TriggerReviewJob = $workflow.Substring($matchJob.Index + $matchJob.Length)
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

Describe '/review trigger setup' {
    It 'downloads only the trusted label helper instead of cloning the repository' {
        $script:TriggerReviewJob | Should -Not -Match 'actions/checkout@'
        $script:TriggerReviewJob | Should -Match 'name: Download trusted label helper'
        $script:TriggerReviewJob | Should -Match ([regex]::Escape(
            'contents/.github/scripts/shared/Update-AgentLabels.ps1?ref=${GITHUB_SHA}'))
        $script:TriggerReviewJob | Should -Match ([regex]::Escape(
            'contents/.github/scripts/shared/Invoke-GhCommandWithRetry.ps1?ref=${GITHUB_SHA}'))
        $script:TriggerReviewJob | Should -Match 'timeout-minutes: 2'
        $script:TriggerReviewJob | Should -Match 'for attempt in 1 2 3'
    }

    It 'sources the downloaded helper for both lock operations' {
        $script:TriggerReviewJob | Should -Match 'id: label_helper'
        $script:TriggerReviewJob | Should -Match 'path=\$\{HELPER_PATH\}'

        $helperSources = [regex]::Matches(
            $script:TriggerReviewJob,
            '(?m)^\s*LABEL_HELPER_PATH: \$\{\{ steps\.label_helper\.outputs\.path \}\}\s*$')
        $dotSources = [regex]::Matches(
            $script:TriggerReviewJob,
            '(?m)^\s*\. \$env:LABEL_HELPER_PATH\s*$')

        $helperSources.Count | Should -Be 2
        $dotSources.Count | Should -Be 2
    }
}

Describe 'review trigger hardening' {
    It 'authorizes in the pre-flight job before provisioning trigger-review' {
        $script:MatchJob | Should -Match '(?m)^      proceed: \$\{\{ steps\.gate\.outputs\.proceed \}\}$'
        $script:MatchJob | Should -Match '(?m)^      - name: Check actor permission$'
        $script:MatchJob | Should -Match 'ACTOR: \$\{\{ github\.actor \}\}'
        $script:MatchJob | Should -Match 'COMMENT_ID: \$\{\{ github\.event\.comment\.id \|\| inputs\.source_comment_id \}\}'
        $script:MatchJob | Should -Match 'COMMENT_NODE_ID: \$\{\{ github\.event\.comment\.node_id \|\| inputs\.source_comment_node_id \}\}'
        $script:MatchJob | Should -Match 'REPO: \$\{\{ github\.repository \}\}'
        $script:MatchJob | Should -Match 'Permission lookup failed.*after 4 attempts.*scheduled recovery'
        $script:MatchJob | Should -Match '(?m)^          for attempt in 1 2 3 4; do$'
        $script:MatchJob | Should -Match 'HTTP 404\\b'
        $script:MatchJob | Should -Not -Match 'treating the caller as unauthorized'
        $script:TriggerJob | Should -Match "(?m)^    if: needs\.match\.outputs\.proceed == 'true'$"
        $script:TriggerJob | Should -Not -Match '(?m)^        id: auth$'
    }

    It 'skips delayed webhook deliveries already handled by recovery' {
        $script:MatchJob | Should -Match 'COMMENT_ID: \$\{\{ github\.event\.comment\.id \|\| inputs\.source_comment_id \}\}'
        $script:MatchJob | Should -Match 'COMMENT_NODE_ID: \$\{\{ github\.event\.comment\.node_id \|\| inputs\.source_comment_node_id \}\}'
        $script:MatchJob | Should -Match 'PR_NUMBER: \$\{\{ github\.event\.issue\.number \|\| inputs\.pr_number \}\}'
        $script:MatchJob | Should -Match 'Recovery source comment identity or command validation failed; skipping dispatch'
        $script:MatchJob | Should -Match 'issues/comments/\$\{COMMENT_ID\}/reactions\?per_page=100'
        $script:MatchJob | Should -Match 'gh api --paginate --slurp'
        $script:MatchJob | Should -Match "jq -e '\.\[\]\[\] \| select"
        $script:MatchJob | Should -Match '\.content == "rocket" and \.user\.login == "github-actions\[bot\]"'
        $script:MatchJob | Should -Match 'already recovered; skipping delayed duplicate delivery'
        $script:MatchJob | Should -Match 'IssueComment\{isMinimized\}'
        $script:MatchJob | Should -Match 'already minimized by recovery; skipping delayed duplicate delivery'
        $script:MatchJob | Should -Match '(?m)^          for attempt in 1 2 3; do$'
        $script:MatchJob | Should -Match 'Recovery source comment is still unacknowledged; revalidating'
        $script:MatchJob | Should -Not -Match 'workflow_dispatch — skipping collaborator check'
    }

    It 'revalidates the source commenter current permission before recovery proceeds' {
        $sourceReadIndex = $script:MatchJob.IndexOf('if ! SOURCE_COMMENT=')
        $permissionIndex = $script:MatchJob.IndexOf('PERMISSION=""', $sourceReadIndex)

        $sourceReadIndex | Should -BeGreaterThan -1
        $permissionIndex | Should -BeGreaterThan $sourceReadIndex
        $recoveryValidation = $script:MatchJob.Substring(
            $sourceReadIndex,
            $permissionIndex - $sourceReadIndex)

        $recoveryValidation | Should -Match ([regex]::Escape(
            'SOURCE_ACTOR=$(echo "${SOURCE_COMMENT}" | jq -r ''.user.login // ""'')'))
        $recoveryValidation | Should -Match ([regex]::Escape(
            'ACTOR="${SOURCE_ACTOR}"'))
        $recoveryValidation | Should -Not -Match 'echo "proceed=true"'

        $permissionBlock = $script:MatchJob.Substring($permissionIndex)
        $permissionBlock | Should -Match ([regex]::Escape(
            'collaborators/${ACTOR}/permission'))
        $permissionBlock | Should -Match 'admin\|maintain\|write'
        $permissionBlock | Should -Match 'echo "proceed=true"'
    }

    It 'rechecks recovery acknowledgement after job concurrency and before the review lock' {
        $script:TriggerJob | Should -Match 'COMMENT_ID: \$\{\{ inputs\.source_comment_id \}\}'
        $script:TriggerJob | Should -Match 'COMMENT_NODE_ID: \$\{\{ inputs\.source_comment_node_id \}\}'
        $script:TriggerJob | Should -Match 'any\(\.\[\]\[\]; \.content == "rocket"'
        $script:TriggerJob | Should -Match 'already acknowledged by an earlier serialized run'

        $dedupeIndex = $script:TriggerJob.IndexOf('# The job concurrency group serializes recovery dispatches')
        $labelReadIndex = $script:TriggerJob.IndexOf('$labels = Get-AgentLabels')
        $lockWriteIndex = $script:TriggerJob.IndexOf('$locked = Set-AgentReviewInProgress')

        $dedupeIndex | Should -BeGreaterOrEqual 0
        $labelReadIndex | Should -BeGreaterThan $dedupeIndex
        $lockWriteIndex | Should -BeGreaterThan $dedupeIndex
        $script:TriggerJob | Should -Match "(?m)^        if: steps\.review_lock\.outputs\.locked != 'true'$"
    }

    It 'keeps a transient recovery re-check failure unacknowledged for retry' {
        $script:TriggerJob | Should -Match ([regex]::Escape('"state=recheck-failed" >> $env:GITHUB_OUTPUT'))
        $script:TriggerJob | Should -Match 'leaving it unacknowledged so recovery retries it'
        $script:TriggerJob | Should -Match ([regex]::Escape("steps.review_lock.outputs.state != 'recheck-failed'"))

        $recheckFailure = $script:TriggerJob.IndexOf('"state=recheck-failed" >> $env:GITHUB_OUTPUT')
        $acknowledgeStep = $script:TriggerJob.IndexOf('- name: Acknowledge and hide the /review command comment')
        $recheckFailure | Should -BeGreaterThan -1
        $acknowledgeStep | Should -BeGreaterThan $recheckFailure
    }

    It 'distinguishes handled lock outcomes from a transient re-check failure' {
        $script:TriggerJob | Should -Match ([regex]::Escape('"state=already-acknowledged" >> $env:GITHUB_OUTPUT'))
        $script:TriggerJob | Should -Match ([regex]::Escape('"state=already-running" >> $env:GITHUB_OUTPUT'))
        $script:TriggerJob | Should -Match ([regex]::Escape('"state=acquired" >> $env:GITHUB_OUTPUT'))
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
        $script:TriggerJob | Should -Match '-H "Authorization: Bearer \$\{GH_TOKEN\}"'
        $script:TriggerJob | Should -Match '-H "Authorization: Bearer \$\{AZDO_TOKEN\}"'
        $script:TriggerJob | Should -Not -Match 'Authorization: \*{6}'
        $script:TriggerJob | Should -Match 'unset OIDC_TOKEN'
        $script:TriggerJob | Should -Match 'unset AZDO_TOKEN'
    }

    It 'acknowledges only commands that were handled or failed deterministically' {
        $script:TriggerJob | Should -Not -Match 'steps\.auth'
        $script:Workflow | Should -Match '(?m)^      source_comment_id:$'
        $script:Workflow | Should -Match '(?m)^      source_comment_node_id:$'
        $script:TriggerJob | Should -Match "github\.event_name == 'issue_comment'"
        $script:TriggerJob | Should -Match "inputs\.source_comment_id != ''"
        $script:TriggerJob | Should -Match "inputs\.source_comment_node_id != ''"
        $script:TriggerJob | Should -Match '(?m)^      - name: Acknowledge and hide the /review command comment$'
        $script:TriggerJob | Should -Match 'issues/comments/\$\{COMMENT_ID\}/reactions'
        $script:TriggerJob | Should -Match "-f content='rocket'"
        $script:TriggerJob | Should -Match 'COMMENT_ID: \$\{\{ github\.event\.comment\.id \|\| inputs\.source_comment_id \}\}'
        $script:TriggerJob | Should -Match 'COMMENT_NODE_ID: \$\{\{ github\.event\.comment\.node_id \|\| inputs\.source_comment_node_id \}\}'
        $script:TriggerJob | Should -Match 'Recovery source comment identity or command validation failed'
        $script:TriggerJob | Should -Match 'is_recoverable_review_command'
        $script:TriggerJob | Should -Match 'jq -Rrs'
        $script:TriggerJob | Should -Match 'ascii_downcase'
        $script:TriggerJob | Should -Match "steps\.trigger_azdo\.outcome == 'success'"
        $script:TriggerJob | Should -Match "steps\.review_lock\.outputs\.locked == 'true'"
        $script:TriggerJob | Should -Match "steps\.trigger_azdo\.outputs\.fail_reason == 'branch-missing'"
        $script:TriggerJob | Should -Match 'left unacknowledged so scheduled recovery can retry'
    }

    It 'retries critical GitHub reads and distinguishes a missing branch from API failure' {
        $script:TriggerJob | Should -Match 'GitHub API did not return valid metadata for PR'
        $script:TriggerJob | Should -Match 'Could not read PR #\$\{PR_NUMBER\} labels after 4 attempts'
        $script:TriggerJob | Should -Match 'BRANCH_HTTP="000"'
        $script:TriggerJob | Should -Match 'BRANCH_HTTP.*= "404"'
        $script:TriggerJob | Should -Match 'fail_reason=branch-missing'
        $script:TriggerJob | Should -Match 'Could not validate pipeline branch.*after 4 attempts'
        $script:TriggerJob | Should -Match 'fail_reason=api-error'
        $script:TriggerJob | Should -Match 'name: Report trigger setup failure to the PR'
    }

    It 'validates recovery commands with whole-body trim and case-insensitive matching' -TestCases @(
        @{ Body = '/review'; Expected = 'true' }
        @{ Body = '/REVIEW'; Expected = 'true' }
        @{ Body = "`n/review"; Expected = 'true' }
        @{ Body = "  `n/ReViEw -p ios`n"; Expected = 'true' }
        @{ Body = '/review tests'; Expected = 'false' }
        @{ Body = "`n/REVIEW RERUN"; Expected = 'false' }
        @{ Body = 'please /review'; Expected = 'false' }
    ) {
        param($Body, $Expected)

        if (-not (Get-Command bash -ErrorAction SilentlyContinue) -or
            -not (Get-Command jq -ErrorAction SilentlyContinue)) {
            Set-ItResult -Skipped -Because 'bash and jq are required'
            return
        }

        $functionMatch = [regex]::Match(
            $script:TriggerJob,
            '(?ms)^            is_recoverable_review_command\(\) \{\r?\n.*?^            \}')
        $functionMatch.Success | Should -BeTrue
        $functionBody = (($functionMatch.Value -split "`n") |
            ForEach-Object { $_ -replace '^            ', '' }) -join "`n"
        $validator = $functionBody + "`n" + @'
if is_recoverable_review_command "$1"; then
  printf 'true'
else
  printf 'false'
fi
'@

        (& bash -c $validator 'review-validator' $Body | Out-String).Trim() |
            Should -BeExactly $Expected
    }

    It 'posts a visible start notice only after AzDO returns a valid build id' {
        $script:TriggerJob | Should -Match 'if ! \[\[ "\$\{RUN_ID\}" =~ \^\[1-9\]\[0-9\]\*\$ \]\]'
        $script:TriggerJob | Should -Match 'echo "run_id=\$\{RUN_ID\}" >> "\$GITHUB_OUTPUT"'
        $script:TriggerJob | Should -Match '(?m)^      - name: Report /review start to the PR$'
        $script:TriggerJob | Should -Match "(?m)^        if: steps\.review_lock\.outputs\.locked == 'false' && steps\.trigger_azdo\.outcome == 'success'$"
        $script:TriggerJob | Should -Match 'RUN_ID: \$\{\{ steps\.trigger_azdo\.outputs\.run_id \}\}'
        $script:TriggerJob | Should -Match '<!-- copilot-review-started:\$\{RUN_ID\} -->'
        $script:TriggerJob | Should -Match 'AzDO build \*\*\$\{RUN_ID\}\*\*'
        $script:TriggerJob | Should -Match 's/agent-review-in-progress'
        $script:TriggerJob | Should -Match 'outcome labels are posted only after Gate, expert review, and Deep UI tests finish'

        $triggerIndex = $script:TriggerJob.IndexOf('- name: Trigger maui-copilot pipeline')
        $noticeIndex = $script:TriggerJob.IndexOf('- name: Report /review start to the PR')
        $hideIndex = $script:TriggerJob.IndexOf('- name: Acknowledge and hide the /review command comment')
        $noticeIndex | Should -BeGreaterThan $triggerIndex
        $hideIndex | Should -BeGreaterThan $noticeIndex
    }
}
