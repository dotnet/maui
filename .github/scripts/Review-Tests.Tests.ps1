#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Review-Tests.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Get-EmbeddedTestFailureReport',
            'Get-EmbeddedTestFailureReportCandidate',
            'Get-MarkdownFenceState',
            'Collapse-OpenDetails',
            'New-TestFailureReviewBody',
            'Invoke-GhApiWithJsonPayload',
            'Publish-TestFailureReviewComment'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }

    $skillPath = Join-Path $PSScriptRoot '../skills/review-test-failures/SKILL.md'
    $skill = Get-Content -LiteralPath $skillPath -Raw
    $templateMatch = [regex]::Match($skill, '(?s)```markdown\r?\n(?<report><!-- Tests Failure -->.*?)\r?\n```')
    if (-not $templateMatch.Success) { throw 'Canonical report template was not found in the skill.' }
    $reportTemplate = $templateMatch.Groups['report'].Value.Replace("`r`n", "`n")
    $runner = Get-Content -LiteralPath $scriptPath -Raw
    $publishStatement = $ast.EndBlock.Statements | Where-Object {
        $_ -is [System.Management.Automation.Language.IfStatementAst] -and
        $_.Clauses[0].Item1.Extent.Text -eq '$PostComment -and -not $DryRun'
    }
    if (-not $publishStatement) { throw 'Local posting guard was not found.' }
    $publishGuard = [scriptblock]::Create($publishStatement.Extent.Text)
}

