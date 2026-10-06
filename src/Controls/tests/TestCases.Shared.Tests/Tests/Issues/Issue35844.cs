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

			var landscapeWidth = portraitWidth;
			try
			{
				App.SetOrientationLandscape();
				// The title view stays present while rotation layout is still in progress.
				App.WaitForElement(() =>
				{
					var titleView = App.FindElement("TitleViewGrid");
					if (titleView is null)
						return null;

					landscapeWidth = titleView.GetRect().Width;
					return landscapeWidth > portraitWidth + 50 ? titleView : null;
				}, "Shell TitleView did not expand to landscape width after rotation");

				Assert.That(landscapeWidth, Is.Not.EqualTo(portraitWidth).Within(50),
					"Shell TitleView width should expand after rotating to landscape on iOS 26+");
				Assert.That(landscapeWidth, Is.GreaterThan(portraitWidth),
					"Shell TitleView should be wider in landscape than portrait");
			}
			finally
			{
				App.SetOrientationPortrait();
			}

			// Rotate back and verify TitleView returns to original width
			var finalRect = portraitRect;
			App.WaitForElement(() =>
			{
				var titleView = App.FindElement("TitleViewGrid");
				if (titleView is null)
					return null;

				finalRect = titleView.GetRect();
				return Math.Abs(finalRect.Width - portraitWidth) <= 5 ? titleView : null;
			}, "Shell TitleView did not return to its original portrait width after rotation");
			Assert.That(finalRect.Width, Is.EqualTo(portraitWidth).Within(5),
				"Shell TitleView should return to original portrait width after rotating back");
		}
	}
}
#endif
