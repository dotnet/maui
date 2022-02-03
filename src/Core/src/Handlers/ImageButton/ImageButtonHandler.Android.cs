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
			var nativeView = new ShapeableImageView(Context);

			// These set the defaults so visually it matches up with other platforms
			nativeView.SetPadding(0, 0, 0, 0);
			nativeView.SoundEffectsEnabled = false;

			return nativeView;
		}

		void OnSetImageSource(Drawable? obj)
		{
			PlatformView.SetImageDrawable(obj);
		}

		protected override void DisconnectHandler(ShapeableImageView nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.Touch -= OnTouch;

			base.DisconnectHandler(nativeView);

			SourceLoader.Reset();
		}

		protected override void ConnectHandler(ShapeableImageView nativeView)
		{
			nativeView.Click += OnClick;
			nativeView.Touch += OnTouch;

			base.ConnectHandler(nativeView);
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