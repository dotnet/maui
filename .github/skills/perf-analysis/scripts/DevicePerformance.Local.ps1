function Get-DeviceProperty($value, [string]$path) {
    foreach ($segment in $path.Split(".")) {
        if ($null -eq $value) {
            return $null
        }
        if ($value -is [Collections.IDictionary]) {
            $value = $value[$segment]
        } else {
            $property = $value.PSObject.Properties[$segment]
            if ($null -eq $property) { return $null }
            $value = $property.Value
        }
    }
    return ,$value
}

function Test-DeviceInteger($value, [long]$minimum = 0, [long]$maximum = [int]::MaxValue) {
    return ($value -is [int] -or $value -is [long]) -and
        $value -ge $minimum -and $value -le $maximum
}

function Assert-DeviceNumber($value, [string]$name, [switch]$allowNegative) {
    if (($value -isnot [int] -and $value -isnot [long] -and
        $value -isnot [double] -and $value -isnot [decimal]) -or
        -not [double]::IsFinite([double]$value) -or (-not $allowNegative -and $value -lt 0)) {
        throw "'$name' must be a finite $(if (-not $allowNegative) { 'nonnegative ' })number."
    }
}

function Assert-LocalDeviceIdentity($identity) {
    if ($identity.repository -isnot [string] -or
        $identity.repository -cnotmatch '\A[a-z0-9][a-z0-9-]*/[a-z0-9][a-z0-9_.-]*\z' -or
        -not (Test-DeviceInteger $identity.pullRequestNumber 1)) {
        throw "Device identity requires a canonical repository and a positive integer PR number."
    }
    foreach ($field in @("baseCommitSha", "headCommitSha", "harnessSha")) {
        $value = Get-DeviceProperty $identity $field
        if ($value -isnot [string] -or $value -cnotmatch '\A[0-9a-f]{40}\z') {
            throw "Device identity '$field' must be a full lowercase commit SHA."
        }
    }
}

function Get-ExistingDeviceItem([string]$path) {
    try {
        return Get-Item -LiteralPath $path -Force -ErrorAction Stop
    } catch [Management.Automation.ItemNotFoundException] {
        return $null
    }
}

function Get-LocalDevicePath([string]$path, [string]$kind = "Any", [switch]$mustExist) {
    if ([string]::IsNullOrWhiteSpace($path) -or
        -not [IO.Path]::IsPathFullyQualified($path) -or
        $path -match '[\x00-\x1f*?]' -or $path -match '^[\\/]{2}' -or
        ($IsWindows -and $path.Substring(2).Contains(":"))) {
        throw "An absolute local filesystem path is required: '$path'."
    }

    $fullPath = [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($path))
    if ($IsWindows -and [IO.DriveInfo]::new([IO.Path]::GetPathRoot($fullPath)).DriveType -eq
        [IO.DriveType]::Network) {
        throw "Network paths are not local device evidence: '$path'."
    }
    $current = $fullPath
    while ($current) {
        $item = Get-ExistingDeviceItem $current
        if ($null -ne $item) {
            if ($item.LinkType -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
                throw "Local device paths must not contain symbolic links or reparse points: '$current'."
            }
            if ($current -ne $fullPath -and -not $item.PSIsContainer) {
                throw "Local device path contains a non-directory component: '$current'."
            }
        }
        $current = [IO.Path]::GetDirectoryName($current)
    }

    $item = Get-ExistingDeviceItem $fullPath
    if ($null -eq $item) {
        if ($mustExist) {
            throw "Local device $kind does not exist: '$fullPath'."
        }
    } elseif (($kind -eq "Directory" -and -not $item.PSIsContainer) -or
        ($kind -eq "File" -and $item.PSIsContainer)) {
        throw "Expected a local device $kind at '$fullPath'."
    }
    return $fullPath
}

function Test-LocalDevicePathEqual([string]$left, [string]$right) {
    $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    return [string]::Equals($left, $right, $comparison)
}

function Assert-DeviceOutputOutsideResults([string]$outputPath, [string]$resultsRoot, [string[]]$inputs = @()) {
    $output = Get-LocalDevicePath $outputPath "File"
    $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    if ((Test-LocalDevicePathEqual $output $resultsRoot) -or
        $output.StartsWith($resultsRoot + [IO.Path]::DirectorySeparatorChar, $comparison)) {
        throw "Validation/request output must be outside the read-only results root: '$output'."
    }
    foreach ($inputPath in $inputs) {
        if (Test-LocalDevicePathEqual $output (Get-LocalDevicePath $inputPath "File")) {
            throw "Output must not overwrite an evidence input: '$output'."
        }
    }
    return $output
}

