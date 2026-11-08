[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Prepare', 'Install', 'Cleanup')]
    [string] $Action,
    [Parameter(Mandatory)]
    [string] $StatePath,
    [string] $ProjectPath,
    [string] $PackageDirectory,
    [ValidateRange(1, 86400)]
    [int] $TimeoutSeconds = 3600
)

$ErrorActionPreference = 'Stop'

function Import-WindowsTestAppxModule {
    param([string] $Edition = $PSVersionTable.PSEdition)
    if ($Edition -eq 'Core') {
        Import-Module Appx -UseWindowsPowerShell -Global -ErrorAction Stop
    }
    else {
        Import-Module Appx -Global -ErrorAction Stop
    }
}

function Write-WindowsTestPackageState {
    param([string] $Path, [System.Collections.IDictionary] $State)
    $pending = "$Path.$([guid]::NewGuid().ToString('N')).next"
    try {
        [IO.File]::WriteAllText($pending, ($State | ConvertTo-Json -Depth 8))
        [IO.File]::Move($pending, $Path, $true)
    }
    finally {
        if (Test-Path -LiteralPath $pending) { Remove-Item -LiteralPath $pending -Force }
    }
}

function Assert-WindowsTestPackageOwner {
    param([System.Collections.IDictionary] $State)
    $owner = [guid]::Empty
    if (![guid]::TryParse($State.OwnerId, [ref]$owner) -or
        $State.IdentityName -notmatch '^[A-Za-z0-9][A-Za-z0-9.-]{2,49}$' -or
        $State.Publisher -cne "CN=MauiTests-$($owner.ToString('N'))") {
        throw 'Invalid or unowned Windows package session.'
    }
}

