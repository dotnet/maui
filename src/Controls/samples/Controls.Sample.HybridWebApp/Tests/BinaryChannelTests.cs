using System.Text.Json;
using System.Threading.Channels;
using Maui.HybridApp;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class BinaryChannelTests
{
	sealed class Host : IDisposable
	{
		internal readonly string Session = Guid.NewGuid().ToString("D");
		internal readonly Channel<NativeEvent> Events = Channel.CreateUnbounded<NativeEvent>();
		internal readonly HybridAppTargetObject Hub;
		internal Host(params object[] plugins)
		{
			var registry = TestComposition.CreateRegistry(plugins);
			Hub = TestComposition.CreateHub(registry, () => new ReadyResult(registry.Names, 4096),
				message => Events.Writer.TryWrite(message));
		}
		internal string Wire(string command, string args = "{}", string? id = null, string? session = null)
		{
			using var document = JsonDocument.Parse(args);
			return JsonSerializer.Serialize(new Request(1, session ?? Session, id ?? Guid.NewGuid().ToString("D"),
				command, document.RootElement), ProtocolJsonContext.Default.Request);
		}
		internal async Task<Response> Json(string command, string args = "{}", string? id = null) =>
			JsonSerializer.Deserialize(await Hub.Invoke(Wire(command, args, id)), ProtocolJsonContext.Default.Response)!;
		internal async Task Connect() => Assert.Equal("success", (await Json("core.connect")).Status);
		internal Task<byte[]> Binary(string command, byte[]? bytes = null, string args = "{}", string? id = null) =>
			Hub.InvokeBinary(Wire(command, args, id), bytes ?? []);
		internal async Task<NativeEvent> Event() => await Events.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(3));
		public void Dispose() => Hub.Dispose();
	}

	static string GuidId() => Guid.NewGuid().ToString("D");

	[Theory]
	[InlineData('a')]
	[InlineData('\u4f60')]
	[InlineData('"')]
	[InlineData('\\')]
	public void EncodedNativeErrorsFitCoreBudgetAndPreserveTheirCode(char character)
	{
		var json = HybridAppBinaryInvoker.SerializeError("permission_denied", new string(character, 512));
		Assert.True(json.Length <= 512);
		var error = JsonSerializer.Deserialize(json, ProtocolJsonContext.Default.CommandError)!;
		Assert.Equal("permission_denied", error.Code);
		Assert.False(string.IsNullOrWhiteSpace(error.Message));
	}

	static string FetchArgs(NativeEvent message, string? owner = null, string? dataId = null) =>
		$$"""{"channelId":"{{message.Data.GetProperty("channelId").GetString()}}","ownerId":"{{owner ?? message.Id}}","dataId":"{{dataId ?? message.Data.GetProperty("dataId").GetString()}}","sequence":{{message.Data.GetProperty("sequence").GetInt32()}}}""";
	static string AckArgs(NativeEvent message, bool accepted = true, int? sequence = null, string? owner = null) =>
		$$"""{"channelId":"{{message.Data.GetProperty("channelId").GetString()}}","ownerId":"{{owner ?? message.Id}}","sequence":{{sequence ?? message.Data.GetProperty("sequence").GetInt32()}},"accepted":{{(accepted ? "true" : "false")}}}""";

	[Theory]
	[InlineData(0)]
	[InlineData(256)]
	[InlineData(262144)]
	[InlineData(4194304)]
	public async Task WholeBufferEchoUsesExactSeparateBytes(int length)
	{
		using var host = new Host();
		await host.Connect();
		var bytes = Enumerable.Range(0, length).Select(i => (byte)(i % 256)).ToArray();
		Assert.Equal(bytes, await host.Binary("sample.binaryEcho", bytes));
		Assert.True(host.Wire("sample.binaryEcho").Length < 100000);
		Assert.Equal("invalid_transport", (await host.Json("sample.binaryEcho")).Error!.Code);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Binary("sample.binaryEcho", bytes, """{"payload":[1,2]}"""))).Code);
	}

	[Fact]
	public async Task BinarySharesAuthorizationReplayRegistrationAndMetadataBudgets()
	{
		using var host = new Host();
		Assert.Equal("stale_session", (await Assert.ThrowsAsync<CommandException>(() => host.Binary("sample.binaryEcho"))).Code);
		await host.Connect();
		var id = GuidId();
		await host.Binary("sample.binaryEcho", [0, 255], id: id);
		Assert.Equal("duplicate_request", (await host.Json("sample.counter", id: id)).Error!.Code);
		Assert.Equal("duplicate_request", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Binary("sample.binaryEcho", id: id))).Code);
		Assert.Equal("unknown_command", (await Assert.ThrowsAsync<CommandException>(() => host.Binary("missing.bytes"))).Code);
		Assert.Equal("disabled_command", (await Assert.ThrowsAsync<CommandException>(() => host.Binary("binary.disabled"))).Code);
		Assert.Equal("invalid_transport", (await Assert.ThrowsAsync<CommandException>(() => host.Binary("sample.counter"))).Code);
		Assert.Equal("binary_limit", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Binary("sample.binaryEcho", new byte[4194305]))).Code);
		Assert.Equal("invalid_request", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Binary("sample.binaryEcho", args: $$"""{"unused":"{{new string('a', 2880)}}"}"""))).Code);
		await host.Hub.Invoke(host.Wire("core.connect", session: GuidId()));
		Assert.Equal("stale_session", (await Assert.ThrowsAsync<CommandException>(() => host.Binary("sample.binaryEcho"))).Code);
	}

	[Fact]
	public async Task NativePdfIsARealByteDocumentWithValidOffsets()
	{
		using var host = new Host();
		await host.Connect();
		var bytes = await host.Binary("sample.binaryDocument");
		var pdf = System.Text.Encoding.ASCII.GetString(bytes);
		Assert.StartsWith("%PDF-1.4", pdf, StringComparison.Ordinal);
		Assert.EndsWith("%%EOF\n", pdf, StringComparison.Ordinal);
		var offset = int.Parse(pdf.Split("startxref\n")[1].Split('\n')[0]);
		Assert.StartsWith("xref\n", pdf[offset..], StringComparison.Ordinal);
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Binary("sample.binaryDocument", [1]))).Code);
	}

	[Fact]
	public async Task OrderedJsonAndRawChunksWaitForConsumptionAndRejectWrongOwnerReplayAndEarlyAck()
	{
		using var host = new Host();
		await host.Connect();
		var raw = GuidId(); var progress = GuidId(); var owner = GuidId();
		var work = host.Json("sample.binaryChunks", $$"""{"chunks":"{{raw}}","progress":"{{progress}}","count":2,"chunkBytes":256}""", owner);
		for (var i = 1; i <= 2; i++)
		{
			var chunk = await host.Event();
			Assert.Equal(owner, chunk.Id);
			Assert.Equal("bytes", chunk.Data.GetProperty("kind").GetString());
			Assert.Equal(i, chunk.Data.GetProperty("sequence").GetInt32());
			Assert.False(work.IsCompleted);
			Assert.Equal("invalid_channel", (await host.Json("core.channel.ack", AckArgs(chunk))).Error!.Code);
			Assert.Equal("invalid_channel", (await Assert.ThrowsAsync<CommandException>(() =>
				host.Binary("core.channel.fetch", args: FetchArgs(chunk, owner: GuidId())))).Code);
			Assert.Equal("invalid_channel", (await Assert.ThrowsAsync<CommandException>(() =>
				host.Binary("core.channel.fetch", args: FetchArgs(chunk, dataId: GuidId())))).Code);
			Assert.Equal(Enumerable.Range(0, 256).Select(n => (byte)n), await host.Binary("core.channel.fetch", args: FetchArgs(chunk)));
			Assert.Equal("invalid_channel", (await Assert.ThrowsAsync<CommandException>(() =>
				host.Binary("core.channel.fetch", args: FetchArgs(chunk)))).Code);
			await Task.Delay(10);
			Assert.False(host.Events.Reader.TryRead(out _), "No unbounded push while the consumer is processing.");
			Assert.Equal("invalid_channel", (await host.Json("core.channel.ack", AckArgs(chunk, sequence: i + 1))).Error!.Code);
			Assert.Equal("invalid_channel", (await host.Json("core.channel.ack", AckArgs(chunk, owner: GuidId()))).Error!.Code);
			Assert.Equal("success", (await host.Json("core.channel.ack", AckArgs(chunk))).Status);
			Assert.Equal("invalid_channel", (await host.Json("core.channel.ack", AckArgs(chunk))).Error!.Code);
			var json = await host.Event();
			Assert.Equal("json", json.Data.GetProperty("kind").GetString());
			Assert.Equal(i, json.Data.GetProperty("value").GetProperty("completed").GetInt32());
			Assert.Equal("success", (await host.Json("core.channel.ack", AckArgs(json))).Status);
		}
		Assert.Equal("success", (await work.WaitAsync(TimeSpan.FromSeconds(3))).Status);
		Assert.Equal("complete", (await host.Event()).Data.GetProperty("kind").GetString());
		Assert.Equal("complete", (await host.Event()).Data.GetProperty("kind").GetString());
	}

	[Theory]
	[InlineData("consumer")]
	[InlineData("cancel")]
	[InlineData("close")]
	[InlineData("reload")]
	public async Task PendingBytesAreDiscardedOnConsumerFailureCancelUnsubscribeAndReload(string kind)
	{
		using var host = new Host();
		await host.Connect();
		var owner = GuidId(); var channel = GuidId();
		var work = host.Json("binary.channel", $$"""{"channel":"{{channel}}"}""", owner);
		var message = await host.Event();
		switch (kind)
		{
			case "consumer": await host.Json("core.channel.ack", AckArgs(message, false)); break;
			case "cancel": await host.Json("core.cancel", $$"""{"requestId":"{{owner}}"}"""); break;
			case "close": await host.Json("core.channel.close", $$"""{"channelId":"{{channel}}","ownerId":"{{owner}}"}"""); break;
			default: await host.Hub.Invoke(host.Wire("core.connect", session: GuidId())); break;
		}
		var result = await work.WaitAsync(TimeSpan.FromSeconds(3));
		Assert.NotEqual("success", result.Status);
		if (kind == "consumer") Assert.Equal("consumer_error", result.Error!.Code);
		var exception = await Assert.ThrowsAsync<CommandException>(() => host.Binary("core.channel.fetch", args: FetchArgs(message)));
		Assert.Equal(kind == "reload" ? "stale_session" : "invalid_channel", exception.Code);
	}

	[Theory]
	[InlineData("binary.largeChannel", "channel_limit")]
	[InlineData("binary.largeJson", "channel_limit")]
	[InlineData("binary.largeEscapedJson", "channel_limit")]
	[InlineData("binary.concurrent", "channel_busy")]
	[InlineData("binary.tooMany", "channel_limit")]
	public async Task ChannelsRejectUnboundedMessagesConcurrentSendsAndTooManyHandles(string command, string code)
	{
		using var host = new Host();
		await host.Connect();
		var args = $$"""{"channel":"{{GuidId()}}","a":"{{GuidId()}}","b":"{{GuidId()}}","c":"{{GuidId()}}","d":"{{GuidId()}}"}""";
		if (command != "binary.tooMany") args = $$"""{"channel":"{{GuidId()}}"}""";
		var result = await host.Json(command, args);
		Assert.Equal(code, result.Error!.Code);
	}

	[Fact]
	public async Task ConsumerRejectionClosesChannelEvenWhenTheActionHandlesTheError()
	{
		using var host = new Host();
		await host.Connect();
		var work = host.Json("binary.channel", $$"""{"channel":"{{GuidId()}}","retry":true}""");
		var message = await host.Event();
		Assert.Equal("success", (await host.Json("core.channel.ack", AckArgs(message, false))).Status);
		Assert.Equal("success", (await work.WaitAsync(TimeSpan.FromSeconds(3))).Status);
		Assert.Equal("error", (await host.Event()).Data.GetProperty("kind").GetString());
		Assert.Equal("invalid_channel", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Binary("core.channel.fetch", args: FetchArgs(message)))).Code);
	}

	[Fact]
	public async Task SixteenChannelOwnersDoNotStarveFetchAckOrCancellation()
	{
		using var host = new Host();
		await host.Connect();
		var owners = Enumerable.Range(0, 16).Select(_ => GuidId()).ToArray();
		var work = owners.Select(owner => host.Json("binary.channel",
			$$"""{"channel":"{{GuidId()}}"}""", owner)).ToArray();
		Assert.Equal("busy", (await host.Json("sample.counter")).Error!.Code);
		for (var i = 0; i < work.Length; i++)
		{
			var message = await host.Event();
			Assert.Equal("bytes", message.Data.GetProperty("kind").GetString());
			if (i == work.Length - 1)
				Assert.Equal("success", (await host.Json("core.cancel", $$"""{"requestId":"{{owners[i]}}"}""")).Status);
			else
			{
				Assert.Equal(new byte[] { 0, 128, 255 }, await host.Binary("core.channel.fetch", args: FetchArgs(message)));
				Assert.Equal("success", (await host.Json("core.channel.ack", AckArgs(message))).Status);
			}
		}
		var results = await Task.WhenAll(work).WaitAsync(TimeSpan.FromSeconds(3));
		Assert.All(results.Take(15), result => Assert.Equal("success", result.Status));
		Assert.Equal("cancelled", results[^1].Status);
	}

	[Fact]
	public async Task BinaryOperationsHoldSharedSlotsAcrossCancellationAndReloadUntilRealHandlerReturns()
	{
		var plugin = new BinaryTestPlugin();
		using var host = new Host(plugin);
		await host.Connect();
		var operations = Enumerable.Range(0, 3).Select(_ => host.Binary("binary.wait")).ToArray();
		Assert.Equal("busy", (await Assert.ThrowsAsync<CommandException>(() => host.Binary("binary.wait"))).Code);
		var owners = Enumerable.Range(0, 13).Select(_ => GuidId()).ToArray();
		var jsonOperations = owners.Select(owner => host.Json("binary.channel",
			$$"""{"channel":"{{GuidId()}}"}""", owner)).ToArray();
		Assert.Equal("busy", (await host.Json("sample.counter")).Error!.Code);
		var message = await host.Event();
		Assert.Equal(new byte[] { 0, 128, 255 }, await host.Binary("core.channel.fetch", args: FetchArgs(message)));
		Assert.Equal("success", (await host.Json("core.channel.ack", AckArgs(message))).Status);
		// Refill the completed owner's regular slot before retiring the document.
		Assert.Equal("success", (await jsonOperations[0]).Status);
		jsonOperations[0] = host.Json("binary.channel", $$"""{"channel":"{{GuidId()}}"}""");
		host.Hub.Disconnect();
		Assert.All(await Task.WhenAll(jsonOperations), result => Assert.Equal("cancelled", result.Status));
		var replacement = GuidId();
		Assert.Equal("success", JsonSerializer.Deserialize(await host.Hub.Invoke(host.Wire("core.connect", session: replacement)),
			ProtocolJsonContext.Default.Response)!.Status);
		Assert.Equal("busy", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Hub.InvokeBinary(host.Wire("binary.wait", session: replacement), []))).Code);
		plugin.Completion.SetResult([128]);
		foreach (var operation in operations) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
	}
}

