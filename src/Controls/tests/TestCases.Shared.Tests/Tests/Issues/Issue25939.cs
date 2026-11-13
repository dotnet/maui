using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25939 : _IssuesUITest
	{
		public Issue25939(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Unable to Remove Thumb Image in Slider Control ";

		[Test]
		[Category(UITestCategories.Slider)]
		public void SliderShouldChangeThumbImageAndResetIt()
		{
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/33967
			}
			App.WaitForElement("ChangeThumbImageSourceButton");
			App.Click("ChangeThumbImageSourceButton");
			App.Click("ResetThumbImageSourceButton");
			VerifyScreenshot();
		}
	}
}