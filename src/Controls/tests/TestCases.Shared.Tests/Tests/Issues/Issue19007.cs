using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue19007 : _IssuesUITest
{
	public Issue19007(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Incomplete Label Display on macOS and IOS When Padding is Applied";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelWithPadding()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}