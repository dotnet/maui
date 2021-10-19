#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIButton;
#elif MONOANDROID
using NativeView = Google.Android.Material.Button.MaterialButton;
#elif WINDOWS
using NativeView = Microsoft.Maui.MauiButton;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : IButtonHandler
	{
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView?.ImageSource, OnSetImageSource);

		public static IPropertyMapper<ITextStyle, IButtonHandler> TextStyleMapper = new PropertyMapper<ITextStyle, IButtonHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
		};

		public static IPropertyMapper<IText, IButtonHandler> TextMapper = new PropertyMapper<IText, IButtonHandler>(TextStyleMapper)
		{
			[nameof(IText.Text)] = MapText,
		};

		public static IPropertyMapper<IButton, IButtonHandler> Mapper = new PropertyMapper<IButton, IButtonHandler>(TextMapper)
		{
			[nameof(IButton.Background)] = MapBackground,
			[nameof(IButton.Padding)] = MapPadding,
			[nameof(IButton.ImageSource)] = MapImageSource
		};

		public ButtonHandler() : base(Mapper)
		{

		}

		public ButtonHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		IButton IButtonHandler.TypedVirtualView => VirtualView;

		NativeView IButtonHandler.TypedNativeView => NativeView;
	}
}
