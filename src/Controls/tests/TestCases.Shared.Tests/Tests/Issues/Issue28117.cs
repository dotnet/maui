#if !MACCATALYST // On Mac platform, Label does not wrap properly when a width request is set https://github.com/dotnet/maui/issues/15559
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28117 : _IssuesUITest
{
	public Issue28117(TestDevice device) : base(device) { }

	public override string Issue => "Label text is cropped inside the border control with a specific padding value on certain Android devices";

	[Test]
	[Category(UITestCategories.Border)]
	public void ShouldDisplayLabelWithoutBeingCroppedInsideBorder()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
#endif