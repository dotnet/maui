#if TEST_FAILS_ON_CATALYST // Tap Coordinates not working on Catalyst
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
		TapCoordinates();
		Assert.That(App.FindElement("Button").GetText(), Is.EqualTo("True"));
	}
	void TapCoordinates()
	{
#if WINDOWS
        App.TapCoordinates(1340,160);
#elif ANDROID
		App.TapCoordinates(1000, 100);
#elif IOS
        App.TapCoordinates(1002,100);
#endif
	}
}
#endif


