using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>The Controls shapes (Line, Path, Polygon, Polyline, Rectangle, RoundRectangle, BoxView) and Frame on real widgets.</summary>
	public class ShapeTests
	{
		static Brush Red => new SolidColorBrush(Colors.Red);

		static (byte R, byte G, byte B) Pixel(global::Gtk.Widget widget, int x, int y) => GtkTestHost.PixelAt(widget, x, y);

		static bool IsRed((byte R, byte G, byte B) p) => p.R > 200 && p.G < 60 && p.B < 60;

		// The graphics widget of a view hosted in a page, once painted.
		static MauiGtkGraphicsView Render(ControlsWindowHost host, View view)
		{
			host.Pump();
			return Assert.IsType<MauiGtkGraphicsView>(host.Widget(view));
		}

		static void AssertRepaints(ControlsWindowHost host, MauiGtkGraphicsView widget, Action change, string what)
		{
			int before = widget.DrawCount;
			change();
			Assert.True(GtkTestHost.PumpUntil(() => widget.DrawCount > before, host.PlatformWindow), what + " did not repaint");
		}

		static ControlsWindowHost Host(View view) => new(new ContentPage { Content = view }, 300, 300);

		static T Fix<T>(T view) where T : View
		{
			view.WidthRequest = 80;
			view.HeightRequest = 80;
			view.HorizontalOptions = LayoutOptions.Start;
			view.VerticalOptions = LayoutOptions.Start;
			return view;
		}

		[GtkFact]
		public void RectangleRendersAndRepaintsOnRadius() => GtkTestHost.Run(() =>
		{
			var rectangle = Fix(new Rectangle { Fill = Red });
			using var host = Host(rectangle);
			Assert.IsType<GtkRectangleHandler>(rectangle.Handler);
			var widget = Render(host, rectangle);
			Assert.True(widget.DrawCount > 0);
			Assert.True(IsRed(Pixel(widget, 40, 40)));
			AssertRepaints(host, widget, () => rectangle.RadiusX = 20, "RadiusX");
			AssertRepaints(host, widget, () => rectangle.RadiusY = 20, "RadiusY");
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void RoundRectangleRendersAndRepaintsOnCornerRadius() => GtkTestHost.Run(() =>
		{
			var shape = Fix(new RoundRectangle { Fill = Red });
			using var host = Host(shape);
			Assert.IsType<GtkRoundRectangleHandler>(shape.Handler);
			var widget = Render(host, shape);
			Assert.True(IsRed(Pixel(widget, 40, 40)));
			AssertRepaints(host, widget, () => shape.CornerRadius = new CornerRadius(30), "CornerRadius");
			// A large radius leaves the corner unpainted.
			Assert.False(IsRed(Pixel(widget, 1, 1)));
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void BoxViewRendersItsColor() => GtkTestHost.Run(() =>
		{
			var box = Fix(new BoxView { Color = Colors.Red });
			using var host = Host(box);
			Assert.IsType<GtkBoxViewHandler>(box.Handler);
			var widget = Render(host, box);
			Assert.True(IsRed(Pixel(widget, 40, 40)));
			AssertRepaints(host, widget, () => box.Color = Colors.Blue, "Color");
			var blue = Pixel(widget, 40, 40);
			Assert.True(blue.B > 200 && blue.R < 60, $"center is {blue}");
			AssertRepaints(host, widget, () => box.CornerRadius = new CornerRadius(40), "CornerRadius");
			Assert.False(Pixel(widget, 1, 1).B > 200);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void LineRendersAndRepaintsOnCoordinates() => GtkTestHost.Run(() =>
		{
			var line = Fix(new Line { X1 = 0, Y1 = 0, X2 = 80, Y2 = 0, Stroke = Red, StrokeThickness = 6 });
			using var host = Host(line);
			Assert.IsType<GtkLineHandler>(line.Handler);
			var widget = Render(host, line);
			Assert.True(widget.DrawCount > 0);
			AssertRepaints(host, widget, () => line.Y2 = 80, "Y2");
			AssertRepaints(host, widget, () => line.X1 = 10, "X1");
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void PathRendersAndRepaintsOnData() => GtkTestHost.Run(() =>
		{
			var path = Fix(new Path { Fill = Red, Data = new RectangleGeometry(new Rect(0, 0, 80, 80)) });
			using var host = Host(path);
			Assert.IsType<GtkPathHandler>(path.Handler);
			var widget = Render(host, path);
			Assert.True(widget.DrawCount > 0);
			Assert.True(IsRed(Pixel(widget, 40, 40)));
			AssertRepaints(host, widget, () => path.Data = new EllipseGeometry(new Point(40, 40), 20, 20), "Data");
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void PolygonRendersAndRepaintsOnPoints() => GtkTestHost.Run(() =>
		{
			var polygon = Fix(new Polygon { Fill = Red, Points = new PointCollection { new Point(0, 0), new Point(80, 0), new Point(80, 80), new Point(0, 80) } });
			using var host = Host(polygon);
			Assert.IsType<GtkPolygonHandler>(polygon.Handler);
			var widget = Render(host, polygon);
			Assert.True(IsRed(Pixel(widget, 40, 40)));
			AssertRepaints(host, widget, () => polygon.Points = new PointCollection { new Point(0, 0), new Point(80, 0), new Point(40, 80) }, "Points");
			AssertRepaints(host, widget, () => polygon.FillRule = FillRule.Nonzero, "FillRule");
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void PolylineRendersAndRepaintsOnPoints() => GtkTestHost.Run(() =>
		{
			var polyline = Fix(new Polyline { Stroke = Red, StrokeThickness = 4, Points = new PointCollection { new Point(0, 0), new Point(80, 40), new Point(0, 80) } });
			using var host = Host(polyline);
			Assert.IsType<GtkPolylineHandler>(polyline.Handler);
			var widget = Render(host, polyline);
			Assert.True(widget.DrawCount > 0);
			AssertRepaints(host, widget, () => polyline.Points = new PointCollection { new Point(0, 0), new Point(80, 80) }, "Points");
			AssertRepaints(host, widget, () => polyline.FillRule = FillRule.Nonzero, "FillRule");
			ControlsWindowHost.AssertNoComplaints();
		});

		// ------------------------------------------------------------------ Frame

#pragma warning disable CS0618 // Frame is obsolete but still supported.
		[GtkFact]
		public void FrameUsesTheBorderWidgetWithItsCornerRadiusAndBorder() => GtkTestHost.Run(() =>
		{
			var frame = new Frame { BorderColor = Colors.Red, CornerRadius = 12, BackgroundColor = Colors.White, Content = new Label { Text = "x" }, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
			using var host = Host(frame);
			host.Pump();

			Assert.IsType<GtkFrameHandler>(frame.Handler);
			var border = Assert.IsType<MauiGtkBorder>(host.Widget(frame));
			Assert.Equal(1, border.StrokeThickness);
			Assert.NotNull(border.Stroke);
			Assert.NotNull(border.Fill);
			var shape = Assert.IsType<RoundRectangle>(border.Shape);
			Assert.Equal(12, shape.CornerRadius.TopLeft);

			frame.CornerRadius = 4;
			Assert.Equal(4, ((RoundRectangle)border.Shape!).CornerRadius.TopLeft);
			frame.BorderColor = null!;
			Assert.Equal(0, border.StrokeThickness);
			Assert.Null(border.Stroke);

			frame.CornerRadius = -1;
			Assert.Equal(5, ((RoundRectangle)border.Shape!).CornerRadius.TopLeft);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void FrameShadowIsACssBoxShadowFollowingHasShadow() => GtkTestHost.Run(() =>
		{
			var frame = new Frame { CornerRadius = 8, Content = new Label { Text = "x" }, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
			using var host = Host(frame);
			host.Pump();
			var border = (MauiGtkBorder)host.Widget(frame);

			string css = GtkStyle.For(border).ToCss();
			Assert.Contains("box-shadow", css);
			Assert.Contains("border-radius: 8px", css);

			frame.HasShadow = false;
			css = GtkStyle.For(border).ToCss();
			Assert.DoesNotContain("box-shadow", css);
			Assert.DoesNotContain("border-radius", css);

			frame.HasShadow = true;
			Assert.Contains("box-shadow", GtkStyle.For(border).ToCss());
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void FramePaddingAndBorderInsetTheContent() => GtkTestHost.Run(() =>
		{
			var content = new BoxView { Color = Colors.Red, WidthRequest = 40, HeightRequest = 40 };
			var frame = new Frame { Padding = new Thickness(10), BorderColor = Colors.Black, Content = content, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
			using var host = Host(frame);
			host.Pump();

			var outer = host.Bounds(frame);
			var inner = host.Bounds(content);
			// Padding 10 plus the 1 px border.
			Assert.Equal(11, inner.X - outer.X);
			Assert.Equal(11, inner.Y - outer.Y);
			Assert.Equal(40 + 22, outer.Width);

			frame.Padding = new Thickness(20);
			Assert.True(GtkTestHost.PumpUntil(() => host.Bounds(content).X - host.Bounds(frame).X == 21, host.PlatformWindow), "padding did not move the content");
			ControlsWindowHost.AssertNoComplaints();
		});
#pragma warning restore CS0618
	}
}
