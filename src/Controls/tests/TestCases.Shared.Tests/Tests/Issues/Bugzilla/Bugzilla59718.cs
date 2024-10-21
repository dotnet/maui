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
	[FailsOnIOS]
	public void Bugzilla59718Test()
	{
		RunningApp.WaitForElement(Target1);
		RunningApp.Tap(Target1);

		RunningApp.WaitForElement(Target1b);

		RunningApp.WaitForElement(Target2);
		RunningApp.Tap(Target2);

		RunningApp.WaitForElement(Target3);

		RunningApp.WaitForElement(GoBackButtonId);
		RunningApp.Tap(GoBackButtonId);

		RunningApp.WaitForElement(Target1);
	}
}