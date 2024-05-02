using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, Button>
	{
		Image? _image;

		PointerEventHandler? _pointerPressedHandler;

		protected override Button CreatePlatformView()
		{
			_image = new Image
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Stretch = Stretch.Uniform,
			};

			var platformImageButton = new Button
			{
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Content = _image
			};

			return platformImageButton;
		}

		protected override void ConnectHandler(Button platformView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);

			if (_image != null)
			{
				_image.ImageOpened += OnImageOpened;
				_image.ImageFailed += OnImageFailed;
			}

			platformView.Click += OnClick;
			platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Button platformView)
		{
			if (_image != null)
			{
				_image.ImageOpened -= OnImageOpened;
				_image.ImageFailed -= OnImageFailed;
			}

			platformView.Click -= OnClick;
			platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);

			_pointerPressedHandler = null;

			base.DisconnectHandler(platformView);

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

		public static void MapPadding(IImageButtonHandler handler, IImageButton imageButton)
		{
			(handler.PlatformView as Button)?.UpdatePadding(imageButton);
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

		void OnImageOpened(object sender, RoutedEventArgs routedEventArgs)
		{
			VirtualView?.UpdateIsLoading(false);
		}

		protected virtual void OnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
		{
			MauiContext?.CreateLogger<ImageButtonHandler>()?.LogWarning("Image failed to load: {exceptionRoutedEventArgs.ErrorMessage}", exceptionRoutedEventArgs.ErrorMessage);
			VirtualView?.UpdateIsLoading(false);
		}

		partial class ImageButtonImageSourcePartSetter
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