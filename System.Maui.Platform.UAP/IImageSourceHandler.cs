using System.Threading;
using System.Threading.Tasks;

namespace System.Maui.Platform.UWP
{
	public interface IImageSourceHandler : IRegisterable
	{
		Task<global::Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken));
	}
}