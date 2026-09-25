function Parse-IssueReplicateCommand {
    param([AllowNull()][string]$Body)

    if ($null -eq $Body -or $Body.Length -gt 512) { return $null }
    $text = $Body.Trim()
    if ($text -cnotmatch '^/issue\s+replicate(?:\s|$)') { return $null }

    $parts = @($text -split '\s+')
    $platform = ''
    $branch = 'main'
    $branchSpecified = $false
    for ($i = 2; $i -lt $parts.Count; $i += 2) {
        if ($i + 1 -ge $parts.Count) { throw 'A command option is missing its value.' }
        switch -CaseSensitive ($parts[$i]) {
            '--platform' {
                if ($platform) { throw 'The platform was specified more than once.' }
                $platform = $parts[$i + 1]
                if ($platform -cnotin @('android', 'ios')) { throw 'Supported platforms: android, ios.' }
            }
            '--branch' {
                if ($branchSpecified) { throw 'The branch was specified more than once.' }
                $branchSpecified = $true
                $branch = $parts[$i + 1]
                if ($branch -cnotmatch '^(main|net[0-9]+\.0)$') { throw 'Supported branches: main, netN.0.' }
            }
            default { throw 'Unsupported /issue replicate option.' }
        }
    }

    return [pscustomobject]@{ Platform = $platform; Branch = $branch }
}

function Resolve-IssueReplicatePlatform {
    param([string[]]$Labels, [string]$Requested = '')

    if ($Requested) {
        if ($Requested -cnotin @('android', 'ios')) { throw 'Supported platforms: android, ios.' }
        return $Requested
    }
    $supported = @($Labels | Where-Object { $_ -in @('platform/android', 'platform/ios') } |
        ForEach-Object { $_.Split('/')[1].ToLowerInvariant() } | Select-Object -Unique)
    if ($supported.Count -ne 1) {
        throw 'Specify --platform android or --platform ios when the issue has no single supported platform label.'
    }
    return $supported[0]
}

function Get-IssueReplicateSource {
    param([Parameter(Mandatory)][string[]]$AuthorTexts)

    foreach ($text in $AuthorTexts) {
        $sources = @()
        foreach ($match in [regex]::Matches($text, '(?i)\[[^\]\r\n]*\.zip\]\((https://github\.com/user-attachments/(?:assets/[a-f0-9-]{36}|files/[1-9][0-9]*/[a-z0-9_.-]+\.zip))\)')) {
            $sources += [pscustomobject]@{ Type = 'attachment'; Url = $match.Groups[1].Value }
        }
        foreach ($match in [regex]::Matches($text, '(?i)https://github\.com/([a-z0-9][a-z0-9-]{0,38})/([a-z0-9_.-]+)(?=[\s)\]>,]|$)')) {
            if ($match.Groups[1].Value -eq 'user-attachments') { continue }
            $sources += [pscustomobject]@{ Type = 'repository'; Url = $match.Value }
        }
        $sources = @($sources | Sort-Object Url -Unique)
        if ($sources.Count -gt 1) {
            throw 'The latest author repro contains multiple supported links; keep one ZIP or public repository link.'
        }
        if ($sources.Count -eq 1) {
            return [pscustomobject]@{
                Type = $sources[0].Type
                Url = $sources[0].Url
                Text = $text.Substring(0, [Math]::Min(8000, $text.Length))
            }
        }
    }

    throw 'No GitHub-hosted ZIP attachment or public GitHub repository was found in the issue author text.'
}

function Assert-IssueReplicateZip {
    param([Parameter(Mandatory)][string]$Path, [string]$ExtractTo = '')

    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.Length -gt 10MB -or $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'The sample archive is too large or is a link.'
    }
    $archive = [System.IO.Compression.ZipFile]::OpenRead($file.FullName)
    try {
        if ($archive.Entries.Count -lt 1 -or $archive.Entries.Count -gt 512) {
            throw 'The sample ZIP contains too many or no entries.'
        }
        $expanded = 0L
        $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        $root = if ($ExtractTo) { [IO.Path]::GetFullPath($ExtractTo) + [IO.Path]::DirectorySeparatorChar } else { '' }
        foreach ($entry in $archive.Entries) {
            $name = $entry.FullName
            $expanded += $entry.Length
            $mode = ($entry.ExternalAttributes -shr 16) -band 0xF000
            if ($expanded -gt 40MB -or $entry.Length -gt 10MB -or
                $name -match '\\|(^|/)\.{1,2}(/|$)|^/|^[A-Za-z]:|[\x00-\x1F]' -or
                $mode -eq 0xA000 -or -not $seen.Add($name.TrimEnd('/'))) {
                throw 'The sample ZIP contains an oversized, linked, repeated, or unsafe entry.'
            }
            if ($root) {
                $destination = [IO.Path]::GetFullPath((Join-Path $root $name))
                if (-not $destination.StartsWith($root, [StringComparison]::Ordinal)) {
                    throw 'A sample ZIP entry escapes its extraction directory.'
                }
            }
        }
        if ($root) {
            New-Item -ItemType Directory -Path $root -Force | Out-Null
            foreach ($entry in $archive.Entries) {
                $destination = [IO.Path]::GetFullPath((Join-Path $root $entry.FullName))
                if ($entry.FullName.EndsWith('/')) {
                    New-Item -ItemType Directory -Path $destination -Force | Out-Null
                } else {
                    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
                    $inputStream = $entry.Open()
                    $outputStream = [IO.File]::Create($destination)
                    try { $inputStream.CopyTo($outputStream) }
                    finally { $outputStream.Dispose(); $inputStream.Dispose() }
                }
            }
        }
    } finally { $archive.Dispose() }
}

