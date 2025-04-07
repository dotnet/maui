using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28725 : _IssuesUITest
{
	public Issue28725(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ImagePaint is not rendering in View";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewImagePaint()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
