using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21394 : _IssuesUITest
	{
		public Issue21394(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Buttons with Images layouts";

        [Test]
		public void Issue21394Test()
		{
			// This test will also fail on iOS 14 and lower since the image padding is currently different.
			this.IgnoreIfPlatform(TestDevice.Windows, "Current issue with Windows placement/sizing");

			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
