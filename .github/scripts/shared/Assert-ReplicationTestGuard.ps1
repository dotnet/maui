#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

function Get-ReplicationUnsafeSourcePatterns {
    # Scope 'raw' rules scan comments and string literals too, because hiding
    # their payload there still matters. Scope 'code' rules describe executable
    # API shape, so they scan with comments removed and must not match ordinary
    # English prose such as "process", "assembly", "timer" or "browser".
    return @(
        [pscustomobject]@{ Code = 'file-system'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*IO\b|\b(?:File|Directory)\s*\.\s*(?:Exists|Open|Create|Delete|Read|Write|Append|Copy|Move|Replace|Enumerate|Get|Set)|\bPath\s*\.\s*(?:Combine|Join|GetFullPath|GetTempPath|GetTempFileName|GetDirectoryName|GetFileName|GetFileNameWithoutExtension|GetExtension|GetRandomFileName|GetPathRoot)|\b(?:FileInfo|DirectoryInfo|FileStream|StreamReader|StreamWriter|FileSystemWatcher|StorageFile|StorageFolder|NSFileManager|NSData)\b|\b(?:Java|Android)\s*\.\s*IO\b|\b(?:XmlDocument|XDocument)\s*\.\s*Load\b|\bXmlReader\s*\.\s*Create\b' },
        [pscustomobject]@{ Code = 'http-client'; Scope = 'code'; Pattern = '(?i)\b(?:HttpClient|HttpMessageHandler|HttpRequestMessage|HttpWebRequest|SocketsHttpHandler|RestClient|GrpcChannel)\b' },
        [pscustomobject]@{ Code = 'network'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Net\b|\b(?:Dns|WebClient|WebRequest|TcpClient|TcpListener|UdpClient|WebSocket|ClientWebSocket|NSUrlSession|NSURLSession)\b|\bSocket\s*[.(]|\bnew\s+\w*Socket\b|\b(?:Java|Android)\s*\.\s*Net\b' },
        [pscustomobject]@{ Code = 'process-start'; Scope = 'code'; Pattern = '\bSystem\s*\.\s*Diagnostics\s*\.\s*Process\b|\bProcess\s*\.\s*(?:Start|GetCurrentProcess|GetProcess|GetProcesses|GetProcessById|EnterDebugMode)\b|\bnew\s+Process\s*[({]|(?i)\b(?:ProcessStartInfo|UseShellExecute|RedirectStandardOutput|WaitForExit|Win32Exception|ManagementObject|NSTask|NSWorkspace)\b' },
        [pscustomobject]@{ Code = 'reflection'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Reflection\b|\bAssembly\s*\.|\.\s*Assembly\b|\b(?:Activator|AppDomain|MethodInfo|PropertyInfo|FieldInfo|ConstructorInfo|DynamicMethod|InvokeMember|GetMethod|GetProperty|GetField)\b|\bGetType\s*\(\s*\)\s*\.\s*(?!Name\b|FullName\b|ToString\b)' },
        [pscustomobject]@{ Code = 'native-code'; Scope = 'code'; Pattern = '(?i)\bSystem\s*\.\s*Runtime\s*\.\s*InteropServices\b(?!\s*\.\s*COMException\b)|\b(?:DllImport|LibraryImport|GeneratedDllImport|NativeLibrary|UnmanagedCallersOnly|GetDelegateForFunctionPointer|GCHandle)\b|\bMarshal\s*\.|(?-i:\bunsafe\s*[{(]|\bunsafe\s+(?:static|void|partial|class|struct|int|byte|char|fixed)\b|\bstackalloc\b|\bextern\s+(?:static|alias)\b|\bstatic\s+extern\b)' },
        [pscustomobject]@{ Code = 'environment-secrets'; Scope = 'raw'; Pattern = '(?i)\bEnvironment\s*\.|\bEnvironmentVariableTarget\b|\b(?:GH_TOKEN|GITHUB_TOKEN|COPILOT_GITHUB_TOKEN|SYSTEM_ACCESSTOKEN|AZURE_STORAGE_KEY|AZURE_STORAGE_SAS_TOKEN)\b' },
        [pscustomobject]@{ Code = 'device-external-access'; Scope = 'code'; Pattern = '(?i)\b(?:Browser|Launcher|SecureStorage|FileSystem|Connectivity|Clipboard|Preferences)\s*\.|\b(?:HybridWebView|BlazorWebView|UrlWebViewSource|UriImageSource|FileImageSource|UIApplication|PendingIntent)\b' },
        [pscustomobject]@{ Code = 'global-exception-suppression'; Scope = 'code'; Pattern = '(?i)\b(?:UnhandledException|UnobservedTaskException|FirstChanceException|MarshalManagedException|AndroidEnvironment)\b'; Remedy = 'A reproduction must not take over the process-wide failure path. Suppressing the crash puts the app in a state a user never sees, and it hides the one symptom the harness can observe without the app reporting on itself. End the Appium plan with assertAppClosed instead, which now works on every platform. If the report names an exact managed exception type, wrap only the reported trigger in a try/catch for that exact type and set the semantic result element from the catch.' },
        [pscustomobject]@{ Code = 'delays-or-background-work'; Scope = 'code'; Pattern = '(?i)\bThread\s*\.\s*Sleep\b|\bTask\s*\.\s*(?:Delay|Run|Factory)\b|\b(?:DispatcherTimer|IDispatcherTimer)\b|\bSystem\s*\.\s*(?:Timers|Threading)\s*\.\s*Timer\b|\bnew\s+\w*Timer\s*\(|\b(?:Create|Start)Timer\s*\(|\bDispatchDelayed\b'; Remedy = 'Write one of these instead. Subscribe to the event that reports the change (Loaded, SizeChanged, PropertyChanged, or the control''s own event) and publish the result from its handler. Or post the measurement with Dispatcher.Dispatch(() => ...), which runs after the pending layout pass without waiting on the clock. Or give the page a separate check control and let the Appium plan tap trigger, wait, then tap check. Waiting on wall-clock time inside the app is never accepted, so re-sending it will fail this attempt again.' },
        [pscustomobject]@{ Code = 'shell-execution'; Scope = 'code'; Pattern = '(?i)\b(?:powershell|pwsh)(?:\.exe)?\s*(?:-|\.exe\b)|\bcmd\.exe\b|/(?:bin/)?(?:ba|z)?sh\b|\bbash\s+-|\bSystem\s*\.\s*Management\s*\.\s*Automation\b' },
        [pscustomobject]@{ Code = 'remote-url'; Scope = 'raw'; Pattern = '(?i)\b(?:https?|ftps?|wss?|file)\b\s*(?::|["'']\s*\+\s*["'']\s*:)|://' },
        [pscustomobject]@{ Code = 'package-reference'; Scope = 'raw'; Pattern = '(?i)\b(?:PackageReference|PackageDownload|dotnet\s+add\s+package|nuget\s*:|nuget\.exe)\b|#(?:r|load)\b' },
        [pscustomobject]@{ Code = 'obfuscated-source'; Scope = 'raw'; Pattern = '\\(?:u[0-9a-fA-F]{4}|U[0-9a-fA-F]{8})' },
        [pscustomobject]@{ Code = 'verification-spoof'; Scope = 'raw'; Pattern = '(?i)\bVERIFICATION\s+(?:PASSED|FAILED|INCONCLUSIVE)\b|##vso\[|::set-output' },
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
        [Parameter(Mandatory = $true)][string]$Path
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
        foreach ($m in [regex]::Matches($Text, '=\s*(?<quote>["''])(?<value>[^"'']*)\k<quote>')) {
            $value = $m.Groups['value']
            if ($value.Value.Contains('{')) { continue }
            & $blank $value.Index ($value.Index + $value.Length)
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
    $codeText = Get-ReplicationCommentFreeText -Text $scanText -Path $Path
    foreach ($entry in Get-ReplicationUnsafeSourcePatterns) {
        $scope = if ($entry.PSObject.Properties['Scope']) { [string]$entry.Scope } else { 'raw' }
        $target = if ($scope -eq 'code') { $codeText } else { $scanText }
        $match = [regex]::Match($target, $entry.Pattern)
        if ($match.Success) {
            $detail = "Candidate source '$Path' contains prohibited '$($entry.Code)' content: $(Get-ReplicationUnsafeMatchDetail -ScanText $target -Match $match)"
            if ($entry.PSObject.Properties['Remedy'] -and -not [string]::IsNullOrWhiteSpace($entry.Remedy)) {
                $detail += " $($entry.Remedy)"
            }

            throw $detail
        }
    }

    # MAUI permits AutomationId to be set only once, so reassigning it to signal
    # progress throws InvalidOperationException and produces a failure that has
    # nothing to do with the reported bug.
    $automationAssignments = [regex]::Matches(
        $codeText,
        '(?<target>[A-Za-z_]\w*)\s*\.\s*AutomationId\s*=(?!=)')
    foreach ($group in ($automationAssignments | Group-Object { $_.Groups['target'].Value })) {
        if ($group.Count -le 1) {
            continue
        }

        $detail = Get-ReplicationUnsafeMatchDetail -ScanText $codeText -Match $group.Group[1]
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
            throw ("The reproduction '$TestFilter' nominates a non-falsifiable oracle: its expected failure is $($oracle.Reason). " +
                'Assert the reported behavior directly, so that a product fix turns this exact test green.')
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

    $relational = $null
    foreach ($match in [regex]::Matches(
        $code,
        '(?:Assert|ClassicAssert)\s*\.\s*(?:Equal|AreEqual|True|IsTrue)\s*\((?<args>[^;]*?)\)\s*;')) {
        $arguments = @(Split-ReplicationAssertionArguments -Arguments $match.Groups['args'].Value)

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
        $arguments = @(Split-ReplicationAssertionArguments -Arguments $match.Groups['args'].Value)
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
        issue-keyed category sits alongside the conventional
        [Category(TestCategory.Entry)] without touching TestCategory -- which
        matters, because editing that shared file is not add-only.

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

function Get-ReplicationEnvironmentCapabilityPattern {
    # Calls that report which lane the test landed in, not what the product did.
    return '(?:OperatingSystem\.Is[A-Za-z]*VersionAtLeast|UIDevice\.CurrentDevice\.CheckSystemVersion|Build\.VERSION\.SdkInt)'
}

function Assert-ReplicationEnvironmentGateSkips {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Path
    )

    # A reproduction pinned to an OS floor is fine; nominating that floor as a
    # failure is not. PR 213 asserted `OperatingSystem.IsIOSVersionAtLeast(26)`
    # on line 25 and every one of its five runs died there in 1.8-3.0 ms,
    # before the oracle it advertised ever executed -- red for the lane, not
    # for the defect, and still red after a complete product fix. Scanning
    # 11,012 files, no Assert of any kind names one of these APIs and no
    # version gate throws, while 49 sites gate with an early return, so this
    # rejects a shape the repository never uses.
    $source = Get-ReplicationCommentFreeText -Text $Content -Path $Path
    $capability = Get-ReplicationEnvironmentCapabilityPattern
    $remedy = ('Gate with the shape this repository uses instead -- ' +
        '"if (!OperatingSystem.IsIOSVersionAtLeast(26)) return;" -- so an ' +
        'unsupported lane skips quietly and the only red this test can ' +
        'produce is the reported defect.')

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
    foreach ($rule in $lifecycleRules) {
        $match = [regex]::Match($Content, $rule.Pattern)
        if ($match.Success) {
            throw "Candidate test source '$Path' contains $($rule.Reason): $(Get-ReplicationUnsafeMatchDetail -ScanText ($Content.Replace("`r`n", "`n")) -Match $match). Move the setup inside the test method body."
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
