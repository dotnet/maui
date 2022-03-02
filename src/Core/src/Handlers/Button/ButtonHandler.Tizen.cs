using System;
using System.Threading.Tasks;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		protected override Button CreatePlatformView() => new Button();

		protected override void ConnectHandler(Button platformView)
		{
			platformView.TouchEvent += OnTouch;
			platformView.Clicked += OnButtonClicked;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.TouchEvent -= OnTouch;
			nativeView.Clicked -= OnButtonClicked;
			base.DisconnectHandler(nativeView);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button);
		}

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(IButtonHandler handler, IImage image)
		{
			if (image.Source == null)
			{
				return Task.CompletedTask;
			}
			return handler.ImageSourceLoader.UpdateImageSourceAsync();
		}

		public static void MapImageSource(IButtonHandler handler, IImageButton image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(IButtonHandler handler, IImageButton image)
		{
			if (image.Source == null)
			{
				return Task.CompletedTask;
			}
			return handler.ImageSourceLoader.UpdateImageSourceAsync();
		}

		[MissingMapper]
		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button) { }

		[MissingMapper]
		public static void MapFont(IButtonHandler handler, ITextStyle button) { }

		[MissingMapper]
		public static void MapPadding(IButtonHandler handler, IButton button) { }

		[MissingMapper]
		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke) { }

		bool OnTouch(object source, View.TouchEventArgs e)
		{
			var state = e.Touch.GetState(0);

			if (state == Tizen.NUI.PointStateType.Down)
			{
				OnButtonPressed(source, e);
			}
			else if (state == Tizen.NUI.PointStateType.Up)
			{
				OnButtonReleased(source, e);
			}
			return false;
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnButtonReleased(object? sender, EventArgs e)
		{
			VirtualView?.Released();
		}

		void OnButtonPressed(object? sender, EventArgs e)
		{
			VirtualView?.Pressed();
		}

		void OnSetImageSource(ImageView? image)
		{
			if (image == null)
				return;
			PlatformView.Icon.ResourceUrl = image.ResourceUrl;
			image.Dispose();
		}
	}
}
