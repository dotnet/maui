using System;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, AppCompatImageView>
	{
		protected override AppCompatImageView CreatePlatformView()
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

		protected override void DisconnectHandler(AppCompatImageView platformView)
		{
			platformView.FocusChange -= OnFocusChange;
			platformView.Click -= OnClick;
			platformView.Touch -= OnTouch;

			base.DisconnectHandler(platformView);

			SourceLoader.Reset();
		}

		protected override void ConnectHandler(AppCompatImageView platformView)
		{
			platformView.FocusChange += OnFocusChange;
			platformView.Click += OnClick;
			platformView.Touch += OnTouch;

			base.ConnectHandler(platformView);
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