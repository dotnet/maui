#!/usr/bin/env pwsh

Set-StrictMode -Version Latest
$script:ReplicationBuildExecutedPathCache = @{}

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
        [pscustomobject]@{ Code = 'xml-external-access'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Xml\b|\b(?:XmlReader|XmlTextReader|XmlValidatingReader|XmlUrlResolver|XmlSecureResolver|XmlPreloadedResolver|XmlResolver|XmlReaderSettings|XmlDocument|XmlDataDocument|XDocument|XElement|XPathDocument|XslCompiledTransform|XmlSchemaSet|XmlSchema)\b|\.\s*(?:Read|Write|Infer)Xml(?:Schema)?\s*\(' },
        [pscustomobject]@{ Code = 'platform-network'; Scope = 'code'; Pattern = '(?i)\b(?:NSUrl|NSURL|NSUrlRequest|NSURLRequest|NSMutableUrlRequest|NSURLConnection|WKWebView|UIWebView|SFSafariViewController|ASWebAuthenticationSession|NSUrlSession|NSURLSession|NWConnection|CFNetwork)\b|\b(?:Android|Java)\s*\.\s*(?:Net|Webkit)\b|\b(?:HttpURLConnection|URLConnection|AndroidHttpClient|OkHttpClient|CustomTabsIntent|DownloadManager|WebViewClient)\b|\b(?:Intent|PendingIntent)\s*(?:[.(]|\b)|\b(?:Windows\s*\.\s*(?:Web\s*\.\s*Http|Foundation\s*\.\s*Uri|System\s*\.\s*Launcher)|Microsoft\s*\.\s*Web\s*\.\s*WebView2|CoreWebView2|WebView2|WinHttpHandler)\b|\b(?:LoadRequest|LoadUrl|LoadDataWithBaseURL|Navigate|NavigateWithHttpRequestMessage|LaunchUriAsync)\s*\(' },
        [pscustomobject]@{ Code = 'windows-activation'; Scope = 'code'; Pattern = '(?i)\b(?:ComImport|IApplicationActivationManager|ApplicationActivationManager|PackageManager|FullTrustProcessLauncher|AppServiceConnection|AppInstance|ActivationFactory|RoGetActivationFactory|CoCreateInstance|GetActiveObject|GetTypeFromProgID)\b|\bWindows\s*\.\s*ApplicationModel\s*\.\s*(?:Activation|AppService)\b|\bSystem\s*\.\s*Runtime\s*\.\s*InteropServices\s*\.\s*ComTypes\b' },
        [pscustomobject]@{ Code = 'dynamic-xaml'; Scope = 'code'; Pattern = '(?i)\b(?:XamlReader|XamlServices|LoadFromXaml|ParseXaml|LoadXaml)\b|\b(?:Microsoft\s*\.\s*UI|Windows\s*\.\s*UI|System\s*\.\s*Windows)\s*\.\s*Xaml\s*\.\s*Markup\b' },
        [pscustomobject]@{ Code = 'process-start'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Diagnostics\s*\.\s*Process\b|\bProcess\s*\.\s*(?:Start|GetCurrentProcess|GetProcess|GetProcesses|GetProcessById|EnterDebugMode|Kill)\b|\bnew\s+Process\s*[({]|\b(?:ProcessStartInfo|UseShellExecute|RedirectStandardOutput|RedirectStandardError|WaitForExit|Win32Exception|ManagementObject|NSTask|NSWorkspace|CreateProcess|ShellExecute|Runtime\s*\.\s*GetRuntime)\b' },
        [pscustomobject]@{ Code = 'reflection'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*(?:Reflection|Runtime\s*\.\s*Loader|Type)\b|\b(?:Assembly|Type|Activator|AppDomain|MethodInfo|MethodBase|PropertyInfo|FieldInfo|ConstructorInfo|DynamicMethod|Delegate)\s*\.\s*(?:Load|LoadFrom|LoadFile|LoadWithPartialName|CreateInstance|CreateDelegate|DynamicInvoke|Invoke|InvokeMember|GetMethod|GetMethods|GetField|GetFields|GetProperty|GetProperties|GetMember|GetMembers|GetTypes|GetConstructors|GetNestedTypes|GetRuntimeMethod|GetRuntimeMethods|GetRuntimeField|GetRuntimeFields|GetRuntimeProperty|GetRuntimeProperties|GetTypeFromHandle|GetType)\b|\b(?:Activator|AppDomain|MethodInfo|MethodBase|PropertyInfo|FieldInfo|ConstructorInfo|DynamicMethod|RuntimeMethodHandle|InvokeMember|GetMethod|GetMethods|GetField|GetFields|GetMember|GetMembers|GetProperties|GetConstructors|GetNestedTypes|GetRuntimeMethod|GetRuntimeMethods|GetRuntimeField|GetRuntimeFields|GetRuntimeProperty|GetRuntimeProperties)\b|(?<!Mapper\s*\.\s*)\bGetProperty\b|\bType\s*\.\s*GetType\b|\btypeof\s*\([^)]*\)\s*\.\s*Assembly\b|\bGetType\s*\(\s*\)\s*\.\s*(?!Name\b|FullName\b|ToString\b)' },
        [pscustomobject]@{ Code = 'dynamic-loading'; Scope = 'code'; Pattern = '(?i)\b(?:dynamic|ExpandoObject|CallSite|DynamicMetaObject|IDynamicMetaObjectProvider)\b|\bRuntimeHelpers\s*\.\s*(?:GetUninitializedObject|PrepareMethod|RunClassConstructor)\b' },
        [pscustomobject]@{ Code = 'native-code'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Runtime\s*\.\s*InteropServices\b(?!\s*\.\s*COMException\b)|\b(?:DllImport|LibraryImport|GeneratedDllImport|NativeLibrary|UnmanagedCallersOnly|GetDelegateForFunctionPointer|GCHandle)\b|\bMarshal\s*\.|(?-i:\bunsafe\s*[{(]|\bunsafe\s+(?:static|void|partial|class|struct|int|byte|char|fixed)\b|\bstackalloc\b|\bextern\s+(?:static|alias)\b|\bstatic\s+extern\b)' },
        [pscustomobject]@{ Code = 'environment-secrets'; Scope = 'code'; Pattern = '(?i)\busing\s+[A-Za-z_]\w*\s*=\s*(?:global\s*::)?System\s*\.\s*Environment\s*;|\b(?:System\s*\.\s*)?Environment\s*\.\s*(?:GetEnvironmentVariable|GetEnvironmentVariables|SetEnvironmentVariable|ExpandEnvironmentVariables)\b|\bEnvironmentVariableTarget\b|\b(?:System\s*\.\s*)?AppContext\s*\.\s*(?:GetData|SetData|BaseDirectory)\b|\b(?:GH_TOKEN|GITHUB_TOKEN|COPILOT_GITHUB_TOKEN|SYSTEM_ACCESSTOKEN|AZURE_STORAGE_KEY|AZURE_STORAGE_SAS_TOKEN)\b' },
        [pscustomobject]@{ Code = 'device-external-access'; Scope = 'code'; Pattern = '(?i)\b(?:Browser|Launcher|SecureStorage|FileSystem|Connectivity|Clipboard|Preferences)\s*\.|\b(?:ImageSource|HybridWebView|BlazorWebView|UrlWebViewSource|UriImageSource|FileImageSource|UIApplication|PendingIntent)\b|(?<![A-Za-z0-9_])(?:Source|BackgroundImageSource|ImageSource|IconImageSource)\s*=(?!=)|\b(?:SetValue|SetBinding)\s*\(\s*(?:[A-Za-z_]\w*\s*\.\s*)*(?:Source|BackgroundImageSource|ImageSource|IconImageSource)Property\b' },
        [pscustomobject]@{ Code = 'webview'; Scope = 'literal'; Pattern = '(?i)<\s*(?:[A-Za-z_]\w*\s*:\s*)?(?:WebView|WebView2|HybridWebView|BlazorWebView)\b|\busing\s+[A-Za-z_]\w*\s*=\s*(?:(?:global\s*::)?[A-Za-z_]\w*\s*\.\s*)*(?:WebView|WebView2|HybridWebView|BlazorWebView|UrlWebViewSource|HtmlWebViewSource)\s*;|\bnew\s+(?:(?:global\s*::)?[A-Za-z_]\w*\s*\.\s*)*(?:WebView|WebView2|HybridWebView|BlazorWebView|UrlWebViewSource|HtmlWebViewSource)\b|\b(?:WebView|WebView2|HybridWebView|BlazorWebView|UrlWebViewSource|HtmlWebViewSource)\s*(?:<|\(|\.|[A-Za-z_]\w*\s*(?:[=;,)])|[),])|\b(?:as|typeof)\s*\(?\s*(?:(?:global\s*::)?[A-Za-z_]\w*\s*\.\s*)*(?:WebView|WebView2|HybridWebView|BlazorWebView)\b' },
        [pscustomobject]@{ Code = 'uri-construction'; Scope = 'code'; Pattern = '(?i)\b(?:System\s*\.\s*)?Uri(?:Builder)?\b|\b(?:Uri|UriBuilder)\s*\.\s*(?:Parse|TryCreate|EscapeDataString|UnescapeDataString)\b' },
        [pscustomobject]@{ Code = 'source-generator-analyzer'; Scope = 'code'; Pattern = '(?i)\bMicrosoft\s*\.\s*CodeAnalysis\b|\b(?:ISourceGenerator|IIncrementalGenerator|GeneratorInitializationContext|IncrementalGeneratorInitializationContext|GeneratorExecutionContext|DiagnosticAnalyzer(?:Attribute)?|AnalysisContext|GeneratorDriver|CSharpGeneratorDriver|Register(?:SourceOutput|ImplementationSourceOutput|PostInitializationOutput))\b|\[\s*(?:[A-Za-z_]\w*\s*\.\s*)?(?:Generator|DiagnosticAnalyzer)(?:Attribute)?\b' },
        [pscustomobject]@{ Code = 'build-execution-deputy'; Scope = 'code'; Pattern = '(?i)\bMicrosoft\s*\.\s*Build\s*\.\s*(?:Utilities|Framework)\b|\b(?:ToolTask|ToolTaskExtension|IBuildEngine\d*|BuildEngine\d*|TaskLoggingHelper|CodeTaskFactory|RoslynCodeTaskFactory)\b|\bnew\s+(?:Microsoft\s*\.\s*Build\s*\.\s*)?(?:Exec|MSBuild)\b' },
        [pscustomobject]@{ Code = 'module-initializer'; Scope = 'code'; Pattern = '(?i)\[\s*(?:[A-Za-z_]\w*\s*\.\s*)?ModuleInitializer(?:Attribute)?\b' },
        [pscustomobject]@{ Code = 'assembly-runtime-policy'; Scope = 'code'; Pattern = '(?i)\b(?:DefaultDllImportSearchPaths|DisableRuntimeMarshalling|SkipLocalsInit|UnverifiableCode|SecurityRules|SecurityPermission)(?:Attribute)?\b' },
        [pscustomobject]@{ Code = 'preprocessor-symbol'; Scope = 'code'; Pattern = '(?im)^\s*#\s*(?:define|undef)\b' },
        [pscustomobject]@{ Code = 'static-constructor'; Scope = 'code'; Pattern = '(?im)(?:^|[;{}])\s*(?:(?:public|internal|protected|private)\s+)?static\s+(?!class\b|struct\b|interface\b|void\b)[A-Za-z_]\w*\s*\(\s*\)' },
        [pscustomobject]@{ Code = 'global-exception-suppression'; Scope = 'code'; Pattern = '(?i)\b(?:UnhandledException|UnobservedTaskException|FirstChanceException|MarshalManagedException|AndroidEnvironment)\b'; Remedy = 'A reproduction must not take over the process-wide failure path. Suppressing the crash puts the app in a state a user never sees, and it hides the one symptom the harness can observe without the app reporting on itself. End the Appium plan with assertAppClosed instead, which now works on every platform. If the report names an exact managed exception type, wrap only the reported trigger in a try/catch for that exact type and set the semantic result element from the catch.' },
        [pscustomobject]@{ Code = 'delays-or-background-work'; Scope = 'code'; Pattern = '(?i)\bThread\s*\.\s*Sleep\b|\bTask\s*\.\s*(?:Delay|Run|Factory)\b|\b(?:DispatcherTimer|IDispatcherTimer)\b|\bSystem\s*\.\s*(?:Timers|Threading)\s*\.\s*Timer\b|\bnew\s+\w*Timer\s*\(|\b(?:Create|Start)Timer\s*\(|\bDispatchDelayed\b'; Remedy = 'Write one of these instead. Subscribe to the event that reports the change (Loaded, SizeChanged, PropertyChanged, or the control''s own event) and publish the result from its handler. Or post the measurement with Dispatcher.Dispatch(() => ...), which runs after the pending layout pass without waiting on the clock. Or give the page a separate check control and let the Appium plan tap trigger, wait, then tap check. Waiting on wall-clock time inside the app is never accepted, so re-sending it will fail this attempt again.' },
        [pscustomobject]@{ Code = 'shell-execution'; Scope = 'code'; Pattern = '(?i)\b(?:powershell|pwsh)(?:\.exe)?\s*(?:-|\.exe\b)|\bcmd\.exe\b|/(?:bin/)?(?:ba|z)?sh\b|\bbash\s+-|\bSystem\s*\.\s*Management\s*\.\s*Automation\b' },
        [pscustomobject]@{ Code = 'remote-url'; Scope = 'raw'; Pattern = '(?i)(?:\b(?:https?|ftps?|wss?|file|data|javascript|mailto)\s*:\s*(?://|[^\s<>\[\]]+)|://)' },
        [pscustomobject]@{ Code = 'encoded-url'; Scope = 'code'; Pattern = '(?i)\bFromBase64String\b|\b(?:Char|System\s*\.\s*Char)\s*\.\s*ConvertFromUtf32\b|\bSystem\s*\.\s*Text\s*\.\s*Encoding\b|\bEncoding\s*\.\s*(?:UTF8|Unicode|ASCII|BigEndianUnicode)\s*\.\s*GetString\b|\(\s*char\s*\)\s*\(?\s*(?:0x[0-9a-f]{1,8}|\d{2,7})\s*\)?|\bnew\s+string\s*\(\s*(?:(?:new\s+)?(?:char|byte)\s*\[|new\s*\[\s*\]\s*\{)' },
        [pscustomobject]@{ Code = 'package-reference'; Scope = 'raw'; Pattern = '(?i)\b(?:PackageReference|PackageDownload|dotnet\s+add\s+package|nuget\s*:|nuget\.exe)\b|#(?:r|load)\b' },
        [pscustomobject]@{ Code = 'project-build-script'; Scope = 'raw'; Pattern = '(?i)\b(?:ProjectReference|Directory\.Build|\.csproj\b|\.props\b|\.targets\b|<Project\b|<Target\b|<PropertyGroup\b|<ItemGroup\b|dotnet\s+(?:build|test|run|pack|restore)\b)\b' },
        [pscustomobject]@{ Code = 'obfuscated-source'; Scope = 'literal'; Pattern = '(?i)\\(?:x0{0,3}(?:2f|3a|68|70|73|74)|u00(?:2f|3a|68|70|73|74)|U000000(?:2f|3a|68|70|73|74))|&#(?:x0*(?:2f|3a|68|70|73|74)|0*(?:47|58|104|112|115|116));' },
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

    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($Text)
    $nullPredicate = [System.Func[Microsoft.CodeAnalysis.SyntaxNode, bool]]$null
    $commentKinds = @(
        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::SingleLineCommentTrivia,
        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::MultiLineCommentTrivia,
        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::SingleLineDocumentationCommentTrivia,
        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::MultiLineDocumentationCommentTrivia,
        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::DocumentationCommentExteriorTrivia
    )
    foreach ($trivia in $tree.GetRoot().DescendantTrivia($nullPredicate, $true)) {
        if ($trivia.RawKind -in $commentKinds) {
            & $blank $trivia.FullSpan.Start $trivia.FullSpan.End
        }
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

function Test-ReplicationLiteralFragmentsCanAssembleUrl {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Fragments)

    $bounded = @($Fragments | ForEach-Object { [string]$_ } | Where-Object {
        $_.Length -gt 0 -and $_.Length -le 4096
    })
    if ($bounded.Count -lt 2) {
        return $false
    }
    if ($bounded.Count -gt 256) {
        return $true
    }

    foreach ($target in @(
            'http://', 'https://', 'ftp://', 'ftps://', 'ws://', 'wss://',
            'file:', 'data:', 'javascript:', 'mailto:'
        )) {
        $reachable = [bool[]]::new($target.Length + 1)
        $reachable[0] = $true
        for ($position = 0; $position -lt $target.Length; $position++) {
            if (-not $reachable[$position]) { continue }
            $remaining = $target.Substring($position)
            foreach ($fragment in $bounded) {
                if ($remaining.StartsWith(
                        $fragment,
                        [StringComparison]::OrdinalIgnoreCase)) {
                    $reachable[$position + $fragment.Length] = $true
                } elseif ($fragment.StartsWith(
                        $remaining,
                        [StringComparison]::OrdinalIgnoreCase)) {
                    return $true
                }
            }
        }
        if ($reachable[$target.Length]) {
            return $true
        }
    }

    return $false
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
        if (Test-ReplicationLiteralFragmentsCanAssembleUrl -Fragments $allFragments) {
            throw "Candidate source '$Path' contains prohibited 'remote-url' content assembled out of source order."
        }
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

    if ([IO.Path]::GetExtension($Path) -ieq '.cs') {
        if ($Content -match '[\u0085\u2028\u2029\u202A-\u202E\u2066-\u2069\u200B-\u200F\uFEFF]') {
            throw "Candidate source '$Path' contains prohibited 'obfuscated-source' Unicode control characters."
        }
        $unicodeText = Get-ReplicationCommentFreeText -Text $Content -Path $Path
        foreach ($escape in [regex]::Matches(
                $unicodeText,
                '\\(?:u(?<short>[0-9a-fA-F]{4})|U(?<long>[0-9a-fA-F]{8}))'
            )) {
            $codePoint = if ($escape.Groups['short'].Success) {
                $escape.Groups['short'].Value
            } else {
                $escape.Groups['long'].Value.TrimStart('0')
            }
            if ($codePoint -notmatch '^(?i:2028|2029)$') {
                throw "Candidate source '$Path' contains prohibited 'obfuscated-source' Unicode escapes that could obscure executable identifiers."
            }
        }
    }
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
        'windows-activation',
        'dynamic-xaml',
        'process-start',
        'reflection',
        'dynamic-loading',
        'native-code',
        'environment-secrets',
        'device-external-access',
        'webview',
        'uri-construction',
        'source-generator-analyzer',
        'build-execution-deputy',
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
            $_.GetType().Name -match '^(?:Class|Struct|Interface|Record|RecordStruct)DeclarationSyntax$'
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
        $_.GetType().Name -match '^(?:Class|Struct|Interface|Record|RecordStruct|Enum)DeclarationSyntax$'
    })) {
        $full = $node.ToFullString()
        $brace = $full.IndexOf('{')
        $header = if ($brace -ge 0) { $full.Substring(0, $brace + 1) } else { $full }
        $containers = @($node.Ancestors() | Where-Object {
            $_.GetType().Name -match '^(?:Class|Struct|Interface|Record|RecordStruct)DeclarationSyntax$'
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

    $conditionalLines = $Content.Replace("`r`n", "`n").Split("`n")
    $conditionalStack = [Collections.Generic.List[int]]::new()
    $conditionalOrdinal = 0
    for ($lineIndex = 0; $lineIndex -lt $conditionalLines.Count; $lineIndex++) {
        if ($conditionalLines[$lineIndex] -match '^\s*#\s*if\b') {
            $conditionalStack.Add($lineIndex)
            continue
        }
        if ($conditionalLines[$lineIndex] -notmatch '^\s*#\s*endif\b' -or
            $conditionalStack.Count -eq 0) {
            continue
        }
        $startIndex = $conditionalStack[$conditionalStack.Count - 1]
        $conditionalStack.RemoveAt($conditionalStack.Count - 1)
        $conditionalOrdinal++
        $records.Add([pscustomobject]@{
            Key = "conditional:$conditionalOrdinal"
            Text = ($conditionalLines[$startIndex..$lineIndex] -join "`n")
            Kind = 'conditional'
        })
    }
    if ($conditionalStack.Count -gt 0) {
        throw "Product fix source '$Path' has an unterminated conditional-compilation region."
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
        $structuralSegments = [Collections.Generic.List[string]]::new()
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
            $structuralSegments.Insert(0, "$($cursor.Name)[$ordinal]")
            $cursor = $cursor.ParentNode
        }
        $attributes = @($element.Attributes | Where-Object {
            $_.NamespaceURI -cne 'http://www.w3.org/2000/xmlns/'
        } | ForEach-Object {
            "$($_.Name)=`"$($_.Value)`""
        } | Sort-Object -CaseSensitive)
        $namespaceMappings = @($element.Attributes | Where-Object {
            $_.NamespaceURI -ceq 'http://www.w3.org/2000/xmlns/'
        } | ForEach-Object {
            "$($_.Name)=$($_.Value)"
        } | Sort-Object -CaseSensitive)
        $records.Add([pscustomobject]@{
            Key = $segments -join '/'
            StructuralKey = $structuralSegments -join '/'
            Header = "<$($element.Name) $($attributes -join ' ')>"
            NamespaceMappings = $namespaceMappings -join "`n"
            DirectText = (@($element.ChildNodes | Where-Object {
                $_.NodeType -in @([Xml.XmlNodeType]::Text, [Xml.XmlNodeType]::CDATA)
            } | ForEach-Object { $_.Value }) -join '')
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
    if ($BeforeContent.StartsWith([char]0xFEFF)) {
        $BeforeContent = $BeforeContent.Substring(1)
    }
    if ($AfterContent.StartsWith([char]0xFEFF)) {
        $AfterContent = $AfterContent.Substring(1)
    }
    if ($BeforeContent -ceq $AfterContent) {
        throw "Product fix source '$Path' is unchanged."
    }
    # A guard, helper call, alias, field, property, or XAML visibility edit can
    # activate a dangerous sink in an otherwise unchanged member/subtree. The
    # complete resulting file is therefore the security boundary. This is
    # intentionally conservative: files that already contain prohibited
    # capabilities are outside model-authored fix scope.
    Assert-ReplicationProductFixSafety -Content $AfterContent -Path $Path

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
        $beforeStructuralRecords = @{}
        foreach ($record in @($beforeRecords.Values)) {
            $beforeStructuralRecords[$record.StructuralKey] = $record
        }
        foreach ($record in @($afterRecords.Values)) {
            if ($beforeStructuralRecords.ContainsKey($record.StructuralKey) -and
                $record.NamespaceMappings -cne
                    $beforeStructuralRecords[$record.StructuralKey].NamespaceMappings) {
                throw "Product fix source '$Path' changes an XAML namespace mapping, which the fail-closed safety scan does not permit."
            }
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
        foreach ($key in $changedKeys) {
            $record = $afterRecords[$key]
            if (-not $beforeRecords.ContainsKey($key)) {
                if (-not [string]::IsNullOrWhiteSpace($record.NamespaceMappings)) {
                    throw "Product fix source '$Path' adds an XAML namespace mapping, which the fail-closed safety scan does not permit."
                }
                Assert-ReplicationProductFixSafety -Content $record.Header -Path $Path
                if (-not [string]::IsNullOrWhiteSpace($record.DirectText)) {
                    Assert-ReplicationProductFixSafety -Content $record.DirectText -Path $Path
                }
            } elseif ($record.NamespaceMappings -cne
                $beforeRecords[$record.Key].NamespaceMappings) {
                throw "Product fix source '$Path' changes an XAML namespace mapping, which the fail-closed safety scan does not permit."
            } elseif ($record.Header -cne $beforeRecords[$record.Key].Header) {
                Assert-ReplicationProductFixSafety -Content $record.Header -Path $Path
            }
            if ($beforeRecords.ContainsKey($key) -and
                $record.DirectText -cne $beforeRecords[$record.Key].DirectText -and
                -not [string]::IsNullOrWhiteSpace($record.DirectText)) {
                Assert-ReplicationProductFixSafety -Content $record.DirectText -Path $Path
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

    $directoryPath = $normalized.Substring(0, $normalized.LastIndexOf('/') + 1)
    if ($directoryPath -match '(?i)(?:^|/)[^/]*(?:BindingSourceGen|SourceGen|SourceGenerator|Generator|Analyzer|CodeFix|Build[._-]?Tasks?|BuildTargets?|Targets?|(?:Core|Xaml)\.Design|Resizetizer|Tooling|Tools|Provisioning|Workloads?|Packaging|Packs?)[^/]*(?:/|$)' -or
        $normalized -match '(?i)^src/SingleProject/Resizetizer/src/' -or
        [IO.Path]::GetFileName($normalized) -match '(?i)(?:Generator|Analyzer|CodeFixProvider)\.cs$') {
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
        if (-not (Test-Path -LiteralPath (Join-Path $root 'src') -PathType Container)) {
            return 'cannot be checked because the repository source inventory is unavailable'
        }
        $buildExecutedPaths = Get-ReplicationBuildExecutedSourcePaths -RepositoryRoot $root
        if ($buildExecutedPaths.Contains($normalized)) {
            return 'is linked into a source generator, analyzer, or build task'
        }
    }

    return $null
}

function Get-ReplicationBuildExecutedSourcePaths {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)

    $root = [IO.Path]::GetFullPath($RepositoryRoot)
    if ($script:ReplicationBuildExecutedPathCache.ContainsKey($root)) {
        return ,$script:ReplicationBuildExecutedPathCache[$root]
    }

    $paths = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($project in @(Get-ChildItem -LiteralPath (Join-Path $root 'src') `
            -Filter '*.csproj' -File -Recurse -ErrorAction Stop)) {
        if ($project.Length -gt 1MB -or
            $project.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Build-executed source inventory found an unsafe project: $($project.FullName)"
        }
        $projectText = [IO.File]::ReadAllText($project.FullName)
        $isBuildExecuted = (
            $projectText -match '(?i)<IsRoslynComponent>\s*true\s*</IsRoslynComponent>' -or
            $projectText -match '(?i)<OutputItemType>\s*Analyzer\s*</OutputItemType>' -or
            $project.BaseName -match '(?i)(?:SourceGen|Generator|Analyzer|Build[._-]?Tasks?|CodeFix)'
        )
        if (-not $isBuildExecuted) { continue }

        $settings = [Xml.XmlReaderSettings]::new()
        $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $document = [Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $reader = [Xml.XmlReader]::Create([IO.StringReader]::new($projectText), $settings)
        try { $document.Load($reader) } finally { $reader.Dispose() }
        foreach ($compile in @($document.SelectNodes('//*[local-name()="Compile"][@Include]'))) {
            $include = [string]$compile.GetAttribute('Include')
            if ([string]::IsNullOrWhiteSpace($include) -or
                $include.Contains('$(') -or
                $include.IndexOfAny([char[]]'*?') -ge 0) {
                continue
            }
            $include = $include.Replace(
                '\',
                [IO.Path]::DirectorySeparatorChar)
            $sourcePath = [IO.Path]::GetFullPath(
                (Join-Path $project.DirectoryName $include))
            if ($sourcePath.StartsWith(
                    $root.TrimEnd([IO.Path]::DirectorySeparatorChar) +
                        [IO.Path]::DirectorySeparatorChar,
                    [StringComparison]::Ordinal)) {
                [void]$paths.Add(
                    [IO.Path]::GetRelativePath($root, $sourcePath).Replace('\', '/'))
            }
        }
    }
    $script:ReplicationBuildExecutedPathCache[$root] = $paths
    return ,$paths
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

function Assert-ReplicationGeneratedTestXamlSafety {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if ([IO.Path]::GetExtension($Path) -ine '.xaml') { return }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $document = [Xml.XmlDocument]::new()
    $document.XmlResolver = $null
    $reader = [Xml.XmlReader]::Create([IO.StringReader]::new($Content), $settings)
    try { $document.Load($reader) } finally { $reader.Dispose() }

    $xamlNamespaces = @(
        'http://schemas.microsoft.com/winfx/2009/xaml',
        'http://schemas.microsoft.com/winfx/2006/xaml'
    )
    $knownSchemaNamespaces = @(
        'http://schemas.microsoft.com/dotnet/2021/maui',
        'http://schemas.microsoft.com/winfx/2009/xaml',
        'http://schemas.microsoft.com/winfx/2006/xaml',
        'http://schemas.microsoft.com/winfx/2006/xaml/presentation',
        'http://schemas.microsoft.com/netfx/2007/xaml/presentation',
        'http://schemas.openxmlformats.org/markup-compatibility/2006',
        'http://schemas.microsoft.com/expression/blend/2008'
    )
    foreach ($element in @($document.SelectNodes('//*'))) {
        foreach ($attribute in @($element.Attributes)) {
            if ($attribute.NamespaceURI -in $xamlNamespaces -and
                $attribute.LocalName -in @('FactoryMethod', 'Arguments', 'Type', 'Static')) {
                throw "Candidate test XAML '$Path' uses prohibited x:$($attribute.LocalName) execution."
            }
            if ($attribute.Value -match '(?i)\{\s*[A-Za-z_]\w*:(?:Static|Type)\b') {
                throw "Candidate test XAML '$Path' uses a prohibited executable XAML markup extension."
            }
            if ($attribute.NamespaceURI -ceq 'http://www.w3.org/2000/xmlns/') {
                $mapping = [string]$attribute.Value
                if ($mapping -in $knownSchemaNamespaces) { continue }
                if ($mapping -match '(?i)^(?:clr-namespace:|using:)(?<namespace>[^;]+)(?:;assembly=(?<assembly>.+))?$') {
                    $namespace = $Matches['namespace']
                    $assembly = $Matches['assembly']
                    if ($namespace -notmatch '^(?:Microsoft\.Maui|Maui\.)' -or
                        ($assembly -and $assembly -notmatch '^(?:Microsoft\.Maui|Controls\.Xaml\.UnitTests)')) {
                        throw "Candidate test XAML '$Path' maps an untrusted CLR namespace or assembly."
                    }
                } else {
                    throw "Candidate test XAML '$Path' maps an untrusted CLR namespace or assembly."
                }
            }
        }
        if ($element.NamespaceURI -in $xamlNamespaces -and
            $element.LocalName -in @('FactoryMethod', 'Arguments', 'Type', 'Static')) {
            throw "Candidate test XAML '$Path' uses prohibited x:$($element.LocalName) execution."
        }
        if ($element.LocalName -match '(?i)^(?:ObjectDataProvider|Process|ToolTask|Exec)$') {
            throw "Candidate test XAML '$Path' constructs a prohibited executable type."
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
        # Apple-suffixed names compile into both iOS and Mac Catalyst. Only a
        # single-platform filename is sufficient by itself; dual-platform
        # names still require a compile directive around each test.
        if ($fileScope.Count -le 1) {
            return
        }
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
            Where-Object {
                $_ -ne $Platform -and
                ($null -eq $fileScope -or $fileScope -contains $_) -and
                $maps[$_][$index]
            })
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

    $source = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $token = "Issue$Issue"
    # [Category("Issue37275")] or [Category(TestCategory.Entry, "Issue37275")]
    $pattern = '(?m)^\s*\[\s*(?:(?:[A-Za-z_]\w*)\.)*Category\s*\([^)]*"' +
        [regex]::Escape($token) + '"'
    return [bool]([regex]::IsMatch($source, $pattern))
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

        Windows ordinary review tests are exempt because their runner filters by
        discovered traits. Replication still requires the issue category alone:
        app-authored category text crosses the AppContainer boundary through a
        trusted result-file path, so no second value is accepted.

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
    if ($Platform -notin @('android', 'ios', 'catalyst', 'windows')) { return '' }

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

function Get-ReplicationControlSemanticReferences {
    if (Get-Variable -Name ReplicationControlSemanticReferences `
            -Scope Script -ErrorAction SilentlyContinue) {
        return @($script:ReplicationControlSemanticReferences)
    }

    $references = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
    $trustedAssemblies = [string][AppContext]::GetData(
        'TRUSTED_PLATFORM_ASSEMBLIES')
    foreach ($assemblyPath in @($trustedAssemblies -split
            [IO.Path]::PathSeparator | Where-Object { $_ })) {
        $references.Add(
            [Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
                $assemblyPath))
    }

    # Bind controls against a closed metadata contract. Source-defined aliases
    # or types then resolve to source symbols and cannot impersonate MAUI APIs.
    $contractSource = @'
namespace Microsoft.Maui.Controls
{
    public class BindableProperty { }
    public class ResourceDictionary
    {
        public object this[string key]
        {
            get => null;
            set { }
        }
    }
    public class Brush { }
    public class SolidColorBrush : Brush
    {
        public SolidColorBrush(global::Microsoft.Maui.Graphics.Color color) { }
    }
    public class BindableObject
    {
        public void SetDynamicResource(params object[] arguments) { }
    }
    public class Element : BindableObject
    {
        public global::Microsoft.Maui.IElementHandler Handler { get; set; }
        public ResourceDictionary Resources { get; } = new ResourceDictionary();
    }
    public class VisualElement : Element
    {
        public object Background { get; set; }
        public static BindableProperty BackgroundProperty { get; }
        public bool IsVisible { get; set; }
    }
    public class View : VisualElement { }
    public class Page : VisualElement
    {
        public event System.EventHandler NavigatedTo;
    }
    public class ContentPage : Page { }
    public class Label : View
    {
        public static BindableProperty BackgroundColorProperty { get; }
        public static BindableProperty TextColorProperty { get; }
        public int MaxLines { get; set; }
        public string Text { get; set; }
    }

    public class NavigationPage : Page
    {
        public NavigationPage(Page root) { }
        public INavigation Navigation { get; }
        public Page CurrentPage { get; }
        public System.Threading.Tasks.Task PushAsync(Page page) =>
            System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task<Page> PopAsync() =>
            System.Threading.Tasks.Task.FromResult<Page>(null);
        public static void SetTitleView(params object[] arguments) { }
        public static void SetBackButtonTitle(BindableObject page, string value) { }
        public static void SetHasBackButton(params object[] arguments) { }
        public static void SetHasNavigationBar(params object[] arguments) { }
        public static void SetIconColor(params object[] arguments) { }
        public static void SetTitleIconImageSource(params object[] arguments) { }
    }

    public class Shell : Page
    {
        public static void SetBackButtonBehavior(params object[] arguments) { }
        public static void SetBackgroundColor(params object[] arguments) { }
        public static void SetFlyoutBehavior(params object[] arguments) { }
        public static void SetFlyoutItemIsVisible(params object[] arguments) { }
        public static void SetFlyoutWidth(params object[] arguments) { }
        public static void SetForegroundColor(params object[] arguments) { }
        public static void SetNavBarHasShadow(params object[] arguments) { }
        public static void SetNavBarIsVisible(params object[] arguments) { }
        public static void SetPresentationMode(params object[] arguments) { }
        public static void SetSearchHandler(params object[] arguments) { }
        public static void SetTabBarBackgroundColor(params object[] arguments) { }
        public static void SetTabBarDisabledColor(params object[] arguments) { }
        public static void SetTabBarForegroundColor(params object[] arguments) { }
        public static void SetTabBarIsVisible(params object[] arguments) { }
        public static void SetTabBarTitleColor(params object[] arguments) { }
        public static void SetTitleColor(params object[] arguments) { }
    }

    public static class VisualStateManager
    {
        public static bool GoToState(params object[] arguments) => false;
    }

    public static class AutomationProperties
    {
        public static void SetExcludedWithChildren(params object[] arguments) { }
        public static void SetHeadingLevel(params object[] arguments) { }
        public static void SetHelpText(params object[] arguments) { }
        public static void SetIsInAccessibleTree(params object[] arguments) { }
        public static void SetLabeledBy(params object[] arguments) { }
        public static void SetName(params object[] arguments) { }
        public static void SetPositionInSet(params object[] arguments) { }
        public static void SetSizeOfSet(params object[] arguments) { }
    }

    public static class SemanticProperties
    {
        public static void SetDescription(params object[] arguments) { }
        public static void SetHeadingLevel(params object[] arguments) { }
        public static void SetHint(params object[] arguments) { }
    }

    public static class BindableLayout
    {
        public static void SetEmptyView(params object[] arguments) { }
        public static void SetEmptyViewTemplate(params object[] arguments) { }
        public static void SetItemTemplate(params object[] arguments) { }
        public static void SetItemTemplateSelector(params object[] arguments) { }
        public static void SetItemsSource(params object[] arguments) { }
    }

    public class Grid : View
    {
        public static void SetColumn(params object[] arguments) { }
        public static void SetColumnSpan(params object[] arguments) { }
        public static void SetRow(params object[] arguments) { }
        public static void SetRowSpan(params object[] arguments) { }
    }

    public class FlexLayout : View
    {
        public static void SetAlignSelf(params object[] arguments) { }
        public static void SetBasis(params object[] arguments) { }
        public static void SetGrow(params object[] arguments) { }
        public static void SetOrder(params object[] arguments) { }
        public static void SetShrink(params object[] arguments) { }
    }

    public class AbsoluteLayout : View
    {
        public static void SetLayoutBounds(params object[] arguments) { }
        public static void SetLayoutFlags(params object[] arguments) { }
    }
    public interface INavigation
    {
        System.Threading.Tasks.Task PushAsync(Page page);
        System.Threading.Tasks.Task<Page> PopAsync();
    }
    public class Layout : View { }
    public class Toolbar : View { }
    public class Window : Element, global::Microsoft.Maui.IElement
    {
        public Window(Page page) { }
    }
}

namespace Microsoft.Maui
{
    public sealed class CategoryAttribute : System.Attribute
    {
        public CategoryAttribute(string value) { }
    }

    public interface IElement { }

    public interface IElementHandler
    {
        object PlatformView { get; }
        IMauiContext MauiContext { get; }
    }

    public interface IMauiContext { }
}

namespace Microsoft.Maui.Graphics
{
    public struct Color { }
    public static class Colors
    {
        public static Color Red { get; }
        public static Color Transparent { get; }
    }
}

namespace Microsoft.Maui.Controls.Handlers
{
    public class LabelHandler { }
    public class LayoutHandler { }
    public class PageHandler { }
    public class ToolbarHandler { }
}

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
    public class NavigationRenderer : UIKit.UINavigationController,
        global::Microsoft.Maui.IElementHandler
    {
        public object PlatformView { get; }
        public global::Microsoft.Maui.IMauiContext MauiContext { get; }
    }
}

namespace Microsoft.Maui.DeviceTests.Stubs
{
    public class WindowHandlerStub : global::Microsoft.Maui.IElementHandler
    {
        public object PlatformView { get; }
        public global::Microsoft.Maui.IMauiContext MauiContext { get; }
    }
}

namespace Microsoft.Maui.Hosting
{
    public class HandlerCollection
    {
        public void AddHandler<TView, THandler>() { }
    }
    public class HandlerBuilder
    {
        public void ConfigureMauiHandlers(
            System.Action<HandlerCollection> configure) { }
    }
}

namespace Microsoft.Maui.DeviceTests
{
    public static class AssertionExtensions
    {
        public static UIKit.UIView GetBackButton(
            this UIKit.UINavigationBar navigationBar) => default;
    }

    public class ControlsHandlerTestBase
    {
        protected void EnsureHandlerCreated(
            System.Action<global::Microsoft.Maui.Hosting.HandlerBuilder> configure) { }
        protected System.Threading.Tasks.Task CreateHandlerAndAddToWindow<THandler>(
            global::Microsoft.Maui.IElement view,
            System.Func<THandler, System.Threading.Tasks.Task> action)
            where THandler : class, global::Microsoft.Maui.IElementHandler
            => System.Threading.Tasks.Task.CompletedTask;
    }
}

namespace CoreGraphics
{
    public struct CGSize
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }
    public struct CGRect
    {
        public static CGRect Empty { get; }
        public double Width { get; set; }
        public double Height { get; set; }
        public static CGRect Intersect(CGRect first, CGRect second) => default;
    }
}

namespace UIKit
{
    public class UIView
    {
        public double Alpha { get; set; }
        public CoreGraphics.CGRect Bounds { get; set; }
        public bool Hidden { get; set; }
        public CoreGraphics.CGSize IntrinsicContentSize { get; }
        public UIView[] Subviews { get; }
        public UIView Window { get; }
        public CoreGraphics.CGRect ConvertRectToView(
            CoreGraphics.CGRect rect,
            UIView view) => default;
    }
    public class UILabel : UIView
    {
        public string Text { get; set; }
    }
    public class UIButton : UIView { }
    public class UIBarButtonItem
    {
        public string Title { get; set; }
    }
    public class UINavigationItem
    {
        public UIBarButtonItem BackBarButtonItem { get; }
    }
    public class UIViewController
    {
        public UINavigationItem NavigationItem { get; }
    }
    public class UINavigationBar : UIView
    {
        public void LayoutIfNeeded() { }
    }
    public class UINavigationController
    {
        public UINavigationBar NavigationBar { get; }
        public UIViewController[] ViewControllers { get; }
    }
}

namespace Microsoft.Maui.Platform
{
    public static class ViewExtensions
    {
        public static T FindDescendantView<T>(this UIKit.UIView view)
            where T : UIKit.UIView => default;
        public static T FindDescendantView<T>(
            this UIKit.UIView view,
            System.Func<UIKit.UIView, bool> predicate)
            where T : UIKit.UIView => default;
    }
    public static class ElementExtensions
    {
        public static UIKit.UIView ToPlatform(
            this global::Microsoft.Maui.Controls.Element element) => default;
        public static UIKit.UIView ToPlatform(
            this global::Microsoft.Maui.Controls.Element element,
            global::Microsoft.Maui.IMauiContext context) => default;
    }
}

namespace Xunit
{
    public sealed class FactAttribute : System.Attribute { }
    public sealed class TheoryAttribute : System.Attribute { }
    public static class Assert
    {
        public static void Contains(params object[] arguments) { }
        public static void DoesNotContain(params object[] arguments) { }
        public static void Empty(params object[] arguments) { }
        public static void EndsWith(params object[] arguments) { }
        public static void Equal(params object[] arguments) { }
        public static void Fail(params object[] arguments) { }
        public static void False(params object[] arguments) { }
        public static void NotEmpty(params object[] arguments) { }
        public static void NotEqual(params object[] arguments) { }
        public static void NotNull(params object[] arguments) { }
        public static void NotSame(params object[] arguments) { }
        public static void Null(params object[] arguments) { }
        public static void Same(params object[] arguments) { }
        public static void Single(params object[] arguments) { }
        public static void StartsWith(params object[] arguments) { }
        public static void True(params object[] arguments) { }
        public static T IsAssignableFrom<T>(params object[] arguments) => default;
        public static T IsType<T>(params object[] arguments) => default;
        public static T Throws<T>(params object[] arguments) => default;
        public static System.Threading.Tasks.Task<T> ThrowsAsync<T>(
            params object[] arguments) => default;
    }
}

namespace Xunit.Sdk
{
    public sealed class XunitException : System.Exception
    {
        public XunitException(string message) : base(message) { }
    }
}

namespace NUnit.Framework
{
    public sealed class TestAttribute : System.Attribute { }
    public class Constraint
    {
        public ConstraintExpression And => new ConstraintExpression();
    }
    public class ConstraintExpression
    {
        public ConstraintExpression And => this;
        public Constraint Null => new Constraint();
        public Constraint True => new Constraint();
        public Constraint False => new Constraint();
        public Constraint Empty => new Constraint();
        public ConstraintExpression Not => this;
        public Constraint EqualTo(params object[] arguments) => new Constraint();
        public Constraint GreaterThan(params object[] arguments) => new Constraint();
        public Constraint LessThan(params object[] arguments) => new Constraint();
        public Constraint SameAs(params object[] arguments) => new Constraint();
    }
    public static class Is
    {
        public static Constraint Null => new Constraint();
        public static Constraint True => new Constraint();
        public static Constraint False => new Constraint();
        public static Constraint Empty => new Constraint();
        public static ConstraintExpression Not => new ConstraintExpression();
        public static Constraint EqualTo(params object[] arguments)
            => new Constraint();
        public static Constraint GreaterThan(params object[] arguments)
            => new Constraint();
        public static Constraint LessThan(params object[] arguments)
            => new Constraint();
        public static Constraint SameAs(params object[] arguments)
            => new Constraint();
    }
    public static class Assert
    {
        public static void Fail(params object[] arguments) { }
        public static void Multiple(params object[] arguments) { }
        public static void That(params object[] arguments) { }
    }
    public static class ClassicAssert
    {
        public static void AreEqual(params object[] arguments) { }
        public static void AreNotEqual(params object[] arguments) { }
        public static void IsFalse(params object[] arguments) { }
        public static void IsNotNull(params object[] arguments) { }
        public static void IsNull(params object[] arguments) { }
        public static void IsTrue(params object[] arguments) { }
    }
}

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public sealed class TestMethodAttribute : System.Attribute { }
    public static class Assert
    {
        public static void AreEqual(params object[] arguments) { }
        public static void AreNotEqual(params object[] arguments) { }
        public static void Fail(params object[] arguments) { }
        public static void IsFalse(params object[] arguments) { }
        public static void IsNotNull(params object[] arguments) { }
        public static void IsNull(params object[] arguments) { }
        public static void IsTrue(params object[] arguments) { }
    }
    public static class CollectionAssert
    {
        public static void AreEqual(params object[] arguments) { }
        public static void AreNotEqual(params object[] arguments) { }
        public static void Contains(params object[] arguments) { }
        public static void DoesNotContain(params object[] arguments) { }
    }
    public static class StringAssert
    {
        public static void Contains(params object[] arguments) { }
        public static void DoesNotMatch(params object[] arguments) { }
        public static void EndsWith(params object[] arguments) { }
        public static void Matches(params object[] arguments) { }
        public static void StartsWith(params object[] arguments) { }
    }
}
'@
    $contractTree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        $contractSource)
    $contractOptions =
        [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
            [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
    $contractCompilation =
        [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
            'Microsoft.Maui.Controls.ReplicationControlContract',
            [Microsoft.CodeAnalysis.SyntaxTree[]]@($contractTree),
            [Microsoft.CodeAnalysis.MetadataReference[]]$references.ToArray(),
            $contractOptions)
    $contractStream = [IO.MemoryStream]::new()
    try {
        $emitResult = $contractCompilation.Emit($contractStream)
        if (-not $emitResult.Success) {
            $details = @($emitResult.Diagnostics | Where-Object {
                    [string]$_.Severity -ceq 'Error'
                } | Select-Object -First 4 | ForEach-Object ToString) -join '; '
            throw "The trusted control semantic contract could not compile: $details"
        }
        $contractStream.Position = 0
        $references.Add(
            [Microsoft.CodeAnalysis.MetadataReference]::CreateFromStream(
                $contractStream))
    }
    finally {
        $contractStream.Dispose()
    }

    $script:ReplicationControlSemanticReferences = $references.ToArray()
    return @($script:ReplicationControlSemanticReferences)
}

function Read-ReplicationControlResult {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [datetime]$MinimumWriteTimeUtc,
        [Parameter(Mandatory = $true)][int]$ExpectedIssueNumber,
        [Parameter(Mandatory = $true)][string]$ExpectedPlatform,
        [Parameter(Mandatory = $true)][string]$ExpectedTestType,
        [Parameter(Mandatory = $true)][string]$ExpectedTestFilter,
        [Parameter(Mandatory = $true)][string]$ExpectedTestClass,
        [Parameter(Mandatory = $true)][string]$ExpectedTestMethod,
        [Parameter(Mandatory = $true)][int]$ExpectedRunCount,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Passed', 'PreExecutionFailure')]
        [string]$ExpectedOutcome
    )

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.PSIsContainer -or
        $item.Length -le 0 -or
        $item.Length -gt 64KB -or
        ($PSBoundParameters.ContainsKey('MinimumWriteTimeUtc') -and
            $item.LastWriteTimeUtc -lt $MinimumWriteTimeUtc)) {
        throw 'The control result is not a fresh bounded regular file.'
    }
    $json = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop
    $document = [Text.Json.JsonDocument]::Parse($json)
    try {
        if ($document.RootElement.ValueKind -ne
            [Text.Json.JsonValueKind]::Object) {
            throw 'The control result root is not an object.'
        }
        $expectedKinds = [ordered]@{
            schemaVersion = [Text.Json.JsonValueKind]::Number
            issueNumber = [Text.Json.JsonValueKind]::Number
            platform = [Text.Json.JsonValueKind]::String
            testType = [Text.Json.JsonValueKind]::String
            testFilter = [Text.Json.JsonValueKind]::String
            testClass = [Text.Json.JsonValueKind]::String
            testMethod = [Text.Json.JsonValueKind]::String
            requestedRunCount = [Text.Json.JsonValueKind]::Number
            runCount = [Text.Json.JsonValueKind]::Number
            executedCount = [Text.Json.JsonValueKind]::Number
            passCount = [Text.Json.JsonValueKind]::Number
            infrastructureFailure = $null
            observedFailureMessages = [Text.Json.JsonValueKind]::Array
            reproductionFailureMessages = [Text.Json.JsonValueKind]::Array
            failureModeChanged = $null
            logFiles = [Text.Json.JsonValueKind]::Array
        }
        $seen = [Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
        foreach ($property in $document.RootElement.EnumerateObject()) {
            $expectedKind = if ($property.Name -cin @(
                    'infrastructureFailure',
                    'failureModeChanged')) {
                if ($property.Value.ValueKind -notin @(
                        [Text.Json.JsonValueKind]::True,
                        [Text.Json.JsonValueKind]::False)) {
                    throw 'The control result has duplicate, unknown, or mistyped fields.'
                }
                $property.Value.ValueKind
            } elseif ($property.Name -cin @($expectedKinds.Keys)) {
                $expectedKinds[$property.Name]
            } else {
                $null
            }
            if (-not $seen.Add($property.Name) -or
                $property.Name -cnotin @($expectedKinds.Keys) -or
                $null -eq $expectedKind -or
                $property.Value.ValueKind -ne $expectedKind) {
                throw 'The control result has duplicate, unknown, or mistyped fields.'
            }
            if ($property.Value.ValueKind -eq
                [Text.Json.JsonValueKind]::Number) {
                $integerValue = 0
                if (-not $property.Value.TryGetInt32([ref]$integerValue)) {
                    throw 'The control result numeric fields must be 32-bit integers.'
                }
            }
            if ($property.Value.ValueKind -eq
                    [Text.Json.JsonValueKind]::String -and
                $property.Value.GetString().Length -gt 4096) {
                throw 'The control result contains an oversized string.'
            }
            if ($property.Value.ValueKind -eq
                [Text.Json.JsonValueKind]::Array) {
                $items = @($property.Value.EnumerateArray())
                if ($items.Count -gt 100 -or
                    @($items | Where-Object {
                            $_.ValueKind -ne [Text.Json.JsonValueKind]::String -or
                            $_.GetString().Length -gt 4096
                        }).Count -ne 0) {
                    throw 'The control result contains an invalid bounded string array.'
                }
            }
        }
        if ($seen.Count -ne $expectedKinds.Count) {
            throw 'The control result is missing required fields.'
        }
    }
    finally {
        $document.Dispose()
    }

    $result = $json | ConvertFrom-Json -AsHashtable -Depth 10 -ErrorAction Stop
    if ([int]$result.schemaVersion -ne 1 -or
        [int]$result.issueNumber -ne $ExpectedIssueNumber -or
        [string]$result.platform -cne $ExpectedPlatform -or
        [string]$result.testType -cne $ExpectedTestType -or
        [string]$result.testFilter -cne $ExpectedTestFilter -or
        [string]$result.testClass -cne $ExpectedTestClass -or
        [string]$result.testMethod -cne $ExpectedTestMethod -or
        [int]$result.requestedRunCount -ne $ExpectedRunCount -or
        [int]$result.runCount -ne $ExpectedRunCount) {
        throw 'The control result identity does not match the trusted run.'
    }

    if ($ExpectedOutcome -ceq 'Passed') {
        if ([int]$result.executedCount -ne $ExpectedRunCount -or
            [int]$result.passCount -ne $ExpectedRunCount -or
            $result.infrastructureFailure -ne $false -or
            $result.failureModeChanged -ne $false -or
            @($result.observedFailureMessages).Count -ne 0) {
            throw 'The control result does not prove every trusted control run passed.'
        }
    } elseif ([int]$result.executedCount -ne 0 -or
        [int]$result.passCount -ne 0 -or
        $result.infrastructureFailure -ne $true -or
        $result.failureModeChanged -ne $false) {
        throw 'The control result does not prove a fresh zero-execution failure.'
    }

    return $result
}

function Confirm-ReplicationTrustedOracleExpression {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression,
        [Parameter(Mandatory = $true)]
        [Microsoft.CodeAnalysis.SemanticModel]$SemanticModel,
        [Parameter(Mandatory = $true)]
        [Microsoft.CodeAnalysis.SyntaxNode]$Root,
        [Collections.Generic.HashSet[string]]$VisitedLocals
    )

    if ($null -eq $VisitedLocals) {
        $VisitedLocals = [Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
    }
    $nodes = @($Expression.DescendantNodesAndSelf())
    if (@($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax] -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseExpressionSyntax]
            }).Count -ne 0) {
        throw (
            'Trusted assertion dataflow may not use this or base because virtual ' +
            'dispatch could execute generated overrides.')
    }
    if (@($nodes | Where-Object {
                ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                    $_.Parent -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax]) -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax] -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.QueryExpressionSyntax] -or
                ($_.RawKind -in @(
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression))
            }).Count -ne 0) {
        throw (
            'Trusted assertion arguments may not contain writes, increments, ' +
            'lambdas, or query expressions.')
    }
    foreach ($argument in @($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax]
            })) {
        if ($argument.RefKindKeyword.RawKind -ne 0) {
            throw 'Trusted assertion arguments may not use ref, out, or in.'
        }
    }
    foreach ($expressionNode in @($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
            })) {
        $conversion = $SemanticModel.GetConversion($expressionNode)
        if ($conversion.IsUserDefined) {
            throw 'Trusted assertion arguments may not execute user-defined conversions.'
        }
    }
    foreach ($call in @($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
            })) {
        $callSymbol = $SemanticModel.GetSymbolInfo($call).Symbol
        if ($callSymbol -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
            @($callSymbol.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            throw (
                'Trusted assertion arguments may call only resolved external ' +
                'metadata methods, never generated helpers.')
        }
        $callType = $callSymbol.ContainingType.ToString()
        $allowedCall = (
            $callType -cin @(
                'NUnit.Framework.Is',
                'NUnit.Framework.ConstraintExpression',
                'System.Math'
            ) -or
            ($callType -ceq 'System.Linq.Enumerable' -and
                $callSymbol.Name -cin @(
                    'All',
                    'Any',
                    'Cast',
                    'Count',
                    'ElementAt',
                    'First',
                    'FirstOrDefault',
                    'OfType',
                    'SequenceEqual',
                    'Single',
                    'SingleOrDefault',
                    'Skip',
                    'Take',
                    'ToArray',
                    'ToList'
                ))
        )
        if (-not $allowedCall) {
            throw (
                "Trusted assertion argument call '$callType.$(
                    $callSymbol.Name)' is not an allowlisted pure observation.")
        }
    }
    foreach ($creation in @($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax] -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitObjectCreationExpressionSyntax]
            })) {
        $constructor = $SemanticModel.GetSymbolInfo($creation).Symbol
        if ($constructor -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
            @($constructor.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0 -or
            $constructor.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract') {
            throw (
                'Trusted assertion dataflow may construct only explicitly ' +
                'allowlisted deterministic contract types.')
        }
    }
    foreach ($member in @($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $_.Parent -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
            })) {
        $memberSymbol = $SemanticModel.GetSymbolInfo($member).Symbol
        if ($memberSymbol -is [Microsoft.CodeAnalysis.INamespaceSymbol]) {
            continue
        }
        if ($null -eq $memberSymbol -or
            @($memberSymbol.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0 -or
            $memberSymbol -is [Microsoft.CodeAnalysis.IEventSymbol]) {
            throw (
                'Trusted assertion member reads must resolve to external metadata ' +
                'properties or fields, never generated getters or events.')
        }
        if ($memberSymbol -is [Microsoft.CodeAnalysis.INamedTypeSymbol]) {
            continue
        }
        if ($null -eq $memberSymbol.ContainingType -or
            $null -eq $memberSymbol.ContainingAssembly) {
            throw (
                'Trusted assertion member reads must have a resolved containing ' +
                'type and assembly.')
        }
        $memberType = $memberSymbol.ContainingType.ToString()
        $memberAssembly = $memberSymbol.ContainingAssembly.Name
        $allowedMemberRead = (
            ($memberAssembly -ceq
                'Microsoft.Maui.Controls.ReplicationControlContract' -and
                ($memberType.StartsWith(
                        'Microsoft.Maui.Controls.',
                        [StringComparison]::Ordinal) -or
                    $memberType.StartsWith(
                        'Microsoft.Maui.Graphics.',
                        [StringComparison]::Ordinal) -or
                    $memberType.StartsWith(
                        'NUnit.Framework.',
                        [StringComparison]::Ordinal))) -or
            $memberType -ceq 'string' -or
            $memberType -ceq 'System.Array' -or
            $memberType -cmatch '^System\.Collections\.Generic\.(?:List|HashSet)<'
        )
        if (-not $allowedMemberRead) {
            throw (
                "Trusted assertion member '$memberType.$(
                    $memberSymbol.Name)' is not an allowlisted deterministic " +
                'observation.')
        }
    }
    foreach ($indexer in @($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax]
            })) {
        $indexerSymbol = $SemanticModel.GetSymbolInfo($indexer).Symbol
        if ($indexerSymbol -isnot [Microsoft.CodeAnalysis.IPropertySymbol] -or
            @($indexerSymbol.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            throw (
                'Trusted assertion indexers must resolve to external metadata, ' +
                'never generated getters.')
        }
    }
    foreach ($identifier in @($nodes | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
                -not ($_.Parent -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $_.Parent.Name -eq $_) -and
                $_.Parent -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax] -and
                $_.Parent -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax]
            })) {
        $identifierSymbol = $SemanticModel.GetSymbolInfo($identifier).Symbol
        if ($identifierSymbol -is [Microsoft.CodeAnalysis.ILocalSymbol]) {
            if ($identifierSymbol.Type.TypeKind -eq
                    [Microsoft.CodeAnalysis.TypeKind]::Error -or
                @($identifierSymbol.Type.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -ne 0) {
                throw (
                    'A local used by trusted dataflow must have an external ' +
                    'metadata type, never a generated or unresolved runtime type.')
            }
            $localKey = '{0}:{1}' -f
                $identifierSymbol.Name,
                $identifierSymbol.Locations[0].SourceSpan.Start
            if (-not $VisitedLocals.Add($localKey)) {
                continue
            }
            $declarations = @($identifierSymbol.DeclaringSyntaxReferences |
                ForEach-Object {
                    $_.GetSyntax([Threading.CancellationToken]::None)
                } |
                Where-Object {
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax]
                })
            if ($declarations.Count -ne 1 -or
                $null -eq $declarations[0].Initializer) {
                throw (
                    'A local used by the trusted assertion must have one traced ' +
                    'initializer.')
            }
            $writes = @($Root.DescendantNodes() | Where-Object {
                    if ($_ -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                        return $false
                    }
                    $candidate = $SemanticModel.GetSymbolInfo($_).Symbol
                    if ($null -eq $candidate -or
                        -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                            $candidate,
                            $identifierSymbol)) {
                        return $false
                    }
                    return (
                        ($_.Parent -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                            $_.Parent.Left -eq $_) -or
                        $_.Parent.RawKind -in @(
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression) -or
                        ($_.Parent -is [Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax] -and
                            $_.Parent.RefKindKeyword.RawKind -ne 0))
                })
            if ($writes.Count -ne 0) {
                throw 'A local used by the trusted assertion may not be reassigned.'
            }
            Confirm-ReplicationTrustedOracleExpression `
                -Expression $declarations[0].Initializer.Value `
                -SemanticModel $SemanticModel `
                -Root $Root `
                -VisitedLocals $VisitedLocals
            continue
        }
        if ($identifierSymbol -is [Microsoft.CodeAnalysis.IParameterSymbol]) {
            if ($identifierSymbol.Type.TypeKind -eq
                    [Microsoft.CodeAnalysis.TypeKind]::Error -or
                @($identifierSymbol.Type.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -ne 0) {
                throw 'Trusted assertion parameters must have external metadata types.'
            }
            continue
        }
        if ($identifierSymbol -is [Microsoft.CodeAnalysis.IPropertySymbol] -or
            $identifierSymbol -is [Microsoft.CodeAnalysis.IFieldSymbol] -or
            $identifierSymbol -is [Microsoft.CodeAnalysis.IEventSymbol]) {
            if ($identifierSymbol -is [Microsoft.CodeAnalysis.IEventSymbol]) {
                throw 'Trusted assertion dataflow may not read events.'
            }
            $identifierType = $identifierSymbol.ContainingType.ToString()
            $identifierAssembly =
                $identifierSymbol.ContainingAssembly.Name
            $allowedIdentifierRead = (
                ($identifierAssembly -ceq
                    'Microsoft.Maui.Controls.ReplicationControlContract' -and
                    ($identifierType.StartsWith(
                            'Microsoft.Maui.Controls.',
                            [StringComparison]::Ordinal) -or
                        $identifierType.StartsWith(
                            'NUnit.Framework.',
                            [StringComparison]::Ordinal))) -or
                $identifierType -ceq 'string' -or
                $identifierType -ceq 'System.Array' -or
                $identifierType -cmatch
                    '^System\.Collections\.Generic\.(?:List|HashSet)<'
            )
            if (-not $allowedIdentifierRead) {
                throw (
                    "Trusted assertion identifier '$identifierType.$(
                        $identifierSymbol.Name)' is not an allowlisted " +
                    'deterministic observation.')
            }
            continue
        }
        if ($null -eq $identifierSymbol -or
            @($identifierSymbol.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            throw (
                "Trusted assertion identifier '$identifier' must resolve to a " +
                'traced local, parameter, or external metadata symbol.')
        }
    }
}

