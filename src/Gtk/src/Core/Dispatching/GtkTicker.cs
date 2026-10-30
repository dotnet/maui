using System;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Gtk.Dispatching
{
	/// <summary>
	/// <see cref="ITicker"/> driven by the GDK frame clock of the first mapped toplevel window
	/// (<c>Widget.AddTickCallback</c>), falling back to a <c>GLib.Timeout</c> at <see cref="Ticker.MaxFps"/> when
	/// no window is mapped yet.
	/// </summary>
	public sealed class GtkTicker : Ticker
	{
		global::Gtk.Widget? _anchor;
		uint _tickId;
		uint _timeoutId;
		bool _running;

		/// <inheritdoc/>
		public override bool IsRunning => _running;

		/// <inheritdoc/>
		public override void Start()
		{
			if (_running)
				return;

			_running = true;

			var anchor = FindAnchor();
			if (anchor is not null)
			{
				_anchor = anchor;
				_tickId = anchor.AddTickCallback(OnTick);
			}
			else
			{
				_timeoutId = GLib.Timeout.Add((uint)Math.Max(1, 1000 / Math.Max(1, MaxFps)), OnTimeout);
			}
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			if (!_running)
				return;

			_running = false;

			if (_tickId != 0 && _anchor is not null)
				_anchor.RemoveTickCallback(_tickId);
			if (_timeoutId != 0)
				GLib.Timeout.Remove(_timeoutId);

			_tickId = 0;
			_timeoutId = 0;
			_anchor = null;
		}

		static global::Gtk.Widget? FindAnchor()
		{
			foreach (var window in global::Gtk.Window.ListToplevels())
			{
				if (window.IsMapped)
					return window;
			}

			return null;
		}

		bool OnTick(global::Gtk.Widget widget, Gdk.FrameClock clock)
		{
			if (!_running)
				return false;

			Fire?.Invoke();
			return _running;
		}

		bool OnTimeout()
		{
			if (!_running)
				return false;

			Fire?.Invoke();
			return _running;
		}
	}
}
