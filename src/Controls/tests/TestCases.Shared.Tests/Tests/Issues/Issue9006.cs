#if TEST_FAILS_ON_IOS // On iOS test consistently crashes on CI, but passes locally. Adding failure for iOS to ensure CI stability.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9006 : _IssuesUITest
	{
#if ANDROID
		const string Back = "";
		const string Tab1 = "";
#else
		const string Back = "Back";
		const string Tab1 = "Tab 1";
#endif
		const string ClickMe = "ClickMe";
		const string FinalLabel = "FinalLabel";
		public Issue9006(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Unable to open a new Page for the second time in Xamarin.Forms Shell Tabbar";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void ClickingOnTabToPopToRootDoesntBreakNavigation()
		{
			App.WaitForElement(ClickMe);
			App.Tap(ClickMe);
			App.WaitForElementTillPageNavigationSettled(FinalLabel);
			App.TapBackArrow(Back);
			App.WaitForElement("This is the intermediate page");
			App.TapBackArrow(Tab1);
			App.WaitForElement(ClickMe);
			App.Tap(ClickMe);
			App.WaitForElementTillPageNavigationSettled(FinalLabel);
			Assert.That(App.WaitForElement(FinalLabel)?.GetText(), Is.EqualTo("Success"));
		}
	}
}
#endif