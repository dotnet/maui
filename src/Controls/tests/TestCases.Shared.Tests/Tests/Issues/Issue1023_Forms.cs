#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1023_Forms : _IssuesUITest
{
	public Issue1023_Forms(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Automate GC checks of picker disposals";

	// [Test]
	// [Category(UITestCategories.Picker)]
	// [FailsOnIOS]
	// public void Bugzilla1023Test()
	// {
	// 	for (var n = 0; n < 10; n++)
	// 	{
	// 		RunningApp.WaitForElement(q => q.Marked("Push"));
	// 		RunningApp.Tap(q => q.Marked("Push"));

	// 		RunningApp.WaitForElement(q => q.Marked("ListView"));
	// 		RunningApp.Back();
	// 	}

	// 	// At this point, the counter can be any value, but it's most likely not zero.
	// 	// Invoking GC once is enough to clean up all garbage data and set counter to zero
	// 	RunningApp.WaitForElement(q => q.Marked("GC"));
	// 	RunningApp.Tap(q => q.Marked("GC"));

	// 	RunningApp.WaitForElement(q => q.Marked("Counter: 0"));
	// }
}
#endif