#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for pure-function helpers in post-ai-summary-comment.ps1.

.EXAMPLE
    Invoke-Pester ./Post-AISummaryComment.Tests.ps1
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'post-ai-summary-comment.ps1'
    . (Join-Path $PSScriptRoot 'shared/Escape-Html.ps1')

    $script:ScriptSource = Get-Content -Raw -LiteralPath $scriptPath
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'Test-PhaseContentIsNoOp',
        'Get-GateStatus',
        'Get-AIReviewEvent',
        'Test-RunValidationFailed',
        'Test-WinnerRequiresPRChanges',
        'Get-AIReviewEventForRun',
        'Test-ExpertReviewIsBlocking',
        'Test-DeepUITestsHadNoSignal',
        'Add-MissingUITestResultsNote',
        'New-FutureActionSection',
        'New-MissingAgentPhaseContent',
        'Get-AuthoritativeGateContent',
        'Limit-MarkdownContent',
        'Get-FirstPhaseContent'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Immutable review snapshot posting' {
    It 'binds formal reviews to the reviewed commit and downgrades stale runs' {
        $script:ScriptSource | Should -Match ([regex]::Escape("[string]`$ReviewedCommit = ''"))
        $script:ScriptSource | Should -Match ([regex]::Escape("`$payload['commit_id'] = `$CommitSha"))
        $script:ScriptSource | Should -Match '(?s)currentHeadSha.*ReviewedCommit.*reviewEvent = ''COMMENT'''
        $script:ScriptSource | Should -Match 'PR advanced to.*while it was running'
    }
}

Describe 'Get-FirstPhaseContent' {
    BeforeEach {
        $script:phaseRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Force -Path (Join-Path $script:phaseRoot 'expert-pr-eval') | Out-Null
        New-Item -ItemType Directory -Force -Path (Join-Path $script:phaseRoot 'pre-flight') | Out-Null
    }

    It 'prefers the current expert-review artifact over the legacy path' {
        'current expert review' | Set-Content (Join-Path $script:phaseRoot 'expert-pr-eval/content.md') -Encoding UTF8
        'legacy code review' | Set-Content (Join-Path $script:phaseRoot 'pre-flight/code-review.md') -Encoding UTF8

        $result = Get-FirstPhaseContent `
            -Root $script:phaseRoot `
            -RelativePaths @('expert-pr-eval/content.md', 'pre-flight/code-review.md')

        $result.Path | Should -Be (Join-Path $script:phaseRoot 'expert-pr-eval/content.md')
        $result.Content.Trim() | Should -BeExactly 'current expert review'
    }

    It 'falls back to the legacy artifact when the current file is empty' {
        '' | Set-Content (Join-Path $script:phaseRoot 'expert-pr-eval/content.md') -Encoding UTF8
        'legacy code review' | Set-Content (Join-Path $script:phaseRoot 'pre-flight/code-review.md') -Encoding UTF8

        $result = Get-FirstPhaseContent `
            -Root $script:phaseRoot `
            -RelativePaths @('expert-pr-eval/content.md', 'pre-flight/code-review.md')

        $result.Path | Should -Be (Join-Path $script:phaseRoot 'pre-flight/code-review.md')
        $result.Content.Trim() | Should -BeExactly 'legacy code review'
    }

    It 'returns null when no candidate contains usable content' {
        Get-FirstPhaseContent `
            -Root $script:phaseRoot `
            -RelativePaths @('expert-pr-eval/content.md', 'pre-flight/code-review.md') |
            Should -BeNullOrEmpty
    }
}

