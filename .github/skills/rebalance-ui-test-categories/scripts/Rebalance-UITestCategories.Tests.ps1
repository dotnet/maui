#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

BeforeAll {
	Import-Module (Join-Path $PSScriptRoot 'Rebalance-UITestCategories.psm1') -Force
}

Describe 'historical timing aggregation' {
	It 'uses p80 of per-run totals grouped by test and platform' {
		$evidence = [pscustomobject]@{
			samples = @(
				[pscustomobject]@{ TestId = 'N.C.A'; Platform = 'Android'; DurationMinutes = 1 }
				[pscustomobject]@{ TestId = 'N.C.A'; Platform = 'Android'; DurationMinutes = 2 }
				[pscustomobject]@{ TestId = 'N.C.A'; Platform = 'Android'; DurationMinutes = 9 }
				[pscustomobject]@{ TestId = 'N.C.A'; Platform = 'Android'; DurationMinutes = 3 }
				[pscustomobject]@{ TestId = 'N.C.A'; Platform = 'Android'; DurationMinutes = 4 }
			)
		}

		$weights = Get-AggregatedWeights -Evidence $evidence
		$weights['N.C.A']['Android'] | Should -Be 4
	}

	It 'uses nearest-rank p80 for fixed overhead evidence' {
		Get-Percentile -Values @(1, 2, 3, 4, 20) -Percentile 0.8 | Should -Be 4
	}

	It 'normalizes missing automated names consistently for parameterized results' {
		$result = [pscustomobject]@{
			automatedTestName = ''
			testCaseTitle = 'N.ParameterizedTests.Case(42)'
		}
		$byClass = @{ 'N.ParameterizedTests' = @([pscustomobject]@{ Id = 'N.ParameterizedTests.Case' }) }

		$name = Get-ResultAutomatedName -Result $result
		$name | Should -Be 'N.ParameterizedTests.Case(42)'
		Resolve-ResultClass -AutomatedName $name -ByClass $byClass | Should -Be 'N.ParameterizedTests'
	}

	It 'recognizes current and legacy shard job names' {
		$run = [pscustomobject]@{
			pipelineReference = [pscustomobject]@{
				jobReference = [pscustomobject]@{ jobName = 'Widget3' }
			}
		}
		Get-OrdinaryRunKind -Run $run -Category Widget | Should -Be 'CategoryMatrixShard'

		$run.pipelineReference.jobReference.jobName = 'Widget_3'
		Get-OrdinaryRunKind -Run $run -Category Widget | Should -Be 'CategoryMatrixShard'
	}
}

