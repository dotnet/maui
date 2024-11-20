using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40173 : _IssuesUITest
{
	public Bugzilla40173(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android BoxView/Frame not clickthrough in ListView";

	// 	[FailsOnAndroidWhenRunningOnXamarinUITest]
	// 	[FailsOnIOSWhenRunningOnXamarinUITest]
	// 	[Test]
	// 	[Category(UITestCategories.InputTransparent)]
	// 	public void ButtonBlocked()
	// 	{
	// 		App.Tap("CantTouchButtonId");

	// 		Assert.That(App.FindElement("outputlabel").GetText()?
	// 			.Equals("Failed", StringComparison.OrdinalIgnoreCase),
	// 			Is.False);

	// 		App.Tap("CanTouchButtonId");

	// 		App.WaitForTextToBePresentInElement("outputlabel", "ButtonTapped");
	// #if !__MACOS__
	// 		App.Tap("ListTapTarget");
	// 		App.WaitForTextToBePresentInElement("outputlabel", "ItemTapped");
	// #endif
	// 	}
}