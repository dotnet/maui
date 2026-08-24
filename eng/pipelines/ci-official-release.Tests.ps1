#Requires -Modules Pester

Describe 'ci-official-release.yml' {
  BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot 'ci-official-release.yml'
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
    $nonWorkloadTemplate = Get-Content -LiteralPath (
      Join-Path $PSScriptRoot 'common/non-workload-publish.yml') -Raw
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

  It 'infers workload releases from the workload repositories' {
    $pipeline | Should -Not -Match '(?m)^- name: releaseWorkload$'
    @([regex]::Matches($pipeline, "in\(parameters\.ghRepo, 'android', 'macios', 'maui'\)")).Count |
      Should -Be 1
    $pipeline | Should -Match '(?s)- name: isWorkload.*?value: \$\{\{ and'
    $pipeline | Should -Match '\$releaseWorkload = \[bool\]::Parse\(\$env:IS_WORKLOAD\)'
    $pipeline | Should -Match '--skip-assets-publishing'
    $nonWorkloadTemplate | Should -Not -Match 'skip-assets-publishing|add-build-to-channel'
  }

  It 'requires commit resolution to return one BAR build for the repository' {
    $pipeline | Should -Not -Match '(?m)^- name: barBuildId$'
    $pipeline | Should -Match '--repo \$sourceRepository --commit'
    $pipeline | Should -Match 'Expected exactly one BAR build'
    $pipeline | Should -Match '\$requiredChannelId = if \(\$requiredChannelName\) \{ 5172 \}'
  }

  It 'publishes a generic audit before approval and supports dry runs' {
    $pipeline | Should -Match 'artifactName: NuGetReleaseAudit'
    $nonWorkloadTemplate | Should -Match '(?s)dependsOn: prepare_release.*ManualValidation@0'
    $pipeline | Should -Match "(?s)eq\(parameters\.pushPackages, true\).*non-workload-publish\.yml"
    $nonWorkloadTemplate | Should -Match '1ES\.PublishNuget@1'
    $nonWorkloadTemplate | Should -Match "publishFeedCredentials: 'nuget\.org \(dotnetframework\)'"
    $pipeline | Should -Not -Match '(?m)^- name: nugetPublishServiceConnection$'
    $pipeline | Should -Not -Match '(?m)^- name: nugetAlreadyAttemptedNonWorkloadFilters$'
  }

  It 'uses the original repository text inputs and rejects workload manifests on the inferred non-workload path' {
    $pipeline | Should -Match '(?s)- name: ghRepo.*?default: maui'
    $pipeline | Should -Match '(?s)- name: ghOwner.*?default: dotnet'
    $pipeline | Should -Not -Match '(?s)- name: ghRepo.*?values:'
    $pipeline | Should -Match 'Repository.*is not enabled for this release pipeline'
    $pipeline | Should -Match 'https://github\.com/dotnet/android'
    $pipeline | Should -Match 'https://github\.com/dotnet/macios'
    $pipeline | Should -Match 'non-workload release cannot contain workload manifest'
    $pipeline | Should -Match 'Name = ''NuGet packages''; Packages = \$allPackages'
    $pipeline | Should -Match '(?s)-Action FilterExisting.*?selectedPackages.*?stagedPackages'
    $pipeline | Should -Match 'belongs to ''\$repository'', not ''\$sourceRepository'''
  }

  It 'rejects workload-only filters for non-workload releases' {
    $pipeline | Should -Match '(?s)else \{.*?\$unsupportedFilters = @\(@\(.*?' +
      'nugetIncludeFilters.*?nugetExcludeFilters.*?' +
      'nugetAlreadyAttemptedPackFilters.*?nugetAlreadyAttemptedManifestFilters.*?' +
      '\) \| Where-Object \{ \$_.Values.Count -gt 0 \}\).*?' +
      'Non-workload releases do not support workload package filters'
    $pipeline | Should -Match 'NUGET_ALREADY_ATTEMPTED_PACK_FILTERS: \$\{\{ parameters\.nugetAlreadyAttemptedPackFilters \}\}'
    $pipeline | Should -Match 'NUGET_ALREADY_ATTEMPTED_MANIFEST_FILTERS: \$\{\{ parameters\.nugetAlreadyAttemptedManifestFilters \}\}'
  }
}
