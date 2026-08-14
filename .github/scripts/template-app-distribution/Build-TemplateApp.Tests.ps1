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
        'Invoke-DotNetPublish',
        'Test-IsNet11OrLater',
        'Add-NativeAotArguments',
        'Get-BinlogConfiguration',
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
    $script:fastfilePath = Join-Path $PSScriptRoot 'fastlane/Fastfile'
    $script:workflowPath = Join-Path $PSScriptRoot '../../workflows/template-app-distribution.yml'
    $script:workflowText = Get-Content -Path $script:workflowPath -Raw
    $script:pwshPath = (Get-Command pwsh -ErrorAction Stop).Source
    $script:rubyPath = (Get-Command ruby -ErrorAction SilentlyContinue).Source
    $script:originalPath = $env:PATH
    $script:testEnvironmentNames = @(
        'FAKE_DOTNET_MODE',
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
        'APPLE_DEVELOPERID_CERTIFICATE_BASE64',
        'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64',
        'IOS_ADHOC_CODESIGN_PROVISION',
        'IOS_CODESIGN_KEY',
        'APPLE_DEVELOPERID_CODESIGN_KEY',
        'APPLE_DEVELOPERID_CODESIGN_PROVISION',
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
        Set-Content -Path $binlogPath -Value "fake binlog"
    }
}
$argumentText = $args -join "`n"

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
        $script:workflowText | Should -Match (
            '(?ms)^  script-tests:.*?Invoke-Pester.*?-CI')
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
