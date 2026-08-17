#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

function Get-ReplicationUnsafeSourcePatterns {
    return @(
        [pscustomobject]@{ Code = 'file-system'; Pattern = '(?i)\bSystem\s*\.\s*IO\b|\b(?:File|Directory|Path)\s*\.|\b(?:FileInfo|DirectoryInfo|FileStream|StreamReader|StreamWriter|FileSystemWatcher|StorageFile|StorageFolder|NSFileManager|NSData)\b|\b(?:Java|Android)\s*\.\s*IO\b|\b(?:XmlDocument|XDocument)\s*\.\s*Load\b|\bXmlReader\s*\.\s*Create\b' },
        [pscustomobject]@{ Code = 'http-client'; Pattern = '(?i)\b(?:HttpClient|HttpMessageHandler|HttpRequestMessage|HttpWebRequest|SocketsHttpHandler|RestClient|GrpcChannel)\b' },
        [pscustomobject]@{ Code = 'network'; Pattern = '(?i)\bSystem\s*\.\s*Net\b|\b(?:Dns|WebClient|WebRequest|Socket|TcpClient|TcpListener|UdpClient|WebSocket|ClientWebSocket|NSUrlSession|NSURLSession)\b|\b(?:Java|Android)\s*\.\s*Net\b' },
        [pscustomobject]@{ Code = 'process-start'; Pattern = '(?i)\b(?:Process|ProcessStartInfo|UseShellExecute|Win32Exception|ManagementObject|NSTask|NSWorkspace)\b' },
        [pscustomobject]@{ Code = 'reflection'; Pattern = '(?i)\bSystem\s*\.\s*Reflection\b|\b(?:Assembly|Activator|AppDomain|MethodInfo|PropertyInfo|FieldInfo|ConstructorInfo|DynamicMethod|InvokeMember|GetMethod|GetProperty|GetField)\b|\bGetType\s*\(\s*\)\s*\.\s*(?!Name\b|FullName\b|ToString\b)' },
        [pscustomobject]@{ Code = 'native-code'; Pattern = '(?i)\bSystem\s*\.\s*Runtime\s*\.\s*InteropServices\b|\b(?:DllImport|LibraryImport|GeneratedDllImport|NativeLibrary|UnmanagedCallersOnly|GetDelegateForFunctionPointer|Marshal|GCHandle)\b|\b(?:unsafe|stackalloc|extern)\b' },
        [pscustomobject]@{ Code = 'environment-secrets'; Pattern = '(?i)\bEnvironment\s*\.|\bEnvironmentVariableTarget\b|\b(?:GH_TOKEN|GITHUB_TOKEN|COPILOT_GITHUB_TOKEN|SYSTEM_ACCESSTOKEN|AZURE_STORAGE_KEY|AZURE_STORAGE_SAS_TOKEN)\b' },
        [pscustomobject]@{ Code = 'device-external-access'; Pattern = '(?i)\b(?:Browser|Launcher|SecureStorage|FileSystem|Connectivity|Clipboard|WebView|UriImageSource|FileImageSource|UIApplication|PendingIntent)\b|\bPreferences\s*\.' },
        [pscustomobject]@{ Code = 'delays-or-background-work'; Pattern = '(?i)\bThread\s*\.\s*Sleep\b|\bTask\s*\.\s*(?:Delay|Run|Factory)\b|\bTimer\b' },
        [pscustomobject]@{ Code = 'shell-execution'; Pattern = '(?i)\b(?:powershell|pwsh|cmd\.exe|bash)\b|/(?:bin/)?(?:ba)?sh\b|\bSystem\.Management\.Automation\b' },
        [pscustomobject]@{ Code = 'remote-url'; Pattern = '(?i)\b(?:https?|ftps?|wss?|file)\b\s*(?::|["'']\s*\+\s*["'']\s*:)|://' },
        [pscustomobject]@{ Code = 'package-reference'; Pattern = '(?i)\b(?:PackageReference|PackageDownload|dotnet\s+add\s+package|nuget\s*:|nuget\.exe)\b|#(?:r|load)\b' },
        [pscustomobject]@{ Code = 'obfuscated-source'; Pattern = '\\(?:u[0-9a-fA-F]{4}|U[0-9a-fA-F]{8})' },
        [pscustomobject]@{ Code = 'verification-spoof'; Pattern = '(?i)\bVERIFICATION\s+(?:PASSED|FAILED|INCONCLUSIVE)\b|##vso\[|::set-output' },
        [pscustomobject]@{ Code = 'conditional-reproduction'; Pattern = '\bMAUI_REPRODUCTION_ISSUE\b' },
        [pscustomobject]@{ Code = 'framework-behavior-switch'; Pattern = '(?i)\bSkipMeasureInvalidatedPropagation\s*=' }
    )
}

