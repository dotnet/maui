#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Merge-TestVisualsIntoComment.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Get-InlineVisualStartMarker',
            'Get-InlineVisualEndMarker',
            'Get-InlineVisualPlaceholder',
            'Get-BoundedText',
            'Escape-VisualText',
            'Test-VisualAssetUrl',
            'Get-CommentLimitCounts',
            'Remove-InlineVisualSection',
            'New-InlineVisualPanel',
            'New-InlineVisualSection',
            'Insert-InlineVisualSection',
            'Test-CommentWithinLimits',
            'Merge-VisualsIntoBody',
            'Write-AtomicUtf8Text',
            'Update-AgentOutputFile',
            'Update-CommentBodyFile'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }

    function New-VisualTestComparison {
        param(
            [string]$Commit = ('a' * 40),
            [int]$PrNumber = 123,
            [string]$TestName = 'VisualTest',
            [string]$Platform = 'ios',
            [switch]$ActualOnly
        )

        $prefix = "https://raw.githubusercontent.com/dotnet/maui/$Commit/pr-$PrNumber/revision/build-1/test"
        return [pscustomobject]@{
            testName = $TestName
            platform = $Platform
            description = '1.25% difference'
            buildId = 456
            baselineStatus = 'resolved from the tested runtime environment'
            baselineUrl = $(if ($ActualOnly) { $null } else { "$prefix-baseline.png" })
            actualUrl = "$prefix-actual.png"
            diffUrl = $(if ($ActualOnly) { $null } else { "$prefix-diff.png" })
        }
    }

    function New-VisualTestContext {
        param(
            [object[]]$Comparisons = @((New-VisualTestComparison)),
            [int]$OmittedCount = 0,
            [bool]$Published = $true,
            [string]$Commit = ('a' * 40),
            [int]$PrNumber = 123
        )

        return [pscustomobject]@{
            repository = 'dotnet/maui'
            pr = [pscustomobject]@{ number = $PrNumber }
            visualAssets = [pscustomobject]@{
                published = $Published
                commit = $Commit
                omittedCount = $OmittedCount
                comparisons = $Comparisons
            }
        }
    }
}

Describe 'Inline visual input validation' {
    It 'accepts only immutable asset URLs for the expected repository, commit, and PR' {
        $commit = 'a' * 40
        $valid = "https://raw.githubusercontent.com/dotnet/maui/$commit/pr-123/revision/build-1/test.png"

        Test-VisualAssetUrl -Url $valid -Repository 'dotnet/maui' -PrNumber 123 -AssetCommit $commit |
            Should -BeTrue
        Test-VisualAssetUrl -Url $valid -Repository 'dotnet/maui' -PrNumber 124 -AssetCommit $commit |
            Should -BeFalse
        Test-VisualAssetUrl -Url $valid -Repository 'dotnet/maui' -PrNumber 123 -AssetCommit ('b' * 40) |
            Should -BeFalse
        Test-VisualAssetUrl `
            -Url "https://evil.example/dotnet/maui/$commit/pr-123/test.png" `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -AssetCommit $commit |
            Should -BeFalse
        Test-VisualAssetUrl `
            -Url "https://raw.githubusercontent.com/dotnet/maui/$commit/pr-123/%2e%2e/test.png" `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -AssetCommit $commit |
            Should -BeFalse
        Test-VisualAssetUrl `
            -Url "$valid?token=unexpected" `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -AssetCommit $commit |
            Should -BeFalse
    }

    It 'escapes markup and neutralizes mention-like text in injected labels' {
        $escaped = Escape-VisualText -Value '<script>@everyone & "quoted"</script>'

        $escaped | Should -Be '&lt;script&gt;&#64;everyone &amp; &quot;quoted&quot;&lt;/script&gt;'
        [regex]::Matches($escaped, '@\w+').Count | Should -Be 0
    }

    It 'uses the same URL, mention, and UTF-16 character counting shape as gh-aw' {
        $body = 'https://one.example/a https://two.example/b > @author email@test'
        $counts = Get-CommentLimitCounts -Body $body

        $counts.urls | Should -Be 2
        $counts.mentions | Should -Be 2
        $counts.characters | Should -Be $body.Length
    }
}

