using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>
	/// Measure/arrange through the real chain (window handler, root panel, page, layouts, handlers) on real widgets.
	/// </summary>
	public class LayoutTests
	{
		/// <summary>A plain coloured view (BoxView needs the shape handlers, which are not part of the core backend).</summary>
		sealed class Box : ContentView
		{
			public Color Color
			{
				set => BackgroundColor = value;
			}
		}

		static void AssertNoLayoutComplaints() => GtkTestHost.AssertNoComplaints();

		[GtkFact]
		public void VerticalStackStacksChildrenAndNeverAllocatesWithoutMeasuring() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var label = new Label { Text = "Hello MAUI" };
			var button = new Microsoft.Maui.Controls.Button { Text = "Click me" };
			var entry = new Entry { Text = "entry" };
			var stack = new VerticalStackLayout { Spacing = 10, Children = { label, button, entry } };

			using var host = new WindowHost(stack, 600, 400);

			var labelBounds = host.Bounds((label));
			var buttonBounds = host.Bounds((button));
			var entryBounds = host.Bounds((entry));

			Assert.True(labelBounds.Width > 0 && labelBounds.Height > 0, "label has a size");
			Assert.Equal(0, labelBounds.Y);

			// Each child starts after the previous one plus the 10px spacing.
			Assert.Equal(labelBounds.Y + labelBounds.Height + 10, buttonBounds.Y);
			Assert.Equal(buttonBounds.Y + buttonBounds.Height + 10, entryBounds.Y);

			// MAUI's measured frame is what was allocated.
			Assert.Equal(Math.Ceiling(button.Frame.Y), buttonBounds.Y);

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void RootPanelDecidesTheWindowSizeNotTheContent() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var huge = new Box { WidthRequest = 5000, HeightRequest = 5000 };
			using var host = new WindowHost(new Grid { Children = { huge } }, 500, 300);

			var root = host.PlatformWindow.Child;
			Assert.IsType<MauiGtkRootPanel>(root);
			Assert.Equal(500, root!.Width);
			Assert.Equal(300, root.Height);

			root.Measure(global::Gtk.Orientation.Horizontal, -1, out int min, out int natural, out _, out _);
			Assert.Equal(0, min);
			Assert.Equal(0, natural);

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void ExplicitSizesAreHonoured() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var button = new Microsoft.Maui.Controls.Button
			{
				Text = "x",
				WidthRequest = 220,
				HeightRequest = 70,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};

			using var host = new WindowHost(new VerticalStackLayout { Children = { button } }, 600, 400);

			var bounds = host.Bounds((button));
			Assert.Equal(220, bounds.Width);
			Assert.Equal(70, bounds.Height);

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void MinimumAndMaximumSizesClampTheDesiredSize() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var label = new Label { Text = "a", MinimumWidthRequest = 150, MaximumWidthRequest = 400, HorizontalOptions = LayoutOptions.Start };
			var wide = new Label
			{
				Text = new string('w', 200),
				MaximumWidthRequest = 120,
				LineBreakMode = LineBreakMode.NoWrap,
				HorizontalOptions = LayoutOptions.Start,
			};

			using var host = new WindowHost(new VerticalStackLayout { Children = { label, wide } }, 600, 400);

			Assert.Equal(150, host.Bounds((label)).Width);
			Assert.True(host.Bounds((wide)).Width <= 120);

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void LabelWrapsToTheConstraintAndGrowsTaller() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var text = string.Join(" ", Enumerable.Repeat("wrapping text", 12));
			var label = new Label { Text = text, VerticalOptions = LayoutOptions.Start };

			using var host = new WindowHost(new VerticalStackLayout { Children = { label } }, 200, 400);
			var narrow = host.Bounds((label));

			GtkTestHost.Resize(host.PlatformWindow, 700, 400);
			host.Page.Content.InvalidateMeasure();
			host.Pump();
			var wide = host.Bounds((label));

			Assert.True(narrow.Width <= 200, $"narrow label is {narrow.Width} wide");
			Assert.True(narrow.Height > wide.Height, $"wrapped label ({narrow.Height}) should be taller than the wide one ({wide.Height})");

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void GridPlacesChildrenInCells() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var a = new Box { Color = Colors.Red };
			var b = new Box { Color = Colors.Blue };
			var grid = new Grid
			{
				ColumnDefinitions = { new ColumnDefinition(100), new ColumnDefinition(GridLength.Star) },
				RowDefinitions = { new RowDefinition(60), new RowDefinition(GridLength.Star) },
			};
			grid.Add(a, 0, 0);
			grid.Add(b, 1, 1);

			using var host = new WindowHost(grid, 500, 300);

			var boundsA = host.Bounds((a));
			var boundsB = host.Bounds((b));

			Assert.Equal(new Gdk.Rectangle(0, 0, 100, 60), boundsA);
			Assert.Equal(100, boundsB.X);
			Assert.Equal(60, boundsB.Y);
			Assert.Equal(400, boundsB.Width);
			Assert.Equal(240, boundsB.Height);

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void InvalidateMeasureRelaysOutTheParent() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var first = new Label { Text = "one", VerticalOptions = LayoutOptions.Start };
			var second = new Label { Text = "two", VerticalOptions = LayoutOptions.Start };
			var stack = new VerticalStackLayout { Children = { first, second } };

			using var host = new WindowHost(stack, 400, 300);
			int before = host.Bounds((second)).Y;

			first.FontSize = 40;

			// The CSS change is validated by the frame clock, so wait for the layout to follow.
			GtkTestHost.PumpUntil(() => host.Bounds(second).Y > before, host.PlatformWindow, 3000);

			int after = host.Bounds((second)).Y;
			Assert.True(after > before, $"second label should move down when the first grows ({before} -> {after})");

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void TranslationScaleAndRotationAreAppliedAsTransforms() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var box = new Box
			{
				Color = Colors.Green,
				WidthRequest = 100,
				HeightRequest = 50,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};

			using var host = new WindowHost(new Grid { Children = { box } }, 400, 300);
			var widget = host.Widget(box);
			Assert.Equal(new Gdk.Rectangle(0, 0, 100, 50), GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child));

			box.TranslationX = 30;
			box.TranslationY = 20;
			host.Pump();
			Assert.Equal(new Gdk.Rectangle(30, 20, 100, 50), GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child));

			box.TranslationX = 0;
			box.TranslationY = 0;
			box.Scale = 2;
			host.Pump();

			// Scaled about the centre (anchor 0.5, 0.5): 200x100 centred on (50, 25).
			var scaled = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);
			Assert.Equal(-50, scaled.X);
			Assert.Equal(-25, scaled.Y);
			Assert.Equal(200, scaled.Width);
			Assert.Equal(100, scaled.Height);

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void RotationXAndYTiltTheViewInPerspective() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var box = new Box
			{
				Color = Colors.Green,
				WidthRequest = 100,
				HeightRequest = 50,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};

			using var host = new WindowHost(new Grid { Children = { box } }, 400, 300);
			var widget = host.Widget(box);
			var flat = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);
			Assert.Equal(new Gdk.Rectangle(0, 0, 100, 50), flat);

			// About the vertical axis through the anchor: narrower, still centred on the anchor.
			foreach (double angle in new[] { 45.0, 70.0, -60.0 })
			{
				box.RotationY = angle;
				host.Pump();
				var tilted = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);
				Assert.True(tilted.Width < 100 && tilted.Width > 0, $"RotationY {angle}: width {tilted.Width}");
				Assert.InRange(tilted.X + tilted.Width / 2, 45, 55);
			}

			// About the horizontal axis: shorter, still centred.
			box.RotationY = 0;
			foreach (double angle in new[] { 45.0, 70.0, -60.0 })
			{
				box.RotationX = angle;
				host.Pump();
				var tilted = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);
				Assert.True(tilted.Height < 50 && tilted.Height > 0, $"RotationX {angle}: height {tilted.Height}");
				Assert.InRange(tilted.Width, 88, 112);
				Assert.InRange(tilted.Y + tilted.Height / 2, 21, 29);
			}

			// Resetting restores the flat placement.
			box.RotationX = 0;
			host.Pump();
			Assert.Equal(flat, GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child));

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void RotationXAndYFollowTheAnchor() => GtkTestHost.Run(() =>
		{
			var box = new Box
			{
				WidthRequest = 100,
				HeightRequest = 50,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
				RotationY = 60,
			};

			using var host = new WindowHost(new Grid { Children = { box } }, 400, 300);
			var widget = host.Widget(box);
			var centred = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);

			// Hinging on the left edge keeps that edge in place, so the view swings to one side of the centred placement.
			box.AnchorX = 0;
			host.Pump();
			var hinged = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);
			Assert.True(hinged.X < centred.X + 2 && hinged.X <= 1, $"hinged {hinged}, centred {centred}");
			Assert.True(hinged.X + hinged.Width < 100);
		});

		[GtkFact]
		public void CollapsedViewsTakeNoSpaceAndHiddenViewsKeepTheirSlot() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var first = new Box { HeightRequest = 40, WidthRequest = 40, HorizontalOptions = LayoutOptions.Start };
			var middle = new Box { HeightRequest = 40, WidthRequest = 40, HorizontalOptions = LayoutOptions.Start };
			var last = new Box { HeightRequest = 40, WidthRequest = 40, HorizontalOptions = LayoutOptions.Start };
			var stack = new VerticalStackLayout { Children = { first, middle, last } };

			using var host = new WindowHost(stack, 300, 300);
			Assert.Equal(80, host.Bounds((last)).Y);

			middle.IsVisible = false;
			host.Pump();
			Assert.Equal(40, host.Bounds((last)).Y);
			Assert.False(host.Widget(middle).Visible);

			middle.IsVisible = true;
			middle.Opacity = 1;
			host.Pump();
			Assert.Equal(80, host.Bounds((last)).Y);

			AssertNoLayoutComplaints();
		});

		[GtkFact]
		public void ScrollViewScrollsContentTallerThanTheViewport() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var content = new VerticalStackLayout();
			for (int i = 0; i < 30; i++)
				content.Add(new Box { HeightRequest = 50, Color = Colors.Orange });

			var scroll = new ScrollView { Content = content };
			using var host = new WindowHost(scroll, 300, 200);

			var scroller = (global::Gtk.ScrolledWindow)host.Widget(scroll);
			Assert.Equal(200, scroller.Height);
			Assert.True(scroller.Vadjustment.Upper >= 1500, $"content height is {scroller.Vadjustment.Upper}");
			Assert.True(scroller.Vadjustment.PageSize <= 200);

			scroller.Vadjustment.Value = 100;
			host.Pump();
			Assert.Equal(100, scroll.ScrollY, 1);

			AssertNoLayoutComplaints();
		});
	}
}
