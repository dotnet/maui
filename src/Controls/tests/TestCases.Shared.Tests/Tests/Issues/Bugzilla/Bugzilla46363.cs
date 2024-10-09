using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla46363 : _IssuesUITest
{
	public Bugzilla46363(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer blocks List View Context Actions";

	// [FailsOnAndroid]
	// [FailsOnIOS]
	// [Test]
	// public void _46363_Tap_Succeeds()
	// {
	// 	RunningApp.WaitForElement(Testing);
	// 	RunningApp.Tap(Target);
	// 	RunningApp.WaitForElement(TapSuccess);

	// 	// First run at fixing this caused the context menu to open on a regular tap
	// 	// So this check is to ensure that doesn't happen again
	// 	RunningApp.WaitForNoElement(ContextAction);
	// }

	// [FailsOnAndroid]
	// [FailsOnIOS]
	// [Test]
	// public void _46363_ContextAction_Succeeds()
	// {
	// 	RunningApp.WaitForElement(Testing);
	// 	RunningApp.ActivateContextMenu(Target);
	// 	RunningApp.WaitForElement(ContextAction);
	// 	RunningApp.Tap(ContextAction);
	// 	RunningApp.WaitForElement(ContextSuccess);
	// }
}