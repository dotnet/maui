using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Platform;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Gesture recognizers on real widgets: the GTK controllers are driven by emitting their signals.</summary>
	public class GestureTests
	{
		static (Label Label, global::Gtk.Widget Widget, ControlsWindowHost Host) Create()
		{
			var label = new Label { Text = "target" };
			var host = new ControlsWindowHost(new ContentPage { Content = label });
			return (label, host.Widget(label), host);
		}

		static void Emit(global::Gtk.EventController controller, string signal, params object[] args) =>
			GLib.Signal.Emit(controller, signal, args);

		[GtkFact]
		public void TapRecognizerFiresOnRelease() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				int tapped = 0;
				Point? position = null;
				var tap = new TapGestureRecognizer();
				tap.Tapped += (_, e) =>
				{
					tapped++;
					position = e.GetPosition(label);
				};
				label.GestureRecognizers.Add(tap);

				var click = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!;
				Assert.NotNull(click);

				Emit(click, "pressed", 1, 12.0, 7.0);
				Assert.Equal(0, tapped);
				Emit(click, "released", 1, 12.0, 7.0);
				Assert.Equal(1, tapped);
				Assert.Equal(new Point(12, 7), position);
			}
		});

		[GtkFact]
		public void DoubleTapNeedsTheSecondPressAndSingleTapFiresOnEveryClick() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				int single = 0, @double = 0;
				var one = new TapGestureRecognizer();
				one.Tapped += (_, _) => single++;
				var two = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
				two.Tapped += (_, _) => @double++;
				label.GestureRecognizers.Add(one);
				label.GestureRecognizers.Add(two);

				var click = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!;
				Emit(click, "released", 1, 1.0, 1.0);
				Assert.Equal((1, 0), (single, @double));

				Emit(click, "released", 2, 1.0, 1.0);
				Assert.Equal((2, 1), (single, @double));
			}
		});

		[GtkFact]
		public void TapOnTheSecondaryButtonOnlyIsNotTriggeredByThePrimary() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				int tapped = 0;
				var tap = new TapGestureRecognizer { Buttons = ButtonsMask.Secondary };
				tap.Tapped += (_, _) => tapped++;
				label.GestureRecognizers.Add(tap);

				// A synthetic emission carries no button, which counts as the primary one.
				Emit(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!, "released", 1, 1.0, 1.0);
				Assert.Equal(0, tapped);
			}
		});

		[GtkFact]
		public void TapCommandIsExecuted() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				object? parameter = null;
				label.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(p => parameter = p), CommandParameter = "go" });

				Emit(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!, "released", 1, 1.0, 1.0);
				Assert.Equal("go", parameter);
			}
		});

		[GtkFact]
		public void ControllersFollowTheRecognizerCollection() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget));
				Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget));

				var tap = new TapGestureRecognizer();
				var pan = new PanGestureRecognizer();
				label.GestureRecognizers.Add(tap);
				label.GestureRecognizers.Add(pan);
				Assert.NotNull(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget));
				Assert.NotNull(GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget));

				label.GestureRecognizers.Remove(tap);
				Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget));
				Assert.NotNull(GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget));

				label.GestureRecognizers.Clear();
				Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget));
			}
		});

		[GtkFact]
		public void RecognizersAddedBeforeTheHandlerExistAreConnected() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "target" };
			int tapped = 0;
			var tap = new TapGestureRecognizer();
			tap.Tapped += (_, _) => tapped++;
			label.GestureRecognizers.Add(tap);

			using var host = new ControlsWindowHost(new ContentPage { Content = label });
			Emit(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(host.Widget(label))!, "released", 1, 1.0, 1.0);
			Assert.Equal(1, tapped);
		});

		[GtkFact]
		public void PanReportsStartedRunningAndCompleted() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				var events = new List<(GestureStatus Status, double X, double Y)>();
				var pan = new PanGestureRecognizer();
				pan.PanUpdated += (_, e) => events.Add((e.StatusType, e.TotalX, e.TotalY));
				label.GestureRecognizers.Add(pan);

				var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget)!;
				Emit(drag, "drag-begin", 5.0, 5.0);
				// Below the threshold it is a click, not a pan.
				Emit(drag, "drag-update", 1.0, 1.0);
				Assert.Empty(events);

				Emit(drag, "drag-update", 10.0, 3.0);
				Emit(drag, "drag-update", 20.0, 6.0);
				Emit(drag, "drag-end", 20.0, 6.0);

				Assert.Equal(new[]
				{
					(GestureStatus.Started, 0.0, 0.0),
					(GestureStatus.Running, 10.0, 3.0),
					(GestureStatus.Running, 20.0, 6.0),
					(GestureStatus.Completed, 0.0, 0.0),
				}, events);
			}
		});

		[GtkFact]
		public void ADragThatNeverMovedIsNotAPan() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				int updates = 0;
				var pan = new PanGestureRecognizer();
				pan.PanUpdated += (_, _) => updates++;
				label.GestureRecognizers.Add(pan);

				var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget)!;
				Emit(drag, "drag-begin", 5.0, 5.0);
				Emit(drag, "drag-end", 0.0, 0.0);
				Assert.Equal(0, updates);
			}
		});

		[GtkTheory]
		[InlineData(-200.0, 0.0, SwipeDirection.Left)]
		[InlineData(200.0, 0.0, SwipeDirection.Right)]
		[InlineData(0.0, -200.0, SwipeDirection.Up)]
		[InlineData(0.0, 200.0, SwipeDirection.Down)]
		public void SwipeIsDetectedAtTheEndOfALongDrag(double dx, double dy, SwipeDirection expected) => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				SwipeDirection? swiped = null;
				var swipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Left | SwipeDirection.Right | SwipeDirection.Up | SwipeDirection.Down };
				swipe.Swiped += (_, e) => swiped = e.Direction;
				label.GestureRecognizers.Add(swipe);

				var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget)!;
				Emit(drag, "drag-begin", 100.0, 100.0);
				Emit(drag, "drag-update", dx, dy);
				Emit(drag, "drag-end", dx, dy);
				Assert.Equal(expected, swiped);
			}
		});

		[GtkFact]
		public void ShortDragIsNoSwipe() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				bool swiped = false;
				var swipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
				swipe.Swiped += (_, _) => swiped = true;
				label.GestureRecognizers.Add(swipe);

				var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(widget)!;
				Emit(drag, "drag-begin", 100.0, 100.0);
				Emit(drag, "drag-end", -20.0, 0.0);
				Assert.False(swiped);
			}
		});

		[GtkFact]
		public void PinchReportsIncrementalScale() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				var events = new List<(GestureStatus Status, double Scale)>();
				var pinch = new PinchGestureRecognizer();
				pinch.PinchUpdated += (_, e) => events.Add((e.Status, e.Scale));
				label.GestureRecognizers.Add(pinch);

				var zoom = GtkTestHost.ControllerOf<global::Gtk.GestureZoom>(widget)!;
				Emit(zoom, "scale-changed", 1.5);
				Emit(zoom, "scale-changed", 3.0);

				Assert.Equal(GestureStatus.Started, events[0].Status);
				Assert.Equal((GestureStatus.Running, 1.5), events[1]);
				// 3.0 since the start is twice the previous 1.5.
				Assert.Equal((GestureStatus.Running, 2.0), events[2]);
			}
		});

		[GtkFact]
		public void PointerEnterMoveAndExitCarryThePosition() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				var log = new List<string>();
				var pointer = new PointerGestureRecognizer();
				pointer.PointerEntered += (_, e) => log.Add("enter " + e.GetPosition(label));
				pointer.PointerMoved += (_, e) => log.Add("move " + e.GetPosition(label));
				pointer.PointerExited += (_, e) => log.Add("exit " + e.GetPosition(label));
				label.GestureRecognizers.Add(pointer);

				var motion = GtkTestHost.ControllerOf<global::Gtk.EventControllerMotion>(widget)!;
				Emit(motion, "enter", 3.0, 4.0);
				Emit(motion, "motion", 5.0, 6.0);
				Emit(motion, "leave");

				Assert.Equal(new[] { "enter {X=3 Y=4}", "move {X=5 Y=6}", "exit {X=5 Y=6}" }, log);
			}
		});

		[GtkFact]
		public void PointerPressAndReleaseAreReported() => GtkTestHost.Run(() =>
		{
			var (label, widget, host) = Create();
			using (host)
			{
				var log = new List<string>();
				var pointer = new PointerGestureRecognizer();
				pointer.PointerPressed += (_, e) => log.Add("down " + e.Button);
				pointer.PointerReleased += (_, e) => log.Add("up " + e.Button);
				label.GestureRecognizers.Add(pointer);

				var click = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!;
				Emit(click, "pressed", 1, 1.0, 1.0);
				Emit(click, "released", 1, 1.0, 1.0);
				Assert.Equal(new[] { "down Primary", "up Primary" }, log);
			}
		});

		[GtkFact]
		public void PositionRelativeToTheWindowUsesTheWidgetOffset() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "target", Margin = new Thickness(40, 25) };
			using var host = new ControlsWindowHost(new ContentPage { Content = label });
			var widget = host.Widget(label);

			Point? local = null, inWindow = null;
			var tap = new TapGestureRecognizer();
			tap.Tapped += (_, e) =>
			{
				local = e.GetPosition(label);
				inWindow = e.GetPosition(null);
			};
			label.GestureRecognizers.Add(tap);

			Emit(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!, "released", 1, 2.0, 3.0);

			Assert.Equal(new Point(2, 3), local);
			var bounds = GtkTestHost.BoundsIn(widget);
			Assert.Equal(new Point(bounds.X + 2, bounds.Y + 3), inWindow);
		});

		[GtkFact]
		public void DragAndDropMoveTextInProcess() => GtkTestHost.Run(() =>
		{
			var source = new Label { Text = "source" };
			var target = new Label { Text = "target" };
			using var host = new ControlsWindowHost(new ContentPage { Content = new VerticalStackLayout { source, target } });

			bool dropCompleted = false;
			var drag = new DragGestureRecognizer();
			drag.DragStarting += (_, e) => e.Data.Text = "payload";
			drag.DropCompleted += (_, _) => dropCompleted = true;
			source.GestureRecognizers.Add(drag);

			string? dropped = null;
			bool over = false, left = false;
			var drop = new DropGestureRecognizer();
			drop.DragOver += (_, _) => over = true;
			drop.DragLeave += (_, _) => left = true;
			drop.Drop += async (_, e) => dropped = await e.Data.GetTextAsync();
			target.GestureRecognizers.Add(drop);

			// The controllers exist on the widgets; their signal handlers forward to these members (a synthetic emission cannot return a value).
			Assert.NotNull(GtkTestHost.ControllerOf<global::Gtk.DragSource>(host.Widget(source)));
			Assert.NotNull(GtkTestHost.ControllerOf<global::Gtk.DropTarget>(host.Widget(target)));
			var sourceConnection = GtkGestureConnection.For(host.Widget(source))!;
			var targetConnection = GtkGestureConnection.For(host.Widget(target))!;

			var provider = sourceConnection.PrepareDrag(1, 1);
			Assert.NotNull(provider);
			Assert.Equal("payload", GtkGestureConnection.ActiveDrag?.Text);

			Assert.Equal(Gdk.DragAction.Copy, targetConnection.DragOver(2, 2));
			Assert.True(over);
			targetConnection.DragLeave();
			Assert.True(left);

			Assert.True(targetConnection.Drop("payload", 2, 2));
			Assert.Equal("payload", dropped);

			sourceConnection.EndDrag();
			Assert.True(dropCompleted);
			Assert.Null(GtkGestureConnection.ActiveDrag);
		});

		[GtkFact]
		public void CancellingTheDragStartingEventCancelsTheDrag() => GtkTestHost.Run(() =>
		{
			var source = new Label { Text = "source" };
			using var host = new ControlsWindowHost(new ContentPage { Content = source });

			var drag = new DragGestureRecognizer();
			drag.DragStarting += (_, e) => e.Cancel = true;
			source.GestureRecognizers.Add(drag);

			Assert.Null(GtkGestureConnection.For(host.Widget(source))!.PrepareDrag(1, 1));
			Assert.Null(GtkGestureConnection.ActiveDrag);
		});

		[GtkFact]
		public void ADropWithoutAnActiveDragDeliversTheText() => GtkTestHost.Run(() =>
		{
			var target = new Label { Text = "target" };
			using var host = new ControlsWindowHost(new ContentPage { Content = target });

			string? dropped = null;
			var drop = new DropGestureRecognizer();
			drop.Drop += async (_, e) => dropped = await e.Data.GetTextAsync();
			target.GestureRecognizers.Add(drop);

			Assert.True(GtkGestureConnection.For(host.Widget(target))!.Drop("from outside", 1, 1));
			Assert.Equal("from outside", dropped);

			drop.AllowDrop = false;
			Assert.False(GtkGestureConnection.For(host.Widget(target))!.Drop("again", 1, 1));
		});
	}
}
