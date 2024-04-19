using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue21630 : _IssuesUITest
{
	public override string Issue => "Entries in NavBar don't trigger keyboard scroll";

	public Issue21630(TestDevice device)
		: base(device)
	{ }

    string NavBarEntry => "NavBarEntry";
    string HeaderEntry => "HeaderEntry";

    [Test]
	public void NavBarEntryDoesNotTriggerKeyboardScroll()
	{
        var navBarEntry = App.WaitForElement(NavBarEntry);
		var navBarLocation = navBarEntry.GetRect();
		var headerEntry = App.WaitForElement(HeaderEntry);
		var headerLocation = headerEntry.GetRect();

        App.Click(NavBarEntry);

        var newNavBarEntry = App.WaitForElement(NavBarEntry);
		var newNavBarEntryLocation = newNavBarEntry.GetRect();
        Assert.AreEqual(navBarLocation, newNavBarEntryLocation);

        var newHeaderEntry = App.WaitForElement(HeaderEntry);
		var newHeaderLocation = newHeaderEntry.GetRect();

        Assert.AreEqual(headerLocation, newHeaderLocation);
	}
}
