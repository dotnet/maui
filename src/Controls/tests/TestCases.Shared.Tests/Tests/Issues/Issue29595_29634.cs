#if IOSUITEST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29595_29634 : _IssuesUITest
{
	public override string Issue => "iOS CV: GridItemsLayout not centering single item, Empty view not resizing when bounds change";

	public Issue29595_29634(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGridItemsLayoutSingleAndEmptyViewResize()
	{
		// This verifies both 29595 and 29634
		App.WaitForElement("StubLabel");
		Task.Delay(1000).Wait(); // Wait for the initial layout to settle
		VerifyScreenshot();
	}
}
#endif