function Invoke-DistributionDotNet {
    param([string[]]$Arguments, [string]$Description)

    $savedTokens = @{}
    foreach ($name in @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')) {
        $savedTokens[$name] = [Environment]::GetEnvironmentVariable($name)
        [Environment]::SetEnvironmentVariable($name, $null)
    }
    try {
        & dotnet @Arguments | Out-Host
        if ($LASTEXITCODE -ne 0) {
            throw "$Description failed with exit code $LASTEXITCODE."
        }
    } finally {
        foreach ($name in $savedTokens.Keys) {
            [Environment]::SetEnvironmentVariable($name, $savedTokens[$name])
        }
    }
}

function Get-SourcePackageInfo {
    param([string]$Path, [string]$SourceSha)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $nuspecs = @($archive.Entries | Where-Object { $_.FullName -match '^[^/]+\.nuspec$' })
        if ($nuspecs.Count -ne 1) {
            throw "Expected one nuspec in '$Path'."
        }
        $reader = [System.IO.StreamReader]::new($nuspecs[0].Open())
        try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $metadata = $nuspec.package.metadata
        if ($metadata.id -notlike 'Microsoft.Maui.*' -or $metadata.repository.commit -ne $SourceSha) {
            throw "Source provenance mismatch in '$Path': expected MAUI package from $SourceSha."
        }
        return [ordered]@{
            id = [string]$metadata.id
            version = [string]$metadata.version
            repositoryCommit = [string]$metadata.repository.commit
            file = [System.IO.Path]::GetFileName($Path)
            sha256 = (Get-FileHash $Path -Algorithm SHA256).Hash.ToLowerInvariant()
            sha512 = [Convert]::ToBase64String([System.Security.Cryptography.SHA512]::HashData([System.IO.File]::ReadAllBytes($Path)))
        }
    } finally {
        $archive.Dispose()
    }
}

function Get-SourcePackageFiles {
    param([string]$Path)

    return Get-ChildItem $Path -Filter 'Microsoft.Maui.*.nupkg' -File |
        Where-Object Name -NotLike '*.symbols.nupkg'
}

function Read-SourcePackageManifest {
    param([string]$Path, [string]$SourceSha)

    $manifest = Get-Content $Path -Raw | ConvertFrom-Json
    if ($SourceSha -notmatch '^[0-9a-f]{40}$' -or $manifest.sourceSha -ne $SourceSha -or
        @($manifest.packages).Count -eq 0) {
        throw "Invalid source package manifest '$Path' for $SourceSha."
    }
    $ids = @{}
    foreach ($package in $manifest.packages) {
        if ($ids.ContainsKey($package.id) -or $package.id -notlike 'Microsoft.Maui.*' -or
            $package.repositoryCommit -ne $SourceSha -or $package.version -ne $manifest.version -or
            [System.IO.Path]::GetFileName($package.file) -ne $package.file) {
            throw "Invalid source package entry '$($package.id)' in '$Path'."
        }
        $ids[$package.id] = $true
        $packagePath = Join-Path (Split-Path $Path -Parent) $package.file
        $actual = Get-SourcePackageInfo -Path $packagePath -SourceSha $SourceSha
        foreach ($field in @('id', 'version', 'sha256', 'sha512')) {
            if ($actual[$field] -cne $package.$field) {
                throw "Source package '$($package.id)' does not match its manifest ($field)."
            }
        }
    }
    foreach ($id in @('Microsoft.Maui.Controls', 'Microsoft.Maui.Controls.Core', 'Microsoft.Maui.Controls.Xaml',
        'Microsoft.Maui.Core', 'Microsoft.Maui.Essentials', 'Microsoft.Maui.Graphics',
        'Microsoft.Maui.Controls.Build.Tasks', 'Microsoft.Maui.Resizetizer', 'Microsoft.Maui.Sdk')) {
        if (-not $ids.ContainsKey($id)) {
            throw "Required source-built package '$id' is missing."
        }
    }
    if (@($manifest.packages | Where-Object id -Like 'Microsoft.Maui.Templates*').Count -ne 1) {
        throw "Expected exactly one source-built template package."
    }
    return $manifest
}

