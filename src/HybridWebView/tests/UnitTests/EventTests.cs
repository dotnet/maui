using System.Text.Json;
using HybridApp.ExternalProvider;
using Microsoft.Maui.Controls.HybridWebApps;
using Xunit;

[assembly: Microsoft.Maui.Interop.HybridExport(typeof(IPluginEventContract))]

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class EventTests
{
	[Fact]
	public async Task DirectEventsPreserveTypedScalarNamedAndEmptyPayloads()
	{
		var source = new EventSource();
		var external = new PluginEventSource();
		using var notifications = new NativeNotifications();
		using var hub = TestComposition.CreateHub([source, external, notifications], new(), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		var received = new List<NativeEvent>();
		hub.NativeEventReceived += received.Add;
		await Call(hub, session, "core.connect");
		foreach (var topic in new[] { "events.changed", "events.empty", "events.scalar", "events.pair", "events.senderPair", "events.static", "plugin.native.changed" })
			Assert.Equal("success", (await Call(hub, session, "core.subscribe", topic)).Status);
		source.Raise();
		external.Raise(9);
		Assert.Equal("""{"count":7,"text":"native"}""", received.Single(e => e.Topic == "events.changed").Data.GetRawText());
		Assert.Equal(JsonValueKind.Null, received.Single(e => e.Topic == "events.empty").Data.ValueKind);
		Assert.Equal(42, received.Single(e => e.Topic == "events.scalar").Data.GetInt32());
		Assert.Equal("""{"count":3,"label":"pair"}""", received.Single(e => e.Topic == "events.pair").Data.GetRawText());
		Assert.Equal("""{"count":4,"label":"sender pair"}""", received.Single(e => e.Topic == "events.senderPair").Data.GetRawText());
		Assert.Equal("static", received.Single(e => e.Topic == "events.static").Data.GetString());
		Assert.Equal(9, received.Single(e => e.Topic == "plugin.native.changed").Data.GetProperty("value").GetInt32());
	}

	[Fact]
	public async Task FirstLastDisconnectAndReattachOwnExactlyOneDelegateAndRejectStaleCallbacks()
	{
		var source = new EventSource();
		using var notifications = new NativeNotifications();
		using var hub = TestComposition.CreateHub([source, notifications], new(), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		var received = new List<NativeEvent>();
		hub.NativeEventReceived += received.Add;
		await Call(hub, session, "core.connect");
		await Call(hub, session, "core.subscribe", "events.changed");
		await Call(hub, session, "core.subscribe", "events.changed");
		Assert.Equal(1, source.Attached);
		var stale = source.Handler!;
		await Call(hub, session, "core.unsubscribe", "events.changed");
		await Call(hub, session, "core.unsubscribe", "events.changed");
		Assert.Equal(1, source.Detached);
		stale(source, new(1, "retired"));
		Assert.Empty(received);
		await Call(hub, session, "core.subscribe", "events.changed");
		stale(source, new(1, "still retired"));
		Assert.Empty(received);
		source.Raise();
		Assert.Single(received);
		hub.Disconnect();
		Assert.Equal(2, source.Attached);
		Assert.Equal(2, source.Detached);
		hub.Dispose();
		Assert.Equal(2, source.Detached);
	}

	[Fact]
	public async Task LocalDocumentsDoNotShareHandlersOrDelivery()
	{
		var source = new EventSource();
		using var leftNotifications = new NativeNotifications();
		using var rightNotifications = new NativeNotifications();
		using var left = TestComposition.CreateHub([source, leftNotifications], new(), notifications: leftNotifications);
		using var right = TestComposition.CreateHub([source, rightNotifications], new(), notifications: rightNotifications);
		var leftSession = Guid.NewGuid().ToString("D");
		var rightSession = Guid.NewGuid().ToString("D");
		int leftCount = 0, rightCount = 0;
		left.NativeEventReceived += _ => leftCount++;
		right.NativeEventReceived += _ => rightCount++;
		await Call(left, leftSession, "core.connect");
		await Call(right, rightSession, "core.connect");
		await Call(left, leftSession, "core.subscribe", "events.changed");
		await Call(right, rightSession, "core.subscribe", "events.changed");
		source.Raise();
		Assert.Equal((1, 1, 2), (leftCount, rightCount, source.Attached));
		left.Disconnect();
		source.Raise();
		Assert.Equal((1, 2, 1), (leftCount, rightCount, source.Detached));
	}

	[Fact]
	public async Task FailedAttachRollsBackAndSerializationFailuresDoNotEscapeTheNativeEvent()
	{
		var source = new EventSource { FailAttach = true };
		using var notifications = new NativeNotifications();
		using var hub = TestComposition.CreateHub([source, notifications], new(), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		await Call(hub, session, "core.connect");
		Assert.Equal("error", (await Call(hub, session, "core.subscribe", "events.changed")).Status);
		Assert.Null(source.Handler);
		Assert.Equal(1, source.Detached);
		source.FailAttach = false;
		Assert.Equal("success", (await Call(hub, session, "core.subscribe", "events.changed")).Status);
		var received = new List<NativeEvent>();
		hub.NativeEventReceived += received.Add;
		source.Handler!(source, new(1, null!));
		Assert.Empty(received);
		source.Raise();
		Assert.Single(received);
	}

	[Fact]
	public async Task ConcurrentSubscribersAwaitTheSameAttachAndBothObserveItsFailure()
	{
		var source = new EventSource { FailAttach = true };
		using var notifications = new NativeNotifications();
		Func<Task>? queued = null;
		var attached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var dispatcher = new NativeDispatcher(action => { queued = action; return attached.Task; });
		using var hub = TestComposition.CreateHub([source, notifications, dispatcher], new(), notifications: notifications);
		var session = Guid.NewGuid().ToString("D");
		await Call(hub, session, "core.connect");
		var first = Call(hub, session, "core.subscribe", "events.changed");
		var second = Call(hub, session, "core.subscribe", "events.changed");
		Assert.False(first.IsCompleted);
		Assert.False(second.IsCompleted);
		try
		{ await queued!(); attached.SetResult(); }
		catch (Exception error) { attached.SetException(error); }
		Assert.Equal("error", (await first).Status);
		Assert.Equal("error", (await second).Status);
		Assert.Equal(1, source.Attached);
		Assert.Equal(1, source.Detached);
	}

	internal static async Task<Response> Call(HybridAppTargetObject hub, string session, string command, string? topic = null)
	{
		using var args = JsonDocument.Parse(topic is null ? "{}" : $$"""{"topic":"{{topic}}"}""");
		var request = new Request(1, session, Guid.NewGuid().ToString("D"), command, args.RootElement);
		return JsonSerializer.Deserialize(await hub.Invoke(JsonSerializer.Serialize(request, ProtocolJsonContext.Default.Request)), ProtocolJsonContext.Default.Response)!;
	}
}

sealed record EventPayload(int Count, string Text);
delegate void PairChanged(int count, string label);
delegate void SenderPairChanged(object sender, int count, string label);

sealed class EventSource
{
	internal int Attached, Detached;
	internal bool FailAttach;
	internal EventHandler<EventPayload>? Handler;
	[HybridEvent("events.changed")]
	internal event EventHandler<EventPayload> Changed
	{
		add { Attached++; Handler += value; if (FailAttach) throw new InvalidOperationException("attach"); }
		remove { Detached++; Handler -= value; }
	}
	[HybridEvent("events.empty")]
	internal event EventHandler? Empty;
	[HybridEvent("events.scalar")]
	internal event Action<int>? Scalar;
	[HybridEvent("events.pair")]
	internal event PairChanged? Pair;
	[HybridEvent("events.senderPair")]
	internal event SenderPairChanged? SenderPair;
	[HybridEvent("events.static")]
	internal static event Action<string>? Static;
	internal void Raise()
	{
		Handler?.Invoke(this, new(7, "native"));
		Empty?.Invoke(this, EventArgs.Empty);
		Scalar?.Invoke(42);
		Pair?.Invoke(3, "pair");
		SenderPair?.Invoke(this, 4, "sender pair");
		Static?.Invoke("static");
	}
}
