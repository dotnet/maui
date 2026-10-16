#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'IssueReplicate.Diagnostics.ps1')
}

Describe 'Bounded native diagnostic readers' {
    It 'reads exactly the last 128 KiB of a large regular file' -Skip:$IsWindows {
        $path = Join-Path $TestDrive 'appium.log'
        [IO.File]::WriteAllText($path, ('x' * 150000) + 'tail', [Text.UTF8Encoding]::new($false))
        $result = Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 128KB -Tail
        $result.Omitted | Should -BeTrue
        [Text.Encoding]::UTF8.GetByteCount($result.Text) | Should -Be 128KB
        $result.Text | Should -BeExactly (('x' * (128KB - 4)) + 'tail')
    }

    It 'preserves WDA startup diagnostics before a large iOS system-log flood' -Skip:$IsWindows {
        $path = Join-Path $TestDrive 'appium-startup.log'
        $noise = "[XCUITestDriver] [IOS_SYSLOG_ROW] device system activity`n" * 50000
        [IO.File]::WriteAllText($path, "[Xcode] WebDriverAgent startup failed`n$noise", [Text.UTF8Encoding]::new($false))
        (Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 128KB -Tail).Text |
            Should -Not -Match 'WebDriverAgent startup failed'
        $result = Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 128KB -Tail -AppiumLog
        $result.Text | Should -BeExactly "[Xcode] WebDriverAgent startup failed`n"
        $result.Text | Should -Not -Match 'IOS_SYSLOG_ROW'
    }

    It 'bounds filtered Appium rows and oversized lines without dropping the latest error' -Skip:$IsWindows {
        $path = Join-Path $TestDrive 'appium-long-lines.log'
        $lines = (1..100 | ForEach-Object { "[XCUITestDriver] diagnostic $_`n" }) -join ''
        [IO.File]::WriteAllText($path, ("x" * 2MB) + "`n$lines[Xcode] latest error", [Text.UTF8Encoding]::new($false))
        $result = Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 1024 -Tail -AppiumLog
        $result.Omitted | Should -BeTrue
        [Text.Encoding]::UTF8.GetByteCount($result.Text) | Should -BeLessOrEqual 1024
        $result.Text | Should -Match '\[Xcode\] latest error$'
        $result.Text | Should -Not -Match 'diagnostic 1\n'
    }

    It 'scans no more than the fixed Appium tail window' -Skip:$IsWindows {
        $path = Join-Path $TestDrive 'appium-sparse.log'
        $stream = [IO.File]::Create($path)
        try {
            $early = [Text.Encoding]::UTF8.GetBytes("[Xcode] outside scan window`n")
            $stream.Write($early)
            $stream.Seek(65MB, [IO.SeekOrigin]::Begin) | Out-Null
            $stream.Write([Text.Encoding]::UTF8.GetBytes("`n[Xcode] latest startup failure`n"))
        }
        finally { $stream.Dispose() }
        $result = Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 128KB -Tail -AppiumLog
        $result.Omitted | Should -BeTrue
        $result.Text | Should -BeExactly "[Xcode] latest startup failure`n"
    }

    It 'rejects a replaced Appium pathname FIFO without changing completed evidence' -Skip:$IsWindows {
        $path = Join-Path $TestDrive 'fifo.log'
        [IO.File]::WriteAllText($path, 'original Appium log')
        $writer = [IO.File]::Open($path, [IO.FileMode]::Open, [IO.FileAccess]::Write,
            [IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete)
        $evidence = [Text.Encoding]::UTF8.GetBytes('{"testExecuted":true,"observedAssertion":true}')
        $original = [Convert]::ToBase64String($evidence)
        try {
            Remove-Item -LiteralPath $path
            & python3 -I -S -c 'import os,sys; os.mkfifo(sys.argv[1])' $path
            if ($LASTEXITCODE -ne 0) { throw 'Could not create the functional FIFO fixture.' }
            $timer = [Diagnostics.Stopwatch]::StartNew()
            { Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 128KB -Tail } |
                Should -Throw '*must be a regular file*'
            { Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 128KB -Tail -AppiumLog } |
                Should -Throw '*must be a regular file*'
            $timer.Elapsed.TotalSeconds | Should -BeLessThan 5
            [Convert]::ToBase64String($evidence) | Should -BeExactly $original
            $writer.WriteByte(65)
        } finally { $writer.Dispose() }
    }

    It 'rejects a symlink and a whole file exceeding its bound' -Skip:$IsWindows {
        $path = Join-Path $TestDrive 'hierarchy.txt'
        $link = Join-Path $TestDrive 'linked.log'
        [IO.File]::WriteAllText($path, 'hierarchy')
        [IO.File]::CreateSymbolicLink($link, $path) | Out-Null
        { Read-IssueReplicateNativeDiagnostic -Path $link -MaxBytes 128KB -Tail } | Should -Throw
        { Read-IssueReplicateNativeDiagnostic -Path $link -MaxBytes 128KB -Tail -AppiumLog } | Should -Throw
        { Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 4 } |
            Should -Throw '*exceed the file bound*'
        (Read-IssueReplicateNativeDiagnostic -Path $path -MaxBytes 1024).Text | Should -BeExactly 'hierarchy'
    }

    It 'kills a stalled reader at its hard process deadline' -Skip:$IsWindows {
        $pidPath = Join-Path $TestDrive 'reader.pid'
        $code = 'import os,sys,time; open(sys.argv[1],"w").write(str(os.getpid())); time.sleep(30)'
        $timer = [Diagnostics.Stopwatch]::StartNew()
        { Invoke-IssueReplicateDiagnosticProcess -Code $code -Arguments @($pidPath) `
            -MaxOutputBytes 1024 -TimeoutSeconds 1 } | Should -Throw '*hard deadline*'
        $timer.Elapsed.TotalSeconds | Should -BeLessThan 5
        $readerPid = [int][IO.File]::ReadAllText($pidPath)
        { [Diagnostics.Process]::GetProcessById($readerPid) } | Should -Throw
    }

    It 'rejects process output exceeding the fixed allocation' -Skip:$IsWindows {
        { Invoke-IssueReplicateDiagnosticProcess -Code 'import sys; sys.stdout.write("x" * 4096)' `
            -MaxOutputBytes 1024 } | Should -Throw '*output bound*'
    }
}
