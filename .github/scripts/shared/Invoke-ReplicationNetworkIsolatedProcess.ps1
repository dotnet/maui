#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('android')]
    [string]$Platform,
    [Parameter(Mandatory = $true)][string]$RepositoryRoot,
    [Parameter(Mandatory = $true)][string]$ScriptPath,
    [Parameter(Mandatory = $true)][string]$ArgumentPayload,
    [Parameter(Mandatory = $true)][string]$DeviceUdid
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
        @{ File = '/usr/bin/systemd-run'; Arguments = @('--user', '--quiet', '/usr/bin/true'); Name = 'systemd user manager' }
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
    }
}

$root = [IO.Path]::GetFullPath($RepositoryRoot)
$target = [IO.Path]::GetFullPath($ScriptPath)
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
    $null = Assert-ReplicationAndroidGuestNetworkIsolation -DeviceUdid $DeviceUdid
    $null = Assert-ReplicationOutboundNetworkIsolation `
        -Platform $Platform `
        -RepositoryRoot $root
} finally {
    [Environment]::SetEnvironmentVariable('MAUI_REPLICATION_EGRESS_ISOLATED', $null)
}

& (Get-Command pwsh -ErrorAction Stop).Source `
    -NoLogo -NoProfile -NonInteractive -File $target @childArguments
exit $LASTEXITCODE
