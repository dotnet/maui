#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Watch-MauiNightlyDelivery.ps1')
    $script:ConfiguredNightlyBranches = @($script:NightlyDeliveryBranches)
    $script:Now = [datetime]::new(2026, 9, 16, 18, 0, 0, [DateTimeKind]::Utc)
    $script:FeedRoot = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet10/nuget/v3'
    $script:Source = @{ Feed = 'dotnet10'; Prefix = '^10\.0\.[0-9]+-ci\.inflight\.' }

    function New-Leaf {
        param([string]$Version = '10.0.120-ci.inflight.26465.1', [datetime]$Published = $script:Now.AddHours(-1))
        @{ catalogEntry = @{ version = $Version; published = $Published.ToString('o'); listed = $true } }
    }
    function New-Delivery {
        param([string]$Result = 'stale', [int]$Hours = 31, [string]$Version = '10.0.120-ci.inflight.26465.1')
        $published = $script:Now.AddHours(-$Hours)
        @{ Sequence = ([datetimeoffset]$published).ToUnixTimeSeconds(); Key = $Version
           Version = $Version; Result = $Result; Published = $published }
    }
    function New-NightlyIssue {
        param([string]$State = 'open', [hashtable]$Observation = (New-Delivery))
        @{ number = 700; state = $State; user = @{ login = 'github-actions[bot]' }
           labels = @(@{ name = 'ci-branch-health' })
           body = Format-NightlyDeliveryObservation -Branch 'inflight/current' -Source $script:Source `
               -Observation $Observation -Now $script:Now -NewIssue }
    }
    function gh { throw 'Live GitHub access is forbidden in tests.' }
}

Describe 'Nightly stream discovery' {
    BeforeEach {
        $script:Props = @'
<Project><PropertyGroup>
<MajorVersion>10</MajorVersion><MinorVersion>0</MinorVersion>
<PreReleaseVersionLabel>ci.main</PreReleaseVersionLabel>
<PreReleaseVersionLabel Condition="'$(BUILD_SOURCEBRANCH)' == 'refs/heads/inflight/current'">ci.inflight</PreReleaseVersionLabel>
<PreReleaseVersionIteration></PreReleaseVersionIteration>
</PropertyGroup></Project>
'@
        Mock Invoke-BranchCiGitHub {
            param($Endpoint)
            if ($Endpoint -match 'matching-refs/heads/(.+)$') {
                return @(@{ ref = "refs/heads/$($Matches[1])"; object = @{ sha = 'a' * 40 } })
            }
            @{ encoding = 'base64'; content = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($script:Props)) }
        }
    }
    It 'keeps main and inflight families isolated within the same feed' {
        $main = Get-NightlyDeliverySource main
        $inflight = Get-NightlyDeliverySource inflight/current
        $main.Feed | Should -Be dotnet10
        '10.0.120-ci.main.26465.1' | Should -Match $main.Prefix
        '10.0.120-ci.main.26465.1' | Should -Not -Match $inflight.Prefix
        '10.0.120-ci.inflight.26465.1' | Should -Match $inflight.Prefix
        Should -Invoke Invoke-BranchCiGitHub -Times 2 -ParameterFilter { $Endpoint -match "/contents/eng/Versions.props\?ref=$('a' * 40)$" }
    }
    It 'follows the development branch preview iteration rather than a guessed family' {
        $script:Props = $script:Props.Replace('<MajorVersion>10', '<MajorVersion>11').
            Replace('>ci.main<', '>preview<').Replace('<PreReleaseVersionIteration>', '<PreReleaseVersionIteration>7')
        $source = Get-NightlyDeliverySource net11.0
        $source.Feed | Should -Be dotnet11
        '11.0.0-preview.7.26465.1' | Should -Match $source.Prefix
        '11.0.0-preview.6.26465.1' | Should -Not -Match $source.Prefix
    }
    It 'skips absent branches and does not mistake a similarly named ref for them' {
        Mock Invoke-BranchCiGitHub { @(@{ ref = 'refs/heads/net12.0-experiment' }) }
        Get-NightlyDeliverySource net12.0 | Should -BeNullOrEmpty
    }
    It 'rejects unsafe or ambiguous version settings: <Kind>' -ForEach @(
        @{ Kind = 'DTD' }, @{ Kind = 'MSBuild' }, @{ Kind = 'duplicate' }, @{ Kind = 'inflight override' }
    ) {
        switch ($Kind) {
            DTD { $script:Props = '<!DOCTYPE Project [<!ENTITY x SYSTEM "file:///etc/passwd">]>' + $script:Props }
            MSBuild { $script:Props = $script:Props.Replace('>10<', '>$(Something)<') }
            duplicate { $script:Props = $script:Props.Replace('</PropertyGroup>', '<MajorVersion>12</MajorVersion></PropertyGroup>') }
            'inflight override' { $script:Props = $script:Props.Replace('refs/heads/inflight/current', 'refs/heads/elsewhere') }
        }
        { Get-NightlyDeliverySource inflight/current } | Should -Throw
    }
}

Describe 'Nightly publication evidence' {
    BeforeEach {
        $script:Controls = @((New-Leaf))
        $script:Sdk = @((New-Leaf))
        $script:Paged = $false
        Mock Invoke-WebRequest {
            param($Uri)
            $url = [string]$Uri
            if ($url -eq "$script:FeedRoot/index.json") {
                $data = @{ resources = @(@{ '@type' = 'RegistrationsBaseUrl/3.6.0'; '@id' = "$script:FeedRoot/registrations2-semver2" }) }
            } else {
                $leaves = @(if ($url -match '/microsoft.maui.controls/') { $script:Controls } else { $script:Sdk })
                $page = @{ count = $leaves.Count; items = $leaves }
                if ($script:Paged -and $url.EndsWith('/index.json')) {
                    $data = @{ count = 1; items = @(@{ '@id' = $url.Replace('/index.json', '/page.json') }) }
                } elseif ($url.EndsWith('/page.json')) {
                    $data = $page
                } else {
                    $data = @{ count = 1; items = @($page) }
                }
            }
            @{ Content = $data | ConvertTo-Json -Depth 20 }
        }
    }
    It 'classifies the exact thirty-hour boundary: <AgeSeconds>' -ForEach @(
        @{ AgeSeconds = 107999; Expected = 'fresh' }
        @{ AgeSeconds = 108000; Expected = 'stale' }
        @{ AgeSeconds = 108001; Expected = 'stale' }
    ) {
        $script:Controls = $script:Sdk = @((New-Leaf -Published $script:Now.AddSeconds(-$AgeSeconds)))
        (Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now).Result | Should -Be $Expected
    }
    It 'requires the same version in both packages and retains the previous completed delivery' {
        $old = New-Leaf -Published $script:Now.AddHours(-31)
        $script:Controls = @($old, (New-Leaf -Version '10.0.120-ci.inflight.26466.1'))
        $script:Sdk = @($old)
        $result = Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now
        $result.Result | Should -Be stale
        $result.Version | Should -Be '10.0.120-ci.inflight.26465.1'
    }
    It 'measures delivery from completion of the package pair' {
        $script:Controls = @((New-Leaf -Published $script:Now.AddHours(-31)))
        $result = Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now
        $result.Result | Should -Be fresh
        $result.Published | Should -Be $script:Now.AddHours(-1)
    }
    It 'normalizes publication offsets before checking the deadline' {
        $script:Controls[0].catalogEntry.published = '2026-09-15T14:00:00+02:00'
        $script:Sdk[0].catalogEntry.published = '2026-09-15T07:00:00-05:00'
        $result = Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now
        $result.Result | Should -Be stale
        $result.Published | Should -Be $script:Now.AddHours(-30)
        $result.Published.Kind | Should -Be Utc
    }
    It 'orders by publication time, not semantic version' {
        $script:Controls = $script:Sdk = @(
            (New-Leaf -Version '10.0.999-ci.inflight.26465.1' -Published $script:Now.AddHours(-40))
            (New-Leaf -Version '10.0.120-ci.inflight.26466.1')
        )
        (Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now).Version | Should -Be '10.0.120-ci.inflight.26466.1'
    }
    It 'does not let fresh main packages mask an inflight outage' {
        $script:Controls = $script:Sdk = @(
            (New-Leaf -Published $script:Now.AddHours(-31))
            (New-Leaf -Version '10.0.120-ci.main.26466.1')
        )
        (Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now).Result | Should -Be stale
    }
    It 'reports missing delivery when no matching pair exists' {
        $script:Sdk = @((New-Leaf -Version '10.0.120-ci.inflight.26466.1'))
        (Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now).Result | Should -Be missing
    }
    It 'follows linked registration pages' {
        $script:Paged = $true
        (Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now).Result | Should -Be fresh
        Should -Invoke Invoke-WebRequest -Times 2 -ParameterFilter { ([string]$Uri).EndsWith('/page.json') }
    }
    It 'does not call a partial page read delivered or missing' {
        $script:Paged = $true
        Mock Invoke-WebRequest { throw 'page read failed' } -ParameterFilter { ([string]$Uri).EndsWith('/page.json') }
        { Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now } | Should -Throw '*Incomplete nightly publication evidence*'
    }
    It 'rejects incomplete or malformed feed data: <Kind>' -ForEach @(
        @{ Kind = 'empty' }, @{ Kind = 'count mismatch' }, @{ Kind = 'missing leaf' }
        @{ Kind = 'bad timestamp' }, @{ Kind = 'future timestamp' }, @{ Kind = 'unsafe version' }
    ) {
        switch ($Kind) {
            empty { $script:Controls = @() }
            'count mismatch' {
                Mock Invoke-WebRequest { @{ Content = '{"count":2,"items":[{"@id":"https://example.com/page"}]}' } }
            }
            'missing leaf' { $script:Controls = @(@{}) }
            'bad timestamp' { $script:Controls[0].catalogEntry.published = 'not a date' }
            'future timestamp' { $script:Controls[0].catalogEntry.published = $script:Now.AddHours(1).ToString('o') }
            'unsafe version' { $script:Controls[0].catalogEntry.version = '@someone <!-- injected -->' }
        }
        { Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now } | Should -Throw '*Incomplete nightly publication evidence*'
    }
    It 'ignores explicitly unlisted packages' {
        $script:Controls[0].catalogEntry.listed = $false
        $script:Controls[0].catalogEntry.published = '1900-01-01T00:00:00Z'
        (Get-NightlyDeliveryObservation -Source $script:Source -Now $script:Now).Result | Should -Be missing
    }
    It 'rejects registration links outside the expected public feed service' {
        { Get-NightlyDeliveryFeedJson -Url 'http://localhost/index.json' -Now $script:Now } | Should -Throw '*Unexpected NuGet registration URL*'
        Should -Invoke Invoke-WebRequest -Times 0
    }
    It 'keeps the shared helper default shape while exposing all versions opt-in' {
        $fetcher = { param($url) Get-NightlyDeliveryFeedJson -Url $url -Now $script:Now }
        $default = Get-NightlyFeedFreshness -Feed dotnet10 -Fetcher $fetcher
        $default.ContainsKey('versions') | Should -BeFalse
        $all = Get-NightlyFeedFreshness -Feed dotnet10 -Fetcher $fetcher -IncludeVersions
        $all.versions.Count | Should -Be 1
        $all.version | Should -Be $default.version
    }
}

Describe 'Nightly outage lifecycle' {
    BeforeEach {
        $script:NightlyDeliveryBranches = @('inflight/current')
        $script:Observation = New-Delivery
        $script:Issues = @()
        $script:Comments = @()
        $script:Writes = [Collections.Generic.List[object]]::new()
        $script:SavedSummaryPath = $env:GITHUB_STEP_SUMMARY
        $env:GITHUB_STEP_SUMMARY = Join-Path $TestDrive 'summary.md'
        Mock Get-NightlyDeliverySource { $script:Source }
        Mock Get-NightlyDeliveryObservation { $script:Observation }
        Mock Invoke-WebRequest { throw 'Live feed access is forbidden in tests.' }
        Mock Invoke-BranchCiGitHub {
            param($Endpoint, $Method = 'GET', $Body)
            if ($Method -ne 'GET') {
                $script:Writes.Add(@{ Method = $Method; Endpoint = $Endpoint; Body = $Body })
                return @{ number = 701 }
            }
            if ($Endpoint -match '/issues\?') { return $script:Issues }
            if ($Endpoint -match '/comments\?') { return $script:Comments }
            if ($Endpoint -match '/labels\?') { return @(@{ name = 'ci-branch-health' }) }
            throw "Unexpected endpoint $Endpoint"
        }
    }
    AfterEach { $env:GITHUB_STEP_SUMMARY = $script:SavedSummaryPath }

    It 'is read-only by default' {
        Invoke-NightlyDeliveryMonitor -Now $script:Now
        $script:Writes.Count | Should -Be 0
    }
    It 'creates an issue for an overdue nightly and pings kubaflo' {
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes.Count | Should -Be 1
        $script:Writes[0].Body.title | Should -Be '[nightly-delivery] inflight/current is overdue'
        $script:Writes[0].Body.body | Should -Match '@kubaflo'
        $script:Writes[0].Body.body | Should -Match '30 hours'
    }
    It 'does not create an issue for a fresh delivery' {
        $script:Observation = New-Delivery -Result fresh -Hours 2
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes.Count | Should -Be 0
    }
    It 'does not repeat the same overdue observation on an <State> issue' -ForEach @(
        @{ State = 'open' }, @{ State = 'closed' }
    ) {
        $script:Issues = @(New-NightlyIssue -State $State)
        Invoke-NightlyDeliveryMonitor -Now $script:Now.AddHours(6) -Apply
        $script:Writes.Count | Should -Be 0
    }
    It 'closes only the nightly issue after a fresh pair arrives' {
        $script:Issues = @(New-NightlyIssue)
        $script:Observation = New-Delivery -Result fresh -Hours 1 -Version '10.0.120-ci.inflight.26466.1'
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes.Count | Should -Be 2
        $script:Writes[0].Body.body | Should -Not -Match '@kubaflo'
        $script:Writes[1].Body.state | Should -Be closed
    }
    It 'opens another outage when the previously recovered version becomes stale' {
        $script:Issues = @(New-NightlyIssue -State closed)
        $script:Comments = @(@{ id = 1; user = @{ login = 'github-actions[bot]' }
            body = Format-NightlyDeliveryObservation -Branch inflight/current -Source $script:Source `
                -Observation (New-Delivery -Result fresh) -Now $script:Now })
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes[0].Endpoint | Should -Be 'repos/dotnet/maui/issues'
    }
    It 'reports known missing delivery and closes it only on fresh evidence' {
        $script:Observation = @{ Sequence = $null; Key = 'none'; Result = 'missing'; Version = 'none'; Published = $null }
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Issues = @(New-NightlyIssue -Observation $script:Observation)
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes.Count | Should -Be 1
        $script:Observation = New-Delivery -Result fresh -Hours 1
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes[-1].Body.state | Should -Be closed
    }
    It 'does not confuse nightly markers with maui-pr issues or human issues' {
        $issue = New-NightlyIssue
        $issue.body = $issue.body.Replace('maui-nightly-delivery:', 'maui-pr-branch-monitor:')
        $human = New-NightlyIssue
        $human.user.login = 'someone'
        $script:Issues = @($issue, $human)
        $script:Observation = New-Delivery -Result fresh -Hours 1
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes.Count | Should -Be 0
    }
    It 'leaves issues alone when the branch is absent' {
        $script:Issues = @(New-NightlyIssue)
        Mock Get-NightlyDeliverySource { $null }
        Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply
        $script:Writes.Count | Should -Be 0
        Should -Invoke Get-NightlyDeliveryObservation -Times 0
    }
    It 'reports feed errors visibly without closing an issue, and checks other branches' {
        $script:Issues = @(New-NightlyIssue)
        $script:NightlyDeliveryBranches = @('inflight/current', 'main')
        Mock Get-NightlyDeliverySource { throw 'Feed unavailable' } -ParameterFilter { $Branch -eq 'inflight/current' }
        { Invoke-NightlyDeliveryMonitor -Now $script:Now -Apply } | Should -Throw '*Feed unavailable*'
        $script:Writes.Count | Should -Be 1
        $script:Writes[0].Body.title | Should -Be '[nightly-delivery] main is overdue'
    }
}

Describe 'Nightly workflow contract' {
    It 'covers scheduled nightly branches, excludes candidate, and gates both suites' {
        $script:ConfiguredNightlyBranches | Should -Be @('inflight/current', 'main', 'net11.0', 'net12.0')
        $workflow = Get-Content (Join-Path $PSScriptRoot '../workflows/maui-pr-branch-monitor.yml') -Raw
        $workflow | Should -Match 'Watch-MauiNightlyDelivery.Tests.ps1'
        $workflow | Should -Match 'Watch-MauiNightlyDelivery.ps1 -Apply:'
        $workflow | Should -Match 'Containers.Count -ne \$paths.Count'
        $workflow | Should -Match "failure\(\) && steps.branch_builds.outcome == 'failure'"
        $gate = Get-Content (Join-Path $PSScriptRoot '../workflows/powershell-script-tests.yml') -Raw
        $gate | Should -Match 'release-readiness/scripts/NightlyFeed.ps1'
        $entrypoint = Get-Content (Join-Path $PSScriptRoot 'Watch-MauiNightlyDelivery.ps1') -Raw
        $entrypoint | Should -Match "Watch-MauiPrBranches.ps1'\) -Apply:\`$Apply"
    }
}