sealed class BinaryTestPlugin
{
	internal TaskCompletionSource<byte[]> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
	[HybridAction("binary.wait")]
	internal Task<byte[]> Wait() => Completion.Task;
	[HybridAction("binary.disabled", Enabled = false)]
	internal static byte[] Disabled() => [];
	[HybridAction("binary.channel")]
	internal static async Task<Ack> Stream(HybridChannel<byte[]> channel, bool retry = false)
	{
		try { await channel.SendAsync([0, 128, 255]); }
		catch (CommandException exception) when (retry && exception.Code == "consumer_error")
		{
			Assert.Equal("channel_closed", (await Assert.ThrowsAsync<CommandException>(() => channel.SendAsync([1]))).Code);
		}
		return new(true);
	}
	[HybridAction("binary.largeChannel")]
	internal static async Task<Ack> Large(HybridChannel<byte[]> channel) { await channel.SendAsync(new byte[65537]); return new(true); }
	[HybridAction("binary.largeJson")]
	internal static async Task<Ack> LargeJson(HybridChannel<string> channel) { await channel.SendAsync(new string('a', 16385)); return new(true); }
	[HybridAction("binary.largeEscapedJson")]
	internal static async Task<Ack> LargeEscapedJson(HybridChannel<string> channel) { await channel.SendAsync(new string('\u4f60', 3000)); return new(true); }
	[HybridAction("binary.concurrent")]
	internal static async Task<Ack> Concurrent(HybridChannel<byte[]> channel)
	{
		var pending = channel.SendAsync([1]);
		_ = pending.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
		await channel.SendAsync([2]);
		return new(true);
	}
	[HybridAction("binary.tooMany")]
	internal static Ack TooMany(HybridChannel<int> channel, HybridChannel<int> a, HybridChannel<int> b, HybridChannel<int> c, HybridChannel<int> d) => new(true);
}
