#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Monitor daily delivery of matching Controls and SDK nightly packages.
.DESCRIPTION
    Read-only by default. Uses public NuGet registrations, not internal build status.
    Nightly streams are identified by each branch's checked-in version settings.
#>
[CmdletBinding()]
param([switch]$Apply)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'Watch-MauiPrBranches.ps1') -Apply:$Apply
. (Join-Path $PSScriptRoot '../skills/release-readiness/scripts/NightlyFeed.ps1')

$script:NightlyDeliveryBranches = @('inflight/current', 'main', 'net11.0', 'net12.0')
$script:NightlyDeliveryPackages = @('Microsoft.Maui.Controls', 'Microsoft.Maui.Sdk')
$script:NightlyDeliveryMaxAgeHours = 30

function Get-NightlyDeliverySource {
    param([string]$Branch)

    $refs = @(Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/git/matching-refs/heads/$Branch")
    $exact = @($refs | Where-Object { $_.ref -ceq "refs/heads/$Branch" })
    if ($exact.Count -eq 0) { return $null }
    if ($exact.Count -ne 1 -or $exact[0].object.sha -cnotmatch '^[0-9a-f]{40}$') {
        throw "Invalid branch identity for $Branch."
    }
    $file = Invoke-BranchCiGitHub -Endpoint "repos/$script:BranchCiRepo/contents/eng/Versions.props?ref=$($exact[0].object.sha)"
    if ($file.encoding -cne 'base64') { throw "Unsupported version-file encoding for $Branch." }
    $text = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($file.content))
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [Xml.XmlReader]::Create([IO.StringReader]::new($text), $settings)
    try {
        $xml = [Xml.XmlDocument]::new()
        $xml.XmlResolver = $null
        $xml.Load($reader)
    } finally {
        $reader.Dispose()
    }
    $values = @{}
    foreach ($name in @('MajorVersion', 'MinorVersion', 'PreReleaseVersionLabel', 'PreReleaseVersionIteration')) {
        $nodes = $xml.SelectNodes("/Project/PropertyGroup[not(@Condition)]/$name[not(@Condition)]")
        if ($nodes.Count -ne 1) { throw "Ambiguous $name in $Branch version settings." }
        $values[$name] = $nodes[0].InnerText.Trim()
    }
    # Only interpret this known branch override; never evaluate MSBuild from a monitored branch.
    if ($Branch -ceq 'inflight/current') {
        $override = @($xml.SelectNodes('/Project/PropertyGroup/PreReleaseVersionLabel[@Condition]') | Where-Object {
            $_.GetAttribute('Condition') -ceq "'`$(BUILD_SOURCEBRANCH)' == 'refs/heads/inflight/current'"
        })
        if ($override.Count -ne 1) { throw 'Missing or ambiguous inflight nightly version override.' }
        $values.PreReleaseVersionLabel = $override[0].InnerText.Trim()
    }
    if ($values.MajorVersion -cnotmatch '^[1-9][0-9]?$' -or $values.MinorVersion -cnotmatch '^[0-9]+$' -or
        $values.PreReleaseVersionLabel -cnotmatch '^[a-z0-9]+(\.[a-z0-9]+)*$' -or
        $values.PreReleaseVersionIteration -cnotmatch '^[0-9]*$') {
        throw "Unsupported nightly version settings for $Branch."
    }
    $label = $values.PreReleaseVersionLabel
    if ($values.PreReleaseVersionIteration) { $label += ".$($values.PreReleaseVersionIteration)" }
    $prefix = "^$($values.MajorVersion)\.$($values.MinorVersion)\.[0-9]+-" + [regex]::Escape($label) + '\.'
    return @{ Feed = "dotnet$($values.MajorVersion)"; Prefix = $prefix }
}

