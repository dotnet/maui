#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Detect-TestsInDiff.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $platformFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Test-DeviceTestFileAppliesToPlatform'
    }, $true)
    if (-not $platformFunction) {
        throw "Function 'Test-DeviceTestFileAppliesToPlatform' not found"
    }
    Invoke-Expression $platformFunction.Extent.Text

    $methodFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Get-AddedDeviceTestMethodsFromPatch'
    }, $true)
    if (-not $methodFunction) {
        throw "Function 'Get-AddedDeviceTestMethodsFromPatch' not found"
    }
    Invoke-Expression $methodFunction.Extent.Text

    $hasTestMethodsFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Test-CsFileHasTestMethods'
    }, $true)
    if (-not $hasTestMethodsFunction) {
        throw "Function 'Test-CsFileHasTestMethods' not found"
    }
    Invoke-Expression $hasTestMethodsFunction.Extent.Text
}

Describe 'Detect-TestsInDiff source test attributes' {
    It 'recognizes a fully qualified xUnit attribute' {
        $testFile = Join-Path $TestDrive 'QualifiedTests.cs'
        @'
public class QualifiedTests
{
    [Xunit.Fact]
    public void Runs()
    {
    }
}
'@ | Set-Content -LiteralPath $testFile -Encoding UTF8

        Test-CsFileHasTestMethods -RelativePath $testFile | Should -BeTrue
    }

    It 'does not treat a qualified lookalike attribute as a test' {
        $testFile = Join-Path $TestDrive 'QualifiedHelpers.cs'
        @'
public class QualifiedHelpers
{
    [Contoso.TestFactory]
    public void Build()
    {
    }
}
'@ | Set-Content -LiteralPath $testFile -Encoding UTF8

        Test-CsFileHasTestMethods -RelativePath $testFile | Should -BeFalse
    }
}

