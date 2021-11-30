using System;
using System.Threading;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIImageView;
#elif MONOANDROID
using NativeView = Android.Widget.ImageView;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Controls.Image;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : IImageHandler
	{
		public static IPropertyMapper<IImage, IImageHandler> Mapper = new PropertyMapper<IImage, IImageHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__ || WINDOWS
			[nameof(IImage.Background)] = MapBackground,
#endif
			[nameof(IImage.Aspect)] = MapAspect,
			[nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
			[nameof(IImage.Source)] = MapSource,
		};

		public static CommandMapper<IImage, IImageHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);

		public ImageHandler() : base(Mapper)
		{
		}

		public ImageHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{
		}


		// TODO MAUI: Should we remove all shadowing? 
		IImage IImageHandler.TypedVirtualView => VirtualView;

		NativeView IImageHandler.TypedNativeView => NativeView;
	}
}