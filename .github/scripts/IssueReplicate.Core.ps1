function Get-IssueReplicateDownload {
    param([Parameter(Mandatory)][uri]$Url, [Parameter(Mandatory)][long]$MaxBytes)

    $handler = [Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $client = [Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(90)
    $client.DefaultRequestHeaders.UserAgent.ParseAdd('maui-issue-replicate/1.0')
    try {
        for ($hop = 0; $hop -lt 5; $hop++) {
            if ($Url.Scheme -cne 'https' -or
                $Url.Host -notin @('api.github.com', 'github.com', 'codeload.github.com',
                    'objects.githubusercontent.com', 'private-user-images.githubusercontent.com')) {
                throw 'The repro source redirected outside the allowlisted GitHub hosts.'
            }
            $request = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::Get, $Url)
            if ($Url.Host -eq 'api.github.com' -and $env:GH_READ_TOKEN) {
                $request.Headers.Authorization = [Net.Http.Headers.AuthenticationHeaderValue]::new(
                    'Bearer', $env:GH_READ_TOKEN)
            }
            try {
                $response = $client.SendAsync($request, [Net.Http.HttpCompletionOption]::ResponseHeadersRead).
                    GetAwaiter().GetResult()
                try {
                    if ([int]$response.StatusCode -ge 300 -and [int]$response.StatusCode -lt 400) {
                        if (-not $response.Headers.Location) { throw 'A repro download redirect has no destination.' }
                        $Url = [uri]::new($Url, $response.Headers.Location)
                        continue
                    }
                    $response.EnsureSuccessStatusCode() | Out-Null
                    if ($response.Content.Headers.ContentLength -gt $MaxBytes) {
                        throw 'The repro source exceeds the download limit.'
                    }
                    $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
                    $output = [IO.MemoryStream]::new()
                    $buffer = [byte[]]::new(81920)
                    try {
                        while (($count = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                            if ($output.Length + $count -gt $MaxBytes) {
                                throw 'The repro source exceeds the download limit.'
                            }
                            $output.Write($buffer, 0, $count)
                        }
                        return ,$output.ToArray()
                    } finally {
                        $stream.Dispose()
                        $output.Dispose()
                    }
                } finally { $response.Dispose() }
            } finally { $request.Dispose() }
        }
        throw 'The repro source redirected too many times.'
    } finally {
        $client.Dispose()
        $handler.Dispose()
    }
}

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
        foreach ($match in [regex]::Matches($text, '(?i)https://github\.com/([a-z0-9][a-z0-9-]{0,38})/([a-z0-9_.-]+)(?:/tree/([a-z0-9_.%-]+))?(?=[\s)\]>,;]|$)')) {
            if ($match.Groups[1].Value -eq 'user-attachments') { continue }
            $fallback = ''
            if (-not $match.Groups[3].Success -and $match.Value.EndsWith('.') -and
                ($match.Index -eq 0 -or $text[$match.Index - 1] -ne '(')) {
                $fallback = $match.Value.TrimEnd('.')
            }
            $sources += [pscustomobject]@{
                Type = 'repository'
                Url = $match.Value
                Repository = "$($match.Groups[1].Value)/$($match.Groups[2].Value)"
                Ref = [uri]::UnescapeDataString($match.Groups[3].Value)
                FallbackUrl = $fallback
            }
        }
        $sources = @($sources | Sort-Object Url -Unique)
        if ($sources.Count -gt 1) {
            throw 'The latest author repro contains multiple supported links; keep one ZIP or public repository link.'
        }
        if ($sources.Count -eq 1) {
            return [pscustomobject]@{
                Type = $sources[0].Type
                Url = $sources[0].Url
                Repository = $sources[0].Repository
                Ref = $sources[0].Ref
                FallbackUrl = $sources[0].FallbackUrl
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
                $name -match '\\|//|(^|/)\.{1,2}(/|$)|^/|^[A-Za-z]:|[\x00-\x1F]' -or
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
                    $outputStream = [IO.File]::Open($destination, [IO.FileMode]::CreateNew)
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

function Get-IssueReplicateUiPlatformCondition {
    param([Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform,
        [switch]$HostApp)

    if ($HostApp) { return $Platform.ToUpperInvariant() }

    switch ($Platform) {
        'android' { return 'TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST' }
        'ios' { return 'TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST' }
    }
}

function Assert-IssueReplicateCandidate {
    param([Parameter(Mandatory)]$Candidate, [Parameter(Mandatory)][int]$IssueNumber,
        [ValidateSet('android', 'ios')][string]$Platform = '')

    if ($Candidate.kind -cnotin @('unit', 'xaml', 'ui')) { throw 'Unsupported candidate test kind.' }
    $uiCondition = ''
    if ($Candidate.kind -eq 'ui') {
        if (-not $Platform) { throw 'A UI candidate requires an explicit verified platform.' }
        $uiCondition = Get-IssueReplicateUiPlatformCondition -Platform $Platform
    }
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
        if ($Candidate.kind -eq 'ui') {
            $condition = $uiCondition
            $surface = 'NUnit'
            if ($path.Contains('TestCases.HostApp/')) {
                $condition = Get-IssueReplicateUiPlatformCondition -Platform $Platform -HostApp
                $surface = 'HostApp'
            }
            # C# recognizes line terminators beyond LF; none may hide an escaping directive.
            $source = [regex]::Replace($file.content, '\r\n|[\r\u0085\u2028\u2029]', "`n").Trim()
            $directives = [regex]::Matches($source, '^\s*#',
                [Text.RegularExpressions.RegexOptions]::Multiline, [TimeSpan]::FromSeconds(1))
            if (-not $source.StartsWith("#if $condition`n", [StringComparison]::Ordinal) -or
                -not $source.EndsWith("`n#endif", [StringComparison]::Ordinal) -or $directives.Count -ne 2) {
                throw "A UI candidate $surface file must use the exclusive $Platform whole-file platform guard without other preprocessor directives."
            }
        }
        $literal = '(?:true|false|null|0[xX][0-9A-Fa-f]+|[0-9]+(?:\.[0-9]+)?(?:[uUlLfFdDmM]+)?|"(?:\\.|[^"\\])*"|''(?:\\.|[^''\\])'')'
        $constant = "(?:$literal|\s|[()+*/%<>=!&|^~?:-])+"
        $single = "Assert\.(?:That|True|False|Null|NotNull|IsTrue|IsFalse|IsNull|IsNotNull)\s*\(\s*$constant\s*(?:,|\))"
        $pair = "Assert\.(?:Equal|NotEqual|AreEqual|AreNotEqual|Same|NotSame|AreSame|AreNotSame)\s*\(\s*$constant\s*,\s*$constant\s*(?:,|\))"
        $constraint = "Is(?:\.Not)?\.(?:True|False|Null|NotNull|EqualTo\s*\(\s*$constant\s*\))"
        $nunit = "Assert\.That\s*\(\s*$constant\s*,\s*$constraint\s*(?:,|\))"
        if ([regex]::IsMatch($file.content, "\bAssert\.Fail\b|\b$single|\b$pair|\b$nunit",
            [Text.RegularExpressions.RegexOptions]::IgnoreCase, [TimeSpan]::FromSeconds(1))) {
            throw 'The generated test contains an unconditional failure.'
        }
        $total += $file.content.Length
    }
    if ($total -gt 60000) { throw 'Candidate test is too large.' }
    if ($Candidate.kind -eq 'unit' -and $files.Count -ne 1) { throw 'Unit candidates must target exactly one test file and project.' }
    if ($Candidate.kind -eq 'ui' -and $files.Count -ne 2) { throw 'UI candidates need a HostApp page and an NUnit test.' }
    if ($Candidate.kind -eq 'xaml' -and $files.Count -ne 2) { throw 'XAML candidates need markup and code-behind.' }
    return $true
}

function Read-IssueReplicateSampleResult {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)]$Manifest)

    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -lt 1 -or $file.Length -gt 16384 -or
        $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'The author sample result must be a bounded regular file.'
    }
    $sample = [Text.UTF8Encoding]::new($false, $true).GetString([IO.File]::ReadAllBytes($file.FullName)) |
        ConvertFrom-Json -Depth 6
    if ($sample.sampleSha256 -cne $Manifest.sampleSha256 -or $sample.targetSha -cne $Manifest.targetSha -or
        $sample.buildSucceeded -isnot [bool] -or
        $sample.targetFramework -cnotmatch "^net[0-9]+\.[0-9]+-$($Manifest.platform)(?:[0-9]+(?:\.[0-9]+)*)?$" -or
        ($null -ne $sample.diagnostic -and
            ($sample.diagnostic -isnot [string] -or $sample.diagnostic.Length -gt 2048))) {
        throw 'The author sample result does not match the immutable snapshot or bounded build contract.'
    }
    return $sample
}

