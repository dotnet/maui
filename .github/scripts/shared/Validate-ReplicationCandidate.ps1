#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates an issue-replication candidate before publication credentials are exposed.

.DESCRIPTION
    Treats the manifest, candidate diff, verification logs, and media as untrusted.
    Validation is bounded, deterministic, add-only, and fail-closed. The only persisted
    result is a small normalized JSON document suitable for a later trusted publisher.
#>

[CmdletBinding(DefaultParameterSetName = 'Patch')]
param(
    [string]$RepoRoot,
    [string]$CandidateManifestPath,
    [Parameter(ParameterSetName = 'Patch')]
    [string]$PatchPath,
    [Parameter(ParameterSetName = 'Commit')]
    [string]$CandidateCommit,
    [Parameter(ParameterSetName = 'Commit')]
    [string]$BaseCommit,
    [string]$EvidenceDir,
    [long]$IssueNumber,
    [string]$Platform,
    [string]$OutputPath,
    [scriptblock]$MediaProbe
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$guardValidatorPath = Join-Path $PSScriptRoot 'Assert-ReplicationTestGuard.ps1'
if (-not (Test-Path -LiteralPath $guardValidatorPath -PathType Leaf)) {
    throw "Trusted replication guard validator is missing: $guardValidatorPath"
}
. $guardValidatorPath

$script:ManifestMaxBytes = 64KB
$script:EvidenceJsonMaxBytes = 64KB
$script:PatchMaxBytes = 2MB
$script:CandidateFileMaxBytes = 256KB
$script:CandidateTotalMaxBytes = 1MB
$script:CandidateFileMaxCount = 24
$script:VerificationArtifactMaxBytes = 2MB
$script:VideoMaxBytes = 100MB
$script:PreviewMaxBytes = 20MB

$script:DisqualifyingFailurePatterns = @(
    [pscustomobject]@{
        Code = 'build-or-compile'
        Pattern = '(?im)\b(?:BUILD (?:ERROR|FAILED)|COMPILATION (?:ERROR|FAILED)|FAILED TO (?:BUILD|COMPILE)|DOES NOT COMPILE|error (?:CS|MSB|NETSDK|XA|XLS|XFC)\d{3,5})\b'
    },
    [pscustomobject]@{
        Code = 'infrastructure'
        Pattern = '(?im)\b(?:VERIFICATION INCONCLUSIVE|ENV ERROR|INFRASTRUCTURE (?:ERROR|FAILURE)|XHarness exit code|APP_LAUNCH_FAILURE|PACKAGE_INSTALLATION_FAILURE|ADB\d{3,5}|device (?:offline|not found)|Appium server failed|Unable to load (?:shared library|DLL)|DllNotFoundException|No test matches|filter matched 0 tests|Total tests:\s*0)\b'
    },
    [pscustomobject]@{
        Code = 'timeout'
        Pattern = '(?im)\b(?:timed out|TimeoutException|timeout (?:waiting|expired|failure|exceeded)|operation canceled before tests completed)\b'
    },
    [pscustomobject]@{
        Code = 'missing-baseline'
        Pattern = '(?im)\b(?:Baseline snapshot not yet created|missing (?:snapshot )?baseline|baseline (?:file )?(?:not found|missing)|no committed baseline|snapshot size mismatch)\b'
    }
)

function Get-PathComparison {
    if ([System.OperatingSystem]::IsWindows()) {
        return [System.StringComparison]::OrdinalIgnoreCase
    }

    return [System.StringComparison]::Ordinal
}

function Assert-NoLinkInExistingPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $current = [System.IO.Path]::GetFullPath($Path)
    while ($current) {
        if (Test-Path -LiteralPath $current) {
            $item = Get-Item -LiteralPath $current -Force
            $linkTargetProperty = $item.PSObject.Properties['LinkTarget']
            $hasLinkTarget = $linkTargetProperty -and -not [string]::IsNullOrEmpty([string]$linkTargetProperty.Value)
            if (
                (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) -or
                $hasLinkTarget
            ) {
                throw 'Symbolic links and reparse points are not allowed in validation paths.'
            }
        }

        $parent = [System.IO.Directory]::GetParent($current)
        if ($null -eq $parent -or $parent.FullName -ceq $current) {
            break
        }
        $current = $parent.FullName
    }
}

function Assert-PathWithinRoot {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root,
        [string]$Context = 'Path'
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullRoot = [System.IO.Path]::GetFullPath($Root)
    $rootPrefix = $fullRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    ) + [System.IO.Path]::DirectorySeparatorChar

    if (
        -not $fullPath.Equals($fullRoot, (Get-PathComparison)) -and
        -not $fullPath.StartsWith($rootPrefix, (Get-PathComparison))
    ) {
        throw "$Context is outside its trusted root."
    }

    return $fullPath
}

function Get-SafeRegularFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [long]$MinimumBytes = 1,
        [long]$MaximumBytes,
        [string]$Root,
        [string]$Context = 'File'
    )

    $fullPath = if ($Root) {
        Assert-PathWithinRoot -Path $Path -Root $Root -Context $Context
    } else {
        [System.IO.Path]::GetFullPath($Path)
    }

    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "$Context does not exist as a regular file."
    }

    Assert-NoLinkInExistingPath -Path $fullPath
    $item = Get-Item -LiteralPath $fullPath -Force
    if ($item.PSIsContainer) {
        throw "$Context must be a regular file."
    }
    if ($item.Length -lt $MinimumBytes) {
        throw "$Context is empty or smaller than the minimum allowed size."
    }
    if ($MaximumBytes -gt 0 -and $item.Length -gt $MaximumBytes) {
        throw "$Context exceeds the maximum allowed size."
    }

    return $item
}

function Read-BoundedUtf8File {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][long]$MaximumBytes,
        [long]$MinimumBytes = 1,
        [string]$Root,
        [string]$Context = 'File'
    )

    $item = Get-SafeRegularFile `
        -Path $Path `
        -MinimumBytes $MinimumBytes `
        -MaximumBytes $MaximumBytes `
        -Root $Root `
        -Context $Context
    $bytes = [System.IO.File]::ReadAllBytes($item.FullName)
    $encoding = [System.Text.UTF8Encoding]::new($false, $true)
    try {
        $text = $encoding.GetString($bytes)
    } catch {
        throw "$Context is not valid UTF-8."
    }

    if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) {
        $text = $text.Substring(1)
    }
    if ($text.IndexOf([char]0) -ge 0) {
        throw "$Context contains a NUL byte."
    }

    return $text
}

function Assert-JsonElementBounds {
    param(
        [Parameter(Mandatory = $true)][System.Text.Json.JsonElement]$Element,
        [Parameter(Mandatory = $true)][ref]$NodeCount,
        [int]$Depth = 0
    )

    if ($Depth -gt 8) {
        throw 'JSON exceeds the maximum nesting depth.'
    }

    $NodeCount.Value++
    if ($NodeCount.Value -gt 256) {
        throw 'JSON contains too many values.'
    }

    switch ($Element.ValueKind) {
        ([System.Text.Json.JsonValueKind]::Object) {
            $names = [System.Collections.Generic.HashSet[string]]::new(
                [System.StringComparer]::OrdinalIgnoreCase
            )
            $propertyCount = 0
            foreach ($property in $Element.EnumerateObject()) {
                $propertyCount++
                if ($propertyCount -gt 64) {
                    throw 'JSON object contains too many properties.'
                }
                if ($property.Name.Length -eq 0 -or $property.Name.Length -gt 64) {
                    throw 'JSON property name is empty or too long.'
                }
                if (-not $names.Add($property.Name)) {
                    throw "JSON contains a duplicate property name."
                }
                Assert-JsonElementBounds `
                    -Element $property.Value `
                    -NodeCount $NodeCount `
                    -Depth ($Depth + 1)
            }
        }
        ([System.Text.Json.JsonValueKind]::Array) {
            $arrayCount = 0
            foreach ($value in $Element.EnumerateArray()) {
                $arrayCount++
                if ($arrayCount -gt 64) {
                    throw 'JSON array contains too many values.'
                }
                Assert-JsonElementBounds `
                    -Element $value `
                    -NodeCount $NodeCount `
                    -Depth ($Depth + 1)
            }
        }
        ([System.Text.Json.JsonValueKind]::String) {
            if ($Element.GetString().Length -gt 4096) {
                throw 'JSON string exceeds the maximum length.'
            }
        }
        ([System.Text.Json.JsonValueKind]::Number) {
            if ($Element.GetRawText().Length -gt 32) {
                throw 'JSON number exceeds the maximum length.'
            }
        }
    }
}

function ConvertFrom-BoundedJson {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [string]$Context = 'JSON'
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        throw "$Context is empty."
    }

    $options = [System.Text.Json.JsonDocumentOptions]::new()
    $options.AllowTrailingCommas = $false
    $options.CommentHandling = [System.Text.Json.JsonCommentHandling]::Disallow
    $options.MaxDepth = 8

    $document = $null
    try {
        $document = [System.Text.Json.JsonDocument]::Parse($Text, $options)
        if ($document.RootElement.ValueKind -ne [System.Text.Json.JsonValueKind]::Object) {
            throw "$Context must be a JSON object."
        }
        $nodeCount = 0
        Assert-JsonElementBounds -Element $document.RootElement -NodeCount ([ref]$nodeCount)
    } catch {
        if ($_.Exception.Message -like "$Context*") {
            throw
        }
        throw "$Context is not valid bounded JSON."
    } finally {
        if ($null -ne $document) {
            $document.Dispose()
        }
    }

    try {
        return $Text | ConvertFrom-Json -Depth 8
    } catch {
        throw "$Context could not be converted from JSON."
    }
}

function Get-ObjectPropertyEntries {
    param([Parameter(Mandatory = $true)][object]$Object)

    $entries = [System.Collections.Generic.List[object]]::new()
    if ($Object -is [System.Collections.IDictionary]) {
        foreach ($key in $Object.Keys) {
            $entries.Add([pscustomobject]@{ Name = [string]$key; Value = $Object[$key] })
        }
    } else {
        foreach ($property in $Object.PSObject.Properties) {
            if ($property.MemberType -in @('NoteProperty', 'Property', 'AliasProperty', 'ScriptProperty')) {
                $entries.Add([pscustomobject]@{ Name = $property.Name; Value = $property.Value })
            }
        }
    }

    return $entries.ToArray()
}

function Find-AliasedProperty {
    param(
        [Parameter(Mandatory = $true)][object]$Object,
        [Parameter(Mandatory = $true)][string[]]$Names,
        [string]$Context = 'JSON object',
        [switch]$Required
    )

    $propertyMatches = @(
        Get-ObjectPropertyEntries -Object $Object |
            Where-Object { $_.Name -iin $Names }
    )
    if ($propertyMatches.Count -gt 1) {
        throw "$Context contains more than one alias for '$($Names[0])'."
    }
    if ($propertyMatches.Count -eq 0) {
        if ($Required) {
            throw "$Context is missing required property '$($Names[0])'."
        }
        return [pscustomobject]@{ Found = $false; Name = ''; Value = $null }
    }

    return [pscustomobject]@{
        Found = $true
        Name = $propertyMatches[0].Name
        Value = $propertyMatches[0].Value
    }
}

function Assert-KnownProperties {
    param(
        [Parameter(Mandatory = $true)][object]$Object,
        [Parameter(Mandatory = $true)][string[]]$AllowedNames,
        [string]$Context = 'JSON object'
    )

    foreach ($entry in Get-ObjectPropertyEntries -Object $Object) {
        if ($entry.Name -inotin $AllowedNames) {
            throw "$Context contains unexpected property '$($entry.Name)'."
        }
    }
}

function ConvertTo-PositiveInteger {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Context
    )

    if ($null -eq $Value) {
        throw "$Context must be a positive integer."
    }
    $text = [string]$Value
    if ($text -notmatch '^[1-9]\d*$') {
        throw "$Context must be a positive integer."
    }
    try {
        $number = [long]::Parse(
            $text,
            [System.Globalization.NumberStyles]::None,
            [System.Globalization.CultureInfo]::InvariantCulture
        )
    } catch {
        throw "$Context is outside the supported integer range."
    }

    return $number
}

function ConvertTo-BoundedSingleLine {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Context,
        [int]$MinimumLength = 1,
        [int]$MaximumLength = 512
    )

    if ($Value -isnot [string]) {
        throw "$Context must be a string."
    }
    $text = [string]$Value
    if (
        $text.Length -lt $MinimumLength -or
        $text.Length -gt $MaximumLength -or
        $text -cne $text.Trim() -or
        $text -match '[\x00-\x1F\x7F]'
    ) {
        throw "$Context is empty, untrimmed, contains controls, or exceeds its length limit."
    }

    return $text
}

function ConvertTo-NormalizedPlatform {
    param(
        [AllowNull()][object]$Value,
        [string]$Context = 'Platform'
    )

    $platformValue = (ConvertTo-BoundedSingleLine `
        -Value $Value `
        -Context $Context `
        -MaximumLength 32).ToLowerInvariant()
    switch ($platformValue) {
        'android' { return 'android' }
        'ios' { return 'ios' }
        'maccatalyst' { return 'catalyst' }
        'catalyst' { return 'catalyst' }
        'mac-catalyst' { return 'catalyst' }
        'windows' { return 'windows' }
        'winui' { return 'windows' }
        default { throw "$Context is not a supported MAUI platform." }
    }
}

