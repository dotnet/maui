using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;
using GtkLabel = global::Gtk.Label;
using GtkWidget = global::Gtk.Widget;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>The properties every view shares (the patched ViewMapper), exercised on real widgets.</summary>
	public class ViewMappingTests
	{
		static GtkLabel CreateLabel(Label label) => GtkTestApp.CreateWidget<GtkLabel>(label);

		[GtkFact]
		public void IsVisibleMapsToWidgetVisibility() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			var widget = CreateLabel(label);
			Assert.True(widget.Visible);

			label.IsVisible = false;
			Assert.False(widget.Visible);

			label.IsVisible = true;
			Assert.True(widget.Visible);
		});

		[GtkFact]
		public void OpacityEnabledAndInputTransparentMapToWidgetState() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x", Opacity = 0.25, IsEnabled = false, InputTransparent = true };
			var widget = CreateLabel(label);

			Assert.Equal(0.25, widget.Opacity, 2);
			Assert.False(widget.Sensitive);
			Assert.False(widget.CanTarget);

			label.Opacity = 1;
			label.IsEnabled = true;
			label.InputTransparent = false;

			Assert.Equal(1.0, widget.Opacity, 2);
			Assert.True(widget.Sensitive);
			Assert.True(widget.CanTarget);
		});

		[GtkFact]
		public void AutomationIdBecomesTheWidgetName() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x", AutomationId = "greeting" };
			var widget = CreateLabel(label);
			Assert.Equal("greeting", widget.Name);
		});

		[GtkFact]
		public void SemanticsMapToTheAccessibleNameAndDescription() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			SemanticProperties.SetDescription(label, "spoken name");
			SemanticProperties.SetHint(label, "spoken hint");

			var widget = CreateLabel(label);
			Assert.Equal("spoken name", widget.Accessible.Name);
			Assert.Equal("spoken hint", widget.Accessible.Description);
		});

		[GtkFact]
		public void ToolTipMapsToTooltipText() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			ToolTipProperties.SetText(label, "a tip");

			var widget = CreateLabel(label);
			Assert.Equal("a tip", widget.TooltipText);
		});

		[GtkFact]
		public void FlowDirectionMapsToTextDirection() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x", FlowDirection = FlowDirection.RightToLeft };
			var widget = CreateLabel(label);
			Assert.Equal(global::Gtk.TextDirection.Rtl, widget.Direction);

			label.FlowDirection = FlowDirection.LeftToRight;
			Assert.Equal(global::Gtk.TextDirection.Ltr, widget.Direction);
		});

		[GtkFact]
		public void BackgroundIsCssAndSwitchesBetweenPaintKinds() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x", BackgroundColor = Colors.Red };
			var widget = CreateLabel(label);
			var css = GtkStyle.For(widget).ToCss();
			Assert.Contains("background-color: rgba(255,0,0,1)", css);

			label.Background = new LinearGradientBrush(new GradientStopCollection
			{
				new GradientStop(Colors.Red, 0),
				new GradientStop(Colors.Blue, 1),
			}, new Point(0, 0), new Point(1, 0));
			css = GtkStyle.For(widget).ToCss();
			Assert.Contains("linear-gradient(90deg, rgba(255,0,0,1) 0%, rgba(0,0,255,1) 100%)", css);
			Assert.DoesNotContain("background-color", css);

			label.Background = null!;
			label.BackgroundColor = null!;
			Assert.DoesNotContain("background", GtkStyle.For(widget).ToCss());
		});

		[GtkFact]
		public void RadialGradientBackgroundIsCss() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			label.Background = new RadialGradientBrush(new GradientStopCollection
			{
				new GradientStop(Colors.White, 0),
				new GradientStop(Colors.Black, 1),
			}, new Point(0.25, 0.75), 0.5);
			var widget = CreateLabel(label);

			Assert.Contains("radial-gradient(circle farthest-side at 25% 75%", GtkStyle.For(widget).ToCss());
		});

		[GtkFact]
		public void ClipAndShadowCreateAWrapperContainerAndRemovingThemRestoresTheWidget() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			var stack = new VerticalStackLayout { Children = { label } };
			using var host = new WindowHost(stack, 300, 200);

			var widget = (GtkWidget)label.Handler!.PlatformView!;
			Assert.Null(label.Handler.ToPlatform_ContainerView());
			Assert.IsType<MauiGtkPanel>(widget.Parent);

			label.Clip = new RoundRectangleGeometry(new CornerRadius(8), new Rect(0, 0, 40, 20));
			host.Pump();

			var container = Assert.IsType<GtkWrapperView>(((Microsoft.Maui.IViewHandler)label.Handler).ContainerView);
			Assert.Same(container, widget.Parent);
			Assert.IsType<MauiGtkPanel>(container.Parent);
			Assert.NotNull(container.Clip);

			label.Shadow = new Shadow { Brush = Colors.Black, Radius = 4, Offset = new Point(2, 2), Opacity = 0.5f };
			Assert.NotNull(container.Shadow);

			label.Clip = null!;
			label.Shadow = null!;
			host.Pump();

			Assert.Null(((Microsoft.Maui.IViewHandler)label.Handler).ContainerView);
			Assert.IsType<MauiGtkPanel>(widget.Parent);
			Assert.False(container.IsRealized && container.Parent is not null);
		});

		[GtkFact]
		public void WrapperClipClipsThePaintedPixels() => GtkTestHost.Run(() =>
		{
			var box = new ContentView
			{
				BackgroundColor = Colors.Red,
				WidthRequest = 100,
				HeightRequest = 100,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
				Clip = new EllipseGeometry(new Point(50, 50), 50, 50),
			};

			using var host = new WindowHost(new Grid { Children = { box } }, 200, 200);

			var inside = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 50, 50);
			var corner = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 3, 3);

			Assert.True(inside.R > 200 && inside.G < 60 && inside.B < 60, $"centre should be red, was {inside}");
			Assert.False(corner.R > 200 && corner.G < 60 && corner.B < 60, $"corner should be clipped away, was {corner}");
		});

		[GtkFact]
		public void LayoutBackgroundPaintsThePanel() => GtkTestHost.Run(() =>
		{
			var stack = new VerticalStackLayout { BackgroundColor = Colors.Blue };
			using var host = new WindowHost(stack, 120, 80);

			var pixel = GtkTestHost.PixelAt(host.PlatformWindow.Child!, 60, 40);
			Assert.True(pixel.B > 200 && pixel.R < 60, $"panel background should be blue, was {pixel}");
		});

		[GtkFact]
		public void InvalidateMeasureCommandQueuesAResize() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			using var host = new WindowHost(new VerticalStackLayout { Children = { label } }, 200, 100);

			label.Text = "a considerably longer text than before";
			Assert.True(GtkTestHost.PumpUntil(() => host.Bounds(label).Width > 100, host.PlatformWindow, 3000));
		});
	}

	static class HandlerTestExtensions
	{
		public static object? ToPlatform_ContainerView(this IElementHandler handler) => ((Microsoft.Maui.IViewHandler)handler).ContainerView;
	}
}