Describe 'multiplatform assignment' {
	BeforeEach {
		$script:inventory = @(
			[pscustomobject]@{ Id = 'N.A.Test'; FullClassName = 'N.A'; MethodName = 'Test'; RelativeFile = 'A.cs'; DisabledAllPlatforms = $false; Ordered = $false }
			[pscustomobject]@{ Id = 'N.B.Test'; FullClassName = 'N.B'; MethodName = 'Test'; RelativeFile = 'B.cs'; DisabledAllPlatforms = $false; Ordered = $false }
			[pscustomobject]@{ Id = 'N.C.Test'; FullClassName = 'N.C'; MethodName = 'Test'; RelativeFile = 'C.cs'; DisabledAllPlatforms = $false; Ordered = $false }
		)
		$script:evidence = [pscustomobject]@{
			category = 'Widget'
			source = [pscustomobject]@{ type = 'fixture' }
			unmatchedResults = @()
			fixedOverheadMinutes = [pscustomobject]@{ Android = 5; iOS = 5 }
			samples = @(
				[pscustomobject]@{ TestId = 'N.A.Test'; Platform = 'Android'; DurationMinutes = 35 }
				[pscustomobject]@{ TestId = 'N.A.Test'; Platform = 'iOS'; DurationMinutes = 5 }
				[pscustomobject]@{ TestId = 'N.B.Test'; Platform = 'Android'; DurationMinutes = 5 }
				[pscustomobject]@{ TestId = 'N.B.Test'; Platform = 'iOS'; DurationMinutes = 35 }
				[pscustomobject]@{ TestId = 'N.C.Test'; Platform = 'Android'; DurationMinutes = 20 }
				[pscustomobject]@{ TestId = 'N.C.Test'; Platform = 'iOS'; DurationMinutes = 20 }
			)
		}
	}

	It 'selects enough shards and is deterministic' {
		$first = New-UITestShardPlan -Category Widget -Inventory $inventory -Evidence $evidence -TargetMinutes 45
		$second = New-UITestShardPlan -Category Widget -Inventory $inventory -Evidence $evidence -TargetMinutes 45

		$first.shardCount | Should -Be 3
		$first.safetyMarginMinutes | Should -Be 2
		$first.planningLimitMinutes | Should -Be 43
		$first.maxProjectedMinutes | Should -BeLessThan 43
		($first.assignments | ConvertTo-Json -Compress) | Should -Be ($second.assignments | ConvertTo-Json -Compress)
	}

	It 'fails when an individual test makes the target impossible' {
		$evidence.samples[0].DurationMinutes = 38
		{ New-UITestShardPlan -Category Widget -Inventory $inventory -Evidence $evidence -TargetMinutes 45 } |
			Should -Throw '*unattainable*'
	}

	It 'keeps methods together when their fixture uses NUnit ordering' {
		$inventory = @(
			[pscustomobject]@{ Id = 'N.Ordered.Setup'; FullClassName = 'N.Ordered'; MethodName = 'Setup'; RelativeFile = 'Ordered.cs'; DisabledAllPlatforms = $false; Ordered = $true }
			[pscustomobject]@{ Id = 'N.Ordered.Verify'; FullClassName = 'N.Ordered'; MethodName = 'Verify'; RelativeFile = 'Ordered.cs'; DisabledAllPlatforms = $false; Ordered = $false }
			[pscustomobject]@{ Id = 'N.Other.Test'; FullClassName = 'N.Other'; MethodName = 'Test'; RelativeFile = 'Other.cs'; DisabledAllPlatforms = $false; Ordered = $false }
		)
		$evidence.samples = @(
			[pscustomobject]@{ TestId = 'N.Ordered.Setup'; Platform = 'Android'; DurationMinutes = 10 }
			[pscustomobject]@{ TestId = 'N.Ordered.Verify'; Platform = 'Android'; DurationMinutes = 10 }
			[pscustomobject]@{ TestId = 'N.Other.Test'; Platform = 'Android'; DurationMinutes = 20 }
		)
		$evidence.fixedOverheadMinutes = [pscustomobject]@{ Android = 1 }

		$plan = New-UITestShardPlan -Category Widget -Inventory $inventory -Evidence $evidence -TargetMinutes 30
		$orderedShards = @($plan.assignments | Where-Object className -eq 'N.Ordered' | Select-Object -ExpandProperty shard -Unique)

		$plan.shardCount | Should -Be 2
		$orderedShards.Count | Should -Be 1
		@($plan.assignments | Where-Object className -eq 'N.Ordered' | Where-Object fixtureCohesionRequired).Count | Should -Be 2
	}

	It 'can require more shards when the safety margin changes' {
		$inventory = 1..4 | ForEach-Object {
			[pscustomobject]@{
				Id = "N.C.M$_"; FullClassName = 'N.C'; MethodName = "M$_"; RelativeFile = 'C.cs'
				DisabledAllPlatforms = $false
				Ordered = $false
			}
		}
		$evidence.samples = @($inventory | ForEach-Object {
			[pscustomobject]@{ TestId = $_.Id; Platform = 'Android'; DurationMinutes = 19 }
		})
		$evidence.fixedOverheadMinutes = [pscustomobject]@{ Android = 3 }

		(New-UITestShardPlan -Category Widget -Inventory $inventory -Evidence $evidence `
			-TargetMinutes 45 -SafetyMarginMinutes 0).shardCount | Should -Be 2
		(New-UITestShardPlan -Category Widget -Inventory $inventory -Evidence $evidence `
			-TargetMinutes 45 -SafetyMarginMinutes 5).shardCount | Should -Be 4
	}

	It 'can preserve an existing shard count' {
		$plan = New-UITestShardPlan -Category Widget -Inventory $inventory -Evidence $evidence `
			-TargetMinutes 45 -MinimumShards 4 -MaxShards 4

		$plan.shardCount | Should -Be 4
	}
}

Describe 'source application' {
	It 'keeps umbrella attributes and adds one method-level shard for standalone and combined attributes' {
		$root = Join-Path $TestDrive 'tests'
		New-Item -ItemType Directory -Path $root | Out-Null
		$path = Join-Path $root 'WidgetTests.cs'
		@'
namespace N;

[Category(UITestCategories.Widget1)]
[Category(UITestCategories.Widget)]
public class WidgetTests
{
	[Test]
	[Category(UITestCategories.Widget)]

	public void Standalone() { }

	[Test, Category(UITestCategories.Widget)]
	[Category(UITestCategories.Widget2)]
	public void Combined() { }

	[Test]
	public void InheritedUmbrella() { }
}
'@ | Set-Content -LiteralPath $path -NoNewline
		$assignments = @(
			[pscustomobject]@{ testId = 'N.WidgetTests.Standalone'; file = 'WidgetTests.cs'; shard = 'Widget1' }
			[pscustomobject]@{ testId = 'N.WidgetTests.Combined'; file = 'WidgetTests.cs'; shard = 'Widget2' }
			[pscustomobject]@{ testId = 'N.WidgetTests.InheritedUmbrella'; file = 'WidgetTests.cs'; shard = 'Widget1' }
		)

		Set-UITestShardCategories -TestRoot $root -Category Widget -Assignments $assignments
		$text = Get-Content -LiteralPath $path -Raw

		[regex]::Matches($text, 'Category\(UITestCategories\.Widget\)').Count | Should -Be 3
		[regex]::Matches($text, 'Category\(UITestCategories\.Widget[12]\)').Count | Should -Be 3
		$text | Should -Not -Match '(?m)^\[Category\(UITestCategories\.Widget\d+\)\]\r?\npublic class'
		$text | Should -Match '(?s)Category\(UITestCategories\.Widget\).*Category\(UITestCategories\.Widget1\).*public void Standalone'
		$text | Should -Match '(?s)Category\(UITestCategories\.Widget\).*Category\(UITestCategories\.Widget2\).*public void Combined'
		$text | Should -Match '(?s)\[Test\]\r?\n\s*\[Category\(UITestCategories\.Widget1\)\]\r?\n\s*public void InheritedUmbrella'
	}

	It 'updates constants, analyzer prefixes, and a standalone matrix entry without changing line endings' {
		$categoriesPath = Join-Path $TestDrive 'UITestCategories.cs'
		$pipelinePath = Join-Path $TestDrive 'ui-tests.yml'
		$analyzerPath = Join-Path $TestDrive 'Analyzer.cs'
		[IO.File]::WriteAllText(
			$categoriesPath,
			"class C {`n`tpublic const string Widget = `"Widget`";`n`tpublic const string Widget1 = `"Widget1`";`n}`n")
		[IO.File]::WriteAllText(
			$pipelinePath,
			"categoryGroupsToTest:`r`n  - 'Alpha'`r`n  - 'Widget'`r`n  - 'Omega'`r`n")
		[IO.File]::WriteAllText(
			$analyzerPath,
			"private static readonly ImmutableArray<string> CiShardCategoryPrefixes =`n`tImmutableArray.Create(`"CollectionView`");`n")

		Set-ShardInfrastructure -Category Widget -ShardCount 3 `
			-CategoriesPath $categoriesPath -PipelinePath $pipelinePath -AnalyzerPath $analyzerPath

		$categories = [IO.File]::ReadAllText($categoriesPath)
		$pipeline = [IO.File]::ReadAllText($pipelinePath)
		$analyzer = [IO.File]::ReadAllText($analyzerPath)
		$categories | Should -Not -Match "`r`n"
		([regex]::Matches($categories, 'public const string Widget\d')).Count | Should -Be 3
		([regex]::Matches($pipeline, "(?m)^\s*-\s*'Widget\d'\s*$")).Count | Should -Be 3
		$pipeline | Should -Not -Match "(?m)^\s*-\s*'Widget'\s*$"
		$pipeline.IndexOf("- 'Widget1'") | Should -BeGreaterThan $pipeline.IndexOf("- 'Alpha'")
		$pipeline.IndexOf("- 'Widget3'") | Should -BeLessThan $pipeline.IndexOf("- 'Omega'")
		$pipeline | Should -Match 'Widget1-3 shard categories'
		$pipeline | Should -Not -Match "`r`r`n"
		$analyzer | Should -Match 'ImmutableArray\.Create\("CollectionView", "Widget"\)'
	}

	It 'removes an umbrella from a comma group while retaining adjacent members and prior prefixes' {
		$categoriesPath = Join-Path $TestDrive 'UITestCategories2.cs'
		$pipelinePath = Join-Path $TestDrive 'ui-tests2.yml'
		$analyzerPath = Join-Path $TestDrive 'Analyzer2.cs'
		Set-Content $categoriesPath "class C {`n`tpublic const string Gadget = `"Gadget`";`n}`n" -NoNewline
		Set-Content $pipelinePath "categoryGroupsToTest:`n  - 'Alpha,Gadget,Omega'`n  - 'Tail'`n" -NoNewline
		Set-Content $analyzerPath @'
private static readonly ImmutableArray<string> CiShardCategoryPrefixes =
	ImmutableArray.Create("CollectionView", "Widget");
'@ -NoNewline

		Set-ShardInfrastructure -Category Gadget -ShardCount 2 `
			-CategoriesPath $categoriesPath -PipelinePath $pipelinePath -AnalyzerPath $analyzerPath

		$pipeline = Get-Content $pipelinePath -Raw
		$pipeline | Should -Match "(?m)^\s*-\s*'Alpha,Omega'\s*$"
		$pipeline | Should -Not -Match 'Alpha,Gadget'
		$pipeline.IndexOf("- 'Gadget1'") | Should -BeGreaterThan $pipeline.IndexOf("- 'Alpha,Omega'")
		$pipeline.IndexOf("- 'Gadget2'") | Should -BeLessThan $pipeline.IndexOf("- 'Tail'")
		(Get-Content $analyzerPath -Raw) | Should -Match (
			'ImmutableArray\.Create\("CollectionView", "Gadget", "Widget"\)')
	}

	It 'validates exact plan coverage and rejects stale shard numbers' {
		$root = Join-Path $TestDrive 'validation'
		New-Item -ItemType Directory $root | Out-Null
		$path = Join-Path $root 'Tests.cs'
		@'
namespace N;
public class Tests
{
	[Test]
	[Category(UITestCategories.Widget)]
	[Category(UITestCategories.Widget1)]
	public void A() { }
}
'@ | Set-Content $path
		$plan = [pscustomobject]@{
			category = 'Widget'
			shardCount = 1
			assignments = @([pscustomobject]@{ testId = 'N.Tests.A'; shard = 'Widget1' })
		}

		{ Test-UITestShardApplication -TestRoot $root -Category Widget -Plan $plan } | Should -Not -Throw
		(Get-Content $path -Raw).Replace('Widget1', 'Widget2') | Set-Content $path
		{ Test-UITestShardApplication -TestRoot $root -Category Widget -Plan $plan } |
			Should -Throw '*planned method-level shard*'
	}
}
