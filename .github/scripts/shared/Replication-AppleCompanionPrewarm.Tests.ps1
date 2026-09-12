#!/usr/bin/env pwsh
#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.9.0' }

BeforeAll {
    $script:HelperPath = Join-Path $PSScriptRoot 'Replication-AppleCompanionPrewarm.ps1'
    . $script:HelperPath
    $script:ScratchRoot = Join-Path $PSScriptRoot (
        ".apple-companion-prewarm-tests-$PID-$([guid]::NewGuid().ToString('N'))")
    New-Item -ItemType Directory -Path $script:ScratchRoot | Out-Null

    function New-ApplePrewarmTestRoots {
        param([Parameter(Mandatory = $true)][string]$Name)

        $root = Join-Path $script:ScratchRoot $Name
        $repository = Join-Path $root 'repository'
        $trusted = Join-Path $root 'trusted'
        $project = Join-Path $repository (
            'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj')
        $entitlements = Join-Path $trusted (
            'source-overrides/ReplicationMacCatalystControlsDeviceTests.entitlements')
        $assets = Join-Path $repository (
            'artifacts/obj/Controls.DeviceTests/project.assets.json')
        foreach ($parent in @(
                (Split-Path -Parent $project),
                (Split-Path -Parent $entitlements),
                (Split-Path -Parent $assets))) {
            New-Item -ItemType Directory -Path $parent -Force | Out-Null
        }
        Set-Content -LiteralPath $project -Value '<Project />' -Encoding utf8NoBOM
        Set-Content -LiteralPath $entitlements -Value '<plist />' -Encoding utf8NoBOM
        return [pscustomobject]@{
            Root = $root
            Repository = $repository
            Trusted = $trusted
            Project = $project
            Entitlements = $entitlements
            Assets = $assets
            Packages = Join-Path $root 'packages'
        }
    }

    function Write-ApplePrewarmTestAssets {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [string[]]$Targets = @(
                'net10.0-ios',
                'net10.0-ios/iossimulator-arm64',
                'net10.0-maccatalyst',
                'net10.0-maccatalyst/maccatalyst-arm64'),
            [string[]]$OriginalTargetFrameworks = @(
                'net10.0-ios',
                'net10.0-maccatalyst'),
            [string[]]$Frameworks = @(
                'net10.0-ios',
                'net10.0-maccatalyst')
        )

        $targetTable = [ordered]@{}
        foreach ($target in $Targets) {
            $targetTable[$target] = @{}
        }
        $frameworkTable = [ordered]@{}
        foreach ($framework in $Frameworks) {
            $frameworkTable[$framework] = @{
                targetAlias = $framework
            }
        }
        $document = [ordered]@{
            version = 3
            targets = $targetTable
            project = [ordered]@{
                restore = [ordered]@{
                    originalTargetFrameworks = @($OriginalTargetFrameworks)
                }
                frameworks = $frameworkTable
            }
        }
        [IO.File]::WriteAllText(
            $Path,
            (($document | ConvertTo-Json -Depth 8) + "`n"),
            [Text.UTF8Encoding]::new($false))
    }
}

