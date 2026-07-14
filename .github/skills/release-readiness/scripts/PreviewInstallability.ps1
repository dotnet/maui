#!/usr/bin/env pwsh
#Requires -Version 7.0

Set-StrictMode -Version Latest

function Get-InstallabilityProperty {
    param(
        [AllowNull()]$Object,
        [Parameter(Mandatory)][string]$Name
    )

    if ($null -eq $Object) { return $null }
    if ($Object -is [System.Collections.IDictionary]) {
        foreach ($key in $Object.Keys) {
            if ([string]$key -ieq $Name) { return $Object[$key] }
        }
        return $null
    }
    if ($Object.PSObject -and $Object.PSObject.Properties[$Name]) {
        return $Object.$Name
    }
    return $null
}

function Get-PreviewSdkFeatureBand {
    param([Parameter(Mandatory)][string]$SdkVersion)

    if ($SdkVersion -notmatch '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)') {
        throw "SDK version '$SdkVersion' does not start with major.minor.patch."
    }

    $band = [int]([Math]::Floor(([int]$Matches.patch) / 100) * 100)
    return "$($Matches.major).$($Matches.minor).$band"
}

function ConvertTo-WorkloadSetNuGetVersion {
    param([Parameter(Mandatory)][string]$CliVersion)

    if ($CliVersion -notmatch '^(?<major>\d+)\.0\.(?<feature>\d+)(?<suffix>-.+)?$') {
        throw "Workload-set CLI version '$CliVersion' is not in '<major>.0.<feature>[-suffix]' form."
    }

    return "$($Matches.major).$($Matches.feature).0$($Matches.suffix)"
}

function ConvertTo-WorkloadSetCliVersion {
    param(
        [Parameter(Mandatory)][string]$NuGetVersion,
        [Parameter(Mandatory)][string]$SdkFeatureBand
    )

    if ($NuGetVersion -notmatch '^\d+\.\d+\.\d+(?<suffix>-.+)?$') {
        throw "Workload-set NuGet version '$NuGetVersion' is not a semantic version."
    }

    return "$SdkFeatureBand$($Matches.suffix)"
}

function ConvertFrom-PreviewPackageSourceSpec {
    param(
        [Parameter(Mandatory)][int]$Major,
        [string[]]$AdditionalPackageSource = @()
    )

    $sources = [System.Collections.Generic.List[object]]::new()
    $add = {
        param($Name, $Uri, $Role, [bool]$IsAdditional)
        [void]$sources.Add([PSCustomObject]@{
            Name         = $Name
            Uri          = $Uri
            Role         = $Role
            IsAdditional = $IsAdditional
            IsInternal   = ($Uri -match '^https://pkgs\.dev\.azure\.com/dnceng/internal/')
        })
    }

    & $add 'dotnet-workloads' "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-workloads/nuget/v3/index.json" 'workload-set' $false
    & $add "dotnet$Major-workloads" "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet$Major-workloads/nuget/v3/index.json" 'platform' $false
    & $add "dotnet$Major" "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet$Major/nuget/v3/index.json" 'product' $false
    & $add "dotnet$Major-transport" "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet$Major-transport/nuget/v3/index.json" 'transport' $false
    & $add 'dotnet-public' "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json" 'shared' $false
    & $add 'nuget.org' 'https://api.nuget.org/v3/index.json' 'shared' $false

    foreach ($spec in @($AdditionalPackageSource)) {
        if ([string]::IsNullOrWhiteSpace($spec)) { continue }
        if ($spec -notmatch '^(?<name>[A-Za-z0-9._-]+)=(?<uri>https://.+)$') {
            throw "Additional package source '$spec' must use name=https://... syntax."
        }

        $name = $Matches.name
        $uriText = $Matches.uri
        $uri = [Uri]$uriText
        if ($uri.Scheme -ne 'https' -or
            $uri.Host -notin @('pkgs.dev.azure.com', 'api.nuget.org') -or
            -not [string]::IsNullOrEmpty($uri.UserInfo)) {
            throw "Additional package source '$name' must be an HTTPS dnceng Azure Artifacts or NuGet.org service index without embedded credentials."
        }
        if ($uri.Host -eq 'pkgs.dev.azure.com' -and -not $uri.AbsolutePath.StartsWith('/dnceng/', [StringComparison]::OrdinalIgnoreCase)) {
            throw "Additional Azure Artifacts source '$name' must belong to the dnceng organization."
        }
        if (@($sources | Where-Object { $_.Name -ieq $name }).Count -gt 0) {
            throw "Package source name '$name' is already defined."
        }

        & $add $name $uri.AbsoluteUri 'additional' $true
    }

    return @($sources)
}

function Get-PackageSourceHeaders {
    param([Parameter(Mandatory)]$Source)

    $variableName = "NuGetPackageSourceCredentials_$($Source.Name)"
    $credential = [Environment]::GetEnvironmentVariable($variableName)
    if ([string]::IsNullOrWhiteSpace($credential)) { return @{} }

    $username = 'unused'
    $password = $null
    foreach ($part in ($credential -split ';')) {
        if ($part -match '^\s*Username=(.*)$') {
            $username = $Matches[1]
        } elseif ($part -match '^\s*Password=(.*)$') {
            $password = $Matches[1]
        }
    }
    if ([string]::IsNullOrEmpty($password)) { return @{} }

    $bytes = [Text.Encoding]::UTF8.GetBytes("${username}:$password")
    return @{ Authorization = "Basic $([Convert]::ToBase64String($bytes))" }
}

function Get-InstallabilityHttpStatus {
    param([AllowNull()]$Exception)

    if ($null -eq $Exception) { return $null }
    $status = Get-InstallabilityProperty $Exception 'StatusCode'
    if ($null -ne $status) {
        try { return [int]$status } catch { }
    }
    $response = Get-InstallabilityProperty $Exception 'Response'
    $status = Get-InstallabilityProperty $response 'StatusCode'
    if ($null -ne $status) {
        try { return [int]$status } catch { }
    }
    return $null
}

function Invoke-InstallabilityJson {
    param(
        [Parameter(Mandatory)][string]$Url,
        [Parameter(Mandatory)]$Source,
        [scriptblock]$Fetcher,
        [int]$TimeoutSec = 20
    )

    if ($Fetcher) {
        return & $Fetcher $Url $Source
    }

    $headers = Get-PackageSourceHeaders -Source $Source
    return Invoke-RestMethod -Uri $Url -Headers $headers -TimeoutSec $TimeoutSec -ErrorAction Stop
}

