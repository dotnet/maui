using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// The platform-agnostic state machine behind <see cref="ScrollAnchorMode"/>: decides from a view's scroll position
	/// which edge of its content it follows, if any, and when to scroll to keep that edge in view.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A platform reports <see cref="ScrollAnchorMetrics"/> after it has laid out a change (so a content size change and
	/// the offset adjustments the platform makes in the same layout pass arrive together) and after the view scrolled,
	/// plus whether the user is scrolling it (dragging or flinging). When <see cref="Update"/> returns an offset, the
	/// platform scrolls there without animating: it is always the edge the view follows.
	/// </para>
	/// <para>The rules, for each edge the view can follow (<see cref="Edges"/>):</para>
	/// <list type="bullet">
	/// <item><description>
	/// The anchor starts following the end if it can, otherwise the start, so a view shows that edge when it first appears.
	/// </description></item>
	/// <item><description>
	/// While it follows an edge, it scrolls to that edge whenever the content or the viewport changes size: items added or
	/// removed, any item growing or shrinking, a header or footer changing, the viewport resizing. A scroll alone never
	/// makes it scroll, so it never fights a scroll in progress.
	/// </description></item>
	/// <item><description>
	/// A user scroll stops it following the edge once the user has moved the viewport more than <see cref="Threshold"/>
	/// away from it. Content growing while the user scrolls doesn't count as the user moving away, and neither does the
	/// viewport springing back from past the edge (a bounce), nor the platform moving the offset while it lays out.
	/// </description></item>
	/// <item><description>
	/// An offset change the user isn't making that comes with a size change is the platform laying out, and never stops it
	/// following the edge, even when the sizes end up where they started (the platform reports the change in between),
	/// unless the platform says the view scrolled (a scroll can bring rows into view that the platform measures as it
	/// scrolls). One that comes alone and moves away from the edge is a deliberate scroll the platform doesn't report as
	/// the user's (the keyboard, accessibility, scrolling to the top): it stops following, also when it moves in steps
	/// smaller than a device pixel.
	/// </description></item>
	/// <item><description>
	/// A scroll the app requests (<c>ScrollTo</c>) is judged by where it lands: it follows an edge only if that's where it
	/// went.
	/// </description></item>
	/// <item><description>
	/// The viewport being at an edge it can follow, within <see cref="Threshold"/>, makes it follow that edge again,
	/// however it got there. A view whose content fits is at both edges: it follows the end.
	/// </description></item>
	/// </list>
	/// </remarks>
	sealed class ScrollAnchorTracker
	{
		/// <summary>The default for how close to an edge, in device-independent units, still counts as at it.</summary>
		public const double DefaultThreshold = 8;

		// Smaller differences are rounding to device pixels, not movement.
		const double Tolerance = 0.5;

		// Smaller differences in size are floating-point noise: content can grow by a device pixel at a time, which is less
		// than Tolerance, and those changes add up.
		const double SizeTolerance = 0.01;

		const ScrollAnchorMode BothEdges = ScrollAnchorMode.Start | ScrollAnchorMode.End;

		ScrollAnchorMetrics? _last;
		bool _lastWasUserScroll;
		double _drift;
		bool _scrollRequested;

		/// <summary>Initializes a new tracker, following the edge a new view shows.</summary>
		/// <param name="edges">The edges the view can follow. Other flags are ignored.</param>
		/// <param name="threshold">How close to an edge, in device-independent units, still counts as at it.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="threshold"/> is negative or not a number.</exception>
		public ScrollAnchorTracker(ScrollAnchorMode edges, double threshold = DefaultThreshold)
		{
			if (double.IsNaN(threshold) || threshold < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(threshold), threshold, "The threshold must be zero or more.");
			}

			Threshold = threshold;
			Edges = edges & BothEdges;
			AnchoredEdge = InitialEdge;
		}

		/// <summary>Gets how close to an edge, in device-independent units, still counts as at it.</summary>
		public double Threshold { get; }

		/// <summary>Gets the edges the view can follow: <see cref="ScrollAnchorMode.Start"/>, <see cref="ScrollAnchorMode.End"/>, or both.</summary>
		public ScrollAnchorMode Edges { get; private set; }

		/// <summary>
		/// Gets the edge the view follows: <see cref="ScrollAnchorMode.Start"/>, <see cref="ScrollAnchorMode.End"/>, or
		/// <see cref="ScrollAnchorMode.None"/>.
		/// </summary>
		public ScrollAnchorMode AnchoredEdge { get; private set; }

		/// <summary>Gets whether the anchor is engaged: the view follows an edge of its content.</summary>
		public bool IsAnchored => AnchoredEdge != ScrollAnchorMode.None;

		/// <summary>Gets whether the tracker has seen where the view is since it started over.</summary>
		public bool HasPosition => _last is not null;

		// A new view shows its end where it follows it, and otherwise its start.
		ScrollAnchorMode InitialEdge => CanFollow(ScrollAnchorMode.End) ? ScrollAnchorMode.End
			: CanFollow(ScrollAnchorMode.Start) ? ScrollAnchorMode.Start
			: ScrollAnchorMode.None;

		/// <summary>Gets whether the view can follow <paramref name="edge"/>: <see cref="ScrollAnchorMode.Start"/> or <see cref="ScrollAnchorMode.End"/>.</summary>
		public bool CanFollow(ScrollAnchorMode edge) =>
			(edge == ScrollAnchorMode.Start || edge == ScrollAnchorMode.End) && (Edges & edge) != 0;

		/// <summary>Changes the edges the view can follow, and starts over as for a new view.</summary>
		/// <param name="edges">The edges the view can follow. Other flags are ignored.</param>
		public void SetEdges(ScrollAnchorMode edges)
		{
			Edges = edges & BothEdges;
			Reset();
		}

		/// <summary>
		/// Starts over with no position seen yet, following the edge a new view shows: for a new platform view or a new set
		/// of items.
		/// </summary>
		public void Reset() => Reset(InitialEdge);

		/// <summary>Starts over with no position seen yet, following <paramref name="edge"/>: the next update scrolls to it.</summary>
		/// <param name="edge">The edge to follow; one the view can't follow follows none.</param>
		public void Reset(ScrollAnchorMode edge)
		{
			_last = null;
			_lastWasUserScroll = false;
			_drift = 0;
			_scrollRequested = false;
			AnchoredEdge = CanFollow(edge) ? edge : ScrollAnchorMode.None;
		}

		/// <summary>
		/// Notes that the app requested a scroll (such as <c>ScrollTo</c>): the next update judges where it landed, following
		/// an edge only if it's there, instead of scrolling the view back.
		/// </summary>
		public void RequestScroll() => _scrollRequested = true;

		/// <summary>
		/// Follows <paramref name="edge"/> for an animated scroll to it that the platform is about to start: the animation
		/// only moves toward that edge, which never stops it following, and a size change on the way scrolls straight to the
		/// edge. It supersedes any scroll the user was making.
		/// </summary>
		/// <param name="edge">The edge to follow; one the view can't follow follows none.</param>
		public void Engage(ScrollAnchorMode edge)
		{
			AnchoredEdge = CanFollow(edge) ? edge : ScrollAnchorMode.None;
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
		/// <param name="scrolled">
		/// Whether the view scrolled since the last update, rather than the platform laying it out: its offset change
		/// counts as movement even when the content changed size meanwhile, as when a platform measures the rows a scroll
		/// brings into view as it scrolls.
		/// </param>
		/// <returns>The offset of the edge to scroll to without animating, or <see langword="null"/> to stay.</returns>
		public double? Update(ScrollAnchorMetrics metrics, bool isUserScrolling, bool sizeChanged = false, bool scrolled = false)
		{
			if (!metrics.IsValid)
			{
				return null;
			}

			var last = _last;
			var wasUserScroll = _lastWasUserScroll;
			_last = metrics;
			_lastWasUserScroll = isUserScrolling;
			var edge = EdgeAt(metrics);

			// A view at both edges, its content fitting, follows the end.
			if (AnchoredEdge == ScrollAnchorMode.Start && edge == ScrollAnchorMode.End && IsAt(ScrollAnchorMode.Start, metrics))
			{
				AnchoredEdge = ScrollAnchorMode.End;
				_drift = 0;
			}

			if (_scrollRequested)
			{
				_scrollRequested = false;
				if (!isUserScrolling)
				{
					// The app scrolled the view: follow an edge again only if that's where it went.
					AnchoredEdge = edge;
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
					// An offset change the platform reports making while it laid out isn't the user's. Sizes that merely
					// differ don't say that: a platform can estimate the extent of content it hasn't laid out.
					if (!sizeChanged)
					{
						_drift = Math.Max(0, _drift + UserMovementAway(previous, metrics));
					}

					if (!IsAt(AnchoredEdge, metrics) && _drift > Threshold)
					{
						AnchoredEdge = ScrollAnchorMode.None;
					}
				}

				// Back at an edge, or scrolled from one edge to the other.
				if (!IsAnchored && edge != ScrollAnchorMode.None)
				{
					AnchoredEdge = edge;
					_drift = 0;
				}

				return null;
			}

			var resized = sizeChanged || Resized(metrics.Extent, previous.Extent) || Resized(metrics.Viewport, previous.Viewport);

			// The offset change is the platform's when it says it laid out a change, or when the size changed and nothing
			// says the view scrolled.
			var laidOut = sizeChanged || (resized && !scrolled);
			if (IsAnchored)
			{
				// Movement away from the edge since the view was last at it, that the platform didn't make laying out: a
				// scroll can move the view in steps too small to tell from rounding, which add up.
				_drift = laidOut || IsAt(AnchoredEdge, metrics) || wasUserScroll
					? 0
					: Math.Max(0, _drift + MovementAway(previous, metrics));
				if (_drift > Tolerance)
				{
					// The view isn't at the edge it followed: it follows the other edge only if it's there.
					AnchoredEdge = edge;
					_drift = 0;
					return null;
				}

				if (resized)
				{
					// The correction below takes the view back to the edge.
					_drift = 0;
				}
			}
			else if (edge != ScrollAnchorMode.None)
			{
				AnchoredEdge = edge;
				_drift = 0;
			}

			// Scroll to the edge for a size change, or when a user scroll just ended (the edge may have moved meanwhile).
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

			var target = AnchoredEdge == ScrollAnchorMode.End ? metrics.EndOffset : 0;
			if (!Differs(target, metrics.Offset))
			{
				return null;
			}

			// The platform scrolls there before anything else happens: measure the next change from there, so this scroll
			// isn't mistaken for part of the next one.
			_last = metrics.WithOffset(target);
			return target;
		}

		bool IsAt(ScrollAnchorMode edge, ScrollAnchorMetrics metrics) => edge switch
		{
			ScrollAnchorMode.Start => metrics.Offset <= Threshold,
			ScrollAnchorMode.End => metrics.DistanceToEnd <= Threshold,
			_ => false,
		};

		// The edge the view is at that it can follow: the end, when the content fits and the view is at both.
		ScrollAnchorMode EdgeAt(ScrollAnchorMetrics metrics) =>
			CanFollow(ScrollAnchorMode.End) && IsAt(ScrollAnchorMode.End, metrics) ? ScrollAnchorMode.End
			: CanFollow(ScrollAnchorMode.Start) && IsAt(ScrollAnchorMode.Start, metrics) ? ScrollAnchorMode.Start
			: ScrollAnchorMode.None;

		// How far the user moved the view away from the edge it follows: moving back toward it counts against that, and
		// neither content growing beyond the viewport nor springing back from past the edge (a bounce) adds to it. Only
		// movement short of the edge counts, and with the content resizing in between, short of both ends.
		double UserMovementAway(ScrollAnchorMetrics previous, ScrollAnchorMetrics metrics)
		{
			if (AnchoredEdge == ScrollAnchorMode.Start)
			{
				return Math.Max(0, metrics.Offset) - Math.Max(0, previous.Offset);
			}

			var end = Math.Min(previous.EndOffset, metrics.EndOffset);
			return Math.Min(previous.Offset, end) - Math.Min(metrics.Offset, end);
		}

		double MovementAway(ScrollAnchorMetrics previous, ScrollAnchorMetrics metrics) =>
			AnchoredEdge == ScrollAnchorMode.Start ? metrics.Offset - previous.Offset : previous.Offset - metrics.Offset;

		static bool Differs(double a, double b) => Math.Abs(a - b) > Tolerance;

		static bool Resized(double a, double b) => Math.Abs(a - b) > SizeTolerance;
	}
}
