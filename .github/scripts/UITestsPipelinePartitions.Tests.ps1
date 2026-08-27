#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Regression guard for the UI-test category *partitioning* logic in
# eng/pipelines/common/ui-tests.yml and ui-tests-steps.yml.
#
# Oversized UI-test categories (e.g. CollectionView, ~787 tests) are split into
# multiple shards so no single Azure DevOps matrix leg dominates the critical
# path. The smaller shard gets an additive fixture category; the other shard is
# the umbrella category excluding that additive category:
#   shard_1 = TestCategory=X1
#   shard_2 = TestCategory=X & TestCategory!=X1
# Tests retain the umbrella X category for local runs, while new untagged X tests
# automatically enter shard 2. This suite locks that invariant plus the YAML
# wiring. It is hermetic: it only reads repo files.

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
    $script:uiTestsPath = Join-Path $script:repoRoot 'eng/pipelines/common/ui-tests.yml'
    $script:uiStepsPath = Join-Path $script:repoRoot 'eng/pipelines/common/ui-tests-steps.yml'
    $script:deviceScripts = @(
        'android.cake'
        'ios.cake'
        'catalyst.cake'
    ) | ForEach-Object {
        Get-Content -LiteralPath (Join-Path $script:repoRoot "eng/devices/$_") -Raw
    }
    $script:testRoot = Join-Path $script:repoRoot 'src/Controls/tests/TestCases.Shared.Tests'
    $script:categoriesPath = Join-Path $script:testRoot 'UITestCategories.cs'
    $script:uiTests = Get-Content -LiteralPath $script:uiTestsPath -Raw
    $script:uiSteps = Get-Content -LiteralPath $script:uiStepsPath -Raw
    $script:categories = Get-Content -LiteralPath $script:categoriesPath -Raw
    $script:testSources = Get-ChildItem -LiteralPath $script:testRoot -Recurse -File -Filter '*.cs' | ForEach-Object {
        [pscustomobject]@{
            Path = $_.FullName
            Text = Get-Content -LiteralPath $_.FullName -Raw
        }
    }

    # Parse the partitionedCategoryGroups entries (name / category / filter).
    $script:partitions = @{}
    $entryRegex = "(?ms)-\s*name:\s*'(?<name>[^']+)'\s*\r?\n\s*category:\s*'(?<cat>[^']+)'\s*\r?\n\s*filter:\s*'(?<filter>[^']+)'"
    foreach ($m in [regex]::Matches($script:uiTests, $entryRegex)) {
        $script:partitions[$m.Groups['name'].Value] = [pscustomobject]@{
            Category = $m.Groups['cat'].Value
            Filter   = $m.Groups['filter'].Value
        }
    }

    function Get-Count([string]$text, [string]$literal) {
        return ([regex]::Matches($text, [regex]::Escape($literal))).Count
    }
}

Describe 'partitionedCategoryGroups definition' {
    It 'removes the standalone CollectionView leg from categoryGroupsToTest' {
        # CollectionView must now be run only through its partitions; a bare
        # "- 'CollectionView'" list entry would double-run it.
        [regex]::Matches($script:uiTests, "(?m)^\s*-\s*'CollectionView'\s*$").Count | Should -Be 0
    }

    It 'defines the CollectionView_1 and CollectionView_2 partitions' {
        $script:partitions.Keys | Should -Contain 'CollectionView_1'
        $script:partitions.Keys | Should -Contain 'CollectionView_2'
        $script:partitions['CollectionView_1'].Category | Should -Be 'CollectionView'
        $script:partitions['CollectionView_2'].Category | Should -Be 'CollectionView'
    }

    It 'uses the additive category and its umbrella complement' {
        $script:partitions['CollectionView_1'].Filter | Should -Be 'TestCategory=CollectionView1'
        $script:partitions['CollectionView_2'].Filter | Should -Be 'TestCategory=CollectionView&TestCategory!=CollectionView1'
    }
}

