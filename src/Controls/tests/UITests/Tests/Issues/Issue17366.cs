using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17366 : _IssuesUITest
	{
		public Issue17366(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Wrong gray color using transparent in iOS gradients";

		[Test]
		public void Issue17366Test()
		{
			UITestContext.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
					"The bug only happens on iOS; see https://github.com/dotnet/maui/pull/17789");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}