function ConvertTo-NormalizedTestType {
    param([AllowNull()][object]$Value)

    $testType = ConvertTo-BoundedSingleLine `
        -Value $Value `
        -Context 'Manifest test type' `
        -MaximumLength 32
    switch -Regex ($testType) {
        '^(?i:UnitTest|unit)$' { return 'UnitTest' }
        '^(?i:XamlUnitTest|xaml)$' { return 'XamlUnitTest' }
        '^(?i:DeviceTest|device)$' { return 'DeviceTest' }
        '^(?i:UITest|ui)$' { return 'UITest' }
        default { throw 'Manifest test type must be UnitTest, XamlUnitTest, DeviceTest, or UITest.' }
    }
}

function ConvertTo-PublishedTestType {
    param([Parameter(Mandatory = $true)][string]$TestType)

    switch ($TestType) {
        'UnitTest' { return 'unit' }
        'XamlUnitTest' { return 'xaml' }
        'DeviceTest' { return 'device' }
        'UITest' { return 'ui' }
        default { throw 'Unsupported normalized test type.' }
    }
}

function ConvertTo-NormalizedPlainStep {
    param([AllowNull()][object]$Value)

    $step = ConvertTo-BoundedSingleLine `
        -Value $Value `
        -Context 'Manifest reproduction step' `
        -MaximumLength 300
    if (
        $step -match '(?i)\b(?:https?|ftps?|wss?)://' -or
        $step -match '@' -or
        $step -match '##vso\[|::'
    ) {
        throw 'Manifest reproduction step contains a URL, mention, or logging directive.'
    }

    $plain = [regex]::Replace($step, '[*_#\[\]<>`]', '')
    $plain = [regex]::Replace($plain, '\s+', ' ').Trim()
    if ([string]::IsNullOrWhiteSpace($plain)) {
        throw 'Manifest reproduction step is empty after Markdown normalization.'
    }

    return $plain
}

function Get-DisqualifyingFailureCode {
    param([AllowNull()][string]$Text)

    if ([string]::IsNullOrEmpty($Text)) {
        return ''
    }
    foreach ($entry in $script:DisqualifyingFailurePatterns) {
        if ($Text -match $entry.Pattern) {
            return $entry.Code
        }
    }

    return ''
}

