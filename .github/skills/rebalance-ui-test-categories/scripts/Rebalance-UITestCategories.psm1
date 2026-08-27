Set-StrictMode -Version Latest

function Get-Percentile {
	param(
		[Parameter(Mandatory)][double[]]$Values,
		[ValidateRange(0, 1)][double]$Percentile = 0.8
	)

	if ($Values.Count -eq 0) {
		throw 'Cannot calculate a percentile for an empty set.'
	}

	$sorted = @($Values | Sort-Object)
	$index = [Math]::Max(0, [Math]::Ceiling($Percentile * $sorted.Count) - 1)
	return [double]$sorted[$index]
}

function Get-UITestInventory {
	param(
		[Parameter(Mandatory)][string]$TestRoot,
		[Parameter(Mandatory)][string]$Category
	)

	$escapedCategory = [regex]::Escape($Category)
	$umbrellaPattern = "Category\s*\(\s*UITestCategories\.$escapedCategory\s*\)"
	$shardedPattern = "ShardedTestCategory(?:Attribute)?\s*\(\s*UITestCategories\.$escapedCategory" +
		"(?:\s*,\s*(?:shard\s*:\s*)?(?<shard>\d+))?\s*\)"
	$methodPattern = '^\s*(?:(?:public|internal|private|protected|static|virtual|override|sealed|async|new)\s+)*[\w.]+(?:<[^>]+>)?(?:\[\])?\s+(?<method>[A-Za-z_]\w*)\s*\('
	$namespacePattern = '^\s*namespace\s+(?<namespace>[\w.]+)'
	$classPattern = '^\s*(?:(?:public|internal|private|protected|sealed|abstract|static|partial)\s+)*class\s+(?<class>[A-Za-z_]\w*)'
	$items = [System.Collections.Generic.List[object]]::new()

	foreach ($file in Get-ChildItem -LiteralPath $TestRoot -Recurse -File -Filter '*.cs' | Sort-Object FullName) {
		$lines = [System.IO.File]::ReadAllLines($file.FullName)
		$namespace = ''
		$className = ''
		$classHasUmbrella = $false
		$disabledStack = [System.Collections.Generic.List[bool]]::new()

		for ($i = 0; $i -lt $lines.Count; $i++) {
			$directive = $lines[$i].Trim()
			if ($directive -match '^#if\s+(?<condition>.+)$') {
				$condition = $Matches['condition']
				$disabledHere = @(
					'TEST_FAILS_ON_ANDROID',
					'TEST_FAILS_ON_CATALYST',
					'TEST_FAILS_ON_IOS',
					'TEST_FAILS_ON_WINDOWS'
				) | ForEach-Object { $condition.Contains($_) }
				$disabledStack.Add(-not ($disabledHere -contains $false))
				continue
			}
			if ($directive -match '^#(?:else|elif)\b' -and $disabledStack.Count -gt 0) {
				$disabledStack[$disabledStack.Count - 1] = $false
				continue
			}
			if ($directive -match '^#endif\b' -and $disabledStack.Count -gt 0) {
				$disabledStack.RemoveAt($disabledStack.Count - 1)
				continue
			}

			$namespaceMatch = [regex]::Match($lines[$i], $namespacePattern)
			if ($namespaceMatch.Success) {
				$namespace = $namespaceMatch.Groups['namespace'].Value
			}

			$classMatch = [regex]::Match($lines[$i], $classPattern)
			if ($classMatch.Success) {
				$className = $classMatch.Groups['class'].Value
				$classAttributeStart = $i
				while ($classAttributeStart -gt 0 -and (
					[string]::IsNullOrWhiteSpace($lines[$classAttributeStart - 1]) -or
					$lines[$classAttributeStart - 1].TrimStart().StartsWith('['))) {
					$classAttributeStart--
				}
				$classAttributeText = if ($classAttributeStart -lt $i) {
					$lines[$classAttributeStart..($i - 1)] -join "`n"
				} else { '' }
				$classHasUmbrella = $classAttributeText -match $umbrellaPattern -or
					$classAttributeText -match $shardedPattern
			}

			$methodMatch = [regex]::Match($lines[$i], $methodPattern)
			if (-not $methodMatch.Success -or -not $className) {
				continue
			}

			$attributeStart = $i
			while ($attributeStart -gt 0 -and (
				[string]::IsNullOrWhiteSpace($lines[$attributeStart - 1]) -or
				$lines[$attributeStart - 1].TrimStart().StartsWith('['))) {
				$attributeStart--
			}
			$attributeText = ($lines[$attributeStart..($i - 1)] -join "`n")
			$methodHasUmbrella = $attributeText -match $umbrellaPattern -or
				$attributeText -match $shardedPattern
			$isTest = $attributeText -match '\[\s*Test(?:Case(?:Source)?)?(?:\s*[\],(])'
			if ((-not $methodHasUmbrella -and -not $classHasUmbrella) -or -not $isTest) {
				continue
			}

			$methodName = $methodMatch.Groups['method'].Value
			$fullClassName = if ($namespace) { "$namespace.$className" } else { $className }
			$legacyShardMatches = [regex]::Matches(
				$attributeText,
				"Category\s*\(\s*UITestCategories\.(?<shard>$escapedCategory\d+)\s*\)")
			$shardedMatches = [regex]::Matches($attributeText, $shardedPattern)
			$shards = @(
				$legacyShardMatches | ForEach-Object { $_.Groups['shard'].Value }
				$shardedMatches | ForEach-Object {
					$number = if ($_.Groups['shard'].Success) { $_.Groups['shard'].Value } else { '1' }
					"$Category$number"
				}
			)

			$items.Add([pscustomobject]@{
				Id = "$fullClassName.$methodName"
				ClassName = $className
				FullClassName = $fullClassName
				MethodName = $methodName
				File = $file.FullName
				RelativeFile = $null
				MethodLine = $i + 1
				AttributeStartLine = $attributeStart + 1
				UmbrellaLine = if ($methodHasUmbrella) {
					(@($attributeStart..($i - 1) | Where-Object {
						$lines[$_] -match $umbrellaPattern -or $lines[$_] -match $shardedPattern
					}) | Select-Object -Last 1) + 1
				} else {
					$i
				}
				Shard = if ($shards.Count -eq 1) { $shards[0] } else { $null }
				ShardCount = $shards.Count
				Ordered = $attributeText -match '\bOrder\s*\('
				ClassShardCount = (
					[regex]::Matches(
						$classAttributeText,
						"Category\s*\(\s*UITestCategories\.$escapedCategory\d+\s*\)").Count +
					[regex]::Matches($classAttributeText, $shardedPattern).Count)
				DisabledAllPlatforms = $disabledStack -contains $true
			})
		}
	}

	return @($items)
}

