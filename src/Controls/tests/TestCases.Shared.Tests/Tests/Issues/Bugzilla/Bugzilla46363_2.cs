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

	// [FailsOnAndroidWhenRunningOnXamarinUITest]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// [Test]
	// public void _46363_2_Tap_Succeeds()
	// {
	// 	App.WaitForElement(Testing);
	// 	App.Tap(Target);
	// 	App.WaitForElement(TapSuccess);

	// 	// Verify that we aren't also opening the context menu
	// 	App.WaitForNoElement(ContextAction);
	// }

	// [FailsOnAndroidWhenRunningOnXamarinUITest]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// [Test]
	// public void _46363_2_ContextAction_Succeeds()
	// {
	// 	App.WaitForElement(Testing);
	// 	App.ActivateContextMenu(Target);
	// 	App.WaitForElement(ContextAction);
	// 	App.Tap(ContextAction);
	// 	App.WaitForElement(ContextSuccess);
	// }
}