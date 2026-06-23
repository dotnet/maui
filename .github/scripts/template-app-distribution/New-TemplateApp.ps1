#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$TemplatePackagePath,

    [Parameter(Mandatory)]
    [string]$BuildRoot,

    [Parameter(Mandatory)]
    [string]$Variant,

    [Parameter(Mandatory)]
    [string]$ProjectName,

    [Parameter(Mandatory)]
    [string]$Template,

    [Parameter(Mandatory)]
    [string]$TemplateArgsJson,

    [Parameter(Mandatory)]
    [string]$DotNetTfm,

    [Parameter(Mandatory)]
    [string]$TargetFramework,

    [Parameter(Mandatory)]
    [string]$ApplicationId,

    [Parameter(Mandatory)]
    [string]$DisplayName,

    [Parameter(Mandatory)]
    [string]$DotNetSdk,

    [Parameter(Mandatory)]
    [string]$AppDisplayVersion,

    [Parameter(Mandatory)]
    [string]$AppBuildNumber
)

$ErrorActionPreference = "Stop"

function ConvertTo-XmlEscaped([string]$Value) {
    return [System.Security.SecurityElement]::Escape($Value)
}

if (-not (Test-Path $TemplatePackagePath)) {
    throw "Template package was not found at '$TemplatePackagePath'."
}

$projectRoot = Join-Path $BuildRoot $Variant
$projectDir = Join-Path $projectRoot $ProjectName
$dotnetHome = Join-Path $projectRoot ".dotnet"
$nugetPackages = Join-Path $projectRoot ".nuget"

Remove-Item -Path $projectRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $projectRoot -Force | Out-Null
New-Item -ItemType Directory -Path $dotnetHome -Force | Out-Null
New-Item -ItemType Directory -Path $nugetPackages -Force | Out-Null

$env:DOTNET_CLI_HOME = $dotnetHome
$env:NUGET_PACKAGES = $nugetPackages

$nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="dotnet-public" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
$nugetConfig | Out-File -FilePath (Join-Path $projectRoot "NuGet.config") -Encoding utf8

Write-Host "Installing template package $TemplatePackagePath"
dotnet new install $TemplatePackagePath

$templateArgs = @()
if (-not [string]::IsNullOrWhiteSpace($TemplateArgsJson)) {
    $templateArgs = @(ConvertFrom-Json $TemplateArgsJson | ForEach-Object { [string]$_ })
}

$dotnetNewArgs = @("new", $Template, "-n", $ProjectName, "-o", $projectDir, "--framework", $DotNetTfm) + $templateArgs
Write-Host "Creating project: dotnet $($dotnetNewArgs -join ' ')"
& dotnet @dotnetNewArgs

$projectFile = Get-ChildItem -Path $projectDir -Filter "*.csproj" -Recurse | Select-Object -First 1
if (-not $projectFile) {
    throw "No project file was created in '$projectDir'."
}

$content = Get-Content $projectFile.FullName -Raw
$targetFrameworksMatches = @([regex]::Matches($content, "<TargetFrameworks(?:\s+Condition=""[^""]*"")?>[^<]+</TargetFrameworks>"))
if ($targetFrameworksMatches.Count -eq 0) {
    throw "Could not find TargetFrameworks in '$($projectFile.FullName)'."
}

for ($i = $targetFrameworksMatches.Count - 1; $i -ge 0; $i--) {
    $match = $targetFrameworksMatches[$i]
    $replacement = if ($i -eq 0) {
        "<TargetFrameworks>$TargetFramework</TargetFrameworks>"
    } else {
        ""
    }

    $content = $content.Remove($match.Index, $match.Length).Insert($match.Index, $replacement)
}

$content = $content -replace "<ApplicationTitle>[^<]+</ApplicationTitle>", "<ApplicationTitle>$(ConvertTo-XmlEscaped $DisplayName)</ApplicationTitle>"
$content = $content -replace "<ApplicationId>[^<]+</ApplicationId>", "<ApplicationId>$(ConvertTo-XmlEscaped $ApplicationId)</ApplicationId>"
$content = $content -replace "<ApplicationDisplayVersion>[^<]+</ApplicationDisplayVersion>", "<ApplicationDisplayVersion>$(ConvertTo-XmlEscaped $AppDisplayVersion)</ApplicationDisplayVersion>"
$content = $content -replace "<ApplicationVersion>[^<]+</ApplicationVersion>", "<ApplicationVersion>$(ConvertTo-XmlEscaped $AppBuildNumber)</ApplicationVersion>"
Set-Content -Path $projectFile.FullName -Value $content -Encoding utf8

@{
    sdk = @{
        version = $DotNetSdk
        rollForward = "latestPatch"
    }
} | ConvertTo-Json -Depth 5 | Set-Content -Path (Join-Path $projectDir "global.json") -Encoding utf8

Write-Host "Generated project: $($projectFile.FullName)"

if ($env:GITHUB_OUTPUT) {
    "project_path=$projectDir" >> $env:GITHUB_OUTPUT
    "project_file=$($projectFile.FullName)" >> $env:GITHUB_OUTPUT
}
