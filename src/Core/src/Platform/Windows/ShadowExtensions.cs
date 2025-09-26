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
			if (element is null)
			{
				return null;
			}

			CompositionBrush mask = null;

			try
			{
				// Check if element is truly visible in the visual tree

				// RenderTargetBitmap.RenderAsync fails with "Value does not fall within the expected range"
				// when called on elements that are not visible (collapsed, hidden, or in process of hiding/showing)
				// See: https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.rendertargetbitmap.renderasync
				if (!IsElementRenderable(element))
				{
					return null;
				}

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
				if (element is ContentPanel contentPanel)
				{
					return contentPanel.BorderPath?.GetAlphaMask();
				}
				else if (element is FrameworkElement frameworkElement)
				{
					var height = (int)frameworkElement.ActualHeight;
					var width = (int)frameworkElement.ActualWidth;

					// RenderTargetBitmap has limitations on maximum dimensions
					// See: https://docs.microsoft.com/en-us/windows/win32/direct2d/direct2d-limits
					// Maximum texture size is typically 16384x16384 pixels for most hardware
					const int maxDimension = 16384;

					if (height > 0 && width > 0 && height <= maxDimension && width <= maxDimension)
					{
						var visual = ElementCompositionPreview.GetElementVisual(element);

						if (visual?.Compositor is null)
						{
							return null;
						}

						var elementVisual = visual.Compositor.CreateSpriteVisual();
						elementVisual.Size = element.RenderSize.ToVector2();

						var bitmap = new RenderTargetBitmap();
						await bitmap.RenderAsync(element, width, height);

						var pixels = await bitmap.GetPixelsAsync();

						if (pixels is null || pixels.Length == 0)
						{
							return null;
						}

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

		static bool IsElementRenderable(UIElement element)
		{
			if (element is null)
				return false;

			// Check the element itself
			if (element.Visibility == UI.Xaml.Visibility.Collapsed)
				return false;

			// For FrameworkElement, also check if it's loaded and has positive ActualWidth/Height
			if (element is FrameworkElement frameworkElement)
			{
				// Element must be loaded into the visual tree
				if (!frameworkElement.IsLoaded)
					return false;

				// Element must have positive dimensions (ActualWidth/Height > 0)
				if (frameworkElement.ActualWidth <= 0 || frameworkElement.ActualHeight <= 0)
					return false;
			}

			// Check all parent elements up the visual tree
			var parent = element;
			while (parent != null)
			{
				if (parent.Visibility == UI.Xaml.Visibility.Collapsed)
					return false;

				// Move up
				if (parent is FrameworkElement fe)
					parent = fe.Parent as UIElement;
				else
					break;
			}

			return true;
		}
	}
}