function Assert-NightlyDeliveryRegistration {
    param([hashtable]$Data, [datetime]$Now)

    if ($Data.Contains('resources')) {
        if ($Data.resources -isnot [array] -or @($Data.resources | Where-Object {
            $_['@type'] -like 'RegistrationsBaseUrl*' -and $_['@id']
        }).Count -eq 0) { throw 'NuGet service index has no registration resource.' }
    } else {
        if ($Data.items -isnot [array] -or $Data.items.Count -eq 0 -or $Data.count -ne $Data.items.Count) {
            throw 'Incomplete NuGet registration page.'
        }
        foreach ($item in $Data.items) {
            if ($item.Contains('catalogEntry')) {
                $entry = $item.catalogEntry
                if ($entry -isnot [System.Collections.IDictionary] -or
                    $entry.version -cnotmatch '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?(\+[0-9A-Za-z.-]+)?$') {
                    throw 'Invalid NuGet catalog entry.'
                }
                $published = ConvertTo-NightlyFeedUtc $entry.published
                if ($entry.Contains('listed') -and $entry.listed -eq $false) { continue }
                if ($null -eq $published -or $published -gt $Now) {
                    throw 'Invalid or future NuGet publication timestamp.'
                }
            } elseif ($item.Contains('items')) {
                Assert-NightlyDeliveryRegistration -Data $item -Now $Now
            } elseif (-not $item['@id']) {
                throw 'NuGet registration page is neither inline nor linked.'
            }
        }
    }
}

function Get-NightlyDeliveryFeedJson {
    param([string]$Url, [datetime]$Now)

    $uri = [uri]$Url
    if ($uri.Scheme -cne 'https' -or $uri.Host -cne 'pkgs.dev.azure.com' -or
        $uri.Port -ne 443 -or $uri.UserInfo -or
        $uri.AbsolutePath -cnotmatch '^/dnceng/(public|[0-9a-f-]{36})/_packaging/[^/]+/nuget/v3/') {
        throw 'Unexpected NuGet registration URL.'
    }
    $response = Invoke-WebRequest -Uri $uri -TimeoutSec 30 -MaximumRetryCount 2 -RetryIntervalSec 2 -MaximumRedirection 0
    $data = $response.Content | ConvertFrom-Json -AsHashtable
    if ($data -isnot [System.Collections.IDictionary]) { throw 'Invalid NuGet registration response.' }
    Assert-NightlyDeliveryRegistration -Data $data -Now $Now
    return $data
}

