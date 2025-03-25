#if TEST_FAILS_ON_CATALYST // This test fails on catalyst, unable to click the back button as its does not have an identifier.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9794 : _IssuesUITest
	{

#if ANDROID
		const string BackButtonIdentifier = "";
#else
		const string BackButtonIdentifier = "tab1";
#endif
		const string tab1 = "tab1";
		const string tab2 = "tab2";
		const string GoForward = "GoForward";

		public Issue9794(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Tabbar Disappears with linker";

		[Test]
		[Category(UITestCategories.Shell)]
		public void EnsureTabBarStaysVisibleAfterPoppingPage()
		{
			App.WaitForElement(GoForward);
			App.Tap(GoForward);
			App.WaitForElement("Click Back Button");
			App.TapBackArrow(BackButtonIdentifier);
			App.WaitForElement(tab2);
			App.Tap(tab2);
			App.Tap(tab1);
			App.Tap(tab2);
			App.Tap(tab1);
			App.Tap(tab2);
			App.Tap(tab1);
		}
	}
}
#endif