function Get-ReplicationTrustedControlsHandlerTestBaseSource {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$GeneratedSourcePath
    )

    $root = (Resolve-Path -LiteralPath $RepositoryRoot -ErrorAction Stop).Path
    $relativePath =
        'src/Controls/tests/DeviceTests/ControlsHandlerTestBase.cs'
    $expectedBaselineBlob = '9fb3f59d29cf6429a5a3b8b392e02f9804fd6b02'
    $expectedNormalizedSha256 =
        '1310c10a6b56651cc999f3cd63d3be0a52bfb00c837e85d3ce1165385e694a14'
    $trustedPath = [IO.Path]::GetFullPath((Join-Path $root $relativePath))
    $rootPrefix = $root.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar) +
        [IO.Path]::DirectorySeparatorChar
    if (-not $trustedPath.StartsWith(
            $rootPrefix,
            [StringComparison]::Ordinal)) {
        throw 'The trusted ControlsHandlerTestBase path escapes the repository root.'
    }

    $generatedPath = if ([IO.Path]::IsPathRooted($GeneratedSourcePath)) {
        [IO.Path]::GetFullPath($GeneratedSourcePath)
    } else {
        [IO.Path]::GetFullPath((Join-Path $root $GeneratedSourcePath))
    }
    if ($generatedPath -ceq $trustedPath) {
        throw (
            'The generated test path may not replace the immutable trusted ' +
            'ControlsHandlerTestBase source.')
    }

    $trustedItem = Get-Item -LiteralPath $trustedPath -Force -ErrorAction Stop
    if (-not $trustedItem.PSIsContainer -and
        ($trustedItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0) {
        $baselineBlob = @(& git -C $root rev-parse --verify "HEAD:$relativePath" 2>&1)
        $baselineExitCode = $LASTEXITCODE
    } else {
        throw (
            'The trusted ControlsHandlerTestBase helper must be one regular, ' +
            'non-linked repository file.')
    }
    if ($baselineExitCode -ne 0 -or
        $baselineBlob.Count -ne 1 -or
        [string]$baselineBlob[0] -cne $expectedBaselineBlob) {
        throw (
            'The trusted ControlsHandlerTestBase helper does not match the ' +
            'reviewed immutable repository blob pinned by this semantic gate.')
    }

    $bytes = [IO.File]::ReadAllBytes($trustedPath)
    $normalizedBytes = [Collections.Generic.List[byte]]::new($bytes.Length)
    for ($index = 0; $index -lt $bytes.Length; $index++) {
        if ($bytes[$index] -eq 13 -and
            ($index + 1) -lt $bytes.Length -and
            $bytes[$index + 1] -eq 10) {
            continue
        }
        $normalizedBytes.Add($bytes[$index])
    }
    $hashAlgorithm = [Security.Cryptography.SHA256]::Create()
    try {
        $actualNormalizedSha256 = [Convert]::ToHexString(
            $hashAlgorithm.ComputeHash(
                [byte[]]$normalizedBytes.ToArray())).ToLowerInvariant()
    }
    finally {
        $hashAlgorithm.Dispose()
    }
    if ($actualNormalizedSha256 -cne $expectedNormalizedSha256) {
        throw (
            "Trusted ControlsHandlerTestBase helper '$relativePath' differs " +
            'from the reviewed immutable source pinned by this semantic gate.')
    }

    $stream = [IO.MemoryStream]::new($bytes, $false)
    $reader = [IO.StreamReader]::new(
        $stream,
        [Text.UTF8Encoding]::new($false, $true),
        $true)
    try {
        $source = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
        $stream.Dispose()
    }

    $helperTree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        $source,
        [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithPreprocessorSymbols(
            [string[]]@('MACCATALYST')),
        $trustedPath)
    $helperDefinitions = @($helperTree.GetRoot().DescendantNodes() |
        Where-Object {
            if ($_ -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -or
                $_.Identifier.ValueText -cne 'CreateHandlerAndAddToWindow' -or
                $_.ReturnType.ToString() -cne 'Task' -or
                $null -eq $_.TypeParameterList -or
                $_.TypeParameterList.Parameters.Count -ne 1 -or
                $_.TypeParameterList.Parameters[0].Identifier.ValueText -cne
                    'THandler' -or
                $_.ParameterList.Parameters.Count -ne 2 -or
                $_.ParameterList.Parameters[0].Identifier.ValueText -cne 'view' -or
                $_.ParameterList.Parameters[0].Type.ToString() -cne 'IElement' -or
                $_.ParameterList.Parameters[1].Identifier.ValueText -cne 'action' -or
                $_.ParameterList.Parameters[1].Type.ToString() -cne
                    'Func<THandler, Task>' -or
                $_.ConstraintClauses.Count -ne 1 -or
                $_.ConstraintClauses[0].Name.Identifier.ValueText -cne 'THandler' -or
                $_.ConstraintClauses[0].Constraints.Count -ne 2 -or
                $_.ConstraintClauses[0].Constraints[0].ToString() -cne 'class' -or
                $_.ConstraintClauses[0].Constraints[1].ToString() -cne
                    'IElementHandler' -or
                @($_.Modifiers | Where-Object {
                        $_.RawKind -eq
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::ProtectedKeyword
                    }).Count -ne 1 -or
                @($_.Modifiers | Where-Object {
                        $_.RawKind -eq
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::StaticKeyword
                    }).Count -ne 0) {
                return $false
            }
            $containingType = @($_.Ancestors() | Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax]
                } | Select-Object -First 1)
            $containingNamespace = @($_.Ancestors() | Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax]
                } | Select-Object -First 1)
            return (
                $containingType.Count -eq 1 -and
                $containingType[0].Identifier.ValueText -ceq
                    'ControlsHandlerTestBase' -and
                $containingNamespace.Count -eq 1 -and
                $containingNamespace[0].Name.ToString() -ceq
                    'Microsoft.Maui.DeviceTests')
        })
    if ($helperDefinitions.Count -ne 1) {
        throw (
            'The immutable ControlsHandlerTestBase source no longer contains ' +
            'exactly one reviewed CreateHandlerAndAddToWindow<THandler>(' +
            'IElement, Func<THandler, Task>) definition.')
    }

    return [pscustomobject]@{
        Path = $trustedPath
        DefinitionLine = $helperTree.GetLineSpan(
            $helperDefinitions[0].Span).StartLinePosition.Line + 1
    }
}

