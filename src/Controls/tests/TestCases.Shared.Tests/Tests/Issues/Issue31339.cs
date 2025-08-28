using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31339 : _IssuesUITest
{
	public Issue31339(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "[iOS] CarouselViewHandler2 - NSInternalInconsistencyException thrown when setting ItemsSources";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselViewHandler2OutOfBoundsTest()
	{
		App.WaitForElement("UpdateButton");

		// Tap the button to trigger the issue reproduction scenario
		// This sets Position = 15 and then sets ItemsSource to a collection with 5-20 items
		App.Tap("UpdateButton");

		// Wait for the CarouselView to be available, if no crash occurred, this should succeed
		App.WaitForElement("TestCarouselView");

		// Verify we can interact with the CarouselView after the update
		// The CarouselView should be visible and functional, not crashed
		var carouselView = App.FindElement("TestCarouselView");
		Assert.IsNotNull(carouselView, "CarouselView should be available after ItemsSource update");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselViewHandler2OutOfBoundsMultipleUpdates()
	{
		// Test multiple rapid updates to ensure stability
		App.WaitForElement("UpdateButton");

		// Perform multiple updates to stress test the bounds validation
		for (int i = 0; i < 3; i++)
		{
			App.Tap("UpdateButton");
			App.WaitForElement("TestCarouselView");
		}

		// Verify the CarouselView is still functional after multiple updates
		var carouselView = App.FindElement("TestCarouselView");
		Assert.IsNotNull(carouselView, "CarouselView should remain available after multiple updates");
	}
}