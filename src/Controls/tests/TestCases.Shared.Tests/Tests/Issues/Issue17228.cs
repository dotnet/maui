#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17228 : _IssuesUITest
{
	public Issue17228(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Back button image color can't be changed";

	[Test]
	[Category(UITestCategories.Shell)]
	public void CustomBackButtonShouldBeRed()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}
#endif