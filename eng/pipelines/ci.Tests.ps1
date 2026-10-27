#Requires -Modules Pester

Describe 'CI provisioning' {
  BeforeAll {
    $provision = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/provision.yml') -Raw
    $helixStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-helix-tests.yml') -Raw
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

  Describe 'Helix MSBuild test payload' {
    BeforeAll {
      $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
      [xml]$helixProject = Get-Content -LiteralPath (Join-Path $repoRoot 'eng/helix.proj') -Raw
      $stagingTarget = $helixProject.SelectSingleNode("/Project/Target[@Name='PrepareMauiHelixPayload']")

      # Execute the real staging target without restoring the Helix SDK or submitting jobs.
      $stagingProject = Join-Path $TestDrive 'staging.proj'
      Set-Content -LiteralPath $stagingProject -Value "<Project>$($stagingTarget.OuterXml)</Project>"
      $payloadDir = Join-Path $TestDrive 'payload'
      $output = & dotnet msbuild $stagingProject -nologo -v:minimal -t:PrepareMauiHelixPayload `
        "-p:RepoRoot=$repoRoot/" "-p:MauiHelixPayloadDir=$payloadDir/"
      if ($LASTEXITCODE -ne 0) {
        throw "Helix payload staging failed with exit code ${LASTEXITCODE}: $output"
      }
    }

    It 'publishes the staged source files at their source-relative paths' {
      $helixProject.SelectSingleNode(
        '/Project/ItemGroup/HelixCorrelationPayload[@Include="$(MauiHelixPayloadDir)src"]/Destination').InnerText |
        Should -Be 'src'
    }

    It 'stages an unchanged copy of <File>' -TestCases @(
      @{ File = 'src/Core/src/nuget/buildTransitive/Microsoft.Maui.Core.Before.targets' }
      @{ File = 'src/Controls/src/Build.Tasks/nuget/buildTransitive/netstandard2.0/Microsoft.Maui.Controls.targets' }
      @{ File = 'src/Maui.InTree.props' }
      @{ File = 'src/Maui.InTree.targets' }
    ) {
      param($File)

      $stagedFile = Join-Path $payloadDir $File
      $stagedFile | Should -Exist
      (Get-FileHash -LiteralPath $stagedFile).Hash |
        Should -Be (Get-FileHash -LiteralPath (Join-Path $repoRoot $File)).Hash
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
