#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.Button.MaterialButton;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Button;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler2 : IButtonHandler2
	{
#if WINDOWS
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => (VirtualView as IImageButton), OnSetImageSource);

		public static IPropertyMapper<IImage, IButtonHandler2> ImageButtonMapper = new PropertyMapper<IImage, IButtonHandler2>()
		{
			[nameof(IImage.Source)] = MapImageSource
		};

		public static IPropertyMapper<ITextButton, IButtonHandler2> TextButtonMapper = new PropertyMapper<ITextButton, IButtonHandler2>()
		{
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(IText.Text)] = MapText
		};

		public static IPropertyMapper<IButton, IButtonHandler2> Mapper = new PropertyMapper<IButton, IButtonHandler2>(TextButtonMapper, ImageButtonMapper, ViewHandler.ViewMapper)
		{
			[nameof(IButton.Background)] = MapBackground,
			[nameof(IButton.Padding)] = MapPadding,
			[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
			[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius
		};

		public static CommandMapper<IButton, IButtonHandler2> CommandMapper = new(ViewCommandMapper);
#endif

		public ButtonHandler2() : base(Mapper, CommandMapper)
		{

		}

		public ButtonHandler2(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ButtonHandler2(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

#if WINDOWS
		IButton IButtonHandler2.VirtualView => VirtualView;

		PlatformView IButtonHandler2.PlatformView => PlatformView;
#else
		IButton IButtonHandler.VirtualView => VirtualView;

		PlatformView IButtonHandler.PlatformView => PlatformView;
#endif
	}
}