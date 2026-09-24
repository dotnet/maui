#Requires -Modules Pester

Describe 'Public CI provisioning' {
  BeforeAll {
    $pipeline = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ci.yml') -Raw
    $provision = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/provision.yml') -Raw
    $helixStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-helix-tests.yml') -Raw
    $sharedVariables = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/variables.yml') -Raw
  }

  Describe 'Apple dependency alignment' {
    BeforeAll {
      [xml]$buildProps = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../Directory.Build.props') -Raw
      [xml]$versions = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../Versions.props') -Raw
      [xml]$buildTargets = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../Directory.Build.targets') -Raw
      [xml]$versionDetails = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../Version.Details.xml') -Raw
      $globalJson = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../global.json') -Raw | ConvertFrom-Json
      $nativeProject = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../src/Core/AppleNative/PlatformInterop/MauiPlatformInterop.xcodeproj/project.pbxproj') -Raw
    }

    It 'uses a consistent SDK new enough to run the Apple 27 binding generator' {
      $sdkVersion = $versions.SelectSingleNode(
        '/Project/PropertyGroup/MicrosoftNETSdkPackageVersion').InnerText
      $globalJson.tools.dotnet | Should -Be $sdkVersion
      $versionDetails.SelectSingleNode(
        "/Dependencies/ProductDependencies/Dependency[@Name='Microsoft.NET.Sdk']").Version |
        Should -Be $sdkVersion
      [semver]$sdkVersion | Should -BeGreaterOrEqual ([semver]'11.0.100-rc.2.26465.108')
    }

    It 'aligns native and managed iOS deployment targets with the Xcode 27 minimum' {
      $iosProperties = $buildTargets.Project.PropertyGroup |
        Where-Object { $_.Condition -eq "'`$(_MauiTargetPlatformIsiOS)' == 'True'" }

      $iosProperties.SupportedOSPlatformVersion | Should -Be '15.0'
      $iosProperties.TargetPlatformMinVersion | Should -Be '15.0'
      ([regex]::Matches($nativeProject, '(?m)^\s*IPHONEOS_DEPLOYMENT_TARGET = 15\.0;')).Count |
        Should -Be 2
      $nativeProject | Should -Not -Match 'IPHONEOS_DEPLOYMENT_TARGET\[sdk=iphoneos'
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

  It 'selects the shared Aces image for public macOS build, pack, and integration jobs' {
    $buildPools = [regex]::Match(
      $pipeline, '(?ms)^- name: BuildPlatformsPublic\r?\n.*?(?=^- name:)').Value
    $macPools = [regex]::Match(
      $pipeline, '(?ms)^- name: MacOSPool\r?\n.*?(?=^- name:)').Value
    $imageDemand = [regex]::Escape('ImageOverride -equals $(AcesMacImageOverride)')

    $buildPools | Should -Match '(?s)- name: AcesShared\s+demands:'
    $buildPools | Should -Match $imageDemand
    $buildPools | Should -Not -Match 'xcode -equals'
    $macPools | Should -Match '(?s)public:\s+name: AcesShared\s+demands:'
    $macPools | Should -Match $imageDemand
    $macPools | Should -Not -Match 'xcode -equals'
  }

  It 'selects the shared Aces image for <Pipeline> <Pool>' -TestCases @(
    @{ Pipeline = 'ci-device-tests.yml'; Pool = 'macOSPoolPublic' }
    @{ Pipeline = 'ci-uitests.yml'; Pool = 'androidPoolPublic' }
    @{ Pipeline = 'ci-uitests.yml'; Pool = 'iosPoolPublic' }
  ) {
    param($Pipeline, $Pool)

    $testPipeline = Get-Content -LiteralPath (Join-Path $PSScriptRoot $Pipeline) -Raw
    $poolMatch = [regex]::Match(
      $testPipeline,
      '(?ms)^\s*- name: ' + [regex]::Escape($Pool) + '\r?\n.*?(?=^\s*- name:|\z)')

    $poolMatch.Success | Should -BeTrue
    $poolMatch.Value | Should -Match '(?m)^\s+name: AcesShared\r?$'
    $poolMatch.Value | Should -Match (
      [regex]::Escape('ImageOverride -equals $(AcesMacImageOverride)'))
    $poolMatch.Value | Should -Not -Match 'xcode -equals|vmImage'
  }

  It 'selects GoldenGate independently of the Azure Pipelines hosted image' {
    $imageVariables = [regex]::Matches(
      $sharedVariables,
      '(?m)^- name: AcesMacImageOverride\r?\n  value: (\S+)\r?$')

    $imageVariables.Count | Should -Be 1
    $imageVariables[0].Groups[1].Value | Should -Be 'ACES_VM_SharedPool_GoldenGate'
    $sharedVariables | Should -Not -Match 'Unsupported_HostedMacImage_'
    $sharedVariables | Should -Match (
      '(?m)^- name: hostedMacImage\r?\n  type: string\r?\n  default: macOS-26\r?$')
    $sharedVariables | Should -Match (
      '(?m)^- name: HostedMacImage\r?\n  value: ' +
      [regex]::Escape('${{ parameters.hostedMacImage }}') + '\r?$')
  }

  It 'retains Xcode selection and simulator setup for the test pipelines' {
    $devicePipeline = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'ci-device-tests.yml') -Raw
    $uiBuildSteps = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/ui-tests-build-sample.yml') -Raw
    $uiTestSteps = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/ui-tests-steps.yml') -Raw

    $devicePipeline | Should -Match '(?m)^          skipXcode: false\r?$'
    $devicePipeline | Should -Match '(?m)^          skipSimulatorSetup: false\r?$'
    $uiBuildSteps | Should -Match ([regex]::Escape(
      'skipXcode: ${{ or(eq(parameters.platform, ''android''), eq(parameters.platform, ''windows'')) }}'))
    $uiTestSteps | Should -Match ([regex]::Escape(
      'skipXcode: ${{ or(eq(parameters.platform, ''android''), eq(parameters.platform, ''windows''), eq(parameters.platform, ''catalyst'')) }}'))
    $uiTestSteps | Should -Match ([regex]::Escape(
      'skipSimulatorSetup: ${{ or(eq(parameters.platform, ''android''), eq(parameters.platform, ''windows''), eq(parameters.platform, ''catalyst'')) }}'))
  }

  It 'handles <Scenario> without relying on the selected Xcode bundle name' -TestCases @(
    @{ Scenario = 'an image-selected beta'; Version = '27.0'; Fallback = $false; UseSelected = $true; ExitCode = 0 }
    @{ Scenario = 'an image-selected patch'; Version = '27.0.1'; Fallback = $false; UseSelected = $true; ExitCode = 0 }
    @{ Scenario = 'a compatible image selection with another installed Xcode'; Version = '27.0'; Fallback = $true; UseSelected = $true; ExitCode = 0 }
    @{ Scenario = 'an incompatible image selection with a compatible fallback'; Version = '26.6'; Fallback = $true; UseSelected = $false; ExitCode = 0 }
    @{ Scenario = 'an incompatible image selection without a fallback'; Version = '26.6'; Fallback = $false; UseSelected = $false; ExitCode = 1 }
    @{ Scenario = 'a different minor version'; Version = '27.01'; Fallback = $false; UseSelected = $false; ExitCode = 1 }
  ) -Skip:($env:OS -eq 'Windows_NT' -or -not (Get-Command bash -ErrorAction SilentlyContinue)) {
    param($Version, $Fallback, $UseSelected, $ExitCode)

    $step = [regex]::Match(
      $provision,
      '(?ms)^  - script: \|\r?\n(?<script>.*?)^    displayName: Select Xcode Version\r?$')
    $step.Success | Should -BeTrue
    $script = [regex]::Replace($step.Groups['script'].Value, '(?m)^      ', '')
    $applications = Join-Path $TestDrive ([guid]::NewGuid().ToString())
    $selectedPath = "$applications/Xcode_27_beta_6.app/Contents/Developer"
    [void][System.IO.Directory]::CreateDirectory($selectedPath)
    if ($Fallback) {
      [void][System.IO.Directory]::CreateDirectory("$applications/Xcode_27.0.app")
    }
    $script = $script.Replace('/Applications', $applications).Replace('$(XCODE)', '27.0')

    $mockTools = @'
export HOME="$1"
selected_path="$2"
selected_version="$3"
xcode-select() {
  case "$1" in
    --print-path) printf '%s\n' "$selected_path" ;;
    -s) selected_path="$2"; printf 'SELECTED: %s\n' "$2" ;;
    *) return 99 ;;
  esac
}
xcodebuild() {
  if [[ "$1" == "-version" ]]; then
    printf 'Xcode %s\nBuild version test\n' "$selected_version"
  fi
}
xcrun() { "$@"; }
sudo() {
  case "$1" in
    xcode-select|xcodebuild) "$@" ;;
    *) return 99 ;;
  esac
}
sw_vers() { printf '27.0\n'; }
'@
    $output = & bash -c ($mockTools + "`n" + $script) -- $TestDrive $selectedPath $Version 2>&1 |
      Out-String
    $LASTEXITCODE | Should -Be $ExitCode -Because $output
    if ($ExitCode -eq 0) {
      $expectedPath = if ($UseSelected) { $selectedPath } else { "$applications/Xcode_27.0.app" }
      $output | Should -Match ([regex]::Escape("SELECTED: $expectedPath"))
    } else {
      $output | Should -Match 'Required Xcode 27\.0 is not installed'
      $output | Should -Not -Match 'SELECTED:'
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

  It 'runs the Helix monitor in the submission pool with the appropriate Linux image' {
    $monitorMatch = [regex]::Match(
      $helixStage,
      '(?ms)^  - job: HelixJobMonitor\r?\n.*?(?=^    steps:)')

    $monitorMatch.Success | Should -BeTrue
    $monitorMatch.Value | Should -Match (
      [regex]::Escape('name: ${{ parameters.helixPool.name }}'))
    $monitorMatch.Value | Should -Not -Match 'DncEng(Public|Internal)BuildPool'
    $monitorMatch.Value | Should -Match (
      '(?s)eq\(variables\[''System.TeamProject''\], ''public''\).*?' +
      'demands: ImageOverride -equals build\.azurelinux\.3\.amd64\.open')
    $monitorMatch.Value | Should -Match (
      '(?s)\$\{\{ else \}\}:.*?demands: ImageOverride -equals build\.azurelinux\.3\.amd64\r?\n')
  }
}
