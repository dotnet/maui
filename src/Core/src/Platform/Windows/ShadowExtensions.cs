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
using Windows.Foundation;
using Windows.Graphics.Imaging;

namespace Microsoft.Maui
{
	public static class ShadowExtensions
	{
		public static async Task<CompositionBrush> GetAlphaMaskAsync(this UIElement element)
		{
			CompositionBrush mask = null;

			try
			{
				if (element is TextBlock textElement)
				{
					mask = textElement.GetAlphaMask();
				}
				// We also use this option with images and shapes, even though have the option to
				// get the AlphaMask directly (in case it is clipped).
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
			catch(Exception exc)
			{
				Debug.WriteLine($"Failed to get AlphaMask {exc}");
				mask = null;
			}

			return mask;
		}
	}
}