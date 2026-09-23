#Requires -Modules Pester

Describe 'ci-official.yml' {
  BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot 'ci-official.yml'
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
    $macPool = [regex]::Match(
      $pipeline, '(?ms)^- name: MacOSPool\r?\n.*?(?=^\S|\z)').Value
    $packStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-pack.yml') -Raw
    $sharedVariables = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/variables.yml') -Raw
  }

  It 'pairs the GitHub-hosted pool with the Xcode 27 ARM64 image' {
    $macPool | Should -Match '(?m)^    name: GitHub-hosted Agents\r?$'
    $macPool | Should -Match '(?m)^    vmImage: xcode-27\r?$'
  }

  It 'uses a hosted image without self-hosted Xcode capability demands' {
    $macPool | Should -Not -Match '(?m)^\s*demands:'
    $macPool | Should -Not -Match 'xcode -equals|ImageOverride'
  }

  It 'retains the macOS operating system for the 1ES template' {
    $macPool | Should -Match '(?m)^    os: macOS\r?$'
  }

  It 'passes caller-provided MacOSPool overrides through to the macOS pack job' {
    $macPool | Should -Match '(?m)^  type: object\r?$'
    $macPool | Should -Match '(?m)^  default:\r?$'
    $pipeline | Should -Match (
      '(?m)^        macPool: ' +
      [regex]::Escape('${{ parameters.MacOSPool }}') + '\r?$')
    $packStage | Should -Match '(?m)^- name: macPool\r?\n  type: object\r?$'
    $packStage | Should -Match (
      '(?m)^      pool: ' +
      [regex]::Escape('${{ parameters.macPool }}') + '\r?$')
  }

  It 'keeps the shared hosted macOS image default independent of official builds' {
    $sharedVariables | Should -Match (
      '(?m)^- name: hostedMacImage\r?\n  type: string\r?\n  default: macOS-26\r?$')
    $sharedVariables | Should -Match (
      '(?m)^- name: HostedMacImage\r?\n  value: ' +
      [regex]::Escape('${{ parameters.hostedMacImage }}') + '\r?$')
    $macPool | Should -Not -Match ([regex]::Escape('$(HostedMacImage)'))
  }

  It 'resolves the OneLoc mirror branch during template expansion' {
    $assignments = [regex]::Matches(
      $pipeline,
      '(?m)^\s*MirrorBranch:\s*(.+?)\s*$')

    $assignments.Count | Should -Be 1
    $assignments[0].Groups[1].Value |
      Should -Be '${{ replace(variables[''Build.SourceBranch''], ''refs/heads/'', '''') }}'
  }
}
