#if WINDOWS || ANDROID
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

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla58833Test()
	{
		// Item #1 should not have a tap gesture, so it should be selectable
		App.WaitForElement("Item #1");
		App.Tap("Item #1");
		App.WaitForElement("ItemSelected Success");

		// Item #2 should have a tap gesture
		App.WaitForElement("Item #2");
		App.Tap("Item #2");
		App.WaitForElement("TapGesture Fired");

		// Both items should allow access to the context menu
		App.ContextActions("Item #2");
		App.WaitForElement("2 Action");
#if ANDROID
		App.TapBackArrow();
#else
		App.Tap("Item #3");
#endif

		App.ContextActions("Item #1");
		App.WaitForElement("1 Action");
#if ANDROID
		App.TapBackArrow();
#else
		App.Tap("Item #3");
#endif
	}
}
#endif