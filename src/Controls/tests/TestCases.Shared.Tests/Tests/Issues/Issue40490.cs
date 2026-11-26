#if ANDROID // <!-- This test case applies only to Android due to the use of android-specific configurations
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40490 : _IssuesUITest
{
	public Issue40490(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android back closes the app after switching tabs";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void BackAfterSwitchingTabsPopsTheSelectedNavigationStack()
	{
		App.WaitForElement("TabARootLabel");
		App.TapTab("Tab B");
		App.Tap("PushTabBDetailButton");
		App.WaitForElement("TabBDetailLabel");

		App.TapTab("Tab A");
		App.Tap("PushTabADetailButton");
		App.WaitForElement("TabADetailLabel");
		App.Back();
		App.WaitForElement("TabARootLabel");

		App.TapTab("Tab B");
		App.WaitForElement("TabBDetailLabel");
		App.Back();

		Assert.That(App.WaitForElement("TabBRootLabel").GetText(), Is.EqualTo("Tab B root"));
	}
}
#endif
