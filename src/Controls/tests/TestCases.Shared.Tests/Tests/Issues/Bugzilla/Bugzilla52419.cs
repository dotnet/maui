using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla52419 : _IssuesUITest
{
#if ANDROID
	const string TabPage1 = "TAB PAGE 1";
	const string TabPage2 = "TAB PAGE 2";
	const string BackButtonIdentifier = "";
#else
	const string TabPage1 = "Tab Page 1";
	const string TabPage2 = "Tab Page 2";
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
		App.WaitForElementTillPageNavigationSettled(TabPage2);
	 	App.Tap(TabPage2);
	 	App.Tap(TabPage1);
	 	App.Tap(TabPage2);
	 	App.Tap(TabPage1);
		App.WaitForElement(PushNewPage);
	 	App.TapBackArrow(BackButtonIdentifier);
	 	App.WaitForElement(AppearanceLabel);
	 	Assert.That(App.WaitForElement(AppearanceLabel).GetText(), Is.EqualTo("Times Appeared: 2"));
	 }
}
