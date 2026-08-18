#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Build-TemplateApp.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'Assert-EnvironmentValue',
        'Get-NewestBuildOutput',
        'Repair-AppleAdhocSignature',
        'Invoke-DotNetPublish',
        'Test-IsNet11OrLater',
        'Add-NativeAotArguments',
        'Get-BinlogConfiguration',
        'Invoke-NotaryTool',
        'Invoke-NotaryToolJson',
        'Write-NotarySubmissionDiagnostics',
        'Get-NotarizationTimeoutSeconds',
        'Get-NotaryTimestamp',
        'Wait-NotarySubmission',
        'Invoke-MacNotarization',
        'New-MacCatalystDeveloperIdSideload',
        'New-IosAdHocSideload'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }

    $testRoot = Join-Path $PSScriptRoot '.test-results'
    New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
    $projectPath = Join-Path $testRoot 'TestApp.csproj'
    Set-Content -Path $projectPath -Value '<Project Sdk="Microsoft.NET.Sdk" />'
    $projectFile = Get-Item $projectPath

    $installerScriptPath = Join-Path $PSScriptRoot 'Install-AppleSigningAssets.ps1'
    $installerTokens = $null
    $installerParseErrors = $null
    $installerAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $installerScriptPath,
        [ref]$installerTokens,
        [ref]$installerParseErrors
    )
    if ($installerParseErrors -and $installerParseErrors.Count -gt 0) {
        throw ($installerParseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $pairedEnvironmentFunction = $installerAst.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Assert-PairedEnvironmentValues'
    }, $true)
    if (-not $pairedEnvironmentFunction) {
        throw "Function 'Assert-PairedEnvironmentValues' not found"
    }
    Invoke-Expression $pairedEnvironmentFunction.Extent.Text

    $script:prepareMatrixScriptPath = Join-Path $PSScriptRoot 'Prepare-Matrix.ps1'
    $script:resolveDotNetSdkScriptPath = Join-Path $PSScriptRoot 'Resolve-DotNetSdk.ps1'
    $script:resolveSourceRefScriptPath = Join-Path $PSScriptRoot 'Resolve-SourceRef.ps1'
    $script:fastfilePath = Join-Path $PSScriptRoot 'fastlane/Fastfile'
    $script:workflowPath = Join-Path $PSScriptRoot '../../workflows/template-app-distribution.yml'
    $script:workflowText = Get-Content -Path $script:workflowPath -Raw
    $script:pwshPath = (Get-Command pwsh -ErrorAction Stop).Source
    $script:rubyPath = (Get-Command ruby -ErrorAction SilentlyContinue).Source
    $script:originalPath = $env:PATH
    $script:testEnvironmentNames = @(
        'FAKE_DOTNET_MODE',
        'FAKE_CODESIGN_MODE',
        'FAKE_TESTFLIGHT_ERROR',
        'FAKE_TESTFLIGHT_GROUPS',
        'FASTFILE_PATH',
        'GITHUB_OUTPUT',
        'RUNNER_TEMP',
        'TEMPLATE_APP_VARIANTS_JSON',
        'ANDROID_KEYSTORE_PATH',
        'ANDROID_KEYSTORE_PASSWORD',
        'ANDROID_KEY_PASSWORD',
        'ANDROID_KEY_ALIAS',
        'ANDROID_SIGNING_KEY_ALIAS',
        'ANDROID_SIGNING_STORE_PASS',
        'ANDROID_SIGNING_KEY_PASS',
        'FAKE_ANDROID_SIGNING_ENV_LOG',
        'APPLE_DEVELOPERID_CERTIFICATE_BASE64',
        'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64',
        'IOS_ADHOC_CODESIGN_PROVISION',
        'IOS_CODESIGN_KEY',
        'IOS_CODESIGN_PROVISION',
        'APPLE_DEVELOPERID_CODESIGN_KEY',
        'APPLE_DEVELOPERID_CODESIGN_PROVISION',
        'TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS',
        'GH_TOKEN',
        'GITHUB_TOKEN',
        'COPILOT_GITHUB_TOKEN'
    )
    $script:originalEnvironment = @{}
    foreach ($name in $script:testEnvironmentNames) {
        $script:originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
    }

    $script:fakeCommandDirectory = Join-Path $testRoot 'fake-commands'
    New-Item -ItemType Directory -Path $script:fakeCommandDirectory -Force | Out-Null
    $fakeDotNetScriptPath = Join-Path $script:fakeCommandDirectory 'fake-dotnet.ps1'
    @'
$ErrorActionPreference = "Stop"

$outputPath = $null
$runtimeIdentifier = $null
$binlogPaths = @()
$projectPath = $args | Where-Object { $_ -like "*.csproj" } | Select-Object -First 1
for ($index = 0; $index -lt $args.Count; $index++) {
    if ($args[$index] -eq "-o" -and $index + 1 -lt $args.Count) {
        $outputPath = $args[$index + 1]
    }
    if ($args[$index] -eq "-r" -and $index + 1 -lt $args.Count) {
        $runtimeIdentifier = $args[$index + 1]
    }
    if ($args[$index].StartsWith("/bl:")) {
        $binlogPath = $args[$index].Substring(4)
        New-Item -ItemType Directory -Path (Split-Path $binlogPath -Parent) -Force | Out-Null
        $binlogPaths += $binlogPath
    }
}
$argumentText = $args -join "`n"
foreach ($binlogPath in $binlogPaths) {
    Set-Content -Path $binlogPath -Value $argumentText
}
if (-not [string]::IsNullOrWhiteSpace($env:FAKE_ANDROID_SIGNING_ENV_LOG)) {
    @(
        "alias=$env:ANDROID_SIGNING_KEY_ALIAS",
        "storePass=$env:ANDROID_SIGNING_STORE_PASS",
        "keyPass=$env:ANDROID_SIGNING_KEY_PASS"
    ) -join "`n" | Add-Content -Path $env:FAKE_ANDROID_SIGNING_ENV_LOG
}

