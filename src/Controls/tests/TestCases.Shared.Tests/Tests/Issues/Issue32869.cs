using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32869 : _IssuesUITest
{
	public Issue32869(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Image control crashes on Android when image width exceeds height";

	[Test]
	[Category(UITestCategories.Image)]
	public void Issue32869_Image()
	{
		App.WaitForElement("TestImage");
		VerifyScreenshot();
	}
}