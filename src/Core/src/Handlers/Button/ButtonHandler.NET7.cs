#if WINDOWS
using System;
using Microsoft.UI.Xaml;
using PlatformView = Microsoft.UI.Xaml.Controls.Button;

namespace Microsoft.Maui.Handlers
{
	partial class ButtonHandlerNET7 : ViewHandlerProxy<IButton, PlatformView>, IButtonHandlerNET7
	{
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => (VirtualView as IImageButton), OnSetImageSource);

		public static IPropertyMapper<IImage, IButtonHandler> ImageButtonMapper = new PropertyMapper<IImage, IButtonHandler>()
		{
			[nameof(IImage.Source)] = MapImageSource
		};

		public static IPropertyMapper<ITextButton, IButtonHandler> TextButtonMapper = new PropertyMapper<ITextButton, IButtonHandler>()
		{
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(IText.Text)] = MapText
		};

		public static IPropertyMapper<IButton, IButtonHandler> Mapper = new PropertyMapper<IButton, IButtonHandler>(TextButtonMapper, ImageButtonMapper, ViewHandler.ViewMapper)
		{
			[nameof(IButton.Background)] = MapBackground,
			[nameof(IButton.Padding)] = MapPadding,
			[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
			[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius
		};

		public static CommandMapper<IButton, IButtonHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		public ButtonHandlerNET7()
		{

		}

		IButton IButtonHandler.VirtualView => VirtualView;

		PlatformView IButtonHandlerNET7.PlatformView => PlatformView;

		FrameworkElement IButtonHandler.PlatformView => PlatformView;
	}
}
#endif