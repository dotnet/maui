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
		bool _isPressed;

		protected override Button CreatePlatformView()
		{
			var platformButton = new MauiContentButton();
			platformButton.ContentPanel.CrossPlatformLayout = VirtualView;
			return platformButton;
		}

		protected override void ConnectHandler(Button platformView)
		{
			_isPressed = false;
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			platformView.Click += OnClick;
			platformView.Unloaded += OnUnloaded;
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
			Release();

			platformView.Click -= OnClick;
			platformView.Unloaded -= OnUnloaded;
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
			if (handler.PlatformView is not MauiContentButton platformButton)
				return;

			var mauiContext = handler.MauiContext ??
				throw new InvalidOperationException($"{nameof(handler.MauiContext)} must be set before updating content.");

			// A new template can reuse the current content while its handler is created.
			platformButton.ClearPresentedContent();

			if (button.PresentedContent is not IView content)
				return;

			var platformContent = content.ToPlatform(mauiContext);
			RemoveFromParent(platformContent);
			platformButton.SetPresentedContent(platformContent);
		}

		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button)
		{
			handler.PlatformView?.UpdateBackground(button);
		}

		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button)
		{
			handler.PlatformView?.UpdateIsEnabled(button);

			if (!button.IsEnabled && handler is ContentButtonHandler contentButtonHandler)
			{
				contentButtonHandler.Release();
			}
		}

		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformButton)
			{
				platformButton.Padding = new WThickness(0);
				platformButton.ContentPanel.InvalidateMeasure();
				platformButton.InvalidateMeasure();
			}
		}

		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button)
		{
			handler.PlatformView?.UpdateStrokeColor(button);
		}

		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button)
		{
			handler.PlatformView?.UpdateStrokeThickness(button);

			if (handler.PlatformView is MauiContentButton platformButton)
			{
				platformButton.UpdateContentLayout(button.StrokeThickness);
			}
		}

		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button)
		{
			handler.PlatformView?.UpdateCornerRadius(button);
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
			Release();

			if (VirtualView?.IsEnabled == true)
			{
				VirtualView.Clicked();
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

		void OnPointerReleased(object sender, PointerRoutedEventArgs e) => Release();

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
				Release();
			}
		}

		void OnUnloaded(object sender, RoutedEventArgs e) => Release();

		void Press()
		{
			if (_isPressed || VirtualView?.IsEnabled != true)
				return;

			_isPressed = true;
			VirtualView.Pressed();
		}

		void Release()
		{
			if (!_isPressed)
				return;

			_isPressed = false;
			VirtualView?.Released();
		}
	}
}
