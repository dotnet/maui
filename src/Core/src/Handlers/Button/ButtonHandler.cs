#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.Button.MaterialButton;
#elif WINDOWS
using System;
using Microsoft.UI.Xaml.Media;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Button;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : IButtonHandler
	{
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => (VirtualView as IImageButton), OnSetImageSource);


#if WINDOWS
		// Once we did all the mappers it would just be this line of code here
		public static IPropertyMapper<IButton, IButtonHandler> Mapper = ButtonHandlerNet8.Mapper;
		public static IPropertyMapper<IImage, IButtonHandler> ImageButtonMapper = ButtonHandlerNet8.ImageButtonMapper;
		public static IPropertyMapper<ITextButton, IButtonHandler> TextButtonMapper = ButtonHandlerNet8.TextButtonMapper;
#else
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

#endif

		public static CommandMapper<IButton, IButtonHandler> CommandMapper = new(ViewCommandMapper);

		partial void InitHandler();

		public ButtonHandler() : base(Mapper, CommandMapper)
		{
			InitHandler();
		}

		public ButtonHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
			InitHandler();
		}

		public ButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
			InitHandler();
		}

		IButton IButtonHandler.VirtualView => VirtualView;

		PlatformView IButtonHandler.PlatformView => PlatformView;


		static bool UseNet7 { get; set; }
		internal static void SetupForNet7()
		{
#if WINDOWS
			UseNet7 = true;
			Mapper = ButtonHandlerNET7.Mapper;
			ImageButtonMapper = ButtonHandlerNET7.ImageButtonMapper;
			TextButtonMapper = ButtonHandlerNET7.TextButtonMapper;
#endif
		}
	}
}