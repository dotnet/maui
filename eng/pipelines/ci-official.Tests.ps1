#Requires -Modules Pester

Describe 'ci-official.yml' {
  BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot 'ci-official.yml'
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
    $macPool = [regex]::Match(
      $pipeline, '(?ms)^- name: MacOSPool\r?\n.*?(?=^\S|\z)').Value
    $packStage = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'arcade/stage-pack.yml') -Raw
    $macPackJob = @([regex]::Matches(
      $packStage, '(?ms)^  - template:.*?(?=^  - template:|\z)') |
      Where-Object { $_.Value -match '(?m)^      - job: pack_net_macOS\r?$' })
    $sharedVariables = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/variables.yml') -Raw
    $provisionSteps = [regex]::Matches(
      $pipeline,
      '(?ms)^        - template: /eng/pipelines/common/provision\.yml@self\r?\n.*?(?=^    \S|\z)')
  }

  It 'uses the AcesShared pool with the GoldenGate image demand' {
    $macPool | Should -Match '(?m)^    name: AcesShared\r?$'
    $macPool | Should -Match (
      '(?m)^    demands:\r?\n      - ImageOverride -equals ACES_VM_SharedPool_GoldenGate\r?$')
  }

  It 'does not fall back to a hosted macOS pool or image' {
    $macPool | Should -Not -Match '(?m)^\s*vmImage:'
    $macPool | Should -Not -Match 'Azure Pipelines|GitHub-hosted Agents'
    $macPool | Should -Not -Match ([regex]::Escape('$(HostedMacImage)'))
  }

  It 'retains the macOS operating system for the 1ES template' {
    $macPool | Should -Match '(?m)^    os: macOS\r?$'
  }

  It 'passes caller-provided MacOSPool overrides through to the macOS pack job' {
    $macPool | Should -Match '(?m)^  type: object\r?$'
    $macPool | Should -Match '(?m)^  default:\r?$'
    $pipeline | Should -Match (
      '(?m)^    - template: /eng/pipelines/arcade/stage-pack\.yml@self\r?\n' +
      '      parameters:\r?\n        pool: [^\r\n]+\r?\n        macPool: ' +
      [regex]::Escape('${{ parameters.MacOSPool }}') + '\r?$')
    $packStage | Should -Match '(?m)^- name: macPool\r?\n  type: object\r?$'
    $macPackJob.Count | Should -Be 1
    $macPackJob[0].Value | Should -Match (
      '(?m)^      pool: ' +
      [regex]::Escape('${{ parameters.macPool }}') + '\r?$')
  }

  It 'keeps the shared hosted macOS image default independent of official builds' {
    $sharedVariables | Should -Match (
      '(?m)^- name: hostedMacImage\r?\n  type: string\r?\n  default: macOS-26\r?$')
    $sharedVariables | Should -Match (
      '(?m)^- name: HostedMacImage\r?\n  value: ' +
      [regex]::Escape('${{ parameters.hostedMacImage }}') + '\r?$')
  }

  It 'uses the pool-selected Xcode without changing the provisioning bypass' {
    $provisionSteps.Count | Should -Be 1
    $provisionSteps[0].Value | Should -Match '(?m)^            skipXcode: true\r?$'
    $provisionSteps[0].Value | Should -Match '(?m)^            skipProvisioning: true\r?$'
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
