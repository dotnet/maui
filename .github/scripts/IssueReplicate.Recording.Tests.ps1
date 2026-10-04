#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'IssueReplicate.Recording.ps1')
    . (Join-Path $PSScriptRoot 'IssueReplicate.Diagnostics.ps1')

    function New-RecordingFixture {
        param([int]$Length = 24)
        $bytes = [byte[]]::new($Length)
        [Random]::new(42).NextBytes($bytes)
        [Array]::Copy([Text.Encoding]::ASCII.GetBytes('ftypisom'), 0, $bytes, 4, 8)
        $recording = @{
            status = 'available'; bytes = $Length; diagnostic = ''
            sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        }
        return @{ Bytes = $bytes; Recording = $recording }
    }

    function Set-RecordingFixtureEnvironment {
        param($Fixture)
        $chunks = @(Export-IssueReplicateRecording -Bytes $Fixture.Bytes -Recording $Fixture.Recording -Provider None)
        for ($index = 0; $index -lt $chunks.Count; $index++) {
            [Environment]::SetEnvironmentVariable("REPRO_VIDEO_$index", $chunks[$index])
        }
    }

    function New-RecordingPostFixture {
        param($Recording)
        $root = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        $inputDirectory = Join-Path $root 'input'
        $resultDirectory = Join-Path $root 'result'
        New-Item -ItemType Directory -Path $inputDirectory, $resultDirectory | Out-Null
        @{
            issueNumber = 12345; commentId = 4925414214; targetSha = 'a' * 40
            sampleSha256 = 'b' * 64; platform = 'android'; sourceType = 'attachment'
        } | ConvertTo-Json | Set-Content (Join-Path $inputDirectory 'manifest.json')
        @{
            schemaVersion = 1; issueNumber = 12345; commentId = 4925414214; targetSha = 'a' * 40
            sampleSha256 = 'b' * 64; platform = 'android'; testKind = 'ui'; attempt = 1
            sampleBuilt = $true; testExecuted = $true; assertionFailed = $false
            status = 'not-reproduced-on-tested-revision'; recording = $Recording
        } | ConvertTo-Json | Set-Content (Join-Path $resultDirectory 'result.json')
        return @{
            IssueNumber = 12345; CommentId = 4925414214; BuildId = 456789
            InputDirectory = $inputDirectory; ResultsDirectory = $resultDirectory
        }
    }
}