function Resolve-ResultMethod {
	param(
		[Parameter(Mandatory)][string]$Title,
		[Parameter(Mandatory)][object[]]$Candidates
	)

	$matches = @($Candidates | Where-Object {
		$Title -eq $_.MethodName -or
		$Title.StartsWith("$($_.MethodName)(") -or
		$Title.StartsWith("$($_.MethodName) ")
	} | Sort-Object @{ Expression = { $_.MethodName.Length }; Descending = $true }, Id)

	if ($matches.Count -eq 1) {
		return $matches[0]
	}

	return $null
}

function Get-PlatformName {
	param([Parameter(Mandatory)]$Run)

	$stage = [string]$Run.pipelineReference.stageReference.stageName
	$name = [string]$Run.name
	switch -Regex ($stage) {
		'^android_ui_tests_coreclr$' { return 'AndroidCoreCLR' }
		'^android_ui_tests$' { return 'AndroidMono' }
		'^winui_ui_tests$' { return 'WinUI' }
		'^mac_ui_tests$' { return 'MacCatalyst' }
		'^ios_ui_tests_mono' {
			if ($name -match '_18[._]5$') { return 'iOS18.5' }
			return 'iOSLatest'
		}
		default { return $stage }
	}
}

function Test-IsConfigurationStageRun {
	param([Parameter(Mandatory)]$Run)

	$stage = [string]$Run.pipelineReference.stageReference.stageName
	return $stage -match '_(?:cv|carv)\d+$'
}

function Get-OrdinaryRunKind {
	param(
		[Parameter(Mandatory)]$Run,
		[Parameter(Mandatory)][string]$Category
	)

	$job = [string]$Run.pipelineReference.jobReference.jobName
	if ($job -match "^$([regex]::Escape($Category))_?\d+$") {
		return 'CategoryMatrixShard'
	}
	return 'UmbrellaBaseline'
}

function Get-ResultAutomatedName {
	param([Parameter(Mandatory)]$Result)

	$name = if ($Result.PSObject.Properties['automatedTestName']) {
		[string]$Result.automatedTestName
	} else { '' }
	if ([string]::IsNullOrWhiteSpace($name) -and $Result.PSObject.Properties['testCaseTitle']) {
		$name = [string]$Result.testCaseTitle
	}
	return $name.Trim()
}

function Resolve-ResultClass {
	param(
		[Parameter(Mandatory)][string]$AutomatedName,
		[Parameter(Mandatory)]$ByClass
	)

	$normalized = $AutomatedName
	if ($normalized.Contains('(')) {
		$normalized = $normalized.Substring(0, $normalized.IndexOf('('))
	}
	if ($ByClass.ContainsKey($normalized)) {
		return $normalized
	}

	$match = @($ByClass.Keys | Where-Object {
		$normalized.StartsWith("$_", [StringComparison]::Ordinal) -and
			($normalized.Length -eq $_.Length -or $normalized[$_.Length] -in '.', '(')
	} | Sort-Object Length -Descending | Select-Object -First 1)
	if ($match.Count -eq 1) { return $match[0] }
	return $null
}

function Invoke-AzDevOpsJson {
	param([Parameter(Mandatory)][string[]]$Arguments)

	$output = & az devops invoke @Arguments 2>&1
	if ($LASTEXITCODE -ne 0) {
		throw "Azure DevOps query failed: $($output -join [Environment]::NewLine)"
	}

	try {
		return ($output -join [Environment]::NewLine) | ConvertFrom-Json
	}
	catch {
		throw "Azure DevOps returned invalid JSON: $($output | Select-Object -First 5)"
	}
}

function Get-RecentBuildIds {
	param(
		[Parameter(Mandatory)][int]$Count,
		[string]$Organization = 'https://dev.azure.com/dnceng-public',
		[string]$Project = 'public',
		[int]$DefinitionId = 313
	)

	$uri = "$Organization/$Project/_apis/build/builds?definitions=$DefinitionId&statusFilter=completed&queryOrder=finishTimeDescending&`$top=$Count&api-version=7.1"
	$response = Invoke-RestMethod -Uri $uri
	return @($response.value | Select-Object -ExpandProperty id)
}

