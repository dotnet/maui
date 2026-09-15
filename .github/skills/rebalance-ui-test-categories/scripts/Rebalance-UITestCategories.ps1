[CmdletBinding()]
param(
	[ValidateSet('Gather', 'Plan', 'Apply', 'All')]
	[string]$Mode = 'All',
	[Parameter(Mandatory)]
	[string]$Category,
	[ValidateRange(1, 240)]
	[double]$TargetMinutes = 60,
	[ValidateRange(0, 60)]
	[double]$SafetyMarginMinutes = 2,
	[string[]]$BuildId,
	[ValidateRange(0, 100)]
	[int]$RecentBuildCount = 0,
	[string]$EvidencePath,
	[string]$PlanPath,
	[string]$OutputPath,
	[ValidateRange(1, 32)]
	[int]$MaxShards = 12,
	[ValidateRange(1, 32)]
	[int]$MinimumShards = 1,
	[ValidateSet('Fail', 'ClassPlatformMax')]
	[string]$UnmeasuredTestPolicy = 'Fail',
	[switch]$Apply,
	[string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Import-Module (Join-Path $PSScriptRoot 'Rebalance-UITestCategories.psm1') -Force

if (-not $RepositoryRoot) {
	$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..' '..')).Path
}
$testRoot = Join-Path $RepositoryRoot 'src/Controls/tests/TestCases.Shared.Tests'
$categoriesPath = Join-Path $testRoot 'UITestCategories.cs'
$pipelinePath = Join-Path $RepositoryRoot 'eng/pipelines/common/ui-tests.yml'
$analyzerPath = Join-Path $RepositoryRoot 'src/TestUtils/src/UITest.Analyzers/NUnit/NUnitTestMissingCategoryAnalyzer.cs'

function Write-JsonFile {
	param([Parameter(Mandatory)]$Value, [Parameter(Mandatory)][string]$Path)
	$parent = Split-Path $Path -Parent
	if ($parent) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
	$Value | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $Path -Encoding utf8NoBOM
}

$evidence = $null
$plan = $null

if ($Mode -in @('Gather', 'Plan', 'All') -and -not $OutputPath) {
	throw "$Mode mode requires -OutputPath."
}

if ($Mode -in @('Gather', 'All')) {
	$parsedBuildIds = @($BuildId | ForEach-Object { $_ -split ',' } | Where-Object { $_ } | ForEach-Object { [int]$_ })
	$evidence = Get-HistoricalEvidence -Category $Category -TestRoot $testRoot `
		-BuildId $parsedBuildIds -RecentBuildCount $RecentBuildCount
	$evidenceOutput = if ($Mode -eq 'Gather') { $OutputPath } elseif ($EvidencePath) { $EvidencePath } else {
		[System.IO.Path]::ChangeExtension($OutputPath, '.evidence.json')
	}
	Write-JsonFile -Value $evidence -Path $evidenceOutput
	Write-Host "Gathered $(@($evidence.samples).Count) timing samples from build(s) $($evidence.source.buildIds -join ', ') -> $evidenceOutput"
}

if ($Mode -in @('Plan', 'All')) {
	if (-not $evidence) {
		if (-not $EvidencePath) {
			throw 'Plan mode requires -EvidencePath. Source-count fallback is intentionally unsupported.'
		}
		$evidence = Get-Content -LiteralPath $EvidencePath -Raw | ConvertFrom-Json
	}
	$inventory = @(Get-UITestInventory -TestRoot $testRoot -Category $Category)
	foreach ($test in $inventory) {
		$test.RelativeFile = [System.IO.Path]::GetRelativePath($testRoot, $test.File)
	}
	$plan = New-UITestShardPlan -Category $Category -Inventory $inventory -Evidence $evidence `
		-TargetMinutes $TargetMinutes -SafetyMarginMinutes $SafetyMarginMinutes `
		-MaxShards $MaxShards -MinimumShards $MinimumShards -UnmeasuredTestPolicy $UnmeasuredTestPolicy
	Write-JsonFile -Value $plan -Path $OutputPath
	Write-Host "Planned $(@($plan.assignments).Count) tests across $($plan.shardCount) shards; worst projection $($plan.maxProjectedMinutes)m -> $OutputPath"
}

if ($Mode -eq 'Apply') {
	if (-not $PlanPath) { throw 'Apply mode requires -PlanPath.' }
	$plan = Get-Content -LiteralPath $PlanPath -Raw | ConvertFrom-Json
}

if ($Mode -in @('Apply', 'All')) {
	if (-not $Apply) {
		throw 'Applying source changes requires the explicit -Apply switch.'
	}
	if (-not $plan) { throw 'No plan is available to apply.' }
	Test-UITestShardPlan -TestRoot $testRoot -Category $Category -Plan $plan
	Set-UITestShardCategories -TestRoot $testRoot -Category $Category -Assignments @($plan.assignments)
	Set-ShardInfrastructure -Category $Category -ShardCount $plan.shardCount `
		-CategoriesPath $categoriesPath -PipelinePath $pipelinePath -AnalyzerPath $analyzerPath
	Test-UITestShardApplication -TestRoot $testRoot -Category $Category -Plan $plan
	Write-Host "Applied method-level $Category shard categories and $($plan.shardCount) ordinary matrix entries."
}
