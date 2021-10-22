using System;
using System.Collections.Generic;
using System.Text;
#if __IOS__ || MACCATALYST
using NativeImage = UIKit.UIImage;
using NativeView = UIKit.UIButton;
#elif MONOANDROID
using NativeImage = Android.Graphics.Drawables.Drawable;
using NativeView = AndroidX.AppCompat.Widget.AppCompatImageButton;
#elif WINDOWS
using NativeImage = Microsoft.UI.Xaml.Media.ImageSource;
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
using NativeImage = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : IImageButtonHandler
	{
		public static IPropertyMapper<IImage, IImageHandler> ImageMapper = new PropertyMapper<IImage, IImageHandler>(ImageHandler.Mapper);
		public static IPropertyMapper<IImageButton, IImageButtonHandler> Mapper = new PropertyMapper<IImageButton, IImageButtonHandler>(ImageMapper);

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

#if __IOS__
		UIKit.UIImageView IImageHandler.TypedNativeView => NativeView.ImageView;
#elif WINDOWS
		UI.Xaml.Controls.Image IImageHandler.TypedNativeView => NativeView.GetImage() ?? (UI.Xaml.Controls.Image)NativeView.Content;
#elif __ANDROID__
		Android.Widget.ImageView IImageHandler.TypedNativeView => NativeView;
#else
		object IImageHandler.TypedNativeView => NativeView;
#endif
		ImageSourcePartLoader IImageHandler.SourceLoader => SourceLoader;
	}
}
