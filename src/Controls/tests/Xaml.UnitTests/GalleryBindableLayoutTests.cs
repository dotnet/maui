using System;
using System.Collections.ObjectModel;
using System.Linq;
using Maui.Controls.Sample;
using Microsoft.Maui.Graphics;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.GalleryTestHelpers;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class GalleryBindableLayoutTests : BaseTestFixture
{
	[Theory]
	[InlineData("ItemsSourceNone", 0)]
	[InlineData("ItemsSourceEmptyCollection", 0)]
	[InlineData("ItemsSourceObservableCollection", 4)]
	public void ItemsSourceChangesRebuildAllLayouts(string source, int count)
	{
		var (page, options, model) = CreateGallery();
		Select(options, "ItemsSourceObservableCollection");
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
		var previousSource = Assert.IsType<ObservableCollection<BindableLayoutTestItem>>(model.ItemsSource);

		Select(options, source);
		foreach (var layout in Layouts(page))
		{
			Assert.Same(model.ItemsSource, BindableLayout.GetItemsSource(layout));
			Assert.Equal(count, layout.Children.Count);
		}

		if (count == 0)
		{
			previousSource.Add(new BindableLayoutTestItem("Detached", 4));
			Assert.All(Layouts(page), layout => Assert.Empty(layout.Children));
		}
	}

	[Theory]
	[InlineData("EmptyViewString", false, "No Items Available(String)")]
	[InlineData("EmptyViewGrid", false, "No Items Available(Grid View)")]
	[InlineData("EmptyViewNone", true, "No Template Items Available(Grid View)")]
	[InlineData("EmptyViewString", true, "No Template Items Available(Grid View)")]
	[InlineData("EmptyViewGrid", true, "No Template Items Available(Grid View)")]
	public void EmptyViewTemplateTakesPrecedenceAndRecovers(string emptyView, bool template, string expected)
	{
		var (page, options, _) = CreateGallery();
		Select(options, "ItemsSourceEmptyCollection");
		Select(options, emptyView);
		if (template)
			Select(options, "EmptyViewTemplateGrid");

		AssertCaptions(page, expected);
		Select(options, "ItemsSourceObservableCollection");
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
		Select(options, "ItemsSourceEmptyCollection");
		AssertCaptions(page, expected);

		if (template && emptyView != "EmptyViewNone")
		{
			Select(options, "EmptyViewTemplateNone");
			AssertCaptions(page, emptyView == "EmptyViewString" ? "No Items Available(String)" : "No Items Available(Grid View)");
		}
	}

	[Theory]
	[InlineData("ItemTemplateBasic")]
	[InlineData("ItemTemplateGrid")]
	[InlineData("ItemTemplateSelectorAlternate")]
	public void ItemTemplatesBindEveryChildAndCanBeReplaced(string template)
	{
		var (page, options, model) = CreateGallery();
		Select(options, "ItemsSourceObservableCollection");
		Select(options, template);
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
		var items = Assert.IsType<ObservableCollection<BindableLayoutTestItem>>(model.ItemsSource);
		foreach (var layout in Layouts(page))
		{
			for (var i = 0; i < items.Count; i++)
			{
				var child = Assert.IsAssignableFrom<View>(layout.Children[i]);
				Assert.Same(items[i], child.BindingContext);
				if (template == "ItemTemplateGrid" || (template == "ItemTemplateSelectorAlternate" && i % 2 != 0))
					Assert.IsType<Grid>(child);
				else
					Assert.IsType<Label>(child);
			}
		}

		Select(options, "ItemTemplateSelectorNone");
		Select(options, "ItemTemplateGrid");
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
		Assert.All(Layouts(page), layout => Assert.All(layout.Children, child => Assert.IsType<Grid>(child)));
	}

	[Theory]
	[InlineData("AddItems", "", "Passionfruit,Dragonfruit,Apple,Banana,Carrot,Broccoli")]
	[InlineData("AddItems", "2", "Apple,Banana,Chikoo,Carrot,Broccoli")]
	[InlineData("RemoveItems", "", "Apple,Banana")]
	[InlineData("RemoveItems", "3", "Apple,Banana,Carrot")]
	[InlineData("ReplaceItems", "", "Cat,Dog,Carrot,Broccoli")]
	[InlineData("ReplaceItems", "2", "Apple,Banana,Monkey,Broccoli")]
	public void CollectionOperationsUpdateAllLayouts(string command, string index, string expected)
	{
		var (page, options, _) = CreateGallery();
		Select(options, "ItemsSourceObservableCollection");
		Find<Entry>(page, "IndexEntry").Text = index;
		Click(page, command);
		if (index.Length == 0)
			Click(page, command);

		Assert.Equal(string.Empty, Find<Entry>(page, "IndexEntry").Text);
		AssertCaptions(page, expected.Split(','));
		AssertGridPositions(page);
	}

	[Theory]
	[InlineData("RemoveItems", "99")]
	[InlineData("RemoveItems", "-1")]
	[InlineData("ReplaceItems", "99")]
	[InlineData("ReplaceItems", "-1")]
	[InlineData("AddItems", "99")]
	[InlineData("AddItems", "invalid")]
	public void InvalidCollectionOperationsPreserveChildren(string command, string index)
	{
		var (page, options, _) = CreateGallery();
		Select(options, "ItemsSourceObservableCollection");
		var children = Layouts(page).Select(layout => layout.Children.ToArray()).ToArray();
		Find<Entry>(page, "IndexEntry").Text = index;
		Click(page, command);
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
		for (var i = 0; i < children.Length; i++)
			Assert.Equal(children[i], Layouts(page)[i].Children);
		Assert.Equal(string.Empty, Find<Entry>(page, "IndexEntry").Text);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void DirectEmptyViewApisUpdateEveryLayout(bool template)
	{
		var (page, options, _) = CreateGallery();
		Select(options, "ItemsSourceEmptyCollection");
		var property = template ? "EmptyViewTemplate" : "EmptyView";
		Click(page, $"Set{property}");
		AssertCaptions(page, template ? "(Set EmptyViewTemplate)" : "(Set EmptyView)");
		foreach (var layout in Layouts(page))
		{
			if (template)
			{
				Assert.NotNull(BindableLayout.GetEmptyViewTemplate(layout));
				Assert.Null(BindableLayout.GetEmptyView(layout));
			}
			else
			{
				Assert.Same(BindableLayout.GetEmptyView(layout), Assert.Single(layout.Children));
				Assert.Null(BindableLayout.GetEmptyViewTemplate(layout));
			}
		}
		Click(page, $"Get{property}");
		Assert.Equal($"[Get {property}] EmptyView={!template} EmptyViewTemplate={template} ItemsSourceCount=0 ItemTemplate=True ItemTemplateSelector=False Has{property}=True",
			Find<Label>(page, "DirectApiSummaryLabel").Text);
	}

	[Fact]
	public void DirectItemsSourceApisPreserveSourceIdentity()
	{
		var (page, options, model) = CreateGallery();
		Select(options, "ItemsSourceObservableCollection");
		Click(page, "SetItemsSource");
		foreach (var layout in Layouts(page))
			Assert.Same(model.ItemsSource, BindableLayout.GetItemsSource(layout));
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
		Click(page, "GetItemsSource");
		Assert.Equal("[Get ItemsSource] EmptyView=False EmptyViewTemplate=False ItemsSourceCount=4 ItemTemplate=True ItemTemplateSelector=False Count=4",
			Find<Label>(page, "DirectApiSummaryLabel").Text);
	}

	[Fact]
	public void DirectItemTemplateApisReplaceGeneratedChildren()
	{
		var (page, options, _) = CreateGallery();
		Select(options, "ItemsSourceObservableCollection");
		Click(page, "SetItemTemplate");
		foreach (var layout in Layouts(page))
		{
			Assert.NotNull(BindableLayout.GetItemTemplate(layout));
			Assert.Null(BindableLayout.GetItemTemplateSelector(layout));
			foreach (var child in layout.Children)
			{
				var grid = Assert.IsType<Grid>(child);
				var label = Assert.IsType<Label>(Assert.Single(grid.Children));
				Assert.Equal(Colors.Green, label.TextColor);
				Assert.Equal(FontAttributes.Bold, label.FontAttributes);
			}
		}
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
		Click(page, "GetItemTemplate");
		Assert.Equal("[Get ItemTemplate] EmptyView=False EmptyViewTemplate=False ItemsSourceCount=4 ItemTemplate=True ItemTemplateSelector=False HasItemTemplate=True",
			Find<Label>(page, "DirectApiSummaryLabel").Text);
	}

	[Fact]
	public void DirectTemplateSelectorApisAlternateTemplatesAndCanBeReplaced()
	{
		var (page, options, model) = CreateGallery();
		Select(options, "ItemsSourceObservableCollection");
		Click(page, "SetItemTemplateSelector");
		foreach (var layout in Layouts(page))
		{
			Assert.Same(model.ItemTemplateSelector, BindableLayout.GetItemTemplateSelector(layout));
			Assert.Null(BindableLayout.GetItemTemplate(layout));
			for (var i = 0; i < layout.Children.Count; i++)
			{
				var label = Assert.IsType<Label>(layout.Children[i]);
				Assert.Equal(i % 2 == 0 ? Colors.Purple : Colors.Green, label.TextColor);
				Assert.Equal(i % 2 == 0 ? FontAttributes.Bold : FontAttributes.Italic, label.FontAttributes);
			}
		}
		AssertCaptions(page, "Apple", "Set Banana", "Carrot", "Set Broccoli");
		Click(page, "GetItemTemplateSelector");
		Assert.Equal("[Get ItemTemplateSelector] EmptyView=False EmptyViewTemplate=False ItemsSourceCount=4 ItemTemplate=False ItemTemplateSelector=True HasSelector=True",
			Find<Label>(page, "DirectApiSummaryLabel").Text);

		Click(page, "SetItemTemplate");
		AssertCaptions(page, "Apple", "Banana", "Carrot", "Broccoli");
	}

	static (BindableLayoutControlMainPage Page, BindableLayoutOptionsPage Options, BindableLayoutViewModel Model) CreateGallery()
	{
		var model = new BindableLayoutViewModel();
		var page = new BindableLayoutControlMainPage(model);
		var options = new BindableLayoutOptionsPage(model);
		page.OnGridLoaded(page.FindByName<Grid>("MainGridBindableLayout"), EventArgs.Empty);
		return (page, options, model);
	}

	static void Select(BindableLayoutOptionsPage options, string name) =>
		options.FindByName<RadioButton>(name).IsChecked = true;

	static Layout[] Layouts(BindableLayoutControlMainPage page) =>
		new Layout[]
		{
			page.FindByName<StackLayout>("MainStackBindableLayout"),
			page.FindByName<FlexLayout>("MainFlexBindableLayout"),
			page.FindByName<Grid>("MainGridBindableLayout")
		};

	static void AssertCaptions(BindableLayoutControlMainPage page, params string[] expected)
	{
		foreach (var layout in Layouts(page))
		{
			Assert.Equal(expected, layout.Children.Cast<View>().Select(child =>
				child is Label label ? label.Text : Assert.Single(child.Descendants().OfType<Label>()).Text));
		}
	}

	static void AssertGridPositions(BindableLayoutControlMainPage page)
	{
		var grid = page.FindByName<Grid>("MainGridBindableLayout");
		Assert.Equal(2, grid.ColumnDefinitions.Count);
		Assert.Equal((grid.Children.Count + 1) / 2, grid.RowDefinitions.Count);
		for (var i = 0; i < grid.Children.Count; i++)
		{
			Assert.Equal(i / 2, Grid.GetRow((BindableObject)grid.Children[i]));
			Assert.Equal(i % 2, Grid.GetColumn((BindableObject)grid.Children[i]));
		}
	}
}
