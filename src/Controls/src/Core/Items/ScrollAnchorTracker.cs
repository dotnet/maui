using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// The platform-agnostic state machine behind <see cref="ScrollAnchorMode.End"/>: decides from a view's scroll
	/// position whether it follows the end of its content, and when to scroll to keep the end in view.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A platform reports <see cref="ScrollAnchorMetrics"/> after it has laid out a change (so a content size change and
	/// the offset adjustments the platform makes in the same layout pass arrive together) and after the view scrolled,
	/// plus whether the user is scrolling it (dragging or flinging). When <see cref="Update"/> returns an offset, the
	/// platform scrolls there without animating: it is always the end of the content.
	/// </para>
	/// <para>The rules:</para>
	/// <list type="bullet">
	/// <item><description>The anchor starts engaged, so a view shows its end when it first appears.</description></item>
	/// <item><description>
	/// While engaged, it scrolls to the end whenever the content or the viewport changes size: items added or removed,
	/// any item growing or shrinking, a header or footer changing, the viewport resizing. A scroll alone never makes it
	/// scroll, so it never fights a scroll in progress.
	/// </description></item>
	/// <item><description>
	/// A user scroll disengages it once the user has moved the viewport more than <see cref="Threshold"/> away from the
	/// end. Content growing while the user scrolls doesn't count as the user moving away, and neither does the viewport
	/// springing back from past the end (a bounce), nor the platform moving the offset while it lays out.
	/// </description></item>
	/// <item><description>
	/// An offset change the user isn't making that comes with a size change is the platform laying out, and never
	/// disengages it, even when the sizes end up where they started (the platform reports the change in between). One that
	/// comes alone and moves away from the end is a deliberate scroll the platform doesn't report as the user's (the
	/// keyboard, accessibility, scrolling to the top): it disengages it, also when it moves in steps smaller than a device
	/// pixel.
	/// </description></item>
	/// <item><description>
	/// A scroll the app requests (<c>ScrollTo</c>) is judged by where it lands: engaged only if that's the end.
	/// </description></item>
	/// <item><description>
	/// The viewport being at the end, within <see cref="Threshold"/>, engages it again, however it got there.
	/// </description></item>
	/// </list>
	/// </remarks>
	sealed class ScrollAnchorTracker
	{
		/// <summary>The default for how close to the end, in device-independent units, still counts as at the end.</summary>
		public const double DefaultThreshold = 8;

		// Smaller differences are rounding to device pixels, not movement.
		const double Tolerance = 0.5;

		// Smaller differences in size are floating-point noise: content can grow by a device pixel at a time, which is less
		// than Tolerance, and those changes add up.
		const double SizeTolerance = 0.01;

		ScrollAnchorMetrics? _last;
		bool _lastWasUserScroll;
		double _drift;
		bool _scrollRequested;

		/// <summary>Initializes a new tracker, engaged.</summary>
		/// <param name="threshold">How close to the end, in device-independent units, still counts as at the end.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="threshold"/> is negative or not a number.</exception>
		public ScrollAnchorTracker(double threshold = DefaultThreshold)
		{
			if (double.IsNaN(threshold) || threshold < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(threshold), threshold, "The threshold must be zero or more.");
			}

			Threshold = threshold;
		}

		/// <summary>Gets how close to the end, in device-independent units, still counts as at the end.</summary>
		public double Threshold { get; }

		/// <summary>Gets whether the anchor is engaged: the view follows the end of its content.</summary>
		public bool IsAnchored { get; private set; } = true;

		/// <summary>Starts over, engaged, with no position seen yet: for a new platform view or a new set of items.</summary>
		public void Reset()
		{
			_last = null;
			_lastWasUserScroll = false;
			_drift = 0;
			_scrollRequested = false;
			IsAnchored = true;
		}

		/// <summary>
		/// Notes that the app requested a scroll (such as <c>ScrollTo</c>): the next update judges where it landed, engaging
		/// the anchor only at the end, instead of scrolling the view back.
		/// </summary>
		public void RequestScroll() => _scrollRequested = true;

		/// <summary>
		/// Engages the anchor for an animated scroll to the end that the platform is about to start: the animation only moves
		/// toward the end, which never disengages it, and a size change on the way scrolls straight to the end. It supersedes
		/// any scroll the user was making.
		/// </summary>
		public void Engage()
		{
			IsAnchored = true;
			_drift = 0;
			_scrollRequested = false;
			_lastWasUserScroll = false;
		}

		/// <summary>Observes where the view is, after the platform laid out a change or the view scrolled.</summary>
		/// <param name="metrics">The view's current position.</param>
		/// <param name="isUserScrolling">Whether the user is scrolling the view: dragging or flinging it.</param>
		/// <param name="sizeChanged">
		/// Whether the platform reported that it laid out a change since the last update that the metrics may not show: a
		/// platform can resize its content and resize it back in one layout pass (re-estimating the rows it measured
		/// before), moving the offset as it goes. The metrics then show the same size and a new offset, which this says is
		/// the layout's doing. While the user scrolls, the offset change it comes with doesn't count as the user moving.
		/// </param>
		/// <returns>The end offset to scroll to without animating, or <see langword="null"/> to stay.</returns>
		public double? Update(ScrollAnchorMetrics metrics, bool isUserScrolling, bool sizeChanged = false)
		{
			if (!metrics.IsValid)
			{
				return null;
			}

			var last = _last;
			var wasUserScroll = _lastWasUserScroll;
			_last = metrics;
			_lastWasUserScroll = isUserScrolling;
			var atEnd = metrics.DistanceToEnd <= Threshold;

			if (_scrollRequested)
			{
				_scrollRequested = false;
				if (!isUserScrolling)
				{
					// The app scrolled the view: follow the end again only if that's where it went.
					IsAnchored = atEnd;
					_drift = 0;
					return null;
				}

				// An animated request scrolls like a fling: judge it as one.
			}

			if (last is not ScrollAnchorMetrics previous)
			{
				return isUserScrolling ? null : Correction(metrics);
			}

			if (isUserScrolling)
			{
				if (IsAnchored)
				{
					// The user's net movement away from the end in this gesture: moving back toward it cancels it out, and
					// neither content growing below the viewport nor springing back from past the end (a bounce) adds to
					// it. Only movement short of the end counts, and with the content resizing in between, short of both
					// ends. An offset change the platform reports making while it laid out isn't the user's. Sizes that
					// merely differ don't say that: a platform can estimate the extent of content it hasn't laid out.
					if (!sizeChanged)
					{
						var end = Math.Min(previous.EndOffset, metrics.EndOffset);
						_drift = Math.Max(0, _drift - (Math.Min(metrics.Offset, end) - Math.Min(previous.Offset, end)));
					}

					if (!atEnd && _drift > Threshold)
					{
						IsAnchored = false;
					}
				}
				else if (atEnd)
				{
					IsAnchored = true;
					_drift = 0;
				}

				return null;
			}

			var resized = sizeChanged || Resized(metrics.Extent, previous.Extent) || Resized(metrics.Viewport, previous.Viewport);
			if (IsAnchored)
			{
				// Movement away from the end since the view was last at it, that the platform didn't make laying out: a scroll
				// can move the view in steps too small to tell from rounding, which add up.
				_drift = resized || atEnd || wasUserScroll ? 0 : Math.Max(0, _drift - (metrics.Offset - previous.Offset));
				if (_drift > Tolerance)
				{
					IsAnchored = false;
					_drift = 0;
					return null;
				}
			}
			else if (atEnd)
			{
				IsAnchored = true;
				_drift = 0;
			}

			// Scroll to the end for a size change, or when a user scroll just ended (the end may have moved meanwhile).
			if (!resized && !wasUserScroll)
			{
				return null;
			}

			return Correction(metrics);
		}

		double? Correction(ScrollAnchorMetrics metrics)
		{
			if (!IsAnchored || metrics.Viewport <= 0)
			{
				return null;
			}

			var target = metrics.EndOffset;
			if (!Differs(target, metrics.Offset))
			{
				return null;
			}

			// The platform scrolls there before anything else happens: measure the next change from there, so this scroll
			// isn't mistaken for part of the next one.
			_last = metrics.WithOffset(target);
			return target;
		}

		static bool Differs(double a, double b) => Math.Abs(a - b) > Tolerance;

		static bool Resized(double a, double b) => Math.Abs(a - b) > SizeTolerance;
	}
}
