using System;
using System.Threading;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler
	{
		readonly SourceManager _sourceManager = new SourceManager();

		public static PropertyMapper<IImage, ImageHandler> ImageMapper = new PropertyMapper<IImage, ImageHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IImage.Aspect)] = MapAspect,
			[nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
			[nameof(IImage.Source)] = MapSource,
		};

		public ImageHandler() : base(ImageMapper)
		{
		}

		public ImageHandler(PropertyMapper mapper) : base(mapper ?? ImageMapper)
		{
		}

		class SourceManager
		{
			CancellationTokenSource? _sourceCancellation;
			IDisposable? _sourceResult;

			public CancellationToken BeginLoad()
			{
				_sourceResult?.Dispose();

				_sourceCancellation?.Cancel();
				_sourceCancellation = new CancellationTokenSource();

				return Token;
			}

			public CancellationToken Token =>
				_sourceCancellation?.Token ?? default;

			public bool IsResolutionDependent { get; private set; }

			public void CompleteLoad<T>(IImageSourceServiceResult<T>? result)
			{
				CompleteLoad((IDisposable?)result);

				IsResolutionDependent = result?.IsResolutionDependent ?? false;
			}

			public void CompleteLoad(IDisposable? result)
			{
				_sourceResult = result;
				_sourceCancellation?.Dispose();
				_sourceCancellation = null;
				IsResolutionDependent = false;
			}
		}
	}
}