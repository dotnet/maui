#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Regression guard for the explicit CollectionView UI-test shard categories.
# CollectionView remains the umbrella category for local runs, while CI runs
# CollectionView1 and CollectionView2 as ordinary category matrix legs.

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

    $script:collectionView1Fixtures = @(
        'CollectionView_EmptyViewFeatureTests'
        'CollectionView_HeaderFooterFeatureTests'
        'CollectionView_ScrollingFeatureTests'
        'CollectionView_SelectionFeatureTests'
    )
}

Describe 'CollectionView shard category definitions' {
    It 'defines both explicit shard categories' {
        $script:categories | Should -Match 'public const string CollectionView1 = "CollectionView1";'
        $script:categories | Should -Match 'public const string CollectionView2 = "CollectionView2";'
    }

    It 'uses the shard categories as ordinary matrix entries' {
        [regex]::Matches($script:uiTests, "(?m)^\s*-\s*'CollectionView1'\s*$").Count | Should -Be 1
        [regex]::Matches($script:uiTests, "(?m)^\s*-\s*'CollectionView2'\s*$").Count | Should -Be 1
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
    It 'tags only the four measured heavy fixtures as CollectionView1' {
        $taggedFixtures = @(
            foreach ($source in $script:testSources) {
                foreach ($match in [regex]::Matches(
                    $source.Text,
                    '(?m)^\s*\[Category\(UITestCategories\.CollectionView1\)\]\s*\r?\n\s*public class\s+(?<class>\w+)')) {
                    $match.Groups['class'].Value
                }
            }
        ) | Sort-Object

        $taggedFixtures | Should -Be ($script:collectionView1Fixtures | Sort-Object)
    }

    It 'assigns every non-CollectionView1 umbrella category to CollectionView2' {
        foreach ($source in $script:testSources) {
            $umbrellaCount = [regex]::Matches($source.Text, 'Category\(UITestCategories\.CollectionView\)').Count
            if ($umbrellaCount -eq 0) {
                continue
            }

            $isShard1 = $source.Text -match 'Category\(UITestCategories\.CollectionView1\)'
            $shard2Count = [regex]::Matches($source.Text, 'Category\(UITestCategories\.CollectionView2\)').Count

            if ($isShard1) {
                $shard2Count | Should -Be 0 -Because "$($source.Path) belongs only to CollectionView1"
            }
            else {
                $shard2Count | Should -Be $umbrellaCount -Because "$($source.Path) must tag every CollectionView test as CollectionView2"
            }
        }
    }

    It 'keeps the CollectionView umbrella category on every shard-tagged source' {
        foreach ($source in $script:testSources | Where-Object {
            $_.Text -match 'Category\(UITestCategories\.CollectionView[12]\)'
        }) {
            $source.Text | Should -Match 'Category\(UITestCategories\.CollectionView\)'
        }
    }

    It 'never mixes CollectionView1 and CollectionView2 in one source file' {
        foreach ($source in $script:testSources) {
            $hasShard1 = $source.Text -match 'Category\(UITestCategories\.CollectionView1\)'
            $hasShard2 = $source.Text -match 'Category\(UITestCategories\.CollectionView2\)'
            ($hasShard1 -and $hasShard2) | Should -BeFalse -Because "$($source.Path) must belong to exactly one shard"
        }
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
