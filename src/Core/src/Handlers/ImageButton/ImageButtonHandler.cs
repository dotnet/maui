#if __IOS__ || MACCATALYST
using NativeImage = UIKit.UIImage;
using NativeImageView = UIKit.UIImageView;
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using NativeImage = Android.Graphics.Drawables.Drawable;
using NativeImageView = Android.Widget.ImageView;
using PlatformView = Google.Android.Material.ImageView.ShapeableImageView;
#elif WINDOWS
using System;
using NativeImage = Microsoft.UI.Xaml.Media.ImageSource;
using NativeImageView = Microsoft.UI.Xaml.Controls.Image;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeImage = System.Object;
using NativeImageView = System.Object;
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : IImageButtonHandler
	{
		public static IPropertyMapper<IImage, IImageHandler> ImageMapper = new PropertyMapper<IImage, IImageHandler>(ImageHandler.Mapper);

		public static IPropertyMapper<IImageButton, IImageButtonHandler> Mapper = new PropertyMapper<IImageButton, IImageButtonHandler>(ImageMapper)
		{
			[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
			[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
#if WINDOWS
			[nameof(IImageButton.Background)] = MapBackground,
#endif
		};

		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);

		public ImageButtonHandler() : base(Mapper)
		{
		}

		public ImageButtonHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{
		}

		IImageButton IImageButtonHandler.TypedVirtualView => VirtualView;

		IImage IImageHandler.TypedVirtualView => VirtualView;

		NativeImageView IImageHandler.TypedPlatformView =>
#if __IOS__
			PlatformView.ImageView;
#elif WINDOWS
			PlatformView.GetContent<NativeImageView>() ?? throw new InvalidOperationException("ImageButton did not contain an Image element.");
#else
			PlatformView;
#endif
		ImageSourcePartLoader IImageHandler.SourceLoader => SourceLoader;
	}
}