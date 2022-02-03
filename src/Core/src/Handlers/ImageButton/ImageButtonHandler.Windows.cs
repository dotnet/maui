using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, Button>
	{
		PointerEventHandler? _pointerPressedHandler;

		protected override Button CreatePlatformView() =>
			new Button
			{
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Content = new Image
				{
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					Stretch = Stretch.Uniform,
				}
			};

		protected override void ConnectHandler(Button nativeView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);

			nativeView.Click += OnClick;
			nativeView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);

			_pointerPressedHandler = null;

			base.DisconnectHandler(nativeView);

			SourceLoader.Reset();
		}

		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as Button)?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as Button)?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as Button)?.UpdateCornerRadius(buttonStroke);
    }

		public static void MapBackground(IImageButtonHandler handler, IImageButton imageButton)
		{
			(handler.PlatformView as Button)?.UpdateBackground(imageButton);
		}

		void OnSetImageSource(ImageSource? nativeImageSource)
		{
			PlatformView.UpdateImageSource(nativeImageSource);
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
	}
}