function Set-SourcePackageConfiguration {
    param([string]$NuGetConfigPath, [string]$PackageDirectory, [string]$ProjectRoot, [string]$Version)

    [xml]$config = Get-Content $NuGetConfigPath -Raw
    $sources = $config.configuration.packageSources
    if (-not $sources) { throw "NuGet.config has no package sources." }
    $local = $config.CreateElement('add')
    $local.SetAttribute('key', 'maui-source')
    $local.SetAttribute('value', [System.IO.Path]::GetFullPath($PackageDirectory))
    $sources.AppendChild($local) | Out-Null
    $mapping = $config.configuration.SelectSingleNode('packageSourceMapping')
    if ($mapping) { $config.configuration.RemoveChild($mapping) | Out-Null }
    $mapping = $config.CreateElement('packageSourceMapping')
    $mapping.AppendChild($config.CreateElement('clear')) | Out-Null
    foreach ($source in $sources.SelectNodes('add')) {
        $entry = $config.CreateElement('packageSource')
        $entry.SetAttribute('key', $source.key)
        $pattern = $config.CreateElement('package')
        $pattern.SetAttribute('pattern', $(if ($source.key -eq 'maui-source') { 'Microsoft.Maui.*' } else { '*' }))
        $entry.AppendChild($pattern) | Out-Null
        $mapping.AppendChild($entry) | Out-Null
    }
    $config.configuration.AppendChild($mapping) | Out-Null
    $config.Save((Join-Path $ProjectRoot 'NuGet.config'))

    $escapedVersion = [System.Security.SecurityElement]::Escape($Version)
    @"
<Project>
  <PropertyGroup>
    <MauiVersion>$escapedVersion</MauiVersion>
  </PropertyGroup>
</Project>
"@ | Set-Content (Join-Path $ProjectRoot 'Directory.Build.props') -Encoding utf8
}

function Test-SourcePackageAssets {
    param([string]$AssetsPath, $Manifest)

    $assets = Get-Content $AssetsPath -Raw | ConvertFrom-Json -AsHashtable
    $expected = @{}
    foreach ($package in $Manifest.packages) { $expected[$package.id] = $package }
    $resolved = @()
    foreach ($key in $assets.libraries.Keys | Sort-Object) {
        $id, $version = $key -split '/', 2
        if ($id -notlike 'Microsoft.Maui.*') { continue }
        $library = $assets.libraries[$key]
        if (-not $expected.ContainsKey($id) -or $version -ne $expected[$id].version -or
            $library.sha512 -cne $expected[$id].sha512) {
            throw "Resolved '$key' in '$AssetsPath' is not the pinned source-built package."
        }

        $resolved += [ordered]@{
            id = $id
            version = $version
            sha512 = $library.sha512
            repositoryCommit = $expected[$id].repositoryCommit
        }
    }
    foreach ($id in @('Microsoft.Maui.Controls', 'Microsoft.Maui.Controls.Core', 'Microsoft.Maui.Controls.Xaml',
        'Microsoft.Maui.Core', 'Microsoft.Maui.Essentials', 'Microsoft.Maui.Graphics', 'Microsoft.Maui.Resizetizer')) {
        if ($id -notin $resolved.id) { throw "Required resolved source package '$id' is missing in '$AssetsPath'." }
    }
    return [ordered]@{
        targets = @($assets.targets.Keys | Sort-Object)
        packages = $resolved
    }
}

