using System.Text.Json.Serialization;
using Microsoft.Maui.Controls.HybridWebApps;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

[HybridTopic("operation.progress")]
sealed class SamplePlugin
{
	int _counter;
	[HybridEvent("service.changed")]
	internal event Action<CounterResult>? CounterChanged;

	[HybridAction("sample.binaryEcho")]
	internal static byte[] BinaryEcho(byte[] payload) => payload;

	[HybridAction("sample.binaryDocument")]
	internal static byte[] BinaryDocument()
	{
		// Controlled PDF fixture: byte offsets are computed from ASCII, not a third-party PDF library.
		const string content = "BT /F1 16 Tf 20 80 Td (Native binary document) Tj ET\n";
		var objects = new[] {
			"<< /Type /Catalog /Pages 2 0 R >>",
			"<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
			"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 300 144] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
			"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
			$"<< /Length {content.Length} >>\nstream\n{content}endstream"
		};
		var pdf = new System.Text.StringBuilder("%PDF-1.4\n");
		var offsets = new List<int>();
		for (var i = 0; i < objects.Length; i++)
		{ offsets.Add(pdf.Length); pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
		var xref = pdf.Length;
		pdf.Append($"xref\n0 6\n0000000000 65535 f \n");
		foreach (var offset in offsets)
			pdf.Append($"{offset:D10} 00000 n \n");
		pdf.Append($"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
		return System.Text.Encoding.ASCII.GetBytes(pdf.ToString());
	}

	[HybridAction("sample.binaryChunks")]
	internal static async Task<Ack> BinaryChunks(HybridChannel<byte[]> chunks, HybridChannel<ChunkProgress> progress,
		CancellationToken cancellation, int count = 4, int chunkBytes = 65536)
	{
		if (count is < 1 or > 16 || chunkBytes is < 1 or > 65536)
			throw new CommandException("invalid_arguments", "Use 1–16 chunks of 1–65536 bytes.");
		for (var i = 0; i < count; i++)
		{
			cancellation.ThrowIfCancellationRequested();
			var bytes = new byte[chunkBytes];
			for (var j = 0; j < bytes.Length; j++)
				bytes[j] = (byte)((i * chunkBytes + j) % 256);
			await chunks.SendAsync(bytes).ConfigureAwait(false);
			await progress.SendAsync(new ChunkProgress(i + 1, count)).ConfigureAwait(false);
		}
		return new(true);
	}

	[HybridAction("sample.counter")]
	internal CounterResult Counter() => new(Volatile.Read(ref _counter));

	[HybridAction("sample.echo")]
	internal async Task<EchoResult> Echo(string? text, double number, bool flag, string[] items, HybridActionContext context)
	{
		if (!double.IsFinite(number) || items.Length > 16)
			throw new CommandException("invalid_arguments", "Supply a finite number and at most 16 non-null strings.");
		if ((text?.Length ?? 0) > SamplePolicy.MaximumTextLength || items.Any(item => item.Length > SamplePolicy.MaximumTextLength))
			throw new CommandException("invalid_arguments", "Text is limited to 4096 UTF-16 code units.");
		await Task.Delay(80, context.Cancellation).ConfigureAwait(false);
		var count = Interlocked.Increment(ref _counter);
		CounterChanged?.Invoke(new CounterResult(count));
		return new(text, number, flag, items, count);
	}

	[HybridAction("sample.fail")]
	internal Ack Fail() => throw new CommandException("intentional_error", "This is the deliberate native failure. The bridge is still usable.");

	[HybridAction("sample.disabled", Enabled = false)]
	internal Ack Disabled() => new(true);

	[HybridAction("sample.work")]
	internal async Task<ProgressResult> Work(int steps, int delayMs, HybridActionContext context)
	{
		if (steps is < 1 or > 50 || delayMs is < 100 or > 500 || steps * delayMs > 15000)
			throw new CommandException("invalid_arguments", "Use 1–50 steps at 100–500 ms, with at most 15 seconds of work.");
		for (var step = 1; step <= steps; step++)
		{
			await Task.Delay(delayMs, context.Cancellation).ConfigureAwait(false);
			context.Publish("operation.progress", new ProgressResult(step, steps), SampleEventJsonContext.Default.ProgressResult);
		}
		return new(steps, steps);
	}
}

sealed record CounterResult(int Counter);
sealed record EchoResult(string? Text, double Number, bool Flag, string[] Items, int Counter);
sealed record ProgressResult(int Completed, int Total);
sealed record ChunkProgress(int Completed, int Total);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ProgressResult))]
partial class SampleEventJsonContext : JsonSerializerContext;