Describe 'Inline visual body merge' {
    It 'replaces the trusted placeholder with escaped expandable panels inside one comment' {
        $comparison = New-VisualTestComparison -TestName '<b>@danger</b>'
        $context = New-VisualTestContext -Comparisons @($comparison)
        $body = @'
## Tests Failure Analysis

> @author - results

<details>
<summary>Test Failure Review</summary>

**Overall verdict:** Not ready.

<!-- GH_AW_TRUSTED_VISUALS -->

### Recommended action

Investigate.
</details>
'@

        $merged = Merge-VisualsIntoBody `
            -Body $body `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        $merged | Should -Match ([regex]::Escape((Get-InlineVisualStartMarker)))
        $merged | Should -Not -Match ([regex]::Escape((Get-InlineVisualPlaceholder)))
        $merged | Should -Match '&lt;b&gt;&#64;danger&lt;/b&gt;'
        $merged | Should -Match '<th>CI baseline</th><th>Fresh PR actual</th><th>CI diff</th>'
        $merged.IndexOf('### Visual failure comparisons') |
            Should -BeLessThan $merged.IndexOf('### Recommended action')
        ([regex]::Matches($merged, '<details>').Count) | Should -Be 2

        $counts = Get-CommentLimitCounts -Body $merged
        $counts.urls | Should -Be 3
        $counts.mentions | Should -Be 1
        $counts.characters | Should -BeLessOrEqual 60000
    }

    It 'dynamically omits panels that would exceed the final comment URL budget' {
        $existingUrls = (1..43 | ForEach-Object { "https://example.com/$_" }) -join ' '
        $body = "$existingUrls`n<details>`n<!-- GH_AW_TRUSTED_VISUALS -->`n</details>"
        $context = New-VisualTestContext

        $merged = Merge-VisualsIntoBody `
            -Body $body `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        ([regex]::Matches($merged, 'visual comparison</summary>').Count) | Should -Be 0
        $merged | Should -Match '1 additional comparison\(s\) were omitted'
        (Get-CommentLimitCounts -Body $merged).urls | Should -Be 43
    }

    It 'can still fit a later one-image panel after a three-image panel is omitted' {
        $existingUrls = (1..43 | ForEach-Object { "https://example.com/$_" }) -join ' '
        $comparisons = @(
            (New-VisualTestComparison -TestName 'ThreeImages'),
            (New-VisualTestComparison -TestName 'ActualOnly' -ActualOnly)
        )
        $context = New-VisualTestContext -Comparisons $comparisons
        $body = "$existingUrls`n<details>`n<!-- GH_AW_TRUSTED_VISUALS -->`n</details>"

        $merged = Merge-VisualsIntoBody `
            -Body $body `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        $merged | Should -Not -Match '<code>ThreeImages</code>'
        $merged | Should -Match '<code>ActualOnly</code>'
        (Get-CommentLimitCounts -Body $merged).urls | Should -Be 44
    }

    It 'bounds by final character count without truncating HTML mid-panel' {
        $comparison = New-VisualTestComparison -TestName ('LongName' + ('x' * 500))
        $context = New-VisualTestContext -Comparisons @($comparison)
        $body = ('a' * 1200) + "`n<details>`n<!-- GH_AW_TRUSTED_VISUALS -->`n</details>"

        $merged = Merge-VisualsIntoBody `
            -Body $body `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 1700

        $merged.Length | Should -BeLessOrEqual 1700
        ([regex]::Matches($merged, '<details>').Count) |
            Should -Be ([regex]::Matches($merged, '</details>').Count)
        $merged | Should -Match 'additional comparison\(s\) were omitted'
    }

    It 'removes the placeholder without adding a section when no assets were published' {
        $context = New-VisualTestContext -Published $false
        $body = "Analysis`n<!-- GH_AW_TRUSTED_VISUALS -->`nDone"

        $merged = Merge-VisualsIntoBody `
            -Body $body `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        $merged | Should -Be "Analysis`n`nDone"
    }

    It 'skips an invalid actual URL and reports the comparison as omitted' {
        $comparison = New-VisualTestComparison
        $comparison.actualUrl = 'https://evil.example/payload.png'
        $context = New-VisualTestContext -Comparisons @($comparison)

        $merged = Merge-VisualsIntoBody `
            -Body '<details><!-- GH_AW_TRUSTED_VISUALS --></details>' `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        $merged | Should -Not -Match 'evil\.example'
        $merged | Should -Match '1 additional comparison\(s\) were omitted'
    }

    It 'is idempotent within the pre-post payload' {
        $context = New-VisualTestContext
        $body = '<details><!-- GH_AW_TRUSTED_VISUALS --></details>'

        $first = Merge-VisualsIntoBody `
            -Body $body `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000
        $second = Merge-VisualsIntoBody `
            -Body $first `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        ([regex]::Matches($second, [regex]::Escape((Get-InlineVisualStartMarker))).Count) |
            Should -Be 1
        ([regex]::Matches($second, 'visual comparison</summary>').Count) | Should -Be 1
    }

    It 'appends the section when an agent omits both the placeholder and details wrapper' {
        $context = New-VisualTestContext
        $merged = Merge-VisualsIntoBody `
            -Body 'Short failure report.' `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        $merged | Should -Match '^Short failure report\.'
        $merged | Should -Match '### Visual failure comparisons'
    }
}

