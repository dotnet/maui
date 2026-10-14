using System.Text.Json;
using Maui.HybridApp;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class InteractivePolicyTests
{
	static async Task<Response> Call(HybridAppTargetObject hub, string session, string name)
	{
		var request = new Request(1, session, Guid.NewGuid().ToString("D"), name, JsonSerializer.SerializeToElement(new { }));
		return JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)),
			ProtocolJsonContext.Default.Response)!;
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task OnlyExplicitInteractiveMetadataExtendsDeadlineAndHoldsLease(bool interactive)
	{
		var clock = new ManualTime();
		var plugin = new TestComposition.InteractiveWait();
		var completion = plugin.Completion;
		var registry = TestComposition.CreateRegistry(plugin);
		using var hub = TestComposition.CreateHub(registry, () => new ReadyResult(registry.Names, "", 4096,
			registry.InteractiveCommands), null, clock);
		var session = Guid.NewGuid().ToString("D");
		var ready = (await Call(hub, session, "core.connect")).Result!.Value;
		Assert.Contains("test.interactiveWait", ready.GetProperty("interactiveCommands").EnumerateArray().Select(value => value.GetString()));
		Assert.DoesNotContain("test.wait", ready.GetProperty("interactiveCommands").EnumerateArray().Select(value => value.GetString()));
		var pending = Call(hub, session, interactive ? "test.interactiveWait" : "test.wait");
		clock.Advance(TimeSpan.FromSeconds(24));
		hub.ExpireDocument();
		if (!interactive)
		{
			Assert.Equal("cancelled", (await pending.WaitAsync(TimeSpan.FromSeconds(2))).Status);
			Assert.False(hub.IsCurrent(session));
		}
		else
		{
			Assert.False(pending.IsCompleted);
			Assert.True(hub.IsCurrent(session));
			clock.Advance(TimeSpan.FromSeconds(100));
			completion.SetResult(new Ack(true));
			Assert.Equal("success", (await pending).Status);
			hub.ExpireDocument();
			Assert.True(hub.IsCurrent(session), "OS dismissal grants a fresh bounded heartbeat lease.");
		}
	}

	[Theory]
	[InlineData("disconnect")]
	[InlineData("dispose")]
	[InlineData("budget")]
	public async Task InteractiveCallsCancelImmediatelyWithoutWaitingForOSDismissal(string outcome)
	{
		var clock = new ManualTime();
		var plugin = new TestComposition.InteractiveWait();
		var completion = plugin.Completion;
		var registry = TestComposition.CreateRegistry(plugin);
		using var hub = TestComposition.CreateHub(registry, () => new ReadyResult(registry.Names, "", 4096), null, clock);
		var session = Guid.NewGuid().ToString("D");
		await Call(hub, session, "core.connect");
		var pending = Call(hub, session, "test.interactiveWait");
		if (outcome == "disconnect") hub.Disconnect();
		else if (outcome == "dispose") hub.Dispose();
		else clock.Advance(TimeSpan.FromSeconds(301));
		Assert.Equal("cancelled", (await pending.WaitAsync(TimeSpan.FromSeconds(2))).Status);
		completion.SetResult(new Ack(true));
		Assert.Equal("cancelled", (await pending).Status);
		Assert.Equal(0, clock.ActiveDeadlines);
	}


	internal sealed class ManualTime : TimeProvider
	{
		readonly List<ManualTimer> _timers = [];
		DateTimeOffset _now = DateTimeOffset.UtcNow;
		public int ActiveDeadlines => _timers.Count(timer => !timer.Disposed && timer.Period == Timeout.InfiniteTimeSpan);
		public override DateTimeOffset GetUtcNow() => _now;
		public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
		{
			var timer = new ManualTimer(callback, state, _now + dueTime, period);
			_timers.Add(timer);
			return timer;
		}
		public void Advance(TimeSpan elapsed)
		{
			_now += elapsed;
			foreach (var timer in _timers.ToArray().Where(timer => !timer.Disposed && timer.Due <= _now && timer.Period == Timeout.InfiniteTimeSpan))
				timer.Fire();
		}
		sealed class ManualTimer(TimerCallback callback, object? state, DateTimeOffset due, TimeSpan period) : ITimer
		{
			public DateTimeOffset Due { get; } = due;
			public TimeSpan Period { get; } = period;
			public bool Disposed { get; private set; }
			public bool Change(TimeSpan dueTime, TimeSpan period) => !Disposed;
			public void Fire() => callback(state);
			public void Dispose() => Disposed = true;
			public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
		}
	}
}
