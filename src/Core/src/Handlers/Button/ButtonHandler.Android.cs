using System;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using Google.Android.Material.Button;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, MaterialButton>
	{
		// The padding value has to be done here because in the Material Components,
		// there is a minimum size of the buttons: 88dp x 48dp
		// So, this is calculated:
		//   - Vertical: 6dp*2 (inset) + 8.5dp*2 (padding) + 2.5dp*2 (text magic) + 14dp (text size) = 48dp
		//   - Horizontal: 16dp (from the styles)
		public readonly static Thickness DefaultPadding = new Thickness(16, 8.5);

		static ColorStateList TransparentColorStateList = Colors.Transparent.ToDefaultColorStateList();

		ButtonClickListener ClickListener { get; } = new ButtonClickListener();
		ButtonTouchListener TouchListener { get; } = new ButtonTouchListener();

		protected override MaterialButton CreatePlatformView()
		{
			MaterialButton platformButton = new MauiMaterialButton(Context)
			{
				IconGravity = MaterialButton.IconGravityTextStart,
				IconTintMode = Android.Graphics.PorterDuff.Mode.Add,
				IconTint = TransparentColorStateList,
				SoundEffectsEnabled = false
			};

			return platformButton;
		}

		protected override void ConnectHandler(MaterialButton platformView)
		{
			ClickListener.Handler = this;
			platformView.SetOnClickListener(ClickListener);

			TouchListener.Handler = this;
			platformView.SetOnTouchListener(TouchListener);

			platformView.FocusChange += OnNativeViewFocusChange;
			platformView.LayoutChange += OnPlatformViewLayoutChange;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MaterialButton platformView)
		{
			ClickListener.Handler = null;
			platformView.SetOnClickListener(null);

			TouchListener.Handler = null;
			platformView.SetOnTouchListener(null);

			platformView.FocusChange -= OnNativeViewFocusChange;
			platformView.LayoutChange -= OnPlatformViewLayoutChange;

			ImageSourceLoader.Reset();

			base.DisconnectHandler(platformView);
		}

		// This is a Android-specific mapping
		public static void MapBackground(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateBackground(button);
		}

		public static void MapStrokeColor(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateStrokeColor(button);
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateStrokeThickness(button);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateCornerRadius(button);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.PlatformView?.UpdateTextPlainText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateCharacterSpacing(button);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdatePadding(button, DefaultPadding);
		}

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(IButtonHandler handler, IImage image)
		{
			return handler.ImageSourceLoader.UpdateImageSourceAsync();
		}

		public override void PlatformArrange(Rect frame)
		{
			// The TextView might need an additional measurement pass at the final size
			this.PrepareForTextViewArrange(frame);

			base.PlatformArrange(frame);
		}

		static bool OnTouch(IButton? button, AView? v, MotionEvent? e)
		{
			switch (e?.ActionMasked)
			{
				case MotionEventActions.Down:
					button?.Pressed();
					break;
				case MotionEventActions.Cancel:
				case MotionEventActions.Up:
					button?.Released();
					break;
			}

			return false;
		}

		static void OnClick(IButton? button, AView? v)
		{
			button?.Clicked();
		}

		void OnNativeViewFocusChange(object? sender, AView.FocusChangeEventArgs e)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = e.HasFocus;
		}

		void OnPlatformViewLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
		{
			if (sender is MaterialButton platformView && VirtualView is not null)
				platformView.UpdateBackground(VirtualView);
		}

		class ButtonClickListener : Java.Lang.Object, AView.IOnClickListener
		{
			public ButtonHandler? Handler { get; set; }

			public void OnClick(AView? v)
			{
				ButtonHandler.OnClick(Handler?.VirtualView, v);
			}
		}

		class ButtonTouchListener : Java.Lang.Object, AView.IOnTouchListener
		{
			public ButtonHandler? Handler { get; set; }

			public bool OnTouch(AView? v, global::Android.Views.MotionEvent? e) =>
				ButtonHandler.OnTouch(Handler?.VirtualView, v, e);
		}

		partial class ButtonImageSourcePartSetter
		{
			public override void SetImageSource(Drawable? platformImage)
			{
				if (Handler?.PlatformView is not MaterialButton button)
					return;

				button.Icon = platformImage is null
					? null
					: (OperatingSystem.IsAndroidVersionAtLeast(23)) ? new MauiMaterialButton.MauiResizableDrawable(platformImage) : platformImage;
			}
		}
	}
}