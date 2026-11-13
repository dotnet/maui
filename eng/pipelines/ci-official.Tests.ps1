#Requires -Modules Pester

Describe 'ci-official.yml' {
  BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot 'ci-official.yml'
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
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
