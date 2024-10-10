using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22520 : _IssuesUITest
{
	public override string Issue => "ImageButton border (BorderWidth) overlaps the image";

	public Issue22520(TestDevice device)
		: base(device)
	{ }

    [Test]
	[Category(UITestCategories.ImageButton)]
	public void ImageShouldNotOverlapBorder()
	{
		_ = App.WaitForElement("imageButton");
		VerifyScreenshot();
	}
}
