using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58779 : _IssuesUITest
{
	const string ButtonId = "button";
	const string CancelId = "cancel";

	public Bugzilla58779(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[MacOS] DisplayActionSheet on MacOS needs scroll bars if list is long";

	[Test]
	[FailsOnIOS]
	[Category(UITestCategories.DisplayAlert)]
	public void Bugzilla58779Test()
	{
		RunningApp.WaitForElement(ButtonId);
		RunningApp.Tap(ButtonId);
		RunningApp.Screenshot("Check list fits on screen");
		RunningApp.WaitForElement(CancelId);
		RunningApp.Tap(CancelId);
	}
}