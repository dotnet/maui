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

		// Large ItemSource
		App.WaitForElement("LargeCollectionButton");
		App.Tap("LargeCollectionButton");

		//Null ItemSource
		App.WaitForElement("NullButton");
		App.Tap("NullButton");

		// Empty ItemSource
		App.WaitForElement("EmptyButton");
		App.Tap("EmptyButton");

		//Position and ItemSource update
		App.WaitForElement("UpdatePositionWithItemSourceButton");
		App.Tap("UpdatePositionWithItemSourceButton");

		App.WaitForElement("TestCarouselView");
	}
}