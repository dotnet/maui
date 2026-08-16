using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public class Issue36573 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task BackgroundChangeRetintsImplicitFontIcon()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<SwipeView, SwipeViewHandler>();
					handlers.AddHandler<SwipeItem, SwipeItemMenuItemHandler>();
				});
			});

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.White,
				IconImageSource = new FontImageSource
				{
					Glyph = "#",
					Size = 28
				},
				Text = "Archive"
			};

			var swipeItems = new SwipeItems
			{
				Mode = SwipeMode.Reveal
			};
			swipeItems.Add(swipeItem);

			var swipeView = new SwipeView
			{
				HeightRequest = 100,
				LeftItems = swipeItems,
				Content = new Grid
				{
					BackgroundColor = Colors.LightGray,
					HeightRequest = 100
				}
			};

			await AttachAndRun(swipeView, async handler =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;
				swipeView.Open(OpenSwipeItem.LeftItems, false);

				await AssertEventually(() => platformView.ChildCount > 1);
				var actionView = Assert.IsAssignableFrom<ViewGroup>(platformView.GetChildAt(1));

				await AssertEventually(() => actionView.ChildCount > 0);
				var textView = Assert.IsAssignableFrom<TextView>(actionView.GetChildAt(0));

				await AssertEventually(() =>
				{
					var drawable = textView.GetCompoundDrawables()[1];
					return drawable is not null && drawable.Bounds.Width() > 0 && drawable.Bounds.Height() > 0;
				});
				var initialDrawable = textView.GetCompoundDrawables()[1];
				var initialColor = RenderForegroundColor(initialDrawable);
				Assert.True(IsNearBlack(initialColor), $"Expected the initial icon to be black, but it was {initialColor}.");

				swipeItem.BackgroundColor = Colors.Black;
				var swipeItemHandler = Assert.IsType<SwipeItemMenuItemHandler>(swipeItem.Handler);
				swipeItemHandler.UpdateValue(nameof(IView.Background));

				Assert.True(
					IsNearWhite(new AColor(textView.CurrentTextColor)),
					$"Expected the updated text to be white, but it was {new AColor(textView.CurrentTextColor)}.");

				var updatedDrawable = Assert.IsAssignableFrom<Drawable>(textView.GetCompoundDrawables()[1]);
				var updatedColor = RenderForegroundColor(updatedDrawable);
				Assert.True(IsNearWhite(updatedColor), "SwipeItem icon should retint to white after its background changes to black.");
			});
		}

		static AColor RenderForegroundColor(Drawable drawable)
		{
			int width = drawable.Bounds.Width();
			int height = drawable.Bounds.Height();
			using var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888!);
			using var canvas = new Canvas(bitmap);
			drawable.Draw(canvas);

			int pixel = 0;
			int highestAlpha = -1;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int candidate = bitmap.GetPixel(x, y);
					int alpha = AColor.GetAlphaComponent(candidate);
					if (alpha > highestAlpha)
					{
						pixel = candidate;
						highestAlpha = alpha;
					}
				}
			}

			return AColor.Rgb(
				AColor.GetRedComponent(pixel),
				AColor.GetGreenComponent(pixel),
				AColor.GetBlueComponent(pixel));
		}

		static bool IsNearBlack(AColor color)
		{
			int value = color.ToArgb();
			return AColor.GetRedComponent(value) <= 5
				&& AColor.GetGreenComponent(value) <= 5
				&& AColor.GetBlueComponent(value) <= 5;
		}

		static bool IsNearWhite(AColor color)
		{
			int value = color.ToArgb();
			return AColor.GetRedComponent(value) >= 250
				&& AColor.GetGreenComponent(value) >= 250
				&& AColor.GetBlueComponent(value) >= 250;
		}
	}
}
