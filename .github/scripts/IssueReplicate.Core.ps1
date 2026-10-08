function Get-IssueReplicateIOSSdkVersion {
    param([Parameter(Mandatory)][string]$RepoRoot)

    [xml]$details = Get-Content -Raw -LiteralPath (Join-Path $RepoRoot 'eng/Version.Details.xml')
    $versions = @($details.SelectNodes('//Dependency') | ForEach-Object {
            if ($_.Name -cmatch '^Microsoft\.iOS\.Sdk\.net[0-9]+\.0_([0-9]+\.[0-9]+)$') {
                [version]$Matches[1]
            }
        } | Sort-Object -Descending -Unique)
    if ($versions.Count -lt 1) { throw 'The pinned branch does not declare a supported iOS SDK contract.' }
    return $versions[0].ToString()
}

function New-IssueReplicateIOSSimulator {
    param([Parameter(Mandatory)][string]$RepoRoot)

    if (-not $IsMacOS) { throw 'Native iOS verification requires a macOS simulator host.' }
    $sdk = Get-IssueReplicateIOSSdkVersion -RepoRoot $RepoRoot
    $runtimeId = "com.apple.CoreSimulator.SimRuntime.iOS-$($sdk.Replace('.', '-'))"
    $json = & xcrun simctl list runtimes available --json
    if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate installed iOS simulator runtimes.' }
    $runtimes = ($json -join "`n") | ConvertFrom-Json
    $matching = @($runtimes.runtimes | Where-Object {
            $_.identifier -ceq $runtimeId -and $_.version -ceq $sdk -and $_.isAvailable -eq $true
        })
    if ($matching.Count -ne 1) {
        throw "The fresh runner lacks the exact available iOS $sdk runtime required by the pinned SDK; refusing a newer runtime."
    }
    $name = "issue-replicate-$([guid]::NewGuid().ToString('N'))"
    $created = & xcrun simctl create $name 'com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro' $runtimeId
    if ($LASTEXITCODE -ne 0) { throw "Could not create a fresh simulator on the pinned iOS $sdk runtime." }
    $udid = ($created -join "`n").Trim()
    if ($udid -cnotmatch '^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$') {
        throw 'Simulator creation did not return one valid device UDID.'
    }
    Write-Host "Pinned iOS SDK $sdk; created fresh iPhone 11 Pro on $runtimeId ($udid)."
    return $udid
}

