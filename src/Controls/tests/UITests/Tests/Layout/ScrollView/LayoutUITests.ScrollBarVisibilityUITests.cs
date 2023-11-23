using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class ScrollBarVisibilityUITests : LayoutUITests
	{
		public ScrollBarVisibilityUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Modify the visibility of the ScrollBars")]
		public async Task ScrollBarVisibility()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android },
				"Currently fails on Android; see https://github.com/dotnet/maui/issues/12028");

			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/7767");

			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Windows },
				"Currently fails on Windows; see https://github.com/dotnet/maui/issues/7766");

			App.Click("ScrollViewScrollBarVisibility");
			App.WaitForElement("TestScrollView");
			
			await Task.Delay(500);

			// 1. The ScrollView use ScrollBarVisibility.Never and
			// the scrollbars should not appear.
			VerifyScreenshot("ScrollBarVisibilityNever");

			// 2. Tap the Button to scroll to the end.
			App.Click("ChangeVisibility");

			// 3. The ScrollView use ScrollBarVisibility.Always and
			// the scrollbars should always appear.
			VerifyScreenshot("ScrollBarVisibilityAlways");
		}
	}
}