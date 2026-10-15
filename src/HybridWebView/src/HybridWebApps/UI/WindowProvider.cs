using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Application = Microsoft.Maui.Controls.Application;

namespace Microsoft.Maui.Controls.HybridWebApps.UI;

/// <summary>Owns only app-created window handles and trusted factories, never hubs or cross-window command dispatch.</summary>
public sealed class HybridWindowHost(HybridAppOptions? options = null)
{
	readonly int _maximumWindows = options?.MaximumWindows ?? 16;
	readonly Dictionary<string, Entry> _windows = new(StringComparer.Ordinal);
	Application? _application;
	Func<HybridWindowRequest, Window>? _factory;
	sealed record Entry(Window Window, string? Owner, string Location, string Lease);

	/// <summary>Binds the generic native window factory once, without knowing the frontend router.</summary>
	/// <param name="application">The actual owning application.</param>
	/// <param name="factory">Creates a fresh owning scope/page/window for a validated bundled location.</param>
	public void Bind(Application application, Func<HybridWindowRequest, Window> factory)
	{
		if (_application is not null)
			throw new InvalidOperationException("The window host is already bound.");
		ArgumentNullException.ThrowIfNull(application);
		ArgumentNullException.ThrowIfNull(factory);
		_application = application;
		_factory = factory;
	}

	/// <summary>Validates a bundle-relative frontend entry location, never an app route catalog or remote URL.</summary>
	/// <param name="location">Frontend-owned asset entry and optional query/fragment.</param>
	public void ValidateLocation(string location)
	{
		BundledLocation.Validate(location);
	}

	/// <summary>Tracks a host-created window and removes it when the OS destroys it.</summary>
	/// <param name="context">The scope bound to that window.</param>
	public void Track(HybridViewContext context)
	{
		ValidateLocation(context.InitialLocation);
		Track(context.Id, context.Window, null, context.InitialLocation, context.Lease);
	}

	void Track(string id, Window window, string? owner, string location, string lease)
	{
		if (_windows.Count >= _maximumWindows)
			throw new CommandException("window_limit", $"The host's native window resource budget of {_maximumWindows} was reached.");
		if (!_windows.TryAdd(id, new Entry(window, owner, location, lease)))
			throw new CommandException("duplicate_window_id", "An app-owned native window already uses this immutable ID.");
		window.Destroying += Destroyed;
		void Destroyed(object? sender, EventArgs args)
		{
			window.Destroying -= Destroyed;
			if (_windows.TryGetValue(id, out var current) && current.Lease == lease)
				_windows.Remove(id);
		}
	}

	internal string Open(HybridViewContext caller, string location, bool supported, HybridWindowOptions? options = null)
	{
		if (!supported)
			throw new NotSupportedException("Separate windows are unavailable on this device.");
		location = BundledLocation.Validate(location);
		var id = options?.Id is not null ? WindowIdentity.Validate(options.Id) : Guid.NewGuid().ToString("D");
		if (_windows.ContainsKey(id))
			throw new CommandException("duplicate_window_id", "An app-owned native window already uses this immutable ID.");
		if (_application is null || _factory is null)
			throw new CommandException("configuration_required", "The host has not supplied a trusted window factory.");
		if (_application.Handler is null)
			throw new CommandException("context_unavailable", "The native application is not attached.");
#if IOS || MACCATALYST
		if (!UIKit.UIApplication.SharedApplication.SupportsMultipleScenes)
			throw new CommandException("configuration_required", "Enable UIApplicationSupportsMultipleScenes in the compiled native app manifest before opening separate windows.");
#endif
		if (_windows.Count >= _maximumWindows)
			throw new CommandException("window_limit", $"The host's native window resource budget of {_maximumWindows} was reached.");
		var window = _factory(new HybridWindowRequest(location, id));
		var pair = _windows.Single(pair => ReferenceEquals(pair.Value.Window, window));
		_windows[pair.Key] = pair.Value with { Owner = caller.Id };
		try
		{
			if (options?.Title is not null)
				window.Title = options.Title;
			if (options?.Width is double width)
				window.Width = width;
			if (options?.Height is double height)
				window.Height = height;
			_application.OpenWindow(window);
		}
		catch
		{
			_windows.Remove(pair.Key);
			if (window is IDisposable disposable)
				disposable.Dispose();
			throw;
		}
		return pair.Key;
	}

