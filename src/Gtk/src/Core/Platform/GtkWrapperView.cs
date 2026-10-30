using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// A single-child container (the "Bin rule": GTK 4 widgets that decorate another widget hold exactly one child) used
	/// for the per-view features that GTK cannot do on the widget itself: clipping to an arbitrary <see cref="IShape"/>
	/// and drop shadows. It is what <c>IViewHandler.ContainerView</c> is on this backend.
	/// </summary>
	public sealed class GtkWrapperView : global::Gtk.Widget, IGtkChildHost
	{
		global::Gtk.Widget? _child;
		IShape? _clip;
		IShadow? _shadow;
		Gsk.Path? _clipPath;
		int _clipWidth = -1;
		int _clipHeight = -1;

		/// <summary>The wrapped widget.</summary>
		public global::Gtk.Widget? Child
		{
			get => _child;
			set
			{
				if (ReferenceEquals(_child, value))
					return;

				_child?.Unparent();
				_child = value;
				if (value is not null)
					value.Parent = this;

				QueueResize();
			}
		}

		/// <summary>The clip applied to the child, or <see langword="null"/>.</summary>
		public IShape? Clip
		{
			get => _clip;
			set
			{
				_clip = value;
				_clipPath = null;
				_clipWidth = _clipHeight = -1;
				QueueDraw();
			}
		}

		/// <summary>The shadow drawn behind the child, or <see langword="null"/>.</summary>
		public IShadow? Shadow
		{
			get => _shadow;
			set
			{
				_shadow = value;
				QueueDraw();
			}
		}

		/// <inheritdoc/>
		protected override global::Gtk.SizeRequestMode OnGetRequestMode() =>
			_child?.RequestMode ?? global::Gtk.SizeRequestMode.ConstantSize;

		/// <inheritdoc/>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			if (_child is null)
			{
				minimum = natural = 0;
				minimumBaseline = naturalBaseline = -1;
				return;
			}

			_child.Measure(orientation, forSize, out minimum, out natural, out minimumBaseline, out naturalBaseline);
		}

		/// <inheritdoc/>
		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			_child?.Allocate(width, height, baseline, null);
		}

		/// <inheritdoc/>
		protected override void OnSnapshot(global::Gtk.Snapshot snapshot)
		{
			var child = _child;
			if (child is null)
				return;

			int width = Width;
			int height = Height;

			if (_shadow is { } shadow && shadow.Paint is SolidPaint solid && solid.Color is { } color && width > 0 && height > 0)
			{
				var outline = new Gsk.RoundedRect(0, 0, width, height, 0);
				var tint = color.WithAlpha(color.Alpha * shadow.Opacity).ToGdkRgba();
				snapshot.AppendOutsetShadow(outline, tint, (float)shadow.Offset.X, (float)shadow.Offset.Y, 0, shadow.Radius);
			}

			var path = GetClipPath(width, height);
			if (path is not null)
			{
				snapshot.PushFill(path, Gsk.FillRule.Winding);
				SnapshotChild(child, snapshot);
				snapshot.Pop();
			}
			else
			{
				SnapshotChild(child, snapshot);
			}
		}

		Gsk.Path? GetClipPath(int width, int height)
		{
			if (_clip is null || width <= 0 || height <= 0)
				return null;

			if (_clipPath is null || width != _clipWidth || height != _clipHeight)
			{
				_clipPath = GtkPathConverter.ToGskPath(_clip.PathForBounds(new Rect(0, 0, width, height)));
				_clipWidth = width;
				_clipHeight = height;
			}

			return _clipPath;
		}

		/// <inheritdoc/>
		public void ReplaceChild(global::Gtk.Widget oldChild, global::Gtk.Widget newChild)
		{
			if (ReferenceEquals(_child, oldChild))
				Child = newChild;
		}
	}

	/// <summary>Converts MAUI <see cref="PathF"/> geometry to a <see cref="Gsk.Path"/>.</summary>
	public static class GtkPathConverter
	{
		/// <summary>Builds the <see cref="Gsk.Path"/> equivalent of <paramref name="path"/> (arcs are flattened to line segments).</summary>
		/// <param name="path">The MAUI path.</param>
		public static Gsk.Path ToGskPath(PathF path)
		{
			if (path is null)
				throw new ArgumentNullException(nameof(path));

			var builder = new Gsk.PathBuilder();
			var source = path;

			for (int i = 0; i < source.OperationCount; i++)
			{
				if (source.GetSegmentType(i) == PathOperation.Arc)
				{
					source = source.GetFlattenedPath(0.25f, true);
					break;
				}
			}

			for (int i = 0; i < source.OperationCount; i++)
			{
				var points = source.GetPointsForSegment(i);
				switch (source.GetSegmentType(i))
				{
					case PathOperation.Move:
						builder.MoveTo(points[0].X, points[0].Y);
						break;
					case PathOperation.Line:
						builder.LineTo(points[0].X, points[0].Y);
						break;
					case PathOperation.Quad:
						builder.QuadTo(points[0].X, points[0].Y, points[1].X, points[1].Y);
						break;
					case PathOperation.Cubic:
						builder.CubicTo(points[0].X, points[0].Y, points[1].X, points[1].Y, points[2].X, points[2].Y);
						break;
					case PathOperation.Close:
						builder.Close();
						break;
				}
			}

			return builder.ToPath();
		}
	}
}
