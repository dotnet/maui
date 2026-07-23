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

    It 'returns null when no complete report is embedded' {
        Get-EmbeddedTestFailureReport -Content 'Only a short analysis sentence.' |
            Should -BeNullOrEmpty
    }
}
