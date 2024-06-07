#if ANDROID
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
	public void SystemFontsShouldRenderCorrectly()
	{
		_ = App.WaitForElement("label");

		// The test passes if fonts are correctly rendered
		VerifyScreenshot();
	}
}
#endif