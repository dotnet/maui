using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.UWP
{
	public interface IImageSourceHandler : IRegisterable
	{
		Task<Microsoft.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken));
	}
}