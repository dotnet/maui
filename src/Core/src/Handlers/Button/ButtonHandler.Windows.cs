#nullable enable
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

		protected override Button CreateNativeView() =>
			new Button
			{
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Content = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Orientation = Orientation.Horizontal,
					Margin = WinUIHelpers.CreateThickness(0),
					Children =
					{
						new Image
						{
							VerticalAlignment = VerticalAlignment.Center,
							HorizontalAlignment = HorizontalAlignment.Center,
							Stretch = Stretch.Uniform,
							Margin = WinUIHelpers.CreateThickness(0),
							Visibility = UI.Xaml.Visibility.Collapsed,
						},
						new TextBlock
						{
							VerticalAlignment = VerticalAlignment.Center,
							HorizontalAlignment = HorizontalAlignment.Center,
							Margin = WinUIHelpers.CreateThickness(0),
							Visibility = UI.Xaml.Visibility.Collapsed,
						}
					}
				}
			};

		protected override void ConnectHandler(Button nativeView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			nativeView.Click += OnClick;
			nativeView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			nativeView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			nativeView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;

			base.DisconnectHandler(nativeView);
		}

		// This is a Windows-specific mapping
		public static void MapBackground(IButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateBackground(button);
		}

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.NativeView?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.NativeView?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.NativeView?.UpdateCornerRadius(buttonStroke);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.NativeView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.NativeView?.UpdateTextColor(button);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.NativeView?.UpdateCharacterSpacing(button);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdatePadding(button);
		}

		public static void MapImageSource(IButtonHandler handler, IButton image) =>
			handler
				.ImageSourceLoader
				.UpdateImageSourceAsync()
				.FireAndForget(handler);

		void OnSetImageSource(ImageSource? nativeImageSource)
		{
			NativeView.UpdateImageSource(nativeImageSource);
		}

		void OnClick(object sender, RoutedEventArgs e)
		{
			VirtualView?.Clicked();
			VirtualView?.Released();
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