AfterAll {
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Closed dual-Apple companion prewarm plan' {
    It 'returns one graph restore followed by exact no-restore iOS and Catalyst builds' {
        $case = New-ApplePrewarmTestRoots -Name 'plan'
        $plan = Get-ReplicationAppleCompanionPrewarmPlan `
            -RepositoryRoot $case.Repository `
            -TrustedRoot $case.Trusted `
            -PackagesPath $case.Packages

        @($plan.Commands).Count | Should -Be 3
        @($plan.Commands.Name) | Should -Be @(
            'restore-dual-apple-graph',
            'build-ios-simulator-no-restore',
            'build-maccatalyst-no-restore')
        $restore = @($plan.Commands)[0].Arguments
        $ios = @($plan.Commands)[1].Arguments
        $catalyst = @($plan.Commands)[2].Arguments

        $restore[0] | Should -BeExactly 'restore'
        $restore | Should -Contain '--force-evaluate'
        $restore | Should -Contain '-p:IncludeIosTargetFrameworks=true'
        $restore | Should -Contain '-p:IncludeMacCatalystTargetFrameworks=true'
        $restore | Should -Contain '-p:IncludeAndroidTargetFrameworks=false'
        $restore | Should -Not -Contain '--no-dependencies'
        $restore | Should -Not -Contain '-r'
        ($restore -join "`n") | Should -Not -Match 'RuntimeIdentifiers'

        $ios[0] | Should -BeExactly 'build'
        $ios | Should -Contain '--no-restore'
        $ios | Should -Contain 'net10.0-ios'
        $ios | Should -Contain 'iossimulator-arm64'
        $ios | Should -Contain '-p:CodesignRequireProvisioningProfile=false'
        ($ios -join "`n") | Should -Not -Match (
            'CodesignEntitlements|CodesignKey|MtouchDebug|UseSystemResourceKeys|' +
            'ValidateXcodeVersion|SkipXcodeVersionCheck')

        $catalyst[0] | Should -BeExactly 'build'
        $catalyst | Should -Contain '--no-restore'
        $catalyst | Should -Contain 'net10.0-maccatalyst'
        $catalyst | Should -Contain 'maccatalyst-arm64'
        $catalyst | Should -Contain '-p:CodesignRequireProvisioningProfile=false'
        $catalyst | Should -Contain '-p:CodesignKey=-'
        $catalyst | Should -Contain '-p:MtouchDebug=false'
        $catalyst | Should -Contain '-p:UseSystemResourceKeys=false'
        $catalyst | Should -Contain "-p:CodesignEntitlements=$($case.Entitlements)"
        ($catalyst -join "`n") | Should -Not -Match (
            'ValidateXcodeVersion|SkipXcodeVersionCheck')
        @($plan.Commands | Where-Object {
                $_.Name -ne 'restore-dual-apple-graph' -and
                $_.Arguments -notcontains '--no-restore'
            }).Count | Should -Be 0
        $plan.AssetsPath | Should -BeExactly $case.Assets
    }

    It 'has no platform source fixture command or signing overrides' {
        $parameters = (Get-Command Get-ReplicationAppleCompanionPrewarmPlan).Parameters
        @($parameters.Keys | Where-Object {
                $_ -match 'Platform|Source|Fixture|Command|Framework|RuntimeIdentifier'
            }).Count | Should -Be 0
    }
}

