#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

function Get-ReplicationUnsafeSourcePatterns {
    # Scope 'raw' rules scan comments and string literals too, because hiding
    # their payload there still matters. Scope 'code' rules describe executable
    # API shape, so they scan with comments removed and must not match ordinary
    # English prose such as "process", "assembly", "timer" or "browser".
    # 'literal' follows the same comment-free path, but retains plain XAML
    # attribute values so a URL cannot hide in Source="...".
    return @(
        [pscustomobject]@{ Code = 'file-system'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*IO\b|\b(?:File|Directory)\s*\.\s*(?:Exists|Open|Create|Delete|Read|Write|Append|Copy|Move|Replace|Enumerate|Get|Set|Resolve)\w*|\bPath\s*\.\s*(?:Combine|Join|GetFullPath|GetTempPath|GetTempFileName|GetDirectoryName|GetFileName|GetFileNameWithoutExtension|GetExtension|GetRandomFileName|GetPathRoot)\b|\b(?:FileInfo|DirectoryInfo|DriveInfo|FileStream|StreamReader|StreamWriter|FileSystemWatcher|IsolatedStorage|StorageFile|StorageFolder|NSFileManager|NSData)\b|\b(?:Java|Android)\s*\.\s*IO\b|\b(?:XmlDocument|XDocument)\s*\.\s*Load\b|\bXmlReader\s*\.\s*Create\b' },
        [pscustomobject]@{ Code = 'http-client'; Scope = 'code'; Pattern = '(?i)\b(?:HttpClient|HttpMessageHandler|HttpRequestMessage|HttpWebRequest|SocketsHttpHandler|RestClient|GrpcChannel)\b' },
        [pscustomobject]@{ Code = 'network'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Net\b|\b(?:Dns|WebClient|WebRequest|FtpWebRequest|SmtpClient|HttpListener|Ping|TcpClient|TcpListener|UdpClient|WebSocket|ClientWebSocket|NSUrlSession|NSURLSession|NetworkStream)\b|\bSocket\s*[.(]|\bnew\s+\w*Socket\b|\b(?:Java|Android)\s*\.\s*Net\b' },
        [pscustomobject]@{ Code = 'service-model-network'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*ServiceModel\b|\b(?:ChannelFactory|WebChannelFactory|ClientBase|HttpBinding|BasicHttpBinding|WSHttpBinding|NetTcpBinding)\b' },
        [pscustomobject]@{ Code = 'xml-external-access'; Scope = 'code'; Pattern = '(?i)\b(?:XmlTextReader|XmlValidatingReader|XmlUrlResolver|XmlSecureResolver|XmlPreloadedResolver|XmlResolver|XmlDataDocument|XPathDocument|XslCompiledTransform|XmlSchemaSet)\b|\bXmlReader\s*\.\s*Create\b|\b(?:XmlReaderSettings|XmlDocument)\s*\.\s*XmlResolver\b|\bXmlResolver\s*=|\b(?:XmlDocument|XDocument|XElement|XPathDocument|XslCompiledTransform|XmlSchema)\s*\.\s*(?:Load|Read)\b|\.\s*(?:Read|Write|Infer)Xml(?:Schema)?\s*\(' },
        [pscustomobject]@{ Code = 'platform-network'; Scope = 'code'; Pattern = '(?i)\b(?:NSUrl|NSURL|NSUrlRequest|NSURLRequest|NSMutableUrlRequest|NSURLConnection|WKWebView|UIWebView|SFSafariViewController|ASWebAuthenticationSession|NSUrlSession|NSURLSession|NWConnection|CFNetwork)\b|\b(?:Android|Java)\s*\.\s*(?:Net|Webkit)\b|\b(?:HttpURLConnection|URLConnection|AndroidHttpClient|OkHttpClient|CustomTabsIntent|DownloadManager|WebViewClient)\b|\b(?:Intent|PendingIntent)\s*(?:[.(]|\b)|\b(?:Windows\s*\.\s*(?:Web\s*\.\s*Http|Foundation\s*\.\s*Uri|System\s*\.\s*Launcher)|Microsoft\s*\.\s*Web\s*\.\s*WebView2|CoreWebView2|WebView2|WinHttpHandler)\b|\b(?:LoadRequest|LoadUrl|LoadDataWithBaseURL|Navigate|NavigateWithHttpRequestMessage|LaunchUriAsync)\s*\(' },
        [pscustomobject]@{ Code = 'process-start'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Diagnostics\s*\.\s*Process\b|\bProcess\s*\.\s*(?:Start|GetCurrentProcess|GetProcess|GetProcesses|GetProcessById|EnterDebugMode|Kill)\b|\bnew\s+Process\s*[({]|\b(?:ProcessStartInfo|UseShellExecute|RedirectStandardOutput|RedirectStandardError|WaitForExit|Win32Exception|ManagementObject|NSTask|NSWorkspace|CreateProcess|ShellExecute|Runtime\s*\.\s*GetRuntime)\b' },
        [pscustomobject]@{ Code = 'reflection'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*(?:Reflection|Runtime\s*\.\s*Loader|Type)\b|\b(?:Assembly|Type|Activator|AppDomain|MethodInfo|MethodBase|PropertyInfo|FieldInfo|ConstructorInfo|DynamicMethod|Delegate)\s*\.\s*(?:Load|LoadFrom|LoadFile|LoadWithPartialName|CreateInstance|CreateDelegate|DynamicInvoke|Invoke|InvokeMember|GetMethod|GetMethods|GetField|GetFields|GetProperty|GetProperties|GetMember|GetMembers|GetTypes|GetConstructors|GetNestedTypes|GetRuntimeMethod|GetRuntimeMethods|GetRuntimeField|GetRuntimeFields|GetRuntimeProperty|GetRuntimeProperties|GetTypeFromHandle|GetType)\b|\b(?:Activator|AppDomain|MethodInfo|MethodBase|PropertyInfo|FieldInfo|ConstructorInfo|DynamicMethod|RuntimeMethodHandle|InvokeMember|GetMethod|GetMethods|GetField|GetFields|GetMember|GetMembers|GetProperties|GetConstructors|GetNestedTypes|GetRuntimeMethod|GetRuntimeMethods|GetRuntimeField|GetRuntimeFields|GetRuntimeProperty|GetRuntimeProperties)\b|(?<!Mapper\s*\.\s*)\bGetProperty\b|\bType\s*\.\s*GetType\b|\btypeof\s*\([^)]*\)\s*\.\s*Assembly\b|\bGetType\s*\(\s*\)\s*\.\s*(?!Name\b|FullName\b|ToString\b)' },
        [pscustomobject]@{ Code = 'dynamic-loading'; Scope = 'code'; Pattern = '(?i)\b(?:dynamic|ExpandoObject|CallSite|DynamicMetaObject|IDynamicMetaObjectProvider)\b|\bRuntimeHelpers\s*\.\s*(?:GetUninitializedObject|PrepareMethod|RunClassConstructor)\b' },
        [pscustomobject]@{ Code = 'native-code'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Runtime\s*\.\s*InteropServices\b(?!\s*\.\s*COMException\b)|\b(?:DllImport|LibraryImport|GeneratedDllImport|NativeLibrary|UnmanagedCallersOnly|GetDelegateForFunctionPointer|GCHandle)\b|\bMarshal\s*\.|(?-i:\bunsafe\s*[{(]|\bunsafe\s+(?:static|void|partial|class|struct|int|byte|char|fixed)\b|\bstackalloc\b|\bextern\s+(?:static|alias)\b|\bstatic\s+extern\b)' },
        [pscustomobject]@{ Code = 'environment-secrets'; Scope = 'code'; Pattern = '(?i)\busing\s+[A-Za-z_]\w*\s*=\s*(?:global\s*::)?System\s*\.\s*Environment\s*;|\b(?:System\s*\.\s*)?Environment\s*\.\s*(?:GetEnvironmentVariable|GetEnvironmentVariables|SetEnvironmentVariable|ExpandEnvironmentVariables)\b|\bEnvironmentVariableTarget\b|\b(?:System\s*\.\s*)?AppContext\s*\.\s*(?:GetData|SetData|BaseDirectory)\b|\b(?:GH_TOKEN|GITHUB_TOKEN|COPILOT_GITHUB_TOKEN|SYSTEM_ACCESSTOKEN|AZURE_STORAGE_KEY|AZURE_STORAGE_SAS_TOKEN)\b' },
        [pscustomobject]@{ Code = 'device-external-access'; Scope = 'code'; Pattern = '(?i)\b(?:Browser|Launcher|SecureStorage|FileSystem|Connectivity|Clipboard|Preferences)\s*\.|\b(?:ImageSource|HybridWebView|BlazorWebView|UrlWebViewSource|UriImageSource|FileImageSource|UIApplication|PendingIntent)\b|(?<![A-Za-z0-9_])(?:Source|BackgroundImageSource|ImageSource|IconImageSource)\s*=(?!=)|\bSetValue\s*\(\s*[A-Za-z_]\w*\s*\.\s*(?:Source|BackgroundImageSource|ImageSource|IconImageSource)Property\b' },
        [pscustomobject]@{ Code = 'webview'; Scope = 'literal'; Pattern = '(?i)<\s*(?:[A-Za-z_]\w*\s*:\s*)?(?:WebView|WebView2|HybridWebView|BlazorWebView)\b|\busing\s+[A-Za-z_]\w*\s*=\s*(?:(?:global\s*::)?[A-Za-z_]\w*\s*\.\s*)*(?:WebView|WebView2|HybridWebView|BlazorWebView|UrlWebViewSource|HtmlWebViewSource)\s*;|\bnew\s+(?:(?:global\s*::)?[A-Za-z_]\w*\s*\.\s*)*(?:WebView|WebView2|HybridWebView|BlazorWebView|UrlWebViewSource|HtmlWebViewSource)\b|\b(?:WebView|WebView2|HybridWebView|BlazorWebView|UrlWebViewSource|HtmlWebViewSource)\s*(?:<|\(|\.|[A-Za-z_]\w*\s*(?:[=;,)])|[),])|\b(?:as|typeof)\s*\(?\s*(?:(?:global\s*::)?[A-Za-z_]\w*\s*\.\s*)*(?:WebView|WebView2|HybridWebView|BlazorWebView)\b' },
        [pscustomobject]@{ Code = 'uri-construction'; Scope = 'code'; Pattern = '(?i)\b(?:System\s*\.\s*)?Uri(?:Builder)?\b|\b(?:Uri|UriBuilder)\s*\.\s*(?:Parse|TryCreate|EscapeDataString|UnescapeDataString)\b' },
        [pscustomobject]@{ Code = 'source-generator-analyzer'; Scope = 'code'; Pattern = '(?i)\bMicrosoft\s*\.\s*CodeAnalysis\b|\b(?:ISourceGenerator|IIncrementalGenerator|GeneratorInitializationContext|IncrementalGeneratorInitializationContext|GeneratorExecutionContext|DiagnosticAnalyzer(?:Attribute)?|AnalysisContext|GeneratorDriver|CSharpGeneratorDriver|Register(?:SourceOutput|ImplementationSourceOutput|PostInitializationOutput))\b|\[\s*(?:[A-Za-z_]\w*\s*\.\s*)?(?:Generator|DiagnosticAnalyzer)(?:Attribute)?\b' },
        [pscustomobject]@{ Code = 'module-initializer'; Scope = 'code'; Pattern = '(?i)\[\s*(?:[A-Za-z_]\w*\s*\.\s*)?ModuleInitializer(?:Attribute)?\b' },
        [pscustomobject]@{ Code = 'assembly-runtime-policy'; Scope = 'code'; Pattern = '(?i)\b(?:DefaultDllImportSearchPaths|DisableRuntimeMarshalling|SkipLocalsInit|UnverifiableCode|SecurityRules|SecurityPermission)(?:Attribute)?\b' },
        [pscustomobject]@{ Code = 'preprocessor-symbol'; Scope = 'code'; Pattern = '(?im)^\s*#\s*(?:define|undef)\b' },
        [pscustomobject]@{ Code = 'static-constructor'; Scope = 'code'; Pattern = '(?im)(?:^|[;{}])\s*(?:(?:public|internal|protected|private)\s+)?static\s+(?!class\b|struct\b|interface\b|void\b)[A-Za-z_]\w*\s*\(\s*\)' },
        [pscustomobject]@{ Code = 'global-exception-suppression'; Scope = 'code'; Pattern = '(?i)\b(?:UnhandledException|UnobservedTaskException|FirstChanceException|MarshalManagedException|AndroidEnvironment)\b'; Remedy = 'A reproduction must not take over the process-wide failure path. Suppressing the crash puts the app in a state a user never sees, and it hides the one symptom the harness can observe without the app reporting on itself. End the Appium plan with assertAppClosed instead, which now works on every platform. If the report names an exact managed exception type, wrap only the reported trigger in a try/catch for that exact type and set the semantic result element from the catch.' },
        [pscustomobject]@{ Code = 'delays-or-background-work'; Scope = 'code'; Pattern = '(?i)\bThread\s*\.\s*Sleep\b|\bTask\s*\.\s*(?:Delay|Run|Factory)\b|\b(?:DispatcherTimer|IDispatcherTimer)\b|\bSystem\s*\.\s*(?:Timers|Threading)\s*\.\s*Timer\b|\bnew\s+\w*Timer\s*\(|\b(?:Create|Start)Timer\s*\(|\bDispatchDelayed\b'; Remedy = 'Write one of these instead. Subscribe to the event that reports the change (Loaded, SizeChanged, PropertyChanged, or the control''s own event) and publish the result from its handler. Or post the measurement with Dispatcher.Dispatch(() => ...), which runs after the pending layout pass without waiting on the clock. Or give the page a separate check control and let the Appium plan tap trigger, wait, then tap check. Waiting on wall-clock time inside the app is never accepted, so re-sending it will fail this attempt again.' },
        [pscustomobject]@{ Code = 'shell-execution'; Scope = 'code'; Pattern = '(?i)\b(?:powershell|pwsh)(?:\.exe)?\s*(?:-|\.exe\b)|\bcmd\.exe\b|/(?:bin/)?(?:ba|z)?sh\b|\bbash\s+-|\bSystem\s*\.\s*Management\s*\.\s*Automation\b' },
        [pscustomobject]@{ Code = 'remote-url'; Scope = 'raw'; Pattern = '(?i)(?:\b(?:https?|ftps?|wss?|file|data|javascript|mailto)\s*:\s*(?://|[^\s<>\[\]]+)|://)' },
        [pscustomobject]@{ Code = 'encoded-url'; Scope = 'code'; Pattern = '(?i)\bFromBase64String\b|\b(?:Char|System\s*\.\s*Char)\s*\.\s*ConvertFromUtf32\b|\bSystem\s*\.\s*Text\s*\.\s*Encoding\b|\bEncoding\s*\.\s*(?:UTF8|Unicode|ASCII|BigEndianUnicode)\s*\.\s*GetString\b|\(\s*char\s*\)\s*(?:0x[0-9a-f]{1,8}|\d{2,7})|\bnew\s+string\s*\(\s*(?:(?:new\s+)?(?:char|byte)\s*\[|new\s*\[\s*\]\s*\{)' },
        [pscustomobject]@{ Code = 'package-reference'; Scope = 'raw'; Pattern = '(?i)\b(?:PackageReference|PackageDownload|dotnet\s+add\s+package|nuget\s*:|nuget\.exe)\b|#(?:r|load)\b' },
        [pscustomobject]@{ Code = 'project-build-script'; Scope = 'raw'; Pattern = '(?i)\b(?:ProjectReference|Directory\.Build|\.csproj\b|\.props\b|\.targets\b|<Project\b|<Target\b|<PropertyGroup\b|<ItemGroup\b|dotnet\s+(?:build|test|run|pack|restore)\b)\b' },
        [pscustomobject]@{ Code = 'obfuscated-source'; Scope = 'literal'; Pattern = '(?i)\\(?:x(?:2f|3a|68|70|73|74)|u00(?:2f|3a|68|70|73|74)|U000000(?:2f|3a|68|70|73|74))|&#(?:x(?:2f|3a|68|70|73|74)|(?:47|58|104|112|115|116));' },
        [pscustomobject]@{ Code = 'prompt-injection'; Scope = 'literal'; Pattern = '(?i)<\|(?:system|developer|assistant|user|tool)\|>|\[/?INST\]|<</?SYS>>|\b(?:ignore|disregard|override|forget)\b[^\r\n]{0,80}\b(?:instructions?|prompts?|messages?)\b' },
        [pscustomobject]@{ Code = 'pipeline-log-command'; Scope = 'literal'; Pattern = '(?i)##vso\[|##\[|::(?:set-output|add-mask|error|warning|notice)\b' },
        [pscustomobject]@{ Code = 'artifact-reference'; Scope = 'literal'; Pattern = '(?i)\b(?:candidate|reproduction-result|verification-result|fix|test)\.(?:json|patch)\b|\b(?:issue-(?:agent-)?context|CustomAgentLogsTmp|IssueReplication|trusted-github)\b' },
        [pscustomobject]@{ Code = 'verification-spoof'; Scope = 'raw'; Pattern = '(?i)\bVERIFICATION\s+(?:PASSED|FAILED|INCONCLUSIVE)\b|##vso\[|##\[|::set-output' },
        [pscustomobject]@{ Code = 'conditional-reproduction'; Scope = 'raw'; Pattern = '\bMAUI_REPRODUCTION_ISSUE\b' },
        [pscustomobject]@{ Code = 'framework-behavior-switch'; Scope = 'raw'; Pattern = '(?i)\bSkipMeasureInvalidatedPropagation\s*=' }
    )
}

function Get-ReplicationCommentFreeText {
    <#
    .SYNOPSIS
        Blanks comment spans while preserving offsets, so 'code' scope rules
        never fire on prose and a '//' inside a string literal cannot be used
        to hide executable code from the scanner.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][string]$Path,
        [switch]$PreserveXamlAttributeValues
    )

    $builder = [Text.StringBuilder]::new($Text)
    $length = $Text.Length
    $isXaml = [IO.Path]::GetExtension($Path) -ieq '.xaml'

    $blank = {
        param([int]$Start, [int]$End)
        for ($i = $Start; $i -lt $End -and $i -lt $length; $i++) {
            if ($Text[$i] -ne "`n") { $null = $builder.Replace($Text[$i], ' ', $i, 1) }
        }
    }

    if ($isXaml) {
        foreach ($m in [regex]::Matches($Text, '<!--[\s\S]*?(?:-->|$)')) {
            & $blank $m.Index ($m.Index + $m.Length)
        }

        # A plain attribute value is displayed text, not an API call. Leaving it
        # visible made a caption reading "WebView Sizing Demo" look like use of
        # the control. Markup extensions stay visible because they do resolve to
        # real members, and every 'raw' rule still scans the untouched source.
        if (-not $PreserveXamlAttributeValues) {
            foreach ($m in [regex]::Matches($Text, '=\s*(?<quote>["''])(?<value>[^"'']*)\k<quote>')) {
                $value = $m.Groups['value']
                if ($value.Value.Contains('{')) { continue }
                & $blank $value.Index ($value.Index + $value.Length)
            }
        }

        return $builder.ToString()
    }

    $index = 0
    while ($index -lt $length) {
        $char = $Text[$index]
        $next = if ($index + 1 -lt $length) { $Text[$index + 1] } else { [char]0 }

        if ($char -eq '/' -and $next -eq '/') {
            $end = $Text.IndexOf("`n", $index)
            if ($end -lt 0) { $end = $length }
            & $blank $index $end
            $index = $end
            continue
        }
        if ($char -eq '/' -and $next -eq '*') {
            $end = $Text.IndexOf('*/', $index + 2)
            $end = if ($end -lt 0) { $length } else { $end + 2 }
            & $blank $index $end
            $index = $end
            continue
        }
        if ($char -eq '"' -and $index + 2 -lt $length -and
            $Text[$index + 1] -eq '"' -and $Text[$index + 2] -eq '"') {
            $fence = 0
            while ($index + $fence -lt $length -and $Text[$index + $fence] -eq '"') { $fence++ }
            $terminator = '"' * $fence
            $end = $Text.IndexOf($terminator, $index + $fence)
            $index = if ($end -lt 0) { $length } else { $end + $fence }
            continue
        }
        if ($char -eq '@' -and $next -eq '"') {
            $index += 2
            while ($index -lt $length) {
                if ($Text[$index] -eq '"') {
                    if ($index + 1 -lt $length -and $Text[$index + 1] -eq '"') { $index += 2; continue }
                    $index++
                    break
                }
                $index++
            }
            continue
        }
        if ($char -eq '"' -or $char -eq "'") {
            $quote = $char
            $index++
            while ($index -lt $length) {
                if ($Text[$index] -eq '\') { $index += 2; continue }
                if ($Text[$index] -eq $quote) { $index++; break }
                if ($Text[$index] -eq "`n") { break }
                $index++
            }
            continue
        }

        $index++
    }

    return $builder.ToString()
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

function Get-ReplicationSourceSafetyScanContext {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $scanText = $Content.Replace("`r`n", "`n")
    $scanText = [regex]::Replace(
        $scanText,
        '(?i)http://schemas\.microsoft\.com/(?:dotnet/2021/maui|winfx/2009/xaml|winfx/2006/xaml/presentation|netfx/2007/xaml/presentation)',
        ''
    )
    $isXaml = [System.IO.Path]::GetExtension($Path) -ieq '.xaml'
    if ($isXaml) {
        # Namespace identifiers are schema/type mappings, not fetched resources.
        # XAML is parsed separately before this scan, so remove complete xmlns
        # declarations rather than treating their standard http:// identifiers
        # as outbound URLs.
        $scanText = [regex]::Replace(
            $scanText,
            '(?i)\s+xmlns(?::[A-Za-z_]\w*)?\s*=\s*(?<quote>["''])[^"'']*\k<quote>',
            ''
        )
    }

    $codeText = Get-ReplicationCommentFreeText -Text $scanText -Path $Path
    $literalText = if ($isXaml) {
        Get-ReplicationCommentFreeText `
            -Text $scanText `
            -Path $Path `
            -PreserveXamlAttributeValues
    } else {
        $codeText
    }

    return [pscustomobject]@{
        ScanText = $scanText
        CodeText = $codeText
        LiteralText = $literalText
    }
}

function Get-ReplicationLiteralFragments {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    $fragments = [System.Collections.Generic.List[string]]::new()
    foreach ($match in [regex]::Matches(
            $Text,
            '(?s)(?:\$+)?(?<delimiter>"{3,})(?<value>.*?)(?:\k<delimiter>)'
        )) {
        $value = $match.Groups['value'].Value
        if ($value.Length -le 4096) {
            [void]$fragments.Add($value)
        }
    }
    foreach ($match in [regex]::Matches(
            $Text,
            '(?s)(?:\$@|@\$|\$|@)?"(?<value>(?:""|\\.|[^"])*)"'
        )) {
        $value = $match.Groups['value'].Value.Replace('""', '"')
        $value = [regex]::Replace($value, '\\(?<escaped>["\\])', '${escaped}')
        if ($value.Length -le 4096) {
            [void]$fragments.Add($value)
        }
    }
    foreach ($match in [regex]::Matches($Text, "(?s)'(?<value>\\.|[^'])'")) {
        $value = $match.Groups['value'].Value
        if ($value.Length -le 4096) {
            [void]$fragments.Add($value)
        }
    }

    return $fragments.ToArray()
}

function Test-ReplicationRemoteUrlText {
    [CmdletBinding()]
    param([AllowEmptyString()][string]$Text)

    $pattern = '(?i)(?:\b(?:https?|ftps?|wss?|file|data|javascript|mailto)\s*:\s*(?://|[^\s<>\[\]]+)|://)'
    $candidate = [string]$Text
    if ([regex]::IsMatch($candidate, $pattern)) {
        return $true
    }
    if ($candidate -match '(?i)\bhttps?[A-Za-z0-9_.{}-]{0,32}//') {
        return $true
    }

    for ($pass = 0; $pass -lt 2 -and $candidate -match '%[0-9a-fA-F]{2}'; $pass++) {
        try {
            $decoded = [uri]::UnescapeDataString($candidate)
        } catch {
            break
        }
        if ($decoded -ceq $candidate) {
            break
        }
        $candidate = $decoded
        if ([regex]::IsMatch($candidate, $pattern)) {
            return $true
        }
    }

    return $false
}

function Test-ReplicationBase64Url {
    [CmdletBinding()]
    param([AllowEmptyString()][string]$Text)

    $candidate = ([string]$Text).Trim()
    if (
        $candidate.Length -lt 8 -or
        $candidate.Length -gt 4096 -or
        $candidate.Length % 4 -ne 0 -or
        $candidate -notmatch '^[A-Za-z0-9+/]+={0,2}$'
    ) {
        return $false
    }

    try {
        $bytes = [Convert]::FromBase64String($candidate)
        $decoded = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
    } catch {
        return $false
    }

    return Test-ReplicationRemoteUrlText -Text $decoded
}

function Get-ReplicationSimpleStringBindings {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$CodeText)

    $bindings = @{}
    for ($pass = 0; $pass -lt 4; $pass++) {
        $changed = $false
        foreach ($match in [regex]::Matches(
                $CodeText,
                '(?is)\b(?:const\s+string|string|var)\s+(?<name>[A-Za-z_]\w*)\s*=\s*(?<expression>[^;]{1,4096});'
            )) {
            $name = $match.Groups['name'].Value
            if ($bindings.ContainsKey($name)) { continue }

            $expression = $match.Groups['expression'].Value
            $resolved = [regex]::Replace(
                $expression,
                '\b[A-Za-z_]\w*\b',
                {
                    param($identifier)
                    if ($bindings.ContainsKey($identifier.Value)) {
                        '"' + [string]$bindings[$identifier.Value] + '"'
                    } else {
                        $identifier.Value
                    }
                }
            )
            $fragments = @(Get-ReplicationLiteralFragments -Text $resolved)
            $remainder = [regex]::Replace(
                $resolved,
                '(?s)(?:\$@|@\$|\$|@)?"(?:""|\\.|[^"])*"',
                ''
            )
            $remainder = [regex]::Replace($remainder, '\s*\+\s*', '')
            if ($fragments.Count -gt 0 -and [string]::IsNullOrWhiteSpace($remainder)) {
                $bindings[$name] = [string]::Concat([string[]]$fragments)
                $changed = $true
            }
        }
        if (-not $changed) { break }
    }

    return $bindings
}

function Invoke-ReplicationJoinedLiteralSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Patterns,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (Test-ReplicationRemoteUrlText -Text $Text) {
        throw "Candidate source '$Path' contains prohibited 'remote-url' content assembled from literals."
    }
    if (Test-ReplicationBase64Url -Text $Text) {
        throw "Candidate source '$Path' contains prohibited 'encoded-url' content assembled from literals."
    }

    foreach ($entry in $Patterns) {
        $match = [regex]::Match($Text, $entry.Pattern)
        if ($match.Success) {
            throw "Candidate source '$Path' contains prohibited '$($entry.Code)' content assembled from literals."
        }
    }
}

function Invoke-ReplicationObfuscatedUrlSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$CodeText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$LiteralText,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Patterns
    )

    $literalFragments = @(Get-ReplicationLiteralFragments -Text $LiteralText)
    foreach ($fragment in $literalFragments) {
        if (Test-ReplicationRemoteUrlText -Text $fragment) {
            throw "Candidate source '$Path' contains prohibited 'remote-url' content encoded in a literal."
        }
        if (Test-ReplicationBase64Url -Text $fragment) {
            throw "Candidate source '$Path' contains prohibited 'encoded-url' content encoded in a literal."
        }
    }

    $bindings = Get-ReplicationSimpleStringBindings -CodeText $CodeText
    foreach ($binding in $bindings.GetEnumerator()) {
        Invoke-ReplicationJoinedLiteralSafety `
            -Text ([string]$binding.Value) `
            -Patterns $Patterns `
            -Path $Path
    }

    $allFragments = @(Get-ReplicationLiteralFragments -Text $CodeText)
    if ($allFragments.Count -gt 1) {
        Invoke-ReplicationJoinedLiteralSafety `
            -Text ([string]::Concat([string[]]$allFragments)) `
            -Patterns $Patterns `
            -Path $Path
    }

    foreach ($statement in ($CodeText -split ';')) {
        if ($statement -notmatch '(?i)(?:\+|\.\s*(?:Concat|Join|Append|Format|Replace|Insert|Substring)\s*\()') {
            continue
        }
        if ($statement.Length -gt 4096) {
            throw "Candidate source '$Path' contains an oversized concatenated string that could obscure a URL."
        }

        $resolvedStatement = $statement
        foreach ($binding in $bindings.GetEnumerator()) {
            $resolvedStatement = [regex]::Replace(
                $resolvedStatement,
                "(?<![A-Za-z0-9_])$([regex]::Escape([string]$binding.Key))(?![A-Za-z0-9_])",
                '"' + [string]$binding.Value + '"')
        }
        $fragments = @(Get-ReplicationLiteralFragments -Text $resolvedStatement)
        if ($fragments.Count -eq 0) {
            continue
        }
        $joined = [string]::Concat([string[]]$fragments)
        Invoke-ReplicationJoinedLiteralSafety `
            -Text $joined `
            -Patterns $Patterns `
            -Path $Path
    }

    foreach ($interpolation in [regex]::Matches(
            $CodeText,
            '(?s)(?:\$@|@\$|\$)"(?<value>(?:""|\\.|[^"])*)"'
        )) {
        $resolved = $interpolation.Groups['value'].Value
        for ($pass = 0; $pass -lt 4; $pass++) {
            $previous = $resolved
            $resolved = [regex]::Replace(
                $resolved,
                '\{\s*(?<name>[A-Za-z_]\w*)\s*(?:,\s*-?\d+)?(?:\:[^}]*)?\}',
                {
                    param($reference)
                    $name = $reference.Groups['name'].Value
                    if ($bindings.ContainsKey($name)) {
                        [string]$bindings[$name]
                    } else {
                        $reference.Value
                    }
                }
            )
            if ($resolved -ceq $previous) { break }
        }
        if ($resolved -cne $interpolation.Groups['value'].Value) {
            Invoke-ReplicationJoinedLiteralSafety `
                -Text $resolved `
                -Patterns $Patterns `
                -Path $Path
        }
    }
}

function Invoke-ReplicationUnsafeSourceCapabilities {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Patterns
    )

    $scan = Get-ReplicationSourceSafetyScanContext -Content $Content -Path $Path
    foreach ($entry in $Patterns) {
        $scope = if ($entry.PSObject.Properties['Scope']) { [string]$entry.Scope } else { 'raw' }
        $target = switch ($scope) {
            'code' { $scan.CodeText }
            'literal' { $scan.LiteralText }
            default { $scan.ScanText }
        }
        $match = [regex]::Match($target, $entry.Pattern)
        if ($match.Success) {
            $detail = "Candidate source '$Path' contains prohibited '$($entry.Code)' content: $(Get-ReplicationUnsafeMatchDetail -ScanText $target -Match $match)"
            if ($entry.PSObject.Properties['Remedy'] -and -not [string]::IsNullOrWhiteSpace($entry.Remedy)) {
                $detail += " $($entry.Remedy)"
            }

            throw $detail
        }
    }

    Invoke-ReplicationObfuscatedUrlSafety `
        -CodeText $scan.CodeText `
        -LiteralText $scan.LiteralText `
        -Path $Path `
        -Patterns $Patterns
    return $scan
}

function Get-ReplicationProductFixUnsafePatterns {
    $productFixCodes = @(
        'file-system',
        'http-client',
        'network',
        'service-model-network',
        'xml-external-access',
        'platform-network',
        'process-start',
        'reflection',
        'dynamic-loading',
        'native-code',
        'environment-secrets',
        'device-external-access',
        'webview',
        'uri-construction',
        'source-generator-analyzer',
        'module-initializer',
        'assembly-runtime-policy',
        'preprocessor-symbol',
        'static-constructor',
        'shell-execution',
        'remote-url',
        'encoded-url',
        'package-reference',
        'project-build-script',
        'obfuscated-source',
        'prompt-injection',
        'pipeline-log-command',
        'artifact-reference',
        'verification-spoof'
    )

    return @(Get-ReplicationUnsafeSourcePatterns |
        Where-Object { $_.Code -in $productFixCodes } |
        ForEach-Object {
            if ($_.Code -ceq 'remote-url') {
                [pscustomobject]@{
                    Code = $_.Code
                    Scope = 'literal'
                    Pattern = $_.Pattern
                }
            } else {
                $_
            }
        })
}

function Assert-ReplicationProductFixSafety {
    <#
    .SYNOPSIS
        Rejects capabilities present in a complete post-patch product source.

    .DESCRIPTION
        Callers pass the complete post-patch file. This deliberately blocks an
        otherwise harmless-looking guard deletion or condition inversion from
        activating a dangerous sink that already existed elsewhere in the file.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [Alias('AddedContent', 'AddedLines')]
        [AllowEmptyString()]
        [string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $extension = [IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($extension -notin @('.cs', '.xaml')) {
        throw "Product fix source '$Path' has an unsupported extension."
    }
    if ($Content.Contains("`r`n")) {
        $Content = $Content.Replace("`r`n", "`n")
    }
    if ($Content.Contains("`r")) {
        throw "Product fix source '$Path' contains non-normalized line endings."
    }
    if ([string]::IsNullOrWhiteSpace($Content)) {
        throw "Product fix source '$Path' is empty."
    }

    $null = Invoke-ReplicationUnsafeSourceCapabilities `
        -Content $Content `
        -Path $Path `
        -Patterns @(Get-ReplicationProductFixUnsafePatterns)
}

function Get-ReplicationCSharpMemberRecords {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    try {
        $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($Content)
    } catch {
        throw "Product fix source '$Path' could not be parsed with the offline Roslyn parser."
    }
    $errors = @($tree.GetDiagnostics() | Where-Object {
        [string]$_.Severity -ceq 'Error'
    } | Select-Object -First 8)
    if ($errors.Count -gt 0) {
        throw "Product fix source '$Path' is not syntactically valid C#."
    }

    $root = $tree.GetRoot()
    $records = [Collections.Generic.List[object]]::new()
    $occurrences = @{}
    $memberKinds = @(
        'MethodDeclarationSyntax',
        'ConstructorDeclarationSyntax',
        'DestructorDeclarationSyntax',
        'PropertyDeclarationSyntax',
        'IndexerDeclarationSyntax',
        'FieldDeclarationSyntax',
        'EventDeclarationSyntax',
        'EventFieldDeclarationSyntax',
        'OperatorDeclarationSyntax',
        'ConversionOperatorDeclarationSyntax',
        'DelegateDeclarationSyntax',
        'GlobalStatementSyntax'
    )

    foreach ($usingNode in @($root.DescendantNodes() | Where-Object {
        $_.GetType().Name -eq 'UsingDirectiveSyntax'
    })) {
        $text = $usingNode.ToString()
        $key = "using:$text"
        if (-not $occurrences.ContainsKey($key)) { $occurrences[$key] = 0 }
        $occurrences[$key]++
        $records.Add([pscustomobject]@{
            Key = "$key#$($occurrences[$key])"
            Text = $text
            Kind = 'using'
        })
    }

    foreach ($attributeList in @($root.AttributeLists | Where-Object {
        $null -ne $_.Target -and
        $_.Target.Identifier.ValueText -in @('assembly', 'module')
    })) {
        $text = $attributeList.ToFullString()
        $key = "compilation-attribute:$($attributeList.Target.Identifier.ValueText):$text"
        if (-not $occurrences.ContainsKey($key)) { $occurrences[$key] = 0 }
        $occurrences[$key]++
        $records.Add([pscustomobject]@{
            Key = "$key#$($occurrences[$key])"
            Text = $text
            Kind = 'compilation-attribute'
        })
    }

    $nullPredicate = [System.Func[Microsoft.CodeAnalysis.SyntaxNode, bool]]$null
    foreach ($directive in @($root.DescendantTrivia($nullPredicate, $true) | Where-Object {
        $_.GetStructure() -and
        $_.GetStructure().GetType().Name -match '^(?:Define|Undef)DirectiveTriviaSyntax$'
    })) {
        $text = $directive.ToFullString()
        $key = "preprocessor:$text"
        if (-not $occurrences.ContainsKey($key)) { $occurrences[$key] = 0 }
        $occurrences[$key]++
        $records.Add([pscustomobject]@{
            Key = "$key#$($occurrences[$key])"
            Text = $text
            Kind = 'preprocessor'
        })
    }

    foreach ($node in @($root.DescendantNodes() | Where-Object {
        $_.GetType().Name -in $memberKinds
    })) {
        $kind = $node.GetType().Name
        $containers = @($node.Ancestors() | Where-Object {
            $_.GetType().Name -match '^(?:Class|Struct|Interface|Record)DeclarationSyntax$'
        } | ForEach-Object { $_.Identifier.ValueText })
        [array]::Reverse($containers)
        $name = if ($node.PSObject.Properties['Identifier']) {
            [string]$node.Identifier.ValueText
        } elseif ($node.PSObject.Properties['Declaration']) {
            (@($node.Declaration.Variables | ForEach-Object {
                $_.Identifier.ValueText
            }) -join ',')
        } elseif ($node.PSObject.Properties['OperatorToken']) {
            [string]$node.OperatorToken.ValueText
        } else {
            $kind
        }
        $parameters = if ($node.PSObject.Properties['ParameterList'] -and
            $null -ne $node.ParameterList) {
            $node.ParameterList.ToString()
        } else {
            ''
        }
        $baseKey = "$($containers -join '.')|$kind|$name|$parameters"
        if (-not $occurrences.ContainsKey($baseKey)) { $occurrences[$baseKey] = 0 }
        $occurrences[$baseKey]++
        $records.Add([pscustomobject]@{
            Key = "$baseKey#$($occurrences[$baseKey])"
            Text = $node.ToFullString()
            Kind = $kind
        })
    }

    foreach ($node in @($root.DescendantNodes() | Where-Object {
        $_.GetType().Name -match '^(?:Class|Struct|Interface|Record|Enum)DeclarationSyntax$'
    })) {
        $full = $node.ToFullString()
        $brace = $full.IndexOf('{')
        $header = if ($brace -ge 0) { $full.Substring(0, $brace + 1) } else { $full }
        $containers = @($node.Ancestors() | Where-Object {
            $_.GetType().Name -match '^(?:Class|Struct|Interface|Record)DeclarationSyntax$'
        } | ForEach-Object { $_.Identifier.ValueText })
        [array]::Reverse($containers)
        $baseKey = "$($containers -join '.')|type|$($node.Identifier.ValueText)"
        if (-not $occurrences.ContainsKey($baseKey)) { $occurrences[$baseKey] = 0 }
        $occurrences[$baseKey]++
        $records.Add([pscustomobject]@{
            Key = "$baseKey#$($occurrences[$baseKey])"
            Text = $header
            Kind = 'type'
        })
    }

    return @($records)
}

function Get-ReplicationXamlElementRecords {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $document = [Xml.XmlDocument]::new()
    $document.XmlResolver = $null
    try {
        $reader = [Xml.XmlReader]::Create([IO.StringReader]::new($Content), $settings)
        try { $document.Load($reader) } finally { $reader.Dispose() }
    } catch {
        throw "Product fix source '$Path' is not well-formed XAML."
    }

    $records = [Collections.Generic.List[object]]::new()
    foreach ($element in @($document.SelectNodes('//*'))) {
        $segments = [Collections.Generic.List[string]]::new()
        $cursor = $element
        while ($cursor -and $cursor.NodeType -eq [Xml.XmlNodeType]::Element) {
            $ordinal = 1
            $sibling = $cursor.PreviousSibling
            while ($sibling) {
                if ($sibling.NodeType -eq [Xml.XmlNodeType]::Element -and
                    $sibling.LocalName -ceq $cursor.LocalName -and
                    $sibling.NamespaceURI -ceq $cursor.NamespaceURI) {
                    $ordinal++
                }
                $sibling = $sibling.PreviousSibling
            }
            $segments.Insert(0, "$($cursor.NamespaceURI)|$($cursor.LocalName)[$ordinal]")
            $cursor = $cursor.ParentNode
        }
        $attributes = @($element.Attributes | Where-Object {
            $_.NamespaceURI -cne 'http://www.w3.org/2000/xmlns/'
        } | ForEach-Object {
            "$($_.Name)=`"$($_.Value)`""
        } | Sort-Object -CaseSensitive)
        $records.Add([pscustomobject]@{
            Key = $segments -join '/'
            Header = "<$($element.Name) $($attributes -join ' ')>"
            Full = $element.OuterXml
        })
    }
    return @($records)
}

function Assert-ReplicationProductFixDeltaSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$BeforeContent,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$AfterContent,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $BeforeContent = $BeforeContent.Replace("`r`n", "`n").TrimEnd("`r", "`n") + "`n"
    $AfterContent = $AfterContent.Replace("`r`n", "`n").TrimEnd("`r", "`n") + "`n"
    if ($BeforeContent -ceq $AfterContent) {
        throw "Product fix source '$Path' is unchanged."
    }
    $extension = [IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($extension -eq '.xaml') {
        $beforeRecords = @{}
        foreach ($record in @(Get-ReplicationXamlElementRecords -Content $BeforeContent -Path $Path)) {
            $beforeRecords[$record.Key] = $record
        }
        $afterRecords = @{}
        foreach ($record in @(Get-ReplicationXamlElementRecords -Content $AfterContent -Path $Path)) {
            $afterRecords[$record.Key] = $record
        }
        $removed = @($beforeRecords.Keys | Where-Object {
            -not $afterRecords.ContainsKey($_)
        })
        if ($removed.Count -gt 0) {
            throw "Product fix source '$Path' removes a complete XAML element, which the fail-closed safety scan does not permit."
        }

        $changedKeys = @($afterRecords.Keys | Where-Object {
            -not $beforeRecords.ContainsKey($_) -or
            $afterRecords[$_].Full -cne $beforeRecords[$_].Full
        })
        $smallestChangedKeys = @($changedKeys | Where-Object {
            $candidate = "$_/"
            -not @($changedKeys | Where-Object {
                $_.StartsWith($candidate, [StringComparison]::Ordinal)
            }).Count
        })
        foreach ($key in $smallestChangedKeys) {
            $record = $afterRecords[$key]
            if (-not $beforeRecords.ContainsKey($key)) {
                Assert-ReplicationProductFixSafety -Content $record.Full -Path $Path
            } elseif ($record.Header -cne $beforeRecords[$record.Key].Header) {
                Assert-ReplicationProductFixSafety -Content $record.Header -Path $Path
            } else {
                Assert-ReplicationProductFixSafety -Content $record.Full -Path $Path
            }
        }
        return
    }
    if ($extension -ne '.cs') {
        throw "Product fix source '$Path' has an unsupported extension."
    }

    $beforeRecords = @{}
    foreach ($record in @(Get-ReplicationCSharpMemberRecords -Content $BeforeContent -Path $Path)) {
        $beforeRecords[$record.Key] = $record
    }
    $afterRecords = @{}
    $allAfterRecords = @(
        Get-ReplicationCSharpMemberRecords -Content $AfterContent -Path $Path
    )
    $aliasRecords = @($allAfterRecords | Where-Object {
        $_.Kind -ceq 'using' -and $_.Text -match '(?i)^\s*using\s+(?<alias>[A-Za-z_]\w*)\s*='
    } | ForEach-Object {
        [pscustomobject]@{
            Alias = [regex]::Match(
                $_.Text,
                '(?i)^\s*using\s+(?<alias>[A-Za-z_]\w*)\s*='
            ).Groups['alias'].Value
            Text = $_.Text
        }
    })
    $fieldRecords = @($allAfterRecords | Where-Object {
        $_.Kind -in @('FieldDeclarationSyntax', 'EventFieldDeclarationSyntax')
    })
    foreach ($record in $allAfterRecords) {
        $afterRecords[$record.Key] = $record
        if (-not $beforeRecords.ContainsKey($record.Key) -or
            $record.Text -cne $beforeRecords[$record.Key].Text) {
            $scanText = $record.Text
            foreach ($alias in $aliasRecords) {
                if ($record.Text -match "(?<![A-Za-z0-9_])$([regex]::Escape($alias.Alias))(?![A-Za-z0-9_])") {
                    $scanText = $alias.Text + "`n" + $scanText
                }
            }
            foreach ($field in $fieldRecords) {
                $fieldName = ($field.Key -split '\|')[-2]
                foreach ($name in ($fieldName -split ',')) {
                    if ($name -and
                        $record.Text -match "(?<![A-Za-z0-9_])$([regex]::Escape($name))(?![A-Za-z0-9_])") {
                        $scanText = $field.Text + "`n" + $scanText
                        break
                    }
                }
            }
            Assert-ReplicationProductFixSafety -Content $scanText -Path $Path
        }
    }

    $removed = @($beforeRecords.Keys | Where-Object {
        -not $afterRecords.ContainsKey($_) -and
        $beforeRecords[$_].Kind -cne 'using'
    })
    if ($removed.Count -gt 0) {
        throw "Product fix source '$Path' removes a complete C# member, which the fail-closed safety scan does not permit."
    }
}

function Get-ReplicationFixPathPolicyRejection {
    [CmdletBinding()]
    param(
        [AllowEmptyString()][string]$Path,
        [string]$RepositoryRoot = ''
    )

    $normalized = ([string]$Path).Replace('\', '/')
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return 'is empty'
    }
    if ($normalized -ne $normalized.Trim()) {
        return 'has leading or trailing whitespace'
    }
    if ($Path -match '\\') {
        return 'uses a backslash; paths must be repository-relative with forward slashes'
    }
    if ([IO.Path]::IsPathRooted($normalized) -or $normalized -match '^[A-Za-z]:') {
        return 'is absolute; paths must be repository-relative'
    }
    if (($normalized -split '/') -contains '..' -or $normalized -match '(?i)(?:^|/)%2e(?:%2e)?(?:/|$)') {
        return 'escapes the repository with a traversal segment'
    }

    $allowedRoot = $normalized -cmatch ('^src/(?:' +
        'Controls/(?:Maps/|Foldable/)?src' +
        '|Core/(?:maps/)?src' +
        '|Essentials/src' +
        '|Graphics/src' +
        '|BlazorWebView/src' +
        '|Compatibility/(?:Core|Maps|Material|Android\.AppLinks)/src' +
        ')/')
    if (-not $allowedRoot) {
        return 'is outside the established runtime product source directories'
    }

    if ($normalized -match '(?i)(?:^|/)[^/]*(?:BindingSourceGen|SourceGen|SourceGenerator|Generator|Analyzer|CodeFix|Build[._-]?Tasks?|BuildTargets?|Targets?|(?:Core|Xaml)\.Design|Resizetizer|Tooling|Tools|Provisioning|Workloads?|Packaging|Packs?)[^/]*(?:/|$)') {
        return 'targets source-generation, analyzer, build-task, tooling, provisioning, workload, or packaging code'
    }
    if ($normalized -match '(?i)(?:^|/)(?:tests?|testcases[^/]*|obj|bin|snapshots?)(?:/|$)') {
        return 'targets test, generated, or snapshot code'
    }
    $fileName = [IO.Path]::GetFileName($normalized)
    if ($fileName -match '(?i)^(?:AssemblyInfo|GlobalUsings|.+\.AssemblyAttributes)\.cs$' -or
        $fileName -match '(?i)\.(?:g|designer|generated)\.cs$') {
        return 'targets generated or assembly-wide build input'
    }
    $extension = [IO.Path]::GetExtension($normalized).ToLowerInvariant()
    if ($extension -notin @('.cs', '.xaml')) {
        return "has extension '$extension'; a fix may only change .cs or .xaml runtime product source"
    }

    if (-not [string]::IsNullOrWhiteSpace($RepositoryRoot)) {
        $root = [IO.Path]::GetFullPath($RepositoryRoot)
        $fullPath = [IO.Path]::GetFullPath((Join-Path $root $normalized))
        $rootPrefix = $root.TrimEnd(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar
        ) + [IO.Path]::DirectorySeparatorChar
        if (-not $fullPath.StartsWith($rootPrefix, [StringComparison]::Ordinal)) {
            return 'resolves outside the repository'
        }
    }

    return $null
}

function Assert-ReplicationFixSources {
    <#
    .SYNOPSIS
        Scans every complete changed product source, optionally after applying a
        strictly parsed patch to a clean trusted checkout.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string[]]$Paths,
        [string]$PatchPath = ''
    )

    $root = [IO.Path]::GetFullPath($RepositoryRoot)
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        throw 'Complete-file fix scan repository root does not exist.'
    }

    $normalizedPaths = @($Paths | Sort-Object -Unique)
    if ($normalizedPaths.Count -eq 0) {
        throw 'Complete-file fix scan requires at least one product path.'
    }
    foreach ($relative in $normalizedPaths) {
        $pathReason = Get-ReplicationFixPathPolicyRejection `
            -Path $relative `
            -RepositoryRoot $root
        if ($pathReason) {
            throw "Product fix path '$relative' $pathReason."
        }
    }

    $applyPatch = -not [string]::IsNullOrWhiteSpace($PatchPath)
    $patch = ''
    if ($applyPatch) {
        $patch = [IO.Path]::GetFullPath($PatchPath)
        if (-not (Test-Path -LiteralPath $patch -PathType Leaf)) {
            throw 'Complete-file fix scan patch does not exist.'
        }
        $patchItem = Get-Item -LiteralPath $patch -Force
        if ($patchItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Complete-file fix scan patch must be a regular file.'
        }
        if (@(& git -C $root status --porcelain --untracked-files=all).Count -ne 0) {
            throw 'Complete-file fix scan requires a clean trusted checkout before applying a patch.'
        }
    }

    $beforeByPath = @{}
    foreach ($relative in $normalizedPaths) {
        $beforeLines = @(& git -C $root show "HEAD:$relative" 2>$null)
        if ($LASTEXITCODE -ne 0) {
            throw "Complete-file fix scan could not read the trusted pre-image: $relative"
        }
        $beforeByPath[$relative] = ($beforeLines -join "`n") + "`n"
    }

    $applied = $false
    try {
        if ($applyPatch) {
            & git -C $root apply --check --whitespace=nowarn -- $patch 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                throw 'Complete-file fix scan patch does not apply to the trusted checkout.'
            }
            & git -C $root apply --whitespace=nowarn -- $patch 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                throw 'Complete-file fix scan could not apply the patch.'
            }
            $applied = $true
        }

        if ($applyPatch) {
            $actualPaths = @(& git -C $root diff --name-only HEAD -- |
                Where-Object { $_ } | Sort-Object -Unique)
            if (($actualPaths -join "`n") -cne ($normalizedPaths -join "`n")) {
                throw 'Complete-file fix scan patch changed a different file set.'
            }
        }

        foreach ($relative in $normalizedPaths) {
            if (-not $applyPatch) {
                & git -C $root diff --quiet HEAD -- $relative
                if ($LASTEXITCODE -eq 0) {
                    throw "Complete-file fix scan was given an unchanged path: $relative"
                }
            }
            $fullPath = [IO.Path]::GetFullPath((Join-Path $root $relative))
            $rootPrefix = $root.TrimEnd(
                [IO.Path]::DirectorySeparatorChar,
                [IO.Path]::AltDirectorySeparatorChar
            ) + [IO.Path]::DirectorySeparatorChar
            if (-not $fullPath.StartsWith($rootPrefix, [StringComparison]::Ordinal)) {
                throw "Complete-file fix scan path escapes the repository: $relative"
            }
            $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction SilentlyContinue
            if (-not $item -or $item.PSIsContainer -or
                ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
                throw "Complete-file fix scan requires an existing regular file: $relative"
            }
            if ($item.Length -gt 512KB) {
                throw "Complete-file fix scan source is oversized: $relative"
            }
            $bytes = [IO.File]::ReadAllBytes($fullPath)
            try {
                $content = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
            } catch {
                throw "Complete-file fix scan source is not strict UTF-8: $relative"
            }
            Assert-ReplicationProductFixDeltaSafety `
                -BeforeContent ([string]$beforeByPath[$relative]) `
                -AfterContent $content `
                -Path $relative
        }
    } finally {
        if ($applyPatch -and $applied) {
            & git -C $root checkout HEAD -- @normalizedPaths 2>&1 | Out-Null
        }
    }

    if ($applyPatch) {
        if (@(& git -C $root status --porcelain --untracked-files=all).Count -ne 0) {
            throw 'Complete-file fix scan failed to restore the trusted checkout.'
        }
    }
}

function Assert-ReplicationGeneratedSourceSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $scan = Invoke-ReplicationUnsafeSourceCapabilities `
        -Content $Content `
        -Path $Path `
        -Patterns @(Get-ReplicationUnsafeSourcePatterns)

    # MAUI permits AutomationId to be set only once, so reassigning it to signal
    # progress throws InvalidOperationException and produces a failure that has
    # nothing to do with the reported bug.
    $automationAssignments = [regex]::Matches(
        $scan.CodeText,
        '(?<target>[A-Za-z_]\w*)\s*\.\s*AutomationId\s*=(?!=)')
    foreach ($group in ($automationAssignments | Group-Object { $_.Groups['target'].Value })) {
        if ($group.Count -le 1) {
            continue
        }

        $detail = Get-ReplicationUnsafeMatchDetail -ScanText $scan.CodeText -Match $group.Group[1]
        throw "Candidate source '$Path' assigns '$($group.Name).AutomationId' $($group.Count) times: $detail. MAUI allows AutomationId to be set only once, so change a dedicated result element's Text to signal progress instead."
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

function Get-ReplicationNonAttributiveOracles {
    # A reproduction is only evidence if its nominated failure is caused by the
    # defect. These messages are emitted by the harness itself, for many
    # unrelated causes, so a test that nominates one of them stays red on a
    # fully fixed product and proves nothing about the reported issue.
    return @(
        [pscustomobject]@{
            Pattern = '(?i)the app was expected to be running still'
            Reason = "the harness teardown assertion in UITestBase, which fires identically for a crash, a clean exit, and an automation session that merely lost its window handle"
        },
        [pscustomobject]@{
            Pattern = '(?i)app became unresponsive, force-closing'
            Reason = 'the harness unresponsive-app teardown path, which reports that the run was abandoned rather than what the product did'
        },
        [pscustomobject]@{
            Pattern = '(?i)\bNoSuchWindowException\b'
            Reason = 'the automation session losing the window it was rooted in, which reports that the driver stopped observing rather than that the product misbehaved'
        },
        [pscustomobject]@{
            Pattern = '(?i)\bInvalidSessionIdException\b'
            Reason = 'a destroyed automation session, which reports that the driver stopped observing rather than that the product misbehaved'
        },
        [pscustomobject]@{
            Pattern = '(?i)\bSessionNotCreatedException\b'
            Reason = 'an automation session that never started, which is an infrastructure failure rather than a product defect'
        },
        [pscustomobject]@{
            # A good reproduction pins the environment it needs, and PR 213 does
            # exactly that. The danger is nominating one of those guards as the
            # expected failure: the test then turns red on an iOS 25 simulator or
            # a landscape window and reports a reproduction, when all it observed
            # was a lane it was never meant to run in. Scanning 11,012 files this
            # phrasing appears once, in "Test requires internet connection" --
            # itself a precondition -- and it does not match either measured
            # oracle in PR 213.
            Pattern = '(?i)\brequires\b.{0,60}?\b(?:or (?:later|newer|higher)|version \d|simulator|emulator|physical device|a display|network|internet|geometry|orientation|accessibility state|default accessibility)\b'
            Reason = 'a precondition on the environment rather than the reported behavior, so the red it predicts is a lane the test was never meant to run in'
        },
        [pscustomobject]@{
            # PR 242 nominated "expected at least 1 purple icon pixel after
            # Shell.ForegroundColor Purple". A reviewer rejected it because one
            # antialiased or contaminated pixel satisfies that threshold, so a
            # completely wrong rendering turns the test green. A measurement
            # oracle has to name a share of what it measured, not a single unit.
            Pattern = '(?i)\b(?:at least\s+(?:1|one)|>=\s*1|>\s*0)\b[^.;]{0,60}?\bpixels?\b'
            Reason = 'a single pixel, which one antialiased or contaminated pixel satisfies, so a fully wrong rendering would still turn this test green'
            Guidance = 'Assert a calibrated share of the opaque pixels you measured, such as a minimum fraction of the icon mask, and report the measured counts in the failure message.'
        },
        [pscustomobject]@{
            # The same weak threshold, written with the measurement first.
            # A camelCase identifier such as purplePixels has no word boundary
            # before "Pixels", so the leading anchor is deliberately absent.
            Pattern = '(?i)pixels?\b[^.;]{0,40}?(?:>=\s*1\b(?!\d)|>\s*0\b|at least\s+(?:1|one)\b)'
            Reason = 'a single pixel, which one antialiased or contaminated pixel satisfies, so a fully wrong rendering would still turn this test green'
            Guidance = 'Assert a calibrated share of the opaque pixels you measured, such as a minimum fraction of the icon mask, and report the measured counts in the failure message.'
        }
    )
}

function Assert-ReplicationOracleIsFalsifiable {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ExpectedFailureSignature,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$TestFilter
    )

    $signature = ([regex]::Replace([string]$ExpectedFailureSignature, '\s+', ' ')).Trim()
    if ([string]::IsNullOrWhiteSpace($signature)) {
        throw 'The reproduction nominates no expected failure signature, so its red cannot be attributed to the reported defect.'
    }

    foreach ($oracle in Get-ReplicationNonAttributiveOracles) {
        if ([regex]::IsMatch($signature, $oracle.Pattern)) {
            $guidance = if ($oracle.PSObject.Properties['Guidance'] -and $oracle.Guidance) {
                [string]$oracle.Guidance
            } else {
                'Assert the reported behavior directly, so that a product fix turns this exact test green.'
            }
            throw ("The reproduction '$TestFilter' nominates a non-falsifiable oracle: its expected failure is $($oracle.Reason). " +
                $guidance)
        }
    }
}

function Assert-ReplicationConditionalCompilationBalance {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if ([System.IO.Path]::GetExtension($Path) -ine '.cs') {
        return
    }

    # A candidate must compile in every target framework of the project it joins,
    # not only the one it was written for. If a conditional branch opens or closes
    # more braces than its siblings, the file is well-formed in one configuration
    # and unparseable in the others, which breaks the whole test assembly.
    $text = $Content.Replace("`r`n", "`n")
    $length = $text.Length
    $frames = [System.Collections.Generic.Stack[object]]::new()
    $current = [pscustomobject]@{
        Delta = 0
        Branches = [System.Collections.Generic.List[int]]::new()
        Directive = ''
        Line = 0
    }
    $line = 1
    $atLineStart = $true
    $index = 0

    while ($index -lt $length) {
        $char = $text[$index]
        $next = if ($index + 1 -lt $length) { $text[$index + 1] } else { [char]0 }

        if ($char -eq "`n") {
            $line++
            $atLineStart = $true
            $index++
            continue
        }
        if ($atLineStart -and ($char -eq ' ' -or $char -eq "`t")) {
            $index++
            continue
        }

        if ($atLineStart -and $char -eq '#') {
            $lineEnd = $text.IndexOf("`n", $index)
            if ($lineEnd -lt 0) { $lineEnd = $length }
            $directive = $text.Substring($index, $lineEnd - $index)
            if ($directive -match '^#\s*if\b') {
                $frames.Push($current)
                $current = [pscustomobject]@{
                    Delta = 0
                    Branches = [System.Collections.Generic.List[int]]::new()
                    Directive = $directive.Trim()
                    Line = $line
                }
            }
            elseif ($directive -match '^#\s*(?:elif|else)\b') {
                if ($frames.Count -gt 0) {
                    $current.Branches.Add($current.Delta)
                    $current.Delta = 0
                }
            }
            elseif ($directive -match '^#\s*endif\b') {
                if ($frames.Count -gt 0) {
                    $current.Branches.Add($current.Delta)
                    $hadElse = $current.Branches.Count -gt 1
                    if (-not $hadElse) {
                        # The omitted branch contributes nothing, so the written
                        # one must contribute nothing either.
                        $current.Branches.Add(0)
                    }
                    $distinct = @($current.Branches | Sort-Object -Unique)
                    if ($distinct.Count -gt 1) {
                        throw ("Candidate source '$Path' has a conditional-compilation block starting at line $($current.Line) (`"$($current.Directive)`") whose branches do not close the same braces. " +
                            'The file parses in one target framework and not in the others, which breaks every other framework in the project. ' +
                            'Keep each #if/#else branch brace-balanced, or guard the whole member instead of part of its body.')
                    }
                    $closedDelta = $distinct[0]
                    $current = $frames.Pop()
                    $current.Delta += $closedDelta
                }
            }
            $index = $lineEnd
            $atLineStart = $false
            continue
        }

        $atLineStart = $false

        if ($char -eq '/' -and $next -eq '/') {
            $end = $text.IndexOf("`n", $index)
            $index = if ($end -lt 0) { $length } else { $end }
            continue
        }
        if ($char -eq '/' -and $next -eq '*') {
            $end = $text.IndexOf('*/', $index + 2)
            $stop = if ($end -lt 0) { $length } else { $end + 2 }
            for ($scan = $index; $scan -lt $stop; $scan++) {
                if ($text[$scan] -eq "`n") { $line++ }
            }
            $index = $stop
            continue
        }
        if ($char -eq '@' -and $next -eq '"') {
            $index += 2
            while ($index -lt $length) {
                if ($text[$index] -eq "`n") { $line++ }
                if ($text[$index] -eq '"') {
                    if ($index + 1 -lt $length -and $text[$index + 1] -eq '"') { $index += 2; continue }
                    $index++
                    break
                }
                $index++
            }
            continue
        }
        if ($char -eq '"' -or $char -eq "'") {
            $quote = $char
            $index++
            while ($index -lt $length) {
                if ($text[$index] -eq '\') { $index += 2; continue }
                if ($text[$index] -eq $quote) { $index++; break }
                if ($text[$index] -eq "`n") { break }
                $index++
            }
            continue
        }

        if ($char -eq '{') { $current.Delta++ }
        elseif ($char -eq '}') { $current.Delta-- }
        $index++
    }
}

function Assert-ReplicationLeakTestMethodology {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    if ($code -notmatch '\bWeakReference\b') {
        return
    }

    $usesCanonicalHelper = $code -match '\bWaitFor(?:GC|Collect)\s*\('
    if ($usesCanonicalHelper) {
        return
    }

    # A reviewer proved this class of test is a false positive: a one-shot GC
    # burst issued inside the frame that created the object observes it as still
    # alive, because the local stays rooted in that frame. The canonical helper
    # retries up to 40 times with a yield between passes, and it reported the
    # same scenario collected on every one of 13 runs.
    $observesLiveness = $code -match '\bGC\.Collect\s*\(' -or
        $code -match '\.IsAlive\b' -or
        $code -match '\.TryGetTarget\s*\('
    if ($observesLiveness) {
        throw ("Candidate test source '$Path' judges a WeakReference without the canonical collection helper. " +
            'A GC burst issued inside the frame that created the object sees it still rooted there and reports a leak that does not exist. ' +
            'Await AssertionExtensions.WaitForGC instead, which retries with a yield between passes.')
    }
}

function Assert-ReplicationGestureTravel {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path

    # Only hand-rolled pointer sequences are at risk. Tests that drag through
    # the harness helpers get a travel the harness already sized correctly.
    if ($code -notmatch '\bCreatePointerDown\s*\(') {
        return
    }

    # A reviewer measured a committed drag at 16 px against a device touch slop
    # of 22 px, so the gesture was never recognised and the assertion failed
    # identically on fixed and on two independently reverted product states.
    # The travel had been scaled from the matched element's rect, which
    # resolved to a 52 px label rather than the 220 dp list it was aimed at.
    $rectScaling = [regex]::Matches(
        $code,
        '(?<operand>[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\.(?:Height|Width)\s*\*\s*(?<factor>0?\.\d+)')
    foreach ($match in $rectScaling) {
        $operand = $match.Groups['operand'].Value
        if ($operand -match '(?i)window|screen|display|viewport') {
            continue
        }
        # The window size is often held in a plainly named local, so treat an
        # operand the file assigns from the window as a window measurement.
        $leaf = ($operand -split '\.')[-1]
        $assignment = "(?:var|[A-Za-z_][A-Za-z0-9_<>,\[\] ]*)\s+" +
            [regex]::Escape($leaf) +
            '\s*=[^;]*Window\.Size'
        if ($code -match $assignment) {
            continue
        }
        throw ("Candidate test source '$Path' scales its drag distance by $($match.Groups['factor'].Value) of '$operand', which is an element rect. " +
            'A rect that resolves to a small label produces a drag below the platform touch slop, so no gesture happens and the test fails the same way on a fixed build. ' +
            'Scale the drag by the window size instead.')
    }

    # A drag written with literal coordinates is checkable directly. Android
    # touch slop is 8 dp, which is 22 px at the density the reviewer measured,
    # and a drag only a little above slop was shown to flip green on an
    # unchanged build. Require travel that clears it with room to spare.
    $literalMoves = [regex]::Matches(
        $code,
        'CreatePointerMove\s*\(\s*CoordinateOrigin\.\w+\s*,\s*(?<x>-?\d+)\s*,\s*(?<y>-?\d+)\s*,')
    if ($literalMoves.Count -ge 2) {
        $longest = 0
        for ($index = 1; $index -lt $literalMoves.Count; $index++) {
            $dx = [Math]::Abs(
                [int]$literalMoves[$index].Groups['x'].Value -
                [int]$literalMoves[$index - 1].Groups['x'].Value)
            $dy = [Math]::Abs(
                [int]$literalMoves[$index].Groups['y'].Value -
                [int]$literalMoves[$index - 1].Groups['y'].Value)
            $longest = [Math]::Max($longest, [Math]::Max($dx, $dy))
        }
        if ($longest -lt 50) {
            throw ("Candidate test source '$Path' drags at most $longest px between pointer positions. " +
                'Android touch slop alone is around 22 px, so a drag this small is not recognised as a gesture and the test fails the same way whether the product is fixed or broken. ' +
                'Drag far enough to clear the platform touch slop.')
        }
    }
}

function Assert-ReplicationProbeGeometryIsMeasured {
    <#
    .SYNOPSIS
        Refuses a probe point that is computed across axes from requested layout
        values instead of read from the element's measured bounds.

    .DESCRIPTION
        PR 265 committed 'var expectedX = border.Height - border.Padding.Left'
        to probe an initially rotated Border, and three exact-one runs failed
        red against it. A reviewer then showed the oracle decides nothing: for
        a correctly start-aligned 170x300 rotated visual the horizontal bounds
        are roughly -65..235 DIP, so the committed X=288 probe lands outside a
        *correct* Border as well. A real product fix can leave the probe
        reporting MISSING, and the reported misplacement can drag the Border
        across X=288 and report a false green.

        Deriving an X from a Height (or a Y from a Width) is the signature of a
        hand-rolled rotation guess. The transformed extent of a rotated visual
        is a measured quantity, so it has to come from the rect the platform
        actually reports. Reading that rect is allowed and is what the other
        nine measured pull requests already do; only the cross-axis arithmetic
        that replaces it is refused.

        The negative control cannot catch this. Removing the trigger moves the
        visual, so the probe flips green and the control passes while the
        oracle is still keyed to a coordinate that means nothing.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path

    # Only an assignment that names a coordinate is examined. A width computed
    # from a height is ordinary layout arithmetic and says nothing about where
    # the test decides to look.
    $assignments = [regex]::Matches(
        $code,
        '(?m)^[^\r\n]*?\b(?<name>[A-Za-z_][A-Za-z0-9_]*(?<axis>X|Y))\s*=\s*(?<rhs>[^;]+);')

    foreach ($assignment in $assignments) {
        $rhs = $assignment.Groups['rhs'].Value

        # A coordinate taken from the rect the platform reported is exactly the
        # measured value this guard is asking for.
        if ($rhs -match '(?i)\b(GetRect|GetBoundingBox|Bounds|Frame|Location|Size)\b') {
            continue
        }

        $axis = $assignment.Groups['axis'].Value
        $crossAxis = if ($axis -eq 'X') { 'Height' } else { 'Width' }
        if ($rhs -notmatch ('\.{0}\b' -f $crossAxis)) {
            continue
        }

        throw ("Candidate test source '$Path' computes the probe coordinate " +
            "'$($assignment.Groups['name'].Value)' from '.$crossAxis': $($rhs.Trim()). " +
            "A $axis derived from a $crossAxis is a hand-rolled guess at where a transformed visual sits, " +
            'and a probe placed by that guess can miss a correctly laid out element and report a false red, ' +
            'or be reached by the reported misplacement and report a false green. ' +
            "Read the element's measured rect and derive the probe from the bounds the platform reports.")
    }
}

function Get-ReplicationBalancedBlock {
    <#
    .SYNOPSIS
        Returns the text between an opening brace and its matching close, so a
        loop body is read as the block it is rather than as everything that
        follows the loop header.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][int]$OpenBraceIndex
    )

    if ($OpenBraceIndex -lt 0 -or $OpenBraceIndex -ge $Text.Length) { return $null }
    if ($Text[$OpenBraceIndex] -ne '{') { return $null }

    $depth = 0
    for ($index = $OpenBraceIndex; $index -lt $Text.Length; $index++) {
        $character = $Text[$index]
        if ($character -eq '{') { $depth++ }
        elseif ($character -eq '}') {
            $depth--
            if ($depth -eq 0) {
                return $Text.Substring($OpenBraceIndex + 1, $index - $OpenBraceIndex - 1)
            }
        }
    }

    # An unbalanced block is a truncated or malformed candidate; the balance
    # guard reports that, so say nothing about it here.
    return $null
}

function Get-ReplicationGesturePattern {
    # Driver calls that hand a gesture to the platform and return before the
    # platform has finished reacting to it.
    return '\b(?:Swipe(?:RightToLeft|LeftToRight|UpToDown|DownToUp)|' +
        'Scroll(?:Up|Down|Left|Right|To)|DragAndDrop|DragCoordinates|' +
        'TouchAndHold|DoubleTap|PerformActions)\s*\('
}

function Get-ReplicationSettlePattern {
    # Calls that block until the app reports the state the gesture was meant to
    # produce. An unconditional sleep is deliberately absent: it waits for the
    # clock rather than for the app, so it does not synchronise anything.
    return '\b(?:WaitFor\w*|WaitUntil\w*|Assert\w*\.\w+|Should\w*\(|' +
        'FindElement|FindElements|QueryUntilPresent)\s*\('
}

function Get-ReplicationLoopBody {
    <#
    .SYNOPSIS
        Returns the body of a loop, whether or not it is wrapped in braces.

    .DESCRIPTION
        A single-statement loop body needs no braces, and that is the shortest
        way to write a burst of gestures, so a scan that only understands
        braced blocks misses the very shape it exists to catch.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][int]$HeaderEndIndex
    )

    $index = $HeaderEndIndex
    while ($index -lt $Text.Length -and [char]::IsWhiteSpace($Text[$index])) { $index++ }
    if ($index -ge $Text.Length) { return $null }

    if ($Text[$index] -eq '{') {
        return Get-ReplicationBalancedBlock -Text $Text -OpenBraceIndex $index
    }

    # An unbraced body is exactly one statement, so it ends at the first
    # semicolon that is not inside a nested call.
    $depth = 0
    for ($scan = $index; $scan -lt $Text.Length; $scan++) {
        $character = $Text[$scan]
        if ($character -eq '(' -or $character -eq '[') { $depth++ }
        elseif ($character -eq ')' -or $character -eq ']') { $depth-- }
        elseif ($character -eq ';' -and $depth -le 0) {
            return $Text.Substring($index, $scan - $index)
        }
    }

    return $null
}

function Get-ReplicationLoopHeaderPattern {
    # Matches a loop header and tolerates one level of nested parentheses, so
    # `for (var i = 0; i < items.Count(); i++)` is still recognised.
    return '\b(?:for|foreach|while)\s*\((?:[^()]|\([^()]*\))*\)'
}

function Assert-ReplicationGestureIsSynchronized {
    <#
    .SYNOPSIS
        Rejects a burst of gestures that never waits for the app between them.

    .DESCRIPTION
        A reviewer broke a carousel reproduction that fired ten 250 ms drags
        back to back and then asserted the exact position trace
        '4,0,1,2,3,4,0,1,2,3'. Nothing in the test waited for snapping or
        settling, so the gestures could coalesce, overlap native animation,
        skip a transition or add one. An exact-sequence inequality then reports
        driver timing, not the defect, and it reports it identically on a fixed
        build. Require the test to wait for the state each gesture was supposed
        to produce before it sends the next one.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $gesture = Get-ReplicationGesturePattern
    $settle = Get-ReplicationSettlePattern

    # A gesture repeated by a loop whose body never waits is the same burst
    # written shorter, and the trip count hides how many gestures are in flight.
    foreach ($match in [regex]::Matches($code, (Get-ReplicationLoopHeaderPattern))) {
        $body = Get-ReplicationLoopBody -Text $code -HeaderEndIndex ($match.Index + $match.Length)
        if ($null -eq $body) { continue }
        if ($body -cmatch $gesture -and $body -cnotmatch $settle) {
            throw ("Candidate test source '$Path' repeats a gesture in a loop whose body never waits for the app to " +
                'react. The gestures can coalesce or overlap the platform animation, so an assertion about what the ' +
                'gestures produced can fail on timing alone and fails the same way on a fixed build. Wait for the ' +
                'state each gesture is supposed to produce before sending the next one.')
        }
    }

    # Straight-line bursts have the same defect once there are enough of them
    # for the platform to still be animating when the next gesture lands.
    $statements = @($code -split ';')
    $run = 0
    foreach ($statement in $statements) {
        if ($statement -cmatch $settle) { $run = 0; continue }
        if ($statement -cmatch $gesture) {
            $run++
            if ($run -ge 3) {
                throw ("Candidate test source '$Path' sends $run gestures in a row without waiting for the app " +
                    'between them. The platform is still settling when the next gesture arrives, so an assertion ' +
                    'about the resulting state reports driver timing rather than the defect. Wait for the state ' +
                    'each gesture is supposed to produce before sending the next one.')
            }
        }
    }
}

function Assert-ReplicationPointerSequenceIsSelfContained {
    <#
    .SYNOPSIS
        Rejects a drag that is split across separate PerformActions calls.

    .DESCRIPTION
        A reviewer found that a SwipeView reproduction pressed the pointer down
        in one action sequence and then moved it in two later ones. Each
        PerformActions call ends the input it was given, so the later sequences
        moved a pointer that was no longer down and injected no touch events at
        all. The assertion that followed then measured nothing, and it measured
        nothing just as reliably on a fixed build.

        A drag has to be one sequence that presses, moves and releases.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path

    # Only touch gestures are in scope. Without a press somewhere this is not a
    # drag test, and a move-only sequence may be deliberate.
    if ($code -cnotmatch 'CreatePointerDown') { return }

    $sequences = @{}
    foreach ($match in [regex]::Matches(
        $code, '(?<name>\w+)\s*=\s*new\s+ActionSequence\s*\(')) {
        $sequences[$match.Groups['name'].Value] = [System.Text.StringBuilder]::new()
    }
    if ($sequences.Count -eq 0) { return }

    foreach ($name in @($sequences.Keys)) {
        foreach ($action in [regex]::Matches(
            $code, ('\b' + [regex]::Escape($name) + '\s*\.\s*AddAction\s*\((?<body>[^;]*)'))) {
            [void]$sequences[$name].Append($action.Groups['body'].Value)
        }
    }

    foreach ($match in [regex]::Matches($code, 'PerformActions\s*\((?<args>[^;]*)\)')) {
        $arguments = $match.Groups['args'].Value
        foreach ($name in $sequences.Keys) {
            if ($arguments -cnotmatch ('\b' + [regex]::Escape($name) + '\b')) { continue }

            $actions = $sequences[$name].ToString()
            if ([string]::IsNullOrWhiteSpace($actions)) { continue }
            if ($actions -cnotmatch 'CreatePointerMove') { continue }
            if ($actions -cmatch 'CreatePointerDown') { continue }

            throw ("Candidate test source '$Path' performs the action sequence '$name', which moves the pointer but " +
                'never presses it down. Each PerformActions call ends the input it was given, so a sequence that ' +
                'only moves is delivered with the pointer up and injects no touch events at all. An assertion after ' +
                'it measures nothing, and measures nothing just as reliably on a fixed build. Build the whole drag ' +
                'as one sequence that presses, moves and releases.')
        }
    }
}

function Get-ReplicationFeatureSwitchedHandlers {
    [CmdletBinding()]
    param([AllowEmptyString()][string]$RepositoryRoot)

    if (-not $RepositoryRoot) {
        return @()
    }

    $hostingPath = Join-Path $RepositoryRoot 'src/Controls/src/Core/Hosting/AppHostBuilderExtensions.cs'
    if (-not (Test-Path -LiteralPath $hostingPath -PathType Leaf)) {
        return @()
    }

    $source = Get-Content -LiteralPath $hostingPath -Raw
    $switched = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)

    # Walk braces from each runtime-feature test and collect only the handlers
    # registered inside that block. Handlers registered unconditionally --
    # CollectionViewHandler2 on iOS, for instance -- are deliberately left out,
    # because registering one by hand matches what the product already does.
    foreach ($match in [regex]::Matches(
            $source,
            'if\s*\(\s*RuntimeFeature\.[A-Za-z_][A-Za-z0-9_]*\s*\)')) {
        $index = $match.Index + $match.Length
        while ($index -lt $source.Length -and $source[$index] -ne '{') {
            if ($source[$index] -notmatch '\s') { break }
            $index++
        }
        if ($index -ge $source.Length -or $source[$index] -ne '{') { continue }

        $depth = 0
        $start = $index
        for (; $index -lt $source.Length; $index++) {
            if ($source[$index] -eq '{') { $depth++ }
            elseif ($source[$index] -eq '}') {
                $depth--
                if ($depth -eq 0) { break }
            }
        }
        if ($depth -ne 0) { continue }

        $block = $source.Substring($start, $index - $start + 1)
        foreach ($registration in [regex]::Matches(
                $block,
                'AddHandler\s*<\s*[^,>]+,\s*(?<handler>[A-Za-z_][A-Za-z0-9_]*)\s*>')) {
            [void]$switched.Add($registration.Groups['handler'].Value)
        }
    }

    return @($switched)
}

function Assert-ReplicationHandlerRegistrationIsNotTautological {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [AllowEmptyString()][string]$RepositoryRoot = ''
    )

    # Every Controls device test registers the handlers its scenario needs, and
    # Assert.IsType<THandler> is the ordinary way to reach PlatformView from
    # there. Reviewers accepted four such tests, so the registration alone says
    # nothing. What made kubaflo/maui#204 wrong is narrower: EntryHandler2 is
    # registered by the product only when RuntimeFeature.IsMaterial3Enabled is
    # on, so hand-registering it runs the switched-off code path and a fix
    # behind that switch can never turn the test green.
    $switched = @(Get-ReplicationFeatureSwitchedHandlers -RepositoryRoot $RepositoryRoot)
    if ($switched.Count -eq 0) {
        return
    }

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path

    foreach ($registration in [regex]::Matches(
            $code,
            'AddHandler\s*<\s*[^,>]+,\s*(?<handler>[A-Za-z_][A-Za-z0-9_]*)\s*>')) {
        $handler = $registration.Groups['handler'].Value
        if ($handler -cnotin $switched) {
            continue
        }
        $assertsOwnRegistration = 'Assert\.(?:IsType|IsAssignableFrom)\s*<\s*' +
            [regex]::Escape($handler) +
            '\s*>'
        if ($code -match $assertsOwnRegistration) {
            # A reviewer proved this shape reports a genuine fix as unfixed. The
            # product registers the handler only behind a runtime feature switch;
            # the test registered it by hand with the switch off, so the gated
            # fix never ran and the test stayed red with an identical message.
            # Asserting the resolved handler then only confirms the test's own
            # setup, and cannot tell a fixed product from a broken one.
            throw ("Candidate test source '$Path' registers '$handler' itself and then asserts the resolved handler is '$handler'. " +
                "The product registers '$handler' only behind a runtime feature switch, so registering it by hand runs the switched-off path and that assertion can only confirm the test setup. " +
                'Arrange the switch and let the product resolve the handler, so a gated fix can turn the test green.')
        }
    }
}

function Assert-ReplicationWaitResultIsUsed {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path

    # A reviewer found `_ = Wait(a, 5s) || Wait(b, 5s);`. The verdict is thrown
    # away and `||` short-circuits, so a transient first condition sends the
    # test straight past the check that was supposed to catch the defect and it
    # passes while the defect is happening. Discarding a wait that throws on
    # timeout is fine and common here; discarding a combined boolean is not.
    foreach ($discard in [regex]::Matches(
            $code,
            '(?m)^\s*_\s*=\s*(?<expression>[^;]*(?:\|\||&&)[^;]*);')) {
        $expression = $discard.Groups['expression'].Value.Trim()
        if ($expression -notmatch '(?i)wait|assert|poll|retry|until') {
            continue
        }
        throw ("Candidate test source '$Path' discards the result of '$expression'. " +
            'A short-circuiting condition whose verdict is thrown away lets the test continue as though it succeeded, so it can pass while the reported defect is happening. ' +
            'Evaluate each wait separately and assert its result.')
    }
}

function Get-ReplicationPlatformCompilationSymbols {
    <#
    .SYNOPSIS
        The symbols each platform test project defines, copied from the four
        DefineConstants lines in src/Controls/tests/TestCases.<Platform>.Tests.
        Every project defines its own platform symbol plus TEST_FAILS_ON_ for
        each of the other three, which is why TEST_FAILS_ON_WINDOWS means
        "compile everywhere except Windows".
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [string]$Platform
    )

    $own = @{
        android  = @('ANDROID')
        ios      = @('IOS', 'IOSUITEST')
        catalyst = @('MACCATALYST', 'MACUITEST')
        windows  = @('WINDOWS', 'WINTEST')
    }
    $exclusion = @{
        android  = 'TEST_FAILS_ON_ANDROID'
        ios      = 'TEST_FAILS_ON_IOS'
        catalyst = 'TEST_FAILS_ON_CATALYST'
        windows  = 'TEST_FAILS_ON_WINDOWS'
    }

    $symbols = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($symbol in $own[$Platform]) { $null = $symbols.Add($symbol) }
    foreach ($other in $exclusion.Keys) {
        if ($other -ne $Platform) { $null = $symbols.Add($exclusion[$other]) }
    }
    return $symbols
}

function Test-ReplicationPreprocessorExpression {
    <#
    .SYNOPSIS
        Evaluates a C# #if/#elif expression against a symbol set. Supports the
        operators the repository actually uses: !, &&, ||, parentheses and the
        true/false literals. An expression it cannot parse evaluates to true,
        so an unreadable condition is treated as "this compiles here" and the
        caller reports an unscoped test rather than silently approving one.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Expression,
        [Parameter(Mandatory = $true)][object]$Symbols
    )

    $tokens = @([regex]::Matches($Expression, '[A-Za-z_]\w*|\|\||&&|[!()]') |
        ForEach-Object { $_.Value })
    if ($tokens.Count -eq 0) { return $true }

    # A scriptblock invoked with & runs in a child scope, so a plain $position
    # counter would never advance for the caller. Carry it on an object instead.
    $state = [pscustomobject]@{ Position = 0 }
    $parseOr = $null
    $parsePrimary = {
        if ($state.Position -ge $tokens.Count) { throw 'unterminated' }
        $token = $tokens[$state.Position]
        if ($token -eq '!') {
            $state.Position++
            return -not (& $parsePrimary)
        }
        if ($token -eq '(') {
            $state.Position++
            $value = & $parseOr
            if ($state.Position -ge $tokens.Count -or $tokens[$state.Position] -ne ')') { throw 'unbalanced' }
            $state.Position++
            return $value
        }
        if ($token -eq ')' -or $token -eq '&&' -or $token -eq '||') { throw 'unexpected' }
        $state.Position++
        if ($token -ceq 'true') { return $true }
        if ($token -ceq 'false') { return $false }
        return $Symbols.Contains($token)
    }
    $parseAnd = {
        $value = & $parsePrimary
        while ($state.Position -lt $tokens.Count -and $tokens[$state.Position] -eq '&&') {
            $state.Position++
            $right = & $parsePrimary
            $value = $value -and $right
        }
        return $value
    }
    $parseOr = {
        $value = & $parseAnd
        while ($state.Position -lt $tokens.Count -and $tokens[$state.Position] -eq '||') {
            $state.Position++
            $right = & $parseAnd
            $value = $value -or $right
        }
        return $value
    }

    try {
        $result = & $parseOr
        if ($state.Position -ne $tokens.Count) { return $true }
        return [bool]$result
    } catch {
        return $true
    }
}

function Get-ReplicationCompiledLineMap {
    <#
    .SYNOPSIS
        Returns one boolean per line saying whether that line survives the
        preprocessor for the given symbol set.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][object]$Symbols
    )

    # A byte order mark sits in front of the first #if in real repository
    # files and is not whitespace, so strip it before reading directives.
    $lines = @($Content.Replace("`r`n", "`n").Replace([string][char]0xFEFF, '') -split "`n")
    $map = New-Object 'bool[]' $lines.Count
    $stack = [System.Collections.Generic.List[object]]::new()

    $isActive = {
        foreach ($frame in $stack) { if (-not $frame.Taken) { return $false } }
        return $true
    }

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^\s*#\s*(?<directive>if|elif|else|endif)\b\s*(?<expression>.*)$') {
            $directive = $Matches['directive']
            # Nearly every scoped file in the repository explains itself in a
            # trailing comment, which is not part of the condition.
            $expression = $Matches['expression'] -replace '/\*.*?\*/', ' ' -replace '//.*$', ''
            switch ($directive) {
                'if' {
                    $taken = Test-ReplicationPreprocessorExpression -Expression $expression -Symbols $Symbols
                    $stack.Add([pscustomobject]@{ Taken = $taken; AnyTaken = $taken })
                }
                'elif' {
                    if ($stack.Count -gt 0) {
                        $frame = $stack[$stack.Count - 1]
                        $taken = (-not $frame.AnyTaken) -and
                            (Test-ReplicationPreprocessorExpression -Expression $expression -Symbols $Symbols)
                        $frame.Taken = $taken
                        $frame.AnyTaken = $frame.AnyTaken -or $taken
                    }
                }
                'else' {
                    if ($stack.Count -gt 0) {
                        $frame = $stack[$stack.Count - 1]
                        $frame.Taken = -not $frame.AnyTaken
                        $frame.AnyTaken = $true
                    }
                }
                'endif' {
                    if ($stack.Count -gt 0) { $stack.RemoveAt($stack.Count - 1) }
                }
            }
            $map[$i] = $false
            continue
        }
        $map[$i] = & $isActive
    }

    return $map
}

