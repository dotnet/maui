using System;
using System.Linq;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for <see cref="XamlNodeDiff.ComputeDiff"/>.
/// Tests use <see cref="GeneratorHelpers.ParseXaml"/> to build real <see cref="SGRootNode"/>
/// trees — no stubs or mocks needed.
/// </summary>
public class XamlNodeDiffTests
{
	// ---------------------------------------------------------------------------
	// Helpers
	// ---------------------------------------------------------------------------

	/// <summary>
	/// Parses a XAML snippet and returns the root ElementNode.
	/// Uses AssemblyAttributes.Empty so no xmlns resolution is required for simple tests.
	/// </summary>
	static SGRootNode Parse(string xaml) =>
		GeneratorHelpers.ParseXaml(xaml, AssemblyAttributes.Empty)
		?? throw new System.Exception("ParseXaml returned null");

	/// <summary>Minimal MAUI xmlns header for test XAML.</summary>
	const string MauiXmlns =
		"""xmlns="http://schemas.microsoft.com/dotnet/2021/maui" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" """;

	static string Page(string content, string? extraAttrs = null) =>
		$"""<ContentPage {MauiXmlns} x:Class="Test.MyPage" {extraAttrs}>{content}</ContentPage>""";

	// ---------------------------------------------------------------------------
	// Identical trees → empty diff
	// ---------------------------------------------------------------------------

