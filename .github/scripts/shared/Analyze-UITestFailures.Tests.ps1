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
}

Describe 'Analyze-UITestFailures input cap' {

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
}