function Get-ReplicationOwningProjectTargetFrameworks {
    <#
    .SYNOPSIS
        Reads the TargetFramework(s) of the project that will compile a test.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$RepositoryRoot
    )

    if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = '.' }
    $directory = Split-Path -Parent (Join-Path $RepositoryRoot ($Path -replace '/', [IO.Path]::DirectorySeparatorChar))
    $rootFull = try { (Resolve-Path -LiteralPath $RepositoryRoot -ErrorAction Stop).Path } catch { $null }

    while ($directory -and (Test-Path -LiteralPath $directory)) {
        $project = @(Get-ChildItem -LiteralPath $directory -Filter '*.csproj' -File -ErrorAction SilentlyContinue) |
            Select-Object -First 1
        if ($project) {
            $text = Get-Content -LiteralPath $project.FullName -Raw -ErrorAction SilentlyContinue
            $single = [regex]::Match($text, '<TargetFramework>\s*([^<]+?)\s*</TargetFramework>')
            $many = [regex]::Match($text, '<TargetFrameworks>\s*([^<]+?)\s*</TargetFrameworks>')
            return [pscustomobject]@{
                ProjectPath = $project.FullName
                Value = if ($many.Success) { $many.Groups[1].Value } elseif ($single.Success) { $single.Groups[1].Value } else { '' }
            }
        }
        $parent = Split-Path -Parent $directory
        if ($parent -eq $directory) { break }
        if ($rootFull -and $directory -eq $rootFull) { break }
        $directory = $parent
    }

    return $null
}