function Expand-AndroidAssemblyPayload {
    param([string]$Path)

    # Android packages discrete managed assemblies in ELF shared libraries with a "payload" section.
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 64 -or [BitConverter]::ToUInt32($bytes, 0) -ne 0x464c457f -or
        $bytes[5] -ne 1 -or $bytes[4] -notin @(1, 2)) {
        throw "Unsupported Android ELF assembly wrapper '$Path'."
    }
    $is64Bit = $bytes[4] -eq 2
    $tableOffset = if ($is64Bit) { [BitConverter]::ToUInt64($bytes, 40) } else { [BitConverter]::ToUInt32($bytes, 32) }
    $headerOffset = if ($is64Bit) { 58 } else { 46 }
    $entrySize = [BitConverter]::ToUInt16($bytes, $headerOffset)
    $entryCount = [BitConverter]::ToUInt16($bytes, $headerOffset + 2)
    $namesIndex = [BitConverter]::ToUInt16($bytes, $headerOffset + 4)
    $minimumSize = if ($is64Bit) { 64 } else { 40 }
    if ($entrySize -lt $minimumSize -or $entryCount -eq 0 -or $namesIndex -ge $entryCount -or
        $tableOffset -gt $bytes.Length -or $entryCount * $entrySize -gt $bytes.Length - $tableOffset) {
        throw "Invalid ELF section table in '$Path'."
    }
    $sections = for ($index = 0; $index -lt $entryCount; $index++) {
        $position = [int]($tableOffset + $index * $entrySize)
        [pscustomobject]@{
            nameOffset = [BitConverter]::ToUInt32($bytes, $position)
            offset = if ($is64Bit) { [BitConverter]::ToUInt64($bytes, $position + 24) } else { [BitConverter]::ToUInt32($bytes, $position + 16) }
            size = if ($is64Bit) { [BitConverter]::ToUInt64($bytes, $position + 32) } else { [BitConverter]::ToUInt32($bytes, $position + 20) }
        }
    }
    $names = $sections[$namesIndex]
    if ($names.offset -gt $bytes.Length -or $names.size -gt $bytes.Length - $names.offset) {
        throw "Invalid ELF section names in '$Path'."
    }
    $stringTable = [System.Text.Encoding]::ASCII.GetString($bytes, [int]$names.offset, [int]$names.size)
    $payloads = @($sections | Where-Object {
        $_.nameOffset -lt $stringTable.Length -and $stringTable.Substring([int]$_.nameOffset).Split([char]0)[0] -ceq 'payload'
    })
    if ($payloads.Count -ne 1 -or $payloads[0].offset -gt $bytes.Length -or
        $payloads[0].size -lt 2 -or $payloads[0].size -gt $bytes.Length - $payloads[0].offset) {
        throw "Missing or invalid ELF managed payload in '$Path'."
    }
    $payload = $payloads[0]
    if ([BitConverter]::ToUInt16($bytes, [int]$payload.offset) -ne 0x5a4d) {
        throw "Android ELF payload is not an uncompressed managed PE image in '$Path'."
    }
    $output = [System.IO.File]::Create($Path)
    try { $output.Write($bytes, [int]$payload.offset, [int]$payload.size) } finally { $output.Dispose() }
}

