using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

/// <summary>
/// Assembly-wide GTK bootstrap and the pump/measure helpers of the real-widget tests. Ported from the Xamarin.Forms
/// GTK test host. It is in the global namespace so every fixture reaches it without a using.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>GTK may only be used from the thread that called <c>gtk_init</c>, and on Windows the GDK backend needs a
/// single-threaded apartment (OleInitialize) - so GTK gets a dedicated STA thread and every test body is marshalled
/// onto it with <see cref="Run(Action)"/>.</item>
/// <item>No <c>async</c> test bodies (the GLib synchronization context would post continuations to a loop nobody runs):
/// await through <see cref="Await(System.Threading.Tasks.Task, int)"/>.</item>
/// <item>Windows are never destroyed (GLib wrappers drop toggle references from the finalizer thread, which aborts the
/// process); they are hidden and kept alive with <see cref="Retire"/>.</item>
/// <item>Set <c>MAUI_GTK_REQUIRE_NATIVE</c> in an environment that is supposed to run GTK: an unusable runtime then fails instead of skipping.</item>
/// </list>
/// </remarks>
public static class GtkTestHost
{
	const string RequiredVariable = "MAUI_GTK_REQUIRE_NATIVE";
	const int MaxDrainIterations = 2000;

	static readonly object s_gate = new();
	static readonly List<Gtk.Window> s_retired = new();
	static readonly List<string> s_complaints = new();
	static readonly System.Collections.Concurrent.BlockingCollection<Action> s_work = new();

	static volatile bool s_backgroundPump;
	static bool s_initialized;
	static string? s_unavailable;
	static Thread? s_gtkThread;

	/// <summary>Pump rounds a layout assertion needs.</summary>
	public const int DefaultPumpRounds = 6;