function Get-IssueReplicateDraftPatch {
    param([Parameter(Mandatory)]$Candidate, [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform)

    Assert-IssueReplicateCandidate -Candidate $Candidate -IssueNumber $IssueNumber -Platform $Platform | Out-Null
    $root = Join-Path ([IO.Path]::GetTempPath()) "issue-replicate-draft-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $root | Out-Null
    try {
        & git init --quiet --template= $root
        if ($LASTEXITCODE -ne 0) { throw 'Could not prepare the draft diff.' }
        $paths = @()
        foreach ($file in @($Candidate.files)) {
            $path = Join-Path $root $file.path
            New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force | Out-Null
            [IO.File]::WriteAllText($path, $file.content, [Text.UTF8Encoding]::new($false))
            $paths += $file.path
        }
        & git -C $root -c core.autocrlf=false add -N -- $paths
        if ($LASTEXITCODE -ne 0) { throw 'Could not stage the draft text.' }
        $patchPath = Join-Path $root 'candidate.patch'
        & git -C $root -c core.autocrlf=false diff --no-ext-diff --no-textconv --binary "--output=$patchPath" -- $paths
        if ($LASTEXITCODE -ne 0) { throw 'Could not render the complete draft diff.' }
        $patch = Get-Item -LiteralPath $patchPath
        if ($patch.Length -lt 1 -or $patch.Length -gt 100KB) { throw 'The draft diff is empty or too large.' }
        return [Text.UTF8Encoding]::new($false, $true).GetString([IO.File]::ReadAllBytes($patchPath))
    } finally {
        Remove-Item -LiteralPath $root -Recurse -Force
    }
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

function Get-IssueReplicateFeedback {
    [CmdletBinding(DefaultParameterSetName = 'File')]
    param(
        [Parameter(Mandatory, ParameterSetName = 'File')][string]$Path,
        [Parameter(Mandatory, ParameterSetName = 'Memory')][AllowEmptyCollection()][string[]]$Lines
    )

    if ($PSCmdlet.ParameterSetName -eq 'File') {
        $file = Get-Item -LiteralPath $Path -ErrorAction Stop
        if ($file.PSIsContainer -or $file.Length -gt 4MB -or
            $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Test feedback must be a bounded regular log file.'
        }
        $Lines = @(Get-Content -LiteralPath $file.FullName)
    }
    $diagnostics = @($Lines | Where-Object {
        $_ -match '(?i):\s*error\s+[A-Z]+[0-9]+:|^\s*(Failed\b|Error Message:|Stack Trace:|Expected:|But was:)|AssertionException'
    } | Select-Object -Last 25)
    $content = if ($diagnostics.Count -gt 0) { $diagnostics -join "`n" }
        else { ($Lines | Select-Object -Last 25) -join "`n" }
    $bytes = [Text.Encoding]::UTF8.GetBytes($content)
    if ($bytes.Length -gt 3500) {
        $content = [Text.Encoding]::UTF8.GetString($bytes, $bytes.Length - 3500, 3500)
    }
    return $content
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
            "(^|[.+])$([regex]::Escape($ClassName))([.+]|\(|$)"
    })
    $ids = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $nunitIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $methods = [System.Collections.Generic.Dictionary[string, Xml.XmlElement]]::new([StringComparer]::Ordinal)
    foreach ($definition in $definitions) {
        if ([string]::IsNullOrWhiteSpace($definition.GetAttribute('id'))) {
            return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
        }
        [void]$ids.Add($definition.GetAttribute('id'))
        $method = $definition.SelectSingleNode("*[local-name()='TestMethod']")
        if (-not $methods.TryAdd($definition.GetAttribute('id'), $method)) {
            return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
        }
        if ($method.GetAttribute('adapterTypeName') -eq 'executor://nunit3testexecutor/') {
            [void]$nunitIds.Add($definition.GetAttribute('id'))
        }
    }
    $allResults = @($xml.SelectNodes("//*[local-name()='UnitTestResult']"))
    $counters = $xml.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")
    $allPassed = @($allResults | Where-Object { $_.GetAttribute('outcome') -eq 'Passed' }).Count
    $allFailures = @($allResults | Where-Object { $_.GetAttribute('outcome') -eq 'Failed' }).Count
    if ($ids.Count -lt 1 -or $allResults.Count -lt 1 -or $null -eq $counters -or
        [int]$counters.GetAttribute('total') -ne $allResults.Count -or
        [int]$counters.GetAttribute('executed') -ne $allResults.Count -or
        [int]$counters.GetAttribute('passed') -ne $allPassed -or
        [int]$counters.GetAttribute('failed') -ne $allFailures -or
        @($allResults | Where-Object {
            [string]::IsNullOrWhiteSpace($_.GetAttribute('testName'))
        }).Count -gt 0) {
        return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
    }
    $results = @($allResults | Where-Object { $ids.Contains($_.GetAttribute('testId')) })
    if ($results.Count -lt 1) { return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() } }
    $passed = @($results | Where-Object { $_.GetAttribute('outcome') -eq 'Passed' }).Count
    $failures = @($results | Where-Object { $_.GetAttribute('outcome') -eq 'Failed' })
    $names = @($results | ForEach-Object { $_.GetAttribute('testName') } | Sort-Object)
    if ($passed -eq $results.Count -and $ExitCode -eq 0 -and $allFailures -eq 0) {
        return [pscustomobject]@{ Status = 'Passed'; Names = $names }
    }
    if ($ExitCode -ne 0 -and $failures.Count -gt 0 -and
        $failures.Count + $passed -eq $results.Count -and
        @($failures | Where-Object {
            $errorInfo = $_.SelectSingleNode("*[local-name()='Output']/*[local-name()='ErrorInfo']")
            $messageNode = if ($errorInfo) { $errorInfo.SelectSingleNode("*[local-name()='Message']") } else { $null }
            $message = if ($messageNode) { $messageNode.InnerText } else { '' }
            $errorInfo -and
                $message -notmatch '(?i)Xunit\.Sdk\.TestClassException' -and
                $message -notmatch '(?im)^\s*(?:OneTimeSetUp|SetUp|OneTimeTearDown|TearDown)\s*:' -and
                ($errorInfo.InnerText -match '(?i)NUnit\.Framework\.AssertionException|Xunit\.Sdk\.(?:True|False|Equal|NotEqual|StrictEqual|Null|NotNull|Empty|NotEmpty|Single|Collection|Contains|DoesNotContain|InRange|NotInRange|IsType|IsNotType|IsAssignableFrom|Throws|ThrowsAny|Same|NotSame|StartsWith|EndsWith|Matches|DoesNotMatch|All|Equivalent|Multiple|PropertyChanged)Exception\b|AssertFailedException|at\s+(?:Xunit|NUnit\.Framework)\.Assert\.' -or
                    ($nunitIds.Contains($_.GetAttribute('testId')) -and
                        $message -match '(?m)^\s*Assert\.That\(' -and
                        $message -match '(?m)^\s*Expected:' -and
                        $message -match '(?m)^\s*But was:'))
        }).Count -eq $failures.Count) {
        $identities = @()
        foreach ($failure in $failures) {
            $errorInfo = $failure.SelectSingleNode("*[local-name()='Output']/*[local-name()='ErrorInfo']")
            $message = $errorInfo.SelectSingleNode("*[local-name()='Message']").InnerText.Replace("`r", '').Trim()
            $stack = $errorInfo.SelectSingleNode("*[local-name()='StackTrace']")
            $stackFrames = @(if ($stack) { $stack.InnerText.Replace("`r", '') -split "`n" })
            $frames = @(
                $stackFrames | Where-Object {
                    $_ -match "\b$([regex]::Escape($ClassName))([.(+])"
                }
            )
            if ($frames.Count -eq 0 -or @($frames | Where-Object { $_ -match '\.c?ctor\b' }).Count -gt 0) {
                return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
            }
            $method = $methods[$failure.GetAttribute('testId')]
            if ([string]::IsNullOrWhiteSpace($method.GetAttribute('name'))) {
                return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
            }
            $recordedClass = $method.GetAttribute('className')
            if ($nunitIds.Contains($failure.GetAttribute('testId'))) {
                $recordedClass = $recordedClass -replace '\([^\r\n]*\)$', ''
            }
            $declaringClass = [regex]::Escape($recordedClass)
            $methodName = [regex]::Escape($method.GetAttribute('name'))
            $bodyPattern = "\bat\s+$declaringClass(?:\.$methodName\s*(?:\(|\[|<)|[.+]<$methodName>)"
            $bodyIndex = -1
            for ($index = 0; $index -lt $stackFrames.Count; $index++) {
                if ($stackFrames[$index] -cmatch $bodyPattern) { $bodyIndex = $index; break }
            }
            # A body can invoke Dispose itself; reject lifecycle callers outside that body instead.
            if ($bodyIndex -lt 0 -or @($stackFrames | Select-Object -Skip $bodyIndex | Where-Object {
                $_ -match '\.c?ctor\b|\.(?:InitializeAsync|DisposeAsync|Dispose)\s*\(|<(?:[^<>\r\n]+\.)?(?:InitializeAsync|DisposeAsync|Dispose)>'
            }).Count -gt 0) {
                return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() }
            }
            $source = $frames[0]
            $identity = "$($failure.GetAttribute('testName'))`n$message`n$($source.Trim())"
            $identities += [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
                [Text.Encoding]::UTF8.GetBytes($identity))).ToLowerInvariant()
        }
        return [pscustomobject]@{
            Status = 'AssertionFailed'
            Names = @($failures | ForEach-Object { $_.GetAttribute('testName') } | Sort-Object)
            FailureIdentities = @($identities | Sort-Object)
        }
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
