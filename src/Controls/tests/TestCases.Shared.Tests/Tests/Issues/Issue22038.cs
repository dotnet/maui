using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22038 : _IssuesUITest
{
	public Issue22038(TestDevice device) : base(device) { }

	public override string Issue => "[Android] Layout with small explicit size and Opacity < 1 clips overflowing children";

	[Test]
	[Category(UITestCategories.Layout)]
	public void OverflowingChildrenShouldNotBeClippedWhenOpacityIsLessThanOne()
	{
		App.WaitForElement("Issue22038DescriptionLabel");
		VerifyScreenshot();
	}
}
