#Requires -Modules Pester

Describe 'Device-test Helix SDK selection' {
    BeforeAll {
        $projectPath = Join-Path $PSScriptRoot 'helix_xharness.proj'

        function Get-HelixSdkProperties {
            param([string[]] $Properties)

            $output = & dotnet msbuild $projectPath -nologo `
                -getProperty:DotNetCliVersion,MicrosoftNETSdkPackageVersion,DotNetCliRuntime,IncludeXHarnessCli `
                -p:NETCoreSdkVersion=10.0.113 @Properties
            if ($LASTEXITCODE -ne 0) {
                throw "Helix project evaluation failed with exit code ${LASTEXITCODE}: $output"
            }

            ($output -join "`n" | ConvertFrom-Json).Properties
        }
    }

    It 'uses the pinned Windows SDK package version, not the installed SDK version' {
        $properties = Get-HelixSdkProperties @(
            '-p:TargetOS=windows'
            '-p:HelixTargetQueue=windows.11.amd64.client.open'
        )

        $properties.MicrosoftNETSdkPackageVersion | Should -Not -BeNullOrEmpty
        $properties.DotNetCliVersion | Should -Be $properties.MicrosoftNETSdkPackageVersion
        $properties.DotNetCliRuntime | Should -Be 'win-x64'
        $properties.IncludeXHarnessCli | Should -Be 'false'
    }

    It 'preserves XHarness SDK selection for <TargetOS>' -TestCases @(
        @{ TargetOS = 'ios'; Queue = 'osx.15.arm64.maui.open'; Runtime = 'osx-arm64' }
        @{ TargetOS = 'maccatalyst'; Queue = 'osx.15.arm64.maui.open'; Runtime = 'osx-arm64' }
        @{ TargetOS = 'android'; Queue = 'ubuntu.2204.amd64.android.33.open'; Runtime = 'linux-x64' }
    ) {
        param($TargetOS, $Queue, $Runtime)

        $properties = Get-HelixSdkProperties @(
            "-p:TargetOS=$TargetOS"
            "-p:HelixTargetQueue=$Queue"
            '-p:XHarnessTargetFramework=net8.0'
        )

        $properties.DotNetCliVersion | Should -Be '8.0.100'
        $properties.DotNetCliRuntime | Should -Be $Runtime
        $properties.IncludeXHarnessCli | Should -Be 'true'
    }

    It 'follows SDK dependency updates to <Version>' -TestCases @(
        @{ Version = '10.0.113-servicing.26454.107' }
        @{ Version = '11.0.100-preview.6.26359.118' }
        @{ Version = '10.0.108' }
    ) {
        param($Version)

        $properties = Get-HelixSdkProperties @(
            '-p:TargetOS=windows'
            "-p:MicrosoftNETSdkPackageVersion=$Version"
        )

        $properties.DotNetCliVersion | Should -Be $Version
    }

    It 'preserves an explicit command-line SDK version override' {
        $properties = Get-HelixSdkProperties @(
            '-p:TargetOS=windows'
            '-p:DotNetCliVersion=10.0.108'
        )

        $properties.DotNetCliVersion | Should -Be '10.0.108'
    }

    It 'preserves an SDK version supplied by the pipeline environment' {
        $originalVersion = $env:DotNetCliVersion
        try {
            $env:DotNetCliVersion = '10.0.108'
            $properties = Get-HelixSdkProperties @('-p:TargetOS=windows')

            $properties.DotNetCliVersion | Should -Be '10.0.108'
        }
        finally {
            $env:DotNetCliVersion = $originalVersion
        }
    }
}
