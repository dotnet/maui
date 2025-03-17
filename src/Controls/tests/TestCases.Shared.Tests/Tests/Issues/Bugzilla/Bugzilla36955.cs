#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //Unable to access the switch element directly when it placed inside the TableView, Also Hardcoded TapCoordinates don't work reliably in CI environments for desktop platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla36955 : _IssuesUITest
{
	public Bugzilla36955(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] ViewCellRenderer.UpdateIsEnabled referencing null object";

	[Category(UITestCategories.TableView)]
	[Test]
	public void Bugzilla36955Test()
	{
		App.WaitForElement("Button");
		Assert.That(App.FindElement("Button").GetText(), Is.EqualTo("False"));

		ToggleSwitch();
		Assert.That(App.FindElement("Button").GetText(), Is.EqualTo("True"));
	}
	void ToggleSwitch()
	{
#if ANDROID
		App.TapCoordinates(1000, 100);
#elif IOS
        App.Tap(AppiumQuery.ByXPath("//XCUIElementTypeSwitch[@name='Toggle switch; nothing should crash']"));
#endif
	}
}
#endif