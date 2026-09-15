function Get-WindowsPackageInfo {
    param([string]$Path)

    if ([IO.Path]::GetExtension($Path) -notin @('.msix', '.appx')) {
        throw "Expected a single MSIX/Appx package, not a bundle: '$Path'."
    }
    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @($archive.Entries | Where-Object FullName -CEQ 'AppxManifest.xml')
        if ($entries.Count -ne 1) { throw "Missing or ambiguous AppxManifest.xml in '$Path'." }
        $reader = [IO.StreamReader]::new($entries[0].Open())
        try { [xml]$manifest = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $identity = $manifest.Package.Identity
        if (-not $identity.Name -or -not $identity.Publisher -or -not $identity.Version) {
            throw "Incomplete MSIX identity in '$Path'."
        }
        return [pscustomobject]@{
            Name = [string]$identity.Name
            Publisher = [string]$identity.Publisher
            Version = [version]$identity.Version
            Architecture = [string]$identity.ProcessorArchitecture
            Framework = [string]$manifest.Package.Properties.Framework -eq 'true'
            Dependencies = @($manifest.SelectNodes("//*[local-name()='Dependencies']/*[local-name()='PackageDependency']") | ForEach-Object {
                [pscustomobject]@{ Name = [string]$_.Name; Publisher = [string]$_.Publisher; MinVersion = [version]$_.MinVersion }
            })
            Applications = @($manifest.Package.Applications.Application | ForEach-Object {
                [pscustomobject]@{ Id = [string]$_.Id; Executable = [string]$_.Executable }
            })
            MinimumOS = @($manifest.Package.Dependencies.TargetDeviceFamily | ForEach-Object { [string]$_.MinVersion })
            Files = @($archive.Entries.FullName)
        }
    } finally { $archive.Dispose() }
}

function Get-WindowsSignTool {
    $sdkRoot = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits/10/bin'
    $tools = @(Get-ChildItem $sdkRoot -Directory | Where-Object Name -Match '^\d+\.\d+\.\d+\.\d+$' |
        Sort-Object { [version]$_.Name } -Descending | ForEach-Object { Join-Path $_.FullName 'x64/signtool.exe' } |
        Where-Object { Test-Path $_ })
    if ($tools.Count -eq 0) { throw "Windows SDK x64 signtool.exe is required." }
    return $tools[0]
}

function Assert-WindowsTestCertificate {
    param($Certificate, [string]$Publisher)

    if ($Certificate.Subject -cne $Publisher -or $Certificate.Subject -cne $Certificate.Issuer -or
        $Certificate.NotAfter -le [DateTime]::Now -or $Certificate.NotBefore -gt [DateTime]::Now) {
        throw "Invalid or expired Windows test signing certificate."
    }
    # Check extensions by OID without relying on localized FriendlyName values.
    $eku = @($Certificate.Extensions | Where-Object { $_.Oid.Value -eq '2.5.29.37' })
    $constraints = @($Certificate.Extensions | Where-Object { $_.Oid.Value -eq '2.5.29.19' })
    if ($eku.Count -ne 1 -or '1.3.6.1.5.5.7.3.3' -notin @($eku[0].EnhancedKeyUsages | ForEach-Object Value) -or
        $constraints.Count -ne 1 -or $constraints[0].CertificateAuthority) {
        throw "Test certificate must be a non-CA code-signing certificate."
    }
}

function Assert-WindowsPackageSignature {
    param($Signature, [string]$Thumbprint)

    if ($Signature.Status -ne 'Valid' -or -not $Signature.SignerCertificate -or
        $Signature.SignerCertificate.Thumbprint -cne $Thumbprint) {
        throw 'MSIX signature is invalid, untrusted, unsigned or does not match the exported certificate.'
    }
}

