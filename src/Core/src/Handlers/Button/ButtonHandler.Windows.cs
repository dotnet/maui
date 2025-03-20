using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;
		bool _isPressed;

		protected override Button CreatePlatformView() => new MauiButton();

		protected override void ConnectHandler(Button platformView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			platformView.Click += OnClick;
			platformView.Unloaded += OnUnloaded;
			platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			platformView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);

			base.ConnectHandler(platformView);
		}


		protected override void DisconnectHandler(Button platformView)
		{
			platformView.Click -= OnClick;
			platformView.Unloaded -= OnUnloaded;
			platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			platformView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;

			base.DisconnectHandler(platformView);
		}

		// This is a Windows-specific mapping
		public static void MapBackground(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateBackground(button);
		}

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateCornerRadius(buttonStroke);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateCharacterSpacing(button);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdatePadding(button);
		}

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			handler
				.ImageSourceLoader
				.UpdateImageSourceAsync()
				.FireAndForget(handler);

		void OnClick(object sender, RoutedEventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			_isPressed = true;
			VirtualView?.Pressed();
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			_isPressed = false;
			VirtualView?.Released();
		}

		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			// WinUI will not raise the PointerReleased event if the pointer is pressed and then unloaded
			if (_isPressed)
			{
				VirtualView?.Released();
			}
		}

		partial class ButtonImageSourcePartSetter
		{
			public override void SetImageSource(ImageSource? platformImage)
			{
				if (Handler?.PlatformView is not Button button)
					return;

				button.UpdateImageSource(platformImage);
			}
		}
	}
}