Describe 'Test-PhaseContentIsNoOp' {
    It 'suppresses the no-UI-tests placeholder and the full-matrix note' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content 'No UI test categories needed for this PR (no UI-relevant changes).' |
            Should -BeTrue

        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content 'Full UI test matrix will run (no specific categories detected from PR changes).' |
            Should -BeTrue
    }

    It 'keeps UI test content when specific categories are present' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content '**Detected UI test categories:** `Button,Entry`' |
            Should -BeFalse
    }

    It 'suppresses regression placeholders when there are no implementation files or risks' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content '● No implementation files modified — skipping regression cross-reference.' |
            Should -BeTrue

        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content "## 🔍 Regression Cross-Reference`n`n● No regression risks detected. No labeled bug-fix PRs in the last 6 months touched the modified files." |
            Should -BeTrue

        # Back-compat: the legacy 🟢 glyph is still recognized as a no-op.
        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content '🟢 No implementation files modified — skipping regression cross-reference.' |
            Should -BeTrue
    }

    It 'keeps actionable regression content' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content "## 🔍 Regression Cross-Reference`n`n⚠ **Overlaps with prior bug-fix PRs** — same files modified, but no exact line revert detected." |
            Should -BeFalse

        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content "## 🔍 Regression Cross-Reference`n`n✗ **Revert risks detected** — this PR removes 2 line(s) previously added by labeled bug-fix PRs." |
            Should -BeFalse

        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content '⚠️ Regression cross-reference failed: gh api failed' |
            Should -BeFalse
    }

    It 'suppresses the pr-finalize section when the title/description are keep-as-is' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content '✅ Current title and description accurately reflect the change — recommend keeping as-is.' |
            Should -BeTrue

        # tolerant of an optional Assessment prefix and trailing optional notes
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content "**Assessment:** ✅ Current title and description accurately reflect the change - recommend keeping as-is.`n`n- Optional: mention the synthetic producer." |
            Should -BeTrue
    }

    It 'keeps the pr-finalize section when an update is recommended' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content "**Assessment:** ✏️ Recommend updating — the description is strong but the winning fix adds behavior.`n`n**Recommended title**" |
            Should -BeFalse

        # an "updating" verdict that merely says the description is accurate must NOT be suppressed
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content '**Assessment:** ✏️ Recommend updating — the current title and description accurately describe the PR shape, but the winning fix differs.' |
            Should -BeFalse
    }
}

Describe 'Add-MissingUITestResultsNote' {
    It 'annotates a bare "detected categories" placeholder with no results' {
        $result = Add-MissingUITestResultsNote -Content '**Detected UI test categories:** `Picker,ViewBaseTests`'
        $result | Should -Match 'No UI test results were produced'
        $result | Should -Match 'Fix the build/gate issues and push again'
        $result | Should -Not -Match '/review rerun'
        # Original content is preserved.
        $result | Should -Match 'Detected UI test categories'
    }

    It 'uses the trusted non-failed gate result for infrastructure guidance' -TestCases @(
        @{ GateResult = 'PASSED' }
        @{ GateResult = 'SKIPPED' }
        @{ GateResult = 'INCONCLUSIVE' }
    ) {
        param($GateResult)

        $result = Add-MissingUITestResultsNote `
            -Content '**Detected UI test categories:** `Picker`' `
            -TrustedGateResult $GateResult

        $result | Should -Match 'interrupted on \*\*infrastructure\*\*'
        $result | Should -Not -Match 'Fix the build/gate issues'
    }

    It 'keeps failed-gate guidance when the trusted result is FAILED' {
        $result = Add-MissingUITestResultsNote `
            -Content '**Detected UI test categories:** `Picker`' `
            -TrustedGateResult 'FAILED'

        $result | Should -Match 'Fix the build/gate issues'
        $result | Should -Not -Match 'PR build itself was'
    }

    It 'uses neutral infrastructure guidance when the trusted gate timed out' {
        $result = Add-MissingUITestResultsNote `
            -Content '**Detected UI test categories:** `Picker`' `
            -TrustedGateResult 'TIMEDOUT'

        $result | Should -Match 'trusted gate timed'
        $result | Should -Match '\*\*infrastructure\*\*'
        $result | Should -Not -Match 'Fix the build/gate issues'
        $result | Should -Not -Match 'PR build itself was\s+fine'
    }

    It 'does not infer gate state from UI-phase text' {
        $content = "**Detected UI test categories:** ``Picker```nGate Result: PASSED"
        $result = Add-MissingUITestResultsNote -Content $content

        $result | Should -Match 'Fix the build/gate issues'
        $result | Should -Not -Match 'PR build itself was'
    }

    It 'does not advertise the removed rerun command' {
        $script:ScriptSource | Should -Not -Match '/review rerun'
    }

    It 'does NOT annotate when deep results are present' {
        $content = "**Detected UI test categories:** ``Picker``" + [Environment]::NewLine +
            '✅ **Deep UI tests** — 12 passed, 0 failed across 1 category on platform-pool agent.'
        Add-MissingUITestResultsNote -Content $content | Should -Be $content
    }

    It 'does NOT annotate when an execution-results count is present' {
        $content = "**Detected UI test categories:** ``Picker``" + [Environment]::NewLine + '5 passed, 1 failed'
        Add-MissingUITestResultsNote -Content $content | Should -Be $content
    }

    It 'does NOT annotate the no-categories / full-matrix placeholders' {
        $noCats = 'No UI test categories needed for this PR (no UI-relevant changes).'
        Add-MissingUITestResultsNote -Content $noCats | Should -Be $noCats

        $fullMatrix = 'Full UI test matrix will run (no specific categories detected from PR changes).'
        Add-MissingUITestResultsNote -Content $fullMatrix | Should -Be $fullMatrix
    }

    It 'is a no-op for empty content' {
        Add-MissingUITestResultsNote -Content '' | Should -Be ''
    }
}

