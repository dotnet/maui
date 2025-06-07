using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29394 : _IssuesUITest
{
	public Issue29394(TestDevice device) : base(device) { }

	public override string Issue => "On Android Shadows should not be rendered over fully transparent areas of drawn shapes";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void TransparentShapeShouldNotDisplayShadow()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}