	internal WindowSummary[] List(HybridViewContext caller) => _windows
		.Where(pair => pair.Key == caller.Id || pair.Value.Owner == caller.Id)
		.Select(pair => new WindowSummary(pair.Key, pair.Value.Window.Title ?? "", pair.Value.Location,
			pair.Key != caller.Id, pair.Value.Lease)).ToArray();
	internal Window Target(HybridViewContext caller, string? id, string? lease = null)
	{
		if (id is null || (id == caller.Id && (lease is null || lease == caller.Lease)))
			return caller.Window;
		if (_windows.TryGetValue(id, out var entry) && entry.Owner == caller.Id && lease == entry.Lease)
			return entry.Window;
		throw new CommandException("invalid_window", "This native window is not owned by the calling view.");
	}
	internal string Location(HybridViewContext caller, string? id) =>
		id is null || id == caller.Id ? caller.InitialLocation :
		_windows.TryGetValue(id, out var entry) && entry.Owner == caller.Id ? entry.Location :
		throw new CommandException("invalid_window", "This native window is not owned by the calling view.");
	internal string Lease(HybridViewContext caller, string? id) =>
		id is null || id == caller.Id ? caller.Lease :
		_windows.TryGetValue(id, out var entry) && entry.Owner == caller.Id ? entry.Lease :
		throw new CommandException("invalid_window", "This native window is not owned by the calling view.");
	internal WindowInfo? Find(HybridViewContext caller, string id, bool desktop, bool multiple, bool observePosition)
	{
		if (id == caller.Id)
		{
			var own = caller.Window;
			return new(id, own.Title ?? "", Finite(own.X), Finite(own.Y), Finite(own.Width), Finite(own.Height),
				desktop, desktop, multiple, caller.InitialLocation, observePosition, WindowCenter.Supported, caller.Lease);
		}
		if (!_windows.TryGetValue(id, out var entry) || entry.Owner != caller.Id)
			return null;
		var window = entry.Window;
		return new(id, window.Title ?? "", Finite(window.X), Finite(window.Y), Finite(window.Width), Finite(window.Height),
			desktop, desktop, multiple, entry.Location, observePosition, WindowCenter.Supported, entry.Lease);
	}
	static double? Finite(double value) => double.IsFinite(value) ? value : null;
	internal Application Application => _application ?? throw new CommandException("configuration_required", "Bind the native application before using app APIs.");

	internal void Close(HybridViewContext caller, string id, string? lease)
	{
		if (!_windows.TryGetValue(id, out var entry) || entry.Owner != caller.Id || entry.Lease != lease)
			throw new CommandException("invalid_window", "Only a child opened by this window can be closed here.");
		if (_application?.Handler is null)
			throw new CommandException("context_unavailable", "The native application is not attached.");
		_application.CloseWindow(entry.Window);
	}

	internal void CloseCurrent(HybridViewContext caller)
	{
		if (!_windows.ContainsKey(caller.Id) || _application?.Handler is null)
			throw new CommandException("context_unavailable", "The owning native window is not attached.");
		_application.CloseWindow(caller.Window);
	}
	internal void Activate(HybridViewContext caller, string? id = null, string? lease = null)
	{
		if (_application?.Handler is null)
			throw new CommandException("context_unavailable", "The native app is not attached.");
		_application.ActivateWindow(Target(caller, id, lease));
	}
}