	[Fact]
	public void IdenticalTree_ReturnsEmptyDiff()
	{
		var xaml = Page("<Label Text=\"Hello\" TextColor=\"Blue\" />");
		var root = Parse(xaml);
		var diff = XamlNodeDiff.ComputeDiff(root, root);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void TwoIdenticalTrees_ReturnsEmptyDiff()
	{
		var xaml = Page("<Label Text=\"Hello\" TextColor=\"Blue\" />");
		var old = Parse(xaml);
		var @new = Parse(xaml);

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void EmptyPage_IdenticalTrees_ReturnsEmptyDiff()
	{
		var xaml = Page("");
		var old = Parse(xaml);
		var @new = Parse(xaml);

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	// ---------------------------------------------------------------------------
	// Property changes → correct diffs
	// ---------------------------------------------------------------------------

	[Fact]
	public void ChangedRootProperty_ReturnsPropertyDiff()
	{
		var old = Parse(Page("", extraAttrs: "Title=\"Hello\""));
		var @new = Parse(Page("", extraAttrs: "Title=\"World\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.False(diff.IsEmpty);
		var nodeDiff = Assert.Single(diff.NodeChanges);
		Assert.Equal("", nodeDiff.NodeId); // root node has empty path
		var propDiff = Assert.Single(nodeDiff.PropertyChanges);

		Assert.Equal("Title", propDiff.PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Set, propDiff.Kind);
		Assert.Equal("World", propDiff.NewValue);
	}

	[Fact]
	public void ChangedChildElementProperty_ReturnsChildNodeDiff()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"World\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff = Assert.Single(diff.NodeChanges);
		// NodeId is non-empty (child path)
		Assert.Contains("Label", nodeDiff.NodeId, StringComparison.Ordinal);
	}

	[Fact]
	public void RemovedProperty_ReturnsClearDiff()
	{
		var old = Parse(Page("", extraAttrs: "Title=\"Hello\""));
		var @new = Parse(Page(""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff = Assert.Single(diff.NodeChanges);
		var propDiff = Assert.Single(nodeDiff.PropertyChanges);
		Assert.Equal("Title", propDiff.PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Clear, propDiff.Kind);
		Assert.Null(propDiff.NewValue);
	}

	[Fact]
	public void AddedProperty_ReturnsSetDiff()
	{
		var old = Parse(Page(""));
		var @new = Parse(Page("", extraAttrs: "Title=\"New\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff = Assert.Single(diff.NodeChanges);
		var propDiff = Assert.Single(nodeDiff.PropertyChanges);
		Assert.Equal("Title", propDiff.PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Set, propDiff.Kind);
		Assert.Equal("New", propDiff.NewValue);
	}

	[Fact]
	public void MultipleChangedProperties_ReturnsAllDiffs()
	{
		var old = Parse(Page("<Label Text=\"Hello\" TextColor=\"Blue\" />"));
		var @new = Parse(Page("<Label Text=\"World\" TextColor=\"Red\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff2 = Assert.Single(diff.NodeChanges);
		var propChanges = nodeDiff2.PropertyChanges;
		Assert.Equal(2, propChanges.Count);
		Assert.Contains(propChanges, p => p.PropertyName.LocalName == "Text" && p.NewValue == "World");
		Assert.Contains(propChanges, p => p.PropertyName.LocalName == "TextColor" && p.NewValue == "Red");
	}

	[Fact]
	public void MultipleChangedNodes_ReturnsAllNodeDiffs()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Hello" />
				<Label Text="World" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Hello" />
				<Label Text="Changed" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff3 = Assert.Single(diff.NodeChanges);
		Assert.Contains("Label", nodeDiff3.NodeId, StringComparison.Ordinal);
		Assert.Single(nodeDiff3.PropertyChanges, p => p.PropertyName.LocalName == "Text" && p.NewValue == "Changed");
	}

	[Fact]
	public void DeeplyNested_PropertyChange_ReturnsDiff()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Grid>
					<Label Text="Deep" />
				</Grid>
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Grid>
					<Label Text="Changed" />
				</Grid>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff4 = Assert.Single(diff.NodeChanges);
		// The path should include all ancestors
		Assert.Contains("Label", nodeDiff4.NodeId, StringComparison.Ordinal);
	}

	// ---------------------------------------------------------------------------
	// Structural changes → returns null
	// ---------------------------------------------------------------------------

	[Fact]
	public void DifferentRootType_ReturnsNull()
	{
		// Both roots are ContentPage so the ROOT won't differ in type; let's use the child
		// Note: root element type can't easily differ while staying valid XAML, test via child
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Button Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void AddedChild_ReturnsNull()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" /><Label Text=\"World\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void RemovedChild_ReturnsNull()
	{
		var old = Parse(Page("<Label Text=\"Hello\" /><Label Text=\"World\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void ChangedChildElementType_ReturnsNull()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Entry Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void ReorderedChildren_ReturnsNull()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Button Text="B" />
				<Label Text="A" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void ChangedPropertyFromValueToMarkup_ReturnsNull()
	{
		// Old: simple string value; New: markup extension (binding)
		// ValueNode → MarkupNode is a type change → structural fallback required.
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		// ValueNode → MarkupNode: different INode concrete types → DiffProperties returns false
		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void XName_Changed_ReturnsNull()
	{
		// x:Name generates a field in code-behind → any change is structural
		var old = Parse(Page("<Label x:Name=\"oldLabel\" Text=\"Hello\" />"));
		var @new = Parse(Page("<Label x:Name=\"newLabel\" Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void XDataType_Changed_ReturnsNull()
	{
		// x:DataType drives compiled bindings → any change is structural
		var old = Parse(Page("<Label />", extraAttrs: "x:DataType=\"MyViewModel\""));
		var @new = Parse(Page("<Label />", extraAttrs: "x:DataType=\"OtherViewModel\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void XName_Identical_NotStructural()
	{
		// Unchanged x:Name should not trigger structural fallback
		var xaml = Page("<Label x:Name=\"myLabel\" Text=\"Hello\" />");
		var old = Parse(xaml);
		var @new = Parse(xaml);

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		// Identical x:Name → not structural
		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void NestedElementProperty_Changed_ReturnsNull()
	{
		// Property element syntax: <Button.Shadow><Shadow Color="Red"/></Button.Shadow>
		// Even though only Color changes, the property is backed by an ElementNode → structural fallback.
		var old = Parse(Page("""
			<Button>
				<Button.Shadow>
					<Shadow Color="Red" />
				</Button.Shadow>
			</Button>
			"""));
		var @new = Parse(Page("""
			<Button>
				<Button.Shadow>
					<Shadow Color="Blue" />
				</Button.Shadow>
			</Button>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		// ElementNode property → conservative structural fallback
		Assert.Null(diff);
	}

	[Fact]
	public void NestedElementProperty_Identical_ReturnsNull()
	{
		// Even identical ElementNode properties trigger structural fallback (conservative)
		var xaml = Page("""
			<Button>
				<Button.Shadow>
					<Shadow Color="Red" />
				</Button.Shadow>
			</Button>
			""");
		var old = Parse(xaml);
		var @new = Parse(xaml);

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		// Conservative: any ElementNode in Properties → structural
		Assert.Null(diff);
	}

	[Fact]
	public void ListNodeProperty_ReturnsNull()
	{
		// A property element with 2+ children becomes a ListNode in Properties → structural
		var old = Parse(Page("""
			<Label>
				<Label.GestureRecognizers>
					<TapGestureRecognizer />
					<SwipeGestureRecognizer />
				</Label.GestureRecognizers>
			</Label>
			"""));
		var @new = Parse(Page("""
			<Label>
				<Label.GestureRecognizers>
					<TapGestureRecognizer />
					<SwipeGestureRecognizer />
				</Label.GestureRecognizers>
			</Label>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		// ListNode property → conservative structural fallback
		Assert.Null(diff);
	}

	// ---------------------------------------------------------------------------
	// Edge cases
	// ---------------------------------------------------------------------------

	[Fact]
	public void SingleNodeNoChildren_PropertyChanged_ReturnsDiff()
	{
		var old = Parse(Page("", extraAttrs: "BackgroundColor=\"White\""));
		var @new = Parse(Page("", extraAttrs: "BackgroundColor=\"Black\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var ecNodeDiff = Assert.Single(diff.NodeChanges);
		Assert.Equal("BackgroundColor", ecNodeDiff.PropertyChanges[0].PropertyName.LocalName);
		Assert.Equal("Black", ecNodeDiff.PropertyChanges[0].NewValue);
	}

	[Fact]
	public void NodeId_Root_IsEmptyString()
	{
		var old = Parse(Page("", extraAttrs: "Title=\"A\""));
		var @new = Parse(Page("", extraAttrs: "Title=\"B\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal("", diff.NodeChanges[0].NodeId);
	}

	[Fact]
	public void NodeId_DirectChild_ContainsTypeName()
	{
		var old = Parse(Page("<Label Text=\"A\" />"));
		var @new = Parse(Page("<Label Text=\"B\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal("Label_1_0", diff.NodeChanges[0].NodeId);
	}

	[Fact]
	public void NodeId_NestedChild_ContainsPath()
	{
		var old = Parse(Page("<VerticalStackLayout><Label Text=\"A\" /></VerticalStackLayout>"));
		var @new = Parse(Page("<VerticalStackLayout><Label Text=\"B\" /></VerticalStackLayout>"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal("Label_2_0", diff.NodeChanges[0].NodeId);
	}

	[Fact]
	public void NoChanges_EmptyTreeToEmptyTree_ReturnsEmptyDiff()
	{
		var old = Parse(Page(""));
		var @new = Parse(Page(""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}
}
