#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla59718 : _IssuesUITest
{
	const string GoBackButtonId = "GoBackButtonId";
	const string Target1 = "Label with TapGesture Cricket";
	const string Target1b = "Label with TapGesture Cricket Tapped!";
	const string Target2 = "Label with no TapGesture Cricket";
	const string Target3 = "You came here from Cricket.";

	public Bugzilla59718(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Multiple issues with listview and navigation in UWP";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	public void Bugzilla59718Test()
	{
		App.WaitForElement(Target1);
		App.Tap(Target1);

		App.WaitForElement(Target1b);

		App.WaitForElement(Target2);
		App.Tap(Target2);

		App.WaitForElement(Target3);

		App.WaitForElement(GoBackButtonId);
		App.Tap(GoBackButtonId);

		App.WaitForElement(Target1);
	}
}
#endif
