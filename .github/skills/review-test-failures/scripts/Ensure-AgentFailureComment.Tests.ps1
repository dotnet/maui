#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Ensure-AgentFailureComment.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Write-AgentFailureAtomicUtf8Text',
            'New-AgentFailureCommentBody',
            'Ensure-AgentFailureComment'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Ensure-AgentFailureComment' {
    It 'adds a bounded fallback comment to empty agent output' {
        $path = Join-Path $TestDrive 'agent_output.json'
        [System.IO.File]::WriteAllText($path, '{"errors":[],"items":[]}')

        $result = Ensure-AgentFailureComment `
            -PrNumber 36226 `
            -Repository 'dotnet/maui' `
            -RunId 30485418341 `
            -AgentOutputPath $path

        $result.changed | Should -BeTrue
        $output = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
        @($output.items).Count | Should -Be 1
        $output.items[0].type | Should -Be 'add_comment'
        $output.items[0].item_number | Should -Be 36226
        $output.items[0].body | Should -Match '<!-- Tests Failure -->'
        $output.items[0].body | Should -Match 'Analysis unavailable'
        $output.items[0].body | Should -Match 'https://github\.com/dotnet/maui/actions/runs/30485418341'
        $output.items[0].body | Should -Match '<!-- GH_AW_TRUSTED_VISUALS -->'
        $output.items[0].body.Length | Should -BeLessThan 2000
    }

    It 'preserves an existing target comment byte-for-byte' {
        $path = Join-Path $TestDrive 'existing.json'
        $original = '{"errors":[],"items":[{"type":"add_comment","item_number":36226,"body":"existing analysis"}]}'
        [System.IO.File]::WriteAllText($path, $original)

        $result = Ensure-AgentFailureComment `
            -PrNumber 36226 `
            -Repository 'dotnet/maui' `
            -RunId 30485418341 `
            -AgentOutputPath $path

        $result.changed | Should -BeFalse
        [System.IO.File]::ReadAllText($path) | Should -BeExactly $original
    }

    It 'replaces wrong-target comments while preserving other safe-output items' {
        $path = Join-Path $TestDrive 'wrong-target.json'
        @{
            errors = @()
            items = @(
                @{
                    type = 'add_comment'
                    item_number = 1
                    body = 'wrong target'
                },
                @{
                    type = 'noop'
                    message = 'preserve me'
                }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path -Encoding UTF8

        Ensure-AgentFailureComment `
            -PrNumber 36226 `
            -Repository 'dotnet/maui' `
            -RunId 30485418341 `
            -AgentOutputPath $path | Out-Null

        $output = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
        @($output.items | Where-Object type -eq 'add_comment').Count | Should -Be 1
        @($output.items | Where-Object type -eq 'add_comment')[0].item_number | Should -Be 36226
        @($output.items | Where-Object type -eq 'noop')[0].message | Should -Be 'preserve me'
    }

    It 'recovers malformed output instead of leaving the run silent' {
        $path = Join-Path $TestDrive 'malformed.json'
        [System.IO.File]::WriteAllText($path, '{"items":[')

        $result = Ensure-AgentFailureComment `
            -PrNumber 36226 `
            -Repository 'dotnet/maui' `
            -RunId 30485418341 `
            -AgentOutputPath $path

        $result.changed | Should -BeTrue
        $result.recoveredMalformedOutput | Should -BeTrue
        $output = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
        @($output.items).Count | Should -Be 1
    }

    It 'rejects a repository value that cannot form a trusted run URL' {
        {
            Ensure-AgentFailureComment `
                -PrNumber 36226 `
                -Repository 'dotnet/maui/issues/1' `
                -RunId 30485418341 `
                -AgentOutputPath (Join-Path $TestDrive 'unused.json')
        } | Should -Throw 'Repository must be in owner/name form.'
    }
}

Describe 'Workflow integration' {
    It 'seals and invokes the fallback without exposing tokens' {
        $workflowPath = Join-Path $PSScriptRoot '../../../workflows/copilot-review-tests.md'
        $workflow = Get-Content -LiteralPath $workflowPath -Raw -Encoding UTF8

        $workflow | Should -Match 'Ensure-AgentFailureComment\.ps1'
        $workflow | Should -Match '(?m)^\s+if: failure\(\)'
        $workflow | Should -Match 'unset COPILOT_GITHUB_TOKEN GH_TOKEN GITHUB_TOKEN'
    }

    It 'uses short-lived organization-billed Copilot authentication' {
        $workflowPath = Join-Path $PSScriptRoot '../../../workflows/copilot-review-tests.md'
        $workflow = Get-Content -LiteralPath $workflowPath -Raw -Encoding UTF8

        $workflow | Should -Match '(?m)^\s+copilot-requests: write$'
        $workflow | Should -Not -Match 'COPILOT_PAT_'
        $workflow | Should -Not -Match 'shared/pat_pool\.md'
        $workflow | Should -Not -Match '(?m)^\s*COPILOT_GITHUB_TOKEN:'
    }

    It 'runs the compiled fallback after placeholder creation and before artifact upload' {
        $lockPath = Join-Path $PSScriptRoot '../../../workflows/copilot-review-tests.lock.yml'
        $lock = Get-Content -LiteralPath $lockPath -Raw -Encoding UTF8

        $placeholderIndex = $lock.IndexOf('name: Write agent output placeholder if missing', [StringComparison]::Ordinal)
        $fallbackIndex = $lock.IndexOf('name: Add fallback comment when the analysis agent fails', [StringComparison]::Ordinal)
        $uploadIndex = $lock.IndexOf('name: Upload agent artifacts', [StringComparison]::Ordinal)

        $placeholderIndex | Should -BeGreaterThan -1
        $fallbackIndex | Should -BeGreaterThan $placeholderIndex
        $uploadIndex | Should -BeGreaterThan $fallbackIndex
    }

    It 'compiles organization billing into the agent and detection jobs' {
        $lockPath = Join-Path $PSScriptRoot '../../../workflows/copilot-review-tests.lock.yml'
        $lock = Get-Content -LiteralPath $lockPath -Raw -Encoding UTF8

        ([regex]::Matches($lock, '(?m)^\s+copilot-requests: write$')).Count | Should -Be 2
        ([regex]::Matches($lock, '(?m)^\s+COPILOT_GITHUB_TOKEN: \$\{\{ github\.token \}\}$')).Count |
            Should -Be 2
        $lock | Should -Not -Match '(?m)^\s+pat_pool:$'
        $lock | Should -Not -Match 'COPILOT_PAT_'
    }
}
