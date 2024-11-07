#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1024 : _IssuesUITest
{
	public Issue1024(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Entry and Editor are leaking when used in ViewCell";

	// [Test]
	// [Category(UITestCategories.Performance)]
	// [FailsOnIOS]
	// public void Bugzilla1024Test()
	// {
	// 	for (var n = 0; n < 10; n++)
	// 	{
	// 		App.WaitForElement(q => q.Marked("Push"));
	// 		App.Tap(q => q.Marked("Push"));

	// 		App.WaitForElement(q => q.Marked("ListView"));
	// 		App.Back();
	// 	}

	// 	// At this point, the counter can be any value, but it's most likely not zero.
	// 	// Invoking GC once is enough to clean up all garbage data and set counter to zero
	// 	App.WaitForElement(q => q.Marked("GC"));
	// 	App.Tap(q => q.Marked("GC"));

	// 	App.WaitForElement(q => q.Marked("Counter: 0"));
	// }
}
#endif