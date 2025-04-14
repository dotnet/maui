using System;
using System.Threading.Tasks;
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

			base.DisconnectHandler(platformView);

			SourceLoader.Reset();
		}

		protected override void ConnectHandler(ShapeableImageView platformView)
		{
			platformView.FocusChange += OnFocusChange;
			platformView.Click += OnClick;
			platformView.Touch += OnTouch;
			platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;

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

		internal static void MapAspect(IImageButtonHandler handler, IImage image)
		{
			handler.PlatformView?.UpdateAspect(image);
			handler.UpdateValue(nameof(IImageButton.StrokeThickness));
		}

		internal static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		internal static async Task MapSourceAsync(IImageHandler handler, IImage image)
		{
			await ImageHandler.MapSourceAsync(handler, image);

			if (handler.IsConnected())
			{
				// Yielding here ensures that the UI thread has a chance to process the updated image source
				// before updating the aspect. This prevents potential race conditions or visual glitches
				// when the image source is updated in quick succession like loading from file instead of URL
				await Task.Yield();
				handler.UpdateValue(nameof(IImage.Aspect));
			}
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