function Get-ReplicationPlatformClosureMarker {
    # The orchestrator escalates the test tier when it sees this, so the
    # sentence is shared rather than written out twice. A rejection here can
    # never be repaired in place: no edit to the test makes its project target
    # another platform.
    return 'platform code is not present in the tested closure'
}

function Assert-ReplicationTestRunsOnEvidencePlatform {
    <#
    .SYNOPSIS
        A test that never compiles for the platform cannot be evidence about it.

    .DESCRIPTION
        Reviewers rejected every headless reproduction on this one ground.
        Controls.Core.UnitTests declares a single non-platform TargetFramework,
        so, as the review of pull request 190 put it, the platform code is "not
        merely unexercised, it is not present in the tested closure"; the review
        of 226 recorded IsMacCatalyst == false and null handlers, and 199 found
        no platform result at all. Every replication ships a recording made on
        one platform and claims to reproduce what that recording shows, so a
        single non-platform target framework contradicts the claim.

        This applies only to the in-process tiers. An Appium UI test project
        also targets a non-platform framework, legitimately: it runs on the host
        and drives a real app on the device, so its own target framework says
        nothing about what the app under test exercises. Only a provable
        contradiction is rejected; an unrecognised property is left alone.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [string]$Platform,
        [Parameter(Mandatory = $true)][string]$TestType,
        [string]$RepositoryRoot
    )

    # A UI test drives the app over WebDriver and a device test is compiled into
    # it; only the in-process tiers claim to exercise platform code themselves.
    if ($TestType -cnotin @('UnitTest', 'XamlUnitTest')) { return }
    if ([IO.Path]::GetExtension($Path) -inotin @('.cs', '.xaml')) { return }

    $project = Get-ReplicationOwningProjectTargetFrameworks -Path $Path -RepositoryRoot $RepositoryRoot
    if (-not $project) { return }

    $value = ([string]$project.Value).Trim().Trim(';')
    # A platform target framework carries an OS moniker; these do not. A list
    # is only non-platform when every entry in it is.
    $entries = @($value -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($entries.Count -eq 0) { return }
    $nonPlatform = -not (@($entries | Where-Object {
                $_ -notmatch '^net\d+\.\d+$' -and $_ -notmatch '(?i)^\$\(_?MauiDotNetTfm\)$'
            }).Count -gt 0)
    if (-not $nonPlatform) { return }

    $projectName = if ($project.ProjectPath) { Split-Path -Leaf $project.ProjectPath } else { 'the owning project' }
    throw ("Candidate test source '$Path' is compiled by $projectName, which declares the single " +
        "non-platform target framework '$value'. There is no $Platform build of that assembly, so the " +
        (Get-ReplicationPlatformClosureMarker) + " and the test cannot be evidence for a " +
        "reproduction recorded on $Platform. Author the test where it builds for $Platform, such as a " +
        'device test project or the UI test host application.')
}