	/// <summary>
	/// Brings GTK up (once) and returns why it cannot be, or <see langword="null"/> when it is available.
	/// </summary>
	public static string? UnavailableReason()
	{
		lock (s_gate)
		{
			if (s_initialized)
				return null;

			if (s_unavailable is not null)
				return s_unavailable;

			var reason = DisplayUnavailableReason();

			if (reason is null)
			{
				try
				{
					StartGtkThread();
					s_initialized = true;
					return null;
				}
				catch (DllNotFoundException e)
				{
					reason = NoNativeRuntime(e.Message);
				}
				catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException inner)
				{
					reason = NoNativeRuntime(inner.Message);
				}
			}

			return s_unavailable = reason;
		}
	}

	/// <summary>True when <c>MAUI_GTK_REQUIRE_NATIVE</c> says an unusable GTK runtime must fail the run instead of skipping.</summary>
	public static bool NativeRuntimeRequired => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RequiredVariable));

	/// <summary>Brings GTK up or throws (the tests of an unavailable runtime are skipped by <see cref="GtkFactAttribute"/> unless it is required).</summary>
	public static void EnsureInitialized()
	{
		var reason = UnavailableReason();
		if (reason is not null)
			throw new InvalidOperationException($"{reason} ({RequiredVariable} requires a usable GTK runtime.)");
	}

	static string? DisplayUnavailableReason()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return null;

		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) ||
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
			return null;

		return "No DISPLAY. These are real GTK widget tests: run them with `xvfb-run -a dotnet test`.";
	}

	static string NoNativeRuntime(string detail) =>
		"The native GTK 4 runtime is not available, so there are no widgets to test: " + detail;

	[DllImport("ole32.dll")]
	static extern int OleInitialize(IntPtr reserved);

	static void StartGtkThread()
	{
		using var ready = new ManualResetEventSlim();
		ExceptionDispatchInfo? startupError = null;

		s_gtkThread = new Thread(() =>
		{
			try
			{
				// GDK's Windows clipboard and drag and drop are OLE based and need the thread to be OLE initialized.
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					OleInitialize(IntPtr.Zero);

				GtkRuntime.EnsureInitialized();
				InstallLogCapture();
			}
			catch (Exception e)
			{
				startupError = ExceptionDispatchInfo.Capture(e);
				ready.Set();
				return;
			}

			ready.Set();

			while (true)
			{
				if (s_work.TryTake(out var work, 5))
					work();

				if (s_backgroundPump)
				{
					for (var i = 0; i < MaxDrainIterations && GLib.MainContext.Iteration(false); i++)
					{
					}
				}
			}
		})
		{
			IsBackground = true,
			Name = "Gtk",
		};

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			s_gtkThread.SetApartmentState(ApartmentState.STA);

		s_gtkThread.Start();
		ready.Wait();

		startupError?.Throw();
	}

	/// <summary>Records GTK criticals/warnings (for example "Allocating size ... without calling gtk_widget_measure").</summary>
	static void InstallLogCapture()
	{
		GLib.LogFunc record = (domain, level, message) =>
		{
			lock (s_complaints)
				s_complaints.Add(domain + ": " + message);
		};

		var flags = GLib.LogLevelFlags.Critical | GLib.LogLevelFlags.Warning;
		GLib.Log.SetLogHandler("Gtk", flags, record);
		GLib.Log.SetLogHandler("Gdk", flags, record);
		GLib.Log.SetLogHandler("GLib-GObject", flags, record);
		GLib.Log.SetLogHandler("Pango", flags, record);
	}

	static StderrCapture? s_capture;

	/// <summary>
	/// The GTK criticals/warnings (including CSS parser errors and "Allocating size ... without calling gtk_widget_measure")
	/// written since the last <see cref="ClearComplaints"/>, as captured from the native stderr during <see cref="Run(Action)"/>.
	/// </summary>
	public static string[] Complaints
	{
		get
		{
			lock (s_complaints)
			{
				if (s_capture is not null)
					s_complaints.AddRange(s_capture.Read());

				// A headless session has no D-Bus accessibility service; GTK logs that once per process, not a defect.
				return s_complaints.FindAll(c => !c.Contains("accessibility bus")).ToArray();
			}
		}
	}

	/// <summary>
	/// Fails when GTK complained since the last <see cref="ClearComplaints"/>: layout violations ("Allocating size ... without calling
	/// gtk_widget_measure", "Allocation ... too small"), CSS parser errors, or any other critical/warning.
	/// </summary>
	public static void AssertNoComplaints()
	{
		var complaints = Complaints;
		Assert.True(complaints.Length == 0, "GTK complained: " + string.Join(" | ", complaints));
	}

	/// <summary>Forgets the recorded GTK criticals/warnings.</summary>
	public static void ClearComplaints()
	{
		lock (s_complaints)
		{
			s_capture?.Read();
			s_complaints.Clear();
		}
	}

	/// <summary>
	/// Queues <paramref name="work"/> on the GTK thread without waiting for it, and makes the thread iterate the GLib main context
	/// between work items from now on (Essentials tests await GTK callbacks, such as the clipboard, from other threads).
	/// Core tests stay deterministic until this is first called: they pump explicitly.
	/// </summary>
	public static void Post(Action work)
	{
		EnsureInitialized();
		s_backgroundPump = true;
		s_work.Add(work);
	}

	/// <summary>Runs a test body on the GTK thread, rethrowing whatever it throws.</summary>
	public static void Run(Action body)
	{
		EnsureInitialized();

		if (Thread.CurrentThread == s_gtkThread)
		{
			body();
			return;
		}

		ExceptionDispatchInfo? error = null;

		using (var done = new ManualResetEventSlim())
		{
			s_work.Add(() =>
			{
				var capture = StderrCapture.Start();
				lock (s_complaints)
					s_capture = capture;

				try
				{
					body();
				}
				catch (Exception e)
				{
					error = ExceptionDispatchInfo.Capture(e);
				}
				finally
				{
					lock (s_complaints)
					{
						if (capture is not null)
							s_complaints.AddRange(capture.Stop());
						s_capture = null;
					}

					done.Set();
				}
			});

			done.Wait();
		}

		error?.Throw();
	}

	/// <summary>Pumps GTK so queued relayout happens, forcing an explicit allocation of the toplevel (the frame clock does not tick headlessly).</summary>
	public static void Pump(Gtk.Window? toplevel = null, int rounds = DefaultPumpRounds)
	{
		for (int i = 0; i < rounds; i++)
		{
			Drain();

			if (toplevel is not null)
				Allocate(toplevel, toplevel.Width, toplevel.Height);

			for (int j = 0; j < MaxDrainIterations && GLib.MainContext.Iteration(false); j++)
			{
			}
		}
	}

	/// <summary>Resizes a test window and commits the new allocation (GTK 4 has no resize request).</summary>
	/// <remarks>
	/// A shown window follows its surface: on Windows the window manager applies the new size asynchronously, and under load a late
	/// configure of the old size lands after the forced allocation and restores it. Re-apply until the window keeps the size (up to 3 s).
	/// </remarks>
	public static void Resize(Gtk.Window window, int width, int height)
	{
		window.SetDefaultSize(width, height);
		PumpUntil(() =>
		{
			if (window.Width == width && window.Height == height)
				return true;

			Allocate(window, width, height);
			return false;
		}, window, 3000);
		Pump(window);
	}

	/// <summary>Pumps until <paramref name="condition"/> holds or the deadline expires.</summary>
	public static bool PumpUntil(Func<bool> condition, Gtk.Window? toplevel = null, int timeoutMs = 5000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

		while (true)
		{
			if (condition())
				return true;

			if (DateTime.UtcNow >= deadline)
				return false;

			Pump(toplevel, 1);
		}
	}

	/// <summary>Waits for a task by pumping GTK instead of blocking the thread.</summary>
	public static T Await<T>(System.Threading.Tasks.Task<T> task, int timeoutMs = 10000)
	{
		Await((System.Threading.Tasks.Task)task, timeoutMs);
		return task.GetAwaiter().GetResult();
	}

	/// <summary>Waits for a task by pumping GTK instead of blocking the thread.</summary>
	public static void Await(System.Threading.Tasks.Task task, int timeoutMs = 10000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

		while (!task.IsCompleted && DateTime.UtcNow < deadline)
		{
			Pump(null, 1);
			Thread.Sleep(1);
		}

		Assert.True(task.IsCompleted, $"task did not complete within {timeoutMs}ms");
		task.GetAwaiter().GetResult();
	}

	/// <summary>Hides a test window and keeps it alive for the rest of the run (never <c>Destroy()</c> it).</summary>
	public static void Retire(Gtk.Window? window)
	{
		if (window is null)
			return;

		window.Visible = false;
		Pump(null, 2);
		s_retired.Add(window);
	}

	/// <summary>Measures a widget and then allocates it, in that order, never below its minimum (GTK 4 drops anything else).</summary>
	static void Allocate(Gtk.Widget widget, int width, int height)
	{
		if (width <= 0 || height <= 0)
			return;

		widget.Measure(Gtk.Orientation.Horizontal, -1, out int minimumWidth, out _, out _, out _);
		width = Math.Max(width, minimumWidth);
		widget.Measure(Gtk.Orientation.Vertical, width, out int minimumHeight, out _, out _, out _);
		height = Math.Max(height, minimumHeight);

		widget.SizeAllocate(new Gdk.Rectangle(0, 0, width, height));
	}

	static void Drain()
	{
		for (int i = 0; i < MaxDrainIterations && Gtk.Application.EventsPending(); i++)
			Gtk.Application.RunIteration();
	}

	/// <summary>Renders a widget and returns its pixels as tightly-packed premultiplied BGRA.</summary>
	public static byte[] RenderToBytes(Gtk.Widget widget, out int width, out int height)
	{
		width = widget.Width;
		height = widget.Height;

		if (width <= 0 || height <= 0)
			throw new InvalidOperationException($"the widget has no allocation to render: {Describe(widget)}");

		var native = widget.Native
			?? throw new InvalidOperationException($"the widget is not in a realized toplevel, so there is no renderer: {Describe(widget)}");

		var paintable = new Gtk.WidgetPaintable(widget);
		var snapshot = new Gtk.Snapshot();
		((Gdk.IPaintable)paintable).Snapshot(snapshot, width, height);

		var node = snapshot.ToNode()
			?? throw new InvalidOperationException($"the widget produced no render nodes: {Describe(widget)}");

		var bounds = Graphene.Rect.Alloc();
		bounds.Init(0, 0, width, height);

		using var texture = native.Renderer.RenderTexture(node, bounds);
		return texture.Download();
	}

	/// <summary>The colour of one pixel of a rendered widget, as 0-255 RGB.</summary>
	public static (byte R, byte G, byte B) PixelAt(Gtk.Widget widget, int x, int y)
	{
		var bytes = RenderToBytes(widget, out int width, out int height);

		if (x < 0 || y < 0 || x >= width || y >= height)
			throw new ArgumentOutOfRangeException(nameof(x), $"({x},{y}) is outside the {width}x{height} widget");

		int offset = (y * width * 4) + (x * 4);
		return (bytes[offset + 2], bytes[offset + 1], bytes[offset]);
	}

	/// <summary>Where a widget sits inside <paramref name="ancestor"/> (default: its root), as a rectangle.</summary>
	public static Gdk.Rectangle BoundsIn(Gtk.Widget widget, Gtk.Widget? ancestor = null)
	{
		var target = ancestor ?? (Gtk.Widget?)widget.Root ?? widget;

		if (!widget.ComputeBounds(target, out var bounds))
			throw new InvalidOperationException($"{Describe(widget)} has no position relative to {Describe(target)}");

		return new Gdk.Rectangle((int)Math.Round(bounds.X), (int)Math.Round(bounds.Y), (int)Math.Round(bounds.Width), (int)Math.Round(bounds.Height));
	}

	/// <summary>Depth-first search of a native widget tree.</summary>
	public static List<T> Find<T>(Gtk.Widget? root) where T : Gtk.Widget
	{
		var found = new List<T>();

		void Walk(Gtk.Widget widget)
		{
			if (widget is T match)
				found.Add(match);

			for (var child = widget.FirstChild; child is not null; child = child.NextSibling)
				Walk(child);
		}

		if (root is not null)
			Walk(root);

		return found;
	}

	/// <summary>Emits <c>pressed</c> on the widget's own <see cref="Gtk.GestureClick"/>.</summary>
	public static void PressButton(Gtk.Widget widget)
	{
		var gesture = ControllerOf<Gtk.GestureClick>(widget)
			?? throw new InvalidOperationException($"the widget has no GestureClick to press. {Describe(widget)}");

		GLib.Signal.Emit(gesture, "pressed", 1, widget.Width / 2.0, widget.Height / 2.0);
	}

	/// <summary>Every controller of the given kind attached to a widget.</summary>
	public static List<T> AllControllersOf<T>(Gtk.Widget widget) where T : Gtk.EventController
	{
		var result = new List<T>();
		var controllers = widget.ObserveControllers();

		for (uint i = 0; i < controllers.NItems; i++)
		{
			if (controllers.GetObject(i) is T match)
				result.Add(match);
		}

		return result;
	}

	/// <summary>The first controller of the given kind attached to a widget, or null.</summary>
	public static T? ControllerOf<T>(Gtk.Widget widget) where T : Gtk.EventController
	{
		var controllers = widget.ObserveControllers();

		for (uint i = 0; i < controllers.NItems; i++)
		{
			if (controllers.GetObject(i) is T match)
				return match;
		}

		return null;
	}

	/// <summary>Every controller of the given kind attached to a widget.</summary>
	public static List<T> ControllersOf<T>(Gtk.Widget widget) where T : Gtk.EventController
	{
		var result = new List<T>();
		var controllers = widget.ObserveControllers();

		for (uint i = 0; i < controllers.NItems; i++)
		{
			if (controllers.GetObject(i) is T match)
				result.Add(match);
		}

		return result;
	}

	/// <summary>A short description of a widget for assertion messages.</summary>
	public static string Describe(Gtk.Widget? widget) =>
		widget is null
			? "<null>"
			: $"{widget.GetType().Name}[{widget.Width}x{widget.Height} visible={widget.Visible}]";
}
