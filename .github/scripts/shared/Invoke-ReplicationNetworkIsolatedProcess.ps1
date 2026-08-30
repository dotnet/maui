#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('android')]
    [string]$Platform,
    [Parameter(Mandatory = $true)][string]$RepositoryRoot,
    [Parameter(Mandatory = $true)][string]$ScriptPath,
    [Parameter(Mandatory = $true)][string]$ArgumentPayload,
    [Parameter(Mandatory = $true)][string]$DeviceUdid,
    [Parameter(Mandatory = $true)]
    [ValidateSet('true', 'false')]
    [string]$AllowDeviceControl
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$assertionPath = Join-Path $PSScriptRoot 'Assert-ReplicationExecutionEnvironment.ps1'
if (-not (Test-Path -LiteralPath $assertionPath -PathType Leaf)) {
    throw 'Trusted replication execution-environment assertion is missing.'
}
. $assertionPath

function Assert-ReplicationPrivilegeEscapesBlocked {
    if (-not [OperatingSystem]::IsLinux()) {
        return
    }

    foreach ($probe in @(
        @{ File = '/usr/bin/sudo'; Arguments = @('-n', 'true'); Name = 'sudo' },
        @{ File = '/usr/bin/systemd-run'; Arguments = @('--user', '--quiet', '/usr/bin/true'); Name = 'systemd user manager' },
        @{ File = '/usr/bin/docker'; Arguments = @('version'); Name = 'Docker daemon' },
        @{ File = '/usr/bin/podman'; Arguments = @('version'); Name = 'Podman runtime' },
        @{ File = '/usr/bin/ctr'; Arguments = @('version'); Name = 'containerd' },
        @{ File = '/usr/bin/nerdctl'; Arguments = @('version'); Name = 'nerdctl' }
    )) {
        $escaped = $false
        try {
            & $probe.File @($probe.Arguments) 2>$null | Out-Null
            $escaped = $LASTEXITCODE -eq 0
        } catch {
            $escaped = $false
        }
        if ($escaped) {
            throw "Generated execution isolation allowed escape through $($probe.Name)."
        }
        foreach ($socket in @(
            '/run/docker.sock',
            '/var/run/docker.sock',
            '/run/containerd/containerd.sock',
            '/run/podman/podman.sock',
            '/run/buildkit/buildkitd.sock'
        )) {
            if (-not (Test-Path -LiteralPath $socket)) {
                continue
            }
            $probeSocket = [Net.Sockets.Socket]::new(
                [Net.Sockets.AddressFamily]::Unix,
                [Net.Sockets.SocketType]::Stream,
                [Net.Sockets.ProtocolType]::Unspecified)
            try {
                $probeSocket.Connect([Net.Sockets.UnixDomainSocketEndPoint]::new($socket))
                if ($probeSocket.Connected) {
                    throw "Generated execution isolation exposed privileged socket $socket."
                }
            } catch {
                if ($_.Exception.Message -match 'exposed privileged socket') { throw }
            } finally {
                $probeSocket.Dispose()
            }
        }
    }
}

$root = [IO.Path]::GetFullPath($RepositoryRoot)
$target = [IO.Path]::GetFullPath($ScriptPath)
$trustedRoot = [IO.Path]::GetFullPath(
    (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
if (-not (Test-Path -LiteralPath $root -PathType Container) -or
    -not (Test-Path -LiteralPath $target -PathType Leaf)) {
    throw 'Network-isolated execution received an invalid repository or script path.'
}

try {
    $argumentJson = [Text.UTF8Encoding]::new($false, $true).GetString(
        [Convert]::FromBase64String($ArgumentPayload))
    $decoded = ConvertFrom-Json -InputObject $argumentJson -NoEnumerate
    $childArguments = @($decoded | ForEach-Object { [string]$_ })
} catch {
    throw 'Network-isolated execution received malformed arguments.'
}
if ($childArguments.Count -gt 128 -or
    @($childArguments | Where-Object { $_.Length -gt 4096 }).Count -gt 0) {
    throw 'Network-isolated execution arguments exceed the trusted bounds.'
}

[Environment]::SetEnvironmentVariable('MAUI_REPLICATION_EGRESS_ISOLATED', '1')
try {
    Assert-ReplicationPrivilegeEscapesBlocked
    if ($AllowDeviceControl -ceq 'false') {
        $adb = Get-Command adb -ErrorAction SilentlyContinue
        if ($adb) {
            try {
                & $adb.Source version 2>$null | Out-Null
                if ($LASTEXITCODE -eq 0) {
                    throw 'Generated execution isolation exposed adb control.'
                }
            } catch {
                if ($_.Exception.Message -match 'exposed adb control') { throw }
            }
        }
    }
    $null = Assert-ReplicationOutboundNetworkIsolation `
        -Platform $Platform `
        -RepositoryRoot $root `
        -TrustedRoot $trustedRoot
} finally {
    [Environment]::SetEnvironmentVariable('MAUI_REPLICATION_EGRESS_ISOLATED', $null)
}

& (Get-Command pwsh -ErrorAction Stop).Source `
    -NoLogo -NoProfile -NonInteractive -File $target @childArguments
exit $LASTEXITCODE
