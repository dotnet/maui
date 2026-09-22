#Requires -Modules Pester

Describe 'ci.yml provisioning' {
  BeforeAll {
    $pipeline = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ci.yml') -Raw
    $setupTestEnv = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/setup-test-env.yml') -Raw
    $helixStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-helix-tests.yml') -Raw
  }

  It 'provisions the required Xcode without enabling certificate provisioning in <Stage>' -TestCases @(
    @{ Stage = 'stage-build' }
    @{ Stage = 'stage-pack' }
  ) {
    param($Stage)

    $stageMatch = [regex]::Match(
      $pipeline,
      '(?ms)^- template: /eng/pipelines/arcade/' + [regex]::Escape($Stage) +
      '\.yml@self\r?\n.*?(?=^- template:|\z)')

    $stageMatch.Success | Should -BeTrue
    $stageMatch.Value | Should -Match '(?m)^\s*skipProvisionator: false\r?$'
    $stageMatch.Value | Should -Match '(?m)^\s*skipCertificates: true\r?$'
    $stageMatch.Value | Should -Match '(?m)^\s*skipXcode: false\r?$'
  }

  It 'provisions Xcode for integration tests while preserving Android-only opt-outs' {
    $setupTestEnv | Should -Match '(?m)^\s*skipProvisionator: false\r?$'
    $setupTestEnv | Should -Match '(?m)^\s*skipCertificates: true\r?$'
    $setupTestEnv | Should -Match (
      [regex]::Escape('skipXcode: ${{ parameters.skipXcode }}'))
    $setupTestEnv | Should -Match (
      [regex]::Escape('skipSimulatorSetup: ${{ parameters.skipXcode }}'))
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
