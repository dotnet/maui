using System;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Gtk.Dispatching
{
	/// <summary>
	/// <see cref="ITicker"/> driven by the GDK frame clock of a mapped toplevel window (<c>Widget.AddTickCallback</c>;
	/// non-modal, non-transient windows are preferred over dialogs), falling back to a <c>GLib.Timeout</c> at
	/// <see cref="Ticker.MaxFps"/> when no window is mapped. When the anchor window is unmapped or destroyed while
	/// running, the ticker re-anchors to another window (or the timeout), so ticks never stop while <see cref="IsRunning"/>.
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
			Attach();
		}

		void Attach()
		{
			var anchor = FindAnchor();
			if (anchor is not null)
			{
				_anchor = anchor;
				_tickId = anchor.AddTickCallback(OnTick);
				anchor.Unmapped += OnAnchorUnmapped;
			}
			else
			{
				_timeoutId = GLib.Timeout.Add((uint)Math.Max(1, 1000 / Math.Max(1, MaxFps)), OnTimeout);
			}
		}

		void Detach()
		{
			if (_anchor is not null)
			{
				_anchor.Unmapped -= OnAnchorUnmapped;
				if (_tickId != 0)
					_anchor.RemoveTickCallback(_tickId);
			}
			if (_timeoutId != 0)
				GLib.Timeout.Remove(_timeoutId);

			_tickId = 0;
			_timeoutId = 0;
			_anchor = null;
		}

		void OnAnchorUnmapped(object? sender, EventArgs e)
		{
			// The anchor's frame clock stops with its window: move to another one once the signal emission is over.
			GLib.Idle.Add(() =>
			{
				if (_running && ReferenceEquals(_anchor, sender))
				{
					Detach();
					Attach();
				}
				return false;
			});
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			if (!_running)
				return;

			_running = false;
			Detach();
		}

		static global::Gtk.Widget? FindAnchor()
		{
			global::Gtk.Window? fallback = null;
			foreach (var window in global::Gtk.Window.ListToplevels())
			{
				if (!window.IsMapped)
					continue;

				// Dialogs and modal pages are transient: prefer a main window, whose lifetime is the app's.
				if (!window.Modal && window.TransientFor is null)
					return window;

				fallback ??= window;
			}

			return fallback;
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
