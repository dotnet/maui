using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11764 : _IssuesUITest
	{
		public Issue11764(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollView doesn't work in the Shell Flyout Header";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public async Task ScrollViewInFlyoutHeaderShouldScroll()
        {
            App.WaitForElement("OpenFlyoutButton");
            App.Tap("OpenFlyoutButton");
            App.WaitForElement("SampleButton");
			await Task.Delay(400); // Wait for the flyout to settle
			App.ScrollDown("FlyoutScrollView",ScrollStrategy.Gesture,0.90);
            App.WaitForElement("The ScrollView is scrolls vertically");
        }
	}
}
