using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Maui.HybridApp.UI;

/// <summary>Observes actual owning-window geometry, with paired/coalesced payloads and last-subscription native teardown.</summary>
public sealed class WindowEventsProvider(HybridViewContext context, IDeviceInfo device, NativeDispatcher dispatcher,
	NativeNotifications notifications, TimeProvider time) : IHybridCommandProvider, IDisposable
{
	readonly object _gate = new();
	Window? _window;
	ITimer? _timer;
	PropertyChangedEventHandler? _handler;
	long _epoch;
	bool _active;
	bool _disposed;
	double _width, _height, _x, _y;
	bool PositionSupported => device.Platform == DevicePlatform.WinUI;

	/// <inheritdoc />
	public void Register(CommandRegistry registry) => registry.RegisterTopic("window.geometryChanged", StartAsync, Stop);
	Task StartAsync()
	{
		long epoch;
		lock (_gate)
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(WindowEventsProvider));
			_active = true;
			epoch = ++_epoch;
		}
		return dispatcher.PostAsync(() =>
		{
			lock (_gate)
			{
				if (!_active || epoch != _epoch)
					return;
				_window = context.Window;
				_width = _window.Width;
				_height = _window.Height;
				_x = _window.X;
				_y = _window.Y;
				_handler = (_, args) =>
				{
					if (args.PropertyName is not (nameof(Window.Width) or nameof(Window.Height) or nameof(Window.X) or nameof(Window.Y)))
						return;
					lock (_gate)
					{
						if (!_active || epoch != _epoch)
							return;
						_timer ??= time.CreateTimer(_ => Flush(epoch), null, TimeSpan.FromMilliseconds(16), Timeout.InfiniteTimeSpan);
					}
				};
				_window.PropertyChanged += _handler;
			}
		});
	}
	void Flush(long epoch)
	{
		_ = dispatcher.PostAsync(() =>
		{
			WindowGeometryChanged? changed = null;
			lock (_gate)
			{
				if (!_active || epoch != _epoch || _window is null)
					return;
				_timer?.Dispose();
				_timer = null;
				var width = _window.Width;
				var height = _window.Height;
				var x = _window.X;
				var y = _window.Y;
				WindowEventSize? size = double.IsFinite(width) && double.IsFinite(height) && (width != _width || height != _height) ? new(width, height) : null;
				WindowEventPosition? position = PositionSupported && double.IsFinite(x) && double.IsFinite(y) && (x != _x || y != _y) ? new(x, y) : null;
				_width = width;
				_height = height;
				_x = x;
				_y = y;
				if (size is not null || position is not null)
					changed = new WindowGeometryChanged(context.Id, size, position);
			}
			if (changed is not null)
				// Recheck generation under the hub gate without reacquiring the provider gate.
				notifications.PublishCurrent("window.geometryChanged", changed, WindowEventsJsonContext.Default.WindowGeometryChanged,
					() => Volatile.Read(ref _active) && Interlocked.Read(ref _epoch) == epoch);
		});
	}
	void Stop()
	{
		Window? window;
		PropertyChangedEventHandler? handler;
		lock (_gate)
		{
			_active = false;
			++_epoch;
			_timer?.Dispose();
			_timer = null;
			window = _window;
			handler = _handler;
			_window = null;
			_handler = null;
		}
		if (window is not null && handler is not null)
			_ = dispatcher.PostAsync(() => window.PropertyChanged -= handler);
	}
	/// <summary>Invalidates queued callbacks and detaches only this scope's native handlers/timer.</summary>
	public void Dispose() { Stop(); lock (_gate) _disposed = true; }
}
sealed record WindowEventSize(double Width, double Height);
sealed record WindowEventPosition(double X, double Y);
sealed record WindowGeometryChanged(string WindowId, WindowEventSize? Size, WindowEventPosition? Position);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(WindowGeometryChanged))]
partial class WindowEventsJsonContext : JsonSerializerContext;