function Read-WindowsTestPackageManifest {
    param([string] $Path, [switch] $Archive)
    $zip = $null
    $stream = $null
    $reader = $null
    try {
        if ($Archive) {
            $zip = [IO.Compression.ZipFile]::OpenRead($Path)
            $entry = $zip.GetEntry('AppxManifest.xml')
            if (!$entry) { throw "No AppxManifest.xml in '$Path'." }
            $stream = $entry.Open()
        }
        else { $stream = [IO.File]::OpenRead($Path) }
        $settings = [Xml.XmlReaderSettings]::new()
        $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $reader = [Xml.XmlReader]::Create($stream, $settings)
        $document = [Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
        return $document
    }
    finally {
        if ($reader) { $reader.Dispose() }
        if ($stream) { $stream.Dispose() }
        if ($zip) { $zip.Dispose() }
    }
}

function New-WindowsTestCertificate {
    param([string] $Publisher, [string] $OwnerId)
    New-SelfSignedCertificate -Type Custom -Subject $Publisher -KeyUsage DigitalSignature `
        -FriendlyName "MAUI test $OwnerId" -CertStoreLocation Cert:\CurrentUser\My `
        -KeyExportPolicy NonExportable `
        -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
}

function Trust-WindowsTestCertificate {
    param($Certificate)
    $store = [Security.Cryptography.X509Certificates.X509Store]::new('TrustedPeople', 'CurrentUser')
    $publicCertificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new($Certificate.RawData)
    try {
        $store.Open('ReadWrite')
        $store.Add($publicCertificate)
    }
    finally {
        $publicCertificate.Dispose()
        $store.Dispose()
    }
}

function Initialize-WindowsTestPackage {
    param([string] $Path, [string] $Project, [int] $TimeoutSeconds)
    if (Test-Path -LiteralPath $Path) { throw 'Refusing to overwrite an existing package session.' }
    New-Item -ItemType Directory -Force -Path (Split-Path $Path -Parent) | Out-Null
    $manifestPath = Join-Path (Split-Path $Project -Parent) 'Platforms/Windows/Package.appxmanifest'
    $manifest = Read-WindowsTestPackageManifest $manifestPath
    $projectXml = Read-WindowsTestPackageManifest $Project
    $applicationId = $projectXml.SelectSingleNode('/Project/PropertyGroup/ApplicationId').InnerText
    if ($applicationId -notmatch '^[A-Za-z0-9][A-Za-z0-9.-]{2,49}$') { throw 'Expected a literal Windows application ID.' }
    $owner = [guid]::NewGuid()
    $state = @{
        OwnerId = $owner.ToString('N'); IdentityName = $applicationId
        Publisher = "CN=MauiTests-$($owner.ToString('N'))"; CertificateThumbprint = ''; InstallRequested = $false
        PackageFullName = ''; PackageFamilyName = ''; InstallLocation = ''
        ApplicationId = 'App'; AppUserModelId = ''; TimeoutSeconds = $TimeoutSeconds
        ProcessId = 0; ProcessStartUtcTicks = 0; ProcessIdentityVerified = $false; ProcessExited = $false; ExitCode = 0
    }
    Write-WindowsTestPackageState $Path $state
    $identity = $manifest.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']")
    $identity.SetAttribute('Name', $state.IdentityName)
    $identity.SetAttribute('Publisher', $state.Publisher)
    $manifest.Save((Join-Path (Split-Path $Path -Parent) 'Package.appxmanifest'))
    $certificate = New-WindowsTestCertificate -Publisher $state.Publisher -OwnerId $state.OwnerId
    $state.CertificateThumbprint = $certificate.Thumbprint
    Write-WindowsTestPackageState $Path $state
    Trust-WindowsTestCertificate $certificate
    return $state
}

function Install-WindowsTestPackage {
    param([string] $Path, [string] $Directory)
    $state = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
    Assert-WindowsTestPackageOwner $state
    $packages = @(Get-ChildItem -LiteralPath $Directory -Recurse -File |
        Where-Object { $_.Extension -in @('.msix', '.appx') -and $_.FullName -notmatch '[/\\]Dependencies[/\\]' })
    if ($packages.Count -ne 1) { throw 'Expected exactly one newly built main MSIX/AppX package.' }
    $manifest = Read-WindowsTestPackageManifest -Path $packages[0].FullName -Archive
    $identity = $manifest.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']")
    $applications = @($manifest.SelectNodes("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']"))
    if ($identity.Name -cne $state.IdentityName -or $identity.Publisher -cne $state.Publisher -or $applications.Count -ne 1) {
        throw 'The built MSIX identity/publisher does not match this owned test session.'
    }
    if (!$state.InstallRequested -and @(Get-AppxPackage -Name $state.IdentityName |
        Where-Object Publisher -CEQ $state.Publisher).Count) {
        throw 'Refusing to replace an already installed package.'
    }
    $dependencies = @(Get-ChildItem -LiteralPath $Directory -Recurse -File |
        Where-Object { $_.Extension -in @('.msix', '.appx') -and $_.FullName -match '[/\\]Dependencies[/\\]x64[/\\]' } |
        ForEach-Object FullName)
    $state.InstallRequested = $true
    Write-WindowsTestPackageState $Path $state
    $install = @{ Path = $packages[0].FullName; ErrorAction = 'Stop' }
    if ($dependencies.Count) { $install.DependencyPath = $dependencies }
    Add-AppxPackage @install
    $registered = @(Get-AppxPackage -Name $state.IdentityName | Where-Object Publisher -CEQ $state.Publisher)
    if ($registered.Count -ne 1 -or $registered[0].Publisher -cne $state.Publisher -or $registered[0].Name -cne $state.IdentityName) {
        throw 'Could not resolve the newly installed owned package.'
    }
    $package = $registered[0]
    $state.PackageFullName = $package.PackageFullName
    $state.PackageFamilyName = $package.PackageFamilyName
    $state.InstallLocation = $package.InstallLocation
    $state.ApplicationId = [string]$applications[0].Id
    $state.AppUserModelId = "$($package.PackageFamilyName)!$($state.ApplicationId)"
    Write-WindowsTestPackageState $Path $state
    return $state
}

function Get-WindowsTestProcessPackage {
    param([int] $ProcessId)
    if (!('Microsoft.Maui.TestUtils.WindowsTesting.WindowsPackageInterop' -as [type])) {
        Add-Type -Path (Join-Path $PSScriptRoot 'windows-launcher/WindowsPackageInterop.cs')
    }
    [Microsoft.Maui.TestUtils.WindowsTesting.WindowsPackageInterop]::GetProcessPackageFullName($ProcessId)
}

function Remove-WindowsTestCertificate {
    param([string] $Thumbprint, [string] $Publisher, [string] $OwnerId)
    if ($Thumbprint -notmatch '^[A-Fa-f0-9]{40}$') { throw 'Invalid owned certificate thumbprint.' }
    $ownedPaths = @()
    foreach ($store in @('Cert:\CurrentUser\My', 'Cert:\CurrentUser\TrustedPeople')) {
        $path = "$store\$Thumbprint"
        if (Test-Path -LiteralPath $path) {
            $certificate = Get-Item -LiteralPath $path -ErrorAction Stop
            if ($certificate.Thumbprint -ine $Thumbprint -or $certificate.Subject -cne $Publisher -or
                ($store -eq 'Cert:\CurrentUser\My' -and $certificate.FriendlyName -cne "MAUI test $OwnerId")) {
                throw 'Refusing to remove a certificate or private key that does not belong to the recorded test session.'
            }
            $ownedPaths += $path
        }
    }
    foreach ($path in $ownedPaths) {
        if ($path.StartsWith('Cert:\CurrentUser\My\', [StringComparison]::OrdinalIgnoreCase)) {
            Remove-Item -LiteralPath $path -DeleteKey -ErrorAction Stop
        } else { Remove-Item -LiteralPath $path -ErrorAction Stop }
    }
}

function Remove-WindowsTestPackage {
    param([string] $Path)
    if (!(Test-Path -LiteralPath $Path)) { return }
    $state = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
    Assert-WindowsTestPackageOwner $state
    $errors = [Collections.Generic.List[string]]::new()
    try {
        if ($state.ProcessId -gt 0 -and !$state.ProcessExited) {
            $process = Get-Process -Id $state.ProcessId -ErrorAction SilentlyContinue
            if ($process -and !$process.HasExited) {
                if ($process.StartTime.ToUniversalTime().Ticks -ne $state.ProcessStartUtcTicks) {
                    throw 'Refusing to terminate a reused PID or a process from another package.'
                }
                $actualPackage = Get-WindowsTestProcessPackage $state.ProcessId
                $registration = @(Get-AppxPackage -Name $state.IdentityName |
                    Where-Object PackageFullName -CEQ $actualPackage)
                if ($actualPackage -cne $state.PackageFullName -or $registration.Count -ne 1 -or
                    $registration[0].Name -cne $state.IdentityName -or
                    $registration[0].Publisher -cne $state.Publisher) {
                    throw 'Refusing to terminate a reused PID or a process from another package.'
                }
                Stop-Process -Id $state.ProcessId -Force -ErrorAction Stop
                if (!$process.WaitForExit(30000)) { throw 'The recorded activated process did not stop.' }
            }
            $state.ProcessExited = $true
            Write-WindowsTestPackageState $Path $state
        }
    }
    catch { $errors.Add($_.Exception.Message) }
    try {
        if ($state.InstallRequested) {
            foreach ($package in @(Get-AppxPackage -Name $state.IdentityName | Where-Object Publisher -CEQ $state.Publisher)) {
                if ($package.Name -cne $state.IdentityName -or $package.Publisher -cne $state.Publisher -or
                    ($state.PackageFullName -and $package.PackageFullName -cne $state.PackageFullName)) {
                    throw 'Refusing to uninstall an unowned or replaced package.'
                }
                Remove-AppxPackage -Package $package.PackageFullName -ErrorAction Stop
            }
        }
    }
    catch { $errors.Add($_.Exception.Message) }
    try {
        if ($state.CertificateThumbprint) {
            Remove-WindowsTestCertificate -Thumbprint $state.CertificateThumbprint -Publisher $state.Publisher -OwnerId $state.OwnerId
        }
    }
    catch { $errors.Add($_.Exception.Message) }
    if ($errors.Count) { throw ($errors -join '; ') }
}

if (!$IsWindows) { throw 'MSIX package setup and cleanup require Windows.' }
if ($Action -eq 'Install' -or ($Action -eq 'Cleanup' -and (Test-Path -LiteralPath $StatePath))) {
    Import-WindowsTestAppxModule
}
switch ($Action) {
    Prepare { Initialize-WindowsTestPackage -Path $StatePath -Project $ProjectPath -TimeoutSeconds $TimeoutSeconds }
    Install { Install-WindowsTestPackage -Path $StatePath -Directory $PackageDirectory }
    Cleanup { Remove-WindowsTestPackage -Path $StatePath }
}
