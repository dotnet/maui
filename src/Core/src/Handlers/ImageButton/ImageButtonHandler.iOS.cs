using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, UIButton>
	{
		readonly ImageButtonProxy _proxy = new();

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
			_proxy.Connect(VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			_proxy.Disconnect(platformView);

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

		partial class ImageButtonImageSourcePartSetter
		{
			public override void SetImageSource(UIImage? platformImage)
			{
				if (Handler?.PlatformView is not UIButton button)
					return;

				if (platformImage?.Images is not null && platformImage.Images.Length > 0)
					platformImage = platformImage.Images[0];

				platformImage = platformImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				button.SetImage(platformImage, UIControlState.Normal);
				button.HorizontalAlignment = UIControlContentHorizontalAlignment.Fill;
				button.VerticalAlignment = UIControlContentVerticalAlignment.Fill;
			}
		}

		class ImageButtonProxy
		{
			WeakReference<IImageButton>? _virtualView;

			IImageButton? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(IImageButton virtualView, UIButton platformView)
			{
				_virtualView = new(virtualView);

				platformView.TouchUpInside += OnButtonTouchUpInside;
				platformView.TouchUpOutside += OnButtonTouchUpOutside;
				platformView.TouchDown += OnButtonTouchDown;
			}

			public void Disconnect(UIButton platformView)
			{
				platformView.TouchUpInside -= OnButtonTouchUpInside;
				platformView.TouchUpOutside -= OnButtonTouchUpOutside;
				platformView.TouchDown -= OnButtonTouchDown;
			}

			void OnButtonTouchUpInside(object? sender, EventArgs e)
			{
				if (VirtualView is IImageButton imageButton)
				{
					imageButton.Released();
					imageButton.Clicked();
				}
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
}