function Resolve-PreviewPackageSource {
    param(
        [Parameter(Mandatory)]$Source,
        [scriptblock]$Fetcher
    )

    try {
        $index = Invoke-InstallabilityJson -Url $Source.Uri -Source $Source -Fetcher $Fetcher
        $search = $null
        $flat = $null
        foreach ($resource in @(Get-InstallabilityProperty $index 'resources')) {
            $type = [string](Get-InstallabilityProperty $resource '@type')
            $id = [string](Get-InstallabilityProperty $resource '@id')
            if (-not $search -and $type.StartsWith('SearchQueryService', [StringComparison]::OrdinalIgnoreCase)) {
                $search = $id
            }
            if (-not $flat -and $type.StartsWith('PackageBaseAddress', [StringComparison]::OrdinalIgnoreCase)) {
                $flat = $id
            }
        }

        if ([string]::IsNullOrWhiteSpace($flat)) {
            return [PSCustomObject]@{
                Source             = $Source
                Available          = $false
                AuthenticationLost = $false
                SearchUrl          = $search
                FlatUrl            = $null
                Reason             = 'service index has no PackageBaseAddress resource'
            }
        }

        return [PSCustomObject]@{
            Source             = $Source
            Available          = $true
            AuthenticationLost = $false
            SearchUrl          = $search
            FlatUrl            = $flat.TrimEnd('/')
            Reason             = $null
        }
    } catch {
        $status = Get-InstallabilityHttpStatus $_.Exception
        return [PSCustomObject]@{
            Source             = $Source
            Available          = $false
            AuthenticationLost = ($status -in @(401, 403))
            SearchUrl          = $null
            FlatUrl            = $null
            Reason             = if ($status) { "HTTP $status" } else { 'source query failed' }
        }
    }
}

function Sort-PreviewNuGetVersions {
    param([string[]]$Version)

    return @($Version |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -Unique |
        Sort-Object -Property @{
            Expression = {
                try { [System.Management.Automation.SemanticVersion]::Parse($_) }
                catch { [System.Management.Automation.SemanticVersion]::new(0, 0, 0) }
            }
            Descending = $true
        })
}

function Find-PreviewWorkloadSetPackage {
    param(
        [Parameter(Mandatory)][array]$ResolvedSources,
        [Parameter(Mandatory)][string]$SdkFeatureBand,
        [Parameter(Mandatory)][int]$Preview,
        [scriptblock]$Fetcher
    )

    $query = "Microsoft.NET.Workloads.$SdkFeatureBand-preview.$Preview"
    $exactId = "^Microsoft\.NET\.Workloads\.$([regex]::Escape($SdkFeatureBand))-preview\.$Preview$"
    $candidates = [System.Collections.Generic.List[object]]::new()

    foreach ($resolved in @($ResolvedSources | Where-Object { $_.Available -and $_.SearchUrl })) {
        try {
            $separator = if ($resolved.SearchUrl.Contains('?')) { '&' } else { '?' }
            $url = "$($resolved.SearchUrl.TrimEnd('/'))/$separator" +
                "q=$([Uri]::EscapeDataString($query))&prerelease=true&semVerLevel=2.0.0&take=100"
            $search = Invoke-InstallabilityJson -Url $url -Source $resolved.Source -Fetcher $Fetcher
            foreach ($package in @(Get-InstallabilityProperty $search 'data')) {
                $id = [string](Get-InstallabilityProperty $package 'id')
                if ($id -notmatch $exactId -or $id -match '\.Msi\.') { continue }

                $versions = [System.Collections.Generic.List[string]]::new()
                foreach ($versionEntry in @(Get-InstallabilityProperty $package 'versions')) {
                    $value = [string](Get-InstallabilityProperty $versionEntry 'version')
                    if ($value -match "-preview\.$Preview(?:\.|$)") {
                        [void]$versions.Add($value)
                    }
                }
                $latest = [string](Get-InstallabilityProperty $package 'version')
                if ($latest -match "-preview\.$Preview(?:\.|$)") {
                    [void]$versions.Add($latest)
                }

                [void]$candidates.Add([PSCustomObject]@{
                    Id       = $id
                    Source   = $resolved
                    Versions = @(Sort-PreviewNuGetVersions -Version $versions)
                })
            }
        } catch {
            continue
        }
    }

    if ($candidates.Count -eq 0) { return $null }

    # Prefer the dedicated workload-set feed over an upstream copy exposed by
    # another feed, then prefer the candidate carrying the most versions.
    return @($candidates | Sort-Object -Property @(
        @{ Expression = { if ($_.Source.Source.Role -eq 'workload-set') { 0 } else { 1 } }; Descending = $false },
        @{ Expression = { $_.Versions.Count }; Descending = $true }
    ))[0]
}

function Read-PreviewNuGetPackageEntries {
    param(
        [Parameter(Mandatory)]$ResolvedSource,
        [Parameter(Mandatory)][string]$PackageId,
        [Parameter(Mandatory)][string]$Version,
        [Parameter(Mandatory)][string[]]$EntryName,
        [scriptblock]$Downloader
    )

    $id = $PackageId.ToLowerInvariant()
    $versionLower = $Version.ToLowerInvariant()
    $url = "$($ResolvedSource.FlatUrl)/$id/$versionLower/$id.$versionLower.nupkg"
    $tempFile = [IO.Path]::Combine([IO.Path]::GetTempPath(), "$([Guid]::NewGuid()).nupkg")
    $zip = $null

    try {
        if ($Downloader) {
            & $Downloader $url $tempFile $ResolvedSource.Source
        } else {
            $headers = Get-PackageSourceHeaders -Source $ResolvedSource.Source
            Invoke-WebRequest -Uri $url -Headers $headers -OutFile $tempFile -TimeoutSec 60 -ErrorAction Stop
        }

        $zip = [IO.Compression.ZipFile]::OpenRead($tempFile)
        $result = [ordered]@{}
        foreach ($wanted in $EntryName) {
            $entry = @($zip.Entries | Where-Object { $_.FullName -ieq $wanted }) | Select-Object -First 1
            if (-not $entry) {
                $result[$wanted] = $null
                continue
            }

            $reader = [IO.StreamReader]::new($entry.Open())
            try {
                $result[$wanted] = $reader.ReadToEnd() | ConvertFrom-Json -AsHashtable
            } finally {
                $reader.Dispose()
            }
        }
        return $result
    } finally {
        if ($zip) { $zip.Dispose() }
        if (Test-Path -LiteralPath $tempFile) {
            Remove-Item -LiteralPath $tempFile -Force
        }
    }
}

function Get-WorkloadSetManifestVersion {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)][string]$WorkloadId
    )

    $raw = [string](Get-InstallabilityProperty $Manifest $WorkloadId)
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return [PSCustomObject]@{ Version = $null; SdkBand = $null; Raw = $null }
    }

    $parts = $raw -split '/', 2
    return [PSCustomObject]@{
        Version = $parts[0]
        SdkBand = if ($parts.Count -gt 1) { $parts[1] } else { $null }
        Raw     = $raw
    }
}

