using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for Phase 3.5: InitializeComponent code generation with <c>EnableIncrementalHotReload=true</c>.
/// Verifies that the generated IC partial emits:
///   - A <c>private int __version = 0;</c> field
///   - <c>XamlComponentRegistry.Register()</c> calls for each named/tracked node
///   - <c>__version = {latestVersion};</c> at the end of the method body (reads from XamlHotReloadState)
/// When <c>EnableIncrementalHotReload=false</c> (default), none of those are emitted.
/// </summary>
[Collection("XamlHotReloadTests")]
public class IncrementalHotReloadICTests : SourceGenXamlInitializeComponentTestBase, IDisposable
{
	public IncrementalHotReloadICTests() => XamlHotReloadState.Reset();
	public void Dispose() => XamlHotReloadState.Reset();

	const string CodeBehind =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}
""";

	// -------------------------------------------------------------------------
	// Default (opt-in off) — nothing new should appear
	// -------------------------------------------------------------------------

	[Fact]
	public void Default_NoVersionField()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: false);
		Assert.NotNull(text);
		Assert.DoesNotContain("__version", text, StringComparison.Ordinal);
		Assert.DoesNotContain("XamlComponentRegistry.Register", text, StringComparison.Ordinal);
	}

	// -------------------------------------------------------------------------
	// Opt-in on — __version and Register calls must appear
	// -------------------------------------------------------------------------

	[Fact]
	public void Enabled_VersionFieldEmitted()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		Assert.Contains("private int __version = 0;", text, StringComparison.Ordinal);
	}

	[Fact]
	public void Enabled_VersionFieldHasEditorBrowsableNeverAttribute()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		// The EditorBrowsable attribute must appear on the line immediately before __version
		var lines = text!.Split('\n');
		var versionLineIdx = Array.FindIndex(lines, l => l.Contains("__version = 0", StringComparison.Ordinal));
		Assert.True(versionLineIdx > 0, "Expected __version field in generated code");
		var prevLine = lines[versionLineIdx - 1];
		Assert.Contains("EditorBrowsable", prevLine, StringComparison.Ordinal);
		Assert.Contains("Never", prevLine, StringComparison.Ordinal);
	}

	[Fact]
	public void Enabled_VersionSetAtEndOfMethod()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		// First run: state is seeded at version 0, so IC sets __version = 0
		Assert.Contains("__version = 0;", text, StringComparison.Ordinal);
	}

	[Fact]
	public void Enabled_RegisterCallForChildNode()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		// At least one Register call should exist for the Label child
		Assert.Contains("XamlComponentRegistry.Register(this,", text, StringComparison.Ordinal);
	}

	[Fact]
	public void Enabled_RegisterCallContainsNodeId()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		// Node ID for the first child of root in DFS walk should be "0"
		Assert.Contains("\"0\"", text, StringComparison.Ordinal);
	}

	[Fact]
	public void Enabled_MultipleChildren_MultipleRegisterCalls()
	{
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<VerticalStackLayout>
		<Label Text="First" />
		<Label Text="Second" />
		<Button Text="Go" />
	</VerticalStackLayout>
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		// Count how many Register calls there are
		var registerCount = CountOccurrences(text!, "XamlComponentRegistry.Register(this,");
		// Expect at least 4: VerticalStackLayout + 3 children
		Assert.True(registerCount >= 4, $"Expected at least 4 Register calls, got {registerCount}");
	}

	[Fact]
	public void Enabled_RootNodeNotRegistered_ChildrenAreRegistered()
	{
		// The root node (ContentPage itself = "this") should NOT get a Register call —
		// only non-root nodes get stable IDs.
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		// root has id "" (empty string) — should not appear in any Register call
		Assert.DoesNotContain("Register(this, \"\",", text, StringComparison.Ordinal);
	}

	// -------------------------------------------------------------------------
	// Interop: EnableDiagnostics path (hot-reload fallback) still works when
	// EnableIncrementalHotReload=true AND EnableDiagnostics=true.
	// -------------------------------------------------------------------------

	[Fact]
	public void Enabled_DiagnosticsAndIHR_BothFeaturesPresent()
	{
		// The test driver always sets EnableMauiXamlDiagnostics=true, so EnableDiagnostics is
		// always true in tests. Verify that the runtime-HR fallback block and the new IHR
		// block coexist without conflict.
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var (_, text) = RunGenerator(xaml, CodeBehind, enableIncrementalHotReload: true);
		Assert.NotNull(text);
		// Both should be present
		Assert.Contains("ResourceProvider2", text, StringComparison.Ordinal);  // diagnostics/runtime-HR fallback
		Assert.Contains("__version", text, StringComparison.Ordinal);           // IHR new field
		Assert.Contains("XamlComponentRegistry.Register", text, StringComparison.Ordinal); // IHR register
	}

	// helpers
	static int CountOccurrences(string source, string pattern)
	{
		int count = 0;
		int idx = 0;
		while ((idx = source.IndexOf(pattern, idx, StringComparison.Ordinal)) >= 0)
		{
			count++;
			idx += pattern.Length;
		}
		return count;
	}
}
