using System.Text.Json.Serialization;
using Microsoft.Maui.Networking;

namespace Maui.HybridApp.Essentials.Connectivity;

/// <summary>Fresh native network reads and scoped actual OS change notifications, not reachability probes.</summary>
public sealed class ConnectivityProvider(IConnectivity connectivity, NativeDispatcher dispatcher,
	NativeNotifications notifications) : IHybridCommandProvider, IDisposable
{
	readonly object _gate = new();
	EventHandler<ConnectivityChangedEventArgs>? _handler;
	long _epoch;
	bool _active;
	bool _disposed;

	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = ConnectivityJsonContext.Default;
		registry.Register("connectivity.info", json.ConnectivityArgs, json.ConnectivityResult, (_, op) =>
			dispatcher.RunAsync(() => Task.FromResult(Read(connectivity.NetworkAccess, connectivity.ConnectionProfiles)), op.Cancellation));
		registry.RegisterTopic("connectivity.changed", StartAsync, Stop);
	}

	static ConnectivityResult Read(NetworkAccess access, IEnumerable<ConnectionProfile> profiles) =>
		new(access.ToString(), profiles.Distinct().Select(profile => profile.ToString()).ToArray());

	Task StartAsync()
	{
		long epoch;
		lock (_gate)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			_active = true;
			epoch = ++_epoch;
		}
		return dispatcher.RunAsync(() =>
		{
			lock (_gate)
			{
				if (!_active || epoch != _epoch) return Task.FromResult(false);
				EventHandler<ConnectivityChangedEventArgs> handler = (_, args) =>
				{
					if (!Volatile.Read(ref _active) || Interlocked.Read(ref _epoch) != epoch) return;
					var value = Read(args.NetworkAccess, args.ConnectionProfiles);
					notifications.PublishCurrent("connectivity.changed", value, ConnectivityJsonContext.Default.ConnectivityResult,
						() => Volatile.Read(ref _active) && Interlocked.Read(ref _epoch) == epoch);
				};
				_handler = handler;
				try { connectivity.ConnectivityChanged += handler; }
				catch
				{
					_active = false;
					++_epoch;
					_handler = null;
					connectivity.ConnectivityChanged -= handler;
					throw;
				}
			}
			return Task.FromResult(true);
		}, CancellationToken.None);
	}

	void Stop()
	{
		EventHandler<ConnectivityChangedEventArgs>? handler;
		lock (_gate)
		{
			_active = false;
			++_epoch;
			handler = _handler;
			_handler = null;
		}
		if (handler is not null) _ = dispatcher.PostAsync(() => connectivity.ConnectivityChanged -= handler);
	}

	/// <summary>Invalidates queued notifications and detaches only this scope's native network handler.</summary>
	public void Dispose()
	{
		lock (_gate) _disposed = true;
		Stop();
	}
}

sealed record ConnectivityArgs;
sealed record ConnectivityResult(string NetworkAccess, string[] ConnectionProfiles);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ConnectivityArgs))]
[JsonSerializable(typeof(ConnectivityResult))]
partial class ConnectivityJsonContext : JsonSerializerContext;
