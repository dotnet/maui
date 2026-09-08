#Requires -Modules Pester

Describe 'ci-official-release.yml' {
  BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot 'ci-official-release.yml'
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
  }

  It 'gathers NuGet package assets without downloading symbol blobs' {
    $assignmentPattern = '(?m)^\s*' +
      [regex]::Escape('$packageAssetFilter') +
      " = '([^']+)'\s*`$"
    $filterMatch = [regex]::Match(
      $pipeline,
      $assignmentPattern)

    $filterMatch.Success | Should -BeTrue
    $packageAssetFilter = $filterMatch.Groups[1].Value
    'Microsoft.Maui.Controls.Foldable' | Should -Match $packageAssetFilter
    'assets/symbols/dotnet-maui/Microsoft.Maui.Controls.Foldable.symbols.nupkg' |
      Should -Not -Match $packageAssetFilter
  }

  It 'keeps the filtered gather fail-fast' {
    $gatherCommands = [regex]::Matches($pipeline, '(?m)^\s*& \$darc gather-drop.*$')

    $gatherCommands.Count | Should -Be 1
    $gatherCommands[0].Value | Should -Match '--asset-filter \$packageAssetFilter'
    $gatherCommands[0].Value | Should -Not -Match '--continue-on-error'
    $pipeline | Should -Match '(?s)& \$darc gather-drop.*?if \(\$LASTEXITCODE -ne 0\) \{\s*throw'
  }

  It 'rejects duplicate package file names before copying release artifacts' {
    $duplicateCheck = [regex]::Match(
      $pipeline,
      '(?s)\$fileNameDuplicates = @\(\$identities \| Group-Object fileName.*?' +
      'throw "[^"]*duplicate package file names.*?' +
      '\$packageSet\.Packages \| Copy-Item')

    $duplicateCheck.Success | Should -BeTrue
  }
}