function New-WindowsTestMsix {
    param($ProjectFile, [string]$TargetFramework, [string]$Configuration, [string]$OutputPath,
        [string]$AppBuildNumber, [string[]]$BinlogArguments)

    if (-not $IsWindows) { throw "Windows test MSIX must be built on Windows." }
    if ($ProjectFile.BaseName -ne 'MauiTemplateSample') { throw "Test MSIX is restricted to MauiTemplateSample." }
    $publisher = 'CN=MAUI Template Test'
    $manifestPath = Join-Path $ProjectFile.DirectoryName 'Platforms/Windows/Package.appxmanifest'
    [xml]$manifest = Get-Content $manifestPath -Raw
    $manifest.Package.Identity.Publisher = $publisher
    $manifest.Package.Properties.PublisherDisplayName = 'MAUI Template Test'
    $manifest.Save($manifestPath)
    $number = [uint32]$AppBuildNumber
    $packageVersion = "1.0.$([math]::Floor($number / 65536)).$($number % 65536)"
    $packageDirectory = Join-Path $OutputPath 'msix-build'
    $installerDirectory = Join-Path $OutputPath 'installable'
    if (Test-Path $packageDirectory) { throw "MSIX staging directory already exists; use a fresh output directory." }
    New-Item -ItemType Directory $packageDirectory, $installerDirectory | Out-Null
    $arguments = @('publish', $ProjectFile.FullName, '-f', $TargetFramework, '-c', $Configuration,
        '-p:RuntimeIdentifierOverride=win-x64', '-p:WindowsPackageType=MSIX',
        '-p:WindowsAppSDKSelfContained=true', '-p:SelfContained=true',
        '-p:GenerateAppxPackageOnBuild=true', '-p:AppxPackageSigningEnabled=false',
        '-p:AppxBundle=Never', '-p:UapAppxPackageBuildMode=SideloadOnly',
        '-p:AppxSymbolPackageEnabled=false', "-p:AppxPackageDir=$packageDirectory/",
        "-p:ApplicationDisplayVersion=1.0.$([math]::Floor($number / 65536))",
        "-p:ApplicationVersion=$($number % 65536)") + $BinlogArguments
    Invoke-DotNetPublish $arguments 'Windows self-contained MSIX publish'
    $packages = @(Get-ChildItem $packageDirectory -Filter '*.msix' -Recurse -File |
        Where-Object { $_.FullName -notmatch '[\\/]Dependencies[\\/]' })
    if ($packages.Count -ne 1) { throw "Expected one Windows x64 MSIX, found $($packages.Count)." }
    $packagePath = Join-Path $installerDirectory 'MauiTemplateSample.msix'
    Copy-Item $packages[0].FullName $packagePath
    $info = Get-WindowsPackageInfo $packagePath
    if ($info.Publisher -cne $publisher -or $info.Architecture -ne 'x64' -or $info.Framework -or
        $info.Version -ne [version]$packageVersion) {
        throw "Generated MSIX publisher/architecture/type mismatch."
    }
    foreach ($name in @('coreclr.dll', 'hostfxr.dll', 'Microsoft.UI.Xaml.dll', 'MauiTemplateSample.exe')) {
        if (-not ($info.Files | Where-Object { [IO.Path]::GetFileName($_) -eq $name })) {
            throw "Self-contained MSIX is missing '$name'."
        }
    }

    $dependencyDirectory = Join-Path $installerDirectory 'Dependencies'
    $dependencyFiles = @()
    $pending = [Collections.Generic.Queue[object]]::new()
    foreach ($dependency in $info.Dependencies) { $pending.Enqueue($dependency) }
    $staged = @{}
    $candidates = @(Get-ChildItem $packageDirectory -Include '*.appx', '*.msix' -Recurse -File)
    while ($pending.Count) {
        $dependency = $pending.Dequeue()
        if ($staged.ContainsKey($dependency.Name)) {
            if ($staged[$dependency.Name].Version -lt $dependency.MinVersion -or
                $staged[$dependency.Name].Publisher -cne $dependency.Publisher) { throw "Conflicting MSIX dependency requirements." }
            continue
        }
        $matches = @($candidates | ForEach-Object {
            $candidate = Get-WindowsPackageInfo $_.FullName
            if ($candidate.Name -eq $dependency.Name -and $candidate.Publisher -ceq $dependency.Publisher -and
                $candidate.Version -ge $dependency.MinVersion -and $candidate.Architecture -in @('x64', 'neutral') -and $candidate.Framework) {
                [pscustomobject]@{ File = $_; Info = $candidate }
            }
        } | Sort-Object { $_.Info.Version } -Descending)
        if ($matches.Count -eq 0) { throw "Required framework '$($dependency.Name)' is not bundled by MSBuild; refusing an incomplete installer." }
        $match = $matches[0]
        New-Item -ItemType Directory $dependencyDirectory -Force | Out-Null
        $destination = Join-Path $dependencyDirectory $match.File.Name
        Copy-Item $match.File.FullName $destination
        $dependencyFiles += [ordered]@{ file = "Dependencies/$($match.File.Name)"; sha256 = (Get-FileHash $destination).Hash }
        $staged[$dependency.Name] = $match.Info
        foreach ($nested in $match.Info.Dependencies) { $pending.Enqueue($nested) }
    }

    $certificate = $null
    try {
        $certificate = New-SelfSignedCertificate -Type Custom -Subject $publisher -KeyUsage DigitalSignature `
            -FriendlyName 'Disposable MAUI template test signing' -CertStoreLocation 'Cert:\CurrentUser\My' `
            -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -KeyExportPolicy NonExportable `
            -NotAfter (Get-Date).AddMonths(3) `
            -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}CA=false')
        Assert-WindowsTestCertificate $certificate $publisher
        $tool = Get-WindowsSignTool
        & $tool sign /fd SHA256 /sha1 $certificate.Thumbprint /s My $packagePath | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "MSIX signing failed ($LASTEXITCODE)." }
        $certificatePath = Join-Path $installerDirectory 'MauiTemplateTest.cer'
        Export-Certificate -Cert $certificate -FilePath $certificatePath -Type CERT | Out-Null
        $public = [Security.Cryptography.X509Certificates.X509Certificate2]::new($certificatePath)
        try {
            if ($public.HasPrivateKey -or $public.Thumbprint -ne $certificate.Thumbprint) { throw "Invalid public-only certificate export." }
        } finally { $public.Dispose() }
        [ordered]@{
            package = [ordered]@{ file = 'MauiTemplateSample.msix'; sha256 = (Get-FileHash $packagePath).Hash }
            certificate = [ordered]@{ file = 'MauiTemplateTest.cer'; thumbprint = $certificate.Thumbprint; sha256 = (Get-FileHash $certificatePath).Hash }
            identity = $info.Name
            publisher = $publisher
            version = $info.Version.ToString()
            minimumOS = $info.MinimumOS
            dependencies = $dependencyFiles
        } | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $installerDirectory 'install.json')
    } finally {
        if ($certificate) { Remove-Item "Cert:\CurrentUser\My\$($certificate.Thumbprint)" -DeleteKey -ErrorAction Stop }
    }
    Copy-Item (Join-Path $PSScriptRoot 'Install-WindowsTestApp.ps1') $installerDirectory
    Copy-Item (Join-Path $PSScriptRoot 'WINDOWS-INSTALL.txt') $installerDirectory
    return Get-Item $packagePath
}
