using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20596 : _IssuesUITest
{
	public Issue20596(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Button with corner radius shadow broken on Android device";

	[Test]
	[Category(UITestCategories.Button)]
	public void ShadowShouldUpdateOnCornerRadiusChange()
	{
		App.WaitForElement("UpdateCornerRadiusButton");
		App.Click("UpdateCornerRadiusButton");
		VerifyScreenshot();
	}
}