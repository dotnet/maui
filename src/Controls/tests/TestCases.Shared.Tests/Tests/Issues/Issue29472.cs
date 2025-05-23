using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29472 : _IssuesUITest
{
	public Issue29472(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ItemsSource is not dynamically cleared in the CarouselView";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewItemsSourceClearedDynamically()
	{
		App.WaitForElement("ClearItemsSourceBtn");
		App.Tap("ClearItemsSourceBtn");
		VerifyScreenshot();
	}
}