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

    $script:Workflow = $workflow
    $script:MatchJob = $matchJob.Value
    $script:TriggerReviewJob = $workflow.Substring($matchJob.Index + $matchJob.Length)
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

        # Extract the match step's `run:` block and neutralize the only ${{ }} expression
        # (a comment event is not workflow_dispatch), then dedent the YAML block.
        $runMatch = [regex]::Match($script:MatchJob, '(?ms)^        run: \|\r?\n(.*)')
        $runMatch.Success | Should -BeTrue
        $rawScript = $runMatch.Groups[1].Value -replace '\$\{\{\s*github\.event_name\s*\}\}', 'issue_comment'
        $scriptBody = (($rawScript -split "`n") | ForEach-Object { $_ -replace '^          ', '' }) -join "`n"

        $scriptFile = Join-Path ([System.IO.Path]::GetTempPath()) ("review-match-$([guid]::NewGuid().ToString('n')).sh")
        Set-Content -LiteralPath $scriptFile -Value $scriptBody -NoNewline

        $invoke = {
            param([string]$Body)
            $outFile = Join-Path ([System.IO.Path]::GetTempPath()) ("gho-$([guid]::NewGuid().ToString('n'))")
            try {
                $env:COMMENT_BODY = $Body
                $env:GITHUB_OUTPUT = $outFile
                & bash $scriptFile | Out-Null
                $line = Get-Content -LiteralPath $outFile -ErrorAction SilentlyContinue |
                    Where-Object { $_ -like 'matched=*' } | Select-Object -Last 1
                return ($line -replace '^matched=', '')
            } finally {
                Remove-Item -LiteralPath $outFile -ErrorAction SilentlyContinue
                Remove-Item Env:\COMMENT_BODY -ErrorAction SilentlyContinue
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