function Get-ReplicationFilenamePlatformScope {
    <#
    .SYNOPSIS
        The platforms a file compiles for by virtue of its name alone.
    .DESCRIPTION
        src/MultiTargeting.targets and Controls.DeviceTests.csproj remove
        platform-suffixed sources from every target framework but their own, so
        Issue37151Tests.Android.cs is already Android-only and an #if ANDROID
        inside it is redundant. Returns $null when the name imposes no scope.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$Path)

    $normalizedPath = $Path.Replace('\', '/')
    $fileName = [System.IO.Path]::GetFileName($normalizedPath)

    # Ordered longest-first: *.MaciOS.cs must not be read as *.iOS.cs.
    $suffixes = [ordered]@{
        '.MacCatalyst.cs' = @('catalyst')
        '.MaciOS.cs'      = @('ios', 'catalyst')
        '.Android.cs'     = @('android')
        '.Windows.cs'     = @('windows')
        '.iOS.cs'         = @('ios', 'catalyst')
        '.Mac.cs'         = @()
        '.Standard.cs'    = @()
    }
    foreach ($suffix in $suffixes.Keys) {
        if ($fileName.EndsWith($suffix, [StringComparison]::OrdinalIgnoreCase)) {
            return , @($suffixes[$suffix])
        }
    }

    $folders = [ordered]@{
        'MacCatalyst' = @('catalyst')
        'MaciOS'      = @('ios', 'catalyst')
        'Android'     = @('android')
        'Windows'     = @('windows')
        'iOS'         = @('ios', 'catalyst')
        'Mac'         = @()
        'Standard'    = @()
    }
    $segments = @($normalizedPath -split '/')
    foreach ($folder in $folders.Keys) {
        if ($segments -contains $folder) {
            return , @($folders[$folder])
        }
    }

    return $null
}

