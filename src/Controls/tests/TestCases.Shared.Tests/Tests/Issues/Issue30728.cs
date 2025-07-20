using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30728 : _IssuesUITest
{
	public Issue30728(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Image randomly disappears while switching tabs due to a race condition";

	[Test]
	[Category(UITestCategories.Image)]
	[Category(UITestCategories.Navigation)]
	public void ImagesShouldNotDisappearWhenSwitchingTabsRapidly()
	{
		// This test validates that images don't disappear when switching tabs rapidly
		// The original issue was caused by a race condition where the IsLoading check
		// prevented legitimate image reloads when Glide cleared the drawable

		App.WaitForElement("Tab1Button");

		// Load images in Tab 1
		App.Tap("Tab1Button");
		App.WaitForElement("TabTitle");
		
		// Verify images are loaded in Tab 1
		for (int i = 0; i < 6; i++)
		{
			App.WaitForElement($"TestImage{i}");
		}

		// Rapidly switch between tabs to trigger the race condition
		for (int cycle = 0; cycle < 5; cycle++)
		{
			App.Tap("Tab2Button");
			App.WaitForElement("TabTitle");
			
			App.Tap("Tab3Button");
			App.WaitForElement("TabTitle");
			
			App.Tap("Tab1Button");
			App.WaitForElement("TabTitle");
		}

		// After rapid switching, verify that images are still visible
		// The fix should prevent the race condition that caused images to disappear
		for (int i = 0; i < 6; i++)
		{
			App.WaitForElement($"TestImage{i}");
		}

		// Switch to Tab 2 and verify images load properly
		App.Tap("Tab2Button");
		App.WaitForElement("TabTitle");
		
		for (int i = 0; i < 6; i++)
		{
			App.WaitForElement($"TestImage{i}");
		}

		// Switch to Tab 3 and verify images load properly
		App.Tap("Tab3Button");
		App.WaitForElement("TabTitle");
		
		for (int i = 0; i < 6; i++)
		{
			App.WaitForElement($"TestImage{i}");
		}
	}
}