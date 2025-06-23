#if ANDROID //This test case verifies the "System Specific Fonts" exclusively on the Android platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19556 : _IssuesUITest
{
	public override string Issue => "[Android] Systemfonts (light/black etc.) not working";

	public Issue19556(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Label)]
	public void SystemFontsShouldRenderCorrectly()
	{
		_ = App.WaitForElement("label");

		// The test passes if fonts are correctly rendered
		VerifyScreenshot();
	}
}
#endif