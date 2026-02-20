#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27377 : _IssuesUITest
	{
		public Issue27377(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SwipeView: SwipeItem.IconImageSource.FontImageSource color value not honored";

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void FontImageSourceShouldHonorColor()
		{
			App.WaitForElement("Button");
			App.Click("Button");
			VerifyScreenshot();
		}
	}
}
#endif