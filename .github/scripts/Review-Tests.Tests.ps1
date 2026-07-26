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
            'Invoke-SealedVisualMerge',
            'Get-EmbeddedTestFailureReport',
            'Get-EmbeddedTestFailureReportCandidate',
            'Get-MarkdownFenceState',
            'Escape-Html',
            'Get-ReportVerdict',
            'Get-VerdictColor',
            'New-Badge',
            'Collapse-OpenDetails',
            'New-TestFailureReviewBody'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Local visual merge trust boundary' {
    It 'runs captured merger content without GitHub tokens and restores the parent environment' {
        $commentPath = Join-Path $TestDrive 'comment.md'
        $priorToken = [Environment]::GetEnvironmentVariable('GH_TOKEN', 'Process')
        [Environment]::SetEnvironmentVariable('GH_TOKEN', 'secret-for-test', 'Process')
        try {
            $mergeScript = @'
param(
    [int]$PrNumber,
    [string]$Repository,
    [string]$ContextJsonPath,
    [string]$CommentBodyPath
)
$tokenState = if ([string]::IsNullOrEmpty($env:GH_TOKEN)) { 'missing' } else { 'present' }
$context = Get-Content -LiteralPath $ContextJsonPath -Raw
Set-Content -LiteralPath $CommentBodyPath -Value "$tokenState|$context" -NoNewline
'@
            $result = Invoke-SealedVisualMerge `
                -MergeScriptContent $mergeScript `
                -ContextJsonContent '{"sealed":true}' `
                -CommentBodyPath $commentPath `
                -PrNumber 123 `
                -Repository 'dotnet/maui'

            $result.exitCode | Should -Be 0
            (Get-Content -LiteralPath $commentPath -Raw) | Should -Be 'missing|{"sealed":true}'
            [Environment]::GetEnvironmentVariable('GH_TOKEN', 'Process') | Should -Be 'secret-for-test'
        }
        finally {
            [Environment]::SetEnvironmentVariable('GH_TOKEN', $priorToken, 'Process')
        }
    }

    It 'returns a nonzero result when sealed merge setup fails' {
        $priorToken = [Environment]::GetEnvironmentVariable('GH_TOKEN', 'Process')
        [Environment]::SetEnvironmentVariable('GH_TOKEN', 'secret-for-setup-failure', 'Process')
        Mock New-Item {
            throw 'simulated setup failure'
        } -ParameterFilter {
            $ItemType -eq 'Directory'
        }

        try {
            $result = Invoke-SealedVisualMerge `
                -MergeScriptContent 'throw "should not run"' `
                -ContextJsonContent '{}' `
                -CommentBodyPath (Join-Path $TestDrive 'comment.md') `
                -PrNumber 123 `
                -Repository 'dotnet/maui'

            $result.exitCode | Should -Be 1
            ($result.output -join "`n") | Should -Match 'simulated setup failure'
            [Environment]::GetEnvironmentVariable('GH_TOKEN', 'Process') |
                Should -Be 'secret-for-setup-failure'
        }
        finally {
            [Environment]::SetEnvironmentVariable('GH_TOKEN', $priorToken, 'Process')
        }
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
        $body | Should -Match '^<!-- Tests Failure \(local\) -->'
        ([regex]::Matches($body, '<!-- Tests Failure \(local\) -->').Count) | Should -Be 1
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
        ([regex]::Matches($body, '<!-- Tests Failure \(local\) -->').Count) | Should -Be 1
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

        $body | Should -Match '^<!-- Tests Failure \(local\) -->'
        ([regex]::Matches($body, '## Tests Failure Analysis').Count) | Should -Be 1
        ([regex]::Matches($body, '\*\*Overall verdict:\*\*').Count) | Should -Be 1
        $body | Should -Not -Match 'Generated report:'
    }

    It 'keeps the refresh command discoverable in synthesized reports' {
        Mock gh {
            $global:LASTEXITCODE = 0
            return '{"author":{"login":"author"},"headRefOid":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"}'
        }

        $body = New-TestFailureReviewBody `
            -PRNumber 123 `
            -Repository 'dotnet/maui' `
            -ReportContent 'Short incomplete analysis.' `
            -ContextJsonPath (Join-Path $TestDrive 'missing.json')

        $body | Should -Match 'Maintainers can request a fresh review'
        $body | Should -Match '/review tests'
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
