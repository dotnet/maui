using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3333 : _IssuesUITest
	{
		const string KSuccess = "If you're reading this the test has passed";

		public Issue3333(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] with ListView on page, Navigation.PopAsync() throws exception";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void SettingBindingContextToNullBeforingPoppingPageCrashes()
		{
			App.WaitForNoElement(KSuccess);
		}
	}
}