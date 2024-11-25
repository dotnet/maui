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

	// #if ANDROID
	// 	[Test]
	// 	public void Bugzilla45027Test()
	// 	{
	// 		var firstItemList = "0";
	// 		App.WaitForElement(firstItemList);

	// 		App.TouchAndHold(firstItemList);
	// 		App.WaitForElement(BUTTON_ACTION_TEXT);
	// 		App.DoubleTap(BUTTON_ACTION_TEXT);

	// 		App.TouchAndHold(firstItemList);
	// 		App.WaitForElement(BUTTON_DELETE_TEXT);
	// 		App.DoubleTap(BUTTON_DELETE_TEXT);
	// 	}
	// #endif

	// #if IOS
	// 	[Test]
	// 	[FailsOnIOSWhenRunningOnXamarinUITest]
	// 	public void Bugzilla45027Test()
	// 	{
	// 		var firstItemList = "0";
	// 		App.WaitForElement(firstItemList);

	// 		App.SwipeRightToLeft(firstItemList);
	// 		App.WaitForElement(BUTTON_ACTION_TEXT);
	// 		App.DoubleTap(BUTTON_ACTION_TEXT);

	// 		App.SwipeRightToLeft(firstItemList);
	// 		App.WaitForElement(BUTTON_DELETE_TEXT);
	// 		App.DoubleTap(BUTTON_DELETE_TEXT);
	// 	}
	// #endif
}