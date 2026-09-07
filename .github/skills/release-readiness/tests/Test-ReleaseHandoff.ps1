#!/usr/bin/env pwsh
#Requires -Version 7.0
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$script:passed = 0
$script:failed = 0

function Assert-HandoffEqual {
    param([string]$Label, $Expected, $Actual)

    if ($Expected -eq $Actual) {
        Write-Host "  PASS $Label" -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host "  FAIL $Label" -ForegroundColor Red
        Write-Host "       expected: $Expected" -ForegroundColor DarkRed
        Write-Host "       actual:   $Actual" -ForegroundColor DarkRed
        $script:failed++
    }
}

function Assert-HandoffTrue {
    param([string]$Label, [bool]$Actual)
    Assert-HandoffEqual -Label $Label -Expected $true -Actual $Actual
}

$scripts = Join-Path $PSScriptRoot '..' 'scripts'
. (Join-Path $scripts 'PreviewInstallability.ps1')
. (Join-Path $scripts 'New-ReleaseHandoff.ps1') -ReadinessJson 'dot-source-test'

Write-Host "`n[Unit] Preview install source mapping" -ForegroundColor Cyan
$sources = @(ConvertFrom-PreviewPackageSourceSpec -Major 11)
$sourceNames = @($sources | ForEach-Object Name)
Assert-HandoffTrue -Label 'Preview 11 includes the prior-major compatibility feed' `
    -Actual ($sourceNames -contains 'dotnet10')
$resolvedSources = @($sources | ForEach-Object { [PSCustomObject]@{ Source = $_ } })
$compatibilityOrder = @(Get-PreviewSourceOrder `
    -ResolvedSources $resolvedSources `
    -PackageId 'Microsoft.MacCatalyst.Sdk.net10.0_27.0')
Assert-HandoffTrue -Label 'Prior-major compatibility feed participates in package resolution' `
    -Actual (@($compatibilityOrder | ForEach-Object { $_.Source.Name }) -contains 'dotnet10')

$config = ConvertTo-IsolatedNuGetConfig -Sources $sources
[xml]$configXml = $config
$workloadMapping = @($configXml.configuration.packageSourceMapping.packageSource |
    Where-Object { $_.key -eq 'dotnet-workloads' })[0]
$productMapping = @($configXml.configuration.packageSourceMapping.packageSource |
    Where-Object { $_.key -eq 'dotnet11' })[0]
Assert-HandoffEqual -Label 'Workload feed is mapped only to workload-set packages' `
    -Expected 'Microsoft.NET.Workloads.*' -Actual ([string]$workloadMapping.package.pattern)
Assert-HandoffEqual -Label 'Component feeds remain eligible for dependency packages' `
    -Expected '*' -Actual ([string]$productMapping.package.pattern)
$fallbackConfig = ConvertTo-IsolatedNuGetConfig -Sources $sources -WorkloadSetSourceName 'dotnet11'
[xml]$fallbackConfigXml = $fallbackConfig
$fallbackProductPatterns = @(
    @($fallbackConfigXml.configuration.packageSourceMapping.packageSource |
        Where-Object { $_.key -eq 'dotnet11' })[0].package |
        ForEach-Object { [string]$_.pattern }
)
Assert-HandoffTrue -Label 'Resolver-selected product feed can supply the workload-set package' `
    -Actual ($fallbackProductPatterns -contains 'Microsoft.NET.Workloads.*')
Assert-HandoffTrue -Label 'Resolver-selected product feed remains eligible for component packages' `
    -Actual ($fallbackProductPatterns -contains '*')

Write-Host "`n[Unit] Preview handoff projection" -ForegroundColor Cyan
$previewReadiness = [PSCustomObject]@{
    GeneratedAt = '2026-08-04T15:06:14Z'
    Branch = 'release/11.0.1xx-preview7'
    BranchType = 'preview'
    MajorVersion = 11
    PreviewNumber = 7
    Mode = 'in-flight'
    OverallStatus = 'WATCH'
    Verdict = 'Conditionally Ready'
    Checks = @(
        [PSCustomObject]@{
            Area = 'Target branch'; Status = 'READY'; Details = 'exists'; NextAction = 'none'
        },
        [PSCustomObject]@{
            Area = 'Insertion'
            Status = 'WATCH'
            Details = 'Contact captain@example.com at https://private.example.invalid/page?token=secret-value'
            NextAction = 'token=do-not-publish'
        }
    )
}

$publicNuGetConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="workload-set" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-workloads/nuget/v3/index.json" protocolVersion="3" />
    <add key="dotnet11" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet11/nuget/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="workload-set">
      <package pattern="Microsoft.NET.Workloads.*" />
    </packageSource>
    <packageSource key="dotnet11">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
'@

$previewEvidence = [PSCustomObject]@{
    PublicEvidence = $true
    ReleaseName = '.NET MAUI 11.0.0 Preview 7 Candidate'
    ReleaseVersion = '11.0.0-preview.7'
    Owner = 'private-owner-is-ignored'
    BreakingChanges = @(
        [PSCustomObject]@{
            Name = 'Handler behavior update'
            Status = 'Reviewed'
            Url = 'https://github.com/dotnet/maui/pull/12345'
            Notes = 'Public change record'
        }
    )
    Tests = @(
        [PSCustomObject]@{
            Name = 'Device tests'
            Status = 'Passed'
            Url = 'https://dev.azure.com/dnceng/public/_build/results?buildId=1539938'
            Notes = 'Selected-build run'
        }
    )
    Assessments = @(
        [PSCustomObject]@{
            Name = 'SDK insertion'
            Status = 'Ready'
            Url = 'https://github.com/dotnet/maui/issues/37000'
            Notes = 'Public tracking issue'
        }
    )
    Builds = @(
        [PSCustomObject]@{
            Name = 'MAUI official build'
            Version = '11.0.0-preview.7.26000.1'
            Build = '20260101.1 / 123456 / BAR 234567'
            Commit = '0123456789abcdef0123456789abcdef01234567'
            Status = 'Published'
            Url = 'https://dev.azure.com/dnceng/public/_build/results?buildId=123456'
        }
    )
    Rollback = [PSCustomObject]@{
        Status = 'Published'
        Url = 'https://aka.ms/dotnet/maui/preview7-rollback.json'
        Notes = 'Validated'
    }
    WorkloadSet = [PSCustomObject]@{
        CliVersion = '11.0.100-preview.7.26000.2'
        NuGetVersion = '11.100.0-preview.7.26000.2'
        ManifestVersion = '11.0.0-preview.7.26000.1'
        Status = 'Install verified'
        Notes = 'Clean isolated install passed'
        NuGetConfigPath = './release-nuget.config'
        NuGetConfig = $publicNuGetConfig
    }
    Sources = @(
        [PSCustomObject]@{
            Name = 'MAUI build'
            Url = 'https://dev.azure.com/dnceng/public/_build/results?buildId=123456'
        }
    )
}

$previewModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence $previewEvidence
$previewMarkdown = Format-ReleaseHandoffMarkdown -Model $previewModel
Assert-HandoffEqual -Label 'Preview report is recognized' -Expected 'preview' -Actual $previewModel.ReleaseType
Assert-HandoffEqual -Label 'Valid public release name is honored' `
    -Expected '.NET MAUI 11.0.0 Preview 7 Candidate' -Actual $previewModel.ReleaseName
Assert-HandoffTrue -Label 'Exact workload-set version is used in the generated command' `
    -Actual $previewModel.WorkloadSet.InstallCommand.Contains('11.0.100-preview.7.26000.2')
Assert-HandoffTrue -Label 'Public MAUI build link survives sanitization' `
    -Actual $previewMarkdown.Contains('buildId=123456')
Assert-HandoffEqual -Label 'Unmodeled owner field is ignored' -Expected $false `
    -Actual $previewMarkdown.Contains('private-owner-is-ignored')
