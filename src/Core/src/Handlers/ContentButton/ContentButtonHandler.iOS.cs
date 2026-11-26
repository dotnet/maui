using System;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentButtonHandler
	{
		readonly ContentButtonEventProxy _proxy = new();

		protected override UIButton CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(MauiContentButton)}.");

			return new MauiContentButton(VirtualView);
		}

		public override void SetVirtualView(IView view)
		{
			var elementHandler = (IElementHandler)this;
			// A live exchange must finish the old button's press before events target the new button.
			if (elementHandler.VirtualView is IContentButton previousView && !ReferenceEquals(previousView, view))
			{
				_proxy.Release();
				if (!ReferenceEquals(elementHandler.VirtualView, previousView))
					return;
			}

			base.SetVirtualView(view);

			if (elementHandler.PlatformView is MauiContentButton platformView &&
				elementHandler.VirtualView is IContentButton currentView)
			{
				platformView.SetVirtualView(currentView);
				if (ReferenceEquals(elementHandler.PlatformView, platformView) &&
					elementHandler.VirtualView is IContentButton connectedView)
					_proxy.UpdateVirtualView(connectedView);
			}
		}

		protected override void ConnectHandler(UIButton platformView)
		{
			if (platformView is not MauiContentButton contentButton)
				throw new InvalidOperationException(NativeViewRequiredMessage);

			contentButton.CaptureNativeDefaults();
			_proxy.Connect(VirtualView, contentButton);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			_proxy.Disconnect(platformView);

			if (platformView is MauiContentButton contentButton)
			{
				contentButton.SetContent(null);
				contentButton.SetVirtualView(null);
				contentButton.RemoveFromSuperview();
			}

			base.DisconnectHandler(platformView);
		}

		public static partial void MapContent(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is not MauiContentButton platformView)
				return;

			var mauiContext = handler.MauiContext ??
				throw new InvalidOperationException($"{nameof(handler.MauiContext)} cannot be null.");

			var presentedContent = button.PresentedContent;
			UIView? content = null;
			if (presentedContent is not null)
			{
				content = presentedContent.ToPlatform(mauiContext);

				if (!IsCurrentContent(handler, button, presentedContent, platformView, mauiContext))
					return;

				if (presentedContent.FlowDirection == FlowDirection.MatchParent)
					content.UpdateFlowDirection(presentedContent);
			}

			if (!IsCurrentContent(handler, button, presentedContent, platformView, mauiContext))
				return;

			platformView.SetContent(content);
		}

		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformView)
				platformView.UpdateBackground(button);
		}

		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button)
		{
			if (!button.IsEnabled && handler is ContentButtonHandler contentButtonHandler)
			{
				contentButtonHandler._proxy.Release();

				if (!IsConnectedTo(handler, button))
					return;
			}

			// The base mapper also updates a Shadow/Clip wrapper's interaction state.
			if (IsConnectedTo(handler, button))
				ViewHandler.MapIsEnabled(handler, button);
		}

		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is UIButton platformView)
				platformView.InvalidateMeasure(button);
		}

		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformView)
			{
				platformView.UpdateStrokeColor(button);
				handler.UpdateValue(nameof(IView.Background));
			}
		}

		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformView)
			{
				platformView.UpdateStrokeThickness(button);
				handler.UpdateValue(nameof(IView.Background));
			}
		}

		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button)
		{
			if (((IElementHandler)handler).PlatformView is MauiContentButton platformView)
			{
				platformView.UpdateCornerRadius(button);
				handler.UpdateValue(nameof(IView.Background));
			}
		}

		sealed class ContentButtonEventProxy
		{
			bool _isPressed;
			WeakReference<IContentButton>? _virtualView;
			WeakReference<IContentButton>? _touchActivationView;

			IContentButton? VirtualView =>
				_virtualView is not null && _virtualView.TryGetTarget(out var view) ? view : null;

			internal void Connect(IContentButton virtualView, MauiContentButton platformView)
			{
				UpdateVirtualView(virtualView);
				platformView.TouchDown += OnTouchDown;
				platformView.TouchUpInside += OnTouchUpInside;
				platformView.TouchUpOutside += OnTouchCancelled;
				platformView.TouchCancel += OnTouchCancelled;
				platformView.PrimaryActionTriggered += OnPrimaryActionTriggered;
			}

			internal void UpdateVirtualView(IContentButton virtualView)
			{
				_virtualView = new(virtualView);
			}

			internal void Disconnect(UIButton platformView)
			{
				Release();
				_virtualView = null;
				_touchActivationView = null;

				platformView.TouchDown -= OnTouchDown;
				platformView.TouchUpInside -= OnTouchUpInside;
				platformView.TouchUpOutside -= OnTouchCancelled;
				platformView.TouchCancel -= OnTouchCancelled;
				platformView.PrimaryActionTriggered -= OnPrimaryActionTriggered;
			}

			internal void Release()
			{
				if (!_isPressed)
					return;

				_isPressed = false;
				VirtualView?.Released();
			}

			void OnTouchDown(object? sender, EventArgs e)
			{
				if (_isPressed || VirtualView is not { IsEnabled: true } button)
					return;

				_isPressed = true;
				_touchActivationView = new(button);
				button.Pressed();
			}

			void OnTouchUpInside(object? sender, EventArgs e)
			{
				Release();
			}

			void OnTouchCancelled(object? sender, EventArgs e)
			{
				Release();
				_touchActivationView = null;
			}

			void OnPrimaryActionTriggered(object? sender, EventArgs e)
			{
				var touchActivationView = _touchActivationView;
				_touchActivationView = null;

				if (VirtualView is { IsEnabled: true } button &&
					(touchActivationView is null ||
						(touchActivationView.TryGetTarget(out var target) && ReferenceEquals(target, button))))
					button.Clicked();
			}
		}
	}
}
