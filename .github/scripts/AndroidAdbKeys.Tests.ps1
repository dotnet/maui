#Requires -Modules Pester

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
    $source = Get-Content (Join-Path $repoRoot 'eng/devices/android.cake') -Raw
    $method = [regex]::Match($source, '(?ms)^void EnsureAdbKeys\((?<settings>AdbToolSettings settings)?\)\r?\n\{.*?^\}')
    if (-not $method.Success) {
        throw 'EnsureAdbKeys was not found in android.cake.'
    }
    $invocation = if ($method.Groups['settings'].Success) {
        'EnsureAdbKeys(new AdbToolSettings());'
    }
    else {
        'EnsureAdbKeys();'
    }

    # Compile the real Cake method with isolated filesystem and process adapters.
    # No adapter can start ADB, contact a device, or access the host key directory.
    Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.IO;

namespace Maui.AndroidAdbKeysRegression
{
    public class CakeScript
    {
        private static string homeDirectory;
        public readonly List<string> DeviceCalls = new List<string>();
        public readonly Dictionary<string, string> Variables = new Dictionary<string, string>();
        public int KeygenCalls;
        public int KeygenExitCode;
        public string KeygenOutput = "both";
        public string KeygenArguments;
        public int PubkeyCalls;
        public int PubkeyExitCode;
        public string PubkeyOutput = "restored-public-key";

        public CakeScript(string home)
        {
            homeDirectory = home;
        }

        public void Run() { $invocation }

        private static class Environment
        {
            public enum SpecialFolder { UserProfile }
            public static string GetFolderPath(SpecialFolder folder) { return homeDirectory; }
        }

        private class AdbToolSettings { }
        private class ProcessSettings
        {
            public object Arguments { get; set; }
            public bool RedirectStandardOutput { get; set; }
            public bool RedirectStandardError { get; set; }
            public int Timeout { get; set; }
        }
        private class ProcessArgumentBuilder
        {
            private readonly List<string> arguments = new List<string>();
            public ProcessArgumentBuilder Append(string value) { arguments.Add(value); return this; }
            public ProcessArgumentBuilder AppendQuoted(string value) { return Append("\"" + value + "\""); }
            public override string ToString() { return string.Join(" ", arguments); }
        }

        private int StartProcess(string executable, ProcessSettings settings)
        {
            string arguments = settings.Arguments.ToString();
            if (executable != "adb" || !arguments.StartsWith("keygen "))
                throw new InvalidOperationException("Unexpected process: " + executable + " " + arguments);

            KeygenCalls++;
            KeygenArguments = arguments;
            if (KeygenExitCode != 0)
                return KeygenExitCode;

            string path = arguments.Substring("keygen ".Length).Trim('"');
            if (path != Path.Combine(homeDirectory, ".android", "adbkey"))
                throw new InvalidOperationException("Key generation escaped the isolated home.");

            if (KeygenOutput != "none")
                File.WriteAllText(path, "generated-private-key");
            if (KeygenOutput == "both" || KeygenOutput == "empty-public")
                File.WriteAllText(path + ".pub", KeygenOutput == "both" ? "generated-public-key" : "");
            return 0;
        }

        private int StartProcess(string executable, string arguments)
        {
            return StartProcess(executable, new ProcessSettings { Arguments = arguments });
        }
        private int StartProcess(string executable, ProcessSettings settings, out IEnumerable<string> output)
        {
            string expected = "pubkey \"" + Path.Combine(homeDirectory, ".android", "adbkey") + "\"";
            if (executable != "adb" || settings.Arguments.ToString() != expected)
                throw new InvalidOperationException("Unexpected public key export.");
            PubkeyCalls++;
            output = new[] { PubkeyOutput };
            return PubkeyExitCode;
        }
        private void Information(string message) { }
        private void Warning(string message) { }
        private void Error(string message) { }
        private bool IsRunningOnLinux() { return false; }
        private void SetEnvironmentVariable(string name, string value) { Variables[name] = value; }

        // Retain adapters for the previous implementation so this test also proves
        // that its key rotation and pre-boot device access fail the regression.
        private void AdbKillServer(AdbToolSettings settings) { DeviceCalls.Add("kill-server"); }
        private void AdbStartServer(AdbToolSettings settings) { DeviceCalls.Add("start-server"); }
        private void AdbShell(string command, AdbToolSettings settings) { DeviceCalls.Add(command); }
        private void RecoverAdbConnection(AdbToolSettings settings) { DeviceCalls.Add("recover"); }
        private bool CreateAdbKeysUsingKeygen(string directory, string path)
        {
            return StartProcess("adb", new ProcessSettings {
                Arguments = new ProcessArgumentBuilder().Append("keygen").AppendQuoted(path)
            }) == 0;
        }
        private bool CreateAdbKeysUsingAutomaticGeneration(AdbToolSettings settings, string key, string pub)
        {
            throw new InvalidOperationException("Unexpected automatic key generation.");
        }
        private bool CreateAdbKeysUsingOpenSSL(string key, string pub)
        {
            throw new InvalidOperationException("Unexpected OpenSSL fallback.");
        }

