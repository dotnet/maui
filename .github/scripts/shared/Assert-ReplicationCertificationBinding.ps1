#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Binds a replication certification to the immutable inputs that produced it.

.DESCRIPTION
    A certification level is a claim about work that happened hours earlier, on
    a different agent, next to generated code. Until it names what it was
    computed over, "certified-oracle" is a string in a JSON file that anything
    with write access to that file can assert.

    The binding is the missing half. It records, as content hashes and immutable
    commit identities, exactly what the grade was earned on: the pipeline
    revision and definition, the trusted tree the gates came from, the
    replication base and the execution HEAD, the two patches, the typed selector
    identity together with the counts the trusted runner discovered and
    executed, the trusted verifier/runner/validator identities, and every
    evidence file. One digest covers the whole set.

    Verification recomputes each field from the artifacts in hand and compares.
    A mismatch, a missing field, a malformed field, or a field nobody expected
    all fail closed -- the last one because a binding that quietly accepts new
    fields is a binding an author can extend to mean whatever they want.
#>

Set-StrictMode -Version Latest

$script:ReplicationBindingSchemaVersion = 1

# Every top-level field, in the order they are written. This doubles as the
# closed set: a document with more, fewer, or differently named fields is
# rejected rather than partially understood.
$script:ReplicationBindingFields = @(
    'schemaVersion',
    'issueNumber',
    'platform',
    'trustedSourceVersion',
    'trustedTreeHash',
    'pipelineSha256',
    'replicationBaseSha',
    'executionHeadSha',
    'testPatchSha256',
    'fixPatchSha256',
    'selector',
    'trustedScripts',
    'evidence',
    'digest'
)

$script:ReplicationBindingSelectorFields = @(
    'variant',
    'testType',
    'testProject',
    'testProjectPath',
    'testClassName',
    'testMethodName',
    'platform',
    'discoveredCount',
    'executedCount'
)

# Evidence files whose content the binding covers. A name absent from the run is
# recorded as an explicit null rather than omitted, so "this run had no
# thumbnail" and "somebody removed the thumbnail field" stay distinguishable.
$script:ReplicationBindingEvidenceNames = @(
    'candidate.json',
    'test.patch',
    'fix.patch',
    'reproduction-result.json',
    'evidence/evidence.json',
    'evidence/repro.mp4',
    'evidence/preview.gif',
    'evidence/thumbnail.png',
    'fix-scope-baseline.json',
    'verification/verification-report.md',
    'verification/verification-log.txt',
    'verification/verification-output.log',
    'verification/verification-console.log',
    'verification/verification-console-run-2.log',
    'verification/verification-console-run-3.log',
    'verification/verification-result.json',
    'verification/verification-test-result.trx',
    'verification/verification-test-result.xml',
    'verification/verify-tests-fail.log',
    'verification/test-without-fix.log',
    'verification/negative-control-baseline.cs',
    'verification/negative-control-oracle.cs',
    'verification/negative-control-variant.cs',
    'verification/negative-control-console.log',
    'verification/negative-control-console-run-2.log',
    'verification/negative-control-console-run-3.log',
    'verification/negative-control-result.json',
    'verification/fix-control-result.json',
    'verification/fix-control-console.log',
    'verification/restoration-result.json',
    'verification/restoration-console.log'
)

function Get-ReplicationBindingFields {
    return @($script:ReplicationBindingFields)
}

function Get-ReplicationBindingEvidenceNames {
    return @($script:ReplicationBindingEvidenceNames)
}

function Get-ReplicationBindingFileDigest {
    <#
        .SYNOPSIS
        Returns the SHA-256 of a regular file, or $null when it is absent.

        .DESCRIPTION
        Absence is a legitimate answer for optional evidence, but a link or a
        directory in place of a file is not: that is how the same recorded hash
        can be made to describe different bytes later.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) { return $null }
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    $item = Get-Item -LiteralPath $Path -Force
    if ($item.PSIsContainer) {
        throw "Binding input must be a regular file, not a directory: $Path"
    }
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Binding input must be a regular file, not a link: $Path"
    }

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($item.FullName)
        try {
            return [System.BitConverter]::ToString($sha256.ComputeHash($stream)).Replace('-', '').ToLowerInvariant()
        } finally {
            $stream.Dispose()
        }
    } finally {
        $sha256.Dispose()
    }
}

