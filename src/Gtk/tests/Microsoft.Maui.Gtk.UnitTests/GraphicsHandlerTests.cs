using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>GraphicsView, ShapeView and IndicatorView: one test per mapped property, on real widgets (pixels are read back from the render).</summary>
	public class GraphicsHandlerTests
	{
		sealed class RedSquare : IDrawable
		{
			public int Draws;
			public RectF LastRect;

			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				Draws++;
				LastRect = dirtyRect;
				canvas.FillColor = Colors.Red;
				canvas.FillRectangle(0, 0, dirtyRect.Width / 2, dirtyRect.Height / 2);
			}
		}

		static string Css(global::Gtk.Widget widget) => GtkStyle.For(widget).ToCss();

		static (byte R, byte G, byte B) Pixel(global::Gtk.Widget widget, int x, int y) => GtkTestHost.PixelAt(widget, x, y);

		// ------------------------------------------------------------- GraphicsView

		[GtkFact]
		public void GraphicsViewDrawsItsDrawableWithTheCairoCanvas() => GtkTestHost.Run(() =>
		{
			var drawable = new RedSquare();
			var view = new GraphicsView { Drawable = drawable, WidthRequest = 80, HeightRequest = 60, HorizontalOptions = LayoutOptions.Start };

			using var host = new WindowHost(view, 200, 200);
			host.Pump();

			var widget = (MauiGtkGraphicsView)host.Widget(view);
			Assert.True(drawable.Draws > 0);
			Assert.Equal(80, drawable.LastRect.Width);
			Assert.Equal(60, drawable.LastRect.Height);

			var inside = Pixel(widget, 10, 10);
			Assert.True(inside.R > 200 && inside.G < 60 && inside.B < 60, $"inside is {inside}");
			var outside = Pixel(widget, 70, 50);
			Assert.False(outside.R > 200 && outside.G < 60 && outside.B < 60, $"outside is {outside}");
		});

		[GtkFact]
		public void GraphicsViewMapsDrawableAndInvalidate() => GtkTestHost.Run(() =>
		{
			var first = new RedSquare();
			var view = new GraphicsView { Drawable = first, WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Start };
			using var host = new WindowHost(view, 100, 100);
			host.Pump();
			var widget = (MauiGtkGraphicsView)host.Widget(view);
			Assert.Same(first, widget.Drawable);

			var second = new RedSquare();
			view.Drawable = second;
			Assert.Same(second, widget.Drawable);
			Assert.True(GtkTestHost.PumpUntil(() => second.Draws > 0, host.PlatformWindow), "the new drawable was never drawn");

			int before = second.Draws;
			view.Invalidate();
			Assert.True(GtkTestHost.PumpUntil(() => second.Draws > before, host.PlatformWindow), "Invalidate did not repaint");
		});

		[GtkFact]
		public void GraphicsViewMapsBackground() => GtkTestHost.Run(() =>
		{
			var view = new GraphicsView { Background = new SolidColorBrush(Colors.Yellow) };
			var widget = GtkTestApp.CreateWidget<MauiGtkGraphicsView>(view);
			Assert.Contains("background-color: rgba(255,255,0,1)", Css(widget));
		});

		[GtkFact]
		public void GraphicsViewMapsFlowDirection() => GtkTestHost.Run(() =>
		{
			var view = new GraphicsView { FlowDirection = FlowDirection.RightToLeft };
			var widget = GtkTestApp.CreateWidget<MauiGtkGraphicsView>(view);
			Assert.Equal(global::Gtk.TextDirection.Rtl, widget.Direction);
		});

		[GtkFact]
		public void GraphicsViewForwardsInteractions() => GtkTestHost.Run(() =>
		{
			var view = new GraphicsView { WidthRequest = 50, HeightRequest = 50 };
			int started = 0, dragged = 0, ended = 0, hoverStarted = 0, hoverMoved = 0, hoverEnded = 0;
			bool inside = false;
			view.StartInteraction += (s, e) => started++;
			view.DragInteraction += (s, e) => dragged++;
			view.EndInteraction += (s, e) => { ended++; inside = e.IsInsideBounds; };
			view.StartHoverInteraction += (s, e) => hoverStarted++;
			view.MoveHoverInteraction += (s, e) => hoverMoved++;
			view.EndHoverInteraction += (s, e) => hoverEnded++;
			var widget = GtkTestApp.CreateWidget<MauiGtkGraphicsView>(view);

			var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget)!;
			var motion = GtkTestHost.ControllerOf<global::Gtk.EventControllerMotion>(widget)!;

			GLib.Signal.Emit(drag, "drag-begin", 5.0, 5.0);
			GLib.Signal.Emit(drag, "drag-update", 3.0, 4.0);
			GLib.Signal.Emit(drag, "drag-end", 3.0, 4.0);
			Assert.Equal(1, started);
			Assert.Equal(1, dragged);
			Assert.Equal(1, ended);
			// The widget is not allocated here, so the end point is outside; the forwarding is what matters.
			Assert.False(inside);

			GLib.Signal.Emit(motion, "enter", 1.0, 1.0);
			GLib.Signal.Emit(motion, "motion", 2.0, 2.0);
			GLib.Signal.Emit(motion, "leave");
			Assert.Equal(1, hoverStarted);
			Assert.Equal(1, hoverMoved);
			Assert.Equal(1, hoverEnded);
		});

		// ---------------------------------------------------------------- ShapeView

		[GtkFact]
		public void ShapeViewUsesTheGtkHandler() => GtkTestHost.Run(() =>
		{
			var handler = GtkTestApp.CreateHandler<GtkShapeViewHandler>(new Ellipse());
			Assert.IsType<MauiGtkGraphicsView>(handler.PlatformView);
		});

		[GtkFact]
		public void ShapeViewMapsShapeFillAndAspect() => GtkTestHost.Run(() =>
		{
			var ellipse = new Ellipse { Fill = new SolidColorBrush(Colors.Red), WidthRequest = 60, HeightRequest = 60, HorizontalOptions = LayoutOptions.Start };
			using var host = new WindowHost(ellipse, 200, 200);
			host.Pump();
			var widget = (MauiGtkGraphicsView)host.Widget(ellipse);

			Assert.True(widget.DrawCount > 0);
			var center = Pixel(widget, 30, 30);
			Assert.True(center.R > 200 && center.G < 60 && center.B < 60, $"center is {center}");
			var corner = Pixel(widget, 1, 1);
			Assert.False(corner.R > 200 && corner.G < 60 && corner.B < 60, $"corner is {corner}");

			int before = widget.DrawCount;
			ellipse.Fill = new SolidColorBrush(Colors.Blue);
			Assert.True(GtkTestHost.PumpUntil(() => widget.DrawCount > before, host.PlatformWindow), "changing Fill did not repaint");
			var blue = Pixel(widget, 30, 30);
			Assert.True(blue.B > 200 && blue.R < 60, $"center is {blue}");

			before = widget.DrawCount;
			ellipse.Aspect = Stretch.None;
			Assert.True(GtkTestHost.PumpUntil(() => widget.DrawCount > before, host.PlatformWindow), "changing Aspect did not repaint");
		});

		[GtkFact]
		public void ShapeViewMapsStrokeProperties() => GtkTestHost.Run(() =>
		{
			var ellipse = new Ellipse { Stroke = new SolidColorBrush(Colors.Green), StrokeThickness = 6, WidthRequest = 60, HeightRequest = 60, HorizontalOptions = LayoutOptions.Start };
			using var host = new WindowHost(ellipse, 200, 200);
			host.Pump();
			var widget = (MauiGtkGraphicsView)host.Widget(ellipse);

			// Each stroke property repaints the shape.
			int count = widget.DrawCount;
			Action[] changes =
			{
				() => ellipse.Stroke = new SolidColorBrush(Colors.Blue),
				() => ellipse.StrokeThickness = 3,
				() => ellipse.StrokeDashArray = new DoubleCollection { 2, 2 },
				() => ellipse.StrokeDashOffset = 1,
				() => ellipse.StrokeLineCap = PenLineCap.Round,
				() => ellipse.StrokeLineJoin = PenLineJoin.Round,
				() => ellipse.StrokeMiterLimit = 4,
			};

			int index = 0;
			foreach (var change in changes)
			{
				index++;
				change();
				int expected = count;
				Assert.True(GtkTestHost.PumpUntil(() => widget.DrawCount > expected, host.PlatformWindow), $"stroke change #{index} did not repaint");
				count = widget.DrawCount;
			}
		});

		[GtkFact]
		public void ShapeViewStrokeIsDrawn() => GtkTestHost.Run(() =>
		{
			var ellipse = new Ellipse { Stroke = new SolidColorBrush(Colors.Green), StrokeThickness = 8, WidthRequest = 60, HeightRequest = 60, HorizontalOptions = LayoutOptions.Start };
			using var host = new WindowHost(ellipse, 200, 200);
			host.Pump();
			var widget = (MauiGtkGraphicsView)host.Widget(ellipse);

			// The left-most point of the circle is on the stroke.
			var edge = Pixel(widget, 3, 30);
			Assert.True(edge.G > 80 && edge.R < 60, $"edge is {edge}");
		});

		[GtkFact]
		public void ShapeViewPaintsTheBackgroundAsTheFillWhenThereIsNone() => GtkTestHost.Run(() =>
		{
			var ellipse = new Ellipse { Background = new SolidColorBrush(Colors.Yellow), WidthRequest = 60, HeightRequest = 60, HorizontalOptions = LayoutOptions.Start };
			using var host = new WindowHost(ellipse, 200, 200);
			host.Pump();
			var widget = (MauiGtkGraphicsView)host.Widget(ellipse);

			// The shape paints its background itself (clipped to the shape), so no CSS background fills the whole rectangle.
			Assert.DoesNotContain("background", Css(widget));
			var center = Pixel(widget, 30, 30);
			Assert.True(center.R > 200 && center.G > 200 && center.B < 60, $"center is {center}");
			var corner = Pixel(widget, 1, 1);
			Assert.False(corner.R > 200 && corner.G > 200 && corner.B < 60, $"corner is {corner}");
		});

		// ------------------------------------------------------------ IndicatorView

		[GtkFact]
		public void IndicatorViewMapsCountPositionAndSize() => GtkTestHost.Run(() =>
		{
			var indicator = new IndicatorView { Count = 4, Position = 2, IndicatorSize = 10 };
			var handler = GtkTestApp.CreateHandler<GtkIndicatorViewHandler>(indicator);
			var widget = (MauiGtkGraphicsView)handler.PlatformView!;

			Assert.Equal(4, handler.Drawable.Count);
			Assert.Equal(2, handler.Drawable.Position);
			Assert.Equal(10, handler.Drawable.IndicatorSize);
			// 4 dots of 10 with a gap of 10: 4 * 10 + 3 * 10.
			Assert.Equal(70, widget.ContentWidth);
			Assert.Equal(10, widget.ContentHeight);

			indicator.Count = 2;
			Assert.Equal(30, widget.ContentWidth);
		});

		[GtkFact]
		public void IndicatorViewMapsHideSingle() => GtkTestHost.Run(() =>
		{
			var indicator = new IndicatorView { Count = 1, HideSingle = true };
			var handler = GtkTestApp.CreateHandler<GtkIndicatorViewHandler>(indicator);
			var widget = (MauiGtkGraphicsView)handler.PlatformView!;
			Assert.Equal(0, widget.ContentWidth);

			indicator.HideSingle = false;
			Assert.True(widget.ContentWidth > 0);
		});

		[GtkFact]
		public void IndicatorViewMapsMaximumVisible() => GtkTestHost.Run(() =>
		{
			var indicator = new IndicatorView { Count = 10, MaximumVisible = 3, IndicatorSize = 4 };
			var handler = GtkTestApp.CreateHandler<GtkIndicatorViewHandler>(indicator);
			var widget = (MauiGtkGraphicsView)handler.PlatformView!;

			Assert.Equal(3, handler.Drawable.VisibleCount);
			Assert.Equal(3 * 4 + 2 * 4, widget.ContentWidth);
		});

		[GtkFact]
		public void IndicatorViewMapsColorsAndShape() => GtkTestHost.Run(() =>
		{
			var indicator = new IndicatorView
			{
				Count = 3,
				Position = 0,
				IndicatorSize = 16,
				IndicatorColor = Colors.Blue,
				SelectedIndicatorColor = Colors.Red,
				IndicatorsShape = IndicatorShape.Square,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};

			using var host = new WindowHost(indicator, 200, 100);
			host.Pump();
			var widget = (MauiGtkGraphicsView)host.Widget(indicator);
			var handler = (GtkIndicatorViewHandler)indicator.Handler!;

			Assert.Equal(Colors.Blue, handler.Drawable.IndicatorColor);
			Assert.Equal(Colors.Red, handler.Drawable.SelectedIndicatorColor);
			Assert.NotNull(handler.Drawable.Shape);

			// Three 16px squares with 16px gaps: the first (selected) is red, the second blue; the gap between them is empty.
			var selected = Pixel(widget, 8, 8);
			Assert.True(selected.R > 200 && selected.B < 60, $"selected is {selected}; position {handler.Drawable.Position} count {handler.Drawable.Count} size {handler.Drawable.IndicatorSize} widget {widget.Width}x{widget.Height}");
			var second = Pixel(widget, 40, 8);
			Assert.True(second.B > 200 && second.R < 60, $"second is {second}");
			var gap = Pixel(widget, 24, 8);
			Assert.False(gap.B > 200 || gap.R > 200, $"gap is {gap}");
		});

		[GtkFact]
		public void GraphicsControlsLayOutWithoutComplaints() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var stack = new VerticalStackLayout
			{
				Children =
				{
					new GraphicsView { Drawable = new RedSquare(), HeightRequest = 40 },
					new Ellipse { Fill = new SolidColorBrush(Colors.Red), HeightRequest = 30, WidthRequest = 30 },
					new IndicatorView { Count = 3 },
				},
			};

			using var host = new WindowHost(stack, 300, 300);
			host.Pump();
			GtkTestHost.AssertNoComplaints();
		});
	}
}
