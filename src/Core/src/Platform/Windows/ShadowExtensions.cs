#nullable disable

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using RectF = Microsoft.Maui.Graphics.RectF;
using ShapeDrawable = Microsoft.Maui.Graphics.ShapeDrawable;


namespace Microsoft.Maui.Platform
{
	internal static class ShadowExtensions
	{
		public static async Task<CompositionBrush> GetAlphaMaskAsync(this UIElement element)
		{
			CompositionBrush mask = null;

			try
			{
				//For some reason, using  TextBlock and getting the AlphaMask
				//generates a shadow with a size more smaller than the control size. 
				if (element is TextBlock textElement)
				{
					return textElement.GetAlphaMask();
				}
				if (element is Image image)
				{
					return image.GetAlphaMask();
				}
				if (element is Shape shape)
				{
					return shape.GetAlphaMask();
				}
				if (element is W2DGraphicsView graphicsView && graphicsView.Drawable is ShapeDrawable)
				{
					// Render the shape offscreen into a static mask. A live CompositionVisualSurface of the
					// element is not captured by RenderTargetBitmap, which made the content disappear from
					// RenderTargetBitmap-based captures (including CaptureAsync) whenever a shadow was set.
					return graphicsView.GetShapeAlphaMask();
				}
				else if (element is FrameworkElement frameworkElement)
				{
					var height = (int)frameworkElement.ActualHeight;
					var width = (int)frameworkElement.ActualWidth;

					if (height > 0 && width > 0)
					{
						var visual = ElementCompositionPreview.GetElementVisual(element);
						var elementVisual = visual.Compositor.CreateSpriteVisual();
						elementVisual.Size = element.RenderSize.ToVector2();
						var bitmap = new RenderTargetBitmap();

						await bitmap.RenderAsync(
							element,
							width,
							height);

						var pixels = await bitmap.GetPixelsAsync();

						using (var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
							pixels,
							BitmapPixelFormat.Bgra8,
							bitmap.PixelWidth,
							bitmap.PixelHeight,
							BitmapAlphaMode.Premultiplied))
						{
							var brush = CompositionImageBrush.FromBGRASoftwareBitmap(
								visual.Compositor,
								softwareBitmap,
								new Size(bitmap.PixelWidth, bitmap.PixelHeight));
							mask = brush.Brush;
						}
					}
				}
			}
			catch (Exception exc)
			{
				Debug.WriteLine($"Failed to get AlphaMask {exc}");
				mask = null;
			}

			return mask;
		}

		// Mirrors W2DGraphicsView's CanvasControl draw so the mask matches the on-screen shape.
		static CompositionBrush GetShapeAlphaMask(this W2DGraphicsView graphicsView)
		{
			var drawable = graphicsView.Drawable;
			var actualWidth = (float)graphicsView.ActualWidth;
			var actualHeight = (float)graphicsView.ActualHeight;

			if (drawable is null || actualWidth <= 0 || actualHeight <= 0)
			{
				return null;
			}

			var logicalWidth = MathF.Round(actualWidth);
			var logicalHeight = MathF.Round(actualHeight);

			float adjustedScaleX = 1f, adjustedScaleY = 1f;
			var dirtyRect = new RectF(0, 0, actualWidth, actualHeight);
			if (logicalWidth > 0 && logicalHeight > 0)
			{
				adjustedScaleX = actualWidth / logicalWidth;
				adjustedScaleY = actualHeight / logicalHeight;
				dirtyRect = new RectF(0, 0, logicalWidth, logicalHeight);
			}

			var dpi = 96f * (float)(graphicsView.XamlRoot?.RasterizationScale ?? 1d);

			using var renderTarget = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), actualWidth, actualHeight, dpi);
			using var canvas = new W2DCanvas();

			using (var session = renderTarget.CreateDrawingSession())
			{
				session.Clear(Microsoft.UI.Colors.Transparent);

				try
				{
					canvas.Session = session;
					canvas.CanvasSize = new Size(dirtyRect.Width, dirtyRect.Height);

					const float scaleEpsilon = 0.0001f;
					if (MathF.Abs(adjustedScaleX - 1f) > scaleEpsilon || MathF.Abs(adjustedScaleY - 1f) > scaleEpsilon)
					{
						canvas.Scale(adjustedScaleX, adjustedScaleY);
					}

					if (graphicsView.FlowDirection == Microsoft.UI.Xaml.FlowDirection.RightToLeft)
					{
						canvas.ConcatenateTransform(
							Matrix3x2.CreateScale(-1, 1) *
							Matrix3x2.CreateTranslation(dirtyRect.Width, 0));
					}

					drawable.Draw(canvas, dirtyRect);
				}
				finally
				{
					try
					{
						canvas.ResetState();
					}
					finally
					{
						canvas.Session = null;
					}
				}
			}

			var pixelWidth = (int)renderTarget.SizeInPixels.Width;
			var pixelHeight = (int)renderTarget.SizeInPixels.Height;

			using var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
				renderTarget.GetPixelBytes().AsBuffer(),
				BitmapPixelFormat.Bgra8,
				pixelWidth,
				pixelHeight,
				BitmapAlphaMode.Premultiplied);

			var compositor = ElementCompositionPreview.GetElementVisual(graphicsView).Compositor;
			return CompositionImageBrush.FromBGRASoftwareBitmap(
				compositor,
				softwareBitmap,
				new Size(pixelWidth, pixelHeight)).Brush;
		}
	}
}