#Requires -Modules Pester

Describe 'ci-official.yml' {
  BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot 'ci-official.yml'
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
    $publicPipeline = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ci.yml') -Raw
    $macPoolPattern = '(?m)^- name: MacOSPool\r?\n(?:[ \t]+[^\r\n]*\r?\n|\r?\n)*'
    $macPool = [regex]::Match($pipeline, $macPoolPattern).Value
    $publicMacPool = [regex]::Match(
      [regex]::Match($publicPipeline, $macPoolPattern).Value,
      '(?m)^    public:\r?\n(?:[ \t]{6,}[^\r\n]*\r?\n)*').Value
  }

  It 'resolves the OneLoc mirror branch during template expansion' {
    $assignments = [regex]::Matches(
      $pipeline,
      '(?m)^\s*MirrorBranch:\s*(.+?)\s*$')

    $assignments.Count | Should -Be 1
    $assignments[0].Groups[1].Value |
      Should -Be '${{ replace(variables[''Build.SourceBranch''], ''refs/heads/'', '''') }}'
  }

  It 'uses the same MAUI macOS pool as public CI' {
    $publicName = [regex]::Match($publicMacPool, '(?m)^\s+name:\s*(\S+)').Groups[1].Value
    $publicName | Should -Be 'MAUI'
    $macPool | Should -Match "(?m)^    name: $([regex]::Escape($publicName))\r?$"
  }

  It 'requires the same Xcode capability as public CI' {
    $publicDemand = [regex]::Match(
      $publicMacPool,
      '(?m)^\s+- (xcode -equals \S+)').Groups[1].Value

    $publicDemand | Should -Be 'xcode -equals /Applications/Xcode_27.0.0-rc.app/Contents/Developer'
    $macPool | Should -Match '(?m)^    demands:\r?$'
    $macPool | Should -Match "(?m)^      - $([regex]::Escape($publicDemand))\r?$"
  }

  It 'does not specify a hosted vmImage for the self-hosted macOS pool' {
    $macPool | Should -Not -BeNullOrEmpty
    $macPool | Should -Not -Match '(?m)^\s+vmImage:'
  }

  It 'retains the macOS operating system for the official pipeline template' {
    $macPool | Should -Match '(?m)^    os: macOS\r?$'
  }

  It 'passes the caller macOS pool parameter through to the pack stage' {
    $packStage = [regex]::Match(
      $pipeline,
      '(?m)^    - template: /eng/pipelines/arcade/stage-pack.yml@self\r?\n(?:[ \t]{6,}[^\r\n]*\r?\n|\r?\n)*').Value

    $packStage | Should -Match '(?m)^        macPool: \$\{\{ parameters\.MacOSPool \}\}\r?$'
  }
}
