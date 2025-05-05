using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17323 : _IssuesUITest
{
	public Issue17323(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Automatic Flow Direction Change for Arabic Strings in When Drawing the String in MAUI on Android and Opposite Behavior in Windows.";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ArabicStringShouldBeLeftToRight()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
