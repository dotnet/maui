using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36629 : _IssuesUITest
{
	public Issue36629(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] SearchHandler FontSize, FontFamily, VerticalTextAlignment, and FontAttributes are not applied";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void SearchHandlerFontPropertiesShouldBeApplied()
	{
		// Wait for the page to load
		App.WaitForElement("ContentLabel36629");

		// Take a screenshot to verify the SearchHandler font properties are visually applied
		// (FontSize=14, FontFamily=Dokdo, FontAttributes=Bold, VerticalTextAlignment=Start)
		VerifyScreenshot();
	}
}