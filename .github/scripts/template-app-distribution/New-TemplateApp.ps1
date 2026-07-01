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
    [string]$AppBuildNumber,

    [Parameter(Mandatory)]
    [string]$NuGetConfigPath
)

$ErrorActionPreference = "Stop"

function ConvertTo-XmlEscaped([string]$Value) {
    return [System.Security.SecurityElement]::Escape($Value)
}

function Set-PlistBooleanFalse([string]$Path, [string]$Key) {
    if (-not (Test-Path $Path)) {
        return
    }

    $plistContent = Get-Content $Path -Raw
    $escapedKey = [regex]::Escape($Key)
    $booleanKeyPattern = "(?s)(<key>$escapedKey</key>\s*)<(true|false)\s*/>"
    if ($plistContent -match $booleanKeyPattern) {
        $plistContent = [regex]::Replace($plistContent, $booleanKeyPattern, '$1<false/>', 1)
        Set-Content -Path $Path -Value $plistContent -Encoding utf8
        return
    }

    $entry = "`t<key>$Key</key>`r`n`t<false/>`r`n"
    $plistContent = $plistContent -replace "(?m)^</dict>", "$entry</dict>"
    Set-Content -Path $Path -Value $plistContent -Encoding utf8
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

if (-not (Test-Path $NuGetConfigPath)) {
    throw "NuGet.config was not found at '$NuGetConfigPath'."
}

Copy-Item -Path $NuGetConfigPath -Destination (Join-Path $projectRoot "NuGet.config") -Force

Write-Host "Installing template package $TemplatePackagePath"
dotnet new install $TemplatePackagePath

$templateArgs = @()
if (-not [string]::IsNullOrWhiteSpace($TemplateArgsJson)) {
    $templateArgs = @(ConvertFrom-Json $TemplateArgsJson | ForEach-Object { [string]$_ })
}

$dotnetNewArgs = @("new", $Template, "-n", $ProjectName, "-o", $projectDir, "--framework", $DotNetTfm, "--no-restore") + $templateArgs
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

if ($TargetFramework.Contains("-windows", [System.StringComparison]::OrdinalIgnoreCase) -and
    $content -notmatch "RuntimeIdentifierOverride") {
    $runtimeIdentifierOverridePropertyGroup = @"

	<PropertyGroup Condition="`$([MSBuild]::GetTargetPlatformIdentifier('`$(TargetFramework)')) == 'windows' and '`$(RuntimeIdentifierOverride)' != ''">
		<RuntimeIdentifier>`$(RuntimeIdentifierOverride)</RuntimeIdentifier>
	</PropertyGroup>
"@

    $content = $content -replace "</Project>\s*$", "$runtimeIdentifierOverridePropertyGroup`r`n</Project>"
}

Set-Content -Path $projectFile.FullName -Value $content -Encoding utf8

if ($TargetFramework.Contains("-ios", [System.StringComparison]::OrdinalIgnoreCase)) {
    Set-PlistBooleanFalse (Join-Path $projectDir "Platforms/iOS/Info.plist") "ITSAppUsesNonExemptEncryption"
} elseif ($TargetFramework.Contains("-maccatalyst", [System.StringComparison]::OrdinalIgnoreCase)) {
    Set-PlistBooleanFalse (Join-Path $projectDir "Platforms/MacCatalyst/Info.plist") "ITSAppUsesNonExemptEncryption"
}

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
