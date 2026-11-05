#Requires -Modules Pester

Describe 'CI provisioning' {
  BeforeAll {
    $provision = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/provision.yml') -Raw
    $unitStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-unit-tests.yml') -Raw
  }

  Describe 'SDK alignment' {
    BeforeAll {
      [xml]$buildProps = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../Directory.Build.props') -Raw
      [xml]$versions = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../Versions.props') -Raw
      [xml]$versionDetails = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../Version.Details.xml') -Raw
      $globalJson = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../global.json') -Raw | ConvertFrom-Json
    }

    It 'keeps the bootstrap, workload provisioning, and provenance SDK pins aligned' {
      $sdkVersion = $versions.SelectSingleNode(
        '/Project/PropertyGroup/MicrosoftNETSdkPackageVersion').InnerText
      $globalJson.tools.dotnet | Should -Be $sdkVersion
      $versionDetails.SelectSingleNode(
        "/Dependencies/ProductDependencies/Dependency[@Name='Microsoft.NET.Sdk']").Version |
        Should -Be $sdkVersion
    }

    It 'aligns <Platform> target frameworks with the selected Apple SDK' -TestCases @(
      @{ Platform = 'Ios'; SdkAlias = 'MicrosoftiOSSdkPackageVersion' }
      @{ Platform = 'Tvos'; SdkAlias = 'MicrosofttvOSSdkPackageVersion' }
      @{ Platform = 'MacCatalyst'; SdkAlias = 'MicrosoftMacCatalystSdkPackageVersion' }
      @{ Platform = 'Macos'; SdkAlias = 'MicrosoftmacOSSdkPackageVersion' }
    ) {
      param($Platform, $SdkAlias)

      $reference = $versions.SelectSingleNode("/Project/PropertyGroup/$SdkAlias").InnerText
      $referenceMatch = [regex]::Match($reference, '^\$\((?<property>[A-Za-z0-9_]+)\)$')
      $referenceMatch.Success | Should -BeTrue
      $sdkProperty = $referenceMatch.Groups['property'].Value
      $sdkVersion = $versions.SelectSingleNode("/Project/PropertyGroup/$sdkProperty").InnerText
      [version]$version = ($sdkVersion -split '-')[0]
      $platformVersion = "$($version.Major).$($version.Minor)"

      $buildProps.SelectSingleNode(
        "/Project/PropertyGroup/${Platform}TargetFrameworkVersionSdkDefault").InnerText |
        Should -Be $platformVersion
      $buildProps.SelectSingleNode(
        "/Project/PropertyGroup/${Platform}TargetFrameworkVersion").InnerText |
        Should -Be $platformVersion
    }
  }

  It 'fails before simulator setup when the required Xcode is missing' {
    $missingXcodeMatch = [regex]::Match(
      $provision,
      '(?s)if \[\[ -z "\$XCODE_PATH" \]\]; then.*?\r?\n      fi')

    $missingXcodeMatch.Success | Should -BeTrue
    $missingXcodeMatch.Value | Should -Match 'task\.logissue type=error'
    $missingXcodeMatch.Value | Should -Match 'Update the macOS agent image'
    $missingXcodeMatch.Value | Should -Match '(?m)^\s*exit 1\r?$'
    $missingXcodeMatch.Value | Should -Not -Match 'LATEST_XCODE'
  }

  It 'executes all previously submitted unit projects directly with Hot Reload enabled' {
    @([regex]::Matches($unitStage, '(?m)^  - src/.*\.csproj$')).Count | Should -Be 14
    $unitStage | Should -Match 'VisualTestUtils.MagickNet.Tests.csproj'
    $unitStage | Should -Match 'BlazorWebView.Windows.UnitTests.csproj'
    $unitStage | Should -Match 'src/TestUtils/tests/UITest.NUnit.Tests/UITest.NUnit.Tests.csproj'
    $unitStage | Should -Match 'src/TestUtils/tests/TestUtils.UnitTests/TestUtils.UnitTests.csproj'
    $unitStage | Should -Match 'buildConfigurations: \[ Debug, Release \]'
    $unitStage | Should -Match 'DOTNET_MODIFIABLE_ASSEMBLIES: debug'
    $unitStage | Should -Match 'command: test'
    $unitStage | Should -Match 'failTaskOnMissingResultsFile: true'
    $unitStage | Should -Not -Match 'Helix|XHarness|--logger'
  }

  It 'uses the emulator-capable Android execution pool rather than a build-only image' {
    $devicePipeline = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'ci-device-tests.yml') -Raw
    $uiPipeline = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'ci-uitests.yml') -Raw

    foreach ($scope in @('Public', 'Internal')) {
      $devicePool = [regex]::Match($devicePipeline,
        "(?s)- name: androidPool$scope\r?\n.*?default:\r?\n(?<pool>.*?)(?=\r?\n- name:)")
      $devicePool.Success | Should -BeTrue
      $devicePool.Groups['pool'].Value | Should -Match 'name: MAUI-DNCENG'
      $devicePool.Groups['pool'].Value | Should -Match 'ImageOverride -equals 1ESPT-Ubuntu22\.04'
      $uiPipeline | Should -Match "(?s)name: androidPoolLinux$scope.*?name: MAUI-DNCENG.*?ImageOverride -equals 1ESPT-Ubuntu22\.04"
    }
  }

  It 'labels the current Apple execution image honestly and retains strict OS checks' {
    $devicePipeline = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'ci-device-tests.yml') -Raw
    $deviceStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-device-tests.yml') -Raw
    $startDevice = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot '../../.github/skills/run-device-tests/scripts/Start-TestDevice.ps1') -Raw

    @([regex]::Matches($devicePipeline, 'name: macos27\r?\n\s+osMajor: 27')).Count |
      Should -Be 2
    $devicePipeline | Should -Not -Match 'name: macos(15|26)'
    $deviceStage | Should -Match 'expectedOSMajor: \$\{\{ apple.osMajor \}\}'
    $startDevice | Should -Match '\$version.Major -ne \$ExpectedOSMajor'
  }

  It 'allows focused mobile reruns without disabling Windows coverage by default' {
    $devicePipeline = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'ci-device-tests.yml') -Raw
    $devicePipeline | Should -Match '(?s)name: runWindowsTests\s+type: boolean\s+default: true'
    $devicePipeline | Should -Match ([regex]::Escape('runWindowsTests: ${{ parameters.runWindowsTests }}'))
  }

  It 'allows focused Windows packaging reruns while retaining both modes by default' {
    $devicePipeline = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'ci-device-tests.yml') -Raw
    $devicePipeline | Should -Match '(?s)name: windowsPackaging\s+type: string\s+default: both\s+values: \[ both, packaged, unpackaged \]'
    $devicePipeline | Should -Match ([regex]::Escape('windowsPackaging: ${{ parameters.windowsPackaging }}'))
  }
}
