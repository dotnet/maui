using System.Threading;
using System.Threading.Tasks;
using Android.Widget;

namespace Microsoft.Maui
{
	public interface IApplyImageSourceService
	{
		Task ApplyDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default);
	}
}