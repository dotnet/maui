using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue488 : _IssuesUITest
	{
		public Issue488(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Resizing the Label results in wrapped text being cropped on iOS";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatforms]
		public void Issue488TestsLongTextRotation()
		{
			App.WaitForNoElement("I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text.");
			App.SetOrientationLandscape();
			App.Screenshot("Resize Label.Text by rotating to landscape");
			App.WaitForNoElement("I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text.");
			App.Screenshot("Entire Label.Text present");
			App.SetOrientationPortrait();
			App.Screenshot("Rotated back to portrait");
		}
	}
}