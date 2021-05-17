#nullable enable
using System;
using System.Threading;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler
	{
		readonly ImageSourceServiceResultManager _sourceManager = new ImageSourceServiceResultManager();

		public static PropertyMapper<IImage, ImageHandler> ImageMapper = new PropertyMapper<IImage, ImageHandler>(ViewHandler.ViewMapper)
		{
			// TODO: Map Background and use the ContainerView (when available)
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
	}
}