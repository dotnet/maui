using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	partial class ButtonHandlerNET7 : ViewHandlerProxy<IButton, Button>
	{
		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;

		new Button PlatformView => (Button)base.PlatformView!;
		new IButton VirtualView => (IButton)base.VirtualView!;

		protected override Button CreatePlatformViewCore() => new MauiButton();

		public override void ConnectHandler(FrameworkElement platformView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			((Button)platformView).Click += OnClick;
			platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			platformView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);

			base.ConnectHandler(platformView);
		}

		public override void DisconnectHandler(FrameworkElement platformView)
		{
			((Button)platformView).Click -= OnClick;
			platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			platformView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;

			base.DisconnectHandler(platformView);
		}


		static Button GetPlatformView(IButtonHandler handler) =>
			(Button)handler.PlatformView;

		// This is a Windows-specific mapping
		public static void MapBackground(IButtonHandler handler, IButton button)
		{
			GetPlatformView(handler).UpdateBackground(button);
		}

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			GetPlatformView(handler).UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			GetPlatformView(handler).UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			GetPlatformView(handler).UpdateCornerRadius(buttonStroke);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			GetPlatformView(handler).UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			GetPlatformView(handler).UpdateTextColor(button);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			GetPlatformView(handler).UpdateCharacterSpacing(button);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			GetPlatformView(handler).UpdateFont(button, fontManager);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			GetPlatformView(handler).UpdatePadding(button);
		}

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			handler
				.ImageSourceLoader
				.UpdateImageSourceAsync()
				.FireAndForget(handler);

		void OnSetImageSource(ImageSource? platformImageSource)
		{
			PlatformView.UpdateImageSource(platformImageSource);
		}

		void OnClick(object sender, RoutedEventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			VirtualView?.Pressed();
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			VirtualView?.Released();
		}
	}
}