#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Android Issue Link: https://github.com/dotnet/maui/issues/24676, Windows Issue Link: https://github.com/dotnet/maui/issues/34071
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34083 : _IssuesUITest
{
	public Issue34083(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Toolbar Items Do Not Reflect Shell ForegroundColor";

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyShellForegroundColorIsAppliedToToolbarItems()
	{
		App.WaitForElement("Issue34083_DescriptionLabel");
		VerifyScreenshot();
	}
}
#endif