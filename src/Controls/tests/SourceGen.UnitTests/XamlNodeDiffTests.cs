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
		// NodeId is numeric (child of root)
		Assert.Equal("0", nodeDiff.NodeId);
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
	public void AttachedPropertyChanged_ReturnsPropertyDiff()
	{
		var old = Parse(Page("<Grid><Label Text=\"Hello\" Grid.Row=\"0\" /></Grid>"));
		var @new = Parse(Page("<Grid><Label Text=\"Hello\" Grid.Row=\"1\" /></Grid>"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff = Assert.Single(diff.NodeChanges);
		var propDiff = Assert.Single(nodeDiff.PropertyChanges);
		// Attached property: LocalName is "Grid.Row" (full dotted name)
		Assert.Equal("Grid.Row", propDiff.PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Set, propDiff.Kind);
		Assert.Equal("1", propDiff.NewValue);
	}

	[Fact]
	public void AttachedPropertyChangedToBinding_ReturnsMarkupNodeDiff()
	{
		var old = Parse(Page("<Grid><Label Text=\"Hello\" Grid.Row=\"0\" /></Grid>"));
		var @new = Parse(Page("<Grid><Label Text=\"Hello\" Grid.Row=\"{Binding RowIndex}\" /></Grid>"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var nodeDiff = Assert.Single(diff.NodeChanges);
		var propDiff = Assert.Single(nodeDiff.PropertyChanges);
		Assert.Equal("Grid.Row", propDiff.PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Set, propDiff.Kind);
		// Binding creates a MarkupNode, not a plain value
		Assert.NotNull(propDiff.NewNode);
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
		Assert.Equal("2", nodeDiff3.NodeId);
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
		Assert.Equal("2", nodeDiff4.NodeId);
	}

	// ---------------------------------------------------------------------------
	// Structural changes → returns null
	// ---------------------------------------------------------------------------

	[Fact]
	public void DifferentRootType_ReturnsNull()
	{
		// Root element type mismatch → structural fallback (x:Class binds to a specific type)
		var old = Parse($"""<ContentPage {MauiXmlns} x:Class="Test.MyPage" />""");
		var @new = Parse($"""<ContentView {MauiXmlns} x:Class="Test.MyPage" />""");

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void AddedChild_ProducesChildListChange()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" /><Label Text=\"World\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.ChildListChanges);
		var change = diff.ChildListChanges[0];
		Assert.Equal(1, change.NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void RemovedChild_ProducesChildListChange()
	{
		var old = Parse(Page("<Label Text=\"Hello\" /><Label Text=\"World\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.ChildListChanges);
		var change = diff.ChildListChanges[0];
		Assert.Single(change.RemovedNodeIds);
	}

	[Fact]
	public void ChangedChildElementType_ProducesChildListChange()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Entry Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.ChildListChanges);
		var change = diff.ChildListChanges[0];
		// Old Label removed, new Entry added
		Assert.Single(change.RemovedNodeIds);
		Assert.Equal(1, change.NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void ReorderedChildren_UniqueTypes_ReturnsReorderDiff()
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

		Assert.NotNull(diff);
		Assert.Empty(diff.NodeChanges); // no property changes
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Equal("0", change.ParentNodeId);
		Assert.Equal(2, change.NewChildren.Count);
		Assert.Empty(change.RemovedNodeIds);
		// New order: Button (was index 1) at index 0, Label (was index 0) at index 1
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[0].Kind);
		Assert.Equal("2", change.NewChildren[0].OldNodeId);
		Assert.Equal("2", change.NewChildren[0].NewNodeId);
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[1].Kind);
		Assert.Equal("1", change.NewChildren[1].OldNodeId);
		Assert.Equal("1", change.NewChildren[1].NewNodeId);
	}

	[Fact]
	public void ReorderedChildren_DuplicateTypes_SmartMatching_DetectsReorder()
	{
		// Two Labels swapped — smart matching detects this as a reorder (0 property changes)
		// rather than 2 property changes (old positional matching behavior)
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Label Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="B" />
				<Label Text="A" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Smart matching: detects swap → child list change (reorder), no property changes
		Assert.Empty(diff.NodeChanges);
		Assert.Single(diff.ChildListChanges);
		var change = diff.ChildListChanges[0];
		Assert.Equal(2, change.NewChildren.Count);
		Assert.True(change.NewChildren.All(c => c.Kind == ChildChangeKind.Retained));
		Assert.Empty(change.RemovedNodeIds);
	}

	[Fact]
	public void ReorderedChildren_WithPropertyChanges_ReturnsBothDiffs()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Button Text="B2" />
				<Label Text="A" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Should have a child list change
		Assert.Single(diff.ChildListChanges);
		// Should have property change on Button (Text: "B" → "B2")
		var propChange = Assert.Single(diff.NodeChanges);
		Assert.Equal("2", propChange.NodeId);
		Assert.Equal("B2", propChange.PropertyChanges[0].NewValue);
	}

	[Fact]
	public void ReorderedChildren_IdentityPermutation_NoChildListChange()
	{
		// Same types, same order — should not produce a child list change
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A2" />
				<Button Text="B2" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Empty(diff.ChildListChanges); // no child list change needed
		Assert.Equal(2, diff.NodeChanges.Count); // just property changes
	}

	[Fact]
	public void ReorderedChildren_ThreeElements_ReturnsCorrectPermutation()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
				<Entry Text="C" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Entry Text="C" />
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Equal(3, change.NewChildren.Count);
		Assert.Empty(change.RemovedNodeIds);
		// Entry was at 2, now at 0
		Assert.Equal(2, change.NewChildren[0].OldIndex);
		// Label was at 0, now at 1
		Assert.Equal(0, change.NewChildren[1].OldIndex);
		// Button was at 1, now at 2
		Assert.Equal(1, change.NewChildren[2].OldIndex);
	}

	// ---------------------------------------------------------------------------
	// Child addition / removal
	// ---------------------------------------------------------------------------

	[Fact]
	public void ChildAdded_SimpleElement_ReturnsAddEntry()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Equal("0", change.ParentNodeId);
		Assert.Equal(2, change.NewChildren.Count);
		Assert.Empty(change.RemovedNodeIds);
		// Label retained at same position
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[0].Kind);
		Assert.Equal("1", change.NewChildren[0].OldNodeId);
		Assert.Equal("1", change.NewChildren[0].NewNodeId);
		// Button added
		Assert.Equal(ChildChangeKind.Added, change.NewChildren[1].Kind);
		Assert.Equal("2", change.NewChildren[1].NewNodeId);
		Assert.NotNull(change.NewChildren[1].NewElement);
	}

	[Fact]
	public void ChildRemoved_ReturnsRemovedEntry()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Equal("0", change.ParentNodeId);
		Assert.Single(change.NewChildren);
		// Label retained
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[0].Kind);
		Assert.Equal("1", change.NewChildren[0].NewNodeId);
		// Button removed
		Assert.Single(change.RemovedNodeIds);
		Assert.Equal("2", change.RemovedNodeIds[0]);
	}

	[Fact]
	public void ChildAddedInMiddle_ShiftsRetainedPositions()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Entry Placeholder="new" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Equal(3, change.NewChildren.Count);
		Assert.Empty(change.RemovedNodeIds);
		// Label retained at 0
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[0].Kind);
		// Entry added at 1
		Assert.Equal(ChildChangeKind.Added, change.NewChildren[1].Kind);
		Assert.Equal("2", change.NewChildren[1].NewNodeId);
		// Button retained, shifted from 1 to 2
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[2].Kind);
		Assert.Equal("2", change.NewChildren[2].OldNodeId);
		Assert.Equal("2", change.NewChildren[2].NewNodeId);
	}

	[Fact]
	public void ChildAddAndRemove_MixedOperation()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
				<Entry Text="C" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Switch />
				<Entry Text="C" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Equal(3, change.NewChildren.Count);
		// Label retained
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[0].Kind);
		// Switch added
		Assert.Equal(ChildChangeKind.Added, change.NewChildren[1].Kind);
		// Entry retained (shifted from 2 to 2 — same position)
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[2].Kind);
		// Button removed
		Assert.Single(change.RemovedNodeIds);
		Assert.Equal("2", change.RemovedNodeIds[0]);
	}

	[Fact]
	public void ChildAdded_WithComplexProperties_Succeeds()
	{
		// Added child has a binding — diff should still succeed (codegen decides how to handle)
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Label Text="{Binding Name}" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.ChildListChanges);
		var change = diff.ChildListChanges[0];
		Assert.Equal(1, change.NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void MultipleChildrenAdded_ReturnsMultipleAddEntries()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
				<Entry Placeholder="C" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Equal(3, change.NewChildren.Count);
		Assert.Empty(change.RemovedNodeIds);
		Assert.Equal(ChildChangeKind.Retained, change.NewChildren[0].Kind);
		Assert.Equal(ChildChangeKind.Added, change.NewChildren[1].Kind);
		Assert.Equal(ChildChangeKind.Added, change.NewChildren[2].Kind);
	}

	// ---------------------------------------------------------------------------
	// Debug output (ToDebugString) — canonical diff verification
	// ---------------------------------------------------------------------------

	[Fact]
	public void ToDebugString_SinglePropertyChange()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"World\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 1 node(s) with property changes\n  [0] Text = \"World\"",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_MultiplePropertiesOnSameNode()
	{
		var old = Parse(Page("<Label Text=\"Hello\" FontSize=\"14\" />"));
		var @new = Parse(Page("<Label Text=\"World\" FontSize=\"18\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 1 node(s) with property changes\n  [0] Text = \"World\", FontSize = \"18\"",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_MultipleNodesChanged()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Label Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="X" />
				<Label Text="Y" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 2 node(s) with property changes\n" +
			"  [1] Text = \"X\"\n" +
			"  [2] Text = \"Y\"",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_RootPropertyChange()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />", extraAttrs: "Title=\"Old\""));
		var @new = Parse(Page("<Label Text=\"Hello\" />", extraAttrs: "Title=\"New\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 1 node(s) with property changes\n  [root] Title = \"New\"",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_PropertyCleared()
	{
		var old = Parse(Page("<Label Text=\"Hello\" FontSize=\"14\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 1 node(s) with property changes\n  [0] FontSize cleared",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_PropertyAdded()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" FontSize=\"20\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 1 node(s) with property changes\n  [0] FontSize = \"20\"",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_ChildReorder()
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

		Assert.NotNull(diff);
		// Button was old index 1 (id "2") → new index 0, Label was old index 0 (id "1") → new index 1
		// After transplant, OldNodeId == NewNodeId for retained nodes → "(unchanged)"
		Assert.Equal(
			"Diff: 0 node(s) with property changes, 1 child list change(s)\n" +
			"  children [0] " +
			"2 (unchanged), " +
			"1 (unchanged)",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_ReorderWithPropertyChanges()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Button Text="B2" />
				<Label Text="A" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var debug = diff.ToDebugString();
		// Should show both reorder and property change
		Assert.Contains("child list change(s)", debug, StringComparison.Ordinal);
		Assert.Contains("children [0]", debug, StringComparison.Ordinal);
		Assert.Contains("Text = \"B2\"", debug, StringComparison.Ordinal);
	}

	[Fact]
	public void ToDebugString_DeeplyNested()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<HorizontalStackLayout>
					<Label Text="Deep" />
				</HorizontalStackLayout>
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<HorizontalStackLayout>
					<Label Text="Changed" />
				</HorizontalStackLayout>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 1 node(s) with property changes\n" +
			"  [2] Text = \"Changed\"",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_EmptyDiff()
	{
		var xaml = Page("<Label Text=\"Hello\" />");
		var root = Parse(xaml);
		var diff = XamlNodeDiff.ComputeDiff(root, root);

		Assert.NotNull(diff);
		Assert.Equal("Diff: 0 node(s) with property changes", diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_ThreeElementReorder()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
				<Entry Placeholder="C" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Entry Placeholder="C" />
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var debug = diff.ToDebugString();
		Assert.Contains("1 child list change(s)", debug, StringComparison.Ordinal);
		Assert.Contains("children [0]", debug, StringComparison.Ordinal);
		// All three children should appear in the reorder (with old-tree IDs, all unchanged)
		Assert.Contains("1 (unchanged)", debug, StringComparison.Ordinal);
		Assert.Contains("2 (unchanged)", debug, StringComparison.Ordinal);
		Assert.Contains("3 (unchanged)", debug, StringComparison.Ordinal);
	}

	[Fact]
	public void ToDebugString_ChildAdded()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 0 node(s) with property changes, 1 child list change(s)\n" +
			"  children [0] " +
			"1 (unchanged), " +
			"+2",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_ChildRemoved()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 0 node(s) with property changes, 1 child list change(s)\n" +
			"  children [0] " +
			"1 (unchanged)" +
			"; removed: -2",
			diff.ToDebugString());
	}

	[Fact]
	public void ToDebugString_ChildAddAndRemove()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Switch />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(
			"Diff: 0 node(s) with property changes, 1 child list change(s)\n" +
			"  children [0] " +
			"1 (unchanged), " +
			"+2" +
			"; removed: -2",
			diff.ToDebugString());
	}

	[Fact]
	public void ChangedPropertyFromValueToMarkup_ProducesPropertyDiffWithNode()
	{
		// Old: simple string value; New: markup extension (binding)
		// At the diff level, this is just a property change — codegen decides how to apply.
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var nd = diff.NodeChanges[0];
		Assert.Single(nd.PropertyChanges);
		var prop = nd.PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Set, prop.Kind);
		Assert.Null(prop.NewValue); // not a simple string
		Assert.NotNull(prop.NewNode); // complex node stored
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
	public void XDataType_Changed_NoBindings_EmptyDiff()
	{
		// x:DataType changed but no bindings → no property diffs needed (incremental, not structural)
		var old = Parse(Page("<Label />", extraAttrs: "x:DataType=\"MyViewModel\""));
		var @new = Parse(Page("<Label />", extraAttrs: "x:DataType=\"OtherViewModel\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Empty(diff.NodeChanges);
		Assert.Empty(diff.ChildListChanges);
	}

	[Fact]
	public void XDataType_Changed_BindingsForceRefreshed()
	{
		// x:DataType changed → all binding MarkupNodes should appear as changed
		var old = Parse(Page("<Label Text=\"{Binding Name}\" />", extraAttrs: "x:DataType=\"MyViewModel\""));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />", extraAttrs: "x:DataType=\"OtherViewModel\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// The Label's Text binding should be forced into the diff even though it's identical
		Assert.Single(diff.NodeChanges);
		var nd = diff.NodeChanges[0];
		Assert.Single(nd.PropertyChanges);
		Assert.Equal("Text", nd.PropertyChanges[0].PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Set, nd.PropertyChanges[0].Kind);
		Assert.NotNull(nd.PropertyChanges[0].NewNode); // MarkupNode
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
	public void NestedElementProperty_Changed_ProducesPropertyDiffWithNode()
	{
		// Property element syntax: <Button.Shadow><Shadow Color="Red"/></Button.Shadow>
		// The Shadow property is backed by an ElementNode — diff records it with the NewNode.
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

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var nd = diff.NodeChanges[0];
		Assert.Single(nd.PropertyChanges);
		var prop = nd.PropertyChanges[0];
		Assert.Equal("Shadow", prop.PropertyName.LocalName);
		Assert.Equal(PropertyDiffKind.Set, prop.Kind);
		Assert.Null(prop.NewValue); // not a simple string
		Assert.NotNull(prop.NewNode); // ElementNode stored
	}

	[Fact]
	public void NestedElementProperty_Identical_NoChange()
	{
		// Identical ElementNode properties should produce an empty diff (no change)
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

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void ListNodeProperty_Identical_NoChange()
	{
		// Identical ListNode properties should produce an empty diff
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

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
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
	public void NodeId_DirectChild_IsNumericId()
	{
		var old = Parse(Page("<Label Text=\"A\" />"));
		var @new = Parse(Page("<Label Text=\"B\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal("0", diff.NodeChanges[0].NodeId);
	}

	[Fact]
	public void NodeId_NestedChild_IsNumericId()
	{
		var old = Parse(Page("<VerticalStackLayout><Label Text=\"A\" /></VerticalStackLayout>"));
		var @new = Parse(Page("<VerticalStackLayout><Label Text=\"B\" /></VerticalStackLayout>"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal("1", diff.NodeChanges[0].NodeId);
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

	// ---------------------------------------------------------------------------
	// Multi-edit scenarios (property + structural changes combined)
	// ---------------------------------------------------------------------------

	[Fact]
	public void MultiEdit_RootPropertyAndChildProperty()
	{
		// Root Title changed AND child Label.Text changed
		var old = Parse(Page("<Label Text=\"Hello\" />", extraAttrs: "Title=\"Page1\""));
		var @new = Parse(Page("<Label Text=\"World\" />", extraAttrs: "Title=\"Page2\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(2, diff.NodeChanges.Count);
		// Root property
		var rootDiff = diff.NodeChanges.Single(n => n.NodeId == "");
		Assert.Equal("Title", rootDiff.PropertyChanges[0].PropertyName.LocalName);
		Assert.Equal("Page2", rootDiff.PropertyChanges[0].NewValue);
		// Child property
		var childDiff = diff.NodeChanges.Single(n => n.NodeId != "");
		Assert.Equal("Text", childDiff.PropertyChanges[0].PropertyName.LocalName);
		Assert.Equal("World", childDiff.PropertyChanges[0].NewValue);
	}

	[Fact]
	public void MultiEdit_PropertyChangeAndChildAdded()
	{
		// Existing Label.Text changed AND new Button added
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="B" />
				<Button Text="New" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Property change on Label
		Assert.Single(diff.NodeChanges);
		Assert.Equal("1", diff.NodeChanges[0].NodeId);
		Assert.Equal("B", diff.NodeChanges[0].PropertyChanges[0].NewValue);
		Assert.Single(diff.ChildListChanges);
		Assert.Equal(1, diff.ChildListChanges[0].NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void MultiEdit_PropertyChangeAndChildRemoved()
	{
		// Label.Text changed AND Button removed
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Changed" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Property change on Label
		Assert.Single(diff.NodeChanges);
		Assert.Equal("Changed", diff.NodeChanges[0].PropertyChanges[0].NewValue);
		// Child list change (Button removed)
		Assert.Single(diff.ChildListChanges);
		Assert.Single(diff.ChildListChanges[0].RemovedNodeIds);
	}

	[Fact]
	public void MultiEdit_ReorderAndPropertyChanges()
	{
		// Reorder Label↔Button AND change properties on both
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" FontSize="14" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Button Text="B2" />
				<Label Text="A2" FontSize="20" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Reorder
		Assert.Single(diff.ChildListChanges);
		// Property changes on both nodes
		Assert.Equal(2, diff.NodeChanges.Count);
		var labelDiff = diff.NodeChanges.Single(n => n.NodeId == "1");
		var buttonDiff = diff.NodeChanges.Single(n => n.NodeId == "2");
		Assert.Equal(2, labelDiff.PropertyChanges.Count); // Text + FontSize
		Assert.Single(buttonDiff.PropertyChanges); // Text only
	}

	[Fact]
	public void MultiEdit_ChildAddRemoveAndDeepPropertyChange()
	{
		// In a nested structure: add Switch, remove Entry, AND change deeply nested Label.Text
		var old = Parse(Page("""
			<VerticalStackLayout>
				<HorizontalStackLayout>
					<Label Text="Deep" />
				</HorizontalStackLayout>
				<Entry Text="Remove" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<HorizontalStackLayout>
					<Label Text="Changed" />
				</HorizontalStackLayout>
				<Switch />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Deep property change on Label
		Assert.Single(diff.NodeChanges);
		Assert.Equal("2", diff.NodeChanges[0].NodeId);
		Assert.Equal("Changed", diff.NodeChanges[0].PropertyChanges[0].NewValue);
		// Child list change on VSL: Entry removed, Switch added
		Assert.Single(diff.ChildListChanges);
		Assert.Single(diff.ChildListChanges[0].RemovedNodeIds);
		Assert.Equal("3", diff.ChildListChanges[0].RemovedNodeIds[0]);
		Assert.Equal(1, diff.ChildListChanges[0].NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void MultiEdit_PropertyChangesAtMultipleDepths()
	{
		// Properties changed at root, first-level child, and second-level child simultaneously
		var old = Parse(Page("""
			<VerticalStackLayout Spacing="10">
				<Label Text="A" />
				<HorizontalStackLayout>
					<Button Text="B" />
				</HorizontalStackLayout>
			</VerticalStackLayout>
			""", extraAttrs: "Title=\"Old\""));
		var @new = Parse(Page("""
			<VerticalStackLayout Spacing="20">
				<Label Text="A2" />
				<HorizontalStackLayout>
					<Button Text="B2" />
				</HorizontalStackLayout>
			</VerticalStackLayout>
			""", extraAttrs: "Title=\"New\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Empty(diff.ChildListChanges); // no structural changes
		Assert.Equal(4, diff.NodeChanges.Count);
		// Root: Title
		Assert.Single(diff.NodeChanges, n => n.NodeId == "");
		// VSL: Spacing
		Assert.Single(diff.NodeChanges, n => n.NodeId == "0");
		// Label: Text
		Assert.Single(diff.NodeChanges, n => n.NodeId == "1");
		// Button: Text
		Assert.Single(diff.NodeChanges, n => n.NodeId == "3");
	}

	[Fact]
	public void MultiEdit_ValueToBindingAndChildAdded()
	{
		// Existing Label.Text changed from value to binding AND new Entry added
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Static" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="{Binding Name}" />
				<Entry Placeholder="New" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Property change: value → binding (complex)
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode); // complex markup
		// Child list change: Entry added
		Assert.Single(diff.ChildListChanges);
		Assert.Equal(1, diff.ChildListChanges[0].NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void ToDebugString_MultiEdit_PropertyAndChildChanges()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			""", extraAttrs: "Title=\"Old\""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A2" />
				<Switch />
			</VerticalStackLayout>
			""", extraAttrs: "Title=\"New\""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var debug = diff.ToDebugString();
		// Root property change
		Assert.Contains("[root] Title = \"New\"", debug, StringComparison.Ordinal);
		// Label property change
		Assert.Contains("Text = \"A2\"", debug, StringComparison.Ordinal);
		// Child list change: Switch added, Button removed
		Assert.Contains("+", debug, StringComparison.Ordinal);
		Assert.Contains("removed:", debug, StringComparison.Ordinal);
	}

	// ---------------------------------------------------------------------------
	// Complex property diff tests (no fallback)
	// ---------------------------------------------------------------------------

	[Fact]
	public void BindingToValue_ProducesPropertyDiffWithSimpleValue()
	{
		// Binding → Value is a property change at the diff level
		var old = Parse(Page("<Label Text=\"{Binding Name}\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.Equal("Hello", prop.NewValue);
		Assert.Null(prop.NewNode); // simple value, no complex node
	}

	[Fact]
	public void NewPropertyWithBinding_ProducesPropertyDiff()
	{
		// Adding a new property as a binding should produce a diff, not fallback
		var old = Parse(Page("<Label />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	// -------------------------------------------------------------------------
	// Property transition matrix: Value ↔ Binding ↔ StaticResource ↔ DynamicResource
	// -------------------------------------------------------------------------

	[Fact]
	public void ValueToBinding_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode); // complex (MarkupNode for binding)
		Assert.Null(prop.NewValue);
	}

	[Fact]
	public void ValueToStaticResource_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
		Assert.Null(prop.NewValue);
	}

	[Fact]
	public void ValueToDynamicResource_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
		Assert.Null(prop.NewValue);
	}

	[Fact]
	public void StaticResourceToValue_ProducesSimplePropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.Equal("Hello", prop.NewValue);
		Assert.Null(prop.NewNode);
	}

	[Fact]
	public void DynamicResourceToValue_ProducesSimplePropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.Equal("Hello", prop.NewValue);
		Assert.Null(prop.NewNode);
	}

	[Fact]
	public void BindingToStaticResource_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{Binding Name}\" />"));
		var @new = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void BindingToDynamicResource_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{Binding Name}\" />"));
		var @new = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void StaticResourceToBinding_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void StaticResourceToDynamicResource_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void DynamicResourceToStaticResource_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void DynamicResourceToBinding_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void BindingPathChange_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{Binding Name}\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Title}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void StaticResourceKeyChange_ProducesComplexPropertyDiff()
	{
		var old = Parse(Page("<Label Text=\"{StaticResource Key1}\" />"));
		var @new = Parse(Page("<Label Text=\"{StaticResource Key2}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Text", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	[Fact]
	public void SameBinding_NoDiff()
	{
		var old = Parse(Page("<Label Text=\"{Binding Name}\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void SameStaticResource_NoDiff()
	{
		var old = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"{StaticResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void SameDynamicResource_NoDiff()
	{
		var old = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));
		var @new = Parse(Page("<Label Text=\"{DynamicResource MyKey}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void ListNodeProperty_Changed_ProducesDiff()
	{
		// Changing a ListNode property (adding a gesture recognizer) should not trigger fallback
		var old = Parse(Page("""
			<Label>
				<Label.GestureRecognizers>
					<TapGestureRecognizer />
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

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("GestureRecognizers", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode); // ListNode stored
	}

	[Fact]
	public void TextContentChanged_ProducesContentDiff()
	{
		// Text content (<Label>Hello</Label>) that changes should be tracked
		var old = Parse(Page("<Label>Hello</Label>"));
		var @new = Parse(Page("<Label>World</Label>"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("_Content", prop.PropertyName.LocalName);
		Assert.Equal("World", prop.NewValue);
	}

	[Fact]
	public void TextContentUnchanged_ProducesEmptyDiff()
	{
		// Same text content should produce empty diff
		var old = Parse(Page("<Label>Hello</Label>"));
		var @new = Parse(Page("<Label>Hello</Label>"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.True(diff.IsEmpty);
	}

	[Fact]
	public void ToDebugString_ValueToBinding()
	{
		var old = Parse(Page("<Label Text=\"Hello\" />"));
		var @new = Parse(Page("<Label Text=\"{Binding Name}\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var debug = diff.ToDebugString();
		Assert.Contains("Text = {MarkupNode}", debug, StringComparison.Ordinal);
	}

	[Fact]
	public void ToDebugString_TextContent()
	{
		var old = Parse(Page("<Label>Hello</Label>"));
		var @new = Parse(Page("<Label>World</Label>"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var debug = diff.ToDebugString();
		Assert.Contains("_Content = \"World\"", debug, StringComparison.Ordinal);
	}

	[Fact]
	public void NestedElementProperty_AddedNew_ProducesDiff()
	{
		// Adding a new nested element property (Shadow on a plain button)
		var old = Parse(Page("<Button Text=\"Click\" />"));
		var @new = Parse(Page("""
			<Button Text="Click">
				<Button.Shadow>
					<Shadow Color="Red" />
				</Button.Shadow>
			</Button>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Single(diff.NodeChanges);
		var prop = diff.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Shadow", prop.PropertyName.LocalName);
		Assert.NotNull(prop.NewNode);
	}

	// ---------------------------------------------------------------------------
	// Sequential edits (xaml1 → xaml2 → xaml3 → …) — simulating a hot reload session
	// ---------------------------------------------------------------------------

	[Fact]
	public void SequentialEdits_PropertyTweaks()
	{
		// Developer iterates on label text and color across 4 saves
		var xaml1 = Parse(Page("<Label Text=\"Draft\" TextColor=\"Gray\" />"));
		var xaml2 = Parse(Page("<Label Text=\"Hello\" TextColor=\"Gray\" />"));
		var xaml3 = Parse(Page("<Label Text=\"Hello\" TextColor=\"Blue\" />"));
		var xaml4 = Parse(Page("<Label Text=\"Hello, World!\" TextColor=\"Blue\" />"));

		// v1→v2: Text changed
		var d12 = XamlNodeDiff.ComputeDiff(xaml1, xaml2);
		Assert.NotNull(d12);
		Assert.Single(d12.NodeChanges);
		Assert.Single(d12.NodeChanges[0].PropertyChanges);
		Assert.Equal("Text", d12.NodeChanges[0].PropertyChanges[0].PropertyName.LocalName);
		Assert.Equal("Hello", d12.NodeChanges[0].PropertyChanges[0].NewValue);

		// v2→v3: TextColor changed
		var d23 = XamlNodeDiff.ComputeDiff(xaml2, xaml3);
		Assert.NotNull(d23);
		Assert.Single(d23.NodeChanges);
		Assert.Single(d23.NodeChanges[0].PropertyChanges);
		Assert.Equal("TextColor", d23.NodeChanges[0].PropertyChanges[0].PropertyName.LocalName);
		Assert.Equal("Blue", d23.NodeChanges[0].PropertyChanges[0].NewValue);

		// v3→v4: Text changed again
		var d34 = XamlNodeDiff.ComputeDiff(xaml3, xaml4);
		Assert.NotNull(d34);
		Assert.Single(d34.NodeChanges);
		Assert.Equal("Hello, World!", d34.NodeChanges[0].PropertyChanges[0].NewValue);
	}

	[Fact]
	public void SequentialEdits_GrowingLayout()
	{
		// Developer adds children one by one across saves
		var xaml1 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Title" />
			</VerticalStackLayout>
			"""));
		var xaml2 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Title" />
				<Entry Placeholder="Name" />
			</VerticalStackLayout>
			"""));
		var xaml3 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Title" />
				<Entry Placeholder="Name" />
				<Button Text="Submit" />
			</VerticalStackLayout>
			"""));

		// v1→v2: Entry added
		var d12 = XamlNodeDiff.ComputeDiff(xaml1, xaml2);
		Assert.NotNull(d12);
		Assert.Empty(d12.NodeChanges);
		Assert.Single(d12.ChildListChanges);
		Assert.Equal(1, d12.ChildListChanges[0].NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
		Assert.Equal("2", d12.ChildListChanges[0].NewChildren.First(e => e.Kind == ChildChangeKind.Added).NewNodeId);

		// v2→v3: Button added
		var d23 = XamlNodeDiff.ComputeDiff(xaml2, xaml3);
		Assert.NotNull(d23);
		Assert.Empty(d23.NodeChanges);
		Assert.Single(d23.ChildListChanges);
		Assert.Equal(1, d23.ChildListChanges[0].NewChildren.Count(e => e.Kind == ChildChangeKind.Added));
		Assert.Equal("3", d23.ChildListChanges[0].NewChildren.First(e => e.Kind == ChildChangeKind.Added).NewNodeId);
	}

	[Fact]
	public void SequentialEdits_AddThenRemoveThenModify()
	{
		// v1: baseline
		var xaml1 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Hello" />
			</VerticalStackLayout>
			"""));
		// v2: add Button
		var xaml2 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Hello" />
				<Button Text="Click" />
			</VerticalStackLayout>
			"""));
		// v3: remove Button (undo)
		var xaml3 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Hello" />
			</VerticalStackLayout>
			"""));
		// v4: change Label text instead
		var xaml4 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Goodbye" />
			</VerticalStackLayout>
			"""));

		// v1→v2: +Button
		var d12 = XamlNodeDiff.ComputeDiff(xaml1, xaml2);
		Assert.NotNull(d12);
		Assert.Single(d12.ChildListChanges);
		Assert.Equal(1, d12.ChildListChanges[0].NewChildren.Count(e => e.Kind == ChildChangeKind.Added));

		// v2→v3: −Button (undo)
		var d23 = XamlNodeDiff.ComputeDiff(xaml2, xaml3);
		Assert.NotNull(d23);
		Assert.Single(d23.ChildListChanges);
		Assert.Single(d23.ChildListChanges[0].RemovedNodeIds);

		// v3→v4: property change only, no structural
		var d34 = XamlNodeDiff.ComputeDiff(xaml3, xaml4);
		Assert.NotNull(d34);
		Assert.Empty(d34.ChildListChanges);
		Assert.Single(d34.NodeChanges);
		Assert.Equal("Goodbye", d34.NodeChanges[0].PropertyChanges[0].NewValue);
	}

	[Fact]
	public void SequentialEdits_ReorderThenPropertyChange()
	{
		// v1: Label, Button, Entry
		var xaml1 = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Button Text="B" />
				<Entry Placeholder="C" />
			</VerticalStackLayout>
			"""));
		// v2: reorder to Entry, Label, Button
		var xaml2 = Parse(Page("""
			<VerticalStackLayout>
				<Entry Placeholder="C" />
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));
		// v3: change Entry placeholder (property change after reorder)
		var xaml3 = Parse(Page("""
			<VerticalStackLayout>
				<Entry Placeholder="Search..." />
				<Label Text="A" />
				<Button Text="B" />
			</VerticalStackLayout>
			"""));

		// v1→v2: reorder only
		var d12 = XamlNodeDiff.ComputeDiff(xaml1, xaml2);
		Assert.NotNull(d12);
		Assert.Single(d12.ChildListChanges);
		Assert.Empty(d12.NodeChanges);

		// v2→v3: property change only (no reorder since order is stable)
		var d23 = XamlNodeDiff.ComputeDiff(xaml2, xaml3);
		Assert.NotNull(d23);
		Assert.Empty(d23.ChildListChanges);
		Assert.Single(d23.NodeChanges);
		Assert.Equal("Search...", d23.NodeChanges[0].PropertyChanges[0].NewValue);
	}

	[Fact]
	public void SequentialEdits_ValueToBindingToValue()
	{
		// Developer tries a binding, then reverts to static value
		var xaml1 = Parse(Page("<Label Text=\"Static\" />"));
		var xaml2 = Parse(Page("<Label Text=\"{Binding Name}\" />"));
		var xaml3 = Parse(Page("<Label Text=\"Back to static\" />"));

		// v1→v2: value → binding
		var d12 = XamlNodeDiff.ComputeDiff(xaml1, xaml2);
		Assert.NotNull(d12);
		Assert.Single(d12.NodeChanges);
		var p12 = d12.NodeChanges[0].PropertyChanges[0];
		Assert.NotNull(p12.NewNode); // complex
		Assert.Null(p12.NewValue);

		// v2→v3: binding → value (now produces simple value diff)
		var d23 = XamlNodeDiff.ComputeDiff(xaml2, xaml3);
		Assert.NotNull(d23);
		Assert.Single(d23.NodeChanges);
		var p23 = d23.NodeChanges[0].PropertyChanges[0];
		Assert.Equal("Back to static", p23.NewValue);
		Assert.Null(p23.NewNode); // simple value, no complex node
	}

	[Fact]
	public void SequentialEdits_ReplaceChildType()
	{
		// Developer replaces one control type with another across two edits
		var xaml1 = Parse(Page("""
			<VerticalStackLayout>
				<Entry Placeholder="Name" />
				<Button Text="Submit" />
			</VerticalStackLayout>
			"""));
		// v2: replace Entry with Editor
		var xaml2 = Parse(Page("""
			<VerticalStackLayout>
				<Editor Placeholder="Name" />
				<Button Text="Submit" />
			</VerticalStackLayout>
			"""));
		// v3: also replace Button with ImageButton
		var xaml3 = Parse(Page("""
			<VerticalStackLayout>
				<Editor Placeholder="Bio" />
				<ImageButton />
			</VerticalStackLayout>
			"""));

		// v1→v2: Entry removed, Editor added
		var d12 = XamlNodeDiff.ComputeDiff(xaml1, xaml2);
		Assert.NotNull(d12);
		Assert.Single(d12.ChildListChanges);
		Assert.Single(d12.ChildListChanges[0].RemovedNodeIds);
		Assert.Equal("1", d12.ChildListChanges[0].RemovedNodeIds[0]);
		Assert.Equal(1, d12.ChildListChanges[0].NewChildren.Count(e => e.Kind == ChildChangeKind.Added));

		// v2→v3: Button removed + ImageButton added, AND Editor.Placeholder changed
		var d23 = XamlNodeDiff.ComputeDiff(xaml2, xaml3);
		Assert.NotNull(d23);
		Assert.Single(d23.ChildListChanges);
		Assert.Single(d23.ChildListChanges[0].RemovedNodeIds);
		Assert.Equal("2", d23.ChildListChanges[0].RemovedNodeIds[0]);
		// Editor property change
		Assert.Single(d23.NodeChanges);
		Assert.Equal("Bio", d23.NodeChanges[0].PropertyChanges[0].NewValue);
	}

	// ---------------------------------------------------------------------------
	// Smart same-type sibling matching
	// ---------------------------------------------------------------------------

	[Fact]
	public void SmartMatching_XNameBased_DetectsReorder()
	{
		// Two Labels with x:Name — swapped positions. x:Name matching identifies them.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label x:Name="first" Text="Hello" />
				<Label x:Name="second" Text="World" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label x:Name="second" Text="World" />
				<Label x:Name="first" Text="Hello" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// x:Name matching → perfect reorder, zero property changes
		Assert.Empty(diff.NodeChanges);
		Assert.Single(diff.ChildListChanges);
		Assert.Empty(diff.ChildListChanges[0].RemovedNodeIds);
		Assert.Equal(2, diff.ChildListChanges[0].NewChildren.Count);
		Assert.True(diff.ChildListChanges[0].NewChildren.All(c => c.Kind == ChildChangeKind.Retained));
	}

	[Fact]
	public void SmartMatching_XNameBased_ReorderWithPropertyChange()
	{
		// Two Labels with x:Name — swapped and one property changed.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label x:Name="first" Text="Hello" />
				<Label x:Name="second" Text="World" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label x:Name="second" Text="Universe" />
				<Label x:Name="first" Text="Hello" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Reorder detected + 1 property change on "second"
		Assert.Single(diff.NodeChanges);
		Assert.Equal("Universe", diff.NodeChanges[0].PropertyChanges[0].NewValue);
		Assert.Single(diff.ChildListChanges);
	}

	[Fact]
	public void SmartMatching_CostBased_MultipleProperties_DetectsSwap()
	{
		// Two Labels with multiple properties each — swapped. Cost matching should detect
		// 0-cost assignment (swap) vs 4+ property changes (positional).
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Hello" FontSize="20" />
				<Label Text="World" FontSize="14" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="World" FontSize="14" />
				<Label Text="Hello" FontSize="20" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Cost matching detects perfect swap → reorder, 0 property changes
		Assert.Empty(diff.NodeChanges);
		Assert.Single(diff.ChildListChanges);
		Assert.True(diff.ChildListChanges[0].NewChildren.All(c => c.Kind == ChildChangeKind.Retained));
	}

	[Fact]
	public void SmartMatching_CostBased_OnePropDiffers_PrefersCheapest()
	{
		// Two Labels — only Text differs but positional match has lower cost than swap.
		// old[0] Text="A" → new[0] Text="A2" (1 diff)
		// old[1] Text="B" → new[1] Text="B"  (0 diffs)
		// Positional cost: 1. Swap cost: 2 (A↔B, B↔A2). Positional wins.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Label Text="B" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A2" />
				<Label Text="B" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Positional is cheaper → 1 property change, no child list change
		Assert.Single(diff.NodeChanges);
		Assert.Equal("A2", diff.NodeChanges[0].PropertyChanges[0].NewValue);
		Assert.Empty(diff.ChildListChanges);
	}

	[Fact]
	public void SmartMatching_ThreeLabels_RotatedOrder()
	{
		// Three Labels rotated: [A, B, C] → [C, A, B]
		// Cost matching should detect the rotation as a reorder.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="A" />
				<Label Text="B" />
				<Label Text="C" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="C" />
				<Label Text="A" />
				<Label Text="B" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Perfect rotation → reorder with 0 property changes
		Assert.Empty(diff.NodeChanges);
		Assert.Single(diff.ChildListChanges);
		Assert.Equal(3, diff.ChildListChanges[0].NewChildren.Count);
		Assert.True(diff.ChildListChanges[0].NewChildren.All(c => c.Kind == ChildChangeKind.Retained));
	}

	[Fact]
	public void SmartMatching_MixedTypes_OnlyDuplicatesUseSmartMatch()
	{
		// Mixed types: Button (unique) + two Labels (duplicate).
		// Labels are swapped. Smart matching should handle the Labels correctly.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="First" />
				<Button Text="Click" />
				<Label Text="Second" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Second" />
				<Button Text="Click" />
				<Label Text="First" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Smart matching detects Label swap → reorder, Button stays in place
		Assert.Empty(diff.NodeChanges);
		Assert.Single(diff.ChildListChanges);
		Assert.Empty(diff.ChildListChanges[0].RemovedNodeIds);
	}

	[Fact]
	public void SmartMatching_IdenticalDuplicates_StaysPositional()
	{
		// Two identical Labels (same properties) — no change.
		// Smart matching should not produce any diffs.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Same" />
				<Label Text="Same" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Same" />
				<Label Text="Same" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Empty(diff.NodeChanges);
		Assert.Empty(diff.ChildListChanges);
	}

	// ---------------------------------------------------------------------------
	// Resource dictionary changes
	// ---------------------------------------------------------------------------

	[Fact]
	public void ResourceAdded_ProducesRootPropertyDiff()
	{
		var old = Parse(Page("""
			<ContentPage.Resources>
				<Color x:Key="AccentColor">DarkBlue</Color>
			</ContentPage.Resources>
			<Label Text="Hello" />
			"""));
		var @new = Parse(Page("""
			<ContentPage.Resources>
				<Color x:Key="AccentColor">DarkBlue</Color>
				<Color x:Key="SecondaryColor">Red</Color>
			</ContentPage.Resources>
			<Label Text="Hello" />
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Resource changes should appear as a root node property change on "Resources"
		Assert.NotEmpty(diff.NodeChanges);
		var rootChange = diff.NodeChanges.First(n => string.IsNullOrEmpty(n.NodeId));
		var resProp = rootChange.PropertyChanges.First(p => p.PropertyName.LocalName == "Resources");
		Assert.Equal(PropertyDiffKind.Set, resProp.Kind);
		Assert.NotNull(resProp.NewNode);
		// The new node should be a ListNode containing all resource elements
		Assert.IsType<ListNode>(resProp.NewNode);
		var listNode = (ListNode)resProp.NewNode;
		Assert.Equal(2, listNode.CollectionItems.Count);
	}

	[Fact]
	public void ResourceRemoved_ProducesRootPropertyDiff()
	{
		var old = Parse(Page("""
			<ContentPage.Resources>
				<Color x:Key="AccentColor">DarkBlue</Color>
				<Color x:Key="SecondaryColor">Red</Color>
			</ContentPage.Resources>
			<Label Text="Hello" />
			"""));
		var @new = Parse(Page("""
			<ContentPage.Resources>
				<Color x:Key="AccentColor">DarkBlue</Color>
			</ContentPage.Resources>
			<Label Text="Hello" />
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.NotEmpty(diff.NodeChanges);
		var rootChange = diff.NodeChanges.First(n => string.IsNullOrEmpty(n.NodeId));
		var resProp = rootChange.PropertyChanges.First(p => p.PropertyName.LocalName == "Resources");
		Assert.Equal(PropertyDiffKind.Set, resProp.Kind);
		Assert.NotNull(resProp.NewNode);
		// With single resource remaining, parser uses ElementNode (not ListNode)
		Assert.IsType<ElementNode>(resProp.NewNode);
	}

	[Fact]
	public void ResourceValueChanged_ProducesRootPropertyDiff()
	{
		var old = Parse(Page("""
			<ContentPage.Resources>
				<Color x:Key="AccentColor">DarkBlue</Color>
			</ContentPage.Resources>
			<Label Text="Hello" />
			"""));
		var @new = Parse(Page("""
			<ContentPage.Resources>
				<Color x:Key="AccentColor">Red</Color>
			</ContentPage.Resources>
			<Label Text="Hello" />
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.NotEmpty(diff.NodeChanges);
		var rootChange = diff.NodeChanges.First(n => string.IsNullOrEmpty(n.NodeId));
		var resProp = rootChange.PropertyChanges.First(p => p.PropertyName.LocalName == "Resources");
		Assert.Equal(PropertyDiffKind.Set, resProp.Kind);
		Assert.NotNull(resProp.NewNode);
	}
}
