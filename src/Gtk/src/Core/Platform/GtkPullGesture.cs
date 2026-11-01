using System;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// The pull-to-refresh gesture of a widget: an upward scroll (wheel or touchpad) while the content is already at its top, accumulated until it
	/// passes <see cref="Threshold"/>. Shared by <c>RefreshView</c> and the legacy <c>ListView</c>.
	/// </summary>
	internal sealed class GtkPullGesture
	{
		/// <summary>The accumulated upward scroll, in pixels, that triggers a refresh.</summary>
		public const double Threshold = 100;

		const double WheelStep = 40;

		readonly global::Gtk.EventControllerScroll _scroll;
		readonly Func<bool> _canPull;
		readonly Func<bool> _isAtTop;
		double _pull;

		/// <summary>Attaches the gesture to <paramref name="host"/> (capture phase: the pull is decided before the scrollable content sees the event).</summary>
		/// <param name="host">The widget that receives the scroll events.</param>
		/// <param name="canPull">Whether pulling is allowed now (enabled and not refreshing).</param>
		/// <param name="isAtTop">Whether the content is scrolled to its top edge.</param>
		public GtkPullGesture(global::Gtk.Widget host, Func<bool> canPull, Func<bool> isAtTop)
		{
			_canPull = canPull;
			_isAtTop = isAtTop;
			_scroll = new global::Gtk.EventControllerScroll(global::Gtk.EventControllerScrollFlags.Vertical)
			{
				PropagationPhase = global::Gtk.PropagationPhase.Capture,
			};
			_scroll.Scroll += OnScroll;
			_scroll.ScrollEnd += (_, _) => _pull = 0;
			host.AddController(_scroll);
		}

		/// <summary>Raised when the user pulled far enough to request a refresh.</summary>
		public event Action? Requested;

		/// <summary>Forgets the accumulated pull.</summary>
		public void Reset() => _pull = 0;

		/// <summary>Feeds an upward scroll of <paramref name="pixels"/> (positive = pulling down) into the gesture.</summary>
		/// <param name="pixels">The scroll distance; zero or negative values (scrolling back) reset the pull.</param>
		/// <returns><see langword="true"/> when the scroll was consumed by the pull.</returns>
		public bool Feed(double pixels)
		{
			if (!_canPull())
				return false;

			if (pixels <= 0 || !_isAtTop())
			{
				_pull = 0;
				return false;
			}

			_pull += pixels;
			if (_pull >= Threshold)
			{
				_pull = 0;
				Requested?.Invoke();
			}

			return true;
		}

		void OnScroll(object o, global::Gtk.ScrollArgs args)
		{
			// dy < 0 scrolls up (towards the top edge): pulling down on the content.
			double pixels = -args.Dy * (_scroll.Unit == Gdk.ScrollUnit.Wheel ? WheelStep : 1);
			args.RetVal = Feed(pixels);
		}

		/// <summary>Whether the first scrolled window found in <paramref name="content"/> is at its top (true without one).</summary>
		/// <param name="content">The content widget.</param>
		public static bool IsAtTop(global::Gtk.Widget? content)
		{
			var scrolled = FindScrolledWindow(content);
			if (scrolled?.Vadjustment is not { } adjustment)
				return true;

			return adjustment.Value <= adjustment.Lower + 0.5;
		}

		static global::Gtk.ScrolledWindow? FindScrolledWindow(global::Gtk.Widget? widget)
		{
			if (widget is global::Gtk.ScrolledWindow scrolled)
				return scrolled;

			for (var child = widget?.FirstChild; child is not null; child = child.NextSibling)
			{
				if (FindScrolledWindow(child) is { } found)
					return found;
			}

			return null;
		}
	}
}
