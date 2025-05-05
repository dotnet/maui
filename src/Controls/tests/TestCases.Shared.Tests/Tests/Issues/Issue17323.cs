using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17323 : _IssuesUITest
{
	public Issue17323(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Arabic text flows RTL on Android in MAUI, but flows LTR on Windows";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ArabicStringShouldBeLeftToRight()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
