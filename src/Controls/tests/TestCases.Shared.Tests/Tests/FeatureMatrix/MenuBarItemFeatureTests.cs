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

	IQuery MenuItemQuery(string text) => Device == TestDevice.Mac
		? AppiumQuery.ByXPath($"//*[self::XCUIElementTypeMenuBarItem or self::XCUIElementTypeMenuItem][@title='{text}' or @label='{text}' or @name='{text}']")
		: AppiumQuery.ByName(text);

	void TapMenuItem(string text)
	{
		var query = MenuItemQuery(text);
		App.WaitForElement(query);
		Assert.That(() =>
		{
			var bounds = App.FindElement(query).GetRect();
			return bounds.Width > 0 && bounds.Height > 0;
		}, Is.True.After(5000, 100), $"Menu item '{text}' must be open before it can be selected.");
		App.Tap(query);
	}

	void ResetLocations()
	{
		// Earlier menu-appearance tests leave a native menu open. Dismiss it without executing an item.
		App.WaitForElement("ViewMenuBarItem");
		App.Tap("ViewMenuBarItem");
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		AssertText("StatusMessageLabel", "Application state has been reset.");
		AssertText("CurrentLocationLabel", "Not set");
		App.WaitForNoElement("LocationEntry");
	}

	void AssertText(string automationId, string text)
	{
		App.WaitForElement(automationId);
		Assert.That(() => App.FindElement(automationId).GetText(), Is.EqualTo(text).After(5000, 100));
	}

	void AddLocation(string location)
	{
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Add Location");
		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.EnterText("LocationEntry", location);
		App.Tap("ConfirmButton");
		App.WaitForNoElement("LocationEntry");
		AssertText("StatusMessageLabel", $"Added location: {location}");
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
		ResetLocations();
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Change Location");
		TapMenuItem("Redmond, USA");
		AssertText("CurrentLocationLabel", "Redmond, USA");
		AssertText("StatusMessageLabel", "Location changed to: Redmond, USA");
	}

	[Test, Order(6)]
	public void MenuBarItem_LocationsMenuAddLocation()
	{
		ResetLocations();
		AddLocation("Tokyo, JP");
		AssertText("LocationName_3", "Tokyo, JP");
	}

	[Test, Order(7)]
	public void MenuBarItem_LocationsMenuEditLocation()
	{
		ResetLocations();
		App.WaitForElement("LocationCheckBox_0");
		App.Tap("LocationCheckBox_0"); // Select Redmond, USA
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Edit Location");
		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.EnterText("LocationEntry", "Seattle, USA");
		App.WaitForElement("ConfirmButton");
		App.Tap("ConfirmButton");
		App.WaitForNoElement("LocationEntry");
		AssertText("LocationName_0", "Seattle, USA");
		AssertText("StatusMessageLabel", "Updated 'Redmond, USA' to 'Seattle, USA'");
	}

	[Test, Order(8)]
	public void MenuBarItem_LocationsMenuRemoveLocation()
	{
		ResetLocations();
		App.WaitForElement("LocationCheckBox_2");
		App.Tap("LocationCheckBox_2"); // Select Berlin, DE
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Remove Location");
		AssertText("StatusMessageLabel", "Removed location: Berlin, DE");
		App.WaitForNoElement("LocationName_2");
		AssertText("LocationName_0", "Redmond, USA");
		AssertText("LocationName_1", "London, UK");
	}

	[Test, Order(9)]
	public void MenuBarItem_ViewMenuRefreshCommand()
	{
		ResetLocations();
		TapMenuItem("ViewMenuBar");
		TapMenuItem("RefreshMenuBarFlyoutItem");
		AssertText("StatusMessageLabel", "Refreshed");
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
	public void MenuBarItem_VerifyAllMenusAndItemsAccessible()
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
		ResetLocations();
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Change Location");
		App.WaitForElement(MenuItemQuery("Redmond, USA"));
		App.WaitForElement(MenuItemQuery("London, UK"));
		App.WaitForElement(MenuItemQuery("Berlin, DE"));
		TapMenuItem("London, UK");
		AssertText("CurrentLocationLabel", "London, UK");
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
		ResetLocations();
		AddLocation("Tokyo, JP");
		AssertText("LocationName_3", "Tokyo, JP");
		AddLocation("Paris, FR");
		AssertText("LocationName_3", "Tokyo, JP");
		AssertText("LocationName_4", "Paris, FR");
	}

	[Test, Order(17)]
	public void MenuBarItem_CancelAddLocation()
	{
		ResetLocations();
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Add Location");
		App.WaitForElement("LocationEntry");
		App.EnterText("LocationEntry", "Cancelled Location");
		App.Tap("CancelButton");
		AssertText("StatusMessageLabel", "Operation cancelled");
		App.WaitForNoElement("LocationEntry");
		App.WaitForNoElement("LocationName_3");
	}

	[Test, Order(18)]
	public void MenuBarItem_ResetRestoresDefaultLocations()
	{
		ResetLocations();
		AddLocation("Custom Location");
		AssertText("LocationName_3", "Custom Location");
		ResetLocations();
		AssertText("LocationName_0", "Redmond, USA");
		AssertText("LocationName_1", "London, UK");
		AssertText("LocationName_2", "Berlin, DE");
		App.WaitForNoElement("LocationName_3");
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
		ResetLocations();
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Add Location");
		App.WaitForElement("LocationEntry");
		App.WaitForElement("ConfirmButton");
		App.WaitForElement("CancelButton");
		App.Tap("CancelButton");
		AssertText("StatusMessageLabel", "Operation cancelled");
		App.WaitForNoElement("LocationEntry");
		App.WaitForNoElement("ConfirmButton");
		App.WaitForNoElement("CancelButton");
	}

	[Test, Order(22)]
	public void MenuBarItem_AddEmptyLocationValidation()
	{
		ResetLocations();
		TapMenuItem("LocationsMenuBar");
		TapMenuItem("Add Location");
		App.WaitForElement("LocationEntry");
		App.ClearText("LocationEntry");
		App.Tap("ConfirmButton");
		AssertText("StatusMessageLabel", "Location name cannot be empty");
		App.WaitForElement("LocationEntry");
		App.WaitForNoElement("LocationName_3");
	}
}
#endif
