using System;
using Android.Graphics.Drawables;
using Android.Views;
using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, ShapeableImageView>
	{
		protected override ShapeableImageView CreatePlatformView()
		{
			var platformView = new MauiShapeableImageView(Context);

			// These set the defaults so visually it matches up with other platforms
			platformView.SetPadding(0, 0, 0, 0);
			platformView.SoundEffectsEnabled = false;

			return platformView;
		}

		protected override void DisconnectHandler(ShapeableImageView platformView)
		{
			platformView.FocusChange -= OnFocusChange;
			platformView.Click -= OnClick;
			platformView.Touch -= OnTouch;
			platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
			platformView.LayoutChange -= OnPlatformViewLayoutChange;

			base.DisconnectHandler(platformView);

			SourceLoader.Reset();
		}

		protected override void ConnectHandler(ShapeableImageView platformView)
		{
			platformView.FocusChange += OnFocusChange;
			platformView.Click += OnClick;
			platformView.Touch += OnTouch;
			platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;

			platformView.LayoutChange += OnPlatformViewLayoutChange;
			base.ConnectHandler(platformView);
		}

		public static void MapBackground(IImageButtonHandler handler, IImageButton imageButton)
		{
			handler.PlatformView?.UpdateBackground(imageButton);
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
			handler.PlatformView?.UpdatePadding(imageButton);
		}

		void OnFocusChange(object? sender, View.FocusChangeEventArgs e)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = e.HasFocus;
		}

		void OnPlatformViewLayoutChange(object? sender, View.LayoutChangeEventArgs e)
		{
			if (sender is ShapeableImageView platformView && VirtualView is not null)
				platformView.UpdateBackground(VirtualView);
		}

		void OnTouch(object? sender, View.TouchEventArgs e)
		{
			var motionEvent = e.Event;
			switch (motionEvent?.ActionMasked)
			{
				case MotionEventActions.Down:
					VirtualView?.Pressed();
					break;
				case MotionEventActions.Up:
					VirtualView?.Released();
					break;
			}

			e.Handled = false;
		}

		void OnClick(object? sender, EventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnPlatformViewAttachedToWindow(object? sender, View.ViewAttachedToWindowEventArgs e)
		{
			if (sender is not View platformView)
			{
				return;
			}

			if (!this.IsConnected())
			{
				platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
				return;
			}

			ImageHandler.OnPlatformViewAttachedToWindow(this);
		}

		partial class ImageButtonImageSourcePartSetter
		{
			public override void SetImageSource(Drawable? platformImage)
			{
				if (Handler?.PlatformView is not ShapeableImageView button)
					return;

				button.SetImageDrawable(platformImage);
			}
		}
	}
}