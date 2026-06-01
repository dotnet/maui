using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for <see cref="UpdateComponentCodeWriter.GenerateUpdateComponent"/>.
/// </summary>
[Collection("XamlHotReloadTests")]
public class UpdateComponentCodeWriterTests
{
	// ---------------------------------------------------------------------------
	// Helpers
	// ---------------------------------------------------------------------------

	const string MauiXmlns =
		"""xmlns="http://schemas.microsoft.com/dotnet/2021/maui" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" """;

	static SGRootNode Parse(string xaml) =>
		GeneratorHelpers.ParseXaml(xaml, AssemblyAttributes.Empty)
		?? throw new System.Exception("ParseXaml returned null");

	/// <summary>
	/// Generates UC code for the diff between two XAML snippets.
	/// Uses a full MAUI compilation for Roslyn type resolution.
	/// </summary>
	static string? Generate(
		string xamlV1,
		string xamlV2,
		int fromVersion = 1,
		int toVersion = 2,
		INamedTypeSymbol? rootType = null,
		string accessModifier = "public")
	{
		var compilation = CreateMauiCompilation();
		var xmlnsCache = GeneratorHelpers.GetAssemblyAttributes(compilation, default);

		// typeCache is lazily populated; start empty — TryResolveTypeSymbol fills it
		var typeCache = new Dictionary<XmlType, INamedTypeSymbol>();

		var oldRoot = Parse(xamlV1);
		var newRoot = Parse(xamlV2);

		// Set Parent pointers (needed for x:DataType resolution in UC)
		newRoot.Accept(new XamlNodeVisitor((node, parent) => node.Parent = parent), null);

		var diff = XamlNodeDiff.ComputeDiff(oldRoot, newRoot);
		if (diff == null) return null; // structural change

		if (rootType == null)
		{
			// Resolve from x:Class (if present)
			if (newRoot.Properties.TryGetValue(XmlName.xClass, out var classNode)
				&& classNode is ValueNode vn
				&& vn.Value is string className)
			{
				XmlnsHelper.ParseXmlns(className, out var typeName, out var ns, out _, out _);
				rootType = compilation.GetTypeByMetadataName($"{ns}.{typeName}");
			}
		}

		if (rootType == null)
		{
			// Synthesize a minimal stub type (enough for the test)
			rootType = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ContentPage")!;
		}

		return UpdateComponentCodeWriter.GenerateUpdateComponent(
			rootType,
			accessModifier,
			diff,
			fromVersion,
			toVersion,
			compilation,
			xmlnsCache,
			typeCache);
	}

	// ---------------------------------------------------------------------------
	// Empty diff → returns null
	// ---------------------------------------------------------------------------