function Get-TestNameFromFilter {
    param(
        [Parameter(Mandatory = $true)][string]$TestFilter,
        [AllowNull()][object]$ExplicitName
    )

    if ($null -ne $ExplicitName) {
        return ConvertTo-BoundedSingleLine `
            -Value $ExplicitName `
            -Context 'Manifest test name' `
            -MaximumLength 256
    }

    $name = $TestFilter
    if ($TestFilter -match '^(?:FullyQualifiedName|Name)[=~](?<name>.+)$') {
        $name = $Matches['name']
    }
    return ConvertTo-BoundedSingleLine `
        -Value $name `
        -Context 'Derived test name' `
        -MaximumLength 256
}

function Assert-CandidatePath {
    param(
        [AllowNull()][object]$Path,
        [Parameter(Mandatory = $true)][string]$TestType
    )

    $candidatePath = ConvertTo-BoundedSingleLine `
        -Value $Path `
        -Context 'Candidate path' `
        -MaximumLength 240
    if (
        $candidatePath.StartsWith('/') -or
        $candidatePath.StartsWith('\') -or
        $candidatePath -match '^[A-Za-z]:' -or
        $candidatePath.Contains('\') -or
        $candidatePath.Contains('%') -or
        $candidatePath -notmatch '^[A-Za-z0-9._+@()/{}\[\]-]+$'
    ) {
        throw "Candidate path is absolute, non-normalized, or contains unsafe characters."
    }

    $segments = $candidatePath.Split('/')
    if ($segments.Count -lt 2) {
        throw 'Candidate path must be repository-relative.'
    }
    foreach ($segment in $segments) {
        if (
            [string]::IsNullOrEmpty($segment) -or
            $segment -in @('.', '..') -or
            $segment.Length -gt 100 -or
            $segment -match '^(?i:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\..*)?$'
        ) {
            throw 'Candidate path contains traversal, an empty segment, or a reserved name.'
        }
    }
    if ($segments -contains '.git' -or $segments -contains 'bin' -or $segments -contains 'obj') {
        throw 'Candidate path targets a prohibited repository or build directory.'
    }

    $extension = [System.IO.Path]::GetExtension($candidatePath).ToLowerInvariant()
    $allowedExtensions = switch ($TestType) {
        'UnitTest' { @('.cs') }
        'XamlUnitTest' { @('.cs', '.xaml') }
        'DeviceTest' { @('.cs') }
        'UITest' { @('.cs', '.xaml') }
    }
    if ($extension -notin $allowedExtensions) {
        throw "Candidate path has an unexpected extension for $TestType."
    }

    $allowed = switch ($TestType) {
        'UnitTest' {
            $candidatePath -cmatch '^src/(?:Controls/tests/(?:Core(?:\.Design)?\.UnitTests|BindingSourceGen\.UnitTests|SourceGen\.UnitTests)|Core/tests/UnitTests|Essentials/test/UnitTests|Graphics/tests/Graphics\.Tests|SingleProject/Resizetizer/test/UnitTests|Compatibility/Core/tests/Compatibility\.UnitTests)/'
        }
        'XamlUnitTest' {
            $candidatePath -cmatch '^src/Controls/tests/Xaml\.UnitTests(?:\.(?:ExternalAssembly|InternalsHiddenAssembly|InternalsVisibleAssembly))?/'
        }
        'DeviceTest' {
            $candidatePath -cmatch '^src/(?:Controls/tests/DeviceTests|Core/tests/DeviceTests(?:\.Shared)?|Essentials/test/DeviceTests|Graphics/tests/DeviceTests|BlazorWebView/tests/DeviceTests)/'
        }
        'UITest' {
            $candidatePath -cmatch '^src/Controls/tests/(?:TestCases\.Shared\.Tests|TestCases\.HostApp)/'
        }
        default { $false }
    }
    if (-not $allowed) {
        throw "Candidate path is outside the established $TestType directories."
    }

    if (
        $candidatePath -match '(?i)/(?:snapshots?|baselines?)/' -or
        [System.IO.Path]::GetFileName($candidatePath) -match '(?i)^(?:Directory\.Build|AssemblyInfo|GlobalUsings)\.'
    ) {
        throw 'Candidate path targets generated, baseline, or project infrastructure content.'
    }

    return $candidatePath
}

function Assert-CandidatePathAppliesToPlatform {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Platform
    )

    $fileName = [System.IO.Path]::GetFileName($Path)
    $requiredPlatform = $null
    if ($fileName -match '(?i)\.android\.cs$' -or $Path -match '(?i)/Platforms?/Android/') {
        $requiredPlatform = @('android')
    } elseif ($fileName -match '(?i)\.windows\.cs$' -or $Path -match '(?i)/Platforms?/Windows/') {
        $requiredPlatform = @('windows')
    } elseif ($fileName -match '(?i)\.maccatalyst\.cs$' -or $Path -match '(?i)/Platforms?/MacCatalyst/') {
        $requiredPlatform = @('catalyst')
    } elseif ($fileName -match '(?i)\.ios\.cs$' -or $Path -match '(?i)/Platforms?/iOS/') {
        $requiredPlatform = @('ios', 'catalyst')
    }

    if ($null -ne $requiredPlatform -and $Platform -notin $requiredPlatform) {
        throw 'Candidate path targets a different platform than the trusted replication run.'
    }
}

function Read-ReplicationManifest {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][long]$ExpectedIssueNumber,
        [Parameter(Mandatory = $true)][string]$ExpectedPlatform
    )

    $text = Read-BoundedUtf8File `
        -Path $Path `
        -MaximumBytes $script:ManifestMaxBytes `
        -Context 'Candidate manifest'
    $manifest = ConvertFrom-BoundedJson -Text $text -Context 'Candidate manifest'
    $allowedProperties = @(
        'schemaVersion', 'schema_version',
        'issueNumber', 'issue_number',
        'platform',
        'baseSha', 'base_sha', 'baseCommit',
        'status', 'blocked',
        'selectedDevice', 'selected_device',
        'attempts',
        'reproductionSteps', 'reproduction_steps',
        'expectedBehavior', 'expected_behavior',
        'observedBehavior', 'observed_behavior',
        'testType', 'test_type',
        'testName', 'test_name',
        'testFilter', 'test_filter', 'exactFilter', 'exact_filter',
        'expectedFailurePattern', 'expected_failure_pattern',
        'expectedFailureSignature', 'expected_failure_signature',
        'reproductionMarker', 'reproduction_marker',
        'proposedFiles', 'proposed_files', 'files',
        'sandboxFiles', 'sandbox_files',
        'reproductionResult', 'reproduction_result',
        'evidenceManifest', 'evidence_manifest',
        'verificationResult', 'verification_result',
        'patch'
    )
    Assert-KnownProperties `
        -Object $manifest `
        -AllowedNames $allowedProperties `
        -Context 'Candidate manifest'

    $schema = Find-AliasedProperty `
        -Object $manifest `
        -Names @('schemaVersion', 'schema_version') `
        -Context 'Candidate manifest'
    if ($schema.Found -and (ConvertTo-PositiveInteger -Value $schema.Value -Context 'Manifest schema version') -ne 1) {
        throw 'Candidate manifest schema version must be 1.'
    }

    $manifestIssue = ConvertTo-PositiveInteger `
        -Value (Find-AliasedProperty `
            -Object $manifest `
            -Names @('issueNumber', 'issue_number') `
            -Context 'Candidate manifest' `
            -Required).Value `
        -Context 'Manifest issue number'
    if ($manifestIssue -ne $ExpectedIssueNumber) {
        throw 'Candidate manifest issue number does not match the trusted issue number.'
    }

    $trustedPlatform = ConvertTo-NormalizedPlatform -Value $ExpectedPlatform -Context 'Trusted platform'
    $manifestPlatform = ConvertTo-NormalizedPlatform `
        -Value (Find-AliasedProperty `
            -Object $manifest `
            -Names @('platform') `
            -Context 'Candidate manifest' `
            -Required).Value `
        -Context 'Manifest platform'
    if ($manifestPlatform -cne $trustedPlatform) {
        throw 'Candidate manifest platform does not match the trusted platform.'
    }

    $isArtifactContract = (
        (Find-AliasedProperty `
            -Object $manifest `
            -Names @('status') `
            -Context 'Candidate manifest').Found -or
        (Find-AliasedProperty `
            -Object $manifest `
            -Names @('baseSha', 'base_sha', 'baseCommit') `
            -Context 'Candidate manifest').Found
    )

    $baseSha = ''
    $reproductionSteps = @()
    $selectedDeviceId = ''
    if ($isArtifactContract) {
        $status = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $manifest `
                -Names @('status') `
                -Context 'Candidate manifest' `
                -Required).Value `
            -Context 'Manifest status' `
            -MaximumLength 32
        if ($status -cne 'reproduced') {
            throw 'Candidate manifest status must be reproduced.'
        }
        $blocked = Find-AliasedProperty `
            -Object $manifest `
            -Names @('blocked') `
            -Context 'Candidate manifest' `
            -Required
        if ($null -ne $blocked.Value) {
            throw 'A reproduced candidate manifest must have blocked set to null.'
        }

        $baseSha = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $manifest `
                -Names @('baseSha', 'base_sha', 'baseCommit') `
                -Context 'Candidate manifest' `
                -Required).Value `
            -Context 'Manifest base SHA' `
            -MaximumLength 64
        if ($baseSha -notmatch '^[0-9a-fA-F]{40,64}$') {
            throw 'Manifest base SHA must be a full hexadecimal commit ID.'
        }
        $baseSha = $baseSha.ToLowerInvariant()

        $selectedDevice = (Find-AliasedProperty `
            -Object $manifest `
            -Names @('selectedDevice', 'selected_device') `
            -Context 'Candidate manifest' `
            -Required).Value
        Assert-KnownProperties `
            -Object $selectedDevice `
            -AllowedNames @('id', 'name', 'osVersion', 'os_version') `
            -Context 'Manifest selected device'
        $selectedDeviceId = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $selectedDevice `
                -Names @('id') `
                -Context 'Manifest selected device' `
                -Required).Value `
            -Context 'Manifest selected device id' `
            -MaximumLength 256
        $null = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $selectedDevice `
                -Names @('name') `
                -Context 'Manifest selected device' `
                -Required).Value `
            -Context 'Manifest selected device name' `
            -MaximumLength 256
        $null = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $selectedDevice `
                -Names @('osVersion', 'os_version') `
                -Context 'Manifest selected device' `
                -Required).Value `
            -Context 'Manifest selected device OS version' `
            -MaximumLength 128

        $attempts = (Find-AliasedProperty `
            -Object $manifest `
            -Names @('attempts') `
            -Context 'Candidate manifest' `
            -Required).Value
        Assert-KnownProperties `
            -Object $attempts `
            -AllowedNames @('sandbox', 'automatedTest', 'automated_test') `
            -Context 'Manifest attempts'
        $sandboxAttemptCount = ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $attempts `
                -Names @('sandbox') `
                -Context 'Manifest attempts' `
                -Required).Value `
            -Context 'Manifest sandbox attempts'
        if ($sandboxAttemptCount -gt 5) {
            throw 'Manifest Sandbox attempt count must be between 1 and 5.'
        }
        $testAttemptCount = ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $attempts `
                -Names @('automatedTest', 'automated_test') `
                -Context 'Manifest attempts' `
                -Required).Value `
            -Context 'Manifest automatedTest attempts'
        if ($testAttemptCount -gt 5) {
            throw 'Manifest automated test attempt count must be between 1 and 5.'
        }

        $stepsProperty = Find-AliasedProperty `
            -Object $manifest `
            -Names @('reproductionSteps', 'reproduction_steps') `
            -Context 'Candidate manifest' `
            -Required
        $rawSteps = @($stepsProperty.Value)
        if ($rawSteps.Count -lt 1 -or $rawSteps.Count -gt 10) {
            throw 'Manifest reproduction steps must contain between 1 and 10 entries.'
        }
        $reproductionSteps = @(
            $rawSteps | ForEach-Object { ConvertTo-NormalizedPlainStep -Value $_ }
        )

        $null = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $manifest `
                -Names @('expectedBehavior', 'expected_behavior') `
                -Context 'Candidate manifest' `
                -Required).Value `
            -Context 'Manifest expected behavior' `
            -MaximumLength 1000
        $null = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $manifest `
                -Names @('observedBehavior', 'observed_behavior') `
                -Context 'Candidate manifest' `
                -Required).Value `
            -Context 'Manifest observed behavior' `
            -MaximumLength 1000

        $sandboxFiles = (Find-AliasedProperty `
            -Object $manifest `
            -Names @('sandboxFiles', 'sandbox_files') `
            -Context 'Candidate manifest' `
            -Required).Value
        Assert-KnownProperties `
            -Object $sandboxFiles `
            -AllowedNames @('xaml', 'codeBehind', 'code_behind', 'appiumPlan', 'appium_plan') `
            -Context 'Manifest sandbox files'
        $expectedSandboxFiles = [ordered]@{
            xaml = @('xaml')
            'codeBehind' = @('codeBehind', 'code_behind')
            appiumPlan = @('appiumPlan', 'appium_plan')
        }
        $expectedSandboxValues = @{
            xaml = 'sandbox/MainPage.xaml'
            codeBehind = 'sandbox/MainPage.xaml.cs'
            appiumPlan = 'sandbox/appium-plan.json'
        }
        foreach ($sandboxEntry in $expectedSandboxFiles.GetEnumerator()) {
            $sandboxValue = ConvertTo-BoundedSingleLine `
                -Value (Find-AliasedProperty `
                    -Object $sandboxFiles `
                    -Names $sandboxEntry.Value `
                    -Context 'Manifest sandbox files' `
                    -Required).Value `
                -Context "Manifest sandbox $($sandboxEntry.Key) path" `
                -MaximumLength 128
            if ($sandboxValue -cne $expectedSandboxValues[$sandboxEntry.Key]) {
                throw 'Manifest sandbox file paths do not match the fixed artifact contract.'
            }
        }

        $fixedArtifactPaths = @(
            @{
                Names = @('reproductionResult', 'reproduction_result')
                Expected = 'reproduction-result.json'
            },
            @{
                Names = @('evidenceManifest', 'evidence_manifest')
                Expected = 'evidence/evidence.json'
            },
            @{
                Names = @('verificationResult', 'verification_result')
                Expected = 'verification/verification-result.json'
            },
            @{
                Names = @('patch')
                Expected = 'test.patch'
            }
        )
        foreach ($artifactPath in $fixedArtifactPaths) {
            $value = ConvertTo-BoundedSingleLine `
                -Value (Find-AliasedProperty `
                    -Object $manifest `
                    -Names $artifactPath.Names `
                    -Context 'Candidate manifest' `
                    -Required).Value `
                -Context 'Manifest artifact path' `
                -MaximumLength 128
            if ($value -cne $artifactPath.Expected) {
                throw 'Manifest artifact paths do not match the fixed artifact contract.'
            }
        }
    }

    $testType = ConvertTo-NormalizedTestType `
        -Value (Find-AliasedProperty `
            -Object $manifest `
            -Names @('testType', 'test_type') `
            -Context 'Candidate manifest' `
            -Required).Value

    $testFilter = ConvertTo-BoundedSingleLine `
        -Value (Find-AliasedProperty `
            -Object $manifest `
            -Names @('testFilter', 'test_filter', 'exactFilter', 'exact_filter') `
            -Context 'Candidate manifest' `
            -Required).Value `
        -Context 'Manifest test filter' `
        -MaximumLength 1000
    if (
        $testFilter.StartsWith('-') -or
        $testFilter -notmatch '^[A-Za-z0-9_.:+~=-]+$' -or
        $testFilter -match '[|&;`$<>()]'
    ) {
        throw 'Manifest test filter is not an exact safe filter.'
    }
    if ($isArtifactContract) {
        $expectedFilter = if ($testType -ceq 'XamlUnitTest') {
            "Maui$ExpectedIssueNumber"
        } else {
            "Issue$ExpectedIssueNumber"
        }
        if ($testFilter -cne $expectedFilter) {
            throw 'Candidate manifest does not use the exact issue-specific test filter.'
        }
    }

    $testNameProperty = Find-AliasedProperty `
        -Object $manifest `
        -Names @('testName', 'test_name') `
        -Context 'Candidate manifest'
    $testName = Get-TestNameFromFilter `
        -TestFilter $testFilter `
        -ExplicitName $(if ($testNameProperty.Found) { $testNameProperty.Value } else { $null })
    if ($testName -notmatch '^[A-Za-z0-9_.+]+$') {
        throw 'Manifest test name contains unsupported characters.'
    }

    $failurePattern = ConvertTo-BoundedSingleLine `
        -Value (Find-AliasedProperty `
            -Object $manifest `
            -Names @(
                'expectedFailurePattern',
                'expected_failure_pattern',
                'expectedFailureSignature',
                'expected_failure_signature'
            ) `
            -Context 'Candidate manifest' `
            -Required).Value `
        -Context 'Manifest expected failure pattern' `
        -MinimumLength 3 `
        -MaximumLength 1000
    if (Get-DisqualifyingFailureCode -Text $failurePattern) {
        throw 'Manifest expected failure pattern describes a build, infrastructure, timeout, or missing-baseline failure.'
    }

    $markerProperty = Find-AliasedProperty `
        -Object $manifest `
        -Names @('reproductionMarker', 'reproduction_marker') `
        -Context 'Candidate manifest'
    $sourceMarker = if ($markerProperty.Found) {
        ConvertTo-BoundedSingleLine `
            -Value $markerProperty.Value `
            -Context 'Manifest reproduction marker' `
            -MaximumLength 96
    } else {
        "MAUI_REPRODUCTION_ISSUE == $ExpectedIssueNumber"
    }
    $allowedMarkers = @(
        "MAUI_REPRODUCTION_ISSUE == $ExpectedIssueNumber",
        "MAUI_REPRODUCTION_ISSUE: $ExpectedIssueNumber",
        "MAUI_REPRODUCTION_ISSUE: #$ExpectedIssueNumber",
        "MAUI_REPRODUCTION_ISSUE=$ExpectedIssueNumber"
    )
    if ($sourceMarker -cnotin $allowedMarkers) {
        throw 'Manifest reproduction marker is not the exact issue-keyed MAUI_REPRODUCTION_ISSUE guard.'
    }

    $proposedProperty = Find-AliasedProperty `
        -Object $manifest `
        -Names @('proposedFiles', 'proposed_files', 'files') `
        -Context 'Candidate manifest' `
        -Required
    $rawProposed = @($proposedProperty.Value)
    if ($rawProposed.Count -eq 0 -or $rawProposed.Count -gt $script:CandidateFileMaxCount) {
        throw 'Manifest proposed files must contain between 1 and 24 entries.'
    }

    $seen = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )
    $proposedFiles = [System.Collections.Generic.List[string]]::new()
    foreach ($pathValue in $rawProposed) {
        $normalizedPath = Assert-CandidatePath -Path $pathValue -TestType $testType
        if (-not $seen.Add($normalizedPath)) {
            throw 'Manifest proposed files contain a duplicate path.'
        }
        Assert-CandidatePathAppliesToPlatform `
            -Path $normalizedPath `
            -Platform $manifestPlatform
        $proposedFiles.Add($normalizedPath)
    }

    return [pscustomobject]@{
        IssueNumber = $manifestIssue
        Platform = $manifestPlatform
        TestType = $testType
        TestName = $testName
        TestFilter = $testFilter
        ExpectedFailurePattern = $failurePattern
        SourceMarker = $sourceMarker
        ReproductionMarker = "MAUI_REPRODUCTION_ISSUE == $ExpectedIssueNumber"
        ProposedFiles = @($proposedFiles.ToArray() | Sort-Object)
        BaseSha = $baseSha
        PublishedTestType = ConvertTo-PublishedTestType -TestType $testType
        ReproductionSteps = @($reproductionSteps)
        SelectedDeviceId = $selectedDeviceId
        ArtifactContract = $isArtifactContract
    }
}

function Get-Sha256Hex {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    $hash = [System.Security.Cryptography.SHA256]::HashData($Bytes)
    return [System.Convert]::ToHexString($hash).ToLowerInvariant()
}

function Get-CandidateFilesFromPatch {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$TestType
    )

    $patchText = Read-BoundedUtf8File `
        -Path $Path `
        -MaximumBytes $script:PatchMaxBytes `
        -Context 'Candidate patch'
    if ($patchText.Contains("`r`n")) {
        $patchText = $patchText.Replace("`r`n", "`n")
    }
    if ($patchText.Contains("`r")) {
        throw 'Candidate patch contains non-normalized line endings.'
    }
    if ($patchText -match '(?m)^(?:GIT binary patch|Binary files .* differ|literal \d+|delta \d+)$') {
        throw 'Candidate patch contains a binary patch.'
    }

    if ($patchText.EndsWith("`n", [System.StringComparison]::Ordinal)) {
        $patchText = $patchText.Substring(0, $patchText.Length - 1)
    }
    $lines = $patchText.Split([char]"`n")
    if ($lines.Count -eq 0 -or [string]::IsNullOrWhiteSpace($patchText)) {
        throw 'Candidate patch is empty.'
    }

    $files = [System.Collections.Generic.List[object]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )
    $totalBytes = 0L
    $index = 0
    while ($index -lt $lines.Count) {
        $header = $lines[$index]
        if ($header -notmatch '^diff --git a/(?<old>[^\s]+) b/(?<new>[^\s]+)$') {
            throw 'Candidate patch contains an unexpected preamble or malformed diff header.'
        }
        $oldPath = $Matches['old']
        $newPath = $Matches['new']
        if ($oldPath -cne $newPath) {
            throw 'Candidate patch contains a rename or path mismatch.'
        }
        $candidatePath = Assert-CandidatePath -Path $newPath -TestType $TestType
        if (-not $seen.Add($candidatePath)) {
            throw 'Candidate patch contains a duplicate path.'
        }

        $index++
        $block = [System.Collections.Generic.List[string]]::new()
        while ($index -lt $lines.Count -and -not $lines[$index].StartsWith('diff --git ', [System.StringComparison]::Ordinal)) {
            $block.Add($lines[$index])
            $index++
        }

        $hasNewMode = $false
        $hasIndex = $false
        $hasOldHeader = $false
        $hasNewHeader = $false
        $hasHunk = $false
        $expectedAddedLines = -1
        $addedLines = [System.Collections.Generic.List[string]]::new()
        $noFinalNewline = $false
        $insideHunk = $false

        for ($blockIndex = 0; $blockIndex -lt $block.Count; $blockIndex++) {
            $line = $block[$blockIndex]
            if ($insideHunk) {
                if ($line -ceq '\ No newline at end of file') {
                    if ($noFinalNewline -or $blockIndex -ne ($block.Count - 1)) {
                        throw 'Candidate patch has a malformed no-newline marker.'
                    }
                    $noFinalNewline = $true
                    continue
                }
                if (-not $line.StartsWith('+', [System.StringComparison]::Ordinal)) {
                    throw 'Candidate patch is not add-only.'
                }
                $addedLines.Add($line.Substring(1))
                continue
            }

            if ($line -ceq 'new file mode 100644') {
                if ($hasNewMode) {
                    throw 'Candidate patch repeats its file mode.'
                }
                $hasNewMode = $true
                continue
            }
            if ($line -match '^new file mode (?<mode>\d+)$') {
                throw 'Candidate patch adds an executable, symlink, submodule, or unsupported mode.'
            }
            if ($line -match '^(?:old mode|deleted file mode|rename from|rename to|copy from|copy to|similarity index|dissimilarity index|Submodule )') {
                throw 'Candidate patch contains an edit, delete, rename, copy, or submodule change.'
            }
            if ($line -match '^index 0+\.\.[0-9a-fA-F]{7,64}(?: 100644)?$') {
                if ($hasIndex) {
                    throw 'Candidate patch repeats its index line.'
                }
                $hasIndex = $true
                continue
            }
            if ($line -ceq '--- /dev/null') {
                $hasOldHeader = $true
                continue
            }
            if ($line -ceq "+++ b/$candidatePath") {
                $hasNewHeader = $true
                continue
            }
            if ($line -match '^@@ -0,0 \+1(?:,(?<count>\d+))? @@(?: .*)?$') {
                if ($hasHunk) {
                    throw 'Candidate patch contains multiple or overlapping hunks.'
                }
                $hasHunk = $true
                $insideHunk = $true
                $expectedAddedLines = if ($Matches['count']) { [int]$Matches['count'] } else { 1 }
                continue
            }
            if ([string]::IsNullOrEmpty($line) -and $blockIndex -eq ($block.Count - 1)) {
                continue
            }

            throw 'Candidate patch contains unsupported metadata or a malformed add-only hunk.'
        }

        if (
            -not $hasNewMode -or
            -not $hasIndex -or
            -not $hasOldHeader -or
            -not $hasNewHeader -or
            -not $hasHunk
        ) {
            throw 'Candidate patch does not describe a complete add-only regular file.'
        }
        if ($expectedAddedLines -le 0 -or $addedLines.Count -ne $expectedAddedLines) {
            throw 'Candidate patch hunk line counts do not match its content.'
        }

        $content = $addedLines -join "`n"
        if (-not $noFinalNewline) {
            $content += "`n"
        }
        $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($content)
        if ($bytes.Length -eq 0 -or $bytes.Length -gt $script:CandidateFileMaxBytes) {
            throw 'Candidate patch adds an empty or oversized file.'
        }

        $totalBytes += $bytes.Length
        if ($totalBytes -gt $script:CandidateTotalMaxBytes) {
            throw 'Candidate patch exceeds the total added-file size limit.'
        }
        if ($files.Count -ge $script:CandidateFileMaxCount) {
            throw 'Candidate patch adds too many files.'
        }

        $files.Add([pscustomobject]@{
            Path = $candidatePath
            Mode = '100644'
            Content = $content
            Size = [long]$bytes.Length
            Sha256 = Get-Sha256Hex -Bytes $bytes
        })
    }

    if ($files.Count -eq 0) {
        throw 'Candidate patch contains no added test files.'
    }

    return @($files.ToArray() | Sort-Object Path)
}

