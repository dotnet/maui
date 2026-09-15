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
    [string]$NuGetConfigPath,

    [Parameter(Mandatory)]
    [string]$SourceManifestPath,

    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-f]{40}$')]
    [string]$SourceSha
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/Source-Packages.ps1"

function ConvertTo-XmlEscaped([string]$Value) {
    return [System.Security.SecurityElement]::Escape($Value)
}

function Set-ProjectElementValue([string]$Content, [string]$Name, [string]$Value) {
    $elementPattern = "<$([regex]::Escape($Name))>[^<]+</$([regex]::Escape($Name))>"
    $replacement = "<$Name>$(ConvertTo-XmlEscaped $Value)</$Name>"
    return ([regex]$elementPattern).Replace($Content, { param($match) $replacement }, 1)
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
    $plistContent = [regex]::Replace($plistContent, "(?s)</dict>(\s*</plist>)", "$entry</dict>`$1", 1)
    Set-Content -Path $Path -Value $plistContent -Encoding utf8
}

function Get-DotNetMajorVersion([string]$DotNetTfm) {
    if ($DotNetTfm -notmatch "^net(?<Major>\d+)\.") {
        return $null
    }

    return [int]$Matches.Major
}

function Test-UsesImplicitXamlXmlns([string]$ProjectDirectory) {
    $xamlFiles = Get-ChildItem -Path $ProjectDirectory -Filter "*.xaml" -Recurse -File
    foreach ($xamlFile in $xamlFiles) {
        $xaml = Get-Content -Path $xamlFile.FullName -Raw
        $usesXamlPrefixWithoutDeclaration = $xaml -match "\bx:[A-Za-z_][A-Za-z0-9_]*" -and $xaml -notmatch "\sxmlns:x\s*="
        $usesDefaultImplicitNamespace = $xaml -notmatch "\sxmlns\s*=" -and $xaml -match "<\s*[A-Za-z_][A-Za-z0-9_.]*"
        if ($usesXamlPrefixWithoutDeclaration -or $usesDefaultImplicitNamespace) {
            return $true
        }
    }

    return $false
}

function Set-ProjectProperty([string]$Content, [string]$Name, [string]$Value) {
    $propertyPattern = "(?s)<$([regex]::Escape($Name))>.*?</$([regex]::Escape($Name))>"
    $property = "<$Name>$Value</$Name>"
    if ($Content -match $propertyPattern) {
        return [regex]::Replace($Content, $propertyPattern, $property, 1)
    }

    $firstPropertyGroupEnd = [regex]::Match($Content, "\r?\n\s*</PropertyGroup>")
    if (-not $firstPropertyGroupEnd.Success) {
        throw "Could not find a PropertyGroup in the generated project."
    }

    return $Content.Insert($firstPropertyGroupEnd.Index, "`r`n`t`t$property")
}

function Add-ProjectDefineConstant([string]$Content, [string]$Constant) {
    $defineConstantsPattern = "(?s)<DefineConstants>(?<Value>.*?)</DefineConstants>"
    $defineConstantsMatch = [regex]::Match($Content, $defineConstantsPattern)
    if ($defineConstantsMatch.Success) {
        $constants = [string]$defineConstantsMatch.Groups["Value"].Value
        if ($constants.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries) -contains $Constant) {
            return $Content
        }

        $value = "$constants;$Constant"
        return [regex]::Replace($Content, $defineConstantsPattern, "<DefineConstants>$value</DefineConstants>", 1)
    }

    return Set-ProjectProperty $Content "DefineConstants" "`$(DefineConstants);$Constant"
}

