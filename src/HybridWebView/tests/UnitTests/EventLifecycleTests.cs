using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.HybridWebApps;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class EventLifecycleTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task UnsubscribeBetweenOwnershipReservationAndSourceStartCannotReviveTheRetiredGeneration(bool replacement)
	{
		using var provider = new ServiceCollection()
			.AddSingleton(new NativeDispatcher(action => action()))
			.AddSingleton(new NativeNotifications()).BuildServiceProvider();
		var notifications = provider.GetRequiredService<NativeNotifications>();
		var nativeRegistry = new CommandRegistry();
		var source = new Source();
		var published = new List<int>();
		int created = 0;
		notifications.Received += (_, data, _) => published.Add(data.GetInt32());
		HybridEvents.Register<Source, Action>(nativeRegistry, provider, "race.changed", () => source,
			publisher =>
			{
				var generation = ++created;
				return () => publisher.Publish(writer => writer.WriteNumberValue(generation));
			},
			(receiver, handler) => { receiver.Attached++; receiver.Handler += handler; },
			(receiver, handler) => { receiver.Detached++; receiver.Handler -= handler; });

		var registry = new CommandRegistry();
		var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		int reservations = 0;
		registry.RegisterTopicSource("race.changed", () =>
		{
			var start = nativeRegistry.ReserveTopicStart("race.changed");
			if (++reservations != 1)
				return start;
			return async () =>
			{
				entered.SetResult();
				await release.Task;
				await start();
			};
		}, () => nativeRegistry.StopTopic("race.changed"));
		using var hub = new HybridAppTargetObject(registry, new HybridAppOptions());
		var retired = hub.Subscribe("race.changed", default);
		await entered.Task;
		hub.Unsubscribe("race.changed", default);
		Action? current = null;
		if (replacement)
		{
			await hub.Subscribe("race.changed", default);
			current = source.Handler;
			Assert.NotNull(current);
		}

		release.SetResult();
		await retired;
		Assert.Equal(replacement ? 1 : 0, source.Attached);
		Assert.Equal(replacement ? 1 : 0, created);
		source.Handler?.Invoke();
		current?.Invoke();
		Assert.Equal(replacement ? new[] { 1, 1 } : [], published);
		hub.Unsubscribe("race.changed", default);
		hub.Disconnect();
		hub.Dispose();
		Assert.Null(source.Handler);
		Assert.Equal(source.Attached, source.Detached);
		current?.Invoke();
		Assert.Equal(replacement ? new[] { 1, 1 } : [], published);
	}

	[Fact]
	public async Task PendingAttachIsCancelledAndQueuedDetachRetainsTheOriginalReceiverAfterServicesDispose()
	{
		var queue = new Queue<(Action Run, TaskCompletionSource Completion)>();
		Task Dispatch(Func<Task> run)
		{
			var completion = new TaskCompletionSource();
			queue.Enqueue((() => run().GetAwaiter().GetResult(), completion));
			return completion.Task;
		}
		void Drain()
		{
			while (queue.TryDequeue(out var next))
			{
				try
				{ next.Run(); next.Completion.SetResult(); }
				catch (Exception error) { next.Completion.SetException(error); }
			}
		}
		var services = new ServiceCollection();
		services.AddSingleton(new NativeDispatcher(Dispatch));
		services.AddSingleton(new NativeNotifications());
		var provider = services.BuildServiceProvider();
		var registry = new CommandRegistry();
		var original = new Source();
		var current = original;
		HybridEvents.Register<Source, Action>(registry, provider, "race.changed", () => current,
			publisher => () => publisher.Publish(writer => writer.WriteNullValue()),
			(source, handler) => { source.Attached++; source.Handler += handler; },
			(source, handler) => { source.Detached++; source.Handler -= handler; });
		var cancelled = registry.StartTopicAsync("race.changed");
		registry.StopTopic("race.changed");
		Drain();
		await cancelled;
		Assert.Equal(0, original.Attached);
		var started = registry.StartTopicAsync("race.changed");
		Drain();
		await started;
		Assert.Equal(1, original.Attached);
		registry.StopTopic("race.changed");
		current = new Source();
		provider.Dispose();
		Drain();
		Assert.Equal(1, original.Detached);
		Assert.Null(original.Handler);
		Assert.Equal(0, current.Detached);
	}

	[Fact]
	public async Task StopDuringNativeAttachDetachesOnceWithoutPublishingOrHoldingALifecycleLock()
	{
		using var provider = new ServiceCollection()
			.AddSingleton(new NativeDispatcher(action => action()))
			.AddSingleton(new NativeNotifications()).BuildServiceProvider();
		var registry = new CommandRegistry();
		int detached = 0, published = 0;
		provider.GetRequiredService<NativeNotifications>().Received += (_, _, _) => published++;
		HybridEvents.Register<Action>(registry, provider, "race.changed",
			publisher => () => publisher.Publish(writer => writer.WriteNullValue()),
			handler =>
			{
				Task.Run(() => registry.StopTopic("race.changed")).GetAwaiter().GetResult();
				handler();
			}, _ => detached++);
		await registry.StartTopicAsync("race.changed");
		registry.StopTopic("race.changed");
		Assert.Equal(1, detached);
		Assert.Equal(0, published);
	}

	sealed class Source
	{
		internal int Attached, Detached;
		internal Action? Handler;
	}
}