switch ($env:FAKE_DOTNET_MODE) {
    "android-success" {
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        if ($argumentText -match "AndroidPackageFormat=apk") {
            New-Item -ItemType File -Path (Join-Path $outputPath "TestApp-Signed.apk") -Force | Out-Null
        } elseif ($argumentText -match "AndroidPackageFormat=aab") {
            New-Item -ItemType File -Path (Join-Path $outputPath "TestApp.aab") -Force | Out-Null
        }
    }
    "ios-device-only" {
        if ($runtimeIdentifier -eq "ios-arm64") {
            $projectDirectory = Split-Path $projectPath -Parent
            $appPath = Join-Path $projectDirectory "bin/$runtimeIdentifier/TestApp.app"
            New-Item -ItemType Directory -Path $appPath -Force | Out-Null
            Set-Content -Path (Join-Path $appPath "Info.plist") -Value "fake app"
        }
    }
    "ios-publish-success" {
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        New-Item -ItemType File -Path (Join-Path $outputPath "TestApp.ipa") -Force | Out-Null
    }
    "ios-store-overwrite" {
        $projectDirectory = Split-Path $projectPath -Parent
        $projectIpa = Join-Path $projectDirectory "bin/$runtimeIdentifier/TestApp.ipa"
        New-Item -ItemType Directory -Path (Split-Path $projectIpa -Parent), $outputPath -Force | Out-Null
        if ($argumentText -match "CodesignProvision=Ad Hoc Profile") {
            Set-Content -Path $projectIpa -Value "ad-hoc"
            Set-Content -Path (Join-Path $outputPath "TestApp.ipa") -Value "ad-hoc"
        } else {
            Set-Content -Path $projectIpa -Value "app-store"
        }
    }
    "ios-adhoc-failure" {
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        if ($argumentText -match "CodesignProvision=Ad Hoc Profile") {
            exit 23
        }
        New-Item -ItemType File -Path (Join-Path $outputPath "TestApp.ipa") -Force | Out-Null
    }
}

exit 0
'@ | Set-Content -Path $fakeDotNetScriptPath -Encoding utf8

    if ($IsWindows) {
        @"
@echo off
pwsh -NoLogo -NoProfile -File "$fakeDotNetScriptPath" %*
exit /b %ERRORLEVEL%
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'dotnet.cmd') -Encoding ascii
    } else {
        @"
#!/bin/sh
exec pwsh -NoLogo -NoProfile -File "$fakeDotNetScriptPath" "`$@"
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'dotnet') -Encoding utf8NoBOM
        & chmod +x (Join-Path $script:fakeCommandDirectory 'dotnet')
    }

    $fakeCodesignScriptPath = Join-Path $script:fakeCommandDirectory 'fake-codesign.ps1'
    @'
$target = $args[-1]
if ($env:FAKE_CODESIGN_MODE -eq "nested-failure" -and $target -match "\.(dylib|so)$") {
    exit 17
}
if ($env:FAKE_CODESIGN_MODE -eq "bundle-failure" -and (Test-Path -Path $target -PathType Container)) {
    exit 19
}
exit 0
'@ | Set-Content -Path $fakeCodesignScriptPath -Encoding utf8

    if ($IsWindows) {
        @"
@echo off
pwsh -NoLogo -NoProfile -File "$fakeCodesignScriptPath" %*
exit /b %ERRORLEVEL%
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'codesign.cmd') -Encoding ascii
    } else {
        @"
#!/bin/sh
exec pwsh -NoLogo -NoProfile -File "$fakeCodesignScriptPath" "`$@"
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'codesign') -Encoding utf8NoBOM
        & chmod +x (Join-Path $script:fakeCommandDirectory 'codesign')
    }

    $pathSeparator = [System.IO.Path]::PathSeparator
    $env:PATH = "$($script:fakeCommandDirectory)$pathSeparator$($env:PATH)"

    $script:fastfileHarnessPath = Join-Path $testRoot 'fastfile-harness.rb'
    @'
$lanes = {}

module UI
  def self.user_error!(message)
    raise message
  end

  def self.important(message)
    puts message
  end

  def self.error(message)
    warn message
  end
end

def default_platform(*_args)
end

def platform(*_args)
  yield
end

def desc(*_args)
end

def lane(name, &block)
  $lanes[name] = block
end

def app_store_connect_api_key(**_kwargs)
  {}
end

def upload_to_play_store(**_kwargs)
end

def upload_to_testflight(*_args)
  raise ENV.fetch("FAKE_TESTFLIGHT_ERROR")
end

load ENV.fetch("FASTFILE_PATH")

options = {
  app_identifier: "com.example.test",
  api_key_id: "key",
  issuer_id: "issuer",
  api_private_key_path: "key.p8",
  ipa: "TestApp.ipa",
  groups: ENV.fetch("FAKE_TESTFLIGHT_GROUPS", "")
}