Describe 'Detect-TestsInDiff device-test filtering' {
    It 'sets class and category filters when a device-test diff has no added method signatures' {
        $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')
        $scriptPath = Join-Path $PSScriptRoot 'Detect-TestsInDiff.ps1'
        $changedFile = 'src/Core/tests/DeviceTests/Handlers/Entry/EntryHandlerTests.cs'

        Push-Location $repoRoot
        try {
            $tests = @(& $scriptPath -ChangedFiles $changedFile)
        } finally {
            Pop-Location
        }

        $test = $tests | Where-Object { $_.Type -eq 'DeviceTest' } | Select-Object -First 1
        $test.Filter | Should -Be 'Category=Entry'
        $test.ClassFilter | Should -Be 'Microsoft.Maui.DeviceTests.EntryHandlerTests'
        $test.Methods | Should -BeNullOrEmpty
    }

    It 'keeps an applicable helper-only platform partial mapped to its shared device tests' {
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("detect-tests-" + [Guid]::NewGuid().ToString('N'))
        $relativeFile = 'src/Controls/tests/DeviceTests/Elements/ContentView/ContentViewTests.Android.cs'
        $platformFile = Join-Path $root $relativeFile
        $sharedFile = Join-Path $root 'src/Controls/tests/DeviceTests/Elements/ContentView/ContentViewTests.cs'

        try {
            New-Item -ItemType Directory -Force -Path (Split-Path $platformFile -Parent) | Out-Null
            @'
namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.ContentView)]
public partial class ContentViewTests
{
    [Fact]
    public void SharedTestUsesPlatformHelper()
    {
        PlatformHelper();
    }
}
'@ | Set-Content $sharedFile -Encoding UTF8
            @'
namespace Microsoft.Maui.DeviceTests;

public partial class ContentViewTests
{
    void PlatformHelper()
    {
    }
}
'@ | Set-Content $platformFile -Encoding UTF8

            Push-Location $root
            try {
                git init -q
                git config user.email tests@example.com
                git config user.name Tests
                git add .
                git commit -q -m base
                $base = (git rev-parse HEAD).Trim()

                @'
namespace Microsoft.Maui.DeviceTests;

public partial class ContentViewTests
{
    void PlatformHelper()
    {
        _ = 1;
    }
}
'@ | Set-Content $platformFile -Encoding UTF8
                git add .
                git commit -q -m helper-change

                $androidTests = @(& $scriptPath `
                    -ChangedFiles $relativeFile `
                    -DiffBase $base `
                    -Platform android)
                $windowsTests = @(& $scriptPath `
                    -ChangedFiles $relativeFile `
                    -DiffBase $base `
                    -Platform windows)
            } finally {
                Pop-Location
            }

            $test = $androidTests | Where-Object { $_.Type -eq 'DeviceTest' } | Select-Object -First 1
            $test | Should -Not -BeNullOrEmpty
            $test.Filter | Should -Be 'Category=ContentView'
            $test.ClassFilter | Should -Be 'Microsoft.Maui.DeviceTests.ContentViewTests'
            $test.Methods | Should -BeNullOrEmpty
            @($windowsTests | Where-Object { $_.Type -eq 'DeviceTest' }).Count | Should -Be 0
        } finally {
            Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'Detect-TestsInDiff platform-specific methods' {
    It 'includes iOS partials for iOS and Mac Catalyst only' {
        $path = 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.iOS.cs'
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform ios | Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform catalyst | Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform android | Should -BeFalse
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform windows | Should -BeFalse
    }

    It 'keeps Mac Catalyst partials exclusive to Mac Catalyst' {
        $path = 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.MacCatalyst.cs'
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform maccatalyst | Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform -Path $path -TargetPlatform ios | Should -BeFalse
    }

    It 'matches Android and Windows partial files only on their target' {
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.Android.cs' `
            -TargetPlatform android |
            Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.Android.cs' `
            -TargetPlatform windows |
            Should -BeFalse
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.Windows.cs' `
            -TargetPlatform windows |
            Should -BeTrue
    }

    It 'keeps shared files and applies platform directory conventions' {
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Handlers/Foo/FooTests.cs' `
            -TargetPlatform windows |
            Should -BeTrue
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Platforms/Android/FooTests.cs' `
            -TargetPlatform windows |
            Should -BeFalse
        Test-DeviceTestFileAppliesToPlatform `
            -Path 'src/Core/tests/DeviceTests/Platforms/iOS/FooTests.cs' `
            -TargetPlatform maccatalyst |
            Should -BeTrue
    }
}

Describe 'Detect-TestsInDiff added device-test methods' {
    It 'includes attributed Task and async Task methods but excludes public helpers' {
        $patch = @'
@@ -0,0 +1,30 @@
+[Fact]
+public Task TextColorCanBeCleared()
+{
+    return Task.CompletedTask;
+}
+
+[Theory]
+[InlineData("red")]
+public async Task IconColorUpdates(string color)
+{
+    await Task.Yield();
+}
+
+public void Enqueue(object request)
+{
+}
'@

        @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch) |
            Should -Be @('TextColorCanBeCleared', 'IconColorUpdates')
    }

    It 'supports namespaced test attributes and attributes on the declaration line' {
        $patch = @'
@@ -0,0 +1,10 @@
+[Xunit.Fact] public void RunsInline()
+{
+}
+
+[NUnit.Framework.Test]
+[Category("Device")]
+public virtual ValueTask RunsWithValueTask()
+{
+    return ValueTask.CompletedTask;
+}
'@

        @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch) |
            Should -Be @('RunsInline', 'RunsWithValueTask')
    }

    It 'keeps multiline Theory and Fact attributes pending through balanced arguments' {
        $patch = @'
@@ -0,0 +1,28 @@
+[Theory(
+    Skip = "",
+    DisplayName = "literal ) and ] " + nameof(Format),
+    Timeout = GetTimeout(new[] { ")", "]" })
+)]
+public async Task TheoryWithArguments(string value)
+{
+    await Task.Yield();
+}
+
+[Fact(
+    Skip = @"verbatim text with ""quotes"",
+a closing-looking ] on another line"
+)]
+public void FactWithArguments()
+{
+}
'@

        @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch) |
            Should -Be @('TheoryWithArguments', 'FactWithArguments')
    }

    It 'supports stacked multiline attributes with comments and directives before the method' {
        $patch = @'
@@ -0,0 +1,24 @@
+[Theory(
+    Skip = GetReason(
+        "nested call")
+)]
+[Trait(
+    "Category",
+    "Device"
+)]
+
+// The declaration remains associated with the test attributes.
+#if TEST_CONFIGURATION
+public virtual ValueTask StackedAttributes()
+{
+    return ValueTask.CompletedTask;
+}
+#endif
'@

        @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch) |
            Should -Be @('StackedAttributes')
    }

    It 'clears a completed test marker when an unrelated declaration intervenes' {
        $patch = @'
@@ -0,0 +1,20 @@
+[Fact(
+    Skip = "temporarily disabled"
+)]
+private const string Reason = "not a method";
+
+public void HelperMustNotInheritFact()
+{
+}
+
+[Theory]
+public Task ActualTest()
+{
+    return Task.CompletedTask;
+}
'@

        @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch) |
            Should -Be @('ActualTest')
    }

    It 'does not leak malformed or unclosed attributes into later methods or hunks' {
        $patch = @'
@@ -0,0 +1,12 @@
+[Theory(
+    Skip = "missing the closing bracket"
+public void HiddenInsideMalformedAttribute()
+{
+}
@@ -40,0 +53,14 @@
+public void MethodInAnotherHunk()
+{
+}
+
+[Fact(
+    Skip = "missing the closing parenthesis"
+]
+public void ClosedButMalformed()
+{
+}
+
+[Fact]
+public void ValidAfterMalformedAttribute()
+{
+}
'@

        @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch) |
            Should -Be @('ValidAfterMalformedAttribute')
    }

    It 'returns no methods when the patch adds only helpers' {
        $patch = @'
@@ -0,0 +1,8 @@
+public async Task WaitForRequest()
+{
+    await Task.Yield();
+}
+
+public void Enqueue(object request)
+{
+}
'@

        @(Get-AddedDeviceTestMethodsFromPatch -Patch $patch) | Should -BeNullOrEmpty
    }

    It 'keeps method selection pinned to DiffBase..HEAD when the worktree changes later' {
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("detect-tests-" + [Guid]::NewGuid().ToString('N'))
        $relativeFile = 'src/Core/tests/DeviceTests/Handlers/Picker/PickerHandlerTests.Android.cs'
        $testFile = Join-Path $root $relativeFile
        $classFile = Join-Path $root 'src/Core/tests/DeviceTests/Handlers/Picker/PickerHandlerTests.cs'

        try {
            New-Item -ItemType Directory -Force -Path (Split-Path $testFile -Parent) | Out-Null
            @'
namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Picker)]
public partial class PickerHandlerTests
{
}
'@ | Set-Content $classFile -Encoding UTF8
            @'
namespace Microsoft.Maui.DeviceTests;

public partial class PickerHandlerTests
{
}
'@ | Set-Content $testFile -Encoding UTF8

            Push-Location $root
            try {
                git init -q
                git config user.email tests@example.com
                git config user.name Tests
                git add .
                git commit -q -m base
                $base = (git rev-parse HEAD).Trim()

                @'
namespace Microsoft.Maui.DeviceTests;

public partial class PickerHandlerTests
{
    [Theory]
    [InlineData(false)]
    public async Task SnapshotMethod(bool useMaterialPicker)
    {
        await Task.Yield();
    }
}
'@ | Set-Content $testFile -Encoding UTF8
                git add .
                git commit -q -m snapshot

                # Simulate a newer live PR/worktree change that is not part of the
                # committed review snapshot selected by the Gate.
                @'

[Theory]
public async Task LaterMethod()
{
    await Task.Yield();
}
'@ | Add-Content $testFile -Encoding UTF8

                $tests = @(& $scriptPath `
                    -ChangedFiles $relativeFile `
                    -DiffBase $base `
                    -Platform android)
            } finally {
                Pop-Location
            }

            $test = $tests | Where-Object { $_.Type -eq 'DeviceTest' } | Select-Object -First 1
            @($test.Methods) | Should -Be @('SnapshotMethod')
        } finally {
            Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'selects the test class when a concrete helper class appears first' {
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("detect-tests-" + [Guid]::NewGuid().ToString('N'))
        $relativeFile = 'src/Controls/tests/DeviceTests/Elements/Shell/ShellHandlerSubclasses.Android.cs'
        $testFile = Join-Path $root $relativeFile

        try {
            New-Item -ItemType Directory -Force -Path (Split-Path $testFile -Parent) | Out-Null
            @'
namespace Microsoft.Maui.DeviceTests;

public class StartupTrackingShellHandler
{
}

[Category(TestCategory.Shell)]
public partial class ShellHandlerTests_Shell
{
}
'@ | Set-Content $testFile -Encoding UTF8

            Push-Location $root
            try {
                git init -q
                git config user.email tests@example.com
                git config user.name Tests
                git add .
                git commit -q -m base
                $base = (git rev-parse HEAD).Trim()

                @'
namespace Microsoft.Maui.DeviceTests;

public class StartupTrackingShellHandler
{
}

[Category(TestCategory.Shell)]
public partial class ShellHandlerTests_Shell
{
    [Fact]
    public async Task SinglePageShellCreatesTabInfrastructureOnlyWhenNeeded()
    {
        await Task.Yield();
    }

    [Fact]
    public async Task SwitchingShellItemsCreatesBottomTabsOnlyWhenNeeded()
    {
        await Task.Yield();
    }
}
'@ | Set-Content $testFile -Encoding UTF8
                git add .
                git commit -q -m tests

                $tests = @(& $scriptPath `
                    -ChangedFiles $relativeFile `
                    -DiffBase $base `
                    -Platform android)
            } finally {
                Pop-Location
            }

            $test = $tests | Where-Object { $_.Type -eq 'DeviceTest' } | Select-Object -First 1
            $test.ClassFilter | Should -Be 'Microsoft.Maui.DeviceTests.ShellHandlerTests_Shell'
            @($test.Methods) | Should -Be @(
                'SinglePageShellCreatesTabInfrastructureOnlyWhenNeeded',
                'SwitchingShellItemsCreatesBottomTabsOnlyWhenNeeded'
            )
        } finally {
            Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'Detect-TestsInDiff PR files cache' {
    It 'gates the fetch on the pinned diff and fetch-attempted sentinel, not on the cache' {
        $scriptContent = Get-Content (Join-Path $PSScriptRoot 'Detect-TestsInDiff.ps1') -Raw

        # `-not @()` is $true, so guarding on the cache alone re-runs `gh api` for every
        # device-test group after a failed or empty fetch.
        $scriptContent | Should -Match '\$PRNumber -and -not \$DiffBase -and -not \$script:_prFilesFetchAttempted'
        $scriptContent | Should -Not -Match '\$PRNumber -and -not \$script:_cachedPRFiles'
    }
}