if (-not (Test-Path $TemplatePackagePath)) {
    throw "Template package was not found at '$TemplatePackagePath'."
}
$sourceManifest = Read-SourcePackageManifest $SourceManifestPath $SourceSha
$templateInfo = Get-SourcePackageInfo $TemplatePackagePath $SourceSha
$expectedTemplate = @($sourceManifest.packages | Where-Object id -Like 'Microsoft.Maui.Templates*')
if ($expectedTemplate.Count -ne 1 -or $templateInfo.sha512 -cne $expectedTemplate[0].sha512) {
    throw "Template content is not from the pinned source package set."
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

Set-SourcePackageConfiguration -NuGetConfigPath $NuGetConfigPath `
    -PackageDirectory (Split-Path $SourceManifestPath -Parent) -ProjectRoot $projectRoot -Version $sourceManifest.version
$sdkPackage = $sourceManifest.packages | Where-Object id -EQ 'Microsoft.Maui.Sdk'
$sdkDirectory = Join-Path $projectRoot '.maui-sdk'
[System.IO.Compression.ZipFile]::ExtractToDirectory(
    (Join-Path (Split-Path $SourceManifestPath -Parent) $sdkPackage.file), $sdkDirectory)
$sdkTargets = Join-Path $sdkDirectory 'Sdk/Sdk.targets'
if (-not (Test-Path $sdkTargets)) { throw "Source-built MAUI SDK has no Sdk/Sdk.targets." }

Write-Host "Installing template package $TemplatePackagePath"
Invoke-DistributionDotNet @('new', 'install', $TemplatePackagePath) "dotnet new install of '$TemplatePackagePath'"

$templateArgs = @()
if (-not [string]::IsNullOrWhiteSpace($TemplateArgsJson)) {
    $templateArgs = @(ConvertFrom-Json $TemplateArgsJson | ForEach-Object { [string]$_ })
}

$dotnetNewArgs = @("new", $Template, "-n", $ProjectName, "-o", $projectDir, "--framework", $DotNetTfm, "--no-restore") + $templateArgs
Write-Host "Creating project: dotnet $($dotnetNewArgs -join ' ')"
Invoke-DistributionDotNet $dotnetNewArgs "dotnet new $Template"

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

$content = Set-ProjectElementValue $content "ApplicationTitle" $DisplayName
$content = Set-ProjectElementValue $content "ApplicationId" $ApplicationId
$content = Set-ProjectElementValue $content "ApplicationDisplayVersion" $AppDisplayVersion
$content = Set-ProjectElementValue $content "ApplicationVersion" $AppBuildNumber
$content = Set-ProjectProperty $content "MauiVersion" (ConvertTo-XmlEscaped $sourceManifest.version)
$content = Set-ProjectProperty $content "SkipMauiWorkloadManifest" "true"
# Explicit template versions must not override the source-built framework or its transitive dependencies.
$projectXml = [xml]::new()
$projectXml.PreserveWhitespace = $true
$projectXml.LoadXml($content)
foreach ($reference in $projectXml.SelectNodes("//PackageReference[starts-with(@Include, 'Microsoft.Maui.')]")) {
    $reference.SetAttribute('Version', $sourceManifest.version)
    $versionElement = $reference.SelectSingleNode('Version')
    if ($versionElement) { $reference.RemoveChild($versionElement) | Out-Null }
}
$content = $projectXml.OuterXml

if ($TargetFramework.Contains("-windows", [System.StringComparison]::OrdinalIgnoreCase) -and
    $content -notmatch "RuntimeIdentifierOverride") {
    $runtimeIdentifierOverridePropertyGroup = @"

	<PropertyGroup Condition="`$([MSBuild]::GetTargetPlatformIdentifier('`$(TargetFramework)')) == 'windows' and '`$(RuntimeIdentifierOverride)' != ''">
		<RuntimeIdentifier>`$(RuntimeIdentifierOverride)</RuntimeIdentifier>
	</PropertyGroup>
"@

    $content = $content -replace "</Project>\s*$", "$runtimeIdentifierOverridePropertyGroup`r`n</Project>"
}

$dotNetMajorVersion = Get-DotNetMajorVersion $DotNetTfm
if ($dotNetMajorVersion -and $dotNetMajorVersion -ge 11 -and (Test-UsesImplicitXamlXmlns $projectDir)) {
    Write-Host "Generated XAML uses implicit xmlns declarations; enabling MAUI implicit xmlns compatibility."
    $content = Add-ProjectDefineConstant $content "MauiAllowImplicitXmlnsDeclaration"
    $content = Set-ProjectProperty $content "EnablePreviewFeatures" "true"
}

$sdkImport = '<Import Project="' + (ConvertTo-XmlEscaped $sdkTargets) + '" />'
$content = [regex]::Replace($content, '</Project>\s*$', { "$sdkImport`r`n</Project>" })
Set-Content -Path $projectFile.FullName -Value $content -Encoding utf8
$sourceManifest | ConvertTo-Json -Depth 20 |
    Set-Content (Join-Path $projectDir 'source-packages.json') -Encoding utf8
$resourceDirectory = Join-Path $projectDir 'Resources/Raw'
New-Item -ItemType Directory -Path $resourceDirectory -Force | Out-Null
[ordered]@{
    sourceSha = $SourceSha
    frameworkVersion = $sourceManifest.version
    template = $templateInfo
    workflowCommit = $env:GITHUB_SHA
    runId = $env:GITHUB_RUN_ID
} | ConvertTo-Json -Depth 10 |
    Set-Content (Join-Path $resourceDirectory 'source-provenance.json') -Encoding utf8

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
