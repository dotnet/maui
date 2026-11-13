#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Renders a copy-ready public release handoff from release-readiness JSON.

.DESCRIPTION
    Projects an existing Preview, RC, or SR readiness report plus separately verified
    release evidence into Markdown and normalized JSON. It does not survey a
    branch, select a build, query private release systems, or publish content.
    Missing evidence is rendered as a literal TBD instead of being inferred.

.PARAMETER ReadinessJson
    JSON emitted by Get-PreviewReadiness.ps1 or Get-ReleaseReadiness.ps1.

.PARAMETER EvidenceJson
    Optional public evidence using the schema documented in
    references/release-handoff.md.

.PARAMETER OutputDir
    Directory for release-handoff.md and release-handoff.json.

.PARAMETER OutputFormat
    markdown, json, or both.

.PARAMETER PublicSafe
    Applies the shared public-report sanitizer, removes email addresses and
    secret-shaped assignments, and allows links only to public release hosts.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ReadinessJson,
    [string]$EvidenceJson,
    [string]$OutputDir,
    [ValidateSet('markdown', 'json', 'both')]
    [string]$OutputFormat = 'both',
    [bool]$PublicSafe = $true
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$publicSanitizerPath = Join-Path $PSScriptRoot 'PublicReportSanitizer.ps1'
if (-not (Test-Path -LiteralPath $publicSanitizerPath)) {
    throw "Required public-report sanitizer not found at $publicSanitizerPath."
}
. $publicSanitizerPath

function Get-HandoffProperty {
    param(
        [AllowNull()]$Object,
        [Parameter(Mandatory)][string]$Name,
        $Default = $null
    )

    if ($null -eq $Object) { return $Default }
    if ($Object -is [Collections.IDictionary]) {
        foreach ($key in $Object.Keys) {
            if ([string]$key -ieq $Name) { return $Object[$key] }
        }
        return $Default
    }
    $property = $Object.PSObject.Properties |
        Where-Object { $_.Name -ieq $Name } |
        Select-Object -First 1
    if ($property) { return $property.Value }
    return $Default
}

function Test-HandoffBooleanProperty {
    param(
        [AllowNull()]$Object,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][bool]$Expected
    )

    # Inspect Value locally: returning it through Get-HandoffProperty would let
    # PowerShell unwrap a singleton JSON array before the strict type check.
    if ($null -eq $Object) { return $false }
    $value = $null
    $found = $false
    if ($Object -is [Collections.IDictionary]) {
        foreach ($key in $Object.Keys) {
            if ([string]$key -ieq $Name) {
                $value = $Object[$key]
                $found = $true
                break
            }
        }
    } else {
        $property = $Object.PSObject.Properties |
            Where-Object { $_.Name -ieq $Name } |
            Select-Object -First 1
        if ($property) {
            $value = $property.Value
            $found = $true
        }
    }
    return $found -and $value -is [bool] -and $value -eq $Expected
}

function Test-HandoffPublicUrl {
    param([AllowNull()][string]$Url)

    if ([string]::IsNullOrWhiteSpace($Url)) { return $false }
    if ($Url -match '[\s<>"''`()\[\]|]' -or
        $Url -match '(?i)%(?:25|2e|2f|3f|23|28|29|5b|5c|5d|60|7c|22|27|20)') {
        return $false
    }

    [Uri]$uri = $null
    if (-not [Uri]::TryCreate($Url, [UriKind]::Absolute, [ref]$uri)) { return $false }
    if ($uri.Scheme -ne 'https' -or -not [string]::IsNullOrEmpty($uri.UserInfo)) { return $false }

    $uriHost = $uri.Host.ToLowerInvariant()
    $path = $uri.AbsolutePath
    if (-not [string]::IsNullOrEmpty($uri.Fragment)) { return $false }
    if (-not [string]::IsNullOrEmpty($uri.Query)) {
        $safeBuildQuery = $Url -match '^https://dev\.azure\.com/dnceng/public/_build/results\?buildId=\d+$'
        if (-not $safeBuildQuery) { return $false }
    }
    switch ($uriHost) {
        'github.com' { return $true }
        'api.github.com' { return $true }
        'aka.ms' { return $true }
        'dotnet.microsoft.com' { return $true }
        'learn.microsoft.com' { return $true }
        'builds.dotnet.microsoft.com' { return $true }
        'download.visualstudio.microsoft.com' { return $true }
        'dotnetcli.azureedge.net' { return $true }
        'api.nuget.org' { return $true }
        'nuget.org' { return $true }
        'www.nuget.org' { return $true }
        'dev.azure.com' { return $path.StartsWith('/dnceng/public/', [StringComparison]::OrdinalIgnoreCase) }
        'pkgs.dev.azure.com' { return $path.StartsWith('/dnceng/public/', [StringComparison]::OrdinalIgnoreCase) }
        default { return $false }
    }
}

function Test-HandoffPublicPackageSourceUrl {
    param([AllowNull()][string]$Url)

    if (-not (Test-HandoffPublicUrl -Url $Url)) { return $false }
    [Uri]$uri = $Url
    if ($uri.Host -eq 'api.nuget.org') {
        return $uri.AbsolutePath -eq '/v3/index.json'
    }
    if ($uri.Host -eq 'pkgs.dev.azure.com') {
        return $uri.AbsolutePath -match '^/dnceng/public/_packaging/[A-Za-z0-9._-]+/nuget/v3/index\.json$'
    }
    return $false
}

