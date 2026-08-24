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
    [Parameter(ParameterSetName = 'Patch')]
    [string]$FixPatchPath,
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

$certificationPath = Join-Path $PSScriptRoot 'Get-ReplicationCertification.ps1'
if (-not (Test-Path -LiteralPath $certificationPath -PathType Leaf)) {
    throw "Trusted replication certification module is missing: $certificationPath"
}
. $certificationPath

$script:ManifestMaxBytes = 64KB
$script:EvidenceJsonMaxBytes = 64KB
$script:PatchMaxBytes = 2MB
$script:CandidateFileMaxBytes = 256KB
$script:CandidateTotalMaxBytes = 1MB
$script:CandidateFileMaxCount = 24
$script:FixPatchMaxBytes = 512KB
$script:FixFileMaxCount = 8
$script:FixChangedLineMaxCount = 400
$script:VerificationArtifactMaxBytes = 2MB
# Reviewers repeatedly rejected reproductions proved by one execution, so a
# candidate has to show the same failure in more than one independent run.
$script:VerificationMinimumRunCount = 2
$script:ValidatedEffectiveFailureSignature = ''
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
        [int]$MaximumLength = 512,
        [switch]$Prose
    )

    if ($Value -isnot [string]) {
        throw "$Context must be a string."
    }
    $text = [string]$Value
    if ($Prose) {
        # Presentation-only text, shown to a reader and acted on by nothing.
        # Build 15075319 passed every arm and was discarded here because a
        # model wrote a reproduction step that was too long or spanned a line,
        # which is the third time in this pipeline that a bound on how text
        # looks has destroyed the work the text describes.
        #
        # Layout is corrected; meaning is still refused. A line break or a tab
        # becomes a space, the value is trimmed and cut to fit, and everything
        # below - a genuine control character, a URL, a mention, a logging
        # directive - still throws, because those change what the text does
        # rather than how it reads.
        $text = [regex]::Replace($text, '[\r\n\t]+', ' ').Trim()
        if ($text.Length -gt $MaximumLength) {
            $text = $text.Substring(0, [Math]::Max(1, $MaximumLength - 1)).TrimEnd() + '…'
        }
    }
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
        -MaximumLength 300 `
        -Prose
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

function Assert-CandidatePathShape {
    param(
        [AllowNull()][object]$Path,
        [Parameter(Mandatory = $true)][string]$Context
    )

    $candidatePath = ConvertTo-BoundedSingleLine `
        -Value $Path `
        -Context "$Context path" `
        -MaximumLength 240
    if (
        $candidatePath.StartsWith('/') -or
        $candidatePath.StartsWith('\') -or
        $candidatePath -match '^[A-Za-z]:' -or
        $candidatePath.Contains('\') -or
        $candidatePath.Contains('%') -or
        $candidatePath -notmatch '^[A-Za-z0-9._+@()/{}\[\]-]+$'
    ) {
        throw "$Context path is absolute, non-normalized, or contains unsafe characters."
    }

    $segments = $candidatePath.Split('/')
    if ($segments.Count -lt 2) {
        throw "$Context path must be repository-relative."
    }
    foreach ($segment in $segments) {
        if (
            [string]::IsNullOrEmpty($segment) -or
            $segment -in @('.', '..') -or
            $segment.Length -gt 100 -or
            $segment -match '^(?i:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\..*)?$'
        ) {
            throw "$Context path contains traversal, an empty segment, or a reserved name."
        }
    }
    if ($segments -contains '.git' -or $segments -contains 'bin' -or $segments -contains 'obj') {
        throw "$Context path targets a prohibited repository or build directory."
    }

    if (
        $candidatePath -match '(?i)/(?:snapshots?|baselines?)/' -or
        [System.IO.Path]::GetFileName($candidatePath) -match '(?i)^(?:Directory\.Build|AssemblyInfo|GlobalUsings)\.'
    ) {
        throw "$Context path targets generated, baseline, or project infrastructure content."
    }

    return $candidatePath
}

function Assert-CandidatePath {
    param(
        [AllowNull()][object]$Path,
        [Parameter(Mandatory = $true)][string]$TestType
    )

    $candidatePath = Assert-CandidatePathShape -Path $Path -Context 'Candidate'

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
        'testClassName', 'test_class_name',
        'testMethodName', 'test_method_name',
        'expectedFailurePattern', 'expected_failure_pattern',
        'expectedFailureSignature', 'expected_failure_signature',
        'reproductionMarker', 'reproduction_marker',
        'proposedFiles', 'proposed_files', 'files',
        'sandboxFiles', 'sandbox_files',
        'reproductionResult', 'reproduction_result',
        'evidenceManifest', 'evidence_manifest',
        'verificationResult', 'verification_result',
        'negativeControl', 'negative_control',
        'patch',
        'fixFiles', 'fix_files',
        'fixPatch', 'fix_patch',
        'fixRootCause', 'fix_root_cause',
        'fixApproach', 'fix_approach',
        'fixRejectedApproaches', 'fix_rejected_approaches'
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

    # The manifest filter is an issue-keyed class token, which reviewers found
    # does not select the test with the repository's runners. Carry the exact
    # declaration so the published PR can name a runnable test.
    # These name the runnable test for the published body. They are descriptive
    # rather than security-critical, so an older manifest without them degrades
    # to the filter instead of failing publication.
    $testClassName = ''
    $testMethodName = ''
    $classProperty = Find-AliasedProperty `
        -Object $manifest `
        -Names @('testClassName', 'test_class_name') `
        -Context 'Candidate manifest'
    $methodProperty = Find-AliasedProperty `
        -Object $manifest `
        -Names @('testMethodName', 'test_method_name') `
        -Context 'Candidate manifest'
    if ($null -ne $classProperty.Value -and $null -ne $methodProperty.Value) {
        $testClassName = ConvertTo-BoundedSingleLine `
            -Value $classProperty.Value `
            -Context 'Manifest test class' `
            -MaximumLength 300
        $testMethodName = ConvertTo-BoundedSingleLine `
            -Value $methodProperty.Value `
            -Context 'Manifest test method' `
            -MaximumLength 300
        foreach ($identifier in @($testClassName, $testMethodName)) {
            if ($identifier -notmatch '^[A-Za-z_][A-Za-z0-9_.]*$') {
                throw 'Manifest test class and method must be plain .NET identifiers.'
            }
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
        'UNCONDITIONAL_REPRODUCTION_TEST'
    }
    if ($sourceMarker -cne 'UNCONDITIONAL_REPRODUCTION_TEST') {
        throw 'Manifest reproduction marker must identify an unconditional reproduction test.'
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

    # A fix is optional: a reproduction that reaches trigger-certified without one
    # is still publishable, and is what every reproduction published before the
    # fix phase existed looked like. When a fix is claimed, the files it may
    # touch are pinned here so the patch cannot widen its own scope later.
    $fixProperty = Find-AliasedProperty `
        -Object $manifest `
        -Names @('fixFiles', 'fix_files') `
        -Context 'Candidate manifest'
    $fixFiles = [System.Collections.Generic.List[string]]::new()
    if ($fixProperty.Found -and $null -ne $fixProperty.Value) {
        $rawFix = @($fixProperty.Value)
        if ($rawFix.Count -gt $script:FixFileMaxCount) {
            throw "Manifest fix files must contain no more than $($script:FixFileMaxCount) entries."
        }
        $seenFix = [System.Collections.Generic.HashSet[string]]::new(
            [System.StringComparer]::OrdinalIgnoreCase
        )
        foreach ($pathValue in $rawFix) {
            $normalizedFix = Assert-ReplicationFixPath `
                -Path $pathValue `
                -AllowedPaths @($pathValue)
            if (-not $seenFix.Add($normalizedFix)) {
                throw 'Manifest fix files contain a duplicate path.'
            }
            if ($seen.Contains($normalizedFix)) {
                throw 'Manifest fix files overlap the proposed test files.'
            }
            $fixFiles.Add($normalizedFix)
        }
    }

    # The gate is handed the fix patch by path, so the manifest's own name for
    # it is documentation. Pin it anyway: a manifest that claims a fix while
    # naming some other artifact is describing a run that did not happen, and a
    # manifest that names a patch while claiming no fix files is the same
    # mismatch from the other side.
    $fixPatchProperty = Find-AliasedProperty `
        -Object $manifest `
        -Names @('fixPatch', 'fix_patch') `
        -Context 'Candidate manifest'
    $fixPatchName = if ($fixPatchProperty.Found -and $null -ne $fixPatchProperty.Value) {
        ConvertTo-BoundedSingleLine `
            -Value $fixPatchProperty.Value `
            -Context 'Manifest fix patch path' `
            -MaximumLength 128
    } else { '' }
    if ($fixFiles.Count -gt 0) {
        if ($fixPatchName -cne 'fix.patch') {
            throw 'Manifest fix patch path does not match the fixed artifact contract.'
        }
    } elseif (-not [string]::IsNullOrWhiteSpace($fixPatchName)) {
        throw 'Manifest names a fix patch but no fix files.'
    }

    return [pscustomobject]@{
        IssueNumber = $manifestIssue
        Platform = $manifestPlatform
        TestType = $testType
        TestName = $testName
        TestFilter = $testFilter
        TestClassName = $testClassName
        TestMethodName = $testMethodName
        ExpectedFailurePattern = $failurePattern
        SourceMarker = $sourceMarker
        ReproductionMarker = 'UNCONDITIONAL_REPRODUCTION_TEST'
        ProposedFiles = @($proposedFiles.ToArray() | Sort-Object)
        FixFiles = @($fixFiles.ToArray() | Sort-Object)
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

function Assert-ReplicationFixPath {
    param(
        [AllowNull()][object]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$AllowedPaths
    )

    $fixPath = Assert-CandidatePathShape -Path $Path -Context 'Fix'

    $extension = [System.IO.Path]::GetExtension($fixPath).ToLowerInvariant()
    if ($extension -notin @('.cs', '.xaml')) {
        throw 'Fix path has an unexpected extension.'
    }

    $allowed = $fixPath -cmatch '^src/(?:(?:Controls|Core|Essentials|Graphics|BlazorWebView)/src|Compatibility/Core/src|SingleProject/Resizetizer/src)/'
    if (-not $allowed) {
        throw 'Fix path is outside the established product source directories.'
    }

    foreach ($segment in $fixPath.Split('/')) {
        if ($segment -match '(?i)^(?:tests?|.*\.(?:unit|device|ui)?tests?|testcases.*)$') {
            throw 'Fix path targets test code rather than product code.'
        }
    }

    $fileName = [System.IO.Path]::GetFileName($fixPath)
    if ($fileName -match '(?i)\.(?:g|designer|generated)\.cs$') {
        throw 'Fix path targets generated source.'
    }

    if ($fixPath -cnotin $AllowedPaths) {
        throw 'Fix path is outside the reviewed fix scope.'
    }

    return $fixPath
}

function Get-ReplicationFixFilesFromPatch {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$AllowedPaths
    )

    $patchText = Read-BoundedUtf8File `
        -Path $Path `
        -MaximumBytes $script:FixPatchMaxBytes `
        -Context 'Fix patch'
    if ($patchText.Contains("`r`n")) {
        $patchText = $patchText.Replace("`r`n", "`n")
    }
    if ($patchText.Contains("`r")) {
        throw 'Fix patch contains non-normalized line endings.'
    }
    if ($patchText -match '(?m)^(?:GIT binary patch|Binary files .* differ|literal \d+|delta \d+)$') {
        throw 'Fix patch contains a binary patch.'
    }

    if ($patchText.EndsWith("`n", [System.StringComparison]::Ordinal)) {
        $patchText = $patchText.Substring(0, $patchText.Length - 1)
    }
    if ([string]::IsNullOrWhiteSpace($patchText)) {
        throw 'Fix patch is empty.'
    }
    $lines = $patchText.Split([char]"`n")

    $files = [System.Collections.Generic.List[object]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase
    )
    $totalChangedLines = 0
    $index = 0
    while ($index -lt $lines.Count) {
        $header = $lines[$index]
        if ($header -notmatch '^diff --git a/(?<old>[^\s]+) b/(?<new>[^\s]+)$') {
            throw 'Fix patch contains an unexpected preamble or malformed diff header.'
        }
        $oldPath = $Matches['old']
        $newPath = $Matches['new']
        if ($oldPath -cne $newPath) {
            throw 'Fix patch contains a rename or path mismatch.'
        }
        $fixPath = Assert-ReplicationFixPath -Path $newPath -AllowedPaths $AllowedPaths
        if (-not $seen.Add($fixPath)) {
            throw 'Fix patch contains a duplicate path.'
        }

        $index++
        $block = [System.Collections.Generic.List[string]]::new()
        while ($index -lt $lines.Count -and -not $lines[$index].StartsWith('diff --git ', [System.StringComparison]::Ordinal)) {
            $block.Add($lines[$index])
            $index++
        }

        $hasIndex = $false
        $hasOldHeader = $false
        $hasNewHeader = $false
        $hunkCount = 0
        $addedLines = 0
        $removedLines = 0
        $remainingOld = 0
        $remainingNew = 0
        $insideHunk = $false

        for ($blockIndex = 0; $blockIndex -lt $block.Count; $blockIndex++) {
            $line = $block[$blockIndex]
            if ($insideHunk -and ($remainingOld -gt 0 -or $remainingNew -gt 0)) {
                if ($line -ceq '\ No newline at end of file') {
                    continue
                }
                $marker = if ([string]::IsNullOrEmpty($line)) { ' ' } else { $line.Substring(0, 1) }
                switch ($marker) {
                    ' ' {
                        $remainingOld--
                        $remainingNew--
                    }
                    '+' {
                        $remainingNew--
                        $addedLines++
                    }
                    '-' {
                        $remainingOld--
                        $removedLines++
                    }
                    default {
                        throw 'Fix patch contains a malformed hunk body line.'
                    }
                }
                if ($remainingOld -lt 0 -or $remainingNew -lt 0) {
                    throw 'Fix patch hunk line counts do not match its content.'
                }
                continue
            }

            if ($insideHunk) {
                $insideHunk = $false
            }

            if ($line -ceq '\ No newline at end of file') {
                continue
            }
            if ($line -match '^index [0-9a-fA-F]{7,64}\.\.[0-9a-fA-F]{7,64}(?: 100644)?$') {
                if ($line -match '^index 0{7,64}\.\.') {
                    throw 'Fix patch adds rather than modifies a file.'
                }
                if ($hasIndex) {
                    throw 'Fix patch repeats its index line.'
                }
                $hasIndex = $true
                continue
            }
            if ($line -match '^(?:new file mode|deleted file mode|old mode|new mode|rename from|rename to|copy from|copy to|similarity index|dissimilarity index|Submodule )') {
                throw 'Fix patch contains an add, delete, rename, copy, mode change, or submodule change.'
            }
            if ($line -ceq "--- a/$fixPath") {
                if ($hasOldHeader) {
                    throw 'Fix patch repeats its source header.'
                }
                $hasOldHeader = $true
                continue
            }
            if ($line -ceq "+++ b/$fixPath") {
                if ($hasNewHeader) {
                    throw 'Fix patch repeats its target header.'
                }
                $hasNewHeader = $true
                continue
            }
            if ($line -match '^@@ -(?<oldStart>\d+)(?:,(?<oldCount>\d+))? \+(?<newStart>\d+)(?:,(?<newCount>\d+))? @@(?: .*)?$') {
                if (-not $hasOldHeader -or -not $hasNewHeader) {
                    throw 'Fix patch has a hunk before its file headers.'
                }
                $hunkCount++
                $insideHunk = $true
                $remainingOld = if ($Matches['oldCount']) { [int]$Matches['oldCount'] } else { 1 }
                $remainingNew = if ($Matches['newCount']) { [int]$Matches['newCount'] } else { 1 }
                if ($remainingOld -eq 0 -and $remainingNew -eq 0) {
                    throw 'Fix patch contains an empty hunk.'
                }
                continue
            }
            if ([string]::IsNullOrEmpty($line) -and $blockIndex -eq ($block.Count - 1)) {
                continue
            }

            throw 'Fix patch contains unsupported metadata or a malformed hunk.'
        }

        if ($remainingOld -gt 0 -or $remainingNew -gt 0) {
            throw 'Fix patch hunk line counts do not match its content.'
        }
        if (-not $hasIndex -or -not $hasOldHeader -or -not $hasNewHeader -or $hunkCount -eq 0) {
            throw 'Fix patch does not describe a complete modification to a tracked file.'
        }
        if (($addedLines + $removedLines) -eq 0) {
            throw 'Fix patch changes no lines.'
        }

        $totalChangedLines += $addedLines + $removedLines
        if ($totalChangedLines -gt $script:FixChangedLineMaxCount) {
            throw 'Fix patch changes too many lines.'
        }
        if ($files.Count -ge $script:FixFileMaxCount) {
            throw 'Fix patch modifies too many files.'
        }

        $files.Add([pscustomobject]@{
            Path = $fixPath
            AddedLines = $addedLines
            RemovedLines = $removedLines
            HunkCount = $hunkCount
        })
    }

    if ($files.Count -eq 0) {
        throw 'Fix patch contains no modified product files.'
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

function Assert-ReplicationFixPathsExist {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Paths
    )

    $repositoryPath = [System.IO.Path]::GetFullPath($Repository)
    foreach ($path in $Paths) {
        $fullPath = Assert-PathWithinRoot `
            -Path (Join-Path $repositoryPath $path) `
            -Root $repositoryPath `
            -Context 'Fix patch path'
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw 'Fix patch modifies a file that does not exist in the trusted checkout.'
        }
        Assert-NoLinkInExistingPath -Path $fullPath
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
        [string]$RepositoryRoot = '',
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
    # The font is named on the host page, so this runs for every candidate file
    # rather than only the test ones. A host file is otherwise exempt from the
    # test-shape guards below, which is how a missing font once reached review.
    if ($RepositoryRoot) {
        Assert-ReplicationFontIsAvailable `
            -Content $normalized `
            -Path $Path `
            -RepositoryRoot $RepositoryRoot `
            -Platform ([string]$Manifest.Platform)
    }
    if (
        [System.IO.Path]::GetExtension($Path) -ieq '.cs' -and
        $normalizedPath -cnotmatch '^src/Controls/tests/TestCases\.HostApp/'
    ) {
        Assert-ReplicationTestLifecycleSafety `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationLeakTestMethodology `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationGestureTravel `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationProbeGeometryIsMeasured `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationGestureIsSynchronized `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationPointerSequenceIsSelfContained `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationGeometryOracleIsPinned `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationHandlerRegistrationIsNotTautological `
            -Content $normalized `
            -Path $Path `
            -RepositoryRoot $RepositoryRoot
        Assert-ReplicationWaitResultIsUsed `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationTestPlatformScope `
            -Content $normalized `
            -Path $Path `
            -Platform ([string]$Manifest.Platform)
        Assert-ReplicationTestRunsOnEvidencePlatform `
            -Path $Path `
            -Platform ([string]$Manifest.Platform) `
            -TestType ([string]$Manifest.TestType) `
            -RepositoryRoot $RepositoryRoot
        Assert-ReplicationEnvironmentGateSkips `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationPlatformViewIdentity `
            -Content $normalized `
            -Path $Path
        Assert-ReplicationVerdictIsNotSelfAnnounced `
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

    Assert-ReplicationConditionalCompilationBalance -Content $normalized -Path $Path
    Assert-ReplicationGeneratedSourceSafety -Content $normalized -Path $Path
    Assert-ReplicationPlatformSourceSafety `
        -Content $normalized `
        -Path $Path `
        -Platform ([string]$Manifest.Platform)
}

function Assert-ReplicationCandidateSources {
    param(
        [Parameter(Mandatory = $true)][object]$Manifest,
        [Parameter(Mandatory = $true)][object[]]$CandidateFiles,
        [string]$RepositoryRoot = ''
    )

    Assert-ManifestMatchesCandidateFiles -Manifest $Manifest -CandidateFiles $CandidateFiles

    $testAttributeFound = $false
    $testNameFound = $false
    $deviceTestIsSelectable = $false
    $testNameLeaf = @($Manifest.TestName -split '[.+]')[-1]
    # dotnet test --filter matches fully qualified names by substring, so the
    # repository convention of naming the type Issue<number><Scenario> is
    # selected by the Issue<number> filter. Require the filter token to start an
    # identifier and allow the descriptive suffix the convention appends.
    $testNamePattern = "(?<![A-Za-z0-9_])$([regex]::Escape($testNameLeaf))(?![0-9])"
    foreach ($file in $CandidateFiles) {
        if ($file.Mode -cne '100644') {
            throw 'Candidate file mode must be regular and non-executable.'
        }
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
                }
            }
            if ($file.Content -match $testNamePattern) {
                $testNameFound = $true
            }
            if (
                $Manifest.TestType -ceq 'DeviceTest' -and
                (Assert-ReplicationDeviceTestIsSelectable `
                        -Content $file.Content `
                        -Path $file.Path `
                        -Issue ([int]$Manifest.IssueNumber))
            ) {
                $deviceTestIsSelectable = $true
            }
        }
        Assert-SourceTextIsSafe `
            -Content $file.Content `
            -Path $file.Path `
            -Manifest $Manifest `
            -RepositoryRoot $RepositoryRoot
    }

    # Whether the oracle merely restates what the host page already shows can
    # only be decided with both files in hand.
    $candidateContents = @{}
    foreach ($file in $CandidateFiles) { $candidateContents[$file.Path] = $file.Content }
    Assert-ReplicationOracleIsNotInitialState -Files $candidateContents
    Assert-ReplicationVerdictIsNotComputedByTheApp -Files $candidateContents

    if (-not $testAttributeFound) {
        throw 'Candidate files do not add a test method in the expected test project.'
    }
    if (-not $testNameFound) {
        throw 'Candidate files do not contain the named test from the exact filter.'
    }
    if ($Manifest.TestType -ceq 'DeviceTest' -and -not $deviceTestIsSelectable) {
        throw (
            'The candidate device test cannot be selected on device: no file declares ' +
            "[Category(`"Issue$($Manifest.IssueNumber)`")]. The runner reads the bare filter " +
            'token as a category name, so with no test declaring it the run selects no ' +
            'categories and executes nothing.')
    }
}

function Get-ReportableArtifactName {
    <#
        .SYNOPSIS
        Renders an artifact name safe to embed in a rejection message.

        .DESCRIPTION
        A rejection that does not say which file it means costs a log download
        to explain, and several lost reproductions did exactly that. The name
        comes from the file system, so it is reduced to a conservative
        character set and bounded before it reaches a message or Markdown.
    #>
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Name)

    $safe = [regex]::Replace($Name, '[^A-Za-z0-9._-]', '?')
    if ($safe.Length -gt 100) {
        $safe = $safe.Substring(0, 100)
    }
    return $safe
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
        'test-without-fix.log',
        'negative-control-baseline.cs',
        'negative-control-oracle.cs',
        'negative-control-variant.cs',
        'negative-control-console.log',
        'negative-control-result.json',
        'fix-control-result.json',
        'fix-control-console.log',
        'restoration-result.json',
        'restoration-console.log'
    )

    # The runner's own result document is retained beside the machine result as
    # evidence, so the gate must expect it. Its extension follows the runner
    # (.trx for VSTest, .xml for NUnit), which is why this is a pattern rather
    # than another literal name.
    $retainedResultPattern = '^verification-test-result\.(?:trx|xml)$'

    $mediaItems = @(Get-ChildItem -LiteralPath $mediaRoot -Force)
    if ($mediaItems.Count -gt 20) {
        throw 'Evidence directory contains too many artifacts.'
    }
    foreach ($item in $mediaItems) {
        $isCombinedVerificationFile = (
            $verificationRoot -ceq $mediaRoot -and
            (
                $item.Name -cin $allowedVerificationNames -or
                $item.Name -cmatch '^test-failure-[A-Za-z0-9_.-]+\.log$' -or
                $item.Name -cmatch '^verification-console-run-[2-3]\.log$' -or
                $item.Name -cmatch $retainedResultPattern
            )
        )
        if ($item.PSIsContainer) {
            throw 'Evidence directory must not contain nested directories.'
        }
        if ($item.Name -cnotin $allowedMediaNames -and -not $isCombinedVerificationFile) {
            throw "Evidence directory contains an unexpected artifact: '$(Get-ReportableArtifactName -Name $item.Name)'."
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
            $_.Name -cmatch '^test-failure-[A-Za-z0-9_.-]+\.log$' -or
            $_.Name -cmatch $retainedResultPattern
        })
    } else {
        @(Get-ChildItem -LiteralPath $verificationRoot -Force)
    }
    if ($verificationItems.Count -gt 26) {
        throw 'Verification directory contains too many artifacts.'
    }
    foreach ($item in $verificationItems) {
        if ($item.PSIsContainer) {
            throw 'Verification directory must not contain nested directories.'
        }
        if (
            $item.Name -cnotin $allowedVerificationNames -and
            $item.Name -cnotmatch '^test-failure-[A-Za-z0-9_.-]+\.log$' -and
            $item.Name -cnotmatch '^verification-console-run-[2-3]\.log$' -and
            $item.Name -cnotmatch '^negative-control-console-run-[2-3]\.log$' -and
            $item.Name -cnotmatch $retainedResultPattern
        ) {
            # Naming the file matters: builds 15032408 and 15032410 each
            # finished a device reproduction and were discarded here, and the
            # message gave no way to tell which artifact was unexpected. The
            # name reaches a log, so only a bounded safe subset is echoed.
            throw "Verification directory contains an unexpected artifact: '$(Get-ReportableArtifactName -Name $item.Name)'."
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
            if ($item.Name -cnotin @(
                    'verification-console.log',
                    'verification-result.json',
                    'negative-control-baseline.cs',
                    'negative-control-oracle.cs',
                    'negative-control-variant.cs',
                    'negative-control-console.log',
                    'negative-control-result.json',
                    'fix-control-result.json',
                    'fix-control-console.log',
                    'restoration-result.json',
                    'restoration-console.log') -and
                $item.Name -cnotmatch '^verification-console-run-[2-3]\.log$' -and
                $item.Name -cnotmatch '^negative-control-console-run-[2-3]\.log$' -and
                $item.Name -cnotmatch $retainedResultPattern) {
                throw ("Machine-readable verification directory contains an unexpected artifact: " +
                    "'$(Get-ReportableArtifactName -Name $item.Name)'.")
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
            'confirmedRuns',
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
    # The orchestrator replays the plan before it claims a reproduction. Check
    # the count here too, so a single lucky observation cannot reach a PR even
    # if the orchestrator stops enforcing it.
    $confirmedRuns = ConvertTo-PositiveInteger `
        -Value (Find-AliasedProperty `
            -Object $result `
            -Names @('confirmedRuns') `
            -Context 'Replication execution result' `
            -Required).Value `
        -Context 'Replication execution confirmed run count'
    if ($confirmedRuns -lt 2) {
        throw 'Replication execution result must record at least two confirmed reproduction runs.'
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
                # The recorder writes the decoded frame count so a container
                # that holds no frames cannot pass as a recording. This
                # allowlist is strict by design, so omitting the field here
                # made the publisher reject every run that produced one, at
                # the last gate and after all the device work: build 15051402
                # died on "unexpected property 'decodedFrames'".
                'decodedFrames',
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

function Assert-ReplicationAuthoritativeResult {
    <#
        .SYNOPSIS
            Reads the runner's own result document and requires it to show
            exactly one executed test, which failed, and which is the test the
            manifest claims.

        .DESCRIPTION
            Every other selection check in this gate reads a number some script
            wrote down. This reads the document the test runner produced.

            It exists because of a specific failure: an XHarness method filter
            cannot express a display name containing a comma, so a theory test
            selected nothing at all, the runner reported no count, and a
            reproduction whose test never executed was published as evidence
            that it failed. A count that is absent is indistinguishable from a
            count of zero unless something reads the run itself.

            Three formats appear here. xUnit v2 XML is what the device test
            lanes emit, TRX is what VSTest emits, and NUnit v3 XML is what the
            UI test lanes emit. All three are read the same way: find the test
            elements, require exactly one, require it to have failed, and
            require its identity to be the manifest's.
    #>
    param(
        [Parameter(Mandatory = $true)][string]$VerificationRoot,
        [Parameter(Mandatory = $true)][string]$TestClass,
        [Parameter(Mandatory = $true)][string]$TestMethod
    )

    $candidates = @(
        Get-ChildItem -LiteralPath $VerificationRoot -Force -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -cmatch '^verification-test-result\.(?:trx|xml)$' })
    if ($candidates.Count -eq 0) {
        throw ('The verification evidence has no authoritative test result document, so ' +
            'there is no proof the named test executed at all.')
    }
    if ($candidates.Count -gt 1) {
        throw 'The verification evidence has more than one authoritative test result document.'
    }

    $file = $candidates[0]
    if ($file.Length -gt 4MB) {
        throw 'The authoritative test result document exceeds the trusted size limit.'
    }

    $document = [System.Xml.XmlDocument]::new()
    # A result document arrives from the untrusted job. Resolving a DTD or an
    # external entity in it would let it read this machine or hang the gate.
    $document.XmlResolver = $null
    $readerSettings = [System.Xml.XmlReaderSettings]::new()
    $readerSettings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $readerSettings.XmlResolver = $null
    try {
        $stream = [IO.File]::OpenRead($file.FullName)
        try {
            $reader = [System.Xml.XmlReader]::Create($stream, $readerSettings)
            try { $document.Load($reader) } finally { $reader.Dispose() }
        } finally { $stream.Dispose() }
    } catch {
        throw "The authoritative test result document could not be read as XML: $($_.Exception.Message)"
    }

    $observed = @(Get-ReplicationAuthoritativeTestEntries -Document $document)
    if ($observed.Count -eq 0) {
        throw ('The authoritative test result document records no executed test, so the ' +
            'reported failure did not come from the named test. This is what a filter that ' +
            'selects nothing looks like.')
    }
    if ($observed.Count -gt 1) {
        throw ("The authoritative test result document records $($observed.Count) executed " +
            'tests, so the failure is not attributable to the named test.')
    }

    $entry = $observed[0]
    if (-not $entry.Failed) {
        throw ('The authoritative test result document records the targeted test as ' +
            "'$($entry.Outcome)', but the reproduction is published as a failing test.")
    }

    # The runner spells identity differently per format: a bare method name with
    # a separate type, or one fully qualified string. Requiring both tokens to
    # appear in the recorded identity covers all three without pinning any one
    # runner's spelling of the separator.
    #
    # 'name' is a display string, not an identity. A test carrying a DisplayName,
    # or a runner that humanizes PascalCase into words, records a name that no
    # method-name match can satisfy -- build 15073806 passed all four arms and
    # was refused for a recorded name of 'Picker ItemsSource Does Not Retain
    # Picker After Unload'. Where the document records the method itself, that is
    # the identity and it must match exactly.
    $shortClass = ($TestClass -split '\.')[-1]
    if (-not [string]::IsNullOrWhiteSpace($entry.Method)) {
        if ($entry.Method -cne $TestMethod) {
            throw ('The authoritative test result document records a test method ' +
                "'$(Get-ReportableArtifactName -Name $entry.Method)', which is not the method the manifest claims.")
        }
    } elseif ($entry.Name -cnotmatch [regex]::Escape($TestMethod)) {
        throw ('The authoritative test result document records a test named ' +
            "'$(Get-ReportableArtifactName -Name $entry.Name)', which is not the method the manifest claims.")
    }
    if (-not [string]::IsNullOrWhiteSpace($entry.Type)) {
        if ($entry.Type -cnotmatch [regex]::Escape($shortClass)) {
            throw ('The authoritative test result document records a test on type ' +
                "'$(Get-ReportableArtifactName -Name $entry.Type)', which is not the class the manifest claims.")
        }
    } elseif ($entry.Name -cnotmatch [regex]::Escape($shortClass)) {
        throw ('The authoritative test result document records a test named ' +
            "'$(Get-ReportableArtifactName -Name $entry.Name)', which is not the class the manifest claims.")
    }

    return [pscustomobject]@{
        Name = $entry.Name
        Type = $entry.Type
        Outcome = $entry.Outcome
        Document = $file.Name
    }
}

function Get-ReplicationAuthoritativeTestEntries {
    <#
        .SYNOPSIS
            Extracts the executed tests from an xUnit, TRX or NUnit document.

        .DESCRIPTION
            Selection is by local name so that a namespaced document reads the
            same as a bare one. TRX in particular is always namespaced, and a
            namespace-sensitive query against it silently returns nothing --
            which would read as "no test executed" and reject a valid run.

            'Name' is a display string and 'Method' is an identity. xUnit and
            NUnit both record the runtime method separately, and a test that
            carries a DisplayName -- or a runner that humanizes PascalCase --
            makes the two differ. Callers matching a manifest must use Method
            when it is present; Name is a fallback for formats without one.
    #>
    param([Parameter(Mandatory = $true)][System.Xml.XmlDocument]$Document)

    $entries = [System.Collections.Generic.List[object]]::new()

    foreach ($node in @($Document.SelectNodes("//*[local-name()='test']"))) {
        $result = [string]$node.GetAttribute('result')
        $entries.Add([pscustomobject]@{
            Name = [string]$node.GetAttribute('name')
            Method = [string]$node.GetAttribute('method')
            Type = [string]$node.GetAttribute('type')
            Outcome = $result
            Failed = $result -imatch '^fail'
        }) | Out-Null
    }
    if ($entries.Count -gt 0) { return $entries.ToArray() }

    foreach ($node in @($Document.SelectNodes("//*[local-name()='UnitTestResult']"))) {
        $outcome = [string]$node.GetAttribute('outcome')
        $entries.Add([pscustomobject]@{
            Name = [string]$node.GetAttribute('testName')
            Method = ''
            Type = ''
            Outcome = $outcome
            Failed = $outcome -imatch '^fail'
        }) | Out-Null
    }
    if ($entries.Count -gt 0) { return $entries.ToArray() }

    foreach ($node in @($Document.SelectNodes("//*[local-name()='test-case']"))) {
        $result = [string]$node.GetAttribute('result')
        $fullName = [string]$node.GetAttribute('fullname')
        if ([string]::IsNullOrWhiteSpace($fullName)) {
            $fullName = [string]$node.GetAttribute('name')
        }
        $entries.Add([pscustomobject]@{
            Name = $fullName
            Method = [string]$node.GetAttribute('methodname')
            Type = [string]$node.GetAttribute('classname')
            Outcome = $result
            Failed = $result -imatch '^fail'
        }) | Out-Null
    }

    return $entries.ToArray()
}

function Get-ReplicationFixArmEvidenceFromRoot {
    param(
        [Parameter(Mandatory = $true)][string]$VerificationRoot,
        [switch]$Enabled
    )

    $evidence = [pscustomobject]@{
        FixRuns = 0
        FixPasses = 0
        RestorationRuns = 0
        RestorationFailures = 0
    }

    if (-not $Enabled) {
        return $evidence
    }

    $fixPath = Join-Path $VerificationRoot 'fix-control-result.json'
    $restorationPath = Join-Path $VerificationRoot 'restoration-result.json'
    $hasFix = Test-Path -LiteralPath $fixPath -PathType Leaf
    $hasRestoration = Test-Path -LiteralPath $restorationPath -PathType Leaf
    if (-not $hasFix -and -not $hasRestoration) {
        return $evidence
    }
    # Half a control is not a control. A run that reports the fix turning the
    # test green without also proving that removing the fix turns it red again
    # has shown nothing that a rebuild would not have shown.
    if ($hasFix -xor $hasRestoration) {
        throw ('A fix arm was reported without its restoration arm, so the green result cannot be ' +
            'attributed to the fix: ' +
            [System.IO.Path]::GetFileName($(if ($hasFix) { $restorationPath } else { $fixPath })) +
            ' is missing.')
    }

    $fixValue = Read-BoundedUtf8File `
        -Path $fixPath `
        -MaximumBytes $script:CandidateFileMaxBytes `
        -Root $VerificationRoot `
        -Context 'Fix control result' |
        ConvertFrom-Json
    $fixRuns = ConvertTo-PositiveInteger `
        -Value (Find-AliasedProperty `
            -Object $fixValue `
            -Names @('runCount') `
            -Context 'Fix control' `
            -Required).Value `
        -Context 'Fix control run count'
    $fixPasses = [int](Find-AliasedProperty `
            -Object $fixValue `
            -Names @('passCount') `
            -Context 'Fix control' `
            -Required).Value
    if ($fixPasses -lt 0 -or $fixPasses -gt $fixRuns) {
        throw 'The fix control reports more passes than runs.'
    }

    $restorationValue = Read-BoundedUtf8File `
        -Path $restorationPath `
        -MaximumBytes $script:CandidateFileMaxBytes `
        -Root $VerificationRoot `
        -Context 'Restoration result' |
        ConvertFrom-Json
    $restorationRuns = ConvertTo-PositiveInteger `
        -Value (Find-AliasedProperty `
            -Object $restorationValue `
            -Names @('runCount') `
            -Context 'Restoration' `
            -Required).Value `
        -Context 'Restoration run count'
    $restorationFailures = [int](Find-AliasedProperty `
            -Object $restorationValue `
            -Names @('failureCount') `
            -Context 'Restoration' `
            -Required).Value
    if ($restorationFailures -lt 0 -or $restorationFailures -gt $restorationRuns) {
        throw 'The restoration arm reports more failures than runs.'
    }

    $evidence.FixRuns = $fixRuns
    $evidence.FixPasses = $fixPasses
    $evidence.RestorationRuns = $restorationRuns
    $evidence.RestorationFailures = $restorationFailures
    return $evidence
}

function Assert-ReplicationVerificationEvidence {
    param(
        [Parameter(Mandatory = $true)][object]$Inventory,
        [Parameter(Mandatory = $true)][object]$Manifest,
        [object]$FixArms
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
                'signatureEquivalent',
                'effectiveFailureSignature',
                'infrastructureFailure',
                'selectionAmbiguous',
                'executedTestCounts',
                'retainedResultFiles',
                'verificationPassed',
                'requestedRunCount',
                'completedRunCount',
                'consistentRuns',
                'stableFailureMessage',
                'observedFailureMessages',
                'negativeControl',
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
        $effectiveSignature = ConvertTo-BoundedSingleLine `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('effectiveFailureSignature') `
                -Context 'Verification result' `
                -Required).Value `
            -Context 'Verification effective failure signature' `
            -MinimumLength 3 `
            -MaximumLength 1000
        # The published signature must be words the targeted test actually
        # produced, whatever the agent predicted beforehand. Verifier output
        # re-wraps long assertions, so compare with runs of whitespace collapsed.
        $normalizedActual = ([regex]::Replace($actualFailureMessage, '\s+', ' ')).Trim()
        $normalizedEffective = ([regex]::Replace($effectiveSignature, '\s+', ' ')).Trim()
        if (-not $normalizedActual.Contains(
            $normalizedEffective,
            [StringComparison]::Ordinal)) {
            throw 'The targeted test failure message does not contain the published failure signature.'
        }
        # Defense in depth: the orchestrator rejects a non-attributive oracle at
        # proposal time, but the message the run actually produced is the one
        # reviewers will read, so re-check both before any credential is used.
        Assert-ReplicationOracleIsFalsifiable `
            -ExpectedFailureSignature $effectiveSignature `
            -TestFilter $resultFilter
        Assert-ReplicationOracleIsFalsifiable `
            -ExpectedFailureSignature $actualFailureMessage `
            -TestFilter $resultFilter
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
            signatureEquivalent = $true
            infrastructureFailure = $false
            verificationPassed = $true
            consistentRuns = $true
        }
        $signatureMatchedProperty = Find-AliasedProperty `
            -Object $result `
            -Names @('signatureMatched') `
            -Context 'Verification result' `
            -Required
        if ($signatureMatchedProperty.Value -isnot [bool]) {
            throw "Verification result 'signatureMatched' must be a boolean."
        }
        $script:ValidatedEffectiveFailureSignature = $effectiveSignature
        if ($signatureMatchedProperty.Value -and
            $effectiveSignature -cne $resultSignature) {
            throw 'An exactly matched signature must be published unchanged.'
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
        # A run produced by an older trusted verifier carries neither field. Both
        # are therefore optional, but strict whenever the verifier reported them,
        # so a build already in flight is not thrown away for a schema addition.
        $ambiguousProperty = $result.PSObject.Properties['selectionAmbiguous']
        if ($ambiguousProperty) {
            if ($ambiguousProperty.Value -isnot [bool] -or $ambiguousProperty.Value) {
                throw ('Verification result reports an ambiguous test selection, ' +
                    'so the failure is not attributable to the named test.')
            }
        }
        $executedCountsProperty = $result.PSObject.Properties['executedTestCounts']
        if ($executedCountsProperty) {
            foreach ($executedCount in @($executedCountsProperty.Value)) {
                # A recorded count is the runner's own answer to "how many tests
                # did this filter select". Anything other than one means the
                # published failure cannot be attributed to the single test.
                if ([string]$executedCount -cne '1') {
                    throw ('Verification result executed ' +
                        "$executedCount tests instead of exactly one targeted test.")
                }
            }
        }
        $requestedRunCount = ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('requestedRunCount') `
                -Context 'Verification result' `
                -Required).Value `
            -Context 'Verification requested run count'
        $completedRunCount = ConvertTo-PositiveInteger `
            -Value (Find-AliasedProperty `
                -Object $result `
                -Names @('completedRunCount') `
                -Context 'Verification result' `
                -Required).Value `
            -Context 'Verification completed run count'
        if ($requestedRunCount -gt 3 -or $completedRunCount -gt 3) {
            throw 'Verification result reports an implausible number of runs.'
        }
        if ($completedRunCount -ne $requestedRunCount) {
            throw 'Verification result did not complete every requested run.'
        }
        if ($completedRunCount -lt $script:VerificationMinimumRunCount) {
            throw ('The targeted test must fail in at least ' +
                "$script:VerificationMinimumRunCount independent runs to prove the defect is deterministic.")
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

        # Every independent execution has to carry the same proof, otherwise a
        # candidate could pass by failing once and being ignored afterwards.
        $exactlyOneTestExecuted = $true
        # The verifier compares the failure messages across runs and records the
        # answer. Asserting it here instead meant the pull request claimed a
        # stable message on the gate's authority while the gate had not looked.
        $stableProperty = Find-AliasedProperty `
            -Object $result `
            -Names @('stableFailureMessage', 'stable_failure_message') `
            -Context 'Verification result'
        $stableFailureMessage = $stableProperty.Found -and [bool]$stableProperty.Value
        $consoleNames = @('verification-console.log')
        for ($runIndex = 2; $runIndex -le $completedRunCount; $runIndex++) {
            $consoleNames += "verification-console-run-$runIndex.log"
        }
        foreach ($consoleName in $consoleNames) {
            $consolePath = Join-Path $Inventory.VerificationRoot $consoleName
            if (-not (Test-Path -LiteralPath $consolePath -PathType Leaf)) {
                throw "Verification evidence is missing the console log for every completed run: $consoleName."
            }
            $console = Read-BoundedUtf8File `
                -Path $consolePath `
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

            # The pull request claims exactly one test was selected and
            # executed. That claim was previously hard-coded here, so a run that
            # dragged in a neighbouring test would still have been published
            # saying it had not.
            #
            # The verifier states the count it acted on in its own summary, and
            # that line is the only count a UI test run prints: device runs also
            # echo the counts parsed from the runner's result file, but UI runs
            # do not, so requiring those would refuse every UI reproduction.
            # All fifty-two summaries in the collected pipeline logs read
            # "All 1 test(s)", and every one of the UI runs among them has it.
            $summary = [regex]::Match($console, '(?i)\bAll\s+(?<count>\d+)\s+test\(s\)\s+FAILED as expected')
            if (-not $summary.Success -or [int]$summary.Groups['count'].Value -ne 1) {
                $exactlyOneTestExecuted = $false
            }
            # A device run additionally reports what the runner itself counted,
            # which is stronger than the verifier's own summary. Honour it when
            # it is there.
            foreach ($total in [regex]::Matches($console, '(?i)Parsed test results:.*?\bTotal=(?<total>\d+)')) {
                if ([int]$total.Groups['total'].Value -ne 1) {
                    $exactlyOneTestExecuted = $false
                }
            }
        }

        Add-Member -InputObject $result -NotePropertyName 'validatedRunCount' -NotePropertyValue $completedRunCount -Force

        # Grade what was actually established. Everything above proves the named
        # test failed repeatedly and identically; none of it shows the failure is
        # caused by the reported defect rather than by something incidental to
        # the scenario. That distinction is what reviewers kept having to make by
        # hand, so it is computed here and carried into the pull request.
        $negativeRuns = 0
        $negativePasses = 0
        # The control runs after this verification result is written, so it can
        # never appear inside it. Reading it there graded every reproduction as
        # 'no negative control was run', including build 15033161, whose control
        # passed 3 of 3 on device. Read the artifact the verifier actually
        # produces for the control instead.
        $controlResultPath = Join-Path $Inventory.VerificationRoot 'negative-control-result.json'
        if (Test-Path -LiteralPath $controlResultPath -PathType Leaf) {
            $controlValue = Read-BoundedUtf8File `
                -Path $controlResultPath `
                -MaximumBytes $script:CandidateFileMaxBytes `
                -Root $Inventory.VerificationRoot `
                -Context 'Negative control result' |
                ConvertFrom-Json
            $negativeRuns = ConvertTo-PositiveInteger `
                -Value (Find-AliasedProperty `
                    -Object $controlValue `
                    -Names @('runCount') `
                    -Context 'Negative control' `
                    -Required).Value `
                -Context 'Negative control run count'
            $negativePasses = [int](Find-AliasedProperty `
                    -Object $controlValue `
                    -Names @('passCount') `
                    -Context 'Negative control' `
                    -Required).Value

            if ($negativePasses -gt $negativeRuns) {
                throw 'The negative control reports more passes than runs.'
            }

            $baselineSourcePath = Join-Path $Inventory.VerificationRoot 'negative-control-baseline.cs'
            $variantSourcePath = Join-Path $Inventory.VerificationRoot 'negative-control-variant.cs'
            foreach ($required in @($baselineSourcePath, $variantSourcePath)) {
                if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
                    throw ('A negative control was reported without its source snapshots, so its claim that ' +
                        'removing the trigger turns the test green cannot be checked: ' +
                        [System.IO.Path]::GetFileName($required) + '.')
                }
            }

            $informativeArguments = @{
                BaselineSource = Read-BoundedUtf8File `
                    -Path $baselineSourcePath `
                    -MaximumBytes $script:CandidateFileMaxBytes `
                    -Root $Inventory.VerificationRoot `
                    -Context 'Negative control baseline source'
                ControlSource = Read-BoundedUtf8File `
                    -Path $variantSourcePath `
                    -MaximumBytes $script:CandidateFileMaxBytes `
                    -Root $Inventory.VerificationRoot `
                    -Context 'Negative control variant source'
                TestFilter = $Manifest.TestName
            }

            # A UI test's control edits the HostApp page and never writes the
            # test file, so the assertions it must preserve are in a third
            # snapshot. Read from the page instead, the guard finds no
            # assertions at all and refuses a control that is correct.
            $oracleSourcePath = Join-Path $Inventory.VerificationRoot 'negative-control-oracle.cs'
            if (Test-Path -LiteralPath $oracleSourcePath -PathType Leaf) {
                $oracleSource = Read-BoundedUtf8File `
                    -Path $oracleSourcePath `
                    -MaximumBytes $script:CandidateFileMaxBytes `
                    -Root $Inventory.VerificationRoot `
                    -Context 'Negative control oracle source'
                $informativeArguments['OracleBaselineSource'] = $oracleSource
                $informativeArguments['OracleControlSource'] = $oracleSource
            }

            Assert-ReplicationNegativeControlIsInformative @informativeArguments
        }

        # The two arms that turn a reproduction into a regression oracle are read
        # by the caller, which also decides whether the fix survived. Reading
        # them here unconditionally would let a run claim 'the fix makes it
        # green' while publishing no fix at all, which is the one claim nobody
        # could check from the PR.
        $arms = if ($FixArms) {
            $FixArms
        } else {
            [pscustomobject]@{ FixRuns = 0; FixPasses = 0; RestorationRuns = 0; RestorationFailures = 0 }
        }

        $certification = Get-ReplicationCertification -Evidence @{
            runtimeAvailable       = $true
            baselineRuns           = $completedRunCount
            baselineFailures       = $completedRunCount
            stableFailureMessage   = $stableFailureMessage
            exactlyOneTestExecuted = $exactlyOneTestExecuted
            negativeControlRuns    = $negativeRuns
            negativeControlPasses  = $negativePasses
            fixControlRuns         = $arms.FixRuns
            fixControlPasses       = $arms.FixPasses
            restorationRuns        = $arms.RestorationRuns
            restorationFailures    = $arms.RestorationFailures
        } -RequiredRuns $script:VerificationMinimumRunCount

        if (-not $certification.Publish) {
            throw ('The candidate is graded ' + $certification.Level + ' and is not publishable: ' +
                (@($certification.Reasons) -join ' '))
        }

        Add-Member -InputObject $result -NotePropertyName 'certificationLevel' `
            -NotePropertyValue $certification.Level -Force
        Add-Member -InputObject $result -NotePropertyName 'certificationSummary' `
            -NotePropertyValue (Get-ReplicationCertificationSummary -Certification $certification) -Force
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
        [string]$FixPatchPath,
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
            -CandidateFiles $candidateFiles `
            -RepositoryRoot $repoPath

        # The fix, if there is one, is a second artifact validated on its own
        # terms: modification-only, product paths only, and no wider than the
        # scope the manifest already committed to.
        $fixFiles = @()
        $hasFixPatch = -not [string]::IsNullOrWhiteSpace($FixPatchPath)
        if ($hasFixPatch) {
            if (-not $hasPatch) {
                throw 'A fix patch may only accompany a patch candidate.'
            }
            if (@($manifest.FixFiles).Count -eq 0) {
                throw 'A fix patch was provided but the manifest names no fix files.'
            }
            $fixFiles = @(Get-ReplicationFixFilesFromPatch `
                -Path $FixPatchPath `
                -AllowedPaths @($manifest.FixFiles))
            $patchedPaths = @($fixFiles | ForEach-Object { $_.Path })
            foreach ($declared in @($manifest.FixFiles)) {
                if ($declared -cnotin $patchedPaths) {
                    throw 'The manifest names a fix file the fix patch never modifies.'
                }
            }
            Assert-ReplicationFixPathsExist `
                -Repository $repoPath `
                -Paths $patchedPaths
        } elseif (@($manifest.FixFiles).Count -gt 0) {
            throw 'The manifest names fix files but no fix patch was provided.'
        }

        if ($manifest.ArtifactContract) {
            Assert-ReplicationExecutionResult `
                -Directory $EvidenceDir `
                -Manifest $manifest
        }
        $inventory = Get-ReplicationEvidenceInventory -Directory $EvidenceDir

        # Read the run rather than a summary of it. Every other selection check
        # here trusts a number some script wrote down, and a number that was
        # never written is indistinguishable from a run that selected nothing --
        # which is exactly how a test that never executed was once published as
        # proof that it failed.
        if ($manifest.TestType -ceq 'DeviceTest' -and
            -not [string]::IsNullOrWhiteSpace($manifest.TestClassName) -and
            -not [string]::IsNullOrWhiteSpace($manifest.TestMethodName)) {
            $authoritative = Assert-ReplicationAuthoritativeResult `
                -VerificationRoot $inventory.VerificationRoot `
                -TestClass $manifest.TestClassName `
                -TestMethod $manifest.TestMethodName
            Write-Host ("The runner's own result document records exactly one executed test, " +
                "'$($authoritative.Name)', which failed.")
        }

        # A fix whose arms did not hold is discarded here rather than allowed to
        # drag the reproduction down with it. The grader treats a failed fix arm
        # as decisive evidence against the test, which is correct when a fix is
        # published; the right answer to a fix that did not work is to publish
        # the reproduction alone, exactly as every run before the fix phase did.
        $fixArms = Get-ReplicationFixArmEvidenceFromRoot `
            -VerificationRoot $inventory.VerificationRoot `
            -Enabled:$hasFixPatch
        if ($hasFixPatch) {
            $fixOutcome = Get-ReplicationControlOutcome `
                -Requested $fixArms.FixRuns `
                -Observed $fixArms.FixPasses `
                -Required $script:VerificationMinimumRunCount
            $restorationOutcome = Get-ReplicationControlOutcome `
                -Requested $fixArms.RestorationRuns `
                -Observed $fixArms.RestorationFailures `
                -Required $script:VerificationMinimumRunCount
            if (-not ($fixOutcome.Attempted -and $fixOutcome.Satisfied -and
                    $restorationOutcome.Attempted -and $restorationOutcome.Satisfied)) {
                Write-Host ('The fix did not satisfy both control arms, so it is discarded and the ' +
                    'reproduction is published on its own.')
                $hasFixPatch = $false
                $fixFiles = @()
                $fixArms = $null
            }
        }

        $null = Read-ReplicationEvidenceMetadata `
            -Inventory $inventory `
            -Manifest $manifest
        $verificationResult = Assert-ReplicationVerificationEvidence `
            -Inventory $inventory `
            -Manifest $manifest `
            -FixArms $fixArms
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
            testClassName = $manifest.TestClassName
            testMethodName = $manifest.TestMethodName
            testFilter = $manifest.TestFilter
            expectedFailureSignature = $manifest.ExpectedFailurePattern
            observedFailureSignature = $script:ValidatedEffectiveFailureSignature
            expectedFailurePattern = $manifest.ExpectedFailurePattern
            actualFailureMessage = if ($verificationResult) {
                [string]$verificationResult.actualFailureMessage
            } else {
                $null
            }
            verificationRunCount = if ($verificationResult) {
                [int]$verificationResult.validatedRunCount
            } else {
                0
            }
            certificationLevel = if ($verificationResult) {
                [string]$verificationResult.certificationLevel
            } else {
                'candidate-scenario'
            }
            certificationSummary = if ($verificationResult) {
                [string]$verificationResult.certificationSummary
            } else {
                ''
            }
            reproductionMarker = $manifest.ReproductionMarker
            files = @($manifest.ProposedFiles)
            proposedFiles = @($manifest.ProposedFiles)
            fixFiles = @($fixFiles | ForEach-Object { $_.Path })
            fixPatch = if ($hasFixPatch) { 'fix.patch' } else { $null }
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
        -FixPatchPath $FixPatchPath `
        -CandidateCommit $CandidateCommit `
        -BaseCommit $BaseCommit `
        -EvidenceDir $EvidenceDir `
        -IssueNumber $IssueNumber `
        -Platform $Platform `
        -OutputPath $OutputPath `
        -MediaProbe $MediaProbe
}