function Get-HistoricalEvidence {
	param(
		[Parameter(Mandatory)][string]$Category,
		[Parameter(Mandatory)][string]$TestRoot,
		[int[]]$BuildId,
		[int]$RecentBuildCount,
		[string]$Organization = 'https://dev.azure.com/dnceng-public',
		[string]$Project = 'public'
	)

	if ((-not $BuildId -or $BuildId.Count -eq 0) -and $RecentBuildCount -le 0) {
		throw 'Historical Azure evidence is required. Supply -BuildId or -RecentBuildCount.'
	}
	if (-not $BuildId -or $BuildId.Count -eq 0) {
		$BuildId = Get-RecentBuildIds -Count $RecentBuildCount -Organization $Organization -Project $Project
	}

	$inventory = @(Get-UITestInventory -TestRoot $TestRoot -Category $Category)
	if ($inventory.Count -eq 0) {
		throw "No method-level $Category umbrella categories were found under $TestRoot."
	}
	$byClass = $inventory | Group-Object FullClassName -AsHashTable -AsString
	$samples = [System.Collections.Generic.List[object]]::new()
	$runRecords = @{}
	$unmatched = [System.Collections.Generic.List[object]]::new()
	$jobDurations = [System.Collections.Generic.List[object]]::new()
	$configurationSamples = [System.Collections.Generic.List[object]]::new()
	$configurationJobDurations = [System.Collections.Generic.List[object]]::new()

	foreach ($build in $BuildId) {
		$resultValues = [System.Collections.Generic.List[object]]::new()
		$continuationToken = $null
		do {
			$queryParameters = @("buildId=$build", '$top=20000')
			if ($continuationToken) {
				$queryParameters += "continuationToken=$continuationToken"
			}
			$arguments = @(
				'--org', $Organization, '--area', 'testresults', '--resource', 'resultsbybuild',
				'--route-parameters', "project=$Project", '--query-parameters'
			) + $queryParameters + @(
				'--api-version', '7.1-preview', '--only-show-errors', '-o', 'json'
			)
			$results = Invoke-AzDevOpsJson -Arguments $arguments
			foreach ($result in $(if ($results.value) { @($results.value) } else { @($results) })) {
				$resultValues.Add($result)
			}
			$continuationToken = [string]$results.continuation_token
		} while ($continuationToken)
		$relevant = [System.Collections.Generic.List[object]]::new()
		$candidateRunIds = [System.Collections.Generic.HashSet[int]]::new()

		foreach ($result in $resultValues) {
			if (-not $result.PSObject.Properties['runId']) {
				continue
			}
			$automatedName = Get-ResultAutomatedName -Result $result
			if (-not $automatedName -or -not (Resolve-ResultClass -AutomatedName $automatedName -ByClass $byClass)) {
				continue
			}
			$null = $candidateRunIds.Add([int]$result.runId)
		}

		foreach ($runId in @($candidateRunIds | Sort-Object)) {
			$run = Invoke-AzDevOpsJson -Arguments @(
				'--org', $Organization, '--area', 'testresults', '--resource', 'runs',
				'--route-parameters', "project=$Project", "runId=$runId",
				'--api-version', '7.1-preview', '--only-show-errors', '-o', 'json'
			)
			$runRecords["$build/$runId"] = $run

			$runResults = Invoke-AzDevOpsJson -Arguments @(
				'--org', $Organization, '--area', 'testresults', '--resource', 'results',
				'--route-parameters', "project=$Project", "runId=$runId",
				'--query-parameters', '$top=10000',
				'--api-version', '7.1-preview', '--only-show-errors', '-o', 'json'
			)
			foreach ($result in @($runResults.value)) {
				if (-not $result.PSObject.Properties['testCaseTitle'] -or
					-not $result.PSObject.Properties['durationInMs'] -or
					-not $result.PSObject.Properties['outcome']) {
					continue
				}
				$automatedName = Get-ResultAutomatedName -Result $result
				$class = Resolve-ResultClass -AutomatedName $automatedName -ByClass $byClass
				if (-not $class) { continue }
				$method = Resolve-ResultMethod -Title ([string]$result.testCaseTitle) -Candidates @($byClass[$class])
				if (-not $method) {
					$unmatched.Add([pscustomobject]@{
						BuildId = $build
						RunId = $runId
						ClassName = $class
						Title = [string]$result.testCaseTitle
					})
					continue
				}
				if ([double]$result.durationInMs -le 0 -or [string]$result.outcome -notin @('Passed', 'Failed')) {
					continue
				}
				$relevant.Add([pscustomobject]@{
					BuildId = $build
					RunId = $runId
					TestId = $method.Id
					ClassName = $method.FullClassName
					TestName = $method.MethodName
					File = $method.File
					DurationMinutes = [double]$result.durationInMs / 60000.0
					Outcome = [string]$result.outcome
				})
			}
		}

		foreach ($group in $relevant | Group-Object RunId, TestId) {
			$first = $group.Group[0]
			$run = $runRecords["$build/$($first.RunId)"]
			$sample = [pscustomobject]@{
				BuildId = $build
				RunId = $first.RunId
				Platform = Get-PlatformName -Run $run
				TestId = $first.TestId
				ClassName = $first.ClassName
				TestName = $first.TestName
				File = $first.File
				DurationMinutes = [Math]::Round([double](($group.Group.DurationMinutes | Measure-Object -Sum).Sum), 6)
				Outcome = if ('Failed' -in $group.Group.Outcome) { 'Failed' } else { 'Passed' }
				CaseCount = $group.Count
			}
			if (Test-IsConfigurationStageRun -Run $run) {
				$configurationSamples.Add($sample)
			} else {
				$samples.Add($sample)
			}
		}

		$timeline = Invoke-RestMethod -Uri "$Organization/$Project/_apis/build/builds/$build/timeline?api-version=7.1"
		foreach ($runId in @($candidateRunIds | Sort-Object)) {
			$run = $runRecords["$build/$runId"]
			$stage = [string]$run.pipelineReference.stageReference.stageName
			$job = [string]$run.pipelineReference.jobReference.jobName
			$record = @($timeline.records | Where-Object {
				$_.type -eq 'Job' -and $_.identifier -eq "$stage.$([string]$run.pipelineReference.phaseReference.phaseName).$job"
			} | Select-Object -First 1)
			if ($record.Count -eq 0) {
				continue
			}
			$wallMinutes = (([datetime]$record[0].finishTime) - ([datetime]$record[0].startTime)).TotalMinutes
			$testRunWallMinutes = if ($run.startedDate -and $run.completedDate) {
				(([datetime]$run.completedDate) - ([datetime]$run.startedDate)).TotalMinutes
			} else { $null }
			$isConfigurationStage = Test-IsConfigurationStageRun -Run $run
			$sampleSource = if ($isConfigurationStage) { $configurationSamples } else { $samples }
			$runSamples = @($sampleSource | Where-Object { $_.BuildId -eq $build -and $_.RunId -eq $runId })
			$testMinutes = if ($runSamples.Count -eq 0) {
				continue
			} else {
				[double](($runSamples | Measure-Object DurationMinutes -Sum).Sum)
			}
			$duration = [pscustomobject]@{
				BuildId = $build
				RunId = $runId
				Platform = Get-PlatformName -Run $run
				Result = [string]$record[0].result
				WallMinutes = [Math]::Round($wallMinutes, 6)
				TestRunWallMinutes = if ($null -eq $testRunWallMinutes) { $null } else {
					[Math]::Round($testRunWallMinutes, 6)
				}
				TestMinutes = [Math]::Round($testMinutes, 6)
				OverheadMinutes = [Math]::Round(
					[Math]::Max([double]0, $wallMinutes - $(if ($null -eq $testRunWallMinutes) {
						$testMinutes
					} else {
						$testRunWallMinutes
					})), 6)
				RunKind = if ($isConfigurationStage) {
					'ConfigurationStage'
				} else {
					Get-OrdinaryRunKind -Run $run -Category $Category
				}
			}
			if ($isConfigurationStage) {
				$configurationJobDurations.Add($duration)
			} else {
				$jobDurations.Add($duration)
			}
		}
	}

	if ($samples.Count -eq 0) {
		throw "Azure returned no valid $Category test timing samples for build(s): $($BuildId -join ', ')."
	}

	$overhead = [ordered]@{}
	foreach ($platformGroup in $jobDurations | Group-Object Platform | Sort-Object Name) {
		$shardRuns = @($platformGroup.Group | Where-Object RunKind -eq 'CategoryMatrixShard')
		$population = if ($shardRuns.Count -gt 0) { $shardRuns } else { @($platformGroup.Group) }
		$successful = @($population | Where-Object Result -eq 'succeeded')
		if ($successful.Count -eq 0) {
			$successful = @($population)
		}
		$overhead[$platformGroup.Name] = [Math]::Round(
			(Get-Percentile -Values @($successful.OverheadMinutes) -Percentile 0.8), 6)
	}

	return [pscustomobject]@{
		schemaVersion = 1
		category = $Category
		source = [pscustomobject]@{
			type = 'AzureDevOps'
			organization = $Organization
			project = $Project
			definitionId = 313
			buildIds = @($BuildId)
			statistic = 'Per-test/platform p80 of ordinary category-matrix run totals; p80 nearest-rank successful observed overhead per platform'
			fixedOverheadPopulation = 'Successful CategoryMatrixShard jobs when available; otherwise successful UmbrellaBaseline jobs. Overhead is job wall time minus test-run wall time.'
			configurationStagePolicy = 'Dedicated testConfigurationArgs stages are recorded separately and excluded from ordinary category-matrix projections.'
		}
		samples = @($samples)
		jobDurations = @($jobDurations)
		configurationStageSamples = @($configurationSamples)
		configurationStageJobDurations = @($configurationJobDurations)
		fixedOverheadMinutes = $overhead
		unmatchedResults = @($unmatched)
	}
}

