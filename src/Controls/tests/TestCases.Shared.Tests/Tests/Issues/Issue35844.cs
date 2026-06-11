#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // SetOrientationLandscape/Portrait is only supported on iOS and Android.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue35844 : _IssuesUITest
	{
		public override string Issue => "Shell TitleView does not resize after rotation on iOS 26+";

		public Issue35844(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellTitleViewResizesOnRotation()
		{
			App.WaitForElement("TitleViewGrid");
			App.WaitForElement("StatusLabel");

			// Capture portrait width
			var portraitRect = App.WaitForElement("TitleViewGrid").GetRect();
			var portraitWidth = portraitRect.Width;

			App.SetOrientationLandscape();

			// After rotation, TitleView width must change to fill the wider nav bar
			var landscapeRect = App.WaitForElement("TitleViewGrid").GetRect();
			var landscapeWidth = landscapeRect.Width;

			Assert.That(landscapeWidth, Is.Not.EqualTo(portraitWidth).Within(100),
				"Shell TitleView width should expand after rotating to landscape on iOS 26+");
			Assert.That(landscapeWidth, Is.GreaterThan(portraitWidth),
				"Shell TitleView should be wider in landscape than portrait");

			// Rotate back and verify TitleView returns to original width
			App.SetOrientationPortrait();

			var finalRect = App.WaitForElement("TitleViewGrid").GetRect();
			Assert.That(finalRect.Width, Is.EqualTo(portraitWidth).Within(5),
				"Shell TitleView should return to original portrait width after rotating back");
		}
	}
}
#endif
