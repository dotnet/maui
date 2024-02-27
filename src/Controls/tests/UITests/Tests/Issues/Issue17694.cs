using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17694 : _IssuesUITest
	{
		public Issue17694(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Circle view not rotating from center";

		[Test]
		public void Issue17694Test()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS,
				TestDevice.Mac,
				TestDevice.Android
			});

			App.WaitForElement("Spin");

			// 1. Click button
			App.Click("Spin");

			VerifyScreenshot();
		}
	}
}