begin
  $lanes.fetch(:template_app_testflight).call(options)
  puts "lane succeeded"
rescue => error
  warn error.message
  exit 42
end
'@ | Set-Content -Path $script:fastfileHarnessPath -Encoding utf8

    function New-BuildTestCase {
        $caseRoot = Join-Path $testRoot ([guid]::NewGuid().ToString("N"))
        $projectRoot = Join-Path $caseRoot 'project'
        $outputRoot = Join-Path $caseRoot 'output'
        $runnerTemp = Join-Path $caseRoot 'runner-temp'
        New-Item -ItemType Directory -Path $projectRoot, $outputRoot, $runnerTemp -Force | Out-Null
        Set-Content -Path (Join-Path $projectRoot 'TestApp.csproj') -Value '<Project Sdk="Microsoft.NET.Sdk" />'

        return [pscustomobject]@{
            Root = $caseRoot
            ProjectRoot = $projectRoot
            OutputRoot = $outputRoot
            RunnerTemp = $runnerTemp
            GitHubOutput = Join-Path $caseRoot 'github-output.txt'
        }
    }

    function Invoke-ExternalPowerShell([string]$FilePath, [string[]]$Arguments) {
        $output = @(& $script:pwshPath -NoLogo -NoProfile -File $FilePath @Arguments 2>&1)
        return [pscustomobject]@{
            ExitCode = $LASTEXITCODE
            Output = ($output | Out-String)
        }
    }

    function Invoke-BuildTemplateApp(
        $TestCase,
        [string]$Platform,
        [string]$TargetFramework,
        [string]$RuntimeIdentifier,
        [switch]$Publish,
        [switch]$CreateBinlog
    ) {
        $arguments = @(
            '-ProjectPath', $TestCase.ProjectRoot,
            '-Platform', $Platform,
            '-TargetFramework', $TargetFramework,
            '-RuntimeIdentifier', $RuntimeIdentifier,
            '-OutputPath', $TestCase.OutputRoot,
            '-AppDisplayVersion', '11.0',
            '-AppBuildNumber', '1'
        )
        if ($Publish) {
            $arguments += '-Publish'
        }
        if ($CreateBinlog) {
            $arguments += '-CreateBinlog'
        }

        return Invoke-ExternalPowerShell $scriptPath $arguments
    }

    function Invoke-PrepareMatrix([string]$Variants, [string]$Platforms) {
        return Invoke-ExternalPowerShell $script:prepareMatrixScriptPath @(
            '-Variants', $Variants,
            '-Platforms', $Platforms,
            '-DotNetTfm', 'net11.0'
        )
    }

    function New-SourceRefTestRepository([switch]$UntrustedHead) {
        $repositoryPath = Join-Path $testRoot ([guid]::NewGuid().ToString("N"))
        New-Item -ItemType Directory -Path $repositoryPath -Force | Out-Null

        & git -C $repositoryPath init --quiet --initial-branch=main
        & git -C $repositoryPath config user.name 'Template App Tests'
        & git -C $repositoryPath config user.email 'template-app-tests@example.invalid'
        & git -C $repositoryPath config commit.gpgsign false
        Set-Content -Path (Join-Path $repositoryPath 'source.txt') -Value 'trusted'
        & git -C $repositoryPath add source.txt
        & git -C $repositoryPath commit --quiet -m 'trusted source'
        $trustedSha = (& git -C $repositoryPath rev-parse HEAD).Trim()
        & git -C $repositoryPath update-ref refs/remotes/origin/main $trustedSha

        if ($UntrustedHead) {
            & git -C $repositoryPath switch --quiet -c feature/untrusted
            Set-Content -Path (Join-Path $repositoryPath 'source.txt') -Value 'untrusted'
            & git -C $repositoryPath commit --quiet -am 'untrusted source'
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create source-ref test repository."
        }

        return $repositoryPath
    }

    function Invoke-ResolveSourceRef(
        [string]$RepositoryPath,
        [string]$SourceRef,
        [bool]$Publish
    ) {
        return Invoke-ExternalPowerShell $script:resolveSourceRefScriptPath @(
            '-RepositoryPath', $RepositoryPath,
            '-SourceRef', $SourceRef,
            '-WorkflowRef', 'refs/heads/main',
            '-DefaultBranch', 'main',
            "-Publish:$($Publish.ToString().ToLowerInvariant())"
        )
    }

    function Invoke-FastfileHarness {
        $output = @(& $script:rubyPath $script:fastfileHarnessPath 2>&1)
        return [pscustomobject]@{
            ExitCode = $LASTEXITCODE
            Output = ($output | Out-String)
        }
    }

    function Reset-BuildTestEnvironment {
        foreach ($name in $script:testEnvironmentNames) {
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        $env:FASTFILE_PATH = $script:fastfilePath
    }
}

