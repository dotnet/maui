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
		public void SliderAcceptsAThumbImageSource() => GtkTestHost.Run(() =>
		{
			var slider = new Slider { ThumbImageSource = ImageSource.FromFile("thumb.png") };
			var widget = GtkTestApp.CreateWidget<GtkScale>(slider);
			Assert.NotNull(widget);
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