Describe 'New-MissingAgentPhaseContent' {
    It 'renders an explicit incomplete placeholder for every required expert phase' {
        foreach ($phase in @('pre-flight', 'code-review', 'try-fix', 'report')) {
            $content = New-MissingAgentPhaseContent -PhaseKey $phase
            $content | Should -Match 'did not produce output'
            $content | Should -Match 'review is \*\*incomplete\*\*'
            $content | Should -Match '/review'
        }
    }

    It 'names the missing Try-Fix phase explicitly' {
        New-MissingAgentPhaseContent -PhaseKey 'try-fix' |
            Should -Match '\*\*Try-Fix did not produce output'
    }
}

Describe 'Summary phase labels' {
    It 'keeps the canonical Try-Fix and PR Finalize names visible in the review body' {
        $script:ScriptSource | Should -Match 'Title = "🛠️ Try-Fix — Analysis & Comparison"'
        $script:ScriptSource | Should -Match 'Title = "📝 PR Finalize — Recommended Title & Description"'
        $script:ScriptSource | Should -Not -Match 'Title = "🛠️ Fix — Analysis & Comparison"'
        $script:ScriptSource | Should -Not -Match 'Title = "📝 Recommended PR Title & Description"'
    }
}

Describe 'Limit-MarkdownContent' {
    It 'keeps short content unchanged' {
        Limit-MarkdownContent -Content 'short content' -MaxChars 512 -SectionName 'test' |
            Should -Be 'short content'
    }

    It 'shortens oversized content, balances markdown blocks, and stays within budget' {
        $content = "<details>`n```text`n" + ('x' * 2000)
        $result = Limit-MarkdownContent -Content $content -MaxChars 700 -SectionName 'UI Tests'

        $result.Length | Should -BeLessOrEqual 700
        $result | Should -Match 'was shortened to keep every required review section visible'
        ([regex]::Matches($result, '(?m)^```')).Count % 2 | Should -Be 0
        ([regex]::Matches($result, '(?i)<details(?:\s|>)')).Count |
            Should -Be ([regex]::Matches($result, '(?i)</details>')).Count
    }
}

Describe 'Get-GateStatus' {
    It 'maps a passed gate to Passed' {
        Get-GateStatus -GateContent '### Gate Result: ✅ PASSED' | Should -Be 'Passed'
    }

    It 'maps a skipped gate to No Tests' {
        Get-GateStatus -GateContent "### Gate Result: ⚠️ SKIPPED`n`nNo tests were detected in this PR." |
            Should -Be 'No Tests'
    }

    It 'maps a clean failed gate to Failed' {
        Get-GateStatus -GateContent '### Gate Result: ❌ FAILED' | Should -Be 'Failed'
    }

    It 'maps a mixed/inconclusive failed gate to Partial' {
        Get-GateStatus -GateContent "### Gate Result: ❌ FAILED`n`n🩺 **Regression in another test** — at least one test goes FAIL→PASS, but another fails both." |
            Should -Be 'Partial'

        Get-GateStatus -GateContent "### Gate Result: ❌ FAILED`n`n🩺 **Test does not reproduce the bug** — ran the same in both states (PASS without fix, PASS with fix)." |
            Should -Be 'Partial'
    }

    It 'maps an INCONCLUSIVE gate (build/env error) to Inconclusive' {
        Get-GateStatus -GateContent "### Gate Result: ⚠️ INCONCLUSIVE`n`nTests could not be built/run (build or env error)." |
            Should -Be 'Inconclusive'
    }

    It 'maps a TIMEDOUT gate (synthesized section) to Timed Out' {
        Get-GateStatus -GateContent "### Gate Result: TIMEDOUT — test verification did not finish`n`nThe automated test-verification gate did not complete on this run." |
            Should -Be 'Timed Out'
    }

    It 'maps prose describing a timed-out gate to Timed Out' {
        Get-GateStatus -GateContent 'The gate timed out before it could finish.' | Should -Be 'Timed Out'
    }

    It 'returns Unknown for empty gate content' {
        Get-GateStatus -GateContent '' | Should -Be 'Unknown'
    }
}