AfterAll {
    $env:PATH = $script:originalPath
    foreach ($name in $script:testEnvironmentNames) {
        [Environment]::SetEnvironmentVariable($name, $script:originalEnvironment[$name])
    }
    Remove-Item -Path $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'dotnet SDK resolution' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'emits a complete SDK version and derived target framework from global.json' {
        $case = New-BuildTestCase
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        @{
            tools = @{
                dotnet = '11.0.100-preview.1.25120.13'
            }
        } | ConvertTo-Json -Depth 3 | Set-Content -Path (Join-Path $case.Root 'global.json')

        $result = Invoke-ExternalPowerShell $script:resolveDotNetSdkScriptPath @(
            '-RepositoryPath', $case.Root,
            '-DotNetSdk', 'global-json'
        )

        $result.ExitCode | Should -Be 0 -Because $result.Output
        Get-Content -Path $case.GitHubOutput | Should -Contain 'dotnet_sdk=11.0.100-preview.1.25120.13'
        Get-Content -Path $case.GitHubOutput | Should -Contain 'dotnet_tfm=net11.0'
    }

    It 'rejects malformed source-derived SDK version <SdkVersion>' -ForEach @(
        @{ SdkVersion = '11.0' }
        @{ SdkVersion = '11.0.100"; Write-Host compromised; "' }
        @{ SdkVersion = "11.0.100`nforged_output=true" }
    ) {
        $case = New-BuildTestCase
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        @{
            sdk = @{
                version = $SdkVersion
            }
        } | ConvertTo-Json -Depth 3 | Set-Content -Path (Join-Path $case.Root 'global.json')

        $result = Invoke-ExternalPowerShell $script:resolveDotNetSdkScriptPath @(
            '-RepositoryPath', $case.Root,
            '-DotNetSdk', 'global-json'
        )

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'complete SDK'
        Test-Path $case.GitHubOutput | Should -BeFalse
    }
}

Describe 'source ref trust resolution' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'allows a trusted branch at the protected workflow ref' {
        $repositoryPath = New-SourceRefTestRepository
        $env:GITHUB_OUTPUT = Join-Path $repositoryPath 'github-output.txt'

        $result = Invoke-ResolveSourceRef `
            -RepositoryPath $repositoryPath `
            -SourceRef 'main' `
            -Publish $true

        $result.ExitCode | Should -Be 0 -Because $result.Output
        Get-Content -Path $env:GITHUB_OUTPUT | Should -Contain 'trusted=true'
    }

    It 'rejects an untrusted branch for protected publishing' {
        $repositoryPath = New-SourceRefTestRepository -UntrustedHead
        $env:GITHUB_OUTPUT = Join-Path $repositoryPath 'github-output.txt'

        $result = Invoke-ResolveSourceRef `
            -RepositoryPath $repositoryPath `
            -SourceRef 'feature/untrusted' `
            -Publish $true

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Publishing requires a trusted source_ref'
        Test-Path $env:GITHUB_OUTPUT | Should -BeFalse
    }
}

Describe 'ad-hoc signature repair' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'fails closed when a nested library cannot be signed' {
        $appBundle = Join-Path $testRoot ([guid]::NewGuid().ToString('N') + '.app')
        New-Item -ItemType Directory -Path $appBundle -Force | Out-Null
        New-Item -ItemType File -Path (Join-Path $appBundle 'libbroken.dylib') -Force | Out-Null
        $env:FAKE_CODESIGN_MODE = 'nested-failure'

        {
            Repair-AppleAdhocSignature $appBundle
        } | Should -Throw '*nested library*failed with exit code 17*'
    }

    It 'fails closed when the final bundle signature fails' {
        $appBundle = Join-Path $testRoot ([guid]::NewGuid().ToString('N') + '.app')
        New-Item -ItemType Directory -Path $appBundle -Force | Out-Null
        $env:FAKE_CODESIGN_MODE = 'bundle-failure'

        {
            Repair-AppleAdhocSignature $appBundle
        } | Should -Throw '*failed with exit code 19*'
    }
}

