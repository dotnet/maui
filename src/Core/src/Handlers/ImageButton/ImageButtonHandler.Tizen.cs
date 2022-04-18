using System;
using TImage = Tizen.UIExtensions.ElmSharp.Image;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, MauiImageButton>
	{
		protected override MauiImageButton CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ImageButton");
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");

			var view = new MauiImageButton(NativeParent);
			return view;
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
