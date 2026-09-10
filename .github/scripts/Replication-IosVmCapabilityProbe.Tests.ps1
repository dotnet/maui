#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:Pipeline = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml') -Raw
    $script:Helper = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot 'Invoke-IosVmCapabilityProbe.ps1') -Raw
    $script:Fixture = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot 'fixtures/IosVmCapabilityProbe.c') -Raw
    $script:Entitlements = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot 'fixtures/IosVmCapabilityProbe.entitlements') -Raw
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseInput(
        $script:Helper, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors.Count -ne 0) {
        throw ($parseErrors.Message -join '; ')
    }
    $definition = $ast.Find({
        param($node)
        $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $node.Name -ceq 'Invoke-FixedNativeCommand'
    }, $false)
    if ($null -eq $definition) {
        throw 'The fixed native command helper is missing.'
    }
    . ([scriptblock]::Create($definition.Extent.Text))
}

Describe 'Trusted iOS VM capability probe' {
    It 'routes only the bounded physical-host diagnostic mode' {
        $script:Pipeline | Should -Match '(?m)^\s+- ios-vm-capability-probe\s*$'
        $script:Pipeline | Should -Match (
            "(?s)- name: iosVmCapabilityPool.*?name: AcesShared.*?" +
            'ImageOverride -equals ACES_arm64_Sequoia_Xcode')
        $stage = [regex]::Match(
            $script:Pipeline,
            '(?ms)^  - stage: ProbeIosVmCapability\r?\n.*?(?=^  - stage:|\z)').Value
        $stage | Should -Not -BeNullOrEmpty
        $stage | Should -Match "eq\('\$\{\{ parameters\.Mode \}\}', 'ios-vm-capability-probe'\)"
        $stage | Should -Match 'pool: \$\{\{ parameters\.iosVmCapabilityPool \}\}'
        $stage | Should -Match 'timeoutInMinutes: 8'
        $stage | Should -Match 'persistCredentials: false'
        $stage | Should -Match 'ios-vm-capability-probe requires Platform=ios and both target numbers=0'
        $stage | Should -Match 'restricted to Azure definition 27723'
        $stage | Should -Match 'refs/heads/copilot/replicate-issues-pipeline'
        $stage | Should -Match '\$head -cne \$env:BUILD_SOURCEVERSION'
        $stage | Should -Match 'Invoke-IosVmCapabilityProbe\.ps1'
        $stage | Should -Match 'artifact: ''IosVmCapabilityProbe'''
        foreach ($scopeField in @(
            'certifiesIssue = $false',
            'enforcesEgress = $false',
            'allowsGeneratedExecution = $false')) {
            $stage | Should -Match ([regex]::Escape($scopeField))
        }
        $stage | Should -Not -Match (
            'persistCredentials: true|GH_TOKEN|COPILOT_GITHUB_TOKEN|SYSTEM_ACCESSTOKEN|' +
            'Replicate-Issue\.ps1|Run-DeviceTests\.ps1|Publish-ReplicationPR\.ps1|' +
            'template: common/provision\.yml|Install-Module|SetEnvironmentVariable|' +
            'MAUI_REPLICATION_APPLE_HYPERVISOR_EGRESS_DENIED')
    }

    It 'uses an empty Hypervisor VM fixture without guest or network devices' {
        [regex]::Matches($script:Fixture, '\bhv_vm_create\s*\(').Count | Should -Be 1
        [regex]::Matches($script:Fixture, '\bhv_vm_destroy\s*\(').Count | Should -Be 1
        $script:Fixture | Should -Match '#include <Hypervisor/Hypervisor\.h>'
        $script:Fixture | Should -Match 'hv_vm_create\(NULL\)'
        $script:Fixture | Should -Not -Match 'HV_VM_DEFAULT'
        $script:Fixture | Should -Match 'destroy=not-attempted'
        $script:Fixture | Should -Not -Match (
            '\bhv_vcpu_|Virtualization|VZVirtualMachine|network|socket|disk|' +
            '\bfork\s*\(|\bexec|system\s*\(')
        $script:Entitlements | Should -Match '<key>com\.apple\.security\.hypervisor</key>'
        [regex]::Matches($script:Entitlements, '<true/>').Count | Should -Be 1
        $script:Helper | Should -Match "'-framework', 'Hypervisor'"
        $script:Helper | Should -Match "'clang', '-arch', 'arm64'"
        $script:Helper | Should -Match (
            [regex]::Escape('''--entitlements'', $entitlementsPath'))
        $script:Helper | Should -Match (
            [regex]::Escape('''--verify'', ''--strict'', $binaryPath'))
        $script:Helper | Should -Match (
            [regex]::Escape('''hash-object'', "--path=$relativePath"'))
        $script:Helper | Should -Match 'does not match its immutable Git blob'
        $script:Helper | Should -Not -Match (
            'curl|wget|Invoke-WebRequest|Install-Module|\btart\b|keychain|' +
            'MAUI_REPLICATION_APPLE_HYPERVISOR_EGRESS_DENIED')
    }

    It 'separates unavailable hardware from probe infrastructure failures' {
        foreach ($field in @(
            'certifiesIssue = $false',
            'enforcesEgress = $false',
            'allowsGeneratedExecution = $false',
            "name = 'kern.hv_support'",
            "name = 'kern.hv_vmm_present'",
            'productVersionExitCode = $null',
            'buildVersionExitCode = $null',
            'processExitCode = $null',
            'createReturnCode = $null',
            'destroyReturnCode = $null')) {
            $script:Helper | Should -Match ([regex]::Escape($field))
        }
        foreach ($outcome in @('unavailable', 'available', 'infrastructure-error')) {
            $script:Helper | Should -Match (
                [regex]::Escape("outcome = '$outcome'"))
        }
        foreach ($reason in @(
            'hypervisor-not-supported',
            'hypervisor-vm-create-failed',
            'host-is-virtual-machine',
            'physical-arm64-hypervisor-available',
            'trusted-fixture-compilation-failed',
            'hypervisor-vm-destroy-failed')) {
            $script:Helper | Should -Match ([regex]::Escape($reason))
        }
        $script:Helper | Should -Match "'/usr/bin/sw_vers'"
        $script:Helper | Should -Match "'-productVersion'"
        $script:Helper | Should -Match "'-buildVersion'"
        $compileIndex = $script:Helper.IndexOf('$result.compile.attempted = $true')
        $unavailableIndex = $script:Helper.IndexOf(
            '$result.reasonCode = ''hypervisor-not-supported''')
        $compileIndex | Should -BeGreaterThan -1
        $compileIndex | Should -BeLessThan $unavailableIndex
        $script:Helper | Should -Match 'ConvertTo-Json -Depth 8'
        $script:Helper | Should -Match ([regex]::Escape('throw $probeError'))
        $script:Helper | Should -Match 'failureMessage = \$null'
    }

    It 'accepts an empty argument list for the fixed native fixture' {
        $script:LASTEXITCODE = 0
        function Invoke-TestVmFixture { 'create=0'; 'destroy=0' }
        $result = Invoke-FixedNativeCommand -FilePath 'Invoke-TestVmFixture' -ArgumentList @()
        $result.ExitCode | Should -Be 0
        $result.Output.Count | Should -Be 2
        $result.Output[0] | Should -Be 'create=0'
        $result.Output[1] | Should -Be 'destroy=0'
    }

    It 'retains bounded diagnostics and a failing command exit code' {
        $script:LASTEXITCODE = 20
        function Invoke-TestVmFailure { 'x' * 5000 }
        $result = Invoke-FixedNativeCommand -FilePath 'Invoke-TestVmFailure' -ArgumentList @()
        $script:LASTEXITCODE = 0
        $result.ExitCode | Should -Be 20
        $result.Diagnostic.Length | Should -Be 4096
        $result.Output[0].Length | Should -Be 5000
    }
}
