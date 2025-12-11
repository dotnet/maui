param(
    [string] $BaseRef,
    [string[]] $CategoryGroups,
    [string] $WorkingDirectory = ".",
    [string] $TestPath = "src/Controls/tests/TestCases.Shared.Tests/Tests",
    [string] $CategoryAttributePattern = '^\+\s*\[Category\(UITestCategories\.([A-Za-z0-9_]+)\)\]'
)

function Write-PipelineVariable {
    param(
        [string] $Name,
        [string] $Value,
        [bool] $IsOutput = $true
    )

    $outputFlag = if ($IsOutput) { ";isOutput=true" } else { "" }
    Write-Host "##vso[task.setvariable variable=$Name$outputFlag]$Value"
}

$isPullRequest = $env:BUILD_REASON -eq "PullRequest"

if (-not $isPullRequest) {
    Write-Host "Not a Pull Request build. Running all UI test categories."
    Write-PipelineVariable -Name "UITestRunAll" -Value "true"
    Write-PipelineVariable -Name "UITestSelectedCategories" -Value ""
    Write-PipelineVariable -Name "UITestCategoryGroups" -Value ""
    Write-PipelineVariable -Name "UITestCategoryGroupsDelimited" -Value ""
    return
}

if (-not $BaseRef) {
    $BaseRef = $env:SYSTEM_PULLREQUEST_TARGETBRANCH
}

if (-not $BaseRef) {
    $BaseRef = "origin/main"
}

if ($BaseRef.StartsWith("refs/heads/")) {
    $BaseRef = $BaseRef.Substring("refs/heads/".Length)
}

$BaseRef = "origin/$BaseRef"

$categories = New-Object System.Collections.Generic.HashSet[string] ([StringComparer]::OrdinalIgnoreCase)
$categoryGroupsToRun = New-Object System.Collections.Generic.List[string]
$runAll = $true

Push-Location $WorkingDirectory
try {
    git fetch --no-tags origin $BaseRef --depth=1 1>$null
    if ($LASTEXITCODE -ne 0) {
        throw "git fetch failed for $BaseRef"
    }
    $mergeBase = git merge-base HEAD $BaseRef
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($mergeBase)) {
        throw "Unable to determine merge-base with $BaseRef"
    }

    if (-not $mergeBase) {
        Write-Host "Unable to determine merge-base with $BaseRef. Falling back to running all categories."
    }
    else {
        $diff = git diff --unified=0 --diff-filter=AM $mergeBase..HEAD -- $TestPath
        if ($LASTEXITCODE -ne 0) {
            throw "git diff failed for $TestPath against $BaseRef"
        }

        if (-not [string]::IsNullOrWhiteSpace($diff)) {
            $pattern = $CategoryAttributePattern
            foreach ($line in $diff -split "`n") {
                if ($line -match $pattern) {
                    [void]$categories.Add($matches[1])
                }
            }
        }

        if ($categories.Count -gt 0) {
            $runAll = $false

            foreach ($group in $CategoryGroups) {
                if ([string]::IsNullOrWhiteSpace($group)) {
                    continue
                }

                $groupCategories = $group.Split(",", [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() }
                if ($groupCategories | Where-Object { $categories.Contains($_) }) {
                    $categoryGroupsToRun.Add($group.Trim())
                }
            }
        }
    }
}
catch {
    Write-Host "Error while detecting UI test categories: $_"
}
finally {
    Pop-Location
}

if ($runAll -or $categories.Count -eq 0) {
    Write-Host "No UI test categories detected from PR changes. Running all categories."
    Write-PipelineVariable -Name "UITestRunAll" -Value "true"
    Write-PipelineVariable -Name "UITestSelectedCategories" -Value ""
    Write-PipelineVariable -Name "UITestCategoryGroups" -Value ""
    Write-PipelineVariable -Name "UITestCategoryGroupsDelimited" -Value ""
    return
}

$selectedCategories = ($categories.ToArray() | Sort-Object)
$selectedCategoriesValue = [string]::Join(",", $selectedCategories)
$categoryGroupsValue = [string]::Join(",", $categoryGroupsToRun)
$categoryGroupsDelimitedValue = if ($categoryGroupsToRun.Count -gt 0) { ";" + ([string]::Join(";", $categoryGroupsToRun)) + ";" } else { "" }

Write-Host "Detected UI test categories: $selectedCategoriesValue"
Write-Host "Mapped category groups to run: $categoryGroupsValue"

Write-PipelineVariable -Name "UITestRunAll" -Value "false"
Write-PipelineVariable -Name "UITestSelectedCategories" -Value $selectedCategoriesValue
Write-PipelineVariable -Name "UITestCategoryGroups" -Value $categoryGroupsValue
Write-PipelineVariable -Name "UITestCategoryGroupsDelimited" -Value $categoryGroupsDelimitedValue