function Test-IssueReplicateCandidatePath {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][ValidateSet('unit', 'xaml', 'ui')][string]$Kind)

    if ($Path -match '\\|(^|/)\.{1,2}(/|$)|(^|/)\.git(/|$)' -or
        $Path -notmatch '^[A-Za-z0-9_./-]+$') { return $false }
    $name = if ($Kind -eq 'xaml') { "Maui$IssueNumber" } else { "Issue$IssueNumber" }
    switch ($Kind) {
        'unit' {
            return $Path -cmatch "^(src/Core/tests/UnitTests|src/Controls/tests/Core\.UnitTests|src/Essentials/test/UnitTests)/(?:.*/)?$name\.cs$"
        }
        'xaml' {
            return $Path -ceq "src/Controls/tests/Xaml.UnitTests/Issues/$name.xaml" -or
                $Path -ceq "src/Controls/tests/Xaml.UnitTests/Issues/$name.xaml.cs"
        }
        'ui' {
            return $Path -ceq "src/Controls/tests/TestCases.HostApp/Issues/$name.cs" -or
                $Path -ceq "src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/$name.cs"
        }
    }
}

function Assert-IssueReplicateCandidate {
    param([Parameter(Mandatory)]$Candidate, [Parameter(Mandatory)][int]$IssueNumber)

    if ($Candidate.kind -cnotin @('unit', 'xaml', 'ui')) { throw 'Unsupported candidate test kind.' }
    $files = @($Candidate.files)
    if ($files.Count -lt 1 -or $files.Count -gt 3) { throw 'A candidate must contain one to three test files.' }
    $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $total = 0
    foreach ($file in $files) {
        $path = [string]$file.path
        if (-not (Test-IssueReplicateCandidatePath -Path $path -IssueNumber $IssueNumber -Kind $Candidate.kind) -or
            -not $seen.Add($path)) {
            throw 'Candidate contains an unexpected or repeated file path.'
        }
        if ($file.content -isnot [string] -or $file.content.Length -gt 30000 -or
            [string]::IsNullOrWhiteSpace($file.content)) { throw 'Candidate test file is empty or too large.' }
        if ($file.content -match '(?i)\bAssert\.(Fail|True\s*\(\s*false\s*\))\b') {
            throw 'The generated test contains an unconditional failure.'
        }
        $total += $file.content.Length
    }
    if ($total -gt 60000) { throw 'Candidate test is too large.' }
    if ($Candidate.kind -eq 'ui' -and $files.Count -ne 2) { throw 'UI candidates need a HostApp page and an NUnit test.' }
    if ($Candidate.kind -eq 'xaml' -and $files.Count -ne 2) { throw 'XAML candidates need markup and code-behind.' }
    return $true
}

function ConvertFrom-IssueReplicateCopilotOutput {
    param([Parameter(Mandatory)][string]$Path)

    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -lt 1 -or $file.Length -gt 512KB) {
        throw 'Copilot did not return a bounded response.'
    }
    $messages = @()
    $completed = $false
    foreach ($line in [IO.File]::ReadLines($file.FullName)) {
        if ($line.Length -gt 300000) { throw 'A Copilot event is too large.' }
        $event = $line | ConvertFrom-Json -Depth 12
        if ($event.type -eq 'assistant.message' -and $event.data.phase -eq 'final_answer') {
            $messages += $event.data
        }
        if ($event.type -eq 'result') {
            if ($completed -or $event.exitCode -ne 0) { throw 'Copilot did not complete successfully.' }
            $completed = $true
        }
    }
    if (-not $completed -or $messages.Count -ne 1 -or
        @($messages[0].toolRequests).Count -ne 0 -or
        [string]::IsNullOrWhiteSpace([string]$messages[0].content)) {
        throw 'Copilot did not return one tool-free final response.'
    }
    $content = [string]$messages[0].content
    if ([Text.Encoding]::UTF8.GetByteCount($content) -gt 80000) {
        throw 'Copilot generated an oversized candidate.'
    }
    return $content | ConvertFrom-Json -Depth 6
}