function Get-ReplicationTrustedAssertEventuallySource {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$GeneratedSourcePath
    )

    $root = (Resolve-Path -LiteralPath $RepositoryRoot -ErrorAction Stop).Path
    $relativePath = 'src/TestUtils/src/DeviceTests/AssertHelpers.cs'
    $expectedBaselineBlob = 'b2728c2b621b375c7599d3df169e9b0cac217ea7'
    $expectedNormalizedSha256 =
        '8fef848763a5185e932a050afdedd5d4d15243ebe9284073f0ecbcd5b633c439'
    $trustedPath = [IO.Path]::GetFullPath((Join-Path $root $relativePath))
    $rootPrefix = $root.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar) +
        [IO.Path]::DirectorySeparatorChar
    if (-not $trustedPath.StartsWith(
            $rootPrefix,
            [StringComparison]::Ordinal)) {
        throw 'The trusted AssertEventually helper path escapes the repository root.'
    }

    $generatedPath = if ([IO.Path]::IsPathRooted($GeneratedSourcePath)) {
        [IO.Path]::GetFullPath($GeneratedSourcePath)
    } else {
        [IO.Path]::GetFullPath((Join-Path $root $GeneratedSourcePath))
    }
    if ($generatedPath -ceq $trustedPath) {
        throw (
            'The generated test path may not replace the immutable trusted ' +
            'AssertEventually helper source.')
    }

    $trustedItem = Get-Item -LiteralPath $trustedPath -Force -ErrorAction Stop
    if (-not $trustedItem.PSIsContainer -and
        ($trustedItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0) {
        $baselineBlob = @(& git -C $root rev-parse --verify "HEAD:$relativePath" 2>&1)
        $baselineExitCode = $LASTEXITCODE
    } else {
        throw (
            'The trusted AssertEventually helper must be one regular, non-linked ' +
            'repository file.')
    }
    if ($baselineExitCode -ne 0 -or
        $baselineBlob.Count -ne 1 -or
        [string]$baselineBlob[0] -cne $expectedBaselineBlob) {
        throw (
            'The trusted AssertEventually helper does not match the reviewed ' +
            'immutable repository blob pinned by this semantic gate.')
    }

    $bytes = [IO.File]::ReadAllBytes($trustedPath)
    # Git's trusted `*.cs text` checkout may materialize CRLF on Windows while
    # the reviewed blob stores LF. Normalize only CRLF, without running any
    # configurable clean/smudge filter, then compare a pinned content digest.
    $normalizedBytes = [Collections.Generic.List[byte]]::new($bytes.Length)
    for ($index = 0; $index -lt $bytes.Length; $index++) {
        if ($bytes[$index] -eq 13 -and
            ($index + 1) -lt $bytes.Length -and
            $bytes[$index + 1] -eq 10) {
            continue
        }
        $normalizedBytes.Add($bytes[$index])
    }
    $hashAlgorithm = [Security.Cryptography.SHA256]::Create()
    try {
        $actualNormalizedSha256 = [Convert]::ToHexString(
            $hashAlgorithm.ComputeHash(
                [byte[]]$normalizedBytes.ToArray())).ToLowerInvariant()
    }
    finally {
        $hashAlgorithm.Dispose()
    }
    if ($actualNormalizedSha256 -cne $expectedNormalizedSha256) {
        throw (
            "Trusted AssertEventually helper '$relativePath' differs from the " +
            'reviewed immutable source pinned by this semantic gate.')
    }

    $stream = [IO.MemoryStream]::new($bytes, $false)
    $reader = [IO.StreamReader]::new(
        $stream,
        [Text.UTF8Encoding]::new($false, $true),
        $true)
    try {
        $source = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
        $stream.Dispose()
    }
    return [pscustomobject]@{
        Path = $trustedPath
        Source = $source
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
        [Parameter(Mandatory = $true)][AllowNull()]$Edits,
        [string]$SourcePath = 'control.cs',
        [ValidateSet('android', 'ios', 'catalyst', 'windows')]
        [string]$Platform = 'catalyst',
        [Parameter(Mandatory = $true)][string]$ExpectedTestMethod,
        [Parameter(Mandatory = $true)][string]$ExpectedTestClass,
        [AllowEmptyCollection()][string[]]$AdditionalSources = @(),
        [string]$RepositoryRoot = ''
    )

    if (-not $SourcePath.EndsWith(
            '.cs',
            [StringComparison]::OrdinalIgnoreCase)) {
        throw (
            'A trusted negative control requires a C# applyReportedTrigger ' +
            'gate; XAML or other free-form source edits are not accepted.')
    }
    $editList = @($Edits)
    if ($editList.Count -ne 1) {
        throw (
            'A negative control requires exactly one trusted gate edit: change ' +
            'var applyReportedTrigger = true; to false.')
    }

    $edit = $editList[0]
    $find = ''
    $replace = ''
    if ($edit -is [System.Collections.IDictionary]) {
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
    if ($find.Trim() -cne 'var applyReportedTrigger = true;' -or
        $replace.Trim() -cne 'var applyReportedTrigger = false;') {
        throw (
            'The control author may request only the trusted ' +
            'applyReportedTrigger true-to-false edit.')
    }

    $symbol = switch ($Platform) {
        'android' { 'ANDROID' }
        'ios' { 'IOS' }
        'catalyst' { 'MACCATALYST' }
        'windows' { 'WINDOWS' }
    }
    $parseOptions = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithPreprocessorSymbols(
        [string[]]@($symbol))
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        $BaselineSource,
        $parseOptions,
        $SourcePath)
    $syntaxErrors = @($tree.GetDiagnostics() | Where-Object {
            [string]$_.Severity -ceq 'Error'
        } | Select-Object -First 4)
    if ($syntaxErrors.Count -gt 0) {
        throw 'The reproduction source is not valid C# for a trusted negative control gate.'
    }

    $root = $tree.GetRoot()
    $semanticTrees =
        [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    $semanticTrees.Add($tree)
    $trustedAssertEventuallyTree = $null
    $trustedAssertEventuallySource = $null
    $trustedControlsHandlerTestBaseSource = $null
    $mentionsCreateHandlerAndAddToWindow = @(
        $root.DescendantNodes() |
        Where-Object {
            if ($_ -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
                return $false
            }
            $invokedName = if ($_.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax]) {
                $_.Expression.Identifier.ValueText
            } elseif ($_.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                $_.Expression.Identifier.ValueText
            } elseif ($_.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
                $_.Expression.Name.Identifier.ValueText
            } else {
                ''
            }
            return $invokedName -ceq 'CreateHandlerAndAddToWindow'
        }).Count -ne 0
    if ($mentionsCreateHandlerAndAddToWindow) {
        if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
            throw (
                'CreateHandlerAndAddToWindow requires an immutable repository ' +
                'baseline, but RepositoryRoot was not provided.')
        }
        $trustedControlsHandlerTestBaseSource =
            Get-ReplicationTrustedControlsHandlerTestBaseSource `
                -RepositoryRoot $RepositoryRoot `
                -GeneratedSourcePath $SourcePath
    }
    $mentionsAssertEventually = @(
        $root.DescendantNodes() |
        Where-Object {
            $_ -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
            (($_.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $_.Expression.Name.Identifier.ValueText -ceq
                        'AssertEventually') -or
                ($_.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
                    $_.Expression.Identifier.ValueText -ceq
                        'AssertEventually'))
        }).Count -ne 0
    if ($mentionsAssertEventually -and
        -not [string]::IsNullOrWhiteSpace($RepositoryRoot)) {
        $trustedAssertEventuallySource =
            Get-ReplicationTrustedAssertEventuallySource `
                -RepositoryRoot $RepositoryRoot `
                -GeneratedSourcePath $SourcePath
        $trustedAssertEventuallyTree =
            [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
                $trustedAssertEventuallySource.Source,
                $parseOptions,
                $trustedAssertEventuallySource.Path)
        $trustedHelperErrors = @(
            $trustedAssertEventuallyTree.GetDiagnostics() |
            Where-Object { [string]$_.Severity -ceq 'Error' } |
            Select-Object -First 4)
        if ($trustedHelperErrors.Count -ne 0) {
            throw (
                'The immutable trusted AssertEventually helper is not valid C# ' +
                'under the control semantic compilation.')
        }
        $semanticTrees.Add($trustedAssertEventuallyTree)
    }
    $additionalSourceIndex = 0
    foreach ($additionalSource in @($AdditionalSources)) {
        if ([string]::IsNullOrWhiteSpace($additionalSource)) {
            throw 'An additional generated control source is empty.'
        }
        $additionalTree =
            [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
                $additionalSource,
                $parseOptions,
                "generated-control-additional-$additionalSourceIndex.cs")
        $semanticTrees.Add($additionalTree)
        $additionalSourceIndex++
    }
    foreach ($semanticTree in $semanticTrees) {
        $conditionalSymbols = @($semanticTree.GetRoot().DescendantTrivia(
                [System.Func[Microsoft.CodeAnalysis.SyntaxNode, bool]]$null,
                $true) | ForEach-Object {
                $structure = $_.GetStructure()
                if ($structure -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.IfDirectiveTriviaSyntax] -or
                    $structure -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.ElifDirectiveTriviaSyntax]) {
                    $structure.Condition.DescendantNodesAndSelf() |
                        Where-Object {
                            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]
                        } |
                        ForEach-Object { $_.Identifier.ValueText }
                }
            } | Where-Object { $_ } | Sort-Object -Unique)
        $unsupportedSymbols = @($conditionalSymbols | Where-Object {
                $_ -cnotin @('ANDROID', 'IOS', 'MACCATALYST', 'WINDOWS')
            })
        if ($unsupportedSymbols.Count -ne 0) {
            throw (
                'Generated control sources use unsupported conditional symbols: ' +
                ($unsupportedSymbols -join ', ') +
                '. Only trusted platform symbols are accepted.')
        }
        if (@($semanticTree.GetRoot().DescendantNodes() | Where-Object {
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorDeclarationSyntax]
                }).Count -ne 0) {
            throw (
                'Generated control sources may not declare conversion operators; ' +
                'argument evaluation must remain side-effect-free.')
        }
    }
    $semanticCompilation =
        [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
            'Maui.Replication.GeneratedControl',
            [Microsoft.CodeAnalysis.SyntaxTree[]]$semanticTrees.ToArray(),
            [Microsoft.CodeAnalysis.MetadataReference[]]@(
                Get-ReplicationControlSemanticReferences),
            [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
                [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary))
    $semanticModel = $semanticCompilation.GetSemanticModel($tree)
    $trustedAssertEventuallyMethod = $null
    if ($null -ne $trustedAssertEventuallyTree) {
        $trustedHelperModel =
            $semanticCompilation.GetSemanticModel($trustedAssertEventuallyTree)
        $trustedAssertEventuallyMethods = @(
            $trustedAssertEventuallyTree.GetRoot().DescendantNodes() |
            Where-Object {
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -and
                $_.Identifier.ValueText -ceq 'AssertEventually'
            } |
            ForEach-Object { $trustedHelperModel.GetDeclaredSymbol($_) } |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
                $_.MethodKind -eq [Microsoft.CodeAnalysis.MethodKind]::Ordinary -and
                $_.IsStatic -and
                $_.DeclaredAccessibility -eq
                    [Microsoft.CodeAnalysis.Accessibility]::Public -and
                $_.Arity -eq 0 -and
                $_.ContainingType.ToString() -ceq
                    'Microsoft.Maui.DeviceTests.AssertHelpers' -and
                $_.ReturnType -is
                    [Microsoft.CodeAnalysis.INamedTypeSymbol] -and
                $_.ReturnType.Name -ceq 'Task' -and
                $_.ReturnType.ContainingNamespace.ToString() -ceq
                    'System.Threading.Tasks' -and
                $_.ReturnType.ContainingAssembly.Name -cne
                    $semanticCompilation.AssemblyName -and
                @($_.ReturnType.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -eq 0 -and
                $_.Parameters.Length -eq 4 -and
                $_.Parameters[0].Name -ceq 'assertion' -and
                $_.Parameters[0].Type -is
                    [Microsoft.CodeAnalysis.INamedTypeSymbol] -and
                $_.Parameters[0].Type.Name -ceq 'Func' -and
                $_.Parameters[0].Type.ContainingNamespace.ToString() -ceq
                    'System' -and
                $_.Parameters[0].Type.ContainingAssembly.Name -cne
                    $semanticCompilation.AssemblyName -and
                @($_.Parameters[0].Type.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -eq 0 -and
                $_.Parameters[0].Type.TypeArguments.Length -eq 1 -and
                $_.Parameters[0].Type.TypeArguments[0].SpecialType -eq
                    [Microsoft.CodeAnalysis.SpecialType]::System_Boolean -and
                @($_.Parameters[0].Type.TypeArguments[0].Locations |
                    Where-Object {
                        $_.IsInSource
                    }).Count -eq 0 -and
                $_.Parameters[0].RefKind -eq
                    [Microsoft.CodeAnalysis.RefKind]::None -and
                -not $_.Parameters[0].IsOptional -and
                $_.Parameters[1].Name -ceq 'timeout' -and
                $_.Parameters[1].Type.SpecialType -eq
                    [Microsoft.CodeAnalysis.SpecialType]::System_Int32 -and
                $_.Parameters[1].RefKind -eq
                    [Microsoft.CodeAnalysis.RefKind]::None -and
                $_.Parameters[1].IsOptional -and
                [int]$_.Parameters[1].ExplicitDefaultValue -eq 1000 -and
                $_.Parameters[2].Name -ceq 'interval' -and
                $_.Parameters[2].Type.SpecialType -eq
                    [Microsoft.CodeAnalysis.SpecialType]::System_Int32 -and
                $_.Parameters[2].RefKind -eq
                    [Microsoft.CodeAnalysis.RefKind]::None -and
                $_.Parameters[2].IsOptional -and
                [int]$_.Parameters[2].ExplicitDefaultValue -eq 100 -and
                $_.Parameters[3].Name -ceq 'message' -and
                $_.Parameters[3].Type.SpecialType -eq
                    [Microsoft.CodeAnalysis.SpecialType]::System_String -and
                $_.Parameters[3].RefKind -eq
                    [Microsoft.CodeAnalysis.RefKind]::None -and
                $_.Parameters[3].IsOptional -and
                [string]$_.Parameters[3].ExplicitDefaultValue -ceq
                    'Assertion timed out'
            })
        if ($trustedAssertEventuallyMethods.Count -ne 1) {
            throw (
                'The immutable trusted AssertEventually helper no longer exposes ' +
                'the one exact Func<bool> baseline overload.')
        }
        $trustedAssertEventuallyMethod = $trustedAssertEventuallyMethods[0]
        $trustedMethodLocations = @(
            $trustedAssertEventuallyMethod.Locations |
            Where-Object { $_.IsInSource })
        if ($trustedMethodLocations.Count -ne 1 -or
            $trustedMethodLocations[0].SourceTree -ne
                $trustedAssertEventuallyTree -or
            [IO.Path]::GetFullPath(
                $trustedMethodLocations[0].SourceTree.FilePath) -cne
                $trustedAssertEventuallySource.Path) {
            throw (
                'The trusted AssertEventually symbol is not bound to the exact ' +
                'immutable pre-existing repository helper file.')
        }
    }
    foreach ($semanticTree in $semanticTrees) {
        $generatedRoot = $semanticTree.GetRoot()
        $fieldInitializers = @($generatedRoot.DescendantNodes() |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseFieldDeclarationSyntax]
            } |
            ForEach-Object {
                $_.Declaration.Variables
            } |
            Where-Object { $null -ne $_.Initializer })
        if ($fieldInitializers.Count -ne 0) {
            $fieldLine = $semanticTree.GetLineSpan(
                $fieldInitializers[0].Span).StartLinePosition.Line + 1
            throw (
                "Generated test source may not execute instance/static field " +
                "initializers; offending field in '$SourcePath' line $fieldLine.")
        }
        $propertyInitializers = @($generatedRoot.DescendantNodes() |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax] -and
                $null -ne $_.Initializer
            })
        if ($propertyInitializers.Count -ne 0) {
            $propertyLine = $semanticTree.GetLineSpan(
                $propertyInitializers[0].Span).StartLinePosition.Line + 1
            throw (
                "Generated test source may not execute property initializers; " +
                "offending property in '$SourcePath' line $propertyLine.")
        }
        $constructors = @($generatedRoot.DescendantNodes() |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax] -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax] -or
                ((($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax]) -or
                    ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax]) -or
                    ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax])) -and
                    $null -ne $_.ParameterList)
            })
        if ($constructors.Count -ne 0) {
            $constructorLine = $semanticTree.GetLineSpan(
                $constructors[0].Span).StartLinePosition.Line + 1
            throw (
                "Generated test source may not declare constructors; offending " +
                "constructor, primary constructor, or destructor in '$SourcePath' " +
                "line $constructorLine.")
        }
    }

    $declarators = @($root.DescendantNodes() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
            $_.Identifier.ValueText -ceq 'applyReportedTrigger'
        })
    if ($declarators.Count -ne 1) {
        throw 'The reproduction must declare exactly one applyReportedTrigger gate.'
    }
    $declarator = $declarators[0]
    $declaration = $declarator.Parent
    $localDeclaration = $declaration.Parent
    if ($localDeclaration -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax] -or
        $declaration.Variables.Count -ne 1 -or
        $declaration.Type.ToString() -cne 'var' -or
        $null -eq $declarator.Initializer -or
        $declarator.Initializer.Value.RawKind -ne
            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::TrueLiteralExpression) {
        throw (
            'The trusted control gate must be the standalone local declaration ' +
            'var applyReportedTrigger = true;')
    }

    $references = @($root.DescendantNodes() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
            $_.Identifier.ValueText -ceq 'applyReportedTrigger'
        })
    if ($references.Count -ne 1 -or
        $references[0].Parent -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax] -or
        $references[0].Parent.Condition -ne $references[0]) {
        throw (
            'applyReportedTrigger must be used exactly once as the complete ' +
            'condition of one if statement.')
    }
    $gate = $references[0].Parent
    if ($gate.Statement -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax] -or
        $gate.Statement.Statements.Count -ne 1 -or
        $gate.Statement.Statements[0] -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -or
        ($gate.Else -and
            ($gate.Else.Statement -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax] -or
                $gate.Else.Statement.Statements.Count -ne 1 -or
                $gate.Else.Statement.Statements[0] -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax]))) {
        throw (
            'Each applyReportedTrigger branch must contain exactly one direct ' +
            'framework operation; the else branch is optional.')
    }
    $testMethod = @($gate.Ancestors() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax]
        } | Select-Object -First 1)
    $interveningScopes = @($gate.Ancestors() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax]
        })
    $testAttributeNames = if ($testMethod.Count -eq 1) {
        @($testMethod[0].AttributeLists.Attributes | ForEach-Object {
                $name = $_.Name.ToString().Split('.')[-1]
                if ($name.EndsWith(
                        'Attribute',
                        [StringComparison]::Ordinal)) {
                    $name.Substring(0, $name.Length - 'Attribute'.Length)
                } else {
                    $name
                }
            })
    } else {
        @()
    }
    if ($testMethod.Count -ne 1 -or
        $testMethod[0].Identifier.ValueText -cne $ExpectedTestMethod -or
        $interveningScopes.Count -ne 0 -or
        @($testAttributeNames | Where-Object {
                $_ -cin @('Fact', 'Theory', 'Test')
            }).Count -eq 0) {
        throw (
            'The trusted control gate must be directly inside the selected ' +
            "test method '$ExpectedTestMethod', not a lambda, local function, " +
            'or different attributed method.')
    }
    $selectedMethodSymbol = $semanticModel.GetDeclaredSymbol($testMethod[0])
    if ($selectedMethodSymbol -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
        $selectedMethodSymbol.ContainingType.ToString() -cne
            $ExpectedTestClass) {
        throw (
            "The trusted control gate must be in verifier-selected class " +
            "'$ExpectedTestClass', not another class with the same method name.")
    }
    $selectedType = $selectedMethodSymbol.ContainingType
    if ($selectedType.AllInterfaces.Length -ne 0) {
        throw (
            'The selected generated test class may not implement lifecycle or ' +
            'other interfaces whose callbacks execute outside the selected method.')
    }
    $baseType = $selectedType.BaseType
    while ($null -ne $baseType -and
        $baseType.SpecialType -ne [Microsoft.CodeAnalysis.SpecialType]::System_Object) {
        if ($baseType.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            $baseType.ToString() -cne
                'Microsoft.Maui.DeviceTests.ControlsHandlerTestBase') {
            throw (
                "The selected generated test class may inherit only trusted " +
                "external control-contract bases, not '$baseType'.")
        }
        $baseType = $baseType.BaseType
    }
    $sourceOverrides = @($selectedType.DeclaringSyntaxReferences |
        ForEach-Object {
            $_.GetSyntax([Threading.CancellationToken]::None)
        } |
        ForEach-Object {
            $_.Members
        } |
        Where-Object {
            $null -ne $_.PSObject.Properties['Modifiers'] -and
            @($_.Modifiers | Where-Object {
                    $_.RawKind -eq
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::OverrideKeyword
                }).Count -ne 0
        })
    if ($sourceOverrides.Count -ne 0) {
        throw (
            'The selected generated test class may not declare overrides whose ' +
            'framework lifecycle callbacks execute outside the selected method.')
    }
    $selectedAttributes = @($testMethod[0].AttributeLists.Attributes)
    $containingClass = @($testMethod[0].Ancestors() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax]
        } | Select-Object -First 1)
    $classCategoryAttributes = @(if ($containingClass.Count -eq 1) {
        $containingClass[0].AttributeLists | ForEach-Object {
                $_.Attributes
            } | Where-Object {
                $symbol = $semanticModel.GetSymbolInfo($_).Symbol
                $symbol -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
                    $symbol.ContainingType.Name -ceq 'CategoryAttribute'
            }
    })
    if ($classCategoryAttributes.Count -ne 0) {
        throw (
            'The issue Category must be declared once on the selected test method, ' +
            'not on its class, so generated lifecycle members cannot inherit it.')
    }
    $selectedAttributeNames = @($selectedAttributes | ForEach-Object {
            $name = $_.Name.ToString().Split('.')[-1]
            if ($name.EndsWith('Attribute', [StringComparison]::Ordinal)) {
                $name.Substring(0, $name.Length - 'Attribute'.Length)
            } else {
                $name
            }
        })
    $selectedTestAttributeCount = @($selectedAttributeNames | Where-Object {
            $_ -cin @('Fact', 'Theory', 'Test')
        }).Count
    $unsupportedMethodAttributes = @($selectedAttributeNames | Where-Object {
            $_ -cnotin @('Fact', 'Theory', 'Test', 'Category')
        })
    if ($testMethod[0].ParameterList.Parameters.Count -ne 0 -or
        $selectedTestAttributeCount -ne 1 -or
        $unsupportedMethodAttributes.Count -ne 0 -or
        @($selectedAttributeNames | Where-Object {
                $_ -ceq 'Category'
            }).Count -gt 1) {
        throw (
            'A trusted negative-control test must be parameterless, have exactly ' +
            'one trusted test attribute, and may additionally have one Category; ' +
            'data-source or other method attributes are not accepted.')
    }
    $unsupportedAttributes = [System.Collections.Generic.List[object]]::new()
    foreach ($semanticTree in $semanticTrees) {
        foreach ($attribute in @($semanticTree.GetRoot().DescendantNodes() |
                Where-Object {
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax]
                })) {
            $isSelectedMethodAttribute =
                $semanticTree -eq $tree -and
                @($selectedAttributes | Where-Object {
                        $_.SpanStart -eq $attribute.SpanStart -and
                        $_.Span.Length -eq $attribute.Span.Length
                    }).Count -eq 1
            if (-not $isSelectedMethodAttribute) {
                $unsupportedAttributes.Add($attribute)
            }
        }
    }
    if ($unsupportedAttributes.Count -ne 0) {
        $attribute = $unsupportedAttributes[0]
        $attributeTree = $attribute.GetLocation().SourceTree
        $attributeLine = $attributeTree.GetLineSpan(
            $attribute.Span).StartLinePosition.Line + 1
        throw (
            "Generated sources may not apply attributes outside the selected " +
            "test method; offending attribute '$attribute' in " +
            "'$($attributeTree.FilePath)' line $attributeLine.")
    }
    $implicitExecution = @($testMethod[0].Body.DescendantNodes() |
        Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.QueryExpressionSyntax] -or
            ($_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax] -and
                $_.UsingKeyword.RawKind -ne 0)
        })
    if ($implicitExecution.Count -ne 0) {
        $implicitLine = $tree.GetLineSpan(
            $implicitExecution[0].Span).StartLinePosition.Line + 1
        throw (
            "Generated tests may not use foreach, using, or query syntax whose " +
            "implicit framework calls cannot be closed semantically; offending " +
            "syntax in '$SourcePath' line $implicitLine.")
    }
    $validateEventuallyPredicateExpression = $null
    $validateEventuallyPredicateExpression = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
        )

        $conversion = $semanticModel.GetConversion($Expression)
        if ($conversion.IsUserDefined) {
            throw (
                'The trusted AssertEventually predicate may not execute a ' +
                'user-defined conversion.')
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax]) {
            & $validateEventuallyPredicateExpression `
                -Expression $Expression.Expression
            return
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
            if ($Expression.RawKind -notin @(
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::TrueLiteralExpression,
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::FalseLiteralExpression)) {
                throw (
                    'The trusted AssertEventually predicate permits only boolean ' +
                    'literals.')
            }
            return
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            $identifierSymbol =
                $semanticModel.GetSymbolInfo($Expression).Symbol
            if ($identifierSymbol -isnot [Microsoft.CodeAnalysis.ILocalSymbol] -or
                $identifierSymbol.RefKind -ne
                    [Microsoft.CodeAnalysis.RefKind]::None) {
                throw (
                    "AssertEventually predicate identifier '$Expression' must bind " +
                    'to a non-ref selected-test local, not ambient or generated state.')
            }
            $localDeclarations = @(
                $identifierSymbol.DeclaringSyntaxReferences |
                ForEach-Object {
                    $_.GetSyntax([Threading.CancellationToken]::None)
                } |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                    @($_.Ancestors() | Where-Object {
                            $_ -eq $testMethod[0]
                        }).Count -eq 1
                })
            $localType = $identifierSymbol.Type
            $localTypeIsTrusted =
                $localType.SpecialType -eq
                    [Microsoft.CodeAnalysis.SpecialType]::System_Boolean -or
                ($localType.TypeKind -ne
                    [Microsoft.CodeAnalysis.TypeKind]::Error -and
                    $localType.ContainingAssembly.Name -ceq
                        'Microsoft.Maui.Controls.ReplicationControlContract' -and
                    @($localType.Locations | Where-Object {
                            $_.IsInSource
                        }).Count -eq 0)
            if ($localDeclarations.Count -ne 1 -or
                -not $localTypeIsTrusted) {
                throw (
                    "AssertEventually predicate local '$Expression' must be declared " +
                    'once in the selected test with a closed trusted type.')
            }
            return
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
            $property = $semanticModel.GetSymbolInfo($Expression).Symbol
            if ($property -isnot [Microsoft.CodeAnalysis.IPropertySymbol] -or
                $property.IsStatic -or
                $property.IsIndexer -or
                $null -eq $property.GetMethod -or
                $property.ContainingAssembly.Name -cne
                    'Microsoft.Maui.Controls.ReplicationControlContract' -or
                -not $property.ContainingType.ToString().StartsWith(
                    'Microsoft.Maui.Controls.',
                    [StringComparison]::Ordinal) -or
                @($property.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -ne 0 -or
                @($property.GetMethod.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -ne 0) {
                throw (
                    "AssertEventually predicate property '$Expression' must bind " +
                    'to one nonstatic trusted MAUI metadata getter.')
            }
            & $validateEventuallyPredicateExpression `
                -Expression $Expression.Expression
            return
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -and
            $Expression.RawKind -eq
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalNotExpression) {
            $operator = $semanticModel.GetSymbolInfo($Expression).Symbol
            if ($null -ne $operator -and
                ($operator -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
                    $operator.MethodKind -ne
                        [Microsoft.CodeAnalysis.MethodKind]::BuiltinOperator)) {
                throw (
                    'The trusted AssertEventually predicate may not execute an ' +
                    'overloaded unary operator.')
            }
            & $validateEventuallyPredicateExpression `
                -Expression $Expression.Operand
            return
        }
        if ($Expression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax] -and
            $Expression.RawKind -in @(
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalAndExpression,
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalOrExpression,
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::EqualsExpression,
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::NotEqualsExpression)) {
            $operator = $semanticModel.GetSymbolInfo($Expression).Symbol
            $convertedType =
                $semanticModel.GetTypeInfo($Expression).ConvertedType
            if (($null -ne $operator -and
                    ($operator -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
                        $operator.MethodKind -ne
                            [Microsoft.CodeAnalysis.MethodKind]::BuiltinOperator)) -or
                $null -eq $convertedType -or
                $convertedType.SpecialType -ne
                    [Microsoft.CodeAnalysis.SpecialType]::System_Boolean) {
                throw (
                    'The trusted AssertEventually predicate may use only built-in ' +
                    'boolean/reference operators.')
            }
            & $validateEventuallyPredicateExpression -Expression $Expression.Left
            & $validateEventuallyPredicateExpression -Expression $Expression.Right
            return
        }
        throw (
            "AssertEventually predicate syntax '$Expression' is outside the closed " +
            'pure-observation contract; only property/local reads and boolean ' +
            'composition are accepted.')
    }
    $acceptedAssertEventuallyInvocations =
        [Collections.Generic.HashSet[int]]::new()
    $acceptedTrustedWindowHelperInvocations =
        [Collections.Generic.HashSet[int]]::new()
    $acceptedTrustedBackTitleSetups =
        [Collections.Generic.HashSet[int]]::new()
    $trustedWindowCallbackBodies =
        [Collections.Generic.HashSet[int]]::new()
    $trustedWindowCallbackOracleMinimums =
        [Collections.Generic.Dictionary[int, int]]::new()
    $trustedWindowCallbackAttachedSymbols =
        [Collections.Generic.Dictionary[int, object]]::new()
    $trustedWindowCallbackNavigationRoots =
        [Collections.Generic.Dictionary[int, object]]::new()
    $trustedWindowCallbackBackTitleRoots =
        [Collections.Generic.Dictionary[int, object]]::new()
    $trustedWindowCallbackFinalPages =
        [Collections.Generic.Dictionary[int, object]]::new()
    $getInvocationName = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]$Invocation
        )
        if ($Invocation.Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax]) {
            return $Invocation.Expression.Identifier.ValueText
        }
        if ($Invocation.Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            return $Invocation.Expression.Identifier.ValueText
        }
        if ($Invocation.Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
            return $Invocation.Expression.Name.Identifier.ValueText
        }
        return ''
    }
    $isExactTrustedWindowHelperMethod = {
        param(
            [AllowNull()]
            [Microsoft.CodeAnalysis.IMethodSymbol]$Method
        )
        if ($null -eq $Method -or
            $Method.MethodKind -ne [Microsoft.CodeAnalysis.MethodKind]::Ordinary -or
            $Method.IsStatic -or
            $Method.DeclaredAccessibility -ne
                [Microsoft.CodeAnalysis.Accessibility]::Protected -or
            $Method.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            $Method.ContainingType.ToString() -cne
                'Microsoft.Maui.DeviceTests.ControlsHandlerTestBase' -or
            $Method.Name -cne 'CreateHandlerAndAddToWindow' -or
            $Method.Arity -ne 1 -or
            $Method.TypeArguments.Length -ne 1 -or
            @($Method.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            return $false
        }

        $definition = $Method.OriginalDefinition
        if ($definition.Parameters.Length -ne 2 -or
            $definition.TypeParameters.Length -ne 1 -or
            $definition.ReturnType -isnot
                [Microsoft.CodeAnalysis.INamedTypeSymbol] -or
            $definition.ReturnType.Name -cne 'Task' -or
            $definition.ReturnType.ContainingNamespace.ToString() -cne
                'System.Threading.Tasks' -or
            $definition.Parameters[0].Name -cne 'view' -or
            $definition.Parameters[0].RefKind -ne
                [Microsoft.CodeAnalysis.RefKind]::None -or
            $definition.Parameters[0].Type.ToString() -cne
                'Microsoft.Maui.IElement' -or
            $definition.Parameters[0].Type.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            $definition.Parameters[1].Name -cne 'action' -or
            $definition.Parameters[1].RefKind -ne
                [Microsoft.CodeAnalysis.RefKind]::None -or
            $definition.Parameters[1].Type -isnot
                [Microsoft.CodeAnalysis.INamedTypeSymbol]) {
            return $false
        }

        $callbackType = $definition.Parameters[1].Type
        $typeParameter = $definition.TypeParameters[0]
        if ($callbackType.Name -cne 'Func' -or
            $callbackType.ContainingNamespace.ToString() -cne 'System' -or
            $callbackType.TypeArguments.Length -ne 2 -or
            -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $callbackType.TypeArguments[0],
                $typeParameter) -or
            $callbackType.TypeArguments[1] -isnot
                [Microsoft.CodeAnalysis.INamedTypeSymbol] -or
            $callbackType.TypeArguments[1].Name -cne 'Task' -or
            $callbackType.TypeArguments[1].ContainingNamespace.ToString() -cne
                'System.Threading.Tasks' -or
            -not $typeParameter.HasReferenceTypeConstraint -or
            $typeParameter.HasValueTypeConstraint -or
            $typeParameter.HasConstructorConstraint -or
            $typeParameter.ConstraintTypes.Length -ne 1 -or
            $typeParameter.ConstraintTypes[0].ToString() -cne
                'Microsoft.Maui.IElementHandler' -or
            $typeParameter.ConstraintTypes[0].ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract') {
            return $false
        }

        $handlerType = $Method.TypeArguments[0]
        if ($handlerType -isnot [Microsoft.CodeAnalysis.INamedTypeSymbol] -or
            $handlerType.ToString() -cne
                'Microsoft.Maui.DeviceTests.Stubs.WindowHandlerStub' -or
            $handlerType.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            @($handlerType.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            return $false
        }
        return @($handlerType.AllInterfaces | Where-Object {
                $_.ToString() -ceq 'Microsoft.Maui.IElementHandler' -and
                $_.ContainingAssembly.Name -ceq
                    'Microsoft.Maui.Controls.ReplicationControlContract' -and
                @($_.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -eq 0
            }).Count -eq 1
    }
    $throwTrustedWindowHelperViolation = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.SyntaxNode]$Node,
            [AllowNull()]$HelperSymbol,
            [Parameter(Mandatory = $true)][string]$Reason,
            [switch]$Callback
        )
        $diagnosticTree = $Node.GetLocation().SourceTree
        $diagnosticPath = if ($diagnosticTree -and
            -not [string]::IsNullOrWhiteSpace($diagnosticTree.FilePath)) {
            $diagnosticTree.FilePath
        } else {
            $SourcePath
        }
        $diagnosticLine = if ($diagnosticTree) {
            $diagnosticTree.GetLineSpan($Node.Span).StartLinePosition.Line + 1
        } else {
            0
        }
        $diagnosticSyntax = $Node.ToString()
        if ($diagnosticSyntax.Length -gt 200) {
            $diagnosticSyntax =
                $diagnosticSyntax.Substring(0, 200) + '...'
        }
        $symbolText = if ($null -ne $HelperSymbol) {
            [string]$HelperSymbol
        } else {
            '<unresolved>'
        }
        $context = if ($Callback) {
            'callback'
        } else {
            'invocation'
        }
        throw (
            "Trusted helper symbol '$symbolText' rejected $context syntax " +
            "'$diagnosticSyntax' in '$diagnosticPath' line ${diagnosticLine}: " +
            $Reason)
    }
    $validateTrustedWindowHelperInvocation = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]$AwaitExpression,
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]$Invocation,
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.IMethodSymbol]$HelperMethod
        )

        if ($null -eq $trustedControlsHandlerTestBaseSource) {
            & $throwTrustedWindowHelperViolation `
                -Node $Invocation `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the exact immutable ControlsHandlerTestBase source location ' +
                    'and hash were not established.')
        }
        $statement = $AwaitExpression.Parent
        if ($statement -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -or
            $statement.Expression -ne $AwaitExpression -or
            $statement.Parent -ne $testMethod[0].Body -or
            $statement.SpanStart -le $gate.Span.End) {
            & $throwTrustedWindowHelperViolation `
                -Node $Invocation `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the immutable helper must be directly awaited as one ' +
                    'top-level selected-method statement after the reported-trigger gate.')
        }
        if ($Invocation.ArgumentList.Arguments.Count -ne 2 -or
            @($Invocation.ArgumentList.Arguments | Where-Object {
                    $_.RefKindKeyword.RawKind -ne 0 -or
                    $null -ne $_.NameColon
                }).Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $Invocation `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the reviewed overload requires exactly the positional ' +
                    'IElement and Func<WindowHandlerStub, Task> arguments.')
        }

        $windowCreation = $Invocation.ArgumentList.Arguments[0].Expression
        if ($windowCreation -isnot
            [Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax]) {
            & $throwTrustedWindowHelperViolation `
                -Node $windowCreation `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the first argument must be a direct new external ' +
                    'Microsoft.Maui.Controls.Window(navigationPage) expression.')
        }
        $windowConstructor =
            $semanticModel.GetSymbolInfo($windowCreation).Symbol
        if ($windowConstructor -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
            $windowConstructor.MethodKind -ne
                [Microsoft.CodeAnalysis.MethodKind]::Constructor -or
            $windowConstructor.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            $windowConstructor.ContainingType.ToString() -cne
                'Microsoft.Maui.Controls.Window' -or
            @($windowConstructor.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0 -or
            $windowConstructor.Parameters.Length -ne 1 -or
            $windowConstructor.Parameters[0].Type.ToString() -cne
                'Microsoft.Maui.Controls.Page' -or
            $windowCreation.ArgumentList.Arguments.Count -ne 1 -or
            $null -ne $windowCreation.Initializer) {
            & $throwTrustedWindowHelperViolation `
                -Node $windowCreation `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the Window constructor must resolve to the exact external ' +
                    'one-Page MAUI contract.')
        }
        $windowContent =
            $windowCreation.ArgumentList.Arguments[0].Expression
        $windowContentSymbol =
            $semanticModel.GetSymbolInfo($windowContent).Symbol
        $windowContentType = $semanticModel.GetTypeInfo($windowContent).Type
        $windowContentDeclarations = @(if ($windowContentSymbol -is
                [Microsoft.CodeAnalysis.ILocalSymbol]) {
            $windowContentSymbol.DeclaringSyntaxReferences |
                ForEach-Object {
                    $_.GetSyntax([Threading.CancellationToken]::None)
                } |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                    $null -ne $_.Initializer
                }
        })
        if ($windowContent -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
            $windowContentSymbol -isnot
                [Microsoft.CodeAnalysis.ILocalSymbol] -or
            $windowContentSymbol.RefKind -ne
                [Microsoft.CodeAnalysis.RefKind]::None -or
            $windowContentType.ToString() -cne
                'Microsoft.Maui.Controls.NavigationPage' -or
            $windowContentType.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            @($windowContentType.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0 -or
            $windowContentDeclarations.Count -ne 1 -or
            $windowContentDeclarations[0].SpanStart -ge $gate.SpanStart) {
            & $throwTrustedWindowHelperViolation `
                -Node $windowContent `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'new Window must receive one stable pre-gate selected-test ' +
                    'NavigationPage local with external MAUI type.')
        }
        try {
            Confirm-ReplicationTrustedOracleExpression `
                -Expression $windowContent `
                -SemanticModel $semanticModel `
                -Root $root
        }
        catch {
            & $throwTrustedWindowHelperViolation `
                -Node $windowContent `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the Window content is not safe closed test-local dataflow: ' +
                    $_.Exception.Message)
        }

        $navigationCreation =
            $windowContentDeclarations[0].Initializer.Value
        $navigationRootExpression = if ($navigationCreation -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax] -and
            $navigationCreation.ArgumentList.Arguments.Count -eq 1) {
            $navigationCreation.ArgumentList.Arguments[0].Expression
        } else {
            $null
        }
        $navigationRootSymbol = if ($null -ne $navigationRootExpression) {
            $semanticModel.GetSymbolInfo($navigationRootExpression).Symbol
        } else {
            $null
        }
        $backTitleSetups = @(
            $testMethod[0].Body.DescendantNodes() |
            Where-Object {
                if ($_ -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
                    return $false
                }
                $method = $semanticModel.GetSymbolInfo($_).Symbol
                return (
                    $method -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
                    $method.ContainingAssembly.Name -ceq
                        'Microsoft.Maui.Controls.ReplicationControlContract' -and
                    $method.ContainingType.ToString() -ceq
                        'Microsoft.Maui.Controls.NavigationPage' -and
                    $method.Name -ceq 'SetBackButtonTitle')
            })
        $backTitleSetup = if ($backTitleSetups.Count -eq 1) {
            $backTitleSetups[0]
        } else {
            $null
        }
        $backTitleTarget = if ($null -ne $backTitleSetup -and
            $backTitleSetup.ArgumentList.Arguments.Count -eq 2) {
            $backTitleSetup.ArgumentList.Arguments[0].Expression
        } else {
            $null
        }
        $backTitleValue = if ($null -ne $backTitleSetup -and
            $backTitleSetup.ArgumentList.Arguments.Count -eq 2) {
            $backTitleSetup.ArgumentList.Arguments[1].Expression
        } else {
            $null
        }
        $backTitleTargetSymbol = if ($null -ne $backTitleTarget) {
            $semanticModel.GetSymbolInfo($backTitleTarget).Symbol
        } else {
            $null
        }
        if ($backTitleSetups.Count -ne 1 -or
            $backTitleSetup.Parent -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -or
            $backTitleSetup.Parent.Parent -ne $testMethod[0].Body -or
            $backTitleSetup.SpanStart -ge $gate.SpanStart -or
            $backTitleTarget -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
            $backTitleValue -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax] -or
            $backTitleValue.Token.ValueText -cne 'Main' -or
            $navigationRootExpression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
            -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $backTitleTargetSymbol,
                $navigationRootSymbol)) {
            & $throwTrustedWindowHelperViolation `
                -Node $(if ($null -ne $backTitleSetup) {
                    $backTitleSetup
                } else {
                    $Invocation
                }) `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the immutable Window helper scenario requires exactly one ' +
                    'top-level pre-gate SetBackButtonTitle(rootPage, "Main") on ' +
                    'the exact root Page used to construct its NavigationPage.')
        }
        [void]$acceptedTrustedBackTitleSetups.Add(
            $backTitleSetup.SpanStart)
        $handlerWrites = @(
            $testMethod[0].Body.DescendantNodes() |
            Where-Object {
                if ($_ -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
                    return $false
                }
                $property = $semanticModel.GetSymbolInfo($_.Left).Symbol
                return (
                    $property -is [Microsoft.CodeAnalysis.IPropertySymbol] -and
                    $property.ContainingAssembly.Name -ceq
                        'Microsoft.Maui.Controls.ReplicationControlContract' -and
                    $property.ContainingType.ToString() -ceq
                        'Microsoft.Maui.Controls.Element' -and
                    $property.Name -ceq 'Handler')
            })
        if ($handlerWrites.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $handlerWrites[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the immutable Window NavigationPage.Handler must be created ' +
                    'only by the trusted helper and may not be assigned by generated code.')
        }

        $callback =
            $Invocation.ArgumentList.Arguments[1].Expression
        $callbackParameters = @(if ($callback -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax]) {
            $callback.ParameterList.Parameters
        } elseif ($callback -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax]) {
            $callback.Parameter
        } else {
            @()
        })
        $callbackType = $semanticModel.GetTypeInfo($callback).ConvertedType
        $callbackParameterSymbol = if ($callbackParameters.Count -eq 1) {
            $semanticModel.GetDeclaredSymbol($callbackParameters[0])
        } else {
            $null
        }
        if ($callbackParameters.Count -ne 1 -or
            $callback.AsyncKeyword.RawKind -eq 0 -or
            $callback.Body -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax] -or
            $callbackType -isnot [Microsoft.CodeAnalysis.INamedTypeSymbol] -or
            $callbackType.Name -cne 'Func' -or
            $callbackType.ContainingNamespace.ToString() -cne 'System' -or
            $callbackType.TypeArguments.Length -ne 2 -or
            $callbackType.TypeArguments[0].ToString() -cne
                'Microsoft.Maui.DeviceTests.Stubs.WindowHandlerStub' -or
            $callbackType.TypeArguments[0].ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            $callbackType.TypeArguments[1].ToString() -cne
                'System.Threading.Tasks.Task' -or
            $callbackParameterSymbol -isnot
                [Microsoft.CodeAnalysis.IParameterSymbol] -or
            $callbackParameterSymbol.RefKind -ne
                [Microsoft.CodeAnalysis.RefKind]::None -or
            -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $callbackParameterSymbol.Type,
                $HelperMethod.TypeArguments[0])) {
            & $throwTrustedWindowHelperViolation `
                -Node $callback `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the callback must be one async block lambda whose sole ' +
                    'parameter is exactly the trusted WindowHandlerStub/IElementHandler type.')
        }

        $callbackBody = $callback.Body
        $deferredEventSubscriptions = @(
            $testMethod[0].Body.DescendantNodes() |
            Where-Object {
                if ($_ -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
                    return $false
                }
                return $semanticModel.GetSymbolInfo($_.Left).Symbol -is
                    [Microsoft.CodeAnalysis.IEventSymbol]
            })
        if ($deferredEventSubscriptions.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $deferredEventSubscriptions[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'generated tests using the immutable Window helper may not ' +
                    'register deferred event callbacks that can mutate state ' +
                    'during the trusted lifecycle transition.') `
                -Callback
        }
        $callbackWrites = @($callbackBody.DescendantNodes() |
            Where-Object {
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                $_.RawKind -in @(
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression)
            })
        if ($callbackWrites.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $callbackWrites[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'generated helper callbacks may not assign framework/local ' +
                    'state or increment values.') `
                -Callback
        }
        $callbackEscapes = @($callbackBody.DescendantNodes() |
            Where-Object {
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ThrowExpressionSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax]
            })
        if ($callbackEscapes.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $callbackEscapes[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'generated helper callbacks may not return, yield, jump, ' +
                    'throw, or declare deferred local functions.') `
                -Callback
        }
        $callbackLoops = @($callbackBody.DescendantNodes() |
            Where-Object {
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax]
            })
        if ($callbackLoops.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $callbackLoops[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'generated helper callbacks may not loop around lifecycle ' +
                    'operations or the mandatory oracle.') `
                -Callback
        }
        $callbackByRefArguments = @($callbackBody.DescendantNodes() |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax] -and
                $_.RefKindKeyword.RawKind -ne 0
            })
        if ($callbackByRefArguments.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $callbackByRefArguments[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'generated helper callbacks may not pass ref, out, or in arguments.') `
                -Callback
        }
        $callbackCreations = @($callbackBody.DescendantNodes() |
            Where-Object {
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax] -or
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitObjectCreationExpressionSyntax]
            })
        if ($callbackCreations.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $callbackCreations[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'generated helper callbacks may observe existing test-local ' +
                    'objects but may not construct replacement state.') `
                -Callback
        }
        $callbackIndexers = @($callbackBody.DescendantNodes() |
            Where-Object {
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax]
            })
        if ($callbackIndexers.Count -ne 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $callbackIndexers[0] `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'generated helper callbacks may not execute indexers.') `
                -Callback
        }

        foreach ($nestedLambda in @($callbackBody.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax]
                })) {
            $argument = $nestedLambda.Parent
            $nestedInvocation = @($nestedLambda.Ancestors() | Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
                } | Select-Object -First 1)
            $nestedMethod = if ($nestedInvocation.Count -eq 1) {
                $semanticModel.GetSymbolInfo($nestedInvocation[0]).Symbol
            } else {
                $null
            }
            $isEventuallyPredicate =
                $argument -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax] -and
                $argument.Expression -eq $nestedLambda -and
                $nestedInvocation.Count -eq 1 -and
                $nestedInvocation[0].ArgumentList.Arguments.Count -ge 1 -and
                $nestedInvocation[0].ArgumentList.Arguments[0] -eq $argument -and
                $null -ne $trustedAssertEventuallyMethod -and
                [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                    $nestedMethod,
                    $trustedAssertEventuallyMethod)
            if (-not $isEventuallyPredicate) {
                & $throwTrustedWindowHelperViolation `
                    -Node $nestedLambda `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'only the exact immutable AssertEventually pure predicate ' +
                        'lambda may be nested in this callback.') `
                    -Callback
            }
        }

        foreach ($callbackExpression in @($callbackBody.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
                })) {
            if ($semanticModel.GetConversion($callbackExpression).IsUserDefined) {
                & $throwTrustedWindowHelperViolation `
                    -Node $callbackExpression `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'generated helper callbacks may not execute user-defined ' +
                        'conversions.') `
                    -Callback
            }
        }
        foreach ($operatorExpression in @($callbackBody.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax] -or
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -or
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax]
                })) {
            $operatorMethod =
                $semanticModel.GetSymbolInfo($operatorExpression).Symbol
            if ($null -ne $operatorMethod -and
                ($operatorMethod -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
                    $operatorMethod.MethodKind -ne
                        [Microsoft.CodeAnalysis.MethodKind]::BuiltinOperator)) {
                & $throwTrustedWindowHelperViolation `
                    -Node $operatorExpression `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'generated helper callbacks may use only built-in operators.') `
                    -Callback
            }
        }

        $trustedCallbackAssertionTypes = @(
            'Xunit.Assert',
            'NUnit.Framework.Assert',
            'NUnit.Framework.ClassicAssert',
            'Microsoft.VisualStudio.TestTools.UnitTesting.Assert',
            'Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert',
            'Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert')
        $callbackLifecycleCalls =
            [Collections.Generic.List[
                Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]]::new()
        $callbackEventuallyCalls =
            [Collections.Generic.List[
                Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]]::new()
        $callbackStateRealizingCalls =
            [Collections.Generic.List[
                Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]]::new()
        $isExactTrustedCallbackOperation = {
            param(
                [Parameter(Mandatory = $true)]
                [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]$Call,
                [Parameter(Mandatory = $true)]
                [Microsoft.CodeAnalysis.IMethodSymbol]$Method,
                [Parameter(Mandatory = $true)]
                [Microsoft.CodeAnalysis.IMethodSymbol]$Definition,
                [Parameter(Mandatory = $true)][string]$Key
            )

            $parameterTypes = @($Definition.Parameters | ForEach-Object {
                    $_.Type.ToString()
                })
            switch -CaseSensitive ($Key) {
                'Microsoft.Maui.Controls.NavigationPage.PushAsync' {
                    return (
                        -not $Definition.IsStatic -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 1 -and
                        $parameterTypes.Count -eq 1 -and
                        $parameterTypes[0] -ceq
                            'Microsoft.Maui.Controls.Page' -and
                        $Definition.ReturnType.ToString() -ceq
                            'System.Threading.Tasks.Task')
                }
                'Microsoft.Maui.Controls.NavigationPage.PopAsync' {
                    return (
                        -not $Definition.IsStatic -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 0 -and
                        $parameterTypes.Count -eq 0 -and
                        $Definition.ReturnType.ToString() -ceq
                            'System.Threading.Tasks.Task<Microsoft.Maui.Controls.Page>')
                }
                'Microsoft.Maui.Controls.INavigation.PushAsync' {
                    return (
                        -not $Definition.IsStatic -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 1 -and
                        $parameterTypes.Count -eq 1 -and
                        $parameterTypes[0] -ceq
                            'Microsoft.Maui.Controls.Page' -and
                        $Definition.ReturnType.ToString() -ceq
                            'System.Threading.Tasks.Task')
                }
                'Microsoft.Maui.Controls.INavigation.PopAsync' {
                    return (
                        -not $Definition.IsStatic -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 0 -and
                        $parameterTypes.Count -eq 0 -and
                        $Definition.ReturnType.ToString() -ceq
                            'System.Threading.Tasks.Task<Microsoft.Maui.Controls.Page>')
                }
                'Microsoft.Maui.Platform.ElementExtensions.ToPlatform' {
                    return (
                        $Definition.IsStatic -and
                        $Definition.IsExtensionMethod -and
                        $null -ne $Method.ReducedFrom -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 0 -and
                        $parameterTypes.Count -eq 1 -and
                        $parameterTypes[0] -ceq
                            'Microsoft.Maui.Controls.Element' -and
                        $Definition.ReturnType.ToString() -ceq 'UIKit.UIView')
                }
                'Microsoft.Maui.Platform.ViewExtensions.FindDescendantView' {
                    $descendantType = if ($Method.TypeArguments.Length -eq 1) {
                        $Method.TypeArguments[0]
                    } else {
                        $null
                    }
                    $isTrustedViewType =
                        $descendantType -is
                            [Microsoft.CodeAnalysis.INamedTypeSymbol] -and
                        $descendantType.ContainingAssembly.Name -ceq
                            'Microsoft.Maui.Controls.ReplicationControlContract' -and
                        @($descendantType.Locations | Where-Object {
                                $_.IsInSource
                            }).Count -eq 0
                    $viewBase = $descendantType
                    $inheritsView = $false
                    while ($isTrustedViewType -and $null -ne $viewBase) {
                        if ($viewBase.ToString() -ceq 'UIKit.UIView') {
                            $inheritsView = $true
                            break
                        }
                        $viewBase = $viewBase.BaseType
                    }
                    return (
                        $Definition.IsStatic -and
                        $Definition.IsExtensionMethod -and
                        $null -ne $Method.ReducedFrom -and
                        $Definition.Arity -eq 1 -and
                        $Call.ArgumentList.Arguments.Count -eq 0 -and
                        $parameterTypes.Count -eq 1 -and
                        $parameterTypes[0] -ceq 'UIKit.UIView' -and
                        $isTrustedViewType -and
                        $inheritsView)
                }
                'Microsoft.Maui.DeviceTests.AssertionExtensions.GetBackButton' {
                    return (
                        $Definition.IsStatic -and
                        $Definition.IsExtensionMethod -and
                        $null -ne $Method.ReducedFrom -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 0 -and
                        $parameterTypes.Count -eq 1 -and
                        $parameterTypes[0] -ceq
                            'UIKit.UINavigationBar' -and
                        $Definition.ReturnType.ToString() -ceq 'UIKit.UIView')
                }
                'CoreGraphics.CGRect.Intersect' {
                    return (
                        $Definition.IsStatic -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 2 -and
                        $parameterTypes.Count -eq 2 -and
                        $parameterTypes[0] -ceq 'CoreGraphics.CGRect' -and
                        $parameterTypes[1] -ceq 'CoreGraphics.CGRect' -and
                        $Definition.ReturnType.ToString() -ceq
                            'CoreGraphics.CGRect')
                }
                'UIKit.UIView.ConvertRectToView' {
                    return (
                        -not $Definition.IsStatic -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 2 -and
                        $parameterTypes.Count -eq 2 -and
                        $parameterTypes[0] -ceq 'CoreGraphics.CGRect' -and
                        $parameterTypes[1] -ceq 'UIKit.UIView' -and
                        $Definition.ReturnType.ToString() -ceq
                            'CoreGraphics.CGRect')
                }
                'UIKit.UINavigationBar.LayoutIfNeeded' {
                    return (
                        -not $Definition.IsStatic -and
                        $Definition.Arity -eq 0 -and
                        $Call.ArgumentList.Arguments.Count -eq 0 -and
                        $parameterTypes.Count -eq 0 -and
                        $Definition.ReturnsVoid)
                }
                default {
                    return $false
                }
            }
        }
        foreach ($callbackCall in @($callbackBody.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
                })) {
            $callbackMethod =
                $semanticModel.GetSymbolInfo($callbackCall).Symbol
            if ($null -ne $trustedAssertEventuallyMethod -and
                [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                    $callbackMethod,
                    $trustedAssertEventuallyMethod)) {
                $eventuallyAwait = $callbackCall.Parent
                $eventuallyStatement = if ($eventuallyAwait -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]) {
                    $eventuallyAwait.Parent
                } else {
                    $null
                }
                if ($eventuallyAwait -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax] -or
                    $eventuallyAwait.Expression -ne $callbackCall -or
                    $eventuallyStatement -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -or
                    $eventuallyStatement.Parent -ne $callbackBody) {
                    & $throwTrustedWindowHelperViolation `
                        -Node $callbackCall `
                        -HelperSymbol $HelperMethod `
                        -Reason (
                            'the immutable AssertEventually predicate must be ' +
                            'directly awaited as a top-level callback statement.') `
                        -Callback
                }
                $callbackEventuallyCalls.Add($callbackCall)
                continue
            }
            $callbackDefinition = if ($callbackMethod -is
                    [Microsoft.CodeAnalysis.IMethodSymbol] -and
                $null -ne $callbackMethod.ReducedFrom) {
                $callbackMethod.ReducedFrom
            } else {
                $callbackMethod
            }
            $callbackCallKey = if ($callbackDefinition -is
                [Microsoft.CodeAnalysis.IMethodSymbol]) {
                "$($callbackDefinition.ContainingType).$($callbackDefinition.Name)"
            } else {
                ''
            }
            $isTrustedAssertionCall =
                $callbackMethod -is
                    [Microsoft.CodeAnalysis.IMethodSymbol] -and
                $callbackMethod.ContainingAssembly.Name -ceq
                    'Microsoft.Maui.Controls.ReplicationControlContract' -and
                $callbackMethod.ContainingType.ToString() -cin
                    $trustedCallbackAssertionTypes -and
                @($callbackMethod.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -eq 0
            $isTrustedLifecycleOrObservationCall =
                $callbackDefinition -is
                    [Microsoft.CodeAnalysis.IMethodSymbol] -and
                $callbackDefinition.MethodKind -eq
                    [Microsoft.CodeAnalysis.MethodKind]::Ordinary -and
                $callbackDefinition.ContainingAssembly.Name -ceq
                    'Microsoft.Maui.Controls.ReplicationControlContract' -and
                @($callbackDefinition.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -eq 0 -and
                (& $isExactTrustedCallbackOperation `
                    -Call $callbackCall `
                    -Method $callbackMethod `
                    -Definition $callbackDefinition `
                    -Key $callbackCallKey)
            if ($callbackCallKey -cin @(
                    'Microsoft.Maui.Controls.NavigationPage.PushAsync',
                    'Microsoft.Maui.Controls.NavigationPage.PopAsync',
                    'Microsoft.Maui.Controls.INavigation.PushAsync',
                    'Microsoft.Maui.Controls.INavigation.PopAsync')) {
                $lifecycleAwait = $callbackCall.Parent
                $lifecycleStatement = if ($lifecycleAwait -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]) {
                    $lifecycleAwait.Parent
                } else {
                    $null
                }
                if ($lifecycleAwait -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax] -or
                    $lifecycleAwait.Expression -ne $callbackCall -or
                    $lifecycleStatement -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -or
                    $lifecycleStatement.Parent -ne $callbackBody) {
                    $isTrustedLifecycleOrObservationCall = $false
                }
            }
            if (@($callbackCall.ArgumentList.Arguments | Where-Object {
                        $null -ne $_.NameColon
                    }).Count -ne 0) {
                $isTrustedLifecycleOrObservationCall = $false
            }
            if (-not $isTrustedAssertionCall -and
                -not $isTrustedLifecycleOrObservationCall) {
                & $throwTrustedWindowHelperViolation `
                    -Node $callbackCall `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        "call '$callbackMethod' is not an exact trusted " +
                        'lifecycle, observation, assertion, or AssertEventually operation.') `
                    -Callback
            }
            if ($isTrustedLifecycleOrObservationCall -and
                $callbackCallKey -cin @(
                    'Microsoft.Maui.Controls.NavigationPage.PushAsync',
                    'Microsoft.Maui.Controls.NavigationPage.PopAsync',
                    'Microsoft.Maui.Controls.INavigation.PushAsync',
                    'Microsoft.Maui.Controls.INavigation.PopAsync')) {
                $callbackLifecycleCalls.Add($callbackCall)
            }
            if ($isTrustedLifecycleOrObservationCall -and
                $callbackCallKey -ceq
                    'UIKit.UINavigationBar.LayoutIfNeeded') {
                $callbackStateRealizingCalls.Add($callbackCall)
            }
        }

        if ($callbackLifecycleCalls.Count -eq 0 -or
            $callbackEventuallyCalls.Count -eq 0) {
            & $throwTrustedWindowHelperViolation `
                -Node $callback `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the trusted post-gate callback requires a directly awaited ' +
                    'lifecycle transition followed by the exact immutable ' +
                    'AssertEventually observation before its oracle.') `
                -Callback
        }
        foreach ($eventuallyCall in $callbackEventuallyCalls) {
            if (@($callbackLifecycleCalls | Where-Object {
                        $_.SpanStart -lt $eventuallyCall.SpanStart
                    }).Count -eq 0) {
                & $throwTrustedWindowHelperViolation `
                    -Node $eventuallyCall `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'the immutable AssertEventually observation must follow ' +
                        'a directly awaited lifecycle transition in the callback.') `
                    -Callback
            }
        }
        $lastLifecycleCall = @($callbackLifecycleCalls |
            Sort-Object SpanStart |
            Select-Object -Last 1)
        $lastEventuallyCall = @($callbackEventuallyCalls |
            Sort-Object SpanStart |
            Select-Object -Last 1)
        if ($lastLifecycleCall.Count -ne 1 -or
            $lastEventuallyCall.Count -ne 1 -or
            $lastEventuallyCall[0].SpanStart -le
                    $lastLifecycleCall[0].SpanStart) {
            & $throwTrustedWindowHelperViolation `
                    -Node $lastLifecycleCall[0] `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'the final directly awaited lifecycle transition must be ' +
                        'followed by a directly awaited immutable AssertEventually ' +
                        'observation before the callback oracle.') `
                    -Callback
        }

        foreach ($callbackMember in @($callbackBody.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]
                })) {
            $callbackMemberSymbol =
                $semanticModel.GetSymbolInfo($callbackMember).Symbol
            if ($callbackMemberSymbol -is [Microsoft.CodeAnalysis.IPropertySymbol] -or
                $callbackMemberSymbol -is [Microsoft.CodeAnalysis.IFieldSymbol]) {
                if ($callbackMemberSymbol -is
                        [Microsoft.CodeAnalysis.IPropertySymbol] -and
                    ($callbackMemberSymbol.IsIndexer -or
                        $null -eq $callbackMemberSymbol.GetMethod)) {
                    & $throwTrustedWindowHelperViolation `
                        -Node $callbackMember `
                        -HelperSymbol $HelperMethod `
                        -Reason (
                            'callback property observations require one direct, ' +
                            'non-indexed external getter.') `
                        -Callback
                }
                if ($callbackMemberSymbol.ContainingAssembly.Name -cne
                        'Microsoft.Maui.Controls.ReplicationControlContract' -or
                    @($callbackMemberSymbol.Locations | Where-Object {
                            $_.IsInSource
                        }).Count -ne 0) {
                    & $throwTrustedWindowHelperViolation `
                        -Node $callbackMember `
                        -HelperSymbol $HelperMethod `
                        -Reason (
                            "member '$callbackMemberSymbol' is generated, ambient, " +
                            'or outside the trusted MAUI/UIKit observation contract.') `
                        -Callback
                }
            }
        }

        $attachedSymbols =
            [System.Collections.Generic.List[
                Microsoft.CodeAnalysis.ISymbol]]::new()
        $attachedSymbols.Add($windowContentSymbol)
        $activePushedPages =
            [System.Collections.Generic.List[
                Microsoft.CodeAnalysis.ISymbol]]::new()
        foreach ($lifecycleCall in $callbackLifecycleCalls) {
            $lifecycleMethod =
                $semanticModel.GetSymbolInfo($lifecycleCall).Symbol
            $lifecycleDefinition = if ($lifecycleMethod -is
                    [Microsoft.CodeAnalysis.IMethodSymbol] -and
                $null -ne $lifecycleMethod.ReducedFrom) {
                $lifecycleMethod.ReducedFrom
            } else {
                $lifecycleMethod
            }
            $lifecycleKey =
                "$($lifecycleDefinition.ContainingType).$($lifecycleDefinition.Name)"
            $lifecycleMember = $lifecycleCall.Expression
            $navigationReceiver = if ($lifecycleKey.StartsWith(
                    'Microsoft.Maui.Controls.NavigationPage.',
                    [StringComparison]::Ordinal) -and
                $lifecycleMember -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
                $lifecycleMember.Expression
            } elseif ($lifecycleKey.StartsWith(
                    'Microsoft.Maui.Controls.INavigation.',
                    [StringComparison]::Ordinal) -and
                $lifecycleMember -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $lifecycleMember.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $lifecycleMember.Expression.Name.Identifier.ValueText -ceq
                    'Navigation') {
                $navigationProperty = $semanticModel.GetSymbolInfo(
                    $lifecycleMember.Expression).Symbol
                if ($navigationProperty -isnot
                        [Microsoft.CodeAnalysis.IPropertySymbol] -or
                    $navigationProperty.ContainingAssembly.Name -cne
                        'Microsoft.Maui.Controls.ReplicationControlContract' -or
                    $navigationProperty.ContainingType.ToString() -cne
                        'Microsoft.Maui.Controls.NavigationPage') {
                    $null
                } else {
                    $lifecycleMember.Expression.Expression
                }
            } else {
                $null
            }
            $navigationReceiverSymbol = if ($null -ne $navigationReceiver) {
                $semanticModel.GetSymbolInfo($navigationReceiver).Symbol
            } else {
                $null
            }
            if ($navigationReceiver -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
                -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                    $navigationReceiverSymbol,
                    $windowContentSymbol)) {
                & $throwTrustedWindowHelperViolation `
                    -Node $lifecycleCall `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'callback lifecycle transitions must execute directly on ' +
                        'the exact NavigationPage attached by the immutable Window helper.') `
                    -Callback
            }

            if ($lifecycleKey.EndsWith(
                    '.PushAsync',
                    [StringComparison]::Ordinal)) {
                $pushedSymbol = $semanticModel.GetSymbolInfo(
                    $lifecycleCall.ArgumentList.Arguments[0].Expression).Symbol
                if ($pushedSymbol -isnot [Microsoft.CodeAnalysis.ILocalSymbol] -and
                    $pushedSymbol -isnot
                        [Microsoft.CodeAnalysis.IParameterSymbol]) {
                    & $throwTrustedWindowHelperViolation `
                        -Node $lifecycleCall.ArgumentList.Arguments[0].Expression `
                        -HelperSymbol $HelperMethod `
                        -Reason (
                            'PushAsync must receive one directly traceable ' +
                            'selected-test Page local.') `
                        -Callback
                }
                $activePushedPages.Add($pushedSymbol)
            } else {
                if ($activePushedPages.Count -eq 0) {
                    & $throwTrustedWindowHelperViolation `
                        -Node $lifecycleCall `
                        -HelperSymbol $HelperMethod `
                        -Reason (
                            'PopAsync may remove only a Page pushed earlier in ' +
                            'the same trusted callback.') `
                        -Callback
                }
                $activePushedPages.RemoveAt($activePushedPages.Count - 1)
            }
        }
        if ($activePushedPages.Count -ne 0) {
            $attachedSymbols.Add(
                $activePushedPages[$activePushedPages.Count - 1])
        }
        $finalPredicate =
            $lastEventuallyCall[0].ArgumentList.Arguments[0].Expression
        $finalPredicateBody = if ($finalPredicate -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax] -and
            $finalPredicate.Body -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]) {
            $finalPredicate.Body
        } else {
            $null
        }
        $currentPageExpression = $null
        $expectedPageExpression = $null
        if ($finalPredicateBody -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax] -and
            $finalPredicateBody.RawKind -eq
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::EqualsExpression) {
            if ($finalPredicateBody.Left -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $finalPredicateBody.Left.Name.Identifier.ValueText -ceq
                        'CurrentPage') {
                $currentPageExpression = $finalPredicateBody.Left
                $expectedPageExpression = $finalPredicateBody.Right
            } elseif ($finalPredicateBody.Right -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $finalPredicateBody.Right.Name.Identifier.ValueText -ceq
                    'CurrentPage') {
                $currentPageExpression = $finalPredicateBody.Right
                $expectedPageExpression = $finalPredicateBody.Left
            }
        }
        $currentPageProperty = if ($null -ne $currentPageExpression) {
            $semanticModel.GetSymbolInfo($currentPageExpression).Symbol
        } else {
            $null
        }
        $currentNavigationExpression = if ($null -ne $currentPageExpression) {
            $currentPageExpression.Expression
        } else {
            $null
        }
        $currentNavigationSymbol = if ($null -ne
            $currentNavigationExpression) {
            $semanticModel.GetSymbolInfo($currentNavigationExpression).Symbol
        } else {
            $null
        }
        $expectedPageSymbol = if ($null -ne $expectedPageExpression) {
            $semanticModel.GetSymbolInfo($expectedPageExpression).Symbol
        } else {
            $null
        }
        $activeTopPage = if ($activePushedPages.Count -ne 0) {
            $activePushedPages[$activePushedPages.Count - 1]
        } else {
            $null
        }
        if ($currentPageProperty -isnot
                [Microsoft.CodeAnalysis.IPropertySymbol] -or
            $currentPageProperty.ContainingAssembly.Name -cne
                'Microsoft.Maui.Controls.ReplicationControlContract' -or
            $currentPageProperty.ContainingType.ToString() -cne
                'Microsoft.Maui.Controls.NavigationPage' -or
            $currentPageProperty.Name -cne 'CurrentPage' -or
            $currentNavigationExpression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
            -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $currentNavigationSymbol,
                $windowContentSymbol) -or
            $expectedPageExpression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
            $null -eq $activeTopPage -or
            -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $expectedPageSymbol,
                $activeTopPage)) {
            & $throwTrustedWindowHelperViolation `
                -Node $finalPredicate `
                -HelperSymbol $HelperMethod `
                -Reason (
                    'the final AssertEventually predicate must directly prove ' +
                    'that the immutable Window NavigationPage.CurrentPage is ' +
                    'the final Page still pushed by this callback.') `
                -Callback
        }
        foreach ($platformCall in @($callbackBody.DescendantNodes() |
                Where-Object {
                    if ($_ -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
                        return $false
                    }
                    $method = $semanticModel.GetSymbolInfo($_).Symbol
                    $definition = if ($method -is
                            [Microsoft.CodeAnalysis.IMethodSymbol] -and
                        $null -ne $method.ReducedFrom) {
                        $method.ReducedFrom
                    } else {
                        $method
                    }
                    return (
                        $definition -is
                            [Microsoft.CodeAnalysis.IMethodSymbol] -and
                        "$($definition.ContainingType).$($definition.Name)" -ceq
                            'Microsoft.Maui.Platform.ElementExtensions.ToPlatform')
                })) {
            $platformReceiver = if ($platformCall.Expression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
                $platformCall.Expression.Expression
            } else {
                $null
            }
            $platformReceiverSymbol = if ($null -ne $platformReceiver) {
                $semanticModel.GetSymbolInfo($platformReceiver).Symbol
            } else {
                $null
            }
            if ($platformReceiver -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
                $platformCall.SpanStart -le
                    $lastEventuallyCall[0].SpanStart -or
                @($attachedSymbols | Where-Object {
                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                            $_,
                            $platformReceiverSymbol)
                    }).Count -eq 0) {
                & $throwTrustedWindowHelperViolation `
                    -Node $platformCall `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'ToPlatform() may observe only after the final wait and ' +
                        'on the directly attached ' +
                        'Window NavigationPage or a Page directly pushed before ' +
                        'the callback wait; gate-dependent handlers are not trusted.') `
                    -Callback
            }
        }
        foreach ($platformViewRead in @($callbackBody.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $_.Name.Identifier.ValueText -ceq 'PlatformView' -and
                    $_.Expression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $_.Expression.Name.Identifier.ValueText -ceq 'Handler'
                })) {
            $handlerReceiver = $platformViewRead.Expression.Expression
            $handlerReceiverSymbol =
                $semanticModel.GetSymbolInfo($handlerReceiver).Symbol
            if ($handlerReceiver -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
                $platformViewRead.SpanStart -le
                    $lastEventuallyCall[0].SpanStart -or
                @($attachedSymbols | Where-Object {
                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                            $_,
                            $handlerReceiverSymbol)
                    }).Count -eq 0) {
                & $throwTrustedWindowHelperViolation `
                    -Node $platformViewRead `
                    -HelperSymbol $HelperMethod `
                    -Reason (
                        'Handler.PlatformView may observe only after the final ' +
                        'wait and on a Page whose ' +
                        'attachment is independent of the trigger gate.') `
                    -Callback
            }
        }
        $trustedWindowCallbackAttachedSymbols[$callbackBody.SpanStart] =
            @($attachedSymbols)
        $trustedWindowCallbackNavigationRoots[$callbackBody.SpanStart] =
            $windowContentSymbol
        $trustedWindowCallbackBackTitleRoots[$callbackBody.SpanStart] =
            $backTitleTargetSymbol
        $trustedWindowCallbackFinalPages[$callbackBody.SpanStart] =
            $activeTopPage

        $oracleMinimum = $callbackBody.SpanStart
        foreach ($sequencedCall in @($callbackLifecycleCalls) +
            @($callbackEventuallyCalls) +
            @($callbackStateRealizingCalls)) {
            if ($sequencedCall.Span.End -gt $oracleMinimum) {
                $oracleMinimum = $sequencedCall.Span.End
            }
        }
        $trustedWindowCallbackOracleMinimums[$callbackBody.SpanStart] =
            $oracleMinimum
        [void]$trustedWindowCallbackBodies.Add($callbackBody.SpanStart)
        [void]$acceptedTrustedWindowHelperInvocations.Add($Invocation.SpanStart)
    }
    foreach ($awaitExpression in @($testMethod[0].Body.DescendantNodes() |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]
            })) {
        $awaitedExpression = $awaitExpression.Expression
        while ($awaitedExpression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax] -or
            $awaitedExpression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax]) {
            $awaitedExpression = $awaitedExpression.Expression
        }
        $awaitedInfo = if ($awaitedExpression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            $semanticModel.GetSymbolInfo($awaitedExpression)
        } else {
            $null
        }
        $awaitedMethod = if ($null -ne $awaitedInfo) {
            $awaitedInfo.Symbol
        } else {
            $null
        }
        $invokedName = if ($awaitedExpression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            & $getInvocationName -Invocation $awaitedExpression
        } else {
            ''
        }
        $isTrustedWindowHelper =
            $invokedName -ceq 'CreateHandlerAndAddToWindow' -and
            (& $isExactTrustedWindowHelperMethod -Method $awaitedMethod)
        if ($isTrustedWindowHelper) {
            & $validateTrustedWindowHelperInvocation `
                -AwaitExpression $awaitExpression `
                -Invocation $awaitedExpression `
                -HelperMethod $awaitedMethod
            continue
        }
        if ($invokedName -ceq 'CreateHandlerAndAddToWindow') {
            $helperCandidate = if ($null -ne $awaitedMethod) {
                $awaitedMethod
            } elseif ($null -ne $awaitedInfo) {
                @($awaitedInfo.CandidateSymbols | Select-Object -First 1)
            } else {
                $null
            }
            & $throwTrustedWindowHelperViolation `
                -Node $awaitedExpression `
                -HelperSymbol $helperCandidate `
                -Reason (
                    'only the exact protected generic one-arity immutable ' +
                    'ControlsHandlerTestBase.CreateHandlerAndAddToWindow<THandler>(' +
                    'IElement, Func<THandler, Task>) definition constructed with ' +
                    'the external WindowHandlerStub is trusted. Use the ' +
                    'already-proven narrow shape: directly await it as a ' +
                    'top-level statement with new Window(navigationPage), ' +
                    'then use one async block callback containing direct ' +
                    'PushAsync, a pure expression AssertEventually ' +
                    '(CurrentPage == destination), and the native oracle. ' +
                    'Do not use SetupBuilder, OnNavigatedToAsync, ' +
                    'HasNavigatedTo, event subscriptions, GetPlatformToolbar, ' +
                    'or block-bodied AssertEventually predicates.')
        }
        $isTrustedAssertEventually =
            $awaitedMethod -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
            $null -ne $trustedAssertEventuallyMethod -and
            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $awaitedMethod,
                $trustedAssertEventuallyMethod)
        if ($isTrustedAssertEventually) {
            if ($awaitedExpression.ArgumentList.Arguments.Count -ne 1) {
                throw (
                    'The trusted AssertEventually invocation must pass exactly one ' +
                    'pure predicate and use the immutable baseline timeout defaults.')
            }
            $predicate =
                $awaitedExpression.ArgumentList.Arguments[0].Expression
            if ($predicate -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax] -or
                $predicate.AsyncKeyword.RawKind -ne 0 -or
                $predicate.ParameterList.Parameters.Count -ne 0 -or
                $predicate.Body -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax] -or
                $semanticModel.GetTypeInfo($predicate).ConvertedType -isnot
                    [Microsoft.CodeAnalysis.INamedTypeSymbol] -or
                $semanticModel.GetTypeInfo(
                    $predicate).ConvertedType.Name -cne 'Func' -or
                $semanticModel.GetTypeInfo(
                    $predicate).ConvertedType.ContainingNamespace.ToString() -cne
                    'System' -or
                $semanticModel.GetTypeInfo(
                    $predicate).ConvertedType.TypeArguments.Length -ne 1 -or
                $semanticModel.GetTypeInfo(
                    $predicate).ConvertedType.TypeArguments[0].SpecialType -ne
                    [Microsoft.CodeAnalysis.SpecialType]::System_Boolean) {
                throw (
                    'The trusted AssertEventually invocation requires one ' +
                    'non-async, parameterless expression lambda of type Func<bool>.')
            }
            & $validateEventuallyPredicateExpression `
                -Expression $predicate.Body
            [void]$acceptedAssertEventuallyInvocations.Add(
                $awaitedExpression.SpanStart)
            continue
        }
        $isTrustedExternalAwait =
            $awaitedMethod -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
            $awaitedMethod.ContainingAssembly.Name -ceq
                'Microsoft.Maui.Controls.ReplicationControlContract' -and
            $awaitedMethod.ReturnType -is
                [Microsoft.CodeAnalysis.INamedTypeSymbol] -and
            $awaitedMethod.ReturnType.Name -ceq 'Task' -and
            $awaitedMethod.ReturnType.ContainingNamespace.ToString() -ceq
                'System.Threading.Tasks' -and
            @($awaitedMethod.Locations | Where-Object {
                    $_.IsInSource
                }).Count -eq 0
        if (-not $isTrustedExternalAwait) {
            $awaitLine = $tree.GetLineSpan(
                $awaitExpression.Span).StartLinePosition.Line + 1
            $awaitedSymbol = if ($awaitedMethod) {
                $awaitedMethod
            } elseif ($null -ne $awaitedInfo) {
                @($awaitedInfo.CandidateSymbols | Select-Object -First 1)
            } else {
                $null
            }
            $awaitedSymbolText = if ($awaitedSymbol) {
                [string]$awaitedSymbol
            } else {
                '<unresolved>'
            }
            if ($awaitedSymbolText.Length -gt 200) {
                $awaitedSymbolText =
                    $awaitedSymbolText.Substring(0, 200) + '...'
            }
            $awaitedSymbolLocation = @(
                if ($awaitedSymbol) {
                    $awaitedSymbol.Locations |
                        Where-Object { $_.IsInSource } |
                        Select-Object -First 1
                })
            if ($awaitedSymbolLocation.Count -eq 1) {
                $awaitedSymbolFile =
                    [string]$awaitedSymbolLocation[0].SourceTree.FilePath
                if ([string]::IsNullOrWhiteSpace($awaitedSymbolFile)) {
                    $awaitedSymbolFile = '<generated source with no path>'
                }
                $awaitedSymbolLine =
                    $awaitedSymbolLocation[0].GetLineSpan(
                    ).StartLinePosition.Line + 1
            } else {
                $awaitedAssembly = if ($awaitedSymbol -and
                    $awaitedSymbol.ContainingAssembly) {
                    $awaitedSymbol.ContainingAssembly.Name
                } else {
                    'unresolved'
                }
                $awaitedSymbolFile = "<metadata:$awaitedAssembly>"
                $awaitedSymbolLine = 0
            }
            if ($awaitedSymbolFile.Length -gt 260) {
                $awaitedSymbolFile =
                    '...' + $awaitedSymbolFile.Substring(
                        $awaitedSymbolFile.Length - 257)
            }
            throw (
                "Generated tests may await only a direct trusted external " +
                'framework invocation or the exact immutable ' +
                'Microsoft.Maui.DeviceTests.AssertHelpers.AssertEventually(' +
                'Func<bool>, int, int, string) helper. Awaited symbol ' +
                "'$awaitedSymbolText' declared in '$awaitedSymbolFile' line " +
                "$awaitedSymbolLine; offending await in '$SourcePath' line " +
                "$awaitLine.")
        }
    }
    $isAcceptedAssertEventuallyNode = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.SyntaxNode]$Node
        )
        $invocation = if ($Node -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            $Node
        } else {
            @($Node.Ancestors() | Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
                } | Select-Object -First 1)
        }
        return (
            $null -ne $invocation -and
            $acceptedAssertEventuallyInvocations.Contains(
                $invocation.SpanStart))
    }
    $isAcceptedTrustedWindowHelperNode = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.SyntaxNode]$Node
        )
        $invocation = if ($Node -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            $Node
        } else {
            @($Node.Ancestors() | Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
                } | Select-Object -First 1)
        }
        return (
            $null -ne $invocation -and
            $acceptedTrustedWindowHelperInvocations.Contains(
                $invocation.SpanStart))
    }
    $isTrustedWindowCallbackNode = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.SyntaxNode]$Node
        )
        return @($Node.AncestorsAndSelf() | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax] -and
                $trustedWindowCallbackBodies.Contains($_.SpanStart)
            }).Count -ne 0
    }
    $isAcceptedTrustedBackTitleSetupNode = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.SyntaxNode]$Node
        )
        $invocation = if ($Node -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            $Node
        } else {
            @($Node.Ancestors() | Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
                } | Select-Object -First 1)
        }
        return (
            $null -ne $invocation -and
            $acceptedTrustedBackTitleSetups.Contains(
                $invocation.SpanStart))
    }
    $aliasOrDeconstruction = @($testMethod[0].Body.DescendantNodes() |
        Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.RefExpressionSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.RefTypeSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationExpressionSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax] -or
            ($_.Parent -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                $_.Parent.Left -eq $_ -and
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.TupleExpressionSyntax]) -or
            ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax] -and
                $_.RefKindKeyword.RawKind -ne 0)
        })
    if ($aliasOrDeconstruction.Count -ne 0) {
        throw (
            'The selected test method may not use ref aliases, ref/out/in, or ' +
            'deconstruction; trusted dataflow must remain directly traceable.')
    }
    if (@($testMethod[0].Body.DescendantNodes() | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax] -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseExpressionSyntax]
            }).Count -ne 0) {
        throw (
            'The selected test method may not use this or base because virtual ' +
            'dispatch could execute generated overrides.')
    }
    $isTrustedClosedType = {
        param(
            [AllowNull()]
            [Microsoft.CodeAnalysis.ITypeSymbol]$Type
        )
        if ($null -eq $Type -or
            $Type.TypeKind -eq [Microsoft.CodeAnalysis.TypeKind]::Error -or
            $Type.TypeKind -eq [Microsoft.CodeAnalysis.TypeKind]::Pointer -or
            @($Type.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            return $false
        }
        if ($Type -is [Microsoft.CodeAnalysis.IArrayTypeSymbol]) {
            return & $isTrustedClosedType -Type $Type.ElementType
        }
        if ($Type -is [Microsoft.CodeAnalysis.INamedTypeSymbol]) {
            foreach ($typeArgument in $Type.TypeArguments) {
                if (-not (& $isTrustedClosedType -Type $typeArgument)) {
                    return $false
                }
            }
        }
        return $true
    }
    $sourceTypedLocals = @($testMethod[0].Body.DescendantNodes() |
        Where-Object {
            if ($_ -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax]) {
                return $false
            }
            $local = $semanticModel.GetDeclaredSymbol($_)
            return (
                $local -is [Microsoft.CodeAnalysis.ILocalSymbol] -and
                -not (& $isTrustedClosedType -Type $local.Type))
        })
    if ($sourceTypedLocals.Count -ne 0) {
        throw (
            'The selected test method may not instantiate or carry generated ' +
            'runtime types through its control or oracle.')
    }
    $typeSyntaxCandidates = @($testMethod[0].Body.DescendantNodes() |
        ForEach-Object {
            if ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax]) {
                $_.Type
            } elseif ($_ -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax]) {
                $_.Type
            } elseif ($_ -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.DefaultExpressionSyntax]) {
                $_.Type
            } elseif ($_ -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax]) {
                $_.Type
            } elseif ($_ -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationExpressionSyntax]) {
                $_.Type
            }
        } | Where-Object { $null -ne $_ } | Select-Object -Unique)
    $untrustedTypeSyntax = @($typeSyntaxCandidates | Where-Object {
            if ($_.ToString() -cin @('nfloat', 'nint', 'nuint')) {
                return $false
            }
            $type = $semanticModel.GetTypeInfo($_).Type
            if ($null -eq $type) {
                $candidateType = $semanticModel.GetSymbolInfo($_).Symbol
                if ($candidateType -is [Microsoft.CodeAnalysis.ITypeSymbol]) {
                    $type = $candidateType
                }
            }
            return (
                -not (& $isTrustedClosedType -Type $type))
        })
    if ($untrustedTypeSyntax.Count -ne 0) {
        $typeLine = $tree.GetLineSpan(
            $untrustedTypeSyntax[0].Span).StartLinePosition.Line + 1
        throw (
            "Type '$($untrustedTypeSyntax[0])' is generated or unresolved in " +
            "'$SourcePath' line $typeLine; control and oracle dataflow require " +
            'trusted external types.')
    }
    $ambientSymbols = @($testMethod[0].Body.DescendantNodes() |
        Where-Object {
            if ($_ -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]) {
                return $false
            }
            $symbol = $semanticModel.GetSymbolInfo($_).Symbol
            if ($symbol -isnot [Microsoft.CodeAnalysis.IPropertySymbol] -and
                $symbol -isnot [Microsoft.CodeAnalysis.IFieldSymbol] -and
                $symbol -isnot [Microsoft.CodeAnalysis.IMethodSymbol]) {
                return $false
            }
            $typeName = $symbol.ContainingType.ToString()
            return (
                $typeName -cin @(
                    'System.DateTime',
                    'System.DateTimeOffset',
                    'System.Environment',
                    'System.Random',
                    'System.Guid',
                    'System.Diagnostics.Stopwatch'
                ))
        })
    if ($ambientSymbols.Count -ne 0) {
        throw (
            'A trusted negative control may not use ambient clock, random, ' +
            'environment, or process-time state.')
    }
    $throwStatements = @($testMethod[0].Body.DescendantNodes() |
        Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ThrowExpressionSyntax]
        })
    if ($throwStatements.Count -ne 0) {
        $throwLine = $tree.GetLineSpan(
            $throwStatements[0].Span).StartLinePosition.Line + 1
        throw (
            "The selected test method may not throw before its trusted oracle; " +
            "offending syntax in '$SourcePath' line $throwLine.")
    }
    $sourceExecutableSymbols = @($testMethod[0].Body.DescendantNodes() |
        Where-Object {
            if ($_ -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]) {
                return $false
            }
            if ($_ -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax]) {
                return $false
            }
            if (& $isAcceptedAssertEventuallyNode -Node $_) {
                return $false
            }
            $symbolInfo = $semanticModel.GetSymbolInfo($_)
            $symbols = @($symbolInfo.Symbol) + @($symbolInfo.CandidateSymbols)
            return @($symbols | Where-Object {
                    ($_ -is [Microsoft.CodeAnalysis.IMethodSymbol] -or
                        $_ -is [Microsoft.CodeAnalysis.IPropertySymbol] -or
                        $_ -is [Microsoft.CodeAnalysis.IFieldSymbol] -or
                        $_ -is [Microsoft.CodeAnalysis.IEventSymbol]) -and
                    @($_.Locations | Where-Object {
                            $_.IsInSource
                        }).Count -ne 0
                }).Count -ne 0
        })
    if ($sourceExecutableSymbols.Count -ne 0) {
        $offendingNode = $sourceExecutableSymbols[0]
        $offendingInfo = $semanticModel.GetSymbolInfo($offendingNode)
        $offendingSymbol = if ($offendingInfo.Symbol) {
            $offendingInfo.Symbol
        } else {
            @($offendingInfo.CandidateSymbols | Select-Object -First 1)
        }
        $offendingKind = if ($offendingSymbol) {
            [string]$offendingSymbol.Kind
        } else {
            'unresolved'
        }
        $offendingLine = $tree.GetLineSpan(
            $offendingNode.Span).StartLinePosition.Line + 1
        $offendingText = $offendingNode.ToString()
        if ($offendingText.Length -gt 160) {
            $offendingText = $offendingText.Substring(0, 160) + '...'
        }
        throw (
            'The selected test method may not execute generated constructors, ' +
            'method groups, properties, indexers, fields, events, or operators. ' +
            "Offending $offendingKind symbol '$offendingSymbol' from syntax " +
            "'$offendingText' in '$SourcePath' line $offendingLine.")
    }
    $trustedContractAssembly =
        'Microsoft.Maui.Controls.ReplicationControlContract'
    $assertAliasDeclared = @($root.Usings | Where-Object {
            $null -ne $_.Alias -and
            $_.Alias.Name.Identifier.ValueText -ceq 'Assert'
        }).Count -ne 0
    foreach ($genericName in @($testMethod[0].Body.DescendantNodes() |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax]
            })) {
        foreach ($typeArgument in $genericName.TypeArgumentList.Arguments) {
            $typeSymbol = $semanticModel.GetTypeInfo($typeArgument).Type
            $isTrustedType = & $isTrustedClosedType -Type $typeSymbol
            if (-not $isTrustedType -or
                ($genericName.Identifier.ValueText -ceq 'AddHandler' -and
                    $typeSymbol.ContainingAssembly.Name -cne
                        $trustedContractAssembly)) {
                $typeLine = $tree.GetLineSpan(
                    $typeArgument.Span).StartLinePosition.Line + 1
                throw (
                    "Generic type argument '$typeArgument' is not a trusted " +
                    "external framework type in '$SourcePath' line $typeLine.")
            }
        }
    }
    foreach ($operationNode in @($testMethod[0].Body.DescendantNodes() |
            Where-Object {
                -not $gate.Span.Contains($_.Span) -and
                ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -or
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -or
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberBindingExpressionSyntax] -or
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax] -or
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitObjectCreationExpressionSyntax] -or
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax])
            })) {
        if (& $isAcceptedAssertEventuallyNode -Node $operationNode) {
            continue
        }
        if (& $isAcceptedTrustedWindowHelperNode -Node $operationNode) {
            continue
        }
        if (& $isTrustedWindowCallbackNode -Node $operationNode) {
            continue
        }
        if (& $isAcceptedTrustedBackTitleSetupNode -Node $operationNode) {
            continue
        }
        $operationText = $operationNode.ToString()
        $operationInfo = $semanticModel.GetSymbolInfo($operationNode)
        $precheckedSymbol = if ($operationInfo.Symbol) {
            $operationInfo.Symbol
        } else {
            @($operationInfo.CandidateSymbols | Select-Object -First 1)
        }
        if ($operationText -cmatch
                '^(?:Assert|ClassicAssert|CollectionAssert|StringAssert)\s*\.' -and
            (($precheckedSymbol -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
                    $precheckedSymbol.ContainingAssembly.Name -ceq
                        $trustedContractAssembly -and
                    $precheckedSymbol.ContainingType.ToString() -cin @(
                        'Xunit.Assert',
                        'NUnit.Framework.Assert',
                        'NUnit.Framework.ClassicAssert',
                        'Microsoft.VisualStudio.TestTools.UnitTesting.Assert',
                        'Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert',
                        'Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert'
                    )) -or
                ($null -eq $precheckedSymbol -and -not $assertAliasDeclared))) {
            # Assertion invocations are checked against the trusted test
            # framework contract below, after trigger-shape diagnostics.
            continue
        }
        $operationSymbol = $precheckedSymbol
        if ($operationNode -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax]) {
            $indexedType = $semanticModel.GetTypeInfo(
                $operationNode.Expression).Type
            if ($indexedType -is [Microsoft.CodeAnalysis.IArrayTypeSymbol] -and
                $indexedType.ElementType.ContainingAssembly.Name -ceq
                    $trustedContractAssembly -and
                @($indexedType.ElementType.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -eq 0) {
                continue
            }
        }
        if ($operationSymbol -and
            ($operationSymbol -is [Microsoft.CodeAnalysis.INamespaceSymbol] -or
                $operationSymbol -is [Microsoft.CodeAnalysis.INamedTypeSymbol])) {
            continue
        }
        $operationAssembly = if ($operationSymbol -and
            $operationSymbol.ContainingAssembly) {
            $operationSymbol.ContainingAssembly.Name
        } else {
            ''
        }
        if ($operationAssembly -ceq $trustedContractAssembly) {
            if ($operationSymbol -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
                $operationSymbol.MethodKind -ne
                    [Microsoft.CodeAnalysis.MethodKind]::Constructor) {
                $operationKey =
                    "$($operationSymbol.ContainingType).$($operationSymbol.Name)"
                $operationDefinition = if ($null -ne
                    $operationSymbol.ReducedFrom) {
                    $operationSymbol.ReducedFrom
                } else {
                    $operationSymbol
                }
                $operationDefinitionKey =
                    "$($operationDefinition.ContainingType).$($operationDefinition.Name)"
                if ($operationNode -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
                    $operationDefinitionKey -ceq
                        'Microsoft.Maui.Platform.ElementExtensions.ToPlatform') {
                    $isExactPureToPlatform =
                        $null -ne $operationSymbol.ReducedFrom -and
                        $operationNode.ArgumentList.Arguments.Count -eq 0 -and
                        $operationDefinition.IsStatic -and
                        $operationDefinition.IsExtensionMethod -and
                        $operationDefinition.Arity -eq 0 -and
                        $operationDefinition.Parameters.Length -eq 1 -and
                        $operationDefinition.Parameters[0].Type.ToString() -ceq
                            'Microsoft.Maui.Controls.Element' -and
                        $operationDefinition.ReturnType.ToString() -ceq
                            'UIKit.UIView'
                    if (-not $isExactPureToPlatform) {
                        $line = $tree.GetLineSpan(
                            $operationNode.Span).StartLinePosition.Line + 1
                        throw (
                            "Trusted framework call '$operationSymbol' is not " +
                            'the exact side-effect-free ToPlatform() observation ' +
                            "overload for '$SourcePath' line $line.")
                    }
                }
                if ($operationNode -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
                    $operationKey -ceq
                        'Microsoft.Maui.Platform.ViewExtensions.FindDescendantView' -and
                    @($operationNode.ArgumentList.DescendantNodes() |
                        Where-Object {
                            $_ -is
                                [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                            $_.RawKind -in @(
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression)
                        }).Count -ne 0) {
                    throw (
                        'Trusted descendant-observation predicates may not write ' +
                        'state or increment values.')
                }
                $allowedContractCall = $operationKey -cin @(
                    'CoreGraphics.CGRect.Intersect',
                    'Microsoft.Maui.DeviceTests.ControlsHandlerTestBase.EnsureHandlerCreated',
                    'Microsoft.Maui.Hosting.HandlerBuilder.ConfigureMauiHandlers',
                    'Microsoft.Maui.Hosting.HandlerCollection.AddHandler',
                    'Microsoft.Maui.Platform.ElementExtensions.ToPlatform',
                    'Microsoft.Maui.Platform.ViewExtensions.FindDescendantView',
                    'Microsoft.Maui.DeviceTests.AssertionExtensions.GetBackButton',
                    'NUnit.Framework.ConstraintExpression.EqualTo',
                    'NUnit.Framework.ConstraintExpression.GreaterThan',
                    'NUnit.Framework.ConstraintExpression.LessThan',
                    'NUnit.Framework.ConstraintExpression.SameAs',
                    'NUnit.Framework.Is.EqualTo',
                    'NUnit.Framework.Is.GreaterThan',
                    'NUnit.Framework.Is.LessThan',
                    'NUnit.Framework.Is.SameAs',
                    'UIKit.UIView.ConvertRectToView',
                    'UIKit.UINavigationBar.LayoutIfNeeded'
                )
                if (-not $allowedContractCall) {
                    $line = $tree.GetLineSpan(
                        $operationNode.Span).StartLinePosition.Line + 1
                    throw (
                        "Trusted framework call '$operationKey' is not in the " +
                        "closed lifecycle/observation allowlist outside the " +
                        "reported-trigger gate for '$SourcePath' line $line.")
                }
            }
            continue
        }
        $allowedPureOperation =
            $operationSymbol -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
            (($operationSymbol.ContainingType.ToString() -ceq 'System.Math' -and
                    $operationSymbol.Name -ceq 'Abs') -or
                ($operationSymbol.ContainingType.ToString() -ceq
                    'System.Linq.Enumerable' -and
                    @($operationSymbol.Parameters | Where-Object {
                            $_.Type.TypeKind -eq
                                [Microsoft.CodeAnalysis.TypeKind]::Delegate
                        }).Count -eq 0 -and
                    $operationSymbol.Name -cin @(
                        'First',
                        'OfType'
                    )))
        if ($allowedPureOperation) {
            continue
        }
        $line = $tree.GetLineSpan(
            $operationNode.Span).StartLinePosition.Line + 1
        $text = $operationText
        if ($text.Length -gt 160) {
            $text = $text.Substring(0, 160) + '...'
        }
        $kind = if ($operationSymbol) {
            [string]$operationSymbol.Kind
        } else {
            'unresolved'
        }
        throw (
            "External operation '$operationSymbol' ($kind) from syntax '$text' " +
            "is not in the trusted semantic MAUI/UIKit contract for " +
            "'$SourcePath' line $line.")
    }
    if ($null -eq $testMethod[0].Body -or
        $localDeclaration.Parent -ne $testMethod[0].Body -or
        $gate.Parent -ne $testMethod[0].Body -or
        $localDeclaration.SpanStart -ge $gate.SpanStart) {
        throw (
            'The trusted control declaration and gate must be ordered top-level ' +
            'statements in the executable body of the selected test method.')
    }
    $declaredGateSymbol = $semanticModel.GetDeclaredSymbol($declarator)
    $conditionGateSymbol = $semanticModel.GetSymbolInfo($references[0]).Symbol
    if ($null -eq $declaredGateSymbol -or
        $null -eq $conditionGateSymbol -or
        -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
            $declaredGateSymbol,
            $conditionGateSymbol)) {
        throw (
            'The trusted control condition must bind to the exact local gate ' +
            'declared in the selected test method.')
    }
    $gateAssertions = @(Get-ReplicationAssertionStatements `
        -Source $gate.Statement.ToFullString())
    if ($gateAssertions.Count -ne 0) {
        throw 'The applyReportedTrigger block must not contain an assertion.'
    }
    $methodAssertions = @(Get-ReplicationAssertionStatements `
        -Source $testMethod[0].ToFullString())
    if ($methodAssertions.Count -eq 0) {
        throw 'The selected test method has no oracle outside its trigger gate.'
    }
    $directiveOrDisabledTrivia = @($gate.DescendantTrivia(
            [System.Func[Microsoft.CodeAnalysis.SyntaxNode, bool]]$null,
            $true) | Where-Object {
            $_.GetStructure() -or
            $_.RawKind -eq [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::DisabledTextTrivia -or
            $_.RawKind -in @(
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::SingleLineCommentTrivia,
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::MultiLineCommentTrivia)
        })
    if ($directiveOrDisabledTrivia.Count -ne 0) {
        throw (
            'The trusted control gate must not contain directives, disabled ' +
            'preprocessor code, or comments.')
    }
    $declaredTypes = @($root.DescendantNodes() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax]
        } | ForEach-Object { $_.Identifier.ValueText })
    $declaredValues = @($root.DescendantNodes() | ForEach-Object {
            if ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax]) {
                $_.Identifier.ValueText
            } elseif ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax]) {
                $_.Identifier.ValueText
            } elseif ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax]) {
                $_.Identifier.ValueText
            } elseif ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax]) {
                $_.Identifier.ValueText
            } elseif ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax] -and
                $null -ne $_.Alias) {
                $_.Alias.Name.Identifier.ValueText
            }
        } | Where-Object { $_ })
    foreach ($reservedAttribute in @(
        'Fact',
        'FactAttribute',
        'Theory',
        'TheoryAttribute',
        'Test',
        'TestAttribute'
    )) {
        if ($declaredTypes -ccontains $reservedAttribute -or
            $declaredValues -ccontains $reservedAttribute) {
            throw 'The generated test may not shadow a trusted test attribute name.'
        }
    }

    $assertStableOperationLocal = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.ISymbol]$Symbol
        )
        if ($Symbol -isnot [Microsoft.CodeAnalysis.ILocalSymbol]) {
            return
        }
        $writes = @($root.DescendantNodes() | Where-Object {
                if ($_ -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                    return $false
                }
                $candidate = $semanticModel.GetSymbolInfo($_).Symbol
                if ($null -eq $candidate -or
                    -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                        $candidate,
                        $Symbol)) {
                    return $false
                }
                return (
                    ($_.Parent -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                        $_.Parent.Left -eq $_) -or
                    $_.Parent.RawKind -in @(
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression) -or
                    ($_.Parent -is [Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax] -and
                        $_.Parent.RefKindKeyword.RawKind -ne 0))
            })
        if ($writes.Count -ne 0) {
            throw (
                "The framework-operation receiver '$($Symbol.Name)' may not be " +
                'reassigned, incremented, or passed by reference.')
        }
    }
    $validateFrameworkOperation = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression,
            [Parameter(Mandatory = $true)][string]$Description
        )
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]) {
            $Expression = $Expression.Expression
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            if ($Expression.Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
                throw "$Description must be a direct framework member invocation."
            }
            $memberAccess = $Expression.Expression
            if (@($Expression.ArgumentList.DescendantNodes() | Where-Object {
                        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -or
                        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax] -or
                        $_.RawKind -in @(
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                            [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression)
                    }).Count -ne 0) {
                throw "$Description arguments may not execute calls, writes, or lambdas."
            }
            $method = $semanticModel.GetSymbolInfo($Expression).Symbol
            if ($method -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
                $method.MethodKind -ne [Microsoft.CodeAnalysis.MethodKind]::Ordinary -or
                $method.ContainingAssembly.Name -cne $trustedContractAssembly -or
                -not $method.ContainingType.ToString().StartsWith(
                    'Microsoft.Maui.Controls.',
                    [StringComparison]::Ordinal) -or
                @($method.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -ne 0) {
                throw "$Description must resolve to a trusted external MAUI method."
            }
            foreach ($argument in $Expression.ArgumentList.Arguments) {
                if ($argument.RefKindKeyword.RawKind -ne 0) {
                    throw "$Description may not use ref, out, or in arguments."
                }
                Confirm-ReplicationTrustedOracleExpression `
                    -Expression $argument.Expression `
                    -SemanticModel $semanticModel `
                    -Root $root
            }
            $relatedSymbols = [System.Collections.Generic.List[
                Microsoft.CodeAnalysis.ISymbol]]::new()
            foreach ($argument in $Expression.ArgumentList.Arguments) {
                $argumentSymbol =
                    $semanticModel.GetSymbolInfo($argument.Expression).Symbol
                if ($argumentSymbol -is [Microsoft.CodeAnalysis.ILocalSymbol] -or
                    $argumentSymbol -is
                        [Microsoft.CodeAnalysis.IParameterSymbol]) {
                    $relatedSymbols.Add($argumentSymbol)
                }
            }
            $affectedSymbol = $null
            $stateFamily = $method.Name
            $resourceKey = $null
            if ($method.IsStatic) {
                $receiverText = $memberAccess.Expression.ToString()
                if ($receiverText -cnotmatch
                    '^global::Microsoft\.Maui\.Controls\.[A-Za-z_][A-Za-z0-9_]*$') {
                    throw "$Description static receiver must be an exact global-qualified MAUI type."
                }
                if ($Expression.ArgumentList.Arguments.Count -gt 0) {
                    $first = $Expression.ArgumentList.Arguments[0].Expression
                    if ($first -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                        $candidate = $semanticModel.GetSymbolInfo($first).Symbol
                        if ($candidate -is [Microsoft.CodeAnalysis.ILocalSymbol] -or
                            $candidate -is [Microsoft.CodeAnalysis.IParameterSymbol]) {
                            $affectedSymbol = $candidate
                            & $assertStableOperationLocal -Symbol $affectedSymbol
                        }
                    }
                }
            } else {
                if ($memberAccess.Expression -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                    throw "$Description instance receiver must be one selected-method local or parameter."
                }
                $affectedSymbol =
                    $semanticModel.GetSymbolInfo($memberAccess.Expression).Symbol
                if ($affectedSymbol -isnot [Microsoft.CodeAnalysis.ILocalSymbol] -and
                    $affectedSymbol -isnot [Microsoft.CodeAnalysis.IParameterSymbol]) {
                    throw "$Description instance receiver must bind to a selected-method local or parameter."
                }
                $receiverType = $semanticModel.GetTypeInfo(
                    $memberAccess.Expression).Type
                if ($null -eq $receiverType -or
                    $receiverType.TypeKind -eq [Microsoft.CodeAnalysis.TypeKind]::Error -or
                    $receiverType.ContainingAssembly.Name -cne $trustedContractAssembly -or
                    @($receiverType.Locations | Where-Object {
                            $_.IsInSource
                        }).Count -ne 0) {
                    throw "$Description instance receiver must have a trusted external framework type."
                }
                & $assertStableOperationLocal -Symbol $affectedSymbol
                if ($method.Name -cin @('SetDynamicResource', 'SetValue') -and
                    $Expression.ArgumentList.Arguments.Count -gt 0) {
                    $propertyArgument =
                        $Expression.ArgumentList.Arguments[0].Expression
                    $propertySymbol =
                        $semanticModel.GetSymbolInfo($propertyArgument).Symbol
                    if ($propertySymbol -isnot
                            [Microsoft.CodeAnalysis.IPropertySymbol] -and
                        $propertySymbol -isnot
                            [Microsoft.CodeAnalysis.IFieldSymbol]) {
                        throw (
                            "$Description must name the affected trusted " +
                            'BindableProperty directly.')
                    }
                    $stateFamily = $propertySymbol.Name
                    if ($stateFamily.EndsWith(
                            'Property',
                            [StringComparison]::Ordinal)) {
                        $stateFamily = $stateFamily.Substring(
                            0,
                            $stateFamily.Length - 'Property'.Length)
                    }
                    if ($method.Name -ceq 'SetDynamicResource' -and
                        $Expression.ArgumentList.Arguments.Count -ge 2) {
                        $keyExpression =
                            $Expression.ArgumentList.Arguments[1].Expression
                        if ($keyExpression -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
                            $resourceKey = $keyExpression.Token.ValueText
                        }
                    }
                } elseif ($method.Name.StartsWith(
                        'Set',
                        [StringComparison]::Ordinal)) {
                    $stateFamily = $method.Name.Substring(3)
                }
            }
            if ($stateFamily -ceq 'BackgroundColor') {
                $stateFamily = 'Background'
            }
            if ($null -ne $affectedSymbol -and
                @($relatedSymbols | Where-Object {
                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                            $_,
                            $affectedSymbol)
                    }).Count -eq 0) {
                $relatedSymbols.Add($affectedSymbol)
            }
            return [pscustomobject]@{
                AffectedSymbol = $affectedSymbol
                Kind = 'invocation'
                StateFamily = $stateFamily
                MethodName = $method.Name
                ResourceKey = $resourceKey
                AssignedValueSymbol = $null
                RelatedSymbols = @($relatedSymbols)
            }
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
            if ($Expression.RawKind -ne
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::SimpleAssignmentExpression -or
                $Expression.Left -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -or
                $Expression.Left.Expression -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                throw "$Description must be one direct framework property assignment."
            }
            $property = $semanticModel.GetSymbolInfo($Expression.Left).Symbol
            $affectedSymbol =
                $semanticModel.GetSymbolInfo($Expression.Left.Expression).Symbol
            if ($property -isnot [Microsoft.CodeAnalysis.IPropertySymbol] -or
                $null -eq $property.SetMethod -or
                $property.ContainingAssembly.Name -cne $trustedContractAssembly -or
                -not $property.ContainingType.ToString().StartsWith(
                    'Microsoft.Maui.Controls.',
                    [StringComparison]::Ordinal) -or
                @($property.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -ne 0 -or
                ($affectedSymbol -isnot [Microsoft.CodeAnalysis.ILocalSymbol] -and
                    $affectedSymbol -isnot [Microsoft.CodeAnalysis.IParameterSymbol])) {
                throw "$Description must resolve to a trusted external MAUI property on a test-local receiver."
            }
            & $assertStableOperationLocal -Symbol $affectedSymbol
            if (@($Expression.Right.DescendantNodesAndSelf() | Where-Object {
                        $_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]
                    }).Count -ne 0) {
                throw "$Description right-hand side may not execute calls."
            }
            Confirm-ReplicationTrustedOracleExpression `
                -Expression $Expression.Right `
                -SemanticModel $semanticModel `
                -Root $root
            $stateFamily = $property.Name
            if ($stateFamily -ceq 'BackgroundColor') {
                $stateFamily = 'Background'
            }
            return [pscustomobject]@{
                AffectedSymbol = $affectedSymbol
                Kind = 'assignment'
                StateFamily = $stateFamily
                MethodName = $null
                ResourceKey = $null
                AssignedValueSymbol = if ($Expression.Right -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                    $semanticModel.GetSymbolInfo($Expression.Right).Symbol
                } else {
                    $null
                }
                RelatedSymbols = @($affectedSymbol)
            }
        }
        throw "$Description must be one direct trusted framework invocation or property assignment."
    }
    $triggerOperation = & $validateFrameworkOperation `
        -Expression $gate.Statement.Statements[0].Expression `
        -Description 'The reported-trigger branch'
    if ($gate.Else) {
        $alternateOperation = & $validateFrameworkOperation `
            -Expression $gate.Else.Statement.Statements[0].Expression `
            -Description 'The alternate-action branch'
        if ($alternateOperation.Kind -cne 'assignment') {
            throw (
                'The optional alternate action must be one direct trusted ' +
                'framework property assignment.')
        }
        if ($triggerOperation.MethodName -cne 'SetDynamicResource') {
            throw (
                'An optional alternate action is accepted only for a trusted ' +
                'SetDynamicResource trigger with closed resource-value provenance.')
        }
        if ($null -eq $triggerOperation.AffectedSymbol -or
            $null -eq $alternateOperation.AffectedSymbol -or
            -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $triggerOperation.AffectedSymbol,
                $alternateOperation.AffectedSymbol)) {
            throw (
                'The optional alternate action must operate on the same selected-' +
                'method affected-state local as the reported trigger.')
        }
        if ($triggerOperation.StateFamily -cne
            $alternateOperation.StateFamily) {
            throw (
                "The optional alternate action changes '$(
                    $alternateOperation.StateFamily)' while the reported trigger " +
                "changes '$($triggerOperation.StateFamily)'; both branches must " +
                'establish the same logical framework state.')
        }
        if ($triggerOperation.MethodName -ceq 'SetDynamicResource' -and
            $alternateOperation.Kind -ceq 'assignment') {
            if ([string]::IsNullOrWhiteSpace(
                    [string]$triggerOperation.ResourceKey) -or
                $alternateOperation.AssignedValueSymbol -isnot
                    [Microsoft.CodeAnalysis.ILocalSymbol]) {
                throw (
                    'A SetDynamicResource/direct-assignment alternate requires ' +
                    'one literal resource key and one trusted local alternate value.')
            }
            $unwrapResourceExpression = {
                param(
                    [Parameter(Mandatory = $true)]
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
                )
                while ($Expression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax] -or
                    $Expression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax]) {
                    $Expression = $Expression.Expression
                }
                return $Expression
            }
            $resourceAliases =
                [System.Collections.Generic.List[Microsoft.CodeAnalysis.ISymbol]]::new()
            $allResourceWrites = @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    if ($_ -isnot
                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                        $_.Left -isnot
                            [Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax]) {
                        return $false
                    }
                    $resourceType = $semanticModel.GetTypeInfo(
                        $_.Left.Expression).Type
                    return (
                        $null -ne $resourceType -and
                        $resourceType.ContainingAssembly.Name -ceq
                            $trustedContractAssembly -and
                        $resourceType.ToString() -ceq
                            'Microsoft.Maui.Controls.ResourceDictionary')
                })
            foreach ($resourceWrite in $allResourceWrites) {
                $resourceKey =
                    $resourceWrite.Left.ArgumentList.Arguments[0].Expression
                if ($resourceKey -isnot
                    [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
                    throw (
                        'Every ResourceDictionary write in a controlled test must ' +
                        'use a literal key.')
                }
            }
            $targetResourceWrites = @($allResourceWrites | Where-Object {
                    $_.Left.ArgumentList.Arguments[0].Expression.Token.ValueText -ceq
                        $triggerOperation.ResourceKey
                })
            $isAffectedResourceExpression = {
                param(
                    [Parameter(Mandatory = $true)]
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
                )
                $Expression = & $unwrapResourceExpression -Expression $Expression
                if ($Expression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $Expression.Name.Identifier.ValueText -ceq 'Resources') {
                    $receiver =
                        $semanticModel.GetSymbolInfo($Expression.Expression).Symbol
                    return [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                        $receiver,
                        $triggerOperation.AffectedSymbol)
                }
                if ($Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                    $symbol = $semanticModel.GetSymbolInfo($Expression).Symbol
                    return @($resourceAliases | Where-Object {
                            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                $_,
                                $symbol)
                        }).Count -ne 0
                }
                if ($Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalExpressionSyntax]) {
                    return (
                        (& $isAffectedResourceExpression `
                            -Expression $Expression.WhenTrue) -or
                        (& $isAffectedResourceExpression `
                            -Expression $Expression.WhenFalse))
                }
                if ($Expression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax] -and
                    $Expression.RawKind -eq
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::CoalesceExpression) {
                    return (
                        (& $isAffectedResourceExpression `
                            -Expression $Expression.Left) -or
                        (& $isAffectedResourceExpression `
                            -Expression $Expression.Right))
                }
                return $false
            }
            $aliasDeclarators = @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                    $null -ne $_.Initializer
                })
            $aliasAssignments = @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                    $_.RawKind -eq
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::SimpleAssignmentExpression -and
                    $_.Left -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]
                })
            do {
                $aliasAdded = $false
                foreach ($aliasDeclarator in $aliasDeclarators) {
                    $aliasSymbol =
                        $semanticModel.GetDeclaredSymbol($aliasDeclarator)
                    $alreadyTracked = @($resourceAliases | Where-Object {
                            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                $_,
                                $aliasSymbol)
                        }).Count -ne 0
                    if ($null -ne $aliasSymbol -and
                        -not $alreadyTracked -and
                        (& $isAffectedResourceExpression `
                            -Expression $aliasDeclarator.Initializer.Value)) {
                        $resourceAliases.Add($aliasSymbol)
                        $aliasAdded = $true
                    }
                }
                foreach ($aliasAssignment in $aliasAssignments) {
                    $aliasSymbol = $semanticModel.GetSymbolInfo(
                        $aliasAssignment.Left).Symbol
                    $alreadyTracked = @($resourceAliases | Where-Object {
                            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                $_,
                                $aliasSymbol)
                        }).Count -ne 0
                    if ($null -ne $aliasSymbol -and
                        -not $alreadyTracked -and
                        (& $isAffectedResourceExpression `
                            -Expression $aliasAssignment.Right)) {
                        $resourceAliases.Add($aliasSymbol)
                        $aliasAdded = $true
                    }
                }
            } while ($aliasAdded)
            $resourceMappings = @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    if ($_ -isnot
                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                        $_.Left -isnot
                            [Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax]) {
                        return $false
                    }
                    $element = $_.Left
                    if ($element.ArgumentList.Arguments.Count -ne 1) {
                        return $false
                    }
                    $resourceExpression = & $unwrapResourceExpression `
                        -Expression $element.Expression
                    $writesAffectedResources = $false
                    if ($resourceExpression -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                        $resourceExpression.Name.Identifier.ValueText -ceq 'Resources') {
                        $receiver = $semanticModel.GetSymbolInfo(
                            $resourceExpression.Expression).Symbol
                        $writesAffectedResources =
                            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                $receiver,
                                $triggerOperation.AffectedSymbol)
                    } elseif ($resourceExpression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
                        $receiver =
                            $semanticModel.GetSymbolInfo($resourceExpression).Symbol
                        $writesAffectedResources = @($resourceAliases | Where-Object {
                                [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                    $_,
                                    $receiver)
                            }).Count -ne 0
                    }
                    if (-not $writesAffectedResources) {
                        return $false
                    }
                    $key = $element.ArgumentList.Arguments[0].Expression
                    if ($key -isnot
                        [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
                        throw (
                            'Writes through an affected Resources alias must use ' +
                            'literal keys so alternate-action provenance is closed.')
                    }
                    return $key.Token.ValueText -ceq
                        $triggerOperation.ResourceKey
                })
            $mappedValue = if ($resourceMappings.Count -eq 1) {
                $semanticModel.GetSymbolInfo(
                    $resourceMappings[0].Right).Symbol
            } else {
                $null
            }
            $directResourceExpression = if ($resourceMappings.Count -eq 1) {
                & $unwrapResourceExpression `
                    -Expression $resourceMappings[0].Left.Expression
            } else {
                $null
            }
            $mappingIsDirect = $resourceMappings.Count -eq 1 -and
                $resourceMappings[0].Parent -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -and
                $resourceMappings[0].Parent.Parent -eq $testMethod[0].Body -and
                $directResourceExpression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $directResourceExpression.Name.Identifier.ValueText -ceq
                    'Resources'
            if ($resourceMappings.Count -ne 1 -or
                $targetResourceWrites.Count -ne 1 -or
                -not $mappingIsDirect -or
                $resourceMappings[0].SpanStart -ge $gate.SpanStart -or
                -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                    $mappedValue,
                    $alternateOperation.AssignedValueSymbol)) {
                throw (
                    'The dynamic-resource key must be mapped exactly once before ' +
                    'the gate to the same local value assigned by the alternate branch.')
            }
        }
    }
    $trustedContractAssembly =
        'Microsoft.Maui.Controls.ReplicationControlContract'
    $trustedAttributeTypes = @(
        'Xunit.FactAttribute',
        'Xunit.TheoryAttribute',
        'NUnit.Framework.TestAttribute',
        'Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute'
    )
    $trustedTestAttributeCount = 0
    $trustedCategoryCount = 0
    foreach ($attribute in $testMethod[0].AttributeLists.Attributes) {
        $attributeSymbol = $semanticModel.GetSymbolInfo($attribute).Symbol
        if ($attributeSymbol -isnot [Microsoft.CodeAnalysis.IMethodSymbol]) {
            continue
        }
        $attributeTypeName = $attributeSymbol.ContainingType.ToString()
        if ($attributeSymbol.ContainingAssembly.Name -ceq
                $trustedContractAssembly -and
            $attributeTypeName -cin $trustedAttributeTypes -and
            @($attributeSymbol.Locations | Where-Object {
                    $_.IsInSource
                }).Count -eq 0) {
            $trustedTestAttributeCount++
        }
        if ($attributeSymbol.ContainingAssembly.Name -ceq
                $trustedContractAssembly -and
            $attributeTypeName -ceq 'Microsoft.Maui.CategoryAttribute' -and
            @($attributeSymbol.Locations | Where-Object {
                    $_.IsInSource
                }).Count -eq 0) {
            $trustedCategoryCount++
        }
    }
    if ($trustedTestAttributeCount -ne 1) {
        throw (
            'The selected test attribute must resolve semantically to a trusted ' +
            'external test framework.')
    }
    $declaredCategoryCount = @($selectedAttributeNames | Where-Object {
            $_ -ceq 'Category'
        }).Count
    if ($trustedCategoryCount -ne $declaredCategoryCount) {
        throw (
            'The Category attribute must resolve semantically to the trusted ' +
            'external Microsoft.Maui.CategoryAttribute.')
    }

    $flowEscapes = @($testMethod[0].Body.DescendantNodes() | Where-Object {
            ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax] -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax] -or
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax]) -and
            @($_.Ancestors() | Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax] -or
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax]
                }).Count -eq 0
        })
    if ($flowEscapes.Count -ne 0) {
        throw (
            'The selected test method may not return, yield, or jump around its ' +
            'trusted assertion; the control must execute the oracle.')
    }

    $trustedAssertionTypes = @(
        'Xunit.Assert',
        'NUnit.Framework.Assert',
        'NUnit.Framework.ClassicAssert',
        'Microsoft.VisualStudio.TestTools.UnitTesting.Assert',
        'Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert',
        'Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert'
    )
    $assertionStatements = @($testMethod[0].Body.DescendantNodes() |
        Where-Object {
            if ($_ -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax]) {
                return $false
            }
            $expression = $_.Expression
            if ($expression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]) {
                $expression = $expression.Expression
            }
            if ($expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
                return $false
            }
            $symbol = $semanticModel.GetSymbolInfo($expression).Symbol
            return (
                $symbol -is [Microsoft.CodeAnalysis.IMethodSymbol] -and
                $symbol.ContainingAssembly.Name -ceq $trustedContractAssembly -and
                $symbol.ContainingType.ToString() -cin $trustedAssertionTypes -and
                @($symbol.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -eq 0)
        })
    if ($assertionStatements.Count -eq 0) {
        throw (
            'The selected test method must contain a semantically trusted ' +
            'assertion statement.')
    }
    $postTriggerAssertionCount = 0
    # Unwraps an expression down to the identity-preserving symbol it
    # ultimately observes, or returns $null if any part of the shape is
    # not identity-preserving. Only two shapes are trusted:
    #   - a bare identifier (through parens/`as`/identity casts), and
    #   - a single-hop native-peer read applied directly to such an
    #     identifier: `X.ToPlatform()` (zero-argument, exact symbol
    #     `Microsoft.Maui.Platform.ElementExtensions.ToPlatform`) or
    #     `X.Handler.PlatformView` (exact `Handler` then `PlatformView`
    #     property symbols on the trusted external contract assembly).
    # Both hops map a managed control to ITS OWN native peer -- never a
    # selection/navigation over some other container -- so they cannot
    # reintroduce the decoy-sibling, container-escape, or delegate-local
    # bypass classes closed in prior rounds. This is a positive allowlist,
    # not a growing denylist, so any other invocation/member-access shape
    # (LINQ selection, descendant-navigation helpers, indexers, lambdas,
    # object/array/collection construction, conditional/coalescing
    # expressions, etc.) is rejected by default and must return $null.
    # Native-peer properties that reveal only AMBIENT reachability/
    # containment -- true for essentially any attached view regardless of
    # what the trigger did -- and therefore can NEVER stand in for a
    # trigger-caused oracle even when reached through an otherwise-trusted
    # native-peer hop. `Window`/`Superview` merely prove the peer exists
    # somewhere in a view hierarchy; `Subviews` and `Handler` expose
    # further navigation/indirection rather than the peer's own rendered
    # state. This is intentionally a narrow denylist of properties whose
    # presence is structural rather than trigger-dependent, not a general
    # allowlist -- every other native-peer property (e.g. `Bounds`,
    # `Text`, `Alpha`, `Hidden`) remains eligible for the family-match
    # relaxation below precisely because it reflects the peer's own
    # current rendered/content state.
    $ambientContainmentNativeProperties = @(
        'Window',
        'Superview',
        'Subviews',
        'Handler')
    $unwrapIdentityHop = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression,
            [ref]$UsedNativeHop
        )
        $current = $Expression
        while ($true) {
            if ($current -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax] -or
                $current -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax]) {
                $current = $current.Expression
                continue
            }
            if ($current -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax] -and
                $current.RawKind -eq
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::AsExpression) {
                $current = $current.Left
                continue
            }
            if ($current -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
                $current.ArgumentList.Arguments.Count -eq 0 -and
                $current.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $current.Expression.Name.Identifier.Text -ceq
                    'ToPlatform') {
                $toPlatformMethod =
                    $semanticModel.GetSymbolInfo($current).Symbol
                $toPlatformMethodKey = if ($toPlatformMethod -is
                    [Microsoft.CodeAnalysis.IMethodSymbol]) {
                    "$($toPlatformMethod.ContainingType).$($toPlatformMethod.Name)"
                } else {
                    ''
                }
                if ($toPlatformMethodKey -cne
                        'Microsoft.Maui.Platform.ElementExtensions.ToPlatform') {
                    return $null
                }
                if ($null -ne $UsedNativeHop) {
                    $UsedNativeHop.Value = $true
                }
                $current = $current.Expression.Expression
                continue
            }
            if ($current -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $current.Name.Identifier.Text -ceq 'PlatformView' -and
                $current.Expression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                $current.Expression.Name.Identifier.Text -ceq 'Handler') {
                $platformViewProperty =
                    $semanticModel.GetSymbolInfo($current).Symbol
                $handlerProperty =
                    $semanticModel.GetSymbolInfo($current.Expression).Symbol
                if ($platformViewProperty -isnot
                        [Microsoft.CodeAnalysis.IPropertySymbol] -or
                    $platformViewProperty.ContainingAssembly.Name -cne
                        $trustedContractAssembly -or
                    $handlerProperty -isnot
                        [Microsoft.CodeAnalysis.IPropertySymbol] -or
                    $handlerProperty.ContainingAssembly.Name -cne
                        $trustedContractAssembly) {
                    return $null
                }
                if ($null -ne $UsedNativeHop) {
                    $UsedNativeHop.Value = $true
                }
                $current = $current.Expression.Expression
                continue
            }
            break
        }
        if ($current -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            return $null
        }
        return $current
    }
    $unwrapAssertionExpression = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
        )
        while ($true) {
            if ($Expression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax]) {
                $Expression = $Expression.Expression
                continue
            }
            if ($Expression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax]) {
                $castType = $semanticModel.GetTypeInfo($Expression).Type
                $innerType = $semanticModel.GetTypeInfo(
                    $Expression.Expression).Type
                if ($null -ne $castType -and
                    $null -ne $innerType -and
                    [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                        $castType,
                        $innerType)) {
                    $Expression = $Expression.Expression
                    continue
                }
            }
            break
        }
        return $Expression
    }
    $normalizeAssertionExpression = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
        )
        $unwrapped = & $unwrapAssertionExpression -Expression $Expression
        return $unwrapped.ToString()
    }
    $getObviousBooleanValue = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
        )
        $Expression = & $unwrapAssertionExpression -Expression $Expression
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
            if ($Expression.Token.ValueText -cin @('true', 'false')) {
                return $Expression.Token.ValueText.ToLowerInvariant()
            }
            return 'unknown'
        }
        if ($Expression -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -and
            $Expression.RawKind -eq
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalNotExpression) {
            $operandValue = & $getObviousBooleanValue `
                -Expression $Expression.Operand
            if ($operandValue -ceq 'true') { return 'false' }
            if ($operandValue -ceq 'false') { return 'true' }
            return 'unknown'
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax]) {
            $leftValue = & $getObviousBooleanValue -Expression $Expression.Left
            $rightValue = & $getObviousBooleanValue -Expression $Expression.Right
            if ($Expression.RawKind -eq
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalOrExpression) {
                if ($leftValue -ceq 'true' -or $rightValue -ceq 'true') {
                    return 'true'
                }
                if ($leftValue -ceq 'false' -and $rightValue -ceq 'false') {
                    return 'false'
                }
            }
            if ($Expression.RawKind -eq
                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalAndExpression) {
                if ($leftValue -ceq 'false' -or $rightValue -ceq 'false') {
                    return 'false'
                }
                if ($leftValue -ceq 'true' -and $rightValue -ceq 'true') {
                    return 'true'
                }
            }
            $leftOperand = & $unwrapAssertionExpression -Expression $Expression.Left
            $rightOperand = & $unwrapAssertionExpression -Expression $Expression.Right
            $leftText = & $normalizeAssertionExpression -Expression $leftOperand
            $rightText = & $normalizeAssertionExpression -Expression $rightOperand
            $rightNegatesLeft =
                $rightOperand -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -and
                $rightOperand.RawKind -eq
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalNotExpression -and
                (& $normalizeAssertionExpression -Expression $rightOperand.Operand) -ceq
                    $leftText
            $leftNegatesRight =
                $leftOperand -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -and
                $leftOperand.RawKind -eq
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalNotExpression -and
                (& $normalizeAssertionExpression -Expression $leftOperand.Operand) -ceq
                    $rightText
            if ($rightNegatesLeft -or $leftNegatesRight) {
                if ($Expression.RawKind -in @(
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalOrExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::NotEqualsExpression)) {
                    return 'true'
                }
                if ($Expression.RawKind -in @(
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalAndExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::EqualsExpression)) {
                    return 'false'
                }
            }
            if ($leftText -ceq $rightText) {
                if ($Expression.RawKind -eq
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::EqualsExpression) {
                    return 'true'
                }
                if ($Expression.RawKind -eq
                    [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::NotEqualsExpression) {
                    return 'false'
                }
            }
            return 'unknown'
        }
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            $local = $semanticModel.GetSymbolInfo($Expression).Symbol
            if ($local -is [Microsoft.CodeAnalysis.ILocalSymbol]) {
                $valueExpressions = @($testMethod[0].Body.DescendantNodes() |
                    Where-Object {
                        $_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                        $_.SpanStart -gt $gate.Span.End -and
                        $_.Left -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                            $semanticModel.GetSymbolInfo($_.Left).Symbol,
                            $local)
                    } |
                    ForEach-Object Right)
                if ($valueExpressions.Count -eq 0) {
                    $valueExpressions = @($local.DeclaringSyntaxReferences |
                        ForEach-Object {
                            $_.GetSyntax([Threading.CancellationToken]::None)
                        } |
                        Where-Object {
                            $_ -is
                                [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                            $null -ne $_.Initializer
                        } |
                        ForEach-Object { $_.Initializer.Value })
                }
                $values = @($valueExpressions | ForEach-Object {
                        $valueExpression = $_
                        $selfReference = @($valueExpression.DescendantNodesAndSelf() |
                            Where-Object {
                                $_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
                                [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                    $semanticModel.GetSymbolInfo($_).Symbol,
                                    $local)
                            }).Count -ne 0
                        if ($selfReference) {
                            'unknown'
                        } else {
                            & $getObviousBooleanValue -Expression $valueExpression
                        }
                    } | Sort-Object -Unique)
                if ($values.Count -eq 1 -and
                    $values[0] -cin @('true', 'false')) {
                    return $values[0]
                }
            }
        }
        return 'unknown'
    }
    $isDirectOracleObservation = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
        )
        $Expression = & $unwrapAssertionExpression -Expression $Expression
        $symbol = $semanticModel.GetSymbolInfo($Expression).Symbol
        if (($symbol -is [Microsoft.CodeAnalysis.IPropertySymbol]) -and
            -not $symbol.IsStatic -and
            $symbol.ContainingAssembly.Name -ceq $trustedContractAssembly) {
            $postGateFrameworkWrites = @(
                $testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    if ($_.SpanStart -le $gate.Span.End) {
                        return $false
                    }
                    $writtenExpression = $null
                    if ($_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
                        $writtenExpression = $_.Left
                    } elseif (
                        ($_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -or
                         $_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax]) -and
                        ($_.RawKind -eq
                            [Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression.value__ -or
                         $_.RawKind -eq
                            [Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression.value__ -or
                         $_.RawKind -eq
                            [Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression.value__ -or
                         $_.RawKind -eq
                            [Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression.value__)) {
                        $writtenExpression = $_.Operand
                    } else {
                        return $false
                    }
                    $writtenProperty =
                        $semanticModel.GetSymbolInfo($writtenExpression).Symbol
                    return (
                        $writtenProperty -is
                            [Microsoft.CodeAnalysis.IPropertySymbol] -and
                        $writtenProperty.ContainingAssembly.Name -ceq
                            $trustedContractAssembly)
                })
            if ($postGateFrameworkWrites.Count -ne 0) {
                return $false
            }
            if ($Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
                return $false
            }
            # Unwrap the receiver through the same identity-preserving
            # shapes trusted everywhere else in the guard (parens/casts/
            # `as`, plus the single-hop `ToPlatform()`/
            # `Handler.PlatformView` native-peer read). This lets an
            # INLINE native-peer observation such as
            # `((UIKit.UILabel)label.ToPlatform()).Bounds` resolve its
            # receiver to `label` exactly as a two-statement form
            # (`var nativeLabel = label.ToPlatform(); nativeLabel.Bounds`)
            # already does -- both are the SAME identity-preserving hop,
            # just written inline.
            $inlineNativeHopRef = [ref]$false
            $receiverExpression = & $unwrapIdentityHop `
                -Expression $Expression.Expression `
                -UsedNativeHop $inlineNativeHopRef
            if ($null -eq $receiverExpression) {
                return $false
            }
            $observationUsedNativeHop = $inlineNativeHopRef.Value
            $receiverSymbol =
                $semanticModel.GetSymbolInfo($receiverExpression).Symbol
            if ($receiverSymbol -isnot [Microsoft.CodeAnalysis.ILocalSymbol] -and
                $receiverSymbol -isnot
                    [Microsoft.CodeAnalysis.IParameterSymbol]) {
                return $false
            }
            $pendingSymbols = [System.Collections.Generic.Queue[
                Microsoft.CodeAnalysis.ISymbol]]::new()
            $seenSymbols = [System.Collections.Generic.List[
                Microsoft.CodeAnalysis.ISymbol]]::new()
            $pendingSymbols.Enqueue($receiverSymbol)
            $causallyRelated = $false
            while ($pendingSymbols.Count -ne 0) {
                $candidateSymbol = $pendingSymbols.Dequeue()
                if (@($seenSymbols | Where-Object {
                            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                $_,
                                $candidateSymbol)
                        }).Count -ne 0) {
                    continue
                }
                $seenSymbols.Add($candidateSymbol)
                if (@($triggerOperation.RelatedSymbols | Where-Object {
                            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                $_,
                                $candidateSymbol)
                        }).Count -ne 0) {
                    $causallyRelated = $true
                    break
                }
                if ($candidateSymbol -is [Microsoft.CodeAnalysis.ILocalSymbol]) {
                    foreach ($candidateDeclaration in @(
                            $candidateSymbol.DeclaringSyntaxReferences |
                            ForEach-Object {
                                $_.GetSyntax(
                                    [Threading.CancellationToken]::None)
                            } |
                            Where-Object {
                                $_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                                $null -ne $_.Initializer
                            })) {
                        # Only propagate causal relation through a true
                        # identity-preserving hop. LINQ/platform helper
                        # chains such as `.Subviews.OfType<T>().First()` or
                        # `.FindDescendantView<T>(...)` do NOT preserve
                        # object identity -- they select/construct a
                        # DIFFERENT object that merely happens to be
                        # reachable through a trigger-related receiver.
                        # Root-typing such a chain to a Page/Window
                        # container is insufficient: the selection can
                        # still land on an unrelated sibling/descendant
                        # view, or escape the container entirely via a
                        # property such as `.Window`, so trusting ANY such
                        # chain to prove causal relation lets a decoy
                        # receiver satisfy the mandatory oracle. See
                        # `$unwrapIdentityHop` for the exact allowlisted
                        # shapes.
                        $rootIdentifier = & $unwrapIdentityHop `
                            -Expression $candidateDeclaration.Initializer.Value
                        if ($null -eq $rootIdentifier) {
                            continue
                        }
                        $rootSymbol =
                            $semanticModel.GetSymbolInfo($rootIdentifier).Symbol
                        if ($rootSymbol -is
                                [Microsoft.CodeAnalysis.ILocalSymbol] -or
                            $rootSymbol -is
                                [Microsoft.CodeAnalysis.IParameterSymbol]) {
                            $pendingSymbols.Enqueue($rootSymbol)
                        }
                    }
                }
            }
            if (-not $causallyRelated) {
                return $false
            }
            if ($receiverSymbol -is [Microsoft.CodeAnalysis.ILocalSymbol]) {
                $receiverDeclarators = @($receiverSymbol.DeclaringSyntaxReferences |
                    ForEach-Object {
                        $_.GetSyntax([Threading.CancellationToken]::None)
                    } |
                    Where-Object {
                        $_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                        $null -ne $_.Initializer
                    })
                if ($receiverDeclarators.Count -ne 1 -or
                    $null -eq $receiverDeclarators[0].Initializer) {
                    return $false
                }
                # `$symbol` is the property actually being observed by the
                # assertion. If it was reached by unwrapping through a
                # trusted native-peer hop (`ToPlatform()` /
                # `Handler.PlatformView`) applied directly to the
                # receiver's own initializer, the property lives in the
                # NATIVE namespace of the peer, not the managed
                # BindableProperty namespace the trigger's state family is
                # drawn from -- e.g. observing `.Bounds`/`.Text` on a
                # native peer reflects that SAME element's own current
                # rendered content, and is not expected to name-match the
                # managed property the trigger set. Requiring a name match
                # there is a category error and was the actual root cause
                # of a regression that rejected legitimate native-peer
                # geometry/content observations. However, a handful of
                # native-peer properties reveal only AMBIENT
                # containment/reachability true of nearly any attached
                # view regardless of what the trigger did (`Window`,
                # `Superview`, `Subviews`, `Handler`) -- those must remain
                # rejected even via a native hop, since they prove nothing
                # about causal linkage to the trigger. This is exactly the
                # round-5 security-review Alert-1 PoC
                # (`nativeRoot.Window`), which is still rejected below by
                # the explicit denylist rather than a repurposed name
                # match. For a receiver reached with NO native hop (a bare
                # identifier, still just a managed-property observation),
                # the family-name match remains the only signal and is
                # still enforced unconditionally.
                $usedNativeHopForFamily = $observationUsedNativeHop
                if (-not $usedNativeHopForFamily -and
                    $receiverDeclarators[0].SpanStart -ge $gate.SpanStart) {
                    $familyCheckInitializer =
                        $receiverDeclarators[0].Initializer.Value
                    $nativeHopRef = [ref]$false
                    $null = & $unwrapIdentityHop `
                        -Expression $familyCheckInitializer `
                        -UsedNativeHop $nativeHopRef
                    $usedNativeHopForFamily = $nativeHopRef.Value
                }
                if ($usedNativeHopForFamily) {
                    if (@($ambientContainmentNativeProperties |
                            Where-Object {
                                $_ -ceq $symbol.Name
                            }).Count -ne 0) {
                        return $false
                    }
                } else {
                    $observedFamily = $symbol.Name
                    if ($observedFamily -ceq 'BackgroundColor') {
                        $observedFamily = 'Background'
                    }
                    $triggerFamily = [string]$triggerOperation.StateFamily
                    if ($triggerFamily.StartsWith(
                            'Set',
                            [StringComparison]::Ordinal)) {
                        $triggerFamily = $triggerFamily.Substring(3)
                    }
                    if ($triggerFamily -ceq 'BackgroundColor') {
                        $triggerFamily = 'Background'
                    }
                    if ($observedFamily -cne $triggerFamily) {
                        return $false
                    }
                }
                if ($receiverDeclarators[0].SpanStart -ge $gate.SpanStart) {
                    $receiverInitializer =
                        $receiverDeclarators[0].Initializer.Value
                    # A receiver declared after the gate can only be
                    # trusted when it is a true identity-preserving hop to
                    # a symbol that is itself trusted by the dependency
                    # walk below. See `$unwrapIdentityHop` for the exact
                    # allowlisted shapes and why no other invocation-based
                    # receiver derivation is trusted here.
                    $unwrappedReceiverInitializer = & $unwrapIdentityHop `
                        -Expression $receiverInitializer
                    if ($null -eq $unwrappedReceiverInitializer) {
                        return $false
                    }
                    $receiverInitializer = $unwrappedReceiverInitializer
                    foreach ($dependency in @(
                            $receiverInitializer.DescendantNodesAndSelf() |
                            Where-Object {
                                $_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]
                            } |
                            ForEach-Object {
                                $semanticModel.GetSymbolInfo($_).Symbol
                            } |
                            Where-Object {
                                $_ -is [Microsoft.CodeAnalysis.ILocalSymbol]
                            } |
                            Select-Object -Unique)) {
                        $dependencyDeclaration = @(
                            $dependency.DeclaringSyntaxReferences |
                            ForEach-Object {
                                $_.GetSyntax(
                                    [Threading.CancellationToken]::None)
                            } |
                            Where-Object {
                                $_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax]
                            })
                        if ($dependencyDeclaration.Count -ne 1 -or
                            $dependencyDeclaration[0].SpanStart -ge
                                $gate.SpanStart) {
                            return $false
                        }
                        $dependencyAliases = @(
                            $testMethod[0].Body.DescendantNodes() |
                            Where-Object {
                                $aliasTarget = $null
                                $copiedExpression = if ($_ -is
                                        [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                                    $null -ne $_.Initializer -and
                                    $_ -ne $receiverDeclarators[0] -and
                                    $_ -ne $dependencyDeclaration[0]) {
                                    $aliasTarget =
                                        $semanticModel.GetDeclaredSymbol($_)
                                    $_.Initializer.Value
                                } elseif ($_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
                                    $aliasTarget =
                                        $semanticModel.GetSymbolInfo($_.Left).Symbol
                                    $_.Right
                                } elseif ($_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax]) {
                                    $aliasTarget =
                                        $semanticModel.GetDeclaredSymbol($_)
                                    $patternExpression = @($_.Ancestors() |
                                        Where-Object {
                                            $_ -is
                                                [Microsoft.CodeAnalysis.CSharp.Syntax.IsPatternExpressionSyntax] -or
                                            $_ -is
                                                [Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax] -or
                                            $_ -is
                                                [Microsoft.CodeAnalysis.CSharp.Syntax.SwitchExpressionSyntax]
                                        } |
                                        Select-Object -First 1)
                                    if ($patternExpression.Count -eq 1) {
                                        if ($patternExpression[0] -is
                                            [Microsoft.CodeAnalysis.CSharp.Syntax.SwitchExpressionSyntax]) {
                                            $patternExpression[0].GoverningExpression
                                        } else {
                                            $patternExpression[0].Expression
                                        }
                                    } else {
                                        $null
                                    }
                                } else {
                                    $null
                                }
                                if ($null -eq $copiedExpression) {
                                    return $false
                                }
                                if ($null -eq $aliasTarget -or
                                    $null -eq $aliasTarget.Type -or
                                    -not $aliasTarget.Type.IsReferenceType) {
                                    return $false
                                }
                                $aliasConversion =
                                    $semanticCompilation.ClassifyConversion(
                                        $dependency.Type,
                                        $aliasTarget.Type)
                                if (-not $aliasConversion.Exists -or
                                    -not $aliasConversion.IsReference) {
                                    return $false
                                }
                                return @(
                                    $copiedExpression.DescendantNodesAndSelf() |
                                    Where-Object {
                                        $_ -is
                                            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
                                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                            $semanticModel.GetSymbolInfo($_).Symbol,
                                            $dependency)
                                    }).Count -ne 0
                            })
                        if ($dependencyAliases.Count -ne 0) {
                            return $false
                        }
                        $dependencyWrites = @(
                            $testMethod[0].Body.DescendantNodes() |
                            Where-Object {
                                $writtenExpression = if ($_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
                                    $_.Left
                                } elseif (($_ -is
                                            [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -or
                                        $_ -is
                                            [Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax]) -and
                                    $_.RawKind -in @(
                                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression)) {
                                    $_.Operand
                                } else {
                                    $null
                                }
                                if ($null -eq $writtenExpression) {
                                    return $false
                                }
                                return @(
                                    $writtenExpression.DescendantNodesAndSelf() |
                                    Where-Object {
                                        $_ -is
                                            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
                                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                            $semanticModel.GetSymbolInfo($_).Symbol,
                                            $dependency)
                                    }).Count -ne 0
                            })
                        if ($dependencyWrites.Count -ne 0) {
                            return $false
                        }
                    }
                }
            }
            $receiverWrites = @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    if ($_ -isnot
                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
                        return $false
                    }
                    $writtenReceiver =
                        $semanticModel.GetSymbolInfo($_.Left).Symbol
                    return (
                        $null -ne $writtenReceiver -and
                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                            $writtenReceiver,
                            $receiverSymbol))
                })
            if ($receiverWrites.Count -ne 0) {
                return $false
            }
            $writes = @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    if ($gate.Span.Contains($_.Span)) {
                        return $false
                    }
                    $writtenExpression = if ($_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax]) {
                        $_.Left
                    } elseif (($_ -is
                                [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -or
                            $_ -is
                                [Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax]) -and
                        ($_.RawKind -eq [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression -or
                            $_.RawKind -eq [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression -or
                            $_.RawKind -eq [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression -or
                            $_.RawKind -eq [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression)) {
                        $_.Operand
                    } else {
                        $null
                    }
                    if ($null -eq $writtenExpression) {
                        return $false
                    }
                    $writtenSymbol =
                        $semanticModel.GetSymbolInfo($writtenExpression).Symbol
                    return (
                        $null -ne $writtenSymbol -and
                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                            $writtenSymbol,
                            $symbol))
                })
            if ($writes.Count -ne 0) {
                return $false
            }
            $observedFamily = $symbol.Name
            if ($observedFamily -ceq 'BackgroundColor') {
                $observedFamily = 'Background'
            }
            $dynamicResourceWrites = @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    if ($_ -isnot
                            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -or
                        $gate.Span.Contains($_.Span)) {
                        return $false
                    }
                    $method = $semanticModel.GetSymbolInfo($_).Symbol
                    if ($method -isnot [Microsoft.CodeAnalysis.IMethodSymbol] -or
                        $method.ContainingAssembly.Name -cne
                            $trustedContractAssembly -or
                        $method.Name -cne 'SetDynamicResource' -or
                        $_.ArgumentList.Arguments.Count -lt 1) {
                        return $false
                    }
                    $propertyArgument =
                        $_.ArgumentList.Arguments[0].Expression
                    $propertySymbol =
                        $semanticModel.GetSymbolInfo($propertyArgument).Symbol
                    if (($propertySymbol -isnot
                                [Microsoft.CodeAnalysis.IPropertySymbol]) -and
                        ($propertySymbol -isnot
                                [Microsoft.CodeAnalysis.IFieldSymbol])) {
                        return $true
                    }
                    $writtenFamily = $propertySymbol.Name
                    if ($writtenFamily.EndsWith(
                            'Property',
                            [StringComparison]::Ordinal)) {
                        $writtenFamily = $writtenFamily.Substring(
                            0,
                            $writtenFamily.Length - 'Property'.Length)
                    }
                    if ($writtenFamily -ceq 'BackgroundColor') {
                        $writtenFamily = 'Background'
                    }
                    return $writtenFamily -ceq $observedFamily
                })
            if ($dynamicResourceWrites.Count -ne 0) {
                return $false
            }
            return $true
        }
        return $false
    }
    $isTrustedWindowCallbackNativeOracle = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression,
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax]$CallbackBody,
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.IMethodSymbol]$AssertionMethod,
            [Parameter(Mandatory = $true)][int]$AssertionArgumentCount,
            [AllowNull()]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$ExpectedExpression
        )

        $Expression = & $unwrapAssertionExpression -Expression $Expression
        if ($Expression -isnot
            [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
            return $false
        }
        $nativeProperty = $semanticModel.GetSymbolInfo($Expression).Symbol
        if ($nativeProperty -isnot [Microsoft.CodeAnalysis.IPropertySymbol] -or
            $nativeProperty.IsStatic -or
            $nativeProperty.IsIndexer -or
            $nativeProperty.ContainingAssembly.Name -cne
                $trustedContractAssembly -or
            @($nativeProperty.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            return $false
        }

        if ($nativeProperty.ContainingType.ToString() -cne 'UIKit.UILabel' -or
            $nativeProperty.Name -cne 'Text' -or
            $AssertionMethod.ContainingType.ToString() -cne 'Xunit.Assert' -or
            $AssertionMethod.Name -cne 'Equal' -or
            $AssertionArgumentCount -ne 2 -or
            $ExpectedExpression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax] -or
            $ExpectedExpression.Token.ValueText -cne 'Main' -or
            $triggerOperation.Kind -cne 'invocation' -or
            $triggerOperation.MethodName -cne 'SetTitleView' -or
            -not $trustedWindowCallbackAttachedSymbols.ContainsKey(
                $CallbackBody.SpanStart) -or
            -not $trustedWindowCallbackNavigationRoots.ContainsKey(
                $CallbackBody.SpanStart) -or
            -not $trustedWindowCallbackBackTitleRoots.ContainsKey(
                $CallbackBody.SpanStart) -or
            -not $trustedWindowCallbackFinalPages.ContainsKey(
                $CallbackBody.SpanStart) -or
            -not $trustedWindowCallbackOracleMinimums.ContainsKey(
                $CallbackBody.SpanStart)) {
            return $false
        }
        $attachedSymbols = @(
            $trustedWindowCallbackAttachedSymbols[$CallbackBody.SpanStart])
        if ($null -eq $triggerOperation.AffectedSymbol -or
            -not [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $triggerOperation.AffectedSymbol,
                $trustedWindowCallbackFinalPages[$CallbackBody.SpanStart]) -or
            @($attachedSymbols | Where-Object {
                    [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                        $_,
                        $triggerOperation.AffectedSymbol)
                }).Count -eq 0) {
            return $false
        }

        $getLocalInitializer = {
            param(
                [Parameter(Mandatory = $true)]
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]$Identifier
            )
            $local = $semanticModel.GetSymbolInfo($Identifier).Symbol
            if ($local -isnot [Microsoft.CodeAnalysis.ILocalSymbol]) {
                return $null
            }
            $declarations = @(
                $local.DeclaringSyntaxReferences |
                ForEach-Object {
                    $_.GetSyntax([Threading.CancellationToken]::None)
                } |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                    $null -ne $_.Initializer -and
                    $_.SpanStart -gt
                        $trustedWindowCallbackOracleMinimums[
                            $CallbackBody.SpanStart] -and
                    $_.Parent.Parent.Parent -eq $CallbackBody
                })
            if ($declarations.Count -ne 1) {
                return $null
            }
            return $declarations[0].Initializer.Value
        }

        $labelIdentifier = $Expression.Expression
        if ($labelIdentifier -isnot
            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            return $false
        }
        $findLabelCall = & $getLocalInitializer -Identifier $labelIdentifier
        if ($findLabelCall -isnot
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            return $false
        }
        $findLabelMethod = $semanticModel.GetSymbolInfo($findLabelCall).Symbol
        $findLabelDefinition = if ($findLabelMethod -is
                [Microsoft.CodeAnalysis.IMethodSymbol] -and
            $null -ne $findLabelMethod.ReducedFrom) {
            $findLabelMethod.ReducedFrom
        } else {
            $findLabelMethod
        }
        if ($findLabelDefinition -isnot
                [Microsoft.CodeAnalysis.IMethodSymbol] -or
            "$($findLabelDefinition.ContainingType).$($findLabelDefinition.Name)" -cne
                'Microsoft.Maui.Platform.ViewExtensions.FindDescendantView' -or
            $findLabelCall.ArgumentList.Arguments.Count -ne 0 -or
            $findLabelMethod.TypeArguments.Length -ne 1 -or
            $findLabelMethod.TypeArguments[0].ToString() -cne 'UIKit.UILabel' -or
            $findLabelCall.Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -or
            $findLabelCall.Expression.Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            return $false
        }

        $backButtonCall = & $getLocalInitializer `
            -Identifier $findLabelCall.Expression.Expression
        if ($backButtonCall -isnot
            [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax]) {
            return $false
        }
        $backButtonMethod = $semanticModel.GetSymbolInfo($backButtonCall).Symbol
        $backButtonDefinition = if ($backButtonMethod -is
                [Microsoft.CodeAnalysis.IMethodSymbol] -and
            $null -ne $backButtonMethod.ReducedFrom) {
            $backButtonMethod.ReducedFrom
        } else {
            $backButtonMethod
        }
        if ($backButtonDefinition -isnot
                [Microsoft.CodeAnalysis.IMethodSymbol] -or
            "$($backButtonDefinition.ContainingType).$($backButtonDefinition.Name)" -cne
                'Microsoft.Maui.DeviceTests.AssertionExtensions.GetBackButton' -or
            $backButtonCall.ArgumentList.Arguments.Count -ne 0 -or
            $backButtonCall.Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -or
            $backButtonCall.Expression.Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            return $false
        }

        $navigationBarRead = & $getLocalInitializer `
            -Identifier $backButtonCall.Expression.Expression
        if ($navigationBarRead -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -or
            $navigationBarRead.Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            return $false
        }
        $navigationBarProperty =
            $semanticModel.GetSymbolInfo($navigationBarRead).Symbol
        if ($navigationBarProperty -isnot
                [Microsoft.CodeAnalysis.IPropertySymbol] -or
            $navigationBarProperty.ContainingAssembly.Name -cne
                $trustedContractAssembly -or
            $navigationBarProperty.ContainingType.ToString() -cne
                'UIKit.UINavigationController' -or
            $navigationBarProperty.Name -cne 'NavigationBar') {
            return $false
        }

        $navigationControllerCast = & $getLocalInitializer `
            -Identifier $navigationBarRead.Expression
        if ($navigationControllerCast -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax] -or
            $semanticModel.GetTypeInfo($navigationControllerCast.Type).Type.ToString() -cne
                'UIKit.UINavigationController') {
            return $false
        }
        $handlerRead = $navigationControllerCast.Expression
        while ($handlerRead -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax]) {
            $handlerRead = $handlerRead.Expression
        }
        if ($handlerRead -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -or
            $handlerRead.Name.Identifier.ValueText -cne 'Handler' -or
            $handlerRead.Expression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) {
            return $false
        }
        $handlerProperty =
            $semanticModel.GetSymbolInfo($handlerRead).Symbol
        $navigationRoot =
            $semanticModel.GetSymbolInfo(
                $handlerRead.Expression).Symbol
        return (
            $handlerProperty -is
                [Microsoft.CodeAnalysis.IPropertySymbol] -and
            $handlerProperty.ContainingAssembly.Name -ceq
                $trustedContractAssembly -and
            $handlerProperty.ContainingType.ToString() -ceq
                'Microsoft.Maui.Controls.Element' -and
            $handlerProperty.Name -ceq 'Handler' -and
            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                $navigationRoot,
                $trustedWindowCallbackNavigationRoots[$CallbackBody.SpanStart]))
    }
    $isExpectedOracleValue = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
        )
        $Expression = & $unwrapAssertionExpression -Expression $Expression
        if ($Expression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
            return $true
        }
        $symbol = $semanticModel.GetSymbolInfo($Expression).Symbol
        if ($symbol -is [Microsoft.CodeAnalysis.IParameterSymbol]) {
            return $true
        }
        if ($symbol -isnot [Microsoft.CodeAnalysis.ILocalSymbol]) {
            return $false
        }
        Confirm-ReplicationTrustedOracleExpression `
            -Expression $Expression `
            -SemanticModel $semanticModel `
            -Root $root
        $declarator = @($symbol.DeclaringSyntaxReferences |
            ForEach-Object {
                $_.GetSyntax([Threading.CancellationToken]::None)
            } |
            Where-Object {
                $_ -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                $null -ne $_.Initializer
            } | Select-Object -First 1)
        if ($declarator.Count -ne 1) {
            return $false
        }
        $instanceReads = @($declarator[0].Initializer.Value.DescendantNodesAndSelf() |
            Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
            } |
            ForEach-Object {
                $semanticModel.GetSymbolInfo($_).Symbol
            } |
            Where-Object {
                ($_ -is [Microsoft.CodeAnalysis.IPropertySymbol] -or
                    $_ -is [Microsoft.CodeAnalysis.IFieldSymbol]) -and
                -not $_.IsStatic
            })
        return $instanceReads.Count -eq 0
    }
    $isNullableOracleExpression = {
        param(
            [Parameter(Mandatory = $true)]
            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]$Expression
        )
        $Expression = & $unwrapAssertionExpression -Expression $Expression
        $expressionSymbol =
            $semanticModel.GetSymbolInfo($Expression).Symbol
        if ($expressionSymbol -is [Microsoft.CodeAnalysis.ILocalSymbol]) {
            $reachingValues = @($expressionSymbol.DeclaringSyntaxReferences |
                ForEach-Object {
                    $_.GetSyntax([Threading.CancellationToken]::None)
                } |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                    $null -ne $_.Initializer
                } |
                ForEach-Object { $_.Initializer.Value })
            $reachingValues += @($testMethod[0].Body.DescendantNodes() |
                Where-Object {
                    $_ -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                    $_.SpanStart -lt $assertionStatement.SpanStart -and
                    $_.Left -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -and
                    [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                        $semanticModel.GetSymbolInfo($_.Left).Symbol,
                        $expressionSymbol)
                } |
                ForEach-Object Right)
            foreach ($value in $reachingValues) {
                $valueType = $semanticModel.GetTypeInfo($value).Type
                if ($null -ne $valueType -and
                    -not $valueType.IsReferenceType -and
                    -not ($valueType -is [Microsoft.CodeAnalysis.INamedTypeSymbol] -and
                        $valueType.OriginalDefinition.SpecialType -eq
                            [Microsoft.CodeAnalysis.SpecialType]::System_Nullable_T)) {
                    return $false
                }
            }
        }
        $type = $semanticModel.GetTypeInfo($Expression).Type
        if ($null -eq $type) {
            return $false
        }
        if ($type.IsReferenceType) {
            return $true
        }
        return (
            $type -is [Microsoft.CodeAnalysis.INamedTypeSymbol] -and
            $type.OriginalDefinition.SpecialType -eq
                [Microsoft.CodeAnalysis.SpecialType]::System_Nullable_T)
    }
    foreach ($assertionStatement in $assertionStatements) {
        $assertionExpression = $assertionStatement.Expression
        if ($assertionExpression -is
            [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]) {
            $assertionExpression = $assertionExpression.Expression
        }
        $assertionSymbol =
            $semanticModel.GetSymbolInfo($assertionExpression).Symbol
        if ($assertionExpression -isnot
                [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -or
            $assertionSymbol -isnot [Microsoft.CodeAnalysis.IMethodSymbol]) {
            throw (
                'Every assertion must resolve semantically to a trusted external ' +
                "test framework member. Expression type: '$(
                    $assertionExpression.GetType().FullName)'; symbol: '$assertionSymbol'.")
        }
        $assertionTypeName = $assertionSymbol.ContainingType.ToString()
        if ($assertionSymbol.ContainingAssembly.Name -cne
                $trustedContractAssembly -or
            $assertionTypeName -cnotin $trustedAssertionTypes -or
            @($assertionSymbol.Locations | Where-Object {
                    $_.IsInSource
                }).Count -ne 0) {
            throw (
                'Every assertion must resolve semantically to a trusted external ' +
                'test framework member.')
        }
        $isDirectPostGateAssertion =
            $assertionStatement.Parent -eq $testMethod[0].Body -and
            $assertionStatement.SpanStart -gt $gate.Span.End
        $isTrustedWindowCallbackAssertion =
            $assertionStatement.Parent -is
                [Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax] -and
            $trustedWindowCallbackBodies.Contains(
                $assertionStatement.Parent.SpanStart) -and
            $trustedWindowCallbackOracleMinimums.ContainsKey(
                $assertionStatement.Parent.SpanStart) -and
            $assertionStatement.SpanStart -gt
                $trustedWindowCallbackOracleMinimums[
                    $assertionStatement.Parent.SpanStart] -and
            $assertionStatement.SpanStart -gt $gate.Span.End
        if ($isDirectPostGateAssertion -or
            $isTrustedWindowCallbackAssertion) {
            $assertionArguments = @(
                $assertionExpression.ArgumentList.Arguments)
            $isSelfComparison =
                $assertionArguments.Count -ge 2 -and
                (& $normalizeAssertionExpression `
                    -Expression $assertionArguments[0].Expression) -ceq
                (& $normalizeAssertionExpression `
                    -Expression $assertionArguments[1].Expression)
            $isTautologicalAssertion = $isSelfComparison
            $trueBooleanTautology = $false
            $falseBooleanTautology = $false
            if ($assertionArguments.Count -ge 1) {
                $booleanExpression = & $unwrapAssertionExpression `
                    -Expression $assertionArguments[0].Expression
                if ($booleanExpression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
                    $trueBooleanTautology =
                        $booleanExpression.Token.ValueText -ceq 'true'
                    $falseBooleanTautology =
                        $booleanExpression.Token.ValueText -ceq 'false'
                } elseif ($booleanExpression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax]) {
                    $leftOperand = & $unwrapAssertionExpression `
                        -Expression $booleanExpression.Left
                    $rightOperand = & $unwrapAssertionExpression `
                        -Expression $booleanExpression.Right
                    $leftText = & $normalizeAssertionExpression `
                        -Expression $leftOperand
                    $rightText = & $normalizeAssertionExpression `
                        -Expression $rightOperand
                    $rightNegatesLeft =
                        $rightOperand -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -and
                        (& $normalizeAssertionExpression `
                            -Expression $rightOperand.Operand) -ceq
                            $leftText
                    $leftNegatesRight =
                        $leftOperand -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax] -and
                        (& $normalizeAssertionExpression `
                            -Expression $leftOperand.Operand) -ceq
                            $rightText
                    $sameOperands = $leftText -ceq $rightText
                    $trueBooleanTautology =
                        ($booleanExpression.RawKind -eq
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalOrExpression -and
                            ($rightNegatesLeft -or $leftNegatesRight)) -or
                        ($booleanExpression.RawKind -eq
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::EqualsExpression -and
                            $sameOperands)
                    $falseBooleanTautology =
                        ($booleanExpression.RawKind -eq
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::LogicalAndExpression -and
                            ($rightNegatesLeft -or $leftNegatesRight)) -or
                        ($booleanExpression.RawKind -eq
                                [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::NotEqualsExpression -and
                            $sameOperands)
                }
            }
            if ($assertionArguments.Count -ge 1) {
                $obviousBooleanValue = & $getObviousBooleanValue `
                    -Expression $assertionArguments[0].Expression
                if ($obviousBooleanValue -ceq 'true') {
                    $trueBooleanTautology = $true
                } elseif ($obviousBooleanValue -ceq 'false') {
                    $falseBooleanTautology = $true
                }
            }
            if ($assertionSymbol.Name -cin @('True', 'IsTrue')) {
                $isTautologicalAssertion =
                    $isTautologicalAssertion -or $trueBooleanTautology
            } elseif ($assertionSymbol.Name -cin @('False', 'IsFalse')) {
                $isTautologicalAssertion =
                    $isTautologicalAssertion -or $falseBooleanTautology
            }
            if ($assertionTypeName -ceq 'NUnit.Framework.Assert' -and
                $assertionSymbol.Name -ceq 'That' -and
                $assertionArguments.Count -ge 2) {
                $constraintExpression = & $unwrapAssertionExpression `
                    -Expression $assertionArguments[1].Expression
                if ($constraintExpression -is
                    [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
                    $constraintExpression.Expression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $constraintExpression.Expression.Expression.ToString() -ceq 'Is' -and
                    $constraintExpression.Expression.Name.Identifier.ValueText -cin
                        @('EqualTo', 'SameAs') -and
                    $constraintExpression.ArgumentList.Arguments.Count -eq 1 -and
                    (& $normalizeAssertionExpression `
                        -Expression $constraintExpression.ArgumentList.Arguments[0].Expression) -ceq
                    (& $normalizeAssertionExpression `
                        -Expression $assertionArguments[0].Expression)) {
                    $isTautologicalAssertion = $true
                }
                if ($constraintExpression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $constraintExpression.Name.Identifier.ValueText -cin @('True', 'False')) {
                    if ($constraintExpression.Name.Identifier.ValueText -ceq 'True') {
                        $isTautologicalAssertion =
                            $isTautologicalAssertion -or $trueBooleanTautology
                    } else {
                        $isTautologicalAssertion =
                            $isTautologicalAssertion -or $falseBooleanTautology
                    }
                }
            }
            $argumentSymbols = @($assertionArguments | ForEach-Object {
                    $_.Expression.DescendantNodesAndSelf() |
                        Where-Object {
                            $_ -is
                                [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
                        } |
                        ForEach-Object {
                            $semanticModel.GetSymbolInfo($_).Symbol
                        } |
                        Where-Object { $null -ne $_ }
                })
            $observesFrameworkMember = @($argumentSymbols |
                Where-Object {
                    ($_ -is [Microsoft.CodeAnalysis.IPropertySymbol] -or
                        $_ -is [Microsoft.CodeAnalysis.IFieldSymbol]) -and
                    -not $_.IsStatic -and
                    -not $_.ContainingType.ToString().StartsWith(
                        'NUnit.Framework.',
                        [StringComparison]::Ordinal)
                }).Count -ne 0
            $observesChangedLocal = $false
            foreach ($local in @($argumentSymbols | Where-Object {
                        $_ -is [Microsoft.CodeAnalysis.ILocalSymbol]
                    } | Select-Object -Unique)) {
                $localWrites = @($testMethod[0].Body.DescendantNodes() |
                    Where-Object {
                        if ($_ -isnot
                            [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax] -or
                            $_.SpanStart -le $gate.Span.End) {
                            return $false
                        }
                        $candidate =
                            $semanticModel.GetSymbolInfo($_).Symbol
                        return (
                            $null -ne $candidate -and
                            [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                $candidate,
                                $local) -and
                            $_.Parent -is
                                [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                            $_.Parent.Left -eq $_)
                    })
                $initializerReads = @($local.DeclaringSyntaxReferences |
                    ForEach-Object {
                        $_.GetSyntax([Threading.CancellationToken]::None)
                    } |
                    Where-Object {
                        $_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                        $null -ne $_.Initializer
                    } |
                    ForEach-Object {
                        $_.Initializer.Value.DescendantNodesAndSelf()
                    } |
                    Where-Object {
                        $_ -is
                            [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
                    } |
                    ForEach-Object {
                        $semanticModel.GetSymbolInfo($_).Symbol
                    } |
                    Where-Object {
                        $_ -is [Microsoft.CodeAnalysis.IPropertySymbol] -or
                        $_ -is [Microsoft.CodeAnalysis.IFieldSymbol]
                    })
                if ($localWrites.Count -ne 0 -or
                    $initializerReads.Count -ne 0) {
                    $observesChangedLocal = $true
                    break
                }
            }
            $supportedGuaranteedOracle = $false
            $guaranteedObservationExpression = $null
            $guaranteedExpectedExpression = $null
            if ($assertionArguments.Count -ge 1 -and
                $assertionSymbol.Name -cin @(
                    'True',
                    'False',
                    'IsTrue',
                    'IsFalse'
                )) {
                $guaranteedObservationExpression =
                    $assertionArguments[0].Expression
                $supportedGuaranteedOracle =
                    & $isDirectOracleObservation `
                        -Expression $guaranteedObservationExpression
            } elseif ($assertionArguments.Count -ge 1 -and
                $assertionSymbol.Name -cin @('NotNull', 'IsNotNull')) {
                $supportedGuaranteedOracle =
                    (& $isDirectOracleObservation `
                        -Expression $assertionArguments[0].Expression) -and
                    (& $isNullableOracleExpression `
                        -Expression $assertionArguments[0].Expression)
            } elseif ($assertionArguments.Count -ge 2 -and
                $assertionSymbol.Name -cin @(
                    'Equal',
                    'NotEqual',
                    'Same',
                    'NotSame',
                    'AreEqual',
                    'AreNotEqual'
                )) {
                if ((& $isDirectOracleObservation `
                        -Expression $assertionArguments[0].Expression) -and
                    (& $isExpectedOracleValue `
                        -Expression $assertionArguments[1].Expression)) {
                    $supportedGuaranteedOracle = $true
                    $guaranteedObservationExpression =
                        $assertionArguments[0].Expression
                    $guaranteedExpectedExpression =
                        $assertionArguments[1].Expression
                } elseif ((& $isDirectOracleObservation `
                        -Expression $assertionArguments[1].Expression) -and
                    (& $isExpectedOracleValue `
                        -Expression $assertionArguments[0].Expression)) {
                    $supportedGuaranteedOracle = $true
                    $guaranteedObservationExpression =
                        $assertionArguments[1].Expression
                    $guaranteedExpectedExpression =
                        $assertionArguments[0].Expression
                }
                $firstValue = & $unwrapAssertionExpression `
                    -Expression $assertionArguments[0].Expression
                $secondValue = & $unwrapAssertionExpression `
                    -Expression $assertionArguments[1].Expression
                if ($firstValue.RawKind -eq
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::NullLiteralExpression -and
                    -not (& $isNullableOracleExpression `
                        -Expression $secondValue)) {
                    $supportedGuaranteedOracle = $false
                }
                if ($secondValue.RawKind -eq
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::NullLiteralExpression -and
                    -not (& $isNullableOracleExpression `
                        -Expression $firstValue)) {
                    $supportedGuaranteedOracle = $false
                }
            } elseif ($assertionTypeName -ceq 'NUnit.Framework.Assert' -and
                $assertionSymbol.Name -ceq 'That' -and
                $assertionArguments.Count -ge 2 -and
                (& $isDirectOracleObservation `
                    -Expression $assertionArguments[0].Expression)) {
                $guaranteedObservationExpression =
                    $assertionArguments[0].Expression
                $constraint = & $unwrapAssertionExpression `
                    -Expression $assertionArguments[1].Expression
                if ($constraint -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
                    $constraint.Expression -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $constraint.Expression.Name.Identifier.ValueText -cin
                        @('EqualTo', 'SameAs', 'GreaterThan', 'LessThan') -and
                    $constraint.ArgumentList.Arguments.Count -eq 1) {
                    $supportedGuaranteedOracle =
                        & $isExpectedOracleValue `
                            -Expression $constraint.ArgumentList.Arguments[0].Expression
                    $guaranteedExpectedExpression =
                        $constraint.ArgumentList.Arguments[0].Expression
                } elseif ($constraint -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                    $constraint.Expression.ToString() -ceq 'Is' -and
                    $constraint.Name.Identifier.ValueText -cin @('True', 'False')) {
                    $supportedGuaranteedOracle = $true
                }
            }
            if ($supportedGuaranteedOracle -and
                $null -ne $guaranteedObservationExpression) {
                $unwrappedObservation = & $unwrapAssertionExpression `
                    -Expression $guaranteedObservationExpression
                $observationSymbol =
                    $semanticModel.GetSymbolInfo($unwrappedObservation).Symbol
                if ($observationSymbol -is [Microsoft.CodeAnalysis.ILocalSymbol]) {
                    $initializer = @($observationSymbol.DeclaringSyntaxReferences |
                        ForEach-Object {
                            $_.GetSyntax([Threading.CancellationToken]::None)
                        } |
                        Where-Object {
                            $_ -is
                                [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and
                            $null -ne $_.Initializer
                        } |
                        Select-Object -First 1)
                    if ($initializer.Count -ne 1) {
                        $supportedGuaranteedOracle = $false
                    } else {
                        $initialValue = $initializer[0].Initializer.Value
                        $initialBoolean = & $getObviousBooleanValue `
                            -Expression $initialValue
                        if (($assertionSymbol.Name -cin @('True', 'IsTrue') -and
                                $initialBoolean -ceq 'true') -or
                            ($assertionSymbol.Name -cin @('False', 'IsFalse') -and
                                $initialBoolean -ceq 'false')) {
                            $supportedGuaranteedOracle = $false
                        }
                        if ($null -ne $guaranteedExpectedExpression -and
                            (& $normalizeAssertionExpression `
                                -Expression $initialValue) -ceq
                            (& $normalizeAssertionExpression `
                                -Expression $guaranteedExpectedExpression)) {
                            $supportedGuaranteedOracle = $false
                        }
                        $observedProperties = @(
                            $guaranteedObservationExpression.DescendantNodesAndSelf() |
                                Where-Object {
                                    $_ -is
                                        [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
                                } |
                                ForEach-Object {
                                    $semanticModel.GetSymbolInfo($_).Symbol
                                } |
                                Where-Object {
                                    $_ -is [Microsoft.CodeAnalysis.IPropertySymbol] -and
                                    -not $_.IsStatic -and
                                    $_.ContainingAssembly.Name -ceq
                                        $trustedContractAssembly
                                } |
                                Select-Object -Unique)
                        foreach ($observedProperty in $observedProperties) {
                            $testWrites = @($testMethod[0].Body.DescendantNodes() |
                                Where-Object {
                                    if ($_ -isnot
                                            [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                                        $gate.Span.Contains($_.Span)) {
                                        return $false
                                    }
                                    $writtenSymbol =
                                        $semanticModel.GetSymbolInfo($_.Left).Symbol
                                    return (
                                        $null -ne $writtenSymbol -and
                                        [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                            $writtenSymbol,
                                            $observedProperty))
                                })
                            if ($testWrites.Count -ne 0) {
                                $supportedGuaranteedOracle = $false
                                break
                            }
                        }
                    }
                    $directObservedProperties = @(
                        $guaranteedObservationExpression.DescendantNodesAndSelf() |
                            Where-Object {
                                $_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
                            } |
                            ForEach-Object {
                                $semanticModel.GetSymbolInfo($_).Symbol
                            } |
                            Where-Object {
                                $_ -is [Microsoft.CodeAnalysis.IPropertySymbol] -and
                                -not $_.IsStatic -and
                                $_.ContainingAssembly.Name -ceq
                                    $trustedContractAssembly
                            } |
                            Select-Object -Unique)
                    foreach ($directObservedProperty in $directObservedProperties) {
                        $directTestWrites = @($testMethod[0].Body.DescendantNodes() |
                            Where-Object {
                                if ($_ -isnot
                                        [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                                    $gate.Span.Contains($_.Span)) {
                                    return $false
                                }
                                $writtenSymbol =
                                    $semanticModel.GetSymbolInfo($_.Left).Symbol
                                return (
                                    $null -ne $writtenSymbol -and
                                    [Microsoft.CodeAnalysis.SymbolEqualityComparer]::Default.Equals(
                                        $writtenSymbol,
                                        $directObservedProperty))
                            })
                        if ($directTestWrites.Count -ne 0) {
                            $supportedGuaranteedOracle = $false
                            break
                        }
                    }
                    $directObservation = & $unwrapAssertionExpression `
                        -Expression $guaranteedObservationExpression
                    if ($directObservation -is
                        [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) {
                        $syntacticWrites = @($testMethod[0].Body.DescendantNodes() |
                            Where-Object {
                                $_ -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -and
                                -not $gate.Span.Contains($_.Span) -and
                                $_.Left -is
                                    [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
                                $_.Left.Name.Identifier.ValueText -ceq
                                    $directObservation.Name.Identifier.ValueText -and
                                (& $normalizeAssertionExpression `
                                    -Expression $_.Left.Expression) -ceq
                                    (& $normalizeAssertionExpression `
                                        -Expression $directObservation.Expression)
                            })
                        if ($syntacticWrites.Count -ne 0) {
                            $supportedGuaranteedOracle = $false
                        }
                    }
                }
            }
            if ($isTrustedWindowCallbackAssertion) {
                if ($null -eq $guaranteedObservationExpression -and
                    $assertionArguments.Count -ge 2 -and
                    $assertionSymbol.Name -cin @(
                        'Equal',
                        'NotEqual',
                        'AreEqual',
                        'AreNotEqual')) {
                    foreach ($argumentIndex in @(0, 1)) {
                        $candidateObservation = & $unwrapAssertionExpression `
                            -Expression $assertionArguments[$argumentIndex].Expression
                        $candidateProperty =
                            $semanticModel.GetSymbolInfo(
                                $candidateObservation).Symbol
                        if ($candidateProperty -is
                                [Microsoft.CodeAnalysis.IPropertySymbol] -and
                            $candidateProperty.ContainingAssembly.Name -ceq
                                $trustedContractAssembly -and
                            $candidateProperty.ContainingType.ToString() -ceq
                                'UIKit.UILabel' -and
                            $candidateProperty.Name -ceq 'Text') {
                            $guaranteedObservationExpression =
                                $assertionArguments[$argumentIndex].Expression
                            $guaranteedExpectedExpression =
                                $assertionArguments[1 - $argumentIndex].Expression
                            break
                        }
                    }
                }
                $supportedGuaranteedOracle =
                    $null -ne $guaranteedObservationExpression -and
                    (& $isTrustedWindowCallbackNativeOracle `
                        -Expression $guaranteedObservationExpression `
                        -CallbackBody $assertionStatement.Parent `
                        -AssertionMethod $assertionSymbol `
                        -AssertionArgumentCount $assertionArguments.Count `
                        -ExpectedExpression $guaranteedExpectedExpression)
            }
            if (-not $isTautologicalAssertion -and
                $supportedGuaranteedOracle -and
                ($observesFrameworkMember -or $observesChangedLocal)) {
                $postTriggerAssertionCount++
            }
        }
        $argumentNodes = @($assertionExpression.ArgumentList.DescendantNodesAndSelf())
        if (@($argumentNodes | Where-Object {
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] -or
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax] -or
                    ($_.RawKind -in @(
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreIncrementExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PreDecrementExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostIncrementExpression,
                        [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::PostDecrementExpression)) -or
                    ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax] -and
                        $_.RefKindKeyword.RawKind -ne 0)
                }).Count -ne 0) {
            throw (
                'Trusted assertion arguments may not contain writes, increments, ' +
                'lambdas, or ref/out/in values.')
        }
        foreach ($argumentNode in @($argumentNodes | Where-Object {
                    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax]
                })) {
            $argumentSymbol = $semanticModel.GetSymbolInfo($argumentNode).Symbol
            if (($argumentSymbol -is [Microsoft.CodeAnalysis.IMethodSymbol] -or
                    $argumentSymbol -is [Microsoft.CodeAnalysis.IPropertySymbol]) -and
                @($argumentSymbol.Locations | Where-Object {
                        $_.IsInSource
                    }).Count -ne 0) {
                throw (
                    'Trusted assertion arguments may not execute generated ' +
                    'helpers or getters.')
            }
            if ($argumentSymbol -is [Microsoft.CodeAnalysis.IMethodSymbol]) {
                $argumentCallType = $argumentSymbol.ContainingType.ToString()
                if ($argumentCallType -ceq 'System.Convert' -or
                    ($argumentCallType -ceq 'string' -and
                        $argumentSymbol.Name -cin @('Concat', 'Join'))) {
                    throw (
                        'Trusted assertion arguments may not use object-dispatching ' +
                        'conversion or string helpers.')
                }
            }
        }
    }
    if ($postTriggerAssertionCount -eq 0) {
        throw 'The selected test method has no trusted assertion after the trigger.'
    }
    $trueLiteral = $declarator.Initializer.Value
    $variant = $BaselineSource.Substring(0, $trueLiteral.Span.Start) +
        'false' +
        $BaselineSource.Substring($trueLiteral.Span.End)
    $variantAssertions = @(Get-ReplicationAssertionStatements -Source $variant)
    $baselineAssertions = @(Get-ReplicationAssertionStatements -Source $BaselineSource)
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
