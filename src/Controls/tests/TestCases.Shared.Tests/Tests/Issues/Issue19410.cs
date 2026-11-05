using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19410 : _IssuesUITest
{
	public Issue19410(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Canvas.DrawImage and Canvas.Rotate cut Image";

	[Test]
	[Category(UITestCategories.ViewBaseTests)]
	public void ClipMaskRemainsOnContainerWhenContentIsTransformed()
	{
		App.WaitForElement("RotateBtn");
		App.Tap("RotateBtn");
		VerifyScreenshot();
	}
}
