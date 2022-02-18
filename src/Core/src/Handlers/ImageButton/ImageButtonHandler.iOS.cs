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

		void OnSetImageSource(UIImage? obj)
		{
			PlatformView.SetImage(obj?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), UIControlState.Normal);
			PlatformView.HorizontalAlignment = UIControlContentHorizontalAlignment.Fill;
			PlatformView.VerticalAlignment = UIControlContentVerticalAlignment.Fill;
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
			(handler.PlatformView as UIButton)?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as UIButton)?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as UIButton)?.UpdateCornerRadius(buttonStroke);
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
	}
}
