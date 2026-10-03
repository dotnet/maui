using System.Linq;
using Maui.Controls.Sample;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.GalleryTestHelpers;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class GalleryMenuBarTests : BaseTestFixture
{
	[Fact]
	public void AddingLocationsUpdatesCollectionMenuAndEntryBindings()
	{
		var page = new MenuBarItemControlPage();
		var model = Assert.IsType<MenuBarItemViewModel>(page.BindingContext);
		foreach (var name in new[] { "Tokyo, JP", "Paris, FR" })
		{
			Activate(page, "Add Location");
			SetEntry(page, name);
			Click(page, "ConfirmButton");
			Assert.Equal(name, model.Locations.Last().Name);
			Assert.Equal($"Added location: {name}", Find<Label>(page, "StatusMessageLabel").Text);
			Assert.False(EntryContainer(page).IsVisible);
		}

		var submenu = page.MenuBarItems.SelectMany(bar => bar).OfType<MenuFlyoutSubItem>().Single();
		Assert.Equal(model.Locations.Select(location => location.Name), submenu.OfType<MenuFlyoutItem>().Select(item => item.Text));
		var selected = submenu.OfType<MenuFlyoutItem>().Last();
		((IMenuElement)selected).Clicked();
		Assert.Equal("Paris, FR", Find<Label>(page, "CurrentLocationLabel").Text);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void EmptyLocationIsRejectedWithoutChangingCollection(string text)
	{
		var page = new MenuBarItemControlPage();
		var model = Assert.IsType<MenuBarItemViewModel>(page.BindingContext);
		var original = model.Locations.ToArray();
		Activate(page, "Add Location");
		SetEntry(page, text);
		Click(page, "ConfirmButton");
		Assert.Equal(original, model.Locations.ToArray());
		Assert.Equal("Location name cannot be empty", Find<Label>(page, "StatusMessageLabel").Text);
		Assert.True(EntryContainer(page).IsVisible);
		SetEntry(page, "Tokyo, JP");
		Click(page, "ConfirmButton");
		Assert.Equal("Tokyo, JP", model.Locations.Last().Name);
		Assert.False(EntryContainer(page).IsVisible);
	}

	[Fact]
	public void CancelDiscardsInputAndNextAddStartsEmpty()
	{
		var page = new MenuBarItemControlPage();
		var model = Assert.IsType<MenuBarItemViewModel>(page.BindingContext);
		var original = model.Locations.ToArray();
		Activate(page, "Add Location");
		SetEntry(page, "Cancelled Location");
		Click(page, "CancelButton");
		Assert.Equal(original, model.Locations.ToArray());
		Assert.Equal("Operation cancelled", Find<Label>(page, "StatusMessageLabel").Text);
		Assert.False(EntryContainer(page).IsVisible);
		Activate(page, "Add Location");
		Assert.Equal(string.Empty, Find<Entry>(page, "LocationEntry").Text);
		Assert.True(EntryContainer(page).IsVisible);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(2)]
	public void EditAndRemoveUseTheSelectedLocation(int index)
	{
		var page = new MenuBarItemControlPage();
		var model = Assert.IsType<MenuBarItemViewModel>(page.BindingContext);
		var selected = model.Locations[index];
		selected.IsSelected = true;
		Assert.Equal(index, model.SelectedLocationIndex);
		Activate(page, "Edit Location");
		Assert.Equal(selected.Name, Find<Entry>(page, "LocationEntry").Text);
		SetEntry(page, "Seattle, USA");
		Click(page, "ConfirmButton");
		Assert.Same(selected, model.Locations[index]);
		Assert.Equal("Seattle, USA", selected.Name);
		Assert.False(EntryContainer(page).IsVisible);

		Activate(page, "Remove Location");
		Assert.DoesNotContain(selected, model.Locations);
		Assert.Equal(-1, model.SelectedLocationIndex);
		Assert.Equal("Removed location: Seattle, USA", Find<Label>(page, "StatusMessageLabel").Text);
	}

	[Fact]
	public void EditAndRemoveWithoutSelectionDoNotChangeLocations()
	{
		var page = new MenuBarItemControlPage();
		var model = Assert.IsType<MenuBarItemViewModel>(page.BindingContext);
		var original = model.Locations.ToArray();
		Activate(page, "Edit Location");
		Assert.Equal("Please select a location to edit by checking its checkbox", model.StatusMessage);
		Activate(page, "Remove Location");
		Assert.Equal("Please select a location to remove by checking its checkbox", model.StatusMessage);
		Assert.Equal(original, model.Locations.ToArray());
		Assert.False(EntryContainer(page).IsVisible);
	}

	[Fact]
	public void ResetRestoresDefaultLocationsSelectionAndCommands()
	{
		var page = new MenuBarItemControlPage();
		var model = Assert.IsType<MenuBarItemViewModel>(page.BindingContext);
		Activate(page, "Add Location");
		SetEntry(page, "Custom Location");
		Click(page, "ConfirmButton");
		model.Locations[1].IsSelected = true;
		model.FileMenuEnabled = model.LocationsMenuEnabled = model.ViewMenuEnabled = false;
		Click(page, "ResetButton");
		Assert.Equal(new[] { "Redmond, USA", "London, UK", "Berlin, DE" }, model.Locations.Select(item => item.Name));
		Assert.All(model.Locations, item => Assert.False(item.IsSelected));
		Assert.Equal(-1, model.SelectedLocationIndex);
		Assert.Equal("Not set", Find<Label>(page, "CurrentLocationLabel").Text);
		Assert.Equal("Application state has been reset.", Find<Label>(page, "StatusMessageLabel").Text);
		Assert.True(model.FileMenuEnabled && model.LocationsMenuEnabled && model.ViewMenuEnabled);
		Assert.False(EntryContainer(page).IsVisible);
	}

	[Fact]
	public void RefreshCommandRespectsCanExecuteAndRecovers()
	{
		var page = new MenuBarItemControlPage();
		var model = Assert.IsType<MenuBarItemViewModel>(page.BindingContext);
		model.ViewMenuEnabled = false;
		var original = model.StatusMessage;
		Activate(page, "RefreshMenuBarFlyoutItem");
		Assert.Equal(original, model.StatusMessage);
		model.ViewMenuEnabled = true;
		Activate(page, "RefreshMenuBarFlyoutItem");
		Assert.Equal("Refreshed", Find<Label>(page, "StatusMessageLabel").Text);
	}

	static void Activate(MenuBarItemControlPage page, string text)
	{
		var item = page.MenuBarItems.SelectMany(bar => bar).OfType<MenuFlyoutItem>().Single(item => item.Text == text);
		((IMenuElement)item).Clicked();
	}

	static void SetEntry(MenuBarItemControlPage page, string text) =>
		Find<Entry>(page, "LocationEntry").Text = text;

	static VisualElement EntryContainer(MenuBarItemControlPage page) =>
		Assert.IsAssignableFrom<VisualElement>(Find<Entry>(page, "LocationEntry").Parent);
}