/// <summary>TypeScript-selected initial window options, applied before native opening.</summary>
/// <param name="Title">Optional native title; normal transport bounds apply.</param>
/// <param name="Width">Optional finite positive desktop width; native OS constraints apply.</param>
/// <param name="Height">Optional finite positive desktop height; native OS constraints apply.</param>
/// <param name="Id">Optional immutable developer ID; native registry uniqueness is authoritative.</param>
public sealed record HybridWindowOptions(string? Title = null, double? Width = null, double? Height = null, string? Id = null);

/// <summary>Generic native host factory input, not an application route or menu recipe.</summary>
/// <param name="Location">Validated bundle-relative frontend URL.</param>
/// <param name="Id">Native-authoritative immutable developer window ID.</param>
public sealed record HybridWindowRequest(string Location, string Id);

/// <summary>Scoped native implementation of TypeScript window creation and lifecycle operations.</summary>
public sealed partial class WindowProvider(HybridViewContext context, HybridWindowHost host, IDeviceInfo device,
	NativeDispatcher dispatcher, NativeNotifications notifications, TimeProvider time) : IDisposable
{
	/// <summary>Gets the actual MAUI multi-window platform capability; iPhone is excluded.</summary>
	public bool CanOpenWindows => device.Platform == DevicePlatform.WinUI || device.Platform == DevicePlatform.MacCatalyst ||
		device.Platform == DevicePlatform.Android || (device.Platform == DevicePlatform.iOS && device.Idiom == DeviceIdiom.Tablet);
	/// <summary>Opens a trusted bundled location and captures its resource generation atomically.</summary>
	/// <param name="location">Frontend-owned validated bundle-relative location.</param>
	/// <param name="cancellation">The owning command or menu lifetime.</param>
	/// <param name="options">Optional frontend-chosen initial native title/desktop dimensions.</param>
	/// <returns>The new native window ID.</returns>
	internal Task<WindowOpenResult> OpenAsync(string location, CancellationToken cancellation = default, HybridWindowOptions? options = null)
	{
		if (options?.Title is string title && title.Any(char.IsControl))
			throw new CommandException("invalid_arguments", "Native titles must not contain control characters.");
		if (options?.Width is double width && (!double.IsFinite(width) || width <= 0) ||
			options?.Height is double height && (!double.IsFinite(height) || height <= 0))
			throw new CommandException("invalid_arguments", "Use finite positive initial desktop dimensions; native OS constraints apply.");
		if ((options?.Width is not null || options?.Height is not null) &&
			device.Platform != DevicePlatform.MacCatalyst && device.Platform != DevicePlatform.WinUI)
			throw new NotSupportedException("Initial desktop dimensions are unsupported on mobile.");
		return dispatcher.RunAsync(() =>
		{
			var id = host.Open(context, location, CanOpenWindows, options);
			return Task.FromResult(new WindowOpenResult(id, host.Lease(context, id)));
		}, cancellation);
	}
	bool Desktop => device.Platform == DevicePlatform.WinUI || device.Platform == DevicePlatform.MacCatalyst;
	bool Multiple => CanOpenWindows;

	[HybridAction("window.find", MainThread = true)]
	internal WindowFindResult Find(string id)
	{
		WindowIdentity.Validate(id);
		return new(host.Find(context, id, Desktop, Multiple, device.Platform == DevicePlatform.WinUI));
	}
	[HybridAction("window.center", MainThread = true)]
	internal WindowAccepted Center(string? windowId = null, string? windowLease = null)
	{
		WindowCenter.Apply(host.Target(context, windowId, windowLease));
		return new(true);
	}
	[HybridAction("window.activate", MainThread = true)]
	internal WindowAccepted Activate(string? windowId = null, string? windowLease = null)
	{
		if (!Desktop)
			throw new NotSupportedException("Native activation is desktop-only.");
		host.Activate(context, windowId, windowLease);
		return new(true);
	}
	[HybridAction("window.size", MainThread = true)]
	internal WindowAccepted Size(double width, double height, string? windowId = null, string? windowLease = null)
	{
		if (!Desktop)
			throw new NotSupportedException("Native resizing is unsupported on mobile.");
		if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0)
			throw new CommandException("invalid_arguments", "Use finite positive desktop dimensions; native OS constraints apply.");
		var window = host.Target(context, windowId, windowLease);
		window.Width = width;
		window.Height = height;
		return new(true);
	}
	[HybridAction("window.position", MainThread = true)]
	internal WindowAccepted Position(double x, double y, string? windowId = null, string? windowLease = null)
	{
		if (!Desktop)
			throw new NotSupportedException("Native positioning is unsupported on mobile.");
		if (!double.IsFinite(x) || !double.IsFinite(y))
			throw new CommandException("invalid_arguments", "Use finite native coordinates; OS work-area constraints apply.");
		var window = host.Target(context, windowId, windowLease);
		window.X = x;
		window.Y = y;
		return new(true);
	}
	[HybridAction("window.closeCurrent", MainThread = true)]
	internal WindowAccepted CloseCurrent()
	{
		host.CloseCurrent(context);
		return new(true);
	}
	[HybridAction("window.info", MainThread = true)]
	internal WindowInfo Info(string? windowId = null, string? windowLease = null) => Read(windowId, windowLease);

	[HybridAction("window.title", MainThread = true)]
	internal WindowInfo Title(string title, string? windowId = null, string? windowLease = null)
	{
		if (title.Any(char.IsControl))
			throw new CommandException("invalid_arguments", "Native titles must not contain control characters.");
		host.Target(context, windowId, windowLease).Title = title;
		return Read(windowId, windowLease);
	}
	[HybridAction("window.bounds", MainThread = true)]
	internal WindowInfo Bounds(double x, double y, double width, double height)
	{
		if (!Desktop)
			throw new NotSupportedException("Window bounds cannot be changed on mobile.");
		if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0)
			throw new CommandException("invalid_arguments", "Use finite coordinates and positive dimensions; native OS constraints apply.");
		var window = context.Window;
		window.X = x;
		window.Y = y;
		window.Width = width;
		window.Height = height;
		return Read();
	}
	[HybridAction("window.open")]
	internal Task<WindowOpenResult> Open(CancellationToken cancellation, string url = "index.html", HybridWindowOptions? options = null) =>
		OpenAsync(url, cancellation, options);

	[HybridAction("window.list", MainThread = true)]
	internal WindowListResult List() => new(host.List(context));

	[HybridAction("window.close", MainThread = true)]
	internal WindowAccepted Close(string id, string? windowLease = null)
	{
		host.Close(context, id, windowLease);
		return new(true);
	}

	WindowInfo Read(string? id = null, string? lease = null)
	{
		var window = host.Target(context, id, lease);
		return new(id ?? context.Id, window.Title ?? "", Finite(window.X), Finite(window.Y), Finite(window.Width), Finite(window.Height),
			Desktop, Desktop, Multiple, host.Location(context, id), device.Platform == DevicePlatform.WinUI, WindowCenter.Supported, host.Lease(context, id));
	}
	static double? Finite(double value) => double.IsFinite(value) ? value : null;
}

sealed record WindowFindResult(WindowInfo? Window);
sealed record WindowAccepted(bool Accepted);
sealed record WindowOpenResult(string Id, string WindowLease);
sealed record WindowInfo(string Id, string Title, double? X, double? Y, double? Width, double? Height,
	bool CanResize, bool CanPosition, bool CanOpenWindows, string InitialLocation, bool CanObservePosition, bool CanCenter, string WindowLease);
sealed record WindowSummary(string Id, string Title, string Url, bool CanClose, string WindowLease);
sealed record WindowListResult(WindowSummary[] Windows);
