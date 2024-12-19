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
		//Unable to access the switch element directly when it placed inside the TableView, so using TapCoordinates to tap on the switch
#if WINDOWS
        App.TapCoordinates(1340,160);
#elif ANDROID
		App.TapCoordinates(1000, 100);
#elif IOS
        App.Tap(AppiumQuery.ByXPath("//XCUIElementTypeSwitch[@name='Toggle switch; nothing should crash']"));
#elif MACCATALYST
		App.ClickCoordinates(774,140);
#endif
	}
}