function Assert-ReplicationTestPlatformScope {
    <#
    .SYNOPSIS
        A reproduction is observed on exactly one platform, but the shared test
        projects link-compile into all four platform assemblies. A test left
        unscoped therefore runs on three lanes that produced no evidence for it.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [string]$Platform
    )

    $normalizedPath = $Path.Replace('\', '/')
    $sharedProject = '^src/(?:Controls/tests/(?:TestCases\.Shared\.Tests|DeviceTests)|' +
        'Core/tests/DeviceTests(?:\.Shared)?|Essentials/test/DeviceTests|' +
        'Graphics/tests/DeviceTests|BlazorWebView/tests/DeviceTests)/'
    if ($normalizedPath -cnotmatch $sharedProject) { return }

    # A platform-suffixed name already restricts the build, and reviewers
    # accepted four device tests written exactly that way. Only the platforms
    # the file can compile for are still in play.
    $fileScope = Get-ReplicationFilenamePlatformScope -Path $normalizedPath
    if ($null -ne $fileScope) {
        if ($fileScope -notcontains $Platform) {
            throw ("Candidate test source '$Path' is named for a platform it cannot serve: its name " +
                'keeps it out of every build except ' +
                $(if ($fileScope.Count -eq 0) { 'the non-platform one' } else { $fileScope -join ', ' }) +
                ", but the reproduction was observed on $Platform. The test must compile on the " +
                'platform that produced the evidence.')
        }
        return
    }

    $normalized = $Content.Replace("`r`n", "`n").Replace([string][char]0xFEFF, '')
    $lines = @($normalized -split "`n")
    $testLines = @()
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^\s*\[\s*(?:(?:[A-Za-z_]\w*)\.)*(?:Test|Fact|Theory)\s*[\](]') {
            $testLines += $i
        }
    }
    if ($testLines.Count -eq 0) { return }

    $names = @{ android = 'ANDROID'; ios = 'IOS'; catalyst = 'MACCATALYST'; windows = 'WINDOWS' }
    $maps = @{}
    foreach ($candidate in @('android', 'ios', 'catalyst', 'windows')) {
        $maps[$candidate] = Get-ReplicationCompiledLineMap `
            -Content $normalized `
            -Symbols (Get-ReplicationPlatformCompilationSymbols -Platform $candidate)
    }

    foreach ($index in $testLines) {
        $lineNumber = $index + 1
        if (-not $maps[$Platform][$index]) {
            throw ("Candidate test source '$Path' excludes its test method on line $lineNumber from " +
                "$Platform, which is the only platform where the reproduction was observed. The test " +
                'must compile on the platform that produced the evidence.')
        }
        $others = @(@('android', 'ios', 'catalyst', 'windows') |
            Where-Object { $_ -ne $Platform -and $maps[$_][$index] })
        if ($others.Count -gt 0) {
            throw ("Candidate test source '$Path' lets its test method on line $lineNumber also run on " +
                ($others -join ', ') + ", although the reproduction was only observed on $Platform. " +
                "Wrap the test in #if $($names[$Platform]) so the lanes that produced no evidence for " +
                'it are not made red by it.')
        }
    }
}

