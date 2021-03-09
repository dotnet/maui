using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public sealed class StreamImageSourceHandler : IImageSourceHandler
	{
		public async Task<Microsoft.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = new CancellationToken())
		{
			BitmapImage bitmapimage = null;

			var streamsource = imagesource as StreamImageSource;
			if (streamsource != null && streamsource.Stream != null)
			{
				using (Stream stream = await ((IStreamImageSource)streamsource).GetStreamAsync(cancellationToken))
				{
					if (stream == null)
						return null;
					bitmapimage = new BitmapImage();
					await bitmapimage.SetSourceAsync(stream.AsRandomAccessStream());
				}
			}

			return bitmapimage;
		}
	}
}