function Invoke-ExternalBytes {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [string]$WorkingDirectory,
        [int]$TimeoutSeconds = 30,
        [int]$MaximumOutputBytes = 1MB
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    if ($WorkingDirectory) {
        $startInfo.WorkingDirectory = $WorkingDirectory
    }
    foreach ($argument in $Arguments) {
        $null = $startInfo.ArgumentList.Add($argument)
    }
    $startInfo.Environment['GIT_TERMINAL_PROMPT'] = '0'
    $startInfo.Environment['LC_ALL'] = 'C'

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $memory = [System.IO.MemoryStream]::new()
    try {
        if (-not $process.Start()) {
            throw "Could not start '$FileName'."
        }
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $buffer = [byte[]]::new(8192)
        $cancellation = [System.Threading.CancellationTokenSource]::new(
            [System.TimeSpan]::FromSeconds($TimeoutSeconds)
        )
        try {
            while ($true) {
                $read = $process.StandardOutput.BaseStream.ReadAsync(
                    $buffer,
                    0,
                    $buffer.Length,
                    $cancellation.Token
                ).GetAwaiter().GetResult()
                if ($read -eq 0) {
                    break
                }
                if (($memory.Length + $read) -gt $MaximumOutputBytes) {
                    try { $process.Kill($true) } catch { $null = $_ }
                    throw "'$FileName' produced more output than allowed."
                }
                $memory.Write($buffer, 0, $read)
            }
        } catch [System.OperationCanceledException] {
            try { $process.Kill($true) } catch { $null = $_ }
            throw "'$FileName' exceeded its timeout."
        } finally {
            $cancellation.Dispose()
        }

        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            try { $process.Kill($true) } catch { $null = $_ }
            throw "'$FileName' exceeded its timeout."
        }
        $stderr = $stderrTask.GetAwaiter().GetResult()
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            Bytes = $memory.ToArray()
            StandardError = $stderr
        }
    } finally {
        $memory.Dispose()
        $process.Dispose()
    }
}

function Invoke-GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [int]$MaximumOutputBytes = 1MB
    )

    $allArguments = @('-C', $Repository, '-c', 'core.quotePath=false') + $Arguments
    $result = Invoke-ExternalBytes `
        -FileName 'git' `
        -Arguments $allArguments `
        -WorkingDirectory $Repository `
        -MaximumOutputBytes $MaximumOutputBytes
    if ($result.ExitCode -ne 0) {
        throw 'A required git validation command failed.'
    }
    try {
        return [System.Text.UTF8Encoding]::new($false, $true).GetString($result.Bytes)
    } catch {
        throw 'A required git command returned non-UTF-8 output.'
    }
}

function Get-CandidateFilesFromCommits {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][string]$Candidate,
        [Parameter(Mandatory = $true)][string]$Base,
        [Parameter(Mandatory = $true)][string]$TestType
    )

    $repositoryPath = [System.IO.Path]::GetFullPath($Repository)
    if (-not (Test-Path -LiteralPath $repositoryPath -PathType Container)) {
        throw 'RepoRoot does not exist as a directory.'
    }
    Assert-NoLinkInExistingPath -Path $repositoryPath

    $topLevel = (Invoke-GitText `
        -Repository $repositoryPath `
        -Arguments @('rev-parse', '--show-toplevel') `
        -MaximumOutputBytes 16KB).Trim()
    if (
        -not [System.IO.Path]::GetFullPath($topLevel).Equals(
            $repositoryPath.TrimEnd(
                [System.IO.Path]::DirectorySeparatorChar,
                [System.IO.Path]::AltDirectorySeparatorChar
            ),
            (Get-PathComparison)
        )
    ) {
        throw 'RepoRoot must be the git worktree root.'
    }

    foreach ($commitValue in @($Candidate, $Base)) {
        if ($commitValue -notmatch '^[0-9a-fA-F]{40,64}$') {
            throw 'CandidateCommit and BaseCommit must be full hexadecimal commit IDs.'
        }
    }
    $resolvedCandidate = (Invoke-GitText `
        -Repository $repositoryPath `
        -Arguments @('rev-parse', '--verify', "$Candidate^{commit}") `
        -MaximumOutputBytes 4KB).Trim()
    $resolvedBase = (Invoke-GitText `
        -Repository $repositoryPath `
        -Arguments @('rev-parse', '--verify', "$Base^{commit}") `
        -MaximumOutputBytes 4KB).Trim()
    if (
        $resolvedCandidate -notmatch '^[0-9a-f]{40,64}$' -or
        $resolvedBase -notmatch '^[0-9a-f]{40,64}$'
    ) {
        throw 'Git did not resolve the requested commits to full object IDs.'
    }

    $ancestorCheck = Invoke-ExternalBytes `
        -FileName 'git' `
        -Arguments @('-C', $repositoryPath, 'merge-base', '--is-ancestor', $resolvedBase, $resolvedCandidate) `
        -WorkingDirectory $repositoryPath `
        -MaximumOutputBytes 4KB
    if ($ancestorCheck.ExitCode -ne 0) {
        throw 'BaseCommit must be an ancestor of CandidateCommit.'
    }

    $rawDiff = Invoke-GitText `
        -Repository $repositoryPath `
        -Arguments @(
            'diff', '--raw', '--no-renames', '--no-abbrev', '--no-ext-diff', '--no-textconv',
            $resolvedBase, $resolvedCandidate, '--'
        ) `
        -MaximumOutputBytes 1MB
    $rawLines = @($rawDiff -split "`r?`n" | Where-Object { $_ })
    if ($rawLines.Count -eq 0) {
        throw 'Candidate commit contains no changed files.'
    }
    if ($rawLines.Count -gt $script:CandidateFileMaxCount) {
        throw 'Candidate commit changes too many files.'
    }

    $files = [System.Collections.Generic.List[object]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )
    $totalBytes = 0L
    foreach ($line in $rawLines) {
        if ($line -notmatch '^:(?<oldMode>[0-7]{6}) (?<newMode>[0-7]{6}) (?<old>[0-9a-f]+) (?<new>[0-9a-f]+) (?<status>[A-Z])\t(?<path>.+)$') {
            throw 'Candidate commit produced an unsupported git diff record.'
        }
        if (
            $Matches['status'] -cne 'A' -or
            $Matches['oldMode'] -cne '000000' -or
            $Matches['newMode'] -cne '100644'
        ) {
            throw 'Candidate commit must be add-only regular non-executable files.'
        }

        $candidatePath = Assert-CandidatePath -Path $Matches['path'] -TestType $TestType
        if (-not $seen.Add($candidatePath)) {
            throw 'Candidate commit contains duplicate or case-colliding paths.'
        }
        $objectId = $Matches['new']
        $sizeText = (Invoke-GitText `
            -Repository $repositoryPath `
            -Arguments @('cat-file', '-s', $objectId) `
            -MaximumOutputBytes 4KB).Trim()
        if ($sizeText -notmatch '^\d+$') {
            throw 'Candidate blob size is not valid.'
        }
        $size = [long]$sizeText
        if ($size -le 0 -or $size -gt $script:CandidateFileMaxBytes) {
            throw 'Candidate commit adds an empty or oversized file.'
        }
        $totalBytes += $size
        if ($totalBytes -gt $script:CandidateTotalMaxBytes) {
            throw 'Candidate commit exceeds the total added-file size limit.'
        }

        $blob = Invoke-ExternalBytes `
            -FileName 'git' `
            -Arguments @('-C', $repositoryPath, 'cat-file', 'blob', $objectId) `
            -WorkingDirectory $repositoryPath `
            -MaximumOutputBytes ([int]($size + 1))
        if ($blob.ExitCode -ne 0 -or $blob.Bytes.Length -ne $size) {
            throw 'Candidate blob could not be read exactly.'
        }
        try {
            $content = [System.Text.UTF8Encoding]::new($false, $true).GetString($blob.Bytes)
        } catch {
            throw 'Candidate commit contains a binary or non-UTF-8 file.'
        }
        if ($content.IndexOf([char]0) -ge 0) {
            throw 'Candidate commit contains a binary file.'
        }

        $files.Add([pscustomobject]@{
            Path = $candidatePath
            Mode = '100644'
            Content = $content
            Size = $size
            Sha256 = Get-Sha256Hex -Bytes $blob.Bytes
        })
    }

    return [pscustomobject]@{
        Files = @($files.ToArray() | Sort-Object Path)
        CandidateCommit = $resolvedCandidate
        BaseCommit = $resolvedBase
    }
}

function Assert-ManifestMatchesCandidateFiles {
    param(
        [Parameter(Mandatory = $true)][object]$Manifest,
        [Parameter(Mandatory = $true)][object[]]$CandidateFiles
    )

    $expected = @($Manifest.ProposedFiles | Sort-Object)
    $actual = @($CandidateFiles.Path | Sort-Object)
    if ($expected.Count -ne $actual.Count) {
        throw 'Manifest proposed files do not exactly match the candidate diff.'
    }

    for ($index = 0; $index -lt $expected.Count; $index++) {
        if ($expected[$index] -cne $actual[$index]) {
            throw 'Manifest proposed files do not exactly match the candidate diff.'
        }
    }
}

function Assert-PatchCandidatePathsAreNew {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][object[]]$CandidateFiles
    )

    $repositoryPath = [System.IO.Path]::GetFullPath($Repository)
    foreach ($file in $CandidateFiles) {
        $fullPath = Assert-PathWithinRoot `
            -Path (Join-Path $repositoryPath $file.Path) `
            -Root $repositoryPath `
            -Context 'Candidate patch path'
        if (Test-Path -LiteralPath $fullPath) {
            throw 'Candidate patch claims an existing repository path as a new file.'
        }
        $parent = [System.IO.Path]::GetDirectoryName($fullPath)
        if ($parent) {
            Assert-NoLinkInExistingPath -Path $parent
        }
    }
}

function Assert-SourceTextIsSafe {
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][object]$Manifest,
        [switch]$RequireGuard
    )

    $normalized = $Content.Replace("`r`n", "`n")
    if ($normalized.Contains("`r")) {
        throw "Candidate source '$Path' contains non-normalized line endings."
    }
    foreach ($character in $normalized.ToCharArray()) {
        $codePoint = [int]$character
        if (($codePoint -lt 32 -and $character -notin @("`n", "`t")) -or $codePoint -eq 127) {
            throw "Candidate source '$Path' contains prohibited control characters."
        }
    }
    if ($normalized -match '\\(?:u[0-9a-fA-F]{4}|U[0-9a-fA-F]{8})') {
        throw "Candidate source '$Path' contains Unicode escapes that could obscure denied code."
    }

    $normalizedPath = $Path.Replace('\', '/')
    if (
        [System.IO.Path]::GetExtension($Path) -ieq '.cs' -and
        $normalizedPath -cnotmatch '^src/Controls/tests/TestCases\.HostApp/'
    ) {
        Assert-ReplicationTestLifecycleSafety `
            -Content $normalized `
            -Path $Path
    }

    if ($RequireGuard) {
        Assert-ReplicationTestGuard `
            -Content $normalized `
            -Path $Path `
            -IssueNumber ([long]$Manifest.IssueNumber) `
            -TestType ([string]$Manifest.TestType)
    }

    Assert-ReplicationGeneratedSourceSafety -Content $normalized -Path $Path
}

