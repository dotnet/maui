using System;
using System.Threading.Tasks;
using Android.Content.Res;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShapeViewHandlerTests
	{
		MauiShapeView GetPlatformShapeView(ShapeViewHandler shapeViewHandler) =>
			shapeViewHandler.PlatformView;

		Task ValidateNativeFill(IShapeView shapeView, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				return GetPlatformShapeView(CreateHandler(shapeView)).AssertContainsColor(color, MauiContext);
			});
		}

		[Fact(DisplayName = "Shape Drawing Uses MAUI Display Density")]
		public Task ShapeDrawingUsesMauiDisplayDensity()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var context = MauiContext.Context!;
				var mauiDensity = context.GetDisplayDensity();
				var configuration = new Configuration(context.Resources!.Configuration)
				{
					DensityDpi = context.Resources.Configuration.DensityDpi + 80
				};

				using var densityChangedContext = context.CreateConfigurationContext(configuration);
				var liveDensity = densityChangedContext.Resources!.DisplayMetrics!.Density;

				Assert.NotEqual(mauiDensity, liveDensity);

				var drawable = new RecordingDrawable();
				using var shapeView = new MauiShapeView(densityChangedContext)
				{
					Drawable = drawable
				};

				const int logicalWidth = 100;
				const int logicalHeight = 50;
				var pixelWidth = (int)Math.Round(logicalWidth * mauiDensity);
				var pixelHeight = (int)Math.Round(logicalHeight * mauiDensity);

				shapeView.Layout(0, 0, pixelWidth, pixelHeight);

				using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(
					pixelWidth,
					pixelHeight,
					global::Android.Graphics.Bitmap.Config.Argb8888!);
				using var canvas = new global::Android.Graphics.Canvas(bitmap);

				shapeView.Draw(canvas);

				Assert.Equal(logicalWidth, drawable.DirtyRect.Width);
				Assert.Equal(logicalHeight, drawable.DirtyRect.Height);
				Assert.Equal(pixelWidth, drawable.DirtyRect.Width * mauiDensity, precision: 0);
				Assert.Equal(pixelHeight, drawable.DirtyRect.Height * mauiDensity, precision: 0);
			});
		}

		sealed class RecordingDrawable : IDrawable
		{
			public RectF DirtyRect { get; private set; }

			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				DirtyRect = dirtyRect;
				canvas.FillColor = Colors.Red;
				canvas.FillRectangle(dirtyRect);
			}
		}
	}
}