function Get-NightlyDeliveryObservation {
    param([hashtable]$Source, [datetime]$Now)

    $publications = @{}
    foreach ($package in $script:NightlyDeliveryPackages) {
        $fresh = Get-NightlyFeedFreshness -Feed $Source.Feed -Package $package `
            -VersionPrefixRegex $Source.Prefix -IncludeVersions -Fetcher {
                param($url) Get-NightlyDeliveryFeedJson -Url $url -Now $Now
            }
        if ($null -eq $fresh) { throw "Incomplete nightly publication evidence for $package on $($Source.Feed)." }
        $publications[$package] = @{}
        foreach ($version in $fresh.versions) {
            $publications[$package][$version.version] = $version.published
        }
    }
    $deliveries = @(foreach ($version in $publications[$script:NightlyDeliveryPackages[0]].Keys) {
        if ($publications[$script:NightlyDeliveryPackages[1]].ContainsKey($version)) {
            $published = @($script:NightlyDeliveryPackages | ForEach-Object { $publications[$_][$version] } |
                Sort-Object -Descending)[0]
            @{ Version = $version; Published = $published }
        }
    })
    if ($deliveries.Count -eq 0) {
        return @{ Sequence = $null; Key = 'none'; Result = 'missing'; Version = 'none'; Published = $null }
    }
    $latest = @($deliveries | Sort-Object { $_.Published } -Descending)[0]
    $result = if (($Now - $latest.Published).TotalHours -ge $script:NightlyDeliveryMaxAgeHours) { 'stale' } else { 'fresh' }
    return @{
        Sequence = ([datetimeoffset]$latest.Published).ToUnixTimeSeconds()
        Key = $latest.Version; Result = $result; Version = $latest.Version; Published = $latest.Published
    }
}

function Read-NightlyDeliveryObservation {
    param([AllowNull()][string]$Body)
    if ($Body -match '(?m)^<!-- maui-nightly-delivery-state:([0-9]+);([^;<> ]+);(fresh|stale|missing) -->\r?$') {
        return @{
            Sequence = if ($Matches[1] -eq '0') { $null } else { [long]$Matches[1] }
            Key = $Matches[2]; Result = $Matches[3]
        }
    }
    return $null
}

function Format-NightlyDeliveryObservation {
    param([string]$Branch, [hashtable]$Source, [hashtable]$Observation, [datetime]$Now, [switch]$NewIssue)

    $sequence = if ($null -eq $Observation.Sequence) { 0 } else { $Observation.Sequence }
    $published = if ($null -eq $Observation.Published) { 'No matching package pair found' } else {
        $Observation.Published.ToString('u', [cultureinfo]::InvariantCulture)
    }
    $lines = @(
        "<!-- maui-nightly-delivery:$Branch -->"
        "<!-- maui-nightly-delivery-state:$sequence;$($Observation.Key);$($Observation.Result) -->"
        ''
        "Daily nightly delivery for ``$Branch`` is **$($Observation.Result)**."
        ''
        "- Feed: [$($Source.Feed)](https://dev.azure.com/dnceng/public/_artifacts/feed/$($Source.Feed))"
        "- Version family: ``$($Source.Prefix)``"
        "- Latest matching Controls + SDK version: ``$($Observation.Version)``"
        "- Pair fully published (UTC): $published"
        "- Checked (UTC): $($Now.ToString('u', [cultureinfo]::InvariantCulture))"
        "- Delivery deadline: $script:NightlyDeliveryMaxAgeHours hours (daily delivery plus six hours of grace)"
        ''
        'Evidence is public NuGet publication metadata for Microsoft.Maui.Controls and Microsoft.Maui.Sdk.'
        'Both packages must have the same version in the configured nightly family; build success alone does not count.'
    )
    if ($NewIssue) {
        $lines += @('', '@kubaflo please investigate nightly publishing and restore daily delivery.',
            'The monitor checks every six hours and closes this issue when a fresh package pair is delivered.')
    }
    return $lines -join "`n"
}

function Invoke-NightlyDeliveryMonitor {
    param([switch]$Apply, [datetime]$Now = [datetime]::UtcNow)

    if ($Now.Kind -ne [DateTimeKind]::Utc) { throw 'Nightly monitor requires a UTC clock.' }
    $issues = @(Get-BranchCiIssues)
    $rows = @('| Branch | Nightly delivery | Action |', '| --- | --- | --- |')
    $errors = @()
    foreach ($branch in $script:NightlyDeliveryBranches) {
        try {
            $source = Get-NightlyDeliverySource $branch
            if ($null -eq $source) {
                Write-Warning "Nightly branch $branch does not exist; skipping until it is created."
                $rows += "| $branch | Branch does not exist | Skipped |"
                continue
            }
            $observation = Get-NightlyDeliveryObservation -Source $source -Now $Now
            $format = @{ Branch = $branch; Source = $source; Observation = $observation; Now = $Now }
            $action = Sync-BranchCiIssue -Issues $issues -Marker "<!-- maui-nightly-delivery:$branch -->" `
                -Observation $observation -Parser { param($text) Read-NightlyDeliveryObservation $text } `
                -Healthy ($observation.Result -ceq 'fresh') -Title "[nightly-delivery] $branch is overdue" `
                -Body (Format-NightlyDeliveryObservation @format) `
                -NewIssueBody (Format-NightlyDeliveryObservation @format -NewIssue) -Apply:$Apply
            $rows += "| $branch | $($observation.Result): $($observation.Version) | $action |"
        } catch {
            $errors += "${branch}: $($_.Exception.Message)"
            Write-Warning $errors[-1]
            $rows += "| $branch | Unknown / operation failed | See workflow errors; not considered delivered |"
        }
    }
    $summary = "## Nightly delivery (30-hour deadline)`n`n" + ($rows -join "`n") + "`n"
    Write-Host $summary
    if ($env:GITHUB_STEP_SUMMARY) { Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $summary }
    if ($errors.Count -gt 0) { throw "Nightly monitor encountered $($errors.Count) error(s):`n$($errors -join "`n")" }
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-NightlyDeliveryMonitor -Apply:$Apply
}
