#nullable enable
using System;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Progress and geometry shared by every platform's built-in page transitions, so the presets move the
	/// same way everywhere: a function of eased progress for each page.
	/// </summary>
	internal static class PageTransitionCurves
	{
		/// <summary>
		/// The standard navigation curve, <c>cubic-bezier(0.4, 0, 0.2, 1)</c>, used when a transition sets no <see cref="PageTransition.Easing"/>.
		/// </summary>
		internal static readonly Easing Standard = new(t => CubicBezier(0.4, 0.0, 0.2, 1.0, t));

		/// <summary>How far the outgoing page drifts during a slide, as a fraction of the page.</summary>
		internal const double SlideParallax = 0.3;

		/// <summary>The curve a transition follows: its own <see cref="PageTransition.Easing"/>, or <see cref="Standard"/>.</summary>
		internal static Easing For(PageTransition transition) => transition.Easing ?? Standard;

		/// <summary>
		/// The unit vector a slide comes in along: <c>(1, 0)</c> from the right, <c>(-1, 0)</c> from the left,
		/// <c>(0, 1)</c> from the bottom and <c>(0, -1)</c> from the top. Start and End follow <paramref name="flowDirection"/>.
		/// </summary>
		internal static (double X, double Y) SlideVector(SlideEdge edge, FlowDirection flowDirection)
		{
			var rtl = flowDirection == FlowDirection.RightToLeft;
			return edge switch
			{
				SlideEdge.Start => (rtl ? 1 : -1, 0),
				SlideEdge.Top => (0, -1),
				SlideEdge.Bottom => (0, 1),
				_ => (rtl ? -1 : 1, 0),
			};
		}

		/// <summary>A page's pose at one point of a transition.</summary>
		internal readonly struct Pose
		{
			public Pose(double translationX, double translationY, double scale, double opacity)
			{
				TranslationX = translationX;
				TranslationY = translationY;
				Scale = scale;
				Opacity = opacity;
			}

			/// <summary>Translation as a fraction of the page's width.</summary>
			public double TranslationX { get; }

			/// <summary>Translation as a fraction of the page's height.</summary>
			public double TranslationY { get; }

			public double Scale { get; }

			public double Opacity { get; }

			public static Pose Identity => new(0, 0, 1, 1);
		}

		/// <summary>
		/// The pose of the incoming (<paramref name="incoming"/> true) or outgoing page at eased
		/// <paramref name="progress"/> through a transition. Back transitions are the forward ones reversed.
		/// </summary>
		internal static Pose GetPose(PageTransition transition, bool isBack, bool incoming, double progress, FlowDirection flowDirection)
		{
			switch (transition)
			{
				case FadeTransition:
					// Forward: the incoming page fades in on top. Back: the outgoing page fades out on top.
					if (!isBack && incoming)
					{
						return new Pose(0, 0, 1, progress);
					}

					if (isBack && !incoming)
					{
						return new Pose(0, 0, 1, 1 - progress);
					}

					return Pose.Identity;

				case SlideTransition slide:
					var (vx, vy) = SlideVector(slide.From, flowDirection);
					if (!isBack)
					{
						return incoming
							? new Pose(vx * (1 - progress), vy * (1 - progress), 1, 1)
							: new Pose(-vx * SlideParallax * progress, -vy * SlideParallax * progress, 1, 1);
					}

					return incoming
						? new Pose(-vx * SlideParallax * (1 - progress), -vy * SlideParallax * (1 - progress), 1, 1)
						: new Pose(vx * progress, vy * progress, 1, 1);

				case ScaleTransition scale:
					var from = scale.FromScale;
					if (!isBack && incoming)
					{
						return new Pose(0, 0, from + (1 - from) * progress, progress);
					}

					if (isBack && !incoming)
					{
						return new Pose(0, 0, 1 - (1 - from) * progress, 1 - progress);
					}

					return Pose.Identity;

				default:
					return Pose.Identity;
			}
		}

		/// <summary>
		/// The pose of the new root page (<paramref name="incoming"/> true) or of the picture of the old root,
		/// which is drawn on top, when a window's root page is replaced.
		/// </summary>
		internal static Pose GetRootPose(PageTransition transition, bool incoming, double progress, FlowDirection flowDirection)
		{
			// The outgoing picture is drawn on top, so the incoming root stays opaque: its motion is kept, but only the
			// picture fades, which crossfades the two instead of letting the window background show through both.
			if (incoming)
			{
				var pose = GetPose(transition, false, true, progress, flowDirection);
				return new Pose(pose.TranslationX, pose.TranslationY, pose.Scale, 1);
			}

			switch (transition)
			{
				case SlideTransition slide:
					var (vx, vy) = SlideVector(slide.From, flowDirection);
					return new Pose(-vx * progress, -vy * progress, 1, 1);

				case ScaleTransition scale:
					return new Pose(0, 0, 1 + (1 - scale.FromScale) * progress, 1 - progress);

				default:
					return new Pose(0, 0, 1, 1 - progress);
			}
		}

		/// <summary>Whether the incoming page is drawn above the outgoing one. Back transitions keep the outgoing page on top.</summary>
		internal static bool IncomingOnTop(bool isBack) => !isBack;

		/// <summary>Evaluates <c>cubic-bezier(x1, y1, x2, y2)</c> at <paramref name="t"/>, as CSS does.</summary>
		internal static double CubicBezier(double x1, double y1, double x2, double y2, double t)
		{
			if (t <= 0)
			{
				return 0;
			}

			if (t >= 1)
			{
				return 1;
			}

			// Solve x(s) = t for s with Newton's method, falling back to bisection.
			double s = t;
			for (int i = 0; i < 8; i++)
			{
				var x = Bezier(x1, x2, s) - t;
				var dx = BezierSlope(x1, x2, s);
				if (Math.Abs(x) < 1e-6)
				{
					return Bezier(y1, y2, s);
				}

				if (Math.Abs(dx) < 1e-6)
				{
					break;
				}

				s -= x / dx;
			}

			double lo = 0, hi = 1;
			s = t;
			for (int i = 0; i < 32; i++)
			{
				var x = Bezier(x1, x2, s);
				if (Math.Abs(x - t) < 1e-6)
				{
					break;
				}

				if (x < t)
				{
					lo = s;
				}
				else
				{
					hi = s;
				}

				s = (lo + hi) / 2;
			}

			return Bezier(y1, y2, s);
		}

		static double Bezier(double p1, double p2, double s)
		{
			var u = 1 - s;
			return 3 * u * u * s * p1 + 3 * u * s * s * p2 + s * s * s;
		}

		static double BezierSlope(double p1, double p2, double s)
		{
			var u = 1 - s;
			return 3 * u * u * p1 + 6 * u * s * (p2 - p1) + 3 * s * s * (1 - p2);
		}
	}
}
