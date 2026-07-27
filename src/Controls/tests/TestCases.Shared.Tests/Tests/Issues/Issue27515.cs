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
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/33967
			}
			App.WaitForElement("ChangeThumbImageButton");
			App.Click("ChangeThumbImageButton");
			App.Click("ChangeThumbColorButton");
			VerifyScreenshot();
		}
	}
}
#endif