Assert-HandoffEqual -Label 'Email address is removed' -Expected $false `
    -Actual $previewMarkdown.Contains('captain@example.com')
Assert-HandoffEqual -Label 'Non-public document URL is removed' -Expected $false `
    -Actual $previewMarkdown.Contains('private.example.invalid')
Assert-HandoffEqual -Label 'Secret-shaped value is removed' -Expected $false `
    -Actual $previewMarkdown.Contains('do-not-publish')
Assert-HandoffTrue -Label 'Public readiness prose is replaced with a fixed source-reference message' `
    -Actual $previewMarkdown.Contains('See the public source readiness report.')
Assert-HandoffEqual -Label 'Local readiness details are not carried into public handoff output' `
    -Expected $false -Actual $previewMarkdown.Contains('Contact captain')
$originalReleaseName = $previewEvidence.ReleaseName
$previewEvidence.ReleaseName = 'Owner: Private Person'
$unsafeReleaseNameModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence $previewEvidence
Assert-HandoffEqual -Label 'Unsafe public release name falls back to the derived name' `
    -Expected '.NET MAUI 11.0.0 Preview 7' -Actual $unsafeReleaseNameModel.ReleaseName
$previewEvidence.ReleaseName = $originalReleaseName

$previewReadiness | Add-Member -MemberType NoteProperty -Name ConsumerInstallability -Value ([PSCustomObject]@{
    PublicEvidence = $false
    VersionConfirmed = $true
    VersionSourceIsSensitive = $false
    CliVersion = '11.0.100-preview.7.26000.9'
    NuGetConfig = $publicNuGetConfig
})
$noProvenanceModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'Readiness installability without public provenance cannot supply a CLI version' `
    -Expected $null -Actual $noProvenanceModel.WorkloadSet.CliVersion
Assert-HandoffEqual -Label 'Readiness installability without public provenance cannot supply NuGet config' `
    -Expected $null -Actual $noProvenanceModel.WorkloadSet.NuGetConfig
$previewReadiness.ConsumerInstallability.PublicEvidence = 'true'
$stringProvenanceModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'String public provenance cannot supply a CLI version' `
    -Expected $null -Actual $stringProvenanceModel.WorkloadSet.CliVersion
$previewReadiness.ConsumerInstallability.PublicEvidence = $true
$previewReadiness.ConsumerInstallability.VersionConfirmed = $false
$unconfirmedVersionModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'Unconfirmed readiness installability cannot supply a CLI version' `
    -Expected $null -Actual $unconfirmedVersionModel.WorkloadSet.CliVersion
Assert-HandoffEqual -Label 'Unconfirmed readiness installability cannot supply NuGet config' `
    -Expected $null -Actual $unconfirmedVersionModel.WorkloadSet.NuGetConfig
$unconfirmedWithEvidenceConfigModel = New-ReleaseHandoffModel `
    -Readiness $previewReadiness `
    -Evidence ([PSCustomObject]@{
        PublicEvidence = $true
        WorkloadSet = [PSCustomObject]@{ NuGetConfig = $publicNuGetConfig }
    })
Assert-HandoffEqual -Label 'Unconfirmed readiness version plus public evidence config cannot produce an install command' `
    -Expected $null -Actual $unconfirmedWithEvidenceConfigModel.WorkloadSet.InstallCommand
$previewReadiness.ConsumerInstallability.VersionConfirmed = 'true'
$stringConfirmedModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'String version confirmation cannot supply a CLI version' `
    -Expected $null -Actual $stringConfirmedModel.WorkloadSet.CliVersion
$previewReadiness.ConsumerInstallability.VersionConfirmed = $true
$previewReadiness.ConsumerInstallability.VersionSourceIsSensitive = $true
$sensitiveProvenanceModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'Sensitive readiness installability cannot supply a CLI version' `
    -Expected $null -Actual $sensitiveProvenanceModel.WorkloadSet.CliVersion