Describe 'bounded Mac notarization polling' {
    BeforeEach {
        Reset-BuildTestEnvironment
        Mock Start-Sleep {}
        Mock Write-NotarySubmissionDiagnostics {}
    }

    It 'uses a workflow-controlled timeout with a bounded default' {
        Get-NotarizationTimeoutSeconds | Should -Be 1800

        $env:TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS = '90'
        Get-NotarizationTimeoutSeconds | Should -Be 90

        $env:TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS = '7201'
        { Get-NotarizationTimeoutSeconds } | Should -Throw '*integer from 1 through 7200*'
    }

    It 'performs a final poll at the configured deadline' {
        $origin = [DateTimeOffset]::Parse('2026-01-01T00:00:00Z')
        $script:notaryTimestamps = @($origin, $origin, $origin.AddSeconds(15))
        $script:notaryTimestampIndex = 0
        Mock Get-NotaryTimestamp {
            $timestamp = $script:notaryTimestamps[$script:notaryTimestampIndex]
            $script:notaryTimestampIndex++
            return $timestamp
        }
        $script:notaryPollCount = 0
        Mock Invoke-NotaryToolJson {
            $script:notaryPollCount++
            [pscustomobject]@{
                status = if ($script:notaryPollCount -lt 3) { 'In Progress' } else { 'Accepted' }
            }
        }

        Wait-NotarySubmission `
            -SubmissionId 'submission-accepted' `
            -CredentialArguments @('--key', 'private-key-path') `
            -TimeoutSeconds 30 `
            -PollIntervalSeconds 15

        Should -Invoke Invoke-NotaryToolJson -Times 3 -Exactly
        Should -Invoke Start-Sleep -Times 2 -Exactly `
            -ParameterFilter { $Seconds -eq 15 }
        Should -Invoke Write-NotarySubmissionDiagnostics -Times 0 -Exactly
    }

    It 'reports the submission log when Apple rejects the submission' {
        Mock Invoke-NotaryToolJson {
            [pscustomobject]@{ status = 'Invalid' }
        }

        {
            Wait-NotarySubmission `
                -SubmissionId 'submission-invalid' `
                -CredentialArguments @('--key', 'private-key-path') `
                -TimeoutSeconds 30 `
                -PollIntervalSeconds 15
        } | Should -Throw "*failed with status 'Invalid'*"

        Should -Invoke Write-NotarySubmissionDiagnostics -Times 1 -Exactly `
            -ParameterFilter { $SubmissionId -eq 'submission-invalid' }
    }

    It 'reports the last status and submission log at the deadline' {
        $origin = [DateTimeOffset]::Parse('2026-01-01T00:00:00Z')
        $script:notaryTimestamps = @(
            $origin,
            $origin,
            $origin.AddSeconds(15),
            $origin.AddSeconds(30)
        )
        $script:notaryTimestampIndex = 0
        Mock Get-NotaryTimestamp {
            $timestamp = $script:notaryTimestamps[$script:notaryTimestampIndex]
            $script:notaryTimestampIndex++
            return $timestamp
        }
        Mock Invoke-NotaryToolJson {
            [pscustomobject]@{ status = 'In Progress' }
        }

        {
            Wait-NotarySubmission `
                -SubmissionId 'submission-timeout' `
                -CredentialArguments @('--key', 'private-key-path') `
                -TimeoutSeconds 30 `
                -PollIntervalSeconds 15
        } | Should -Throw "*did not complete within 30 seconds*Last status: 'In Progress'*"

        Should -Invoke Invoke-NotaryToolJson -Times 3 -Exactly
        Should -Invoke Start-Sleep -Times 2 -Exactly `
            -ParameterFilter { $Seconds -eq 15 }
        Should -Invoke Write-NotarySubmissionDiagnostics -Times 1 -Exactly `
            -ParameterFilter { $SubmissionId -eq 'submission-timeout' }
    }

    It 'submits without the unbounded notarytool wait option' {
        (Get-Command Invoke-MacNotarization).Definition | Should -Not -Match '--wait'
        (Get-Command Invoke-MacNotarization).Definition | Should -Match 'Wait-NotarySubmission'
    }
}

