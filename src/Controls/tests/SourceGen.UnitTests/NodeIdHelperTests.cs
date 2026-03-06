using System.Collections.Generic;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for <see cref="NodeIdHelper.AssignIds"/>.
/// Uses <see cref="GeneratorHelpers.ParseXaml"/> to build real <see cref="SGRootNode"/> trees.
/// </summary>
public class NodeIdHelperTests
{
	// -------------------------------------------------------------------------
	// Helpers
	// -------------------------------------------------------------------------

	static SGRootNode Parse(string xaml) =>
		GeneratorHelpers.ParseXaml(xaml, AssemblyAttributes.Empty)
		?? throw new System.Exception("ParseXaml returned null");

	const string Ns = "http://schemas.microsoft.com/dotnet/2021/maui";

	// -------------------------------------------------------------------------
	// Root ID
	// -------------------------------------------------------------------------

	[Fact]
	public void Root_AlwaysGetsEmptyId()
	{
		var root = Parse($@"<ContentPage xmlns=""{Ns}"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" />");
		var ids = NodeIdHelper.AssignIds(root);
		Assert.Equal("", ids[root]);
	}

	// -------------------------------------------------------------------------
	// Single child
	// -------------------------------------------------------------------------

	[Fact]
	public void SingleChild_GetsExpectedId()
	{
		var root = Parse($@"
			<ContentPage xmlns=""{Ns}"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Label Text=""Hello"" />
			</ContentPage>");

		var ids = NodeIdHelper.AssignIds(root);

		// Find the Label node
		var label = FindByType(ids, "Label");
		Assert.NotNull(label);
		// Child of root — first node in DFS walk gets "0"
		Assert.Equal("0", ids[label]);
	}

	// -------------------------------------------------------------------------
	// Multiple siblings
	// -------------------------------------------------------------------------

	[Fact]
	public void MultipleChildren_IndexedCorrectly()
	{
		var root = Parse($@"
			<VerticalStackLayout xmlns=""{Ns}"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Label Text=""First"" />
				<Button Text=""Second"" />
				<Entry Placeholder=""Third"" />
			</VerticalStackLayout>");

		var ids = NodeIdHelper.AssignIds(root);

		var label = FindByType(ids, "Label");
		var button = FindByType(ids, "Button");
		var entry = FindByType(ids, "Entry");

		Assert.NotNull(label);
		Assert.NotNull(button);
		Assert.NotNull(entry);

		Assert.Equal("0", ids[label]);
		Assert.Equal("1", ids[button]);
		Assert.Equal("2", ids[entry]);
	}

	// -------------------------------------------------------------------------
	// Nested tree
	// -------------------------------------------------------------------------

	[Fact]
	public void NestedChildren_AssignDepthCorrectly()
	{
		var root = Parse($@"
			<ContentPage xmlns=""{Ns}"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<VerticalStackLayout>
					<Label Text=""Nested"" />
				</VerticalStackLayout>
			</ContentPage>");

		var ids = NodeIdHelper.AssignIds(root);

		var vsl = FindByType(ids, "VerticalStackLayout");
		var label = FindByType(ids, "Label");

		Assert.NotNull(vsl);
		Assert.NotNull(label);

		Assert.Equal("0", ids[vsl]);
		Assert.Equal("1", ids[label]);
	}

	// -------------------------------------------------------------------------
	// Structural stability: same XAML produces same IDs
	// -------------------------------------------------------------------------

	[Fact]
	public void SameXaml_ProducesSameIds()
	{
		const string xaml = @"
			<VerticalStackLayout xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Label Text=""Hello"" />
				<Button Text=""World"" />
			</VerticalStackLayout>";

		var ids1 = NodeIdHelper.AssignIds(Parse(xaml));
		var ids2 = NodeIdHelper.AssignIds(Parse(xaml));

		// IDs should match structurally
		var label1 = FindByType(ids1, "Label");
		var label2 = FindByType(ids2, "Label");
		Assert.Equal(ids1[label1!], ids2[label2!]);

		var btn1 = FindByType(ids1, "Button");
		var btn2 = FindByType(ids2, "Button");
		Assert.Equal(ids1[btn1!], ids2[btn2!]);
	}

	// -------------------------------------------------------------------------
	// All nodes are present
	// -------------------------------------------------------------------------

	[Fact]
	public void AllElementNodes_AreInDictionary()
	{
		var root = Parse($@"
			<VerticalStackLayout xmlns=""{Ns}"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Label Text=""A"" />
				<Button Text=""B"" />
			</VerticalStackLayout>");

		var ids = NodeIdHelper.AssignIds(root);

		// Root + 2 children = 3 entries
		Assert.Equal(3, ids.Count);
	}

	// -------------------------------------------------------------------------
	// Empty root (no children)
	// -------------------------------------------------------------------------

	[Fact]
	public void EmptyRoot_OnlyContainsRoot()
	{
		var root = Parse($@"<ContentPage xmlns=""{Ns}"" xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" />");
		var ids = NodeIdHelper.AssignIds(root);
		Assert.Single(ids);
		Assert.Equal("", ids[root]);
	}

	// -------------------------------------------------------------------------
	// Helper
	// -------------------------------------------------------------------------

	static ElementNode? FindByType(Dictionary<ElementNode, string> ids, string typeName)
	{
		foreach (var kvp in ids)
		{
			if (kvp.Key.XmlType.Name == typeName)
				return kvp.Key;
		}
		return null;
	}
}
