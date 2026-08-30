#!/usr/bin/env pwsh
#Requires -Modules Pester

# Generated Sandbox code, generated tests, and generated fixes run on the same
# agent as the credentials that publish the result. These tests cover the
# construction of the child environment those processes get, and the scan that
# proves nothing leaked into what the run published.

BeforeAll {
    . (Join-Path $PSScriptRoot '../shared/Assert-ReplicationExecutionEnvironment.ps1')

    $script:ScratchRoot = Join-Path $PSScriptRoot 'execution-environment-scratch'
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $script:ScratchRoot -Force | Out-Null

    $script:Canary = (Get-ReplicationSecretCanaryPrefix) + '15121999-1'

    # A realistic agent environment: what a device pool actually carries when
    # the replicate step starts.
    function script:New-AgentEnvironment {
        return @{
            # The real PATH, because one of these tests starts a grandchild
            # process and a fabricated PATH would prove only that pwsh is missing.
            PATH = [Environment]::GetEnvironmentVariable('PATH')
            HOME = [Environment]::GetEnvironmentVariable('HOME')
            TMPDIR = [IO.Path]::GetTempPath()
            DOTNET_ROOT = '/usr/share/dotnet'
            DOTNET_NOLOGO = '1'
            JAVA_HOME = '/usr/lib/jvm/temurin-17'
            ANDROID_HOME = '/usr/local/lib/android/sdk'
            ANDROID_SDK_ROOT = '/usr/local/lib/android/sdk'
            APPIUM_HOME = '/agent/_temp/.appium'
            DEVICE_UDID = 'emulator-5554'

            GH_TOKEN = 'ghp_pretend_this_is_a_pat_0123456789'
            GITHUB_TOKEN = 'ghp_pretend_this_is_a_pat_0123456789'
            GH_COMMENT_TOKEN = 'ghp_pretend_this_is_a_pat_0123456789'
            COPILOT_GITHUB_TOKEN = 'cop_pretend_token'
            SYSTEM_ACCESSTOKEN = 'azdo-oauth-token'
            AZURE_STORAGE_KEY = 'base64+storage+key'
            AZURE_CLIENT_SECRET = 'client-secret'
            ENDPOINT_AUTH_SYSTEMVSSCONNECTION = '{"parameters":{"AccessToken":"x"}}'
            VSS_NUGET_EXTERNAL_FEED_ENDPOINTS = '{"endpointCredentials":[]}'
            NUGET_PLUGIN_PATHS = '/agent/credprovider'
            GIT_ASKPASS = '/agent/askpass.sh'
            GIT_CONFIG_PARAMETERS = "'http.extraheader=AUTHORIZATION: basic Zm9v'"
            HTTPS_PROXY = 'http://proxyuser:proxypassword@proxy.internal:8080'
            HTTP_PROXY = 'http://proxyuser:proxypassword@proxy.internal:8080'
            AGENT_PROXYPASSWORD = 'proxypassword'
            MY_PRIVATE_FEED_TOKEN = 'feed-token'
            SOME_SERVICE_PASSWORD = 'hunter2'
            MAUI_REPLICATION_SECRET_CANARY = $script:Canary
        }
    }
}