function ConvertTo-ReplicationBindingCanonicalText {
    <#
        .SYNOPSIS
        Renders a binding as the exact text its digest is taken over.

        .DESCRIPTION
        Not JSON. Two PowerShell versions serialize the same object with
        different spacing and different number formatting, and a digest that
        depends on the serializer is a digest that changes when nothing did.
        This is a flat, ordinally sorted `key=value` list with a fixed
        separator, so any implementation can reproduce it byte for byte.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][object]$Binding
    )

    $pairs = [System.Collections.Generic.List[string]]::new()

    function Add-Pair {
        param([string]$Key, $Value)

        $text = if ($null -eq $Value) { '' } else { [string]$Value }
        $pairs.Add("$Key=$text")
    }

    foreach ($field in $script:ReplicationBindingFields) {
        if ($field -eq 'digest') { continue }
        $value = Get-ReplicationBindingValue -Source $Binding -Name $field
        switch ($field) {
            'selector' {
                foreach ($selectorField in $script:ReplicationBindingSelectorFields) {
                    Add-Pair -Key "selector.$selectorField" -Value (
                        Get-ReplicationBindingValue -Source $value -Name $selectorField)
                }
            }
            'trustedScripts' {
                $names = @(Get-ReplicationBindingKeys -Source $value | Sort-Object -CaseSensitive)
                foreach ($name in $names) {
                    Add-Pair -Key "trustedScripts.$name" -Value (
                        Get-ReplicationBindingValue -Source $value -Name $name)
                }
            }
            'evidence' {
                foreach ($name in $script:ReplicationBindingEvidenceNames) {
                    Add-Pair -Key "evidence.$name" -Value (
                        Get-ReplicationBindingValue -Source $value -Name $name)
                }
            }
            default { Add-Pair -Key $field -Value $value }
        }
    }

    return (($pairs) -join "`n") + "`n"
}

function Get-ReplicationBindingValue {
    [CmdletBinding()]
    param(
        [AllowNull()][object]$Source,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $Source) { return $null }
    if ($Source -is [System.Collections.IDictionary]) {
        if ($Source.Contains($Name)) { return $Source[$Name] }
        return $null
    }
    $property = $Source.PSObject.Properties[$Name]
    if ($property) { return $property.Value }

    return $null
}

function Get-ReplicationBindingKeys {
    [CmdletBinding()]
    param(
        [AllowNull()][object]$Source
    )

    if ($null -eq $Source) { return @() }
    if ($Source -is [System.Collections.IDictionary]) {
        return @($Source.Keys | ForEach-Object { [string]$_ })
    }

    return @($Source.PSObject.Properties.Name)
}

function Get-ReplicationBindingDigest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][object]$Binding
    )

    $text = ConvertTo-ReplicationBindingCanonicalText -Binding $Binding
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
        return [System.BitConverter]::ToString($sha256.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant()
    } finally {
        $sha256.Dispose()
    }
}

function Get-ReplicationBindingSelector {
    <#
        .SYNOPSIS
        Projects a typed selector contract into the binding's selector shape.

        .DESCRIPTION
        The writer and the two verifiers must agree on this projection exactly,
        so it lives here once. The raw filter text is deliberately not part of
        it: the identity that matters is which typed variant selected which
        class and method on which platform, and how many tests the trusted
        runner discovered and executed as a result.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()][object]$Selector,
        [AllowEmptyString()][string]$TestType = ''
    )

    return [ordered]@{
        variant = [string](Get-ReplicationBindingValue -Source $Selector -Name 'variant')
        testType = [string]$TestType
        testProject = [string](Get-ReplicationBindingValue -Source $Selector -Name 'project')
        testProjectPath = [string](Get-ReplicationBindingValue -Source $Selector -Name 'projectPath')
        testClassName = [string](Get-ReplicationBindingValue -Source $Selector -Name 'class')
        testMethodName = [string](Get-ReplicationBindingValue -Source $Selector -Name 'method')
        platform = [string](Get-ReplicationBindingValue -Source $Selector -Name 'platform')
        discoveredCount = [string](Get-ReplicationBindingValue -Source $Selector -Name 'discoveredCount')
        executedCount = [string](Get-ReplicationBindingValue -Source $Selector -Name 'executedCount')
    }
}

