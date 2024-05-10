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
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.iOS, TestDevice.Android }, "iOS will be fixed in https://github.com/dotnet/maui/pull/20953");

			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}