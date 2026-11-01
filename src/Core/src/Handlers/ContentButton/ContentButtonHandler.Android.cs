using System;
using Android.Views;
using Google.Android.Material.Card;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentButtonHandler
	{
		ButtonClickListener ClickListener { get; } = new();
		ButtonTouchListener TouchListener { get; } = new();
		bool _isPressed;

		protected override MaterialCardView CreatePlatformView()
		{
			if (VirtualView is null)
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ContentButton.");

			return new MauiContentButton(Context)
			{
				CardElevation = 0,
				CrossPlatformLayout = VirtualView,
				SoundEffectsEnabled = false,
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (PlatformView is MauiContentButton platformView)
				platformView.CrossPlatformLayout = VirtualView;
		}

		protected override void ConnectHandler(MaterialCardView platformView)
		{
			if (platformView is not MauiContentButton contentButton)
				throw new InvalidOperationException($"{nameof(ContentButtonHandler)} requires a {nameof(MauiContentButton)} platform view.");

			contentButton.CaptureNativeDefaults();

			ClickListener.Handler = this;
			contentButton.SetOnClickListener(ClickListener);

			TouchListener.Handler = this;
			contentButton.SetOnTouchListener(TouchListener);

			contentButton.FocusChange += OnNativeViewFocusChange;
			contentButton.LayoutChange += OnPlatformViewLayoutChange;
			contentButton.DetachedFromWindow += OnPlatformViewDetached;
			contentButton.ThemeChanged += OnPlatformViewThemeChanged;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MaterialCardView platformView)
		{
			ReleasePressed();

			if (platformView is MauiContentButton contentButton)
			{
				contentButton.CrossPlatformLayout = null;
				contentButton.SetContent(null);

				ClickListener.Handler = null;
				contentButton.SetOnClickListener(null);

				TouchListener.Handler = null;
				contentButton.SetOnTouchListener(null);

				contentButton.FocusChange -= OnNativeViewFocusChange;
				contentButton.LayoutChange -= OnPlatformViewLayoutChange;
				contentButton.DetachedFromWindow -= OnPlatformViewDetached;
				contentButton.ThemeChanged -= OnPlatformViewThemeChanged;
			}

			base.DisconnectHandler(platformView);
		}

		public static partial void MapContent(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is not MauiContentButton platformView)
				return;

			if (handler.MauiContext is null)
				throw new InvalidOperationException($"{nameof(handler.MauiContext)} must be set before mapping ContentButton content.");

			AView? content = null;
			if (button.PresentedContent is IView presentedContent)
				content = presentedContent.ToPlatform(handler.MauiContext);

			platformView.SetContent(content);
			platformView.RequestLayout();
		}

		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateButtonBackground(button);
		}

		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button)
		{
			if (!button.IsEnabled && handler is ContentButtonHandler contentButtonHandler)
				contentButtonHandler.ReleasePressed();

			handler.PlatformView?.UpdateIsEnabled(button);
		}

		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button)
		{
			handler.PlatformView?.RequestLayout();
		}

		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateButtonStroke(button);
		}

		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
			{
				platformView.UpdateButtonStroke(button);
				platformView.RequestLayout();
			}
		}

		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateButtonStroke(button);

			if (button.Shadow is not null)
				handler.UpdateValue(nameof(IContentButton.Shadow));
		}

		static bool OnTouch(ContentButtonHandler? handler, MotionEvent? e)
		{
			if (handler?.VirtualView is not { } button)
				return false;

			switch (e?.ActionMasked)
			{
				case MotionEventActions.Down when button.IsEnabled && !handler._isPressed:
					handler._isPressed = true;
					button.Pressed();
					break;
				case MotionEventActions.Cancel:
				case MotionEventActions.Up:
					handler.ReleasePressed();
					break;
			}

			return false;
		}

		static void OnClick(ContentButtonHandler? handler)
		{
			if (handler?.VirtualView is { IsEnabled: true } button)
				button.Clicked();
		}

		void ReleasePressed()
		{
			if (!_isPressed)
				return;

			_isPressed = false;
			VirtualView?.Released();
		}

		void OnNativeViewFocusChange(object? sender, AView.FocusChangeEventArgs e)
		{
			VirtualView?.IsFocused = e.HasFocus;
		}

		void OnPlatformViewLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
		{
			if (sender is MauiContentButton platformView &&
				VirtualView is { Background: { } background } virtualView &&
				!background.IsNullOrEmpty())
			{
				platformView.UpdateButtonBackground(virtualView);
			}
		}

		void OnPlatformViewDetached(object? sender, EventArgs e)
		{
			ReleasePressed();
		}

		void OnPlatformViewThemeChanged(object? sender, EventArgs e)
		{
			if (sender is not MauiContentButton platformView)
				return;

			var virtualView = VirtualView;
			platformView.Post(() =>
			{
				if (!ReferenceEquals(PlatformView, platformView) ||
					!ReferenceEquals(VirtualView, virtualView) ||
					!ReferenceEquals(platformView.CrossPlatformLayout, virtualView) ||
					virtualView is null)
				{
					return;
				}

				platformView.RefreshNativeDefaults();
				platformView.UpdateRippleColor(null);
				UpdateValue(nameof(IView.Background));
				UpdateValue(nameof(IButtonStroke.StrokeColor));
				UpdateValue(nameof(IButtonStroke.StrokeThickness));
				UpdateValue(nameof(IButtonStroke.CornerRadius));
			});
		}

		sealed class ButtonClickListener : Java.Lang.Object, AView.IOnClickListener
		{
			public ContentButtonHandler? Handler { get; set; }

			public void OnClick(AView? v) => ContentButtonHandler.OnClick(Handler);
		}

		sealed class ButtonTouchListener : Java.Lang.Object, AView.IOnTouchListener
		{
			public ContentButtonHandler? Handler { get; set; }

			public bool OnTouch(AView? v, MotionEvent? e) => ContentButtonHandler.OnTouch(Handler, e);
		}
	}
}
