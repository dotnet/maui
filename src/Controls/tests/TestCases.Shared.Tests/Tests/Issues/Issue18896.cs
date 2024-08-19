using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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
		[FailsOnIOS("Currently fails on iOS; see https://github.com/dotnet/maui/issues/18811")]
		[FailsOnMac("Currently fails on Catalyst; see https://github.com/dotnet/maui/issues/18811")]
		[FailsOnWindows("Currently fails on Windows; see https://github.com/dotnet/maui/issues/15994")]
		public async Task Issue18896Test()
		{
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