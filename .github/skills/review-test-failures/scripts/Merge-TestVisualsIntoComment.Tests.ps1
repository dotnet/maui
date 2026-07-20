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
            'Test-VisualSnapshotPathMatchesPlatform',
            'Test-VisualComparisonChanged',
            'Get-VisualRelationship',
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
            [string]$SnapshotFileName,
            [string]$AutomatedTestName,
            [switch]$ActualOnly
        )

        $prefix = "https://raw.githubusercontent.com/dotnet/maui/$Commit/pr-$PrNumber/revision/build-1/test"
        return [pscustomobject]@{
            testName = $TestName
            platform = $Platform
            snapshotFileName = $(if ($SnapshotFileName) { $SnapshotFileName } else { "$TestName.png" })
            automatedTestName = $AutomatedTestName
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
            [object[]]$Failures = @(),
            [string[]]$ChangedFiles = @(),
            [int]$OmittedCount = 0,
            [bool]$Published = $true,
            [string]$Commit = ('a' * 40),
            [int]$PrNumber = 123
        )

        return [pscustomobject]@{
            repository = 'dotnet/maui'
            pr = [pscustomobject]@{ number = $PrNumber }
            failures = [pscustomobject]@{
                unique = $Failures
            }
            scope = [pscustomobject]@{
                changedFiles = $ChangedFiles
            }
            visualAssets = [pscustomobject]@{
                published = $Published
                commit = $Commit
                omittedCount = $OmittedCount
                comparisons = $Comparisons
            }
        }
    }

    function New-VisualTestFailure {
        param(
            [string]$TestName = 'VisualTest',
            [string]$Platform = 'ios',
            [string]$DeterministicAttribution = 'indeterminate',
            [bool]$AlsoFailsOnBaseline = $false,
            [bool]$LegAlsoFailsOnBase = $false
        )

        return [pscustomobject]@{
            testName = $TestName
            platform = $Platform
            deterministicAttribution = $DeterministicAttribution
            alsoFailsOnBaseline = $AlsoFailsOnBaseline
            legAlsoFailsOnBase = $LegAlsoFailsOnBase
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
        $failure = New-VisualTestFailure `
            -TestName '<b>@danger</b>' `
            -DeterministicAttribution 'regressed-vs-base'
        $context = New-VisualTestContext -Comparisons @($comparison) -Failures @($failure)
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
        $merged | Should -Match '<strong>Likely PR-caused</strong> - visual comparison'
        $merged | Should -Match 'The same leg was green on the sampled base build and red on this PR'
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
            -MaxCommentCharacters 2000

        $merged.Length | Should -BeLessOrEqual 2000
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

Describe 'Inline visual relationship classification' {
    It 'maps exact deterministic attribution to the review taxonomy' {
        $context = New-VisualTestContext -Failures @(
            (New-VisualTestFailure -TestName 'Regression' -DeterministicAttribution 'regressed-vs-base'),
            (New-VisualTestFailure -TestName 'Existing' -DeterministicAttribution 'pre-existing-on-base'),
            (New-VisualTestFailure -TestName 'Known' -DeterministicAttribution 'known-issue'),
            (New-VisualTestFailure -TestName 'Mixed' -AlsoFailsOnBaseline $true)
        )

        (Get-VisualRelationship `
                -Comparison (New-VisualTestComparison -TestName 'Regression') `
                -Context $context).label |
            Should -Be 'Likely PR-caused'
        (Get-VisualRelationship `
                -Comparison (New-VisualTestComparison -TestName 'Existing') `
                -Context $context).label |
            Should -Be 'Likely unrelated'
        (Get-VisualRelationship `
                -Comparison (New-VisualTestComparison -TestName 'Known') `
                -Context $context).label |
            Should -Be 'Likely unrelated'

        $mixed = Get-VisualRelationship `
            -Comparison (New-VisualTestComparison -TestName 'Mixed') `
            -Context $context
        $mixed.label | Should -Be 'Needs human investigation'
        $mixed.detail | Should -Match 'not strong enough to dismiss'
    }

    It 'requires an exact platform match before calling a visual failure unrelated' {
        $context = New-VisualTestContext -Failures @(
            (New-VisualTestFailure `
                    -TestName 'VisualTest' `
                    -Platform 'unknown' `
                    -DeterministicAttribution 'pre-existing-on-base')
        )

        $relationship = Get-VisualRelationship `
            -Comparison (New-VisualTestComparison -Platform 'ios') `
            -Context $context

        $relationship.label | Should -Be 'Needs human investigation'
        $relationship.detail | Should -Match 'No decisive exact test-and-platform'
    }

    It 'does not surface an unrecognized attribution value' {
        $context = New-VisualTestContext -Failures @(
            (New-VisualTestFailure -DeterministicAttribution '<script>@all</script>')
        )

        $relationship = Get-VisualRelationship `
            -Comparison (New-VisualTestComparison) `
            -Context $context

        $relationship.label | Should -Be 'Needs human investigation'
        $relationship.detail | Should -Not -Match 'script|@all'
    }

    It 'marks an exact changed platform snapshot as likely PR-caused' {
        $comparison = New-VisualTestComparison `
            -TestName 'ChangedSnapshot' `
            -Platform 'windows' `
            -SnapshotFileName 'ChangedSnapshot.png'
        $context = New-VisualTestContext `
            -Comparisons @($comparison) `
            -Failures @(
                (New-VisualTestFailure `
                        -TestName 'ChangedSnapshot' `
                        -Platform 'windows' `
                        -DeterministicAttribution 'pre-existing-on-base')
            ) `
            -ChangedFiles @(
                'src/Controls/tests/TestCases.WinUI.Tests/snapshots/windows/ChangedSnapshot.png'
            )

        $relationship = Get-VisualRelationship -Comparison $comparison -Context $context

        $relationship.label | Should -Be 'Likely PR-caused'
        $relationship.detail | Should -Match 'exact snapshot or visual test'
    }

    It 'does not use a same-named snapshot changed for another platform' {
        $comparison = New-VisualTestComparison `
            -TestName 'CrossPlatformSnapshot' `
            -Platform 'windows' `
            -SnapshotFileName 'CrossPlatformSnapshot.png'
        $context = New-VisualTestContext `
            -Comparisons @($comparison) `
            -Failures @(
                (New-VisualTestFailure `
                        -TestName 'CrossPlatformSnapshot' `
                        -Platform 'windows' `
                        -DeterministicAttribution 'pre-existing-on-base')
            ) `
            -ChangedFiles @(
                'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/CrossPlatformSnapshot.png'
            )

        (Get-VisualRelationship -Comparison $comparison -Context $context).label |
            Should -Be 'Likely unrelated'
    }

    It 'marks the exact changed visual test class as likely PR-caused' {
        $comparison = New-VisualTestComparison `
            -TestName 'VerifySearch' `
            -Platform 'windows' `
            -AutomatedTestName 'Microsoft.Maui.TestCases.Tests.ShellSearchHandlerFeatureTests(Windows).VerifySearch'
        $context = New-VisualTestContext `
            -Comparisons @($comparison) `
            -Failures @(
                (New-VisualTestFailure -TestName 'VerifySearch' -Platform 'windows')
            ) `
            -ChangedFiles @(
                'src/Controls/tests/TestCases.Shared.Tests/Tests/FeatureMatrix/ShellSearchHandlerFeatureTests.cs'
            )

        (Get-VisualRelationship -Comparison $comparison -Context $context).label |
            Should -Be 'Likely PR-caused'
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