Describe 'Strict dual-Apple assets validation' {
    It 'accepts both plain and RID targets plus both framework declarations' {
        $case = New-ApplePrewarmTestRoots -Name 'valid-assets'
        Write-ApplePrewarmTestAssets -Path $case.Assets

        $summary = Assert-ReplicationAppleCompanionAssets `
            -RepositoryRoot $case.Repository

        $summary.AssetsSha256 | Should -Match '^[0-9a-f]{64}$'
        @($summary.TargetPairs) | Should -Be @(
            'net10.0-ios/iossimulator-arm64',
            'net10.0-maccatalyst/maccatalyst-arm64')
        @($summary.OriginalTargetFrameworks) | Should -Be @(
            'net10.0-ios',
            'net10.0-maccatalyst')
        @($summary.Frameworks) | Should -Be @(
            'net10.0-ios',
            'net10.0-maccatalyst')
    }

    It 'binds versioned framework keys to their exact target aliases' {
        $case = New-ApplePrewarmTestRoots -Name 'versioned-assets'
        $targets = @(
            'net10.0-ios26.0',
            'net10.0-ios26.0/iossimulator-arm64',
            'net10.0-maccatalyst26.0',
            'net10.0-maccatalyst26.0/maccatalyst-arm64')
        $frameworkTable = [ordered]@{
            'net10.0-ios26.0' = @{ targetAlias = 'net10.0-ios' }
            'net10.0-maccatalyst26.0' = @{
                targetAlias = 'net10.0-maccatalyst'
            }
        }
        $targetTable = [ordered]@{}
        foreach ($target in $targets) {
            $targetTable[$target] = @{}
        }
        $document = [ordered]@{
            version = 3
            targets = $targetTable
            project = [ordered]@{
                restore = [ordered]@{
                    originalTargetFrameworks = @(
                        'net10.0-ios',
                        'net10.0-maccatalyst')
                }
                frameworks = $frameworkTable
            }
        }
        [IO.File]::WriteAllText(
            $case.Assets,
            (($document | ConvertTo-Json -Depth 8) + "`n"),
            [Text.UTF8Encoding]::new($false))

        $summary = Assert-ReplicationAppleCompanionAssets `
            -RepositoryRoot $case.Repository
        @($summary.TargetPairs) | Should -Be @(
            'net10.0-ios26.0/iossimulator-arm64',
            'net10.0-maccatalyst26.0/maccatalyst-arm64')
    }

    It 'rejects a missing iOS runtime target' {
        $case = New-ApplePrewarmTestRoots -Name 'missing-ios'
        $missing = 'net10.0-ios/iossimulator-arm64'
        $targets = @(
            'net10.0-ios',
            'net10.0-ios/iossimulator-arm64',
            'net10.0-maccatalyst',
            'net10.0-maccatalyst/maccatalyst-arm64') |
            Where-Object { $_ -cne $missing }
        Write-ApplePrewarmTestAssets -Path $case.Assets -Targets $targets

        {
            Assert-ReplicationAppleCompanionAssets `
                -RepositoryRoot $case.Repository
        } | Should -Throw "*missing required target '$missing'*"
    }

    It 'rejects a missing Catalyst runtime target' {
        $case = New-ApplePrewarmTestRoots -Name 'missing-catalyst'
        $missing = 'net10.0-maccatalyst/maccatalyst-arm64'
        $targets = @(
            'net10.0-ios',
            'net10.0-ios/iossimulator-arm64',
            'net10.0-maccatalyst')
        Write-ApplePrewarmTestAssets -Path $case.Assets -Targets $targets
        {
            Assert-ReplicationAppleCompanionAssets -RepositoryRoot $case.Repository
        } | Should -Throw "*missing required target '$missing'*"
    }

    It 'rejects a missing original framework declaration' {
        $case = New-ApplePrewarmTestRoots -Name 'missing-original-ios'
        Write-ApplePrewarmTestAssets `
            -Path $case.Assets `
            -OriginalTargetFrameworks @('net10.0-maccatalyst')

        {
            Assert-ReplicationAppleCompanionAssets `
                -RepositoryRoot $case.Repository
        } | Should -Throw '*missing required*framework*'
    }

    It 'rejects a missing framework alias' {
        $case = New-ApplePrewarmTestRoots -Name 'missing-framework-catalyst'
        Write-ApplePrewarmTestAssets -Path $case.Assets -Frameworks @('net10.0-ios')
        {
            Assert-ReplicationAppleCompanionAssets -RepositoryRoot $case.Repository
        } | Should -Throw '*missing required framework alias*'
    }

    It 'rejects framework declarations outside NuGet restore metadata' {
        $case = New-ApplePrewarmTestRoots -Name 'misplaced-original-frameworks'
        Write-ApplePrewarmTestAssets -Path $case.Assets
        $document = Get-Content -LiteralPath $case.Assets -Raw | ConvertFrom-Json -AsHashtable
        $document.project.originalTargetFrameworks = $document.project.restore.originalTargetFrameworks
        $document.project.Remove('restore')
        $document | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $case.Assets -Encoding utf8NoBOM
        {
            Assert-ReplicationAppleCompanionAssets -RepositoryRoot $case.Repository
        } | Should -Throw '*missing required*framework*'
    }

    It 'rejects malformed assets' {
        $case = New-ApplePrewarmTestRoots -Name 'malformed'
        Set-Content -LiteralPath $case.Assets -Value '{ nope' -Encoding utf8NoBOM
        {
            Assert-ReplicationAppleCompanionAssets `
                -RepositoryRoot $case.Repository
        } | Should -Throw '*valid bounded JSON*'
    }

    It 'rejects a symlinked assets file' {
        $case = New-ApplePrewarmTestRoots -Name 'symlink'
        $outside = Join-Path $case.Root 'outside-assets.json'
        Write-ApplePrewarmTestAssets -Path $outside
        New-Item -ItemType SymbolicLink -Path $case.Assets -Target $outside | Out-Null
        {
            Assert-ReplicationAppleCompanionAssets `
                -RepositoryRoot $case.Repository
        } | Should -Throw '*regular no-link*'
    }
}
