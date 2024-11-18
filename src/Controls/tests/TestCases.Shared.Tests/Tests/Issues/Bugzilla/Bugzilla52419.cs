#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
//TapBackArrow doesn't work on iOS and Mac. On the Android platform, tapping on Page 2 doesn't work.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla52419 : _IssuesUITest
{
	public Bugzilla52419(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[A] OnAppearing called for previous pages in a tab's navigation when switching active tabs";

	 [Test]
	 [Category(UITestCategories.TabbedPage)]
	 public void Bugzilla52419Test()
	 {
	 	App.WaitForElement("Push new page");
	 	App.Tap("Push new page");
	 	App.WaitForElement("Push new page");
	 	App.Tap("Push new page");
	 	App.WaitForElement("Push new page");
	 	App.Tap("Push new page");
	 	App.Tap("Tab Page 2");
	 	App.Tap("Tab Page 1");
	 	App.Tap("Tab Page 2");
	 	App.Tap("Tab Page 1");
	 	App.TapBackArrow();
	 	App.WaitForElement("AppearanceLabel");
	 	Assert.That(App.WaitForElement("AppearanceLabel").GetText(), Is.EqualTo("Times Appeared: 2"));
	 }
}
#endif