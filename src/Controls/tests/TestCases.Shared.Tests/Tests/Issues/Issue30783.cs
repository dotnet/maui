using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30783 : _IssuesUITest
{
	public Issue30783(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Android: PlatformImage.FromStream() terminates app in release builds";

	[Test]
	[Category(UITestCategories.Image)]
	public void PlatformImageFromStreamShouldNotCrashInReleaseBuilds()
	{
		App.WaitForElement("LoadImageButton");
		App.Tap("LoadImageButton");

		// Wait for the image loading to complete
		App.WaitForElement("StatusLabel");

		// Verify that the status shows success (no crash occurred)
		var statusText = App.FindElement("StatusLabel").GetText();
		Assert.That(statusText, Does.Contain("Image loaded successfully"),
			"PlatformImage.FromStream should successfully load image without crashing");

		// Verify the image is present
		App.WaitForElement("TestImage");
	}
}