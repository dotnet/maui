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
    'verification/restoration-console.log',
    'regression/baseline/strict-test-evidence.json',
    'regression/fix/strict-test-evidence.json',
    'regression/regression-evidence.json'
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

function Get-ReplicationRegressionLaneSelection {
    <#
        .SYNOPSIS
            Derives a bounded sibling lane exclusively from an immutable Git tree.
    #>
    param(
        [Parameter(Mandatory = $true)][string]$TestPath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$BaselineSha,
        [Parameter(Mandatory = $true)][string]$Platform
    )

    if ($BaselineSha -cnotmatch '^[0-9a-f]{40}$' -or
        $TestPath -cnotmatch '^src/Controls/tests/DeviceTests/[A-Za-z0-9._/-]+\.cs$' -or
        $TestPath.Contains('..') -or $TestPath.Contains('\')) {
        return $null
    }
    $directory = [IO.Path]::GetDirectoryName($TestPath).Replace('\', '/')
    $component = [IO.Path]::GetFileName($directory)
    $paths = @(& git -C $RepositoryRoot ls-tree -r --name-only $BaselineSha -- $directory)
    if ($LASTEXITCODE -ne 0 -or $paths.Count -eq 0 -or $paths.Count -gt 64) {
        return $null
    }

    $pairs = [Collections.Generic.List[object]]::new()
    foreach ($pathValue in $paths) {
        $path = ([string]$pathValue).Trim()
        if ($path -ceq $TestPath -or
            $path -cnotmatch '^[A-Za-z0-9._/-]+\.cs$' -or
            ([IO.Path]::GetDirectoryName($path).Replace('\', '/')) -cne $directory) {
            continue
        }
        $sizeText = (& git -C $RepositoryRoot cat-file -s "${BaselineSha}:$path" 2>$null |
            Select-Object -First 1)
        $size = 0L
        if ($LASTEXITCODE -ne 0 -or
            -not [long]::TryParse([string]$sizeText, [ref]$size) -or
            $size -le 0 -or $size -gt 256KB) {
            return $null
        }
        $content = (@(& git -C $RepositoryRoot show "${BaselineSha}:$path" 2>$null) -join "`n")
        if ($LASTEXITCODE -ne 0) { return $null }
        $namespaceMatch = [regex]::Match(
            $content, '(?m)^\s*namespace\s+(?<namespace>[A-Za-z_][A-Za-z0-9_.]*)\s*(?:;|\{)')
        if (-not $namespaceMatch.Success) { continue }
        foreach ($match in [regex]::Matches(
                $content,
                '(?s)\[Category\(TestCategory\.(?<category>[A-Za-z][A-Za-z0-9_]*)\)\]' +
                '.{0,1024}?\b(?:public\s+)?(?:sealed\s+|abstract\s+)?(?:partial\s+)?class\s+' +
                '(?<class>[A-Za-z_][A-Za-z0-9_]*)')) {
            $pairs.Add([pscustomobject]@{
                Category = $match.Groups['category'].Value
                Class = "$($namespaceMatch.Groups['namespace'].Value).$($match.Groups['class'].Value)"
                SourcePath = $path
            })
        }
    }

    $distinct = @($pairs | Sort-Object Category, Class -Unique)
    if ($distinct.Count -ne 1 -or $distinct[0].Category -cne $component) {
        return $null
    }
    return [pscustomobject]@{
        SchemaVersion = 1
        BaselineSha = $BaselineSha
        Platform = $Platform
        Project = 'Controls'
        ProjectPath = 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
        Category = [string]$distinct[0].Category
        TestClass = [string]$distinct[0].Class
        MetadataSourcePath = [string]$distinct[0].SourcePath
        GeneratedTestPath = $TestPath
    }
}

function Get-ReplicationDeviceTestFailureSignature {
    param(
        [Parameter(Mandatory = $true)][System.Xml.XmlElement]$Test
    )

    $failure = $Test.SelectSingleNode('./failure')
    $message = if ($failure) {
        $messageNode = $failure.SelectSingleNode('./message')
        if ($messageNode) { [string]$messageNode.InnerText } else { '' }
    } else {
        [string]$Test.GetAttribute('message')
    }
    $exceptionType = if ($failure) {
        [string]$failure.GetAttribute('exception-type')
    } else { '' }
    $stackNode = if ($failure) { $failure.SelectSingleNode('./stack-trace') } else { $null }
    $stackFrames = [Collections.Generic.List[string]]::new()
    if ($stackNode) {
        foreach ($line in ([regex]::Replace(
                    [string]$stackNode.InnerText, '\r\n?', "`n") -split "`n")) {
            $frame = $line.Trim()
            if ([string]::IsNullOrWhiteSpace($frame)) { continue }
            $frame = [regex]::Replace(
                $frame,
                '\s+in\s+.+?:line\s+\d+\s*$',
                '')
            $frame = [regex]::Replace($frame, '\s+\[0x[0-9a-fA-F]+\]\s*$', '')
            $frame = [regex]::Replace($frame, '\s+', ' ').Trim()
            if ($frame -match '^(?:at\s+)?[A-Za-z_][A-Za-z0-9_.+`<>]*(?:\.[A-Za-z_][A-Za-z0-9_.+`<>]*)*\s*\(') {
                $stackFrames.Add($frame)
            }
        }
    }
    if ($stackFrames.Count -eq 0) {
        throw 'Strict device-test evidence requires every failed test to carry a stable stack frame.'
    }

    $canonical = (
        $exceptionType.Trim() + "`n" +
        ([regex]::Replace($message, '\r\n?', "`n")).Trim() + "`n" +
        ($stackFrames -join "`n")).Trim()
    if ([string]::IsNullOrWhiteSpace($exceptionType) -or
        [string]::IsNullOrWhiteSpace($message)) {
        throw 'Strict device-test evidence requires every failed test to carry an exception type and message.'
    }

    $bytes = [Text.Encoding]::UTF8.GetBytes($canonical)
    return ([BitConverter]::ToString(
        [Security.Cryptography.SHA256]::HashData($bytes)) -replace '-', '').ToLowerInvariant()
}

function Read-ReplicationDeviceTestResultXmlStrict {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][datetime]$NotBeforeUtc,
        [Parameter(Mandatory = $true)][string]$ExpectedClass
    )

    if ($ExpectedClass -cnotmatch '^[A-Za-z_][A-Za-z0-9_.]{0,499}$') {
        throw 'Strict device-test XML requires one valid expected class.'
    }
    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.Length -le 0 -or
        $item.Length -gt 10MB -or
        $item.LastWriteTimeUtc -lt $NotBeforeUtc) {
        throw "Strict device-test evidence requires a fresh bounded regular XML file: $Path"
    }

    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $settings.MaxCharactersInDocument = 10MB
    $reader = $null
    try {
        $reader = [Xml.XmlReader]::Create($item.FullName, $settings)
        $xml = [Xml.XmlDocument]::new()
        $xml.XmlResolver = $null
        $xml.Load($reader)
    } finally {
        if ($reader) { $reader.Dispose() }
    }
    if ($xml.DocumentElement.LocalName -cne 'assemblies') {
        throw "Strict device-test evidence requires an xUnit assemblies document: $Path"
    }

    $assemblies = @($xml.SelectNodes('/assemblies/assembly'))
    if ($assemblies.Count -lt 1 -or $assemblies.Count -gt 8) {
        throw 'Strict device-test evidence requires between one and eight completed assemblies.'
    }
    $records = [Collections.Generic.List[object]]::new()
    $totals = [ordered]@{ Total = 0; Passed = 0; Failed = 0; Skipped = 0; Errors = 0 }
    foreach ($assembly in $assemblies) {
        $counts = @{}
        foreach ($name in @('total', 'passed', 'failed', 'skipped', 'errors')) {
            $value = 0
            if (-not [int]::TryParse([string]$assembly.GetAttribute($name), [ref]$value) -or
                $value -lt 0) {
                throw "Strict device-test evidence has an invalid '$name' assembly count."
            }
            $counts[$name] = $value
        }
        $rows = @($assembly.SelectNodes('.//test'))
        if ($counts.errors -ne 0 -or
            $counts.total -ne $rows.Count -or
            ($counts.passed + $counts.failed + $counts.skipped) -ne $counts.total) {
            throw 'Strict device-test evidence found incomplete or inconsistent assembly totals.'
        }
        $totals.Total += $counts.total
        $totals.Passed += $counts.passed
        $totals.Failed += $counts.failed
        $totals.Skipped += $counts.skipped
        $totals.Errors += $counts.errors

        foreach ($test in $rows) {
            $type = [string]$test.GetAttribute('type')
            $method = [string]$test.GetAttribute('method')
            $displayName = [string]$test.GetAttribute('name')
            $outcome = [string]$test.GetAttribute('result')
            if ($type -cne $ExpectedClass -or
                $method -cnotmatch '^[A-Za-z_][A-Za-z0-9_]{0,255}$' -or
                [string]::IsNullOrWhiteSpace($displayName) -or $displayName.Length -gt 1000 -or
                $outcome -cnotin @('Pass', 'Fail', 'Skip')) {
                throw 'Strict device-test evidence found a test with a missing identity, unexpected class, or unknown outcome.'
            }
            $records.Add([ordered]@{
                type = $type
                method = $method
                displayName = $displayName
                outcome = $outcome
                failureSignature = if ($outcome -ceq 'Fail') {
                    Get-ReplicationDeviceTestFailureSignature -Test $test
                } else { '' }
            })
        }
    }
    if ($records.Count -lt 1 -or $records.Count -gt 256 -or
        ($totals.Passed + $totals.Failed) -eq 0) {
        throw 'Strict device-test evidence requires between 1 and 256 records and at least one executed test.'
    }

    return [pscustomobject]@{
        Name = $item.Name
        Sha256 = Get-ReplicationBindingFileDigest -Path $item.FullName
        Total = $totals.Total
        Passed = $totals.Passed
        Failed = $totals.Failed
        Skipped = $totals.Skipped
        Errors = $totals.Errors
        Records = @($records)
    }
}

function Get-ReplicationRegressionJson {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string[]]$Fields,
            [Parameter(Mandatory = $true)][string]$Context,
            [ValidateRange(1, 1048576)][int]$MaximumBytes = 262144
        )

        $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
        if ($item.PSIsContainer -or
            $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
            $item.Length -le 0 -or $item.Length -gt $MaximumBytes) {
            throw "$Context must be a non-empty bounded regular file."
        }
        $raw = [IO.File]::ReadAllText($item.FullName)
        $json = $null
        try {
            $json = [Text.Json.JsonDocument]::Parse($raw)
            function Assert-NoDuplicateProperties {
                param([Text.Json.JsonElement]$Element)
                if ($Element.ValueKind -eq [Text.Json.JsonValueKind]::Object) {
                    $names = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
                    foreach ($property in $Element.EnumerateObject()) {
                        if (-not $names.Add($property.Name)) {
                            throw "$Context contains a duplicate JSON property."
                        }
                        Assert-NoDuplicateProperties -Element $property.Value
                    }
                } elseif ($Element.ValueKind -eq [Text.Json.JsonValueKind]::Array) {
                    foreach ($child in $Element.EnumerateArray()) {
                        Assert-NoDuplicateProperties -Element $child
                    }
                }
            }
            Assert-NoDuplicateProperties -Element $json.RootElement
        } finally {
            if ($json) { $json.Dispose() }
        }

        $document = $raw | ConvertFrom-Json -Depth 12 -ErrorAction Stop
        $actual = @($document.PSObject.Properties.Name | Sort-Object -CaseSensitive)
        $expected = @($Fields | Sort-Object -CaseSensitive)
        if (($actual -join "`n") -cne ($expected -join "`n")) {
            throw "$Context has unexpected or missing fields."
        }
        return $document
    }

