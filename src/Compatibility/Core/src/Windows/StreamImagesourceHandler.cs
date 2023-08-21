//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
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