#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue24547: _IssuesUITest
{
    public Issue24547(TestDevice device) : base(device) { }

    public override string Issue => "[Windows] FlyoutPage ShouldShowToolbarButton when overridden to return false, still shows button in title bar";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyInitialToolbarButtonHidden()
	{
		App.WaitForElement("DetailButton");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyToolbarButtonHiddenOnNavigateBack()
    {
		App.WaitForElement("DetailButton");
		App.Tap("DetailButton");
		App.WaitForElement("PopButton");
		App.Tap("PopButton");
		App.WaitForElement("DetailButton");
		VerifyScreenshot();
    }
}
#endif