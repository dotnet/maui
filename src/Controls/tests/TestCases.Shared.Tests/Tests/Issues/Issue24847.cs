using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24847 : _IssuesUITest
	{
		public Issue24847(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Background layer frame mapping has poor performance";

		[Test]
		[Category(UITestCategories.Brush)]
		public void BackgroundFrameResizesFastAndCorrectly()
		{
			Exception? exception = null;

			App.WaitForElement("ChangeSizeBtn");
			VerifyScreenshotOrSetException(ref exception, "BackgroundFrameResizesFastAndCorrectly");

			App.Tap("ChangeSizeBtn");
			VerifyScreenshotOrSetException(ref exception, "BackgroundFrameResizesFastAndCorrectlySizeChanged");

			if (exception is not null)
			{
				throw exception;
			}
		}
	}
}