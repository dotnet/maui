#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31700 : _IssuesUITest
{
	public Issue31700(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Image is not displayed in Mac and iOS using Media Picker when it is placed in a Grid Layout in .NET 10";

	[Test]
	[Category(UITestCategories.Image)]
	public void ImageWithAbsolutePathShouldDisplay()
	{
		// Wait for the load button to appear
		App.WaitForElement("LoadImageButton");
		
		// Click the button to load image from absolute path
		App.Tap("LoadImageButton");
		
		// Wait for the image to be loaded
		App.WaitForElement("TestImage");
		
		// Verify the button text changed, indicating success
		var buttonText = App.FindElement("LoadImageButton").GetText();
		Assert.That(buttonText, Is.EqualTo("Image Loaded!"));
	}
}
#endif
