using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17789 : _IssuesUITest
	{
		public Issue17789(TestDevice device) : base(device)
		{
		}

		public override string Issue => "ContentPage BackgroundImageSource not working";

		[Test]
		public void ContentPageBackgroundImageSourceWorks()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
					"The bug only happens on iOS; see https://github.com/dotnet/maui/pull/17789");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
