param(
    [string] $ArtifactsPath = (Join-Path $PSScriptRoot '../../artifacts/tests/workload-msi-versions')
)

$ErrorActionPreference = 'Stop'
$testDirectory = Join-Path ([IO.Path]::GetFullPath($ArtifactsPath)) ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null

function Invoke-VersionTargets {
    param(
        [object[]] $Cases,
        [switch] $IncludeLegacyTarget,
        [switch] $ExpectFailure
    )

    $propsPath = Join-Path $testDirectory 'vs-workload.props'
    $items = foreach ($case in $Cases) {
        $identity = [Security.SecurityElement]::Escape(
            (Join-Path $testDirectory "$($case.PackageId).$($case.Version).nupkg"))
        $msiVersion = [Security.SecurityElement]::Escape([string] $case.MsiVersion)
        @"
    <WorkloadPackages Include="$identity" Version="$($case.Version)" SupportsMachineArch="true"
        MsiVersion="$msiVersion" UseNuGetVersionAsMsiVersion="$($case.UseNuGetVersionAsMsiVersion)" />
"@
    }
    Set-Content -LiteralPath $propsPath -Value "<Project><ItemGroup>$($items -join "`n")</ItemGroup></Project>"

    $targets = '_SetMsiVersionFromNuGetVersion'
    if ($IncludeLegacyTarget) {
        $targets += ';_CreateManifestMsiVersionsWithDateTime'
    }
    # Evaluate the real packaging targets without restoring Windows signing tools or running MSI creation.
    $output = & dotnet msbuild (Join-Path $PSScriptRoot 'workloads.csproj') -nologo -verbosity:quiet `
        "-t:$targets" -getItem:WorkloadPackages `
        -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false `
        "-p:WorkloadMsiGenProps=$propsPath" 2>&1
    $exitCode = $LASTEXITCODE
    $text = $output -join "`n"
    if ($ExpectFailure) {
        if ($exitCode -eq 0 -or $text -notmatch 'Cannot generate MSI version.*MSI major/minor limited to 255 and patch limited to 65535') {
            throw "Expected an explicit MSI bounds error, got exit code ${exitCode}:`n$text"
        }
        return
    }
    if ($exitCode -ne 0) {
        throw "Version target execution failed with exit code ${exitCode}:`n$text"
    }

    $packages = @((ConvertFrom-Json $text).Items.WorkloadPackages)
    if ($packages.Count -ne $Cases.Count) {
        throw "Expected $($Cases.Count) packages, got $($packages.Count)."
    }
    for ($i = 0; $i -lt $Cases.Count; $i++) {
        $case = $Cases[$i]
        $package = $packages[$i]
        if ([string] $package.MsiVersion -cne [string] $case.Expected) {
            throw "$($case.PackageId) $($case.Version): expected MSI '$($case.Expected)', got '$($package.MsiVersion)'."
        }
        if ($package.Version -cne $case.Version -or $package.SupportsMachineArch -ne 'true') {
            throw "Product version or architecture metadata changed for $($package.Identity)."
        }
    }
    return $packages
}

try {
    $stableId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.100'
    $stableCases = foreach ($patch in @(0, 1, 10, 15, 16, 31, 32, 255, 256, 65535)) {
        @{
            PackageId = $stableId
            Version = "11.0.$patch.9706"
            Expected = "111.0.$patch"
        }
    }
    $stableCases += @(
        @{ PackageId = $stableId; Version = '11.1.0.0'; Expected = '111.1.0' }
        @{ PackageId = $stableId; Version = '155.255.65535.0'; Expected = '255.255.65535' }
    )
    $stablePackages = @(Invoke-VersionTargets -Cases $stableCases)
    Invoke-VersionTargets -Cases $stableCases -IncludeLegacyTarget | Out-Null
    if ([version] $stablePackages[0].MsiVersion -le [version] '88.0.61402') {
        throw 'Stable net11 MSI version must sort above the previously shipped RC2 version.'
    }
    for ($i = 1; $i -lt $stablePackages.Count; $i++) {
        if ([version] $stablePackages[$i].MsiVersion -le [version] $stablePackages[$i - 1].MsiVersion) {
            throw "Successive product versions must produce increasing MSI versions (index $i)."
        }
    }
    Write-Host "PASS: $($stableCases.Count) stable/servicing versions, RC2 ordering, large patches and maximum MSI bounds."

    $otherCases = @(
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-10.0.100'; Version = '10.0.10.0'; Expected = '10.0.10' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.200'; Version = '11.0.10.0'; Expected = '11.0.10' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-12.0.100'; Version = '12.0.0.0'; Expected = '12.0.0' }
        @{ PackageId = 'Microsoft.NET.Sdk.Android.Manifest-11.0.100'; Version = '37.0.0.0'; Expected = '37.0.0' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.1000'; Version = '11.0.0.0'; Expected = '11.0.0' }
        @{ PackageId = $stableId; Version = '11.0.256.9706'; MsiVersion = '120.2.256'; Expected = '120.2.256' }
        @{ PackageId = $stableId; Version = '156.256.65536.0'; MsiVersion = '120.2.300'; Expected = '120.2.300' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.100-rc.2'; Version = '11.0.0.9706'; MsiVersion = '88.0.61402'; Expected = '88.0.61402' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.100-preview.7'; Version = '11.0.0.9706'; UseNuGetVersionAsMsiVersion = 'true'; Expected = '11.0.0' }
    )
    Invoke-VersionTargets -Cases ($stableCases + $otherCases) -IncludeLegacyTarget | Out-Null
    Write-Host "PASS: $($otherCases.Count) unaffected bands, package identities, opt-ins and explicit overrides in a mixed batch."

    # The legacy tasks are intentionally not loaded: these cases must still defer to their unchanged encoder.
    $legacyCases = @(
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-6.0.400'; Version = '6.0.0.9706'; Expected = '' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.100-rc.2'; Version = '11.0.0.9706'; Expected = '' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.100-preview.7'; Version = '11.0.0.9706'; Expected = '' }
        @{ PackageId = 'Microsoft.NET.Sdk.Maui.Manifest-11.0.100-ci.main.1'; Version = '11.0.0.9706'; Expected = '' }
    )
    Invoke-VersionTargets -Cases ($stableCases + $legacyCases) | Out-Null
    Write-Host "PASS: $($legacyCases.Count) legacy/preview manifests remain on the existing encoder path."

    foreach ($version in @('156.0.0.0', '11.256.0.0', '11.0.65536.0', '11.0')) {
        Invoke-VersionTargets -Cases @(@{ PackageId = $stableId; Version = $version }) -ExpectFailure
    }
    Write-Host 'PASS: major, minor and patch overflow and a missing patch fail explicitly without wrapping or falling back.'
}
finally {
    Remove-Item -LiteralPath $testDirectory -Recurse -Force
}