Assert-HandoffEqual -Label 'Sensitive readiness installability cannot supply NuGet config' `
    -Expected $null -Actual $sensitiveProvenanceModel.WorkloadSet.NuGetConfig
$previewReadiness.ConsumerInstallability.PSObject.Properties.Remove('VersionSourceIsSensitive')
$missingSensitivityModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'Readiness installability without explicit non-sensitive provenance fails closed' `
    -Expected $null -Actual $missingSensitivityModel.WorkloadSet.CliVersion
$previewReadiness.ConsumerInstallability | Add-Member -MemberType NoteProperty `
    -Name VersionSourceIsSensitive -Value $false
foreach ($invalidSensitivity in @($null, 0, 0.0, '', @())) {
    $previewReadiness.ConsumerInstallability.VersionSourceIsSensitive = $invalidSensitivity
    $invalidSensitivityModel = New-ReleaseHandoffModel `
        -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
    Assert-HandoffEqual -Label "Non-boolean sensitivity '$invalidSensitivity' fails closed" `
        -Expected $null -Actual $invalidSensitivityModel.WorkloadSet.CliVersion
}
$previewReadiness.ConsumerInstallability.VersionSourceIsSensitive = $false
$publicProvenanceModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'Explicit public readiness installability can supply a CLI version' `
    -Expected '11.0.100-preview.7.26000.9' -Actual $publicProvenanceModel.WorkloadSet.CliVersion
Assert-HandoffTrue -Label 'Explicit public readiness installability can supply validated NuGet config' `
    -Actual (-not [string]::IsNullOrWhiteSpace($publicProvenanceModel.WorkloadSet.NuGetConfig))
$previewReadiness.PSObject.Properties.Remove('ConsumerInstallability')

$sectionOrder = @(
    '## Breaking Changes',
    '## Testing',
    '## Assessments and Insertion PRs',
    '## Builds & Releases',
    '## Rollback file',
    '## Workload Set',
    '## Release Readiness',
    '## Sources'
)
$previous = -1
$ordered = $true
foreach ($heading in $sectionOrder) {
    $current = $previewMarkdown.IndexOf($heading, [StringComparison]::Ordinal)
    if ($current -le $previous) { $ordered = $false }
    $previous = $current
}
Assert-HandoffTrue -Label 'Handoff sections use the documented order' -Actual $ordered

Write-Host "`n[Unit] SR handoff semantics and safe defaults" -ForegroundColor Cyan
$srReadiness = [PSCustomObject]@{
    metadata = [PSCustomObject]@{
        srBranch = 'release/10.0.1xx-sr10'
        mode = 'in-flight'
        fetchedAt = '2026-09-01T00:00:00Z'
    }
    verdict = [PSCustomObject]@{
        label = 'Not Ready'
        tier = 3
    }
    shipChecks = @(
        [PSCustomObject]@{
            Area = 'Ship Assessment'
            Status = 'BLOCKED'
            Details = 'Assessment is incomplete.'
            NextAction = 'Complete assessment.'
        }
    )
}
$credentialConfig = @'
<configuration>
  <packageSources>
    <add key="private" value="https://private.example.invalid/nuget/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <private><add key="ClearTextPassword" value="not-a-real-secret" /></private>
  </packageSourceCredentials>
</configuration>
'@
$srEvidence = [PSCustomObject]@{
    PublicEvidence = $true
    ReleaseVersion = '10.0.100'
    WorkloadSet = [PSCustomObject]@{
        CliVersion = '10.0.100'
        NuGetConfig = $credentialConfig
    }
    Sources = @(
        [PSCustomObject]@{
            Name = 'private source'
            Url = 'https://private.example.invalid/nuget/v3/index.json'
        }
    )
}
$srModel = New-ReleaseHandoffModel -Readiness $srReadiness -Evidence $srEvidence
$srMarkdown = Format-ReleaseHandoffMarkdown -Model $srModel
Assert-HandoffEqual -Label 'SR report is recognized' -Expected 'sr' -Actual $srModel.ReleaseType
Assert-HandoffTrue -Label 'SR10 title is derived from readiness metadata' `
    -Actual $srMarkdown.Contains('.NET MAUI 10.0 SR 10 Release Handoff')
Assert-HandoffTrue -Label 'Missing SR breaking-change review remains explicit TBD' `
    -Actual $srMarkdown.Contains('confirm that the servicing release contains no breaking changes')
