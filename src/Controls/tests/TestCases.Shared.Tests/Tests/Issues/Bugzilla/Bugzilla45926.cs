using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla45926 : _IssuesUITest
{
#if ANDROID
	const string BackButtonIdentifier1 = "";
	const string BackButtonIdentifier2 = "";
#else
	const string BackButtonIdentifier1 = "Back";
	const string BackButtonIdentifier2 = "Test";
#endif

	public Bugzilla45926(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "MessagingCenter prevents subscriber from being collected";

	[Test]
	[Category(UITestCategories.Page)]
	public void Issue45926Test()
	{
		App.WaitForElement("New Page");

		App.Tap("New Page");
		App.WaitForElementTillPageNavigationSettled("Second Page #1");

		//Getting nullreference exception on iOS while tapping back button. so using waitforElement for back button element.
#if IOS
		App.WaitForElement("Back");
#endif

		App.TapBackArrow(BackButtonIdentifier1);
		App.WaitForElementTillPageNavigationSettled("Intermediate Page");
		App.TapBackArrow(BackButtonIdentifier2);
		App.WaitForElement("Do GC");
		App.Tap("Do GC");
		App.Tap("Do GC");
		App.Tap("Send Message");
		App.Tap("Do GC");

		App.WaitForElement("Instances: 0");
		App.WaitForElement("Messages: 0");
	}
}