        $($method.Value)
    }
}
"@
}

Describe 'Android host ADB key preparation' {
    BeforeEach {
        $homeDirectory = Join-Path $TestDrive ("home with spaces " + [guid]::NewGuid())
        $keyDirectory = Join-Path $homeDirectory '.android'
        $null = New-Item $keyDirectory -ItemType Directory -Force
        $privateKey = Join-Path $keyDirectory 'adbkey'
        $publicKey = "$privateKey.pub"
        $cake = [Maui.AndroidAdbKeysRegression.CakeScript]::new($homeDirectory)
    }

    It 'preserves the existing identity throughout repeated boot checks' -Tag 'RegressionBaseline' {
        Set-Content $privateKey 'trusted-private-key' -NoNewline
        Set-Content $publicKey 'trusted-public-key' -NoNewline
        $failure = $null
        try {
            1..3 | ForEach-Object { $cake.Run() }
        }
        catch {
            $failure = $_
        }

        Get-Content $privateKey -Raw | Should -BeExactly 'trusted-private-key'
        Get-Content $publicKey -Raw | Should -BeExactly 'trusted-public-key'
        $failure | Should -BeNullOrEmpty
        $cake.KeygenCalls | Should -Be 0
        $cake.DeviceCalls.Count | Should -Be 0
        $cake.Variables['ADB_VENDOR_KEYS'] | Should -BeExactly $privateKey
    }

    It 'generates a missing pair once without a connected device or running server' {
        1..3 | ForEach-Object { $cake.Run() }

        $cake.KeygenCalls | Should -Be 1
        $cake.KeygenArguments | Should -BeExactly "keygen `"$privateKey`""
        Get-Content $privateKey -Raw | Should -BeExactly 'generated-private-key'
        Get-Content $publicKey -Raw | Should -BeExactly 'generated-public-key'
        $cake.DeviceCalls.Count | Should -Be 0
        $cake.Variables['ADB_VENDOR_KEYS'] | Should -BeExactly $privateKey
    }

    It 'restores a missing public key without rotating the private identity' {
        Set-Content $privateKey 'trusted-private-key' -NoNewline

        1..3 | ForEach-Object { $cake.Run() }

        Get-Content $privateKey -Raw | Should -BeExactly 'trusted-private-key'
        Get-Content $publicKey -Raw | Should -BeExactly 'restored-public-key'
        $cake.PubkeyCalls | Should -Be 1
        $cake.KeygenCalls | Should -Be 0
        $cake.Variables['ADB_VENDOR_KEYS'] | Should -BeExactly $privateKey
    }

    It 'does not replace an existing public key when the private key is missing' {
        Set-Content $publicKey 'retained-key' -NoNewline

        { $cake.Run() } | Should -Throw '*Incomplete ADB key pair*'

        Get-Content $publicKey -Raw | Should -BeExactly 'retained-key'
        $cake.KeygenCalls | Should -Be 0
        $cake.Variables.Count | Should -Be 0
    }

    It 'preserves the private key when public key export <Failure>' -TestCases @(
        @{ Failure = 'fails'; ExitCode = 17; Output = 'error output' }
        @{ Failure = 'returns empty output'; ExitCode = 0; Output = '' }
    ) {
        param($Failure, $ExitCode, $Output)
        Set-Content $privateKey 'trusted-private-key' -NoNewline
        $cake.PubkeyExitCode = $ExitCode
        $cake.PubkeyOutput = $Output

        { $cake.Run() } | Should -Throw '*ADB public key export failed*'

        Get-Content $privateKey -Raw | Should -BeExactly 'trusted-private-key'
        Test-Path $publicKey | Should -BeFalse
        $cake.Variables.Count | Should -Be 0
    }

    It 'reports keygen failure without falling back to device access or invalid keys' {
        $cake.KeygenExitCode = 17

        { $cake.Run() } | Should -Throw '*exit code 17*'

        $cake.KeygenCalls | Should -Be 1
        $cake.DeviceCalls.Count | Should -Be 0
        $cake.Variables.Count | Should -Be 0
    }

    It 'rejects successful keygen with <Output> output' -TestCases @(
        @{ Output = 'none' }
        @{ Output = 'private' }
        @{ Output = 'empty-public' }
    ) {
        param($Output)
        $cake.KeygenOutput = $Output

        { $cake.Run() } | Should -Throw '*ADB keys were not created successfully*'

        $cake.Variables.Count | Should -Be 0
    }

    It 'rejects an empty existing key without deleting either file' {
        Set-Content $privateKey '' -NoNewline
        Set-Content $publicKey 'retained-public-key' -NoNewline

        { $cake.Run() } | Should -Throw '*ADB keys were not created successfully*'

        Test-Path $privateKey | Should -BeTrue
        Get-Content $publicKey -Raw | Should -BeExactly 'retained-public-key'
        $cake.KeygenCalls | Should -Be 0
    }
}
