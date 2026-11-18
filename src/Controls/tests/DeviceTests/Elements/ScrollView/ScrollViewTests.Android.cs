using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using ABitmap = Android.Graphics.Bitmap;
using ACanvas = Android.Graphics.Canvas;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewTests
	{
		// https://github.com/dotnet/maui/issues/37306
		[Fact(DisplayName = "ScrollView content draws under the safe-area padding while scrolled")]
		public async Task ContentDrawsUnderSafeAreaPaddingWhileScrolled()
		{
			SetupBuilder();

			var scrollView = new ScrollView
			{
				BackgroundColor = Colors.Blue,
				Content = new VerticalStackLayout { BackgroundColor = Colors.Red, HeightRequest = 5000 }
			};
			var page = new ContentPage { SafeAreaEdges = SafeAreaEdges.None, Content = scrollView };

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				var platformView = (MauiScrollView)scrollView.Handler.PlatformView;
				await AssertEventually(() => platformView.PaddingTop > 0 || platformView.PaddingBottom > 0,
					timeout: 5000, message: "The ScrollView never received safe-area padding.");

				await scrollView.ScrollToAsync(0, 1000, false);
				Assert.True(platformView.ScrollY > 0, "The ScrollView did not scroll.");

				using var bitmap = ABitmap.CreateBitmap(platformView.Width, platformView.Height, ABitmap.Config.Argb8888);
				using (var canvas = new ACanvas(bitmap))
				{
					// A parent applies the scroll offset when it draws the ScrollView, so do the same here.
					canvas.Translate(-platformView.ScrollX, -platformView.ScrollY);
					platformView.Draw(canvas);
				}

				// Mid-scroll, the padding bands must show the same content as the middle of the viewport,
				// not the ScrollView's background.
				var x = bitmap.Width / 2;
				var content = bitmap.ColorAtPoint(x, bitmap.Height / 2);
				if (platformView.PaddingTop > 0)
					bitmap.AssertColorAtPoint(content, x, platformView.PaddingTop / 2);
				if (platformView.PaddingBottom > 0)
					bitmap.AssertColorAtPoint(content, x, bitmap.Height - 1 - platformView.PaddingBottom / 2);

				Assert.False(platformView.ClipToPadding);
			});
		}
	}
}
