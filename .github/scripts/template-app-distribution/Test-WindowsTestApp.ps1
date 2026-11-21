#Requires -Version 5.1
#Requires -RunAsAdministrator

param(
    [Parameter(Mandatory)][string]$Path,
    [switch]$AllowDisposableRunnerTrust
)

$ErrorActionPreference = 'Stop'
if (-not $AllowDisposableRunnerTrust -or $env:GITHUB_ACTIONS -ne 'true' -or
    $env:RUNNER_ENVIRONMENT -ne 'github-hosted' -or $env:OS -ne 'Windows_NT') {
    throw 'Trust/install verification is permitted only with explicit consent on a disposable GitHub-hosted Windows runner.'
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
. "$PSScriptRoot/Windows-Msix.ps1"
$data = Get-Content (Join-Path $Path 'install.json') -Raw | ConvertFrom-Json
$packagePath = Join-Path $Path $data.package.file
$certificatePath = Join-Path $Path $data.certificate.file
$certificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new($certificatePath)
Assert-WindowsTestCertificate $certificate $data.publisher
if ($certificate.HasPrivateKey -or $certificate.Thumbprint -cne $data.certificate.thumbprint) {
    throw 'Expected the public-only certificate matching this package.'
}
foreach ($record in @($data.package, $data.certificate) + @($data.dependencies)) {
    if ((Get-FileHash (Join-Path $Path $record.file)).Hash -cne $record.sha256) { throw 'Installer payload hash mismatch.' }
}
$info = Get-WindowsPackageInfo $packagePath
if ($info.Publisher -cne $certificate.Subject -or $info.Name -cne $data.identity) { throw 'MSIX identity mismatch.' }
$tool = Get-WindowsSignTool
$trustPath = "Cert:\LocalMachine\TrustedPeople\$($certificate.Thumbprint)"
if (Test-Path $trustPath) { throw 'Disposable runner unexpectedly already trusts this test certificate.' }
if (Get-AppxPackage -Name $info.Name) { throw 'Disposable runner unexpectedly already has this app installed.' }
$result = [ordered]@{ signatureVerified = $false; installed = $false; launched = $false; error = $null; os = [Environment]::OSVersion.Version.ToString() }
$process = $null
$addedTrust = $false
$installed = $null
try {
    Import-Certificate -FilePath $certificatePath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' | Out-Null
    $addedTrust = $true
    $negativeDirectory = Join-Path (Split-Path $Path) 'signature-negative'
    New-Item -ItemType Directory $negativeDirectory | Out-Null
    try {
        foreach ($case in @('unsigned', 'tampered')) {
            $negative = Join-Path $negativeDirectory "$case.msix"
            Copy-Item $packagePath $negative
            $zip = [IO.Compression.ZipFile]::Open($negative, [IO.Compression.ZipArchiveMode]::Update)
            try {
                if ($case -eq 'unsigned') { $zip.GetEntry('AppxSignature.p7x').Delete() }
                else {
                    $zip.GetEntry('AppxManifest.xml').Delete()
                    $writer = [IO.StreamWriter]::new($zip.CreateEntry('AppxManifest.xml').Open())
                    try { $writer.Write('<Package>Tampered payload</Package>') } finally { $writer.Dispose() }
                }
            } finally { $zip.Dispose() }
            & $tool verify /pa $negative | Out-Host
            if ($LASTEXITCODE -eq 0) { throw "Signature verification unexpectedly accepted $case MSIX." }
            $result["rejects_$case"] = $true
        }
    } finally { Remove-Item $negativeDirectory -Recurse -Force }
    foreach ($record in @($data.package) + @($data.dependencies)) {
        & $tool verify /pa /all (Join-Path $Path $record.file) | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Windows signature verification failed: $($record.file)." }
    }
    $signature = Get-AuthenticodeSignature $packagePath
    Assert-WindowsPackageSignature $signature $certificate.Thumbprint
    $result.signatureVerified = $true
    if ($env:GITHUB_OUTPUT) { 'signature_verified=true' >> $env:GITHUB_OUTPUT }
    $arguments = @{ Path = $packagePath; ErrorAction = 'Stop' }
    if ($data.dependencies.Count) { $arguments.DependencyPath = @($data.dependencies | ForEach-Object { Join-Path $Path $_.file }) }
    Add-AppxPackage @arguments
    $installed = Get-AppxPackage -Name $info.Name
    if (-not $installed -or $installed.Version -ne $info.Version.ToString()) { throw 'MSIX was not registered at the expected version.' }
    $result.installed = $true
    $result['packageFullName'] = $installed.PackageFullName

    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class PackagedAppLauncher {
    [ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IApplicationActivationManager {
        [PreserveSig] int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string id,
            [MarshalAs(UnmanagedType.LPWStr)] string arguments, uint options, out uint processId);
    }
    public static uint Launch(string id) {
        var type = Type.GetTypeFromCLSID(new Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C"));
        var manager = (IApplicationActivationManager)Activator.CreateInstance(type);
        try {
            uint pid;
            Marshal.ThrowExceptionForHR(manager.ActivateApplication(id, null, 0, out pid));
            return pid;
        } finally { Marshal.ReleaseComObject(manager); }
    }
}
'@
    $applicationId = "$($installed.PackageFamilyName)!$($info.Applications[0].Id)"
    $savedTokens = @{}
    try {
        foreach ($name in @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')) {
            $savedTokens[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        $launchedPid = [PackagedAppLauncher]::Launch($applicationId)
    } finally {
        foreach ($name in $savedTokens.Keys) { [Environment]::SetEnvironmentVariable($name, $savedTokens[$name]) }
    }
    $process = Get-Process -Id $launchedPid -ErrorAction Stop
    Start-Sleep -Seconds 15
    $process.Refresh()
    if ($process.HasExited) { throw 'Installed app exited during the launch check.' }
    if ($process.MainModule.FileName -notlike "$($installed.InstallLocation)\*") {
        throw 'Launched process is not from the installed package.'
    }
    $result.launched = $true
    $result['processId'] = $launchedPid
    $result['windowTitle'] = $process.MainWindowTitle
    $result['hasWindow'] = $process.MainWindowHandle -ne [IntPtr]::Zero
    Write-Host "Installed and launched $applicationId (PID $launchedPid)."
} catch {
    $result.error = $_.Exception.Message
    throw
} finally {
    $result | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $Path 'validation.json')
    if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id -ErrorAction Stop }
    if ($installed) { Remove-AppxPackage -Package $installed.PackageFullName -ErrorAction Stop }
    if ($addedTrust) { Remove-Item $trustPath -ErrorAction Stop }
    $certificate.Dispose()
}
