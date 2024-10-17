using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58779 : _IssuesUITest
{
	public Bugzilla58779(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[MacOS] DisplayActionSheet on MacOS needs scroll bars if list is long";

	// [Test]
	// [FailsOnIOS]
	// public void Bugzilla58779Test()
	// {
	// 	RunningApp.WaitForElement(q => q.Marked(ButtonId));
	// 	RunningApp.Tap(q => q.Marked(ButtonId));
	// 	RunningApp.Screenshot("Check list fits on screen");
	// 	RunningApp.WaitForElement(q => q.Marked(CancelId));
	// 	RunningApp.Tap(q => q.Marked(CancelId));
	// }
}