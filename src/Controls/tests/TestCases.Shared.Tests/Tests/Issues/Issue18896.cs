#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS//Related issues : https://github.com/dotnet/maui/issues/18811, https://github.com/dotnet/maui/issues/15994
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
		[FailsOnIOSWhenRunningOnXamarinUITest("Currently fails on iOS; see https://github.com/dotnet/maui/issues/18811")]
		[FailsOnMacWhenRunningOnXamarinUITest("Currently fails on Catalyst; see https://github.com/dotnet/maui/issues/18811")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Currently fails on Windows; see https://github.com/dotnet/maui/issues/15994")]
		public void Issue18896Test()
		{
			VerifyInternetConnectivity();
			App.WaitForElement("WaitForStubControl");

			App.ScrollDown(ListView);

			App.ScrollUp(ListView);

			// Load images and hide scrollbar.
			Thread.Sleep(2000);

			// The test passes if you are able to see the image, name, and location of each monkey.
			VerifyScreenshot(retryDelay: TimeSpan.FromSeconds(2));
		}
	}
}
#endif