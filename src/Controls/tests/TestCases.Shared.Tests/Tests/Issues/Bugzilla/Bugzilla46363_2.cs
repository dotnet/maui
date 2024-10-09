using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla46363_2 : _IssuesUITest
{
	public Bugzilla46363_2(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer blocks List View Context Actions1";

	// [FailsOnAndroid]
	// [FailsOnIOS]
	// [Test]
	// public void _46363_2_Tap_Succeeds()
	// {
	// 	RunningApp.WaitForElement(Testing);
	// 	RunningApp.Tap(Target);
	// 	RunningApp.WaitForElement(TapSuccess);

	// 	// Verify that we aren't also opening the context menu
	// 	RunningApp.WaitForNoElement(ContextAction);
	// }

	// [FailsOnAndroid]
	// [FailsOnIOS]
	// [Test]
	// public void _46363_2_ContextAction_Succeeds()
	// {
	// 	RunningApp.WaitForElement(Testing);
	// 	RunningApp.ActivateContextMenu(Target);
	// 	RunningApp.WaitForElement(ContextAction);
	// 	RunningApp.Tap(ContextAction);
	// 	RunningApp.WaitForElement(ContextSuccess);
	// }
}