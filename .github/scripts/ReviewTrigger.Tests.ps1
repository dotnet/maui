#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $workflowPath = Join-Path $PSScriptRoot '..' 'workflows' 'review-trigger.yml' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $workflow = Get-Content -Raw -LiteralPath $workflowPath

    $matchJob = [regex]::Match(
        $workflow,
        '(?ms)^  match:\s.*?(?=^  mark-rerun-ready:)')
    if (-not $matchJob.Success) {
        throw 'Could not find the match job in review-trigger.yml.'
    }

    $script:MatchJob = $matchJob.Value
}

Describe '/review rerun comment matching' {
    It 'rejects rerun comments before the generic review command matcher' {
        $rerunGuard = $script:MatchJob.IndexOf(
            'if [[ "${TRIMMED_BODY}" =~ ^/review[[:space:]]+rerun([[:space:]]|$) ]]')
        $genericReviewMatcher = $script:MatchJob.IndexOf(
            'elif [[ "${COMMENT_BODY}" =~ ^[[:space:]]*/review([[:space:]]|$) ]]')
        $rerunGuardEnd = $script:MatchJob.IndexOf("`n          fi", $rerunGuard)

        $rerunGuard | Should -BeGreaterOrEqual 0
        $rerunGuardEnd | Should -BeGreaterThan $rerunGuard
        $genericReviewMatcher | Should -BeGreaterThan $rerunGuard
        $rerunBlock = $script:MatchJob.Substring(
            $rerunGuard,
            $rerunGuardEnd - $rerunGuard)

        $rerunBlock | Should -Match 'echo "matched=false" >> "\$GITHUB_OUTPUT"'
        $rerunBlock | Should -Match 'echo "command=none" >> "\$GITHUB_OUTPUT"'
        $rerunBlock | Should -Match 'exit 0'
        $rerunBlock | Should -Not -Match 'echo "matched=true"'
        $rerunBlock | Should -Not -Match 'echo "command=(review|rerun)"'
        $script:MatchJob | Should -Not -Match 'echo "command=rerun"'
    }

    It 'continues to route workflow dispatch to a normal review' {
        $script:MatchJob | Should -Match 'github\.event_name.*workflow_dispatch'
        $script:MatchJob | Should -Match 'echo "command=review" >> "\$GITHUB_OUTPUT"'
    }
}