function ConvertTo-PublicHandoffText {
    param([AllowNull()][AllowEmptyString()][string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return $Text }
    $preservedBuildUrls = [Collections.Generic.List[string]]::new()
    $sanitizerInput = [regex]::Replace(
        $Text,
        'https://dev\.azure\.com/dnceng/public/_build/results\?buildId=\d+(?=$|[\s<>"''`|)])',
        {
            param($match)
            $index = $preservedBuildUrls.Count
            [void]$preservedBuildUrls.Add($match.Value)
            return "__PUBLIC_HANDOFF_BUILD_URL_${index}__"
        },
        [Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $safe = ConvertTo-PublicSafeMarkdown -Text $sanitizerInput
    for ($index = 0; $index -lt $preservedBuildUrls.Count; $index++) {
        $safe = $safe.Replace("__PUBLIC_HANDOFF_BUILD_URL_${index}__", $preservedBuildUrls[$index])
    }
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?<![A-Za-z0-9._%+-])[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}',
        '_email omitted_')
    $safe = [regex]::Replace(
        $safe,
        '(?i)\b(password|cleartextpassword|token|pat|secret|api[_-]?key)\s*[:=]\s*[^\s,;]+',
        '$1=[omitted]')
    $safe = [regex]::Replace(
        $safe,
        '(?i)\b(approved|signed[- ]?off|owner|contact)\s*[:=-]?\s*(?:by\s+)?[A-Z][A-Za-z.-]+',
        '$1 _person omitted_')
    $safe = [regex]::Replace(
        $safe,
        '(?i)\b(password|cleartextpassword|token|pat|secret|api[_-]?key)\s+(?:is|was)\s+[^\s,;]+',
        '$1 is [omitted]')
    $safe = [regex]::Replace(
        $safe,
        '(?i)(?:[A-Z]:\\Users\\[^\\/\s]+|/(?:Users|home)/[^/\s]+)(?:[\\/][^\s|]*)?',
        '_local path omitted_')
    $safe = [regex]::Replace($safe, 'https?://[^\s<>"''`|)]+', {
        param($match)
        if (Test-HandoffPublicUrl -Url $match.Value) { return $match.Value }
        return '_non-public URL omitted_'
    })
    return $safe
}

function ConvertTo-HandoffText {
    param(
        [AllowNull()]$Value,
        [bool]$PublicSafe = $true
    )

    if ($null -eq $Value) { return $null }
    $text = [string]$Value
    if ($PublicSafe) { return ConvertTo-PublicHandoffText -Text $text }
    return $text
}

function ConvertTo-HandoffStructuredText {
    param(
        [AllowNull()]$Value,
        [ValidateSet('name', 'status', 'version', 'build', 'commit')]
        [string]$Kind,
        [bool]$PublicSafe = $true
    )

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) { return $null }
    if (-not $PublicSafe) { return ConvertTo-HandoffText -Value $Value -PublicSafe $false }
    $text = ConvertTo-PublicHandoffText -Text ([string]$Value)
    if (
        $text -match '(?i)(?:gh[pousr]_|github_pat_|glpat-|xox[baprs]-)[A-Za-z0-9_-]{20,}' -or
        $text -match '(?i)\b(?:pat|token|secret|password|api[_-]?key)[_:=/-][A-Za-z0-9_-]{16,}' -or
        $text -match '\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\b' -or
        $text -match '\b[A-Za-z0-9]{48,}\b'
    ) {
        return $null
    }
    $valid = switch ($Kind) {
        'name' {
            $text.Length -le 120 -and
            $text -match '^[A-Za-z0-9 ._()/+-]+$' -and
            $text -notmatch '(?i)\b(owner|contact|approved by|signed[- ]?off by)\b'
        }
        'status' {
            $text -match '^(?i:ready|passed|succeeded|published|available|validated|verified|install verified|merged|released|shipped|pending|watch|blocked|unknown|insufficient_data|tbd|not required|conditionally ready|not ready)$'
        }
        'version' { $text -match '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$' }
        'build' { $text.Length -le 120 -and $text -match '^[A-Za-z0-9 ._()/+-]+$' }
        'commit' { $text -match '^[0-9a-fA-F]{7,40}$' }
    }
    if ($valid) { return $text }
    return $null
}

function ConvertTo-HandoffTimestamp {
    param(
        [AllowNull()]$Value,
        [bool]$PublicSafe = $true
    )

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) { return $null }
    if (-not $PublicSafe) { return ConvertTo-HandoffText -Value $Value -PublicSafe $false }
    [DateTimeOffset]$timestamp = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParse(
            [string]$Value,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::AssumeUniversal,
            [ref]$timestamp)) {
        return $null
    }
    return $timestamp.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
}

function ConvertTo-HandoffMarkdownCell {
    param([AllowNull()]$Value)

    if ($null -eq $Value) { return '' }
    $cell = [string]$Value
    $cell = $cell -replace "`r`n|`n|`r", ' '
    $cell = $cell -replace '\|', '\|'
    $cell = $cell -replace '<', '&lt;' -replace '>', '&gt;'
    return $cell
}

function ConvertTo-HandoffMarkdownText {
    param([AllowNull()]$Value)

    if ($null -eq $Value) { return '' }
    $text = ConvertTo-HandoffMarkdownCell -Value $Value
    $text = $text.Replace('\', '\\')
    foreach ($character in @('`', '*', '_', '[', ']', '(', ')', '#', '!')) {
        $text = $text.Replace($character, "\$character")
    }
    return $text
}

function Get-HandoffTbd {
    param([string]$Reason)

    if ([string]::IsNullOrWhiteSpace($Reason)) { $Reason = 'authoritative evidence was not supplied' }
    return "TBD — $Reason"
}

function Get-HandoffDisplayValue {
    param(
        [AllowNull()]$Value,
        [string]$Reason = 'authoritative evidence was not supplied'
    )

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) {
        return Get-HandoffTbd -Reason $Reason
    }
    return [string]$Value
}

function Get-HandoffReleaseType {
    param([Parameter(Mandatory)]$Readiness)

    $branchType = [string](Get-HandoffProperty -Object $Readiness -Name 'BranchType')
    if ($branchType -ieq 'preview') { return 'preview' }
    if ($branchType -ieq 'rc') { return 'rc' }
    if ($branchType -ieq 'sr') { return 'sr' }
    if (Get-HandoffProperty -Object $Readiness -Name 'metadata') { return 'sr' }
    throw 'Readiness JSON is not recognized as a Preview, RC, or SR report.'
}

