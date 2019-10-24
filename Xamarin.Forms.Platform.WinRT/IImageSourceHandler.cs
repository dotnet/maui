using System.Threading;
using System.Threading.Tasks;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public interface IImageSourceHandler : IRegisterable
	{
		Task<Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken));
	}
}