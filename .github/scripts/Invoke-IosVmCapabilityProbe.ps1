#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-f]{40}$')]
    [string] $ExpectedSourceVersion,

    [Parameter(Mandatory)]
    [string] $OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-FixedNativeCommand {
    param(
        [Parameter(Mandatory)]
        [string] $FilePath,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]] $ArgumentList
    )

    $PSNativeCommandUseErrorActionPreference = $false
    $output = @(& $FilePath @ArgumentList 2>&1 | ForEach-Object { "$_" })
    $exitCode = $LASTEXITCODE
    $diagnostic = $output -join "`n"
    [pscustomobject]@{
        ExitCode = $exitCode
        Output = $output
        Diagnostic = $diagnostic.Substring(0, [Math]::Min(4096, $diagnostic.Length))
    }
}

$pipelineWorkspace = [Environment]::GetEnvironmentVariable('PIPELINE_WORKSPACE')
if ([string]::IsNullOrWhiteSpace($pipelineWorkspace)) {
    throw 'PIPELINE_WORKSPACE is required for the bounded capability output.'
}
$expectedOutputDirectory = [IO.Path]::GetFullPath(
    (Join-Path $pipelineWorkspace 'IosVmCapabilityProbe'))
$actualOutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if ($actualOutputDirectory -cne $expectedOutputDirectory) {
    throw 'The iOS VM capability output must use the fixed pipeline workspace directory.'
}

New-Item -ItemType Directory -Path $actualOutputDirectory -Force | Out-Null
$resultPath = Join-Path $actualOutputDirectory 'result.json'
if (Test-Path -LiteralPath $resultPath) {
    throw 'The iOS VM capability probe refuses to overwrite an existing result.'
}
$workDirectory = Join-Path $actualOutputDirectory 'work'
if (Test-Path -LiteralPath $workDirectory) {
    throw 'The iOS VM capability probe refuses a pre-existing work directory.'
}
New-Item -ItemType Directory -Path $workDirectory | Out-Null

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$fixturePath = Join-Path $PSScriptRoot 'fixtures/IosVmCapabilityProbe.c'
$entitlementsPath = Join-Path $PSScriptRoot 'fixtures/IosVmCapabilityProbe.entitlements'
$binaryPath = Join-Path $workDirectory 'ios-vm-capability-probe'
$trackedProbePaths = @(
    '.github/scripts/Invoke-IosVmCapabilityProbe.ps1',
    '.github/scripts/fixtures/IosVmCapabilityProbe.c',
    '.github/scripts/fixtures/IosVmCapabilityProbe.entitlements'
)
$trackedProbeFiles = @{
    '.github/scripts/Invoke-IosVmCapabilityProbe.ps1' = $PSCommandPath
    '.github/scripts/fixtures/IosVmCapabilityProbe.c' = $fixturePath
    '.github/scripts/fixtures/IosVmCapabilityProbe.entitlements' = $entitlementsPath
}