function Get-AggregatedWeights {
	param([Parameter(Mandatory)]$Evidence)

	$weights = @{}
	foreach ($group in $Evidence.samples | Group-Object TestId, Platform) {
		$first = $group.Group[0]
		if (-not $weights.ContainsKey($first.TestId)) {
			$weights[$first.TestId] = [ordered]@{}
		}
		$weights[$first.TestId][$first.Platform] = [Math]::Round(
			(Get-Percentile -Values @($group.Group.DurationMinutes) -Percentile 0.8), 6)
	}
	return $weights
}

function New-UITestShardPlan {
	param(
		[Parameter(Mandatory)][string]$Category,
		[Parameter(Mandatory)][object[]]$Inventory,
		[Parameter(Mandatory)]$Evidence,
		[double]$TargetMinutes = 60,
		[double]$SafetyMarginMinutes = 2,
		[int]$MaxShards = 12,
		[int]$MinimumShards = 1,
		[ValidateSet('Fail', 'ClassPlatformMax')]
		[string]$UnmeasuredTestPolicy = 'Fail'
	)

	if ($Evidence.category -ne $Category) {
		throw "Evidence category '$($Evidence.category)' does not match '$Category'."
	}
	if ($SafetyMarginMinutes -lt 0 -or $SafetyMarginMinutes -ge $TargetMinutes) {
		throw 'SafetyMarginMinutes must be non-negative and less than TargetMinutes.'
	}
	$planningLimit = $TargetMinutes - $SafetyMarginMinutes
	$weights = Get-AggregatedWeights -Evidence $Evidence
	$missing = @($Inventory | Where-Object {
		-not $weights.ContainsKey($_.Id) -and -not $_.DisabledAllPlatforms
	})
	if ($missing.Count -gt 0 -and $UnmeasuredTestPolicy -eq 'Fail') {
		throw "Historical evidence is missing for $($missing.Count) test method(s), including: $($missing[0].Id)."
	}
	$imputedIds = [System.Collections.Generic.HashSet[string]]::new()
	if ($missing.Count -gt 0) {
		foreach ($test in $missing) {
			$weights[$test.Id] = [ordered]@{}
			foreach ($platform in @($Evidence.samples.Platform | Sort-Object -Unique)) {
				$classValues = @(
					$Inventory | Where-Object {
						$_.FullClassName -eq $test.FullClassName -and $weights.ContainsKey($_.Id) -and
						$weights[$_.Id].Contains($platform)
					} | ForEach-Object { [double]$weights[$_.Id][$platform] }
				)
				$values = @(if ($classValues.Count -gt 0) {
					$classValues
				} else {
					$weights.Values | Where-Object { $_.Contains($platform) } | ForEach-Object { [double]$_[$platform] }
				})
				if ($values.Count -gt 0) {
					$weights[$test.Id][$platform] = [double](($values | Measure-Object -Maximum).Maximum)
				}
			}
			$null = $imputedIds.Add($test.Id)
		}
	}
	foreach ($disabled in $Inventory | Where-Object DisabledAllPlatforms) {
		if (-not $weights.ContainsKey($disabled.Id)) {
			$weights[$disabled.Id] = [ordered]@{}
		}
	}
	$platforms = @($Evidence.samples.Platform | Sort-Object -Unique)
	$overhead = @{}
	foreach ($platform in $platforms) {
		$value = $Evidence.fixedOverheadMinutes.$platform
		$overhead[$platform] = if ($null -eq $value) { 0.0 } else { [double]$value }
	}

	$cohesiveClasses = @(
		$Inventory | Group-Object FullClassName | Where-Object {
			@($_.Group | Where-Object {
				$_.PSObject.Properties['Ordered'] -and $_.Ordered
			}).Count -gt 0
		} | ForEach-Object Name
	)
	$planningGroups = @(
		$Inventory | Group-Object {
			if ($_.FullClassName -in $cohesiveClasses) { $_.FullClassName } else { $_.Id }
		} | ForEach-Object {
			[pscustomobject]@{
				Id = $_.Name
				Tests = @($_.Group)
			}
		}
	)
	$planningGroupWeights = @{}
	foreach ($group in $planningGroups) {
		$planningGroupWeights[$group.Id] = [ordered]@{}
		$isCohesive = $group.Tests.Count -gt 1 -and $group.Id -in $cohesiveClasses
		foreach ($platform in $platforms) {
			if ($isCohesive) {
				$testIds = @($group.Tests.Id)
				$runTotals = @(
					$Evidence.samples | Where-Object {
						$_.Platform -eq $platform -and $_.TestId -in $testIds
					} | Group-Object BuildId, RunId | ForEach-Object {
						[double](($_.Group.DurationMinutes | Measure-Object -Sum).Sum)
					}
				)
				if ($runTotals.Count -gt 0) {
					$planningGroupWeights[$group.Id][$platform] =
						Get-Percentile -Values $runTotals -Percentile 0.8
				}
			}
			else {
				$test = $group.Tests[0]
				if ($weights[$test.Id].Contains($platform)) {
					$planningGroupWeights[$group.Id][$platform] = [double]$weights[$test.Id][$platform]
				}
			}
		}
	}

	foreach ($group in $planningGroups) {
		foreach ($platform in $platforms) {
			$duration = if ($planningGroupWeights[$group.Id].Contains($platform)) {
				[double]$planningGroupWeights[$group.Id][$platform]
			} else { 0.0 }
			if ($duration + $overhead[$platform] -ge $planningLimit) {
				throw "Target $TargetMinutes minutes with a $SafetyMarginMinutes minute margin is unattainable: $($group.Id) projects to $([Math]::Round($duration + $overhead[$platform], 2)) minutes on $platform."
			}
		}
	}

	$orderedGroups = @($planningGroups | Sort-Object @{
		Expression = {
			$planningGroup = $_
			[double](($platforms | ForEach-Object {
				$platform = $_
				if ($planningGroupWeights[$planningGroup.Id].Contains($platform)) {
					[double]$planningGroupWeights[$planningGroup.Id][$platform]
				} else { 0.0 }
			} | Measure-Object -Maximum).Maximum)
		}
		Descending = $true
	}, Id)

	$selected = $null
	if ($MinimumShards -gt $MaxShards) {
		throw "MinimumShards ($MinimumShards) cannot exceed MaxShards ($MaxShards)."
	}
	for ($shardCount = $MinimumShards; $shardCount -le $MaxShards; $shardCount++) {
		$loads = @()
		for ($i = 0; $i -lt $shardCount; $i++) {
			$entry = [ordered]@{}
			foreach ($platform in $platforms) { $entry[$platform] = 0.0 }
			$loads += ,$entry
		}
		$assignment = @{}

		foreach ($group in $orderedGroups) {
			$candidates = for ($i = 0; $i -lt $shardCount; $i++) {
				$worst = 0.0
				$total = 0.0
				foreach ($platform in $platforms) {
					$groupDuration = if ($planningGroupWeights[$group.Id].Contains($platform)) {
						[double]$planningGroupWeights[$group.Id][$platform]
					} else { 0.0 }
					$projected = $loads[$i][$platform] + $groupDuration + $overhead[$platform]
					$worst = [Math]::Max($worst, $projected)
					$total += $projected
				}
				[pscustomobject]@{ Index = $i; Worst = $worst; Total = $total }
			}
			$choice = $candidates | Sort-Object Worst, Total, Index | Select-Object -First 1
			foreach ($test in $group.Tests) {
				$assignment[$test.Id] = $choice.Index
			}
			foreach ($platform in $platforms) {
				if ($planningGroupWeights[$group.Id].Contains($platform)) {
					$loads[$choice.Index][$platform] += [double]$planningGroupWeights[$group.Id][$platform]
				}
			}
		}

		$maxProjected = 0.0
		foreach ($load in $loads) {
			foreach ($platform in $platforms) {
				$maxProjected = [Math]::Max($maxProjected, $load[$platform] + $overhead[$platform])
			}
		}
		if ($maxProjected -lt $planningLimit) {
			$selected = [pscustomobject]@{
				ShardCount = $shardCount
				Loads = $loads
				Assignment = $assignment
				MaxProjected = $maxProjected
			}
			break
		}
	}

	if (-not $selected) {
		throw "No assignment with at most $MaxShards shards stays below the $planningLimit minute planning limit."
	}

	$projected = [ordered]@{}
	for ($i = 0; $i -lt $selected.ShardCount; $i++) {
		$shardName = "$Category$($i + 1)"
		$projected[$shardName] = [ordered]@{}
		foreach ($platform in $platforms) {
			$projected[$shardName][$platform] = [Math]::Round(
				$selected.Loads[$i][$platform] + $overhead[$platform], 3)
		}
	}

	$assignments = foreach ($test in $Inventory | Sort-Object Id) {
		$platformWeights = [ordered]@{}
		foreach ($platform in $platforms) {
			$platformWeights[$platform] = if ($weights[$test.Id].Contains($platform)) {
				[Math]::Round([double]$weights[$test.Id][$platform], 6)
			} else { 0.0 }
		}
		[pscustomobject]@{
			testId = $test.Id
			className = $test.FullClassName
			methodName = $test.MethodName
			file = $test.RelativeFile
			shard = "$Category$([int]$selected.Assignment[$test.Id] + 1)"
			platformMinutes = $platformWeights
			disabledAllPlatforms = [bool]$test.DisabledAllPlatforms
			fixtureCohesionRequired = $test.FullClassName -in $cohesiveClasses
			imputedFromHistoricalClassMaximum = $imputedIds.Contains($test.Id)
		}
	}
	$configurationSamplesForReport = @(if ($Evidence.PSObject.Properties['configurationStageSamples']) {
		@($Evidence.configurationStageSamples)
	} else { @() })
	$configurationJobsForReport = @(if ($Evidence.PSObject.Properties['configurationStageJobDurations']) {
		@($Evidence.configurationStageJobDurations)
	} else { @() })
	$configurationOverhead = [ordered]@{}
	foreach ($group in $configurationJobsForReport | Group-Object Platform | Sort-Object Name) {
		$successful = @($group.Group | Where-Object Result -eq 'succeeded')
		if ($successful.Count -eq 0) { $successful = @($group.Group) }
		$configurationOverhead[$group.Name] = [Math]::Round(
			(Get-Percentile -Values @($successful.OverheadMinutes) -Percentile 0.8), 6)
	}

	return [pscustomobject]@{
		schemaVersion = 1
		category = $Category
		targetMinutes = $TargetMinutes
		safetyMarginMinutes = $SafetyMarginMinutes
		planningLimitMinutes = $planningLimit
		shardCount = $selected.ShardCount
		maxProjectedMinutes = [Math]::Round($selected.MaxProjected, 3)
		platforms = $platforms
		fixedOverheadMinutes = $Evidence.fixedOverheadMinutes
		projectedShardMinutes = $projected
		assignments = @($assignments)
		evidence = $Evidence.source
		configurationStageEvidence = [pscustomobject]@{
			sampleCount = $configurationSamplesForReport.Count
			jobCount = $configurationJobsForReport.Count
			fixedOverheadMinutesP80 = $configurationOverhead
			projectionPolicy = 'Recorded separately and excluded from ordinary category-matrix projections.'
		}
		unmatchedEvidenceCount = @($Evidence.unmatchedResults).Count
		disabledUnmeasuredTestCount = @($Inventory | Where-Object {
			$_.DisabledAllPlatforms -and @($Evidence.samples.TestId) -notcontains $_.Id
		}).Count
		imputedTestCount = $imputedIds.Count
		limitations = @(
			'Projections use historical per-test p80 durations and measured job overhead; they are estimates, not guarantees.'
			"Assignments must project strictly below target minus the $SafetyMarginMinutes minute safety margin."
			'Dedicated testConfigurationArgs stages are retained as separate evidence and do not reduce ordinary category-matrix projections.'
			'Methods in fixtures containing NUnit Order attributes are assigned together so ordered setup and state remain available.'
			'Tests without samples on a platform contribute zero to that platform because they were not executed there.'
			'When explicitly enabled, unmeasured active tests use the maximum measured duration from their class/platform (or platform-wide maximum).'
		)
	}
}

