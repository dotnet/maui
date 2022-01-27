using System;
using TImage = Tizen.UIExtensions.ElmSharp.Image;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, MauiImageButton>
	{
		protected override MauiImageButton CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ImageButton");
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");

			var view = new MauiImageButton(NativeParent);
			return view;
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

		[MissingMapper]
		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		private void OnReleased(object? sender, EventArgs e)
		{
			VirtualView?.Released();
		}

		private void OnPressed(object? sender, EventArgs e)
		{
			VirtualView?.Pressed();
		}

		private void OnClicked(object? sender, EventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnSetImageSource(TImage? img)
		{
			//Empty on purpose
		}
	}
}