function Get-LocalDeviceRequirements($selection) {
    $registryPath = [IO.Path]::Combine($PSScriptRoot, "..", "references", "platform-scenarios.json")
    $registry = Get-Content -LiteralPath $registryPath -Raw | ConvertFrom-Json
    $pairs = [ordered]@{}
    foreach ($scenario in @($selection.deviceScenarios | Where-Object { $null -ne $_ })) {
        if ($scenario.automationStatus -ne "manual-local-ready") {
            if ($null -ne $scenario.localRun) {
                throw "Unsupported scenario '$($scenario.id)' must not expose a local handoff."
            }
            continue
        }
        $definition = @($registry.scenarios | Where-Object {
            $_.id -ceq $scenario.id -and $_.automationStatus -eq "manual-local-ready"
        })
        if ($definition.Count -ne 1 -or
            [string]$scenario.resultScenario -cne [string]$definition[0].resultScenario -or
            [string]$scenario.localRun.driver -cne [string]$definition[0].localRun.driver -or
            (@($scenario.platforms) -join "|") -cne (@($definition[0].platforms) -join "|") -or
            (@($scenario.localRun.platforms) -join "|") -cne (@($definition[0].localRun.platforms) -join "|")) {
            throw "Scenario '$($scenario.id)' does not match the trusted local scenario/platform contract."
        }
        $coverageMode = if ($scenario.coverageMode) { [string]$scenario.coverageMode } else { "direct" }
        $expectedCoverageMode = if ($definition[0].coverageMode) { [string]$definition[0].coverageMode } else { "direct" }
        if ($coverageMode -cne $expectedCoverageMode) {
            throw "Scenario '$($scenario.id)' must retain its trusted '$expectedCoverageMode' coverage mode."
        }
        foreach ($platform in @($definition[0].localRun.platforms)) {
            $pair = "$($scenario.resultScenario)|$platform"
            if (-not $pairs.Contains($pair)) {
                $pairs[$pair] = [PSCustomObject]@{
                    scenarioIds = @()
                    resultScenario = [string]$scenario.resultScenario
                    platform = [string]$platform
                    driver = [string]$definition[0].localRun.driver
                }
            }
            $pairs[$pair].scenarioIds = @($pairs[$pair].scenarioIds + [string]$scenario.id | Sort-Object -Unique)
        }
    }
    return @($pairs.Values | Sort-Object resultScenario, platform)
}

function Get-LocalDeviceRequestKey($identity, [string]$scenario, [string]$platform) {
    $rawKey = @(
        $identity.repository, $identity.pullRequestNumber, $identity.baseCommitSha,
        $identity.headCommitSha, $identity.harnessSha, $scenario, $platform, 2
    ) -join "|"
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
        [Text.Encoding]::UTF8.GetBytes($rawKey))).ToLowerInvariant()
    return "maui-perf-$hash"
}

function New-LocalDeviceRequests($selection, $identity, [string]$resultsRoot) {
    Assert-LocalDeviceIdentity $identity
    $root = Get-LocalDevicePath $resultsRoot "Directory" -mustExist
    if (Test-LocalDevicePathEqual $root ([IO.Path]::GetPathRoot($root))) {
        throw "ResultsRoot must be a caller-owned trial directory, not a filesystem root."
    }
    foreach ($requirement in @(Get-LocalDeviceRequirements $selection)) {
        $key = Get-LocalDeviceRequestKey $identity $requirement.resultScenario $requirement.platform
        [PSCustomObject][ordered]@{
            schemaVersion = 1
            executionMode = "manual-local"
            requestKey = $key
            repository = $identity.repository
            pullRequestNumber = $identity.pullRequestNumber
            baseCommitSha = $identity.baseCommitSha
            headCommitSha = $identity.headCommitSha
            harnessSha = $identity.harnessSha
            scenarioIds = @($requirement.scenarioIds)
            expectedScenario = $requirement.resultScenario
            platform = $requirement.platform
            expectedVariantRuns = 2
            driver = $requirement.driver
            resultDirectory = Get-LocalDevicePath (Join-Path $root $key) "Directory"
        }
    }
}