	[Fact]
	public void EmptyDiff_ReturnsNull()
	{
		var xaml =
$"""
<ContentPage {MauiXmlns} x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var result = Generate(xaml, xaml);
		Assert.Null(result); // diff is empty → no UC file needed
	}

	// ---------------------------------------------------------------------------
	// Structural change → diff is null → UC not generated
	// ---------------------------------------------------------------------------

	[Fact]
	public void ChildAdd_ProducesUCWithChildListChange()
	{
		var v1 =
$"""
<ContentPage {MauiXmlns} x:Class="Test.TestPage">
	<Label Text="Hello" />
</ContentPage>
""";
		var v2 =
$"""
<ContentPage {MauiXmlns} x:Class="Test.TestPage">
	<Label Text="Hello" />
	<Button Text="New" />
</ContentPage>
""";
		// Child add → UC generated with child list change handling
		var result = Generate(v1, v2);
		Assert.NotNull(result);
		Assert.Contains("__version == 1", result!, StringComparison.Ordinal);
		// No goto fallback label or fallback block
		Assert.DoesNotContain("goto fallback", result!, StringComparison.Ordinal);
		Assert.DoesNotContain("fallback:", result!, StringComparison.Ordinal);
	}

	// ---------------------------------------------------------------------------
	// Single property change → method structure
	// ---------------------------------------------------------------------------

	[Fact]
	public void SinglePropertyChange_GeneratesMethodHeader()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"Hello\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"World\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		Assert.Contains("UpdateComponent", result, System.StringComparison.Ordinal);
		// Must NOT contain old versioned method names
		Assert.DoesNotContain("UpdateComponent_v", result, System.StringComparison.Ordinal);
	}

	[Fact]
	public void SinglePropertyChange_ContainsVersionGuard()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"Hello\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"World\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		// Per spec: if (__version == fromVersion) { ... } — uses == not !=
		Assert.Contains("if (__version == 1)", result, System.StringComparison.Ordinal);
	}

	[Fact]
	public void SinglePropertyChange_NoFallbackLabel()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"Hello\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"World\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		Assert.DoesNotContain("fallback:", result, System.StringComparison.Ordinal);
		Assert.DoesNotContain("InitializeComponentRuntime", result, System.StringComparison.Ordinal);
	}

	[Fact]
	public void SinglePropertyChange_ContainsVersionBump()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"Hello\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"World\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		Assert.Contains("__version = 2;", result, System.StringComparison.Ordinal);
	}

	[Fact]
	public void SinglePropertyChange_RegistryLookupPresent()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"Hello\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"World\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		Assert.Contains("XamlComponentRegistry.TryGet(this,", result, System.StringComparison.Ordinal);
		// With numeric IDs: direct child of root is "0"
		Assert.Contains("\"0\"", result, System.StringComparison.Ordinal);
	}

	[Fact]
	public void SinglePropertyChange_NewValueInOutput()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"Hello\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"World\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		// The new value "World" must appear in the generated property assignment
		Assert.Contains("World", result, System.StringComparison.Ordinal);
	}

	// ---------------------------------------------------------------------------
	// Custom from/to versions
	// ---------------------------------------------------------------------------

	[Fact]
	public void CustomVersions_MethodNameIncludesVersions()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"A\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"B\" /></ContentPage>";

		var result = Generate(v1, v2, fromVersion: 5, toVersion: 6);
		Assert.NotNull(result);
		// Single UpdateComponent() method with if (__version == 5) guard
		Assert.Contains("void UpdateComponent()", result, System.StringComparison.Ordinal);
		Assert.Contains("if (__version == 5)", result, System.StringComparison.Ordinal);
		Assert.Contains("__version = 6;", result, System.StringComparison.Ordinal);
	}

	// ---------------------------------------------------------------------------
	// Multiple nodes changed
	// ---------------------------------------------------------------------------

	[Fact]
	public void MultipleNodes_MultipleRegistryLookups()
	{
		var v1 =
$"""
<ContentPage {MauiXmlns} x:Class="Test.TestPage">
	<VerticalStackLayout>
		<Label Text="A" />
		<Label Text="B" />
	</VerticalStackLayout>
</ContentPage>
""";
		var v2 =
$"""
<ContentPage {MauiXmlns} x:Class="Test.TestPage">
	<VerticalStackLayout>
		<Label Text="X" />
		<Label Text="Y" />
	</VerticalStackLayout>
</ContentPage>
""";
		var result = Generate(v1, v2);
		Assert.NotNull(result);

		// Two distinct node IDs should appear (numeric IDs: "1" and "2" for children of VerticalStackLayout)
		Assert.Contains("\"1\"", result, System.StringComparison.Ordinal);
		Assert.Contains("\"2\"", result, System.StringComparison.Ordinal);
		// Both TryGet calls present
		var lookupCount = CountOccurrences(result!, "XamlComponentRegistry.TryGet(this,");
		Assert.True(lookupCount >= 2, $"Expected at least 2 TryGet calls, got {lookupCount}");
	}

	// ---------------------------------------------------------------------------
	// EditorBrowsable attribute on the method
	// ---------------------------------------------------------------------------

	[Fact]
	public void GeneratedMethod_HasEditorBrowsableNeverAttribute()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"A\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"B\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		Assert.Contains("EditorBrowsable", result, System.StringComparison.Ordinal);
		Assert.Contains("Never", result, System.StringComparison.Ordinal);
	}

	// ---------------------------------------------------------------------------
	// Auto-generated header
	// ---------------------------------------------------------------------------

	[Fact]
	public void GeneratedFile_ContainsAutoGeneratedHeader()
	{
		var v1 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"A\" /></ContentPage>";
		var v2 = $"<ContentPage {MauiXmlns} x:Class=\"Test.TestPage\"><Label Text=\"B\" /></ContentPage>";

		var result = Generate(v1, v2);
		Assert.NotNull(result);
		Assert.Contains("<auto-generated>", result, System.StringComparison.Ordinal);
	}

	// ---------------------------------------------------------------------------
	// Child reorder → generates reorder code
	// ---------------------------------------------------------------------------

	[Fact]
	public void ReorderedChildren_GeneratesReorderCode()
	{
		var v1 =
$"""
<ContentPage {MauiXmlns} x:Class="Test.TestPage">
	<VerticalStackLayout>
		<Label Text="A" />
		<Button Text="B" />
	</VerticalStackLayout>
</ContentPage>
""";
		var v2 =
$"""
<ContentPage {MauiXmlns} x:Class="Test.TestPage">
	<VerticalStackLayout>
		<Button Text="B" />
		<Label Text="A" />
	</VerticalStackLayout>
</ContentPage>
""";
		var result = Generate(v1, v2);
		Assert.NotNull(result);

		// Should contain parent registry lookup (numeric ID "0" for VerticalStackLayout)
		Assert.Contains("\"0\"", result, System.StringComparison.Ordinal);
		// Should cast parent to Layout
		Assert.Contains("as global::Microsoft.Maui.Controls.Layout", result, System.StringComparison.Ordinal);
		// Should contain Clear and Add calls
		Assert.Contains(".Clear()", result, System.StringComparison.Ordinal);
		Assert.Contains(".Add(", result, System.StringComparison.Ordinal);
		// With stable IDs, retained children keep their old IDs — no ReRoot needed
		Assert.DoesNotContain("ReRoot", result, System.StringComparison.Ordinal);
	}

	// helpers
	static int CountOccurrences(string source, string pattern)
	{
		int count = 0;
		int idx = 0;
		while ((idx = source.IndexOf(pattern, idx, System.StringComparison.Ordinal)) >= 0)
		{
			count++;
			idx += pattern.Length;
		}
		return count;
	}
}
