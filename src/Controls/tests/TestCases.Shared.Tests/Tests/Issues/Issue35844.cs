#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // SetOrientationLandscape/Portrait is only supported on iOS and Android.

using System.Drawing;
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
		[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
		public void ShellTitleViewResizesOnRotation()
		{
			try
			{
				App.SetOrientationPortrait();
				App.WaitForElement("TitleViewGrid");
				App.WaitForElement("StatusLabel");

				Rectangle? previousPortraitRect = null;
				var portraitRect = Rectangle.Empty;
				App.WaitForElement(() =>
				{
					var view = App.FindElement("TitleViewGrid");
					if (view is null)
					{
						previousPortraitRect = null;
						return null;
					}

					var rect = view.GetRect();
					var stable = rect.Width > 0 && rect.Height > 0 && previousPortraitRect == rect;
					previousPortraitRect = rect;
					portraitRect = rect;
					return stable ? view : null;
				});
				var portraitWidth = portraitRect.Width;

				App.SetOrientationLandscape();
				var landscapeWidth = 0;
				App.WaitForElement(() =>
				{
					var view = App.FindElement("TitleViewGrid");
					if (view is null)
						return null;

					landscapeWidth = view.GetRect().Width;
					return landscapeWidth > portraitWidth + 50 ? view : null;
				});

				Assert.That(landscapeWidth, Is.Not.EqualTo(portraitWidth).Within(50),
					"Shell TitleView width should expand after rotating to landscape on iOS 26+");
				Assert.That(landscapeWidth, Is.GreaterThan(portraitWidth),
					"Shell TitleView should be wider in landscape than portrait");

				App.SetOrientationPortrait();

				App.RetryAssert(() =>
				{
					var finalRect = App.WaitForElement("TitleViewGrid").GetRect();
					Assert.That(finalRect.Width, Is.EqualTo(portraitWidth).Within(5),
						"Shell TitleView should return to original portrait width after rotating back");
				});
			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}
	}
}
#endif
