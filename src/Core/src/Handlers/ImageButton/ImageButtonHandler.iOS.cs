using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, UIButton>
	{
		// Because we can't inherit from Button we use the container to handle
		// Life cycle events and things like monitoring focus changed
		public override bool NeedsContainer => true;

		readonly ImageButtonProxy _proxy = new();

		protected override void SetupContainer()
		{
			base.SetupContainer();
			if (ContainerView is WrapperView wrapperView)
			{
				wrapperView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
			}
		}

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
			handler.UpdateValue(nameof(IImageButton.Padding));
		}

		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateCornerRadius(buttonStroke);
			handler.UpdateValue(nameof(IImageButton.Padding));
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

				// UIButton.SetImage(image, forState:) does not immediately assign the image to UIButton.ImageView.Image.
				// Instead, the image is set internally and only applied to ImageView when the button is rendered.
				// To ensure SizeThatFits is correct, and avoid race conditions, we have to force a layout.
				button.LayoutIfNeeded();
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
