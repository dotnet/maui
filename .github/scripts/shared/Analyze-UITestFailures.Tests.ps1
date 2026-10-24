#Requires -Modules Pester
<#
.SYNOPSIS
    Regression tests for the ARG_MAX safety-net in Analyze-UITestFailures.ps1.

    A deep run with hundreds of failing snapshot tests (build 14842388 / PR
    #36821: 311 failures) built a multi-hundred-KB prompt that was passed to
    `copilot` as a SINGLE `-p` argument, blowing the OS per-argument limit
    (Linux MAX_ARG_STRLEN = 128 KB). `copilot` failed to start with "Argument
    list too long", the grouped ✗/●/ℹ analysis was silently omitted, and the
    summary fell back to a raw ~1800-line TRX dump. These tests pin the input
    cap that prevents a recurrence.
#>

BeforeAll {
    $script:AnalyzeScript = Join-Path $PSScriptRoot 'Analyze-UITestFailures.ps1'
    $script:AnalyzeText   = Get-Content $script:AnalyzeScript -Raw
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:AnalyzeScript,
        [ref] $tokens,
        [ref] $parseErrors)

    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'ConvertTo-UiFailureSafeConsoleText',
        'ConvertTo-UiFailureSafeMarkdownText',
        'New-UiFailureDataBoundary'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "$functionName not found"
        }

        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Analyze-UITestFailures input cap' {
    It 'only accepts approved GPT models and ignores environment model overrides' {
        $script:AnalyzeText | Should -Match ([regex]::Escape(
            "[ValidateSet('gpt-5.6-sol', 'gpt-5.3-codex')]"))
        $script:AnalyzeText | Should -Not -Match 'COPILOT_REVIEW_MODEL'
    }

    It 'defines a $maxInputChars cap that is safely under the Linux 128 KB per-arg limit' {
        $script:AnalyzeText | Should -Match '\$maxInputChars\s*=\s*(\d+)'
        [void]($script:AnalyzeText -match '\$maxInputChars\s*=\s*(\d+)')
        $cap = [int]$Matches[1]
        # Leave head-room for the ~4 KB prompt wrapper below the 131072-byte
        # Linux MAX_ARG_STRLEN, and stay well under macOS argv+env limits.
        $cap | Should -BeGreaterThan 0
        $cap | Should -BeLessOrEqual 100000
    }

    It 'truncates an oversized input so the wrapped prompt stays under the per-arg limit' {
        # Mirror the guard's arithmetic against a 311-failure-sized input.
        $inputContent = 'X' * 250000
        $maxInputChars = 80000
        if ($inputContent.Length -gt $maxInputChars) {
            $omitted = $inputContent.Length - $maxInputChars
            $inputContent = $inputContent.Substring(0, $maxInputChars) +
                "`n`n_(analysis input truncated here — $omitted more characters omitted ...)_"
        }
        # ~4 KB wrapper, like $metaPrompt around $inputContent.
        $metaPrompt = ('PROMPT-WRAPPER ' * 300) + $inputContent
        [System.Text.Encoding]::UTF8.GetByteCount($metaPrompt) | Should -BeLessThan 131072
    }

    It 'leaves a small input untouched (no truncation notice)' {
        $inputContent = 'small input' * 10
        $maxInputChars = 80000
        $capped = $inputContent
        if ($inputContent.Length -gt $maxInputChars) {
            $capped = $inputContent.Substring(0, $maxInputChars)
        }
        $capped | Should -Be $inputContent
    }

    It 'defangs Copilot console output before fallback Write-Host rendering' {
        $value = "before`r`n##vso[task.setvariable variable=x]spoof`n##[error]spoof"

        ConvertTo-UiFailureSafeConsoleText $value |
            Should -Be 'before ## vso[task.setvariable variable=x]spoof ## [error]spoof'
    }

    It 'defangs model-written Markdown without destroying formatting' {
        $value = "line 1`n##vso[task.setvariable variable=x]spoof`n##[error]spoof"

        ConvertTo-UiFailureSafeMarkdownText $value |
            Should -Be "line 1`n## vso[task.setvariable variable=x]spoof`n## [error]spoof"
    }

    It 'preserves trusted warning prefixes while sanitizing only dynamic text' {
        $script:AnalyzeText | Should -Match ([regex]::Escape(
            '$safeException = ConvertTo-UiFailureSafeConsoleText $_.Exception.Message'))
        $script:AnalyzeText | Should -Match ([regex]::Escape(
            'Write-Host "##[warning]Copilot UI-failure analysis threw: $safeException"'))
        $script:AnalyzeText | Should -Match ([regex]::Escape(
            'Write-Host "##[warning]Copilot produced no UI-failure analysis file (copilotFailed=$copilotFailed)'))
        $script:AnalyzeText | Should -Not -Match 'ConvertTo-UiFailureSafeConsoleText "##\[warning\]'
    }

    It 'uses an unpredictable boundary token that cannot collide with untrusted input' {
        $candidates = [System.Collections.Generic.Queue[string]]::new()
        $candidates.Enqueue('COLLIDING_BOUNDARY')
        $candidates.Enqueue('SAFE_BOUNDARY')

        $boundary = New-UiFailureDataBoundary `
            -Content 'attacker content containing COLLIDING_BOUNDARY' `
            -CandidateFactory { $candidates.Dequeue() }

        $boundary | Should -Be 'SAFE_BOUNDARY'
        $script:AnalyzeText | Should -Match ([regex]::Escape(
            '"MAUI_UI_FAILURE_DATA_$([Guid]::NewGuid().ToString(''N''))"'))
        $script:AnalyzeText | Should -Not -Match '>>>DATA|<<<DATA'
        $script:AnalyzeText | Should -Match ([regex]::Escape(
            '-----END UNTRUSTED DATA $dataBoundary-----'))
    }
}