Describe 'Agent output mutation' {
    It 'atomically updates only the matching add_comment item and preserves its schema' {
        $context = New-VisualTestContext
        $path = Join-Path $TestDrive 'agent_output.json'
        @{
            errors = @()
            items = @(
                @{
                    type = 'add_comment'
                    item_number = 123
                    body = '<details><!-- GH_AW_TRUSTED_VISUALS --></details>'
                    temporary_id = 'aw_123'
                },
                @{
                    type = 'noop'
                    message = 'keep me'
                }
            )
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path -Encoding UTF8

        $result = Update-AgentOutputFile `
            -Path $path `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        $result.changed | Should -BeTrue
        $result.mergedComments | Should -Be 1
        $updated = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
        @($updated.items).Count | Should -Be 2
        $updated.items[0].temporary_id | Should -Be 'aw_123'
        $updated.items[0].body | Should -Match '### Visual failure comparisons'
        $updated.items[1].message | Should -Be 'keep me'
    }

    It 'leaves noop-only output byte-for-byte unchanged' {
        $context = New-VisualTestContext
        $path = Join-Path $TestDrive 'noop_output.json'
        $original = '{"errors":[],"items":[{"type":"noop","message":"dry run"}]}'
        [System.IO.File]::WriteAllText($path, $original)

        $result = Update-AgentOutputFile `
            -Path $path `
            -Context $context `
            -Repository 'dotnet/maui' `
            -PrNumber 123 `
            -MaxCommentUrls 45 `
            -MaxCommentMentions 10 `
            -MaxCommentCharacters 60000

        $result.changed | Should -BeFalse
        [System.IO.File]::ReadAllText($path) | Should -BeExactly $original
    }

    It 'does not damage malformed agent output when parsing fails' {
        $context = New-VisualTestContext
        $path = Join-Path $TestDrive 'malformed.json'
        $original = '{"items":['
        [System.IO.File]::WriteAllText($path, $original)

        {
            Update-AgentOutputFile `
                -Path $path `
                -Context $context `
                -Repository 'dotnet/maui' `
                -PrNumber 123 `
                -MaxCommentUrls 45 `
                -MaxCommentMentions 10 `
                -MaxCommentCharacters 60000
        } | Should -Throw
        [System.IO.File]::ReadAllText($path) | Should -BeExactly $original
    }
}
