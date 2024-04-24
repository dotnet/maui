using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue19556 : _IssuesUITest
{
	public override string Issue => "[Android] Systemfonts (light/black etc.) not working";

	public Issue19556(TestDevice device)
		: base(device)
	{ }

    [Test]
	public void SystemFontsShouldRenderCorrectly()
	{
		this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

		_ = App.WaitForElement("label");

		// The test passes if fonts are correctly rendered
		VerifyScreenshot();
	}
}
