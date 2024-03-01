using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18896 : _IssuesUITest
	{
		const string ListView = "TestListView";

		public Issue18896(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Can scroll ListView inside RefreshView";

		[Test]
		[Category(UITestCategories.ListView)]
		public async Task Issue18896Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/18811");

			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Windows },
				"Currently fails on Windows; see https://github.com/dotnet/maui/issues/15994");

			App.WaitForElement("WaitForStubControl");

			App.ScrollDown(ListView);

			App.ScrollUp(ListView);

			// Load images and hide scrollbar.
			await Task.Delay(2000);

			// The test passes if you are able to see the image, name, and location of each monkey.
			VerifyScreenshot();
		}
	}
}