function Get-HandoffReadinessSummary {
    param(
        [Parameter(Mandatory)]$Readiness,
        [Parameter(Mandatory)][ValidateSet('preview', 'rc', 'sr')][string]$ReleaseType,
        [bool]$PublicSafe = $true
    )

    $rows = [Collections.Generic.List[object]]::new()
    if ($ReleaseType -in @('preview', 'rc')) {
        $checks = @(Get-HandoffProperty -Object $Readiness -Name 'Checks' -Default @())
    } else {
        $checks = @(Get-HandoffProperty -Object $Readiness -Name 'shipChecks' -Default @())
    }

    foreach ($check in $checks) {
        if ($null -eq $check) { continue }
        $status = [string](Get-HandoffProperty -Object $check -Name 'Status' -Default 'UNKNOWN')
        if ($status -in @('READY', 'CLEANUP')) { continue }
        [void]$rows.Add([PSCustomObject]@{
            Item       = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $check -Name 'Area' -Default 'Readiness check') -Kind name -PublicSafe $PublicSafe
            Status     = ConvertTo-HandoffStructuredText -Value $status -Kind status -PublicSafe $PublicSafe
            Evidence   = if ($PublicSafe) {
                'See the public source readiness report.'
            } else {
                ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $check -Name 'Details') -PublicSafe $false
            }
            NextAction = if ($PublicSafe) {
                'Review and resolve this item in the source readiness report.'
            } else {
                ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $check -Name 'NextAction') -PublicSafe $false
            }
        })
    }
    return @($rows.ToArray())
}

function ConvertTo-HandoffEvidenceRows {
    param(
        [AllowNull()]$Items,
        [bool]$PublicSafe = $true,
        [switch]$RequirePublicUrl
    )

    $rows = [Collections.Generic.List[object]]::new()
    foreach ($item in @($Items)) {
        if ($null -eq $item) { continue }
        if ($item -is [string]) {
            if ($RequirePublicUrl) { continue }
            [void]$rows.Add([PSCustomObject]@{
                Name = ConvertTo-HandoffText -Value $item -PublicSafe $PublicSafe
                Status = 'TBD'
                Url = $null
                Notes = Get-HandoffTbd -Reason 'status was not supplied'
            })
            continue
        }
        $rawUrl = [string](Get-HandoffProperty -Object $item -Name 'Url')
        if ($RequirePublicUrl -and -not (Test-HandoffPublicUrl -Url $rawUrl)) { continue }
        [void]$rows.Add([PSCustomObject]@{
            Name   = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $item -Name 'Name' -Default (Get-HandoffProperty -Object $item -Name 'Title')) -Kind name -PublicSafe $PublicSafe
            Status = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $item -Name 'Status' -Default 'TBD') -Kind status -PublicSafe $PublicSafe
            Url    = ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $item -Name 'Url') -PublicSafe $PublicSafe
            Notes  = if ($PublicSafe) {
                $null
            } else {
                ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $item -Name 'Notes') -PublicSafe $false
            }
        })
    }
    return @($rows.ToArray())
}

function ConvertTo-HandoffBuildRows {
    param(
        [AllowNull()]$Items,
        [bool]$PublicSafe = $true,
        [switch]$RequirePublicUrl
    )

    $rows = [Collections.Generic.List[object]]::new()
    foreach ($item in @($Items)) {
        if ($null -eq $item) { continue }
        $rawUrl = [string](Get-HandoffProperty -Object $item -Name 'Url')
        if ($RequirePublicUrl -and -not (Test-HandoffPublicUrl -Url $rawUrl)) { continue }
        [void]$rows.Add([PSCustomObject]@{
            Name    = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $item -Name 'Name') -Kind name -PublicSafe $PublicSafe
            Version = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $item -Name 'Version') -Kind version -PublicSafe $PublicSafe
            Build   = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $item -Name 'Build') -Kind build -PublicSafe $PublicSafe
            Commit  = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $item -Name 'Commit') -Kind commit -PublicSafe $PublicSafe
            Status  = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $item -Name 'Status' -Default 'TBD') -Kind status -PublicSafe $PublicSafe
            Url     = ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $item -Name 'Url') -PublicSafe $PublicSafe
            Notes   = if ($PublicSafe) {
                $null
            } else {
                ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $item -Name 'Notes') -PublicSafe $false
            }
        })
    }
    return @($rows.ToArray())
}

