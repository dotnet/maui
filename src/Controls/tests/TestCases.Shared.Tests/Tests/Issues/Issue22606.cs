using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.Border)]
	public class Issue22606 : _IssuesUITest
	{
		public Issue22606(TestDevice device) : base(device) { }

		public override string Issue => "Border does not expand on Content size changed";

		[Test]
		public void BorderBackgroundExpandsOnContentSizeChanged()
		{
			Exception? exception = null;

			App.WaitForElement("SetHeightTo200");
			App.Tap("SetHeightTo200");
			VerifyScreenshotOrSetException(ref exception, "Issue22606_SetHeightTo200");

			App.Tap("SetHeightTo500");
			VerifyScreenshotOrSetException(ref exception, "Issue22606_SetHeightTo500");

			if (exception is not null)
			{
				throw exception;
			}
		}

#if ANDROID || IOS  //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
		[Test]
		public void BorderBackgroundSizeUpdatesWhenRotatingScreen()
		{
			App.WaitForElement("SetHeightTo200");
			App.Tap("SetHeightTo200");
			App.SetOrientationLandscape();
#if ANDROID
			VerifyScreenshot(cropLeft: 125);
#else
			VerifyScreenshot();
#endif
		}
#endif
	}
}