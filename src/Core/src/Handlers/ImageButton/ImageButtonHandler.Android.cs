using System;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, AppCompatImageButton>
	{
		protected override AppCompatImageButton CreateNativeView()
		{
			var nativeView = new AppCompatImageButton(Context);

			// These set the defaults so visually it matches up with other platforms
			nativeView.SetPadding(0, 0, 0, 0);
			nativeView.SoundEffectsEnabled = false;

			return nativeView;
		}

		void OnSetImageSource(Drawable? obj)
		{
			NativeView.SetImageDrawable(obj);
		}

		protected override void DisconnectHandler(AppCompatImageButton nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.Touch -= OnTouch;

			base.DisconnectHandler(nativeView);

			SourceLoader.Reset();
		}

		protected override void ConnectHandler(AppCompatImageButton nativeView)
		{
			nativeView.Click += OnClick;
			nativeView.Touch += OnTouch;

			base.ConnectHandler(nativeView);
		}

		[MissingMapper]
		public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke) { }

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