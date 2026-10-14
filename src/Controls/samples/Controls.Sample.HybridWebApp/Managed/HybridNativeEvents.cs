using Microsoft.Extensions.DependencyInjection;

namespace Maui.HybridApp;

/// <summary>Owns sender-free native change subscriptions for generated referenced contracts.</summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class HybridNativeEvents
{
	/// <summary>Registers a bounded native change source with epoch-guarded first/last ownership.</summary>
	/// <param name="registry">The local command registry.</param><param name="services">The local proxy.</param>
	/// <param name="topic">The declared native topic.</param><param name="attach">Typed native attach.</param>
	/// <param name="detach">Typed native detach.</param>
	public static void Register(CommandRegistry registry, IServiceProvider services, string topic,
		Action<EventHandler<EventArgs>> attach, Action<EventHandler<EventArgs>> detach)
	{
		var dispatcher = services.GetRequiredService<NativeDispatcher>();
		var notifications = services.GetRequiredService<NativeNotifications>();
		long epoch = 0;
		EventHandler<EventArgs>? changed = null;
		registry.RegisterTopic(topic, () =>
		{
			var current = Interlocked.Increment(ref epoch);
			return dispatcher.PostAsync(() =>
			{
				if (current != Volatile.Read(ref epoch)) return;
				changed = (_, _) =>
				{
					if (current == Volatile.Read(ref epoch))
						notifications.PublishCurrent(topic, new Ack(true), ProtocolJsonContext.Default.Ack,
							() => current == Volatile.Read(ref epoch));
				};
				attach(changed);
			});
		}, () =>
		{
			Interlocked.Increment(ref epoch);
			var previous = changed; changed = null;
			if (previous is not null) _ = dispatcher.PostAsync(() => detach(previous));
		});
	}
}