Assert-HandoffEqual -Label 'Credential-bearing NuGet config is always rejected' `
    -Expected $null -Actual $srModel.WorkloadSet.NuGetConfig
Assert-HandoffEqual -Label 'No install command is emitted without safe mapped config' `
    -Expected $null -Actual $srModel.WorkloadSet.InstallCommand
Assert-HandoffEqual -Label 'Private feed URL is not emitted' -Expected $false `
    -Actual $srMarkdown.Contains('private.example.invalid')
Assert-HandoffTrue -Label 'Blocked SR check is retained in the handoff' `
    -Actual ($srMarkdown.Contains('Ship Assessment') -and $srMarkdown.Contains('source readiness report'))

$srCandidateReadiness = [PSCustomObject]@{
    trackerKey = 'net10-sr10'
    metadata = [PSCustomObject]@{
        srBranch = 'main'
        priorSrBranch = 'release/10.0.1xx-sr9'
        mode = 'candidate'
        fetchedAt = '2026-08-20T00:00:00Z'
    }
    verdict = [PSCustomObject]@{ label = 'Conditionally Ready'; tier = 2 }
    shipChecks = @()
}
$srCandidateModel = New-ReleaseHandoffModel -Readiness $srCandidateReadiness -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'SR candidate target branch is derived from tracker and prior SR' `
    -Expected 'release/10.0.1xx-sr10' -Actual $srCandidateModel.Branch
Assert-HandoffEqual -Label 'SR candidate title retains the target SR number' `
    -Expected '.NET MAUI 10.0 SR 10' -Actual $srCandidateModel.ReleaseName
$mismatchedCandidate = [PSCustomObject]@{
    trackerKey = 'net10-sr10'
    metadata = [PSCustomObject]@{
        srBranch = 'main'
        priorSrBranch = 'release/9.0.1xx-sr9'
        mode = 'candidate'
        fetchedAt = '2026-08-20T00:00:00Z'
    }
    verdict = [PSCustomObject]@{ label = 'Unknown'; tier = 3 }
    shipChecks = @()
}
$mismatchedCandidateModel = New-ReleaseHandoffModel `
    -Readiness $mismatchedCandidate -Evidence ([PSCustomObject]@{})
Assert-HandoffEqual -Label 'Mismatched SR tracker and prior branch do not fabricate a target branch' `
    -Expected $null -Actual $mismatchedCandidateModel.Branch

$unassertedEvidence = [PSCustomObject]@{
    ReleaseVersion = '11.0.0-preview.7.private'
    Builds = @([PSCustomObject]@{
        Name = 'Private build'
        Url = 'https://github.com/dotnet/maui/actions/runs/123'
        Status = 'Approved by Alice'
    })
}
$unassertedModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence $unassertedEvidence
Assert-HandoffEqual -Label 'Public-safe mode ignores evidence without PublicEvidence declaration' `
    -Expected 0 -Actual @($unassertedModel.Builds).Count
Assert-HandoffEqual -Label 'Unasserted release version remains unavailable' `
    -Expected $null -Actual $unassertedModel.ReleaseVersion
$stringFalseEvidence = [PSCustomObject]@{
    PublicEvidence = 'false'
    ReleaseVersion = '11.0.0-preview.7.private'
    Builds = @([PSCustomObject]@{
        Name = 'Private build'
        Url = 'https://github.com/dotnet/maui/actions/runs/123'
        Status = 'Approved'
    })
}
$stringFalseModel = New-ReleaseHandoffModel -Readiness $previewReadiness -Evidence $stringFalseEvidence
Assert-HandoffEqual -Label 'String false cannot satisfy the PublicEvidence boolean gate' `
    -Expected 0 -Actual @($stringFalseModel.Builds).Count

$configWithExtraXml = $publicNuGetConfig.Replace(
    '</configuration>',
    '  <apikeys><add key="https://example.invalid" value="hidden-value" /></apikeys></configuration>')
Assert-HandoffEqual -Label 'NuGet config with unrecognized secret-bearing XML is rejected' `
    -Expected $null -Actual (Get-HandoffNuGetConfig -Config $configWithExtraXml)
