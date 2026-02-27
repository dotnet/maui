using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30403 : _IssuesUITest
{
	public Issue30403(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit";

	[Test]
	[Category(UITestCategories.Image)]
	public void ImageRespectsVerticalAndHorizontalOptionsWithAspectFit()
	{
		App.WaitForElement("TestImage");

		// Verify that the image is positioned correctly according to VerticalOptions.Center and HorizontalOptions.Center
		// with AspectFit aspect ratio. The image should appear in the center of the Grid.
		VerifyScreenshot();
	}
}
