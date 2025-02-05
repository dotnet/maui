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
	// 		App.WaitForElement(q => q.Marked("Item #1"));
	// 		App.Tap(q => q.Marked("Item #1"));
	// 		App.WaitForElement(q => q.Marked(ItemSelectedSuccess));

	// 		// Item #2 should have a tap gesture
	// 		App.WaitForElement(q => q.Marked("Item #2"));
	// 		App.Tap(q => q.Marked("Item #2"));
	// 		App.WaitForElement(q => q.Marked(TapGestureSucess));

	// 		// Both items should allow access to the context menu
	// 		App.ActivateContextMenu("Item #2");
	// 		App.WaitForElement("2 Action");
	// #if __ANDROID__
	// 		App.Back();
	// #else
	// 		App.Tap(q => q.Marked("Item #3"));


	// 		App.ActivateContextMenu("Item #1");
	// 		App.WaitForElement("1 Action");
	// #if __ANDROID__
	// 		App.Back();
	// #else
	// 		App.Tap(q => q.Marked("Item #3"));
	// 	}
}