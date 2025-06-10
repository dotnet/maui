using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29898 : _IssuesUITest
{
	public override string Issue => "[iOS, macOS] StrokeDashArray on Border does not reset when set to null";

	public Issue29898(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Border)]
	public void VerifyBorderWithNullStrokeDashArray()
	{
		App.WaitForElement("ClearDashButton");
		App.Tap("ClearDashButton");
		VerifyScreenshot();
	}
}