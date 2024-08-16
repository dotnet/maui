using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue10645 : _IssuesUITest
	{
		public Issue10645(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Image is not centered in AspectFill mode";

		[Test]
		[Category(UITestCategories.ActionSheet)]
		public void Issue10645Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.iOS }, "Only affects Windows.");

			App.WaitForElement("AspectFillImage", timeout: TimeSpan.FromSeconds(4));

			VerifyScreenshot();
		}
	}
}