function Get-ReplicationUnsafeMatchDetail {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$ScanText,
        [Parameter(Mandatory = $true)][System.Text.RegularExpressions.Match]$Match
    )

    $lineNumber = ($ScanText.Substring(0, $Match.Index) -split "`n").Count
    $lineStart = $ScanText.LastIndexOf("`n", [Math]::Max($Match.Index - 1, 0)) + 1
    if ($Match.Index -eq 0) { $lineStart = 0 }
    $lineEnd = $ScanText.IndexOf("`n", $Match.Index)
    if ($lineEnd -lt 0) { $lineEnd = $ScanText.Length }
    $line = $ScanText.Substring($lineStart, $lineEnd - $lineStart)

    $sanitize = {
        param([string]$Value)
        $clean = [regex]::Replace($Value, '[\p{C}]', ' ').Trim()
        if ($clean.Length -gt 160) { $clean = $clean.Substring(0, 160) }
        return $clean
    }

    $token = & $sanitize $Match.Value
    $context = & $sanitize $line
    return "matched text '$token' on line $lineNumber -> $context"
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
    foreach ($entry in Get-ReplicationUnsafeSourcePatterns) {
        $match = [regex]::Match($scanText, $entry.Pattern)
        if ($match.Success) {
            throw "Candidate source '$Path' contains prohibited '$($entry.Code)' content: $(Get-ReplicationUnsafeMatchDetail -ScanText $scanText -Match $match)"
        }
    }
}

function Assert-ReplicationPlatformSourceSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Platform
    )

    if ([System.IO.Path]::GetExtension($Path) -ine '.cs') {
        return
    }

    $normalizedPath = $Path.Replace('\', '/')
    if ($normalizedPath -match '(?i)\.MacCatalyst\.cs$') {
        throw "Candidate source '$Path' uses an unsafe MacCatalyst filename. Use an .iOS.cs file with the repository's existing platform pattern."
    }

    $platformNamespaceRules = @(
        [pscustomobject]@{
            Name = 'UIKit'
            Pattern = '(?i)\b(?:UIKit|Foundation|CoreGraphics)\b'
            PathPattern = '(?i)\.iOS\.cs$|/(?:iOS|MacCatalyst)/'
            GuardPattern = '(?i)^(?:IOS|MACCATALYST)(?:\s*\|\|\s*(?:IOS|MACCATALYST))*$'
        },
        [pscustomobject]@{
            Name = 'Android'
            Pattern = '(?i)\b(?:Android|AndroidX)\s*\.'
            PathPattern = '(?i)\.Android\.cs$|/Android/'
            GuardPattern = '(?i)^ANDROID$'
        },
        [pscustomobject]@{
            Name = 'Windows'
            Pattern = '(?i)\b(?:Microsoft\s*\.\s*UI|Windows\s*\.\s*UI)\b'
            PathPattern = '(?i)\.Windows\.cs$|/Windows/'
            GuardPattern = '(?i)^WINDOWS$'
        }
    )

    foreach ($rule in $platformNamespaceRules) {
        $unscopedContent = Get-ReplicationUnscopedPlatformContent `
            -Content $Content `
            -GuardPattern $rule.GuardPattern
        if ($unscopedContent -match $rule.Pattern -and $normalizedPath -notmatch $rule.PathPattern) {
            throw "Candidate source '$Path' uses $($rule.Name) APIs without a matching platform-specific path."
        }
    }

    $unscopedUIKitContent = Get-ReplicationUnscopedPlatformContent `
        -Content $Content `
        -GuardPattern '(?i)^(?:IOS|MACCATALYST)(?:\s*\|\|\s*(?:IOS|MACCATALYST))*$'
    if ($Platform -ceq 'catalyst' -and $unscopedUIKitContent -match '(?i)\bUIKit\b' -and $normalizedPath -notmatch '(?i)\.iOS\.cs$|/(?:iOS|MacCatalyst)/') {
        throw "Catalyst candidate source '$Path' must place UIKit code in an .iOS.cs file or existing Apple-platform directory."
    }

    if ($Platform -ceq 'ios' -and $normalizedPath -match '(?i)\.iOS\.cs$') {
        $catalystIncludedContent = Get-ReplicationUnscopedPlatformContent `
            -Content $Content `
            -GuardPattern '(?i)^(?:!\s*MACCATALYST|IOS\s*&&\s*!\s*MACCATALYST|!\s*MACCATALYST\s*&&\s*IOS)$'
        if ($catalystIncludedContent -match '(?m)^\s*\[\s*(?:Fact|Test)\b') {
            throw "iOS candidate test '$Path' must exclude Mac Catalyst with a compile-time !MACCATALYST guard."
        }
    }
}