function Initialize-IssueReplicateIOSWebDriverAgent {
    param(
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$SimulatorUdid
    )

    if (-not $IsMacOS) { throw 'WebDriverAgent preparation requires a macOS simulator host.' }
    $sdk = Get-IssueReplicateIOSSdkVersion -RepoRoot $RepoRoot
    $runtimeId = "com.apple.CoreSimulator.SimRuntime.iOS-$($sdk.Replace('.', '-'))"
    $json = & xcrun simctl list devices available --json
    if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate the owned iOS verification simulator.' }
    $inventory = ($json -join "`n") | ConvertFrom-Json
    $devices = @($inventory.devices.$runtimeId | Where-Object { $_.udid -ceq $SimulatorUdid })
    if ($devices.Count -ne 1 -or $devices[0].name -cnotmatch '^issue-replicate-[0-9a-f]{32}$') {
        throw 'WebDriverAgent preparation requires the uniquely owned simulator on the pinned iOS runtime.'
    }
    $node = Get-Command node -CommandType Application -ErrorAction Stop | Select-Object -First 1
    if (-not $env:APPIUM_HOME -or -not [IO.Path]::IsPathFullyQualified($env:APPIUM_HOME)) {
        throw 'WebDriverAgent preparation requires the provisioned absolute Appium home.'
    }
    $prebuild = @'
const {createRequire} = require('node:module');
const {isAbsolute, join} = require('node:path');
const load = createRequire(join(process.env.APPIUM_HOME, 'node_modules/appium-xcuitest-driver/package.json'));
const {WebDriverAgent} = load('appium-webdriveragent');
const xcode = load('appium-xcode');
const {getSimulator} = load('appium-ios-simulator');
const [udid, sdk] = process.argv.slice(1);

async function prepare() {
    const device = await getSimulator(udid, {platform: 'iOS', checkExistence: true});
    const wda = new WebDriverAgent(await xcode.getVersion(true), {
        device,
        iosSdkVersion: sdk,
        platformVersion: sdk,
        showXcodeLog: true,
        wdaLaunchTimeout: 50000,
    });
    const derivedDataPath = await wda.retrieveDerivedDataPath();
    if (!derivedDataPath || !isAbsolute(derivedDataPath)) {
        throw new Error('The installed driver did not resolve an absolute WebDriverAgent derived-data directory.');
    }
    await wda.xcodebuild.start(true);
    console.log(`WebDriverAgent preparation used the installed driver derived-data directory: ${derivedDataPath}`);
    try {
        await wda.launch('issue-replicate-preflight');
        const status = await wda.getStatus(50000);
        if (!status || !status.os || status.os.name !== 'iOS') {
            throw new Error('WebDriverAgent did not return a responsive native iOS status.');
        }
        console.log(`Native iOS preflight responsive on ${udid}: ${JSON.stringify(status)}`);
    } finally {
        await wda.quit();
    }
}

prepare().catch(error => {
    console.error(error);
    process.exitCode = 1;
});
'@
    . (Join-Path $RepoRoot '.github/scripts/shared/shared-utils.ps1')

    if ($devices[0].state -cne 'Booted') {
        $boot = Invoke-ProcessWithTimeout -FilePath 'xcrun' -TimeoutSeconds 60 `
            -ArgumentList @('simctl', 'boot', $SimulatorUdid)
        if ($boot.TimedOut -or $boot.OutputDrainTimedOut -or $boot.ExitCode -ne 0) {
            throw 'The owned iOS simulator could not begin booting within its deadline.'
        }
    }
    $ready = Invoke-ProcessWithTimeout -FilePath 'xcrun' -TimeoutSeconds 600 `
        -ArgumentList @('simctl', 'bootstatus', $SimulatorUdid, '-b')
    foreach ($row in @($ready.Output | Select-Object -Last 20)) {
        $line = $row.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', ''
        if ($line.Length -gt 2000) { $line = $line.Substring(0, 2000) }
        Write-Host "Owned simulator boot status: $line"
    }
    if ($ready.TimedOut -or $ready.OutputDrainTimedOut -or $ready.ExitCode -ne 0) {
        throw "The owned iOS simulator did not finish booting within its ten-minute cold-boot deadline (exit $($ready.ExitCode)); the candidate did not execute."
    }
    Write-Host "Building and checking the installed WebDriverAgent for pinned iOS $sdk ($SimulatorUdid); ten-minute preflight deadline."
    $timer = [Diagnostics.Stopwatch]::StartNew()
    $build = Invoke-ProcessWithTimeout -FilePath $node.Source -TimeoutSeconds 600 `
        -ArgumentList @('-e', $prebuild, $SimulatorUdid, $sdk)
    foreach ($row in @($build.Output | Select-Object -Last 80)) {
        $line = $row.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', ''
        if ($line.Length -gt 2000) { $line = $line.Substring(0, 2000) }
        Write-Host $line
    }
    if ($build.TimedOut) { throw 'WebDriverAgent preflight exceeded its ten-minute deadline; the candidate did not execute.' }
    if ($build.OutputDrainTimedOut -or $build.ExitCode -ne 0) {
        throw "WebDriverAgent preflight failed (exit $($build.ExitCode)); the candidate did not execute."
    }
    Write-Host "Responsive WebDriverAgent preflight completed in $([Math]::Ceiling($timer.Elapsed.TotalSeconds)) seconds; the pinned runner and its launch timeout remain unchanged."
}

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
    $sourceUrl = ''
    $androidApi = ''
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
            '--source' {
                if ($sourceUrl) { throw 'The source was specified more than once.' }
                $sourceUrl = $parts[$i + 1]
                $source = Get-IssueReplicateSource -AuthorTexts @("[repro.zip]($sourceUrl)")
                if ($source.Url -cne $sourceUrl) { throw 'The selected source must be one supported GitHub URL.' }
            }
            '--android-api' {
                if ($androidApi) { throw 'The Android API was specified more than once.' }
                $androidApi = $parts[$i + 1]
                if ($androidApi -cnotin @('30', '35', '36')) { throw 'Supported Android APIs: 30, 35, 36.' }
            }
            default { throw 'Unsupported /issue replicate option.' }
        }
    }

    return [pscustomobject]@{ Platform = $platform; Branch = $branch; SourceUrl = $sourceUrl; AndroidApi = $androidApi }
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

function Resolve-IssueReplicateAndroidApi {
    param([Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform,
        [string]$Requested = '')

    if ($Platform -cnotin @('android', 'ios')) { throw 'Supported platforms: android, ios.' }
    if ($Platform -ceq 'ios') {
        if ($Requested) { throw '--android-api is supported only for Android.' }
        return ''
    }
    if (-not $Requested) { return '30' }
    if ($Requested -cnotin @('30', '35', '36')) { throw 'Supported Android APIs: 30, 35, 36.' }
    return $Requested
}

function Get-IssueReplicateSnapshotAndroidApi {
    param([Parameter(Mandatory)]$Snapshot)

    if ($Snapshot.platform -cnotin @('android', 'ios')) { throw 'The snapshot platform is unsupported.' }
    if (-not $Snapshot.PSObject.Properties['androidApi']) { return '' }
    if ($Snapshot.androidApi -isnot [string] -or
        ($Snapshot.platform -ceq 'android' -and $Snapshot.androidApi -cnotin @('30', '35', '36')) -or
        ($Snapshot.platform -ceq 'ios' -and $Snapshot.androidApi -cne '')) {
        throw 'The snapshot contains an invalid or platform-incompatible Android API.'
    }
    return [string]$Snapshot.androidApi
}

function Get-IssueReplicateNativeAndroidApi {
    param([Parameter(Mandatory)][string]$RepoRoot)

    if ($env:DEVICE_UDID -cnotmatch '^emulator-[0-9]+$') {
        throw 'Android runtime verification requires the explicitly owned emulator serial.'
    }
    . (Join-Path $RepoRoot '.github/scripts/shared/shared-utils.ps1')
    $api = Invoke-ProcessWithTimeout -FilePath 'adb' -TimeoutSeconds 20 `
        -ArgumentList @('-s', $env:DEVICE_UDID, 'shell', 'getprop', 'ro.build.version.sdk')
    $value = ($api.Output -join "`n").Trim()
    if ($api.TimedOut -or $api.OutputDrainTimedOut -or $api.ExitCode -ne 0 -or
        $value -cnotmatch '^[1-9][0-9]{0,2}$') {
        throw 'The owned Android emulator did not report one API level within its deadline.'
    }
    return $value
}

function Get-IssueReplicateSource {
    param([Parameter(Mandatory)][string[]]$AuthorTexts, [string]$SelectedUrl = '')

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
        $sourceKeys = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $sources = @($sources | Where-Object {
            $key = if ($_.Type -eq 'repository') {
                "repository:$($_.Repository.ToLowerInvariant()):$($_.Ref)"
            } else {
                $_.Url.ToLowerInvariant()
            }
            $sourceKeys.Add($key)
        })
        $alternatives = @()
        if ($SelectedUrl -and $sources.Count -gt 0) {
            $alternatives = @($sources | Where-Object Url -CNE $SelectedUrl | ForEach-Object Url)
            $sources = @($sources | Where-Object Url -CEQ $SelectedUrl)
            if ($sources.Count -ne 1) {
                throw 'The selected source is not a supported link in the latest author repro text.'
            }
        }
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
                Alternatives = $alternatives
                Text = $text.Substring(0, [Math]::Min(8000, $text.Length))
            }
        }
    }

    throw 'No GitHub-hosted ZIP attachment or public GitHub repository was found in the issue author text.'
}

