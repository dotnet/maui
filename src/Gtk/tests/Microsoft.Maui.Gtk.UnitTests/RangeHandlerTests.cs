using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;
using GtkProgressBar = global::Gtk.ProgressBar;
using GtkScale = global::Gtk.Scale;
using GtkSpinner = global::Gtk.Spinner;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>ActivityIndicator, ProgressBar, Slider and Stepper: one test per mapped property, on real widgets.</summary>
	public class RangeHandlerTests
	{
		static string Css(global::Gtk.Widget widget) => GtkStyle.For(widget).ToCss();

		// ------------------------------------------------------- ActivityIndicator

		[GtkFact]
		public void ActivityIndicatorUsesTheGtkHandler() => GtkTestHost.Run(() =>
		{
			var handler = GtkTestApp.CreateHandler<GtkActivityIndicatorHandler>(new ActivityIndicator());
			Assert.IsType<GtkSpinner>(handler.PlatformView);
		});

		[GtkFact]
		public void ActivityIndicatorMapsIsRunning() => GtkTestHost.Run(() =>
		{
			var indicator = new ActivityIndicator { IsRunning = true };
			var widget = GtkTestApp.CreateWidget<GtkSpinner>(indicator);
			Assert.True(widget.Spinning);

			indicator.IsRunning = false;
			Assert.False(widget.Spinning);
		});

		[GtkFact]
		public void ActivityIndicatorMapsColor() => GtkTestHost.Run(() =>
		{
			var indicator = new ActivityIndicator { Color = Colors.Red };
			var widget = GtkTestApp.CreateWidget<GtkSpinner>(indicator);
			Assert.Contains("color: rgba(255,0,0,1)", Css(widget));

			indicator.Color = null;
			Assert.DoesNotContain("color:", Css(widget));
		});

		// ------------------------------------------------------------- ProgressBar

		[GtkFact]
		public void ProgressBarMapsProgress() => GtkTestHost.Run(() =>
		{
			var progress = new ProgressBar { Progress = 0.25 };
			var widget = GtkTestApp.CreateWidget<GtkProgressBar>(progress);
			Assert.Equal(0.25, widget.Fraction, 3);

			progress.Progress = 0.9;
			Assert.Equal(0.9, widget.Fraction, 3);
		});

		[GtkFact]
		public void ProgressBarMapsProgressColor() => GtkTestHost.Run(() =>
		{
			var progress = new ProgressBar { ProgressColor = Colors.Green };
			var widget = GtkTestApp.CreateWidget<GtkProgressBar>(progress);
			Assert.Contains(" progress {", Css(widget));
			Assert.Contains("background-color: rgba(0,128,0,1)", Css(widget));
		});

		[GtkFact]
		public void ProgressBarMapsFlowDirection() => GtkTestHost.Run(() =>
		{
			var progress = new ProgressBar { Progress = 0.5, FlowDirection = FlowDirection.RightToLeft };
			var widget = GtkTestApp.CreateWidget<GtkProgressBar>(progress);
			Assert.True(widget.Inverted);
			Assert.Equal(global::Gtk.TextDirection.Rtl, widget.Direction);

			progress.FlowDirection = FlowDirection.LeftToRight;
			Assert.False(widget.Inverted);
		});

		// ------------------------------------------------------------------ Slider

		[GtkFact]
		public void SliderMapsMinimumMaximumAndValue() => GtkTestHost.Run(() =>
		{
			var slider = new Slider(10, 50, 20);
			var widget = GtkTestApp.CreateWidget<GtkScale>(slider);

			Assert.Equal(10, widget.Adjustment.Lower);
			Assert.Equal(50, widget.Adjustment.Upper);
			Assert.Equal(20, widget.Value);

			slider.Maximum = 100;
			Assert.Equal(100, widget.Adjustment.Upper);

			slider.Value = 75;
			Assert.Equal(75, widget.Value);
		});

		[GtkFact]
		public void SliderWritesTheUserValueBack() => GtkTestHost.Run(() =>
		{
			var slider = new Slider(0, 10, 2);
			var widget = GtkTestApp.CreateWidget<GtkScale>(slider);

			widget.Value = 7;
			Assert.Equal(7, slider.Value);
		});

		[GtkFact]
		public void SliderMapsTrackAndThumbColors() => GtkTestHost.Run(() =>
		{
			var slider = new Slider
			{
				MinimumTrackColor = Colors.Red,
				MaximumTrackColor = Colors.Green,
				ThumbColor = Colors.Blue,
			};
			var widget = GtkTestApp.CreateWidget<GtkScale>(slider);
			var css = Css(widget);

			Assert.Contains(" trough highlight {", css);
			Assert.Contains("background-color: rgba(255,0,0,1)", css);
			Assert.Contains(" trough {", css);
			Assert.Contains("background-color: rgba(0,128,0,1)", css);
			Assert.Contains(" slider {", css);
			Assert.Contains("background-color: rgba(0,0,255,1)", css);
		});

		[GtkFact]
		public void SliderTrackAndThumbColorsAreActuallyPainted() => GtkTestHost.Run(() =>
		{
			// Inner-node rules (trough, trough highlight, slider) only reach GTK through the display provider of GtkStyle.
			var slider = new Slider { Minimum = 0, Maximum = 1, Value = 0.5, ThumbColor = Colors.Red, MinimumTrackColor = Colors.Lime, MaximumTrackColor = Colors.Purple };
			using var host = new WindowHost(new Grid { Children = { slider } }, 200, 60);
			host.Pump();
			GtkTestHost.PumpUntil(() => CountColors(host.PlatformWindow.Child!).Red > 20, host.PlatformWindow);
			var (red, lime, purple) = CountColors(host.PlatformWindow.Child!);
			Assert.True(red > 20, $"thumb not red ({red})");
			Assert.True(lime > 20, $"minimum track not lime ({lime})");
			Assert.True(purple > 20, $"maximum track not purple ({purple})");

			slider.ThumbColor = Colors.Blue;
			Assert.True(GtkTestHost.PumpUntil(() => CountColors(host.PlatformWindow.Child!).Red == 0, host.PlatformWindow), "the old thumb colour is still painted");
		});

		static (int Red, int Lime, int Purple) CountColors(global::Gtk.Widget widget)
		{
			var bytes = GtkTestHost.RenderToBytes(widget, out int w, out int h);
			int red = 0, lime = 0, purple = 0;
			for (int i = 0; i < w * h; i++)
			{
				byte r = bytes[i * 4 + 2], g = bytes[i * 4 + 1], b = bytes[i * 4];
				if (r > 200 && g < 60 && b < 60) red++;
				else if (g > 200 && r < 60 && b < 60) lime++;
				else if (r > 100 && r < 160 && g < 60 && b > 100 && b < 160) purple++;
			}

			return (red, lime, purple);
		}

		[GtkFact]
		public void SliderThumbImageSourceReplacesTheThumbLook() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var data = TestImages.CreatePng(24, 24, 255, 0, 0);
			var slider = new Slider { Minimum = 0, Maximum = 1, Value = 0, ThumbColor = Colors.Blue, ThumbImageSource = ImageSource.FromStream(() => new System.IO.MemoryStream(data)) };
			using var host = new WindowHost(new Grid { Children = { slider } }, 200, 60);
			var widget = (GtkScale)host.Widget(slider);

			Assert.True(GtkTestHost.PumpUntil(() => Css(widget).Contains("data:image/png;base64,"), host.PlatformWindow), "the thumb image was never applied");
			host.Pump();
			var css = Css(widget);
			Assert.Contains("min-width: 24px", css);
			Assert.Contains("min-height: 24px", css);
			Assert.DoesNotContain("rgba(0,0,255,1)", css);

			var bounds = GtkTestHost.BoundsIn(widget, host.PlatformWindow.Child);
			Assert.True(GtkTestHost.PumpUntil(() =>
			{
				var pixel = GtkTestHost.PixelAt(host.PlatformWindow.Child!, bounds.X + 12, bounds.Y + bounds.Height / 2);
				return pixel.R > 200 && pixel.G < 60 && pixel.B < 60;
			}, host.PlatformWindow), "the thumb image was never painted");

			slider.ThumbImageSource = null!;
			Assert.True(GtkTestHost.PumpUntil(() => !Css(widget).Contains("data:image"), host.PlatformWindow));
			Assert.Contains("background-color: rgba(0,0,255,1)", Css(widget));
			Assert.DoesNotContain("min-width", Css(widget));
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void SliderRaisesDragStartedAndCompleted() => GtkTestHost.Run(() =>
		{
			var slider = new Slider();
			int started = 0, completed = 0;
			slider.DragStarted += (s, e) => started++;
			slider.DragCompleted += (s, e) => completed++;
			var widget = GtkTestApp.CreateWidget<GtkScale>(slider);

			var gesture = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!;
			GLib.Signal.Emit(gesture, "pressed", 1, 1.0, 1.0);
			GLib.Signal.Emit(gesture, "released", 1, 1.0, 1.0);

			Assert.Equal(1, started);
			Assert.Equal(1, completed);
		});

		[GtkFact]
		public void SliderWithAnEmptyRangeStaysValid() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var slider = new Slider(5, 6, 5);
			var widget = GtkTestApp.CreateWidget<GtkScale>(slider);
			Assert.Equal(5, widget.Adjustment.Lower);
			GtkTestHost.AssertNoComplaints();
		});

		// ----------------------------------------------------------------- Stepper

		[GtkFact]
		public void StepperMapsMinimumMaximumIntervalAndValue() => GtkTestHost.Run(() =>
		{
			var stepper = new Stepper(0, 10, 4, 2);
			var widget = GtkTestApp.CreateWidget<MauiGtkStepper>(stepper);

			Assert.Equal(0, widget.Minimum);
			Assert.Equal(10, widget.Maximum);
			Assert.Equal(2, widget.Interval);
			Assert.Equal(4, widget.Value);

			stepper.Value = 8;
			Assert.Equal(8, widget.Value);
			stepper.Maximum = 6;
			Assert.Equal(6, widget.Maximum);
			Assert.Equal(6, widget.Value);
		});

		[GtkFact]
		public void StepperButtonsStepTheVirtualViewAndStopAtTheBounds() => GtkTestHost.Run(() =>
		{
			var stepper = new Stepper(0, 4, 2, 1);
			var widget = GtkTestApp.CreateWidget<MauiGtkStepper>(stepper);

			GLib.Signal.Emit(widget.PlusButton, "clicked");
			Assert.Equal(3, stepper.Value);
			GLib.Signal.Emit(widget.PlusButton, "clicked");
			Assert.Equal(4, stepper.Value);
			Assert.False(widget.PlusButton.Sensitive);
			Assert.True(widget.MinusButton.Sensitive);

			stepper.Value = 0;
			Assert.False(widget.MinusButton.Sensitive);
			Assert.True(widget.PlusButton.Sensitive);
		});

		[GtkFact]
		public void StepperDisabledMakesTheWidgetInsensitive() => GtkTestHost.Run(() =>
		{
			var stepper = new Stepper { IsEnabled = false };
			var widget = GtkTestApp.CreateWidget<MauiGtkStepper>(stepper);
			Assert.False(widget.Sensitive);
		});

		[GtkFact]
		public void StepperMapsBackground() => GtkTestHost.Run(() =>
		{
			var stepper = new Stepper { Background = new SolidColorBrush(Colors.Yellow) };
			var widget = GtkTestApp.CreateWidget<MauiGtkStepper>(stepper);
			Assert.Contains("background-color: rgba(255,255,0,1)", Css(widget));
		});

		[GtkFact]
		public void RangeControlsLayOutWithoutComplaints() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var stack = new VerticalStackLayout
			{
				Spacing = 6,
				Children =
				{
					new ActivityIndicator { IsRunning = true, Color = Colors.Red },
					new ProgressBar { Progress = 0.4, ProgressColor = Colors.Green },
					new Slider(0, 10, 3) { MinimumTrackColor = Colors.Red, ThumbColor = Colors.Blue },
					new Stepper(0, 10, 3, 1),
				},
			};

			using var host = new WindowHost(stack, 400, 300);
			host.Pump();

			foreach (var child in stack.Children)
				Assert.True(host.Bounds((IView)child).Height > 0, child.GetType().Name + " has no height");

			GtkTestHost.AssertNoComplaints();
		});
	}
}
