using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58833 : _IssuesUITest
{
	public Bugzilla58833(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView SelectedItem Binding does not fire";

// 	[Test]
// 	[Category(UITestCategories.ListView)]
// 	[Ignore("Failing without explanation on XTC, please run manually")]
// 	public void Bugzilla58833Test()
// 	{
// 		// Item #1 should not have a tap gesture, so it should be selectable
// 		RunningApp.WaitForElement(q => q.Marked("Item #1"));
// 		RunningApp.Tap(q => q.Marked("Item #1"));
// 		RunningApp.WaitForElement(q => q.Marked(ItemSelectedSuccess));

// 		// Item #2 should have a tap gesture
// 		RunningApp.WaitForElement(q => q.Marked("Item #2"));
// 		RunningApp.Tap(q => q.Marked("Item #2"));
// 		RunningApp.WaitForElement(q => q.Marked(TapGestureSucess));

// 		// Both items should allow access to the context menu
// 		RunningApp.ActivateContextMenu("Item #2");
// 		RunningApp.WaitForElement("2 Action");
// #if __ANDROID__
// 		RunningApp.Back();
// #else
// 		RunningApp.Tap(q => q.Marked("Item #3"));


// 		RunningApp.ActivateContextMenu("Item #1");
// 		RunningApp.WaitForElement("1 Action");
// #if __ANDROID__
// 		RunningApp.Back();
// #else
// 		RunningApp.Tap(q => q.Marked("Item #3"));
// 	}
}