function Compare-PreviewWorkloadSetPins {
    param(
        [Parameter(Mandatory)]$Manifest,
        [AllowNull()]$Pins,
        [Parameter(Mandatory)][int]$Major,
        [Parameter(Mandatory)][int]$Preview,
        [string]$ExpectedMauiManifestVersion
    )

    $android = Get-InstallabilityProperty (Get-InstallabilityProperty $Pins 'Android') 'Version'
    $macios = Get-InstallabilityProperty (Get-InstallabilityProperty $Pins 'Macios') 'Version'
    $vmr = Get-InstallabilityProperty (Get-InstallabilityProperty $Pins 'Vmr') 'Version'
    $mauiPattern = '^' + [regex]::Escape("$Major.0.0-preview.$Preview") + '(?:\.|$)'
    $expectations = @(
        @{ Id = 'Microsoft.NET.Sdk.Android'; Expected = [string]$android; Pattern = $null },
        @{ Id = 'Microsoft.NET.Sdk.iOS'; Expected = [string]$macios; Pattern = $null },
        @{ Id = 'Microsoft.NET.Sdk.MacCatalyst'; Expected = [string]$macios; Pattern = $null },
        @{ Id = 'Microsoft.NET.Sdk.macOS'; Expected = [string]$macios; Pattern = $null },
        @{ Id = 'Microsoft.NET.Sdk.tvOS'; Expected = [string]$macios; Pattern = $null },
        @{ Id = 'Microsoft.NET.Workload.Mono.ToolChain.Current'; Expected = [string]$vmr; Pattern = $null },
        @{ Id = 'Microsoft.NET.Workload.Emscripten.Current'; Expected = [string]$vmr; Pattern = $null },
        @{
            Id       = 'Microsoft.NET.Sdk.Maui'
            Expected = $ExpectedMauiManifestVersion
            Pattern  = if ([string]::IsNullOrWhiteSpace($ExpectedMauiManifestVersion)) {
                $mauiPattern
            } else { $null }
        }
    )

    $comparisons = [System.Collections.Generic.List[object]]::new()
    foreach ($expectation in $expectations) {
        $actual = (Get-WorkloadSetManifestVersion -Manifest $Manifest -WorkloadId $expectation.Id).Version
        $status = 'unverified'
        if ([string]::IsNullOrWhiteSpace($actual)) {
            $status = 'missing'
        } elseif (-not [string]::IsNullOrWhiteSpace($expectation.Expected)) {
            $status = if ($actual -eq $expectation.Expected) { 'match' } else { 'mismatch' }
        } elseif ($expectation.Pattern) {
            $status = if ($actual -match $expectation.Pattern) { 'match' } else { 'mismatch' }
        }

        [void]$comparisons.Add([PSCustomObject]@{
            WorkloadId = $expectation.Id
            Expected   = if ($expectation.Expected) { $expectation.Expected } else { "preview.$Preview build" }
            Actual     = $actual
            Status     = $status
        })
    }

    return @($comparisons)
}

function Get-PreviewManifestPackageRequests {
    param([Parameter(Mandatory)]$Manifest)

    $requiredWorkloads = @(
        'Microsoft.NET.Sdk.Android',
        'Microsoft.NET.Sdk.iOS',
        'Microsoft.NET.Sdk.MacCatalyst',
        'Microsoft.NET.Sdk.macOS',
        'Microsoft.NET.Sdk.Maui',
        'Microsoft.NET.Workload.Mono.ToolChain.Current'
    )

    $requests = [System.Collections.Generic.List[object]]::new()
    $seen = @{}
    foreach ($workloadId in $requiredWorkloads) {
        $value = Get-WorkloadSetManifestVersion -Manifest $Manifest -WorkloadId $workloadId
        if ([string]::IsNullOrWhiteSpace($value.Version) -or [string]::IsNullOrWhiteSpace($value.SdkBand)) {
            continue
        }

        $packageId = "$workloadId.Manifest-$($value.SdkBand)"
        $key = "$($packageId.ToLowerInvariant())|$($value.Version.ToLowerInvariant())"
        if ($seen.ContainsKey($key)) { continue }
        $seen[$key] = $true
        [void]$requests.Add([PSCustomObject]@{
            WorkloadId = $workloadId
            PackageId  = $packageId
            Version    = $value.Version
        })
    }
    return @($requests)
}

function Get-PreviewSourceOrder {
    param(
        [Parameter(Mandatory)][array]$ResolvedSources,
        [Parameter(Mandatory)][string]$PackageId
    )

    $id = $PackageId.ToLowerInvariant()
    $roleOrder = if ($id -match '^microsoft\.net\.workloads\.') {
        @('workload-set', 'additional', 'product', 'platform', 'transport', 'shared')
    } elseif ($id -match '^microsoft\.net\.sdk\.android\.manifest-' -or $id -match '^microsoft\.android\.') {
        @('platform', 'additional', 'product', 'transport', 'shared')
    } elseif ($id -match '^microsoft\.netcore\.app\.runtime\.' -or $id -match '^microsoft\.net\.runtime\.') {
        @('additional', 'transport', 'product', 'platform', 'shared')
    } elseif ($id -match '^microsoft\.(ios|maccatalyst|macos|tvos)\.') {
        @('product', 'additional', 'platform', 'transport', 'shared')
    } elseif ($id -match '^microsoft\.net\..+\.manifest-' -or $id -match '^microsoft\.maui\.') {
        @('product', 'additional', 'platform', 'transport', 'shared')
    } else {
        @('additional', 'product', 'platform', 'transport', 'shared')
    }

    return @(
        $ResolvedSources |
            Where-Object { $_.Source.Role -in $roleOrder } |
            Sort-Object -Property @{
                Expression = { [Array]::IndexOf($roleOrder, $_.Source.Role) }
                Descending = $false
            }
    )
}

