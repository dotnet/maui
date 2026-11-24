using System;
using System.Threading;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIImageView;
#elif MONOANDROID
using PlatformView = Android.Widget.ImageView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Image;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : IImageHandler
	{
		public static IPropertyMapper<IImage, IImageHandler> Mapper = new PropertyMapper<IImage, IImageHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS
			[nameof(IImage.Height)] = MapHeight,
			[nameof(IImage.Width)] = MapWidth,
#endif
			[nameof(IImage.Aspect)] = MapAspect,
			[nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
#if __ANDROID__ || __IOS__ || MACCATALYST || WINDOWS
			[nameof(IImage.Background)] = MapBackground,
#endif
			[nameof(IImage.Source)] = MapSource,
		};

		internal const string ReloadCommand = "Reload";

		public static CommandMapper<IImage, IImageHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
			[ReloadCommand] = MapReload,
		};

		ImageSourcePartLoader? _imageSourcePartLoader;

		public virtual ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(new ImageImageSourcePartSetter(this));

		public ImageHandler() : base(Mapper, CommandMapper)
		{
		}

		public ImageHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ImageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}


		/// <summary>
		/// Loads the image source again, for example after it failed to load.
		/// </summary>
		/// <param name="handler">The handler of the image.</param>
		/// <param name="image">The image to reload.</param>
		/// <param name="args">Not used.</param>
		public static void MapReload(IImageHandler handler, IImage image, object? args) =>
			handler.UpdateValue(nameof(IImage.Source));

		// TODO MAUI: Should we remove all shadowing? 
		IImage IImageHandler.VirtualView => VirtualView;

		PlatformView IImageHandler.PlatformView => PlatformView;

		partial class ImageImageSourcePartSetter : ImageSourcePartSetter<IImageHandler>
		{
			public ImageImageSourcePartSetter(IImageHandler handler)
				: base(handler)
			{
			}
		}
	}
}
