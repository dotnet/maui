using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29772 : _IssuesUITest
{
	public Issue29772(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ItemSpacing doesnot work on CarouselView";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void Issue29772ItemSpaceShouldApply()
	{
		App.WaitForElement("29772DescriptionLabel");
		VerifyScreenshot();
	}
}