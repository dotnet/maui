using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class FontImageSourceHandler : IImageSourceHandler
	{
		float _minimumDpi = 300;

		public Task<Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken))
		{
			if (!(imagesource is FontImageSource fontsource))
				return null;

			var device = CanvasDevice.GetSharedDevice();
			var dpi = Math.Max(_minimumDpi, Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi);
			var canvasSize = (float)fontsource.Size + 2;

			var imageSource = new CanvasImageSource(device, canvasSize, canvasSize, dpi);
			using (var ds = imageSource.CreateDrawingSession(Windows.UI.Colors.Transparent))
			{
				var textFormat = new CanvasTextFormat
				{
					FontFamily = fontsource.FontFamily,
					FontSize = (float)fontsource.Size,
					HorizontalAlignment = CanvasHorizontalAlignment.Center,
					Options = CanvasDrawTextOptions.Default,
				};
				var iconcolor = (fontsource.Color != Color.Default ? fontsource.Color : Color.White).ToWindowsColor();
				ds.DrawText(fontsource.Glyph, textFormat.FontSize / 2, 0, iconcolor, textFormat);
			}

			return Task.FromResult((Windows.UI.Xaml.Media.ImageSource)imageSource);
		}
	}
}
