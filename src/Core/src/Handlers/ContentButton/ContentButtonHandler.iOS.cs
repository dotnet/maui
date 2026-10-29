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
			base.SetVirtualView(view);

			if (PlatformView is MauiContentButton platformView)
			{
				platformView.SetVirtualView(VirtualView);
				_proxy.UpdateVirtualView(VirtualView);
			}
		}

		protected override void ConnectHandler(UIButton platformView)
		{
			if (platformView is MauiContentButton contentButton)
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
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(handler.MauiContext)} cannot be null.");

			if (handler.PlatformView is not MauiContentButton platformView)
				return;

			UIView? content = null;
			if (button.PresentedContent is IView presentedContent)
			{
				content = presentedContent.ToPlatform(handler.MauiContext);

				if (presentedContent.FlowDirection == FlowDirection.MatchParent)
					content.UpdateFlowDirection(presentedContent);
			}

			platformView.SetContent(content);
		}

		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateBackground(button);
		}

		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button)
		{
			if (!button.IsEnabled && handler is ContentButtonHandler contentButtonHandler)
				contentButtonHandler._proxy.Release();

			handler.PlatformView?.UpdateIsEnabled(button);
		}

		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button)
		{
			handler.PlatformView?.InvalidateMeasure(button);
		}

		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateStrokeColor(button);
		}

		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateStrokeThickness(button);
		}

		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateCornerRadius(button);
		}

		sealed class ContentButtonEventProxy
		{
			bool _isPressed;
			WeakReference<IContentButton>? _virtualView;

			IContentButton? VirtualView =>
				_virtualView is not null && _virtualView.TryGetTarget(out var view) ? view : null;

			internal void Connect(IContentButton virtualView, MauiContentButton platformView)
			{
				UpdateVirtualView(virtualView);
				platformView.TouchDown += OnTouchDown;
				platformView.TouchUpInside += OnTouchEnded;
				platformView.TouchUpOutside += OnTouchEnded;
				platformView.TouchCancel += OnTouchEnded;
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

				platformView.TouchDown -= OnTouchDown;
				platformView.TouchUpInside -= OnTouchEnded;
				platformView.TouchUpOutside -= OnTouchEnded;
				platformView.TouchCancel -= OnTouchEnded;
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
				button.Pressed();
			}

			void OnTouchEnded(object? sender, EventArgs e)
			{
				Release();
			}

			void OnPrimaryActionTriggered(object? sender, EventArgs e)
			{
				if (VirtualView is { IsEnabled: true } button)
					button.Clicked();
			}
		}
	}
}
