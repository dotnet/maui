#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Regression guard for the UI-test category *partitioning* logic in
# eng/pipelines/common/ui-tests.yml and ui-tests-steps.yml.
#
# Oversized UI-test categories (e.g. CollectionView, ~787 tests) are split into
# multiple fixture-level shards so no single Azure DevOps matrix leg dominates
# the critical path. Each shard runs a DISJOINT subset of ONE TestCategory via a
# VSTest `--filter` FullyQualifiedName expression. The shards MUST stay
# exhaustive and mutually exclusive or coverage silently changes:
#   shard_1 = TestCategory=X & ( FQN~a | FQN~b | FQN~c )
#   shard_2 = TestCategory=X & FQN!~a & FQN!~b & FQN!~c
# so shard_1 (the tokens under `~`) must reference exactly the same fixtures as
# shard_2 (the tokens under `!~`). This suite locks that invariant plus the YAML
# wiring (every matrix expands the partition list, every CATEGORYGROUP step also
# forwards the filter expression). It is hermetic: it only reads repo files.

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
    $script:uiTests = Get-Content -LiteralPath $script:uiTestsPath -Raw
    $script:uiSteps = Get-Content -LiteralPath $script:uiStepsPath -Raw

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
    function Get-IncludeTokens([string]$filter) {
        return [regex]::Matches($filter, 'FullyQualifiedName~(?<t>\w+)') | ForEach-Object { $_.Groups['t'].Value }
    }
    function Get-ExcludeTokens([string]$filter) {
        return [regex]::Matches($filter, 'FullyQualifiedName!~(?<t>\w+)') | ForEach-Object { $_.Groups['t'].Value }
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

    It 'scopes every partition filter to its owning TestCategory' {
        foreach ($name in $script:partitions.Keys) {
            $p = $script:partitions[$name]
            $p.Filter | Should -BeLike "TestCategory=$($p.Category)&*"
        }
    }
}

Describe 'CollectionView partitions are exhaustive and mutually exclusive' {
    It 'uses the same fixture token set under ~ (shard 1) and !~ (shard 2)' {
        $include = @(Get-IncludeTokens $script:partitions['CollectionView_1'].Filter | Sort-Object -Unique)
        $exclude = @(Get-ExcludeTokens $script:partitions['CollectionView_2'].Filter | Sort-Object -Unique)
        $include.Count | Should -BeGreaterThan 0
        # Set equality guarantees shard_1 ∪ shard_2 == whole category and
        # shard_1 ∩ shard_2 == empty (De Morgan on the shared token set).
        ($include -join ',') | Should -Be ($exclude -join ',')
    }

    It 'targets only CollectionView_-prefixed feature fixtures' {
        foreach ($t in Get-IncludeTokens $script:partitions['CollectionView_1'].Filter) {
            $t | Should -BeLike 'CollectionView_*'
        }
    }

    It 'targets existing CollectionView test fixtures' {
        $sourceFiles = Get-ChildItem -LiteralPath $script:testRoot -Recurse -File -Filter '*.cs'

        foreach ($token in Get-IncludeTokens $script:partitions['CollectionView_1'].Filter) {
            $classPattern = "\bclass\s+$([regex]::Escape($token))\w*\b"
            $matches = @($sourceFiles | Where-Object {
                $source = Get-Content -LiteralPath $_.FullName -Raw
                $source -match $classPattern
            })

            $matches.Count | Should -Be 1 -Because "partition token '$token' must resolve to exactly one fixture"

            $fixtureSource = Get-Content -LiteralPath $matches[0].FullName -Raw
            $fixtureSource | Should -Match 'UITestCategories\.CollectionView'
            $fixtureSource | Should -Match '(?m)^\s*\[(Test|TestCase|TestCaseSource)'
        }
    }

    It 'groups shard 1 OR-terms in parentheses so & does not bind them apart' {
        $script:partitions['CollectionView_1'].Filter | Should -Match '&\('
        $script:partitions['CollectionView_1'].Filter | Should -Match '\)$'
        $script:partitions['CollectionView_1'].Filter | Should -Match '\|'
    }

    It 'keeps shard 2 all-AND (no OR) so operator precedence is unambiguous' {
        $script:partitions['CollectionView_2'].Filter | Should -Not -Match '\|'
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
