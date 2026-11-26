#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Describes how a page arrives and leaves when it is pushed, popped or presented.
	/// </summary>
	/// <remarks>
	/// <para>The hierarchy is closed: the built-in transitions are <see cref="FadeTransition"/>,
	/// <see cref="SlideTransition"/> and <see cref="ScaleTransition"/>, plus the shared
	/// <see cref="Platform"/> and <see cref="None"/> values. Custom motion is supplied through an
	/// <see cref="INavigationTransitionProvider"/>.</para>
	/// <para>A transition's values are snapshotted when a navigation starts, so changing a property
	/// afterwards never affects a navigation already running. Going back from a page reverses the
	/// transition that actually ran when the page was shown.</para>
	/// </remarks>
	[TypeConverter(typeof(PageTransitionTypeConverter))]
	public abstract class PageTransition
	{
		internal static readonly TimeSpan DefaultDuration = TimeSpan.FromMilliseconds(300);

		TimeSpan _duration = DefaultDuration;
		Easing? _easing;
		Brush? _background;
		bool _frozen;

		private protected PageTransition()
		{
		}

		/// <summary>
		/// The platform's own navigation animation, unchanged. This is the default everywhere.
		/// </summary>
		public static PageTransition Platform { get; } = new PlatformPageTransition();

		/// <summary>
		/// No animation.
		/// </summary>
		public static PageTransition None { get; } = new NonePageTransition();

		/// <summary>
		/// How long the transition runs. Defaults to 300 milliseconds. Ignored by <see cref="Platform"/> and <see cref="None"/>.
		/// </summary>
		public TimeSpan Duration
		{
			get => _duration;
			set
			{
				ThrowIfFrozen();
				if (value < TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "Duration can't be negative.");
				}

				_duration = value;
			}
		}

		/// <summary>
		/// The curve the transition follows. <see langword="null"/> (the default) uses the platform's
		/// standard navigation curve. Curves with a native form run natively; any other curve is sampled.
		/// </summary>
		public Easing? Easing
		{
			get => _easing;
			set
			{
				ThrowIfFrozen();
				_easing = value;
			}
		}

		/// <summary>
		/// Shown behind the pages while the transition runs, where neither page covers the screen.
		/// <see langword="null"/> (the default) uses the incoming page's background.
		/// </summary>
		public Brush? Background
		{
			get => _background;
			set
			{
				ThrowIfFrozen();
				_background = value;
			}
		}

		internal abstract PageTransitionKind Kind { get; }

		internal bool IsPlatform => Kind == PageTransitionKind.Platform;

		internal bool IsNone => Kind == PageTransitionKind.None;

		/// <summary>
		/// A copy of the transition's current values, taken when a navigation starts.
		/// </summary>
		internal PageTransition Snapshot()
		{
			if (_frozen)
			{
				return this;
			}

			var copy = (PageTransition)MemberwiseClone();
			copy._frozen = true;
			return copy;
		}

		private protected void ThrowIfFrozen()
		{
			if (_frozen)
			{
				throw new InvalidOperationException($"This {nameof(PageTransition)} is a shared or in-use snapshot and can't be changed. Create a transition of your own instead.");
			}
		}

		sealed class PlatformPageTransition : PageTransition
		{
			public PlatformPageTransition() => _frozen = true;

			internal override PageTransitionKind Kind => PageTransitionKind.Platform;

			public override string ToString() => nameof(Platform);
		}

		sealed class NonePageTransition : PageTransition
		{
			public NonePageTransition()
			{
				_duration = TimeSpan.Zero;
				_frozen = true;
			}

			internal override PageTransitionKind Kind => PageTransitionKind.None;

			public override string ToString() => nameof(None);
		}
	}

	/// <summary>
	/// The incoming page fades in over the outgoing one; going back, the outgoing page fades out.
	/// </summary>
	public sealed class FadeTransition : PageTransition
	{
		internal override PageTransitionKind Kind => PageTransitionKind.Fade;

		/// <inheritdoc/>
		public override string ToString() => "Fade";
	}

	/// <summary>
	/// The incoming page slides in from an edge while the outgoing page moves a little the other way.
	/// Going back, the page leaves toward the same edge.
	/// </summary>
	public sealed class SlideTransition : PageTransition
	{
		SlideEdge _from = SlideEdge.End;

		/// <summary>
		/// The edge the page comes in from. <see cref="SlideEdge.Start"/> and <see cref="SlideEdge.End"/>
		/// follow the page's <see cref="FlowDirection"/>. Defaults to <see cref="SlideEdge.End"/>.
		/// </summary>
		public SlideEdge From
		{
			get => _from;
			set
			{
				ThrowIfFrozen();
				_from = value;
			}
		}

		internal override PageTransitionKind Kind => PageTransitionKind.Slide;

		/// <inheritdoc/>
		public override string ToString() => "SlideFrom" + _from.ToString();
	}

	/// <summary>
	/// The incoming page grows in from slightly smaller while fading in; going back, it shrinks and fades out.
	/// </summary>
	public sealed class ScaleTransition : PageTransition
	{
		double _fromScale = 0.92;

		/// <summary>
		/// The scale the incoming page starts at: a finite number, zero or greater. Defaults to 0.92.
		/// </summary>
		public double FromScale
		{
			get => _fromScale;
			set
			{
				ThrowIfFrozen();
				if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "FromScale must be a finite number, zero or greater.");
				}

				_fromScale = value;
			}
		}

		internal override PageTransitionKind Kind => PageTransitionKind.Scale;

		/// <inheritdoc/>
		public override string ToString() => "Scale";
	}

	/// <summary>
	/// The edge a <see cref="SlideTransition"/> comes in from.
	/// </summary>
	public enum SlideEdge
	{
		/// <summary>The leading edge: left in left-to-right layouts, right in right-to-left layouts.</summary>
		Start,

		/// <summary>The trailing edge: right in left-to-right layouts, left in right-to-left layouts.</summary>
		End,

		/// <summary>The top edge.</summary>
		Top,

		/// <summary>The bottom edge.</summary>
		Bottom,
	}

	internal enum PageTransitionKind
	{
		Platform,
		None,
		Fade,
		Slide,
		Scale,
	}

	/// <summary>
	/// Converts <c>Platform</c>, <c>None</c>, <c>Fade</c>, <c>SlideFromEnd</c>, <c>SlideFromStart</c>,
	/// <c>SlideFromTop</c>, <c>SlideFromBottom</c> and <c>Scale</c> to a <see cref="PageTransition"/>.
	/// </summary>
	public sealed class PageTransitionTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <inheritdoc/>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var text = (value as string ?? value?.ToString())?.Trim();
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			if (TryParse(text!, out var transition))
			{
				return transition;
			}

			throw new InvalidOperationException($"Cannot convert \"{text}\" into {typeof(PageTransition)}.");
		}

		/// <inheritdoc/>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is PageTransition transition)
			{
				return transition.ToString();
			}

			throw new NotSupportedException();
		}

		internal static bool TryParse(string text, out PageTransition? transition)
		{
			transition = text.ToUpperInvariant() switch
			{
				"PLATFORM" or "DEFAULT" => PageTransition.Platform,
				"NONE" => PageTransition.None,
				"FADE" => new FadeTransition(),
				"SLIDE" or "SLIDEFROMEND" => new SlideTransition { From = SlideEdge.End },
				"SLIDEFROMSTART" => new SlideTransition { From = SlideEdge.Start },
				"SLIDEFROMTOP" => new SlideTransition { From = SlideEdge.Top },
				"SLIDEFROMBOTTOM" => new SlideTransition { From = SlideEdge.Bottom },
				"SCALE" => new ScaleTransition(),
				_ => null,
			};

			return transition is not null;
		}
	}
}