function Get-ReplicationPlatformViewRoot {
    <#
        .SYNOPSIS
        Names the element whose platform view an expression reads, if any.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Expression,

        [Parameter(Mandatory)]
        [AllowNull()]
        [hashtable] $CapturedRoots
    )

    $trimmed = $Expression.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        return $null
    }

    # A direct read: someElement.Handler.PlatformView or someElement.ToPlatform().
    $direct = [regex]::Match(
        $trimmed,
        '(?<root>[A-Za-z_][A-Za-z0-9_]*)\s*(?:\.\s*[A-Za-z_][A-Za-z0-9_]*\s*)*?\.\s*(?:Handler\s*(?:\!\s*)?\.\s*PlatformView|ToPlatform\s*\()')
    if ($direct.Success) {
        return $direct.Groups['root'].Value
    }

    # A local that was assigned from such a read earlier in the test.
    $identifier = [regex]::Match($trimmed, '^(?<name>[A-Za-z_][A-Za-z0-9_]*)$')
    if ($identifier.Success -and $null -ne $CapturedRoots) {
        $name = $identifier.Groups['name'].Value
        if ($CapturedRoots.ContainsKey($name)) {
            return $CapturedRoots[$name]
        }
    }

    return $null
}

function Split-ReplicationAssertionArguments {
    <#
        .SYNOPSIS
        Splits an argument list on commas that are not nested.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Arguments
    )

    $parts = [System.Collections.Generic.List[string]]::new()
    $current = [System.Text.StringBuilder]::new()
    $depth = 0
    foreach ($character in $Arguments.ToCharArray()) {
        switch ($character) {
            '(' { $depth++; [void]$current.Append($character); continue }
            '[' { $depth++; [void]$current.Append($character); continue }
            ')' { $depth--; [void]$current.Append($character); continue }
            ']' { $depth--; [void]$current.Append($character); continue }
            ',' {
                if ($depth -eq 0) {
                    [void]$parts.Add($current.ToString())
                    [void]$current.Clear()
                    continue
                }
                [void]$current.Append($character)
                continue
            }
            default { [void]$current.Append($character); continue }
        }
    }

    [void]$parts.Add($current.ToString())
    return $parts.ToArray()
}

function Assert-ReplicationPlatformViewIdentity {
    <#
        .SYNOPSIS
        Refuses a test whose verdict is that a native view instance did or
        did not survive the reported trigger.

        .DESCRIPTION
        A reproduction that asserts the platform view is the same object it
        was before the trigger stays red after a correct fix that recreates
        the view, and a reproduction that asserts it is a different object
        stays red after a correct fix that reuses it. Either way the test
        pins an implementation detail the issue never reported, so it can
        never turn green. Comparing a platform view with a container or a
        sibling is unaffected; only comparing an element's platform view
        with its own platform view is refused.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Content,

        [Parameter(Mandatory)]
        [string] $Path
    )

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return
    }

    $scanned = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $capturedRoots = @{}

    foreach ($capture in [regex]::Matches(
        $scanned,
        '(?:var|[A-Za-z_][A-Za-z0-9_\.\<\>\[\]\?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>[^;]{1,400});')) {
        $root = Get-ReplicationPlatformViewRoot `
            -Expression $capture.Groups['value'].Value `
            -CapturedRoots $null
        if ($root) {
            $capturedRoots[$capture.Groups['name'].Value] = $root
        }
    }

    $comparisons = [System.Collections.Generic.List[string[]]]::new()

    foreach ($call in [regex]::Matches(
        $scanned,
        '(?:Assert\s*\.\s*(?:Same|NotSame|AreSame|AreNotSame)|Object\s*\.\s*ReferenceEquals|ReferenceEquals)\s*\((?<args>[^;]{1,400})\)')) {
        $arguments = Split-ReplicationAssertionArguments -Arguments $call.Groups['args'].Value
        if ($arguments.Count -eq 2) {
            [void]$comparisons.Add([string[]]@($arguments[0], $arguments[1]))
        }
    }

    foreach ($fluent in [regex]::Matches(
        $scanned,
        '(?<subject>[A-Za-z_][A-Za-z0-9_\!\?]*(?:\s*\.\s*[A-Za-z_][A-Za-z0-9_\!\?]*(?:\s*\(\s*\))?){0,6})\s*\.\s*Should\s*\(\s*\)\s*\.\s*(?:Be|NotBe)SameAs\s*\(\s*(?<other>[^;\)]{1,200})\)')) {
        [void]$comparisons.Add([string[]]@(
            $fluent.Groups['subject'].Value,
            $fluent.Groups['other'].Value))
    }

    foreach ($comparison in $comparisons) {
        $left = Get-ReplicationPlatformViewRoot -Expression $comparison[0] -CapturedRoots $capturedRoots
        if (-not $left) {
            continue
        }

        $right = Get-ReplicationPlatformViewRoot -Expression $comparison[1] -CapturedRoots $capturedRoots
        if (-not $right) {
            continue
        }

        if ($left -cne $right) {
            continue
        }

        throw (
            "Candidate source '$Path' decides the issue by comparing the platform view of '$left' " +
            'with its own platform view. Whether a handler reuses or recreates its native view is an ' +
            'implementation detail the report does not describe, so this test stays red no matter how ' +
            'the product is fixed. Assert the behaviour the reporter actually observed instead - the ' +
            'text, the size, the position, the visibility, or the state that was wrong on screen.')
    }
}

function Get-ReplicationVerdictLiteralPattern {
    # Strings that announce an outcome rather than describe a value. A test
    # may set one for a human watching the screen; it may not then read its
    # own announcement back as the proof.
    return '(?i)\b(?:bug\s+)?(?:not\s+)?reproduc\w*|' +
        '\b(?:test\s+)?(?:passed|failed|failure|success|succeeded)\b|' +
        '\bPASS\b|\bFAIL\b|\bOK\b'
}

function Assert-ReplicationVerdictIsNotSelfAnnounced {
    <#
        .SYNOPSIS
        Refuses a test that proves the issue by reading back a verdict string
        it assigned itself.

        .DESCRIPTION
        kubaflo/maui#221 drove a pulse animation, set
        statusLabel.Text = "BUG REPRODUCED:" from the animation's completion
        callback, and then asserted statusLabel.Text equalled that string. The
        reviewer's finding: "that label is assigned unconditionally when the
        animation completes", so the assertion establishes that the animation
        ran, not that any pixel was wrong. A product fix cannot change it.

        Setting a value and asserting it is not itself suspect -- StyleTests
        does exactly that to prove a local set outranks a Style, and the
        product decides the answer. What makes this different is the literal:
        a string announcing "BUG REPRODUCED" or "FAILED" is the test's own
        conclusion, and reading a conclusion back is bookkeeping.

        Scoped to that: an equality assertion against a verdict-announcing
        literal that the same file assigns to the same member of the same
        identifier. Across the 5,265 test sources in src/Controls, src/Core
        and src/Essentials this matches nothing.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Content,

        [Parameter(Mandatory)]
        [string] $Path
    )

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return
    }

    $scanned = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $verdict = Get-ReplicationVerdictLiteralPattern

    # identifier.Member = "literal"
    $assigned = @{}
    foreach ($assignment in [regex]::Matches(
        $scanned,
        '(?<![\w.])(?<target>[A-Za-z_]\w*)\s*\.\s*(?<member>\w+)\s*=\s*(?<literal>"(?:[^"\\]|\\.)*")')) {
        $key = '{0}.{1}={2}' -f
            $assignment.Groups['target'].Value,
            $assignment.Groups['member'].Value,
            $assignment.Groups['literal'].Value
        $assigned[$key] = $true
    }
    if ($assigned.Count -eq 0) {
        return
    }

    # The xUnit form puts the expected value first. NUnit reverses it, and the
    # repository's UI tests are NUnit: across the ten published reproductions
    # Assert.That outnumbered Assert.Equal 57 to 9, so matching only the xUnit
    # spelling left the dominant form unguarded. The two shapes are matched in
    # separate passes because one alternation would make 'literal' a duplicate
    # group name, and the branch that did not participate reports an empty
    # capture.
    # Each pattern is parenthesised because PowerShell binds the comma tighter
    # than the plus: without them '@(a + b, c + d)' evaluates to the single
    # element 'ab cd' and both patterns are silently destroyed.
    $assertionForms = @(
        ('(?:Assert\s*\.\s*(?:Equal|AreEqual)|ClassicAssert\s*\.\s*AreEqual)\s*\(\s*' +
            '(?<literal>"(?:[^"\\]|\\.)*")\s*,\s*(?<target>[A-Za-z_]\w*)\s*\.\s*(?<member>\w+)\b'),
        ('Assert\s*\.\s*That\s*\(\s*(?<target>[A-Za-z_]\w*)\s*\.\s*(?<member>\w+)\s*,\s*' +
            'Is\s*\.\s*EqualTo\s*\(\s*(?<literal>"(?:[^"\\]|\\.)*")\s*\)')
    )

    $assertions = [System.Collections.Generic.List[System.Text.RegularExpressions.Match]]::new()
    foreach ($form in $assertionForms) {
        foreach ($match in [regex]::Matches($scanned, $form)) {
            $assertions.Add($match)
        }
    }

    foreach ($assertion in $assertions) {
        $literal = $assertion.Groups['literal'].Value
        if (-not [regex]::IsMatch($literal, $verdict)) {
            continue
        }

        $key = '{0}.{1}={2}' -f
            $assertion.Groups['target'].Value,
            $assertion.Groups['member'].Value,
            $literal
        if (-not $assigned.ContainsKey($key)) {
            continue
        }

        throw (
            "Candidate source '$Path' decides the issue by reading back a verdict it announced " +
            "itself: it assigns $($assertion.Groups['target'].Value)." +
            "$($assertion.Groups['member'].Value) = $literal and then asserts that same value. " +
            'That establishes only that the assigning code path ran, so the test stays red for a ' +
            'reason no product fix can change. Assert the quantity the reporter observed - the ' +
            'text, the size, the position, the visibility, or the colour that was wrong on screen.')
    }
}

function Get-ReplicationMeasurementPattern {
    # Reads that come back from the rendered app rather than from the test.
    return '\.(?:GetRect|GetLocationOnScreen|GetBoundingRect|Frame|Bounds|' +
        'X|Y|Left|Top|Right|Bottom|Width|Height|Center[XY])\b'
}

function Assert-ReplicationGeometryOracleIsPinned {
    <#
    .SYNOPSIS
        Rejects a geometry oracle that only relates two measurements to each
        other and never pins either to an expected value.

    .DESCRIPTION
        A reviewer broke a safe-area reproduction whose oracle asserted that
        the top and bottom gaps were equal. The defect made them 79/62, so the
        assertion did go red - but it also passed on 79/79, which is just as
        wrong as 62/62 is right, and it passed with the SafeAreaEdges
        assignment removed altogether. A relation between two measured values
        is satisfied by a uniformly wrong layout, so it cannot tell a fixed
        product from a differently broken one. At least one measurement has to
        be compared with the value the correct layout produces.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $code = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $measurement = Get-ReplicationMeasurementPattern

    # PR 229 read each gap into a local and then asserted on the locals, so an
    # assertion argument is measured when the local it names was assigned from
    # a measurement.
    $measuredLocals = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($match in [regex]::Matches(
        $code,
        '(?:var|int|double|float|nfloat|decimal|long)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>[^;]+);')) {
        if ($match.Groups['value'].Value -cmatch $measurement) {
            [void]$measuredLocals.Add($match.Groups['name'].Value)
        }
    }

    $isMeasured = {
        param([string]$Argument)
        if ($Argument -cmatch $measurement) { return $true }
        $identifier = [regex]::Match($Argument.Trim(), '^(?<name>[A-Za-z_][A-Za-z0-9_]*)$')
        return $identifier.Success -and $measuredLocals.Contains($identifier.Groups['name'].Value)
    }

    # The last argument of Assert.True(condition, message) reports the failure,
    # it is not an operand. Build 15069709 spent all five attempts refused
    # because its message interpolated the frame it was reporting, so the
    # splitter counted a pinned assertion as a relation between two
    # measurements. A message that happens to end in a digit would equally have
    # been counted as the expected value and exempted a relational oracle, so
    # dropping it removes a false accept as well as a false refusal.
    $isMessage = {
        param([string]$Argument)
        return $Argument.Trim() -match '^[\$@]{0,2}"'
    }

    $relational = $null
    foreach ($match in [regex]::Matches(
        $code,
        '(?:Assert|ClassicAssert)\s*\.\s*(?:Equal|AreEqual|True|IsTrue)\s*\((?<args>[^;]*?)\)\s*;')) {
        $arguments = @(Split-ReplicationAssertionArguments -Arguments $match.Groups['args'].Value |
            Where-Object { -not (& $isMessage $_) })

        # Assert.True(a == b) carries both operands in a single argument.
        if ($arguments.Count -eq 1 -and $arguments[0] -match '==|!=') {
            $arguments = @($arguments[0] -split '==|!=')
        }
        if ($arguments.Count -lt 2) { continue }

        $measured = @($arguments | Where-Object { & $isMeasured $_ })
        if ($measured.Count -lt 2) { continue }

        # A tolerance argument is a constant, not a second measurement, so an
        # assertion that pins a measurement to a number is already specific.
        if ($arguments | Where-Object { $_ -match '(?<![A-Za-z0-9_.])\d+(?:\.\d+)?\s*$' -and -not (& $isMeasured $_) }) {
            continue
        }

        $relational = $match.Value.Trim()
        break
    }

    if ($null -eq $relational) { return }

    # An expected value anywhere in the oracle is enough: the test then fails
    # on a uniformly wrong layout as well as on an asymmetric one.
    $pinned = [regex]::Matches(
        $code,
        '(?:Assert|ClassicAssert)\s*\.\s*\w+\s*\((?<args>[^;]*?)\)\s*;')
    foreach ($match in $pinned) {
        $arguments = @(Split-ReplicationAssertionArguments -Arguments $match.Groups['args'].Value |
            Where-Object { -not (& $isMessage $_) })
        $hasExpected = @($arguments | Where-Object {
            $_ -match '(?<![A-Za-z0-9_.])\d+(?:\.\d+)?\s*$' -and -not (& $isMeasured $_) }).Count -gt 0
        $hasMeasured = @($arguments | Where-Object { & $isMeasured $_ }).Count -gt 0
        if ($hasExpected -and $hasMeasured) { return }
    }

    throw ("Candidate test source '$Path' compares two measured values with each other and never compares either " +
        'with the value a correct layout produces. A layout that is uniformly wrong satisfies the relation, so the ' +
        'assertion passes on a product that is still broken and cannot prove a fix. Assert at least one measurement ' +
        "against its expected value. The relational assertion is: $relational")
}

function Get-ReplicationInitialElementText {
    <#
    .SYNOPSIS
        Maps an automation id to the text its element already shows before the
        test touches anything, read from the host application page.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content
    )

    $initial = @{}
    foreach ($match in [regex]::Matches($Content, 'AutomationId\s*=\s*"(?<id>[^"]+)"')) {
        $start = [Math]::Max(0, $match.Index - 300)
        $length = [Math]::Min($Content.Length - $start, ($match.Index - $start) + 300)
        $window = $Content.Substring($start, $length)

        # Only an initializer or a XAML attribute states what the element shows
        # to begin with. "status.Text = ..." inside a handler is what the app
        # does later, which is the opposite of an initial value.
        $text = [regex]::Match($window, '(?<![.\w])Text\s*=\s*"(?<value>[^"]*)"')
        if ($text.Success) {
            $initial[$match.Groups['id'].Value] = $text.Groups['value'].Value
        }
    }

    return $initial
}

