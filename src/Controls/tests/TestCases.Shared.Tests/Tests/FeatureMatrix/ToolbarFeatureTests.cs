using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ToolbarFeatureTests : UITest
{
	public const string ToolbarFeatureMatrix = "Toolbar Feature Matrix";

	public ToolbarFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ToolbarFeatureMatrix);
	}


	[Test, Order(1)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifyPrimaryToolBar_DisabledState()
	{
		App.WaitForElement("IsEnabledButton1");
		App.Tap("IsEnabledButton1");
		App.WaitForElement("Test (1)");
		App.Tap("Test (1)");
		var MenuLabel = App.FindElement("MenuLabel").GetText();
		Assert.That(MenuLabel, Is.Not.EqualTo("You clicked on ToolbarItem: Test (1)"), "When the primary toolbar in Disabled state, clicking it should not change the status label text.");
	}

	[Test, Order(2)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifyPrimaryToolBar_EnabledState()
	{
		App.WaitForElement("IsEnabledButton1");
		App.Tap("IsEnabledButton1");
		App.WaitForElement("Test (1)");
		App.Tap("Test (1)");
		var MenuLabel = App.FindElement("MenuLabel").GetText();
		Assert.That(MenuLabel, Is.EqualTo("You clicked on ToolbarItem: Test (1)"), "Primary toolbar item click should be reflected in status label");
	}

	[Test, Order(3)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_EnabledState()
	{
		App.WaitForElement("IsEnabledButton4");
		App.Tap("IsEnabledButton4");
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (4)");
		App.Tap("Test Secondary (4)");
		var MenuLabel = App.FindElement("MenuLabel").GetText();
		Assert.That(MenuLabel, Is.EqualTo("You clicked on ToolbarItem: Test Secondary (4)"), "When the secondary toolbar in Enabled state, clicking it should change the status label text.");
	}

	[Test, Order(4)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_Command()
	{
		App.WaitForElement("ChangeCommandButton3");
		App.Tap("ChangeCommandButton3");
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (3)");
		App.Tap("Test Secondary (3)");  // This taps the secondary toolbar item again to verify the command change
		var MenuLabel = App.FindElement("MenuLabel").GetText();
		Assert.That(MenuLabel, Is.EqualTo("You clicked on ToolbarItem: Test Secondary (3) with changed Command"));
	}

	[Test, Order(5)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_RemoveSecondaryToolBar()
	{
		App.WaitForElement("RemoveAddButton3");
		App.Tap("RemoveAddButton3");
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForNoElement("Test Secondary (3)");
		App.Tap("Test Secondary (1)"); // To close the toolbar item menu
	}

	[Test, Order(6)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_AddSecondaryToolBar()
	{
		App.WaitForElement("RemoveAddButton3");
		App.Tap("RemoveAddButton3");
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (3)"); // To verify that the secondary toolbar item is added back
		App.Tap("Test Secondary (3)");
	}

	[Test, Order(7)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_ChangeText()
	{
		App.WaitForElement("ChangeTextButton1");
		App.Tap("ChangeTextButton1");
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Changed Text");
		App.Tap("Changed Text");
	}

	[Test, Order(8)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_ChangeTextBack()
	{
		App.WaitForElement("ChangeTextButton1");
		App.Tap("ChangeTextButton1");
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (1)");
		CloseSecondaryToolbarMenu();
	}

	[Test, Order(9)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_EnabledDisabledWithTime()
	{
		App.WaitForElement("IsEnabledButton2");
		App.Tap("IsEnabledButton2");
		// Wait for 1 seconds to ensure the button is disabled after the delay
		Thread.Sleep(1000);
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (2)");
		App.Tap("Test Secondary (2)");
		App.WaitForElement("Test Secondary (2)");// The toolbar item is in a disabled state.
		CloseSecondaryToolbarMenu();
	}

	[Test, Order(10)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifyToolBar_MultiplePrimaryItems()
	{
		// Test both primary toolbar items
		App.WaitForElement("Test (1)");
		App.Tap("Test (1)");
		var menuLabel1 = App.FindElement("MenuLabel").GetText();
		Assert.That(menuLabel1, Is.EqualTo("You clicked on ToolbarItem: Test (1)"));

		App.WaitForElement("Test (2)");
		App.Tap("Test (2)");
		var menuLabel2 = App.FindElement("MenuLabel").GetText();
		Assert.That(menuLabel2, Is.EqualTo("You clicked on ToolbarItem: Test (2)"));
	}

#if TEST_FAILS_ON_WINDOWS // When running on Windows, the test fails because tapping the Secondary Toolbar a second time results in a QA.Selenium.ElementNotFoundException.
	[Test, Order(11)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifyToolBar_PrimaryAndSecondaryInteraction()
	{
		// Click primary first
		App.WaitForElement("Test (1)");
		App.Tap("Test (1)");

		// Then click secondary
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (1)");
		App.Tap("Test Secondary (1)");

		var menuLabel = App.FindElement("MenuLabel").GetText();
		Assert.That(menuLabel, Is.EqualTo("You clicked on ToolbarItem: Test Secondary (1)"));
	}

	[Test, Order(12)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifyToolBar_MultipleSecondaryInteractions()
	{
		App.WaitForMoreButton();
		App.TapMoreButton();
		// Click on the secondary toolbar item
		App.WaitForElement("Test Secondary (1)");
		App.Tap("Test Secondary (1)");
		var menuLabel = App.FindElement("MenuLabel").GetText();
		Assert.That(menuLabel, Is.EqualTo("You clicked on ToolbarItem: Test Secondary (1)"));
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (4)");
		App.Tap("Test Secondary (4)");
		var menuLabel2 = App.FindElement("MenuLabel").GetText();
		Assert.That(menuLabel2, Is.EqualTo("You clicked on ToolbarItem: Test Secondary (4)"));
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS// Issue Link: https://github.com/dotnet/maui/issues/30675
	[Test, Order(13)]
	[Category(UITestCategories.ToolbarItem)]
	public void VerifySecondaryToolBar_IconAppearance()
	{
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.WaitForElement("Test Secondary (2)");
		VerifyScreenshot();
	}
#endif

	public void CloseSecondaryToolbarMenu() // This method is used to close the secondary toolbar menu
	{
#if WINDOWS
		App.Tap("MenuLabel");  
#else
		App.Tap("Test Secondary (1)");
#endif
	}
}