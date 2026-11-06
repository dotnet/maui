[CmdletBinding()]
param(
    [string]$DotNetPath = 'dotnet'
)

$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$propertyNames = @(
    'TargetFrameworks', 'IncludeCurrentTfms', '_MauiDotNetTfm', '_MauiPreviousDotNetTfm',
    'WindowsMauiPlatforms', 'MauiPlatforms', 'MauiGraphicsPlatforms', 'MauiSamplePlatforms',
    'MauiDeviceTestsPlatforms', 'MauiEmbeddingPlatforms', 'MauiManualTestsPlatforms'
)
$platformProperties = @(
    '-p:CI=true', '-p:EnableWindowsTargeting=true',
    '-p:IncludeWindowsTargetFrameworks=true', '-p:IncludeAndroidTargetFrameworks=true',
    '-p:IncludeIosTargetFrameworks=true', '-p:IncludeMacCatalystTargetFrameworks=true',
    '-p:IncludeMacOSTargetFrameworks=true'
)
$previousProperties = @(
    '-p:IncludePreviousTfms=true', '-p:IncludePreviousTfmsEssentials=true',
    '-p:IncludePreviousTfmsGraphics=true'
)

function Get-FrameworkProperties([string]$Project, [string[]]$Properties) {
    $output = & $DotNetPath msbuild $Project -nologo "-getProperty:$($propertyNames -join ',')" @platformProperties @Properties
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild evaluation failed for $Project ($($Properties -join ' ')): $($output -join [Environment]::NewLine)"
    }
    return ($output -join [Environment]::NewLine | ConvertFrom-Json).Properties
}

function Get-Frameworks([string]$Value) {
    return @($Value.Split(';', [StringSplitOptions]::RemoveEmptyEntries) | Sort-Object -Unique)
}

function Assert-Equal([string]$Actual, [string]$Expected, [string]$Context) {
    if ($Actual -cne $Expected) {
        throw "${Context}: expected '$Expected', got '$Actual'."
    }
}

$projects = @(Get-ChildItem (Join-Path $repoRoot 'src') -Filter '*.csproj' -Recurse |
    Where-Object {
        [xml]$xml = Get-Content -LiteralPath $_.FullName -Raw
        $xml.SelectNodes('/Project/PropertyGroup/TargetFrameworks[contains(@Condition, "IncludePreviousTfms")]').Count -gt 0
    })
if ($projects.Count -eq 0) {
    throw 'No projects with previous-TFM selection were discovered.'
}

foreach ($project in $projects) {
    $default = Get-FrameworkProperties $project.FullName @()
    $current = Get-FrameworkProperties $project.FullName @('-p:IncludeCurrentTfms=true')
    $both = Get-FrameworkProperties $project.FullName $previousProperties
    $previous = Get-FrameworkProperties $project.FullName (@('-p:IncludeCurrentTfms=false') + $previousProperties)
    $name = [IO.Path]::GetRelativePath($repoRoot, $project.FullName)
    $currentPrefix = $default._MauiDotNetTfm
    $previousPrefix = $default._MauiPreviousDotNetTfm

    Assert-Equal $default.IncludeCurrentTfms 'true' "$name default"
    Assert-Equal $default.TargetFrameworks $current.TargetFrameworks "$name explicit true preserves defaults"
    $defaultFrameworks = Get-Frameworks $default.TargetFrameworks
    if (-not @($defaultFrameworks | Where-Object { $_ -eq $currentPrefix -or $_.StartsWith("$currentPrefix-") }).Count) {
        throw "$name has no current frameworks in its default selection."
    }
    if (@($defaultFrameworks | Where-Object { $_ -eq $previousPrefix -or $_.StartsWith("$previousPrefix-") }).Count) {
        throw "$name enables previous frameworks by default."
    }

    $previousFrameworks = Get-Frameworks $previous.TargetFrameworks
    if (-not @($previousFrameworks | Where-Object { $_ -eq $previousPrefix -or $_.StartsWith("$previousPrefix-") }).Count) {
        throw "$name has no previous frameworks when current frameworks are disabled."
    }
    $expectedPrevious = @(Get-Frameworks $both.TargetFrameworks | Where-Object {
        $_ -ne $currentPrefix -and -not $_.StartsWith("$currentPrefix-")
    })
    Assert-Equal ($previousFrameworks -join ';') ($expectedPrevious -join ';') "$name previous-only selection"
    $expectedBoth = @(@($defaultFrameworks) + @($previousFrameworks) | Sort-Object -Unique)
    Assert-Equal ((Get-Frameworks $both.TargetFrameworks) -join ';') ($expectedBoth -join ';') "$name combined selection"

    foreach ($property in $propertyNames | Where-Object { $_ -like '*Platforms' }) {
        Assert-Equal $previous.$property '' "$name excludes $property"
        if ([string]::IsNullOrEmpty($default.$property)) {
            throw "$name does not exercise $property with all platforms enabled."
        }
    }

    $standardFrameworks = @($defaultFrameworks | Where-Object { $_.StartsWith('netstandard') })
    if ($standardFrameworks.Count -gt 0) {
        $standard = Get-FrameworkProperties $project.FullName @('-p:IncludeCurrentTfms=false')
        Assert-Equal ((Get-Frameworks $standard.TargetFrameworks) -join ';') ($standardFrameworks -join ';') "$name .NET Standard only"
    }
    Write-Host "PASS $name"
}

Write-Host "PASS TFM selection for $($projects.Count) projects (default, current, combined, previous-only, and applicable .NET Standard-only selections)."
