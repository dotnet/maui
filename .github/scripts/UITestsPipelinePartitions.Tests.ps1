#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Regression guard for the explicit CollectionView UI-test shard categories.
# CollectionView remains the umbrella category for local runs, while CI runs
# CollectionView1-4 as ordinary category matrix legs. Shard categories belong
# on fixtures except when a fixture also contains non-CollectionView tests.

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
    $script:uiTests = Get-Content -LiteralPath (Join-Path $script:repoRoot 'eng/pipelines/common/ui-tests.yml') -Raw
    $script:uiSteps = Get-Content -LiteralPath (Join-Path $script:repoRoot 'eng/pipelines/common/ui-tests-steps.yml') -Raw
    $script:testRoot = Join-Path $script:repoRoot 'src/Controls/tests/TestCases.Shared.Tests'
    $script:categories = Get-Content -LiteralPath (Join-Path $script:testRoot 'UITestCategories.cs') -Raw
    $script:testSources = Get-ChildItem -LiteralPath $script:testRoot -Recurse -File -Filter '*.cs' | ForEach-Object {
        [pscustomobject]@{
            Path = $_.FullName
            Text = Get-Content -LiteralPath $_.FullName -Raw
        }
    }

    $script:shards = 1..4 | ForEach-Object { "CollectionView$_" }
    $script:methodLevelExceptions = @('Issue7814.cs')
}

Describe 'CollectionView shard category definitions' {
    It 'defines all explicit shard categories' {
        foreach ($shard in $script:shards) {
            $script:categories | Should -Match "public const string $shard = `"$shard`";"
        }
    }

    It 'uses the shard categories as ordinary matrix entries' {
        foreach ($shard in $script:shards) {
            [regex]::Matches($script:uiTests, "(?m)^\s*-\s*'$shard'\s*$").Count | Should -Be 1
        }
        [regex]::Matches($script:uiTests, "(?m)^\s*-\s*'CollectionView'\s*$").Count | Should -Be 0
    }

    It 'does not retain custom partition or raw-filter wiring' {
        $script:uiTests | Should -Not -Match 'partitionedCategoryGroups'
        $script:uiTests | Should -Not -Match 'TESTFILTEREXPRESSION'
        $script:uiTests | Should -Not -Match 'testFilterExpression'
        $script:uiSteps | Should -Not -Match 'testFilterExpression'
    }
}

Describe 'CollectionView shards are exhaustive and mutually exclusive' {
    It 'assigns each CollectionView fixture to exactly one shard' {
        foreach ($source in $script:testSources) {
            $umbrellaCount = [regex]::Matches($source.Text, 'Category\(UITestCategories\.CollectionView\)').Count
            if ($umbrellaCount -eq 0) {
                continue
            }

            $shardAttributes = [regex]::Matches(
                $source.Text,
                'Category\(UITestCategories\.(?<shard>CollectionView[1-4])\)')
            $fileName = Split-Path $source.Path -Leaf

            if ($fileName -in $script:methodLevelExceptions) {
                $shardAttributes.Count | Should -Be $umbrellaCount -Because "$fileName requires method-level shard tags"
            }
            else {
                $shardAttributes.Count | Should -Be 1 -Because "$fileName should use one class-level shard tag"
                $source.Text | Should -Match '(?m)^\s*\[Category\(UITestCategories\.CollectionView[1-4]\)\]\s*\r?\n[ \t]*(?:(?:public|internal|private|protected)\s+)?(?:(?:sealed|abstract|static|partial)\s+)*class\s+'
            }
        }
    }

    It 'keeps the CollectionView umbrella category on every shard-tagged source' {
        foreach ($source in $script:testSources | Where-Object {
            $_.Text -match 'Category\(UITestCategories\.CollectionView[1-4]\)'
        }) {
            $source.Text | Should -Match 'Category\(UITestCategories\.CollectionView\)'
        }
    }

    It 'uses method-level shard tags only for mixed-category fixtures' {
        $methodTaggedFiles = @(
            foreach ($source in $script:testSources) {
                if ($source.Text -match '(?m)^\s*\[Category\(UITestCategories\.CollectionView[1-4]\)\]\s*\r?\n[ \t]*public void') {
                    Split-Path $source.Path -Leaf
                }
            }
        ) | Sort-Object -Unique

        $methodTaggedFiles | Should -Be ($script:methodLevelExceptions | Sort-Object)
    }

    It 'never mixes shard categories in one source file' {
        foreach ($source in $script:testSources) {
            $assignedShards = @(
                [regex]::Matches($source.Text, 'Category\(UITestCategories\.(?<shard>CollectionView[1-4])\)') |
                    ForEach-Object { $_.Groups['shard'].Value } |
                    Sort-Object -Unique
            )
            $assignedShards.Count | Should -BeLessOrEqual 1 -Because "$($source.Path) must belong to exactly one shard"
        }
    }

    It 'keeps source test counts reasonably balanced' {
        $counts = @{}
        foreach ($shard in $script:shards) {
            $counts[$shard] = 0
        }

        foreach ($source in $script:testSources) {
            $umbrellaCount = [regex]::Matches($source.Text, 'Category\(UITestCategories\.CollectionView\)').Count
            $match = [regex]::Match($source.Text, 'Category\(UITestCategories\.(?<shard>CollectionView[1-4])\)')
            if ($umbrellaCount -gt 0 -and $match.Success) {
                $counts[$match.Groups['shard'].Value] += $umbrellaCount
            }
        }

        $values = @($counts.Values)
        ($values | Measure-Object -Minimum).Minimum | Should -BeGreaterThan 0
        (($values | Measure-Object -Maximum).Maximum - ($values | Measure-Object -Minimum).Minimum) |
            Should -BeLessOrEqual 25
    }
}

Describe 'standard category filter wiring' {
    It 'expands comma-separated categories into TestCategory filters' {
        $script:uiSteps | Should -Match '"\$\{\{ parameters\.testFilter \}\}"\.Split\(","\)'
        $script:uiSteps | Should -Match '\$testFilter \+= "TestCategory="'
    }

    It 'uses the category filter directly for result file names' {
        foreach ($deviceScript in 'android.cake', 'ios.cake', 'catalyst.cake') {
            $contents = Get-Content -LiteralPath (Join-Path $script:repoRoot "eng/devices/$deviceScript") -Raw
            $contents | Should -Not -Match 'GetTestResultsFilterName'
            $contents | Should -Match 'SanitizeTestResultsFilename\(.+testFilter'
        }
    }
}
