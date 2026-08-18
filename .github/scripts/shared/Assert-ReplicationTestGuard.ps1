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
        [pscustomobject]@{ Code = 'device-external-access'; Scope = 'code'; Pattern = '(?i)\b(?:Browser|Launcher|SecureStorage|FileSystem|Connectivity|Clipboard|Preferences)\s*\.|\b(?:WebView|HybridWebView|BlazorWebView|UriImageSource|FileImageSource|UIApplication|PendingIntent)\b' },
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