function Assert-ReplicationCandidateSources {
    param(
        [Parameter(Mandatory = $true)][object]$Manifest,
        [Parameter(Mandatory = $true)][object[]]$CandidateFiles
    )

    Assert-ManifestMatchesCandidateFiles -Manifest $Manifest -CandidateFiles $CandidateFiles

    $testAttributeFound = $false
    $testNameFound = $false
    $testNameLeaf = @($Manifest.TestName -split '[.+]')[-1]
    $testNamePattern = "(?<![A-Za-z0-9_])$([regex]::Escape($testNameLeaf))(?![A-Za-z0-9_])"
    foreach ($file in $CandidateFiles) {
        if ($file.Mode -cne '100644') {
            throw 'Candidate file mode must be regular and non-executable.'
        }
        $isTestSource = $false
        if ([System.IO.Path]::GetExtension($file.Path) -ieq '.cs') {
            $testAttributeMatches = @([regex]::Matches(
                $file.Content,
                '(?m)^\s*\[\s*(?:(?:[A-Za-z_]\w*)\.)*(?:Fact|Test)\b'
            ))
            if ($testAttributeMatches.Count -gt 1) {
                throw "Candidate source '$($file.Path)' adds more than one targeted test method."
            }
            if ($testAttributeMatches.Count -eq 1) {
                if (
                    $Manifest.TestType -cne 'UITest' -or
                    $file.Path -cmatch '^src/Controls/tests/TestCases\.Shared\.Tests/'
                ) {
                    $testAttributeFound = $true
                    $isTestSource = $true
                }
            }
            if ($file.Content -match $testNamePattern) {
                $testNameFound = $true
            }
        }
        Assert-SourceTextIsSafe `
            -Content $file.Content `
            -Path $file.Path `
            -Manifest $Manifest `
            -RequireGuard:$isTestSource
    }

    if (-not $testAttributeFound) {
        throw 'Candidate files do not add a test method in the expected test project.'
    }
    if (-not $testNameFound) {
        throw 'Candidate files do not contain the named test from the exact filter.'
    }
}

function Get-ReplicationEvidenceInventory {
    param([Parameter(Mandatory = $true)][string]$Directory)

    $inputRoot = [System.IO.Path]::GetFullPath($Directory)
    if (-not (Test-Path -LiteralPath $inputRoot -PathType Container)) {
        throw 'EvidenceDir does not exist as a directory.'
    }
    Assert-NoLinkInExistingPath -Path $inputRoot

    $nestedEvidence = Join-Path $inputRoot 'evidence'
    $nestedVerification = Join-Path $inputRoot 'verification'
    if (
        (Test-Path -LiteralPath $nestedEvidence -PathType Container) -and
        (Test-Path -LiteralPath $nestedVerification -PathType Container)
    ) {
        $mediaRoot = [System.IO.Path]::GetFullPath($nestedEvidence)
        $verificationRoot = [System.IO.Path]::GetFullPath($nestedVerification)
    } else {
        $mediaRoot = $inputRoot
        $siblingVerification = Join-Path ([System.IO.Path]::GetDirectoryName($inputRoot)) 'verification'
        $verificationRoot = if (
            [System.IO.Path]::GetFileName($inputRoot) -ceq 'evidence' -and
            (Test-Path -LiteralPath $siblingVerification -PathType Container)
        ) {
            [System.IO.Path]::GetFullPath($siblingVerification)
        } else {
            $inputRoot
        }
    }
    Assert-NoLinkInExistingPath -Path $mediaRoot
    Assert-NoLinkInExistingPath -Path $verificationRoot

    $allowedMediaNames = @(
        'evidence.json',
        'repro.mp4',
        'preview.gif',
        'thumbnail.png'
    )
    $allowedVerificationNames = @(
        'verification-report.md',
        'verification-log.txt',
        'verification-output.log',
        'verification-console.log',
        'verification-result.json',
        'verify-tests-fail.log',
        'test-without-fix.log'
    )

    $mediaItems = @(Get-ChildItem -LiteralPath $mediaRoot -Force)
    if ($mediaItems.Count -gt 20) {
        throw 'Evidence directory contains too many artifacts.'
    }
    foreach ($item in $mediaItems) {
        $isCombinedVerificationFile = (
            $verificationRoot -ceq $mediaRoot -and
            (
                $item.Name -cin $allowedVerificationNames -or
                $item.Name -cmatch '^test-failure-[A-Za-z0-9_.-]+\.log$'
            )
        )
        if ($item.PSIsContainer) {
            throw 'Evidence directory must not contain nested directories.'
        }
        if ($item.Name -cnotin $allowedMediaNames -and -not $isCombinedVerificationFile) {
            throw 'Evidence directory contains an unexpected artifact.'
        }
        $null = Get-SafeRegularFile `
            -Path $item.FullName `
            -MinimumBytes 1 `
            -MaximumBytes $(
                switch -Regex ($item.Name) {
                    '^repro\.mp4$' { $script:VideoMaxBytes; break }
                    '^(?:preview\.gif|thumbnail\.png)$' { $script:PreviewMaxBytes; break }
                    '^evidence\.json$' { $script:EvidenceJsonMaxBytes; break }
                    default { $script:VerificationArtifactMaxBytes }
                }
            ) `
            -Root $mediaRoot `
            -Context "Evidence artifact"
    }

    foreach ($requiredName in @('evidence.json', 'repro.mp4')) {
        if (-not (Test-Path -LiteralPath (Join-Path $mediaRoot $requiredName) -PathType Leaf)) {
            throw "EvidenceDir is missing required artifact '$requiredName'."
        }
    }
    $hasGif = Test-Path -LiteralPath (Join-Path $mediaRoot 'preview.gif') -PathType Leaf
    $hasThumbnail = Test-Path -LiteralPath (Join-Path $mediaRoot 'thumbnail.png') -PathType Leaf
    if (-not $hasGif -and -not $hasThumbnail) {
        throw 'EvidenceDir must contain preview.gif or thumbnail.png.'
    }

    $verificationItems = if ($verificationRoot -ceq $mediaRoot) {
        @($mediaItems | Where-Object {
            $_.Name -cin $allowedVerificationNames -or
            $_.Name -cmatch '^test-failure-[A-Za-z0-9_.-]+\.log$'
        })
    } else {
        @(Get-ChildItem -LiteralPath $verificationRoot -Force)
    }
    if ($verificationItems.Count -gt 20) {
        throw 'Verification directory contains too many artifacts.'
    }
    foreach ($item in $verificationItems) {
        if ($item.PSIsContainer) {
            throw 'Verification directory must not contain nested directories.'
        }
        if (
            $item.Name -cnotin $allowedVerificationNames -and
            $item.Name -cnotmatch '^test-failure-[A-Za-z0-9_.-]+\.log$'
        ) {
            throw 'Verification directory contains an unexpected artifact.'
        }
        $null = Get-SafeRegularFile `
            -Path $item.FullName `
            -MinimumBytes 1 `
            -MaximumBytes $script:VerificationArtifactMaxBytes `
            -Root $verificationRoot `
            -Context 'Verification artifact'
    }

    $hasMachineResult = Test-Path `
        -LiteralPath (Join-Path $verificationRoot 'verification-result.json') `
        -PathType Leaf
    if ($hasMachineResult) {
        foreach ($item in $verificationItems) {
            if ($item.Name -cnotin @('verification-console.log', 'verification-result.json')) {
                throw 'Machine-readable verification directory contains an unexpected artifact.'
            }
        }
        if (-not (Test-Path -LiteralPath (Join-Path $verificationRoot 'verification-console.log') -PathType Leaf)) {
            throw "Verification directory is missing required artifact 'verification-console.log'."
        }
    } else {
        foreach ($requiredName in @('verification-report.md', 'verification-log.txt')) {
            if (-not (Test-Path -LiteralPath (Join-Path $verificationRoot $requiredName) -PathType Leaf)) {
                throw "Verification directory is missing required artifact '$requiredName'."
            }
        }
    }

    $failureLogs = @(
        $verificationItems |
            Where-Object {
                $_.Name -ceq 'test-without-fix.log' -or
                $_.Name -cmatch '^test-failure-[A-Za-z0-9_.-]+\.log$'
            }
    )
    if (-not $hasMachineResult -and $failureLogs.Count -eq 0) {
        throw 'EvidenceDir must contain a bounded raw failure log.'
    }

    return [pscustomobject]@{
        Root = $inputRoot
        MediaRoot = $mediaRoot
        VerificationRoot = $verificationRoot
        MediaItems = $mediaItems
        VerificationItems = $verificationItems
        FailureLogs = $failureLogs
        PreviewName = if ($hasGif) { 'preview.gif' } else { 'thumbnail.png' }
        HasGif = $hasGif
        HasThumbnail = $hasThumbnail
        HasMachineResult = $hasMachineResult
    }
}

function Assert-ReplicationExecutionResult {
    param(
        [Parameter(Mandatory = $true)][string]$Directory,
        [Parameter(Mandatory = $true)][object]$Manifest
    )

    $root = [System.IO.Path]::GetFullPath($Directory)
    $resultPath = Join-Path $root 'reproduction-result.json'
    $resultText = Read-BoundedUtf8File `
        -Path $resultPath `
        -MaximumBytes $script:EvidenceJsonMaxBytes `
        -Root $root `
        -Context 'Replication execution result'
    $result = ConvertFrom-BoundedJson -Text $resultText -Context 'Replication execution result'
    Assert-KnownProperties `
        -Object $result `
        -AllowedNames @(
            'schemaVersion',
            'issueNumber',
            'platform',
            'baseSha',
            'attempt',
            'succeeded',
            'device',
            'evidenceManifest'
        ) `
        -Context 'Replication execution result'

    if (
        (ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('schemaVersion') `
                -Context 'Replication execution result' `
                -Required).Value `
            -Context 'Replication execution result schema version') -ne 1
    ) {
        throw 'Replication execution result schema version must be 1.'
    }
    if (
        (ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('issueNumber') `
                -Context 'Replication execution result' `
                -Required).Value `
            -Context 'Replication execution issue number') -ne $Manifest.IssueNumber
    ) {
        throw 'Replication execution result issue number does not match the manifest.'
    }
    if (
        (ConvertTo-NormalizedPlatform `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('platform') `
                -Context 'Replication execution result' `
                -Required).Value `
            -Context 'Replication execution platform') -cne $Manifest.Platform
    ) {
        throw 'Replication execution result platform does not match the manifest.'
    }

    $baseSha = ConvertTo-BoundedSingleLine `
        -Value (Find-AliasedProperty `
            -Object $result `
            -Names @('baseSha') `
            -Context 'Replication execution result' `
            -Required).Value `
        -Context 'Replication execution base SHA' `
        -MaximumLength 64
    if ($baseSha.ToLowerInvariant() -cne $Manifest.BaseSha) {
        throw 'Replication execution result base SHA does not match the manifest.'
    }
    $attempt = ConvertTo-PositiveInteger `
        -Value (Find-AliasedProperty `
            -Object $result `
            -Names @('attempt') `
            -Context 'Replication execution result' `
            -Required).Value `
        -Context 'Replication execution attempt'
    if ($attempt -gt 5) {
        throw 'Replication execution attempt must be between 1 and 5.'
    }

    $succeeded = (Find-AliasedProperty `
        -Object $result `
        -Names @('succeeded') `
        -Context 'Replication execution result' `
        -Required).Value
    if ($succeeded -isnot [bool] -or $succeeded -ne $true) {
        throw 'Replication execution result does not prove a successful trusted run.'
    }
    $device = ConvertTo-BoundedSingleLine `
        -Value (Find-AliasedProperty `
            -Object $result `
            -Names @('device') `
            -Context 'Replication execution result' `
            -Required).Value `
        -Context 'Replication execution device' `
        -MaximumLength 256
    if ($Manifest.SelectedDeviceId -and $device -cne $Manifest.SelectedDeviceId) {
        throw 'Replication execution device does not match the selected device.'
    }
    $evidenceManifest = ConvertTo-BoundedSingleLine `
        -Value (Find-AliasedProperty `
            -Object $result `
            -Names @('evidenceManifest') `
            -Context 'Replication execution result' `
            -Required).Value `
        -Context 'Replication execution evidence manifest' `
        -MaximumLength 128
    if ($evidenceManifest -cne 'evidence/evidence.json') {
        throw 'Replication execution result evidence path does not match the fixed artifact contract.'
    }
}

