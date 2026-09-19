#Requires -Modules Pester

Describe 'Device-test Helix SDK payload' {
  BeforeAll {
    $projectPath = Join-Path $PSScriptRoot 'helix_xharness.proj'
    $sdkPackageVersion = '10.0.113-servicing.26454.107'
  }

  It 'uses the published SDK version on <TargetOS>' -ForEach @(
    @{ TargetOS = 'windows' }
    @{ TargetOS = 'android' }
    @{ TargetOS = 'ios' }
    @{ TargetOS = 'maccatalyst' }
  ) {
    $output = & dotnet msbuild $projectPath -nologo -v:q `
      "-p:TargetOS=$TargetOS" `
      "-p:MicrosoftNETSdkPackageVersion=$sdkPackageVersion" `
      -p:NETCoreSdkVersion=10.0.113 `
      -getProperty:DotNetCliVersion,DotNetCliPackageType,IncludeDotNetCli,FailOnTestFailure

    $LASTEXITCODE | Should -Be 0
    $properties = ($output | Out-String | ConvertFrom-Json).Properties
    $properties.DotNetCliVersion | Should -Be $sdkPackageVersion
    $properties.DotNetCliPackageType | Should -Be 'sdk'
    $properties.IncludeDotNetCli | Should -Be 'true'
    $properties.FailOnTestFailure | Should -Be 'true'
  }

  It 'preserves an explicit command-line SDK version' {
    $output = & dotnet msbuild $projectPath -nologo -v:q `
      -p:TargetOS=windows `
      "-p:MicrosoftNETSdkPackageVersion=$sdkPackageVersion" `
      -p:DotNetCliVersion=10.0.100 `
      -getProperty:DotNetCliVersion

    $LASTEXITCODE | Should -Be 0
    ($output | Out-String).Trim() | Should -Be '10.0.100'
  }
}