function Remove-ShardCategoryFromLine {
	param(
		[Parameter(Mandatory)][AllowEmptyString()][string]$Line,
		[Parameter(Mandatory)][string]$Category
	)

	$token = "Category\s*\(\s*UITestCategories\.$([regex]::Escape($Category))\d+\s*\)"
	if ($Line -notmatch $token) { return $Line }
	if ($Line -match "^\s*\[\s*$token\s*\]\s*$") { return $null }

	$result = $Line
	$result = [regex]::Replace($result, "$token\s*,\s*", '')
	$result = [regex]::Replace($result, "\s*,\s*$token", '')
	$result = [regex]::Replace($result, $token, '')
	return $result
}

function Remove-UmbrellaCategoryFromLine {
	param(
		[Parameter(Mandatory)][AllowEmptyString()][string]$Line,
		[Parameter(Mandatory)][string]$Category
	)

	$token = "(?<!ShardedTest)Category\s*\(\s*UITestCategories\.$([regex]::Escape($Category))\s*\)"
	if ($Line -notmatch $token) { return $Line }
	if ($Line -match "^\s*\[\s*$token\s*\]\s*$") { return $null }

	$result = $Line
	$result = [regex]::Replace($result, "$token\s*,\s*", '')
	$result = [regex]::Replace($result, "\s*,\s*$token", '')
	return [regex]::Replace($result, $token, '')
}

