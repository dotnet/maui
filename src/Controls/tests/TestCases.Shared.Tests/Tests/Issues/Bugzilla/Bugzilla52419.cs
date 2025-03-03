using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla52419 : _IssuesUITest
{
	const string TabPage1 = "Tab Page 1";
	const string TabPage2 = "Tab Page 2";
#if ANDROID
	const string BackButtonIdentifier = "";
#else
	const string BackButtonIdentifier = "Tab Page 1";
#endif

	const string PushNewPage = "PushNewPage";
	const string AppearanceLabel = "AppearanceLabel";

	public Bugzilla52419(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[A] OnAppearing called for previous pages in a tab's navigation when switching active tabs";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Bugzilla52419Test()
	{
		App.WaitForElementTillPageNavigationSettled(PushNewPage);
		App.Tap(PushNewPage);
		App.WaitForElementTillPageNavigationSettled(PushNewPage);
		App.Tap(PushNewPage);
		App.WaitForElementTillPageNavigationSettled(PushNewPage);
		App.Tap(PushNewPage);
		App.TapTab(TabPage2);
		App.TapTab(TabPage1);
		App.TapTab(TabPage2);
		App.TapTab(TabPage1);
		App.WaitForElement(PushNewPage);
		App.TapBackArrow(BackButtonIdentifier);
		App.WaitForElement(AppearanceLabel);
		Assert.That(App.WaitForElement(AppearanceLabel).GetText(), Is.EqualTo("Times Appeared: 2"));
	}
}