Describe 'optional Apple sideload signing' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'preserves the no-secret iOS fallback' {
        $result = New-IosAdHocSideload `
            -ProjectFile $projectFile `
            -TargetFramework 'net11.0-ios' `
            -Configuration 'Release' `
            -RuntimeIdentifier 'ios-arm64' `
            -OutputPath $testRoot `
            -AppDisplayVersion '11.0' `
            -AppBuildNumber '1'

        $result | Should -BeNullOrEmpty
    }

    It 'fails when a configured iOS ad-hoc publish fails' {
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'AdHoc Profile'
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $binlogPath = Join-Path $testRoot 'ios-adhoc-build.binlog'
        $script:publishArguments = $null
        Mock Invoke-DotNetPublish {
            param($Arguments)
            $script:publishArguments = $Arguments
            throw 'simulated ad-hoc publish failure'
        }

        {
            New-IosAdHocSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-ios' `
                -Configuration 'Release' `
                -RuntimeIdentifier 'ios-arm64' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -BinlogArguments @("/bl:$binlogPath")
        } | Should -Throw '*simulated ad-hoc publish failure*'

        $script:publishArguments | Should -Contain "/bl:$binlogPath"
    }

    It 'fails when Developer ID signing is only partially configured' {
        $env:APPLE_DEVELOPERID_CODESIGN_KEY = 'Developer ID Application'

        {
            New-MacCatalystDeveloperIdSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-maccatalyst' `
                -Configuration 'Release' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -RuntimeIdentifier 'maccatalyst-arm64'
        } | Should -Throw '*partially configured*'
    }

    It 'fails when a configured Developer ID publish fails' {
        $env:APPLE_DEVELOPERID_CODESIGN_KEY = 'Developer ID Application'
        $env:APPLE_DEVELOPERID_CODESIGN_PROVISION = 'Developer ID Profile'
        $binlogPath = Join-Path $testRoot 'maccatalyst-developer-id-build.binlog'
        $script:publishArguments = $null
        Mock Invoke-DotNetPublish {
            param($Arguments)
            $script:publishArguments = $Arguments
            throw 'simulated Developer ID publish failure'
        }

        {
            New-MacCatalystDeveloperIdSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-maccatalyst' `
                -Configuration 'Release' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -RuntimeIdentifier 'maccatalyst-arm64' `
                -BinlogArguments @("/bl:$binlogPath")
        } | Should -Throw '*simulated Developer ID publish failure*'

        $script:publishArguments | Should -Contain "/bl:$binlogPath"
    }
}

Describe 'Developer ID installer configuration' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'accepts Developer ID assets when both are absent' {
        Assert-PairedEnvironmentValues `
            'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
            'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
            'Developer ID sideload signing' |
            Should -BeFalse
    }

    It 'rejects a Developer ID certificate without its profile' {
        $env:APPLE_DEVELOPERID_CERTIFICATE_BASE64 = 'certificate'

        {
            Assert-PairedEnvironmentValues `
                'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
                'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
                'Developer ID sideload signing'
        } | Should -Throw '*partially configured*'
    }

    It 'rejects a Developer ID profile without its certificate' {
        $env:APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64 = 'profile'

        {
            Assert-PairedEnvironmentValues `
                'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
                'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
                'Developer ID sideload signing'
        } | Should -Throw '*partially configured*'
    }

    It 'accepts Developer ID assets when both are present' {
        $env:APPLE_DEVELOPERID_CERTIFICATE_BASE64 = 'certificate'
        $env:APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64 = 'profile'

        Assert-PairedEnvironmentValues `
            'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
            'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
            'Developer ID sideload signing' |
            Should -BeTrue
    }
}

Describe 'custom template variant validation' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'accepts a custom variant with safe project name <ProjectName>' -ForEach @(
        @{ ProjectName = 'CustomApp' }
        @{ ProjectName = 'Custom.App_2-Preview' }
    ) {
        param($ProjectName)

        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            'Custom_Variant-2' = @{
                displayName = 'Custom App'
                projectName = $ProjectName
                template = 'maui'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom_variant-2' 'android'

        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match '"variant":"custom_variant-2"'
        $result.Output | Should -Match ([regex]::Escape("""projectName"":""$ProjectName"""))
    }

    It 'rejects a custom variant without a template' {
        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            custom = @{
                displayName = 'Custom App'
                projectName = 'CustomApp'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match "Variant 'custom' does not define required field 'template'"
    }

    It 'rejects a custom variant without a project name' {
        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            custom = @{
                displayName = 'Custom App'
                template = 'maui'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match "Variant 'custom' does not define required field 'projectName'"
    }

    It 'rejects unsafe project name <Case>' -ForEach @(
        @{ Case = 'parent path escape'; ProjectName = '../escape' }
        @{ Case = 'forward-slash path'; ProjectName = 'nested/name' }
        @{ Case = 'backslash path'; ProjectName = 'nested\name' }
        @{ Case = 'quote injection'; ProjectName = 'Bad"Name' }
        @{ Case = 'newline injection'; ProjectName = "Bad`nName" }
    ) {
        param($ProjectName)

        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            custom = @{
                displayName = 'Custom App'
                projectName = $ProjectName
                template = 'maui'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Invalid project name'
    }

    It 'rejects an unsafe custom variant name <Name>' -ForEach @(
        @{ Name = '../escape' }
        @{ Name = 'nested/name' }
        @{ Name = 'nested\name' }
        @{ Name = 'custom.variant' }
    ) {
        param($Name)

        $customDefinitions = @{}
        $customDefinitions[$Name] = @{
            displayName = 'Custom App'
            projectName = 'CustomApp'
            template = 'maui'
            androidApplicationId = 'com.example.custom'
        }
        $env:TEMPLATE_APP_VARIANTS_JSON = $customDefinitions | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'all' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Invalid custom template app variant name'
    }
}

Describe 'Android artifact safety' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'requires an APK before assigning the sideload output' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'no-artifacts'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Android APK publish completed but no APK artifact was found'
        if (Test-Path $case.GitHubOutput) {
            Get-Content -Path $case.GitHubOutput -Raw | Should -Not -Match 'sideload_package_path='
        }
    }

    It 'emits installable APK, store AAB, and distinct binlog outputs' {
        $case = New-BuildTestCase
        $keystorePath = Join-Path $case.Root 'test.keystore'
        Set-Content -Path $keystorePath -Value 'fake keystore'
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:ANDROID_KEYSTORE_PATH = $keystorePath
        $env:ANDROID_KEYSTORE_PASSWORD = 'password'
        $env:ANDROID_KEY_ALIAS = 'alias'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.package_path | Should -Match '\.aab$'
        $outputValues.sideload_package_path | Should -Match '\.apk$'
        $outputValues.binlog_path | Should -Be (Join-Path $case.OutputRoot 'build.binlog')
        $outputValues.store_binlog_path | Should -Be (Join-Path $case.OutputRoot 'store-build.binlog')
        Test-Path $outputValues.package_path | Should -BeTrue
        Test-Path $outputValues.sideload_package_path | Should -BeTrue
        Test-Path $outputValues.binlog_path | Should -BeTrue
        Test-Path $outputValues.store_binlog_path | Should -BeTrue
    }

    It 'keeps every configured Android signing secret out of APK and AAB binlog command lines' {
        $case = New-BuildTestCase
        $keystorePath = Join-Path $case.Root 'test.keystore'
        $signingEnvironmentLog = Join-Path $case.Root 'signing-environment.log'
        Set-Content -Path $keystorePath -Value 'fake keystore'
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:FAKE_ANDROID_SIGNING_ENV_LOG = $signingEnvironmentLog
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:ANDROID_KEYSTORE_PATH = $keystorePath
        $env:ANDROID_KEYSTORE_PASSWORD = 'store-password-secret'
        $env:ANDROID_KEY_PASSWORD = 'key-password-secret'
        $env:ANDROID_KEY_ALIAS = 'signing-alias-secret'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $binlogCommandLines = @(
            Get-Content -Path (Join-Path $case.OutputRoot 'build.binlog') -Raw
            Get-Content -Path (Join-Path $case.OutputRoot 'store-build.binlog') -Raw
        )
        foreach ($commandLine in $binlogCommandLines) {
            $commandLine | Should -Match '(?s)-p\s+AndroidSigningKeyAlias=env:ANDROID_SIGNING_KEY_ALIAS'
            $commandLine | Should -Match '(?s)-p\s+AndroidSigningStorePass=env:ANDROID_SIGNING_STORE_PASS'
            $commandLine | Should -Match '(?s)-p\s+AndroidSigningKeyPass=env:ANDROID_SIGNING_KEY_PASS'
            $commandLine | Should -Not -Match 'signing-alias-secret|store-password-secret|key-password-secret'
        }

        $signingEnvironment = Get-Content -Path $signingEnvironmentLog -Raw
        $signingEnvironment | Should -Match 'alias=signing-alias-secret'
        $signingEnvironment | Should -Match 'storePass=store-password-secret'
        $signingEnvironment | Should -Match 'keyPass=key-password-secret'
    }
}

