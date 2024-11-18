#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19926 : _IssuesUITest
{
	public override string Issue => "[Android] Opacity bug on BoxView.Background";

	public Issue19926(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.BoxView)]
	public void PropertiesShouldBeCorrectlyApplied()
	{
		_ = App.WaitForElement("boxView");

		VerifyScreenshot();
	}
}
#endif
