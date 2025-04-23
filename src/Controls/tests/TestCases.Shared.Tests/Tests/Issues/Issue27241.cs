using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27241 : _IssuesUITest
{
	public Issue27241(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] CarouselView2 does not render properly when using the Vertical orientation of the LinearItemsLayout";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselViewItemsShouldRenderVertically()
	{
		App.WaitForElement("VerticalItem1");
		VerifyScreenshot();
	}
}