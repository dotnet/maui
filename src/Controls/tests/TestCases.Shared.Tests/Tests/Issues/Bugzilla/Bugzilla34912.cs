#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ListView IsEnabled property is not working on iOS and Catalyst, Issue: https://github.com/dotnet/maui/issues/19768.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla34912 : _IssuesUITest
{
	public Bugzilla34912(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView.IsEnabled has no effect on iOS";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla34912Test()
	{
		App.WaitForElement("Allen");
		App.Tap("Allen");
		App.WaitForElement("You tapped Allen");
		App.WaitForElement("OK");
		App.Tap("OK");
		App.WaitForElement("Disable ListView");
		App.Tap("Disable ListView");
		App.WaitForElement("Allen");
		App.Tap("Allen");
		App.WaitForNoElement("You tapped Allen");
	}
}
#endif