using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue15330 : _IssuesUITest
	{
		public Issue15330(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Grid wrong Row height";

		[Test]
		public void Issue15330Test()
		{
			// This issue is not working on net7 for the following platforms 
			// This is not a regression it's just the test being backported from net8
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Android, TestDevice.Windows, TestDevice.iOS, TestDevice.Mac
			}, BackportedTestMessage);

			UITestContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/17125");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
