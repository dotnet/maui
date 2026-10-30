<#
.SYNOPSIS
    Opens a solution in Visual Studio on the repo's provisioned .dotnet SDK (lightweight `dotnet cake --target=VS`).

.DESCRIPTION
    global.json makes Visual Studio pick the SDK in .dotnet, but a plain launch reads the workload manifests from the
    machine-wide dotnet (C:\Program Files\dotnet), whose versions do not match the packs in .dotnet: every project with a
    mobile TFM then fails with MSB4236 (Microsoft.Android.Sdk.net10, Microsoft.iOS.Windows.Sdk.Aliased.*,
    Microsoft.NET.Runtime.MonoTargets.Sdk.net10). DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR makes the resolver use .dotnet for both.
    Sets the same variables as SetDotNetEnvironmentVariables in eng/cake/dotnet.cake, builds the build tasks (needed before
    opening the solution, and done here so Visual Studio's MSBuild nodes never lock .buildtasks while it rebuilds them),
    then starts devenv. Unlike the cake target it does not clean, provision or pack.

.PARAMETER Solution
    Solution or filter to open, relative to the repo root. Default: Microsoft.Maui-windows.slnf.

.PARAMETER SkipBuildTasks
    Do not build Microsoft.Maui.BuildTasks.slnf first.

.PARAMETER Prerelease
    Allow a prerelease Visual Studio.

.EXAMPLE
    pwsh eng/scripts/Start-VisualStudio.ps1 -Solution Microsoft.Maui.sln
#>
param(
    [string]$Solution = 'Microsoft.Maui-windows.slnf',
    [switch]$SkipBuildTasks,
    [switch]$Prerelease
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '../..')
$dotnet = Join-Path $root '.dotnet'
$dotnetExe = Join-Path $dotnet 'dotnet.exe'
if (-not (Test-Path $dotnetExe)) {
    throw "$dotnetExe not found: provision the SDK first (./build.cmd -restore)."
}

$env:DOTNET_INSTALL_DIR = $dotnet
$env:DOTNET_ROOT = $dotnet
$env:DOTNET_HOST_PATH = $dotnetExe
$env:DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR = $dotnet
$env:DOTNET_MULTILEVEL_LOOKUP = '0'
$env:DOTNET_SYSTEM_NET_SECURITY_NOREVOCATIONCHECKBYDEFAULT = 'true'
$env:MSBuildEnableWorkloadResolver = 'true'
$env:VSDebugger_ValidateDotnetDebugLibSignatures = '0'
$env:PATH = "$dotnet;$env:PATH"

if (-not $SkipBuildTasks) {
    Push-Location $root
    try {
        & $dotnetExe tool restore
        & $dotnetExe build ./Microsoft.Maui.BuildTasks.slnf
        if ($LASTEXITCODE -ne 0) { throw "Building Microsoft.Maui.BuildTasks.slnf failed ($LASTEXITCODE)." }
        # Release the build-server nodes that loaded the tasks, so the next build can overwrite .buildtasks.
        & $dotnetExe build-server shutdown | Out-Null
    }
    finally { Pop-Location }
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vswhereArgs = @('-latest', '-property', 'productPath')
if ($Prerelease) { $vswhereArgs = @('-prerelease') + $vswhereArgs }
$devenv = & $vswhere @vswhereArgs
if (-not $devenv) { throw 'Visual Studio not found (vswhere).' }

Start-Process $devenv -ArgumentList "`"$(Join-Path $root $Solution)`"" -WorkingDirectory $root
