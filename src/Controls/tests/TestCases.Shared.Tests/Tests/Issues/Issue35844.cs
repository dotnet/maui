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
			try
			{
				App.SetOrientationPortrait();
				WaitForOrientation(landscape: false);
				var portraitWidth = App.WaitForElementAndGetRect("TitleViewGrid").Width;
				Assert.That(portraitWidth, Is.GreaterThan(0));

				App.SetOrientationLandscape();
				WaitForOrientation(landscape: true);
				App.RetryAssert(() =>
				{
					var landscapeWidth = App.WaitForElementAndGetRect("TitleViewGrid").Width;
					Assert.That(landscapeWidth, Is.GreaterThan(portraitWidth + 50),
						"Shell TitleView should expand to fill the wider navigation bar");
				});

				App.SetOrientationPortrait();
				WaitForOrientation(landscape: false);
				App.RetryAssert(() =>
					Assert.That(App.WaitForElementAndGetRect("TitleViewGrid").Width,
						Is.EqualTo(portraitWidth).Within(5),
						"Shell TitleView should return to its original portrait width"));
			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}

		void WaitForOrientation(bool landscape)
		{
			App.RetryAssert(() =>
			{
				var content = App.WaitForElementAndGetRect("RotationContent");
				Assert.That(content.Width > content.Height, Is.EqualTo(landscape));
				Assert.That(content.Width, Is.GreaterThan(0));
				Assert.That(content.Height, Is.GreaterThan(0));
			});
		}
	}
}
#endif
