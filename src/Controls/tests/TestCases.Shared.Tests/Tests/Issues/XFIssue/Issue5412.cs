using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5412 : _IssuesUITest
{
	public Issue5412(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "5412 - (NavigationBar disappears on FlyoutPage)";

	// TODO: Check corresponding AppHost UI page, that needs updating. Things are commented out there.
	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Issue5412Test() 
	{
	    App.WaitForElement("Index Page");
#if ANDROID || WINDOWS
		App.TapFlyoutPageIcon();
#else
		App.Tap("Menu title");
#endif
		App.WaitForElement("Settings");
		App.Tap("Settings");
		App.WaitForElement("Settings Page");
#if ANDROID || WINDOWS || MACCATALYST
		App.TapBackArrow();
#elif IOS
		App.Back();
#endif
		// This fails if the menu isn't displayed (original error behavior)
		App.WaitForElement("Index Page");
	}
}