function Find-PreviewPackageLocation {
    param(
        [Parameter(Mandatory)][array]$ResolvedSources,
        [Parameter(Mandatory)][string]$PackageId,
        [Parameter(Mandatory)][string]$Version,
        [scriptblock]$Fetcher
    )

    $unknownSources = [System.Collections.Generic.List[string]]::new()
    foreach ($resolved in @(Get-PreviewSourceOrder -ResolvedSources $ResolvedSources -PackageId $PackageId)) {
        if (-not $resolved.Available -or [string]::IsNullOrWhiteSpace($resolved.FlatUrl)) {
            if ($resolved.AuthenticationLost -or $resolved.Source.IsAdditional) {
                [void]$unknownSources.Add($resolved.Source.Name)
            }
            continue
        }

        $id = $PackageId.ToLowerInvariant()
        try {
            $index = Invoke-InstallabilityJson -Url "$($resolved.FlatUrl)/$id/index.json" -Source $resolved.Source -Fetcher $Fetcher
            $versions = @(Get-InstallabilityProperty $index 'versions')
            if (@($versions | Where-Object { [string]$_ -ieq $Version }).Count -gt 0) {
                return [PSCustomObject]@{
                    Status         = 'found'
                    PackageId      = $PackageId
                    Version        = $Version
                    ResolvedSource = $resolved
                    UnknownSources = @($unknownSources)
                }
            }
        } catch {
            $status = Get-InstallabilityHttpStatus $_.Exception
            if ($status -in @(401, 403) -or $resolved.Source.IsAdditional) {
                [void]$unknownSources.Add($resolved.Source.Name)
            } elseif ($status -ne 404) {
                [void]$unknownSources.Add($resolved.Source.Name)
            }
        }
    }

    return [PSCustomObject]@{
        Status         = if ($unknownSources.Count -gt 0) { 'unknown' } else { 'missing' }
        PackageId      = $PackageId
        Version        = $Version
        ResolvedSource = $null
        UnknownSources = @($unknownSources | Select-Object -Unique)
    }
}

function Get-PreviewPlatformRequirements {
    param([array]$ManifestEvidence)

    $requirements = [ordered]@{
        Jdk            = $null
        AndroidSdk     = @()
        Xcode          = $null
        AppleSdk       = $null
        WindowsAppSdk  = $null
    }

    foreach ($evidence in @($ManifestEvidence)) {
        $dependencies = Get-InstallabilityProperty $evidence 'Dependencies'
        if ($null -eq $dependencies) { continue }

        $android = Get-InstallabilityProperty $dependencies 'microsoft.net.sdk.android'
        if ($android) {
            $jdk = Get-InstallabilityProperty $android 'jdk'
            if ($jdk) {
                $requirements.Jdk = [PSCustomObject]@{
                    Version            = [string](Get-InstallabilityProperty $jdk 'version')
                    RecommendedVersion = [string](Get-InstallabilityProperty $jdk 'recommendedVersion')
                }
            }

            $androidSdk = Get-InstallabilityProperty $android 'androidsdk'
            $packages = [System.Collections.Generic.List[string]]::new()
            foreach ($package in @(Get-InstallabilityProperty $androidSdk 'packages')) {
                $sdkPackage = Get-InstallabilityProperty $package 'sdkPackage'
                $id = Get-InstallabilityProperty $sdkPackage 'id'
                if ($id -is [string]) {
                    [void]$packages.Add($id)
                }
            }
            $requirements.AndroidSdk = @($packages)
        }

        $ios = Get-InstallabilityProperty $dependencies 'microsoft.net.sdk.ios'
        if ($ios) {
            $xcode = Get-InstallabilityProperty $ios 'xcode'
            if ($xcode) {
                $requirements.Xcode = [PSCustomObject]@{
                    Version            = [string](Get-InstallabilityProperty $xcode 'version')
                    RecommendedVersion = [string](Get-InstallabilityProperty $xcode 'recommendedVersion')
                }
            }
            $sdk = Get-InstallabilityProperty $ios 'sdk'
            if ($sdk) {
                $requirements.AppleSdk = [string](Get-InstallabilityProperty $sdk 'version')
            }
        }

        $maui = Get-InstallabilityProperty $dependencies 'microsoft.net.sdk.maui'
        if ($maui) {
            $windows = Get-InstallabilityProperty $maui 'windowsAppSdk'
            if ($windows) {
                $requirements.WindowsAppSdk = [string](Get-InstallabilityProperty $windows 'recommendedVersion')
            }
        }
    }

    return [PSCustomObject]$requirements
}

function Get-PreviewRepresentativePackRequests {
    param(
        [array]$ManifestEvidence,
        [Parameter(Mandatory)][int]$Major
    )

    $requests = [System.Collections.Generic.List[object]]::new()
    $representatives = @(
        @{ Category = 'android-sdk'; Pattern = "^Microsoft\.Android\.Sdk\.net$Major$" },
        @{ Category = 'ios-sdk'; Pattern = "^Microsoft\.iOS\.Sdk\.net$Major\.0_" },
        @{ Category = 'maccatalyst-sdk'; Pattern = "^Microsoft\.MacCatalyst\.Sdk\.net$Major\.0_" },
        @{ Category = 'android-runtime'; Pattern = "^Microsoft\.NETCore\.App\.Runtime\.Mono\.net$Major\.android-arm64$" },
        @{ Category = 'ios-runtime'; Pattern = "^Microsoft\.NETCore\.App\.Runtime\.Mono\.net$Major\.ios-arm64$" },
        @{ Category = 'maccatalyst-runtime'; Pattern = "^Microsoft\.NETCore\.App\.Runtime\.Mono\.net$Major\.maccatalyst-arm64$" },
        @{ Category = 'maui-controls'; Pattern = '^Microsoft\.Maui\.Controls$' }
    )
    $seen = @{}

    foreach ($evidence in @($ManifestEvidence)) {
        $manifest = Get-InstallabilityProperty $evidence 'Manifest'
        $packs = Get-InstallabilityProperty $manifest 'packs'
        if ($null -eq $packs) { continue }

        $entries = if ($packs -is [System.Collections.IDictionary]) {
            @($packs.GetEnumerator() | ForEach-Object {
                [PSCustomObject]@{ Name = [string]$_.Key; Value = $_.Value }
            })
        } else {
            @($packs.PSObject.Properties | ForEach-Object {
                [PSCustomObject]@{ Name = $_.Name; Value = $_.Value }
            })
        }

        foreach ($entry in $entries) {
            $representative = $representatives |
                Where-Object { $entry.Name -match $_.Pattern } |
                Select-Object -First 1
            if (-not $representative) { continue }
            $version = [string](Get-InstallabilityProperty $entry.Value 'version')
            if ([string]::IsNullOrWhiteSpace($version)) { continue }
            $key = "$($entry.Name.ToLowerInvariant())|$($version.ToLowerInvariant())"
            if ($seen.ContainsKey($key)) { continue }
            $seen[$key] = $true
            [void]$requests.Add([PSCustomObject]@{
                Category  = $representative.Category
                PackageId = $entry.Name
                Version   = $version
            })
        }
    }

    foreach ($representative in $representatives) {
        if (@($requests | Where-Object { $_.Category -eq $representative.Category }).Count -eq 0) {
            [void]$requests.Add([PSCustomObject]@{
                Category  = $representative.Category
                PackageId = $null
                Version   = $null
            })
        }
    }

    return @($requests)
}

