#if !WINDOWS
// This test won't work on Windows right now because we can only test desktop, so touch events
// (like LongPress) don't really work. The test should work manually on a touch screen, though.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla60122 : _IssuesUITest
{
	public Bugzilla60122(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "LongClick on image not working";

	// [FailsOnAndroid]
	// [FailsOnIOS]
	// [Test]
	// [Category(UITestCategories.Gestures)]
	// public void LongClickFiresOnCustomImageRenderer()
	// {
	// 	App.WaitForElement(ImageId);
	// 	App.TouchAndHold(ImageId);
	// 	App.WaitForElement(Success);
	// }
}

#endif