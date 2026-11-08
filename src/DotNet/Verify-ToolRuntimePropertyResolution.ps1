[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $DotNetDirectory,
    [Parameter(Mandatory)][string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$configuration = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../global.json') -Raw | ConvertFrom-Json
$version = $configuration.tools.runtimes.dotnet[0]
if ($version -notmatch '\A\d+(?:\.\d+){0,3}(?:-[A-Za-z0-9.-]+)?(?:\+[A-Za-z0-9.-]+)?\z') {
    throw "The property-resolution control needs a literal declared runtime version, not '$version'."
}
$expectedDirectory = Join-Path $DotNetDirectory "shared/Microsoft.NETCore.App/$version"
if (-not [System.IO.Directory]::Exists($expectedDirectory)) {
    throw "The property-resolution control requires the installed runtime in '$expectedDirectory'."
}

[System.IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
$globalJsonPath = Join-Path $OutputDirectory 'property-global.json'
$propsPath = Join-Path $OutputDirectory 'runtime-version.props'
$configuration.tools.runtimes = [pscustomobject]@{ dotnet = @('$(ResolvedToolRuntimeVersion)') }
$configuration | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $globalJsonPath
$installer = Join-Path $PSScriptRoot 'dotnet-install-tool-runtimes.ps1'

foreach ($phase in @('valid', 'traversal')) {
    $resolvedVersion = if ($phase -eq 'valid') { $version } else { "../Microsoft.NETCore.App/$version" }
    "<Project><PropertyGroup><ResolvedToolRuntimeVersion>$resolvedVersion</ResolvedToolRuntimeVersion></PropertyGroup></Project>" |
        Set-Content -LiteralPath $propsPath
    $logPath = Join-Path $OutputDirectory "$phase.log"
    & pwsh -NoProfile -File $installer -DotNetDirectory $DotNetDirectory `
        -GlobalJsonPath $globalJsonPath -VersionsPropsPath $propsPath 2>&1 | Tee-Object -FilePath $logPath
    $exitCode = $LASTEXITCODE
    $log = [System.IO.File]::ReadAllText($logPath)
    if ($phase -eq 'valid') {
        if ($exitCode -ne 0 -or -not $log.Contains("already installed in '$expectedDirectory'.", [StringComparison]::Ordinal)) {
            throw 'The valid MSBuild property did not resolve to the intended literal runtime directory.'
        }
    } elseif ($exitCode -eq 0 -or
        -not $log.Contains("Invalid resolved tool runtime version: $resolvedVersion.", [StringComparison]::Ordinal) -or
        $log.Contains('already installed', [StringComparison]::Ordinal)) {
        throw 'The path-containing MSBuild property was not rejected before installation.'
    }
}