Describe 'Review tests workflow contract' {
    BeforeAll {
        $workflowPath = Join-Path $PSScriptRoot '../workflows/copilot-review-tests.md'
        $workflow = Get-Content -LiteralPath $workflowPath -Raw
        $workflowBody = ([regex]::Split($workflow, '(?m)^---\r?$'))[2]
        $workflowLock = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/copilot-review-tests.lock.yml') -Raw
        $guard = [regex]::Match(
            $workflow,
            '(?ms)^    - name: Confirm exact /review tests command\r?\n.*?^      run: \|\r?\n(?<script>(?:^        [^\r\n]*\r?\n)+)')
        if (-not $guard.Success) {
            throw 'Exact-command guard was not found in the workflow.'
        }
        $guardPath = Join-Path $TestDrive 'exact-command.sh'
        Set-Content -LiteralPath $guardPath -Value ($guard.Groups['script'].Value -replace '(?m)^ {8}', '')
    }

    It 'uses one skill and one comment without a competing format or visual publisher' {
        $workflow | Should -Match '(?m)^skills:\r?\n  - \.github/skills/review-test-failures\r?\n\r?\nsafe-outputs:'
        $workflow | Should -Match 'add-comment:\r?\n    max: 1'
        $workflow | Should -Match 'roles: \[admin, maintain, write\]'
        $workflow | Should -Match 'persist-credentials: false'
        $workflow | Should -Match '(?m)^model: gpt-'
        $workflow | Should -Match '-SkipVisualEvidence'
        $workflow | Should -Not -Match 'Publish-TestVisualAssets|Merge-TestVisualsIntoComment'
        $workflowBody | Should -Match 'Invoke \*\*review-test-failures\*\*'
        $workflowBody | Should -Not -Match 'gate\.verdictCeiling|Comment-format precedence|img.shields.io|<details>'
    }

    It 'propagates the canonical presentation without a second caller template' {
        foreach ($caller in @($workflowBody, $runner)) {
            $caller | Should -Match 'visible author/commit attribution and three badges'
            $caller | Should -Match 'two closed top-level accordions: CI Analysis, then its sibling Follow-up'
            $caller | Should -Match 'never nest Follow-up inside it'
        }
        $workflowBody | Should -Match 'call `add_comment` exactly once'
        $workflowBody | Should -Match 'In dry-run\r?\nmode return the report without posting'
        $skill | Should -Not -Match 'Do not add badges'
    }

    It 'preserves the marker and allows badge URLs through compiled safe outputs' {
        $workflow | Should -Match 'body-header: "<!-- Tests Failure -->"'
        $workflow | Should -Match '(?m)^    - img\.shields\.io$'
        $messageSettings = [regex]::Matches($workflowLock, '(?m)^\s+GH_AW_SAFE_OUTPUT_MESSAGES: (?<json>"[^\r\n]+")')
        $messageSettings.Count | Should -BeGreaterThan 0
        foreach ($setting in $messageSettings) {
            $messages = $setting.Groups['json'].Value | ConvertFrom-Json | ConvertFrom-Json
            $messages.bodyHeader | Should -Be '<!-- Tests Failure -->'
        }
        $domainSettings = [regex]::Matches($workflowLock, '(?m)^\s+GH_AW_ALLOWED_DOMAINS: "(?<domains>[^"]+)"')
        $domainSettings.Count | Should -BeGreaterThan 0
        foreach ($setting in $domainSettings) {
            $domains = $setting.Groups['domains'].Value.Split(',')
            $domains | Should -Contain 'img.shields.io'
            $domains | Should -Contain 'dev.azure.com'
            $domains | Should -Contain 'helix.dot.net'
        }
    }

    It 'accepts only the exact subcommand on PR comments: <Comment> / <Event> / <PullRequest>' -ForEach @(
        @{ Comment = '/review tests'; Event = 'issue_comment'; PullRequest = 'https://api.github.com/repos/dotnet/maui/pulls/1'; Expected = 'true' }
        @{ Comment = "/review tests`n"; Event = 'issue_comment'; PullRequest = 'https://api.github.com/repos/dotnet/maui/pulls/1'; Expected = 'true' }
        @{ Comment = '/review'; Event = 'issue_comment'; PullRequest = 'https://api.github.com/repos/dotnet/maui/pulls/1'; Expected = 'false' }
        @{ Comment = '/review android'; Event = 'issue_comment'; PullRequest = 'https://api.github.com/repos/dotnet/maui/pulls/1'; Expected = 'false' }
        @{ Comment = '/review tests extra'; Event = 'issue_comment'; PullRequest = 'https://api.github.com/repos/dotnet/maui/pulls/1'; Expected = 'false' }
        @{ Comment = '/review tests'; Event = 'issue_comment'; PullRequest = ''; Expected = 'false' }
        @{ Comment = '/review tests'; Event = 'pull_request_review_comment'; PullRequest = 'https://api.github.com/repos/dotnet/maui/pulls/1'; Expected = 'false' }
        @{ Comment = ''; Event = 'workflow_dispatch'; PullRequest = ''; Expected = 'true' }
    ) {
        $names = @('EVENT_NAME', 'COMMENT_BODY', 'ISSUE_PULL_REQUEST_URL', 'GITHUB_OUTPUT')
        $saved = @{}
        foreach ($name in $names) {
            $saved[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
        }
        try {
            $env:EVENT_NAME = $Event
            $env:COMMENT_BODY = $Comment
            $env:ISSUE_PULL_REQUEST_URL = $PullRequest
            $env:GITHUB_OUTPUT = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            & bash $guardPath
            $LASTEXITCODE | Should -Be 0
            (Get-Content -LiteralPath $env:GITHUB_OUTPUT -Raw).Trim() | Should -Be "should_run=$Expected"
        }
        finally {
            foreach ($name in $names) {
                [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process')
            }
        }
    }
}

Describe 'Canonical test-failure presentation' {
    It 'keeps the header visible above exactly two closed sibling accordions in order' {
        $header = $reportTemplate.Substring(0, $reportTemplate.IndexOf('<details>'))
        $header | Should -Match '^<!-- Tests Failure -->\r?\n\r?\n## Tests Failure Analysis'
        $header | Should -Match '> @AUTHOR_LOGIN .*?\[`SHORT_SHA`\]\(https://github.com/OWNER/REPO/commit/FULL_SHA\)'
        $header | Should -Match '<p align="left">'
        ([regex]::Matches($header, '<img ').Count) | Should -Be 3
        ([regex]::Matches($header, 'labelColor=30363d&amp;style=flat-square').Count) | Should -Be 3
        $header | Should -Match 'alt="Scope CI failures" src="https://img.shields.io/badge/Scope-CI%20failures-1f6feb'
        $header | Should -Match 'alt="Commit SHORT_SHA" src="https://img.shields.io/badge/Commit-SHORT_SHA-1f6feb'
        $reportTemplate | Should -Not -Match '<details\s+open'

        $summaries = @(
            '&#x1F9EA; CI Analysis',
            '&#x1F4CB; Summary',
            '&#x1F4CA; Pipeline coverage',
            '&#x1F50E; Failure attribution',
            '&#x1F4CB; Recovered attempts',
            '&#x1F4CB; Previous-run comparison',
            '&#x1F52C; Code and regression-test evidence',
            '&#x1F4CB; Coverage and limitations',
            '&#x1F9ED; Follow-up'
        )
        $summaryMatches = [regex]::Matches($reportTemplate, '<summary><strong>(?<title>.*?)</strong>.*?</summary>\r?\n<br/>')
        @($summaryMatches | ForEach-Object { $_.Groups['title'].Value }) | Should -Be $summaries
        $reportTemplate | Should -Match '<summary><strong>&#x1F9EA; CI Analysis</strong> &#x2014; click to expand</summary>'
        $reportTemplate | Should -Match '<summary><strong>&#x1F9ED; Follow-up</strong> &#x2014; actions and refresh</summary>'
        ([regex]::Matches($reportTemplate, '</details>\r?\n\r?\n---\r?\n\r?\n<details>').Count) | Should -Be 7

        $depth = 0
        $roots = 0
        $rootTitles = @()
        foreach ($tag in [regex]::Matches($reportTemplate, '(?m)^</?details>$|^<summary><strong>(?<title>.*?)</strong>.*?</summary>$')) {
            if ($tag.Value -eq '<details>') {
                if ($depth -eq 0) { $roots++ }
                $depth++
                $depth | Should -BeLessOrEqual 2
            }
            elseif ($tag.Value -eq '</details>') {
                $depth--
                $depth | Should -BeGreaterOrEqual 0
            }
            elseif ($depth -eq 1) {
                $rootTitles += $tag.Groups['title'].Value
            }
        }
        $roots | Should -Be 2
        $rootTitles | Should -Be @('&#x1F9EA; CI Analysis', '&#x1F9ED; Follow-up')
        $depth | Should -Be 0
        $reportTemplate | Should -Match '</details>\s+</details>\s+---\s+<details>\s+<summary><strong>&#x1F9ED; Follow-up'
        $reportTemplate | Should -Match '(?s)> Maintainers: comment `/review tests` to refresh this report\.\s*</details>$'
    }

    It 'preserves dynamic metadata and the causal verdict: <Verdict>' -ForEach @(
        @{ Verdict = 'PR-related failures found'; Badge = 'PR--related%20failures%20found'; Color = 'd1242f'; Author = 'first-author'; Sha = '1111111222222233333334444444555555566666666' }
        @{ Verdict = 'Observed failures appear unrelated'; Badge = 'Observed%20failures%20appear%20unrelated'; Color = '1a7f37'; Author = 'second-author'; Sha = '2222222333333344444445555555666666677777777' }
        @{ Verdict = 'No failures found'; Badge = 'No%20failures%20found'; Color = '1a7f37'; Author = 'third-author'; Sha = '3333333444444455555556666666777777788888888' }
        @{ Verdict = 'Inconclusive'; Badge = 'Inconclusive'; Color = 'bf8700'; Author = 'fourth-author'; Sha = '4444444555555566666667777777888888899999999' }
    ) {
        $skill | Should -Match ([regex]::Escape("| $Verdict | ``$Badge`` | ``$Color`` |"))
        $shortSha = $Sha.Substring(0, 7)
        $content = $reportTemplate.Replace('AUTHOR_LOGIN', $Author).
            Replace('OWNER/REPO', 'dotnet/maui').Replace('FULL_SHA', $Sha).
            Replace('SHORT_SHA', $shortSha).Replace('OVERALL_VERDICT', $Verdict).
            Replace('VERDICT_BADGE_MESSAGE', $Badge).Replace('VERDICT_COLOR', $Color)

        $body = New-TestFailureReviewBody -PRNumber 99999 -Repository 'dotnet/maui' `
            -ReportContent $content -ContextJsonPath 'unused.json'

        $body | Should -Be $content.Insert('<!-- Tests Failure -->'.Length, "`n<!-- Test Failure Review (local) -->")
        $body | Should -Match ([regex]::Escape("> @$Author "))
        $body | Should -Match ([regex]::Escape("/commit/$Sha"))
        $body | Should -Match ([regex]::Escape("Verdict-$Badge-$Color"))
        $body | Should -Match ([regex]::Escape("Commit-$shortSha-1f6feb"))
        $body | Should -Match ([regex]::Escape("**Overall verdict:** $Verdict"))
        ([regex]::Matches($body, '<img ').Count) | Should -Be 3
        $body | Should -Not -Match 'Ready to merge|Not ready|Deterministic ceiling'
        $body | Should -Match '/review tests'
    }

    It 'allows the optional recovery section to be absent without dropping follow-up' {
        $skill | Should -Match 'Omit \*\*Recovered attempts\*\* entirely unless actual retry evidence supports it'
        $content = [regex]::Replace($reportTemplate,
            '(?s)<details>\r?\n<summary><strong>&#x1F4CB; Recovered attempts</strong>.*?</details>\r?\n\r?\n---\r?\n\r?\n', '')

        $body = New-TestFailureReviewBody -PRNumber 99999 -Repository 'dotnet/maui' `
            -ReportContent $content -ContextJsonPath 'unused.json'

        $body | Should -Not -Match 'Recovered attempts'
        ([regex]::Matches($body, '<details>').Count) | Should -Be 8
        $body | Should -Match 'Follow-up'
        $body | Should -Match '/review tests'
    }

    It 'collapses accidental open attributes without changing the report evidence' {
        $content = $reportTemplate.Replace('<details>', '<details open="open">')
        $body = New-TestFailureReviewBody -PRNumber 99999 -Repository 'dotnet/maui' `
            -ReportContent $content -ContextJsonPath 'unused.json'
        $body | Should -Be $reportTemplate.Insert('<!-- Tests Failure -->'.Length, "`n<!-- Test Failure Review (local) -->")
    }

    It 'normalizes legacy local markers and remains idempotent' {
        $legacy = $reportTemplate.Replace('<!-- Tests Failure -->', '<!-- Tests Failure (local) -->')
        $body = New-TestFailureReviewBody -PRNumber 99999 -Repository 'dotnet/maui' `
            -ReportContent $legacy -ContextJsonPath 'unused.json'
        $again = New-TestFailureReviewBody -PRNumber 99999 -Repository 'dotnet/maui' `
            -ReportContent $body -ContextJsonPath 'unused.json'

        $body | Should -Match '^<!-- Tests Failure -->'
        $body | Should -Not -Match '<!-- Tests Failure \(local\) -->'
        ([regex]::Matches($body, '<!-- Test Failure Review \(local\) -->').Count) | Should -Be 1
        $again | Should -Be $body
    }

    It 'extracts both sibling accordions without trailing chatter (fenced=<Fenced>, CRLF=<CrLf>, decoded=<Decoded>)' -ForEach @(
        @{ Fenced = $false; CrLf = $false; Decoded = $false }
        @{ Fenced = $true; CrLf = $false; Decoded = $false }
        @{ Fenced = $false; CrLf = $true; Decoded = $false }
        @{ Fenced = $true; CrLf = $true; Decoded = $false }
        @{ Fenced = $false; CrLf = $false; Decoded = $true }
        @{ Fenced = $true; CrLf = $true; Decoded = $true }
    ) {
        $expected = if ($Decoded) { [System.Net.WebUtility]::HtmlDecode($reportTemplate) } else { $reportTemplate }
        $content = if ($Fenced) { "``````markdown`n$expected`n``````" } else { $expected }
        $content = "Assistant preamble.`n`n$content`n`nTrailing note:`n<details>`n<summary>Chatter</summary>`nNot part of the report.`n</details>"
        if ($CrLf) {
            $content = $content.Replace("`n", "`r`n")
            $expected = $expected.Replace("`n", "`r`n")
        }

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Be $expected
        $report | Should -Not -Match 'Assistant preamble|Trailing note|Chatter'
        $report | Should -Match '/review tests'
    }

    It 'refuses a rich report whose Follow-up sibling is missing or unclosed' {
        $followUpBoundary = [regex]::Match($reportTemplate,
            '</details>\s+---\s+<details>\s+<summary><strong>&#x1F9ED; Follow-up')
        $followUpBoundary.Success | Should -BeTrue
        $withoutFollowUp = $reportTemplate.Substring(0, $followUpBoundary.Index + '</details>'.Length)
        $unclosedFollowUp = $reportTemplate.Substring(0, $reportTemplate.LastIndexOf('</details>'))

        foreach ($content in @($withoutFollowUp, $unclosedFollowUp)) {
            Get-EmbeddedTestFailureReport -Content $content | Should -BeNullOrEmpty
            {
                New-TestFailureReviewBody -PRNumber 99999 -Repository 'dotnet/maui' `
                    -ReportContent $content -ContextJsonPath 'unused.json'
            } | Should -Throw '*complete structured report*'
        }
    }

    It 'refuses a balanced rich report with Follow-up incorrectly nested inside CI Analysis' {
        $misnested = [regex]::Replace($reportTemplate,
            '</details>\s+---\s+(?=<details>\s+<summary><strong>&#x1F9ED; Follow-up)', "---`n`n") + "`n`n</details>"

        Get-EmbeddedTestFailureReport -Content $misnested | Should -BeNullOrEmpty
    }
}

Describe 'Local single-comment publication' {
    BeforeEach {
        $PRNumber = 99999
        $Repository = 'dotnet/maui'
        $CommentPath = Join-Path $TestDrive 'comment.md'
        $reviewBody = New-TestFailureReviewBody -PRNumber $PRNumber -Repository $Repository `
            -ReportContent $reportTemplate -ContextJsonPath 'unused.json'
        Mock Invoke-GhApiWithJsonPayload { 'https://github.com/dotnet/maui/pull/99999#issuecomment-1' }
    }

    It 'updates exactly one local report for ownership marker <Marker>' -ForEach @(
        @{ Marker = '<!-- Test Failure Review (local) -->' }
        @{ Marker = '<!-- Tests Failure (local) -->' }
    ) {
        Mock gh {
            $global:LASTEXITCODE = 0
            @(
                @{ id = 100; body = '<!-- Tests Failure --> workflow-owned report' },
                @{ id = 101; body = $Marker }
            ) | ConvertTo-Json
        }

        Publish-TestFailureReviewComment -PRNumber $PRNumber -Repository $Repository `
            -CommentPath $CommentPath -CommentBody $reviewBody | Out-Null

        Should -Invoke Invoke-GhApiWithJsonPayload -Times 1 -Exactly -ParameterFilter {
            ($Arguments -join ' ') -eq '--method PATCH repos/dotnet/maui/issues/comments/101' -and
            $Payload.body -eq $reviewBody
        }
        (Get-Content -LiteralPath $CommentPath -Raw).Trim() | Should -Be $reviewBody
    }

    It 'posts one local comment without replacing a workflow-owned report' {
        Mock gh {
            $global:LASTEXITCODE = 0
            '[{"id":100,"body":"<!-- Tests Failure --> workflow-owned report"}]'
        }

        Publish-TestFailureReviewComment -PRNumber $PRNumber -Repository $Repository `
            -CommentPath $CommentPath -CommentBody $reviewBody | Out-Null

        Should -Invoke Invoke-GhApiWithJsonPayload -Times 1 -Exactly -ParameterFilter {
            ($Arguments -join ' ') -eq '--method POST repos/dotnet/maui/issues/99999/comments' -and
            $Payload.body -eq $reviewBody
        }
    }

    It 'honors posting and dry-run flags: PostComment=<Post>, DryRun=<Dry>' -ForEach @(
        @{ Post = $false; Dry = $false; Calls = 0 }
        @{ Post = $false; Dry = $true; Calls = 0 }
        @{ Post = $true; Dry = $true; Calls = 0 }
        @{ Post = $true; Dry = $false; Calls = 1 }
    ) {
        $PostComment = $Post
        $DryRun = $Dry
        Mock Publish-TestFailureReviewComment { 'https://github.com/dotnet/maui/pull/99999#issuecomment-1' }

        & $publishGuard

        Should -Invoke Publish-TestFailureReviewComment -Times $Calls -Exactly
    }
}

Describe 'Local test-failure report extraction' {
    It 'extracts a fenced complete report without the assistant preamble or code fence' {
        $content = @'
I could not write report.md. Report follows.

```markdown
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Review</summary>

<details>
<summary>Evidence</summary>
Evidence
</details>

</details>
```

Trailing assistant prose.
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match '## Tests Failure Analysis'
        $report | Should -Not -Match 'I could not write'
        $report | Should -Not -Match '```'
        $report | Should -Not -Match 'Trailing assistant prose'
        ([regex]::Matches($report, '<details>').Count) |
            Should -Be ([regex]::Matches($report, '</details>').Count)
    }

    It 'ignores a marker quoted in assistant prose before the standalone report marker' {
        $content = @'
The report could not be written. Per the fallback rule ("return the report beginning with
`<!-- Tests Failure -->`"), here is the complete report:

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Review</summary>
Evidence
</details>
'@

        $body = New-TestFailureReviewBody `
            -PRNumber 1 `
            -Repository 'dotnet/maui' `
            -ReportContent $content `
            -ContextJsonPath 'unused.json'

        $body | Should -Not -Match 'fallback rule|here is the complete report'
        $body | Should -Match '^<!-- Tests Failure -->'
        ([regex]::Matches($body, '<!-- Test Failure Review \(local\) -->').Count) | Should -Be 1
    }

    It 'continues past a standalone marker in an earlier fenced example' {
        $content = @'
The required output shape is:

```markdown
<!-- Tests Failure -->
Example only.
```

The actual report follows.

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Actual evidence'
        $report | Should -Not -Match 'Example only|actual report follows'
    }

    It 'does not let an earlier unfenced example marker borrow the later report structure' {
        $content = @'
<!-- Tests Failure -->
Example only.

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence
</details>
'@

        $body = New-TestFailureReviewBody `
            -PRNumber 1 `
            -Repository 'dotnet/maui' `
            -ReportContent $content `
            -ContextJsonPath 'unused.json'

        $body | Should -Not -Match 'Example only'
        $body | Should -Match 'Actual evidence'
        ([regex]::Matches($body, '<!-- Test Failure Review \(local\) -->').Count) | Should -Be 1
    }

    It 'prefers a standalone marker over an earlier explanatory report heading' {
        $content = @'
## Tests Failure Analysis

This heading only explains the output that follows.

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Actual evidence'
        $report | Should -Not -Match 'only explains'
    }

    It 'prefers the earliest real report over a later structurally-valid fenced duplicate' {
        # Regression guard for the reverse-iteration tie-break: a real report FOLLOWED by a
        # complete, fenced copy of the template must still return the first (real) block. The
        # extracted content is published and can quote untrusted material, so a trailing
        # well-formed duplicate must never displace the real verdict.
        $content = @'
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence
</details>

Then the assistant echoes the expected shape as a fenced example:

```markdown
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Template example</summary>
Example evidence
</details>
```
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Actual evidence'
        $report | Should -Not -Match 'Template example|Example evidence'
    }

    It 'keeps a real report that quotes the marker inside its own details block (tier 1)' {
        # Round-3 regression guard (❌ 3/3). A real report that quotes the required template
        # marker inside its own fenced code — before its closing </details> — must NOT be
        # truncated. Bounding at the raw next anchor sliced off the report's own </details>,
        # the details-balance never returned to 0, and the candidate collapsed to $null —
        # which New-TestFailureReviewBody silently replaces with a synthesized skeleton,
        # the exact content-loss this PR exists to prevent. The quoted marker sits inside
        # code, so it is not a structural sibling and must never bound the report.
        $content = @'
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence.

The expected template shape is:

```markdown
<!-- Tests Failure -->

## Tests Failure Analysis
```

More real evidence after the example.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Not -BeNullOrEmpty
        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Actual evidence'
        $report | Should -Match 'More real evidence after the example'
    }

    It 'keeps a heading-anchored report that quotes the heading inside its own details block (tier 2)' {
        # Tier-2 counterpart: with no HTML marker, `## Tests Failure Analysis` anchors the
        # report. A heading quoted inside the report's own fenced code must not bound it
        # either — the same structural-anchor rule applies to the heading tier.
        $content = @'
## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence.

Reports must start with:

```markdown
## Tests Failure Analysis
```

Final evidence line.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Not -BeNullOrEmpty
        $report | Should -Match '^## Tests Failure Analysis'
        $report | Should -Match 'Actual evidence'
        $report | Should -Match 'Final evidence line'
    }

    It 'keeps a real report that quotes the marker as a bare unfenced line in its own details (tier 1)' {
        # Round-4 follow-up: the quoted marker is a STANDALONE UNFENCED line (0-3 space indent),
        # not fenced or 4-space-indented, sitting inside the report's own still-open <details>.
        # The structural (non-depth-aware) bound treated it as a sibling and collapsed the
        # report to $null; the depth-aware bound recognizes depth > 0 means it's the report's
        # own content and continues to the report's real closing </details>.
        $content = @'
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence.

The report must begin with this exact line:

<!-- Tests Failure -->

and everything after it is analysis. More real evidence follows.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Not -BeNullOrEmpty
        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Actual evidence'
        $report | Should -Match 'More real evidence follows'
    }

    It 'keeps a heading-anchored report that quotes the heading as a bare unfenced line in its own details (tier 2)' {
        $content = @'
## Tests Failure Analysis

<details>
<summary>Actual review</summary>
Actual evidence.

Every report opens with the heading:

## Tests Failure Analysis

which is just quoted here. Final evidence line.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Not -BeNullOrEmpty
        $report | Should -Match '^## Tests Failure Analysis'
        $report | Should -Match 'Actual evidence'
        $report | Should -Match 'Final evidence line'
    }

    It 'returns the earliest report when a self-quoting first report precedes a genuine second (no silent substitution)' {
        # The sharpest round-4 case: a self-quoting report #1 (bare unfenced marker inside its
        # own <details>) FOLLOWED by a genuine report #2. The structural bound truncated #1 to
        # $null, then returned #2 — silently dropping #1 and violating earliest-wins. Depth-aware
        # bounding keeps #1 intact and returns it.
        $content = @'
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Report ONE</summary>
Evidence for report ONE.

Template reference line:

<!-- Tests Failure -->

end of the quoted template.
</details>

Then, later, a genuinely separate second report:

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Report TWO</summary>
Evidence for report TWO.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Evidence for report ONE'
        $report | Should -Not -Match 'Evidence for report TWO'
    }

    It 'keeps a full self-quoting report that also opens a nested details block (depth 1)' {
        # Round-6 regression guard. The production report template ALWAYS contains a nested
        # <details> evidence sub-block, and self-quoting the marker inside the outer <details>
        # is the realistic behavior round-4 exists for. The round-5 eager "reject on the next
        # <details> open" broke exactly this: it dropped Evidence + the Verdict/Recommendation
        # (a genuine sibling report and a report's own nested evidence are structurally
        # identical, so eager rejection can't tell them apart). The depth-aware bound must keep
        # the whole report — evidence AND verdict.
        $content = @'
<!-- Tests Failure -->

<details>
<summary>Analysis</summary>
Evidence ONE (the root cause).

The required marker the model was told to emit:

<!-- Tests Failure -->

<details>
<summary>Stack trace</summary>
at Foo.Bar()
</details>

Verdict: real product bug. Recommendation: fix Foo.Bar.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Not -BeNullOrEmpty
        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Evidence ONE'
        $report | Should -Match 'at Foo\.Bar'
        $report | Should -Match 'Verdict: real product bug'
    }

    It 'keeps a full self-quoting report when the quote is nested at depth two' {
        # Worse round-6 variant: the marker is quoted inside a nested <details> (depth 2), and
        # the report then opens a SECOND nested evidence block. Under round-5 the flag was reset
        # only at depth 0, so it stayed armed across the sibling nested block and the second
        # <details> open returned the WHOLE report as $null (a "couldn't extract" skeleton).
        # The report and its verdict must survive.
        $content = @'
<!-- Tests Failure -->

<details>
<summary>Analysis</summary>
Root cause established.

<details>
<summary>Inner evidence A</summary>
The template the model must emit begins with:

<!-- Tests Failure -->

and more inner evidence.
</details>

<details>
<summary>Inner evidence B</summary>
at Baz.Qux().
</details>

Verdict: real product bug. Recommendation: fix Baz.Qux.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Not -BeNullOrEmpty
        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Root cause established'
        $report | Should -Match 'at Baz\.Qux'
        $report | Should -Match 'Verdict: real product bug'
    }

    It 'falls through to a genuine second report when the first is truly unclosed' {
        # The REAL borrow protection (distinct from the round-5 trailing-close fixture, which is
        # structurally a single nested report). Here Report ONE's <details> is genuinely never
        # closed, so its candidate's balance never returns to depth 0 and it is rejected —
        # extraction falls through to the well-formed Report TWO without commingling.
        $content = @'
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Report ONE (truly unclosed)</summary>
Evidence ONE — this block is never closed.

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Report TWO</summary>
Evidence TWO.
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match 'Evidence TWO'
        $report | Should -Not -Match 'Evidence ONE'
    }

    It 'accepts the rebalancing-commingle fixture as a single over-captured report (documented tradeoff)' {
        # Pins the KNOWINGLY-ACCEPTED tradeoff from round 6/7. This fixture — an earlier report
        # whose <details> is rebalanced to depth 0 by a TRAILING unmatched </details>, with a
        # full report in between — is byte-identical (OPEN → anchor@depth1 → OPEN → CLOSE →
        # CLOSE) to a legitimate report that self-quotes its marker then opens nested evidence.
        # No purely-structural per-<details>-open gate can separate them, so extraction captures
        # both under the first anchor (a superset — over-capture, not new data loss). The removed
        # round-5 test was the only one exercising this shape; without this pin, a future balance-
        # loop change or a re-added round-5 gate would silently alter the accepted behavior.
        $content = @'
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Report ONE (rebalanced by a trailing close)</summary>
Evidence ONE.

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Report TWO (well-formed)</summary>
Evidence TWO.
</details>

Some trailing prose:
</details>

FINAL CONTENT after the outer close.
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Not -BeNullOrEmpty
        # Over-capture: both reports are pulled under the first anchor (the accepted superset).
        $report | Should -Match 'Evidence ONE'
        $report | Should -Match 'Evidence TWO'
        # Content after the outer </details> is excluded (true on both parent and head — no new
        # trailing-content loss is introduced by the accepted tradeoff).
        $report | Should -Not -Match 'FINAL CONTENT'
    }

    It 'recognizes a standalone marker indented up to three spaces' {
        # Column-0-only anchoring missed markers re-indented by list nesting / wrapping.
        # 0-3 spaces is still a paragraph-level standalone line, so it must anchor.
        $content = @'
   <!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Indented marker</summary>
Indented evidence
</details>
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match 'Indented evidence'
    }

    It 'ignores a marker indented four or more spaces (indented code block)' {
        # 4-space indent = CommonMark indented code block; must NOT anchor. Guards against
        # loosening the anchor to an unbounded [ \t]* (which would match markers in code).
        $content = "Example, shown as an indented code block:`n`n    <!-- Tests Failure -->`n`n<details>`n<summary>Code sample</summary>`nSample evidence`n</details>"

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -BeNullOrEmpty
    }

    It 'preserves code fences inside an unfenced report and trims trailing prose' {
        $content = @'
The write was denied, so the report is below.

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Review</summary>

```text
error: sample
```

</details>

This sentence is outside the report.
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '```text'
        $report | Should -Match 'error: sample'
        $report | Should -Not -Match 'outside the report'
    }

    It 'preserves inner code fences inside a fenced complete report' {
        $content = @'
I could not write report.md. Report follows.

```markdown
<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Review</summary>

<details>
<summary>Evidence</summary>

```text
error: sample
```

</details>

</details>
```

Trailing assistant prose.
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '```text'
        $report | Should -Match 'error: sample'
        $report | Should -Not -Match 'Trailing assistant prose'
        ([regex]::Matches($report, '<details>').Count) |
            Should -Be ([regex]::Matches($report, '</details>').Count)
    }

    It 'ignores stray inline backticks before an unfenced report with evidence fences' {
        $content = @'
The assistant mentions an inline marker ``` before the report.

<!-- Tests Failure -->

## Tests Failure Analysis

<details>
<summary>Review</summary>

```text
error: sample
```

**Overall verdict:** Not ready

</details>

Trailing assistant prose.
'@

        $report = Get-EmbeddedTestFailureReport -Content $content

        $report | Should -Match '^<!-- Tests Failure -->'
        $report | Should -Match '```text'
        $report | Should -Match 'error: sample'
        $report | Should -Match '\*\*Overall verdict:\*\* Not ready'
        $report | Should -Not -Match 'Trailing assistant prose'
    }

    It 'reuses a complete report instead of wrapping a second title and badge section' {
        $content = @'
Generated report:

<!-- Tests Failure -->

## Tests Failure Analysis

> @author - results

<details>
<summary>Review</summary>
**Overall verdict:** Not ready
</details>
'@

        $body = New-TestFailureReviewBody `
            -PRNumber 123 `
            -Repository 'dotnet/maui' `
            -ReportContent $content `
            -ContextJsonPath (Join-Path $TestDrive 'unused.json')

        $body | Should -Match '^<!-- Tests Failure -->'
        ([regex]::Matches($body, '## Tests Failure Analysis').Count) | Should -Be 1
        ([regex]::Matches($body, '\*\*Overall verdict:\*\*').Count) | Should -Be 1
        $body | Should -Not -Match 'Generated report:'
    }

    It 'refuses to manufacture a comment from an incomplete analysis' {
        {
            New-TestFailureReviewBody `
                -PRNumber 123 `
                -Repository 'dotnet/maui' `
                -ReportContent 'Short incomplete analysis.' `
                -ContextJsonPath (Join-Path $TestDrive 'missing.json')
        } | Should -Throw '*complete structured report*'
    }

    It 'preserves legacy report evidence without manufacturing a replacement' {
        $content = @'
<!-- Tests Failure -->

## Tests Failure Analysis

**Overall verdict:** Inconclusive

<details>
<summary>Pipeline results and failure evidence</summary>

| Pipeline | Current build | Previous five runs on the same branch | Coverage |
| --- | --- | --- | --- |
| maui-pr | 123 | refs/pull/1/merge; 5/5 available | Complete |
| maui-pr-devicetests | 124 | refs/pull/1/merge; 3/5 available | Unverified |
| maui-pr-uitests | 125 | refs/pull/1/merge; 5/5 available | Complete |

**Coverage:** Incomplete - device results unavailable.
**Next action:** Inspect the missing device results.

</details>
'@
        $body = New-TestFailureReviewBody `
            -PRNumber 1 `
            -Repository 'dotnet/maui' `
            -ReportContent $content `
            -ContextJsonPath (Join-Path $TestDrive 'unused.json')

        $body | Should -Be $content.Insert('<!-- Tests Failure -->'.Length, "`n<!-- Test Failure Review (local) -->")
        $body | Should -Not -Match 'Deterministic ceiling|Ready to merge'
    }

    It 'returns null when no complete report is embedded' {
        Get-EmbeddedTestFailureReport -Content 'Only a short analysis sentence.' |
            Should -BeNullOrEmpty
    }

    It 'rejects a report with an unclosed details block' {
        Get-EmbeddedTestFailureReport -Content @'
<!-- Tests Failure -->
## Tests Failure Analysis
<details>
<summary>Review</summary>
Partial analysis
'@ | Should -BeNullOrEmpty
    }

    It 'rejects a fenced report without the outer closing fence' {
        Get-EmbeddedTestFailureReport -Content @'
```markdown
<!-- Tests Failure -->
## Tests Failure Analysis
<details>
<summary>Review</summary>
Complete-looking analysis
</details>
'@ | Should -BeNullOrEmpty
    }

    It 'stops at the first balanced outer details block before trailing details chatter' {
        $report = Get-EmbeddedTestFailureReport -Content @'
<!-- Tests Failure -->
## Tests Failure Analysis
<details>
<summary>Review</summary>
<details>
<summary>Evidence</summary>
Expected evidence
</details>
</details>

Trailing note:
<details>
<summary>Not part of the report</summary>
Unexpected chatter
</details>
'@

        $report | Should -Match 'Expected evidence'
        $report | Should -Not -Match 'Unexpected chatter|Not part of the report'
    }

    It 'ignores details-like evidence inside fenced and indented code blocks' {
        $report = Get-EmbeddedTestFailureReport -Content @'
<!-- Tests Failure -->
## Tests Failure Analysis
<details>
<summary>Review</summary>

```text
expected closing tag:
</details>
```

    </details>

**Overall verdict:** Not ready

### Recommended action
Keep the recommendation.
</details>
'@

        $report | Should -Match '\*\*Overall verdict:\*\* Not ready'
        $report | Should -Match 'Keep the recommendation'
    }

    It 'tracks tilde and longer backtick fences before reading structural details tags' {
        $report = Get-EmbeddedTestFailureReport -Content @'
~~~markdown
<!-- Tests Failure -->
## Tests Failure Analysis
<details>
<summary>Review</summary>

~~~~text
</details>
~~~~

~~~text
</details>
~~~

**Overall verdict:** Not ready
</details>
~~~
'@

        $report | Should -Match '\*\*Overall verdict:\*\* Not ready'
    }
}
