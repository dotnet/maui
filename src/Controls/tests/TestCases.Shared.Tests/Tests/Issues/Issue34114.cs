#if TEST_FAILS_ON_WINDOWS // This test fails on Windows because of a known issue with clipping in WinUI.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34114 : _IssuesUITest
{
	public Issue34114(TestDevice device) : base(device) { }

	public override string Issue => "Label with background clip is not working properly";

	[Test]
	[Category(UITestCategories.Label)]
	public void VerifyLabelBackgroundIsClippedWithRectangleGeometry()
	{
		App.WaitForElement("ClippedLabel");
		App.Tap("ChangeClip");
		VerifyScreenshot();
	}
}
#endif
