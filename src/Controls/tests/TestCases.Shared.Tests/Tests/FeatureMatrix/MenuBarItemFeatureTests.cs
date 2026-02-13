// This feature test is applicable only on desktop platforms (Windows and Mac).
#if MACCATALYST || WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Shell)]
public class MenuBarItemFeatureTests : _GalleryUITest
{
	public const string MenuBarItemFeatureMatrix = "MenuBarItem Feature Matrix";
	public override string GalleryPageName => MenuBarItemFeatureMatrix;

	public MenuBarItemFeatureTests(TestDevice device)
	 : base(device)
	{
	}


#if WINDOWS
	[Test, Order(1)]
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

	[Test, Order(2)]
	public void MenuBarItem_RefreshMenuItemProperties()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open View menu
		App.WaitForElement("ViewMenuBar");
		App.Tap("ViewMenuBar");

		VerifyScreenshot();
	}
	[Test, Order(3)]
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

	[Test, Order(4)]
	public void MenuBarItem_MediaMenuBarItemPresent()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Open Locations menu which has a separator
		App.WaitForElement("MediaMenuBar");
		App.Tap("MediaMenuBar");

		// Take screenshot to verify separator visual appearance
		VerifyScreenshot();
	}
#endif



	[Test, Order(5)]
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

	[Test, Order(6)]
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

	[Test, Order(7)]
	public void MenuBarItem_LocationsMenuEditLocation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("LocationCheckBox_0");
		App.Tap("LocationCheckBox_0"); // Select Redmond, USA

		// Open Locations menu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Click Edit Location
		App.WaitForElement("Edit Location");
		App.Tap("Edit Location");

		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.EnterText("LocationEntry", "Seattle, USA");

		App.WaitForElement("ConfirmButton");
		App.Tap("ConfirmButton");

		App.FindElementByText("Seattle, USA");

	}

	[Test, Order(8)]
	public void MenuBarItem_LocationsMenuRemoveLocation()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("LocationCheckBox_2");
		App.Tap("LocationCheckBox_2"); // Select Berlin, DE

		// Open Locations menu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Click Remove Location
		App.WaitForElement("Remove Location");
		App.Tap("Remove Location");

		var locationLabel = App.FindElement("StatusMessageLabel");
		Assert.That(locationLabel.GetText(), Is.EqualTo("Removed location: Berlin, DE"));

	}

	[Test, Order(9)]
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

#if TEST_FAILS_ON_CATALYST //For more info, see: https://github.com/dotnet/maui/issues/34038
	[Test, Order(10)]
	public void MenuBarItem_DisableFileMenu()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Disable File menu
		App.WaitForElement("FileMenuEnabledSwitch");
		App.Tap("FileMenuEnabledSwitch");

		// Try to open File menu
		App.WaitForElement("FileMenuBar");
		App.Tap("FileMenuBar");

		// Verify "Exit" menu item is not accessible when menu is disabled
		var elements = App.FindElements("ExitMenuBarFlyoutItem");
		Assert.That(elements, Is.Empty, "Disabled menu items should not be accessible");
	}

	[Test, Order(11)]
	public void MenuBarItem_DisableLocationsMenu()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Disable Locations menu
		App.WaitForElement("LocationsMenuEnabledSwitch");
		App.Tap("LocationsMenuEnabledSwitch");

		// Try to open Locations menu
		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");

		// Verify "Add Location" menu item is not accessible when menu is disabled
		var elements = App.FindElements("AddLocationMenuFlyoutItem");
		Assert.That(elements, Is.Empty, "Disabled menu items should not be accessible");
	}

	[Test, Order(12)]
	public void MenuBarItem_DisableViewMenu()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Disable View menu
		App.WaitForElement("ViewMenuEnabledSwitch");
		App.Tap("ViewMenuEnabledSwitch");

		// Try to open View menu
		App.WaitForElement("ViewMenuBar");
		App.Tap("ViewMenuBar");

		// Verify "Refresh" menu item is not accessible when menu is disabled
		var elements = App.FindElements("RefreshMenuBarFlyoutItem");
		Assert.That(elements, Is.Empty, "Disabled menu items should not be accessible");
	}
#endif

	[Test, Order(13)]
	public void MenuBarItem_MenuTextBindingUpdates()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		App.WaitForElement("FileMenuBar");
		App.Tap("FileMenuBar");
		App.WaitForElement("ExitMenuBarFlyoutItem");

		App.WaitForElement("ViewMenuBarItem");
		App.Tap("ViewMenuBarItem");

		App.WaitForElement("LocationsMenuBar");
		App.Tap("LocationsMenuBar");
		App.WaitForElement("Change Location");
		App.WaitForElement("Add Location");
		App.WaitForElement("Edit Location");
		App.WaitForElement("Remove Location");

		App.WaitForElement("ViewMenuBarItem");
		App.Tap("ViewMenuBarItem");

		App.WaitForElement("ViewMenuBar");
		App.Tap("ViewMenuBar");
		App.WaitForElement("RefreshMenuBarFlyoutItem");

		App.WaitForElement("ViewMenuBarItem");
		App.Tap("ViewMenuBarItem");

		App.WaitForElement("MediaMenuBar");
		App.Tap("MediaMenuBar");
		App.WaitForElement("Play");
		App.WaitForElement("Pause");
		App.WaitForElement("Stop");
	}

	[Test, Order(14)]
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

	[Test, Order(15)]
	public void MenuBarItem_VerifyAllMenusPresent()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Verify all three menu bar items are present
		App.WaitForElement("FileMenuBar");
		App.WaitForElement("LocationsMenuBar");
		App.WaitForElement("ViewMenuBar");
		App.WaitForElement("MediaMenuBar");

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

	[Test, Order(16)]
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

	[Test, Order(17)]
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
		App.WaitForElement("Operation cancelled");
	}

	[Test, Order(18)]
	public void MenuBarItem_ResetRestoresDefaultLocations()
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
		App.WaitForElement("Custom Location");

		// Reset
		App.Tap("ResetButton");

		// Verify only default locations remain
		App.WaitForElement("Redmond, USA");
		App.WaitForElement("London, UK");
		App.WaitForElement("Berlin, DE");
		App.WaitForNoElement("Custom Location");
	}

	[Test, Order(19)]
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

	[Test, Order(20)]
	public void MenuBarItem_VerifyInitialLocationState()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");

		// Verify initial location is "Not set"
		var locationLabel = App.FindElement("CurrentLocationLabel");
		Assert.That(locationLabel.GetText(), Is.EqualTo("Not set"));

		// Verify default locations in collection
		var locationsCollection = App.WaitForElement("LocationsCollectionView");
		App.WaitForElement("Redmond, USA");
		App.WaitForElement("London, UK");
		App.WaitForElement("Berlin, DE");
	}

	[Test, Order(21)]
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
		App.WaitForNoElement("LocationEntry");
	}

	[Test, Order(22)]
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
}
#endif