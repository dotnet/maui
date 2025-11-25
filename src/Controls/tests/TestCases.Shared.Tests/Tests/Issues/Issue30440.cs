using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue30440 : _IssuesUITest
{
	public Issue30440(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Image clipping not working";

	[Test]
	[Category(UITestCategories.Image)]
	public void Issue30440ImageShouldClipCorrectly()
	{
		App.WaitForElement("circleLabel");
		VerifyScreenshot();
	}
}