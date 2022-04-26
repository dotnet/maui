using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, MauiImageButton>
	{
		protected override MauiImageButton CreatePlatformView()
		{
			return new MauiImageButton
			{
				Focusable = true,
			};
		}

		protected override void ConnectHandler(MauiImageButton platformView)
		{
			platformView.Clicked += OnClicked;
			platformView.Pressed += OnPressed;
			platformView.Released += OnReleased;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiImageButton platformView)
		{
			platformView.Clicked -= OnClicked;
			platformView.Pressed -= OnPressed;
			platformView.Released -= OnReleased;
			base.DisconnectHandler(platformView);
		}

		[MissingMapper]
		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapPadding(IImageButtonHandler handler, IImageButton imageButton) { }

		void OnReleased(object? sender, EventArgs e)
		{
			VirtualView?.Released();
		}

		void OnPressed(object? sender, EventArgs e)
		{
			VirtualView?.Pressed();
		}

		void OnClicked(object? sender, EventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnSetImageSource(MauiImageSource? img)
		{
			if (img == null)
				return;

			PlatformView.ResourceUrl = img.ResourceUrl;
		}
	}
}
