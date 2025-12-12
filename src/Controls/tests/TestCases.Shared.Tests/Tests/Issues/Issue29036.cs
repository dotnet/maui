using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29036 : _IssuesUITest
{
	public Issue29036(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Button RTL text and image overlap";

	[Test]
	[Category(UITestCategories.Button)]
	public void ButtonRTLTextAndImageShouldNotOverlap()
	{
		App.WaitForElement("button");
		VerifyScreenshot();
	}
}