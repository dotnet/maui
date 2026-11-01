using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Gtk.Web;
using Microsoft.Web.WebView2.Core;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>A recording stand-in for the WebView2 controller (no WebView2 runtime needed).</summary>
sealed class FakeWebView2Controller : IGtkWebView2Controller
{
	public readonly List<(int X, int Y, int Width, int Height)> Bounds = new();
	public readonly List<bool> Visibility = new();
	public int MoveFocusCalls;
	public int PositionNotifications;
	public bool Disposed;

	public CoreWebView2? Core => null;

	public CoreWebView2Controller? Controller => null;

	public event EventHandler? GotFocus;

	public event EventHandler<bool>? FocusLeaveRequested;

	public void SetBounds(int x, int y, int width, int height) => Bounds.Add((x, y, width, height));

	public void SetVisible(bool visible) => Visibility.Add(visible);

	public void MoveFocusIn() => MoveFocusCalls++;

	public void NotifyParentWindowPositionChanged() => PositionNotifications++;

	public void Dispose() => Disposed = true;

	public void RaiseGotFocus() => GotFocus?.Invoke(this, EventArgs.Empty);

	public void RaiseFocusLeave(bool forward) => FocusLeaveRequested?.Invoke(this, forward);
}

/// <summary>Host services that create <see cref="FakeWebView2Controller"/>s, optionally only when the test completes them.</summary>
sealed class FakeHostServices : GtkWebView2HostServices
{
	public static readonly IntPtr FakeHandle = new(0x1234);

	public double Scale = 1;
	public bool AutoComplete = true;
	public readonly List<IntPtr> Parents = new();
	public readonly List<FakeWebView2Controller> Controllers = new();
	public readonly List<TaskCompletionSource<IGtkWebView2Controller>> Pending = new();
	public readonly List<IntPtr> FocusedToplevels = new();
	public readonly List<Action> LocationCallbacks = new();
	public int Unwatched;

	internal override IntPtr GetToplevelHandle(global::Gtk.INative native) => FakeHandle;

	internal override double GetScale(global::Gtk.INative native) => Scale;

	internal override Task<IGtkWebView2Controller> CreateControllerAsync(IntPtr parentWindow)
	{
		Parents.Add(parentWindow);

		if (!AutoComplete)
		{
			var source = new TaskCompletionSource<IGtkWebView2Controller>();
			Pending.Add(source);
			return source.Task;
		}

		var controller = new FakeWebView2Controller();
		Controllers.Add(controller);
		return Task.FromResult<IGtkWebView2Controller>(controller);
	}

	internal override IDisposable? WatchLocation(IntPtr hwnd, Action moved)
	{
		LocationCallbacks.Add(moved);
		return new Unwatch(this);
	}

	internal override void FocusToplevel(IntPtr hwnd) => FocusedToplevels.Add(hwnd);

	sealed class Unwatch : IDisposable
	{
		readonly FakeHostServices _services;

		public Unwatch(FakeHostServices services) => _services = services;

		public void Dispose() => _services.Unwatched++;
	}
}

/// <summary>
/// <see cref="MauiGtkWebView2Host"/>: lifecycle, bounds, visibility and focus against a fake controller, so they run on every platform
/// (the host is plain GTK; only the services touch Win32/WebView2).
/// </summary>
public class WebView2HostTests
{
	const int WindowWidth = 400;
	const int WindowHeight = 300;

	sealed class Fixture
	{
		public global::Gtk.Window Window = null!;
		public global::Gtk.Box Content = null!;
		public MauiGtkWebView2Host Host = null!;
		public FakeHostServices Services = null!;
	}