function New-ReplicationCertificationBinding {
    <#
        .SYNOPSIS
        Computes the binding from the artifacts and identities in hand.

        .DESCRIPTION
        Every hash is read from disk here rather than accepted from a caller, so
        the only inputs that are taken on trust are the ones that are not files:
        the commit identities the trusted pipeline resolved and the selector
        identity plus counts the trusted runner reported.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][long]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Platform,
        [Parameter(Mandatory = $true)][string]$ArtifactRoot,
        [Parameter(Mandatory = $true)][string]$TrustedSourceVersion,
        [Parameter(Mandatory = $true)][string]$TrustedTreeHash,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$PipelineSha256,
        [Parameter(Mandatory = $true)][string]$ReplicationBaseSha,
        [Parameter(Mandatory = $true)][string]$ExecutionHeadSha,
        [Parameter(Mandatory = $true)][System.Collections.IDictionary]$TrustedScripts,
        [object]$Selector,
        [string]$OutputPath = ''
    )

    foreach ($pair in @(
        @{ Name = 'TrustedSourceVersion'; Value = $TrustedSourceVersion },
        @{ Name = 'ReplicationBaseSha'; Value = $ReplicationBaseSha },
        @{ Name = 'ExecutionHeadSha'; Value = $ExecutionHeadSha }
    )) {
        if ([string]$pair.Value -cnotmatch '^[0-9a-f]{40}$') {
            throw "Certification binding requires a lowercase 40-character commit for $($pair.Name)."
        }
    }
    if ($TrustedTreeHash -cnotmatch '^[0-9a-f]{64}$') {
        throw 'Certification binding requires a SHA-256 trusted tree hash.'
    }
    if ($IssueNumber -le 0) {
        throw 'Certification binding requires a positive issue number.'
    }

    $root = [System.IO.Path]::GetFullPath($ArtifactRoot)
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        throw "Certification binding artifact root does not exist: $ArtifactRoot"
    }

    $evidence = [ordered]@{}
    foreach ($name in $script:ReplicationBindingEvidenceNames) {
        $evidence[$name] = Get-ReplicationBindingFileDigest -Path (Join-Path $root $name)
    }

    $scripts = [ordered]@{}
    foreach ($name in @($TrustedScripts.Keys | Sort-Object -CaseSensitive)) {
        $value = [string]$TrustedScripts[$name]
        if ($value -cnotmatch '^[0-9a-f]{64}$') {
            throw "Certification binding requires a SHA-256 hash for trusted script $name."
        }
        $scripts[[string]$name] = $value
    }
    if ($scripts.Count -eq 0) {
        throw 'Certification binding requires at least one trusted script identity.'
    }

    $selectorDocument = [ordered]@{}
    foreach ($field in $script:ReplicationBindingSelectorFields) {
        $value = Get-ReplicationBindingValue -Source $Selector -Name $field
        $selectorDocument[$field] = if ($null -eq $value) { '' } else { [string]$value }
    }

    $binding = [ordered]@{
        schemaVersion = $script:ReplicationBindingSchemaVersion
        issueNumber = [long]$IssueNumber
        platform = [string]$Platform
        trustedSourceVersion = [string]$TrustedSourceVersion
        trustedTreeHash = [string]$TrustedTreeHash
        pipelineSha256 = [string]$PipelineSha256
        replicationBaseSha = [string]$ReplicationBaseSha
        executionHeadSha = [string]$ExecutionHeadSha
        testPatchSha256 = $evidence['test.patch']
        fixPatchSha256 = $evidence['fix.patch']
        selector = $selectorDocument
        trustedScripts = $scripts
        evidence = $evidence
        digest = ''
    }
    $binding['digest'] = Get-ReplicationBindingDigest -Binding $binding

    if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
        $fullOutput = [System.IO.Path]::GetFullPath($OutputPath)
        $parent = [System.IO.Path]::GetDirectoryName($fullOutput)
        if ([string]::IsNullOrEmpty($parent) -or -not (Test-Path -LiteralPath $parent -PathType Container)) {
            throw 'Certification binding output parent directory must already exist.'
        }
        $binding | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $fullOutput -Encoding utf8NoBOM
    }

    return $binding
}