function ConvertTo-IsolatedNuGetConfig {
    param([Parameter(Mandatory)][array]$Sources)

    $builder = [Text.StringBuilder]::new()
    [void]$builder.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$builder.AppendLine('<configuration>')
    [void]$builder.AppendLine('  <packageSources>')
    [void]$builder.AppendLine('    <clear />')
    foreach ($source in @($Sources | Sort-Object -Property Name -Unique)) {
        $name = [Security.SecurityElement]::Escape([string]$source.Name)
        $uri = [Security.SecurityElement]::Escape([string]$source.Uri)
        [void]$builder.AppendLine("    <add key=`"$name`" value=`"$uri`" protocolVersion=`"3`" />")
    }
    [void]$builder.AppendLine('  </packageSources>')
    [void]$builder.AppendLine('</configuration>')
    return $builder.ToString()
}

function ConvertTo-PublicInstallabilityResult {
    param([Parameter(Mandatory)]$Result)

    $sensitiveSourceNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($source in @($Result.RequiredSources)) {
        if ([bool](Get-InstallabilityProperty $source 'IsAdditional') -or
            [bool](Get-InstallabilityProperty $source 'IsInternal')) {
            [void]$sensitiveSourceNames.Add([string](Get-InstallabilityProperty $source 'Name'))
        }
    }

    $sanitizeLocation = {
        param($Location)

        $resolvedSource = Get-InstallabilityProperty $Location 'ResolvedSource'
        $source = Get-InstallabilityProperty $resolvedSource 'Source'
        $sourceName = [string](Get-InstallabilityProperty $source 'Name')
        $isSensitive = [bool](Get-InstallabilityProperty $source 'IsAdditional') -or
            [bool](Get-InstallabilityProperty $source 'IsInternal')
        $unknownSources = @(
            @(Get-InstallabilityProperty $Location 'UnknownSources') |
                ForEach-Object {
                    if ($sensitiveSourceNames.Contains([string]$_)) { 'authenticated-source' } else { [string]$_ }
                } |
                Select-Object -Unique
        )

        return [PSCustomObject]@{
            Category       = Get-InstallabilityProperty $Location 'Category'
            WorkloadId     = Get-InstallabilityProperty $Location 'WorkloadId'
            PackageId      = Get-InstallabilityProperty $Location 'PackageId'
            Version        = Get-InstallabilityProperty $Location 'Version'
            Status         = Get-InstallabilityProperty $Location 'Status'
            ContentStatus  = Get-InstallabilityProperty $Location 'ContentStatus'
            Reason         = Get-InstallabilityProperty $Location 'Reason'
            SourceName     = if ($isSensitive) { 'authenticated-source' } else { $sourceName }
            SourceRole     = if ($isSensitive) { 'additional' } else { Get-InstallabilityProperty $source 'Role' }
            UnknownSources = $unknownSources
        }
    }

    $copy = [ordered]@{}
    foreach ($property in $Result.PSObject.Properties) {
        $copy[$property.Name] = $property.Value
    }

    $copy.NuGetConfig = $null
    $copy.InstallCommand = if ($Result.CliVersion) {
        "dotnet workload install maui --version $($Result.CliVersion) --configfile <local-nuget-config>"
    } else { $null }
    $copy.ManifestPackages = @($Result.ManifestPackages | ForEach-Object { & $sanitizeLocation $_ })
    $copy.PackProbes = @($Result.PackProbes | ForEach-Object { & $sanitizeLocation $_ })
    $copy.RequiredSources = @(
        $Result.RequiredSources | ForEach-Object {
            if ($_.IsAdditional -or $_.IsInternal) {
                [PSCustomObject]@{
                    Name       = 'authenticated-source'
                    Role       = 'additional'
                    Uri        = $null
                    IsInternal = $true
                }
            } else {
                [PSCustomObject]@{
                    Name       = $_.Name
                    Role       = $_.Role
                    Uri        = $_.Uri
                    IsInternal = $false
                }
            }
        } | Sort-Object -Property Name -Unique
    )
    return [PSCustomObject]$copy
}

function Complete-PreviewInstallabilityResult {
    param(
        [Parameter(Mandatory)]$Result,
        [bool]$PublicSafe
    )

    if ($PublicSafe) {
        return ConvertTo-PublicInstallabilityResult -Result $Result
    }
    return $Result
}

function ConvertTo-PreviewInstallabilityCheck {
    param([Parameter(Mandatory)]$Result)

    $status = switch ($Result.Status) {
        'installable' { 'READY' }
        'missing' { 'BLOCKED' }
        'mismatched' { 'BLOCKED' }
        default { 'UNKNOWN' }
    }
    $nextAction = switch ($Result.Status) {
        'installable' { 'No installability action needed.' }
        'missing' { 'Publish or add the missing workload assets, then rerun this check with an isolated package-source configuration.' }
        'mismatched' { 'Align the workload set with the branch SDK, Android, Apple, and runtime pins and the target MAUI Preview train before shipping.' }
        default {
            if (-not $Result.VersionConfirmed) {
                'Supply the confirmed workload-set CLI version and any required authenticated package source, then rerun locally.'
            } else {
                'Authenticate the required package sources and rerun; do not treat an inaccessible source as proof that a package is missing.'
            }
        }
    }

    return [PSCustomObject]@{
        Area       = 'Consumer installability'
        Status     = $status
        Details    = $Result.Summary
        NextAction = $nextAction
    }
}

