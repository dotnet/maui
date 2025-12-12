using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30081 : _IssuesUITest
{
	public Issue30081(TestDevice device) : base(device) { }

	public override string Issue => "[Android] ScrollView scroll position changes unexpectedly when Orientation is set to Horizontal and FlowDirection is RTL at runtime";
	[Test]
	[Category(UITestCategories.ScrollView)]
	public void VerifyHorizontalScrollViewPositionAtRuntime()
	{
		App.WaitForElement("ToggleOrientationButton");
		App.Tap("ToggleOrientationButton");
		VerifyScreenshot();
	}
}