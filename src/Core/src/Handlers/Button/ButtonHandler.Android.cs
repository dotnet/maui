using System;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, AppCompatButton>
	{
		ButtonClickListener ClickListener { get; } = new ButtonClickListener();
		ButtonTouchListener TouchListener { get; } = new ButtonTouchListener();

		protected override AppCompatButton CreateNativeView()
		{
			AppCompatButton nativeButton = new AppCompatButton(Context)
			{
				SoundEffectsEnabled = false
			};

			return nativeButton;
		}

		protected override void ConnectHandler(AppCompatButton nativeView)
		{
			ClickListener.Handler = this;
			nativeView.SetOnClickListener(ClickListener);

			TouchListener.Handler = this;
			nativeView.SetOnTouchListener(TouchListener);

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(AppCompatButton nativeView)
		{
			ClickListener.Handler = null;
			nativeView.SetOnClickListener(null);

			TouchListener.Handler = null;
			nativeView.SetOnTouchListener(null);

			base.DisconnectHandler(nativeView);
		}

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateText(button);
		}

		public static void MapTextColor(ButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateTextColor(button);
		}

		public static void MapFont(ButtonHandler handler, IButton button)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(ButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdatePadding(button);
		}

		public bool OnTouch(IButton? button, AView? v, MotionEvent? e)
		{
			switch (e?.ActionMasked)
			{
				case MotionEventActions.Down:
					button?.Pressed();
					break;
				case MotionEventActions.Up:
					button?.Released();
					break;
			}

			return false;
		}

		public void OnClick(IButton? button, AView? v)
		{
			button?.Clicked();
		}

		public class ButtonClickListener : Java.Lang.Object, AView.IOnClickListener
		{
			public ButtonHandler? Handler { get; set; }

			public void OnClick(AView? v)
			{
				Handler?.OnClick(Handler?.VirtualView, v);
			}
		}

		public class ButtonTouchListener : Java.Lang.Object, AView.IOnTouchListener
		{
			public ButtonHandler? Handler { get; set; }

			public bool OnTouch(AView? v, global::Android.Views.MotionEvent? e) =>
				Handler?.OnTouch(Handler?.VirtualView, v, e) ?? false;
		}
	}
}