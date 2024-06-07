#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageSourceAsync((IFontImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IFontImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			try
			{
				// TODO: The DPI not working as the view is not respecting the
				//       value, so just reset to 1 to keep the correct size.
				//       https://github.com/dotnet/maui/issues/1000
				scale = 1;

				// TODO: use a cached way
				var image = RenderImageSource(imageSource, scale);

				if (image == null)
					throw new InvalidOperationException("Unable to generate font image.");

				// TODO: The DPI not working as the view is not respecting the
				//       value, so mark this image as non-resolution-dependent.
				//       https://github.com/dotnet/maui/issues/1000
				var result = new ImageSourceServiceResult(image, false);

				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", imageSource.Glyph);
				throw;
			}
		}

		static Task<IImageSourceServiceResult<WImageSource>?> FromResult(IImageSourceServiceResult<WImageSource>? result) =>
			Task.FromResult(result);

		internal CanvasImageSource RenderImageSource(IFontImageSource imageSource, float scale)
		{
			// TODO: The DPI not working as the view is not respecting the
			//       value, so just reset to 1 to keep the correct size.
			//       https://github.com/dotnet/maui/issues/1000
			scale = 1;

			var dpi = scale * DeviceDisplay.BaseLogicalDpi;

			var fontFamily = GetFontSource(imageSource);
			var fontSize = (float)imageSource.Font.Size;
			var color = (imageSource.Color ?? Colors.White).ToWindowsColor();

			var textFormat = new CanvasTextFormat
			{
				FontFamily = fontFamily,
				FontSize = fontSize,
				HorizontalAlignment = CanvasHorizontalAlignment.Center,
				VerticalAlignment = CanvasVerticalAlignment.Center,
				Options = CanvasDrawTextOptions.Default
			};

			var device = CanvasDevice.GetSharedDevice();
			using var layout = new CanvasTextLayout(device, imageSource.Glyph, textFormat, 0, 0);

			// add a 1px padding all around
			var canvasWidth = (float)layout.DrawBounds.Width + 2;
			var canvasHeight = (float)layout.DrawBounds.Height + 2;

			var canvasImageSource = new CanvasImageSource(device, canvasWidth, canvasHeight, dpi);
			using (var ds = canvasImageSource.CreateDrawingSession(UI.Colors.Transparent))
			{
				// offset by 1px as we added a 1px padding
				var x = (layout.DrawBounds.X * -1) + 1;
				var y = (layout.DrawBounds.Y * -1) + 1;

				ds.DrawTextLayout(layout, (float)x, (float)y, color);
			}

			return canvasImageSource;
		}

		string GetFontSource(IFontImageSource imageSource)
		{
			if (imageSource == null)
				return string.Empty;

			var fontFamily = FontManager.GetFontFamily(imageSource.Font);

			var fontSource = fontFamily.Source;

			var allFamilies = fontFamily.Source.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			if (allFamilies.Length > 1)
			{
				// There's really no perfect solution to handle font families with fallbacks (comma-separated)	
				// So if the font family has fallbacks, only one is taken, because CanvasTextFormat	
				// only supports one font family
				var source = imageSource.Font.Family ?? String.Empty;

				foreach (var family in allFamilies)
				{
					if (family.Contains(source, StringComparison.Ordinal))
					{
						fontSource = family;
						break;
					}
				}
			}

			// unpackaged apps can't load files using packaged schemes
			if (!AppInfoUtils.IsPackagedApp)
			{
				// the Uri type encodes the fragment, so let's remove first
				var fragment = "";
				if (fontSource.IndexOf('#', StringComparison.OrdinalIgnoreCase) is int index && index >= 0)
				{
					fragment = fontSource.Substring(index);
					fontSource = fontSource.Substring(0, index);
				}

				var fontUri = new Uri(fontSource, UriKind.RelativeOrAbsolute);

				var path = fontUri.AbsolutePath.TrimStart('/');
				if (FileSystemUtils.TryGetAppPackageFileUri(path, out var uri))
				{
					fontSource = uri + fragment;
				}
			}

			return fontSource;
		}
	}
}