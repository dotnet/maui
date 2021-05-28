using System;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, AppCompatButton>
	{
		static Thickness? DefaultPadding;
		static Drawable? DefaultBackground;

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

		protected override void SetupDefaults(AppCompatButton nativeView)
		{
			DefaultPadding = new Thickness(
				nativeView.PaddingLeft,
				nativeView.PaddingTop,
				nativeView.PaddingRight,
				nativeView.PaddingBottom);

			DefaultBackground = nativeView.Background;

			base.SetupDefaults(nativeView);
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

		// This is a Android-specific mapping
		public static void MapBackground(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateBackground(button, DefaultBackground);
		}

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateText(button);
		}

		public static void MapTextColor(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateTextColor(button);
		}

		public static void MapCharacterSpacing(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateCharacterSpacing(button);
		}

		public static void MapFont(ButtonHandler handler, IButton button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdatePadding(button, DefaultPadding);
		}

		bool OnTouch(IButton? button, AView? v, MotionEvent? e)
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

		void OnClick(IButton? button, AView? v)
		{
			button?.Clicked();
		}

		class ButtonClickListener : Java.Lang.Object, AView.IOnClickListener
		{
			public ButtonHandler? Handler { get; set; }

			public void OnClick(AView? v)
			{
				Handler?.OnClick(Handler?.VirtualView, v);
			}
		}

		class ButtonTouchListener : Java.Lang.Object, AView.IOnTouchListener
		{
			public ButtonHandler? Handler { get; set; }

			public bool OnTouch(AView? v, global::Android.Views.MotionEvent? e) =>
				Handler?.OnTouch(Handler?.VirtualView, v, e) ?? false;
		}
	}
}