Describe 'iOS dry-run artifact safety' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'keeps a device IPA when simulator artifact discovery fails' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-device-only'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64'

        $result.ExitCode | Should -Be 0
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.package_path | Should -Match '\.ipa$'
        $outputValues.sideload_package_path | Should -Be $outputValues.package_path
        Test-Path $outputValues.package_path | Should -BeTrue
    }
}

Describe 'publish binlogs' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'uses a distinct store binlog for an Android publish' {
        $configuration = Get-BinlogConfiguration `
            -OutputPath $testRoot `
            -Platform 'android' `
            -Publish `
            -CreateBinlog

        $configuration.BuildPath | Should -Be (Join-Path $testRoot 'build.binlog')
        $configuration.StorePath | Should -Be (Join-Path $testRoot 'store-build.binlog')
        $configuration.BuildArguments | Should -Contain "/bl:$($configuration.BuildPath)"
        $configuration.StoreArguments | Should -Contain "/bl:$($configuration.StorePath)"
    }

    It 'uses a dedicated binlog for an iOS ad-hoc publish' {
        $configuration = Get-BinlogConfiguration `
            -OutputPath $testRoot `
            -Platform 'ios' `
            -Publish `
            -CreateBinlog

        $configuration.SideloadPath | Should -Be (Join-Path $testRoot 'ios-adhoc-build.binlog')
        $configuration.SideloadArguments | Should -Contain "/bl:$($configuration.SideloadPath)"
    }

    It 'uses a dedicated binlog for a Mac Catalyst Developer ID publish' {
        $configuration = Get-BinlogConfiguration `
            -OutputPath $testRoot `
            -Platform 'maccatalyst' `
            -Publish `
            -CreateBinlog

        $configuration.SideloadPath | Should -Be (Join-Path $testRoot 'maccatalyst-developer-id-build.binlog')
        $configuration.SideloadArguments | Should -Contain "/bl:$($configuration.SideloadPath)"
    }

    It 'emits both primary and ad-hoc iOS binlogs from the publish path' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-publish-success'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $env:IOS_CODESIGN_PROVISION = 'App Store Profile'
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'Ad Hoc Profile'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.binlog_path | Should -Be (Join-Path $case.OutputRoot 'build.binlog')
        $outputValues.sideload_binlog_path | Should -Be (Join-Path $case.OutputRoot 'ios-adhoc-build.binlog')
        Test-Path $outputValues.binlog_path | Should -BeTrue
        Test-Path $outputValues.sideload_binlog_path | Should -BeTrue
    }

    It 'preserves the App Store IPA before the ad-hoc publish can overwrite project outputs' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-store-overwrite'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $env:IOS_CODESIGN_PROVISION = 'App Store Profile'
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'Ad Hoc Profile'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64' `
            -Publish

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.package_path | Should -Be (Join-Path $case.OutputRoot 'store/TestApp.ipa')
        Get-Content -Path $outputValues.package_path -Raw | Should -Match '^app-store'
        Get-Content -Path (Join-Path $case.ProjectRoot 'bin/ios-arm64/TestApp.ipa') -Raw | Should -Match '^ad-hoc'
        Get-Content -Path $outputValues.sideload_package_path -Raw | Should -Match '^ad-hoc'
    }

    It 'preserves the ad-hoc binlog output when the secondary publish fails' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-adhoc-failure'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $env:IOS_CODESIGN_PROVISION = 'App Store Profile'
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'Ad Hoc Profile'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'iOS ad-hoc publish failed with exit code 23'
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.sideload_binlog_path | Should -Be (Join-Path $case.OutputRoot 'ios-adhoc-build.binlog')
        Test-Path $outputValues.sideload_binlog_path | Should -BeTrue
    }
}

