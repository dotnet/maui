using System.Buffers;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Owns generated typed event subscriptions for one local document.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HybridEvents
{
	/// <summary>Registers typed attach/detach callbacks, capturing the source before scope disposal.</summary>
	/// <typeparam name="THandler">The native delegate type.</typeparam>
	/// <param name="registry">The document's registry.</param><param name="services">Its local services.</param>
	/// <param name="topic">The declared topic.</param><param name="create">Creates a typed native delegate.</param>
	/// <param name="attach">Attaches that delegate.</param><param name="detach">Detaches the exact same delegate.</param>
	public static void Register<THandler>(CommandRegistry registry, IServiceProvider services, string topic,
		Func<Publisher, THandler> create, Action<THandler> attach, Action<THandler> detach) where THandler : Delegate
	{
		Register<int, THandler>(registry, services, topic, () => 0, create, (_, handler) => attach(handler), (_, handler) => detach(handler));
	}

	/// <summary>Resolves a source on first subscription, retaining that exact receiver for teardown.</summary>
	/// <typeparam name="TSource">The declared receiver type.</typeparam><typeparam name="THandler">The native delegate.</typeparam>
	/// <param name="registry">The local registry.</param><param name="services">The local services.</param>
	/// <param name="topic">The topic.</param><param name="resolve">Resolves the owning receiver before disposal.</param>
	/// <param name="create">Creates the delegate.</param><param name="attach">Typed attach.</param><param name="detach">Typed detach.</param>
	public static void Register<TSource, THandler>(CommandRegistry registry, IServiceProvider services, string topic,
		Func<TSource> resolve, Func<Publisher, THandler> create, Action<TSource, THandler> attach, Action<TSource, THandler> detach) where THandler : Delegate
	{
		var source = new Source<TSource, THandler>(services, topic, resolve, create, attach, detach);
		registry.RegisterTopicSource(topic, source.ReserveStart, source.Stop);
	}

	/// <summary>Serializes generated native payloads while guarding against retired subscription epochs.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class Publisher
	{
		readonly NativeNotifications _notifications;
		readonly string _topic;
		readonly Func<bool> _current;
		internal Publisher(NativeNotifications notifications, string topic, Func<bool> current)
			=> (_notifications, _topic, _current) = (notifications, topic, current);

		/// <summary>Publishes a single typed native value with its strict generated contract.</summary>
		/// <typeparam name="T">The native payload.</typeparam><param name="value">The actual event argument.</param>
		/// <param name="metadata">Its explicit JSON metadata.</param>
		public void Publish<T>(T value, JsonTypeInfo<T> metadata) =>
			Publish(writer => JsonSerializer.Serialize(writer, value, metadata));

		/// <summary>Publishes a generated empty or named multi-parameter payload without runtime reflection.</summary>
		/// <param name="write">The generated, statically typed payload writer.</param>
		public void Publish(Action<Utf8JsonWriter> write)
		{
			if (!_current())
				return;
			try
			{
				var buffer = new ArrayBufferWriter<byte>();
				using (var writer = new Utf8JsonWriter(buffer))
					write(writer);
				using var document = JsonDocument.Parse(buffer.WrittenMemory);
				if (_current())
					_notifications.PublishCurrentJson(_topic, document.RootElement.Clone(), _current);
			}
			catch (Exception error) { Report(_topic, "publish", error); }
		}
	}

	sealed class Source<TSource, THandler>(IServiceProvider services, string topic,
		Func<TSource> resolve, Func<Publisher, THandler> create, Action<TSource, THandler> attach, Action<TSource, THandler> detach) where THandler : Delegate
	{
		readonly object _gate = new();
		long _epoch;
		(NativeDispatcher Dispatcher, TSource Receiver, THandler Handler)? _subscription;

		internal Func<Task> ReserveStart()
		{
			long current;
			lock (_gate)
				current = ++_epoch;
			return () => StartAsync(current);
		}

		Task StartAsync(long current)
		{
			lock (_gate)
			{
				if (current != _epoch)
					return Task.CompletedTask;
			}
			var dispatcher = services.GetRequiredService<NativeDispatcher>();
			var notifications = services.GetRequiredService<NativeNotifications>();
			var receiver = resolve();
			return dispatcher.PostAsync(() =>
			{
				lock (_gate)
				{ if (current != _epoch) return; }
				var handler = create(new Publisher(notifications, topic, () => current == Volatile.Read(ref _epoch)));
				try
				{ attach(receiver, handler); }
				catch
				{
					lock (_gate)
					{ if (current == _epoch) ++_epoch; }
					// A custom add accessor may attach and then throw.
					try
					{ detach(receiver, handler); }
					catch (Exception error) { Report(topic, "attach-rollback", error); }
					throw;
				}
				lock (_gate)
				{
					if (current == _epoch)
					{ _subscription = (dispatcher, receiver, handler); return; }
				}
				// Stop can race a custom add accessor. Never hold our gate while invoking native code.
				try
				{ detach(receiver, handler); }
				catch (Exception error) { Report(topic, "retired-attach", error); }
			});
		}

		internal void Stop()
		{
			(NativeDispatcher Dispatcher, TSource Receiver, THandler Handler)? previous;
			lock (_gate)
			{ ++_epoch; previous = _subscription; _subscription = null; }
			if (previous is { } subscription)
				_ = DetachAsync(subscription.Dispatcher, subscription.Receiver, subscription.Handler);
		}

		async Task DetachAsync(NativeDispatcher dispatcher, TSource receiver, THandler previous)
		{
			try
			{ await dispatcher.PostAsync(() => detach(receiver, previous)).ConfigureAwait(false); }
			catch (Exception error) { Report(topic, "detach", error); }
		}
	}

	static void Report(string topic, string phase, Exception error) =>
		Console.WriteLine($"HYBRIDWEBAPP_EVENT_ERROR topic={topic} phase={phase} type={error.GetType().Name}");
}