function Test-DeviceJsonEqual($left, $right) {
    if ($null -eq $left -or $null -eq $right) {
        return $null -eq $left -and $null -eq $right
    }
    # Sorted scalars can be wrapped in PSObject; compare primitives before objects.
    if ($left -is [bool] -or $right -is [bool]) {
        return $left -is [bool] -and $right -is [bool] -and $left -eq $right
    }
    if ($left -is [string] -or $right -is [string]) {
        return $left -is [string] -and $right -is [string] -and $left -ceq $right
    }
    $leftNumeric = $left -is [int] -or $left -is [long] -or $left -is [double] -or $left -is [decimal]
    $rightNumeric = $right -is [int] -or $right -is [long] -or $right -is [double] -or $right -is [decimal]
    if ($leftNumeric -or $rightNumeric) {
        return $leftNumeric -and $rightNumeric -and $left -eq $right
    }
    if ($left -is [array] -or $right -is [array]) {
        if ($left -isnot [array] -or $right -isnot [array] -or $left.Count -ne $right.Count) {
            return $false
        }
        for ($index = 0; $index -lt $left.Count; $index++) {
            if (-not (Test-DeviceJsonEqual $left[$index] $right[$index])) { return $false }
        }
        return $true
    }
    if ($left -is [PSCustomObject] -or $right -is [PSCustomObject]) {
        if ($left -isnot [PSCustomObject] -or $right -isnot [PSCustomObject]) { return $false }
        $names = @($left.PSObject.Properties.Name | Sort-Object)
        if (($names -join "|") -ine (@($right.PSObject.Properties.Name | Sort-Object) -join "|")) {
            return $false
        }
        foreach ($name in $names) {
            if (-not (Test-DeviceJsonEqual $left.$name $right.$name)) { return $false }
        }
        return $true
    }
    return $false
}

function Get-LocalDeviceEvidenceState($selection, $validation) {
    $required = @(Get-LocalDeviceRequirements $selection)
    $unsupported = @($selection.deviceScenarios | Where-Object {
        $null -ne $_ -and $_.automationStatus -ne "manual-local-ready"
    })
    $state = [PSCustomObject]@{
        hasEvidence = $false
        complete = $false
        advisory = $false
        supportedPath = $required.Count -gt 0
        unsupportedPath = $unsupported.Count -gt 0
    }
    if ($null -eq $validation) { return $state }
    if (-not (Test-DeviceInteger $validation.schemaVersion 2 2) -or
        -not (Test-DeviceInteger $validation.nativeSchemaVersion 3 3) -or
        $validation.evidenceKind -cne "manual-local-device" -or $validation.sealed -isnot [bool]) {
        throw "DeviceValidationPath must contain local schema-3 evidence from Validate-DevicePerformanceEvidence.ps1."
    }
    if (-not $validation.sealed) { return $state }
    Assert-LocalDeviceIdentity $validation
    foreach ($flag in @("deviceEvidenceComplete", "correctnessPassed", "allAffectedPlatformsCovered")) {
        if ((Get-DeviceProperty $validation $flag) -isnot [bool]) {
            throw "Local device validation '$flag' must be a boolean."
        }
    }
    if (@($validation.errors).Count -gt 0) {
        throw "Sealed local device evidence must not contain validation errors."
    }
    $accepted = @($validation.acceptedMeasurements)
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($measurement in $accepted) {
        $pair = "$($measurement.resultScenario)|$($measurement.platform)"
        $match = @($required | Where-Object {
            $_.resultScenario -ceq $measurement.resultScenario -and $_.platform -ceq $measurement.platform
        })
        $key = Get-LocalDeviceRequestKey $validation $measurement.resultScenario $measurement.platform
        if ($match.Count -ne 1 -or -not $seen.Add($pair) -or $measurement.requestKey -cne $key -or
            $measurement.verdict -cnotin @("neutral", "time-regression-advisory", "time-improvement-advisory") -or
            $measurement.correctnessPassed -isnot [bool] -or -not $measurement.correctnessPassed -or
            $measurement.baseCorrectnessPassed -isnot [bool]) {
            throw "Accepted local device measurement '$pair' is invalid or does not match selection."
        }
        foreach ($variant in @("base", "head")) {
            foreach ($field in @("Minimum", "Maximum", "Median")) {
                Assert-DeviceNumber (Get-DeviceProperty $measurement "$variant.$field") "$pair $variant.$field"
            }
            if (-not (Test-DeviceInteger (Get-DeviceProperty $measurement "$variant.Count") 1)) {
                throw "Accepted local device measurement '$pair' is missing its $variant sample count."
            }
        }
        $direct = @($selection.deviceScenarios | Where-Object {
            $_.resultScenario -ceq $measurement.resultScenario -and $_.coverageMode -ne "sampled"
        }).Count -gt 0
        if ($direct -and ($measurement.verdict -ne "neutral" -or -not $measurement.baseCorrectnessPassed)) {
            $state.advisory = $true
        }
    }
    $state.hasEvidence = $accepted.Count -gt 0
    $state.complete = $state.hasEvidence -and $required.Count -eq $accepted.Count -and
        $unsupported.Count -eq 0 -and
        $validation.deviceEvidenceComplete -is [bool] -and $validation.deviceEvidenceComplete -and
        $validation.correctnessPassed -is [bool] -and $validation.correctnessPassed -and
        $validation.allAffectedPlatformsCovered -is [bool] -and $validation.allAffectedPlatformsCovered
    return $state
}