function Assert-ReplicationOracleIsNotInitialState {
    <#
    .SYNOPSIS
        Rejects an oracle that asserts the value an element already had, unless
        the test first proves the interaction actually happened.

    .DESCRIPTION
        A reviewer showed that a carousel reproduction came down to
        'ResultStatus == "NO BUG:"', which is the text the page starts with. An
        oracle like that is satisfied by the defect, but equally by a tap that
        was never delivered, by an acknowledgement that arrived late, and by
        the driver simply missing the change. It cannot tell a broken product
        from a test that did nothing, so it has to be paired with a value the
        app can only produce after the interaction landed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][hashtable]$Files
    )

    $initial = @{}
    foreach ($path in $Files.Keys) {
        if (($path.Replace('\', '/')) -cnotmatch '(?i)TestCases\.HostApp/') { continue }
        foreach ($entry in (Get-ReplicationInitialElementText -Content $Files[$path]).GetEnumerator()) {
            $initial[$entry.Key] = $entry.Value
        }
    }
    if ($initial.Count -eq 0) { return }

    $initialValues = [System.Collections.Generic.HashSet[string]]::new(
        [string[]]@($initial.Values), [System.StringComparer]::Ordinal)

    foreach ($path in $Files.Keys) {
        $normalized = $path.Replace('\', '/')
        if ($normalized -cmatch '(?i)TestCases\.HostApp/') { continue }
        if ($normalized -cnotmatch '(?i)\.cs$') { continue }

        $code = Get-ReplicationCommentFreeText -Text $Files[$path] -Path $path
        $statements = @($code -split ';')

        $offending = $null
        foreach ($statement in $statements) {
            if ($statement -cnotmatch '(?:Assert|ClassicAssert)\s*\.|WaitForText') { continue }
            $literals = @([regex]::Matches($statement, '"(?<value>[^"]*)"') |
                ForEach-Object { $_.Groups['value'].Value })
            foreach ($id in $literals) {
                if (-not $initial.ContainsKey($id)) { continue }
                if ($literals -ccontains $initial[$id]) {
                    $offending = [pscustomobject]@{ Id = $id; Value = $initial[$id] }
                    break
                }
            }
            if ($null -ne $offending) { break }
        }
        if ($null -eq $offending) { continue }

        # A value the page never shows at startup can only come from the app
        # reacting, so an assertion on one proves the interaction landed.
        $acknowledged = $false
        foreach ($statement in $statements) {
            if ($statement -cnotmatch '(?:Assert|ClassicAssert)\s*\.|WaitForText') { continue }
            foreach ($match in [regex]::Matches($statement, '"(?<value>[^"]*)"')) {
                $literal = $match.Groups['value'].Value
                if ([string]::IsNullOrWhiteSpace($literal)) { continue }
                if ($initial.ContainsKey($literal)) { continue }
                if ($initialValues.Contains($literal)) { continue }
                $acknowledged = $true
                break
            }
            if ($acknowledged) { break }
        }
        if ($acknowledged) { continue }

        throw ("Candidate test source '$path' asserts that '$($offending.Id)' shows " +
            "'$($offending.Value)', which is the text the host application already shows before the test does " +
            'anything. That assertion is satisfied by the defect, but equally by an interaction that was never ' +
            'delivered or that the driver missed, so it cannot tell a broken product from a test that did nothing. ' +
            'First assert a value the app can only produce once the interaction has landed, then assert the ' +
            'reported behavior.')
    }
}

function Get-ReplicationComparisonSelectedLiteral {
    <#
    .SYNOPSIS
        Reports the decision that picked a string literal, when the host
        application picked it by comparing values itself.

    .DESCRIPTION
        Returns a description of the branch that selected the literal at
        'Index', or $null when nothing nearby chose it. A literal written
        unconditionally, such as a caption on a label or a starting sentinel,
        is not selected by anything and returns $null.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Code,
        [Parameter(Mandatory = $true)][int]$Index
    )

    $comparison =
        '(==|!=|<=|>=|(?<![<>=!])[<>](?![<>=])|\.Equals\s*\(|\.Contains\s*\(' +
        '|\.Any\s*\(|\.All\s*\(|\.SequenceEqual\s*\(|\.StartsWith\s*\(|\.EndsWith\s*\()'

    # A decision that selects a literal sits within the same statement or the
    # branch immediately around it, so the search stays local on purpose: a
    # comparison elsewhere in the file did not choose this text.
    $start = [Math]::Max(0, $Index - 600)
    $window = $Code.Substring($start, $Index - $start)

    # Conditional expression: cond ? "literal" : other. Exclude ?. ?? and ?[.
    $ternaries = @([regex]::Matches($window, '\?(?![\.\?\[])'))
    if ($ternaries.Count -gt 0) {
        $question = $ternaries[$ternaries.Count - 1].Index
        $before = $window.Substring(0, $question)
        $conditionStart = 0
        foreach ($boundary in [regex]::Matches($before, '[;{}]')) {
            $conditionStart = $boundary.Index + 1
        }
        $condition = $before.Substring($conditionStart)
        if ($condition -cmatch $comparison) {
            return "the conditional expression '$(($condition.Trim() -replace '\s+', ' '))'"
        }
    }

    # Guarded assignment: if (cond) { target = "literal"; }
    $ifs = @([regex]::Matches($window, '\bif\s*\('))
    if ($ifs.Count -eq 0) { return $null }

    $lastIf = $ifs[$ifs.Count - 1]
    $cursor = $lastIf.Index + $lastIf.Length
    $depth = 1
    $condition = ''
    while ($cursor -lt $window.Length -and $depth -gt 0) {
        $character = $window[$cursor]
        if ($character -ceq '(') { $depth++ }
        elseif ($character -ceq ')') {
            $depth--
            if ($depth -eq 0) { break }
        }
        $condition += $character
        $cursor++
    }
    if ($condition -cnotmatch $comparison) { return $null }

    # The literal only belongs to that branch while the branch still contains
    # it: a braced body must still be open, and a braceless body ends at its
    # first statement.
    $body = $window.Substring([Math]::Min($cursor + 1, $window.Length))
    if ($body.TrimStart().StartsWith('{')) {
        $opened = @([regex]::Matches($body, '\{')).Count
        $closed = @([regex]::Matches($body, '\}')).Count
        if ($opened -le $closed) { return $null }
    }
    elseif ($body -cmatch ';') { return $null }

    return "the branch 'if ($(($condition.Trim() -replace '\s+', ' ')))'"
}

function Assert-ReplicationDisappearanceOracleProvesPresence {
    <#
    .SYNOPSIS
        Rejects an oracle that proves an element went away without ever
        proving the element was there.

    .DESCRIPTION
        kubaflo/maui#506 counted the native elements matching
        "CustomBusyIndicator", kept only the ones answering IsDisplayed(), and
        asserted the total was Is.Zero. The reviewer deleted a single line --
        the AutomationId on the host page -- left the product bug untouched,
        and the test went green: FindElements matched nothing, so the counter
        never left 0. An oracle that passes when its own locator is broken
        cannot distinguish a fixed product from an absent one.

        The test looked well guarded because it asserted
        "IndicatorAttachedAndVisible=True" first, but that string is a label
        the host page assigns itself; it says the app believed the indicator
        was attached, not that the runner could see it.

        Asserting absence is not itself suspect. kubaflo/maui#545 asserts
        FindElements(EmptyViewText) is empty, and there the emptiness is the
        whole contract -- that view must never appear while items exist, so
        there is no earlier moment at which it could be found. What separates
        the two is the IsDisplayed() filter: counting *displayed* matches and
        expecting none is a disappearance claim, and a disappearance
        presupposes a prior appearance that the same locator has to witness.

        So this fires only on the disappearance shape, and only when the
        locator is never handed to a WaitForElement* call anywhere in the
        file. Waiting on the same locator both proves it resolves and pins
        the before-state the zero is measured against.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Content)) { return }

    $source = Get-ReplicationCommentFreeText -Text $Content -Path $Path

    foreach ($match in [regex]::Matches(
            $source,
            'FindElements\s*\(\s*([^),]+?)\s*\)')) {
        $locator = $match.Groups[1].Value.Trim()
        if (-not $locator) { continue }

        # The claim is read from the query forward: the filter and the zero
        # belong to this call, not to some later unrelated assertion.
        $start = $match.Index + $match.Length
        $window = $source.Substring($start, [Math]::Min(600, $source.Length - $start))

        if ($window -notmatch 'IsDisplayed\s*\(') { continue }
        if ($window -notmatch '(?:Is\s*\.\s*Zero|Is\s*\.\s*EqualTo\s*\(\s*0\s*\)|Is\s*\.\s*Empty)') { continue }

        if ([regex]::IsMatch($source, 'WaitForElement\w*\s*\(\s*' + [regex]::Escape($locator) + '\s*[),]')) { continue }

        throw (
            "$Path counts displayed elements matching $locator and asserts none remain, " +
            "but nothing in the test ever proves that $locator resolves to an element. " +
            "Breaking the locator alone would make this pass on unfixed product code -- " +
            "kubaflo/maui#506 was refuted exactly that way, by deleting one AutomationId. " +
            "Wait for $locator and assert it is displayed before the behaviour that is " +
            "supposed to hide it, so the count is measured falling from a witnessed " +
            "non-zero to zero. A status label the page assigns itself is not a witness."
        )
    }
}

function Assert-ReplicationVerdictIsNotComputedByTheApp {
    <#
    .SYNOPSIS
        Rejects an oracle that asserts a word the host application chose by
        comparing values itself.

    .DESCRIPTION
        Two reviewed reproductions failed for the same reason: the scene
        compared the observed state against its own expectation, wrote the
        answer into a label as a word, and the test asserted that word. One
        page decided 'edge = element == border ? "ALIGNED" : "MISSING"'; the
        other decided '_scrollBarChanges == 0 ? "scrollbar stable" : ...'. In
        both the app is the judge and the test only repeats the judgement, so
        a wrong expectation in the scene reads as a passing test and the
        reproduction proves nothing about the product.

        A scene may still report freely. Captions, starting sentinels and
        interpolated measurements such as $"TAPPED: Width={width}" are all
        untouched, because none of them is chosen by a comparison. Only the
        act of deciding is refused, and the fix is to emit the measurement and
        let the test compare it.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][hashtable]$Files
    )

    $asserted = @{}
    foreach ($path in $Files.Keys) {
        $normalized = $path.Replace('\', '/')
        if ($normalized -cmatch '(?i)TestCases\.HostApp/') { continue }
        if ($normalized -cnotmatch '(?i)\.cs$') { continue }

        $code = Get-ReplicationCommentFreeText -Text $Files[$path] -Path $path
        $pattern =
            '(?:Is\s*\.\s*(?:All\s*\.\s*)?EqualTo\s*\(\s*' +
            '|(?:Assert|ClassicAssert)\s*\.\s*(?:Equal|AreEqual)\s*\(\s*)' +
            '"(?<literal>[^"]+)"'
        foreach ($match in [regex]::Matches($code, $pattern)) {
            $asserted[$match.Groups['literal'].Value] = $normalized
        }
    }
    if ($asserted.Count -eq 0) { return }

    foreach ($path in $Files.Keys) {
        $normalized = $path.Replace('\', '/')
        if ($normalized -cnotmatch '(?i)TestCases\.HostApp/') { continue }
        if ($normalized -cnotmatch '(?i)\.cs$') { continue }

        $code = Get-ReplicationCommentFreeText -Text $Files[$path] -Path $path
        foreach ($literal in $asserted.Keys) {
            $needle = '"' + [regex]::Escape($literal) + '"'
            foreach ($occurrence in [regex]::Matches($code, $needle)) {
                $decision = Get-ReplicationComparisonSelectedLiteral `
                    -Code $code `
                    -Index $occurrence.Index
                if (-not $decision) { continue }

                throw ("Candidate test source '$($asserted[$literal])' asserts the text " +
                    "'$literal', which the host page '$normalized' selects with $decision. " +
                    'The scene is comparing the observed state against its own expectation and ' +
                    'writing the answer down, so the test only repeats a verdict the application ' +
                    'already reached. If that expectation is wrong the test still passes, and a ' +
                    'product fix cannot turn it green on its own. Report the measured value from ' +
                    'the page instead and let the test compare it.')
            }
        }
    }
}

function Get-ReplicationSystemFonts {
    <#
    .SYNOPSIS
        Fonts an operating system provides without any registration.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [string]$Platform
    )

    $shared = @(
        'Arial', 'Courier New', 'Georgia', 'Times New Roman',
        'Trebuchet MS', 'Verdana'
    )
    switch ($Platform) {
        'windows' {
            return @($shared + @(
                'Segoe UI', 'Segoe UI Bold', 'Segoe UI Semibold', 'Segoe UI Light',
                'Segoe UI Variable', 'Segoe MDL2 Assets', 'Segoe Fluent Icons',
                'Calibri', 'Cambria', 'Consolas', 'Tahoma', 'MS Gothic'))
        }
        'android' {
            return @(
                'Roboto', 'sans-serif', 'sans-serif-medium', 'sans-serif-light',
                'sans-serif-condensed', 'sans-serif-thin', 'sans-serif-black',
                'serif', 'monospace', 'casual', 'cursive',
                'Droid Sans', 'Droid Serif', 'Noto Sans', 'Noto Serif')
        }
        default {
            # iOS and Mac Catalyst share the same system font library.
            return @($shared + @(
                'Helvetica', 'Helvetica Neue', 'San Francisco', '.SFUI-Regular',
                'Avenir', 'Avenir Next', 'Courier', 'Menlo', 'Palatino',
                'Marker Felt', 'Zapfino', 'Chalkboard SE', 'Baskerville'))
        }
    }
}

function Assert-ReplicationFontIsAvailable {
    <#
    .SYNOPSIS
        Rejects a candidate that asks for a font the repository does not ship.

    .DESCRIPTION
        A reviewer rejected a text-metrics reproduction as a wrong-reason
        failure: the font it needed was not present, so the test went red for a
        missing dependency rather than for the reported defect, and the failure
        it did produce tracked a wrapped-tail coordinate instead. A candidate
        is add-only and cannot ship a font binary, so a font alias the host
        application never registers can only fail for the wrong reason.

        The operating system's own fonts are the exception, because nothing has
        to be registered or shipped to use them. That is the whole difference
        between the two cases reviewers ruled on: kubaflo/maui#230 asked for
        "Segoe UI Bold" on iOS, where no such font exists, while #232 asked for
        "Arial" on Windows, where it always does, and was accepted.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [AllowEmptyString()][string]$Platform = ''
    )

    $requested = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($match in [regex]::Matches($Content, 'FontFamily\s*=\s*"(?<name>[^"]+)"')) {
        [void]$requested.Add($match.Groups['name'].Value)
    }
    if ($requested.Count -eq 0) { return }

    $program = Join-Path $RepositoryRoot 'src/Controls/tests/TestCases.HostApp/MauiProgram.cs'
    if (-not (Test-Path -LiteralPath $program -PathType Leaf)) { return }

    $registered = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($match in [regex]::Matches(
        [System.IO.File]::ReadAllText($program),
        'AddFont\s*\(\s*"(?<file>[^"]+)"\s*(?:,\s*"(?<alias>[^"]+)"\s*)?\)')) {
        [void]$registered.Add($match.Groups['file'].Value)
        if ($match.Groups['alias'].Success) {
            [void]$registered.Add($match.Groups['alias'].Value)
        }
    }

    if ($Platform) {
        foreach ($systemFont in (Get-ReplicationSystemFonts -Platform $Platform)) {
            [void]$registered.Add($systemFont)
        }
    }

    foreach ($name in $requested) {
        if ($registered.Contains($name)) { continue }
        $where = if ($Platform) { " and $Platform does not provide it as a system font" } else { '' }
        throw ("Candidate test source '$Path' asks for the font '$name', which the test host application does not " +
            "register$where. The font cannot be added by an add-only reproduction, so the test would go red because the " +
            'font is missing rather than because of the reported defect. Use a font the host application already ' +
            'registers, or assert something that does not depend on a font the repository does not ship.')
    }
}


function Assert-ReplicationDeviceTestIsSelectable {
    <#
        .SYNOPSIS
        Requires a generated device test to carry an issue-keyed category.

        .DESCRIPTION
        The replication verifier hands Run-DeviceTests.ps1 the bare token
        "Issue<N>" as its -TestFilter. Get-CategoryFiltersFromTestFilter reads
        a token with no '=' or '~' in it as a *category name*, and
        Select-DeviceTestCategories then keeps only the discovered categories
        that match it -- exactly first, then by substring. When no test
        declares that category, nothing matches and the run executes zero
        tests, which the verifier reports as an infrastructure failure. The
        candidate is then thrown away after paying for a full device run.

        -IncludeClasses narrows further *within* the selected categories; it
        cannot widen an empty selection back out. So the category is what makes
        the test reachable at all, and the class filter is what keeps a noisy
        sibling from muddying the verdict.

        CategoryAttribute takes params string[] and allows multiples, so an
        issue-keyed category can sit alongside the conventional
        [Category(TestCategory.Entry)] without touching TestCategory -- which
        matters, because editing that shared file is not add-only. On the
        skip-filtered platforms that second category makes the test
        unselectable; Assert-ReplicationDeviceCategoryIsExclusive rejects it.

        With [Category("Issue<N>")] present, the token names a real discovered
        category, so the selector published in the pull request is one the
        stock runner actually honours.
    #>
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][int]$Issue
    )

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return $false
    }

    $token = "Issue$Issue"
    # [Category("Issue37275")] or [Category(TestCategory.Entry, "Issue37275")]
    $pattern = '(?m)^\s*\[\s*(?:(?:[A-Za-z_]\w*)\.)*Category\s*\([^)]*"' +
        [regex]::Escape($token) + '"'
    return [bool]([regex]::IsMatch($Content, $pattern))
}

function Assert-ReplicationDeviceCategoryIsExclusive {
    <#
        .SYNOPSIS
        Reports a conventional category that makes an issue-keyed device test
        unselectable on the skip-filtered platforms.

        .DESCRIPTION
        DeviceTestSharedHelpers.GetExcludedTestCategories implements
        "TestFilter=Category=X" by *subtraction*: it lists the public static
        string fields of TestCategory, removes X, and excludes everything that
        is left. "Issue<N>" is deliberately not a TestCategory field, so
        removing it removes nothing and every conventional category ends up in
        the excluded list. A test that also declares [Category(TestCategory.Shape)]
        therefore carries an excluded category and is skipped -- the published
        selector selects it out rather than in.

        Reviewers measured exactly this twice: PR 533 (Android, Shape +
        Issue31330) reported 576 discovered / 3 passed / 573 ignored, and
        PR 515 (Mac Catalyst, Accessibility + Issue37140) executed zero tests.
        In both cases removing only the broad category made the exact test run.

        Windows is exempt. Its runner filters by *discovered* traits
        (ControlsHeadlessTestRunner collects tc.Traits["Category"]), so
        "Issue<N>" is a real category there and a second one is harmless --
        which is why PR 525 selected its single Windows test correctly.

        Returns the offending category argument, or an empty string when the
        issue-keyed category is the only one.
    #>
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][int]$Issue,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Platform
    )

    if ([string]::IsNullOrWhiteSpace($Content)) { return '' }
    if ($Platform -notin @('android', 'ios', 'catalyst')) { return '' }

    # A commented-out attribute declares nothing.
    $source = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $token = '"Issue' + $Issue + '"'

    foreach ($match in [regex]::Matches(
            $source,
            '(?m)^\s*\[\s*(?:(?:[A-Za-z_]\w*)\.)*Category\s*\(([^)]*)\)')) {
        foreach ($argument in ($match.Groups[1].Value -split ',')) {
            $trimmed = $argument.Trim()
            if (-not $trimmed) { continue }
            if ($trimmed -ceq $token) { continue }
            return $trimmed
        }
    }

    return ''
}

function Get-ReplicationEnvironmentCapabilityPattern {
    # Calls that report which lane the test landed in, not what the product did.
    return '(?:OperatingSystem\.Is[A-Za-z]*VersionAtLeast|UIDevice\.CurrentDevice\.CheckSystemVersion|Build\.VERSION\.SdkInt)'
}

function Get-ReplicationUiLaneGuardPattern {
    # UI-tier only. The lane a shared NUnit file landed in is decided by the
    # driver type or by a version helper, not by the OperatingSystem APIs that
    # Get-ReplicationEnvironmentCapabilityPattern names: across the 5 real
    # offending fix PRs the guards were "App is not AppiumIOSApp" and
    # "HelperExtensions.IsIOS26OrHigher", and neither matches that pattern. It
    # is kept separate rather than folded in so the assert/throw clauses above
    # keep their measured behaviour on the device tier.
    return '(?:App\s+is\s+not\s+Appium[A-Za-z]*App' +
        '|OperatingSystem\.Is[A-Za-z]*VersionAtLeast' +
        '|UIDevice\.CurrentDevice\.CheckSystemVersion' +
        '|Build\.VERSION\.SdkInt' +
        '|\bIs(?:IOS|Android|Windows|MacCatalyst)[0-9]*OrHigher)'
}

