using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, MauiImageButton>
	{
		protected override MauiImageButton CreatePlatformView() => new();

		[MissingMapper]
		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapPadding(IImageButtonHandler handler, IImageButton imageButton) { }

		partial class ImageButtonImageSourcePartSetter
		{
			public override void SetImageSource(Gdk.Pixbuf? platformImage)
			{
				if (Handler?.PlatformView is not MauiImageButton button)
					return;

				var imageView = button.ImageView ?? new ();
				imageView.Image = platformImage;
				button.ImageView = imageView;
			}
		}
	}
}