function Get-IssueReplicateTrxVerdict {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][string]$ClassName,
        [Parameter(Mandatory)][int]$ExitCode)

    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -lt 1 -or $file.Length -gt 1MB) {
        throw 'A test result is missing or too large.'
    }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $settings.MaxCharactersInDocument = 1MB
    $reader = [Xml.XmlReader]::Create($file.FullName, $settings)
    try {
        $xml = [xml]::new()
        $xml.XmlResolver = $null
        $xml.Load($reader)
    } finally { $reader.Dispose() }
    $definitions = @($xml.SelectNodes("//*[local-name()='UnitTest']") | Where-Object {
        $method = $_.SelectSingleNode("*[local-name()='TestMethod']")
        $method -and $method.GetAttribute('className') -match
            "(^|[.+])$([regex]::Escape($ClassName))([.+]|$)"
    })
    $ids = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($definition in $definitions) {
        if ([string]::IsNullOrWhiteSpace($definition.GetAttribute('id'))) {
            return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
        }
        [void]$ids.Add($definition.GetAttribute('id'))
    }
    $results = @($xml.SelectNodes("//*[local-name()='UnitTestResult']"))
    $counters = $xml.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")
    $passed = @($results | Where-Object { $_.GetAttribute('outcome') -eq 'Passed' }).Count
    $failures = @($results | Where-Object { $_.GetAttribute('outcome') -eq 'Failed' })
    if ($ids.Count -lt 1 -or $results.Count -lt 1 -or $null -eq $counters -or
        [int]$counters.GetAttribute('total') -ne $results.Count -or
        [int]$counters.GetAttribute('executed') -ne $results.Count -or
        [int]$counters.GetAttribute('passed') -ne $passed -or
        [int]$counters.GetAttribute('failed') -ne $failures.Count -or
        @($results | Where-Object {
            -not $ids.Contains($_.GetAttribute('testId')) -or
            [string]::IsNullOrWhiteSpace($_.GetAttribute('testName'))
        }).Count -gt 0) {
        return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
    }
    $names = @($results | ForEach-Object { $_.GetAttribute('testName') } | Sort-Object)
    if ($passed -eq $results.Count -and $ExitCode -eq 0) {
        return [pscustomobject]@{ Status = 'Passed'; Names = $names }
    }
    if ($ExitCode -ne 0 -and $failures.Count -gt 0 -and
        $failures.Count + $passed -eq $results.Count -and
        @($failures | Where-Object {
            $errorInfo = $_.SelectSingleNode("*[local-name()='Output']/*[local-name()='ErrorInfo']")
            $errorInfo -and
                $errorInfo.InnerText -match '(?i)NUnit\.Framework\.AssertionException|Xunit\.(Sdk\.)?\w+Exception|AssertFailedException|at\s+.*\bAssert\.'
        }).Count -eq $failures.Count) {
        return [pscustomobject]@{ Status = 'AssertionFailed'; Names = $names }
    }
    return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
}

function Assert-IssueReplicateResult {
    param([Parameter(Mandatory)]$Result, [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][long]$CommentId)

    if ($Result.schemaVersion -ne 1 -or $Result.issueNumber -ne $IssueNumber -or
        $Result.commentId -ne $CommentId -or $Result.platform -cnotin @('android', 'ios') -or
        $Result.targetSha -cnotmatch '^[a-fA-F0-9]{40}$' -or
        $Result.sampleSha256 -cnotmatch '^[a-fA-F0-9]{64}$' -or
        $Result.status -cnotin @('candidate-failed', 'not-reproduced-on-tested-revision', 'inconclusive', 'unsupported')) {
        throw 'The issue reproduction result does not match the requested issue, revision, or status.'
    }
    if ($Result.status -eq 'candidate-failed' -and
        ($Result.testExecuted -ne $true -or $Result.assertionFailed -ne $true -or
         $Result.sampleBuilt -ne $true -or $Result.testKind -cnotin @('unit', 'xaml', 'ui') -or
         $Result.patchSha256 -cnotmatch '^[a-fA-F0-9]{64}$')) {
        throw 'A failing test candidate needs a built sample, repeated executed assertions, and a test patch.'
    }
    if ($Result.status -ne 'candidate-failed' -and $Result.patchSha256) {
        throw 'Only a verified failing test candidate can publish a patch.'
    }
    if ($Result.status -eq 'not-reproduced-on-tested-revision' -and
        ($Result.testExecuted -ne $true -or $Result.assertionFailed -ne $false -or
         $Result.sampleBuilt -ne $true)) {
        throw 'A passing verdict needs an executed test and a built sample.'
    }
    return $true
}
