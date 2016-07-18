using System;
using System.Threading;
using System.Threading.Tasks;

using WImageSource = Windows.UI.Xaml.Media.ImageSource;

#if WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal
#else
using Xamarin.Forms.Platform.WinRT;

namespace Xamarin.Forms.ControlGallery.WinRT
#endif
{
	public sealed class BrokenImageSourceHandler : IImageSourceHandler
	{
		public Task<WImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = new CancellationToken())
		{
			throw new Exception("Fail");
		}
	}
}