function Set-UITestShardCategories {
	param(
		[Parameter(Mandatory)][string]$TestRoot,
		[Parameter(Mandatory)][string]$Category,
		[Parameter(Mandatory)][object[]]$Assignments
	)

	$assignmentById = @{}
	foreach ($assignment in $Assignments) { $assignmentById[$assignment.testId] = $assignment.shard }
	$escapedCategory = [regex]::Escape($Category)
	$files = @(
		$Assignments.file
		Get-ChildItem -LiteralPath $TestRoot -Recurse -File -Filter '*.cs' | Where-Object {
			[System.IO.File]::ReadAllText($_.FullName) -match
				"(?:Category\s*\(\s*UITestCategories\.$escapedCategory\d+\s*\)|" +
				"ShardedTestCategory(?:Attribute)?\s*\(\s*UITestCategories\.$escapedCategory)"
		} | ForEach-Object {
			[System.IO.Path]::GetRelativePath($TestRoot, $_.FullName)
		}
	) | Sort-Object -Unique
	foreach ($relativeFile in $files) {
		$path = Join-Path $TestRoot $relativeFile
		$originalBytes = [System.IO.File]::ReadAllBytes($path)
		$hasUtf8Bom = $originalBytes.Length -ge 3 -and
			$originalBytes[0] -eq 0xEF -and $originalBytes[1] -eq 0xBB -and $originalBytes[2] -eq 0xBF
		$encoding = [System.Text.UTF8Encoding]::new($hasUtf8Bom)
		$original = [System.IO.File]::ReadAllText($path)
		$newLine = if ($original.Contains("`r`n")) { "`r`n" } else { "`n" }
		$hasFinalNewLine = $original.EndsWith("`n")
		$lines = [System.Collections.Generic.List[string]]::new()
		foreach ($line in ($original -split '\r?\n')) { $lines.Add($line) }
		if ($hasFinalNewLine -and $lines.Count -gt 0 -and $lines[$lines.Count - 1] -eq '') {
			$lines.RemoveAt($lines.Count - 1)
		}

		$inventory = @(Get-UITestInventory -TestRoot $TestRoot -Category $Category |
			Where-Object { $_.File -eq $path })
		foreach ($test in $inventory | Sort-Object MethodLine -Descending) {
			if (-not $assignmentById.ContainsKey($test.Id)) {
				continue
			}

			$shardNumber = [string]$assignmentById[$test.Id] -replace "^$escapedCategory", ''
			$replacement = "ShardedTestCategory(UITestCategories.$Category, shard: $shardNumber)"
			$lineIndex = [int]$test.UmbrellaLine - 1
			$categoryToken = "(?<!ShardedTest)Category\s*\(\s*UITestCategories\.$escapedCategory\s*\)"
			$shardedToken = "ShardedTestCategory(?:Attribute)?\s*\(\s*UITestCategories\.$escapedCategory" +
				"(?:\s*,\s*(?:shard\s*:\s*)?\d+)?\s*\)"
			if ($lines[$lineIndex] -match $categoryToken) {
				$lines[$lineIndex] = [regex]::Replace($lines[$lineIndex], $categoryToken, $replacement)
			} elseif ($lines[$lineIndex] -match $shardedToken) {
				$lines[$lineIndex] = [regex]::Replace($lines[$lineIndex], $shardedToken, $replacement)
			} else {
				$methodIndex = [int]$test.MethodLine - 1
				$indent = [regex]::Match($lines[$methodIndex], '^\s*').Value
				$lines.Insert($methodIndex, "$indent[$replacement]")
			}
		}

		$cleanedLines = [System.Collections.Generic.List[string]]::new()
		foreach ($line in $lines) {
			$cleaned = Remove-ShardCategoryFromLine -Line $line -Category $Category
			if ($null -ne $cleaned) {
				$cleaned = Remove-UmbrellaCategoryFromLine -Line $cleaned -Category $Category
			}
			if ($null -ne $cleaned) { $cleanedLines.Add($cleaned) }
		}
		[System.IO.File]::WriteAllText(
			$path,
			($cleanedLines -join $newLine) + $(if ($hasFinalNewLine) { $newLine } else { '' }),
			$encoding)
	}
}

