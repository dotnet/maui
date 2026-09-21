BeforeAll {
    $startScript = Join-Path $PSScriptRoot "Start-UiEvidenceAppium.ps1"

    function New-StartupProcessStub([string]$Failure) {
        $process = [PSCustomObject]@{
            Id = 123
            State = @{ Failure = $Failure; Queries = 0; Kills = 0; Waits = 0; Disposes = 0 }
        }
        $process | Add-Member ScriptProperty HasExited {
            $this.State.Queries++
            return $false
        }
        $process | Add-Member ScriptMethod Kill {
            param([bool]$EntireTree)
            $this.State.Kills++
            if ($this.State.Failure -eq "kill") { throw "cleanup-kill-failure" }
        }
        $process | Add-Member ScriptMethod WaitForExit {
            param([int]$Timeout)
            $this.State.Waits++
            return $this.State.Failure -ne "wait"
        }
        $process | Add-Member ScriptMethod Dispose {
            $this.State.Disposes++
            if ($this.State.Failure -eq "dispose") { throw "cleanup-dispose-failure" }
        }
        return $process
    }
}

Describe "UI evidence Appium failed-startup cleanup" {
    BeforeEach {
        Mock Get-Command -ParameterFilter { $Name -in @("appium.cmd", "appium.ps1", "appium") } {
            [PSCustomObject]@{ Source = "appium-test-placeholder" }
        }
        Mock Invoke-RestMethod { [PSCustomObject]@{ value = [PSCustomObject]@{ ready = $false } } }
        Mock Start-Sleep { throw "primary-startup-failure" }
    }

    It "preserves the primary error and disposes after <Failure> cleanup" -TestCases @(
        @{ Failure = "none" },
        @{ Failure = "kill" },
        @{ Failure = "wait" },
        @{ Failure = "dispose" }
    ) {
        param($Failure)
        $process = New-StartupProcessStub $Failure
        Mock Start-Process -MockWith ({ $process }.GetNewClosure())
        $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
        $listener.Start()
        $port = $listener.LocalEndpoint.Port
        $listener.Stop()
        $oldWarningPreference = $WarningPreference
        $WarningPreference = "Stop"
        try {
            {
                & $startScript -Port $port -StatePath (Join-Path $TestDrive "$Failure-state.json") `
                    -LogPath (Join-Path $TestDrive "$Failure.log")
            } | Should -Throw "*primary-startup-failure*"
            $process.State.Disposes | Should -Be 1
            $process.State.Kills | Should -Be 1
            Should -Invoke Start-Process -Times 1 -Exactly
        }
        finally {
            $WarningPreference = $oldWarningPreference
        }
    }
}
