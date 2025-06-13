#if ANDROID || IOS
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17228 : _IssuesUITest
{
	public Issue17228(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Back button image color can't be changed";

	[Fact]
	[Category(UITestCategories.Shell)]
	public void CustomBackButtonShouldBeRed()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}
#endif