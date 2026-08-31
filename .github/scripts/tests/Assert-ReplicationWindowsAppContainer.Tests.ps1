#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot '../shared/Assert-ReplicationWindowsAppContainer.ps1')
    $script:BridgePath = Join-Path $PSScriptRoot (
        '../shared/Invoke-ReplicationWindowsAppx.ps1')
    $script:SandboxManifest = Join-Path $PSScriptRoot (
        '../../../src/Controls/samples/Controls.Sample.Sandbox/Platforms/Windows/ReplicationAppContainerManifest.xml')
    $script:DeviceManifest = Join-Path $PSScriptRoot (
        '../../../src/Controls/tests/DeviceTests/Platforms/Windows/ReplicationAppContainerManifest.xml')
    $script:OverrideTargets = Join-Path $PSScriptRoot (
        '../shared/ReplicationWindowsAppContainerManifest.targets')
}

Describe 'Windows AppX compatibility bridge' {
    It 'keeps AppX cmdlets inside a fixed Windows PowerShell 5.1 script' {
        $bridge = Get-Content -LiteralPath $script:BridgePath -Raw
        $parent = Get-Content -LiteralPath (Join-Path $PSScriptRoot (
            '../shared/Assert-ReplicationWindowsAppContainer.ps1')) -Raw
        $sandbox = Get-Content -LiteralPath (Join-Path $PSScriptRoot (
            '../BuildAndRunSandbox.ps1')) -Raw

        $bridge | Should -Match '^#!/usr/bin/env powershell\.exe'
        $bridge | Should -Match (
            "\[ValidateSet\('Query', 'Install', 'Remove'\)\]")
        $bridge | Should -Match 'Get-AppxPackage'
        $bridge | Should -Match 'Get-AppxPackageManifest'
        $bridge | Should -Match 'Add-AppxPackage'
        $bridge | Should -Match 'Remove-AppxPackage'
        $parent | Should -Not -Match (
            '(?m)^\s*(?:Get|Add|Remove)-AppxPackage')
        $sandbox | Should -Not -Match (
            '(?m)^\s*(?:Get|Add|Remove)-AppxPackage')
        $parent | Should -Match (
            'System32\\WindowsPowerShell\\v1\.0\\powershell\.exe')
        $parent | Should -Match '\$startInfo\.Environment\.Clear\(\)'
    }
}

Describe 'Windows replication AppContainer manifests' {
    It 'accepts only the dedicated capability-free manifests' {
        foreach ($path in @($script:SandboxManifest, $script:DeviceManifest)) {
            $result = Assert-ReplicationWindowsAppContainerManifest -Path $path
            $result.Publisher | Should -BeExactly 'CN=DotNetMauiReplication'
            $result.TrustLevel | Should -BeExactly 'appContainer'
            $result.RuntimeBehavior | Should -BeExactly 'packagedClassicApp'
        }
    }

    It 'rejects every network, full-trust, and device capability' {
        $source = Get-Content -LiteralPath $script:SandboxManifest -Raw
        foreach ($capability in @(
            'runFullTrust',
            'internetClient',
            'internetClientServer',
            'privateNetworkClientServer',
            'location'
        )) {
            $mutated = $source.Replace(
                '</Package>',
                "<Capabilities><Capability Name=`"$capability`" /></Capabilities></Package>")
            $document = Read-ReplicationWindowsManifestXml `
                -Content $mutated `
                -Description 'mutated manifest'
            {
                Assert-ReplicationWindowsAppContainerManifestDocument `
                    -Document $document `
                    -Description 'mutated manifest'
            } | Should -Throw '*must not declare package capabilities*'
        }
    }

    It 'rejects full-trust runtime behavior and URI or service extensions' {
        $source = Get-Content -LiteralPath $script:SandboxManifest -Raw
        $fullTrust = $source.Replace(
            'uap10:TrustLevel="appContainer"',
            'uap10:TrustLevel="mediumIL"')
        {
            $document = Read-ReplicationWindowsManifestXml `
                -Content $fullTrust `
                -Description 'full trust manifest'
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'full trust manifest'
        } | Should -Throw (
            "*Actual TrustLevel='mediumIL', " +
            "RuntimeBehavior='packagedClassicApp'; Application attributes:*" +
            'TrustLevel{http://schemas.microsoft.com/appx/manifest/uap/windows10/10}=mediumIL*')

        $extension = $source.Replace(
            '</Application>',
            '<Extensions><uap:Extension Category="windows.protocol" /></Extensions></Application>')
        {
            $document = Read-ReplicationWindowsManifestXml `
                -Content $extension `
                -Description 'extension manifest'
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'extension manifest'
        } | Should -Throw '*must not declare application or package extensions*'
    }

    It 'quotes packaged-app arguments without introducing a shell' {
        ConvertTo-ReplicationWindowsAppArguments -Arguments @(
            'C:\Path With Spaces\result.xml',
            'Issue37540'
        ) | Should -BeExactly '"C:\Path With Spaces\result.xml" "Issue37540"'

        {
            ConvertTo-ReplicationWindowsAppArguments -Arguments @("value`nnext")
        } | Should -Throw '*invalid value*'
    }

    It 'replaces the manifest item only for the exact top-level project' {
        $content = Get-Content -LiteralPath $script:OverrideTargets -Raw
        $content | Should -Match (
            "MSBuildProjectFullPath.*MauiReplicationAppContainerProject")
        $content | Should -Match '<AppxManifest Remove="@\(AppxManifest\)"'
        $content | Should -Match (
            '<AppxManifest Include="\$\(MauiReplicationAppContainerManifest\)"')
        $content | Should -Match (
            'BeforeTargets="MauiGeneratePackageAppxManifest"')
        $content | Should -Match (
            'DependsOnTargets="MauiGeneratePackageAppxManifest"')
        $content | Should -Match '<XmlPeek'
        $content | Should -Match (
            'generated replication manifest does not preserve appContainer trust')
    }
}
