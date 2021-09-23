using System;
using System.Collections.Generic;
using System.Text;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, AppCompatImageButton>
	{
		protected override AppCompatImageButton CreateNativeView()
		{
			return new AppCompatImageButton(Context);
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
