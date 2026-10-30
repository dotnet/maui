using System;
using Microsoft.Maui.Graphics;
using GtkWidget = Gtk.Widget;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// A two-pane container: a flyout and a detail. In <see cref="FlyoutBehavior.Locked"/> the flyout sits beside the detail; in
	/// <see cref="FlyoutBehavior.Flyout"/> it slides over the detail on a dimmed scrim; <see cref="FlyoutBehavior.Disabled"/> hides it.
	/// </summary>
	/// <remarks>
	/// Both panes are <see cref="MauiGtkFillHost"/>s. The panel is a custom widget (it allocates the panes itself, measure before
	/// allocate), so the slide is a plain transform of the flyout's allocation and needs no layout manager.
	/// </remarks>
	public sealed class MauiGtkFlyoutPanel : GtkWidget
	{
		/// <summary>The width used when <see cref="FlyoutWidth"/> is not specified.</summary>
		public const int DefaultFlyoutWidth = 320;

		const uint StepMilliseconds = 16;

		readonly MauiGtkFillHost _flyout = new();
		readonly MauiGtkFillHost _detail = new();
		FlyoutBehavior _behavior = FlyoutBehavior.Flyout;
		double _flyoutWidth = -1;
		bool _presented;
		double _progress;
		uint _animation;
		DateTime _animationStart;
		double _animationFrom;

		/// <summary>Creates the panel.</summary>
		public MauiGtkFlyoutPanel()
		{
			_detail.Parent = this;
			_flyout.Parent = this;
			_flyout.Visible = false;

			var click = new global::Gtk.GestureClick();
			click.Pressed += OnPressed;
			AddController(click);
		}

		/// <summary>The host of the flyout pane.</summary>
		public MauiGtkFillHost FlyoutHost => _flyout;

		/// <summary>The host of the detail pane.</summary>
		public MauiGtkFillHost DetailHost => _detail;

		/// <summary>Duration of the slide, in milliseconds (0 = instant).</summary>
		public uint AnimationDuration { get; set; } = 200;

		/// <summary>The current slide position: 0 (hidden) to 1 (fully shown).</summary>
		public double Progress => _progress;

		/// <summary>Raised when the user closed the flyout (scrim click or Escape), so the virtual view can follow.</summary>
		public event Action? DismissRequested;

		/// <summary>How the flyout is shown.</summary>
		public FlyoutBehavior Behavior
		{
			get => _behavior;
			set
			{
				if (_behavior == value)
					return;

				_behavior = value;
				ApplyTarget(animate: false);
				QueueResize();
			}
		}

		/// <summary>The flyout width in pixels; a value that is not positive uses <see cref="DefaultFlyoutWidth"/>.</summary>
		public double FlyoutWidth
		{
			get => _flyoutWidth;
			set
			{
				_flyoutWidth = value;
				QueueResize();
			}
		}

		/// <summary>Whether the flyout is shown (in <see cref="FlyoutBehavior.Flyout"/> mode).</summary>
		public bool IsPresented
		{
			get => _presented;
			set
			{
				if (_presented == value)
					return;

				_presented = value;
				ApplyTarget(animate: true);
			}
		}

		/// <summary>The effective flyout width for an allocation of <paramref name="available"/> pixels.</summary>
		int EffectiveWidth(int available)
		{
			int width = _flyoutWidth > 0 ? GtkInterop.ToPixels(_flyoutWidth) : DefaultFlyoutWidth;
			return Math.Max(0, Math.Min(width, available));
		}

		double Target =>
			_behavior == FlyoutBehavior.Locked ? 1 :
			_behavior == FlyoutBehavior.Disabled ? 0 :
			_presented ? 1 : 0;

		void ApplyTarget(bool animate)
		{
			double target = Target;
			StopAnimation();

			if (!animate || AnimationDuration == 0 || _behavior != FlyoutBehavior.Flyout || !Visible)
			{
				SetProgress(target);
				return;
			}

			if (Math.Abs(target - _progress) < 0.001)
				return;

			_animationFrom = _progress;
			_animationStart = DateTime.UtcNow;
			_animation = GLib.Timeout.Add(StepMilliseconds, () => Step(target));
		}

		bool Step(double target)
		{
			double t = Math.Min(1, (DateTime.UtcNow - _animationStart).TotalMilliseconds / AnimationDuration);
			double eased = 1 - Math.Pow(1 - t, 3);
			SetProgress(_animationFrom + ((target - _animationFrom) * eased));

			if (t >= 1)
			{
				_animation = 0;
				SetProgress(target);
				return false;
			}

			return true;
		}

		void StopAnimation()
		{
			if (_animation != 0)
			{
				GLib.Timeout.Remove(_animation);
				_animation = 0;
			}
		}

		void SetProgress(double value)
		{
			_progress = value;
			_flyout.Visible = _behavior != FlyoutBehavior.Disabled && (_behavior == FlyoutBehavior.Locked || value > 0);
			QueueAllocate();
			QueueDraw();
		}

		void OnPressed(object? sender, global::Gtk.PressedArgs args)
		{
			if (_behavior == FlyoutBehavior.Flyout && _presented && args.X > EffectiveWidth(Width))
				DismissRequested?.Invoke();
		}

		/// <inheritdoc/>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimum = natural = 0;
			minimumBaseline = naturalBaseline = -1;
		}

		/// <inheritdoc/>
		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			int flyoutWidth = EffectiveWidth(width);

			if (_behavior == FlyoutBehavior.Locked)
			{
				AllocateChild(_flyout, flyoutWidth, height, 0);
				AllocateChild(_detail, Math.Max(0, width - flyoutWidth), height, flyoutWidth);
				return;
			}

			AllocateChild(_detail, width, height, 0);

			if (_flyout.Visible)
				AllocateChild(_flyout, flyoutWidth, height, (int)Math.Round(-flyoutWidth * (1 - _progress)));
		}

		static void AllocateChild(GtkWidget child, int width, int height, int x)
		{
			// Measure before allocate; never allocate below the minimum.
			child.Measure(global::Gtk.Orientation.Horizontal, -1, out int minWidth, out _, out _, out _);
			width = Math.Max(width, minWidth);
			child.Measure(global::Gtk.Orientation.Vertical, width, out int minHeight, out _, out _, out _);
			height = Math.Max(height, minHeight);

			child.Allocate(width, height, -1, new Gsk.Transform().Translate(GtkInterop.ToGraphene(x, 0)));
		}

		/// <inheritdoc/>
		protected override void OnSnapshot(global::Gtk.Snapshot snapshot)
		{
			SnapshotChild(_detail, snapshot);

			if (!_flyout.Visible)
				return;

			if (_behavior == FlyoutBehavior.Flyout && _progress > 0)
			{
				var scrim = new Gdk.RGBA { Red = 0, Green = 0, Blue = 0, Alpha = (float)(0.4 * _progress) };
				var bounds = GtkInterop.ToGrapheneRect(0, 0, Width, Height);
				snapshot.AppendColor(scrim, bounds);
			}

			SnapshotChild(_flyout, snapshot);
		}
	}
}