function Read-ReplicationCertificationBinding {
    <#
        .SYNOPSIS
        Reads a binding document and rejects anything malformed or extended.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Certification binding is missing: $Path"
    }
    $item = Get-Item -LiteralPath $Path -Force
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Certification binding must be a regular file: $Path"
    }
    if ($item.Length -gt 256KB) {
        throw 'Certification binding exceeds its size bound.'
    }

    $document = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -Depth 12
    if ($null -eq $document) {
        throw 'Certification binding is empty.'
    }

    $actual = @($document.PSObject.Properties.Name | Sort-Object -CaseSensitive)
    $expected = @($script:ReplicationBindingFields | Sort-Object -CaseSensitive)
    if (($actual -join ',') -cne ($expected -join ',')) {
        throw 'Certification binding has unexpected or missing fields.'
    }
    if ([int]$document.schemaVersion -ne $script:ReplicationBindingSchemaVersion) {
        throw 'Unsupported certification binding schema version.'
    }

    $selectorActual = @($document.selector.PSObject.Properties.Name | Sort-Object -CaseSensitive)
    $selectorExpected = @($script:ReplicationBindingSelectorFields | Sort-Object -CaseSensitive)
    if (($selectorActual -join ',') -cne ($selectorExpected -join ',')) {
        throw 'Certification binding selector has unexpected or missing fields.'
    }

    $evidenceActual = @($document.evidence.PSObject.Properties.Name | Sort-Object -CaseSensitive)
    $evidenceExpected = @($script:ReplicationBindingEvidenceNames | Sort-Object -CaseSensitive)
    if (($evidenceActual -join ',') -cne ($evidenceExpected -join ',')) {
        throw 'Certification binding evidence has unexpected or missing entries.'
    }

    foreach ($field in @('trustedSourceVersion', 'replicationBaseSha', 'executionHeadSha')) {
        if ([string]$document.$field -cnotmatch '^[0-9a-f]{40}$') {
            throw "Certification binding records an invalid commit for $field."
        }
    }
    foreach ($field in @('trustedTreeHash', 'digest')) {
        if ([string]$document.$field -cnotmatch '^[0-9a-f]{64}$') {
            throw "Certification binding records an invalid hash for $field."
        }
    }

    $recomputed = Get-ReplicationBindingDigest -Binding $document
    if ($recomputed -cne [string]$document.digest) {
        throw 'Certification binding digest does not cover its own contents.'
    }

    return $document
}