Describe 'Get-AuthoritativeGateContent' {
    It 'overrides partial FAILED content when the trusted Gate verdict is TIMEDOUT' {
        $partial = @'
### Gate Result: ❌ FAILED

The fix does not pass the tests.
'@

        $result = Get-AuthoritativeGateContent -GateContent $partial -TrustedGateResult 'TIMEDOUT'

        $result | Should -Match 'Gate Result: TIMEDOUT'
        $result | Should -Match 'test verification did not finish'
        $result | Should -Not -Match 'Gate Result: ❌ FAILED'
        $result | Should -Not -Match 'fix does not pass'
    }

    It 'preserves completed Gate content for non-timeout verdicts' {
        $content = '### Gate Result: ✅ PASSED'

        Get-AuthoritativeGateContent -GateContent $content -TrustedGateResult 'PASSED' |
            Should -Be $content
    }
}

Describe 'Get-AIReviewEvent' {
    It 'maps an exact approve recommendation to APPROVE' {
        Get-AIReviewEvent -ReportContent "## ✅ Final Recommendation: APPROVE`n`nLooks good." |
            Should -Be 'APPROVE'
    }

    It 'maps an exact request-changes recommendation to REQUEST_CHANGES' {
        Get-AIReviewEvent -ReportContent "## ⚠️ Final Recommendation: REQUEST CHANGES`n`nNeeds the try-fix candidate." |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'falls back to COMMENT when the recommendation is missing or ambiguous' {
        Get-AIReviewEvent -ReportContent '' | Should -Be 'COMMENT'
        Get-AIReviewEvent -ReportContent 'Recommendation: APPROVE after manual review' | Should -Be 'COMMENT'
    }
}

Describe 'Get-AIReviewEventForRun' {
    BeforeEach {
        $script:testDir = Join-Path ([System.IO.Path]::GetTempPath()) "ai-summary-tests-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $script:testDir -Force | Out-Null
    }

    AfterEach {
        Remove-Item -LiteralPath $script:testDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'requests changes when a non-PR try-fix candidate wins and the report is otherwise comment-only' {
        @{
            winner = 'try-fix-1'
            isPRFix = $false
            candidateDiff = 'diff --git a/file.cs b/file.cs'
            summary = 'Candidate fixes the issue more directly.'
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Report still in progress.' -PRAgentDir $script:testDir -TrustedGateResult 'SKIPPED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'requests changes when pr-plus-reviewer wins and the report is otherwise comment-only' {
        @{
            winner = 'pr-plus-reviewer'
            isPRFix = $true
            candidateDiff = ''
            summary = 'Expert feedback improves the submitted PR.'
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Report omitted its canonical recommendation.' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'vetoes an exact approve recommendation when a try-fix candidate wins' {
        @{
            winner = 'try-fix-1'
            isPRFix = $true
            candidateDiff = 'diff --git a/file.cs b/file.cs'
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'SKIPPED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'keeps an exact approve recommendation when the raw PR wins' {
        @{
            winner = 'pr'
            isPRFix = $true
            candidateDiff = ''
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'APPROVE'
    }

    It 'vetoes APPROVE to REQUEST_CHANGES when the trusted gate verdict is FAILED' {
        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'FAILED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'trusts the supplied FAILED verdict even when a forged gate-result.txt says PASSED' {
        # Security: the veto must IGNORE the agent-writable worktree file. A prompt-injected
        # review agent that overwrites gate-result.txt with PASSED after a real FAILED gate must
        # not be able to bypass the veto — the trusted pipeline verdict wins.
        $gateDir = Join-Path $script:testDir 'gate'
        New-Item -ItemType Directory -Path $gateDir -Force | Out-Null
        'PASSED' | Set-Content (Join-Path $gateDir 'gate-result.txt') -Encoding UTF8
        'Gate Result: ✅ PASSED' | Set-Content (Join-Path $gateDir 'content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'FAILED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'keeps APPROVE when the trusted verdict is PASSED even if a forged gate-result.txt says FAILED' {
        $gateDir = Join-Path $script:testDir 'gate'
        New-Item -ItemType Directory -Path $gateDir -Force | Out-Null
        'FAILED' | Set-Content (Join-Path $gateDir 'gate-result.txt') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'APPROVE'
    }

    It 'keeps APPROVE when the trusted gate verdict is INCONCLUSIVE (build/env error must not block)' {
        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'INCONCLUSIVE' |
            Should -Be 'APPROVE'
    }

    It 'vetoes APPROVE when the expert review verdict is blocking (contradictory artifacts)' {
        # Contradictory artifacts: the raw PR wins, validation is green, and the report LLM
        # emitted APPROVE — but the expert code-review section rendered into the SAME summary
        # says NEEDS_CHANGES. Approving would post a visibly self-contradictory review.
        @{ winner = 'pr'; isPRFix = $true; candidateDiff = '' } |
            ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8
        New-Item -ItemType Directory -Path (Join-Path $script:testDir 'expert-pr-eval') -Force | Out-Null
        "### Findings`n### Verdict: NEEDS_CHANGES" |
            Set-Content (Join-Path $script:testDir 'expert-pr-eval/content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'vetoes APPROVE on a NEEDS_DISCUSSION expert verdict written under an Initial verdict heading' {
        New-Item -ItemType Directory -Path (Join-Path $script:testDir 'expert-pr-eval') -Force | Out-Null
        "### Initial verdict`n`n**NEEDS_DISCUSSION — medium confidence.**" |
            Set-Content (Join-Path $script:testDir 'expert-pr-eval/content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'vetoes APPROVE on a blocking legacy code-review verdict when no expert artifact exists' {
        New-Item -ItemType Directory -Path (Join-Path $script:testDir 'pre-flight') -Force | Out-Null
        '**Verdict:** NEEDS_CHANGES' |
            Set-Content (Join-Path $script:testDir 'pre-flight/code-review.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'keeps APPROVE when the current expert verdict is LGTM even if a stale legacy verdict is blocking' {
        # Precedence must match Get-OutcomeFromCodeReviewVerdict: the current artifact wins,
        # so a stale pre-flight verdict cannot veto an up-to-date LGTM.
        New-Item -ItemType Directory -Path (Join-Path $script:testDir 'expert-pr-eval') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $script:testDir 'pre-flight') -Force | Out-Null
        '### Verdict: LGTM' | Set-Content (Join-Path $script:testDir 'expert-pr-eval/content.md') -Encoding UTF8
        '**Verdict:** NEEDS_CHANGES' | Set-Content (Join-Path $script:testDir 'pre-flight/code-review.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'APPROVE'
    }

    It 'keeps APPROVE when no expert verdict is present at all' {
        New-Item -ItemType Directory -Path (Join-Path $script:testDir 'expert-pr-eval') -Force | Out-Null
        '### Findings`n_No blocking issues._' |
            Set-Content (Join-Path $script:testDir 'expert-pr-eval/content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'APPROVE'
    }

    It 'vetoes APPROVE to REQUEST_CHANGES when the trusted gate verdict is TIMEDOUT (fix unverified)' {
        # A gate that never finished (hang-safety timeout / no verdict) leaves the fix unverified,
        # so an APPROVE recommendation must be vetoed just like a FAILED gate.
        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'TIMEDOUT' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'softens APPROVE to COMMENT when the deep-UI run had no passing signal (all setup-failed)' {
        $uiDir = Join-Path $script:testDir 'uitests'
        New-Item -ItemType Directory -Path $uiDir -Force | Out-Null
        '⚠️ **Deep UI tests** — 2 categories (8 tests) could not run: OneTimeSetUp/fixture setup failure on the platform-pool agent — infrastructure, not a PR test failure.' |
            Set-Content (Join-Path $uiDir 'content.md') -Encoding UTF8
        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'COMMENT'
    }

    It 'softens APPROVE to COMMENT when the selected deep category ran zero tests' {
        $uiDir = Join-Path $script:testDir 'uitests'
        New-Item -ItemType Directory -Path $uiDir -Force | Out-Null
        '⚠️ **Deep UI tests** — 0 passed, 0 failed across 1 category on platform-pool agent. 1 category reported 0 tests.' |
            Set-Content (Join-Path $uiDir 'content.md') -Encoding UTF8
        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'PASSED' |
            Should -Be 'COMMENT'
    }

    It 'throws when the trusted gate verdict is not supplied (fail closed)' {
        { Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir } |
            Should -Throw '*TrustedGateResult is required*'
    }

    It 'vetoes APPROVE when deep UI tests report failures (real render format)' {
        $uiDir = Join-Path $script:testDir 'uitests'
        New-Item -ItemType Directory -Path $uiDir -Force | Out-Null
        '❌ **Deep UI tests** — 12 passed, 3 failed across 4 categories on platform-pool agent (replaces in-process counts above).' |
            Set-Content (Join-Path $uiDir 'content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'SKIPPED' |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'keeps APPROVE when deep UI tests pass (TRX-marked-failed wording does not false-trigger)' {
        $uiDir = Join-Path $script:testDir 'uitests'
        New-Item -ItemType Directory -Path $uiDir -Force | Out-Null
        '✅ **Deep UI tests** — 50 passed; 2 setup categories (1 marked failed by TRX) across 4 categories on platform-pool agent.' |
            Set-Content (Join-Path $uiDir 'content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir -TrustedGateResult 'SKIPPED' |
            Should -Be 'APPROVE'
    }

    It 'does not force changes for missing, malformed, or PR-fix winner files' {
        Get-AIReviewEventForRun -ReportContent '' -PRAgentDir $script:testDir -TrustedGateResult 'SKIPPED' |
            Should -Be 'COMMENT'

        'not json' | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8
        Get-AIReviewEventForRun -ReportContent '' -PRAgentDir $script:testDir -TrustedGateResult 'SKIPPED' |
            Should -Be 'COMMENT'

        @{
            winner = 'pr'
            isPRFix = $true
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8
        Get-AIReviewEventForRun -ReportContent '' -PRAgentDir $script:testDir -TrustedGateResult 'SKIPPED' |
            Should -Be 'COMMENT'
    }
}

Describe 'Test-DeepUITestsHadNoSignal' {
    BeforeEach {
        $script:dsDir = Join-Path ([System.IO.Path]::GetTempPath()) "ds-tests-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path (Join-Path $script:dsDir 'uitests') -Force | Out-Null
    }
    AfterEach { Remove-Item -LiteralPath $script:dsDir -Recurse -Force -ErrorAction SilentlyContinue }

    It 'is true for an all-setup-failure run with no passing signal' {
        '⚠️ **Deep UI tests** — 2 categories (8 tests) could not run: OneTimeSetUp/fixture setup failure on the platform-pool agent — infrastructure, not a PR test failure (replaces in-process counts above).' |
            Set-Content (Join-Path $script:dsDir 'uitests/content.md') -Encoding UTF8
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeTrue
    }

    It 'is true for an all-crash run (HostApp crashed mid-run, no passes)' {
        '⚠️ **Deep UI tests** — the HostApp crashed mid-run, so 8 tests could not complete. An app crash can be an infrastructure flake OR a regression introduced by this PR — review the screenshots + logcat in the drop-deep-uitests artifact before concluding.' |
            Set-Content (Join-Path $script:dsDir 'uitests/content.md') -Encoding UTF8
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeTrue
    }

    It 'is true for a selected category that reported zero runnable tests' {
        '⚠️ **Deep UI tests** — 0 passed, 0 failed across 1 category on platform-pool agent. 1 category reported 0 tests.' |
            Set-Content (Join-Path $script:dsDir 'uitests/content.md') -Encoding UTF8
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeTrue
    }

    It 'is false when another category passed even if one category reported zero tests' {
        '⚠️ **Deep UI tests** — 5 passed, 0 failed across 2 categories on platform-pool agent. 1 category reported 0 tests.' |
            Set-Content (Join-Path $script:dsDir 'uitests/content.md') -Encoding UTF8
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeFalse
    }

    It 'is false for an app crash that still had passes' {
        '⚠️ **Deep UI tests** — 5 passed; the HostApp crashed mid-run, so 3 tests could not complete.' |
            Set-Content (Join-Path $script:dsDir 'uitests/content.md') -Encoding UTF8
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeFalse
    }

    It 'is false when some tests passed alongside the setup failures' {
        '⚠️ **Deep UI tests** — 5 passed; 1 category (3 tests) could not run: OneTimeSetUp/fixture setup failure on the platform-pool agent — infrastructure, not a PR test failure.' |
            Set-Content (Join-Path $script:dsDir 'uitests/content.md') -Encoding UTF8
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeFalse
    }

    It 'is false for a normal passing run' {
        '✅ **Deep UI tests** — 50 passed, 0 failed across 4 categories on platform-pool agent.' |
            Set-Content (Join-Path $script:dsDir 'uitests/content.md') -Encoding UTF8
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeFalse
    }

    It 'is false when there is no uitests content' {
        Test-DeepUITestsHadNoSignal -PRAgentDir $script:dsDir | Should -BeFalse
    }
}

Describe 'New-FutureActionSection' {
    BeforeEach {
        $script:testDir = Join-Path ([System.IO.Path]::GetTempPath()) "future-action-tests-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $script:testDir -Force | Out-Null
    }

    AfterEach {
        Remove-Item -LiteralPath $script:testDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'renders selected try-fix candidate guidance in the AI Summary Next Steps section' {
        @{
            winner = 'try-fix-2'
            isPRFix = $false
            summary = 'Candidate avoids the regression.'
            candidateDiff = "diff --git a/file.cs b/file.cs`n+fixed"
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        $section = New-FutureActionSection -PRAgentDir $script:testDir

        $section | Should -Match '<strong>🧭 Next Steps</strong>'
        $section | Should -Match 'alternative fix proposed'
        $section | Should -Match 'try-fix-2'
        $section | Should -Match 'Candidate avoids the regression'
        $section | Should -Match 'diff --git a/file.cs b/file.cs'
    }

    It 'renders an explicit patch-required action when pr-plus-reviewer wins' {
        @{
            winner = 'pr-plus-reviewer'
            isPRFix = $true
            summary = 'The reviewer patch closes a correctness gap.'
            candidateDiff = ''
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        $section = New-FutureActionSection -PRAgentDir $script:testDir

        $section | Should -Match 'reviewer patch required'
        $section | Should -Match 'submitted PR still needs those changes'
        $section | Should -Match 'PRAgent/pr-plus-reviewer/reviewer.patch'
        $section | Should -Match 'The reviewer patch closes a correctness gap'
        $section | Should -Not -Match 'No alternative fix was selected'
    }

    It 'keeps generated guidance inside details when the agent summary contains a closing tag' {
        @{
            winner = 'pr-plus-reviewer'
            isPRFix = $true
            summary = 'Readable rationale </details> & follow-up.'
            candidateDiff = ''
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        $section = New-FutureActionSection -PRAgentDir $script:testDir
        $guidanceIndex = $section.IndexOf('Apply <code>PRAgent/pr-plus-reviewer/reviewer.patch</code>')
        $closingIndex = $section.LastIndexOf('</details>')

        $section | Should -Match ([regex]::Escape('Readable rationale &lt;/details&gt; &amp; follow-up.'))
        ([regex]::Matches($section, '(?i)<details(?:\s|>)')).Count |
            Should -Be ([regex]::Matches($section, '(?i)</details>')).Count
        $guidanceIndex | Should -BeGreaterOrEqual 0
        $guidanceIndex | Should -BeLessThan $closingIndex
    }
}
