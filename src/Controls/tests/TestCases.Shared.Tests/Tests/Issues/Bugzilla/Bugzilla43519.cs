using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla43519 : _IssuesUITest
	{
		const string Pop = "PopModal";
		const string Push = "PushModal";
		const string Page2 = "Page 2";


		public Bugzilla43519(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] FlyoutPage ArgumentException when nested in a TabbedPage and returning from modal page";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		[Category(UITestCategories.Compatibility)]
		public void TabbedModalNavigation()
		{
			App.TapTab(Page2);
			App.WaitForElement(Push);
			App.Tap(Push);
			App.WaitForElement(Pop);
			App.Tap(Pop);
			App.WaitForTabElement(Page2);
		}
	}
}