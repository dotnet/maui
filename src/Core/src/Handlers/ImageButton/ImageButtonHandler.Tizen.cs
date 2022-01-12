using System;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, MauiImageButton>
	{
		protected override MauiImageButton CreatePlatformView()
		{
			return new MauiImageButton();
		}

		protected override void ConnectHandler(MauiImageButton nativeView)
		{
			nativeView.Clicked += OnClicked;
			nativeView.Pressed += OnPressed;
			nativeView.Released += OnReleased;
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiImageButton nativeView)
		{
			nativeView.Clicked -= OnClicked;
			nativeView.Pressed -= OnPressed;
			nativeView.Released -= OnReleased;
			base.DisconnectHandler(nativeView);
		}

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

		void OnSetImageSource(Image? img)
		{
			//Empty on purpose
		}
	}
}
