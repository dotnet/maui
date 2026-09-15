#Requires -Version 5.1
#Requires -RunAsAdministrator

[CmdletBinding(SupportsShouldProcess)]
param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$data = Get-Content (Join-Path $PSScriptRoot 'install.json') -Raw | ConvertFrom-Json

function Get-CheckedFile($Record) {
    if (-not $Record.file -or $Record.file -notmatch '^(?:Dependencies/)?[A-Za-z0-9._-]+$') {
        throw 'Invalid installer filename.'
    }
    $path = Join-Path $PSScriptRoot $Record.file
    if ((Get-FileHash $path -Algorithm SHA256).Hash -cne $Record.sha256) {
        throw "Download hash mismatch: $($Record.file). Download the complete artifact again."
    }
    return $path
}

$packagePath = Get-CheckedFile $data.package
$certificatePath = Get-CheckedFile $data.certificate
$dependencies = @($data.dependencies | ForEach-Object { Get-CheckedFile $_ })
$certificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new($certificatePath)
$addedTrust = $false
$installed = $false
try {
    $eku = @($certificate.Extensions | Where-Object { $_.Oid.Value -eq '2.5.29.37' })
    $constraints = @($certificate.Extensions | Where-Object { $_.Oid.Value -eq '2.5.29.19' })
    if ($certificate.HasPrivateKey -or $certificate.Thumbprint -cne $data.certificate.thumbprint -or
        $certificate.Subject -cne $data.publisher -or $certificate.Subject -cne $certificate.Issuer -or
        $certificate.NotAfter -le [DateTime]::Now -or $certificate.NotBefore -gt [DateTime]::Now -or
        $eku.Count -ne 1 -or '1.3.6.1.5.5.7.3.3' -notin @($eku[0].EnhancedKeyUsages | ForEach-Object Value) -or
        $constraints.Count -ne 1 -or $constraints[0].CertificateAuthority) {
        throw 'The public test certificate does not match this installer or is expired.'
    }
    $archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $entry = $archive.GetEntry('AppxManifest.xml')
        if (-not $entry) { throw 'MSIX manifest is missing.' }
        $reader = [IO.StreamReader]::new($entry.Open())
        try { [xml]$manifest = $reader.ReadToEnd() } finally { $reader.Dispose() }
        if ($manifest.Package.Identity.Publisher -cne $certificate.Subject -or
            $manifest.Package.Identity.Name -cne $data.identity -or $manifest.Package.Identity.ProcessorArchitecture -ne 'x64') {
            throw 'MSIX publisher, identity or architecture does not match this installer.'
        }
    } finally { $archive.Dispose() }

    foreach ($dependency in $dependencies) {
        if ((Get-AuthenticodeSignature $dependency).Status -ne 'Valid') {
            throw "Framework dependency signature is not trusted: '$dependency'. No trust changes made."
        }
    }
    $trustPath = "Cert:\LocalMachine\TrustedPeople\$($certificate.Thumbprint)"
    Write-Host "App: $($data.identity) $($data.version)"
    Write-Host "TEST publisher: $($certificate.Subject)"
    Write-Host "Certificate SHA-1 thumbprint: $($certificate.Thumbprint)"
    Write-Host "Certificate expires: $($certificate.NotAfter.ToString('u'))"
    Write-Host 'This is a self-signed test app, not a Microsoft production-signed release.'
    if (-not $PSCmdlet.ShouldProcess($data.identity, 'Trust test publisher if needed and install MSIX for this user')) { return }
    if (-not (Test-Path $trustPath)) {
        Write-Host 'This adds only this test certificate to LocalMachine\TrustedPeople, NOT Trusted Root Certification Authorities.'
        Write-Host 'Obtain IT approval on a managed PC. Decline if you do not trust the artifact source or publisher.'
        $consent = Read-Host 'Type TRUST to approve this certificate and install; anything else cancels'
        if ($consent -cne 'TRUST') { throw 'Installation cancelled; certificate trust was not changed.' }
        Import-Certificate -FilePath $certificatePath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' | Out-Null
        $addedTrust = $true
    }
    $signature = Get-AuthenticodeSignature $packagePath
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Thumbprint -cne $certificate.Thumbprint) {
        throw "MSIX signature validation failed: $($signature.Status)."
    }
    $arguments = @{ Path = $packagePath; ErrorAction = 'Stop' }
    if ($dependencies.Count) { $arguments.DependencyPath = $dependencies }
    Add-AppxPackage @arguments
    $installed = $true
    Write-Host 'Installed. Open MAUI Template Sample from Start. No files need to be copied manually.'
} finally {
    if ($addedTrust -and -not $installed) { Remove-Item $trustPath -ErrorAction Stop }
    $certificate.Dispose()
}