Describe 'Native recording helpers' {
    BeforeEach {
        $savedEnvironment = @{}
        for ($index = 0; $index -lt 8; $index++) {
            $name = "REPRO_VIDEO_$index"
            $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, $null)
        }
    }

    AfterEach {
        foreach ($name in $savedEnvironment.Keys) {
            [Environment]::SetEnvironmentVariable($name, $savedEnvironment[$name])
        }
    }

    Describe 'Bounded native recording transport' {
        It 'round-trips exact MP4 bytes across a chunk boundary (<Length>)' -ForEach @(
            @{ Length = 24 }, @{ Length = 65535 }, @{ Length = 65536 },
            @{ Length = 65537 }, @{ Length = 524288 }
        ) {
            $fixture = New-RecordingFixture -Length $Length
            Set-RecordingFixtureEnvironment $fixture
            $actual = Import-IssueReplicateRecording -Recording $fixture.Recording -Prefix REPRO_VIDEO_
            [Convert]::ToBase64String($actual) | Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
            for ($index = 0; $index -lt 8; $index++) {
                ([Environment]::GetEnvironmentVariable("REPRO_VIDEO_$index")).Length | Should -BeLessOrEqual 87384
            }
        }

        It 'rejects missing, malformed, corrupt and surplus fragments' -ForEach @(
            @{ Fault = 'missing' }, @{ Fault = 'malformed' },
            @{ Fault = 'corrupt' }, @{ Fault = 'surplus' }
        ) {
            $fixture = New-RecordingFixture -Length 65537
            Set-RecordingFixtureEnvironment $fixture
            switch ($Fault) {
                missing { $env:REPRO_VIDEO_1 = $null }
                malformed { $env:REPRO_VIDEO_0 = '*' * $env:REPRO_VIDEO_0.Length }
                corrupt {
                    $bytes = [Convert]::FromBase64String($env:REPRO_VIDEO_0)
                    $bytes[20] = $bytes[20] -bxor 1
                    $env:REPRO_VIDEO_0 = [Convert]::ToBase64String($bytes)
                }
                surplus { $env:REPRO_VIDEO_2 = 'AAAA' }
            }
            { Import-IssueReplicateRecording -Recording $fixture.Recording -Prefix REPRO_VIDEO_ } | Should -Throw
        }

        It 'rejects an invalid descriptor without allocating its claimed size' -ForEach @(
            @{ Field = 'bytes'; Value = 524289 }, @{ Field = 'bytes'; Value = '24' },
            @{ Field = 'bytes'; Value = 0 }, @{ Field = 'sha256'; Value = 'bad' },
            @{ Field = 'status'; Value = 'capturing' }, @{ Field = 'diagnostic'; Value = ('x' * 1001) }
        ) {
            $fixture = New-RecordingFixture
            $fixture.Recording[$Field] = $Value
            { Import-IssueReplicateRecording -Recording $fixture.Recording -Prefix REPRO_VIDEO_ } | Should -Throw
        }

        It 'requires a reason when recording did not complete' {
            { Assert-IssueReplicateRecording @{ status = 'failed'; diagnostic = '' } } | Should -Throw '*diagnostic*'
            { Assert-IssueReplicateRecording @{ status = 'not-started'; diagnostic = 'Fixture never started.' } } |
            Should -Not -Throw
        }

        It 'rejects a mismatched completed hash before exporting any media' {
            $fixture = New-RecordingFixture
            $fixture.Bytes[20] = $fixture.Bytes[20] -bxor 1
            { Export-IssueReplicateRecording -Bytes $fixture.Bytes -Recording $fixture.Recording -Provider None } |
            Should -Throw '*completed result*'
        }

        It 'rejects non-MP4 and oversized data' {
            { Assert-IssueReplicateRecordingBytes -Bytes ([byte[]]::new(24)) } | Should -Throw
            { Assert-IssueReplicateRecordingBytes -Bytes ((New-RecordingFixture -Length 524289).Bytes) } | Should -Throw
        }

        It 'does not export media beyond the smaller GitHub job-output budget' {
            $fixture = New-RecordingFixture -Length 262145
            { Export-IssueReplicateRecording -Bytes $fixture.Bytes -Recording $fixture.Recording -Provider GitHub } |
            Should -Throw '*GitHub job-output*'
        }

        It 'does not attach native footage to a non-UI result' {
            { Assert-IssueReplicateResultRecording @{ testKind = 'unit'; recording = (New-RecordingFixture).Recording } } |
            Should -Throw '*Only a UI result*'
        }
    }

    Describe 'Native Appium recording control' {
        It 'rejects UI lifecycle/reset hooks before recording (<Hook>)' -ForEach @(
            @{ Hook = 'protected override bool ResetAfterEachTest => true;' },
            @{ Hook = 'protected override bool @ResetAfterEachTest => true;' },
            @{ Hook = 'protected override bool ResetAfter\u0045achTest => true;' },
            @{ Hook = 'protected override bool ResetAfter\U00000045achTest => true;' },
            @{ Hook = 'protected override bool \u0052esetAfterEachTest => true;' },
            @{ Hook = 'protected override bool ResetAfterEach\u200BTest => true;' },
            @{ Hook = 'protected override void FixtureSetup() { base.FixtureSetup(); }' },
            @{ Hook = 'public override void TestSetup() { base.TestSetup(); }' },
            @{ Hook = '[NUnit.Framework.SetUp] public void ChangeSession() { }' },
            @{ Hook = '[SetUpAttribute] public void ChangeSession() { }' },
            @{ Hook = '[OneTimeSetUp] public void ChangeSession() { }' },
            @{ Hook = '[TearDown] public void ChangeSession() { }' },
            @{ Hook = 'public void ChangeSession() { Reset(); }' },
            @{ Hook = '// ResetAfterEachTest must not be enabled here.' }
        ) {
            . (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
            $candidate = @{
                kind = 'ui'; files = @(
                    @{
                        path = 'src/Controls/tests/TestCases.HostApp/Issues/Issue12345.cs'
                        content = "#if ANDROID`nclass Issue12345 { }`n#endif"
                    },
                    @{
                        path = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue12345.cs'
                        content = "#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST`nclass Issue12345 {`n$Hook`n}`n#endif"
                    }
                )
            }
            { Assert-IssueReplicateCandidate -Candidate $candidate -IssueNumber 12345 -Platform android } |
            Should -Throw '*default fixture lifecycle*'
        }

        It 'binds recording to one test after fixture retries (<Fault>, test started=<TestStarted>, assertion=<Assertion>, recovery=<Recovery>)' -ForEach @(
            @{ Fault = ''; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = ''; TestStarted = $false; Assertion = $false; Recovery = '' },
            @{ Fault = ''; TestStarted = $true; Assertion = $true; Recovery = '' },
            @{ Fault = ''; TestStarted = $true; Assertion = $false; Recovery = 'new session' },
            @{ Fault = ''; TestStarted = $true; Assertion = $false; Recovery = 'same session' },
            @{ Fault = 'restart failed'; TestStarted = $true; Assertion = $false; Recovery = 'new session' },
            @{ Fault = 'later failure'; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = 'extra result'; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = 'repeated case'; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = 'wrong method'; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = 'wrong stop'; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = 'missing stop'; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = 'stale TRX'; TestStarted = $true; Assertion = $false; Recovery = '' },
            @{ Fault = 'missing TRX'; TestStarted = $true; Assertion = $false; Recovery = '' }
        ) {
            . (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
            $fixture = New-RecordingFixture
            $parameters = New-RecordingPostFixture $fixture.Recording
            $repo = Join-Path $parameters.InputDirectory 'repo'
            $project = Join-Path $repo 'src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj'
            New-Item -ItemType Directory -Path (Split-Path $project) -Force | Out-Null
            Set-Content $project '<Project />'
            & git -C $repo init -q
            & git -C $repo add .
            & git -C $repo -c user.name=Fixture -c user.email=fixture@example.invalid commit -q -m Fixture
            $LASTEXITCODE | Should -Be 0
            $revision = (& git -C $repo rev-parse HEAD).Trim()
            $manifestPath = Join-Path $parameters.InputDirectory 'manifest.json'
            $manifest = Get-Content -Raw $manifestPath | ConvertFrom-Json
            $manifest.targetSha = $revision
            $manifest | Add-Member -NotePropertyName schemaVersion -NotePropertyValue 1
            $manifest | ConvertTo-Json | Set-Content $manifestPath
            $samplePath = Join-Path $parameters.InputDirectory 'sample-result.json'
            @{ targetSha = $revision; sampleSha256 = $manifest.sampleSha256; buildSucceeded = $true } |
            ConvertTo-Json | Set-Content $samplePath
            $candidatePath = Join-Path $parameters.InputDirectory 'candidate.json'
            @{
                kind = 'ui'; files = @(
                    @{
                        path = 'src/Controls/tests/TestCases.HostApp/Issues/Issue12345.cs'
                        content = "#if ANDROID`nclass Issue12345 { }`n#endif"
                    },
                    @{
                        path = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue12345.cs'
                        content = "#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST`nclass Issue12345 { }`n#endif"
                    }
                )
            } | ConvertTo-Json -Depth 5 | Set-Content $candidatePath
            $global:recordingRetryLog = Join-Path $repo 'CustomAgentLogsTmp/UITests/appium.log'
            New-Item -ItemType Directory -Path (Split-Path $global:recordingRetryLog) -Force | Out-Null
            $global:recordingRetryTestStarted = $TestStarted
            $global:recordingRetryFault = $Fault
            $global:recordingRetryAssertion = [bool]$Assertion
            $global:recordingRetryRecovery = $Recovery
            $global:recordingRetryCalls = [Collections.Generic.List[string]]::new()
            Mock Invoke-IssueReplicateRecordingRequest {
                $global:recordingRetryCalls.Add($Path)
                if ($Path -like 'session/aaaaaaaa-*') { throw 'The first fixture session was discarded.' }
                if ($global:recordingRetryFault -eq 'restart failed' -and $Path -like 'session/cccccccc-*') {
                    throw 'The recreated session rejected recording startup.'
                }
                if ($Path.EndsWith('/stop_recording_screen')) {
                    return [Convert]::ToBase64String($fixture.Bytes)
                }
                return ''
            }
            function pwsh {
                [IO.File]::WriteAllText($global:recordingRetryLog,
                    '[AppiumDriver] New AndroidDriver session created successfully, session aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa added')
                '>>>>> 10/04/2026 12:00:00 FixtureSetup for ChecksBehavior'
                '>>>>> 10/04/2026 12:00:00 Detected instrumentation crash, attempting session recreation...'
                [IO.File]::AppendAllText($global:recordingRetryLog,
                    "`n[AppiumDriver] New AndroidDriver session created successfully, session bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb added")
                '>>>>> 10/04/2026 12:00:01 FixtureSetup for ChecksBehavior'
                if ($global:recordingRetryTestStarted) {
                    '>>>>> 10/04/2026 12:00:02 ChecksBehavior Start'
                    if ($global:recordingRetryRecovery) {
                        if ($global:recordingRetryRecovery -eq 'new session') {
                            [IO.File]::AppendAllText($global:recordingRetryLog,
                                "`n[AppiumDriver] New AndroidDriver session created successfully, session cccccccc-cccc-cccc-cccc-cccccccccccc added")
                        }
                        '>>>>> 10/04/2026 12:00:02 Session recreation successful in TestSetup'
                    }
                    if ($global:recordingRetryFault -eq 'wrong stop') {
                        '>>>>> 10/04/2026 12:00:03 OtherBehavior Stop'
                    } elseif ($global:recordingRetryFault -ne 'missing stop') {
                        '>>>>> 10/04/2026 12:00:03 ChecksBehavior Stop'
                    }
                    if ($global:recordingRetryFault -eq 'later failure') {
                        '>>>>> 10/04/2026 12:00:04 OtherBehavior Start'
                        '>>>>> 10/04/2026 12:00:05 OtherBehavior Stop'
                    } elseif ($global:recordingRetryFault -eq 'repeated case') {
                        '>>>>> 10/04/2026 12:00:04 ChecksBehavior Start'
                        '>>>>> 10/04/2026 12:00:05 ChecksBehavior Stop'
                    }
                }
                $trx = Join-Path (Split-Path $global:recordingRetryLog) 'TestResults/fixture.trx'
                New-Item -ItemType Directory -Path (Split-Path $trx) -Force | Out-Null
                $outcome = if ($global:recordingRetryTestStarted) { 'Passed' } else { 'NotExecuted' }
                $executed = if ($global:recordingRetryTestStarted) { 1 } else { 0 }
                $method = if ($global:recordingRetryFault -eq 'wrong method') { 'OtherBehavior' } else { 'ChecksBehavior' }
                $xml = @"
<TestRun>
  <TestDefinitions><UnitTest id="test-1"><TestMethod className="Example.Issue12345" name="$method" /></UnitTest></TestDefinitions>
  <Results><UnitTestResult testId="test-1" testName="$method" outcome="$outcome" /></Results>
  <ResultSummary outcome="$outcome"><Counters total="1" executed="$executed" passed="$executed" failed="0" /></ResultSummary>
</TestRun>
"@
                $global:LASTEXITCODE = 0
                if ($global:recordingRetryAssertion) {
                    $xml = $xml.Replace('outcome="Passed"', 'outcome="Failed"').
                    Replace('passed="1" failed="0"', 'passed="0" failed="1"').
                    Replace('<UnitTestResult testId="test-1" testName="ChecksBehavior" outcome="Failed" />', @'
<UnitTestResult testId="test-1" testName="ChecksBehavior" outcome="Failed">
  <Output><ErrorInfo><Message>NUnit.Framework.AssertionException: Expected: 1 But was: 0</Message>
  <StackTrace>at Example.Issue12345.ChecksBehavior() in /test/Issue12345.cs:line 12</StackTrace></ErrorInfo></Output>
</UnitTestResult>
'@)
                    $global:LASTEXITCODE = 1
                }
                if ($global:recordingRetryFault -in @('later failure', 'extra result', 'repeated case')) {
                    $otherMethod = if ($global:recordingRetryFault -eq 'repeated case') { 'ChecksBehavior' } else { 'OtherBehavior' }
                    $xml = $xml.Replace('</TestDefinitions>',
                        "<UnitTest id=`"test-2`"><TestMethod className=`"Example.Issue12345`" name=`"$otherMethod`" /></UnitTest></TestDefinitions>")
                    $xml = $xml.Replace('</Results>', @"
<UnitTestResult testId="test-2" testName="$otherMethod(2)" outcome="Failed">
  <Output><ErrorInfo><Message>NUnit.Framework.AssertionException: Expected: 1 But was: 0</Message>
  <StackTrace>at Example.Issue12345.$otherMethod() in /test/Issue12345.cs:line 12</StackTrace></ErrorInfo></Output>
</UnitTestResult></Results>
"@).Replace('total="1" executed="1" passed="1" failed="0"', 'total="2" executed="2" passed="1" failed="1"')
                    $global:LASTEXITCODE = 1
                }
                [IO.File]::WriteAllText($trx, $xml)
                $timestamp = if ($global:recordingRetryFault -eq 'stale TRX') {
                    [DateTime]::UtcNow.AddMinutes(-1)
                } else { [DateTime]::UtcNow.AddSeconds(1) }
                [IO.File]::SetLastWriteTimeUtc($trx, $timestamp)
                if ($global:recordingRetryFault -eq 'missing TRX') { Remove-Item -LiteralPath $trx }
                ">>> TRX_RESULT_FILE: $trx"
            }
            try {
                & (Join-Path $PSScriptRoot 'IssueReplicate.Verify.ps1') -ManifestPath $manifestPath `
                    -SampleResultPath $samplePath -CandidatePath $candidatePath -RepoRoot $repo `
                    -OutputDirectory $parameters.ResultsDirectory -RecordVideo -CoreLoaded
                $result = Get-Content -Raw (Join-Path $parameters.ResultsDirectory 'result.json') | ConvertFrom-Json
                if ($Fault) {
                    $result.status | Should -BeExactly 'inconclusive'
                    $result.assertionFailed | Should -BeFalse
                    $result.recording.status | Should -BeExactly 'failed'
                    $result.recording.diagnostic | Should -Not -BeNullOrEmpty
                    $result.observedAssertion | Should -BeFalse
                    Test-Path (Join-Path $parameters.ResultsDirectory 'recording.mp4') | Should -BeFalse
                    Test-Path (Join-Path $parameters.ResultsDirectory 'test.patch') | Should -BeFalse
                } elseif ($TestStarted) {
                    $result.testExecuted | Should -BeTrue
                    $result.recording.status | Should -BeExactly 'available'
                    $global:recordingRetryCalls[0] |
                    Should -BeExactly 'session/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/appium/start_recording_screen'
                    if ($Recovery -eq 'new session') {
                        $global:recordingRetryCalls.Count | Should -Be 3
                        $global:recordingRetryCalls[1] |
                        Should -BeExactly 'session/cccccccc-cccc-cccc-cccc-cccccccccccc/appium/start_recording_screen'
                        $global:recordingRetryCalls[2] |
                        Should -BeExactly 'session/cccccccc-cccc-cccc-cccc-cccccccccccc/appium/stop_recording_screen'
                    } else {
                        $global:recordingRetryCalls.Count | Should -Be 2
                        $global:recordingRetryCalls[1] |
                        Should -BeExactly 'session/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/appium/stop_recording_screen'
                    }
                    [Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $parameters.ResultsDirectory 'recording.mp4'))) |
                    Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
                    if ($Assertion) {
                        $result.observedAssertion | Should -BeTrue
                        $secondRepo = "$repo-second"
                        & git clone --quiet --no-local $repo $secondRepo
                        $LASTEXITCODE | Should -Be 0
                        $global:recordingRetryLog = Join-Path $secondRepo 'CustomAgentLogsTmp/UITests/appium.log'
                        New-Item -ItemType Directory -Path (Split-Path $global:recordingRetryLog) -Force | Out-Null
                        $confirmation = Join-Path $parameters.InputDirectory 'confirmation'
                        & (Join-Path $PSScriptRoot 'IssueReplicate.Verify.ps1') -ManifestPath $manifestPath `
                            -SampleResultPath $samplePath -CandidatePath $candidatePath -RepoRoot $secondRepo `
                            -OutputDirectory $confirmation -RecordVideo -CoreLoaded -Attempt 2 `
                            -PreviousResultPath (Join-Path $parameters.ResultsDirectory 'result.json')
                        $confirmed = Get-Content -Raw (Join-Path $confirmation 'result.json') | ConvertFrom-Json
                        $confirmed.status | Should -BeExactly 'candidate-failed'
                        $confirmed.recording.status | Should -BeExactly 'available'
                        $confirmed.assertionFailed | Should -BeTrue
                        [IO.File]::ReadAllBytes((Join-Path $confirmation 'recording.mp4')).Length |
                        Should -Be $fixture.Bytes.Length
                        Test-Path (Join-Path $confirmation 'test.patch') | Should -BeTrue
                    }
                } else {
                    $result.testExecuted | Should -BeFalse
                    $result.recording.status | Should -BeExactly 'not-started'
                    $global:recordingRetryCalls.Count | Should -Be 0
                    Test-Path (Join-Path $parameters.ResultsDirectory 'recording.mp4') | Should -BeFalse
                }
            } finally {
                Remove-Variable recordingRetryLog, recordingRetryTestStarted, recordingRetryFault, recordingRetryAssertion,
                    recordingRetryRecovery, recordingRetryCalls -Scope Global
            }
        }

        It 'uses the latest bounded session reference without enabling session discovery' {
            $log = Join-Path $TestDrive 'sessions.log'
            [IO.File]::WriteAllText($log,
                "[HTTP] --> POST /wd/hub/session/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/element`n" +
                '[AppiumDriver] New XCUITestDriver session created successfully, session bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb added')
            Mock Invoke-IssueReplicateRecordingRequest { '' }
            Start-IssueReplicateRecording -Platform ios -LogPath $log |
            Should -BeExactly 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'
            Should -Invoke Invoke-IssueReplicateRecordingRequest -Times 1 -Exactly -ParameterFilter {
                $Method -eq 'POST' -and $Path -eq 'session/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/appium/start_recording_screen' -and
                $Body.options.timeLimit -eq 30 -and $Body.options.videoType -eq 'libx264'
            }
        }

        It 'fails explicitly when no live session reference is present' {
            $log = Join-Path $TestDrive 'no-session.log'
            [IO.File]::WriteAllText($log, 'WDA startup failed')
            { Start-IssueReplicateRecording -Platform android -LogPath $log } | Should -Throw '*session identifier*'
        }

        It 'decodes a bounded stop response without losing binary bytes' {
            $fixture = New-RecordingFixture
            Mock Invoke-IssueReplicateRecordingRequest { [Convert]::ToBase64String($fixture.Bytes) }
            [Convert]::ToBase64String((Stop-IssueReplicateRecording -SessionId 'session-id')) |
            Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
        }

        It 'never addresses a non-loopback server or arbitrary endpoint' {
            { Invoke-IssueReplicateRecordingRequest -Method POST -Path 'sessions' } | Should -Throw '*endpoint*'
            { Invoke-IssueReplicateRecordingRequest -Method POST `
                    -Path 'session/id/appium/start_recording_screen' -ServerUri 'https://example.com/wd/hub/' } |
            Should -Throw '*loopback*'
        }
    }

    Describe 'Bounded HTTP media responses' {
        It 'sends a real loopback POST without session discovery or authentication' {
            $python = Get-Command python3 -CommandType Application -ErrorAction Stop | Select-Object -First 1
            $code = @'
import json,socket,sys
with socket.socket() as server:
    server.bind(("127.0.0.1",0)); server.listen(1); server.settimeout(5)
    print(server.getsockname()[1],flush=True)
    with server.accept()[0] as client:
        client.settimeout(5); data=b""
        while b"\r\n\r\n" not in data: data+=client.recv(4096)
        header,body=data.split(b"\r\n\r\n",1)
        size=int(next(line.split(b":",1)[1] for line in header.split(b"\r\n") if line.lower().startswith(b"content-length:")))
        while len(body)<size: body+=client.recv(4096)
        print(json.dumps({"header":header.decode(),"body":json.loads(body)}),flush=True)
        reply=b'{"value":""}'
        client.sendall(b"HTTP/1.1 200 OK\r\nContent-Length: 12\r\nConnection: close\r\n\r\n"+reply)
'@
            $start = [Diagnostics.ProcessStartInfo]::new($python.Source)
            $start.UseShellExecute = $false
            $start.RedirectStandardOutput = $true
            foreach ($argument in @('-I', '-S', '-c', $code)) { $start.ArgumentList.Add($argument) }
            $process = [Diagnostics.Process]::new()
            $process.StartInfo = $start
            $started = $false
            try {
                $started = $process.Start()
                $started | Should -BeTrue
                $portRead = $process.StandardOutput.ReadLineAsync()
                if (-not $portRead.Wait(5000)) { throw 'The HTTP fixture did not become ready.' }
                $port = [int]$portRead.Result
                Invoke-IssueReplicateRecordingRequest -Method POST `
                    -Path 'session/id/appium/start_recording_screen' -Body @{ options = @{ timeLimit = 30 } } `
                    -ServerUri "http://127.0.0.1:$port/wd/hub/" | Should -BeExactly ''
                if (-not $process.WaitForExit(5000)) { throw 'The HTTP fixture did not complete.' }
                $process.ExitCode | Should -Be 0
                $request = $process.StandardOutput.ReadToEnd() | ConvertFrom-Json
                $request.header | Should -Match '^POST /wd/hub/session/id/appium/start_recording_screen HTTP/1.1'
                $request.header | Should -Not -Match 'Authorization'
                $request.body.options.timeLimit | Should -Be 30
            } finally {
                if ($started -and -not $process.HasExited) { $process.Kill($true) }
                $process.Dispose()
            }
        }

        It 'reads strict UTF-8 within the declared bound' {
            $response = [Net.Http.HttpResponseMessage]::new([Net.HttpStatusCode]::OK)
            $response.Content = [Net.Http.StringContent]::new('{"value":""}')
            try {
                Read-IssueReplicateHttpResponse -Response $response -MaxBytes 32 `
                    -CancellationToken ([Threading.CancellationToken]::None) | Should -BeExactly '{"value":""}'
            } finally { $response.Dispose() }
        }

        It 'rejects an oversized response before reading its body' {
            $response = [Net.Http.HttpResponseMessage]::new([Net.HttpStatusCode]::OK)
            $response.Content = [Net.Http.StringContent]::new('x' * 100)
            try {
                { Read-IssueReplicateHttpResponse -Response $response -MaxBytes 32 `
                        -CancellationToken ([Threading.CancellationToken]::None) } | Should -Throw '*bound*'
            } finally { $response.Dispose() }
        }

        It 'bounds a streamed response without Content-Length' {
            $compressed = [IO.MemoryStream]::new()
            $writer = [IO.Compression.DeflateStream]::new($compressed, [IO.Compression.CompressionMode]::Compress, $true)
            $writer.Write([Text.Encoding]::UTF8.GetBytes('x' * 100))
            $writer.Dispose()
            $compressed.Position = 0
            $reader = [IO.Compression.DeflateStream]::new($compressed, [IO.Compression.CompressionMode]::Decompress)
            $response = [Net.Http.HttpResponseMessage]::new([Net.HttpStatusCode]::OK)
            $response.Content = [Net.Http.StreamContent]::new($reader)
            try {
                $response.Content.Headers.ContentLength | Should -BeNullOrEmpty
                { Read-IssueReplicateHttpResponse -Response $response -MaxBytes 32 `
                        -CancellationToken ([Threading.CancellationToken]::None) } | Should -Throw '*bound*'
            } finally { $response.Dispose(); $compressed.Dispose() }
        }

        It 'honors a cancelled response-read deadline' {
            $response = [Net.Http.HttpResponseMessage]::new([Net.HttpStatusCode]::OK)
            $response.Content = [Net.Http.StringContent]::new('{"value":""}')
            $deadline = [Threading.CancellationTokenSource]::new()
            $deadline.Cancel()
            try {
                { Read-IssueReplicateHttpResponse -Response $response -MaxBytes 32 `
                        -CancellationToken $deadline.Token } | Should -Throw
            } finally { $response.Dispose(); $deadline.Dispose() }
        }

        It 'rejects HTTP errors and invalid UTF-8' -ForEach @(
            @{ Status = 403; Bytes = [byte[]](65) }, @{ Status = 200; Bytes = [byte[]](255) }
        ) {
            $response = [Net.Http.HttpResponseMessage]::new([Net.HttpStatusCode]$Status)
            $response.Content = [Net.Http.ByteArrayContent]::new($Bytes)
            try {
                { Read-IssueReplicateHttpResponse -Response $response -MaxBytes 32 `
                        -CancellationToken ([Threading.CancellationToken]::None) } | Should -Throw
            } finally { $response.Dispose() }
        }
    }

    Describe 'Video comment publication' {
        It 'checkpoints media side effects and refuses uncertain retries (<Fault>)' -ForEach @(
            @{ Fault = '' }, @{ Fault = 'pending write' },
            @{ Fault = 'upload' }, @{ Fault = 'receipt write' }
        ) {
            $fixture = New-RecordingFixture
            $state = @{ Body = ''; Events = [Collections.Generic.List[string]]::new() }
            $url = 'https://github.com/user-attachments/assets/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
            Mock Publish-IssueReplicateRecording {
                $state.Body | Should -Match '^<!-- issue-replicate-recording-pending:'
                [Convert]::ToBase64String($Bytes) | Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
                $state.Events.Add('upload')
                if ($Fault -eq 'upload') { throw 'The upload response was lost.' }
                return $url
            }
            $save = {
                param([string]$Checkpoint)
                $event = if ($Checkpoint.Contains('recording-pending:')) { 'pending write' } else { 'receipt write' }
                $state.Events.Add($event)
                if ($Fault -eq $event) { throw 'The comment write failed.' }
                $state.Body = $Checkpoint
            }
            $publish = {
                Sync-IssueReplicateRecordingPublication -Bytes $fixture.Bytes -Recording $fixture.Recording `
                    -IssueNumber 12345 -ExistingBody $state.Body -SaveCheckpoint $save
            }
            if (-not $Fault) {
                $receipt = & $publish
                $receipt.Url | Should -BeExactly $url
                ($state.Events -join ',') | Should -BeExactly 'pending write,upload,receipt write'
                $state.Body | Should -BeExactly $receipt.Checkpoint
                $again = & $publish
                $again.Url | Should -BeExactly $url
                $state.Events.Count | Should -Be 3
                Should -Invoke Publish-IssueReplicateRecording -Times 1 -Exactly
            } else {
                $publish | Should -Throw
                if ($Fault -eq 'pending write') {
                    Should -Invoke Publish-IssueReplicateRecording -Times 0 -Exactly
                    $state.Body | Should -BeExactly ''
                } else {
                    $state.Body | Should -Match '^<!-- issue-replicate-recording-pending:'
                    $publish | Should -Throw '*automatic retry will not upload again*'
                    Should -Invoke Publish-IssueReplicateRecording -Times 1 -Exactly
                }
            }
        }

        It 'does not repeat an uncertain upload or erase its owned pending report' {
            $fixture = New-RecordingFixture
            $parameters = New-RecordingPostFixture $fixture.Recording
            $pending = "<!-- issue-replicate-result:456789 -->`nMedia publication is incomplete.`n" +
                "<!-- issue-replicate-recording-pending:$($fixture.Recording.sha256) -->"
            $global:recordingPendingBody = $pending
            $global:recordingUploadAttempts = 0
            function gh {
                begin { $inputBody = [Collections.Generic.List[string]]::new() }
                process { if ($null -ne $_) { $inputBody.Add([string]$_) } }
                end {
                    $global:LASTEXITCODE = 0
                    if ($args[1] -eq 'user') { 'fixture-publisher'; return }
                    if ('--slurp' -in $args) { throw 'gh does not support --slurp with --jq.' }
                    $query = $args[[array]::IndexOf($args, '--jq') + 1]
                    if ($query.EndsWith('| @json')) {
                        ConvertTo-Json -InputObject @{ body = $global:recordingPendingBody } -Compress
                        return
                    }
                    if ($args[1] -eq 'repos/dotnet/maui') {
                        $global:recordingUploadAttempts++
                        $global:LASTEXITCODE = 1
                        return
                    }
                    if ('--paginate' -in $args) { '7'; return }
                    $global:recordingPendingBody = $inputBody -join "`n"
                    'https://github.com/dotnet/maui/issues/12345#issuecomment-7'
                }
            }
            try {
                { & (Join-Path $PSScriptRoot 'IssueReplicate.Post.ps1') @parameters `
                    -VideoBytes $fixture.Bytes } | Should -Throw
                $global:recordingUploadAttempts | Should -Be 0
                $global:recordingPendingBody | Should -BeExactly $pending
            } finally { Remove-Variable recordingPendingBody, recordingUploadAttempts -Scope Global }
        }

        It 'accepts hash-bound video bytes in memory without inflating the local CLI environment' {
            $fixture = New-RecordingFixture -Length 524288
            $parameters = New-RecordingPostFixture $fixture.Recording
            $preview = Join-Path $TestDrive 'in-memory-video.md'
            function gh { throw 'A preview must not call GitHub.' }
            & (Join-Path $PSScriptRoot 'IssueReplicate.Post.ps1') @parameters -NativeCanary `
                -VideoBytes $fixture.Bytes -OutputPath $preview
            [Convert]::ToBase64String([IO.File]::ReadAllBytes("$preview.recording.mp4")) |
            Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
            [Environment]::GetEnvironmentVariable('REPRO_VIDEO_0') | Should -BeNullOrEmpty
            $fixture.Bytes[20] = $fixture.Bytes[20] -bxor 1
            { & (Join-Path $PSScriptRoot 'IssueReplicate.Post.ps1') @parameters `
                    -VideoBytes $fixture.Bytes -OutputPath $preview } | Should -Throw '*completed result*'
        }

        It 'renders an honest preview and preserves exact video bytes without calling GitHub' {
            $fixture = New-RecordingFixture
            Set-RecordingFixtureEnvironment $fixture
            $parameters = New-RecordingPostFixture $fixture.Recording
            $preview = Join-Path $TestDrive 'video-preview.md'
            function gh { throw 'A preview must not call GitHub.' }
            & (Join-Path $PSScriptRoot 'IssueReplicate.Post.ps1') @parameters -NativeCanary -OutputPath $preview
            $body = [IO.File]::ReadAllText($preview)
            $body | Should -Match 'Native recording'
            $body | Should -Match 'not independent proof'
            $body | Should -Match 'replays a reviewed historical'
            $body | Should -Match 'remains disabled'
            $body | Should -Not -Match 'https://github.com/user-attachments/assets/'
            [Convert]::ToBase64String([IO.File]::ReadAllBytes("$preview.recording.mp4")) |
            Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
        }

        It 'forwards and exports a completed UI recording through the parent-memory wrapper' {
            $fixture = New-RecordingFixture -Length 65537
            Set-RecordingFixtureEnvironment $fixture
            $parameters = New-RecordingPostFixture $fixture.Recording
            $manifestPath = Join-Path $parameters.InputDirectory 'manifest.json'
            $manifest = Get-Content -Raw $manifestPath | ConvertFrom-Json
            $manifest | Add-Member -NotePropertyName schemaVersion -NotePropertyValue 1
            $manifest | ConvertTo-Json | Set-Content $manifestPath
            $sample = Join-Path $parameters.InputDirectory 'sample-result.json'
            @{ targetSha = $manifest.targetSha; sampleSha256 = $manifest.sampleSha256; buildSucceeded = $true } |
            ConvertTo-Json | Set-Content $sample
            $candidate = Join-Path $parameters.InputDirectory 'candidate.json'
            @{
                kind = 'ui'; files = @(
                    @{
                        path = 'src/Controls/tests/TestCases.HostApp/Issues/Issue12345.cs'
                        content = "#if ANDROID`nclass Issue12345 { }`n#endif"
                    },
                    @{
                        path = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue12345.cs'
                        content = "#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST`nclass Issue12345 { }`n#endif"
                    }
                )
            } | ConvertTo-Json -Depth 5 | Set-Content $candidate
            $resultPath = Join-Path $parameters.ResultsDirectory 'result.json'
            $result = Get-Content -Raw $resultPath | ConvertFrom-Json
            $result | Add-Member -NotePropertyName observedAssertion -NotePropertyValue $false
            $result | Add-Member -NotePropertyName candidateSha256 `
                -NotePropertyValue ((Get-FileHash $candidate -Algorithm SHA256).Hash.ToLowerInvariant())
            $result | ConvertTo-Json -Depth 6 | Set-Content $resultPath
            $output = Join-Path $parameters.InputDirectory 'forwarded'
            $oldOutput = $env:GITHUB_OUTPUT
            $env:GITHUB_OUTPUT = Join-Path $parameters.InputDirectory 'job-output.txt'
            try {
                & (Join-Path $PSScriptRoot 'IssueReplicate.Run.ps1') -Mode Forward `
                    -InputDirectory $parameters.InputDirectory -SampleResultPath $sample -CandidatePath $candidate `
                    -OutputDirectory $output -PreviousResultPath $resultPath -Attempt 1 -Provider GitHub
                [Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $output 'recording.mp4'))) |
                Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
                $jobOutput = [IO.File]::ReadAllLines($env:GITHUB_OUTPUT)
                $videoLines = @($jobOutput | Where-Object { $_ -match '^recording[0-7]=' })
                $videoLines.Count | Should -Be 2
                $decoded = $videoLines | ForEach-Object { [Convert]::FromBase64String($_.Split('=', 2)[1]) }
                [Convert]::ToBase64String([byte[]]$decoded) |
                Should -BeExactly ([Convert]::ToBase64String($fixture.Bytes))
            } finally { $env:GITHUB_OUTPUT = $oldOutput }
        }

        It 'reuses an owned multiline report video instead of uploading or adding a duplicate comment' {
            $fixture = New-RecordingFixture
            Set-RecordingFixtureEnvironment $fixture
            $parameters = New-RecordingPostFixture $fixture.Recording
            $global:recordingExistingBody = "<!-- issue-replicate-result:456789 -->`nPrevious report`n" +
            "<!-- issue-replicate-recording:$($fixture.Recording.sha256) -->`n" +
            'https://github.com/user-attachments/assets/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
            $global:recordingPostedBodies = [Collections.Generic.List[string]]::new()
            function gh {
                begin { $inputBody = [Collections.Generic.List[string]]::new() }
                process { if ($null -ne $_) { $inputBody.Add([string]$_) } }
                end {
                    $global:LASTEXITCODE = 0
                    if ($args[1] -eq 'user') { 'fixture-publisher'; return }
                    if ('--slurp' -in $args) { throw 'gh does not support --slurp with --jq.' }
                    $query = $args[[array]::IndexOf($args, '--jq') + 1]
                    if ($query.EndsWith('| @json')) {
                        ConvertTo-Json -InputObject @{ body = $global:recordingExistingBody } -Compress
                        return
                    }
                    if ('--paginate' -in $args) { '7'; return }
                    if ('PATCH' -notin $args -or $args[1] -ne 'repos/dotnet/maui/issues/comments/7') {
                        throw 'The retry must update the owned comment only.'
                    }
                    $global:recordingPostedBodies.Add(($inputBody -join "`n"))
                    'https://github.com/dotnet/maui/issues/12345#issuecomment-7'
                }
            }
            try {
                & (Join-Path $PSScriptRoot 'IssueReplicate.Post.ps1') @parameters
                $global:recordingPostedBodies.Count | Should -Be 1
                $global:recordingPostedBodies[0] | Should -Match 'https://github.com/user-attachments/assets/aaaaaaaa'
                $global:recordingPostedBodies[0] | Should -Not -Match 'Video publication failed'
            } finally { Remove-Variable recordingExistingBody, recordingPostedBodies -Scope Global }
        }

        It 'preserves a recording receipt across failed patch continuation publication and retry' {
            $fixture = New-RecordingFixture
            $parameters = New-RecordingPostFixture $fixture.Recording
            $paths = @(
                'src/Controls/tests/TestCases.HostApp/Issues/Issue12345.cs',
                'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue12345.cs'
            )
            $patch = ($paths | ForEach-Object {
                "diff --git a/$_ b/$_`n--- /dev/null`n+++ b/$_`n@@ -0,0 +1 @@`n+//" + ('x' * 24000) + "`n"
            }) -join ''
            $patchPath = Join-Path $parameters.ResultsDirectory 'test.patch'
            [IO.File]::WriteAllText($patchPath, $patch)
            $resultPath = Join-Path $parameters.ResultsDirectory 'result.json'
            $result = Get-Content -Raw $resultPath | ConvertFrom-Json
            $result.status = 'candidate-failed'
            $result.assertionFailed = $true
            $result | Add-Member -NotePropertyName patchSha256 -NotePropertyValue (
                (Get-FileHash $patchPath -Algorithm SHA256).Hash.ToLowerInvariant())
            $result | ConvertTo-Json -Depth 6 | Set-Content $resultPath
            $url = 'https://github.com/user-attachments/assets/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
            $global:recordingContinuationRemote = [Collections.Generic.List[object]]::new()
            $global:recordingContinuationRemote.Add(@{
                id = 7; body = "<!-- issue-replicate-result:456789 -->`n" +
                    "<!-- issue-replicate-recording:$($fixture.Recording.sha256) -->`n$url"
            })
            $global:recordingContinuationFail = $true
            function gh {
                begin { $inputBody = [Collections.Generic.List[string]]::new() }
                process { if ($null -ne $_) { $inputBody.Add([string]$_) } }
                end {
                    $global:LASTEXITCODE = 0
                    if ($args[1] -eq 'user') { 'fixture-publisher'; return }
                    if ($args[1] -eq 'repos/dotnet/maui') { throw 'A receipt retry must not upload again.' }
                    if ('--paginate' -in $args) {
                        $query = $args[[array]::IndexOf($args, '--jq') + 1]
                        $query | Should -Match '\.user\.login == "fixture-publisher"'
                        $marker = [regex]::Match($query, 'startswith\("([^"]+)"\)').Groups[1].Value
                        $items = @($global:recordingContinuationRemote | Where-Object { $_.body.StartsWith($marker) })
                        if ('--slurp' -in $args) { throw 'gh does not support --slurp with --jq.' }
                        if ($query.EndsWith('| @json')) {
                            $items | ForEach-Object { ConvertTo-Json -InputObject @{ body = $_.body } -Compress }
                        } else { $items | ForEach-Object id }
                        return
                    }
                    $body = $inputBody -join "`n"
                    if ($global:recordingContinuationFail -and $body.StartsWith('<!-- issue-replicate-patch:2:')) {
                        $global:LASTEXITCODE = 1
                        return
                    }
                    if ($args[1] -match '/issues/comments/([0-9]+)$') {
                        $id = [int]$Matches[1]
                        @($global:recordingContinuationRemote | Where-Object id -eq $id)[0].body = $body
                    } else {
                        $id = 7 + $global:recordingContinuationRemote.Count
                        $global:recordingContinuationRemote.Add(@{ id = $id; body = $body })
                    }
                    "https://github.com/dotnet/maui/issues/12345#issuecomment-$id"
                }
            }
            $post = {
                & (Join-Path $PSScriptRoot 'IssueReplicate.Post.ps1') @parameters -VideoBytes $fixture.Bytes
            }
            try {
                $post | Should -Throw '*Could not post*'
                $global:recordingContinuationRemote[0].body | Should -Match 'Candidate publication is incomplete'
                $global:recordingContinuationRemote[0].body | Should -Match ([regex]::Escape($url))
                $global:recordingContinuationFail = $false
                & $post
                $global:recordingContinuationRemote[0].body | Should -Not -Match 'publication is incomplete'
                $global:recordingContinuationRemote[0].body | Should -Match 'Native recording'
                $global:recordingContinuationRemote[0].body | Should -Match ([regex]::Escape($url))
                @($global:recordingContinuationRemote | Where-Object {
                    $_.body.StartsWith('<!-- issue-replicate-result:')
                }).Count | Should -Be 1
            } finally { Remove-Variable recordingContinuationRemote, recordingContinuationFail -Scope Global }
        }

        It 'posts one honest test report and then fails explicitly when recording is unavailable' {
            $parameters = New-RecordingPostFixture @{ status = 'failed'; diagnostic = 'Recorder timed out.' }
            $global:recordingPostedBodies = [Collections.Generic.List[string]]::new()
            function gh {
                begin { $inputBody = [Collections.Generic.List[string]]::new() }
                process { if ($null -ne $_) { $inputBody.Add([string]$_) } }
                end {
                    $global:LASTEXITCODE = 0
                    if ($args[1] -eq 'user') { 'fixture-publisher'; return }
                    if ('--paginate' -in $args) { return }
                    $global:recordingPostedBodies.Add(($inputBody -join "`n"))
                    'https://github.com/dotnet/maui/issues/12345#issuecomment-8'
                }
            }
            try {
                { & (Join-Path $PSScriptRoot 'IssueReplicate.Post.ps1') @parameters } | Should -Throw '*video is incomplete*'
                $global:recordingPostedBodies.Count | Should -Be 1
                $global:recordingPostedBodies[0] | Should -Match 'Native recording unavailable'
                $global:recordingPostedBodies[0] | Should -Match 'Recorder timed out'
                $global:recordingPostedBodies[0] | Should -Match 'test ran and passed'
                $global:recordingPostedBodies[0] | Should -Not -Match 'user-attachments/assets'
            } finally { Remove-Variable recordingPostedBodies -Scope Global }
        }
    }
}
