#if __IOS__ || MACCATALYST
using PlatformImage = UIKit.UIImage;
using PlatformImageView = UIKit.UIImageView;
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
using PlatformImageView = Android.Widget.ImageView;
using PlatformView = Google.Android.Material.ImageView.ShapeableImageView;
#elif WINDOWS
using System;
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
using PlatformImageView = Microsoft.UI.Xaml.Controls.Image;
using PlatformView = Microsoft.UI.Xaml.Controls.Button;
#elif TIZEN
using PlatformImage = Microsoft.Maui.Platform.MauiImageSource;
using PlatformImageView = Tizen.UIExtensions.NUI.Image;
using PlatformView = Microsoft.Maui.Platform.MauiImageButton;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformImage = System.Object;
using PlatformImageView = System.Object;
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : IImageButtonHandler, IImageSourcePartSetter
	{
		public static IPropertyMapper<IImage, IImageHandler> ImageMapper = new PropertyMapper<IImage, IImageHandler>(ImageHandler.Mapper);

		public static IPropertyMapper<IImageButton, IImageButtonHandler> Mapper = new PropertyMapper<IImageButton, IImageButtonHandler>(ImageMapper)
		{
			[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
			[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
			[nameof(IImageButton.Padding)] = MapPadding,
#if ANDROID || WINDOWS
			[nameof(IImageButton.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<IImageButton, IImageButtonHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader SourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this);

		public ImageButtonHandler() : base(Mapper, CommandMapper)
		{
		}

		public ImageButtonHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ImageButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IImageButton IImageButtonHandler.VirtualView => VirtualView;

		IImage IImageHandler.VirtualView => VirtualView;

		PlatformImageView IImageHandler.PlatformView =>
#if __IOS__
			PlatformView.ImageView;
#elif WINDOWS
			PlatformView.GetContent<PlatformImageView>() ?? throw new InvalidOperationException("ImageButton did not contain an Image element.");
#else
			PlatformView;
#endif

		PlatformView IImageButtonHandler.PlatformView => PlatformView;

		ImageSourcePartLoader IImageHandler.SourceLoader => SourceLoader;
	}
}
