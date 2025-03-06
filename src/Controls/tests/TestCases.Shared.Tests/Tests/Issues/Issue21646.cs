using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21646: _IssuesUITest
{

#if IOS || MACCATALYST
		protected const string FlyoutIconId = "Menu";
#else
		protected const string FlyoutIconId = "OK";
#endif
	public Issue21646(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout icon should remain visible when a page is pushed onto a NavigationPage with the back button disabled.";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void FlyoutIconShouldBeVisibleWithBackButtonDisabledInNavigationPage()
	{
		App.WaitForElement("NavigateToNextPageButton");
		App.Tap("NavigateToNextPageButton");
		App.WaitForElement("SecondPageLabel");
		App.WaitForFlyoutIcon(FlyoutIconId, false);
	}
}