function Get-AppPayloadProof {
    param([string]$Path, [string]$SourceSha, $Manifest)

    $isMsix = [IO.Path]::GetExtension($Path) -in @('.msix', '.appx')
    if ([IO.Path]::GetExtension($Path) -in @('.msixbundle', '.appxbundle')) {
        throw "Bundle provenance is not supported; build a single x64 MSIX so no payload can be skipped."
    }
    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    $assemblies = @()
    $dependencies = @()
    try {
        if ($isMsix) {
            if (-not $Manifest) { throw 'MSIX verification requires the source package manifest.' }
            if (-not $archive.GetEntry('AppxManifest.xml')) { throw 'MSIX AppxManifest.xml is missing.' }
            foreach ($name in @('coreclr.dll', 'hostfxr.dll', 'Microsoft.UI.Xaml.dll')) {
                if ($name -notin $archive.Entries.Name) { throw "Self-contained MSIX is missing '$name'." }
            }
            $resources = @($archive.Entries | Where-Object Name -CEQ 'source-provenance.json')
            if ($resources.Count -ne 1) { throw 'MSIX template source provenance is missing or ambiguous.' }
            $reader = [IO.StreamReader]::new($resources[0].Open())
            try { $templateProof = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
            $template = @($Manifest.packages | Where-Object id -Like 'Microsoft.Maui.Templates*')
            if ($template.Count -ne 1 -or $templateProof.sourceSha -ne $SourceSha -or
                $templateProof.frameworkVersion -ne $Manifest.version -or
                $templateProof.template.sha512 -cne $template[0].sha512) {
                throw 'MSIX embedded template provenance does not match the source-built package.'
            }
            $runtimeFiles = @($archive.Entries | Where-Object Name -Like '*.runtimeconfig.json')
            if ($runtimeFiles.Count -ne 1) { throw 'MSIX self-contained runtime configuration is missing or ambiguous.' }
            $reader = [IO.StreamReader]::new($runtimeFiles[0].Open())
            try { $runtime = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
            if ('Microsoft.NETCore.App' -notin @($runtime.runtimeOptions.includedFrameworks.name)) {
                throw 'MSIX does not include its .NET runtime.'
            }
        }
        foreach ($entry in $archive.Entries) {
            $wrapped = $entry.Name -match '^(?:lib)?_(Microsoft\.Maui.*\.dll)\.so$'
            $assemblyName = if ($wrapped) { $Matches[1] } else { $entry.Name }
            if ($assemblyName -like 'Microsoft.Maui*.dll') {
                $temporaryFile = [System.IO.Path]::GetTempFileName()
                try {
                    [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $temporaryFile, $true)
                    if ($wrapped) { Expand-AndroidAssemblyPayload -Path $temporaryFile }
                    $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($temporaryFile).ProductVersion
                    if ($version -notlike "*$SourceSha*") {
                        throw "Packaged assembly '$($entry.FullName)' is not from $SourceSha (informational version '$version')."
                    }
                    $assemblies += [ordered]@{
                        path = $entry.FullName
                        name = $assemblyName
                        informationalVersion = $version
                        sha256 = (Get-FileHash $temporaryFile -Algorithm SHA256).Hash.ToLowerInvariant()
                    }
                } finally { Remove-Item $temporaryFile -Force }
            } elseif ($entry.Name -like '*.deps.json') {
                $reader = [System.IO.StreamReader]::new($entry.Open())
                try { $deps = $reader.ReadToEnd() | ConvertFrom-Json -AsHashtable } finally { $reader.Dispose() }
                $dependencies += @($deps.libraries.Keys | Where-Object { $_ -like 'Microsoft.Maui.*' } | Sort-Object)
            }
        }
    } finally { $archive.Dispose() }
    if ($assemblies.Count -eq 0) {
        throw "No inspectable MAUI assemblies found in '$Path'; cannot verify the app payload."
    }
    if ($Manifest) {
        foreach ($dependency in $dependencies) {
            $id, $version = $dependency -split '/', 2
            if ($id -notin $Manifest.packages.id -or $version -ne $Manifest.version) {
                throw "Packaged dependency '$dependency' is not from the pinned source package set."
            }
        }
        foreach ($name in @('Microsoft.Maui.dll', 'Microsoft.Maui.Controls.dll', 'Microsoft.Maui.Graphics.dll')) {
            if ($name -notin $assemblies.name) {
                throw "Required MAUI assembly '$name' is missing from '$Path'."
            }
        }
        if ($isMsix) {
            foreach ($id in @('Microsoft.Maui.Controls', 'Microsoft.Maui.Controls.Core', 'Microsoft.Maui.Controls.Xaml',
                'Microsoft.Maui.Core', 'Microsoft.Maui.Essentials', 'Microsoft.Maui.Graphics')) {
                if ("$id/$($Manifest.version)" -notin $dependencies) { throw "MSIX dependency manifest is missing '$id'." }
            }
        }
    }
    return [ordered]@{
        file = [System.IO.Path]::GetFileName($Path)
        sha256 = (Get-FileHash $Path -Algorithm SHA256).Hash.ToLowerInvariant()
        assemblies = $assemblies
        dependencies = $dependencies
    }
}
