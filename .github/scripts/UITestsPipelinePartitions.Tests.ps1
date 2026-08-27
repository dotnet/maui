#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Regression guard for method-level CollectionView UI-test shards.
# ShardedTestCategory exposes both the umbrella and numbered shard categories.

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
    Import-Module (Join-Path $script:repoRoot '.github/skills/rebalance-ui-test-categories/scripts/Rebalance-UITestCategories.psm1') -Force

    $script:uiTests = Get-Content -LiteralPath (Join-Path $script:repoRoot 'eng/pipelines/common/ui-tests.yml') -Raw
    $script:uiSteps = Get-Content -LiteralPath (Join-Path $script:repoRoot 'eng/pipelines/common/ui-tests-steps.yml') -Raw
    $script:testRoot = Join-Path $script:repoRoot 'src/Controls/tests/TestCases.Shared.Tests'
    $script:categories = Get-Content -LiteralPath (Join-Path $script:testRoot 'UITestCategories.cs') -Raw
    $script:summary = Get-Content -LiteralPath (
        Join-Path $script:repoRoot '.github/skills/rebalance-ui-test-categories/CollectionView-rebalance-summary.json'
    ) -Raw | ConvertFrom-Json
    $script:inventory = @(Get-UITestInventory -TestRoot $script:testRoot -Category CollectionView)
    $script:shards = 1..$script:summary.shardCount | ForEach-Object { "CollectionView$_" }
}

Describe 'CollectionView shard category definitions' {
    It 'defines exactly the planned shard constants' {
        foreach ($shard in $script:shards) {
            $script:categories | Should -Match "public const string $shard = `"$shard`";"
        }
        $script:categories | Should -Not -Match "public const string CollectionView$($script:summary.shardCount + 1) ="
    }

    It 'uses each shard as one ordinary matrix entry' {
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
    It 'uses exactly one planned method-level ShardedTestCategory on every test' {
        foreach ($test in $script:inventory) {
            $test.ShardCount | Should -Be 1 -Because "$($test.Id) requires exactly one shard"
            $test.Shard | Should -BeIn $script:shards -Because "$($test.Id) must use a current shard"
        }
    }

    It 'does not retain paired or class-level CollectionView categories' {
        $sources = Get-ChildItem -LiteralPath $script:testRoot -Recurse -File -Filter '*.cs'
        foreach ($source in $sources) {
            $text = Get-Content -LiteralPath $source.FullName -Raw
            $text | Should -Not -Match '\[Category\(UITestCategories\.CollectionView\d*\)'
            $text | Should -Not -Match '\[ShardedTest\]'
            $text | Should -Not -Match (
                '(?m)^\s*\[ShardedTestCategory\(UITestCategories\.CollectionView.*\]\s*\r?\n' +
                '\s*(?:(?:public|internal|private|protected|sealed|abstract|static|partial)\s+)*class\s+')
        }
    }

    It 'initializes each sharded FeatureMatrix fixture independently' {
        $featureMatrixSources = Get-ChildItem -LiteralPath (
            Join-Path $script:testRoot 'Tests/FeatureMatrix'
        ) -File -Filter 'CollectionView_*FeatureTests.cs'

        foreach ($source in $featureMatrixSources) {
            $text = Get-Content -LiteralPath $source.FullName -Raw
            $text | Should -Match 'protected override string GallerySubPageButton =>'
            $text | Should -Not -Match '\[Test,\s*Order\(1\)\]'
        }
    }

    It 'keeps ordered fixtures in one shard while retaining method-level tags' {
        $orderedFixtures = @(
            $script:inventory | Group-Object FullClassName | Where-Object {
                @($_.Group | Where-Object { $_.Ordered }).Count -gt 0
            }
        )
        foreach ($fixture in $orderedFixtures) {
            @($fixture.Group | Select-Object -ExpandProperty Shard -Unique).Count |
                Should -Be 1 -Because "$($fixture.Name) uses ordered setup and shared state"
        }
    }

    It 'projects every platform and shard below the target' {
        foreach ($shard in $script:summary.projectedShardMinutes.PSObject.Properties) {
            foreach ($platform in $shard.Value.PSObject.Properties) {
                [double]$platform.Value | Should -BeLessThan $script:summary.planningLimitMinutes -Because (
                    "$($shard.Name) on $($platform.Name) must remain below the target"
                )
            }
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
