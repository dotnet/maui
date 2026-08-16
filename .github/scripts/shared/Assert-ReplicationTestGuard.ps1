#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

function Get-ReplicationUnsafeSourcePatterns {
    return @(
        [pscustomobject]@{ Code = 'file-system'; Pattern = '(?i)\bSystem\s*\.\s*IO\b|\b(?:File|Directory|Path)\s*\.|\b(?:FileInfo|DirectoryInfo|FileStream|StreamReader|StreamWriter|FileSystemWatcher|StorageFile|StorageFolder|NSFileManager|NSData)\b|\b(?:Java|Android)\s*\.\s*IO\b|\b(?:XmlDocument|XDocument)\s*\.\s*Load\b|\bXmlReader\s*\.\s*Create\b' },
        [pscustomobject]@{ Code = 'http-client'; Pattern = '(?i)\b(?:HttpClient|HttpMessageHandler|HttpRequestMessage|HttpWebRequest|SocketsHttpHandler|RestClient|GrpcChannel)\b' },
        [pscustomobject]@{ Code = 'network'; Pattern = '(?i)\bSystem\s*\.\s*Net\b|\b(?:Dns|WebClient|WebRequest|Socket|TcpClient|TcpListener|UdpClient|WebSocket|ClientWebSocket|NSUrlSession|NSURLSession)\b|\b(?:Java|Android)\s*\.\s*Net\b' },
        [pscustomobject]@{ Code = 'process-start'; Pattern = '(?i)\bSystem\s*\.\s*Diagnostics\b|\b(?:Process|ProcessStartInfo|UseShellExecute|Win32Exception|ManagementObject|NSTask|NSWorkspace)\b' },
        [pscustomobject]@{ Code = 'reflection'; Pattern = '(?i)\bSystem\s*\.\s*Reflection\b|\b(?:Assembly|Activator|AppDomain|MethodInfo|PropertyInfo|FieldInfo|ConstructorInfo|DynamicMethod|InvokeMember|GetMethod|GetProperty|GetField|GetType)\b' },
        [pscustomobject]@{ Code = 'native-code'; Pattern = '(?i)\bSystem\s*\.\s*Runtime\s*\.\s*InteropServices\b|\b(?:DllImport|LibraryImport|GeneratedDllImport|NativeLibrary|UnmanagedCallersOnly|GetDelegateForFunctionPointer|Marshal|GCHandle)\b|\b(?:unsafe|stackalloc|extern)\b' },
        [pscustomobject]@{ Code = 'environment-secrets'; Pattern = '(?i)\bEnvironment\s*\.|\bEnvironmentVariableTarget\b|\b(?:GH_TOKEN|GITHUB_TOKEN|COPILOT_GITHUB_TOKEN|SYSTEM_ACCESSTOKEN|AZURE_STORAGE_KEY|AZURE_STORAGE_SAS_TOKEN)\b' },
        [pscustomobject]@{ Code = 'device-external-access'; Pattern = '(?i)\b(?:Browser|Launcher|SecureStorage|Preferences|FileSystem|Connectivity|Clipboard|WebView|UriImageSource|FileImageSource|UIApplication|PendingIntent)\b' },
        [pscustomobject]@{ Code = 'delays-or-background-work'; Pattern = '(?i)\bThread\s*\.\s*Sleep\b|\bTask\s*\.\s*(?:Delay|Run|Factory)\b|\bTimer\b' },
        [pscustomobject]@{ Code = 'shell-execution'; Pattern = '(?i)\b(?:powershell|pwsh|cmd\.exe|bash)\b|/(?:bin/)?(?:ba)?sh\b|\bSystem\.Management\.Automation\b' },
        [pscustomobject]@{ Code = 'remote-url'; Pattern = '(?i)\b(?:https?|ftps?|wss?|file)\b\s*(?::|["'']\s*\+\s*["'']\s*:)|://' },
        [pscustomobject]@{ Code = 'package-reference'; Pattern = '(?i)\b(?:PackageReference|PackageDownload|dotnet\s+add\s+package|nuget\s*:|nuget\.exe)\b|#(?:r|load)\b' },
        [pscustomobject]@{ Code = 'obfuscated-source'; Pattern = '\\(?:u[0-9a-fA-F]{4}|U[0-9a-fA-F]{8})' },
        [pscustomobject]@{ Code = 'verification-spoof'; Pattern = '(?i)\bVERIFICATION\s+(?:PASSED|FAILED|INCONCLUSIVE)\b|##vso\[|::set-output' }
    )
}

function Assert-ReplicationGeneratedSourceSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $scanText = $Content.Replace("`r`n", "`n")
    $scanText = [regex]::Replace(
        $scanText,
        '(?i)http://schemas\.microsoft\.com/(?:dotnet/2021/maui|winfx/2009/xaml)(?=["''])',
        ''
    )
    if ([System.IO.Path]::GetExtension($Path) -ieq '.xaml') {
        $scanText = [regex]::Replace(
            $scanText,
            '(?i)(\bxmlns(?::[A-Za-z_]\w*)?\s*=\s*["''][^"'']*);assembly=[^"'']+(?=["''])',
            '$1'
        )
    }
    $environmentGuardCall = '(?:global::)?(?:System\.)?Environment\.GetEnvironmentVariable\s*\(\s*"MAUI_REPRODUCTION_ISSUE"\s*\)'
    $scanText = [regex]::Replace($scanText, $environmentGuardCall, '')
    foreach ($entry in Get-ReplicationUnsafeSourcePatterns) {
        if ($scanText -match $entry.Pattern) {
            throw "Candidate source '$Path' contains prohibited '$($entry.Code)' content."
        }
    }
}

function Assert-ReplicationTestLifecycleSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (
        $Content -match '(?i)\[\s*(?:SetUp|TearDown|OneTimeSetUp|OneTimeTearDown|TestInitialize|TestCleanup|ClassInitialize|ClassCleanup|AssemblyInitialize|AssemblyCleanup|ModuleInitializer)\b' -or
        $Content -match '(?i)\b(?:IAsyncLifetime|IClassFixture|ICollectionFixture|BeforeAfterTestAttribute)\b' -or
        $Content -match '(?m)^\s*(?:(?:public|internal|protected|private)\s+)?static\s+[^;(){}]+\s+[A-Za-z_]\w*\s*=(?!>)' -or
        $Content -match '(?m)^\s*(?:(?:public|internal|protected|private)\s+)?readonly\s+[^;(){}]+\s+[A-Za-z_]\w*\s*=(?!>)' -or
        $Content -match '(?m)^\s*(?:public|internal|protected|private)\s+(?!class\b|interface\b|enum\b|record\b)[^;(){}]+\s+[A-Za-z_]\w*\s*=(?!>)'
    ) {
        throw "Candidate test source '$Path' contains an unguarded test lifecycle hook."
    }

    $classNames = @(
        [regex]::Matches($Content, '\bclass\s+(?<name>[A-Za-z_]\w*)\b') |
            ForEach-Object { $_.Groups['name'].Value } |
            Sort-Object -Unique
    )
    foreach ($className in $classNames) {
        $constructorPattern = "(?m)^\s*(?:(?:public|internal|protected|private)\s+)?$([regex]::Escape($className))\s*\("
        if ($Content -match $constructorPattern) {
            throw "Candidate test source '$Path' contains an unguarded test-class constructor."
        }
        $staticConstructorPattern = "(?m)^\s*static\s+$([regex]::Escape($className))\s*\("
        if ($Content -match $staticConstructorPattern) {
            throw "Candidate test source '$Path' contains an unguarded static constructor."
        }
        $primaryConstructorPattern = "\bclass\s+$([regex]::Escape($className))\s*\("
        if ($Content -match $primaryConstructorPattern) {
            throw "Candidate test source '$Path' contains an unguarded primary constructor."
        }
    }
}

function Assert-ReplicationTestGuard {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]
        [ValidateRange(1, [int]::MaxValue)]
        [long]$IssueNumber,
        [Parameter(Mandatory = $true)]
        [ValidateSet('UnitTest', 'XamlUnitTest', 'DeviceTest', 'UITest')]
        [string]$TestType
    )

    $normalized = $Content.Replace("`r`n", "`n")
    $environmentGuardCall = '(?:global::)?(?:System\.)?Environment\.GetEnvironmentVariable\s*\(\s*"MAUI_REPRODUCTION_ISSUE"\s*\)'
    $guardCall = if ($TestType -ceq 'DeviceTest') {
        'GetReplicationIssue\s*\(\s*\)'
    } else {
        $environmentGuardCall
    }
    $issueText = [string]$IssueNumber
    $issueOperand = "(?:`"$([regex]::Escape($issueText))`"|IssueNumber)"
    $guardPattern = "if\s*\(\s*!\s*string\.Equals\s*\(\s*$guardCall\s*,\s*$issueOperand\s*,\s*StringComparison\.Ordinal\s*\)\s*\)\s*\{\s*return\s*;\s*\}"
    $testMethodPrefix = "(?s)\[\s*(?:(?:[A-Za-z_]\w*)\.)*(?:Fact|Test)\b[^\]]*\][^{};/]{1,1000}\{\s*"
    $testGuardPattern = "$testMethodPrefix$guardPattern"
    $hasGuard = $normalized -match $testGuardPattern

    if ($hasGuard -and $TestType -ceq 'DeviceTest') {
        $androidIssueCall = '(?:global::)?Microsoft\.Maui\.TestUtils\.DeviceTests\.Runners\.HeadlessRunner\s*\.\s*MauiTestInstrumentation\.Current\?\.Arguments\?\.GetString\s*\(\s*"MAUI_REPRODUCTION_ISSUE"\s*\)'
        $appleIssueCall = '(?:global::)?Foundation\.NSProcessInfo\.ProcessInfo\.Environment\s*\[\s*"MAUI_REPRODUCTION_ISSUE"\s*\]\?\.ToString\s*\(\s*\)'
        $deviceHelperPattern = "(?s)static\s+string\?\s+GetReplicationIssue\s*\(\s*\)\s*\{\s*#if\s+ANDROID\s*return\s+$androidIssueCall\s*;\s*#elif\s+IOS\s*\|\|\s*MACCATALYST\s*return\s+$appleIssueCall\s*;\s*#else\s*return\s+$environmentGuardCall\s*;\s*#endif\s*\}"
        $hasGuard = $normalized -match $deviceHelperPattern
    }

    if ($hasGuard -and $normalized -match '\bIssueNumber\b') {
        $constantPattern = "(?m)^\s*const\s+string\s+IssueNumber\s*=\s*`"$([regex]::Escape($issueText))`"\s*;\s*$"
        if ($normalized -notmatch $constantPattern) {
            $hasGuard = $false
        }
    }

    if (-not $hasGuard) {
        throw "Candidate test source '$Path' is missing the exact issue-keyed MAUI_REPRODUCTION_ISSUE ordinal guard."
    }
}
