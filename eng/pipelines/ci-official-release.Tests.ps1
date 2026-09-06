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

  It 'keeps workload gathering fail-fast and validates non-workload misses' {
    $gatherCommands = [regex]::Matches($pipeline, '(?m)^\s*& \$darc @gatherArguments.*$')

    $gatherCommands.Count | Should -Be 1
    $pipeline | Should -Match ([regex]::Escape("'--asset-filter', `$packageAssetFilter"))
    $pipeline | Should -Match '(?s)if \(!\$releaseWorkload\) \{\s*\$gatherArguments \+= ''--continue-on-error'''
    $pipeline | Should -Match '(?s)& \$darc @gatherArguments.*?if \(\$gatherFailed -and \$releaseWorkload\) \{\s*throw'
    $pipeline | Should -Match 'missing selected packages must already exist on NuGet\.org'
    $pipeline | Should -Match '(?s)if \(\$gatherFailed\) \{.*?Write-Warning.*?\$LASTEXITCODE = 0'
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

  It 'preserves first-match BAR resolution for every repository' {
    $pipeline | Should -Not -Match '(?m)^- name: barBuildId$'
    $pipeline | Should -Match '--repo \$sourceRepository --commit'
    $pipeline | Should -Match '\$build = \$builds\[0\]'
    $pipeline | Should -Not -Match 'Expected exactly one BAR build'
    $pipeline | Should -Not -Match 'requiredChannel(Name|Id)|build\.channels|5172'
  }

  It 'publishes a generic audit before approval and supports dry runs' {
    $pipeline | Should -Match 'artifactName: NuGetReleaseAudit'
    $nonWorkloadTemplate | Should -Match '(?s)dependsOn: prepare_release.*ManualValidation@0'
    $pipeline | Should -Match "(?s)eq\(parameters\.pushPackages, true\).*non-workload-publish\.yml"
    $nonWorkloadTemplate | Should -Match '(?s)-Action FilterExisting.*?1ES\.PublishNuget@1'
    $nonWorkloadTemplate | Should -Match "eq\(variables\['NuGetPackagesToPublish'\], 'true'\)"
    $nonWorkloadTemplate | Should -Match '1ES\.PublishNuget@1'
    $nonWorkloadTemplate | Should -Match "publishFeedCredentials: 'nuget\.org \(dotnetframework\)'"
    $pipeline | Should -Not -Match '(?m)^- name: nugetPublishServiceConnection$'
    $pipeline | Should -Not -Match '(?m)^- name: nugetAlreadyAttemptedNonWorkloadFilters$'
    $pipeline | Should -Not -Match 'HasPackages'
    $nonWorkloadTemplate | Should -Not -Match 'HasPackages'
  }

  It 'uses the original repository text inputs and rejects workload manifests on the inferred non-workload path' {
    $pipeline | Should -Match '(?s)- name: ghRepo.*?default: maui'
    $pipeline | Should -Match '(?s)- name: ghOwner.*?default: dotnet'
    $pipeline | Should -Not -Match '(?s)- name: ghRepo.*?values:'
    $pipeline | Should -Match 'Repository.*is not enabled for this release pipeline'
    $pipeline | Should -Match "(?m)^\s*'https://github\.com/dotnet/maui',\r?$"
    $pipeline | Should -Match "(?m)^\s*'https://github\.com/dotnet/android',\r?$"
    $pipeline | Should -Match "(?m)^\s*'https://github\.com/dotnet/macios',\r?$"
    $pipeline | Should -Match "(?m)^\s*'https://github\.com/dotnet/android-libraries'\r?$"
    $pipeline | Should -Match 'non-workload release cannot contain workload manifest'
    $pipeline | Should -Match '(?s)Name = ''NuGet packages''.*?Packages = \$selectedPackages.*?Identities = \$selectedIdentities'
    $pipeline | Should -Match '(?s)-Action FilterExisting.*?selectedPackages.*?stagedPackages'
    $pipeline | Should -Match 'belongs to ''\$repository'', not ''\$sourceRepository'''
    $pipeline | Should -Match '(?s)\$barPackageAssets = @\(\$build\.assets.*?\$selectedBarAssets = @\(\$barPackageAssets'
    $pipeline | Should -Match 'Gathered packages are absent from the selected BAR assets'
  }

  It 'supports package selection and rejects workload recovery filters for non-workload releases' {
    $pipeline | Should -Match '(?s)\$selectedPackages = @\(\$allPackages \| Where-Object \{.*?' +
      'Test-AnyFilter \$_.Name \$includeFilters.*?' +
      'Test-AnyFilter \$_.Name \$excludeFilters.*?' +
      'Package filtering selected no non-workload NuGet packages'
    $pipeline | Should -Match '(?s)else \{.*?\$unsupportedFilters = @\(@\(.*?' +
      'nugetAlreadyAttemptedPackFilters.*?nugetAlreadyAttemptedManifestFilters.*?' +
      '\) \| Where-Object \{ \$_.Values.Count -gt 0 \}\).*?' +
      'Non-workload releases do not support workload recovery filters'
    $pipeline | Should -Match 'NUGET_ALREADY_ATTEMPTED_PACK_FILTERS: \$\{\{ parameters\.nugetAlreadyAttemptedPackFilters \}\}'
    $pipeline | Should -Match 'NUGET_ALREADY_ATTEMPTED_MANIFEST_FILTERS: \$\{\{ parameters\.nugetAlreadyAttemptedManifestFilters \}\}'
  }
}
