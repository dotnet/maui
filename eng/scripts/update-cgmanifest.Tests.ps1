#Requires -Modules Pester

Describe 'Native test dependency inventory' {
    It 'generates real xUnit v3 and MTP package IDs from central versions' {
        $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
        $fixture = Join-Path $TestDrive 'inventory'
        New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'eng/scripts'), (Join-Path $fixture 'src/Templates/src') | Out-Null
        Copy-Item -LiteralPath (Join-Path $repoRoot 'eng/Versions.props'), (Join-Path $repoRoot 'eng/NuGetVersions.targets') -Destination (Join-Path $fixture 'eng')
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'update-cgmanifest.ps1') -Destination (Join-Path $fixture 'eng/scripts')
        & (Join-Path $fixture 'eng/scripts/update-cgmanifest.ps1')
        $manifest = Get-Content -LiteralPath (Join-Path $fixture 'src/Templates/src/cgmanifest.json') -Raw | ConvertFrom-Json
        [xml]$versions = Get-Content -LiteralPath (Join-Path $fixture 'eng/Versions.props') -Raw
        $xunitVersion = $versions.SelectSingleNode('//XunitPackageVersion').InnerText
        $mtpVersion = $versions.SelectSingleNode('//MicrosoftTestingPlatformVersion').InnerText
        $packages = @{}
        foreach ($registration in $manifest.registrations) {
            $packages[$registration.component.nuget.name] = $registration.component.nuget.version
        }
        foreach ($package in @('xunit.v3.mtp-v2', 'xunit.v3.core.mtp-v2', 'xunit.v3.extensibility.core', 'xunit.v3.assert')) {
            $packages[$package] | Should -Be $xunitVersion
        }
        foreach ($package in @('Microsoft.Testing.Platform.MSBuild', 'Microsoft.Testing.Extensions.TrxReport', 'Microsoft.Testing.Extensions.CrashDump')) {
            $packages[$package] | Should -Be $mtpVersion
        }
        $packages['xunit.analyzers'] | Should -Be $versions.SelectSingleNode('//XUnitAnalyzersPackageVersion').InnerText
        foreach ($package in @('xunit', 'xunit.runner.visualstudio', 'xunit.analyzer')) {
            $packages.ContainsKey($package) | Should -BeFalse
        }
    }
}
