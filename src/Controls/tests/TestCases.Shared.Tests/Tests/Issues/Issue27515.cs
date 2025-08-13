#if ANDROID || IOS || MACCATALYST // Slider's thumb image doesn't respect color on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27515 : _IssuesUITest
	{
		public Issue27515(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Slider's thumb image doesn't respect color";

		[Test]
		[Category(UITestCategories.Slider)]
		public void SliderThumbImageShouldRespectColor()
		{
			App.WaitForElement("ChangeThumbImageButton");
			App.Click("ChangeThumbImageButton");
			App.Click("ChangeThumbColorButton");
			VerifyScreenshot();
		}
	}
}
#endif
