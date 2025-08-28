using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29462 : _IssuesUITest
{
	public override string Issue => "CarouselView ItemTemplate Not Updating at Runtime";

	public Issue29462(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void TestDynamicItemTemplateChangeInCarouselView()
	{
		App.WaitForElement("ChangeItemTemplate");
		App.Tap("ChangeItemTemplate");
		VerifyScreenshot();
	}
}