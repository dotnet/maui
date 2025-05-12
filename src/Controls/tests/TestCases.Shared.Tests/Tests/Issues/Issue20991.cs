using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue20991 : _IssuesUITest
{
	public Issue20991(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Custom IDrawable control does not support binding";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewDrawableShouldSupportBinding()
	{
		App.WaitForElement("descriptionLabel");
		VerifyScreenshot();
	}
}