	/// <summary>A borderless window with a box that holds the host (margins 20/30/40/50) - realized, mapped and laid out.</summary>
	static Fixture Show(FakeHostServices? services = null, bool present = true)
	{
		var f = new Fixture
		{
			Services = services ?? new FakeHostServices(),
			Window = new global::Gtk.Window { Decorated = false, DefaultWidth = WindowWidth, DefaultHeight = WindowHeight },
			Content = new global::Gtk.Box(global::Gtk.Orientation.Horizontal, 0),
			Host = new MauiGtkWebView2Host
			{
				MarginStart = 20,
				MarginTop = 30,
				MarginEnd = 40,
				MarginBottom = 50,
				Hexpand = true,
				Vexpand = true,
			},
		};

		f.Host.Services = f.Services;
		f.Content.Append(f.Host);
		f.Window.Child = f.Content;

		if (present)
		{
			f.Window.Present();
			GtkTestHost.Resize(f.Window, WindowWidth, WindowHeight);
			GtkTestHost.PumpUntil(() => f.Host.Controller is not null, f.Window, 3000);
			GtkTestHost.Pump(f.Window);
		}

		return f;
	}

	static void Done(Fixture f) => GtkTestHost.Retire(f.Window);

	static (int X, int Y, int Width, int Height) Expected(Fixture f, double scale)
	{
		var native = (global::Gtk.INative)f.Window;
		native.GetSurfaceTransform(out var tx, out var ty);
		var b = GtkTestHost.BoundsIn(f.Host, f.Window);
		var left = (int)Math.Floor((b.X + tx) * scale);
		var top = (int)Math.Floor((b.Y + ty) * scale);
		var right = (int)Math.Ceiling((b.X + b.Width + tx) * scale);
		var bottom = (int)Math.Ceiling((b.Y + b.Height + ty) * scale);
		return (left, top, right - left, bottom - top);
	}

	[GtkFact]
	public void RealizingCreatesOneControllerOnTheToplevelHandle() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			Assert.Equal(new[] { FakeHostServices.FakeHandle }, f.Services.Parents);
			Assert.Single(f.Services.Controllers);
			Assert.Same(f.Services.Controllers[0], f.Host.Controller);
			Assert.Equal(FakeHostServices.FakeHandle, f.Host.ToplevelHandle);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void NothingIsCreatedBeforeTheWidgetIsRealized() => GtkTestHost.Run(() =>
	{
		var f = Show(present: false);
		GtkTestHost.Pump();

		Assert.Empty(f.Services.Parents);
		Assert.Null(f.Host.Controller);
		Assert.Equal(IntPtr.Zero, f.Host.ToplevelHandle);
		Done(f);
	});

