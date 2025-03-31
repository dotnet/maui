using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla46363 : _IssuesUITest
{
	public Bugzilla46363(TestDevice testDevice) : base(testDevice)
	{
	}
	const string Target = "Two";
	const string ContextAction = "Context Action";
	const string ContextSuccess = "Context Menu Success";
	const string TapSuccess = "Tap Success";
	public override string Issue => "TapGestureRecognizer blocks List View Context Actions";

	[Test]
	[Category(UITestCategories.ListView)]
	public void _46363_Tap_Succeeds()
	{
		App.WaitForElement("TestingLabel");
		App.Tap(Target);
		App.WaitForElement(TapSuccess);
		App.WaitForNoElement(ContextAction);
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
	//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
	[Test]
	[Category(UITestCategories.ListView)]
	public void _46363_ContextAction_Succeeds()
	{
		App.WaitForElement("TestingLabel");
		App.ActivateContextMenu(Target);
		App.WaitForElement(ContextAction);
		App.Tap(ContextAction);
		App.WaitForElement(ContextSuccess);
	}
#endif
}