AfterAll {
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Constructing the environment a generated process sees' {
    BeforeEach {
        $script:Built = Get-ReplicationExecutionEnvironment -Inherited (script:New-AgentEnvironment)
    }

    It 'keeps only the runtime variables a build and a device run need' {
        foreach ($name in @(
            'PATH', 'HOME', 'TMPDIR', 'DOTNET_ROOT', 'DOTNET_NOLOGO',
            'JAVA_HOME', 'ANDROID_HOME', 'ANDROID_SDK_ROOT', 'APPIUM_HOME',
            'DEVICE_UDID')) {
            $script:Built.Contains($name) | Should -BeTrue -Because "$name is required to build and run"
        }
    }

    It 'drops every GitHub, Copilot, and Azure DevOps credential' {
        foreach ($name in @(
            'GH_TOKEN', 'GITHUB_TOKEN', 'GH_COMMENT_TOKEN',
            'COPILOT_GITHUB_TOKEN', 'SYSTEM_ACCESSTOKEN')) {
            $script:Built.Contains($name) | Should -BeFalse -Because "$name may never reach generated code"
        }
    }

    It 'drops every AZURE_ variable and every service-connection endpoint' {
        foreach ($name in @(
            'AZURE_STORAGE_KEY', 'AZURE_CLIENT_SECRET',
            'ENDPOINT_AUTH_SYSTEMVSSCONNECTION')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops git credential helpers, askpass, and injected git config' {
        foreach ($name in @('GIT_ASKPASS', 'GIT_CONFIG_PARAMETERS')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops proxy variables, which is where proxy credentials live' {
        foreach ($name in @('HTTP_PROXY', 'HTTPS_PROXY', 'AGENT_PROXYPASSWORD')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
        foreach ($value in @($script:Built.Values)) {
            [string]$value | Should -Not -Match 'proxypassword'
        }
    }

    It 'drops NuGet and dotnet feed credentials' {
        foreach ($name in @('VSS_NUGET_EXTERNAL_FEED_ENDPOINTS', 'NUGET_PLUGIN_PATHS')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops an inherited secret nobody thought to list' {
        # The point of an allowlist. Neither of these appears anywhere in the
        # pipeline; both are refused because they were never permitted.
        foreach ($name in @('MY_PRIVATE_FEED_TOKEN', 'SOME_SERVICE_PASSWORD')) {
            $script:Built.Contains($name) | Should -BeFalse
        }
    }

    It 'drops the canary' {
        $script:Built.Contains('MAUI_REPLICATION_SECRET_CANARY') | Should -BeFalse
        foreach ($value in @($script:Built.Values)) {
            [string]$value | Should -Not -Match ([regex]::Escape((Get-ReplicationSecretCanaryPrefix)))
        }
    }

    It 'drops a permitted name that was given the canary as its value' {
        # An allowlisted name is not a licence to carry anything.
        $inherited = script:New-AgentEnvironment
        $inherited['APPIUM_HOME'] = $script:Canary
        $built = Get-ReplicationExecutionEnvironment -Inherited $inherited
        $built.Contains('APPIUM_HOME') | Should -BeFalse
    }

    It 'lets a trusted caller add a required runtime value' {
        $built = Get-ReplicationExecutionEnvironment `
            -Inherited (script:New-AgentEnvironment) `
            -Additional @{ MSBUILDDISABLENODEREUSE = '1' }
        $built['MSBUILDDISABLENODEREUSE'] | Should -Be '1'
    }

    It 'refuses a trusted caller that tries to add a credential back' {
        {
            Get-ReplicationExecutionEnvironment `
                -Inherited @{ PATH = '/usr/bin' } `
                -Additional @{ GH_TOKEN = 'ghp_x' }
        } | Should -Throw '*may not add a forbidden variable*'
    }

    It 'refuses a trusted caller that tries to smuggle the canary in' {
        {
            Get-ReplicationExecutionEnvironment `
                -Inherited @{ PATH = '/usr/bin' } `
                -Additional @{ APPIUM_HOME = $script:Canary }
        } | Should -Throw '*canary-bearing content*'
    }
}

Describe 'Re-checking a constructed environment' {
    It 'accepts the environment the allowlist produced' {
        $built = Get-ReplicationExecutionEnvironment -Inherited (script:New-AgentEnvironment)
        { Assert-ReplicationExecutionEnvironment -Environment $built } | Should -Not -Throw
    }

    It 'refuses a forbidden name that reached the set some other way' {
        # The assertion is deliberately independent of the builder: a mistake in
        # the allowlist has to be caught rather than trusted.
        {
            Assert-ReplicationExecutionEnvironment `
                -Environment @{ PATH = '/usr/bin'; SYSTEM_ACCESSTOKEN = 'x' } `
                -Context 'test'
        } | Should -Throw '*carries variables it may not*SYSTEM_ACCESSTOKEN*'
    }

    It 'refuses a name that is merely not on the allowlist' {
        {
            Assert-ReplicationExecutionEnvironment -Environment @{ PATH = '/usr/bin'; SOMETHING_ELSE = 'x' }
        } | Should -Throw '*SOMETHING_ELSE*'
    }

    It 'refuses an allowed name carrying the canary' {
        {
            Assert-ReplicationExecutionEnvironment -Environment @{ PATH = $script:Canary }
        } | Should -Throw '*PATH*'
    }
}

Describe 'A generated process really does not see the secrets' {
    It 'keeps them from the child and from the grandchild it starts' {
        # The end-to-end claim, made by actually starting the processes: a pwsh
        # child launched with the constructed environment, which itself starts a
        # grandchild and reports what both could see.
        $grandchild = Join-Path $script:ScratchRoot 'grandchild.ps1'
        Set-Content -LiteralPath $grandchild -Encoding utf8NoBOM -Value @'
$names = @('GH_TOKEN','GITHUB_TOKEN','COPILOT_GITHUB_TOKEN','SYSTEM_ACCESSTOKEN',
  'AZURE_STORAGE_KEY','AZURE_CLIENT_SECRET','ENDPOINT_AUTH_SYSTEMVSSCONNECTION',
  'VSS_NUGET_EXTERNAL_FEED_ENDPOINTS','GIT_ASKPASS','GIT_CONFIG_PARAMETERS',
  'HTTPS_PROXY','HTTP_PROXY','AGENT_PROXYPASSWORD','MY_PRIVATE_FEED_TOKEN',
  'SOME_SERVICE_PASSWORD','MAUI_REPLICATION_SECRET_CANARY')
foreach ($n in $names) {
  $v = [Environment]::GetEnvironmentVariable($n)
  if (-not [string]::IsNullOrEmpty($v)) { Write-Output "GRANDCHILD-LEAK:$n" }
}
Write-Output "GRANDCHILD-PATH:$([bool][Environment]::GetEnvironmentVariable('PATH'))"
'@

        $child = Join-Path $script:ScratchRoot 'child.ps1'
        Set-Content -LiteralPath $child -Encoding utf8NoBOM -Value @"
`$names = @('GH_TOKEN','GITHUB_TOKEN','COPILOT_GITHUB_TOKEN','SYSTEM_ACCESSTOKEN',
  'AZURE_STORAGE_KEY','AZURE_CLIENT_SECRET','ENDPOINT_AUTH_SYSTEMVSSCONNECTION',
  'VSS_NUGET_EXTERNAL_FEED_ENDPOINTS','GIT_ASKPASS','GIT_CONFIG_PARAMETERS',
  'HTTPS_PROXY','HTTP_PROXY','AGENT_PROXYPASSWORD','MY_PRIVATE_FEED_TOKEN',
  'SOME_SERVICE_PASSWORD','MAUI_REPLICATION_SECRET_CANARY')
foreach (`$n in `$names) {
  `$v = [Environment]::GetEnvironmentVariable(`$n)
  if (-not [string]::IsNullOrEmpty(`$v)) { Write-Output "CHILD-LEAK:`$n" }
}
& '$((Get-Command pwsh).Source)' -NoLogo -NoProfile -NonInteractive -File '$grandchild'
"@

        $built = Get-ReplicationExecutionEnvironment -Inherited (script:New-AgentEnvironment)
        $startInfo = [Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = (Get-Command pwsh).Source
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        foreach ($argument in @('-NoLogo', '-NoProfile', '-NonInteractive', '-File', $child)) {
            [void]$startInfo.ArgumentList.Add($argument)
        }
        $startInfo.Environment.Clear()
        foreach ($name in @($built.Keys)) {
            $startInfo.Environment[[string]$name] = [string]$built[$name]
        }

        $process = [Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        try {
            $process.Start() | Should -BeTrue
            $stdout = $process.StandardOutput.ReadToEnd()
            $stderr = $process.StandardError.ReadToEnd()
            $process.WaitForExit(120000) | Should -BeTrue
        } finally {
            $process.Dispose()
        }

        if ([string]::IsNullOrWhiteSpace($stdout)) {
            throw "The child produced no output. stderr: $stderr"
        }
        $stdout | Should -Not -Match 'CHILD-LEAK'
        $stdout | Should -Not -Match 'GRANDCHILD-LEAK'
        $stdout | Should -Match 'GRANDCHILD-PATH:True'
        # And nothing the child printed carries a marker either.
        Get-ReplicationSecretMarkerMatch -Text ($stdout + $stderr) | Should -BeNullOrEmpty
    }
}

Describe 'Scanning what the run published' {
    BeforeEach {
        $script:ArtifactRoot = Join-Path $script:ScratchRoot ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $script:ArtifactRoot 'evidence') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $script:ArtifactRoot 'verification') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'candidate.json') `
            -Value '{"schemaVersion":1}' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'test.patch') `
            -Value "diff --git a/x b/x`n" -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'verification/verification-console.log') `
            -Value 'Passed! - Failed: 1' -Encoding utf8NoBOM
        [IO.File]::WriteAllBytes(
            (Join-Path $script:ArtifactRoot 'evidence/repro.mp4'),
            [byte[]](0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70))
    }

    It 'accepts artifacts with no marker in them' {
        $result = Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot
        $result.ScannedFiles | Should -BeGreaterThan 0
    }

    It 'refuses the run canary in a log' {
        Add-Content -LiteralPath (Join-Path $script:ArtifactRoot 'verification/verification-console.log') `
            -Value "env dump: $script:Canary"
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw "*Secret marker 'canary'*"
    }

    It 'refuses the run canary in a patch' {
        Add-Content -LiteralPath (Join-Path $script:ArtifactRoot 'test.patch') `
            -Value "+// $script:Canary"
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw "*Secret marker 'canary'*"
    }

    It 'refuses the run canary in a JSON document' {
        Set-Content -LiteralPath (Join-Path $script:ArtifactRoot 'candidate.json') `
            -Value ('{"note":"' + $script:Canary + '"}') -Encoding utf8NoBOM
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw "*Secret marker 'canary'*"
    }

    It 'refuses a real credential shape the canary would never have covered' {
        foreach ($case in @(
            @{ Text = 'token ghp_0123456789abcdefghijklmnopqrstuv'; Code = 'github-pat' },
            @{ Text = 'token github_pat_11ABCDEFG0123456789_abcdef'; Code = 'github-fine-grained-pat' },
            @{ Text = 'http.extraheader=AUTHORIZATION: basic eHg6Z2hwX2FiY2RlZmdoaWprbG1ub3A='; Code = 'git-extraheader' },
            @{ Text = 'remote https://x-access-token:ghp_abcdefghijkl@github.com/o/r'; Code = 'url-userinfo-credential' },
            @{ Text = 'AccountKey=abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGH'; Code = 'azure-storage-key' },
            @{ Text = 'https://s.blob.core.windows.net/c?sv=2021&sig=abcdefghijklmnopqrstuvwx'; Code = 'azure-sas' }
        )) {
            (Get-ReplicationSecretMarkerMatch -Text $case.Text).Code |
                Should -Be $case.Code -Because "'$($case.Code)' must be recognised"
        }
    }

    It 'refuses a link planted in the artifact tree' -Skip:([System.OperatingSystem]::IsWindows()) {
        $outside = Join-Path $script:ScratchRoot 'outside.txt'
        Set-Content -LiteralPath $outside -Value 'x' -Encoding utf8NoBOM
        & ln -s $outside (Join-Path $script:ArtifactRoot 'linked.txt')
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } |
            Should -Throw '*found a link*'
    }

    It 'does not report noise from a media file' {
        # Scanning an MP4 for token shapes finds coincidences and proves
        # nothing, so binaries are skipped by extension rather than by guesswork.
        [IO.File]::WriteAllBytes(
            (Join-Path $script:ArtifactRoot 'evidence/preview.gif'),
            [byte[]](1..255))
        { Assert-ReplicationNoSecretMarkers -Root $script:ArtifactRoot } | Should -Not -Throw
    }
}
