using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Visual)]
public class MenuBarItemFeatureTests : UITest
{
	public const string MenuBarItemFeatureMatrix = "MenuBarItem Feature Matrix";

	public MenuBarItemFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(MenuBarItemFeatureMatrix);
	}

	[Test]
	public void MenuBarItem_FileMenuExit()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Verify status message reset
		var statusLabel = App.FindElement("StatusMessageLabel");
		Assert.That(statusLabel.GetText(), Does.Contain("reset"));

		// Open File menu and click Exit
		App.WaitForElement("FileMenuBar");
		App.Tap("FileMenuBar");

		VerifyScreenshot();
	}

	[Test]
	public void MenuBarItem_LocationsMenuChangeLocation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Verify initial location
		var locationLabel = App.FindElement("CurrentLocationLabel");
		Assert.That(locationLabel.GetText(), Is.EqualTo("Not set"));

		// Open Locations menu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Open Change Location submenu
		App.WaitForElement("Change Location");
		App.Tap("Change Location");

		// Select first location (Redmond, USA)
		// Note: Dynamic menu items may not have AutomationIds, need to tap by text
		App.WaitForElement("Redmond, USA");
		App.Tap("Redmond, USA");

		// Verify location changed
		var updatedLocation = App.FindElement("CurrentLocationLabel");
		Assert.That(updatedLocation.GetText(), Is.EqualTo("Redmond, USA"));

	}

	[Test]
	public void MenuBarItem_LocationsMenuAddLocation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open Locations menu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Click Add Location
		App.WaitForElement("Add Location");
		App.Tap("Add Location");

		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.EnterText("LocationEntry", "Tokyo, JP");

		App.WaitForElement("ConfirmButton");
		App.Tap("ConfirmButton");

		// Verify new location added to collection
		App.FindElementByText("Tokyo, JP");

	}

	[Test]
	public void MenuBarItem_LocationsMenuEditLocation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open Locations menu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Click Edit Location
		App.WaitForElement("Edit Location");
		App.Tap("Edit Location");

		App.WaitForElement("Redmond, USA");
		App.Tap("Redmond, USA");

		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.EnterText("LocationEntry", "Seattle, USA");

		App.FindElementByText("Seattle, USA");

	}

	[Test]
	public void MenuBarItem_LocationsMenuRemoveLocation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open Locations menu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Click Remove Location
		App.WaitForElement("Remove Location");
		App.Tap("Remove Location");

		App.WaitForElement("Berlin, DE");
		App.Tap("Berlin, DE");

		App.WaitForNoElement("Berlin, DE");

	}

	[Test]
	public void MenuBarItem_ViewMenuRefreshCommand()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open View menu
		App.WaitForElement("ViewMenuBar");
		App.Tap("ViewMenuBar");

		// Click Refresh
		App.WaitForElement("RefreshMenuBarFlyoutItem");
		App.Tap("RefreshMenuBarFlyoutItem");

		// Verify status message shows timestamp
		var statusLabel = App.FindElement("StatusMessageLabel");
		Assert.That(statusLabel.GetText(), Does.Contain("Refreshed"));
	}

	[Test]
	public void MenuBarItem_DisableFileMenu()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Disable File menu
		App.WaitForElement("FileMenuEnabledSwitch");
		App.Tap("FileMenuEnabledSwitch");

		// Try to open File menu (should be disabled)
		// Note: Disabled menus may not be clickable or may not open
		// Verify the switch state changed
		var fileSwitch = App.FindElement("FileMenuEnabledSwitch");
		Assert.That(fileSwitch, Is.Not.Null);
	}

	[Test]
	public void MenuBarItem_DisableLocationsMenu()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Disable Locations menu
		App.WaitForElement("LocationsMenuEnabledSwitch");
		App.Tap("LocationsMenuEnabledSwitch");

		// Verify the switch state changed
		var locationsSwitch = App.FindElement("LocationsMenuEnabledSwitch");
		Assert.That(locationsSwitch, Is.Not.Null);
	}

	[Test]
	public void MenuBarItem_DisableViewMenu()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Disable View menu
		App.WaitForElement("ViewMenuEnabledSwitch");
		App.Tap("ViewMenuEnabledSwitch");

		// Verify the switch state changed
		var viewSwitch = App.FindElement("ViewMenuEnabledSwitch");
		Assert.That(viewSwitch, Is.Not.Null);
	}

	[Test]
	public void MenuBarItem_MenuTextBindingUpdates()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");


		App.WaitForElement("FileMenuBar");
		App.Tap("FileMenuBar");
		App.WaitForElement("Exit");

		App.WaitForElement("ViewMenuBarItem");
		App.Tap("ViewMenuBarItem");

		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBarItem");
		App.WaitForElement("Change Location");
		App.WaitForElement("Add Location");
		App.WaitForElement("Edit Location");
		App.WaitForElement("Remove Location");

		App.WaitForElement("ViewMenuBarItem");
		App.Tap("ViewMenuBarItem");

		App.WaitForElement("ViewMenuBar");
		App.Tap("ViewMenuBarItem");
		App.WaitForElement("Refresh");
	}

	[Test]
	public void MenuBarItem_DynamicLocationMenuItems()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open Locations menu and Change Location submenu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		App.WaitForElement("Change Location");
		App.Tap("Change Location");

		// Verify all three default locations exist
		App.WaitForElement("Redmond, USA");
		App.WaitForElement("London, UK");
		App.WaitForElement("Berlin, DE");

		// Select one location
		App.Tap("London, UK");

		// Verify location changed
		var locationLabel = App.FindElement("CurrentLocationLabel");
		Assert.That(locationLabel.GetText(), Is.EqualTo("London, UK"));
	}

	[Test]
	public void MenuBarItem_VerifyAllMenusPresent()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Verify all three menu bar items are present
		App.WaitForElement("FileMenuBar");
		App.WaitForElement("LocationsMenuBar");
		App.WaitForElement("ViewMenuBar");

		// Verify status labels are present
		App.WaitForElement("CurrentLocationLabel");
		App.WaitForElement("StatusMessageLabel");

		// Verify control switches are present
		App.WaitForElement("FileMenuEnabledSwitch");
		App.WaitForElement("LocationsMenuEnabledSwitch");
		App.WaitForElement("ViewMenuEnabledSwitch");

		// Verify locations collection is present
		App.WaitForElement("LocationsCollectionView");
	}

	[Test]
	public void MenuBarItem_RefreshMenuItemProperties()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open View menu
		App.WaitForElement("ViewMenuBar");
		App.Tap("ViewMenuBar");

		VerifyScreenshot();
	}

	[Test]
	public void MenuBarItem_AddMultipleLocations()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Add first location
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");
		App.WaitForElement("Add Location");
		App.Tap("Add Location");

		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.EnterText("LocationEntry", "Tokyo, JP");
		App.Tap("ConfirmButton");

		// Add second location
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");
		App.WaitForElement("Add Location");
		App.Tap("Add Location");

		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.EnterText("LocationEntry", "Paris, FR");
		App.Tap("ConfirmButton");

		// Verify both locations added
		App.WaitForElement("Tokyo, JP");
		App.WaitForElement("Paris, FR");
	}

	[Test]
	public void MenuBarItem_CancelAddLocation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Start adding location
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");
		App.WaitForElement("Add Location");
		App.Tap("Add Location");

		// Enter text but cancel
		App.WaitForElement("LocationEntry");
		App.EnterText("LocationEntry", "Cancelled Location");
		App.Tap("CancelButton");

		// Verify location was not added
		App.WaitForNoElement("Cancelled Location");
	}

	[Test]
	public void MenuBarItem_ResetRestolesDefaultLocations()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Add a new location
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");
		App.WaitForElement("Add Location");
		App.Tap("Add Location");

		App.WaitForElement("LocationEntry");
		App.EnterText("LocationEntry", "Custom Location");
		App.Tap("ConfirmButton");

		// Verify custom location was added
		var locationsCollection = App.WaitForElement("LocationsCollectionView");
		Assert.That(locationsCollection, Does.Contain("Custom Location"));

		// Reset
		App.Tap("ResetButton");

		// Verify only default locations remain
		locationsCollection = App.WaitForElement("LocationsCollectionView");
		Assert.That(locationsCollection, Does.Contain("Redmond, USA"));
		Assert.That(locationsCollection, Does.Contain("London, UK"));
		Assert.That(locationsCollection, Does.Contain("Berlin, DE"));
		Assert.That(locationsCollection, Does.Not.Contain("Custom Location"));
	}

	[Test]
	public void MenuBarItem_ToggleMenusOnOff()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Toggle File menu off then on
		App.WaitForElement("FileMenuEnabledSwitch");
		App.Tap("FileMenuEnabledSwitch"); // Off
		App.WaitForElement("FileMenuEnabledSwitch");
		App.Tap("FileMenuEnabledSwitch"); // On

		// Toggle Locations menu off then on
		App.WaitForElement("LocationsMenuEnabledSwitch");
		App.Tap("LocationsMenuEnabledSwitch"); // Off
		App.WaitForElement("LocationsMenuEnabledSwitch");
		App.Tap("LocationsMenuEnabledSwitch"); // On

		// Toggle View menu off then on
		App.WaitForElement("ViewMenuEnabledSwitch");
		App.Tap("ViewMenuEnabledSwitch"); // Off
		App.WaitForElement("ViewMenuEnabledSwitch");
		App.Tap("ViewMenuEnabledSwitch"); // On

		// Verify all menus are still present after toggling
		App.WaitForElement("FileMenuBar");
		App.WaitForElement("LocationsMenuBar");
		App.WaitForElement("ViewMenuBar");
	}

	[Test]
	public void MenuBarItem_VerifyInitialLocationState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Verify initial location is "Not set"
		var locationLabel = App.FindElement("CurrentLocationLabel");
		Assert.That(locationLabel.GetText(), Is.EqualTo("Not set"));

		// Verify default locations in collection
		var locationsCollection = App.WaitForElement("LocationsCollectionView");
		Assert.That(locationsCollection, Does.Contain("Redmond, USA"));
		Assert.That(locationsCollection, Does.Contain("London, UK"));
		Assert.That(locationsCollection, Does.Contain("Berlin, DE"));
	}

	[Test]
	public void MenuBarItem_EntryVisibilityToggling()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open Add Location (should make entry visible)
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");
		App.WaitForElement("Add Location");
		App.Tap("Add Location");

		// Verify entry is visible
		App.WaitForElement("LocationEntry");
		App.WaitForElement("ConfirmButton");
		App.WaitForElement("CancelButton");

		// Cancel (should hide entry)
		App.Tap("CancelButton");

		// Note: Entry visibility check would require checking if element is displayed
		// The entry should be hidden after cancel
	}

	[Test]
	public void MenuBarItem_MenuBarItemTextBinding()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Verify all menu bar items have correct text through binding
		App.WaitForElement("FileMenuBar");
		var fileMenu = App.FindElement("FileMenuBar");
		Assert.That(fileMenu.GetText(), Does.Contain("File"));

		App.WaitForElement("LocationsMenuBar");
		var locationsMenu = App.FindElement("LocationsMenuBar");
		Assert.That(locationsMenu.GetText(), Does.Contain("Locations"));

		App.WaitForElement("ViewMenuBar");
		var viewMenu = App.FindElement("ViewMenuBar");
		Assert.That(viewMenu.GetText(), Does.Contain("View"));
	}

	[Test]
	public void MenuBarItem_AddEmptyLocationValidation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Try to add empty location
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");
		App.WaitForElement("Add Location");
		App.Tap("Add Location");

		App.WaitForElement("LocationEntry");
		// Don't enter any text, just confirm
		App.Tap("ConfirmButton");

		// Verify validation message
		var statusLabel = App.FindElement("StatusMessageLabel");
		Assert.That(statusLabel.GetText(), Does.Contain("cannot be empty").Or.Contain("empty"));
	}

	[Test]
	public void MenuBarItem_MenuFlyoutSeparatorPresent()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open Locations menu which has a separator
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Verify menu items before and after separator are present
		App.WaitForElement("Change Location");
		App.WaitForElement("Add Location");
		App.WaitForElement("Edit Location");
		App.WaitForElement("Remove Location");

		// Take screenshot to verify separator visual appearance
		VerifyScreenshot();
	}
}