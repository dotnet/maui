using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18896 : _IssuesUITest
	{
		public Issue18896(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Can scroll ListView inside RefreshView";

		[Test]
		public async Task Issue18896Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Windows },	
				"Currently fails on Windows; see https://github.com/dotnet/maui/issues/15994");

			App.WaitForElement("WaitForStubControl");

			// Load images
			await Task.Delay(1000);

			// The test passes if you are able to see the image, name, and location of each monkey.
			VerifyScreenshot();
		}
	}
}