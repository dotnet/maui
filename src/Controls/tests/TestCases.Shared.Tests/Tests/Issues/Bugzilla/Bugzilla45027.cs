#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.ListView)]
public class Bugzilla45027 : _IssuesUITest
{
	const string BUTTON_ACTION_TEXT = "Action";
	const string BUTTON_DELETE_TEXT = "Delete";
	public Bugzilla45027(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "App crashes when double tapping on ToolbarItem or MenuItem very quickly";


	[Test]
	public void Bugzilla45027Test()
	{
		var firstItemList = "0";
		var secondItemList = "1";
		App.WaitForElement(firstItemList);
		App.ActivateContextMenu(firstItemList);
		App.WaitForElement(BUTTON_ACTION_TEXT);
		App.DoubleTap(BUTTON_ACTION_TEXT);
		App.WaitForElement(secondItemList);
		App.Tap(secondItemList);
		App.ActivateContextMenu(firstItemList);
		App.WaitForElement(BUTTON_DELETE_TEXT);
		App.DoubleTap(BUTTON_DELETE_TEXT);
	}
}
#endif