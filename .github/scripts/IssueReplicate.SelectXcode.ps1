#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$RepoRoot,
    [ValidatePattern('^[0-9]+\.[0-9]+$')][string]$SdkVersion = '',
    [string]$SdkPackDirectory = '',
    [ValidateSet('None', 'Azure', 'GitHub')][string]$Provider = 'None'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$sdk = if ($SdkVersion) { $SdkVersion } else { Get-IssueReplicateIOSSdkVersion -RepoRoot $RepoRoot }
if (-not $SdkPackDirectory) {
    [xml]$details = Get-Content -Raw -LiteralPath (Join-Path $RepoRoot 'eng/Version.Details.xml')
    $dependencies = @($details.SelectNodes('//Dependency') | Where-Object {
            $_.Name -cmatch "^Microsoft\.iOS\.Sdk\.net[0-9]+\.0_$([regex]::Escape($sdk))$" -and
            -not $_.HasAttribute('CoherentParentDependency')
        })
    if ($dependencies.Count -ne 1) { throw 'The pinned branch must declare exactly one primary matching iOS SDK pack.' }
    $dependency = $dependencies[0]
    if ($dependency.Version -cnotmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[A-Za-z0-9.-]+)?$') {
        throw 'The pinned iOS SDK pack version is invalid.'
    }
    $SdkPackDirectory = Join-Path $RepoRoot ".dotnet/packs/$($dependency.Name)/$($dependency.Version)"
}
$pack = Get-Item -LiteralPath $SdkPackDirectory -Force
if (-not $pack.PSIsContainer -or $pack.Attributes -band [IO.FileAttributes]::ReparsePoint -or
    $pack.Parent.Name -cnotmatch "^Microsoft\.iOS\.Sdk\.net[0-9]+\.[0-9]+_$([regex]::Escape($sdk))$") {
    throw 'Xcode selection requires the actual installed matching iOS SDK pack.'
}
$metadata = Get-Item -LiteralPath (Join-Path $pack.FullName 'targets/Microsoft.iOS.Sdk.Versions.props') -Force
if ($metadata.PSIsContainer -or $metadata.Attributes -band [IO.FileAttributes]::ReparsePoint -or
    $metadata.Length -gt 32KB) {
    throw 'The installed iOS SDK requirement metadata must be a bounded regular file.'
}
$metadataHash = (Get-FileHash -LiteralPath $metadata.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "Installed iOS SDK $($pack.Parent.Name)/$($pack.Name); requirement metadata SHA256 $metadataHash."
$settings = [Xml.XmlReaderSettings]::new()
$settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
$settings.XmlResolver = $null
$settings.MaxCharactersInDocument = 32KB
$reader = [Xml.XmlReader]::Create($metadata.FullName, $settings)
try {
    $versions = [Xml.XmlDocument]::new()
    $versions.XmlResolver = $null
    $versions.Load($reader)
}
finally { $reader.Dispose() }
$declarations = @($versions.SelectNodes("/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='RecommendedXcodeVersion' or local-name()='_RecommendedXcodeVersion']"))
$requirements = @($declarations | Where-Object { $_.InnerText.Trim() -cmatch '^[0-9]+\.[0-9]+(?:\.[0-9]+){0,2}$' })
$aliases = @($declarations | Where-Object {
        $_.LocalName -ceq '_RecommendedXcodeVersion' -and $_.InnerText.Trim() -ceq '$(RecommendedXcodeVersion)'
    })
if ($declarations.Count -lt 1 -or $declarations.Count -gt 2 -or $requirements.Count -ne 1 -or
    $aliases.Count -ne $declarations.Count - 1 -or
    ($aliases.Count -eq 1 -and $requirements[0].LocalName -cne 'RecommendedXcodeVersion')) {
    throw 'The installed iOS SDK must declare one literal RecommendedXcodeVersion or legacy _RecommendedXcodeVersion, with only its exact supported alias.'
}
$required = [version]$requirements[0].InnerText.Trim()
$xcode = "$($required.Major).$($required.Minor)"
$choices = @(Get-ChildItem -LiteralPath /Applications -Directory -Filter 'Xcode_*.app' | ForEach-Object {
        if ($_.Name -cmatch "^Xcode_($([regex]::Escape($xcode))(?:\.[0-9]+)?)\.app$") {
            [pscustomobject]@{ Path = $_.FullName; Version = [version]$Matches[1] }
        }
    } | Sort-Object Version -Descending)
if ($choices.Count -lt 1) { throw "The installed iOS SDK $($pack.Parent.Name)/$($pack.Name) requires Xcode $required, which is unavailable on this runner; refusing a substitution." }
$selected = $choices[0]
$developer = Join-Path $selected.Path 'Contents/Developer'
if (-not (Test-Path -LiteralPath $developer -PathType Container)) { throw 'The selected Xcode is incomplete.' }
Write-Host "Installed requirement $($requirements[0].LocalName)=$required; selected Xcode $($selected.Version); exact iOS runtime remains $sdk."
switch ($Provider) {
    'Azure' {
        Write-Host "##vso[task.setvariable variable=XCODE]$($selected.Version)"
        Write-Host "##vso[task.setvariable variable=REQUIRED_XCODE]$($selected.Version)"
    }
    'GitHub' {
        if (-not $env:GITHUB_ENV) { throw 'The GitHub environment file is unavailable.' }
        [IO.File]::AppendAllText($env:GITHUB_ENV, "DEVELOPER_DIR=$developer`n", [Text.UTF8Encoding]::new($false))
    }
    default { $developer }
}
