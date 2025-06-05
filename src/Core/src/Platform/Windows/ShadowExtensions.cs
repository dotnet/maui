#nullable disable

using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.Graphics.Imaging;


namespace Microsoft.Maui.Platform
{
	internal static class ShadowExtensions
	{
		public static async Task<CompositionBrush> GetAlphaMaskAsync(this UIElement element)
		{
			CompositionBrush mask = null;

			try
			{
				var visual = ElementCompositionPreview.GetElementVisual(element);
				var isClipped = visual.Clip is not null;

				if (!isClipped && element is TextBlock textElement)
				{
					return textElement.GetAlphaMask();
				}
				if (!isClipped && element is Image image)
				{
					return image.GetAlphaMask();
				}
				if (!isClipped && element is Shape shape)
				{
					return shape.GetAlphaMask();
				}
				if (!isClipped && element is ContentPanel contentPanel)
				{
					return contentPanel.BorderPath?.GetAlphaMask();
				}
				if (element is FrameworkElement frameworkElement)
				{
					var height = (int)frameworkElement.ActualHeight;
					var width = (int)frameworkElement.ActualWidth;

					if (height > 0 && width > 0)
					{
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
	}
}