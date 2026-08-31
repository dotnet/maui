#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot '../shared/Assert-ReplicationAppleAppSandbox.ps1')
    $script:SandboxEntitlements = Join-Path $PSScriptRoot (
        '../../../src/Controls/samples/Controls.Sample.Sandbox/Platforms/MacCatalyst/ReplicationNetworkIsolation.entitlements')
    $script:DeviceEntitlements = Join-Path $PSScriptRoot (
        '../../../src/Controls/tests/DeviceTests/Platforms/MacCatalyst/ReplicationNetworkIsolation.entitlements')
}

Describe 'Mac Catalyst replication App Sandbox entitlements' {
    It 'accepts only app-sandbox source templates' {
        foreach ($path in @($script:SandboxEntitlements, $script:DeviceEntitlements)) {
            $result = Assert-ReplicationMacCatalystEntitlements -Path $path
            $result.AppSandbox | Should -BeTrue
            $result.EntitlementCount | Should -Be 1
        }
    }

    It 'rejects network and temporary-exception entitlements' {
        $source = Get-Content -LiteralPath $script:SandboxEntitlements -Raw
        foreach ($key in @(
            'com.apple.security.network.client',
            'com.apple.security.network.server',
            'com.apple.security.temporary-exception.mach-lookup.global-name',
            'com.apple.security.cs.disable-library-validation'
        )) {
            $mutated = $source.Replace(
                '</dict>',
                "<key>$key</key><true/></dict>")
            {
                $document = Read-ReplicationApplePlist `
                    -Content $mutated `
                    -Description 'mutated entitlements'
                Assert-ReplicationMacCatalystEntitlementDocument `
                    -Document $document `
                    -Description 'mutated entitlements' `
                    -SourceTemplate
            } | Should -Throw '*forbidden entitlement*'
        }
    }

    It 'proves this unsandboxed test process is not an acceptable app boundary' `
            -Skip:(-not [OperatingSystem]::IsMacOS()) {
        $process = Get-Process -Id $PID
        try {
            {
                Assert-ReplicationMacCatalystProcessNetworkDenied `
                    -Process $process `
                    -ExpectedExecutablePath $process.Path
            } | Should -Throw '*permitted outbound networking*'
        } finally {
            $process.Dispose()
        }
    }
}

Describe 'Apple trusted host command boundary' {
    BeforeEach {
        $script:TrustedRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        foreach ($relative in @(
            'scripts/BuildAndRunSandbox.ps1',
            'scripts/shared/Record-Reproduction.ps1',
            'scripts/shared/Invoke-ReplicationTestVerification.ps1',
            'scripts/Other.ps1'
        )) {
            $path = Join-Path $script:TrustedRoot $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force |
                Out-Null
            '# trusted fixture' | Set-Content -LiteralPath $path
        }
        $script:Environment = @{
            PATH = [Environment]::GetEnvironmentVariable('PATH')
        }
    }

    It 'admits the exact Catalyst Sandbox runner with enforcement' {
        $command = Get-ReplicationAppleIsolatedCommand `
            -Platform catalyst `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath (Join-Path $script:TrustedRoot (
                'scripts/BuildAndRunSandbox.ps1')) `
            -Arguments @(
                '-Platform', 'catalyst',
                '-PrepareOnly',
                '-EnforceNetworkIsolation'
            ) `
            -Environment $script:Environment `
            -OperatingSystem macos
        $command.Boundary | Should -BeExactly 'mac-catalyst-app-sandbox'
    }

    It 'rejects duplicate security-critical runner arguments' {
        {
            Get-ReplicationAppleIsolatedCommand `
                -Platform catalyst `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath (Join-Path $script:TrustedRoot (
                    'scripts/BuildAndRunSandbox.ps1')) `
                -Arguments @(
                    '-Platform', 'catalyst',
                    '-Platform', 'catalyst',
                    '-EnforceNetworkIsolation'
                ) `
                -Environment $script:Environment `
                -OperatingSystem macos
        } | Should -Throw "*exactly one value for '-Platform'*"

        {
            Get-ReplicationAppleIsolatedCommand `
                -Platform catalyst `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath (Join-Path $script:TrustedRoot (
                    'scripts/BuildAndRunSandbox.ps1')) `
                -Arguments @(
                    '-Platform', 'catalyst',
                    '-EnforceNetworkIsolation',
                    '-EnforceNetworkIsolation'
                ) `
                -Environment $script:Environment `
                -OperatingSystem macos
        } | Should -Throw '*requires its bounded app boundary*'
    }

    It 'round-trips recording replay arguments without flattening the array' {
        $nestedArguments = @(
            '-Platform', 'catalyst',
            '-Configuration', 'Debug',
            '-EnforceNetworkIsolation'
        )
        $payload = [Convert]::ToBase64String(
            [Text.Encoding]::UTF8.GetBytes(
                (ConvertTo-Json -InputObject $nestedArguments -Compress)))
        $sandboxPath = Join-Path $script:TrustedRoot (
            'scripts/BuildAndRunSandbox.ps1')

        $command = Get-ReplicationAppleIsolatedCommand `
            -Platform catalyst `
            -TrustedRoot $script:TrustedRoot `
            -ScriptPath (Join-Path $script:TrustedRoot (
                'scripts/shared/Record-Reproduction.ps1')) `
            -Arguments @(
                '-Platform', 'catalyst',
                '-ReproductionScriptPath', $sandboxPath,
                '-ReproductionArgumentsPayload', $payload
            ) `
            -Environment $script:Environment `
            -OperatingSystem macos

        $command.Boundary | Should -BeExactly 'mac-catalyst-app-sandbox'
    }

    It 'rejects recording replay arguments that drop App Sandbox enforcement' {
        $nestedArguments = @(
            '-Platform', 'catalyst',
            '-Configuration', 'Debug'
        )
        $payload = [Convert]::ToBase64String(
            [Text.Encoding]::UTF8.GetBytes(
                (ConvertTo-Json -InputObject $nestedArguments -Compress)))
        {
            Get-ReplicationAppleIsolatedCommand `
                -Platform catalyst `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath (Join-Path $script:TrustedRoot (
                    'scripts/shared/Record-Reproduction.ps1')) `
                -Arguments @(
                    '-Platform', 'catalyst',
                    '-ReproductionScriptPath', (Join-Path $script:TrustedRoot (
                        'scripts/BuildAndRunSandbox.ps1')),
                    '-ReproductionArgumentsPayload', $payload
                ) `
                -Environment $script:Environment `
                -OperatingSystem macos
        } | Should -Throw '*dropped its bounded app boundary*'
    }

    It 'rejects null host command arguments' {
        {
            Get-ReplicationAppleIsolatedCommand `
                -Platform catalyst `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath (Join-Path $script:TrustedRoot (
                    'scripts/BuildAndRunSandbox.ps1')) `
                -Arguments @(
                    '-Platform', 'catalyst',
                    $null,
                    '-EnforceNetworkIsolation'
                ) `
                -Environment $script:Environment `
                -OperatingSystem macos
        } | Should -Throw '*null*'
    }

    It 'withholds iOS without the independently supplied hypervisor attestation' {
        $prior = [Environment]::GetEnvironmentVariable(
            'MAUI_REPLICATION_APPLE_HYPERVISOR_EGRESS_DENIED')
        try {
            [Environment]::SetEnvironmentVariable(
                'MAUI_REPLICATION_APPLE_HYPERVISOR_EGRESS_DENIED',
                $null)
            {
                Get-ReplicationAppleIsolatedCommand `
                    -Platform ios `
                    -TrustedRoot $script:TrustedRoot `
                    -ScriptPath (Join-Path $script:TrustedRoot (
                        'scripts/BuildAndRunSandbox.ps1')) `
                    -Arguments @(
                        '-Platform', 'ios',
                        '-PrepareOnly',
                        '-EnforceNetworkIsolation'
                    ) `
                    -Environment $script:Environment `
                    -OperatingSystem macos
            } | Should -Throw '*requires an Aces host/hypervisor egress boundary*'
        } finally {
            [Environment]::SetEnvironmentVariable(
                'MAUI_REPLICATION_APPLE_HYPERVISOR_EGRESS_DENIED',
                $prior)
        }
    }

    Describe 'Mac Catalyst trusted file result channel' {
        It 'runs the signed app directly with no inherited environment or TCP listener' {
            $content = Get-Content -LiteralPath (Join-Path $PSScriptRoot (
                '../shared/Assert-ReplicationAppleAppSandbox.ps1')) -Raw
            $content | Should -Match 'function Invoke-ReplicationMacCatalystDeviceTests'
            $content | Should -Match '\$startInfo\.Environment\.Clear\(\)'
            $content | Should -Match "\['NUNIT_ENABLE_NETWORK'\]\s*=\s*'false'"
            $content | Should -Match "\['NUNIT_ENABLE_XML_OUTPUT'\]\s*=\s*'false'"
            $content | Should -Match 'Documents/\.config/TestResults\.xUnit\.xml'
            $content | Should -Match 'Assert-ReplicationMacCatalystProcessNetworkDenied'
            $networkGuard = [regex]::Match(
                $content,
                '(?ms)^function Assert-ReplicationMacCatalystProcessNetworkDenied\b.*?^}').Value
            $networkGuard | Should -Match (
                '(?s)for \(\$attempt = 1;.*?\$Process\.Refresh\(\).*?' +
                '\$Process\.HasExited.*?sandbox_check')
            $networkGuard | Should -Match (
                '(?s)if \(\$networkCheck -gt 0\).*?' +
                '\$Process\.Refresh\(\).*?\$Process\.HasExited')
            $content | Should -Not -Match (
                "NUNIT_ENABLE_NETWORK'\]\s*=\s*'true'")
            $channel = [regex]::Match(
                $content,
                '(?ms)^function Invoke-ReplicationMacCatalystDeviceTests\b.*?^}').Value
            $channel | Should -Match (
                '(?s)Prior Mac Catalyst device-test result is not a regular file.*?' +
                'Remove-Item -LiteralPath \$resultPath')
            $channel | Should -Not -Match (
                "(?s)Assert-ReplicationAppleRegularResultFile.*?" +
                "Description 'Prior Mac Catalyst")
            $channel | Should -Match '\$runStartedUtc = \[DateTime\]::UtcNow'
            $channel | Should -Match (
                '\$result\.LastWriteTimeUtc -lt \$runStartedUtc\.AddSeconds\(-2\)')
            $channel | Should -Match (
                'writing a valid fresh result; using the verified result file')
        }

        It 'rejects linked trusted roots before constructing a host command' `
                -Skip:(-not [OperatingSystem]::IsMacOS()) {
            $realRoot = Join-Path $TestDrive 'real-root'
            $linkedRoot = Join-Path $TestDrive 'linked-root'
            $scriptPath = Join-Path $realRoot 'scripts/BuildAndRunSandbox.ps1'
            New-Item -ItemType Directory -Path (Split-Path -Parent $scriptPath) `
                -Force | Out-Null
            '# fixture' | Set-Content -LiteralPath $scriptPath
            New-Item -ItemType SymbolicLink -Path $linkedRoot -Target $realRoot |
                Out-Null
            {
                Get-ReplicationAppleIsolatedCommand `
                    -Platform catalyst `
                    -TrustedRoot $linkedRoot `
                    -ScriptPath (Join-Path $linkedRoot (
                        'scripts/BuildAndRunSandbox.ps1')) `
                    -Arguments @(
                        '-Platform', 'catalyst',
                        '-EnforceNetworkIsolation'
                    ) `
                    -Environment @{} `
                    -OperatingSystem macos
            } | Should -Throw '*trusted root must be a regular directory*'
        }

        It 'accepts only bounded regular result files' {
            $path = Join-Path $TestDrive 'result.xml'
            '<assemblies />' | Set-Content -LiteralPath $path
            (Assert-ReplicationAppleRegularResultFile `
                -Path $path `
                -Description 'test result').Length | Should -BeGreaterThan 0

            [IO.File]::WriteAllBytes($path, [byte[]]@())
            {
                Assert-ReplicationAppleRegularResultFile `
                    -Path $path `
                    -Description 'test result'
            } | Should -Throw '*bounded regular file*'
        }
    }

    It 'rejects an unlisted script or non-device verifier' {
        {
            Get-ReplicationAppleIsolatedCommand `
                -Platform catalyst `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath (Join-Path $script:TrustedRoot 'scripts/Other.ps1') `
                -Arguments @('-Platform', 'catalyst') `
                -Environment $script:Environment `
                -OperatingSystem macos
        } | Should -Throw '*limited to exact trusted runners*'

        {
            Get-ReplicationAppleIsolatedCommand `
                -Platform catalyst `
                -TrustedRoot $script:TrustedRoot `
                -ScriptPath (Join-Path $script:TrustedRoot (
                    'scripts/shared/Invoke-ReplicationTestVerification.ps1')) `
                -Arguments @(
                    '-Platform', 'catalyst',
                    '-TestType', 'UnitTest'
                ) `
                -Environment $script:Environment `
                -OperatingSystem macos
        } | Should -Throw '*only sandboxed device tests*'
    }
}
