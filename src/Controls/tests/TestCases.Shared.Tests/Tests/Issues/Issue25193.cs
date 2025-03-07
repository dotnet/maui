#if TEST_FAILS_ON_CATALYST // This specific test takes a full screen screenshot in CI (cannot reproduce locally).
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25193 : _IssuesUITest
{
	public Issue25193(TestDevice device) : base(device) { }

	public override string Issue => "Background gradients don't work for some views";

	[Test]
	[Category(UITestCategories.Label)]
	[Category(UITestCategories.ScrollView)]
	[Category(UITestCategories.Button)]
	[Category(UITestCategories.Entry)]
	[Category(UITestCategories.Picker)]
	public void BackgroundGradientsShouldRenderCorrectly()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}
#endif