function Assert-ReplicationCertificationBinding {
    <#
        .SYNOPSIS
        Recomputes the binding from artifacts in hand and compares every field.

        .DESCRIPTION
        This is the check that makes the binding worth writing. It is run by the
        credentialless validation job over the downloaded artifacts and again by
        the publisher before it extracts a credential, so an artifact mutated in
        between the two is caught by the second.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][object]$Binding,
        [Parameter(Mandatory = $true)][string]$ArtifactRoot,
        [Parameter(Mandatory = $true)][string]$TrustedSourceVersion,
        [string]$TrustedTreeHash = '',
        [string]$PipelineSha256 = '',
        [string]$ReplicationBaseSha = '',
        [System.Collections.IDictionary]$TrustedScripts,
        [object]$Selector,
        [long]$IssueNumber = 0,
        [string]$Platform = '',
        [string]$Context = 'certification binding'
    )

    if ($Binding -is [string]) {
        $Binding = Read-ReplicationCertificationBinding -Path $Binding
    } else {
        $recomputed = Get-ReplicationBindingDigest -Binding $Binding
        if ($recomputed -cne [string](Get-ReplicationBindingValue -Source $Binding -Name 'digest')) {
            throw "Certification binding digest does not cover its own contents ($Context)."
        }
    }

    $mismatches = [System.Collections.Generic.List[string]]::new()

    function Compare-Field {
        param([string]$Name, $Expected, $Actual)

        if ([string]$Expected -cne [string]$Actual) {
            $mismatches.Add($Name)
        }
    }

    Compare-Field -Name 'trustedSourceVersion' `
        -Expected $TrustedSourceVersion.ToLowerInvariant() `
        -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'trustedSourceVersion')
    if (-not [string]::IsNullOrWhiteSpace($TrustedTreeHash)) {
        Compare-Field -Name 'trustedTreeHash' -Expected $TrustedTreeHash `
            -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'trustedTreeHash')
    }
    if (-not [string]::IsNullOrWhiteSpace($PipelineSha256)) {
        Compare-Field -Name 'pipelineSha256' -Expected $PipelineSha256 `
            -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'pipelineSha256')
    }
    if (-not [string]::IsNullOrWhiteSpace($ReplicationBaseSha)) {
        Compare-Field -Name 'replicationBaseSha' -Expected $ReplicationBaseSha.ToLowerInvariant() `
            -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'replicationBaseSha')
    }
    if ($IssueNumber -gt 0) {
        Compare-Field -Name 'issueNumber' -Expected $IssueNumber `
            -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'issueNumber')
    }
    if (-not [string]::IsNullOrWhiteSpace($Platform)) {
        Compare-Field -Name 'platform' -Expected $Platform `
            -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'platform')
    }

    if ($null -ne $TrustedScripts) {
        $bound = Get-ReplicationBindingValue -Source $Binding -Name 'trustedScripts'
        $boundNames = @(Get-ReplicationBindingKeys -Source $bound | Sort-Object -CaseSensitive)
        $expectedNames = @($TrustedScripts.Keys | ForEach-Object { [string]$_ } | Sort-Object -CaseSensitive)
        if (($boundNames -join ',') -cne ($expectedNames -join ',')) {
            $mismatches.Add('trustedScripts (set)')
        } else {
            foreach ($name in $expectedNames) {
                Compare-Field -Name "trustedScripts.$name" `
                    -Expected ([string]$TrustedScripts[$name]) `
                    -Actual (Get-ReplicationBindingValue -Source $bound -Name $name)
            }
        }
    }

    if ($null -ne $Selector) {
        $bound = Get-ReplicationBindingValue -Source $Binding -Name 'selector'
        foreach ($field in $script:ReplicationBindingSelectorFields) {
            $expected = Get-ReplicationBindingValue -Source $Selector -Name $field
            if ($null -eq $expected) { $expected = '' }
            Compare-Field -Name "selector.$field" -Expected $expected `
                -Actual (Get-ReplicationBindingValue -Source $bound -Name $field)
        }
    }

    $root = [System.IO.Path]::GetFullPath($ArtifactRoot)
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        throw "Certification binding artifact root does not exist ($Context): $ArtifactRoot"
    }
    $boundEvidence = Get-ReplicationBindingValue -Source $Binding -Name 'evidence'
    foreach ($name in $script:ReplicationBindingEvidenceNames) {
        $actual = Get-ReplicationBindingFileDigest -Path (Join-Path $root $name)
        $expected = Get-ReplicationBindingValue -Source $boundEvidence -Name $name
        if ([string]$expected -cne [string]$actual) {
            $mismatches.Add("evidence.$name")
        }
    }

    Compare-Field -Name 'testPatchSha256' `
        -Expected (Get-ReplicationBindingValue -Source $boundEvidence -Name 'test.patch') `
        -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'testPatchSha256')
    Compare-Field -Name 'fixPatchSha256' `
        -Expected (Get-ReplicationBindingValue -Source $boundEvidence -Name 'fix.patch') `
        -Actual (Get-ReplicationBindingValue -Source $Binding -Name 'fixPatchSha256')

    if ($mismatches.Count -gt 0) {
        $detail = (@($mismatches | Sort-Object -CaseSensitive -Unique | Select-Object -First 12) -join ', ')
        throw "Certification binding does not match its inputs ($Context): $detail"
    }

    return [pscustomobject]@{
        Digest = [string](Get-ReplicationBindingValue -Source $Binding -Name 'digest')
        TrustedTreeHash = [string](Get-ReplicationBindingValue -Source $Binding -Name 'trustedTreeHash')
        Context = $Context
    }
}