function Get-HandoffNuGetConfig {
    param(
        [AllowNull()][string]$Config,
        [bool]$PublicSafe = $true
    )

    if ([string]::IsNullOrWhiteSpace($Config)) { return $null }
    if ($Config -match '(?i)<packageSourceCredentials\b|cleartextpassword|password\s*=') {
        return $null
    }

    try { [xml]$xml = $Config } catch { return $null }
    $root = $xml.DocumentElement
    if ($null -eq $root -or $root.Name -ne 'configuration' -or $root.Attributes.Count -ne 0) { return $null }
    if (@($xml.SelectNodes('//comment()')).Count -gt 0) { return $null }

    $allowedRootElements = @('packageSources', 'packageSourceMapping')
    foreach ($node in @($root.ChildNodes)) {
        if ($node.NodeType -in @([Xml.XmlNodeType]::Whitespace, [Xml.XmlNodeType]::SignificantWhitespace)) { continue }
        if ($node.NodeType -ne [Xml.XmlNodeType]::Element -or $node.Name -notin $allowedRootElements) {
            return $null
        }
    }

    $sourceContainers = @($root.SelectNodes('packageSources'))
    $mappingContainers = @($root.SelectNodes('packageSourceMapping'))
    if ($sourceContainers.Count -ne 1 -or $mappingContainers.Count -gt 1) { return $null }
    $sourceContainer = $sourceContainers[0]
    $mappingContainer = if ($mappingContainers.Count -eq 1) { $mappingContainers[0] } else { $null }
    if ($null -eq $sourceContainer -or ($PublicSafe -and $null -eq $mappingContainer)) { return $null }
    if ($sourceContainer.Attributes.Count -ne 0 -or
        ($mappingContainer -and $mappingContainer.Attributes.Count -ne 0)) {
        return $null
    }

    $sourceRecords = [Collections.Generic.List[object]]::new()
    foreach ($node in @($sourceContainer.ChildNodes)) {
        if ($node.NodeType -in @([Xml.XmlNodeType]::Whitespace, [Xml.XmlNodeType]::SignificantWhitespace)) { continue }
        if ($node.NodeType -ne [Xml.XmlNodeType]::Element) { return $null }
        if ($node.Name -eq 'clear') {
            if ($node.Attributes.Count -ne 0 -or $node.HasChildNodes) { return $null }
            continue
        }
        if ($node.Name -ne 'add') { return $null }
        $attributeNames = @($node.Attributes | ForEach-Object Name)
        if (@($attributeNames | Where-Object { $_ -notin @('key', 'value', 'protocolVersion') }).Count -gt 0) { return $null }
        $key = [string]$node.GetAttribute('key')
        $url = [string]$node.GetAttribute('value')
        $protocol = [string]$node.GetAttribute('protocolVersion')
        if ($key -notmatch '^[A-Za-z0-9._-]+$' -or -not (Test-HandoffPublicPackageSourceUrl -Url $url)) { return $null }
        if ($protocol -and $protocol -ne '3') { return $null }
        if ($node.HasChildNodes) { return $null }
        [void]$sourceRecords.Add([PSCustomObject]@{ Key = $key; Url = $url })
    }
    if ($sourceRecords.Count -eq 0) { return $null }
    if (@($sourceRecords | Select-Object -ExpandProperty Key -Unique).Count -ne $sourceRecords.Count) { return $null }

    $mappingRecords = [Collections.Generic.List[object]]::new()
    if ($mappingContainer) {
        foreach ($node in @($mappingContainer.ChildNodes)) {
            if ($node.NodeType -in @([Xml.XmlNodeType]::Whitespace, [Xml.XmlNodeType]::SignificantWhitespace)) { continue }
            if ($node.NodeType -ne [Xml.XmlNodeType]::Element -or $node.Name -ne 'packageSource') { return $null }
            $attributeNames = @($node.Attributes | ForEach-Object Name)
            if (@($attributeNames | Where-Object { $_ -ne 'key' }).Count -gt 0) { return $null }
            $key = [string]$node.GetAttribute('key')
            if (@($sourceRecords | Where-Object Key -eq $key).Count -ne 1) { return $null }
            $patterns = [Collections.Generic.List[string]]::new()
            foreach ($packageNode in @($node.ChildNodes)) {
                if ($packageNode.NodeType -in @([Xml.XmlNodeType]::Whitespace, [Xml.XmlNodeType]::SignificantWhitespace)) { continue }
                if ($packageNode.NodeType -ne [Xml.XmlNodeType]::Element -or $packageNode.Name -ne 'package') { return $null }
                $packageAttributes = @($packageNode.Attributes | ForEach-Object Name)
                if (@($packageAttributes | Where-Object { $_ -ne 'pattern' }).Count -gt 0 -or $packageNode.HasChildNodes) { return $null }
                $pattern = [string]$packageNode.GetAttribute('pattern')
                if ($pattern -notmatch '^[A-Za-z0-9.*_-]+$') { return $null }
                [void]$patterns.Add($pattern)
            }
            if ($patterns.Count -eq 0) { return $null }
            [void]$mappingRecords.Add([PSCustomObject]@{ Key = $key; Patterns = @($patterns) })
        }
    }
    if ($PublicSafe -and $mappingRecords.Count -ne $sourceRecords.Count) { return $null }
    if (@($mappingRecords | Select-Object -ExpandProperty Key -Unique).Count -ne $mappingRecords.Count) { return $null }

    foreach ($source in $sourceRecords) {
        if ($source.Url -match '/dotnet-workloads/' -and
            @($mappingRecords | Where-Object Key -eq $source.Key |
                ForEach-Object Patterns | Where-Object { $_ -ne 'Microsoft.NET.Workloads.*' }).Count -gt 0) {
            return $null
        }
    }

    $builder = [Text.StringBuilder]::new()
    [void]$builder.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$builder.AppendLine('<configuration>')
    [void]$builder.AppendLine('  <packageSources>')
    [void]$builder.AppendLine('    <clear />')
    foreach ($source in @($sourceRecords | Sort-Object Key)) {
        $key = [Security.SecurityElement]::Escape([string]$source.Key)
        $url = [Security.SecurityElement]::Escape([string]$source.Url)
        [void]$builder.AppendLine("    <add key=`"$key`" value=`"$url`" protocolVersion=`"3`" />")
    }
    [void]$builder.AppendLine('  </packageSources>')
    if ($mappingRecords.Count -gt 0) {
        [void]$builder.AppendLine('  <packageSourceMapping>')
        foreach ($mapping in @($mappingRecords | Sort-Object Key)) {
            $key = [Security.SecurityElement]::Escape([string]$mapping.Key)
            [void]$builder.AppendLine("    <packageSource key=`"$key`">")
            foreach ($patternValue in @($mapping.Patterns | Sort-Object -Unique)) {
                $pattern = [Security.SecurityElement]::Escape([string]$patternValue)
                [void]$builder.AppendLine("      <package pattern=`"$pattern`" />")
            }
            [void]$builder.AppendLine('    </packageSource>')
        }
        [void]$builder.AppendLine('  </packageSourceMapping>')
    }
    [void]$builder.AppendLine('</configuration>')
    return $builder.ToString().Trim()
}