Assert-HandoffEqual -Label 'Absolute NuGet config path is rejected' `
    -Expected $null -Actual (Get-HandoffNuGetConfigPath -ConfigPath 'C:\Users\alice\release-nuget.config')
$configWithQuerySecret = $publicNuGetConfig.Replace(
    'dotnet11/nuget/v3/index.json',
    'dotnet11/nuget/v3/index.json?sig=do-not-publish')
Assert-HandoffEqual -Label 'NuGet source URL with query data is rejected' `
    -Expected $null -Actual (Get-HandoffNuGetConfig -Config $configWithQuerySecret)
$suffixedBuildUrl = ConvertTo-PublicHandoffText `
    'https://dev.azure.com/dnceng/public/_build/results?buildId=123456&sig=do-not-publish'
Assert-HandoffEqual -Label 'Public build URL with extra query data does not preserve the suffix' `
    -Expected $false -Actual $suffixedBuildUrl.Contains('do-not-publish')
$unsafeFieldEvidence = [PSCustomObject]@{
    PublicEvidence = $true
    ReleaseVersion = '11.0.0-preview.7'
    Builds = @([PSCustomObject]@{
        Name = 'Owner: Alice'
        Version = '/Users/alice/private-version'
        Build = 'token is do-not-publish'
        Commit = 'C:\Users\alice\private-commit'
        Status = 'Approved by Alice'
        Url = 'https://github.com/dotnet/maui/actions/runs/123'
    })
    Rollback = [PSCustomObject]@{
        Status = 'Published'
        Url = 'https://github.com/dotnet/maui?sig=do-not-publish'
    }
}
$unsafeFieldModel = New-ReleaseHandoffModel `
    -Readiness $previewReadiness -Evidence $unsafeFieldEvidence
Assert-HandoffEqual -Label 'Public-safe build name rejects owner prose' `
    -Expected $null -Actual $unsafeFieldModel.Builds[0].Name
Assert-HandoffEqual -Label 'Public-safe build version rejects local paths' `
    -Expected $null -Actual $unsafeFieldModel.Builds[0].Version
Assert-HandoffEqual -Label 'Public-safe build identifier rejects token prose' `
    -Expected $null -Actual $unsafeFieldModel.Builds[0].Build
Assert-HandoffEqual -Label 'Public-safe build identifier rejects PAT-shaped values' `
    -Expected $null -Actual (ConvertTo-HandoffStructuredText -Value ('ghp_' + ('A' * 36)) -Kind build)
Assert-HandoffEqual -Label 'Public-safe build commit rejects local paths' `
    -Expected $null -Actual $unsafeFieldModel.Builds[0].Commit
Assert-HandoffEqual -Label 'Public-safe build status rejects sign-off prose' `
    -Expected $null -Actual $unsafeFieldModel.Builds[0].Status
Assert-HandoffEqual -Label 'Rollback URL with query data is rejected' `
    -Expected $null -Actual $unsafeFieldModel.Rollback.Url
