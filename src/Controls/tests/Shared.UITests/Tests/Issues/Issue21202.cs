using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21202 : _IssuesUITest
	{
		public Issue21202(TestDevice device) : base(device)
		{
		}

		public override string Issue => "FontImageSource incorrectly sized";

		[Test]
		public void Issue21202Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.iOS }, "Only Windows for now");

			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
