using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2597 : _IssuesUITest
{
	public Issue2597(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Stepper control .IsEnabled doesn't work";

	// [Test]
	// [Category(UITestCategories.Stepper)]
	// [FailsOnIOS]
// 	public void Issue2597Test()
// 	{
// #if __IOS__
// 		App.Tap(x => x.Marked("Increment"));
// #else
// 		App.Tap("+");
// #endif

// 		App.WaitForElement(q => q.Marked("Stepper value is 0"));


// #if __IOS__
// 		App.Tap(x => x.Marked("Decrement"));
// #else
// 		App.Tap("−");
// #endif

// 		App.WaitForElement(q => q.Marked("Stepper value is 0"));
// 	}
}