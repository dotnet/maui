#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	class Issue14587 : _IssuesUITest
	{
		public Issue14587(TestDevice device) : base(device) { }
		public override string Issue => "ImageButton with FontImageSource becomes invisible";

		[Test]
		[Category(UITestCategories.Image)]
		public void ImageDoesNotDisappearWhenSwitchingTab()
		{
			App.WaitForElement("Icon1");
			App.Tap("Icon2");
			App.Tap("Icon3");
			App.Tap("Icon1");
			App.Tap("ToTab2");

			App.WaitForElement("ToTab1");
			App.Tap("ToTab1");

			App.WaitForElement("Icon1");
			VerifyScreenshot();
		}
	}
}
#endif