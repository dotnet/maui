using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Handlers.ButtonHandler;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public sealed partial class ButtonHandler : ViewHandler<IButton, MaterialButton>
	{
		static Thickness? DefaultPadding;
		static Drawable? DefaultBackground;
		ImageSourcePartWrapper<ButtonHandler>? _imageSourcePartWrapper;

		ButtonClickListener ClickListener { get; } = new ButtonClickListener();
		ButtonTouchListener TouchListener { get; } = new ButtonTouchListener();

		ImageSourcePartWrapper<ButtonHandler> ImageSourcePartWrapper =>
			_imageSourcePartWrapper ??= new ImageSourcePartWrapper<ButtonHandler>(
				this, (h) => h.VirtualView.ImageSource, null, null, OnSetImageSourceDrawable);

		static ColorStateList? _transparentColorStateList;

		protected override MaterialButton CreateNativeView()
		{
			MaterialButton nativeButton = new MauiMaterialButton(Context)
			{
				IconGravity = MaterialButton.IconGravityTextStart,
				IconTintMode = Android.Graphics.PorterDuff.Mode.Add,
				IconTint = (_transparentColorStateList ??= Colors.Transparent.ToDefaultColorStateList()),
				SoundEffectsEnabled = false
			};

			return nativeButton;
		}

		void SetupDefaults(MaterialButton nativeView)
		{
			DefaultPadding = new Thickness(
				nativeView.PaddingLeft,
				nativeView.PaddingTop,
				nativeView.PaddingRight,
				nativeView.PaddingBottom);

			DefaultBackground = nativeView.Background;
		}

		protected override void ConnectHandler(MaterialButton nativeView)
		{
			ClickListener.Handler = this;
			nativeView.SetOnClickListener(ClickListener);

			TouchListener.Handler = this;
			nativeView.SetOnTouchListener(TouchListener);

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MaterialButton nativeView)
		{
			ClickListener.Handler = null;
			nativeView.SetOnClickListener(null);

			TouchListener.Handler = null;
			nativeView.SetOnTouchListener(null);

			_sourceManager.Reset();

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

		public static void MapImageSource(ButtonHandler handler, IButton image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(ButtonHandler handler, IButton image)
		{
			if (image.ImageSource == null)
			{
				handler.OnSetImageSourceDrawable(null);
				return Task.CompletedTask;
			}

			return handler.ImageSourcePartWrapper.UpdateImageSource();
		}

		bool NeedsExactMeasure()
		{
			if (VirtualView.VerticalLayoutAlignment != Primitives.LayoutAlignment.Fill
				&& VirtualView.HorizontalLayoutAlignment != Primitives.LayoutAlignment.Fill)
			{
				// Layout Alignments of Start, Center, and End will be laying out the TextView at its measured size,
				// so we won't need another pass with MeasureSpecMode.Exactly
				return false;
			}

			if (VirtualView.Width >= 0 && VirtualView.Height >= 0)
			{
				// If the Width and Height are both explicit, then we've already done MeasureSpecMode.Exactly in 
				// both dimensions; no need to do it again
				return false;
			}

			// We're going to need a second measurement pass so TextView can properly handle alignments
			return true;
		}

		public override void NativeArrange(Rectangle frame)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null || Context == null)
			{
				return;
			}

			if (frame.Width < 0 || frame.Height < 0)
			{
				return;
			}

			// Depending on our layout situation, the TextView may need an additional measurement pass at the final size
			// in order to properly handle any TextAlignment properties.
			if (NeedsExactMeasure())
			{
				nativeView.Measure(MakeMeasureSpecExact(frame.Width), MakeMeasureSpecExact(frame.Height));
			}

			base.NativeArrange(frame);
		}

		int MakeMeasureSpecExact(double size)
		{
			// Convert to a native size to create the spec for measuring
			var deviceSize = (int)Context!.ToPixels(size);
			return MeasureSpecMode.Exactly.MakeMeasureSpec(deviceSize);
		}


		void OnSetImageSourceDrawable(Drawable? obj)
		{
			NativeView.Icon = obj;
			VirtualView?.ImageSourceLoaded();
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