function Get-IssueReplicateSampleProject {
    param([Parameter(Mandatory)][string]$Directory,
        [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform)

    $projects = @(Get-ChildItem -LiteralPath $Directory -Filter *.csproj -File -Recurse |
        Where-Object { $_.FullName -notmatch '[/\\](obj|bin|__MACOSX)[/\\]|[/\\]\._' })
    if ($projects.Count -ne 1) { throw 'The sample needs exactly one buildable .csproj.' }
    $project = $projects[0]
    $xml = Get-Content -LiteralPath $project.FullName -Raw
    $tfm = [regex]::Match($xml, "net[0-9]+\.[0-9]+-$Platform(?:[0-9]+(?:\.[0-9]+)*)?(?=[;\s<`"']|$)").Value
    if (-not $tfm) { throw "The sample project does not target $Platform." }
    return [pscustomobject]@{ Project = $project; TargetFramework = $tfm }
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

function Get-IssueReplicateCSharpIdentifiers {
    param([Parameter(Mandatory)][string]$Source)

    $identifiers = [regex]::Replace($Source, '\\u([0-9a-fA-F]{4})|\\U([0-9a-fA-F]{8})', {
            param($match)
            if ($match.Groups[1].Success) {
                return [string][char][Convert]::ToInt32($match.Groups[1].Value, 16)
            }
            $codePoint = [Convert]::ToInt64($match.Groups[2].Value, 16)
            if ($codePoint -gt 0x10FFFF -or ($codePoint -ge 0xD800 -and $codePoint -le 0xDFFF)) {
                throw 'The candidate contains an invalid Unicode identifier escape.'
            }
            return [char]::ConvertFromUtf32([int]$codePoint)
        }, [Text.RegularExpressions.RegexOptions]::None, [TimeSpan]::FromSeconds(1))
    $identifiers = [regex]::Replace($identifiers, '\p{Cf}', '')
    return [regex]::Replace($identifiers, '@(?=[\p{L}\p{Nl}_])', ' ')
}

function Get-IssueReplicateAssertionExceptionNames {
    return @('AssertionException', 'AssertFailedException') + @(
        'True|False|Equal|NotEqual|StrictEqual|Null|NotNull|Empty|NotEmpty|Single|Collection|Contains|DoesNotContain|InRange|NotInRange|IsType|IsNotType|IsAssignableFrom|Throws|ThrowsAny|Same|NotSame|StartsWith|EndsWith|Matches|DoesNotMatch|All|Equivalent|Multiple|PropertyChanged'.Split('|') |
            ForEach-Object { "${_}Exception" }
    )
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
    $assertionExceptionPattern = '\b(?:' + ((Get-IssueReplicateAssertionExceptionNames |
        ForEach-Object { [regex]::Escape($_) }) -join '|') + ')\b'
    foreach ($file in $files) {
        $path = [string]$file.path
        if (-not (Test-IssueReplicateCandidatePath -Path $path -IssueNumber $IssueNumber -Kind $Candidate.kind) -or
            -not $seen.Add($path)) {
            throw 'Candidate contains an unexpected or repeated file path.'
        }
        if ($file.content -isnot [string] -or $file.content.Length -gt 30000 -or
            [string]::IsNullOrWhiteSpace($file.content)) { throw 'Candidate test file is empty or too large.' }
        $validationSource = if ($path.EndsWith('.xaml', [StringComparison]::Ordinal)) {
            [Net.WebUtility]::HtmlDecode($file.content)
        } else { $file.content }
        $identifiers = Get-IssueReplicateCSharpIdentifiers -Source $validationSource
        if ([regex]::IsMatch($identifiers, $assertionExceptionPattern,
            [Text.RegularExpressions.RegexOptions]::None, [TimeSpan]::FromSeconds(1))) {
            throw 'Generated tests must use assertion APIs, not reference assertion-exception implementation types.'
        }
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
            if ($surface -eq 'NUnit') {
                if ($identifiers -match '\b(?:ResetAfterEachTest|FixtureSetup|FixtureOneTimeTearDown|TestSetup|TestTearDown|RecordTestSetup|RecordTestTeardown|InitialSetup|Reset|(?:SetUp|TearDown|OneTimeSetUp|OneTimeTearDown)(?:Attribute)?)\b') {
                    $lifecycleToken = $Matches[0]
                    throw "A UI candidate must leave the default fixture lifecycle unchanged; lifecycle/reset hook token '$lifecycleToken' is unsupported, including in comments and literals."
                }
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
        [Parameter(Mandatory, ParameterSetName = 'Memory')][AllowEmptyCollection()][AllowEmptyString()][string[]]$Lines
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
        $_ -match '(?i)\berror(?:\s+[A-Z]+[0-9]+)?\s*:|^\s*(Failed\b|Error Message:|Stack Trace:|Expected:|But was:|Native recording failed:|Native crash diagnostic|Verification incomplete:|(?:[\w+`]+\.)*[\w+`]+Exception\s*:|at\s+(?:[\w+`]+\.)*(?:Issue|Maui)[1-9][0-9]*\.|(?:OneTime)?(?:SetUp|TearDown)\s*:)|AssertionException'
    } | Select-Object -Last 25)
    $content = if ($diagnostics.Count -gt 0) { $diagnostics -join "`n" }
        else { ($Lines | Select-Object -Last 25) -join "`n" }
    $bytes = [Text.Encoding]::UTF8.GetBytes($content)
    if ($bytes.Length -gt 3500) {
        $content = [Text.Encoding]::UTF8.GetString($bytes, $bytes.Length - 3500, 3500)
    }
    return $content
}

function Read-IssueReplicateTestResultXml {
    param([Parameter(Mandatory)][string]$Path)
    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -lt 1 -or $file.Length -gt 1MB -or
        $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
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
    }
    finally { $reader.Dispose() }
    return , $xml
}

function Get-IssueReplicateTrxVerdict {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][string]$ClassName,
        [Parameter(Mandatory)][int]$ExitCode, [string]$SingleTestMethod = '',
        [string]$NUnitResultPath = '')

    $xml = Read-IssueReplicateTestResultXml -Path $Path
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
    $nunitCases = [System.Collections.Generic.Dictionary[string, Xml.XmlElement]]::new([StringComparer]::Ordinal)
    if ($NUnitResultPath) {
        if (-not $ids.SetEquals($nunitIds)) {
            return [pscustomobject]@{
                Status = 'Inconclusive'; Names = @()
                Diagnostic = 'Structured NUnit evidence requires the matching NUnit TRX adapter.'
            }
        }
        $nunitXml = Read-IssueReplicateTestResultXml -Path $NUnitResultPath
        $nunitRun = $nunitXml.SelectSingleNode('/test-run')
        $cases = @($nunitXml.SelectNodes('/test-run//test-case'))
        if (-not $nunitRun -or $cases.Count -ne $allResults.Count -or
            [int]$nunitRun.GetAttribute('total') -ne $allResults.Count -or
            [int]$nunitRun.GetAttribute('passed') -ne $allPassed -or
            [int]$nunitRun.GetAttribute('failed') -ne $allFailures -or
            @($cases | Where-Object { $_.GetAttribute('result') -eq 'Passed' }).Count -ne $allPassed -or
            @($cases | Where-Object { $_.GetAttribute('result') -eq 'Failed' }).Count -ne $allFailures) {
            throw 'NUnit and TRX results do not describe the same completed run.'
        }
        foreach ($case in $cases) {
            $fullName = $case.GetAttribute('fullname')
            if ([string]::IsNullOrWhiteSpace($fullName) -or -not $nunitCases.TryAdd($fullName, $case)) {
                throw 'NUnit results have missing or duplicate test identities.'
            }
        }
    }
    $results = @($allResults | Where-Object { $ids.Contains($_.GetAttribute('testId')) })
    if ($results.Count -lt 1) { return [pscustomobject]@{ Status = 'Inconclusive'; Names = @() } }
    if ($SingleTestMethod -and
        ($allResults.Count -ne 1 -or $results.Count -ne 1 -or
         $methods[$results[0].GetAttribute('testId')].GetAttribute('name') -cne $SingleTestMethod)) {
        return [pscustomobject]@{
            Status = 'Inconclusive'; Names = @()
            Diagnostic = 'A recorded UI candidate must execute exactly one test matching its recording markers.'
        }
    }
    $passed = @($results | Where-Object { $_.GetAttribute('outcome') -eq 'Passed' }).Count
    $failures = @($results | Where-Object { $_.GetAttribute('outcome') -eq 'Failed' })
    $names = @($results | ForEach-Object { $_.GetAttribute('testName') } | Sort-Object)
    $xunitExceptions = ((Get-IssueReplicateAssertionExceptionNames | Where-Object {
        $_ -cnotin @('AssertionException', 'AssertFailedException')
    } | ForEach-Object { [regex]::Escape($_) }) -join '|')
    if ($passed -eq $results.Count -and $ExitCode -eq 0 -and $allFailures -eq 0) {
        return [pscustomobject]@{ Status = 'Passed'; Names = $names }
    }
    if ($ExitCode -ne 0 -and $failures.Count -gt 0 -and
        $failures.Count -eq $allFailures -and
        $failures.Count + $passed -eq $results.Count -and
        @($failures | Where-Object {
            $errorInfo = $_.SelectSingleNode("*[local-name()='Output']/*[local-name()='ErrorInfo']")
            $messageNode = if ($errorInfo) { $errorInfo.SelectSingleNode("*[local-name()='Message']") } else { $null }
            $message = if ($messageNode) { $messageNode.InnerText } else { '' }
            $stackNode = if ($errorInfo) { $errorInfo.SelectSingleNode("*[local-name()='StackTrace']") } else { $null }
            $stackText = if ($stackNode) { $stackNode.InnerText } else { '' }
            $assertionType = $message -match "(?i)\A\s*(?:NUnit\.Framework\.AssertionException|Xunit\.Sdk\.(?:$xunitExceptions)\b|(?:Microsoft\.VisualStudio\.TestTools\.UnitTesting\.)?AssertFailedException)\b"
            $ordinaryException = -not $assertionType -and
                $message -match '(?i)\A\s*(?:[\w.+`]*Exception|(?:[\w+`]+\.)+[\w+`]+)\s*:'
            $nunitAssertion = $false
            if ($nunitIds.Contains($_.GetAttribute('testId')) -and $NUnitResultPath) {
                $method = $methods[$_.GetAttribute('testId')]
                $fullName = "$($method.GetAttribute('className')).$($method.GetAttribute('name'))"
                $case = $null
                if ($nunitCases.TryGetValue($fullName, [ref]$case)) {
                    $failureMessage = $case.SelectSingleNode('failure/message')
                    $assertions = @($case.SelectNodes('assertions/assertion'))
                    $nunitAssertion = $case.GetAttribute('result') -ceq 'Failed' -and
                        $case.GetAttribute('label') -ceq '' -and $case.GetAttribute('site') -ceq '' -and
                        $case.GetAttribute('name') -ceq $_.GetAttribute('testName') -and
                        $case.GetAttribute('methodname') -ceq $method.GetAttribute('name') -and
                        [DateTimeOffset]::Parse($case.GetAttribute('start-time'), [Globalization.CultureInfo]::InvariantCulture) -eq
                            [DateTimeOffset]::Parse($_.GetAttribute('startTime'), [Globalization.CultureInfo]::InvariantCulture) -and
                        [DateTimeOffset]::Parse($case.GetAttribute('end-time'), [Globalization.CultureInfo]::InvariantCulture) -eq
                            [DateTimeOffset]::Parse($_.GetAttribute('endTime'), [Globalization.CultureInfo]::InvariantCulture) -and
                        $failureMessage -and
                        $failureMessage.InnerText.Replace("`r", '').Trim() -ceq $message.Replace("`r", '').Trim() -and
                        @($assertions | Where-Object { $_.GetAttribute('result') -ceq 'Failed' }).Count -gt 0 -and
                        @($assertions | Where-Object { $_.GetAttribute('result') -cnotin @('Passed', 'Failed') }).Count -eq 0
                }
            }
            $errorInfo -and -not [string]::IsNullOrWhiteSpace($message) -and
                -not $ordinaryException -and
                $message -notmatch '(?i)Xunit\.Sdk\.TestClassException' -and
                $message -notmatch '(?im)^\s*(?:OneTimeSetUp|SetUp|OneTimeTearDown|TearDown)\s*:' -and
                $(if ($nunitIds.Contains($_.GetAttribute('testId')) -and $NUnitResultPath) {
                    $nunitAssertion
                } else {
                    $assertionType -or $stackText -match '(?im)^\s*at\s+(?:Xunit|NUnit\.Framework)\.Assert\.\w+\b'
                })
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
            if ($nunitIds.Contains($failure.GetAttribute('testId')) -and $NUnitResultPath) {
                $fullName = "$($method.GetAttribute('className')).$($method.GetAttribute('name'))"
                $nativeStack = $nunitCases[$fullName].SelectSingleNode('failure/stack-trace')
                $nativeText = if ($nativeStack) { $nativeStack.InnerText.Replace("`r", '').Trim() } else { '' }
                $nativeAssertions = @($nunitCases[$fullName].SelectNodes("assertions/assertion[@result='Failed']"))
                $boundAssertions = @($nativeAssertions | Where-Object {
                    $assertionStack = $_.SelectSingleNode('stack-trace')
                    $assertionText = if ($assertionStack) { $assertionStack.InnerText.Replace("`r", '').Trim() } else { '' }
                    $assertionFrames = @($assertionText -split "`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
                    $assertionFrames.Count -gt 0 -and $assertionFrames[-1] -cmatch $bodyPattern -and
                        $stack.InnerText.Replace("`r", '').Contains($assertionText, [StringComparison]::Ordinal)
                })
                # NUnit 4 can omit the setup site; a setup caller below the body is still not a body failure.
                if (-not $nativeText -or $boundAssertions.Count -ne $nativeAssertions.Count -or
                    -not $stack.InnerText.Replace("`r", '').Trim().StartsWith($nativeText, [StringComparison]::Ordinal)) {
                    return [pscustomobject]@{
                        Status = 'Inconclusive'; Names = @()
                        Diagnostic = 'The structured NUnit assertion stack did not establish a matching test-body failure.'
                    }
                }
            }
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
    $diagnostic = 'The selected test result did not establish a pass or a verified issue assertion.'
    if ($failures.Count -gt 0) {
        $messageNode = $failures[0].SelectSingleNode(
            "*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']")
        if ($messageNode -and -not [string]::IsNullOrWhiteSpace($messageNode.InnerText)) {
            $message = ($messageNode.InnerText -replace '##vso\[[^]]*\]', '' -replace '[\x00-\x1f\x7f]', ' ').Trim()
            $message = $message.Substring(0, [Math]::Min(500, $message.Length))
            $diagnostic = "The selected test reported an unqualified failure: $message"
        }
    }
    return [pscustomobject]@{ Status = 'Inconclusive'; Names = @(); Diagnostic = $diagnostic }
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
    if ($Result.PSObject.Properties['androidApi'] -or $Result.PSObject.Properties['nativeAndroidApi']) {
        if ($Result.androidApi -isnot [string] -or
            $Result.androidApi -cnotin @('', '30', '35', '36') -or
            $Result.nativeAndroidApi -isnot [string] -or
            ($Result.nativeAndroidApi -and $Result.nativeAndroidApi -cnotmatch '^[1-9][0-9]{0,2}$') -or
            ($Result.platform -ceq 'ios' -and ($Result.androidApi -or $Result.nativeAndroidApi))) {
            throw 'The result contains invalid or platform-incompatible Android runtime evidence.'
        }
        if ($Result.platform -ceq 'android' -and $Result.androidApi -and $Result.testKind -ceq 'ui' -and
            $Result.status -cin @('candidate-failed', 'not-reproduced-on-tested-revision') -and
            $Result.nativeAndroidApi -cne $Result.androidApi) {
            throw 'A qualified Android UI outcome must execute on the requested API level.'
        }
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
    if ($null -ne $Result.confirmationTestExecuted -and
        ($Result.confirmationTestExecuted -isnot [bool] -or $Result.attempt -ne 2 -or
        $Result.testExecuted -ne $true -or $Result.observedAssertion -ne $true -or
        $Result.status -cnotin @('inconclusive', 'candidate-failed') -or
        ($Result.status -eq 'candidate-failed' -and -not $Result.confirmationTestExecuted))) {
        throw 'Fresh confirmation execution must match a second-attempt aggregate with observed assertion evidence.'
    }
    if ($Result.status -eq 'not-reproduced-on-tested-revision' -and
        ($Result.testExecuted -ne $true -or $Result.assertionFailed -ne $false -or
        $Result.sampleBuilt -ne $true)) {
        throw 'A passing verdict needs an executed test and a built sample.'
    }
    return $true
}

function New-IssueReplicateReportHeader {
    param(
        [Parameter(Mandatory)][string]$Marker,
        [string]$Heading = 'Issue reproduction',
        [string]$TargetSha = '',
        [string]$Verdict = ''
    )

    $attribution = '> AI-generated issue replication attempt. Review the generated test and evidence before relying on the result.'
    $commitBadge = ''
    if ($TargetSha) {
        if ($TargetSha -cnotmatch '^[0-9a-f]{40}$') {
            throw 'The report header requires a validated MAUI target revision.'
        }
        $shortSha = $TargetSha.Substring(0, 7)
        $attribution = "> AI-generated issue replication attempt targeting .NET MAUI commit [``$shortSha``](https://github.com/dotnet/maui/commit/$TargetSha). Review the generated test and evidence before relying on the result."
        $commitBadge = "  <img alt=`"Commit $shortSha`" src=`"https://img.shields.io/badge/Commit-$shortSha-1f6feb?labelColor=30363d&style=flat-square`">"
    }
    return @(
        $Marker,
        "## $Heading",
        '',
        '**AI-generated issue replication attempt.**',
        '',
        $Verdict,
        '',
        '<details>',
        '<summary>Report provenance</summary>',
        '',
        $attribution,
        '',
        '<p align="left">',
        '  <img alt="Scope Issue replication" src="https://img.shields.io/badge/Scope-Issue%20replication-1f6feb?labelColor=30363d&style=flat-square">',
        $commitBadge,
        '</p>',
        '',
        '</details>'
    ) -join "`n"
}

function Get-IssueReplicateReportComments {
    param([Parameter(Mandatory)][int]$IssueNumber)

    $query = 'query($issue:Int!,$endCursor:String){repository(owner:"dotnet",name:"maui"){issue(number:$issue){comments(first:100,after:$endCursor){pageInfo{hasNextPage endCursor}nodes{databaseId id body isMinimized author{login}}}}}}'
    $rows = @(gh api graphql --paginate -f query="$query" -F issue="$IssueNumber" `
        --jq '.data.repository.issue.comments.nodes[] | {id:.databaseId,node_id:.id,body,isMinimized,user:{login:.author.login}} | @json')
    if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate issue report publication state.' }
    return @($rows | ForEach-Object { $_ | ConvertFrom-Json -Depth 5 })
}

function Test-IssueReplicateCompletedReport {
    param([string]$Body)

    $complete = $Body.TrimEnd().EndsWith('<!-- issue-replicate-report-complete -->', [StringComparison]::Ordinal)
    if ($Body -cmatch '\A<!-- issue-replicate-independent-report -->\r?\n') {
        return $complete
    }
    if ($Body -cnotmatch '\A<!-- issue-replicate-result:(?:[1-9][0-9]*|github:(?:dotnet|kubaflo)/maui:[1-9][0-9]*) -->\r?\n## (?:Issue reproduction|Issue Reproduction Analysis)\r?\n') {
        return $false
    }
    if ($complete) { return $true }
    return $Body -notmatch 'Media publication is incomplete|Candidate publication is incomplete'
}

function Complete-IssueReplicateReportPublication {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][string]$Url,
        [Parameter(Mandatory)][string]$Body
    )

    if ($Url -cnotmatch "^https://github\.com/dotnet/maui/issues/$IssueNumber#issuecomment-([1-9][0-9]*)$") {
        throw 'The current report URL does not belong to the requested issue.'
    }
    $currentId = [long]$Matches[1]
    $identity = gh api user --jq .login
    if ($LASTEXITCODE -ne 0 -or "$identity" -cnotmatch '^[A-Za-z0-9][A-Za-z0-9_\[\]-]{0,99}$') {
        throw 'Could not identify the report reconciliation publisher.'
    }
    $trustedAuthors = @([string]$identity, 'kubaflo', 'MauiBot', 'maui-bot', 'maui-bot[bot]', 'github-actions[bot]')
    $comments = @(Get-IssueReplicateReportComments -IssueNumber $IssueNumber)
    $current = @($comments | Where-Object { $_.id -eq $currentId -and $_.user.login -ieq $identity })
    if ($current.Count -ne 1 -or $current[0].body -cne $Body -or
        -not (Test-IssueReplicateCompletedReport -Body $Body)) {
        throw 'The completed owned report could not be verified; earlier reports were preserved.'
    }
    if ($current[0].isMinimized -ne $false) {
        throw 'This report is already hidden; do not reconcile or resurrect superseded evidence.'
    }
    $reports = @($comments | Where-Object {
        $_.user.login -iin $trustedAuthors -and $_.isMinimized -eq $false -and
        (Test-IssueReplicateCompletedReport -Body ([string]$_.body))
    } | Sort-Object { [long]$_.id } -Descending)
    if ($reports.Count -eq 0) { throw 'No visible completed report is available; reconciliation stopped.' }
    $winner = $reports[0]
    foreach ($older in @($reports | Select-Object -Skip 1)) {
        $check = 'query($winner:ID!,$older:ID!){winner:node(id:$winner){... on IssueComment{databaseId body isMinimized author{login} issue{number repository{nameWithOwner}}}}older:node(id:$older){... on IssueComment{databaseId body isMinimized author{login} issue{number repository{nameWithOwner}}}}}'
        $rows = @(gh api graphql -f query="$check" -f winner="$($winner.node_id)" -f older="$($older.node_id)" `
            --jq '[.data.winner,.data.older][] | {id:.databaseId,body,isMinimized,user:{login:.author.login},issue:.issue.number,repository:.issue.repository.nameWithOwner} | @json')
        if ($LASTEXITCODE -ne 0) { throw 'Could not recheck the replacement and superseded report.' }
        $fresh = @($rows | ForEach-Object { $_ | ConvertFrom-Json -Depth 5 })
        if ($fresh.Count -ne 2 -or @($fresh | Where-Object {
            $_.issue -ne $IssueNumber -or $_.repository -cne 'dotnet/maui'
        }).Count -ne 0) {
            throw 'The report recheck did not match the requested issue and repository.'
        }
        $visibleWinner = @($fresh | Where-Object {
            $_.id -eq $winner.id -and $_.isMinimized -eq $false -and
            $_.user.login -ieq $winner.user.login -and $_.body -ceq $winner.body
        })
        $unchangedOlder = @($fresh | Where-Object {
            $_.id -eq $older.id -and $_.isMinimized -eq $false -and
            $_.user.login -ieq $older.user.login -and $_.body -ceq $older.body
        })
        if ($visibleWinner.Count -ne 1) {
            throw 'The replacement report changed during reconciliation; remaining evidence was preserved.'
        }
        if ($unchangedOlder.Count -ne 1) {
            throw 'An older report changed during reconciliation; remaining evidence was preserved.'
        }
        if ($PSCmdlet.ShouldProcess("Issue $IssueNumber comment $($older.id)", 'Minimize superseded report as OUTDATED; retain all evidence')) {
            $minimize = 'mutation($id:ID!){minimizeComment(input:{subjectId:$id,classifier:OUTDATED}){minimizedComment{isMinimized}}}'
            $hidden = gh api graphql -f query="$minimize" -f id="$($older.node_id)" `
                --jq '.data.minimizeComment.minimizedComment.isMinimized'
            if ($LASTEXITCODE -ne 0 -or "$hidden" -cne 'true') {
                throw 'Could not minimize a superseded report; the current report and older evidence were preserved.'
            }
        }
    }
    if ($current[0].isMinimized -ne $false -or $winner.id -ne $currentId) {
        throw 'This attempt was superseded by a newer completed report; its evidence was preserved.'
    }
    if ($WhatIfPreference) { return $Url }
    $remaining = @(Get-IssueReplicateReportComments -IssueNumber $IssueNumber | Where-Object {
        $_.user.login -iin $trustedAuthors -and $_.isMinimized -eq $false -and
        (Test-IssueReplicateCompletedReport -Body ([string]$_.body))
    })
    if ($remaining.Count -ne 1 -or $remaining[0].id -ne $currentId -or $remaining[0].body -cne $Body) {
        throw 'Report state changed during final reconciliation; publication did not establish one current report.'
    }
    return $Url
}

function Set-IssueReplicateResultComment {
    param(
        [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][string]$Marker,
        [Parameter(Mandatory)][string]$Body
    )

    if ([Text.Encoding]::UTF8.GetByteCount($Body) -gt 60000 -or -not $Body.StartsWith($Marker, [StringComparison]::Ordinal)) {
        throw 'The reproduction comment exceeds its publication bound or does not match its ownership marker.'
    }
    $identity = gh api user --jq .login
    if ($LASTEXITCODE -ne 0 -or [string]$identity -cnotmatch '^[A-Za-z0-9][A-Za-z0-9_\[\]-]{0,99}$') {
        throw 'Could not identify the authenticated comment publisher.'
    }
    $existing = @(gh api --paginate "repos/dotnet/maui/issues/$IssueNumber/comments?per_page=100" `
        --jq ".[] | select(.user.login == `"$identity`" and (.body | startswith(`"$Marker`"))) | .id")
    if ($LASTEXITCODE -ne 0) { throw 'Could not check for a prior result comment.' }
    if ($existing.Count -gt 1) { throw 'Multiple result comments exist for the same run.' }
    $payload = @{ body = $Body } | ConvertTo-Json -Compress
    if ($existing.Count -eq 1) {
        $state = @(Get-IssueReplicateReportComments -IssueNumber $IssueNumber | Where-Object {
            $_.id -eq $existing[0] -and $_.user.login -ieq $identity -and $_.body.StartsWith($Marker, [StringComparison]::Ordinal)
        })
        if ($state.Count -ne 1 -or $state[0].isMinimized -ne $false) {
            throw 'The owned report or continuation is hidden or changed; do not resurrect superseded evidence.'
        }
        $url = $payload | gh api "repos/dotnet/maui/issues/comments/$($existing[0])" --method PATCH --input - --jq .html_url
    } else {
        $url = $payload | gh api "repos/dotnet/maui/issues/$IssueNumber/comments" --method POST --input - --jq .html_url
    }
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace([string]$url) -or
        [string]$url -cnotmatch "^https://github\.com/dotnet/maui/issues/$IssueNumber#issuecomment-[1-9][0-9]*$") {
        throw 'Could not post the issue reproduction comment.'
    }
    return $url
}