Describe 'workflow test gate' {
    It 'runs the behavioral suite before matrix preparation' {
        $scriptTestsJob = [regex]::Match(
            $script:workflowText,
            '(?ms)^  script-tests:[ \t]*\r?\n.*?(?=^  [A-Za-z0-9_-]+:[ \t]*\r?$|\z)'
        )

        $scriptTestsJob.Success | Should -BeTrue
        $scriptTestsJob.Value | Should -Match '(?s)Invoke-Pester.*?-CI'
        $script:workflowText | Should -Match (
            '(?ms)^  prepare:.*?^\s{4}needs: script-tests\s*$')
        $script:workflowText | Should -Not -Match (
            '(?ms)uses:\s*actions/checkout@v4\s+with:\s+' +
            'ref:\s*\$\{\{\s*github\.ref\s*\}\}')
        [regex]::Matches(
            $script:workflowText,
            'ref:\s*\$\{\{\s*github\.sha\s*\}\}').Count |
            Should -Be 4
    }

    It 'passes source-derived SDK outputs to PowerShell through environment variables' {
        $script:workflowText | Should -Not -Match (
            '-DotNet(?:Sdk|Tfm)\s+"\$\{\{\s*(?:steps\.sdk|needs\.prepare)\.outputs\.dotnet_')
        $script:workflowText | Should -Not -Match (
            '\$displayVersion\s*=\s*"\$\{\{\s*steps\.sdk\.outputs\.dotnet_tfm')
        [regex]::Matches($script:workflowText, '-DotNetSdk "\$env:DOTNET_SDK"').Count |
            Should -Be 2
        [regex]::Matches($script:workflowText, '-DotNetTfm "\$env:DOTNET_TFM"').Count |
            Should -Be 3
    }

    It 'passes generated project paths to PowerShell through environment variables' {
        $script:workflowText | Should -Not -Match (
            '-ProjectPath\s+"\$\{\{\s*steps\.app\.outputs\.project_path\s*\}\}"')
        [regex]::Matches(
            $script:workflowText,
            'PROJECT_PATH:\s*\$\{\{\s*steps\.app\.outputs\.project_path\s*\}\}').Count |
            Should -Be 2
        [regex]::Matches($script:workflowText, '-ProjectPath "\$env:PROJECT_PATH"').Count |
            Should -Be 2
    }

    It 'serializes publish runs across source refs while preserving dry-run concurrency' {
        $expectedGroup = "group: `${{ github.workflow }}-`${{ inputs.publish && 'publish' || format('dry-run-{0}', inputs.source_ref) }}"
        $script:workflowText | Should -Match ([regex]::Escape($expectedGroup))
        $script:workflowText | Should -Match 'TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS:.*1800'
    }

    It 'sets realistic deterministic timeouts for every job' {
        $expectedTimeouts = [ordered]@{
            'script-tests' = 15
            'prepare' = 30
            'dry-run-build' = 120
            'publish' = 180
        }

        foreach ($jobName in $expectedTimeouts.Keys) {
            $jobBlock = [regex]::Match(
                $script:workflowText,
                "(?ms)^  $([regex]::Escape($jobName)):\s*\r?\n.*?(?=^  [A-Za-z0-9_-]+:\s*\r?$|\z)"
            )
            $jobBlock.Success | Should -BeTrue -Because "job '$jobName' must exist"
            $jobBlock.Value | Should -Match (
                "(?m)^\s{4}timeout-minutes:\s+$($expectedTimeouts[$jobName])\s*$")
        }
    }

    It 'includes the workflow attempt in every uploaded artifact name' {
        $artifactNames = [regex]::Matches(
            $script:workflowText,
            '(?m)^\s+name:\s+(template-app-(?:dryrun|publish)[^\r\n]+)$'
        )

        $artifactNames.Count | Should -Be 3
        foreach ($artifactName in $artifactNames) {
            $artifactName.Groups[1].Value | Should -Match (
                '\$\{\{\s*github\.run_attempt\s*\}\}')
        }
    }
}

Describe 'template metadata replacement' {
    BeforeAll {
        $newTemplateScriptPath = Join-Path $PSScriptRoot 'New-TemplateApp.ps1'
        $newTemplateTokens = $null
        $newTemplateParseErrors = $null
        $newTemplateAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $newTemplateScriptPath,
            [ref]$newTemplateTokens,
            [ref]$newTemplateParseErrors
        )
        if ($newTemplateParseErrors -and $newTemplateParseErrors.Count -gt 0) {
            throw ($newTemplateParseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
        }

        foreach ($functionName in @('ConvertTo-XmlEscaped', 'Set-ProjectElementValue')) {
            $function = $newTemplateAst.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)

            if (-not $function) {
                throw "Function '$functionName' not found"
            }

            Invoke-Expression $function.Extent.Text
        }
    }

    It 'treats dollar signs as literal replacement text' {
        $content = '<ApplicationTitle>Old</ApplicationTitle>'

        Set-ProjectElementValue $content 'ApplicationTitle' 'Cash $$ App $&' |
            Should -Be '<ApplicationTitle>Cash $$ App $&amp;</ApplicationTitle>'
    }
}

Describe 'TestFlight error handling' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'fails when external groups cannot receive a build due to a beta-review conflict' {
        $env:FAKE_TESTFLIGHT_ERROR = 'Another build is in review'
        $env:FAKE_TESTFLIGHT_GROUPS = 'External Testers'

        $result = Invoke-FastfileHarness

        $result.ExitCode | Should -Be 42
        $result.Output | Should -Match 'requested external TestFlight groups did not receive it'
    }

    It 'allows an upload-only beta-review conflict when no external groups were requested' {
        $env:FAKE_TESTFLIGHT_ERROR = 'Another build is in review'
        $env:FAKE_TESTFLIGHT_GROUPS = ''

        $result = Invoke-FastfileHarness

        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'no external distribution was requested'
        $result.Output | Should -Match 'lane succeeded'
    }

    It 'fails instead of reporting a processing timeout as successful' {
        $env:FAKE_TESTFLIGHT_ERROR = 'BuildWatcher exceeded processing timeout'
        $env:FAKE_TESTFLIGHT_GROUPS = 'External Testers'

        $result = Invoke-FastfileHarness

        $result.ExitCode | Should -Be 42
        $result.Output | Should -Match 'requested TestFlight distribution could not be completed'
    }
}