foreach ($unsafeUrl in @(
    'https://github.com/dotnet/maui/pull/1)[click](https://example.invalid/phish',
    'https://github.com/dotnet/maui/pull/1%3Ftoken=do-not-publish',
    'https://aka.ms/dotnet/maui/rollback%2529',
    'https://github.com/dotnet/maui/releases/tag/x|forged-cell',
    'https://github.com/dotnet/maui/releases/tag/x%7Cforged-cell',
    'https://pkgs.dev.azure.com/dnceng/public/%2f..%2finternal/feed'
)) {
    Assert-HandoffEqual -Label "Markdown or encoded-control URL is rejected: $unsafeUrl" `
        -Expected $false -Actual (Test-HandoffPublicUrl -Url $unsafeUrl)
}
$unsafeUrlEvidence = [PSCustomObject]@{
    PublicEvidence = $true
    ReleaseVersion = '11.0.0-preview.7'
    Builds = @([PSCustomObject]@{
        Name = 'MAUI build'
        Version = '11.0.0-preview.7.26000.1'
        Build = '123456'
        Commit = '0123456789abcdef0123456789abcdef01234567'
        Status = 'Published'
        Url = 'https://github.com/dotnet/maui/pull/1)[click](https://example.invalid/phish'
    })
}
$unsafeUrlModel = New-ReleaseHandoffModel `
    -Readiness $previewReadiness -Evidence $unsafeUrlEvidence
Assert-HandoffEqual -Label 'Evidence row with Markdown-structural URL is omitted' `
    -Expected 0 -Actual @($unsafeUrlModel.Builds).Count
$githubFeedConfig = $publicNuGetConfig.Replace(
    'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet11/nuget/v3/index.json',
    'https://raw.githubusercontent.com/attacker/feed/main/index.json')
Assert-HandoffEqual -Label 'Arbitrary GitHub content cannot be used as a NuGet source' `
    -Expected $null -Actual (Get-HandoffNuGetConfig -Config $githubFeedConfig)

$unsafeTimestampReadiness = $previewReadiness.PSObject.Copy()
$unsafeTimestampReadiness.GeneratedAt = '2026-08-04T15:06:14Z ![evil](https://example.invalid)'
$unsafeTimestampModel = New-ReleaseHandoffModel `
    -Readiness $unsafeTimestampReadiness -Evidence $previewEvidence
$unsafeTimestampMarkdown = Format-ReleaseHandoffMarkdown -Model $unsafeTimestampModel
Assert-HandoffEqual -Label 'Invalid readiness timestamp is not emitted' `
    -Expected $null -Actual $unsafeTimestampModel.ReadinessGeneratedAt
Assert-HandoffEqual -Label 'Readiness timestamp cannot inject Markdown' `
    -Expected $false -Actual $unsafeTimestampMarkdown.Contains('![evil]')

$markdownInjectionEvidence = [PSCustomObject]@{
    PublicEvidence = $true
    ReleaseVersion = '11.0.0-preview.7'
    BreakingChanges = @([PSCustomObject]@{
        Name = 'Change <img src=x onerror=alert(1)> [redirect](https://github.com/example)'
        Status = 'Reviewed **override**'
        Url = 'https://github.com/dotnet/maui/pull/12345'
    })
    Sources = @([PSCustomObject]@{
        Name = 'Source [redirect](https://github.com/example)'
        Url = 'https://github.com/dotnet/maui'
    })
}
$markdownInjectionModel = New-ReleaseHandoffModel `
    -Readiness $previewReadiness -Evidence $markdownInjectionEvidence
$markdownInjectionOutput = Format-ReleaseHandoffMarkdown -Model $markdownInjectionModel
Assert-HandoffEqual -Label 'Breaking-change HTML is escaped in bullet output' `
    -Expected $false -Actual $markdownInjectionOutput.Contains('<img')
Assert-HandoffEqual -Label 'Injected Markdown link syntax is not emitted from structured names' `
    -Expected $false -Actual $markdownInjectionOutput.Contains('[redirect](https://github.com/example)')

Write-Host "`n[Integration] Handoff entry point" -ForegroundColor Cyan
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) "maui-release-handoff-$([Guid]::NewGuid().ToString('N'))"
$outputDir = Join-Path $tempRoot 'output'
try {
    New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null
    $readinessPath = Join-Path $tempRoot 'preview-readiness.json'
    $evidencePath = Join-Path $tempRoot 'evidence.json'
    $previewReadiness | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $readinessPath -Encoding utf8
    $previewEvidence | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $evidencePath -Encoding utf8

    & pwsh -NoProfile -File (Join-Path $scripts 'New-ReleaseHandoff.ps1') `
        -ReadinessJson $readinessPath `
        -EvidenceJson $evidencePath `
        -OutputDir $outputDir
    Assert-HandoffEqual -Label 'Entry point exits successfully' -Expected 0 -Actual $LASTEXITCODE
    Assert-HandoffTrue -Label 'Entry point writes Markdown' `
        -Actual (Test-Path -LiteralPath (Join-Path $outputDir 'release-handoff.md'))
    Assert-HandoffTrue -Label 'Entry point writes normalized JSON' `
        -Actual (Test-Path -LiteralPath (Join-Path $outputDir 'release-handoff.json'))

    $arrayGateCases = @(
        @{
            Label = 'Evidence PublicEvidence singleton array'
            EvidencePublic = @($true)
            InstallPublic = $true
            Confirmed = $true
            Sensitive = $false
            ExpectEvidence = $false
        },
        @{
            Label = 'Installability PublicEvidence singleton array'
            EvidencePublic = $true
            InstallPublic = @($true)
            Confirmed = $true
            Sensitive = $false
            ExpectEvidence = $true
        },
        @{
            Label = 'VersionConfirmed singleton array'
            EvidencePublic = $true
            InstallPublic = $true
            Confirmed = @($true)
            Sensitive = $false
            ExpectEvidence = $true
        },
        @{
            Label = 'VersionSourceIsSensitive singleton array'
            EvidencePublic = $true
            InstallPublic = $true
            Confirmed = $true
            Sensitive = @($false)
            ExpectEvidence = $true
        }
    )
    foreach ($case in $arrayGateCases) {
        $caseReadiness = $previewReadiness.PSObject.Copy()
        if ($case.ExpectEvidence) {
            $caseReadiness | Add-Member -MemberType NoteProperty -Name ConsumerInstallability -Value ([PSCustomObject]@{
                PublicEvidence = $case.InstallPublic
                VersionConfirmed = $case.Confirmed
                VersionSourceIsSensitive = $case.Sensitive
                CliVersion = '11.0.100-preview.7.26000.9'
                NuGetConfig = $publicNuGetConfig
            })
        }
        $caseEvidence = [PSCustomObject]@{
            PublicEvidence = $case.EvidencePublic
            ReleaseVersion = '11.0.0-preview.7'
        }
        if (-not $case.ExpectEvidence) {
            $caseEvidence | Add-Member -MemberType NoteProperty -Name WorkloadSet -Value ([PSCustomObject]@{
                CliVersion = '11.0.100-preview.7.26000.9'
                NuGetConfig = $publicNuGetConfig
            })
        }
        $caseReadiness | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $readinessPath -Encoding utf8
        $caseEvidence | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $evidencePath -Encoding utf8
        & pwsh -NoProfile -File (Join-Path $scripts 'New-ReleaseHandoff.ps1') `
            -ReadinessJson $readinessPath `
            -EvidenceJson $evidencePath `
            -OutputDir $outputDir
        $caseOutput = Get-Content -LiteralPath (Join-Path $outputDir 'release-handoff.json') -Raw |
            ConvertFrom-Json
        Assert-HandoffEqual -Label "$($case.Label) does not emit readiness CLI version" `
            -Expected $null -Actual $caseOutput.WorkloadSet.CliVersion
        Assert-HandoffEqual -Label "$($case.Label) does not emit readiness NuGet config" `
            -Expected $null -Actual $caseOutput.WorkloadSet.NuGetConfig
        Assert-HandoffEqual -Label "$($case.Label) does not emit an install command" `
            -Expected $null -Actual $caseOutput.WorkloadSet.InstallCommand
        if (-not $case.ExpectEvidence) {
            Assert-HandoffEqual -Label "$($case.Label) does not admit top-level evidence" `
                -Expected $null -Actual $caseOutput.ReleaseVersion
        }
    }
} finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}

Write-Host "`n----------------------------------------" -ForegroundColor Cyan
Write-Host "Passed: $script:passed   Failed: $script:failed" -ForegroundColor $(if ($script:failed -eq 0) { 'Green' } else { 'Red' })
exit $(if ($script:failed -eq 0) { 0 } else { 1 })
