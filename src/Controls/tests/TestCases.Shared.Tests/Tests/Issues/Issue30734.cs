using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30734 : _IssuesUITest
{
	public Issue30734(TestDevice device) : base(device) { }

	public override string Issue => "Android: Updating Image.Source with ImageSource.FromStream() is broken in Release builds";

	[Test]
	[Category(UITestCategories.Image)]
	public void StreamImageSourceShouldDisplayProperlyInReleaseBuilds()
	{
		App.WaitForElement("StreamImageButton");
		App.Tap("StreamImageButton");
		VerifyScreenshot();
	}
}