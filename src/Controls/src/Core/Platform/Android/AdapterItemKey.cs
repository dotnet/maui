using System;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	class AdapterItemKey
	{
		Page _page;
		Action<AdapterItemKey>? _markInvalid;
		object? _platformView;
		bool _disconnected;

		public AdapterItemKey(Page page, Action<AdapterItemKey> markInvalid)
		{
			// We aren't setting the platform view in the ctor because
			// the PlatformView might not be valid. It might
			// be from a destroyed context or from a page that was moved
			// from a different location.
			_page = page;
			_markInvalid = markInvalid;
			_page.HandlerChanging += OnHandlerChanging;
			_page.HandlerChanged += OnHandlerChanged;
			ItemId = AView.GenerateViewId();
		}

		public bool Disconnected => _disconnected;
		public Page Page => _page;
		public long ItemId { get; }
		public void Disconnect()
		{
			_disconnected = true;
			_markInvalid?.Invoke(this);

			if (_page != null)
			{
				_page.HandlerChanging -= OnHandlerChanging;
				_page.HandlerChanged -= OnHandlerChanged;
			}

			_platformView = null;
		}

		void OnHandlerChanging(object? sender, HandlerChangingEventArgs e)
		{
			if (_platformView != null)
				Disconnect();
		}

		// This will only ever fire once. This is purely waiting
		// for the xplat view to get filled in with a PlatformView.
		// Once a handler is set, then this key is locked to that platformview. 
		// If that handler gets disconnected (OnHandlerChanging) then we have to
		// disconnect this key, and once the same page is requested again a new key/handler 
		// will need to get created. We can't reuse keys for different PlatformViews.
		// The ItemKey/PlatformView relationship is immutable
		void OnHandlerChanged(object? sender, EventArgs e)
		{
			if (_disconnected)
			{
				if (sender is Page page)
					page.HandlerChanged -= OnHandlerChanged;

				return;
			}

			SetToStableView();
		}

		internal void SetToStableView()
		{
			_platformView = _page.Handler?.PlatformView;

			if (_platformView != null)
				_page.HandlerChanged -= OnHandlerChanged;
		}
	}
}