function Get-ReplicationUnscopedPlatformContent {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$GuardPattern
    )

    $frames = [System.Collections.Generic.List[object]]::new()
    $result = [System.Text.StringBuilder]::new()
    $isProtected = $false

    foreach ($line in $Content.Replace("`r`n", "`n").Split("`n")) {
        if ($line -match '^\s*#if\s+(?<expression>.+?)\s*$') {
            $parentProtected = $isProtected
            $guardedBranch = $Matches['expression'].Trim() -match $GuardPattern
            $frames.Add([pscustomobject]@{
                ParentProtected = $parentProtected
                GuardedBranch = $guardedBranch
            })
            $isProtected = $parentProtected -or $guardedBranch
            continue
        }
        if ($line -match '^\s*#elif\s+(?<expression>.+?)\s*$') {
            if ($frames.Count -gt 0) {
                $frame = $frames[$frames.Count - 1]
                $frame.GuardedBranch = $Matches['expression'].Trim() -match $GuardPattern
                $isProtected = $frame.ParentProtected -or $frame.GuardedBranch
            }
            continue
        }
        if ($line -match '^\s*#else\b') {
            if ($frames.Count -gt 0) {
                $frame = $frames[$frames.Count - 1]
                $frame.GuardedBranch = $false
                $isProtected = $frame.ParentProtected
            }
            continue
        }
        if ($line -match '^\s*#endif\b') {
            if ($frames.Count -gt 0) {
                $frame = $frames[$frames.Count - 1]
                $frames.RemoveAt($frames.Count - 1)
                $isProtected = $frame.ParentProtected
            }
            continue
        }

        if (-not $isProtected) {
            $null = $result.AppendLine($line)
        }
    }

    return $result.ToString()
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

    $constructorScanContent = $Content
    if ($Path.Replace('\', '/') -cmatch '^src/Controls/tests/TestCases\.Shared\.Tests/Tests/Issues/') {
        $canonicalUiTestConstructor = '(?ms)^\s*public\s+(?<class>[A-Za-z_]\w*)\s*(?:/\*.*?\*/\s*)?\(\s*TestDevice\s+(?<device>[A-Za-z_]\w*)\s*\)\s*:\s*base\s*\(\s*\k<device>\s*\)\s*\{\s*\}\s*$'
        $constructorScanContent = [regex]::Replace(
            $constructorScanContent,
            $canonicalUiTestConstructor,
            '')
    }

    $classNames = @(
        [regex]::Matches($Content, '\bclass\s+(?<name>[A-Za-z_]\w*)\b') |
            ForEach-Object { $_.Groups['name'].Value } |
            Sort-Object -Unique
    )
    foreach ($className in $classNames) {
        $constructorPattern = "(?m)^\s*(?:(?:public|internal|protected|private)\s+)?$([regex]::Escape($className))\s*\("
        if ($constructorScanContent -match $constructorPattern) {
            throw "Candidate test source '$Path' contains an unguarded test-class constructor."
        }
        $staticConstructorPattern = "(?m)^\s*static\s+$([regex]::Escape($className))\s*\("
        if ($constructorScanContent -match $staticConstructorPattern) {
            throw "Candidate test source '$Path' contains an unguarded static constructor."
        }
        $primaryConstructorPattern = "\bclass\s+$([regex]::Escape($className))\s*\("
        if ($constructorScanContent -match $primaryConstructorPattern) {
            throw "Candidate test source '$Path' contains an unguarded primary constructor."
        }
    }
}
