#!/usr/bin/env pwsh
#requires -Version 7

$ErrorActionPreference = "Stop"
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "..", ".."))
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-perf-behavior-" + [Guid]::NewGuid().ToString("N"))
$assertionCount = 0

function Assert-Equal($expected, $actual, [string]$message) {
    $script:assertionCount++
    if ($expected -cne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Assert-Categories([string[]]$expected, [string[]]$actual, [string]$message) {
    Assert-Equal (($expected | Sort-Object -CaseSensitive) -join ",") `
        (($actual | Sort-Object -CaseSensitive) -join ",") $message
}

# Use PowerShell's existing C# compiler to execute production method bodies, not
# copies of the policy or assertions about which expressions appear in the source.
Add-Type -AssemblyName Microsoft.CodeAnalysis.CSharp
function Get-SourceMethod([string]$source, [string]$name) {
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
    $methods = @($tree.GetRoot().DescendantNodes() | Where-Object {
        ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -or
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax]) -and
        $_.Identifier.ValueText -ceq $name
    })
    if ($methods.Count -ne 1) {
        throw "Expected one production method named $name, found $($methods.Count)."
    }
    return $methods[0].ToFullString()
}

New-Item -ItemType Directory -Path $testRoot | Out-Null
try {
    $categoryAssembly = Join-Path $testRoot "CategoryFixture.dll"
    Add-Type -OutputAssembly $categoryAssembly -TypeDefinition @'
namespace Microsoft.Maui.DeviceTests
{
    public static class TestCategory
    {
        public const string Button = "Button";
        public const string Shell = "Shell";
        public const string Performance = "Performance";
        public const string Swipe = "PerformanceCarouselViewSwipe";
        public const string Future = "PerformanceFutureScenario";
        public const string Lowercase = "performanceLowercase";
        public const string Other = "NotPerformance";
        public const int NotAString = 42;
        public static string NotAField => "PropertyMustNotBeDiscovered";
    }
}
'@
    $assemblyStream = [IO.MemoryStream]::new([IO.File]::ReadAllBytes($categoryAssembly))
    try {
        $categoryType = [Runtime.Loader.AssemblyLoadContext]::Default.LoadFromStream($assemblyStream).
            GetType("Microsoft.Maui.DeviceTests.TestCategory")
    }
    finally {
        $assemblyStream.Dispose()
    }
    $ordinary = @("Button", "Shell", "performanceLowercase", "NotPerformance")
    $performance = @("Performance", "PerformanceCarouselViewSwipe", "PerformanceFutureScenario")
    $allCategories = $ordinary + $performance

    $cakeSource = Get-Content (Join-Path $repositoryRoot "eng\devices\devices-shared.cake") -Raw
    $selector = Get-SourceMethod $cakeSource "GetTestCategoriesToRunSeparately"
    Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Linq;
public class CakeCategoryFixture
{
    string testFilter;
    public string AssemblyPath;
    public int DiscoveryCalls;
    public ContextStub Context = new ContextStub();
    public class FilePath
    {
        public string FullPath;
        public FilePath GetDirectory() => this;
    }
    public class ContextStub
    {
        public ContextStub GetCallerInfo() => this;
        public FilePath SourceFilePath => new FilePath { FullPath = "." };
    }
    public IEnumerable<FilePath> GetFiles(string pattern)
    {
        DiscoveryCalls++;
        return new[] { new FilePath { FullPath = AssemblyPath } };
    }
    public void Information(string message) { }
    public void Warning(string message) { }
    public List<string> Select(string project, string filter)
    {
        testFilter = filter;
        DiscoveryCalls = 0;
        return GetTestCategoriesToRunSeparately(project);
    }
    $selector
}
"@
    $cake = [CakeCategoryFixture]::new()
    $cake.AssemblyPath = $categoryAssembly
    foreach ($project in @("Controls.DeviceTests.csproj", "Core.DeviceTests.csproj")) {
        Assert-Categories ($ordinary | ForEach-Object { "Category=$_" }) `
            $cake.Select($project, $null) "Cake ordinary $project discovery excludes all Performance* categories"
        Assert-Equal 1 $cake.DiscoveryCalls "Ordinary selection actually discovers an assembly"
        foreach ($filter in @("Category=Performance", "Category=PerformanceFutureScenario", "Category=Button", "SkipCategories=Shell")) {
            Assert-Categories @($filter) $cake.Select($project, $filter) "Cake preserves explicit '$filter'"
            Assert-Equal 0 $cake.DiscoveryCalls "Explicit filters do not require category discovery"
        }
    }
    Assert-Categories @("") $cake.Select("Essentials.DeviceTests.csproj", $null) "Unsharded projects retain their default run"
    Assert-Equal 0 $cake.DiscoveryCalls "Unsharded projects do not discover category shards"

    $helperSource = Get-Content (Join-Path $repositoryRoot "src\Core\tests\DeviceTests.Shared\DeviceTestSharedHelpers.cs") -Raw
    foreach ($platform in @("ANDROID", "IOS", "MACCATALYST")) {
        $fixtureNamespace = "CategoryFixture$platform"
        $source = $helperSource.Replace("Microsoft.Maui", "$fixtureNamespace.Microsoft.Maui").
            Replace("Foundation.", "$fixtureNamespace.Foundation.")
        $source += @"

#nullable disable
namespace $fixtureNamespace
{
    public static class Input
    {
        public static string Filter;
        public static bool HasInstrumentation = true;
    }
    public static class Fixture
    {
        public static System.Collections.Generic.List<string> Excluded(System.Type categories, string filter)
        {
            Input.Filter = filter;
            return Microsoft.Maui.DeviceTests.DeviceTestSharedHelpers.GetExcludedTestCategories(categories);
        }
    }
}
namespace $fixtureNamespace.Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
    public class MauiTestInstrumentation
    {
        public static MauiTestInstrumentation Current => $fixtureNamespace.Input.HasInstrumentation ? new MauiTestInstrumentation() : null;
        public MauiTestInstrumentation Arguments => this;
        public string GetString(string key) => key == "TestFilter" ? $fixtureNamespace.Input.Filter : null;
    }
}
namespace $fixtureNamespace.Foundation
{
    public class NSProcessInfo
    {
        public static NSProcessInfo ProcessInfo => new NSProcessInfo();
        public System.Collections.Generic.Dictionary<string, string> Environment
        {
            get
            {
                var values = new System.Collections.Generic.Dictionary<string, string>();
                values.Add("Unrelated", "Category=Wrong");
                if ($fixtureNamespace.Input.Filter != null)
                    values.Add("TestFilter", $fixtureNamespace.Input.Filter);
                return values;
            }
        }
    }
}
"@
        # The test shims intentionally permit absent platform inputs.
        Add-Type -TypeDefinition $source -CompilerOptions "/define:$platform", "/nullable:disable"
        $fixture = "$fixtureNamespace.Fixture" -as [type]
        $excluded = $fixture.GetMethod("Excluded")
        foreach ($filter in @($null, "", "UnknownFilter")) {
            $actual = $excluded.Invoke($null, @($categoryType, $filter))
            Assert-Categories ($performance | ForEach-Object { "Category=$_" }) $actual "$platform ordinary '$filter' run excludes performance"
        }
        foreach ($category in @("Button", "PerformanceCarouselViewSwipe", "PerformanceFutureScenario", "UnknownCategory")) {
            $actual = $excluded.Invoke($null, @($categoryType, "Category=$category"))
            $expected = $allCategories | Where-Object { $_ -cne $category } | ForEach-Object { "Category=$_" }
            Assert-Categories $expected $actual "$platform selects only '$category'"
        }
        Assert-Categories ($ordinary | ForEach-Object { "Category=$_" }) `
            $excluded.Invoke($null, @($categoryType, "Category=Performance")) "$platform explicit aggregate opts into all Performance* categories"
        Assert-Categories (@("Category=Shell", "Category=Button") + ($performance | ForEach-Object { "Category=$_" })) `
            $excluded.Invoke($null, @($categoryType, "SkipCategories= Shell, Button ; Performance")) "$platform skip-list retains performance isolation without duplicate exclusions"
        if ($platform -eq "ANDROID") {
            $inputType = "$fixtureNamespace.Input" -as [type]
            $inputType.GetField("HasInstrumentation").SetValue($null, $false)
            Assert-Categories ($performance | ForEach-Object { "Category=$_" }) `
                $excluded.Invoke($null, @($categoryType, "Category=Performance")) "Android missing instrumentation fails closed to ordinary exclusions"
        }
    }

    $scenarioSource = Get-Content (Join-Path $repositoryRoot "src\Controls\tests\DeviceTests\Performance\CarouselViewSwipePerformanceTests.iOS.cs") -Raw
    $viewSource = Get-Content (Join-Path $repositoryRoot "src\Controls\src\Core\Handlers\Items\iOS\MauiCollectionView.cs") -Raw
    $scenarioMethod = Get-SourceMethod $scenarioSource "ApplyLayoutAfterNativeStateReset"
    $embeddedViews = Get-SourceMethod $scenarioSource "GetEmbeddedScrollViews"
    $setSwipe = Get-SourceMethod $viewSource "SetSwipeEnabled"
    $addSubview = Get-SourceMethod $viewSource "AddSubview"
    $layout = Get-SourceMethod $viewSource "LayoutSubviews"
    $applySwipe = Get-SourceMethod $viewSource "ApplySwipeEnabledToEmbeddedScrollViews"
    $applyOuterBounce = Get-SourceMethod $viewSource "ApplyOuterBounceState"
    $applyState = Get-SourceMethod $viewSource "ApplyState"
    $scenarioTree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($scenarioSource)
    $iterations = @($scenarioTree.GetRoot().DescendantNodes() | Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax] -and
        $_.Declaration.Variables.Identifier.ValueText -contains "LayoutsPerIteration"
    })
    Assert-Equal 1 $iterations.Count "Use the production workload iteration count"
    Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using Microsoft.Maui.Controls.Handlers.Items;
namespace UIKit
{
    public class UIView
    {
        readonly List<UIView> children = new List<UIView>();
        public UIView[] Subviews => children.ToArray();
        public virtual void AddSubview(UIView view) => children.Add(view);
        public virtual void LayoutSubviews() { }
    }
    public class UIScrollView : UIView
    {
        public bool ScrollEnabled = true, Bounces = true, AlwaysBounceHorizontal = true, AlwaysBounceVertical = true;
    }
    public class UICollectionView : UIScrollView { }
}
namespace Microsoft.Maui.Controls.Handlers.Items
{
    public class MauiCollectionView : UICollectionView
    {
        bool? _isSwipeEnabled;
        bool? _isBounceEnabled = false;
        $setSwipe
        $addSubview
        $layout
        $applySwipe
        $applyOuterBounce
        $applyState
    }
}
public static class ApplePerformanceFixture
{
    $($iterations[0].ToFullString())
    $scenarioMethod
    $embeddedViews
    public static int[] Run(bool swipe, bool configured, bool hasEmbeddedView)
    {
        var view = new MauiCollectionView();
        if (hasEmbeddedView)
            view.AddSubview(new UIScrollView());
        view.AddSubview(new UICollectionView());
        if (configured)
            view.SetSwipeEnabled(swipe);
        var result = ApplyLayoutAfterNativeStateReset(view);
        return new[] { result.EmbeddedScrollViewCount, result.Failures };
    }
    public static bool NewlyAddedScrollerIsDisabled()
    {
        var view = new MauiCollectionView();
        view.SetSwipeEnabled(false);
        var scroller = new UIScrollView();
        view.AddSubview(scroller);
        return !scroller.ScrollEnabled && !scroller.Bounces &&
            !scroller.AlwaysBounceHorizontal && !scroller.AlwaysBounceVertical;
    }
}
"@
    $result = [ApplePerformanceFixture]::Run($false, $true, $true)
    Assert-Equal 1 $result[0] "The Apple workload observes the embedded scroller, not nested collection views"
    Assert-Equal 0 $result[1] "Real layout reapplication restores disabled swipe and bounce state"
    Assert-Equal $true ([ApplePerformanceFixture]::Run($true, $true, $true)[1] -gt 0) "Enabled swipe must fail the workload's correctness checks"
    Assert-Equal $true ([ApplePerformanceFixture]::Run($false, $false, $true)[1] -gt 0) "Missing cached state must fail the workload's correctness checks"
    Assert-Equal $true ([ApplePerformanceFixture]::Run($false, $true, $false)[1] -gt 0) "Missing embedded scrollers must not pass vacuously"
    Assert-Equal $true ([ApplePerformanceFixture]::NewlyAddedScrollerIsDisabled()) "The real native-view method reapplies cached swipe state to new scrollers"

    Write-Host "Device performance behavioral tests passed ($assertionCount assertions)."
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