function Get-PreviewConsumerInstallability {
    param(
        [Parameter(Mandatory)][int]$Major,
        [Parameter(Mandatory)][int]$Preview,
        [AllowNull()]$Pins,
        [string]$WorkloadSetCliVersion,
        [string]$ExpectedMauiManifestVersion,
        [string[]]$AdditionalPackageSource = @(),
        [bool]$PublicSafe = $true,
        [scriptblock]$Fetcher,
        [scriptblock]$PackageReader
    )

    $sdkVersion = [string](Get-InstallabilityProperty (Get-InstallabilityProperty $Pins 'Vmr') 'Version')
    if ([string]::IsNullOrWhiteSpace($sdkVersion)) {
        $result = [PSCustomObject]@{
            Status = 'unknown'; Summary = 'The branch SDK pin could not be resolved.'
            SdkVersion = $null; SdkFeatureBand = $null; PackageId = $null
            CliVersion = $null; NuGetVersion = $null; VersionConfirmed = $false
            PinComparisons = @(); ManifestPackages = @(); PackProbes = @()
            RequiredSources = @(); PlatformRequirements = $null
            NuGetConfig = $null; InstallCommand = $null
        }
        return Complete-PreviewInstallabilityResult -Result $result -PublicSafe $PublicSafe
    }

    $sources = ConvertFrom-PreviewPackageSourceSpec -Major $Major -AdditionalPackageSource $AdditionalPackageSource
    $resolvedSources = @($sources | ForEach-Object {
        Resolve-PreviewPackageSource -Source $_ -Fetcher $Fetcher
    })

    $featureBand = Get-PreviewSdkFeatureBand -SdkVersion $sdkVersion
    $packageId = "Microsoft.NET.Workloads.$featureBand-preview.$Preview"
    $requestedNuGetVersion = $null
    $versionMismatch = $null
    if ($WorkloadSetCliVersion) {
        try {
            $requestedFeatureBand = Get-PreviewSdkFeatureBand -SdkVersion $WorkloadSetCliVersion
            $requestedNuGetVersion = ConvertTo-WorkloadSetNuGetVersion -CliVersion $WorkloadSetCliVersion
            if ($requestedFeatureBand -ne $featureBand) {
                $versionMismatch = "The confirmed workload-set feature band $requestedFeatureBand does not match branch SDK band $featureBand."
            } elseif ($WorkloadSetCliVersion -notmatch "-preview\.$Preview(?:\.|$)") {
                $versionMismatch = "The confirmed workload-set version does not target preview $Preview."
            }
        } catch {
            $versionMismatch = 'The confirmed workload-set CLI version is not valid.'
        }
    }

    if ($versionMismatch) {
        $result = [PSCustomObject]@{
            Status = 'mismatched'
            Summary = $versionMismatch
            SdkVersion = $sdkVersion; SdkFeatureBand = $featureBand; PackageId = $packageId
            CliVersion = $WorkloadSetCliVersion; NuGetVersion = $requestedNuGetVersion; VersionConfirmed = $true
            PinComparisons = @(); ManifestPackages = @(); PackProbes = @()
            RequiredSources = @($sources); PlatformRequirements = $null
            NuGetConfig = $null; InstallCommand = $null
        }
        return Complete-PreviewInstallabilityResult -Result $result -PublicSafe $PublicSafe
    }

    $package = $null
    if ($requestedNuGetVersion) {
        $location = Find-PreviewPackageLocation -ResolvedSources $resolvedSources `
            -PackageId $packageId -Version $requestedNuGetVersion -Fetcher $Fetcher
        if ($location.Status -ne 'found') {
            $result = [PSCustomObject]@{
                Status = $location.Status
                Summary = if ($location.Status -eq 'missing') {
                    "The confirmed workload-set version $WorkloadSetCliVersion is not available from the supplied sources."
                } else {
                    "Availability of the confirmed workload-set version $WorkloadSetCliVersion could not be established."
                }
                SdkVersion = $sdkVersion; SdkFeatureBand = $featureBand; PackageId = $packageId
                CliVersion = $WorkloadSetCliVersion; NuGetVersion = $requestedNuGetVersion; VersionConfirmed = $true
                PinComparisons = @(); ManifestPackages = @(); PackProbes = @()
                RequiredSources = @($sources); PlatformRequirements = $null
                NuGetConfig = $null; InstallCommand = $null
            }
            return Complete-PreviewInstallabilityResult -Result $result -PublicSafe $PublicSafe
        }

        $package = [PSCustomObject]@{
            Id       = $packageId
            Source   = $location.ResolvedSource
            Versions = @($requestedNuGetVersion)
        }
    } else {
        $package = Find-PreviewWorkloadSetPackage -ResolvedSources $resolvedSources `
            -SdkFeatureBand $featureBand -Preview $Preview -Fetcher $Fetcher
        if (-not $package) {
            $result = [PSCustomObject]@{
                Status = 'unknown'
                Summary = "No workload-set candidate was discovered for SDK band $featureBand preview $Preview, and no release-owner-confirmed version was supplied."
                SdkVersion = $sdkVersion; SdkFeatureBand = $featureBand; PackageId = $packageId
                CliVersion = $null; NuGetVersion = $null; VersionConfirmed = $false
                PinComparisons = @(); ManifestPackages = @(); PackProbes = @()
                RequiredSources = @($sources); PlatformRequirements = $null
                NuGetConfig = $null; InstallCommand = $null
            }
            return Complete-PreviewInstallabilityResult -Result $result -PublicSafe $PublicSafe
        }
    }

    $versions = @(Sort-PreviewNuGetVersions -Version $package.Versions)
    $candidateVersions = if ($requestedNuGetVersion) { @($requestedNuGetVersion) } else { @($versions | Select-Object -First 20) }
    $selectedVersion = $null
    $selectedManifest = $null
    $selectedComparisons = @()
    $lastComparisons = @()
    foreach ($version in $candidateVersions) {
        try {
            $entries = if ($PackageReader) {
                & $PackageReader $package.Source $package.Id $version @('data/microsoft.net.workloads.workloadset.json')
            } else {
                Read-PreviewNuGetPackageEntries -ResolvedSource $package.Source -PackageId $package.Id -Version $version `
                    -EntryName @('data/microsoft.net.workloads.workloadset.json')
            }
            $manifest = Get-InstallabilityProperty $entries 'data/microsoft.net.workloads.workloadset.json'
            if (-not $manifest) { continue }
            $comparisons = @(Compare-PreviewWorkloadSetPins -Manifest $manifest -Pins $Pins -Major $Major -Preview $Preview `
                -ExpectedMauiManifestVersion $ExpectedMauiManifestVersion)
            $lastComparisons = $comparisons
            if (@($comparisons | Where-Object { $_.Status -in @('mismatch', 'missing') }).Count -eq 0) {
                $selectedVersion = $version
                $selectedManifest = $manifest
                $selectedComparisons = $comparisons
                break
            }
        } catch {
            continue
        }
    }

    if (-not $selectedManifest) {
        $status = if ($lastComparisons.Count -gt 0) { 'mismatched' } else { 'unknown' }
        $result = [PSCustomObject]@{
            Status = $status
            Summary = if ($status -eq 'mismatched') {
                'Available workload-set candidates do not match the branch component pins.'
            } else {
                'The workload-set package was found, but its manifest could not be read.'
            }
            SdkVersion = $sdkVersion; SdkFeatureBand = $featureBand; PackageId = $package.Id
            CliVersion = $WorkloadSetCliVersion; NuGetVersion = $requestedNuGetVersion; VersionConfirmed = [bool]$WorkloadSetCliVersion
            PinComparisons = $lastComparisons; ManifestPackages = @(); PackProbes = @()
            RequiredSources = @($package.Source.Source); PlatformRequirements = $null
            NuGetConfig = $null; InstallCommand = $null
        }
        return Complete-PreviewInstallabilityResult -Result $result -PublicSafe $PublicSafe
    }

    $selectedCliVersion = ConvertTo-WorkloadSetCliVersion -NuGetVersion $selectedVersion -SdkFeatureBand $featureBand
    $manifestRequests = @(Get-PreviewManifestPackageRequests -Manifest $selectedManifest)
    $manifestLocations = [System.Collections.Generic.List[object]]::new()
    foreach ($request in $manifestRequests) {
        $location = Find-PreviewPackageLocation -ResolvedSources $resolvedSources -PackageId $request.PackageId `
            -Version $request.Version -Fetcher $Fetcher
        [void]$manifestLocations.Add([PSCustomObject]@{
            WorkloadId      = $request.WorkloadId
            PackageId       = $request.PackageId
            Version         = $request.Version
            Status          = $location.Status
            ContentStatus   = if ($location.Status -eq 'found') { 'pending' } else { $null }
            ResolvedSource  = $location.ResolvedSource
            UnknownSources  = $location.UnknownSources
        })
    }

    $manifestEvidence = [System.Collections.Generic.List[object]]::new()
    foreach ($location in @($manifestLocations | Where-Object { $_.Status -eq 'found' })) {
        try {
            $entries = if ($PackageReader) {
                & $PackageReader $location.ResolvedSource $location.PackageId $location.Version `
                    @('data/WorkloadDependencies.json', 'data/WorkloadManifest.json')
            } else {
                Read-PreviewNuGetPackageEntries -ResolvedSource $location.ResolvedSource -PackageId $location.PackageId `
                    -Version $location.Version -EntryName @('data/WorkloadDependencies.json', 'data/WorkloadManifest.json')
            }
            $manifestContent = Get-InstallabilityProperty $entries 'data/WorkloadManifest.json'
            $location.ContentStatus = if ($manifestContent) { 'read' } else { 'unknown' }
            [void]$manifestEvidence.Add([PSCustomObject]@{
                WorkloadId   = $location.WorkloadId
                Dependencies = Get-InstallabilityProperty $entries 'data/WorkloadDependencies.json'
                Manifest     = $manifestContent
            })
        } catch {
            $location.ContentStatus = 'unknown'
            [void]$manifestEvidence.Add([PSCustomObject]@{
                WorkloadId   = $location.WorkloadId
                Dependencies = $null
                Manifest     = $null
            })
        }
    }

    $packRequests = @(Get-PreviewRepresentativePackRequests -ManifestEvidence $manifestEvidence -Major $Major)
    $packLocations = [System.Collections.Generic.List[object]]::new()
    foreach ($request in $packRequests) {
        if ([string]::IsNullOrWhiteSpace($request.PackageId) -or [string]::IsNullOrWhiteSpace($request.Version)) {
            [void]$packLocations.Add([PSCustomObject]@{
                Category       = $request.Category
                PackageId      = $null
                Version        = $null
                Status         = 'unknown'
                Reason         = "The $($request.Category) representative pack could not be derived from the component manifests."
                ResolvedSource = $null
                UnknownSources = @()
            })
            continue
        }

        $location = Find-PreviewPackageLocation -ResolvedSources $resolvedSources -PackageId $request.PackageId `
            -Version $request.Version -Fetcher $Fetcher
        [void]$packLocations.Add([PSCustomObject]@{
            Category       = $request.Category
            PackageId      = $request.PackageId
            Version        = $request.Version
            Status         = $location.Status
            Reason         = $null
            ResolvedSource = $location.ResolvedSource
            UnknownSources = $location.UnknownSources
        })
    }

    $allLocations = @($manifestLocations) + @($packLocations)
    $missing = @($allLocations | Where-Object { $_.Status -eq 'missing' })
    $unknown = @($allLocations | Where-Object { $_.Status -eq 'unknown' })
    $unreadableManifests = @($manifestLocations | Where-Object {
        $_.Status -eq 'found' -and $_.ContentStatus -ne 'read'
    })
    $status = if ($missing.Count -gt 0) {
        'missing'
    } elseif ($unknown.Count -gt 0 -or $unreadableManifests.Count -gt 0) {
        'unknown'
    } elseif (-not $WorkloadSetCliVersion) {
        'unknown'
    } else {
        'installable'
    }

    $requiredSources = [System.Collections.Generic.List[object]]::new()
    [void]$requiredSources.Add($package.Source.Source)
    foreach ($location in @($allLocations | Where-Object { $_.Status -eq 'found' })) {
        [void]$requiredSources.Add($location.ResolvedSource.Source)
    }
    foreach ($baseline in @($sources | Where-Object { $_.Role -eq 'shared' })) {
        [void]$requiredSources.Add($baseline)
    }
    if ($status -ne 'installable') {
        foreach ($additional in @($sources | Where-Object { $_.IsAdditional })) {
            [void]$requiredSources.Add($additional)
        }
    }
    $requiredSources = @($requiredSources | Sort-Object -Property Name -Unique)

    $config = ConvertTo-IsolatedNuGetConfig -Sources $requiredSources
    $command = "dotnet workload install maui --version $selectedCliVersion --configfile ./preview-nuget.config"
    $summary = switch ($status) {
        'installable' { 'The confirmed workload set matches branch pins and its required manifest and representative pack assets are resolvable.' }
        'missing' { "$($missing.Count) required manifest or representative pack asset(s) were not found in the supplied sources." }
        'unknown' {
            if (-not $WorkloadSetCliVersion) {
                'A coherent public workload-set candidate was found, but its official CLI version was not supplied and full source availability could not be confirmed.'
            } else {
                "$($unknown.Count + $unreadableManifests.Count) required asset location or manifest content check(s) could not be confirmed."
            }
        }
    }

    $result = [PSCustomObject]@{
        Status               = $status
        Summary              = $summary
        SdkVersion           = $sdkVersion
        SdkFeatureBand       = $featureBand
        PackageId            = $package.Id
        CliVersion           = $selectedCliVersion
        NuGetVersion         = $selectedVersion
        VersionConfirmed     = [bool]$WorkloadSetCliVersion
        PinComparisons       = $selectedComparisons
        ManifestPackages     = @($manifestLocations)
        PackProbes           = @($packLocations)
        RequiredSources      = $requiredSources
        PlatformRequirements = Get-PreviewPlatformRequirements -ManifestEvidence $manifestEvidence
        NuGetConfig          = $config
        InstallCommand       = $command
    }

    return Complete-PreviewInstallabilityResult -Result $result -PublicSafe $PublicSafe
}