function Read-ReplicationRegressionRunEvidence {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$ExpectedPlatform,
            [Parameter(Mandatory = $true)][string]$ExpectedProject,
            [Parameter(Mandatory = $true)][string]$ExpectedCategory,
            [Parameter(Mandatory = $true)][string]$ExpectedClass
        )

        $document = Get-ReplicationRegressionJson `
            -Path $Path `
            -Fields @(
                'schemaVersion', 'completed', 'runStartedUtc', 'completedUtc',
                'project', 'platform', 'testFilter', 'includeClass', 'records',
                'resultFiles', 'total', 'passed', 'failed', 'skipped', 'errors') `
            -Context 'Strict regression run evidence'
        if ([int]$document.schemaVersion -ne 1 -or $document.completed -isnot [bool] -or
            -not [bool]$document.completed) {
            throw 'Strict regression run evidence does not record a completed schema-v1 run.'
        }
        if ([string]$document.platform -cne $ExpectedPlatform -or
            [string]$document.project -cne $ExpectedProject -or
            [string]$document.testFilter -cne "Category=$ExpectedCategory" -or
            [string]$document.includeClass -cne $ExpectedClass) {
            throw 'Strict regression run evidence does not match its trusted selector.'
        }
        $started = [datetime]::MinValue
        $completed = [datetime]::MinValue
        if (-not [datetime]::TryParse(
                [string]$document.runStartedUtc,
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::RoundtripKind,
                [ref]$started) -or
            -not [datetime]::TryParse(
                [string]$document.completedUtc,
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::RoundtripKind,
                [ref]$completed) -or
            $completed -lt $started) {
            throw 'Strict regression run evidence has invalid completion timestamps.'
        }

        $sourceFiles = @($document.resultFiles)
        if ($sourceFiles.Count -lt 1 -or $sourceFiles.Count -gt 8) {
            throw 'Strict regression run evidence must bind between one and eight result files.'
        }
        $sourceNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $xmlRecordMap = [Collections.Specialized.OrderedDictionary]::new([StringComparer]::Ordinal)
        $xmlTotals = [ordered]@{ Total = 0; Passed = 0; Failed = 0; Skipped = 0; Errors = 0 }
        foreach ($source in $sourceFiles) {
            $fields = @($source.PSObject.Properties.Name | Sort-Object -CaseSensitive)
            if (($fields -join "`n") -cne "name`nsha256" -or
                [string]$source.name -cnotmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,127}\.xml$' -or
                [string]$source.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
                -not $sourceNames.Add([string]$source.name)) {
                throw 'Strict regression run evidence contains invalid or duplicate result-file digests.'
            }
            $sourcePath = Join-Path (Split-Path -Parent $Path) ([string]$source.name)
            $actualDigest = Get-ReplicationBindingFileDigest -Path $sourcePath
            if ([string]$actualDigest -cne [string]$source.sha256) {
                throw 'Strict regression run evidence source result digest does not match its XML file.'
            }
            $parsedXml = Read-ReplicationDeviceTestResultXmlStrict `
                -Path $sourcePath `
                -NotBeforeUtc $started `
                -ExpectedClass $ExpectedClass
            $xmlTotals.Total += $parsedXml.Total
            $xmlTotals.Passed += $parsedXml.Passed
            $xmlTotals.Failed += $parsedXml.Failed
            $xmlTotals.Skipped += $parsedXml.Skipped
            $xmlTotals.Errors += $parsedXml.Errors
            foreach ($record in $parsedXml.Records) {
                $identity = "$($record.type)`n$($record.method)`n$($record.displayName)"
                if (-not $xmlRecordMap.Contains($identity)) {
                    $xmlRecordMap[$identity] =
                        [Collections.Generic.List[object]]::new()
                }
                $xmlRecordMap[$identity].Add([pscustomobject]@{
                    Outcome = [string]$record.outcome
                    FailureSignature = [string]$record.failureSignature
                })
            }
        }

        $records = @($document.records)
        if ($records.Count -lt 1 -or $records.Count -gt 256) {
            throw 'Strict regression run evidence must contain between one and 256 test records.'
        }
        $recordMap = [Collections.Specialized.OrderedDictionary]::new([StringComparer]::Ordinal)
        foreach ($record in $records) {
            $fields = @($record.PSObject.Properties.Name | Sort-Object -CaseSensitive)
            if (($fields -join "`n") -cne "displayName`nfailureSignature`nmethod`noutcome`ntype") {
                throw 'Strict regression run evidence contains a malformed test record.'
            }
            $type = [string]$record.type
            $method = [string]$record.method
            $displayName = [string]$record.displayName
            $outcome = [string]$record.outcome
            $signature = [string]$record.failureSignature
            if ($type -cne $ExpectedClass -or
                $method -cnotmatch '^[A-Za-z_][A-Za-z0-9_]{0,255}$' -or
                [string]::IsNullOrWhiteSpace($displayName) -or $displayName.Length -gt 1000 -or
                $outcome -cnotin @('Pass', 'Fail', 'Skip') -or
                ($outcome -ceq 'Fail' -and $signature -cnotmatch '^[0-9a-f]{64}$') -or
                ($outcome -cne 'Fail' -and $signature -cne '')) {
                throw 'Strict regression run evidence contains an invalid test identity or outcome.'
            }
            $identity = "$type`n$method`n$displayName"
            if (-not $recordMap.Contains($identity)) {
                $recordMap[$identity] =
                    [Collections.Generic.List[object]]::new()
            }
            $recordMap[$identity].Add([pscustomobject]@{
                Outcome = $outcome
                FailureSignature = $signature
            })
        }
        foreach ($name in @('total', 'passed', 'failed', 'skipped', 'errors')) {
            $value = 0
            if (-not [int]::TryParse([string]$document.$name, [ref]$value) -or $value -lt 0 -or
                $value -ne [int]$xmlTotals[
                    $name.Substring(0, 1).ToUpperInvariant() + $name.Substring(1)]) {
                throw 'Strict regression run evidence totals do not match retained XML.'
            }
        }
        if ([int]$document.total -ne $records.Count -or
            ([int]$document.passed + [int]$document.failed + [int]$document.skipped) -ne
                [int]$document.total -or [int]$document.errors -ne 0) {
            throw 'Strict regression run evidence contains inconsistent totals.'
        }
        if ($recordMap.Count -ne $xmlRecordMap.Count) {
            throw 'Strict regression run evidence records do not match retained XML.'
        }
        foreach ($identity in $recordMap.Keys) {
            if (-not $xmlRecordMap.Contains($identity)) {
                throw 'Strict regression run evidence records do not match retained XML.'
            }
            $jsonStates = @($recordMap[$identity] | ForEach-Object {
                "$($_.Outcome)`n$($_.FailureSignature)"
            } | Sort-Object -CaseSensitive)
            $xmlStates = @($xmlRecordMap[$identity] | ForEach-Object {
                "$($_.Outcome)`n$($_.FailureSignature)"
            } | Sort-Object -CaseSensitive)
            if (($jsonStates -join "`n--state--`n") -cne
                ($xmlStates -join "`n--state--`n")) {
                throw 'Strict regression run evidence records do not match retained XML.'
            }
        }
        $executedRecordCount = 0
        foreach ($group in $recordMap.Values) {
            foreach ($record in $group) {
                if ($record.Outcome -ne 'Skip') {
                    $executedRecordCount++
                }
            }
        }
        if ($executedRecordCount -eq 0) {
            throw 'Strict regression run evidence contains only skipped tests.'
        }
        return [pscustomobject]@{
            Document = $document
            Records = $recordMap
            Digest = Get-ReplicationBindingFileDigest -Path $Path
        }
    }