	[GtkTheory]
	[InlineData(1.0)]
	[InlineData(1.5)]
	[InlineData(2.0)]
	public void BoundsAreTheWidgetAllocationInToplevelPixels(double scale) => GtkTestHost.Run(() =>
	{
		var services = new FakeHostServices { Scale = scale };
		var f = Show(services);
		try
		{
			var controller = f.Services.Controllers[0];
			var expected = Expected(f, scale);

			Assert.Equal(expected, controller.Bounds[^1]);
			Assert.Equal(expected, f.Host.ComputeNativeBounds());
			// The margins (20/30/40/50) fix the allocation independently of the scale.
			Assert.True(expected.Width >= (int)Math.Floor((WindowWidth - 60 - 2) * scale), $"width {expected.Width}");
			Assert.True(expected.Height >= (int)Math.Floor((WindowHeight - 80 - 2) * scale), $"height {expected.Height}");
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void BoundsFollowTheWindowSize() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var controller = f.Services.Controllers[0];
			var before = controller.Bounds[^1];

			GtkTestHost.Resize(f.Window, WindowWidth + 100, WindowHeight + 60);
			GtkTestHost.Pump(f.Window);

			var after = controller.Bounds[^1];
			Assert.Equal(before.Width + 100, after.Width);
			Assert.Equal(before.Height + 60, after.Height);
			Assert.Equal(Expected(f, 1), after);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void BoundsFollowAPositionChangeWithoutASizeChange() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var controller = f.Services.Controllers[0];
			var before = controller.Bounds[^1];

			// Same size, new position: GTK does not call size_allocate for that.
			f.Host.MarginStart = 60;
			f.Host.MarginEnd = 0;
			GtkTestHost.Pump(f.Window);
			f.Host.UpdateBounds();

			var after = controller.Bounds[^1];
			Assert.Equal(before.X + 40, after.X);
			Assert.Equal(before.Width, after.Width);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void UnchangedBoundsAreNotPushedAgain() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var controller = f.Services.Controllers[0];
			var count = controller.Bounds.Count;

			f.Host.UpdateBounds();
			f.Host.UpdateBounds();
			GtkTestHost.Pump(f.Window);

			Assert.Equal(count, controller.Bounds.Count);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void VisibilityFollowsMapAndAncestors() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var controller = f.Services.Controllers[0];
			Assert.True(controller.Visibility[^1]);

			f.Content.Visible = false;
			GtkTestHost.Pump(f.Window);
			Assert.False(controller.Visibility[^1]);

			f.Content.Visible = true;
			GtkTestHost.Pump(f.Window);
			Assert.True(controller.Visibility[^1]);

			f.Host.Visible = false;
			GtkTestHost.Pump(f.Window);
			Assert.False(controller.Visibility[^1]);

			f.Host.Visible = true;
			GtkTestHost.Pump(f.Window);
			Assert.True(controller.Visibility[^1]);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void UnrealizingClosesTheControllerAndRealizingAgainCreatesANewOne() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var closing = 0;
			f.Host.ControllerClosing += (s, e) => closing++;
			var first = f.Services.Controllers[0];

			f.Window.Child = null;
			GtkTestHost.Pump(f.Window);

			Assert.True(first.Disposed);
			Assert.Equal(1, closing);
			Assert.Null(f.Host.Controller);
			Assert.Equal(1, f.Services.Unwatched);

			f.Window.Child = f.Content;
			GtkTestHost.PumpUntil(() => f.Host.Controller is not null, f.Window, 3000);

			Assert.Equal(2, f.Services.Controllers.Count);
			Assert.Same(f.Services.Controllers[1], f.Host.Controller);
			Assert.NotEmpty(f.Services.Controllers[1].Bounds);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void AControllerCompletedAfterUnrealizeIsClosedAndNotAdopted() => GtkTestHost.Run(() =>
	{
		var services = new FakeHostServices { AutoComplete = false };
		var f = Show(services);
		try
		{
			Assert.Single(services.Pending);
			Assert.Null(f.Host.Controller);

			f.Window.Child = null;
			GtkTestHost.Pump(f.Window);

			var late = new FakeWebView2Controller();
			services.Pending[0].SetResult(late);
			GtkTestHost.PumpUntil(() => late.Disposed, f.Window, 3000);

			Assert.True(late.Disposed);
			Assert.Null(f.Host.Controller);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void AFailedCreationIsReportedAndLeavesNoController() => GtkTestHost.Run(() =>
	{
		var services = new FakeHostServices { AutoComplete = false };
		var f = Show(services);
		try
		{
			Exception? failure = null;
			f.Host.ControllerFailed += (s, e) => failure = e;

			services.Pending[0].SetException(new InvalidOperationException("locked"));
			GtkTestHost.PumpUntil(() => failure is not null, f.Window, 3000);

			Assert.Equal("locked", failure!.Message);
			Assert.Null(f.Host.Controller);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void ToplevelMovesAreForwardedToTheController() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var controller = f.Services.Controllers[0];
			Assert.Single(f.Services.LocationCallbacks);

			f.Services.LocationCallbacks[0]();
			f.Services.LocationCallbacks[0]();

			Assert.Equal(2, controller.PositionNotifications);

			f.Window.Child = null;
			GtkTestHost.Pump(f.Window);
			f.Host.NotifyToplevelMoved();
			Assert.Equal(2, controller.PositionNotifications);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void FocusEnteringTheHostMovesFocusIntoTheWebView() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var controller = f.Services.Controllers[0];
			var baseline = controller.MoveFocusCalls;

			f.Host.FocusEntered();

			Assert.Equal(baseline + 1, controller.MoveFocusCalls);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void AClickInsideTheWebViewMakesTheHostTheGtkFocusWidget() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var controller = f.Services.Controllers[0];
			var other = new global::Gtk.Button { Label = "other", Focusable = true };
			f.Content.Prepend(other);
			GtkTestHost.Pump(f.Window);
			other.GrabFocus();
			Assert.False(f.Host.IsFocus);
			var baseline = controller.MoveFocusCalls;

			controller.RaiseGotFocus();

			Assert.True(f.Host.IsFocus);
			// Taking the GTK focus because of the click must not push the focus into the web view again.
			Assert.Equal(baseline, controller.MoveFocusCalls);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void TabbingOutOfTheWebViewReturnsFocusToTheToplevelAndMovesOn(bool forward) => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var next = new global::Gtk.Button { Label = "next", Focusable = true };
			var previous = new global::Gtk.Button { Label = "previous", Focusable = true };
			f.Content.Prepend(previous);
			f.Content.Append(next);
			GtkTestHost.Pump(f.Window);
			f.Host.GrabFocus();
			var controller = f.Services.Controllers[0];

			controller.RaiseFocusLeave(forward);

			Assert.Equal(new[] { FakeHostServices.FakeHandle }, f.Services.FocusedToplevels);
			Assert.False(f.Host.IsFocus);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void AnotherWidgetTakingTheGtkFocusReturnsTheWin32FocusToTheToplevel() => GtkTestHost.Run(() =>
	{
		var f = Show();
		try
		{
			var other = new global::Gtk.Button { Label = "other", Focusable = true };
			f.Content.Prepend(other);
			GtkTestHost.Pump(f.Window);
			Assert.True(GtkTestHost.PumpUntil(() => f.Window.IsActive, f.Window, 5000), "the window never became active");
			f.Host.GrabFocus();
			GtkTestHost.Pump(f.Window);
			Assert.Empty(f.Services.FocusedToplevels);

			other.GrabFocus();
			GtkTestHost.Pump(f.Window);

			Assert.False(f.Host.IsFocus);
			Assert.Equal(new[] { FakeHostServices.FakeHandle }, f.Services.FocusedToplevels);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void LosingTheGtkFocusWithoutAControllerDoesNotTouchWin32() => GtkTestHost.Run(() =>
	{
		var f = Show(new FakeHostServices { AutoComplete = false });
		try
		{
			f.Host.FocusLeft();

			Assert.Empty(f.Services.FocusedToplevels);
		}
		finally
		{
			Done(f);
		}
	});

	[GtkFact]
	public void EvaluationsQueuedBeforeAControllerExistsAreCancelledWhenTheEngineIsDisposed() => GtkTestHost.Run(() =>
	{
		var host = new MauiGtkWebView2Host();
		var first = new EvaluateJavaScriptAsyncRequest("1");
		var second = new EvaluateJavaScriptAsyncRequest("2");
		host.Engine.EvaluateScript("1", first);
		host.Engine.EvaluateScript("2", second);
		Assert.False(first.Task.IsCompleted);

		host.Engine.Dispose();

		Assert.True(first.Task.IsCanceled);
		Assert.True(second.Task.IsCanceled);

		// An evaluation issued after the disposal is not left hanging either.
		var late = new EvaluateJavaScriptAsyncRequest("3");
		host.Engine.EvaluateScript("3", late);
		Assert.True(late.Task.IsCanceled);
	});

	[GtkFact]
	public void EvaluationsQueuedBeforeAControllerExistsFailWhenTheControllerCannotBeCreated() => GtkTestHost.Run(() =>
	{
		var f = Show(new FakeHostServices { AutoComplete = false });
		try
		{
			var request = new EvaluateJavaScriptAsyncRequest("1");
			f.Host.Engine.EvaluateScript("1", request);
			Assert.True(GtkTestHost.PumpUntil(() => f.Services.Pending.Count > 0, f.Window, 3000));
			var failure = new InvalidOperationException("locked");

			f.Services.Pending[0].SetException(failure);

			Assert.True(GtkTestHost.PumpUntil(() => request.Task.IsCompleted, f.Window, 3000), "the evaluation never completed");
			Assert.True(request.Task.IsFaulted);
			Assert.Same(failure, request.Task.Exception!.GetBaseException());
		}
		finally
		{
			Done(f);
		}
	});
}