function Format-PreviewInstallabilityMarkdown {
    param(
        [Parameter(Mandatory)]$Result,
        [bool]$PublicSafe = $true
    )

    $builder = [Text.StringBuilder]::new()
    [void]$builder.AppendLine('## Consumer installability')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine("**Status:** **$($Result.Status)** - $($Result.Summary)")
    [void]$builder.AppendLine('')
    if ($Result.PackageId) {
        [void]$builder.AppendLine("| Evidence | Value |")
        [void]$builder.AppendLine("|----------|-------|")
        [void]$builder.AppendLine("| SDK | ``$($Result.SdkVersion)`` |")
        [void]$builder.AppendLine("| Workload-set package | ``$($Result.PackageId)`` |")
        [void]$builder.AppendLine("| CLI version | ``$($Result.CliVersion)`` |")
        [void]$builder.AppendLine("| NuGet package version | ``$($Result.NuGetVersion)`` |")
        [void]$builder.AppendLine("| Version source | $(if ($Result.VersionConfirmed) { 'release evidence supplied to the script' } else { 'latest coherent public candidate; official confirmation still required' }) |")
        [void]$builder.AppendLine('')
    }

    if (@($Result.PinComparisons).Count -gt 0) {
        [void]$builder.AppendLine("| Component | Expected | Workload set | Result |")
        [void]$builder.AppendLine("|-----------|----------|--------------|--------|")
        foreach ($comparison in $Result.PinComparisons) {
            [void]$builder.AppendLine("| ``$($comparison.WorkloadId)`` | ``$($comparison.Expected)`` | ``$($comparison.Actual)`` | **$($comparison.Status)** |")
        }
        [void]$builder.AppendLine('')
    }

    if (@($Result.ManifestPackages).Count -gt 0) {
        [void]$builder.AppendLine("| Manifest package | Version | Availability | Content | Source |")
        [void]$builder.AppendLine("|------------------|---------|--------------|---------|--------|")
        foreach ($manifestPackage in $Result.ManifestPackages) {
            $sourceName = Get-InstallabilityProperty $manifestPackage 'SourceName'
            if ([string]::IsNullOrWhiteSpace([string]$sourceName)) {
                $resolved = Get-InstallabilityProperty $manifestPackage 'ResolvedSource'
                $sourceName = Get-InstallabilityProperty (Get-InstallabilityProperty $resolved 'Source') 'Name'
            }
            if ([string]::IsNullOrWhiteSpace([string]$sourceName)) { $sourceName = '—' }
            $contentStatus = Get-InstallabilityProperty $manifestPackage 'ContentStatus'
            if ([string]::IsNullOrWhiteSpace([string]$contentStatus)) { $contentStatus = '—' }
            [void]$builder.AppendLine("| ``$($manifestPackage.PackageId)`` | ``$($manifestPackage.Version)`` | **$($manifestPackage.Status)** | **$contentStatus** | ``$sourceName`` |")
        }
        [void]$builder.AppendLine('')
    }

    if (@($Result.PackProbes).Count -gt 0) {
        [void]$builder.AppendLine("| Representative | Package | Version | Availability | Source |")
        [void]$builder.AppendLine("|----------------|---------|---------|--------------|--------|")
        foreach ($packProbe in $Result.PackProbes) {
            $sourceName = Get-InstallabilityProperty $packProbe 'SourceName'
            if ([string]::IsNullOrWhiteSpace([string]$sourceName)) {
                $resolved = Get-InstallabilityProperty $packProbe 'ResolvedSource'
                $sourceName = Get-InstallabilityProperty (Get-InstallabilityProperty $resolved 'Source') 'Name'
            }
            if ([string]::IsNullOrWhiteSpace([string]$sourceName)) { $sourceName = '—' }
            $packageId = if ([string]::IsNullOrWhiteSpace([string]$packProbe.PackageId)) { 'not derived' } else { "``$($packProbe.PackageId)``" }
            $version = if ([string]::IsNullOrWhiteSpace([string]$packProbe.Version)) { '—' } else { "``$($packProbe.Version)``" }
            [void]$builder.AppendLine("| $($packProbe.Category) | $packageId | $version | **$($packProbe.Status)** | ``$sourceName`` |")
        }
        [void]$builder.AppendLine('')
    }

    if (@($Result.RequiredSources).Count -gt 0) {
        $sourceNames = @($Result.RequiredSources | ForEach-Object { "``$($_.Name)``" }) -join ', '
        [void]$builder.AppendLine("**Required package sources:** $sourceNames")
        [void]$builder.AppendLine('')
    }

    $requirements = $Result.PlatformRequirements
    if ($requirements) {
        $items = [System.Collections.Generic.List[string]]::new()
        if ($requirements.Jdk) {
            [void]$items.Add("JDK ``$($requirements.Jdk.Version)`` (recommended ``$($requirements.Jdk.RecommendedVersion)``)")
        }
        if (@($requirements.AndroidSdk).Count -gt 0) {
            $androidPackages = @($requirements.AndroidSdk | ForEach-Object { "``$_``" }) -join ', '
            [void]$items.Add("Android SDK: $androidPackages")
        }
        if ($requirements.Xcode) {
            [void]$items.Add("Xcode ``$($requirements.Xcode.Version)`` (recommended ``$($requirements.Xcode.RecommendedVersion)``)")
        }
        if ($requirements.AppleSdk) {
            [void]$items.Add("Apple SDK ``$($requirements.AppleSdk)``")
        }
        if ($requirements.WindowsAppSdk) {
            [void]$items.Add("Windows App SDK ``$($requirements.WindowsAppSdk)``")
        }
        if ($items.Count -gt 0) {
            [void]$builder.AppendLine("**Platform prerequisites:** $($items -join '; ')")
            [void]$builder.AppendLine('')
        }
    }

    if (-not $PublicSafe -and $Result.NuGetConfig -and $Result.InstallCommand) {
        [void]$builder.AppendLine('Create a local-only NuGet configuration:')
        [void]$builder.AppendLine('')
        [void]$builder.AppendLine('```xml')
        [void]$builder.Append($Result.NuGetConfig.TrimEnd())
        [void]$builder.AppendLine('')
        [void]$builder.AppendLine('```')
        [void]$builder.AppendLine('')
        [void]$builder.AppendLine('Then install with the isolated configuration:')
        [void]$builder.AppendLine('')
        [void]$builder.AppendLine('```bash')
        [void]$builder.AppendLine($Result.InstallCommand)
        [void]$builder.AppendLine('```')
        [void]$builder.AppendLine('')
    } elseif ($PublicSafe) {
        [void]$builder.AppendLine('_Exact feed URLs and credential setup are intentionally local-only. Re-run with ``-PublicSafe $false`` and the confirmed workload-set CLI version to generate an isolated ``<clear />`` configuration. If an authenticated source is required, use a short-lived Packaging Read PAT through NuGet credentials; never place it in the tracker or repository._')
        [void]$builder.AppendLine('')
    }

    return $builder.ToString()
}
