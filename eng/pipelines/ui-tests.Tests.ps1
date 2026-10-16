#Requires -Modules Pester

Describe 'Android Shell UI-test lanes' {
  BeforeAll {
    $uiTests = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/ui-tests.yml') -Raw
    $uiTestSteps = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/ui-tests-steps.yml') -Raw
    $androidCake = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot '../devices/android.cake') -Raw

    $mainLane = [regex]::Match(
      $uiTests, '(?ms)^  - stage: android_ui_tests_coreclr\r?\n.*?(?=^  - stage:)').Value
    $rendererLane = [regex]::Match(
      $uiTests, '(?ms)^  - stage: android_ui_tests_shell_renderer\r?\n.*?(?=^  - stage:)').Value
  }

  It 'keeps Shell in the main category matrix without opting into the renderer' {
    $uiTests | Should -Match "(?m)^    - 'Shell'\r?$"
    $mainLane | Should -Match 'each categoryGroup in parameters\.categoryGroupsToTest'
    $mainLane | Should -Match 'testFilter: \$\(CATEGORYGROUP\)'
    $mainLane | Should -Not -Match 'testConfigurationArgs:'
  }

  It 'reuses the common CoreCLR app build in both lanes' {
    foreach ($lane in @($mainLane, $rendererLane)) {
      $lane | Should -Match '(?m)^    dependsOn: build_ui_tests_coreclr\r?$'
      $lane | Should -Match 'template: ui-tests-steps\.yml'
      $lane | Should -Match 'runtimeVariant: "CoreCLR"'
      $lane | Should -Not -Match 'ui-tests-build-sample|useMaterial3:|cakeArgs:'
    }
    $uiTestSteps | Should -Match (
      "(?s)eq\('\$\{\{ parameters.runtimeVariant \}\}' , 'CoreCLR'\).*?" +
      "eq\('\$\{\{ parameters.useMaterial3 \}\}', 'false'\).*?" +
      'artifact: ui-tests-samples-coreclr')
  }

  It 'runs only Shell with the runtime renderer configuration in the extra lane' {
    $rendererLane | Should -Match 'testFilter: "Shell"'
    $rendererLane | Should -Match 'testConfigurationArgs: "ShellRenderer"'
    $rendererLane | Should -Not -Match 'strategy:|matrix:'
    $rendererLane | Should -Match 'uitest-snapshot-results-android-shell-renderer-'
  }

  It 'honors the same Android projects, API levels, and exclusions' {
    foreach ($lane in @($mainLane, $rendererLane)) {
      $lane | Should -Match 'each project in parameters\.projects'
      $lane | Should -Match "if ne\(project.android, ''\)"
      $lane | Should -Match 'each api in parameters\.androidApiLevels'
      $lane | Should -Match 'not\(containsValue\(project.androidApiLevelsExclude, api\)\)'
      $lane | Should -Match 'pool: \$\{\{ parameters\.androidLinuxPool \}\}'
      $lane | Should -Match 'android-emulator-32_'
      $lane | Should -Match 'android-emulator-64_'
    }
  }

  It 'forwards the runtime configuration to the Android test process' {
    $uiTestSteps | Should -Match '--TEST_CONFIGURATION_ARGS='
    $androidCake | Should -Match (
      '(?s)void ExecuteUITests\(.*?' +
      'Argument\("TEST_CONFIGURATION_ARGS", EnvironmentVariable\("TEST_CONFIGURATION_ARGS"\) \?\? ""\).*?' +
      'SetEnvironmentVariable\("TEST_CONFIGURATION_ARGS", TEST_CONFIGURATION_ARGS\).*?' +
      'RunTestWithLocalDotNet')
  }
}
