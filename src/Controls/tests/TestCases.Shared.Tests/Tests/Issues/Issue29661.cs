using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29661 : _IssuesUITest
{
	public override string Issue => "[iOS, Mac] StrokeDashArray Property not Rendering";

	public Issue29661(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Border)]
	public void VerifyBorderWithStrokeDashArray()
	{
		App.WaitForElement("StrokeDashArrayLabel");
		VerifyScreenshot();
	}
}