using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22887 : _IssuesUITest
{

	public Issue22887(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Microsoft.Maui.Controls.ImageSource.FromFile fails in iOS when image in subfolder";


	[Test]
	[Category(UITestCategories.Image)]
	public void ImageShouldLoadFromSubfolder()
	{
		App.WaitForElement("ImageView", timeoutMessage: "Image is not rendered in the view");

	}
}