function Read-ReplicationEvidenceMetadata {
    param(
        [Parameter(Mandatory = $true)][object]$Inventory,
        [Parameter(Mandatory = $true)][object]$Manifest
    )

    $metadataText = Read-BoundedUtf8File `
        -Path (Join-Path $Inventory.MediaRoot 'evidence.json') `
        -MaximumBytes $script:EvidenceJsonMaxBytes `
        -Root $Inventory.MediaRoot `
        -Context 'Evidence metadata'
    $metadata = ConvertFrom-BoundedJson -Text $metadataText -Context 'Evidence metadata'
    $isRecorderMetadata = (
        (Find-AliasedProperty `
            -Object $metadata `
            -Names @('durationSeconds') `
            -Context 'Evidence metadata').Found -or
        (Find-AliasedProperty `
            -Object $metadata `
            -Names @('sha256') `
            -Context 'Evidence metadata').Found
    )

    if ($isRecorderMetadata) {
        Assert-KnownProperties `
            -Object $metadata `
            -AllowedNames @(
                'schemaVersion',
                'platform',
                'device',
                'durationSeconds',
                'dimensions',
                'sha256',
                'videoBytes',
                'files'
            ) `
            -Context 'Evidence metadata'
        if (
            (ConvertTo-PositiveInteger `
                -Value (Find-AliasedProperty `
                    -Object $metadata `
                    -Names @('schemaVersion') `
                    -Context 'Evidence metadata' `
                    -Required).Value `
                -Context 'Evidence schema version') -ne 1
        ) {
            throw 'Evidence metadata schema version must be 1.'
        }
        $evidencePlatform = ConvertTo-NormalizedPlatform `
            -Value (Find-AliasedProperty `
                -Object $metadata `
                -Names @('platform') `
                -Context 'Evidence metadata' `
                -Required).Value `
            -Context 'Evidence platform'
        if ($evidencePlatform -cne $Manifest.Platform) {
            throw 'Evidence metadata platform does not match the manifest.'
        }
        $device = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $metadata `
                -Names @('device') `
                -Context 'Evidence metadata' `
                -Required).Value `
            -Context 'Evidence device' `
            -MaximumLength 256
        if ($Manifest.SelectedDeviceId -and $device -cne $Manifest.SelectedDeviceId) {
            throw 'Evidence metadata device does not match the runner-selected device.'
        }

        $duration = ConvertTo-NullableDouble `
            -Value (Find-AliasedProperty `
                -Object $metadata `
                -Names @('durationSeconds') `
                -Context 'Evidence metadata' `
                -Required).Value
        if ($null -eq $duration -or $duration -le 0 -or $duration -gt 600) {
            throw 'Evidence metadata duration must be a bounded positive number.'
        }

        $dimensions = (Find-AliasedProperty `
            -Object $metadata `
            -Names @('dimensions') `
            -Context 'Evidence metadata' `
            -Required).Value
        Assert-KnownProperties `
            -Object $dimensions `
            -AllowedNames @('width', 'height') `
            -Context 'Evidence dimensions'
        foreach ($dimensionName in @('width', 'height')) {
            $dimension = ConvertTo-PositiveInteger `
                -Value (Find-AliasedProperty `
                    -Object $dimensions `
                    -Names @($dimensionName) `
                    -Context 'Evidence dimensions' `
                    -Required).Value `
                -Context "Evidence $dimensionName"
            if ($dimension -gt 8192) {
                throw 'Evidence dimensions exceed the supported bound.'
            }
        }

        $videoPath = Join-Path $Inventory.MediaRoot 'repro.mp4'
        $videoItem = Get-SafeRegularFile `
            -Path $videoPath `
            -MinimumBytes 1 `
            -MaximumBytes $script:VideoMaxBytes `
            -Root $Inventory.MediaRoot `
            -Context 'repro.mp4'
        $declaredBytes = ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $metadata `
                -Names @('videoBytes') `
                -Context 'Evidence metadata' `
                -Required).Value `
            -Context 'Evidence video byte count'
        if ($declaredBytes -ne $videoItem.Length) {
            throw 'Evidence metadata byte count does not match repro.mp4.'
        }
        $declaredHash = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $metadata `
                -Names @('sha256') `
                -Context 'Evidence metadata' `
                -Required).Value `
            -Context 'Evidence SHA-256' `
            -MaximumLength 64
        if ($declaredHash -notmatch '^[0-9a-fA-F]{64}$') {
            throw 'Evidence metadata SHA-256 is invalid.'
        }
        $actualHash = (Get-FileHash -LiteralPath $videoPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($declaredHash.ToLowerInvariant() -cne $actualHash) {
            throw 'Evidence metadata SHA-256 does not match repro.mp4.'
        }

        $filesObject = (Find-AliasedProperty `
            -Object $metadata `
            -Names @('files') `
            -Context 'Evidence metadata' `
            -Required).Value
        Assert-KnownProperties `
            -Object $filesObject `
            -AllowedNames @('video', 'thumbnail', 'preview') `
            -Context 'Evidence files'
        $expectedMediaFiles = @{
            video = 'repro.mp4'
            preview = 'preview.gif'
            thumbnail = 'thumbnail.png'
        }
        foreach ($entry in $expectedMediaFiles.GetEnumerator()) {
            $declaredFile = ConvertTo-BoundedSingleLine `
                -Value (Find-AliasedProperty `
                    -Object $filesObject `
                    -Names @($entry.Key) `
                    -Context 'Evidence files' `
                    -Required).Value `
                -Context "Evidence $($entry.Key) filename" `
                -MaximumLength 64
            if ($declaredFile -cne $entry.Value) {
                throw 'Evidence metadata filenames do not match the fixed media contract.'
            }
            if (-not (Test-Path -LiteralPath (Join-Path $Inventory.MediaRoot $entry.Value) -PathType Leaf)) {
                throw "Evidence metadata names missing file '$($entry.Value)'."
            }
        }

        return [pscustomobject]@{
            Kind = 'recorder'
            Value = $metadata
        }
    }

    $allowedProperties = @(
        'schemaVersion', 'schema_version',
        'issueNumber', 'issue_number',
        'platform',
        'testType', 'test_type',
        'testName', 'test_name',
        'testFilter', 'test_filter',
        'expectedFailurePattern', 'expected_failure_pattern',
        'verificationMode', 'verification_mode', 'mode',
        'verificationStatus', 'verification_status', 'status',
        'video', 'repro',
        'preview', 'thumbnail',
        'media'
    )
    Assert-KnownProperties `
        -Object $metadata `
        -AllowedNames $allowedProperties `
        -Context 'Evidence metadata'

    $schema = Find-AliasedProperty `
        -Object $metadata `
        -Names @('schemaVersion', 'schema_version') `
        -Context 'Evidence metadata'
    if ($schema.Found -and (ConvertTo-PositiveInteger -Value $schema.Value -Context 'Evidence schema version') -ne 1) {
        throw 'Evidence metadata schema version must be 1.'
    }

    $evidenceIssue = ConvertTo-PositiveInteger `
        -Value (Find-AliasedProperty `
            -Object $metadata `
            -Names @('issueNumber', 'issue_number') `
            -Context 'Evidence metadata' `
            -Required).Value `
        -Context 'Evidence issue number'
    if ($evidenceIssue -ne $Manifest.IssueNumber) {
        throw 'Evidence metadata issue number does not match the manifest.'
    }
    $evidencePlatform = ConvertTo-NormalizedPlatform `
        -Value (Find-AliasedProperty `
            -Object $metadata `
            -Names @('platform') `
            -Context 'Evidence metadata' `
            -Required).Value `
        -Context 'Evidence platform'
    if ($evidencePlatform -cne $Manifest.Platform) {
        throw 'Evidence metadata platform does not match the manifest.'
    }
    $evidenceTypeProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('testType', 'test_type') `
        -Context 'Evidence metadata'
    if ($evidenceTypeProperty.Found) {
        $evidenceType = ConvertTo-NormalizedTestType -Value $evidenceTypeProperty.Value
        if ($evidenceType -cne $Manifest.TestType) {
            throw 'Evidence metadata test type does not match the manifest.'
        }
    }
    $evidenceFilterProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('testFilter', 'test_filter') `
        -Context 'Evidence metadata'
    if ($evidenceFilterProperty.Found) {
        $evidenceFilter = ConvertTo-BoundedSingleLine `
            -Value $evidenceFilterProperty.Value `
            -Context 'Evidence test filter' `
            -MaximumLength 1000
        if ($evidenceFilter -cne $Manifest.TestFilter) {
            throw 'Evidence metadata test filter does not exactly match the manifest.'
        }
    }

    $evidenceNameProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('testName', 'test_name') `
        -Context 'Evidence metadata'
    if ($evidenceNameProperty.Found) {
        $evidenceName = ConvertTo-BoundedSingleLine `
            -Value $evidenceNameProperty.Value `
            -Context 'Evidence test name' `
            -MaximumLength 256
        if ($evidenceName -cne $Manifest.TestName) {
            throw 'Evidence metadata test name does not exactly match the manifest.'
        }
    }

    $evidenceFailureProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('expectedFailurePattern', 'expected_failure_pattern') `
        -Context 'Evidence metadata'
    if ($evidenceFailureProperty.Found) {
        $evidenceFailure = ConvertTo-BoundedSingleLine `
            -Value $evidenceFailureProperty.Value `
            -Context 'Evidence expected failure pattern' `
            -MinimumLength 3 `
            -MaximumLength 512
        if ($evidenceFailure -cne $Manifest.ExpectedFailurePattern) {
            throw 'Evidence metadata expected failure pattern does not exactly match the manifest.'
        }
    }

    $modeProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('verificationMode', 'verification_mode', 'mode') `
        -Context 'Evidence metadata'
    if ($modeProperty.Found) {
        $mode = ConvertTo-BoundedSingleLine `
            -Value $modeProperty.Value `
            -Context 'Evidence verification mode' `
            -MaximumLength 64
        if ($mode -notmatch '^(?i:failure[-_ ]?only|verify failure only mode)$') {
            throw 'Evidence metadata must prove failure-only verification mode.'
        }
    }
    $statusProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('verificationStatus', 'verification_status', 'status') `
        -Context 'Evidence metadata'
    if ($statusProperty.Found) {
        $status = ConvertTo-BoundedSingleLine `
            -Value $statusProperty.Value `
            -Context 'Evidence verification status' `
            -MaximumLength 64
        if ($status -notmatch '^(?i:VERIFICATION PASSED)(?: ✅)?$') {
            throw 'Evidence metadata does not contain the required VERIFICATION PASSED status.'
        }
    }

    $mediaObjectProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('media') `
        -Context 'Evidence metadata'
    $mediaObject = if ($mediaObjectProperty.Found) { $mediaObjectProperty.Value } else { $null }
    if ($null -ne $mediaObject) {
        Assert-KnownProperties `
            -Object $mediaObject `
            -AllowedNames @('video', 'repro', 'preview', 'thumbnail') `
            -Context 'Evidence media metadata'
    }
    $videoProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('video', 'repro') `
        -Context 'Evidence metadata'
    if (-not $videoProperty.Found -and $null -ne $mediaObject) {
        $videoProperty = Find-AliasedProperty `
            -Object $mediaObject `
            -Names @('video', 'repro') `
            -Context 'Evidence media metadata'
    }
    if ($videoProperty.Found -and ([string]$videoProperty.Value) -cne 'repro.mp4') {
        throw 'Evidence metadata video must be repro.mp4.'
    }

    $previewProperty = Find-AliasedProperty `
        -Object $metadata `
        -Names @('preview', 'thumbnail') `
        -Context 'Evidence metadata'
    if (-not $previewProperty.Found -and $null -ne $mediaObject) {
        $previewProperty = Find-AliasedProperty `
            -Object $mediaObject `
            -Names @('preview', 'thumbnail') `
            -Context 'Evidence media metadata'
    }
    if ($previewProperty.Found -and ([string]$previewProperty.Value) -cne $Inventory.PreviewName) {
        throw 'Evidence metadata preview does not match the validated preview artifact.'
    }

    return [pscustomobject]@{
        Kind = 'legacy'
        Value = $metadata
    }
}

function Test-LabeledExactValue {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string[]]$Labels,
        [Parameter(Mandatory = $true)][string]$Value
    )

    $labelPattern = ($Labels | ForEach-Object { [regex]::Escape($_) }) -join '|'
    $valuePattern = [regex]::Escape($Value)
    return $Text -match "(?m)^\s*(?:[-*]\s*)?(?:$labelPattern)\s*:\s*\x60{0,2}$valuePattern\x60{0,2}\s*$"
}

