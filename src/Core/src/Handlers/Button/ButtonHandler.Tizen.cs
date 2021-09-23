using System;
using System.Threading.Tasks;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		protected override Button CreateNativeView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			return new Button(NativeParent);
		}

		protected override void ConnectHandler(Button nativeView)
		{
			nativeView.Released += OnButtonClicked;
			nativeView.Clicked += OnButtonReleased;
			nativeView.Pressed += OnButtonPressed;
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.Released -= OnButtonClicked;
			nativeView.Clicked -= OnButtonReleased;
			nativeView.Pressed -= OnButtonPressed;
			base.DisconnectHandler(nativeView);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.TypedNativeView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.TypedNativeView?.UpdateTextColor(button);
		}

		public static void MapImageSource(IButtonHandler handler, IButton image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(IButtonHandler handler, IButton image)
		{
			if (image.ImageSource == null)
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
			NativeView.Image = image;
			VirtualView.ImageSourceLoaded();
		}
	}
}