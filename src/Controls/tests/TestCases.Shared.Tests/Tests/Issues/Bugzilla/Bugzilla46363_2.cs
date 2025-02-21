using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla46363_2 : _IssuesUITest
{
	public Bugzilla46363_2(TestDevice testDevice) : base(testDevice)
	{
	}
	const string Target = "Two";
	const string ContextAction = "Context Action";
	const string TapSuccess = "Tap Success";
	const string ContextSuccess = "Context Menu Success";

	public override string Issue => "TapGestureRecognizer blocks List View Context Actions1";

	[Test]
	[Category(UITestCategories.ListView)]
	public void _46363_2_Tap_Succeeds()
	{
		App.WaitForElement("TestingLabel");
		App.WaitForElement(Target);
		App.Tap(Target);
		App.WaitForElement(TapSuccess);
		// Verify that we aren't also opening the context menu
		App.WaitForNoElement(ContextAction);
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST//ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
	//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
	[Test]
	[Category(UITestCategories.ListView)]
	public void _46363_2_ContextAction_Succeeds()
	{
		App.WaitForElement("TestingLabel");
		App.ActivateContextMenu(Target);
		App.WaitForElement(ContextAction);
		App.Tap(ContextAction);
		App.WaitForElement(ContextSuccess);
	}
#endif
}