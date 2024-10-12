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

		public override bool NeedsContainer => false;

		protected override void ConnectHandler(MauiImageButton platformView)
		{
			platformView.Clicked += OnClicked;
			platformView.Pressed += OnPressed;
			platformView.Released += OnReleased;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiImageButton platformView)
		{
			if (!platformView.HasBody())
				return;

			platformView.Clicked -= OnClicked;
			platformView.Pressed -= OnPressed;
			platformView.Released -= OnReleased;
			base.DisconnectHandler(platformView);
		}

		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView.UpdateStrokeThickness(buttonStroke);
			handler.UpdateValue(nameof(IImageButton.Padding));
		}

		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView.UpdateCornerRadius(buttonStroke);
			handler.UpdateValue(nameof(IImageButton.Padding));
		}

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

		partial class ImageButtonImageSourcePartSetter
		{
			public override void SetImageSource(MauiImageSource? platformImage)
			{
				if (Handler?.PlatformView is not MauiImageButton button)
					return;

				if (platformImage is null)
					return;

				button.ResourceUrl = platformImage.ResourceUrl;
			}
		}
	}
}