Describe 'CollectionView partitions are exhaustive and mutually exclusive' {
    It 'declares the additive CollectionView1 category' {
        $script:categories | Should -Match 'public const string CollectionView1 = "CollectionView1";'
    }

    It 'tags only the measured CollectionView_1 fixtures' {
        $expectedFixtures = @(
            'CollectionView_EmptyViewFeatureTests'
            'CollectionView_HeaderFooterFeatureTests'
            'CollectionView_ScrollingFeatureTests'
            'CollectionView_SelectionFeatureTests'
        )

        $taggedFixtures = @(
            foreach ($source in $script:testSources) {
                $matches = [regex]::Matches(
                    $source.Text,
                    '(?m)^\s*\[Category\(UITestCategories\.CollectionView1\)\]\s*\r?\n\s*public class\s+(?<class>\w+)')
                foreach ($match in $matches) {
                    $match.Groups['class'].Value
                }
            }
        ) | Sort-Object

        $taggedFixtures | Should -Be ($expectedFixtures | Sort-Object)
    }

    It 'keeps the umbrella CollectionView category on every tagged fixture' {
        foreach ($source in $script:testSources | Where-Object { $_.Text -match 'Category\(UITestCategories\.CollectionView1\)' }) {
            $source.Text | Should -Match 'Category\(UITestCategories\.CollectionView\)'
            $source.Text | Should -Match '(?m)^\s*\[(Test|TestCase|TestCaseSource)'
        }
    }

    It 'does not use fixture-name filters' {
        $script:partitions['CollectionView_1'].Filter | Should -Not -Match 'FullyQualifiedName'
        $script:partitions['CollectionView_2'].Filter | Should -Not -Match 'FullyQualifiedName'
    }
}

Describe 'matrix wiring in ui-tests.yml' {
    It 'expands the partition list in every category matrix' {
        $catLoops  = Get-Count $script:uiTests 'each categoryGroup in parameters.categoryGroupsToTest'
        $partLoops = Get-Count $script:uiTests 'each partition in parameters.partitionedCategoryGroups'
        $catLoops | Should -BeGreaterThan 0
        $partLoops | Should -Be $catLoops
    }

    It 'defines TESTFILTEREXPRESSION on both the main and partition legs of every matrix' {
        $mainLoops = Get-Count $script:uiTests 'each categoryGroup in parameters.categoryGroupsToTest'
        (Get-Count $script:uiTests "TESTFILTEREXPRESSION: ''") | Should -Be $mainLoops
        (Get-Count $script:uiTests "TESTFILTEREXPRESSION: '`${{ partition.filter }}'") | Should -Be $mainLoops
    }

    It 'forwards testFilterExpression wherever a CATEGORYGROUP testFilter is passed' {
        $cat  = Get-Count $script:uiTests 'testFilter: $(CATEGORYGROUP)'
        $expr = Get-Count $script:uiTests 'testFilterExpression: $(TESTFILTEREXPRESSION)'
        $cat | Should -BeGreaterThan 0
        $expr | Should -Be $cat
    }
}

Describe 'filter selection logic in ui-tests-steps.yml' {
    It 'declares the testFilterExpression parameter' {
        $script:uiSteps | Should -Match "(?m)^\s*testFilterExpression:\s*''"
    }

    It 'prefers the raw filter expression over the comma->TestCategory transform' {
        # The raw expression wins verbatim; only the empty case falls back to the
        # historical "TestCategory=" expansion.
        $script:uiSteps | Should -Match 'if \(\$testFilterExpression\)'
        $script:uiSteps | Should -Match '\$testFilter = \$testFilterExpression'
        $script:uiSteps | Should -Match '"TestCategory="'
    }

    It 'uses the short matrix job name for partition result files' {
        $script:uiSteps | Should -Match '\$env:TEST_RESULT_NAME\s*=\s*"\$\(System\.JobName\)"'

        foreach ($deviceScript in $script:deviceScripts) {
            $deviceScript | Should -Match 'GetTestResultsFilterName\(testFilter\)'
        }
    }

    It 'sanitizes spaces and parentheses out of partition result file names' {
        # The short job name (e.g. "Controls (API 30) CollectionView_1") contains
        # spaces and parentheses that split an unquoted MSBuild -bl: argument
        # (MSB1008). SanitizeTestResultsFilename must strip them.
        $sharedCake = Get-Content -LiteralPath (Join-Path $script:repoRoot 'eng/devices/devices-shared.cake') -Raw
        $sharedCake | Should -Match '\.Replace\(" ", "_"\)'
        $sharedCake | Should -Match '\.Replace\("\(", string\.Empty\)'
    }
}
