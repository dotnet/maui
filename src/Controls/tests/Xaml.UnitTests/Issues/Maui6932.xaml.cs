using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui6932 : ContentPage
{
	public Maui6932() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void ClearingPopulatedLayoutsShowsEachEmptyViewAndAddingHidesIt(XamlInflator inflator)
		{
			var items = new ObservableCollection<int>(Enumerable.Range(0, 10));
			var page = CreatePage(inflator, items);
			AssertItems(page, items);
			for (var i = 10; i < 15; i++)
				items.Add(i);
			AssertItems(page, items);
			items.Clear();
			AssertEmptyViews(page);
			items.Add(15);
			AssertItems(page, items);
			items.Clear();
			AssertEmptyViews(page);
		}

		[Theory]
		[XamlInflatorData]
		internal void RemovingLastItemShowsEachEmptyViewAndAddingHidesIt(XamlInflator inflator)
		{
			var items = new ObservableCollection<int>(Enumerable.Range(0, 10));
			var page = CreatePage(inflator, items);
			while (items.Count > 1)
			{
				items.RemoveAt(0);
				AssertItems(page, items);
			}
			items.RemoveAt(0);
			AssertEmptyViews(page);
			items.Add(10);
			AssertItems(page, items);
		}

		[Theory]
		[XamlInflatorData]
		internal void ReplacingBindingContextUpdatesEmptyViewsAndDetachesOldSource(XamlInflator inflator)
		{
			var oldItems = new ObservableCollection<int>();
			var page = CreatePage(inflator, oldItems);
			AssertEmptyViews(page);
			var newItems = new ObservableCollection<int>();
			page.BindingContext = new
			{
				Items = newItems,
				EmptyViewId = "Replacement view",
				EmptyTemplateId = "Replacement template",
				EmptyText = "Replacement text"
			};
			Assert.Equal("Replacement view", ((Grid)Assert.Single(page.ViewLayout.Children)).AutomationId);
			Assert.Equal("Replacement template", ((Frame)Assert.Single(page.TemplateLayout.Children)).AutomationId);
			Assert.Equal("Replacement text", ((Label)Assert.Single(page.StringLayout.Children)).Text);
			oldItems.Add(1);
			Assert.Single(page.ViewLayout.Children);
			Assert.IsType<Grid>(page.ViewLayout.Children[0]);
			newItems.Add(2);
			AssertItems(page, newItems);
		}

		static Maui6932 CreatePage(XamlInflator inflator, ObservableCollection<int> items) =>
			new Maui6932(inflator)
			{
				BindingContext = new
				{
					Items = items,
					EmptyViewId = "EmptyViewId",
					EmptyTemplateId = "EmptyTemplateId",
					EmptyText = "Nothing to see here"
				}
			};

		static void AssertItems(Maui6932 page, ObservableCollection<int> items)
		{
			foreach (var layout in new[] { page.ViewLayout, page.StringLayout, page.TemplateLayout })
			{
				Assert.Equal(items.Count, layout.Children.Count);
				for (var i = 0; i < items.Count; i++)
				{
					var frame = Assert.IsType<Frame>(layout.Children[i]);
					Assert.Equal(Colors.Yellow, frame.BackgroundColor);
					Assert.Equal(items[i], frame.BindingContext);
					Assert.Equal(items[i].ToString(), Assert.IsType<Label>(frame.Content).Text);
				}
			}
		}

		static void AssertEmptyViews(Maui6932 page)
		{
			var view = Assert.IsType<Grid>(Assert.Single(page.ViewLayout.Children));
			Assert.Equal("No Results", Assert.IsType<Label>(Assert.Single(view.Children)).Text);
			Assert.Equal("EmptyViewId", view.AutomationId);
			Assert.Equal(Colors.Blue, view.BackgroundColor);
			Assert.Same(BindableLayout.GetEmptyView(page.ViewLayout), view);
			Assert.Equal("Nothing to see here", Assert.IsType<Label>(Assert.Single(page.StringLayout.Children)).Text);
			var template = Assert.IsType<Frame>(Assert.Single(page.TemplateLayout.Children));
			Assert.Equal("No items here", Assert.IsType<Label>(template.Content).Text);
			Assert.Equal("EmptyTemplateId", template.AutomationId);
			Assert.Equal(Colors.Fuchsia, template.BackgroundColor);
		}
	}
}
