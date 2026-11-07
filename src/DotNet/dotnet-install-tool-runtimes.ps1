[CmdletBinding()]
param(
    [string] $DotNetDirectory = $env:MAUI_TOOL_RUNTIME_DIRECTORY,
    [Parameter(Mandatory)][string] $GlobalJsonPath,
    [Parameter(Mandatory)][string] $VersionsPropsPath,
    [string] $Platform = 'AnyCPU'
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($DotNetDirectory)) {
    throw 'The tool runtime installation directory is missing.'
}
. (Join-Path $PSScriptRoot '../../eng/common/tools.ps1')
$runtimeConfiguration = Get-Content -LiteralPath $GlobalJsonPath -Raw | ConvertFrom-Json
$runtimes = $runtimeConfiguration.tools.PSObject.Properties['runtimes']

if ($null -eq $runtimes -or $null -eq $runtimes.Value) {
    Write-Host 'No tool runtimes are declared in global.json.'
    return
}

foreach ($runtimeProperty in $runtimes.Value.PSObject.Properties) {
    if ($runtimeProperty.Value -isnot [Array]) {
        throw "Tool runtime '$($runtimeProperty.Name)' must declare an array of versions."
    }
    $parts = $runtimeProperty.Name.Split('/', 2)
    $runtime = $parts[0]
    $architecture = if ($parts.Length -eq 2) {
        $parts[1]
    } elseif ($Platform -and $Platform -ne 'AnyCPU') {
        $Platform
    } elseif ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString() -in @('X86', 'X64')) {
        'x64'
    } else {
        ''
    }

    foreach ($declaredVersion in $runtimeProperty.Value) {
        if ([string]::IsNullOrWhiteSpace($declaredVersion)) {
            throw "Tool runtime '$runtime' has an empty version."
        }
        $version = $declaredVersion
        if ($version -notmatch '^\d+(?:\.\d+){0,3}(?:-[A-Za-z0-9.-]+)?(?:\+[A-Za-z0-9.-]+)?$') {
            $propertyName = $version.Trim('$', '(', ')')
            if ($propertyName -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
                throw "Invalid runtime version or MSBuild property: $version."
            }
            $dotnet = Join-Path $DotNetDirectory (GetExecutableFileName 'dotnet')
            $msbuild = Join-Path $DotNetDirectory "sdk/$($runtimeConfiguration.tools.dotnet)/MSBuild.dll"
            $version = (& $dotnet $msbuild $VersionsPropsPath "-getProperty:$propertyName" -nologo -verbosity:quiet) -join [Environment]::NewLine
            if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($version)) {
                throw "Unable to resolve runtime version property '$propertyName' from $VersionsPropsPath."
            }
            $version = $version.Trim()
        }

        $installDirectory = if ($architecture -eq 'x86') {
            Join-Path $DotNetDirectory 'x86'
        } else {
            $DotNetDirectory
        }
        $sharedDirectory = switch ($runtime) {
            'dotnet' { 'Microsoft.NETCore.App' }
            'aspnetcore' { 'Microsoft.AspNetCore.App' }
            'windowsdesktop' { 'Microsoft.WindowsDesktop.App' }
            default { throw "Unsupported tool runtime '$runtime'." }
        }
        $runtimeDirectory = Join-Path $installDirectory "shared/$sharedDirectory/$version"
        if (Test-Path -LiteralPath $runtimeDirectory -PathType Container) {
            Write-Host "Runtime toolset '$runtime/$architecture v$version' already installed in '$runtimeDirectory'."
            continue
        }

        InstallDotNet $installDirectory $version $architecture $runtime $true
    }
}
