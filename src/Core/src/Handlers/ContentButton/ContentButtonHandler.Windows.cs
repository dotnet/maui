#nullable enable
using System;
using Microsoft.Maui.Platform;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentButtonHandler : ViewHandler<IContentButton, Button>
	{
		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;
		WeakReference<IContentButton>? _activationView;
		bool _isPressed;

		internal override bool PreventGestureBubbling => true;

		public override void SetVirtualView(IView view)
		{
			var elementHandler = (IElementHandler)this;
			var previousView = elementHandler.VirtualView;
			var wasFocused = previousView is IView &&
				elementHandler.PlatformView is Button { FocusState: not FocusState.Unfocused };
			if (_isPressed && !ReferenceEquals(previousView, view))
			{
				Release();

				if (!ReferenceEquals(elementHandler.VirtualView, previousView))
					return;
			}

			base.SetVirtualView(view);

			if (wasFocused && previousView is IView { IsFocused: true } previous &&
				!ReferenceEquals(elementHandler.VirtualView, previous))
			{
				previous.IsFocused = false;
			}

			if (view is IContentButton button &&
				IsConnectedTo(this, button) &&
				elementHandler.PlatformView is MauiContentButton platformView)
			{
				platformView.ContentPanel.UpdateCrossPlatformLayout(button);

				if (wasFocused && platformView.FocusState != FocusState.Unfocused && !button.IsFocused)
					button.IsFocused = true;
			}
		}

		protected override Button CreatePlatformView()
		{
			var platformButton = new MauiContentButton();
			platformButton.ContentPanel.CrossPlatformLayout = VirtualView;
			return platformButton;
		}

		protected override void ConnectHandler(Button platformView)
		{
			if (platformView is not MauiContentButton nativeButton)
				throw new InvalidOperationException(NativeViewRequiredMessage);

			_isPressed = false;
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			nativeButton.CaptureNativeDefaults();

			platformView.Click += OnClick;
			platformView.Unloaded += OnUnloaded;
			platformView.LostFocus += OnLostFocus;
			platformView.IsEnabledChanged += OnIsEnabledChanged;
			platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			platformView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);
			platformView.AddHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler, true);
			platformView.AddHandler(UIElement.PointerCaptureLostEvent, _pointerReleasedHandler, true);

			if (platformView is MauiContentButton contentButton)
			{
				contentButton.ActivationKeyDown += OnKeyDown;
				contentButton.ActivationKeyUp += OnKeyUp;
			}

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Button platformView)
		{
			CancelPress();

			platformView.Click -= OnClick;
			platformView.Unloaded -= OnUnloaded;
			platformView.LostFocus -= OnLostFocus;
			platformView.IsEnabledChanged -= OnIsEnabledChanged;
			platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			platformView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);
			platformView.RemoveHandler(UIElement.PointerCanceledEvent, _pointerReleasedHandler);
			platformView.RemoveHandler(UIElement.PointerCaptureLostEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;

			if (platformView is MauiContentButton contentButton)
			{
				contentButton.ActivationKeyDown -= OnKeyDown;
				contentButton.ActivationKeyUp -= OnKeyUp;
				contentButton.ContentPanel.CrossPlatformLayout = null;
				contentButton.ClearPresentedContent();
			}

			base.DisconnectHandler(platformView);
		}

		public static partial void MapContent(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is not MauiContentButton platformButton)
				return;

			var mauiContext = handler.MauiContext ??
				throw new InvalidOperationException($"{nameof(handler.MauiContext)} must be set before updating content.");

			var content = button.PresentedContent;
			// A new template can reuse the current content while its handler is created.
			platformButton.ClearPresentedContent();

			if (!IsCurrentContent(handler, button, content, platformButton, mauiContext) || content is null)
				return;

			var platformContent = content.ToPlatform(mauiContext);
			if (!IsCurrentContent(handler, button, content, platformButton, mauiContext))
				return;

			RemoveFromParent(platformContent);
			if (!IsCurrentContent(handler, button, content, platformButton, mauiContext))
				return;

			platformButton.SetPresentedContent(platformContent);
		}

		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformButton)
				platformButton.UpdateBackground(button);
			else if (((IElementHandler)handler).PlatformView is Button nativeButton)
				nativeButton.UpdateBackground(button);
		}

		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is Button platformButton)
				platformButton.UpdateIsEnabled(button);

			if (!button.IsEnabled && handler is ContentButtonHandler contentButtonHandler)
			{
				contentButtonHandler.Release();
			}
		}

		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformButton)
			{
				platformButton.Padding = new WThickness(0);
				platformButton.ContentPanel.InvalidateMeasure();
				platformButton.InvalidateMeasure();
			}
		}

		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformButton)
				platformButton.UpdateStrokeColor(button);
			else if (((IElementHandler)handler).PlatformView is Button nativeButton)
				nativeButton.UpdateStrokeColor(button);
		}

		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformButton)
			{
				platformButton.UpdateStrokeThickness(button);
				platformButton.UpdateContentLayout(button.StrokeThickness);
			}
			else if (((IElementHandler)handler).PlatformView is Button nativeButton)
			{
				nativeButton.UpdateStrokeThickness(button);
			}
		}

		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformButton)
				platformButton.UpdateCornerRadius(button);
			else if (((IElementHandler)handler).PlatformView is Button nativeButton)
				nativeButton.UpdateCornerRadius(button);

			if (button.Shadow is not null)
				handler.UpdateValue(nameof(IView.Shadow));
		}

		static void RemoveFromParent(FrameworkElement platformView)
		{
			if (platformView.Parent is ContentPanel existingContentPanel)
			{
				if (ReferenceEquals(existingContentPanel.Content, platformView))
				{
					existingContentPanel.Content = null;
				}
				else
				{
					existingContentPanel.CachedChildren.Remove(platformView);
				}
			}
			else if (platformView.Parent is MauiPanel existingPanel)
			{
				existingPanel.CachedChildren.Remove(platformView);
			}
		}

		void OnClick(object sender, RoutedEventArgs e)
		{
			var button = ((IElementHandler)this).VirtualView as IContentButton;
			var activationView = _activationView;
			_activationView = null;
			Release();

			if (button is { IsEnabled: true } &&
				(activationView is null ||
					(activationView.TryGetTarget(out var target) && ReferenceEquals(target, button))) &&
				IsConnectedTo(this, button))
			{
				button.Clicked();
			}
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (sender is UIElement element &&
				(e.Pointer.PointerDeviceType != PointerDeviceType.Mouse ||
					e.GetCurrentPoint(element).Properties.IsLeftButtonPressed))
			{
				Press();
			}
		}

		// WinUI raises a successful pointer Click before release or capture loss.
		void OnPointerReleased(object sender, PointerRoutedEventArgs e) => CancelPress();

		void OnKeyDown(object? sender, KeyRoutedEventArgs e)
		{
			Press();
		}

		void OnKeyUp(object? sender, KeyRoutedEventArgs e)
		{
			Release();
		}

		void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is Button { IsEnabled: false })
			{
				CancelPress();
			}
		}

		void OnUnloaded(object sender, RoutedEventArgs e) => CancelPress();

		void OnLostFocus(object sender, RoutedEventArgs e) => CancelPress();

		void Press()
		{
			if (_isPressed || ((IElementHandler)this).VirtualView is not IContentButton { IsEnabled: true } button)
				return;

			_isPressed = true;
			_activationView = new(button);
			button.Pressed();
		}

		void Release()
		{
			if (!_isPressed)
				return;

			_isPressed = false;
			(((IElementHandler)this).VirtualView as IContentButton)?.Released();
		}

		void CancelPress()
		{
			Release();
			_activationView = null;
		}
	}
}
