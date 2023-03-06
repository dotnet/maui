using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;

namespace Microsoft.Maui.Handlers
{
	class ButtonHandlerNet8 : ViewHandlerProxy<IButton, FrameworkElement>, IButtonHandlerNET8
	{
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => (VirtualView as IImageButton), OnSetImageSource);

		PlatformView IButtonHandlerNET8.PlatformView => (FrameworkElement)PlatformView!;

		IButton IButtonHandler.VirtualView => (IButton)VirtualView!;

		PlatformView IButtonHandler.PlatformView => (FrameworkElement)PlatformView!;

		void OnSetImageSource(ImageSource? obj)
		{
		}

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

		public static CommandMapper<IButton, IButtonHandler> CommandMapper = new(ViewHandler.ViewCommandMapper);

		public ButtonHandlerNet8()
		{

		}

		protected override FrameworkElement CreatePlatformViewCore() => new StackPanel()
		{
			Children = {
				new Microsoft.UI.Xaml.Controls.TextBlock()
				{
					Text = "I'm a button"
				}
			}
		};

		public override void ConnectHandler(FrameworkElement platformView)
		{
			base.ConnectHandler(platformView);
		}

		public override void DisconnectHandler(FrameworkElement platformView)
		{
			base.DisconnectHandler(platformView);
		}

		// This is a Windows-specific mapping
		static void MapBackground(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateBackground(button);
		}

		static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
		}

		static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
		}

		static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
		}

		static void MapText(IButtonHandler handler, IText button)
		{
			((handler.PlatformView as StackPanel)!
				.Children[0] as TextBlock)!.Text = button.Text;
		}

		static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
		}

		static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
		}

		static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
		}

		static void MapPadding(IButtonHandler handler, IButton button)
		{
		}

		static void MapImageSource(IButtonHandler handler, IImage image) =>
			handler
				.ImageSourceLoader
				.UpdateImageSourceAsync()
				.FireAndForget(handler);


		//void OnClick(object sender, RoutedEventArgs e)
		//{
		//	VirtualView?.Clicked();
		//}

		//void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		//{
		//	VirtualView?.Pressed();
		//}

		//void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		//{
		//	VirtualView?.Released();
		//}
	}
}