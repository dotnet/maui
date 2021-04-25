using System.Threading;
using System.Threading.Tasks;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFontImageSource fontImageSource)
				return GetImageSourceAsync(fontImageSource, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<WImageSource>?>(null);
		}

		public Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IFontImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			return FromResult(null);
		}

		static Task<IImageSourceServiceResult<WImageSource>?> FromResult(IImageSourceServiceResult<WImageSource>? result) =>
			Task.FromResult(result);
	}
}