using System;
using System.Collections.Generic;
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
	// Helpers

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

	// Identical trees → empty diff

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

	// Property changes → correct diffs

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

	// Structural changes → returns null

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
	public void RemovedBoundary_IncludesOwnNameButExcludesNestedNames()
	{
		var old = Parse(Page("""
			<DataTemplate x:Name="NamedTemplate">
				<Label x:Name="TemplateLabel" />
			</DataTemplate>
			"""));
		var @new = Parse(Page(""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		var removedName = Assert.Single(Assert.Single(diff!.ChildListChanges).RemovedNames);
		Assert.Equal("NamedTemplate", removedName.Name);
	}

	[Fact]
	public void GeneratedFields_UseNamespaceAwareBoundaries()
	{
		var root = Parse($"""
			<ContentPage {MauiXmlns} xmlns:local="clr-namespace:Test">
				<local:Style x:Name="CustomStyle">
					<Label x:Name="NestedLabel" />
				</local:Style>
			</ContentPage>
			""");

		var fields = XamlGeneratedFieldCollector.Collect(root);

		Assert.Contains("CustomStyle", fields);
		Assert.Contains("NestedLabel", fields);
	}

	[Fact]
	public void GeneratedFields_IncludeBoundaryAndExcludeItsDescendants()
	{
		var root = Parse(Page("""
			<Style x:Name="NamedStyle">
				<Label x:Name="NestedLabel" />
			</Style>
			"""));

		var fields = XamlGeneratedFieldCollector.Collect(root);

		Assert.Contains("NamedStyle", fields);
		Assert.DoesNotContain("NestedLabel", fields);
	}

	[Fact]
	public void GeneratedFields_ExcludeVisualStateNames()
	{
		var root = Parse(Page("""
			<VisualStateManager.VisualStateGroups>
				<VisualStateGroup x:Name="CommonStates">
					<VisualState x:Name="Normal" />
				</VisualStateGroup>
			</VisualStateManager.VisualStateGroups>
			"""));

		var fields = XamlGeneratedFieldCollector.Collect(root);

		Assert.DoesNotContain("CommonStates", fields);
		Assert.DoesNotContain("Normal", fields);
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

	// Child addition / removal

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

	// Debug output (ToDebugString) — canonical diff verification

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
	public void XName_Changed_IsLocalRebuild_NotGlobalStructural()
	{
		// x:Name generates a field in code-behind, so it can't be textually patched like a normal
		// property — but that unpatchability must stay LOCAL to this one node (rebuilt in place,
		// same id) rather than cascading the whole page to structural.
		var old = Parse(Page("<Label x:Name=\"oldLabel\" Text=\"Hello\" />"));
		var @new = Parse(Page("<Label x:Name=\"newLabel\" Text=\"Hello\" />"));

		var oldIds = NodeIdHelper.AssignIds(old);
		var newIds = NodeIdHelper.AssignIds(@new);
		var diff = XamlNodeDiff.ComputeDiff(old, @new, oldIds, newIds, out var effectiveNewIds);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		var entry = Assert.Single(change.NewChildren);
		Assert.Equal(ChildChangeKind.Rebuilt, entry.Kind); // recreated...
		var oldLabelId = oldIds[(ElementNode)old.CollectionItems[0]];
		Assert.Equal(oldLabelId, entry.NewNodeId); // ...but re-registered under the SAME id
		Assert.Empty(change.RemovedNodeIds); // never unregistered — would race the same-id re-add
	}

	[Fact]
	public void XKey_Changed_CascadesToStructural_NotLocalRebuild()
	{
		// Unlike x:Name, x:Key identifies an entry in a ResourceDictionary. That dictionary is
		// rewritten via the dedicated TryEmitResourceDictionaryItemChange/
		// TryEmitResourceDictionaryChange path (plain add/remove by key), not the Layout-child/
		// content-property path EmitChildListChange (the local-rebuild emitter) understands — so
		// renaming x:Key must still cascade to a full structural (null) result, not a same-id
		// local rebuild (see IsLocallyRebuildable's doc comment).
		var old = Parse($"""
			<ResourceDictionary {MauiXmlns} x:Class="Test.Resources">
				<Color x:Key="OldKey">Red</Color>
			</ResourceDictionary>
			""");
		var @new = Parse($"""
			<ResourceDictionary {MauiXmlns} x:Class="Test.Resources">
				<Color x:Key="NewKey">Red</Color>
			</ResourceDictionary>
			""");

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

	// Edge cases

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

	// Multi-edit scenarios (property + structural changes combined)

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

	// Complex property diff tests (no fallback)

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

	// Property transition matrix: Value ↔ Binding ↔ StaticResource ↔ DynamicResource

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
		Assert.Equal("__MAUI_Content__", prop.PropertyName.LocalName);
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
		Assert.Contains("__MAUI_Content__ = \"World\"", debug, StringComparison.Ordinal);
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

	// Sequential edits (xaml1 → xaml2 → xaml3 → …) — simulating a hot reload session

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

	// Smart same-type sibling matching

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

	// Resource dictionary changes

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

	// Id lifecycle across a version chain — mirrors XamlGenerator.cs's own id
	// bookkeeping exactly (fresh IDs assigned to the new tree starting at the
	// previous NextNodeId, ComputeDiff threading oldIds/newIds/effectiveNewIds,
	// and a full fresh-from-0 reassignment via NodeIdHelper.AssignIds(root, 0, ...)
	// whenever ComputeDiff returns null — the "structural" branch).

	/// <summary>
	/// Applies one version transition the same way <c>XamlGenerator.cs</c> does: assigns fresh
	/// ids to <paramref name="newRoot"/> starting at <paramref name="nextId"/>, diffs against
	/// <paramref name="oldRoot"/>/<paramref name="oldIds"/>, and on a structural (null) result,
	/// throws away all id continuity and reassigns fresh ids to <paramref name="newRoot"/>
	/// starting back at 0 — exactly like the "structural change" branch in XamlGenerator.cs.
	/// Returns the diff (null if structural) and the ids to carry into the next transition.
	/// </summary>
	static XamlTreeDiff? ApplyEdit(
		ElementNode oldRoot, Dictionary<ElementNode, string> oldIds,
		ElementNode newRoot, int nextId,
		out Dictionary<ElementNode, string> idsForNextStep)
	{
		var newIds = NodeIdHelper.AssignIds(newRoot, nextId, out _);
		var diff = XamlNodeDiff.ComputeDiff(oldRoot, newRoot, oldIds, newIds, out var effectiveNewIds);
		if (diff is null)
		{
			// Structural branch: fresh ids from 0, no continuity with oldIds whatsoever.
			idsForNextStep = NodeIdHelper.AssignIds(newRoot, 0, out _);
		}
		else
		{
			idsForNextStep = effectiveNewIds!;
		}
		return diff;
	}

	[Fact]
	public void StructuralResetMidReorder_NoLongerDesyncsIdsFromLiveApp()
	{
		// THE FIX: a reorder that coincides with a transient codegen-sensitive x:Name change (e.g.
		// mid-keystroke while dragging a line) used to trip a GLOBAL "structural" fallback that
		// reset every id in the page to a fresh DFS numbering of the NEW tree — discarding the old
		// ids the LIVE running app had actually registered its objects under. A later ordinary
		// property edit would then address the wrong live object (e.g. a Label instead of an
		// Entry), producing the observed InvalidCastException.
		//
		// Now, a codegen-sensitive property change below the root is contained to just that one
		// node (rebuilt under its SAME id) instead of cascading to a whole-page id reset — so ids
		// stay in sync with the live app through the entire edit sequence below.
		var seed = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
				<Label Text="Static" />
			</VerticalStackLayout>
			"""));
		// Transient: reordered AND x:Name momentarily missing (mid-keystroke state).
		var transientNoName = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Static" />
				<Entry Text="hello" />
			</VerticalStackLayout>
			"""));
		// Settled: same reordered shape, x:Name restored.
		var settled = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Static" />
				<Entry x:Name="entry1" Text="hello" />
			</VerticalStackLayout>
			"""));
		// Final: an ordinary property edit.
		var finalEdit = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Static" />
				<Entry x:Name="entry1" Text="hello" FontSize="20" />
			</VerticalStackLayout>
			"""));

		var seedIds = NodeIdHelper.AssignIds(seed, 0, out var nextId);
		// This is what the LIVE app actually has entry1 registered under — captured once, from the
		// seed, and never revisited (the live app is never explicitly told about ids afterward —
		// it should simply never need to be, because they stop drifting).
		var liveEntryId = seedIds[FindFirstEntry(seed)];

		var d1 = ApplyEdit(seed, seedIds, transientNoName, nextId, out var ids1);
		// Removing x:Name is codegen-sensitive, but is now a LOCAL rebuild of just the Entry
		// (same id, recreated) — not a page-wide structural reset.
		Assert.NotNull(d1);

		var d2 = ApplyEdit(transientNoName, ids1, settled, nextId, out var ids2);
		// Restoring x:Name is likewise a local rebuild, not a cascade.
		Assert.NotNull(d2);

		var d3 = ApplyEdit(settled, ids2, finalEdit, nextId, out var ids3);
		Assert.NotNull(d3); // plain FontSize change -> ordinary patchable diff

		var entryFinalNode = FindFirstEntry(finalEdit);
		var patchedId = ids3[entryFinalNode];

		// THE FIX, VERIFIED: the id the final patch addresses for entry1 now matches what the live
		// app actually registered entry1 under at the seed — no desync, despite the transient
		// name-loss/restore and reorder in between.
		Assert.Equal(liveEntryId, patchedId);
	}

	static ElementNode FindFirstEntry(ElementNode root) =>
		TryFindFirstEntry(root) ?? throw new InvalidOperationException("No Entry found");

	static ElementNode? TryFindFirstEntry(ElementNode node)
	{
		foreach (var item in node.CollectionItems)
		{
			if (item is not ElementNode el)
				continue;
			if (el.XmlType.Name == "Entry")
				return el;
			var found = TryFindFirstEntry(el);
			if (found != null)
				return found;
		}
		return null;
	}

	// Reparenting — current gap. An element identified by x:Name (or otherwise
	// matchable) that moves to a DIFFERENT parent is not matched at all today:
	// XamlNodeDiff's matching (MatchTypeGroupByCost) only runs within one parent's
	// own child list (DiffChildrenWithMatching), so a moved node shows up as a
	// plain remove-from-old-parent + add-to-new-parent, losing its id. These tests
	// document the CURRENT (to-be-fixed) behavior as a baseline for the upcoming
	// tree-global identity-matching change.

	[Fact]
	public void Reparenting_SameXName_DifferentParent_CurrentlyNotMatched()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA">
					<Entry x:Name="entry1" Text="hello" />
				</Border>
				<Border x:Name="borderB" />
			</VerticalStackLayout>
			"""));
		// entry1 moved from borderA to borderB — same x:Name, same type, just a different parent.
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA" />
				<Border x:Name="borderB">
					<Entry x:Name="entry1" Text="hello" />
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Current (buggy) behavior: two separate child-list changes, one per parent — entry1 is
		// removed from borderA's list and a brand-new entry is added to borderB's list. It is NOT
		// recognized as the same element, so it would get a fresh id instead of keeping its old one.
		Assert.Equal(2, diff.ChildListChanges.Count);
		var borderAChange = diff.ChildListChanges.First(c => c.RemovedNodeIds.Count > 0);
		var borderBChange = diff.ChildListChanges.First(c => c.NewChildren.Any(e => e.Kind == ChildChangeKind.Added));
		Assert.Single(borderAChange.RemovedNodeIds);
		Assert.Contains(borderBChange.NewChildren, e => e.Kind == ChildChangeKind.Added);
		// TODO once tree-global identity matching lands: this should instead be recognized as a
		// single "moved" retained node carrying its original id into the new parent, with zero
		// removed/added entries for entry1.
	}

	[Fact]
	public void Reparenting_TypeChangeAtSamePosition_NeverMatches()
	{
		// Clarifies the companion invariant: even with identical x:Name reused at the same slot,
		// a type change must NEVER be treated as the same element (Label -> Button is a new
		// element, old id retired, new id allocated) — this must hold both today and after the
		// upcoming tree-global identity-matching change.
		var old = Parse(Page("""<Label x:Name="thing" Text="Hello" />"""));
		var @new = Parse(Page("""<Button x:Name="thing" Text="Hello" />"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Single(change.RemovedNodeIds); // old Label retired, not reused
		Assert.Equal(1, change.NewChildren.Count(e => e.Kind == ChildChangeKind.Added)); // new Button is fresh
	}

	[Fact]
	public void Reparenting_UnnamedElement_CostMatchable_DifferentParent_CurrentlyNotMatched()
	{
		// No x:Name at all — only cost-based structural similarity could identify this as "the
		// same" Entry across parents (identical properties). Today's matching never gets the
		// chance because it's scoped per-parent, so this is lost even though it's a textbook
		// case for cost-based matching to succeed were it given the opportunity.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA">
					<Entry Text="hello" Placeholder="Search" />
				</Border>
				<Border x:Name="borderB" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA" />
				<Border x:Name="borderB">
					<Entry Text="hello" Placeholder="Search" />
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(2, diff.ChildListChanges.Count);
		Assert.Contains(diff.ChildListChanges, c => c.RemovedNodeIds.Count > 0);
		Assert.Contains(diff.ChildListChanges, c => c.NewChildren.Any(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void Reparenting_WithSimultaneousPropertyChange_CurrentlyNotMatched()
	{
		// Moved AND edited in the same version — the common real-world case (drag a control to a
		// different container, then also tweak a property before the next save).
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA">
					<Entry x:Name="entry1" Text="hello" />
				</Border>
				<Border x:Name="borderB" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA" />
				<Border x:Name="borderB">
					<Entry x:Name="entry1" Text="hello world" />
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Current (buggy) behavior: the property edit is invisible — entry1 is simply removed
		// and a brand-new Entry (with the edited Text already baked in) is added. There is no
		// NodeDiff recording "Text changed", because the matcher never recognizes it's the same
		// node to begin with.
		Assert.Empty(diff.NodeChanges);
		Assert.Equal(2, diff.ChildListChanges.Count);
	}

	[Fact]
	public void Reparenting_AcrossDifferentNestingDepths_CurrentlyNotMatched()
	{
		// Moved from depth 3 to depth 1 — not just a different parent, but a different level
		// entirely. Tree-global matching must not assume "same depth" as a precondition.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border>
					<VerticalStackLayout>
						<Entry x:Name="entry1" Text="hello" />
					</VerticalStackLayout>
				</Border>
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
				<Border>
					<VerticalStackLayout />
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		Assert.Equal(2, diff.ChildListChanges.Count);
		Assert.Contains(diff.ChildListChanges, c => c.RemovedNodeIds.Count > 0);
		Assert.Contains(diff.ChildListChanges, c => c.NewChildren.Any(e => e.Kind == ChildChangeKind.Added));
	}

	[Fact]
	public void Reparenting_IntoNewlyAddedParent_CurrentlyNotMatched()
	{
		// The destination parent doesn't exist in the old tree at all — it's added in the same
		// edit as the move. A correct global matcher should still carry entry1's id forward.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="newBorder">
					<Entry x:Name="entry1" Text="hello" />
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Only ONE child-list change is produced, at the root: Entry removed, Border added.
		// Since Border is wholesale "Added", its own children (including entry1's replacement)
		// are emitted as part of Border's creation code, not as an independently-diffed child
		// list — so entry1 never gets a chance to be recognized as the same node either way.
		var rootChange = Assert.Single(diff.ChildListChanges);
		Assert.Single(rootChange.RemovedNodeIds);
		var addedBorder = Assert.Single(rootChange.NewChildren, e => e.Kind == ChildChangeKind.Added);
		Assert.NotNull(addedBorder.NewElement); // Border's whole subtree, including the "new" Entry, created fresh
	}

	[Fact]
	public void Reparenting_SwapBetweenTwoParents_EachSideLocallyRebuilt_NoCascade()
	{
		// Two named elements cross-swap parents simultaneously. Each Border's child list has
		// exactly one old Entry and one new Entry of the SAME type, so MatchTypeGroupByCost
		// force-pairs them (the only candidates in that type group) even though their x:Name
		// differs between old and new. FIXED: that mismatched pairing no longer trips a
		// page-wide cascade — it's contained to a local rebuild of just that one Entry (within
		// its own Border), same id, reused in place.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA">
					<Entry x:Name="entry1" Text="A" />
				</Border>
				<Border x:Name="borderB">
					<Entry x:Name="entry2" Text="B" />
				</Border>
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA">
					<Entry x:Name="entry2" Text="B" />
				</Border>
				<Border x:Name="borderB">
					<Entry x:Name="entry1" Text="A" />
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// One local rebuild per Border — neither cascades to the other, let alone the whole page.
		Assert.Equal(2, diff.ChildListChanges.Count);
		Assert.All(diff.ChildListChanges, c =>
		{
			var entry = Assert.Single(c.NewChildren);
			Assert.Equal(ChildChangeKind.Rebuilt, entry.Kind);
			Assert.Empty(c.RemovedNodeIds);
		});
	}

	[Fact]
	public void Reparenting_TypeChangedAlso_NeverMatchesEvenWithSameXName()
	{
		// Combines both constraints at once: reparented AND type-changed. Must never match,
		// regardless of how permissive the future global matcher becomes about reparenting.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA">
					<Label x:Name="thing" Text="hello" />
				</Border>
				<Border x:Name="borderB" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA" />
				<Border x:Name="borderB">
					<Button x:Name="thing" Text="hello" />
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var borderAChange = diff.ChildListChanges.First(c => c.RemovedNodeIds.Count > 0);
		var borderBChange = diff.ChildListChanges.First(c => c.NewChildren.Any(e => e.Kind == ChildChangeKind.Added));
		Assert.Single(borderAChange.RemovedNodeIds); // old Label retired
		Assert.Contains(borderBChange.NewChildren, e => e.Kind == ChildChangeKind.Added); // new Button is fresh
	}

	[Fact]
	public void Reparenting_OldParentAlsoRemoved_ChildStillLost()
	{
		// The source parent is itself deleted entirely (not just emptied) — the child moved out
		// of it just before the parent vanished. Confirms the loss isn't an artifact of the old
		// parent surviving; it's the move itself that's unmatched.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border x:Name="borderA">
					<Entry x:Name="entry1" Text="hello" />
				</Border>
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		// borderA (and with it, entry1) is reported wholesale-removed; entry1 is then re-created
		// fresh at the root instead of being recognized as having moved up.
		Assert.Equal(2, change.RemovedNodeIds.Count); // borderA + entry1 subtree
		Assert.Single(change.NewChildren, e => e.Kind == ChildChangeKind.Added); // fresh Entry at root
	}

	// Cascading "blast radius" of a single structural trigger — demonstrates that
	// ComputeDiff returning null is an ALL-OR-NOTHING signal for the whole page,
	// not scoped to the subtree where the actual trigger occurred.

	[Fact]
	public void NestedXNameChange_IsLocalRebuild_UnrelatedSiblingEditStaysOrdinary()
	{
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border>
					<VerticalStackLayout>
						<Label x:Name="deepLabel" Text="Hello" />
					</VerticalStackLayout>
				</Border>
				<Label Text="Unrelated" />
			</VerticalStackLayout>
			"""));
		// Two independent edits in the same version: a harmless property tweak on the completely
		// unrelated sibling Label, AND an x:Name change buried three levels deep.
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border>
					<VerticalStackLayout>
						<Label x:Name="renamedLabel" Text="Hello" />
					</VerticalStackLayout>
				</Border>
				<Label Text="Also unrelated now" />
			</VerticalStackLayout>
			"""));

		var oldIds = NodeIdHelper.AssignIds(old);
		var newIds = NodeIdHelper.AssignIds(@new);
		var diff = XamlNodeDiff.ComputeDiff(old, @new, oldIds, newIds, out _);

		// FIXED: the deeply-nested x:Name change is contained to a local rebuild of just the
		// innermost VerticalStackLayout's single child (same id, recreated) — it does NOT cascade.
		// The completely unrelated sibling Label's property edit survives as an ordinary,
		// independent, patchable NodeDiff.
		Assert.NotNull(diff);
		var childChange = Assert.Single(diff.ChildListChanges);
		var rebuiltEntry = Assert.Single(childChange.NewChildren);
		Assert.Equal(ChildChangeKind.Rebuilt, rebuiltEntry.Kind);
		Assert.Empty(childChange.RemovedNodeIds);

		var unrelatedChange = Assert.Single(diff.NodeChanges);
		Assert.Equal("Also unrelated now", unrelatedChange.PropertyChanges[0].NewValue);
	}

	[Fact]
	public void TypeChangeDeepInTree_IsLocalNotCascading()
	{
		// Contrast with the x:Name-change cascade above: a type change at a child position is
		// NEVER passed into a type-mismatched DiffNode call to begin with — children are
		// partitioned by type BEFORE any pairing/recursion happens (MatchTypeGroupByCost only
		// ever matches within one type group), so a Label -> Button swap is simply "Label
		// removed, Button added" scoped to Border's own child list. It does NOT cascade — the
		// completely unrelated Entry's property edit is still reported as an ordinary,
		// independent, patchable NodeDiff. DiffNode's own "type mismatch -> structural" check is
		// only ever reachable at the root (see DifferentRootType_ReturnsNull).
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border>
					<Label Text="Hello" />
				</Border>
				<Entry Placeholder="Unrelated" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border>
					<Button Text="Hello" />
				</Border>
				<Entry Placeholder="Unrelated edit" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var childChange = Assert.Single(diff.ChildListChanges);
		Assert.Single(childChange.RemovedNodeIds); // old Label retired
		Assert.Single(childChange.NewChildren, e => e.Kind == ChildChangeKind.Added); // new Button
		// The unrelated Entry's property edit survives as a normal, independent patch.
		var entryChange = Assert.Single(diff.NodeChanges);
		Assert.Equal("Unrelated edit", entryChange.PropertyChanges[0].NewValue);
	}

	// Id-stability contrast: a structural reset does NOT always desync ids — only
	// when it coincides with an actual shape change relative to what the live app
	// has. These tests delineate the precise boundary of the bug reproduced above.

	[Fact]
	public void NoNameLossAndRestore_SameShape_IdsStayInSync()
	{
		// The x:Name is removed and then restored, but the tree SHAPE (declaration order) never
		// actually changes in between. FIXED: both the removal and the restoration are now local
		// rebuilds of just the Entry (same id, recreated) — not page-wide resets — so ids were
		// never at risk of drifting here regardless of shape.
		var seed = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
				<Label Text="Static" />
			</VerticalStackLayout>
			"""));
		var transientNoName = Parse(Page("""
			<VerticalStackLayout>
				<Entry Text="hello" />
				<Label Text="Static" />
			</VerticalStackLayout>
			"""));
		var settled = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
				<Label Text="Static" />
			</VerticalStackLayout>
			"""));

		var seedIds = NodeIdHelper.AssignIds(seed, 0, out var nextId);
		var liveEntryId = seedIds[FindFirstEntry(seed)];

		var d1 = ApplyEdit(seed, seedIds, transientNoName, nextId, out var ids1);
		Assert.NotNull(d1); // x:Name removed -> local rebuild, not structural

		var d2 = ApplyEdit(transientNoName, ids1, settled, nextId, out var ids2);
		Assert.NotNull(d2); // x:Name restored -> local rebuild again

		// The Entry's id is carried forward through both rebuilds — no desync.
		Assert.Equal(liveEntryId, ids2[FindFirstEntry(settled)]);
	}

	[Fact]
	public void NameLossDuringReorder_ThenRevertedToOriginalOrder_IdsStayInSync()
	{
		// A reorder happens mid-sequence (coinciding with a transient x:Name loss), but the user
		// then undoes the reorder before the next save. FIXED: both edits are local rebuilds of
		// just the Entry (same id, recreated) rather than page-wide resets, so the Label's id is
		// never touched and the Entry's id is carried forward throughout, regardless of shape.
		var seed = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
				<Label Text="Static" />
			</VerticalStackLayout>
			"""));
		// Reordered AND x:Name transiently missing (triggers a local rebuild of the Entry).
		var reorderedNoName = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Static" />
				<Entry Text="hello" />
			</VerticalStackLayout>
			"""));
		// Undo: back to original order, x:Name restored (also a local rebuild of the Entry, since
		// x:Name is being re-added relative to the previous — no-name — state).
		var revertedToOriginal = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
				<Label Text="Static" />
			</VerticalStackLayout>
			"""));

		var seedIds = NodeIdHelper.AssignIds(seed, 0, out var nextId);
		var liveEntryId = seedIds[FindFirstEntry(seed)];

		var d1 = ApplyEdit(seed, seedIds, reorderedNoName, nextId, out var ids1);
		Assert.NotNull(d1);

		var d2 = ApplyEdit(reorderedNoName, ids1, revertedToOriginal, nextId, out var ids2);
		Assert.NotNull(d2);

		// The Entry's id survived both the reorder-with-rebuild and the revert — no desync.
		Assert.Equal(liveEntryId, ids2[FindFirstEntry(revertedToOriginal)]);
	}

	[Fact]
	public void TypeGroupMatching_PairsByType_NotByCoincidentalContentSimilarity()
	{
		// Both Label and Button exist on both sides here (just at swapped positions), so this is
		// legitimately resolved as a type-correct reorder: old Label <-> new Label (even though
		// its Text changed "Shared"->"Other"), old Button <-> new Button (even though its Text
		// changed "Other"->"Shared", acquiring the content that used to belong to the Label).
		// This guards against a naive content-similarity matcher being fooled into pairing
		// old-Label-"Shared" with new-Button-"Shared" just because the text coincides — type
		// partitioning happens first and is never crossed, regardless of content overlap.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label Text="Shared" FontSize="20" />
				<Button Text="Other" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Button Text="Shared" FontSize="20" />
				<Label Text="Other" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		// Both pairs are retained (type-matched) and recursively diffed for their property churn —
		// nothing is removed or added, and type never crosses.
		var change = Assert.Single(diff.ChildListChanges);
		Assert.Empty(change.RemovedNodeIds);
		Assert.True(change.NewChildren.All(e => e.Kind == ChildChangeKind.Retained));
		Assert.Equal(2, diff.NodeChanges.Count); // property churn recorded on each type-matched pair
	}

	[Fact]
	public void XNameRename_SamePositionSameType_RetainsSameIdViaLocalRebuild()
	{
		// FIXED: this element never moved, never reparented, never changed type — only its
		// x:Name changed. Patchability (can we emit incremental code) and identity (does this id
		// carry forward) are two different questions. Renaming x:Name is still not a simple
		// textual property patch (it affects a code-behind field), but it is no longer treated as
		// "can never be the same element": the node is rebuilt in place under its SAME id, and
		// nothing else in the page is touched.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry1" Text="hello" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Entry x:Name="entry2" Text="hello" />
			</VerticalStackLayout>
			"""));

		var oldIds = NodeIdHelper.AssignIds(old);
		var newIds = NodeIdHelper.AssignIds(@new);
		var diff = XamlNodeDiff.ComputeDiff(old, @new, oldIds, newIds, out _);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		var entry = Assert.Single(change.NewChildren);
		Assert.Equal(ChildChangeKind.Rebuilt, entry.Kind);
		var oldEntryElement = (ElementNode)((ElementNode)old.CollectionItems[0]).CollectionItems[0];
		Assert.Equal(oldIds[oldEntryElement], entry.NewNodeId); // same id retained
		Assert.Empty(change.RemovedNodeIds);
	}

	// Local-rebuild namescope cleanup — a rebuilt node's OLD subtree is entirely discarded, so
	// any x:Name it (or a named descendant) carried must be unregistered, or FindByName/
	// x:Reference would keep resolving the detached old instance.

	[Fact]
	public void XNameChanged_LocalRebuild_UnregistersOldNameFromNamescope()
	{
		var old = Parse(Page("<Label x:Name=\"oldLabel\" Text=\"Hello\" />"));
		var @new = Parse(Page("<Label x:Name=\"newLabel\" Text=\"Hello\" />"));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		var change = Assert.Single(diff!.ChildListChanges);
		var removedName = Assert.Single(change.RemovedNames);
		Assert.Equal("oldLabel", removedName.Name);
	}

	[Fact]
	public void NestedXNameChange_LocalRebuild_UnregistersDeeplyNestedOldName()
	{
		// Same tree as NestedXNameChange_IsLocalRebuild_UnrelatedSiblingEditStaysOrdinary: the
		// rebuilt node here is the OUTER VerticalStackLayout's single child (the inner
		// VerticalStackLayout), whose entire old subtree — including "deepLabel", two levels
		// further down — is discarded. The name must be unregistered even though it belongs to a
		// named DESCENDANT of the rebuilt node, not the rebuilt node itself.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Border>
					<VerticalStackLayout>
						<Label x:Name="deepLabel" Text="Hello" />
					</VerticalStackLayout>
				</Border>
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Border>
					<VerticalStackLayout>
						<Label x:Name="renamedLabel" Text="Hello" />
					</VerticalStackLayout>
				</Border>
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		var change = Assert.Single(diff!.ChildListChanges);
		var removedName = Assert.Single(change.RemovedNames);
		Assert.Equal("deepLabel", removedName.Name);
	}

	[Fact]
	public void XNameChanged_LocalRebuild_WithReorder_UnregistersOldNameFromNamescope()
	{
		// Same local-rebuild-plus-namescope-cleanup requirement, but routed through
		// DiffChildrenWithMatching (not DiffChildrenPositional) because a sibling reorder is
		// also present, exercising the matched-pair rebuild path's own RemovedNames collection.
		var old = Parse(Page("""
			<VerticalStackLayout>
				<Label x:Name="oldLabel" Text="First" />
				<Entry Placeholder="Second" />
			</VerticalStackLayout>
			"""));
		var @new = Parse(Page("""
			<VerticalStackLayout>
				<Entry Placeholder="Second" />
				<Label x:Name="newLabel" Text="First" />
			</VerticalStackLayout>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		var removedName = Assert.Single(change.RemovedNames);
		Assert.Equal("oldLabel", removedName.Name);
	}

	// Rebuild-eligibility boundary — only a plain x:Name change is safe to route through a
	// same-id local rebuild (see IsLocallyRebuildable's doc comment: the rebuild codegen always
	// emits a plain "new {Type}()" and skips all x: directives, which would be wrong for a
	// factory-constructed or open-generic type, and EmitChildListChange doesn't understand
	// ResourceDictionary entries either). Every other codegen-sensitive directive (including
	// x:Key), any codegen-sensitive change inside a template, and any COMPOUND edit where x:Name
	// changes alongside an ineligible directive, must still cascade to a full structural (null)
	// result.

	[Fact]
	public void XFactoryMethodChanged_CascadesToStructural_NotLocalRebuild()
	{
		var old = Parse(Page("""<Label x:FactoryMethod="Create" Text="Hello" />"""));
		var @new = Parse(Page("""<Label x:FactoryMethod="CreateOther" Text="Hello" />"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void XTypeArgumentsChanged_TreatedAsTypeChange_NotSameIdLocalRebuild()
	{
		// Unlike x:FactoryMethod (a plain property-diff failure gated by IsLocallyRebuildable),
		// x:TypeArguments is baked into the constructed generic XmlType itself, so
		// ContentView<string> vs ContentView<int> are already unequal XmlTypes before
		// DiffProperties ever runs. That routes this case through the ordinary type-change
		// "remove old (old id) + add new (FRESH id)" path (like Label -> Button), which is
		// inherently safe — it never reuses the old id, so it needs no IsLocallyRebuildable gate.
		var old = Parse(Page("""<ContentView x:TypeArguments="x:String" />"""));
		var @new = Parse(Page("""<ContentView x:TypeArguments="x:Int32" />"""));

		var oldIds = NodeIdHelper.AssignIds(old);
		var newIds = NodeIdHelper.AssignIds(@new, 100, out _); // offset so reuse vs. fresh is distinguishable
		var diff = XamlNodeDiff.ComputeDiff(old, @new, oldIds, newIds, out _);

		Assert.NotNull(diff);
		var change = Assert.Single(diff.ChildListChanges);
		var oldElem = (ElementNode)old.CollectionItems[0];
		var newElem = (ElementNode)(@new).CollectionItems[0];
		Assert.Single(change.RemovedNodeIds, oldIds[oldElem]); // old retired under its OWN id
		var added = Assert.Single(change.NewChildren, e => e.Kind == ChildChangeKind.Added);
		Assert.Equal(newIds[newElem], added.NewNodeId); // new gets its OWN fresh id, not the old one reused
	}

	[Fact]
	public void XNameChanged_InsideDataTemplate_CascadesToStructural_NotLocalRebuild()
	{
		// Unlike the plain-tree case, an x:Name change on a node realized per-instance inside a
		// DataTemplate/ControlTemplate can't be routed through a local rebuild: the parent
		// resolution EmitChildListChange relies on (XamlComponentRegistry.TryGet) only covers the
		// template's own declaration-time instance, not the (potentially many) realized template
		// instances tracked via RegisterTemplateComponent/GetTemplateComponents.
		var old = Parse(Page("""
			<DataTemplate x:Name="RowTemplate">
				<Label x:Name="oldLabel" Text="Hello" />
			</DataTemplate>
			"""));
		var @new = Parse(Page("""
			<DataTemplate x:Name="RowTemplate">
				<Label x:Name="newLabel" Text="Hello" />
			</DataTemplate>
			"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}

	[Fact]
	public void XNameAndXFactoryMethodBothChanged_CascadesToStructural_NotLocalRebuild()
	{
		// Compound edit: x:Name (eligible) and x:FactoryMethod (not eligible) change together on
		// the same node. DiffProperties must not let the eligible directive mask the ineligible
		// one — the local-rebuild creation path always emits a plain "new {Type}()", which would
		// silently discard the factory-method change if this were allowed through as a rebuild.
		var old = Parse(Page("""<Label x:Name="oldLabel" x:FactoryMethod="Create" Text="Hello" />"""));
		var @new = Parse(Page("""<Label x:Name="newLabel" x:FactoryMethod="CreateOther" Text="Hello" />"""));

		var diff = XamlNodeDiff.ComputeDiff(old, @new);

		Assert.Null(diff);
	}
}