function Set-ShardInfrastructure {
	param(
		[Parameter(Mandatory)][string]$Category,
		[Parameter(Mandatory)][int]$ShardCount,
		[Parameter(Mandatory)][string]$CategoriesPath,
		[Parameter(Mandatory)][string]$PipelinePath,
		[Parameter(Mandatory)][string]$AnalyzerPath
	)

	$categoryBytes = [System.IO.File]::ReadAllBytes($CategoriesPath)
	$categoryEncoding = [System.Text.UTF8Encoding]::new(
		$categoryBytes.Length -ge 3 -and $categoryBytes[0] -eq 0xEF -and
		$categoryBytes[1] -eq 0xBB -and $categoryBytes[2] -eq 0xBF)
	$categories = [System.IO.File]::ReadAllText($CategoriesPath)
	$categoryNewLine = if ($categories.Contains("`r`n")) { "`r`n" } else { "`n" }
	$categories = [regex]::Replace(
		$categories,
		"(?m)^\s*public const string $([regex]::Escape($Category))\d+ = `"$([regex]::Escape($Category))\d+`";\r?\n",
		'')
	$umbrellaPattern = "(?m)^(?<indent>[ \t]*)public const string $([regex]::Escape($Category)) = `"$([regex]::Escape($Category))`";[ \t]*(?=\r?$)"
	$match = [regex]::Match($categories, $umbrellaPattern)
	if (-not $match.Success) { throw "Could not find the $Category constant in $CategoriesPath." }
	$constants = 1..$ShardCount | ForEach-Object {
		"$($match.Groups['indent'].Value)public const string $Category$_ = `"$Category$_`";"
	}
	$categories = $categories.Insert(
		$match.Index + $match.Length,
		$categoryNewLine + ($constants -join $categoryNewLine))
	[System.IO.File]::WriteAllText($CategoriesPath, $categories, $categoryEncoding)

	$pipelineBytes = [System.IO.File]::ReadAllBytes($PipelinePath)
	$pipelineEncoding = [System.Text.UTF8Encoding]::new(
		$pipelineBytes.Length -ge 3 -and $pipelineBytes[0] -eq 0xEF -and
		$pipelineBytes[1] -eq 0xBB -and $pipelineBytes[2] -eq 0xBF)
	$pipeline = [System.IO.File]::ReadAllText($PipelinePath)
	$pipelineNewLine = if ($pipeline.Contains("`r`n")) { "`r`n" } else { "`n" }
	$lines = [System.Collections.Generic.List[string]]::new()
	foreach ($line in ($pipeline -split '\r?\n')) { $lines.Add($line) }
	$matrixStart = -1
	$matrixEnd = $lines.Count
	$matrixIndentLength = 0
	for ($i = 0; $i -lt $lines.Count; $i++) {
		if ($lines[$i] -match '^(?<indent>[ \t]*)categoryGroupsToTest\s*:') {
			$matrixStart = $i
			$matrixIndentLength = $Matches['indent'].Length
			break
		}
	}
	if ($matrixStart -lt 0) {
		throw "Could not find categoryGroupsToTest in $PipelinePath."
	}
	for ($i = $matrixStart + 1; $i -lt $lines.Count; $i++) {
		if ($lines[$i] -match '^(?<indent>[ \t]*)[A-Za-z_][^:]*:' -and
			$Matches['indent'].Length -le $matrixIndentLength) {
			$matrixEnd = $i
			break
		}
	}
	$categoryLine = -1
	$categoryHasOtherMembers = $false
	$firstShardLine = -1
	$commentLine = -1
	$indent = ''
	for ($i = $matrixStart + 1; $i -lt $matrixEnd; $i++) {
		if ($lines[$i] -match "^(?<indent>[ \t]*)-[ \t]*'(?<members>[^']*)'[ \t]*$") {
			$lineIndent = $Matches['indent']
			$members = @($Matches['members'] -split ',' | ForEach-Object { $_.Trim() })
			if ($members -contains $Category) {
				$categoryLine = $i
				$categoryHasOtherMembers = $members.Count -gt 1
				$indent = $lineIndent
			}
			if ($firstShardLine -lt 0 -and $members.Count -eq 1 -and
				$members[0] -match "^$([regex]::Escape($Category))\d+$") {
				$firstShardLine = $i
				$indent = $lineIndent
			}
		}
		if ($commentLine -lt 0 -and $lines[$i] -match
			"^\s*#\s*$([regex]::Escape($Category)) remains the umbrella category\b") {
			$commentLine = $i
		}
	}
	$insertAt = if ($categoryLine -ge 0) { $categoryLine } elseif ($firstShardLine -ge 0) {
		$firstShardLine
	} elseif ($commentLine -ge 0) {
		$commentLine
	} else {
		throw "Could not find $Category in categoryGroupsToTest in $PipelinePath."
	}

	for ($i = $matrixEnd - 1; $i -gt $matrixStart; $i--) {
		if ($lines[$i] -match "^(?<indent>[ \t]*)-[ \t]*'(?<members>[^']*)'[ \t]*$") {
			$lineIndent = $Matches['indent']
			$originalMembers = @($Matches['members'] -split ',')
			$members = @($originalMembers | ForEach-Object { $_.Trim() } | Where-Object {
				$_ -ne $Category -and $_ -notmatch "^$([regex]::Escape($Category))\d+$"
			})
			if ($members.Count -eq 0) {
				$lines.RemoveAt($i)
				if ($i -lt $insertAt) { $insertAt-- }
			} elseif ($originalMembers.Count -ne $members.Count) {
				$lines[$i] = "$lineIndent- '$($members -join ',')'"
			}
		} elseif ($lines[$i] -match "^\s*#.*$([regex]::Escape($Category)) remains the umbrella category\b" -or
			$lines[$i] -match "^\s*#.*explicit $([regex]::Escape($Category))1-\d+ shard categories below\.\s*$") {
			$lines.RemoveAt($i)
			if ($i -lt $insertAt) { $insertAt-- }
		}
	}
	if ($categoryLine -ge 0 -and $categoryHasOtherMembers) {
		$insertAt++
	}
	$comment = "$indent# $Category remains the umbrella category for local runs. CI uses the explicit $Category" +
		"1-$ShardCount shard categories below."
	$replacement = @($comment) + @(1..$ShardCount | ForEach-Object { "$indent- '$Category$_'" })
	for ($i = $replacement.Count - 1; $i -ge 0; $i--) {
		$lines.Insert($insertAt, $replacement[$i])
	}
	$pipeline = $lines -join $pipelineNewLine
	[System.IO.File]::WriteAllText($PipelinePath, $pipeline, $pipelineEncoding)

	$analyzerBytes = [System.IO.File]::ReadAllBytes($AnalyzerPath)
	$analyzerEncoding = [System.Text.UTF8Encoding]::new(
		$analyzerBytes.Length -ge 3 -and $analyzerBytes[0] -eq 0xEF -and
		$analyzerBytes[1] -eq 0xBB -and $analyzerBytes[2] -eq 0xBF)
	$analyzer = [System.IO.File]::ReadAllText($AnalyzerPath)
	$prefixPattern = '(?s)(?<start>CiShardCategoryPrefixes\s*=\s*\r?\n?\s*ImmutableArray\.Create\()(?<values>.*?)(?<end>\);)'
	$prefixMatch = [regex]::Match($analyzer, $prefixPattern)
	if (-not $prefixMatch.Success) {
		throw "Could not find CiShardCategoryPrefixes in $AnalyzerPath."
	}
	$prefixes = @(
		[regex]::Matches($prefixMatch.Groups['values'].Value, '"(?<prefix>[^"]+)"') |
			ForEach-Object { $_.Groups['prefix'].Value }
		$Category
	) | Sort-Object -Unique
	$values = $prefixes | ForEach-Object { "`"$($_)`"" }
	$replacement = $prefixMatch.Groups['start'].Value + ($values -join ', ') + $prefixMatch.Groups['end'].Value
	$analyzer = $analyzer.Remove($prefixMatch.Index, $prefixMatch.Length).Insert($prefixMatch.Index, $replacement)
	[System.IO.File]::WriteAllText($AnalyzerPath, $analyzer, $analyzerEncoding)
}

function Test-UITestShardApplication {
	param(
		[Parameter(Mandatory)][string]$TestRoot,
		[Parameter(Mandatory)][string]$Category,
		[Parameter(Mandatory)]$Plan
	)

	if ($Plan.category -ne $Category) {
		throw "Plan category '$($Plan.category)' does not match '$Category'."
	}
	$inventory = @(Get-UITestInventory -TestRoot $TestRoot -Category $Category)
	$planned = @{}
	foreach ($assignment in @($Plan.assignments)) {
		if ($planned.ContainsKey([string]$assignment.testId)) {
			throw "Plan contains duplicate test ID '$($assignment.testId)'."
		}
		$planned[[string]$assignment.testId] = [string]$assignment.shard
	}
	$inventoryIds = @($inventory.Id | Sort-Object)
	$planIds = @($planned.Keys | Sort-Object)
	if (($inventoryIds -join "`n") -ne ($planIds -join "`n")) {
		throw 'Applied inventory IDs do not exactly match plan assignment IDs.'
	}

	$validShards = @(1..([int]$Plan.shardCount) | ForEach-Object { "$Category$_" })
	foreach ($test in $inventory | Where-Object { -not $_.DisabledAllPlatforms }) {
		if ($test.ShardCount -ne 1 -or $test.Shard -ne $planned[$test.Id]) {
			throw "Active test '$($test.Id)' must have exactly its planned method-level shard '$($planned[$test.Id])'."
		}
		if ($test.ClassShardCount -ne 0) {
			throw "Active test '$($test.Id)' has a forbidden class-level shard category."
		}
	}

	foreach ($file in Get-ChildItem -LiteralPath $TestRoot -Recurse -File -Filter '*.cs') {
		$text = [System.IO.File]::ReadAllText($file.FullName)
		foreach ($match in [regex]::Matches(
			$text,
			"Category\s*\(\s*UITestCategories\.(?<shard>$([regex]::Escape($Category))\d+)\s*\)")) {
			if ($match.Groups['shard'].Value -notin $validShards) {
				throw "Stale shard '$($match.Groups['shard'].Value)' remains in $($file.FullName)."
			}
		}
		if ($text -match "\[Category\s*\(\s*UITestCategories\.$([regex]::Escape($Category))\s*\)") {
			throw "Legacy $Category umbrella category remains in $($file.FullName)."
		}
		if ($text -match (
			"(?m)^\s*\[Category\(UITestCategories\.$([regex]::Escape($Category))\d+\)\]\s*\r?\n" +
			"\s*(?:(?:public|internal|private|protected|sealed|abstract|static|partial)\s+)*class\s+")) {
			throw "Class-level $Category shard category remains in $($file.FullName)."
		}
	}
}

Export-ModuleMember -Function @(
	'Get-Percentile',
	'Get-UITestInventory',
	'Get-HistoricalEvidence',
	'Get-ResultAutomatedName',
	'Resolve-ResultClass',
	'Get-OrdinaryRunKind',
	'Get-AggregatedWeights',
	'New-UITestShardPlan',
	'Set-UITestShardCategories',
	'Set-ShardInfrastructure',
	'Test-UITestShardApplication'
)
