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

function Install-ToolRuntimeLiteral([string] $installDirectory, [string] $version, [string] $architecture, [string] $runtime) {
    $temporaryRoot = [System.IO.Path]::GetTempPath()
    if ($temporaryRoot -match '[\[\]*?`]') {
        throw 'The shared installer requires a staging TEMP path without PowerShell wildcard characters.'
    }
    $stagingDirectory = Join-Path $temporaryRoot ('maui-tool-runtime-' + [Guid]::NewGuid().ToString('N'))
    try {
        [System.IO.Directory]::CreateDirectory($stagingDirectory) | Out-Null
        InstallDotNet $stagingDirectory $version $architecture $runtime $true
        [System.IO.Directory]::CreateDirectory($installDirectory) | Out-Null
        foreach ($directory in [System.IO.Directory]::GetDirectories($stagingDirectory, '*', [System.IO.SearchOption]::AllDirectories)) {
            $relative = $directory.Substring($stagingDirectory.Length + 1)
            [System.IO.Directory]::CreateDirectory((Join-Path $installDirectory $relative)) | Out-Null
        }
        foreach ($file in [System.IO.Directory]::GetFiles($stagingDirectory, '*', [System.IO.SearchOption]::AllDirectories)) {
            $relative = $file.Substring($stagingDirectory.Length + 1)
            if ($relative -eq 'dotnet-install.ps1') {
                continue
            }
            $destination = Join-Path $installDirectory $relative
            $normalized = $relative.Replace('\', '/')
            $versioned = $normalized.StartsWith('shared/', [StringComparison]::OrdinalIgnoreCase) -or
                $normalized.StartsWith('host/fxr/', [StringComparison]::OrdinalIgnoreCase)
            if ($versioned -or -not [System.IO.File]::Exists($destination)) {
                [System.IO.File]::Copy($file, $destination, $versioned)
            }
        }
    } finally {
        if ([System.IO.Directory]::Exists($stagingDirectory)) {
            Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
        }
    }
}

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
            if ([System.IO.Directory]::GetFiles($runtimeDirectory).Length -eq 0) {
                throw "The existing tool runtime directory '$runtimeDirectory' is empty."
            }
            Write-Host "Runtime toolset '$runtime/$architecture v$version' already installed in '$runtimeDirectory'."
            continue
        }

        Install-ToolRuntimeLiteral $installDirectory $version $architecture $runtime
        if (-not [System.IO.Directory]::Exists($runtimeDirectory) -or
            [System.IO.Directory]::GetFiles($runtimeDirectory).Length -eq 0) {
            throw "The requested tool runtime was not installed at the literal directory '$runtimeDirectory'."
        }
    }
}