$result = [ordered]@{
    schemaVersion = 1
    mode = 'ios-vm-capability-probe'
    pipelineCommit = $ExpectedSourceVersion
    certifiesIssue = $false
    enforcesEgress = $false
    allowsGeneratedExecution = $false
    os = [ordered]@{
        platform = if ([OperatingSystem]::IsMacOS()) { 'macos' } else { 'other' }
        description = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        osArchitecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
        processArchitecture = [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant()
        productVersionExitCode = $null
        productVersion = $null
        buildVersionExitCode = $null
        buildVersion = $null
    }
    trustedSource = [ordered]@{
        helperSha256 = $null
        fixtureSha256 = $null
        entitlementsSha256 = $null
    }
    sysctl = [ordered]@{
        hypervisorSupport = [ordered]@{
            name = 'kern.hv_support'
            exitCode = $null
            value = $null
        }
        virtualMachinePresent = [ordered]@{
            name = 'kern.hv_vmm_present'
            exitCode = $null
            value = $null
        }
    }
    compile = [ordered]@{ attempted = $false; exitCode = $null }
    sign = [ordered]@{ attempted = $false; exitCode = $null }
    verifySignature = [ordered]@{ attempted = $false; exitCode = $null }
    fixture = [ordered]@{
        attempted = $false
        processExitCode = $null
        createReturnCode = $null
        destroyAttempted = $false
        destroyReturnCode = $null
    }
    outcome = 'infrastructure-error'
    reasonCode = 'probe-not-completed'
    failureMessage = $null
    cleanupFailed = $false
}

$probeError = $null
$infrastructureReason = 'probe-execution-error'

try {
    :ProbeSteps do {
        if ([Environment]::GetEnvironmentVariable('SYSTEM_DEFINITIONID') -cne '27723' -or
            [Environment]::GetEnvironmentVariable('BUILD_SOURCEBRANCH') -cne
                'refs/heads/copilot/replicate-issues-pipeline') {
            $infrastructureReason = 'trusted-scope-validation-failed'
            throw 'The probe is outside its fixed trusted Azure definition or source branch.'
        }

        $head = Invoke-FixedNativeCommand -FilePath '/usr/bin/git' -ArgumentList @(
            '-C', $repositoryRoot, 'rev-parse', 'HEAD')
        if ($head.ExitCode -ne 0 -or @($head.Output).Count -ne 1 -or
            $head.Output[0] -cne $ExpectedSourceVersion) {
            $infrastructureReason = 'trusted-source-validation-failed'
            throw 'The probe checkout does not match the expected source version.'
        }
        $diff = Invoke-FixedNativeCommand -FilePath '/usr/bin/git' -ArgumentList (
            @('-C', $repositoryRoot, 'diff', '--quiet', $ExpectedSourceVersion, '--') +
                $trackedProbePaths)
        if ($diff.ExitCode -ne 0) {
            $infrastructureReason = 'trusted-source-validation-failed'
            throw 'The checked-in probe source differs from the expected source version.'
        }
        foreach ($relativePath in $trackedProbePaths) {
            $path = $trackedProbeFiles[$relativePath]
            if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
                $infrastructureReason = 'trusted-source-validation-failed'
                throw 'A fixed checked-in probe input is missing.'
            }
            $expectedBlob = Invoke-FixedNativeCommand -FilePath '/usr/bin/git' -ArgumentList @(
                '-C', $repositoryRoot, 'rev-parse',
                "$ExpectedSourceVersion`:$relativePath")
            $actualBlob = Invoke-FixedNativeCommand -FilePath '/usr/bin/git' -ArgumentList @(
                '-C', $repositoryRoot, 'hash-object', "--path=$relativePath", '--', $path)
            if ($expectedBlob.ExitCode -ne 0 -or $actualBlob.ExitCode -ne 0 -or
                @($expectedBlob.Output).Count -ne 1 -or @($actualBlob.Output).Count -ne 1 -or
                $expectedBlob.Output[0] -cnotmatch '^[0-9a-f]{40}$' -or
                $actualBlob.Output[0] -cne $expectedBlob.Output[0]) {
                $infrastructureReason = 'trusted-source-validation-failed'
                throw 'A fixed probe input does not match its immutable Git blob.'
            }
        }
        $result.trustedSource.helperSha256 = (
            Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $result.trustedSource.fixtureSha256 = (
            Get-FileHash -LiteralPath $fixturePath -Algorithm SHA256).Hash.ToLowerInvariant()
        $result.trustedSource.entitlementsSha256 = (
            Get-FileHash -LiteralPath $entitlementsPath -Algorithm SHA256).Hash.ToLowerInvariant()

        if (-not [OperatingSystem]::IsMacOS()) {
            $result.outcome = 'unavailable'
            $result.reasonCode = 'host-is-not-macos'
            break ProbeSteps
        }
        if ($result.os.osArchitecture -cne 'arm64') {
            $result.outcome = 'unavailable'
            $result.reasonCode = 'host-is-not-arm64'
            break ProbeSteps
        }
        foreach ($requiredTool in @(
            '/usr/bin/sw_vers',
            '/usr/sbin/sysctl',
            '/usr/bin/xcrun',
            '/usr/bin/codesign')) {
            if (-not (Test-Path -LiteralPath $requiredTool -PathType Leaf)) {
                $infrastructureReason = 'required-apple-tool-missing'
                throw 'A fixed Apple platform tool required by the probe is missing.'
            }
        }

        $productVersion = Invoke-FixedNativeCommand -FilePath '/usr/bin/sw_vers' -ArgumentList @(
            '-productVersion')
        $result.os.productVersionExitCode = $productVersion.ExitCode
        $productVersionValue = (@($productVersion.Output) -join '').Trim()
        if ($productVersion.ExitCode -ne 0 -or
            $productVersionValue -notmatch '^\d+(?:\.\d+){0,2}$') {
            $infrastructureReason = 'operating-system-version-query-failed'
            throw 'The fixed macOS product-version query failed.'
        }
        $result.os.productVersion = $productVersionValue

        $buildVersion = Invoke-FixedNativeCommand -FilePath '/usr/bin/sw_vers' -ArgumentList @(
            '-buildVersion')
        $result.os.buildVersionExitCode = $buildVersion.ExitCode
        $buildVersionValue = (@($buildVersion.Output) -join '').Trim()
        if ($buildVersion.ExitCode -ne 0 -or
            $buildVersionValue -notmatch '^[0-9A-Za-z.]{1,32}$') {
            $infrastructureReason = 'operating-system-build-query-failed'
            throw 'The fixed macOS build-version query failed.'
        }
        $result.os.buildVersion = $buildVersionValue

        $hvSupport = Invoke-FixedNativeCommand -FilePath '/usr/sbin/sysctl' -ArgumentList @(
            '-n', 'kern.hv_support')
        $result.sysctl.hypervisorSupport.exitCode = $hvSupport.ExitCode
        $hvSupportValue = (@($hvSupport.Output) -join '').Trim()
        if ($hvSupport.ExitCode -ne 0 -or $hvSupportValue -notmatch '^[01]$') {
            $infrastructureReason = 'hypervisor-support-query-failed'
            throw 'The fixed Hypervisor support sysctl query failed.'
        }
        $result.sysctl.hypervisorSupport.value = [int]$hvSupportValue

        $vmPresent = Invoke-FixedNativeCommand -FilePath '/usr/sbin/sysctl' -ArgumentList @(
            '-n', 'kern.hv_vmm_present')
        $result.sysctl.virtualMachinePresent.exitCode = $vmPresent.ExitCode
        $vmPresentValue = (@($vmPresent.Output) -join '').Trim()
        if ($vmPresent.ExitCode -ne 0 -or $vmPresentValue -notmatch '^[01]$') {
            $infrastructureReason = 'virtual-machine-presence-query-failed'
            throw 'The fixed virtual-machine-presence sysctl query failed.'
        }
        $result.sysctl.virtualMachinePresent.value = [int]$vmPresentValue

        $result.compile.attempted = $true
        $compile = Invoke-FixedNativeCommand -FilePath '/usr/bin/xcrun' -ArgumentList @(
            '--sdk', 'macosx', 'clang', '-arch', 'arm64', '-std=c11', '-Wall',
            '-Wextra', '-Werror', $fixturePath, '-framework', 'Hypervisor', '-o',
            $binaryPath)
        $result.compile.exitCode = $compile.ExitCode
        if ($compile.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $binaryPath -PathType Leaf)) {
            $infrastructureReason = 'trusted-fixture-compilation-failed'
            throw "The fixed Hypervisor fixture did not compile: $($compile.Diagnostic)"
        }

        $result.sign.attempted = $true
        $sign = Invoke-FixedNativeCommand -FilePath '/usr/bin/codesign' -ArgumentList @(
            '--force', '--sign', '-', '--entitlements', $entitlementsPath, $binaryPath)
        $result.sign.exitCode = $sign.ExitCode
        if ($sign.ExitCode -ne 0) {
            $infrastructureReason = 'trusted-fixture-signing-failed'
            throw "The fixed Hypervisor fixture could not be ad-hoc signed: $($sign.Diagnostic)"
        }

        $result.verifySignature.attempted = $true
        $verify = Invoke-FixedNativeCommand -FilePath '/usr/bin/codesign' -ArgumentList @(
            '--verify', '--strict', $binaryPath)
        $result.verifySignature.exitCode = $verify.ExitCode
        if ($verify.ExitCode -ne 0) {
            $infrastructureReason = 'trusted-fixture-signature-verification-failed'
            throw "The fixed Hypervisor fixture signature did not verify: $($verify.Diagnostic)"
        }

        $result.fixture.attempted = $true
        $fixture = Invoke-FixedNativeCommand -FilePath $binaryPath -ArgumentList @()
        $result.fixture.processExitCode = $fixture.ExitCode
        $fixtureOutput = @($fixture.Output) -join "`n"
        $createMatch = [regex]::Match($fixtureOutput, '(?m)^create=(-?\d+)$')
        $destroyMatch = [regex]::Match($fixtureOutput, '(?m)^destroy=(-?\d+|not-attempted)$')
        if (-not $createMatch.Success -or -not $destroyMatch.Success) {
            $infrastructureReason = 'trusted-fixture-output-invalid'
            throw 'The fixed Hypervisor fixture returned malformed output.'
        }
        $result.fixture.createReturnCode = [int]$createMatch.Groups[1].Value
        if ($destroyMatch.Groups[1].Value -cne 'not-attempted') {
            $result.fixture.destroyAttempted = $true
            $result.fixture.destroyReturnCode = [int]$destroyMatch.Groups[1].Value
        }

        if ($result.fixture.createReturnCode -ne 0) {
            if ($fixture.ExitCode -ne 20 -or $result.fixture.destroyAttempted) {
                $infrastructureReason = 'trusted-fixture-exit-invalid'
                throw 'The fixed Hypervisor fixture returned an inconsistent create failure.'
            }
            if ([int]$hvSupportValue -eq 0) {
                $result.outcome = 'unavailable'
                $result.reasonCode = 'hypervisor-not-supported'
                break ProbeSteps
            }
            if ([int]$vmPresentValue -ne 0) {
                $result.outcome = 'unavailable'
                $result.reasonCode = 'host-is-virtual-machine'
                break ProbeSteps
            }
            $infrastructureReason = 'hypervisor-vm-create-failed'
            throw 'The signed empty Hypervisor VM could not be created.'
        }
        if (-not $result.fixture.destroyAttempted -or
            $result.fixture.destroyReturnCode -ne 0 -or $fixture.ExitCode -ne 0) {
            $infrastructureReason = 'hypervisor-vm-destroy-failed'
            throw 'The empty Hypervisor VM was not cleanly destroyed.'
        }
        if ([int]$hvSupportValue -eq 0) {
            $infrastructureReason = 'hypervisor-result-inconsistent'
            throw 'The empty Hypervisor VM succeeded despite a negative support query.'
        }
        if ([int]$vmPresentValue -ne 0) {
            $result.outcome = 'unavailable'
            $result.reasonCode = 'host-is-virtual-machine'
            break ProbeSteps
        }

        $result.outcome = 'available'
        $result.reasonCode = 'physical-arm64-hypervisor-available'
    } while ($false)
} catch {
    $result.outcome = 'infrastructure-error'
    $result.reasonCode = $infrastructureReason
    $result.failureMessage = $_.Exception.Message.Substring(
        0, [Math]::Min(4096, $_.Exception.Message.Length))
    $probeError = $_
} finally {
    try {
        Remove-Item -LiteralPath $workDirectory -Recurse -Force
    } catch {
        $result.outcome = 'infrastructure-error'
        $result.cleanupFailed = $true
        if ($null -eq $probeError) {
            $result.reasonCode = 'probe-work-cleanup-failed'
            $result.failureMessage = $_.Exception.Message.Substring(
                0, [Math]::Min(4096, $_.Exception.Message.Length))
            $probeError = $_
        }
    }
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $resultPath -Encoding utf8
}

if ($null -ne $probeError) {
    throw $probeError
}