function Get-HandoffNuGetConfigPath {
    param([AllowNull()][string]$ConfigPath)

    if ([string]::IsNullOrWhiteSpace($ConfigPath)) { return './release-nuget.config' }
    $normalized = $ConfigPath -replace '\\', '/'
    if ($normalized.StartsWith('./', [StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(2)
    }
    if ($normalized -notmatch '^[A-Za-z0-9_.-]+$') { return $null }
    return "./$normalized"
}

function New-HandoffInstallCommand {
    param(
        [AllowNull()][string]$CliVersion,
        [AllowNull()][string]$ConfigPath
    )

    if ([string]::IsNullOrWhiteSpace($CliVersion) -or
        $CliVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
        return $null
    }
    $ConfigPath = Get-HandoffNuGetConfigPath -ConfigPath $ConfigPath
    if (-not $ConfigPath) { return $null }

    return "dotnet workload install maui --version $CliVersion --configfile $ConfigPath --verbosity diag"
}

function New-ReleaseHandoffModel {
    param(
        [Parameter(Mandatory)]$Readiness,
        [AllowNull()]$Evidence,
        [bool]$PublicSafe = $true
    )

    if ($null -eq $Evidence) { $Evidence = [PSCustomObject]@{} }
    $evidenceIsPublic = Test-HandoffBooleanProperty `
        -Object $Evidence -Name 'PublicEvidence' -Expected $true
    $useEvidence = -not $PublicSafe -or $evidenceIsPublic
    $effectiveEvidence = if ($useEvidence) { $Evidence } else { [PSCustomObject]@{} }
    $releaseType = Get-HandoffReleaseType -Readiness $Readiness
    if ($releaseType -in @('preview', 'rc')) {
        $major = Get-HandoffProperty -Object $Readiness -Name 'MajorVersion'
        $iteration = if ($releaseType -eq 'rc') {
            Get-HandoffProperty -Object $Readiness -Name 'RcNumber'
        } else {
            Get-HandoffProperty -Object $Readiness -Name 'PreviewNumber'
        }
        $branch = Get-HandoffProperty -Object $Readiness -Name 'Branch'
        $mode = Get-HandoffProperty -Object $Readiness -Name 'Mode'
        $verdict = Get-HandoffProperty -Object $Readiness -Name 'Verdict'
        $checkState = Get-HandoffProperty -Object $Readiness -Name 'OverallStatus'
        $generatedAt = Get-HandoffProperty -Object $Readiness -Name 'GeneratedAt'
        $stageName = if ($releaseType -eq 'rc') { 'RC' } else { 'Preview' }
        $defaultName = ".NET MAUI $major.0.0 $stageName $iteration"
        $installability = Get-HandoffProperty -Object $Readiness -Name 'ConsumerInstallability'
    } else {
        $metadata = Get-HandoffProperty -Object $Readiness -Name 'metadata'
        $branch = Get-HandoffProperty -Object $metadata -Name 'srBranch'
        $mode = Get-HandoffProperty -Object $metadata -Name 'mode'
        $generatedAt = Get-HandoffProperty -Object $metadata -Name 'fetchedAt'
        $verdictObject = Get-HandoffProperty -Object $Readiness -Name 'verdict'
        $verdict = Get-HandoffProperty -Object $verdictObject -Name 'label'
        $checkState = Get-HandoffProperty -Object $verdictObject -Name 'tier'
        $installability = $null
        $match = [regex]::Match([string]$branch, '^release/(?<major>\d+)\.\d+\.\d+xx-sr(?<sr>\d+)$')
        if (-not $match.Success -and $mode -eq 'candidate') {
            $trackerKey = [string](Get-HandoffProperty -Object $Readiness -Name 'trackerKey')
            $trackerMatch = [regex]::Match($trackerKey, '^net(?<major>\d+)-sr(?<sr>\d+)$')
            $priorBranch = [string](Get-HandoffProperty -Object $metadata -Name 'priorSrBranch')
            $priorMatch = [regex]::Match($priorBranch, '^release/(?<major>\d+)\.(?<minor>\d+)\.(?<band>\d+)xx-sr\d+$')
            if ($trackerMatch.Success -and
                $priorMatch.Success -and
                $trackerMatch.Groups['major'].Value -eq $priorMatch.Groups['major'].Value) {
                $match = $trackerMatch
                $branch = "release/$($priorMatch.Groups['major'].Value).$($priorMatch.Groups['minor'].Value).$($priorMatch.Groups['band'].Value)xx-sr$($trackerMatch.Groups['sr'].Value)"
            } else {
                $branch = $null
            }
        }
        $defaultName = if ($match.Success) {
            ".NET MAUI $($match.Groups['major'].Value).0 SR $($match.Groups['sr'].Value)"
        } else {
            '.NET MAUI servicing release'
        }
    }

    $workload = Get-HandoffProperty -Object $effectiveEvidence -Name 'WorkloadSet'
    $installabilityIsPublic = Test-HandoffBooleanProperty `
        -Object $installability -Name 'PublicEvidence' -Expected $true
    $installabilityVersionIsConfirmed = Test-HandoffBooleanProperty `
        -Object $installability -Name 'VersionConfirmed' -Expected $true
    $installabilityVersionIsPublic = Test-HandoffBooleanProperty `
        -Object $installability -Name 'VersionSourceIsSensitive' -Expected $false
    $useInstallabilityFallback = -not $PublicSafe -or (
        $installabilityIsPublic -and
        $installabilityVersionIsConfirmed -and
        $installabilityVersionIsPublic
    )
    $cliVersion = Get-HandoffProperty -Object $workload -Name 'CliVersion'
    if ([string]::IsNullOrWhiteSpace([string]$cliVersion) -and $installability -and $useInstallabilityFallback) {
        $candidateVersion = Get-HandoffProperty -Object $installability -Name 'CliVersion'
        if ($candidateVersion -and $candidateVersion -ne 'withheld') { $cliVersion = $candidateVersion }
    }
    $config = Get-HandoffProperty -Object $workload -Name 'NuGetConfig'
    if ([string]::IsNullOrWhiteSpace([string]$config) -and $installability -and $useInstallabilityFallback) {
        $config = Get-HandoffProperty -Object $installability -Name 'NuGetConfig'
    }
    $config = Get-HandoffNuGetConfig -Config $config -PublicSafe $PublicSafe
    $configPath = Get-HandoffProperty -Object $workload -Name 'NuGetConfigPath' -Default './release-nuget.config'
    $configPath = Get-HandoffNuGetConfigPath -ConfigPath $configPath
    if (-not $configPath) { $config = $null }
    $installCommand = if ($config) {
        New-HandoffInstallCommand -CliVersion $cliVersion -ConfigPath $configPath
    } else {
        $null
    }

    $releaseName = Get-HandoffProperty -Object $effectiveEvidence -Name 'ReleaseName'
    if ($PublicSafe) {
        $releaseName = ConvertTo-HandoffStructuredText -Value $releaseName -Kind name -PublicSafe $true
    }
    if ([string]::IsNullOrWhiteSpace([string]$releaseName)) { $releaseName = $defaultName }
    $releaseVersion = [string](Get-HandoffProperty -Object $effectiveEvidence -Name 'ReleaseVersion')
    if ($PublicSafe -and $releaseVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
        $releaseVersion = $null
    }
    $rollback = Get-HandoffProperty -Object $effectiveEvidence -Name 'Rollback'
    $rollbackUrl = [string](Get-HandoffProperty -Object $rollback -Name 'Url')
    if ($PublicSafe -and -not (Test-HandoffPublicUrl -Url $rollbackUrl)) {
        $rollbackUrl = $null
    }
    if ($PublicSafe) {
        if ($branch -notmatch '^release/\d+\.\d+\.\d+xx-(?:preview\d+|rc\d+|sr\d+)$') { $branch = $null }
        if ($mode -notin @('in-flight', 'candidate', 'shipped')) { $mode = $null }
        $verdict = ConvertTo-HandoffStructuredText -Value $verdict -Kind status -PublicSafe $true
        $checkStateText = [string]$checkState
        if ($checkStateText -match '^\d+$') {
            $checkState = $checkStateText
        } else {
            $checkState = ConvertTo-HandoffStructuredText -Value $checkState -Kind status -PublicSafe $true
        }
    }

    return [PSCustomObject]@{
        SchemaVersion = 1
        ReleaseType = $releaseType
        ReleaseName = ConvertTo-HandoffText -Value $releaseName -PublicSafe $PublicSafe
        ReleaseVersion = ConvertTo-HandoffText -Value $releaseVersion -PublicSafe $PublicSafe
        Branch = ConvertTo-HandoffText -Value $branch -PublicSafe $PublicSafe
        Mode = ConvertTo-HandoffText -Value $mode -PublicSafe $PublicSafe
        Verdict = ConvertTo-HandoffText -Value $verdict -PublicSafe $PublicSafe
        CheckState = ConvertTo-HandoffText -Value $checkState -PublicSafe $PublicSafe
        ReadinessGeneratedAt = ConvertTo-HandoffTimestamp -Value $generatedAt -PublicSafe $PublicSafe
        BreakingChanges = @(ConvertTo-HandoffEvidenceRows `
            -Items (Get-HandoffProperty -Object $effectiveEvidence -Name 'BreakingChanges' -Default @()) `
            -PublicSafe $PublicSafe -RequirePublicUrl:$PublicSafe)
        Tests = @(ConvertTo-HandoffEvidenceRows `
            -Items (Get-HandoffProperty -Object $effectiveEvidence -Name 'Tests' -Default @()) `
            -PublicSafe $PublicSafe -RequirePublicUrl:$PublicSafe)
        Assessments = @(ConvertTo-HandoffEvidenceRows `
            -Items (Get-HandoffProperty -Object $effectiveEvidence -Name 'Assessments' -Default @()) `
            -PublicSafe $PublicSafe -RequirePublicUrl:$PublicSafe)
        Builds = @(ConvertTo-HandoffBuildRows `
            -Items (Get-HandoffProperty -Object $effectiveEvidence -Name 'Builds' -Default @()) `
            -PublicSafe $PublicSafe -RequirePublicUrl:$PublicSafe)
        Rollback = [PSCustomObject]@{
            Status = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $rollback -Name 'Status') -Kind status -PublicSafe $PublicSafe
            Url = ConvertTo-HandoffText -Value $rollbackUrl -PublicSafe $PublicSafe
            Notes = if ($PublicSafe) {
                $null
            } else {
                ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $rollback -Name 'Notes') -PublicSafe $false
            }
        }
        WorkloadSet = [PSCustomObject]@{
            CliVersion = ConvertTo-HandoffStructuredText -Value $cliVersion -Kind version -PublicSafe $PublicSafe
            NuGetVersion = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $workload -Name 'NuGetVersion') -Kind version -PublicSafe $PublicSafe
            ManifestVersion = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $workload -Name 'ManifestVersion') -Kind version -PublicSafe $PublicSafe
            Status = ConvertTo-HandoffStructuredText -Value (Get-HandoffProperty -Object $workload -Name 'Status') -Kind status -PublicSafe $PublicSafe
            Notes = if ($PublicSafe) {
                $null
            } else {
                ConvertTo-HandoffText -Value (Get-HandoffProperty -Object $workload -Name 'Notes') -PublicSafe $false
            }
            NuGetConfigPath = if ($config) { ConvertTo-HandoffText -Value $configPath -PublicSafe $PublicSafe } else { $null }
            NuGetConfig = $config
            InstallCommand = $installCommand
        }
        ReadinessItems = @(Get-HandoffReadinessSummary `
            -Readiness $Readiness -ReleaseType $releaseType -PublicSafe $PublicSafe)
        Sources = @(ConvertTo-HandoffEvidenceRows `
            -Items (Get-HandoffProperty -Object $effectiveEvidence -Name 'Sources' -Default @()) `
            -PublicSafe $PublicSafe -RequirePublicUrl:$PublicSafe)
    }
}

function Add-HandoffEvidenceTable {
    param(
        [Parameter(Mandatory)][Text.StringBuilder]$Builder,
        [AllowNull()]$Rows,
        [string]$EmptyReason
    )

    if (@($Rows).Count -eq 0) {
        [void]$Builder.AppendLine("- $(Get-HandoffTbd -Reason $EmptyReason)")
        [void]$Builder.AppendLine()
        return
    }
    [void]$Builder.AppendLine('| Item | Status | Evidence |')
    [void]$Builder.AppendLine('|------|--------|----------|')
    foreach ($row in @($Rows)) {
        $name = ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $row.Name -Reason 'item name was not supplied')
        $status = ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $row.Status -Reason 'status was not verified')
        $evidence = if ($row.Url) {
            $label = if ($row.Notes) { ConvertTo-HandoffMarkdownText $row.Notes } else { 'Evidence' }
            "[$label]($($row.Url))"
        } else {
            ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $row.Notes -Reason 'evidence link or notes were not supplied')
        }
        [void]$Builder.AppendLine("| $name | $status | $evidence |")
    }
    [void]$Builder.AppendLine()
}

function Format-ReleaseHandoffMarkdown {
    param([Parameter(Mandatory)]$Model)

    $builder = [Text.StringBuilder]::new()
    $version = Get-HandoffDisplayValue -Value $Model.ReleaseVersion -Reason 'release version was not verified'
    [void]$builder.AppendLine("# $(ConvertTo-HandoffMarkdownText $Model.ReleaseName) Release Handoff")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("> **Release version:** ``$version``  ")
    [void]$builder.AppendLine("> **Readiness:** **$(Get-HandoffDisplayValue -Value $Model.Verdict -Reason 'readiness verdict was not supplied')** (``$(Get-HandoffDisplayValue -Value $Model.CheckState -Reason 'check state was not supplied')``)  ")
    [void]$builder.AppendLine("> **Branch:** ``$(Get-HandoffDisplayValue -Value $Model.Branch -Reason 'release branch was not supplied')``  ")
    [void]$builder.AppendLine("> **Mode:** ``$(Get-HandoffDisplayValue -Value $Model.Mode -Reason 'release mode was not supplied')``  ")
    [void]$builder.AppendLine("> **Readiness evidence generated:** $(Get-HandoffDisplayValue -Value $Model.ReadinessGeneratedAt -Reason 'readiness timestamp was not supplied')")
    [void]$builder.AppendLine()

    [void]$builder.AppendLine('## Breaking Changes')
    [void]$builder.AppendLine()
    if (@($Model.BreakingChanges).Count -eq 0) {
        $reason = if ($Model.ReleaseType -eq 'sr') {
            'confirm that the servicing release contains no breaking changes; any intentional break requires explicit approval'
        } else {
            'breaking-change review was not supplied'
        }
        [void]$builder.AppendLine("- $(Get-HandoffTbd -Reason $reason)")
        [void]$builder.AppendLine()
    } else {
        foreach ($change in @($Model.BreakingChanges)) {
            $name = ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $change.Name -Reason 'change description was not supplied')
            $status = ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $change.Status -Reason 'review status was not supplied')
            $suffix = if ($change.Url) {
                " — [evidence]($($change.Url))"
            } elseif ($change.Notes) {
                " — $(ConvertTo-HandoffMarkdownText $change.Notes)"
            } else {
                ''
            }
            [void]$builder.AppendLine("- **${status}:** $name$suffix")
        }
        [void]$builder.AppendLine()
    }

    [void]$builder.AppendLine('## Testing')
    [void]$builder.AppendLine()
    Add-HandoffEvidenceTable -Builder $builder -Rows $Model.Tests -EmptyReason 'test runs for the selected build were not supplied'

    [void]$builder.AppendLine('## Assessments and Insertion PRs')
    [void]$builder.AppendLine()
    Add-HandoffEvidenceTable -Builder $builder -Rows $Model.Assessments -EmptyReason 'assessment and insertion evidence was not supplied'

    [void]$builder.AppendLine('## Builds & Releases')
    [void]$builder.AppendLine()
    if (@($Model.Builds).Count -eq 0) {
        [void]$builder.AppendLine("- $(Get-HandoffTbd -Reason 'official SDK, runtime, MAUI, workload-set, tag, and release evidence was not supplied')")
        [void]$builder.AppendLine()
    } else {
        [void]$builder.AppendLine('| Artifact | Version | Build | Commit | Status | Evidence |')
        [void]$builder.AppendLine('|----------|---------|-------|--------|--------|----------|')
        foreach ($build in @($Model.Builds)) {
            $evidence = if ($build.Url) {
                "[link]($($build.Url))"
            } else {
                ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $build.Notes -Reason 'public evidence link was not supplied')
            }
            [void]$builder.AppendLine("| $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $build.Name 'artifact name was not supplied')) | $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $build.Version 'version was not supplied')) | $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $build.Build 'build ID was not supplied')) | $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $build.Commit 'commit was not supplied')) | $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $build.Status 'status was not supplied')) | $evidence |")
        }
        [void]$builder.AppendLine()
    }

    [void]$builder.AppendLine('## Rollback file')
    [void]$builder.AppendLine()
    $rollbackStatus = ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $Model.Rollback.Status -Reason 'rollback publication status was not verified')
    if ($Model.Rollback.Url) {
        [void]$builder.AppendLine("- **${rollbackStatus}:** [rollback file]($($Model.Rollback.Url))$(if ($Model.Rollback.Notes) { " — $(ConvertTo-HandoffMarkdownText $Model.Rollback.Notes)" })")
    } else {
        $notes = ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $Model.Rollback.Notes -Reason 'rollback URL and verification evidence were not supplied')
        [void]$builder.AppendLine("- **${rollbackStatus}:** $notes")
    }
    [void]$builder.AppendLine()

    [void]$builder.AppendLine('## Workload Set')
    [void]$builder.AppendLine()
    [void]$builder.AppendLine('| Field | Value |')
    [void]$builder.AppendLine('|-------|-------|')
    [void]$builder.AppendLine("| CLI version | ``$(ConvertTo-HandoffMarkdownCell (Get-HandoffDisplayValue $Model.WorkloadSet.CliVersion 'workload-set CLI version was not verified'))`` |")
    [void]$builder.AppendLine("| NuGet version | ``$(ConvertTo-HandoffMarkdownCell (Get-HandoffDisplayValue $Model.WorkloadSet.NuGetVersion 'workload-set NuGet version was not verified'))`` |")
    [void]$builder.AppendLine("| MAUI manifest | ``$(ConvertTo-HandoffMarkdownCell (Get-HandoffDisplayValue $Model.WorkloadSet.ManifestVersion 'MAUI manifest version was not verified'))`` |")
    [void]$builder.AppendLine("| Installability | **$(ConvertTo-HandoffMarkdownCell (Get-HandoffDisplayValue $Model.WorkloadSet.Status 'clean-machine installation was not verified'))** |")
    [void]$builder.AppendLine("| Notes | $(ConvertTo-HandoffMarkdownCell (Get-HandoffDisplayValue $Model.WorkloadSet.Notes 'workload-set notes were not supplied')) |")
    [void]$builder.AppendLine()

    if ($Model.WorkloadSet.NuGetConfig -and $Model.WorkloadSet.InstallCommand) {
        [void]$builder.AppendLine("Save this as ``$($Model.WorkloadSet.NuGetConfigPath)``:")
        [void]$builder.AppendLine()
        [void]$builder.AppendLine('```xml')
        [void]$builder.AppendLine($Model.WorkloadSet.NuGetConfig)
        [void]$builder.AppendLine('```')
        [void]$builder.AppendLine()
        [void]$builder.AppendLine('### Windows')
        [void]$builder.AppendLine()
        [void]$builder.AppendLine('```powershell')
        [void]$builder.AppendLine($Model.WorkloadSet.InstallCommand)
        [void]$builder.AppendLine('```')
        [void]$builder.AppendLine()
        [void]$builder.AppendLine('### macOS')
        [void]$builder.AppendLine()
        [void]$builder.AppendLine('```bash')
        [void]$builder.AppendLine($Model.WorkloadSet.InstallCommand)
        [void]$builder.AppendLine('```')
        [void]$builder.AppendLine()
    } else {
        [void]$builder.AppendLine("- $(Get-HandoffTbd -Reason 'a credential-free mapped NuGet configuration and verified workload-set version are required before publishing an installation command')")
        [void]$builder.AppendLine()
    }

    [void]$builder.AppendLine('## Release Readiness')
    [void]$builder.AppendLine()
    if (@($Model.ReadinessItems).Count -eq 0) {
        [void]$builder.AppendLine('- No non-ready checklist items were present in the supplied readiness report.')
        [void]$builder.AppendLine()
    } else {
        [void]$builder.AppendLine('| Item | Status | Evidence | Next action |')
        [void]$builder.AppendLine('|------|--------|----------|-------------|')
        foreach ($item in @($Model.ReadinessItems)) {
            [void]$builder.AppendLine("| $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $item.Item 'check name was not supplied')) | $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $item.Status 'status was not supplied')) | $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $item.Evidence 'check evidence was not supplied')) | $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue $item.NextAction 'next action was not supplied')) |")
        }
        [void]$builder.AppendLine()
    }

    [void]$builder.AppendLine('## Sources')
    [void]$builder.AppendLine()
    if (@($Model.Sources).Count -eq 0) {
        [void]$builder.AppendLine("- $(Get-HandoffTbd -Reason 'public source URLs were not supplied')")
    } else {
        foreach ($source in @($Model.Sources)) {
            $name = ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $source.Name -Reason 'source name was not supplied')
            if ($source.Url) {
                [void]$builder.AppendLine("- [$name]($($source.Url))")
            } else {
                [void]$builder.AppendLine("- $name — $(ConvertTo-HandoffMarkdownText (Get-HandoffDisplayValue -Value $source.Notes -Reason 'public source URL was not supplied'))")
            }
        }
    }
    [void]$builder.AppendLine()
    return $builder.ToString()
}