function Assert-ReplicationVerificationEvidence {
    param(
        [Parameter(Mandatory = $true)][object]$Inventory,
        [Parameter(Mandatory = $true)][object]$Manifest
    )

    if ($Inventory.HasMachineResult) {
        $resultText = Read-BoundedUtf8File `
            -Path (Join-Path $Inventory.VerificationRoot 'verification-result.json') `
            -MaximumBytes $script:EvidenceJsonMaxBytes `
            -Root $Inventory.VerificationRoot `
            -Context 'Verification result'
        $result = ConvertFrom-BoundedJson -Text $resultText -Context 'Verification result'
        Assert-KnownProperties `
            -Object $result `
            -AllowedNames @(
                'schemaVersion',
                'issueNumber',
                'platform',
                'testType',
                'testFilter',
                'testProject',
                'testProjectPath',
                'testClass',
                'testMethod',
                'expectedFailureSignature',
                'actualFailureMessage',
                'verifierExitCode',
                'verifierPassed',
                'signatureMatched',
                'infrastructureFailure',
                'verificationPassed',
                'logFiles'
            ) `
            -Context 'Verification result'
        if (
            (ConvertTo-PositiveInteger `
                -Value (Find-AliasedProperty `
                    -Object $result `
                    -Names @('schemaVersion') `
                    -Context 'Verification result' `
                    -Required).Value `
                -Context 'Verification schema version') -ne 1
        ) {
            throw 'Verification result schema version must be 1.'
        }
        $resultIssue = ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('issueNumber') `
                -Context 'Verification result' `
                -Required).Value `
            -Context 'Verification issue number'
        if ($resultIssue -ne $Manifest.IssueNumber) {
            throw 'Verification result issue number does not match the manifest.'
        }
        $resultPlatform = ConvertTo-NormalizedPlatform `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('platform') `
                -Context 'Verification result' `
                -Required).Value `
            -Context 'Verification platform'
        if ($resultPlatform -cne $Manifest.Platform) {
            throw 'Verification result platform does not match the manifest.'
        }
        $resultType = ConvertTo-NormalizedTestType `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('testType') `
                -Context 'Verification result' `
                -Required).Value
        if ($resultType -cne $Manifest.TestType) {
            throw 'Verification result test type does not match the manifest.'
        }
        $resultFilter = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('testFilter') `
                -Context 'Verification result' `
                -Required).Value `
            -Context 'Verification test filter' `
            -MaximumLength 512
        if ($resultFilter -cne $Manifest.TestFilter) {
            throw 'Verification result test filter does not exactly match the manifest.'
        }
        $resultSignature = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('expectedFailureSignature') `
                -Context 'Verification result' `
                -Required).Value `
            -Context 'Verification failure signature' `
            -MinimumLength 3 `
            -MaximumLength 1000
        if ($resultSignature -cne $Manifest.ExpectedFailurePattern) {
            throw 'Verification result failure signature does not exactly match the manifest.'
        }
        $actualFailureProperty = Find-AliasedProperty `
            -Object $result `
            -Names @('actualFailureMessage') `
            -Context 'Verification result' `
            -Required
        if ($actualFailureProperty.Value -isnot [string]) {
            throw 'Verification result actual failure message must be a string.'
        }
        $actualFailureMessage = [string]$actualFailureProperty.Value
        if ([string]::IsNullOrWhiteSpace($actualFailureMessage) -or
            $actualFailureMessage.Length -gt 10000 -or
            $actualFailureMessage.Contains([char]0)) {
            throw 'Verification result actual failure message is missing or invalid.'
        }
        if (-not $actualFailureMessage.Contains(
            $resultSignature,
            [StringComparison]::Ordinal)) {
            throw 'The targeted test failure message does not contain the expected failure signature.'
        }
        $exitCodeProperty = Find-AliasedProperty `
            -Object $result `
            -Names @('verifierExitCode') `
            -Context 'Verification result' `
            -Required
        if ([string]$exitCodeProperty.Value -cne '0') {
            throw 'Verification result has a nonzero verifier exit code.'
        }
        $expectedBooleans = @{
            verifierPassed = $true
            signatureMatched = $true
            infrastructureFailure = $false
            verificationPassed = $true
        }
        foreach ($entry in $expectedBooleans.GetEnumerator()) {
            $property = Find-AliasedProperty `
                -Object $result `
                -Names @($entry.Key) `
                -Context 'Verification result' `
                -Required
            if ($property.Value -isnot [bool] -or $property.Value -ne $entry.Value) {
                throw "Verification result '$($entry.Key)' does not prove a valid failure-only run."
            }
        }
        $logFilesProperty = Find-AliasedProperty `
            -Object $result `
            -Names @('logFiles') `
            -Context 'Verification result'
        if ($logFilesProperty.Found) {
            $logFiles = @($logFilesProperty.Value)
            if ($logFiles.Count -gt 20) {
                throw 'Verification result contains too many log references.'
            }
            foreach ($logFile in $logFiles) {
                $null = ConvertTo-BoundedSingleLine `
                    -Value $logFile `
                    -Context 'Verification log reference' `
                    -MaximumLength 1000
            }
        }

        $console = Read-BoundedUtf8File `
            -Path (Join-Path $Inventory.VerificationRoot 'verification-console.log') `
            -MaximumBytes $script:VerificationArtifactMaxBytes `
            -Root $Inventory.VerificationRoot `
            -Context 'Verification console log'
        $disqualifier = Get-DisqualifyingFailureCode -Text $console
        if ($disqualifier) {
            throw "Verification evidence is disqualified by '$disqualifier' failure evidence."
        }
        if (
            $console -match '(?im)\bVERIFICATION (?:FAILED|INCONCLUSIVE)\b' -or
            $console -match '(?im)\bFULL VERIFICATION MODE\b|\bTests WITH fix\b|\bPASS with fix\b'
        ) {
            throw 'Verification evidence is conflicting, spoofed, or not failure-only.'
        }
        if (
            $console -notmatch '(?i)\bVERIFICATION PASSED\b' -or
            $console -notmatch '(?i)\bVERIFY FAILURE ONLY MODE\b|\bFailure Only Mode\b'
        ) {
            throw 'Verification console does not prove VERIFICATION PASSED in failure-only mode.'
        }
        if (
            $console -notmatch (
                '(?im)\[' +
                [regex]::Escape($Manifest.TestType) +
                '\]\s+' +
                [regex]::Escape($Manifest.TestName) +
                '.*FAILED'
            )
        ) {
            throw 'Verification console does not prove the named test failed as expected.'
        }

        return $result
    }

    $report = Read-BoundedUtf8File `
        -Path (Join-Path $Inventory.VerificationRoot 'verification-report.md') `
        -MaximumBytes $script:VerificationArtifactMaxBytes `
        -Root $Inventory.VerificationRoot `
        -Context 'Verification report'
    $verificationLog = Read-BoundedUtf8File `
        -Path (Join-Path $Inventory.VerificationRoot 'verification-log.txt') `
        -MaximumBytes $script:VerificationArtifactMaxBytes `
        -Root $Inventory.VerificationRoot `
        -Context 'Verification log'

    $proofTexts = [System.Collections.Generic.List[string]]::new()
    $proofTexts.Add($report)
    $proofTexts.Add($verificationLog)
    foreach ($optionalName in @(
        'verification-output.log',
        'verification-console.log',
        'verify-tests-fail.log'
    )) {
        $optionalPath = Join-Path $Inventory.VerificationRoot $optionalName
        if (Test-Path -LiteralPath $optionalPath -PathType Leaf) {
            $proofTexts.Add((Read-BoundedUtf8File `
                -Path $optionalPath `
                -MaximumBytes $script:VerificationArtifactMaxBytes `
                -Root $Inventory.VerificationRoot `
                -Context 'Verification output'))
        }
    }

    $rawFailureTexts = [System.Collections.Generic.List[string]]::new()
    foreach ($failureLog in $Inventory.FailureLogs) {
        $failureText = Read-BoundedUtf8File `
            -Path $failureLog.FullName `
            -MaximumBytes $script:VerificationArtifactMaxBytes `
            -Root $Inventory.VerificationRoot `
            -Context 'Raw failure log'
        $rawFailureTexts.Add($failureText)
        $proofTexts.Add($failureText)
    }
    $combinedProof = $proofTexts -join "`n"

    $disqualifier = Get-DisqualifyingFailureCode -Text $combinedProof
    if ($disqualifier) {
        throw "Verification evidence is disqualified by '$disqualifier' failure evidence."
    }
    if (
        $combinedProof -match '(?im)\bVERIFICATION (?:FAILED|INCONCLUSIVE)\b' -or
        $combinedProof -match '(?im)\bFULL VERIFICATION MODE\b|\bTests WITH fix\b|\bPASS with fix\b'
    ) {
        throw 'Verification evidence is conflicting, spoofed, or not failure-only.'
    }
    if (
        $report -notmatch '(?m)^## Gate: Test Verification \(Failure-Only Mode\)\s*$' -or
        $report -notmatch '(?m)^\*\*Result:\*\*\s*(?:✅\s*)?PASSED\s*$'
    ) {
        throw 'Verification report does not prove a passed failure-only run.'
    }
    if ($verificationLog -notmatch '(?im)^Verify Tests Fail \(Failure Only Mode\)\s*$') {
        throw 'Verification log does not prove failure-only mode.'
    }

    $rowPattern = '(?m)^\|\s*`{0,2}' +
        [regex]::Escape($Manifest.TestName) +
        '`{0,2}\s*\|\s*' +
        [regex]::Escape($Manifest.TestType) +
        '\s*\|\s*FAIL\s*(?:✅\s*)?\(expected\)\s*\|'
    if ($report -notmatch $rowPattern) {
        throw 'Verification artifacts do not identify the named test and expected failed outcome.'
    }
    if (
        -not (Test-LabeledExactValue `
            -Text $combinedProof `
            -Labels @('TestFilter', 'Test Filter', 'Filter') `
            -Value $Manifest.TestFilter)
    ) {
        throw 'Verification artifacts do not prove the exact test filter.'
    }
    if (
        -not (Test-LabeledExactValue `
            -Text $combinedProof `
            -Labels @('Platform') `
            -Value $Manifest.Platform) -and
        -not (Test-LabeledExactValue `
            -Text $combinedProof `
            -Labels @('Platform') `
            -Value $Manifest.Platform.ToUpperInvariant())
    ) {
        throw 'Verification artifacts do not prove the trusted platform.'
    }

    $assertionProved = $false
    foreach ($failureText in $rawFailureTexts) {
        if (
            $failureText.Contains(
                $Manifest.ExpectedFailurePattern,
                [System.StringComparison]::Ordinal
            ) -and
            $failureText -match '(?im)(?:^\s*\[FAIL\]|^\s*Failed\s+\S|^\s*Failed:\s*[1-9]\d*|Failed=[1-9]\d*|\b(?:Xunit|NUnit)\..*Exception\b|\bAssertion(?:Exception| failed)\b)'
        ) {
            $assertionProved = $true
            break
        }
    }
    if (-not $assertionProved) {
        throw 'Raw failure logs do not prove the expected assertion signature.'
    }
}

function Get-LoosePropertyValue {
    param(
        [AllowNull()][object]$Object,
        [Parameter(Mandatory = $true)][string[]]$Names
    )

    if ($null -eq $Object) {
        return $null
    }
    $property = Find-AliasedProperty -Object $Object -Names $Names -Context 'Probe result'
    if ($property.Found) {
        return $property.Value
    }

    return $null
}

function ConvertTo-NullableDouble {
    param([AllowNull()][object]$Value)

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value) -or [string]$Value -ceq 'N/A') {
        return $null
    }
    $number = 0.0
    if (
        -not [double]::TryParse(
            [string]$Value,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$number
        )
    ) {
        return $null
    }

    return $number
}

function Invoke-DefaultMediaProbe {
    param([Parameter(Mandatory = $true)][string]$Path)

    $result = Invoke-ExternalBytes `
        -FileName 'ffprobe' `
        -Arguments @(
            '-v', 'error',
            '-print_format', 'json',
            '-show_format',
            '-show_streams',
            '--',
            $Path
        ) `
        -TimeoutSeconds 15 `
        -MaximumOutputBytes 1MB
    if ($result.ExitCode -ne 0) {
        throw 'ffprobe could not decode a required media artifact.'
    }
    try {
        $json = [System.Text.UTF8Encoding]::new($false, $true).GetString($result.Bytes)
        $probe = ConvertFrom-BoundedJson -Text $json -Context 'ffprobe output'
    } catch {
        throw 'ffprobe returned invalid bounded JSON.'
    }

    return [pscustomobject]@{
        IsDecodable = $true
        Format = $probe.format
        Streams = @($probe.streams)
    }
}

function Assert-MediaMagic {
    param(
        [Parameter(Mandatory = $true)][System.IO.FileInfo]$File,
        [Parameter(Mandatory = $true)][string]$Kind
    )

    $bytes = [System.IO.File]::ReadAllBytes($File.FullName)
    switch ($Kind) {
        'mp4' {
            if (
                $bytes.Length -lt 12 -or
                [System.Text.Encoding]::ASCII.GetString($bytes, 4, 4) -cne 'ftyp'
            ) {
                throw 'repro.mp4 does not have a valid MP4 file signature.'
            }
        }
        'gif' {
            if ($bytes.Length -lt 6) {
                throw 'preview.gif does not have a valid GIF file signature.'
            }
            $signature = [System.Text.Encoding]::ASCII.GetString($bytes, 0, 6)
            if ($signature -notin @('GIF87a', 'GIF89a')) {
                throw 'preview.gif does not have a valid GIF file signature.'
            }
        }
        'png' {
            $signature = [byte[]](137, 80, 78, 71, 13, 10, 26, 10)
            if ($bytes.Length -lt $signature.Length) {
                throw 'thumbnail.png does not have a valid PNG file signature.'
            }
            for ($index = 0; $index -lt $signature.Length; $index++) {
                if ($bytes[$index] -ne $signature[$index]) {
                    throw 'thumbnail.png does not have a valid PNG file signature.'
                }
            }
        }
    }
}

function Assert-MediaProbeResult {
    param(
        [Parameter(Mandatory = $true)][object]$ProbeResult,
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $decodableValue = Get-LoosePropertyValue `
        -Object $ProbeResult `
        -Names @('IsDecodable', 'isDecodable', 'Success', 'success')
    if ($null -ne $decodableValue) {
        $decodable = if ($decodableValue -is [bool]) {
            $decodableValue
        } else {
            ([string]$decodableValue -match '^(?i:true|1)$')
        }
        if (-not $decodable) {
            throw "$Name is not decodable."
        }
    }

    $formatObject = Get-LoosePropertyValue -Object $ProbeResult -Names @('Format', 'format')
    $streamsValue = Get-LoosePropertyValue -Object $ProbeResult -Names @('Streams', 'streams')
    $streams = @($streamsValue | Where-Object { $null -ne $_ })
    $videoStream = @(
        $streams |
            Where-Object {
                $codecType = Get-LoosePropertyValue -Object $_ -Names @('codec_type', 'CodecType')
                $null -eq $codecType -or ([string]$codecType -ieq 'video')
            }
    ) | Select-Object -First 1

    $formatName = Get-LoosePropertyValue `
        -Object $ProbeResult `
        -Names @('FormatName', 'formatName')
    if ($null -eq $formatName -and $null -ne $formatObject) {
        $formatName = Get-LoosePropertyValue `
            -Object $formatObject `
            -Names @('format_name', 'FormatName')
    }
    $formatText = [string]$formatName
    $formatMatches = switch ($Kind) {
        'mp4' { $formatText -match '(?i)(?:^|,)(?:mov|mp4|m4a|3gp|3g2|mj2)(?:,|$)' }
        'gif' { $formatText -match '(?i)gif' }
        'png' { $formatText -match '(?i)png' }
    }
    if (-not $formatMatches) {
        throw "$Name probe format does not match its extension."
    }

    $width = Get-LoosePropertyValue -Object $ProbeResult -Names @('Width', 'width')
    $height = Get-LoosePropertyValue -Object $ProbeResult -Names @('Height', 'height')
    if ($null -eq $width -and $null -ne $videoStream) {
        $width = Get-LoosePropertyValue -Object $videoStream -Names @('width', 'Width')
    }
    if ($null -eq $height -and $null -ne $videoStream) {
        $height = Get-LoosePropertyValue -Object $videoStream -Names @('height', 'Height')
    }
    if (
        ([string]$width -notmatch '^[1-9]\d*$') -or
        ([string]$height -notmatch '^[1-9]\d*$')
    ) {
        throw "$Name probe did not return positive video dimensions."
    }

    if ($Kind -ceq 'mp4') {
        $duration = Get-LoosePropertyValue `
            -Object $ProbeResult `
            -Names @('DurationSeconds', 'durationSeconds', 'Duration', 'duration')
        if ($null -eq $duration -and $null -ne $formatObject) {
            $duration = Get-LoosePropertyValue `
                -Object $formatObject `
                -Names @('duration', 'Duration')
        }
        if ($null -eq $duration -and $null -ne $videoStream) {
            $duration = Get-LoosePropertyValue `
                -Object $videoStream `
                -Names @('duration', 'Duration')
        }
        $durationNumber = ConvertTo-NullableDouble -Value $duration
        if ($null -eq $durationNumber -or $durationNumber -le 0 -or $durationNumber -gt 600) {
            throw 'repro.mp4 probe did not return a bounded positive duration.'
        }
    }
}

function Assert-ReplicationMediaEvidence {
    param(
        [Parameter(Mandatory = $true)][object]$Inventory,
        [scriptblock]$Probe
    )

    $media = [System.Collections.Generic.List[object]]::new()
    $media.Add(
        [pscustomobject]@{
            Name = 'repro.mp4'
            Kind = 'mp4'
            MaxBytes = $script:VideoMaxBytes
        }
    )
    if ($Inventory.HasGif) {
        $media.Add(
        [pscustomobject]@{
            Name = 'preview.gif'
            Kind = 'gif'
            MaxBytes = $script:PreviewMaxBytes
        }
        )
    }
    if ($Inventory.HasThumbnail) {
        $media.Add(
            [pscustomobject]@{
                Name = 'thumbnail.png'
                Kind = 'png'
                MaxBytes = $script:PreviewMaxBytes
            }
        )
    }

    foreach ($entry in $media) {
        $path = Join-Path $Inventory.MediaRoot $entry.Name
        $file = Get-SafeRegularFile `
            -Path $path `
            -MinimumBytes 1 `
            -MaximumBytes $entry.MaxBytes `
            -Root $Inventory.MediaRoot `
            -Context $entry.Name
        Assert-MediaMagic -File $file -Kind $entry.Kind

        $probeResult = if ($Probe) {
            $results = @(& $Probe $file.FullName $entry.Kind | Where-Object { $null -ne $_ })
            if ($results.Count -ne 1) {
                throw 'Injected media probe must return exactly one result per file.'
            }
            $results[0]
        } else {
            Invoke-DefaultMediaProbe -Path $file.FullName
        }
        Assert-MediaProbeResult `
            -ProbeResult $probeResult `
            -Kind $entry.Kind `
            -Name $entry.Name
    }
}

function Write-TrustedValidationJson {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][object]$Document
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $parent = [System.IO.Path]::GetDirectoryName($fullPath)
    if ([string]::IsNullOrEmpty($parent) -or -not (Test-Path -LiteralPath $parent -PathType Container)) {
        throw 'OutputPath parent directory must already exist.'
    }
    Assert-NoLinkInExistingPath -Path $parent
    if (Test-Path -LiteralPath $fullPath) {
        $existing = Get-Item -LiteralPath $fullPath -Force
        if ($existing.PSIsContainer) {
            throw 'OutputPath must be a file.'
        }
        Assert-NoLinkInExistingPath -Path $fullPath
    }

    $json = $Document | ConvertTo-Json -Depth 6 -Compress
    $temporaryPath = Join-Path $parent ".$([System.IO.Path]::GetFileName($fullPath)).$PID.$([guid]::NewGuid().ToString('N')).tmp"
    try {
        [System.IO.File]::WriteAllText(
            $temporaryPath,
            $json + [Environment]::NewLine,
            [System.Text.UTF8Encoding]::new($false)
        )
        if (Test-Path -LiteralPath $fullPath) {
            Remove-Item -LiteralPath $fullPath -Force
        }
        Move-Item -LiteralPath $temporaryPath -Destination $fullPath
    } finally {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
}

function Invoke-ReplicationCandidateValidation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepoRoot,
        [Parameter(Mandatory = $true)][string]$CandidateManifestPath,
        [string]$PatchPath,
        [string]$CandidateCommit,
        [string]$BaseCommit,
        [Parameter(Mandatory = $true)][string]$EvidenceDir,
        [Parameter(Mandatory = $true)][long]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Platform,
        [Parameter(Mandatory = $true)][string]$OutputPath,
        [scriptblock]$MediaProbe
    )

    if ($IssueNumber -le 0) {
        throw 'IssueNumber must be a positive integer.'
    }
    foreach ($requiredPath in @($RepoRoot, $CandidateManifestPath, $EvidenceDir, $OutputPath)) {
        if ([string]::IsNullOrWhiteSpace($requiredPath)) {
            throw 'All required validation paths must be provided.'
        }
    }
    $hasPatch = -not [string]::IsNullOrWhiteSpace($PatchPath)
    $hasCommitPair = (
        -not [string]::IsNullOrWhiteSpace($CandidateCommit) -and
        -not [string]::IsNullOrWhiteSpace($BaseCommit)
    )
    $hasPartialCommitPair = (
        [string]::IsNullOrWhiteSpace($CandidateCommit) -xor
        [string]::IsNullOrWhiteSpace($BaseCommit)
    )
    if ($hasPartialCommitPair -or $hasPatch -eq $hasCommitPair) {
        throw 'Provide exactly one candidate source: PatchPath or CandidateCommit with BaseCommit.'
    }

    $rejectedDocument = [ordered]@{
        schemaVersion = 1
        status = 'rejected'
        validationPassed = $false
    }
    Write-TrustedValidationJson -Path $OutputPath -Document $rejectedDocument

    try {
        $repoPath = [System.IO.Path]::GetFullPath($RepoRoot)
        if (-not (Test-Path -LiteralPath $repoPath -PathType Container)) {
            throw 'RepoRoot does not exist as a directory.'
        }
        Assert-NoLinkInExistingPath -Path $repoPath
        $normalizedPlatform = ConvertTo-NormalizedPlatform -Value $Platform -Context 'Trusted platform'
        $manifest = Read-ReplicationManifest `
            -Path $CandidateManifestPath `
            -ExpectedIssueNumber $IssueNumber `
            -ExpectedPlatform $normalizedPlatform

        $candidateSource = ''
        if ($hasPatch) {
            $candidateFiles = @(Get-CandidateFilesFromPatch `
                -Path $PatchPath `
                -TestType $manifest.TestType)
            Assert-PatchCandidatePathsAreNew `
                -Repository $repoPath `
                -CandidateFiles $candidateFiles
            if ($manifest.BaseSha) {
                $headSha = (Invoke-GitText `
                    -Repository $repoPath `
                    -Arguments @('rev-parse', '--verify', 'HEAD^{commit}') `
                    -MaximumOutputBytes 4KB).Trim().ToLowerInvariant()
                if ($headSha -cne $manifest.BaseSha) {
                    throw 'Manifest base SHA does not match the trusted patch checkout.'
                }
            } elseif (Test-Path -LiteralPath (Join-Path $repoPath '.git')) {
                $manifest.BaseSha = (Invoke-GitText `
                    -Repository $repoPath `
                    -Arguments @('rev-parse', '--verify', 'HEAD^{commit}') `
                    -MaximumOutputBytes 4KB).Trim().ToLowerInvariant()
            }
            $candidateSource = 'patch'
        } else {
            $commitResult = Get-CandidateFilesFromCommits `
                -Repository $repoPath `
                -Candidate $CandidateCommit `
                -Base $BaseCommit `
                -TestType $manifest.TestType
            $candidateFiles = @($commitResult.Files)
            if ($manifest.BaseSha -and $commitResult.BaseCommit -cne $manifest.BaseSha) {
                throw 'Manifest base SHA does not match BaseCommit.'
            }
            if (-not $manifest.BaseSha) {
                $manifest.BaseSha = $commitResult.BaseCommit
            }
            $candidateSource = 'commit'
        }

        Assert-ReplicationCandidateSources `
            -Manifest $manifest `
            -CandidateFiles $candidateFiles

        if ($manifest.ArtifactContract) {
            Assert-ReplicationExecutionResult `
                -Directory $EvidenceDir `
                -Manifest $manifest
        }
        $inventory = Get-ReplicationEvidenceInventory -Directory $EvidenceDir
        $null = Read-ReplicationEvidenceMetadata `
            -Inventory $inventory `
            -Manifest $manifest
        $verificationResult = Assert-ReplicationVerificationEvidence `
            -Inventory $inventory `
            -Manifest $manifest
        if ($manifest.ArtifactContract -and $null -eq $verificationResult) {
            throw 'Verification evidence must include a trusted targeted failure message.'
        }
        Assert-ReplicationMediaEvidence `
            -Inventory $inventory `
            -Probe $MediaProbe

        $validatedDocument = [ordered]@{
            schemaVersion = 1
            status = 'validated'
            validationPassed = $true
            issueNumber = $manifest.IssueNumber
            platform = $manifest.Platform
            baseSha = $manifest.BaseSha
            testType = $manifest.PublishedTestType
            verificationTestType = $manifest.TestType
            testName = $manifest.TestName
            testFilter = $manifest.TestFilter
            expectedFailureSignature = $manifest.ExpectedFailurePattern
            expectedFailurePattern = $manifest.ExpectedFailurePattern
            actualFailureMessage = if ($verificationResult) {
                [string]$verificationResult.actualFailureMessage
            } else {
                $null
            }
            reproductionMarker = $manifest.ReproductionMarker
            files = @($manifest.ProposedFiles)
            proposedFiles = @($manifest.ProposedFiles)
            reproductionSteps = @($manifest.ReproductionSteps)
            candidateSource = $candidateSource
            evidence = [ordered]@{
                video = 'repro.mp4'
                preview = $inventory.PreviewName
                thumbnail = if ($inventory.HasThumbnail) { 'thumbnail.png' } else { $null }
            }
        }
        Write-TrustedValidationJson -Path $OutputPath -Document $validatedDocument
        return [pscustomobject]$validatedDocument
    } catch {
        try {
            Write-TrustedValidationJson -Path $OutputPath -Document $rejectedDocument
        } catch {
            $null = $_
        }
        throw
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-ReplicationCandidateValidation `
        -RepoRoot $RepoRoot `
        -CandidateManifestPath $CandidateManifestPath `
        -PatchPath $PatchPath `
        -CandidateCommit $CandidateCommit `
        -BaseCommit $BaseCommit `
        -EvidenceDir $EvidenceDir `
        -IssueNumber $IssueNumber `
        -Platform $Platform `
        -OutputPath $OutputPath `
        -MediaProbe $MediaProbe
}
