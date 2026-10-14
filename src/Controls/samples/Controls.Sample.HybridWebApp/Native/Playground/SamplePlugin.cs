using Maui.HybridApp;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

[HybridTopic("service.changed")]
[HybridTopic("operation.progress")]
sealed class SamplePlugin
{
	int _counter;

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
		context.Publish("service.changed", new CounterResult(count), SampleEventJsonContext.Default.CounterResult);
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

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(CounterResult))]
[JsonSerializable(typeof(ProgressResult))]
partial class SampleEventJsonContext : JsonSerializerContext;
