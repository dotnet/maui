#if TEST_FAILS_ON_WINDOWS
// Windows: The snapshot does not capture the FlyoutIcon applied to the TitleBar.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueShell6738 : _IssuesUITest
	{
		public override string Issue => "The color of the custom icon in Shell always resets to the default blue";

		public IssueShell6738(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test, Order(1)]
		[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
		public void EnsureCustomFlyoutIconColor()
		{
			App.WaitForElement("IconColorChangeButton");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
		public void EnsureFlyoutIconWithForegroundColor()
		{
			Exception? exception = null;

			string changeColor = "IconColorChangeButton";
			string defaultColor = "IconColorDefaultButton";
			App.WaitForElement(changeColor);
			App.Tap(changeColor);
			App.WaitForElement(changeColor);
			VerifyScreenshotOrSetException(ref exception);
			App.WaitForElement(defaultColor);
			App.Tap(defaultColor);
			App.WaitForElement(defaultColor);
			VerifyScreenshotOrSetException(ref exception, "EnsureFlyoutIconAsDefaultIconColor");

			if (exception is not null)
			{
				throw exception;
			}
		}
	}
}
#endif