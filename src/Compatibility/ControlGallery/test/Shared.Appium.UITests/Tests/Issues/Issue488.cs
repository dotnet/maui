using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue488 : IssuesUITest
	{
		public Issue488(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Resizing the Label results in wrapped text being cropped on iOS";

		[Test]
		[Category(UITestCategories.Label)]
		public void Issue488TestsLongTextRotation()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement("I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text.");
			RunningApp.SetOrientationLandscape();
			RunningApp.Screenshot("Resize Label.Text by rotating to landscape");
			RunningApp.WaitForNoElement("I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text.");
			RunningApp.Screenshot("Entire Label.Text present");
			RunningApp.SetOrientationPortrait();
			RunningApp.Screenshot("Rotated back to portrait");
		}
	}
}