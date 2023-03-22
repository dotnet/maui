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
			var platformView = new ShapeableImageView(Context);

			// These set the defaults so visually it matches up with other platforms
			platformView.SetPadding(0, 0, 0, 0);
			platformView.SoundEffectsEnabled = false;

			return platformView;
		}

		void OnSetImageSource(Drawable? obj)
		{
			PlatformView.SetImageDrawable(obj);
		}

		protected override void DisconnectHandler(ShapeableImageView platformView)
		{
			platformView.FocusChange -= OnFocusChange;
			platformView.Click -= OnClick;
			platformView.Touch -= OnTouch;

			base.DisconnectHandler(platformView);

			SourceLoader.Reset();
		}

		protected override void ConnectHandler(ShapeableImageView platformView)
		{
			platformView.FocusChange += OnFocusChange;
			platformView.Click += OnClick;
			platformView.Touch += OnTouch;

			base.ConnectHandler(platformView);
		}

		// TODO: NET8 make this public
		internal static void MapBackground(IImageButtonHandler handler, IImageButton imageButton)
		{
			(handler.PlatformView as ShapeableImageView)?.UpdateBackground(imageButton);
		}

		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as ShapeableImageView)?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as ShapeableImageView)?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
		{
			(handler.PlatformView as ShapeableImageView)?.UpdateCornerRadius(buttonStroke);
		}

		public static void MapPadding(IImageButtonHandler handler, IImageButton imageButton)
		{
			(handler.PlatformView as ShapeableImageView)?.UpdatePadding(imageButton);
		}

		void OnFocusChange(object? sender, View.FocusChangeEventArgs e)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = e.HasFocus;
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
	}
}