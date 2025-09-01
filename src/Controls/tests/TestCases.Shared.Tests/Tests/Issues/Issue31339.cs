using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31339 : _IssuesUITest
{
	public Issue31339(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] CarouselViewHandler2 - NSInternalInconsistencyException thrown when setting ItemsSources";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselView2ShouldNotCrashAndMaintainProperPosition()
	{
		App.WaitForElement("TestCarouselView");
		App.Tap("UpdateButton");
		App.WaitForElement("0");
	}
}