using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Draws MAUI <see cref="Paint"/> values (solid and gradient) into a GTK snapshot, for widgets that paint their own
	/// shapes (for example <see cref="Handlers.MauiGtkBorder"/>). Widgets that only need a rectangular background use CSS instead.
	/// </summary>
	public static class GtkPaintRenderer
	{
		/// <summary>Whether <paramref name="paint"/> is a paint type this renderer can draw.</summary>
		/// <param name="paint">The paint.</param>
		public static bool CanRender(Paint? paint) => paint switch
		{
			SolidPaint { Color: not null } => true,
			LinearGradientPaint { GradientStops.Length: > 0 } => true,
			RadialGradientPaint { GradientStops.Length: > 0 } => true,
			_ => false,
		};

		/// <summary>Appends nodes that fill <paramref name="bounds"/> with <paramref name="paint"/>.</summary>
		/// <param name="snapshot">The snapshot.</param>
		/// <param name="paint">The paint (check <see cref="CanRender"/> first).</param>
		/// <param name="bounds">The area to fill.</param>
		/// <param name="width">The width of the area in pixels.</param>
		/// <param name="height">The height of the area in pixels.</param>
		public static void Render(global::Gtk.Snapshot snapshot, Paint paint, Graphene.Rect bounds, int width, int height)
		{
			switch (paint)
			{
				case SolidPaint { Color: { } color }:
					snapshot.AppendColor(color.ToGdkRgba(), bounds);
					break;
				case LinearGradientPaint linear:
					RenderGradient(snapshot, bounds,
						new Cairo.LinearGradient(linear.StartPoint.X * width, linear.StartPoint.Y * height, linear.EndPoint.X * width, linear.EndPoint.Y * height),
						linear.GradientStops);
					break;
				case RadialGradientPaint radial:
					RenderGradient(snapshot, bounds,
						new Cairo.RadialGradient(
							radial.Center.X * width, radial.Center.Y * height, 0,
							radial.Center.X * width, radial.Center.Y * height, radial.Radius * Math.Max(width, height)),
						radial.GradientStops);
					break;
			}
		}

		// The GSK gradient entry points take a single colour stop in the current bindings, so gradients are
		// painted through a Cairo node (still part of the render tree, clipped by the caller).
		static void RenderGradient(global::Gtk.Snapshot snapshot, Graphene.Rect bounds, Cairo.Gradient pattern, PaintGradientStop[] stops)
		{
			using (pattern)
			{
				foreach (var stop in stops)
					pattern.AddColorStop(stop.Offset, new Cairo.Color(stop.Color.Red, stop.Color.Green, stop.Color.Blue, stop.Color.Alpha));

				using var context = snapshot.AppendCairo(bounds);
				context.SetSource(pattern);
				context.Paint();
			}
		}
	}
}
