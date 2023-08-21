using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, UIButton>
	{
		protected override UIButton CreatePlatformView()
		{
			var platformView = new UIButton(UIButtonType.System)
			{
				ClipsToBounds = true
			};

			return platformView;
		}

		protected override void ConnectHandler(UIButton platformView)
		{
			platformView.TouchUpInside += OnButtonTouchUpInside;
			platformView.TouchUpOutside += OnButtonTouchUpOutside;
			platformView.TouchDown += OnButtonTouchDown;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			platformView.TouchUpInside -= OnButtonTouchUpInside;
			platformView.TouchUpOutside -= OnButtonTouchUpOutside;
			platformView.TouchDown -= OnButtonTouchDown;

			base.DisconnectHandler(platformView);

			SourceLoader.Reset();
		}

		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateCornerRadius(buttonStroke);
		}

		public static void MapPadding(IImageButtonHandler handler, IImageButton imageButton)
		{
			(handler.PlatformView as UIButton)?.UpdatePadding(imageButton);
		}

		void OnButtonTouchUpInside(object? sender, EventArgs e)
		{
			VirtualView?.Released();
			VirtualView?.Clicked();
		}

		void OnButtonTouchUpOutside(object? sender, EventArgs e)
		{
			VirtualView?.Released();
		}

		void OnButtonTouchDown(object? sender, EventArgs e)
		{
			VirtualView?.Pressed();
		}

		partial class ImageButtonImageSourcePartSetter
		{
			public override void SetImageSource(UIImage? platformImage)
			{
				if (Handler?.PlatformView is not UIButton button)
					return;

				platformImage = platformImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				button.SetImage(platformImage, UIControlState.Normal);
				button.HorizontalAlignment = UIControlContentHorizontalAlignment.Fill;
				button.VerticalAlignment = UIControlContentVerticalAlignment.Fill;
			}
		}
	}
}
