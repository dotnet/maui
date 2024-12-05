using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2272 : _IssuesUITest
{
	public Issue2272(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting a different Detail page from a FlyoutPage after 2nd time on MainPage";

	// [Test]
	// [Category(UITestCategories.Navigation)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// #if MACCATALYST // Check if this is still try for MAUI?
	// 	[Ignore("EnterText problems in UITest Desktop")]
	// #endif
	// 	public void TestFocusIsOnTheEndAfterSettingText ()
	// 	{
	// 		App.WaitForElement("userNameEditorEmptyString");
	// 		App.Tap (c => c.Marked ("userNameEditorEmptyString"));
	// 		App.EnterText ("1");
	// 		PressEnter ();
	// 		var q = App.Query(c => c.Marked("userNameEditorEmptyString"));
	// 		Assert.AreEqual("focused1", q[0].Text);
	// 	}

	// 	void PressEnter ()
	// 	{
	// 		var androidApp = RunningApp as AndroidApp;
	// 		if (androidApp != null) {
	// 			androidApp.PressUserAction (UserAction.Done);
	// 		}
	// 		else {
	// 			App.PressEnter ();
	// 		}
	// 	}
}
