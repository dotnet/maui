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

		public override string Issue => "ImageButton is not properly anti-aliased when scaled down";

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

			App.WaitForElement("MenuImage");
			VerifyScreenshot();
		}
	}
}
