using System;
using System.Threading.Tasks;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		protected override Button CreatePlatformView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			return new Button(NativeParent);
		}

		protected override void ConnectHandler(Button platformView)
		{
			platformView.Released += OnButtonReleased;
			platformView.Clicked += OnButtonClicked;
			platformView.Pressed += OnButtonPressed;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Button platformView)
		{
			platformView.Released -= OnButtonReleased;
			platformView.Clicked -= OnButtonClicked;
			platformView.Pressed -= OnButtonPressed;
			base.DisconnectHandler(platformView);
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

		[MissingMapper]
		public static void MapLineBreakMode(IButtonHandler handler, IButton button) { }

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

		void OnSetImageSource(Image? image)
		{
			PlatformView.Image = image;
		}
	}
}