function Assert-ReplicationEnvironmentGateSkips {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Path,
        [Parameter(Mandatory = $false)][AllowEmptyString()][string]$TestType = ''
    )

    # A reproduction pinned to an OS floor is fine; nominating that floor as a
    # failure is not. PR 213 asserted `OperatingSystem.IsIOSVersionAtLeast(26)`
    # on line 25 and every one of its five runs died there in 1.8-3.0 ms,
    # before the oracle it advertised ever executed -- red for the lane, not
    # for the defect, and still red after a complete product fix. No Assert of
    # any kind names one of these APIs and no version gate throws, so this
    # rejects a shape the repository never uses.
    #
    # The remedy is tier-specific, and saying otherwise caused a second defect.
    # An earlier revision prescribed the early return unconditionally on the
    # strength of "49 sites gate with an early return" -- a count taken over
    # the whole tree. Stratified, those sites are 31 xUnit device tests and 31
    # product/sample files; the NUnit UI tier uses the shape ZERO times and
    # reaches for Assert.Ignore in 44 files instead. So the prescription had no
    # precedent in the tier it was most often applied to, and there it is
    # actively harmful: NUnit records a returned test as Passed, and a UI test
    # is link-compiled into all four platform assemblies, so a lane the gate
    # excludes reports a green it never earned. A reviewer measured exactly
    # that on PR 464 -- "PASS in 32 ms without opening the page" on an iOS 18.5
    # runner against a production-reverted build.
    #
    # xUnit has no Assert.Ignore (0 uses in DeviceTests), so device and unit
    # tests keep the early return: refusing it there would leave the author no
    # legal answer, which this pipeline has already paid for twice.
    $source = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $capability = Get-ReplicationEnvironmentCapabilityPattern
    $isUiTest = $TestType -ceq 'UITest'
    $skipShape = if ($isUiTest) {
        'Assert.Ignore("the reported defect needs iOS 26 or later") -- the ' +
        'shape the UI tests use in 44 files -- so an unsupported lane is ' +
        'recorded as skipped rather than as a pass it never earned'
    }
    else {
        '"if (!OperatingSystem.IsIOSVersionAtLeast(26)) return;" -- the shape ' +
        'the device and unit test projects use at 31 sites, and the only one ' +
        'available to them, because xUnit has no Assert.Ignore'
    }
    $remedy = "Gate with the shape this tier supports instead -- $skipShape, " +
        'so the only red this test can produce is the reported defect.'

    $asserted = [regex]::Match($source, "Assert\.[A-Za-z]+\s*\([^;]{0,400}?$capability")
    if ($asserted.Success) {
        throw ("The reproduction in '$Path' asserts an environment precondition: " +
            "'$($asserted.Value.Trim())'. That turns every device below the floor red " +
            'before the oracle runs, so the failure reports the lane rather than the defect. ' +
            $remedy)
    }

    $gatedFailure = [regex]::Match(
        $source,
        "if\s*\(\s*!\s*$capability[^;{]{0,200}?\)\s*\{?\s*(?:throw\s+new|Assert\.Fail)"
    )
    if ($gatedFailure.Success) {
        throw ("The reproduction in '$Path' fails outright when an environment " +
            "precondition is unmet: '$($gatedFailure.Value.Trim())'. That red survives a " +
            'complete product fix, so it cannot be attributed to the reported defect. ' +
            $remedy)
    }

    if (-not $isUiTest) { return }

    # NUnit only: a bare `return;` reached from an environment guard is scored
    # Passed, so it is a false green rather than a skip. Assert.Ignore, a
    # throw, and Assert.Inconclusive all record the truth, so none of them is
    # matched here. Measured across 1,642 real files under
    # TestCases.Shared.Tests: 0 firings.
    $gatedReturn = [regex]::Match(
        $source,
        'if\s*\(\s*[^;{)]{0,200}?' + (Get-ReplicationUiLaneGuardPattern) +
            '[^;{]{0,200}?\)\s*(?:\{\s*)?return\s*;'
    )
    if ($gatedReturn.Success) {
        throw ("The reproduction in '$Path' returns without asserting when an " +
            "environment precondition is unmet: '$($gatedReturn.Value.Trim())'. NUnit " +
            'records a returned test as PASSED, and this file is link-compiled into the ' +
            'Android, iOS, MacCatalyst and Windows assemblies alike, so every lane the ' +
            'gate excludes reports a pass it never earned. ' + $remedy)
    }
}

function Assert-ReplicationTestLifecycleSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $lifecycleRules = @(
        [pscustomobject]@{
            Reason = 'a test lifecycle attribute'
            Pattern = '(?i)\[\s*(?:SetUp|TearDown|OneTimeSetUp|OneTimeTearDown|TestInitialize|TestCleanup|ClassInitialize|ClassCleanup|AssemblyInitialize|AssemblyCleanup|ModuleInitializer)\b'
        },
        [pscustomobject]@{
            Reason = 'a test fixture lifecycle contract'
            Pattern = '(?i)\b(?:IAsyncLifetime|IClassFixture|ICollectionFixture|BeforeAfterTestAttribute)\b'
        },
        [pscustomobject]@{
            Reason = 'a static field initializer that runs outside the test'
            Pattern = '(?m)^\s*(?:(?:public|internal|protected|private)\s+)?static\s+[^;(){}]+\s+[A-Za-z_]\w*\s*=(?!>)'
        },
        [pscustomobject]@{
            Reason = 'a readonly field initializer that runs outside the test'
            Pattern = '(?m)^\s*(?:(?:public|internal|protected|private)\s+)?readonly\s+[^;(){}]+\s+[A-Za-z_]\w*\s*=(?!>)'
        },
        [pscustomobject]@{
            Reason = 'a field initializer that runs outside the test'
            Pattern = '(?m)^\s*(?:public|internal|protected|private)\s+(?!class\b|interface\b|enum\b|record\b)[^;(){}]+\s+[A-Za-z_]\w*\s*=(?!>)'
        }
    )
    # A bindable property has to be declared as a static readonly field: the
    # MAUI type system offers no other way to write one, so demanding it move
    # into the test body asks for something the language cannot express. Build
    # 15066948 spent all five attempts failing that demand. Mask those
    # declarations before the lifecycle rules run, preserving every newline so
    # the reported line numbers still point at the real source.
    $bindablePropertyDeclaration =
        '(?ms)^[^\S\r\n]*(?:(?:public|internal|protected|private)[^\S\r\n]+)*' +
        'static[^\S\r\n]+readonly[^\S\r\n]+Bindable(?:Property|PropertyKey)[^\S\r\n]+' +
        '\w+\s*=\s*Bindable(?:Property|PropertyKey)\.Create\w*\s*\(.*?\)\s*;'
    $lifecycleScanContent = [regex]::Replace(
        $Content,
        $bindablePropertyDeclaration,
        {
            param($bindableMatch)
            ($bindableMatch.Value -replace '[^\r\n]', ' ')
        })

    foreach ($rule in $lifecycleRules) {
        $match = [regex]::Match($lifecycleScanContent, $rule.Pattern)
        if ($match.Success) {
            throw "Candidate test source '$Path' contains $($rule.Reason): $(Get-ReplicationUnsafeMatchDetail -ScanText ($lifecycleScanContent.Replace("`r`n", "`n")) -Match $match). Move the setup inside the test method body."
        }
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

function Get-ReplicationAssertionPattern {
    # The statements that carry the oracle. An ablation is only informative if
    # every one of these survives it untouched.
    return '(?i)(?:^|[^\w.])(?:Assert\.|Assume\.|StringAssert\.|CollectionAssert\.|ClassicAssert\.)|\.Should(?:Be|NotBe|Match|Contain|Have|Throw|Not)|\bVerifyScreenshot\b'
}

function Get-ReplicationAssertionStatements {
    <#
        .SYNOPSIS
        Returns the normalised assertion statements in a test source file.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Source
    )

    $pattern = Get-ReplicationAssertionPattern
    $statements = New-Object 'System.Collections.Generic.List[string]'
    foreach ($line in ([string]$Source -split "`r?`n")) {
        $trimmed = $line.Trim()
        if ($trimmed -match '^\s*//') { continue }
        if ([regex]::IsMatch($trimmed, $pattern)) {
            $statements.Add(([regex]::Replace($trimmed, '\s+', ' ')))
        }
    }

    # Emitted unwrapped so that callers wrapping in @() see an empty result as
    # zero assertions. Returning ,@($statements) would hand them a single empty
    # element instead, which reads as "one assertion" and defeats the check for
    # a reproduction that has no oracle at all.
    return $statements.ToArray()
}

function Get-ReplicationDisabledTestPatterns {
    # Ways a control can be made green by never really running. The certification
    # matrix rewards a passing control, so these are the shapes an agent reaches
    # for when the ablation is inconvenient.
    return @(
        [pscustomobject]@{
            Pattern = '(?im)^\s*\[\s*(?:Ignore|Explicit)\b'
            Reason  = 'it is attributed away with [Ignore] or [Explicit]'
        },
        [pscustomobject]@{
            Pattern = '(?i)\bAssert\.(?:Ignore|Inconclusive)\s*\('
            Reason  = 'it ends itself with Assert.Ignore or Assert.Inconclusive'
        },
        [pscustomobject]@{
            Pattern = '(?i)\bAssert\.Pass\s*\('
            Reason  = 'it short-circuits with Assert.Pass'
        },
        [pscustomobject]@{
            Pattern = '(?im)^\s*\[\s*Test\s*\([^)]*\bSkip\s*='
            Reason  = 'it is declared with a Skip reason'
        }
    )
}

function Assert-ReplicationNegativeControlIsInformative {
    <#
        .SYNOPSIS
        Rejects a negative control that cannot distinguish the defect.

        .DESCRIPTION
        The negative control is the arm that promotes a reproduction to a
        certified oracle, so it is the arm worth faking. There are exactly two
        cheap ways to make a control green, and both leave the reproduction
        unproven:

          - weaken the oracle, so the control passes because it no longer
            measures anything;
          - stop the control from running, so it passes vacuously.

        A control is only informative when it removes the reported trigger and
        changes nothing else. This compares the two sources directly rather than
        trusting the manifest's description of them.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$BaselineSource,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ControlSource,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$TestFilter,
        # A UI test keeps its oracle in the test file and the condition that
        # provokes the defect in the HostApp page, so the control edits one file
        # and must preserve the assertions in another. When these are omitted
        # the edited file is the oracle and the checks read the same sources as
        # before.
        [AllowEmptyString()][string]$OracleBaselineSource,
        [AllowEmptyString()][string]$OracleControlSource
    )

    if ([string]::IsNullOrWhiteSpace($ControlSource)) {
        throw "The negative control for '$TestFilter' is empty, so removing the reported trigger was never actually tried."
    }

    $normalisedBaseline = [regex]::Replace([string]$BaselineSource, '\s+', ' ').Trim()
    $normalisedControl = [regex]::Replace([string]$ControlSource, '\s+', ' ').Trim()
    if ($normalisedBaseline -ceq $normalisedControl) {
        throw ("The negative control for '$TestFilter' is identical to the reproduction, so it removes nothing and " +
            'cannot show that the failure depends on the reported trigger.')
    }

    foreach ($disabled in Get-ReplicationDisabledTestPatterns) {
        if ([regex]::IsMatch($normalisedControl, $disabled.Pattern) -and
            -not [regex]::IsMatch($normalisedBaseline, $disabled.Pattern)) {
            throw ("The negative control for '$TestFilter' passes only because $($disabled.Reason), " +
                'so it proves the control did not run rather than that the trigger causes the failure.')
        }
    }

    $oracleBaseline = if ($PSBoundParameters.ContainsKey('OracleBaselineSource')) {
        $OracleBaselineSource
    } else {
        $BaselineSource
    }
    $oracleControl = if ($PSBoundParameters.ContainsKey('OracleControlSource')) {
        $OracleControlSource
    } else {
        $ControlSource
    }
    $baselineAssertions = @(Get-ReplicationAssertionStatements -Source $oracleBaseline)
    $controlAssertions = @(Get-ReplicationAssertionStatements -Source $oracleControl)

    if ($baselineAssertions.Count -eq 0) {
        throw "The reproduction '$TestFilter' contains no assertion, so there is no oracle for a control to preserve."
    }

    if ($controlAssertions.Count -ne $baselineAssertions.Count) {
        throw ("The negative control for '$TestFilter' asserts $($controlAssertions.Count) times where the " +
            "reproduction asserts $($baselineAssertions.Count). A control must remove the reported trigger and " +
            'leave the oracle untouched, otherwise it passes because it stopped measuring.')
    }

    for ($index = 0; $index -lt $baselineAssertions.Count; $index++) {
        if ($baselineAssertions[$index] -cne $controlAssertions[$index]) {
            throw ("The negative control for '$TestFilter' changes the oracle: the reproduction asserts " +
                "'$($baselineAssertions[$index])' where the control asserts '$($controlAssertions[$index])'. " +
                'A control must differ only in the reported trigger.')
        }
    }
}

function Get-ReplicationWhitespaceInsensitiveSpan {
    <#
        .SYNOPSIS
        Locates text in a source ignoring how it happens to be indented.

        .DESCRIPTION
        An author quoting several lines of XAML has to reproduce every tab to
        match byte for byte, and build 15033553 shows they do not: two of three
        attempts quoted the right element with the wrong indentation and the
        control was skipped. Collapsing whitespace asks the author for the right
        code instead of the right bytes.

        This is stricter about ambiguity rather than looser. Two regions that
        differ only in whitespace collapse to the same text, so both are counted
        and the caller refuses the edit.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Source,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Find
    )

    $normalized = [System.Text.StringBuilder]::new()
    $map = [System.Collections.Generic.List[int]]::new()
    $cursor = 0
    while ($cursor -lt $Source.Length) {
        if ([char]::IsWhiteSpace($Source[$cursor])) {
            $runStart = $cursor
            while ($cursor -lt $Source.Length -and [char]::IsWhiteSpace($Source[$cursor])) {
                $cursor++
            }
            [void] $normalized.Append(' ')
            [void] $map.Add($runStart)
            continue
        }

        [void] $normalized.Append($Source[$cursor])
        [void] $map.Add($cursor)
        $cursor++
    }

    $needle = ([regex]::Replace($Find, '\s+', ' ')).Trim()
    if ([string]::IsNullOrEmpty($needle)) {
        return [pscustomobject]@{ Count = 0; Index = -1; Length = 0 }
    }

    $haystack = $normalized.ToString()
    $count = 0
    $first = -1
    $search = 0
    while ($search -ge 0 -and $search -le ($haystack.Length - $needle.Length)) {
        $hit = $haystack.IndexOf($needle, $search, [StringComparison]::Ordinal)
        if ($hit -lt 0) { break }
        $count++
        if ($first -lt 0) { $first = $hit }
        $search = $hit + 1
    }

    if ($count -ne 1) {
        return [pscustomobject]@{ Count = $count; Index = -1; Length = 0 }
    }

    # The needle is trimmed, so its last character is never a collapsed
    # whitespace run and the original span ends one past that character.
    $startOriginal = $map[$first]
    $endOriginal = $map[$first + $needle.Length - 1] + 1
    return [pscustomobject]@{
        Count  = 1
        Index  = $startOriginal
        Length = ($endOriginal - $startOriginal)
    }
}

function New-ReplicationControlVariant {
    <#
        .SYNOPSIS
        Builds a negative control by removing the reported trigger from the
        reproduction source.

        .DESCRIPTION
        Authors were asked three times, in explicit prose, to copy the
        reproduction and delete only the trigger, and they returned a variant
        with no assertions every time. Prose does not constrain an author, so
        this takes the file away from them: they describe the trigger edits and
        trusted code performs them, which makes the oracle byte-identical by
        construction rather than by instruction.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$BaselineSource,
        [Parameter(Mandatory = $true)][AllowNull()]$Edits
    )

    $editList = @($Edits)
    if ($editList.Count -eq 0) {
        throw 'The control edits list is empty. List the exact trigger statements to remove.'
    }
    if ($editList.Count -gt 10) {
        throw "The control edits list has $($editList.Count) entries, which is more than the 10 a trigger removal should need."
    }

    $baselineAssertions = @(Get-ReplicationAssertionStatements -Source $BaselineSource)
    $variant = [string]$BaselineSource

    foreach ($edit in $editList) {
        $find = ''
        $replace = ''
        if ($edit -is [string]) {
            $find = [string]$edit
        } elseif ($edit -is [System.Collections.IDictionary]) {
            if ($edit.Contains('find')) { $find = [string]$edit['find'] }
            if ($edit.Contains('replace') -and $null -ne $edit['replace']) {
                $replace = [string]$edit['replace']
            }
        } else {
            $findProperty = $edit.PSObject.Properties['find']
            if ($findProperty) { $find = [string]$findProperty.Value }
            $replaceProperty = $edit.PSObject.Properties['replace']
            if ($replaceProperty -and $null -ne $replaceProperty.Value) {
                $replace = [string]$replaceProperty.Value
            }
        }

        if ([string]::IsNullOrWhiteSpace($find)) {
            throw 'A control edit has an empty find value. Quote the exact trigger text from the reproduction.'
        }

        # An edit that carries an assertion is the author deleting the oracle
        # under the name of removing the trigger, which is the exact failure
        # this function exists to make impossible.
        if (@(Get-ReplicationAssertionStatements -Source $find).Count -gt 0) {
            throw "A control edit removes an assertion: '$($find.Trim())'. Remove only the trigger and leave every assertion in place."
        }
        if (@(Get-ReplicationAssertionStatements -Source $replace).Count -gt 0) {
            throw "A control edit introduces an assertion: '$($replace.Trim())'. The control must keep the reproduction's assertions unchanged."
        }

        $occurrences = ([regex]::Matches($variant, [regex]::Escape($find))).Count
        if ($occurrences -eq 1) {
            $index = $variant.IndexOf($find, [StringComparison]::Ordinal)
            $length = $find.Length
        } else {
            $span = Get-ReplicationWhitespaceInsensitiveSpan -Source $variant -Find $find
            if ($span.Count -ne 1) {
                throw ("The control edit text occurs $($span.Count) times in the reproduction, " +
                    "ignoring indentation, but it must occur exactly once: '$($find.Trim())'.")
            }
            $index = $span.Index
            $length = $span.Length
        }

        $variant = $variant.Substring(0, $index) + $replace +
            $variant.Substring($index + $length)
    }

    if ($variant -ceq [string]$BaselineSource) {
        throw 'The control edits changed nothing, so the control would rerun the reproduction unchanged.'
    }

    $variantAssertions = @(Get-ReplicationAssertionStatements -Source $variant)
    if ($variantAssertions.Count -ne $baselineAssertions.Count) {
        throw "The control has $($variantAssertions.Count) assertions where the reproduction has $($baselineAssertions.Count). Remove only the trigger."
    }
    for ($i = 0; $i -lt $variantAssertions.Count; $i++) {
        if ($variantAssertions[$i] -cne $baselineAssertions[$i]) {
            throw "Control assertion $($i + 1) changed from '$($baselineAssertions[$i])' to '$($variantAssertions[$i])'."
        }
    }

    return $variant
}
