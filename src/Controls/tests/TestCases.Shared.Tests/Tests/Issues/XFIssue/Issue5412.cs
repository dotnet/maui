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
	//[Test]
	//[Category(UITestCategories.FlyoutPage)]
	//public void Issue5412Test()
	//{
	//	var hamburgerText = "\uE700";
	//	var settings = "Settings";
	//	var back = "Back";

	//	App.WaitForElement(hamburgerText);
	//	App.Tap(hamburgerText);

	//	App.WaitForElement(settings);
	//	App.Tap(settings);

	//	App.WaitForElement(back);
	//	App.Tap(back);

	//	// This fails if the menu isn't displayed (original error behavior)
	//	App.WaitForElement(hamburgerText);
	//}
}