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
		ButtonKeyListener KeyListener { get; } = new();
		bool _isPressed;
		IContentButton? _touchTarget;
		IContentButton? _keyTarget;
		Keycode? _activationKey;

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
			var elementHandler = (IElementHandler)this;
			var previousView = elementHandler.VirtualView;
			var wasFocused = previousView is IView &&
				elementHandler.PlatformView is AView { IsFocused: true };
			if (!ReferenceEquals(previousView, view))
			{
				ReleasePressed();
				if (!ReferenceEquals(elementHandler.VirtualView, previousView))
					return;
			}

			base.SetVirtualView(view);

			if (wasFocused && previousView is IView { IsFocused: true } previous &&
				!ReferenceEquals(elementHandler.VirtualView, previous))
			{
				previous.IsFocused = false;
			}

			if (elementHandler.PlatformView is MauiContentButton platformView &&
				elementHandler.VirtualView is IContentButton currentView)
			{
				if (!ReferenceEquals(platformView.CrossPlatformLayout, currentView))
				{
					platformView.CrossPlatformLayout = currentView;
					platformView.MarkSafeAreaEdgeConfigurationChanged();
				}

				if (wasFocused && platformView.IsFocused && !currentView.IsFocused)
					currentView.IsFocused = true;
			}
		}

		protected override void ConnectHandler(MaterialCardView platformView)
		{
			if (platformView is not MauiContentButton contentButton)
				throw new InvalidOperationException(NativeViewRequiredMessage);

			contentButton.CaptureNativeDefaults();

			ClickListener.Handler = this;
			contentButton.SetOnClickListener(ClickListener);

			TouchListener.Handler = this;
			contentButton.SetOnTouchListener(TouchListener);

			KeyListener.Handler = this;
			contentButton.SetOnKeyListener(KeyListener);

			contentButton.FocusChange += OnNativeViewFocusChange;
			contentButton.LayoutChange += OnPlatformViewLayoutChange;
			contentButton.DetachedFromWindow += OnPlatformViewDetached;
			contentButton.ThemeChanged += OnPlatformViewThemeChanged;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MaterialCardView platformView)
		{
			ReleasePressed();
			_touchTarget = null;

			if (platformView is MauiContentButton contentButton)
			{
				contentButton.CrossPlatformLayout = null;
				contentButton.SetContent(null);

				ClickListener.Handler = null;
				contentButton.SetOnClickListener(null);

				TouchListener.Handler = null;
				contentButton.SetOnTouchListener(null);

				KeyListener.Handler = null;
				contentButton.SetOnKeyListener(null);

				contentButton.FocusChange -= OnNativeViewFocusChange;
				contentButton.LayoutChange -= OnPlatformViewLayoutChange;
				contentButton.DetachedFromWindow -= OnPlatformViewDetached;
				contentButton.ThemeChanged -= OnPlatformViewThemeChanged;
			}

			base.DisconnectHandler(platformView);
		}

		public static partial void MapContent(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is not MauiContentButton platformView)
				return;

			var mauiContext = handler.MauiContext ??
				throw new InvalidOperationException($"{nameof(handler.MauiContext)} must be set before mapping ContentButton content.");

			var presentedContent = button.PresentedContent;
			AView? content = null;
			if (presentedContent is not null)
				content = presentedContent.ToPlatform(mauiContext);

			if (!IsCurrentContent(handler, button, presentedContent, platformView, mauiContext))
				return;

			platformView.SetContent(content);
			platformView.RequestLayout();
		}

		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is not MauiContentButton platformView)
				return;

			var hadMauiBackground = platformView.HasMauiBackground;
			platformView.UpdateButtonBackground(button);

			// Leaving a MAUI background restores the card stroke, so let StrokeColor
			// mapper customizations apply to it again.
			if (hadMauiBackground && !platformView.HasMauiBackground && IsConnectedTo(handler, button))
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
		}

		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button)
		{
			if (!button.IsEnabled && handler is ContentButtonHandler contentButtonHandler)
			{
				contentButtonHandler.ReleasePressed();

				if (!IsConnectedTo(handler, button))
					return;
			}

			if (((IElementHandler)handler).PlatformView is MaterialCardView platformView)
				platformView.UpdateIsEnabled(button);
		}

		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MaterialCardView platformView)
				platformView.RequestLayout();
		}

		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformView)
				platformView.UpdateButtonStroke(button, updateShape: false, updateColor: true);
		}

		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformView)
			{
				platformView.UpdateButtonStroke(button, updateShape: false);
				platformView.RequestLayout();
			}
		}

		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformView)
			{
				platformView.UpdateButtonStroke(button);

				if (button.Shadow is not null)
					handler.UpdateValue(nameof(IContentButton.Shadow));
			}
		}

		static bool OnTouch(ContentButtonHandler? handler, AView? platformView, MotionEvent? e)
		{
			if (handler is null || ((IElementHandler)handler).VirtualView is not IContentButton button)
				return false;

			switch (e?.ActionMasked)
			{
				case MotionEventActions.Down when button.IsEnabled && !handler._isPressed:
					handler._touchTarget = button;
					handler._isPressed = true;
					button.Pressed();
					break;
				case MotionEventActions.Cancel:
					handler._touchTarget = null;
					handler.ReleasePressed();
					break;
				case MotionEventActions.Up:
					var touchTarget = handler._touchTarget;
					handler._touchTarget = null;
					var releasedOutside = platformView is not null &&
						(e.GetX() < 0 || e.GetX() >= platformView.Width ||
							e.GetY() < 0 || e.GetY() >= platformView.Height);

					if (platformView is not null)
					{
						// UP is handled here, so cancel native delayed press and long-press callbacks.
						using var cancelEvent = MotionEvent.Obtain(e) ??
							throw new InvalidOperationException("Unable to cancel the ContentButton's native touch sequence.");
						cancelEvent.Action = MotionEventActions.Cancel;
						platformView.OnTouchEvent(cancelEvent);
					}

					handler.ReleasePressed();
					var cancelActivation = releasedOutside || !ReferenceEquals(touchTarget, button) ||
						!IsConnectedTo(handler, button) ||
						!ReferenceEquals(((IElementHandler)handler).PlatformView, platformView);
					if (platformView is not null)
					{
						platformView.Pressed = false;
						if (!cancelActivation)
							platformView.PerformClick();
					}

					return true;
			}

			return false;
		}

		static void OnClick(ContentButtonHandler? handler)
		{
			if (((IElementHandler?)handler)?.VirtualView is IContentButton { IsEnabled: true } button)
				button.Clicked();
		}

		static bool OnKey(ContentButtonHandler? handler, AView? platformView, Keycode keyCode, KeyEvent? e)
		{
			if (handler is null || platformView is null || e is null || !keyCode.IsConfirmKey())
				return false;

			if (e.Action == KeyEventActions.Down)
			{
				if (handler._isPressed)
					return handler._activationKey == keyCode;

				if (((IElementHandler)handler).VirtualView is not IContentButton { IsEnabled: true } button)
					return false;

				handler._keyTarget = button;
				handler._activationKey = keyCode;
				handler._isPressed = true;
				platformView.Pressed = true;
				button.Pressed();
				return true;
			}

			if (e.Action == KeyEventActions.Up && handler._activationKey == keyCode)
			{
				var keyTarget = handler._keyTarget;
				handler.ReleasePressed();
				if (!e.IsCanceled && keyTarget is { IsEnabled: true } &&
					IsConnectedTo(handler, keyTarget) &&
					ReferenceEquals(((IElementHandler)handler).PlatformView, platformView))
				{
					platformView.PerformClick();
				}

				return true;
			}

			return false;
		}

		void ReleasePressed()
		{
			_touchTarget = null;
			if (_activationKey is not null && ((IElementHandler)this).PlatformView is AView platformView)
				platformView.Pressed = false;
			_keyTarget = null;
			_activationKey = null;

			if (!_isPressed)
				return;

			_isPressed = false;
			if (((IElementHandler)this).VirtualView is IContentButton button)
				button.Released();
		}

		void OnNativeViewFocusChange(object? sender, AView.FocusChangeEventArgs e)
		{
			var view = ((IElementHandler)this).VirtualView as IView;
			if (!e.HasFocus && _activationKey is not null)
				ReleasePressed();

			if (view is not null && ReferenceEquals(((IElementHandler)this).VirtualView, view))
				view.IsFocused = e.HasFocus;
		}

		void OnPlatformViewLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
		{
			if ((e.Right - e.Left != e.OldRight - e.OldLeft ||
				e.Bottom - e.Top != e.OldBottom - e.OldTop) &&
				sender is MauiContentButton platformView)
			{
				var elementHandler = (IElementHandler)this;
				if (ReferenceEquals(elementHandler.PlatformView, platformView) &&
					elementHandler.VirtualView is IContentButton { Background: GradientPaint })
				{
					UpdateValue(nameof(IView.Background));
				}
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

			var elementHandler = (IElementHandler)this;
			platformView.Post(() =>
			{
				var virtualView = elementHandler.VirtualView as IContentButton;
				if (!ReferenceEquals(elementHandler.PlatformView, platformView) ||
					!ReferenceEquals(platformView.CrossPlatformLayout, virtualView) ||
					virtualView is null)
				{
					return;
				}

				platformView.RefreshNativeDefaults();
				platformView.UpdateRippleColor(null);
				UpdateValue(nameof(IView.Background));
				if (!IsConnectedTo(elementHandler, virtualView))
					return;

				UpdateValue(nameof(IButtonStroke.StrokeColor));
				if (!IsConnectedTo(elementHandler, virtualView))
					return;

				UpdateValue(nameof(IButtonStroke.StrokeThickness));
				if (!IsConnectedTo(elementHandler, virtualView))
					return;

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

			public bool OnTouch(AView? v, MotionEvent? e) => ContentButtonHandler.OnTouch(Handler, v, e);
		}

		sealed class ButtonKeyListener : Java.Lang.Object, AView.IOnKeyListener
		{
			public ContentButtonHandler? Handler { get; set; }

			public bool OnKey(AView? v, Keycode keyCode, KeyEvent? e) => ContentButtonHandler.OnKey(Handler, v, keyCode, e);
		}
	}
}
