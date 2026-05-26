using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19095 : _IssuesUITest
{
	public Issue19095(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shape stroke is drawing only the external part, the inner part (within the shape area) is not drawing";

	[Test]
	[Category(UITestCategories.Shape)]
	public void PolygonWithFillRuleNonZero()
	{
		App.WaitForElement("descriptionLabel");
		VerifyScreenshot();
	}
}