function Assert-ReplicationRegressionEvidence {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory = $true)][string]$ArtifactRoot,
            [Parameter(Mandatory = $true)][string]$ExpectedBaselineSha,
            [Parameter(Mandatory = $true)][string]$ExpectedPlatform,
            [string]$ExpectedCategory = '',
            [string]$ExpectedProject = '',
            [string]$ExpectedProjectPath = '',
            [string]$ExpectedClass = '',
            [string]$ExpectedGeneratedTestPath = ''
        )

        $evidencePath = Join-Path $ArtifactRoot 'regression/regression-evidence.json'
        $evidence = Get-ReplicationRegressionJson `
            -Path $evidencePath `
            -Fields @(
                'schemaVersion', 'baselineSha', 'productPatchSha256', 'platform',
                'project', 'projectPath', 'category', 'testClass', 'generatedTestPath',
                'baselineResult', 'fixResult', 'baselineResultSha256',
                'fixResultSha256', 'comparison') `
            -Context 'Regression evidence'
        if ([int]$evidence.schemaVersion -ne 1 -or
            [string]$evidence.baselineSha -cne $ExpectedBaselineSha.ToLowerInvariant() -or
            [string]$evidence.platform -cne $ExpectedPlatform -or
            [string]$evidence.productPatchSha256 -cnotmatch '^[0-9a-f]{64}$' -or
            [string]$evidence.comparison -cne 'pass') {
            throw 'Regression evidence does not match its immutable run inputs.'
        }
        foreach ($expectation in @(
            @{ Expected = $ExpectedCategory; Actual = [string]$evidence.category; Name = 'category' },
            @{ Expected = $ExpectedProject; Actual = [string]$evidence.project; Name = 'project' },
            @{ Expected = $ExpectedProjectPath; Actual = [string]$evidence.projectPath; Name = 'projectPath' },
            @{ Expected = $ExpectedClass; Actual = [string]$evidence.testClass; Name = 'testClass' },
            @{ Expected = $ExpectedGeneratedTestPath; Actual = [string]$evidence.generatedTestPath; Name = 'generatedTestPath' }
        )) {
            if (-not [string]::IsNullOrWhiteSpace($expectation.Expected) -and
                $expectation.Expected -cne $expectation.Actual) {
                throw "Regression evidence $($expectation.Name) does not match the trusted selector."
            }
        }
        if ([string]$evidence.baselineResult -cne 'regression/baseline/strict-test-evidence.json' -or
            [string]$evidence.fixResult -cne 'regression/fix/strict-test-evidence.json') {
            throw 'Regression evidence uses an unexpected result path.'
        }
        $patchDigest = Get-ReplicationBindingFileDigest -Path (Join-Path $ArtifactRoot 'fix.patch')
        if ([string]$evidence.productPatchSha256 -cne [string]$patchDigest) {
            throw 'Regression evidence product patch digest does not match fix.patch.'
        }

        $baselinePath = Join-Path $ArtifactRoot ([string]$evidence.baselineResult)
        $fixPath = Join-Path $ArtifactRoot ([string]$evidence.fixResult)
        $baseline = Read-ReplicationRegressionRunEvidence `
            -Path $baselinePath `
            -ExpectedPlatform $ExpectedPlatform `
            -ExpectedProject ([string]$evidence.project) `
            -ExpectedCategory ([string]$evidence.category) `
            -ExpectedClass ([string]$evidence.testClass)
        $fix = Read-ReplicationRegressionRunEvidence `
            -Path $fixPath `
            -ExpectedPlatform $ExpectedPlatform `
            -ExpectedProject ([string]$evidence.project) `
            -ExpectedCategory ([string]$evidence.category) `
            -ExpectedClass ([string]$evidence.testClass)
        if ($baseline.Digest -cne [string]$evidence.baselineResultSha256 -or
            $fix.Digest -cne [string]$evidence.fixResultSha256) {
            throw 'Regression evidence does not bind the exact baseline and fix result documents.'
        }

        $baselineIds = @($baseline.Records.Keys | Sort-Object -CaseSensitive)
        $fixIds = @($fix.Records.Keys | Sort-Object -CaseSensitive)
        if (($baselineIds -join "`n") -cne ($fixIds -join "`n")) {
            throw 'Regression baseline and fix runs did not execute the same exact test identities.'
        }
        $comparablePassingCount = 0
        foreach ($identity in $baselineIds) {
            $beforeGroup = @($baseline.Records[$identity])
            $afterGroup = @($fix.Records[$identity])
            if ($beforeGroup.Count -ne $afterGroup.Count) {
                throw 'Regression baseline and fix runs did not execute the same exact test identity multiset.'
            }
            foreach ($after in $afterGroup) {
                foreach ($before in $beforeGroup) {
                    $accepted = switch ($before.Outcome) {
                        'Pass' { $after.Outcome -ceq 'Pass' }
                        'Fail' {
                            $after.Outcome -ceq 'Pass' -or
                            ($after.Outcome -ceq 'Fail' -and
                                $after.FailureSignature -ceq
                                    $before.FailureSignature)
                        }
                        'Skip' { $after.Outcome -in @('Skip', 'Pass') }
                        default { $false }
                    }
                    if (-not $accepted) {
                        $display = $identity -replace "`n", '.'
                        throw "Regression evidence detected an incompatible outcome for '$display': $($before.Outcome) -> $($after.Outcome)."
                    }
                }
            }
            if (@($beforeGroup | Where-Object { $_.Outcome -ceq 'Pass' }).Count -gt 0 -and
                @($afterGroup | Where-Object { $_.Outcome -ceq 'Pass' }).Count -gt 0) {
                $comparablePassingCount++
            }
        }
        if ($comparablePassingCount -lt 1) {
            throw 'Regression evidence contains no comparable passing sibling execution.'
        }
        return $evidence
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
        [string]$ExpectedRegressionCategory = '',
        [string]$ExpectedRegressionClass = '',
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
    if ($null -ne $evidence['fix.patch']) {
        if ([string]::IsNullOrWhiteSpace($ExpectedRegressionCategory) -or
            [string]::IsNullOrWhiteSpace($ExpectedRegressionClass)) {
            throw 'Certification binding requires the trusted regression category and class for a fix.'
        }
        $null = Assert-ReplicationRegressionEvidence `
            -ArtifactRoot $root `
            -ExpectedBaselineSha $ReplicationBaseSha `
            -ExpectedPlatform $Platform `
            -ExpectedCategory $ExpectedRegressionCategory `
            -ExpectedClass $ExpectedRegressionClass
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
        [string]$ExpectedRegressionCategory = '',
        [string]$ExpectedRegressionClass = '',
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

    $fixPatchDigest = Get-ReplicationBindingValue -Source $boundEvidence -Name 'fix.patch'
    if ($null -ne $fixPatchDigest -and
        -not [string]::IsNullOrWhiteSpace([string]$fixPatchDigest)) {
        if ([string]::IsNullOrWhiteSpace($ExpectedRegressionCategory) -or
            [string]::IsNullOrWhiteSpace($ExpectedRegressionClass)) {
            throw "Certification binding requires trusted regression selector inputs ($Context)."
        }
        $null = Assert-ReplicationRegressionEvidence `
            -ArtifactRoot $root `
            -ExpectedBaselineSha ([string](Get-ReplicationBindingValue `
                -Source $Binding -Name 'replicationBaseSha')) `
            -ExpectedPlatform ([string](Get-ReplicationBindingValue `
                -Source $Binding -Name 'platform')) `
            -ExpectedCategory $ExpectedRegressionCategory `
            -ExpectedClass $ExpectedRegressionClass
    }

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