function Invoke-ReleaseHandoff {
    if (-not (Test-Path -LiteralPath $ReadinessJson -PathType Leaf)) {
        throw "Readiness JSON not found: $ReadinessJson"
    }
    $readiness = Get-Content -LiteralPath $ReadinessJson -Raw | ConvertFrom-Json -Depth 100
    $evidence = if ($EvidenceJson) {
        if (-not (Test-Path -LiteralPath $EvidenceJson -PathType Leaf)) {
            throw "Evidence JSON not found: $EvidenceJson"
        }
        Get-Content -LiteralPath $EvidenceJson -Raw | ConvertFrom-Json -Depth 100
    } else {
        [PSCustomObject]@{}
    }

    $model = New-ReleaseHandoffModel -Readiness $readiness -Evidence $evidence -PublicSafe $PublicSafe
    $markdown = Format-ReleaseHandoffMarkdown -Model $model
    $json = $model | ConvertTo-Json -Depth 30

    if ($OutputDir) {
        if (-not (Test-Path -LiteralPath $OutputDir)) {
            New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
        }
        if ($OutputFormat -in @('markdown', 'both')) {
            Set-Content -LiteralPath (Join-Path $OutputDir 'release-handoff.md') -Value $markdown -Encoding utf8
        }
        if ($OutputFormat -in @('json', 'both')) {
            Set-Content -LiteralPath (Join-Path $OutputDir 'release-handoff.json') -Value $json -Encoding utf8
        }
    } else {
        if ($OutputFormat -in @('markdown', 'both')) { Write-Output $markdown }
        if ($OutputFormat -in @('json', 'both')) { Write-Output $json }
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-ReleaseHandoff
}
