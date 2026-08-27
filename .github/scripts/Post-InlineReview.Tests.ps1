#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:ScriptPath = Join-Path $PSScriptRoot 'post-inline-review.ps1'
    $script:Content = Get-Content -Path $script:ScriptPath -Raw

    function Get-FunctionBody {
        param([string]$ScriptText, [string]$FunctionName)
        $start = $ScriptText.IndexOf("function $FunctionName")
        if ($start -lt 0) { throw "Function '$FunctionName' not found" }
        $i = $ScriptText.IndexOf('{', $start)
        $depth = 0
        for (; $i -lt $ScriptText.Length; $i++) {
            if ($ScriptText[$i] -eq '{') { $depth++ }
            elseif ($ScriptText[$i] -eq '}') {
                $depth--
                if ($depth -eq 0) {
                    return $ScriptText.Substring($start, $i - $start + 1)
                }
            }
        }
        throw "Function '$FunctionName' has no closing brace"
    }

    Invoke-Expression (Get-FunctionBody -ScriptText $script:Content -FunctionName 'New-InlineReviewMarker')
    Invoke-Expression (Get-FunctionBody -ScriptText $script:Content -FunctionName 'Test-InlineReviewMarkerExists')
}

Describe 'post-inline-review findings JSON handling' {
    It 'checks for empty findings before parsing JSON' {
        $emptyGuard = $script:Content.IndexOf('[string]::IsNullOrWhiteSpace($rawJson)')
        $parseCall = $script:Content.IndexOf('ConvertFrom-Json -ErrorAction Stop')

        $emptyGuard | Should -BeGreaterOrEqual 0
        $parseCall | Should -BeGreaterThan $emptyGuard
    }

    Context 'post-inline-review idempotency' {
        BeforeAll {
            $script:HeadA = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
            $script:HeadB = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
            $script:Findings = @(
                [pscustomobject]@{ path = 'src/B.cs'; line = 20; body = 'second' },
                [pscustomobject]@{ path = 'src/A.cs'; line = 10; body = 'first' }
            )
        }

        It 'skips a same-head rerun with the same findings regardless of finding order' {
            $marker = New-InlineReviewMarker -ReviewedHead $script:HeadA -Comments $script:Findings
            $reordered = @($script:Findings[1], $script:Findings[0])
            $rerunMarker = New-InlineReviewMarker -ReviewedHead $script:HeadA -Comments $reordered

            $rerunMarker | Should -Be $marker
            Test-InlineReviewMarkerExists -ReviewBodies @("summary`n$marker") -Marker $rerunMarker |
                Should -BeTrue
        }

        It 'allows changed findings on the same head' {
            $oldMarker = New-InlineReviewMarker -ReviewedHead $script:HeadA -Comments $script:Findings
            $changed = @([pscustomobject]@{ path = 'src/A.cs'; line = 10; body = 'changed' })
            $newMarker = New-InlineReviewMarker -ReviewedHead $script:HeadA -Comments $changed

            $newMarker | Should -Not -Be $oldMarker
            Test-InlineReviewMarkerExists -ReviewBodies @($oldMarker) -Marker $newMarker |
                Should -BeFalse
        }

        It 'allows the same findings on a new reviewed head' {
            $oldMarker = New-InlineReviewMarker -ReviewedHead $script:HeadA -Comments $script:Findings
            $newMarker = New-InlineReviewMarker -ReviewedHead $script:HeadB -Comments $script:Findings

            $newMarker | Should -Not -Be $oldMarker
            Test-InlineReviewMarkerExists -ReviewBodies @($oldMarker) -Marker $newMarker |
                Should -BeFalse
        }

        It 'queries durable review bodies before the POST and keeps stale-head rejection' {
            $queryIndex = $script:Content.IndexOf('pulls/$PRNumber/reviews?per_page=100')
            $postIndex = $script:Content.IndexOf('--method POST "repos/$Repository/pulls/$PRNumber/reviews"')

            $queryIndex | Should -BeGreaterThan -1
            $postIndex | Should -BeGreaterThan $queryIndex
            $script:Content | Should -Match 'maui-copilot-inline-review:'
            $script:Content | Should -Match ([regex]::Escape('$reviewAuthor.Equals($viewerLogin, [StringComparison]::OrdinalIgnoreCase)'))
            $script:Content | Should -Match 'PR advanced after the review snapshot; skipping stale inline findings'
        }
    }

    It 'does not silently ignore malformed non-empty JSON' {
        $script:Content | Should -Match 'ConvertFrom-Json -ErrorAction Stop'
        $script:Content | Should -Match 'contains malformed JSON'
    }
}
