#requires -Version 5
<#
.SYNOPSIS
    Launches a packaged Windows app via IApplicationActivationManager and waits for it to exit.

.DESCRIPTION
    Unlike `Start-Process shell:AppsFolder\...`, this script obtains the actual PID of the launched
    packaged app and waits for it to exit. This avoids racing against asynchronous Application.Exit()
    when external pollers want to read the test results file.

    Exit codes:
      0 - App launched and exited cleanly
      2 - Timeout waiting for app to exit (process was killed)
      3 - Other launch/wait failure
#>
param(
    [Parameter(Mandatory=$true)][string]$PackageName,
    [Parameter(Mandatory=$true)][string]$AppArguments,
    [int]$TimeoutSeconds = 480
)

$ErrorActionPreference = 'Stop'

$pkg = Get-AppxPackage -Name $PackageName -ErrorAction Stop
if (-not $pkg) { throw "Package '$PackageName' is not installed." }
$aumid = "$($pkg.PackageFamilyName)!App"

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IApplicationActivationManager {
    int ActivateApplication([In, MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [In, MarshalAs(UnmanagedType.LPWStr)] string arguments,
        [In] uint options,
        [Out] out uint processId);
}
[ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
public class ApplicationActivationManager { }

public static class PackagedAppLauncher {
    public static uint Launch(string aumid, string arguments) {
        var mgr = (IApplicationActivationManager)(new ApplicationActivationManager());
        uint pid;
        int hr = mgr.ActivateApplication(aumid, arguments ?? string.Empty, 0, out pid);
        if (hr != 0) {
            throw new System.ComponentModel.Win32Exception(hr,
                string.Format("ActivateApplication failed (HRESULT=0x{0:X8})", hr));
        }
        return pid;
    }
}
"@ -ErrorAction SilentlyContinue | Out-Null

[uint32]$procId = 0
try {
    $procId = [PackagedAppLauncher]::Launch($aumid, $AppArguments)
} catch {
    Write-Error "ActivateApplication threw: $_"
    exit 3
}
if ($procId -eq 0) {
    Write-Error "ActivateApplication returned PID 0"
    exit 3
}
Write-Host "Launched $aumid (PID $procId) with args: $AppArguments"

try {
    $proc = Get-Process -Id $procId -ErrorAction Stop
} catch {
    Write-Error "App process $procId disappeared immediately after launch: $_"
    exit 3
}

if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
    Write-Warning "Timed out after ${TimeoutSeconds}s waiting for PID $procId to exit; killing it."
    try { Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue } catch {}
    exit 2
}

Write-Host "App PID $procId exited with code $($proc.ExitCode)"
exit 0
