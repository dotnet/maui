using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37892 : _IssuesUITest
{
	const int LoopDetectionThreshold = 50;

	public override string Issue => "ScrollView enters an infinite measure loop near the scrollability boundary";

	public Issue37892(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ScrollViewLayoutShouldSettle()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("The issue reproduction is not reliable on iOS 26 because UIKit no longer exposes the fluctuating adjusted content inset."); // Issue Link: https://github.com/dotnet/maui/issues/37892
		}

		App.WaitForElement("Issue37892LaunchButton", timeout: TimeSpan.FromSeconds(10));
		App.Tap("Issue37892LaunchButton");

		var timeout = DateTime.UtcNow.AddSeconds(15);
		while (DateTime.UtcNow < timeout)
		{
			if (App.FindElements("Layout Loop Detected").Count > 0)
			{
				App.TapDisplayAlertButton("OK");
				Assert.Fail(
					$"The ScrollView entered a layout loop and reached {LoopDetectionThreshold} root size changes.");
			}

			if (HasStatus("Issue37892LayoutLoopDetected"))
			{
				Assert.Fail(
					$"The ScrollView entered a layout loop and reached {LoopDetectionThreshold} root size changes.");
			}

			if (HasStatus("Layout Settled"))
			{
				App.TapDisplayAlertButton("OK");
				return;
			}

			Thread.Sleep(100);
		}

		Assert.Fail("The ScrollView layout did not settle within 15 seconds.");
	}

	bool HasStatus(string status) =>
		App.FindElements(status).Count > 0 ||
		App.FindElementsByText(status).Count > 0;
}
