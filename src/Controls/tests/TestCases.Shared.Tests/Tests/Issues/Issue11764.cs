using System;
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
		public void ScrollViewInFlyoutHeaderShouldScroll()
		{
			const string scrollResultLabel = "ScrollResultLabel";
			const string expectedText = "The ScrollView is scrolls vertically";

			App.WaitForElement("OpenFlyoutButton");
			App.Tap("OpenFlyoutButton");

			// Wait for flyout content to be ready before attempting gesture input.
			App.WaitForElement("FlyoutScrollView", timeout: TimeSpan.FromSeconds(10));
			App.WaitForElement("SampleButton", timeout: TimeSpan.FromSeconds(10));
			App.WaitForElement(scrollResultLabel, timeout: TimeSpan.FromSeconds(10));

			for (var attempt = 0; attempt < 4; attempt++)
			{
				App.ScrollDown("FlyoutScrollView", ScrollStrategy.Gesture, 0.90);

				var labelText = App.WaitForElement(scrollResultLabel).GetText();
				if (labelText == expectedText)
					return;
			}

			Assert.Fail($"Expected '{expectedText}' after scrolling flyout header ScrollView.");
		}
	}
}
