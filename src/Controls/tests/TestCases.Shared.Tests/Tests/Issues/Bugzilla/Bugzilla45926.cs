#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
//TapBackArrow doesn't work on iOS and Mac.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla45926 : _IssuesUITest
{
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
		App.WaitForElement("Second Page #1");
		App.TapBackArrow();
		App.WaitForElement("Intermediate Page");
		App.TapBackArrow();
		App.Tap("Do GC");
		App.Tap("Do GC");
		App.Tap("Send Message");
		App.Tap("Do GC");

		App.WaitForElement("Instances: 0");
		App.WaitForElement("Messages: 0");
	}
}
#endif