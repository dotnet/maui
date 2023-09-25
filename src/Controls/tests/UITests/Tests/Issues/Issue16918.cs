using Microsoft.Maui.Appium;
using Microsoft.Maui.AppiumTests;
using NUnit.Framework;

namespace Controls.AppiumTests.Tests.Issues
{
	public class Issue16918 : _IssuesUITest
	{
		public Issue16918(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Image buttons aliasing";

		[Test]
		public void Issue16918Test()
		{
			// https://github.com/dotnet/maui/issues/16918
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac,
				TestDevice.Android,
				TestDevice.iOS
			});

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
