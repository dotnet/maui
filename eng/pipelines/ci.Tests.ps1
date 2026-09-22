#Requires -Modules Pester

Describe 'ci.yml provisioning' {
  BeforeAll {
    $pipeline = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ci.yml') -Raw
    $provision = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/provision.yml') -Raw
    $helixStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-helix-tests.yml') -Raw
  }

  It 'selects Xcode 27 RC agents for public macOS build, pack, and integration jobs' {
    $buildPools = [regex]::Match(
      $pipeline, '(?ms)^- name: BuildPlatformsPublic\r?\n.*?(?=^- name:)').Value
    $macPools = [regex]::Match(
      $pipeline, '(?ms)^- name: MacOSPool\r?\n.*?(?=^- name:)').Value
    $xcodeDemand = [regex]::Escape(
      'xcode -equals /Applications/Xcode_27.0.0-rc.app/Contents/Developer')

    $buildPools | Should -Match '(?s)- name: MAUI\s+demands:'
    $buildPools | Should -Match $xcodeDemand
    $macPools | Should -Match '(?s)public:\s+name: MAUI\s+demands:'
    $macPools | Should -Match $xcodeDemand
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
