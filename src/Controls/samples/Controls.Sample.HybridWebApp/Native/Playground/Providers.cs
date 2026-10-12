using System.Text.Json;
using System.Text.RegularExpressions;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

static partial class EssentialsProvider
{
	public const string PreferenceNamespace = "maui.playground.v1.";
	public const int MaximumTextLength = 4096;

	public static void Register(CommandRegistry registry, IPlaygroundPlatform platform)
	{
		var json = PlaygroundJsonContext.Default;
		registry.Register("app.info", json.EmptyArgs, json.IdentityResult, (_, op) => platform.GetIdentityAsync(op.Cancellation));
		registry.Register("clipboard.read", json.EmptyArgs, json.TextResult, (_, op) => platform.GetClipboardAsync(op.Cancellation));
		registry.Register("clipboard.write", json.TextArgs, json.Ack, (args, op) =>
		{
			ValidateText(args.Text);
			return platform.SetClipboardAsync(args.Text, op.Cancellation);
		});
		registry.Register("preferences.read", json.PreferenceArgs, json.PreferenceResult, (args, op) =>
		{
			ValidateKey(args.Key);
			return platform.GetPreferenceAsync(args.Key, op.Cancellation);
		});
		registry.Register("preferences.write", json.SetPreferenceArgs, json.PreferenceResult, (args, op) =>
		{
			ValidateKey(args.Key);
			ValidateText(args.Value);
			return platform.SetPreferenceAsync(args.Key, args.Value, op.Cancellation);
		});
		registry.Register("preferences.remove", json.PreferenceArgs, json.PreferenceResult, (args, op) =>
		{
			ValidateKey(args.Key);
			return platform.SetPreferenceAsync(args.Key, null, op.Cancellation);
		});
	}

	public static void ValidateText(string text)
	{
		if (text.Length > MaximumTextLength)
			throw new CommandException("invalid_arguments", "Text is limited to 4096 UTF-16 code units.");
	}

	static void ValidateKey(string key)
	{
		if (!PreferenceKey().IsMatch(key))
			throw new CommandException("invalid_arguments", "Keys must be 1–48 ASCII letters, digits, underscores or hyphens. Only the playground namespace is accessible.");
	}

	[GeneratedRegex("^[a-zA-Z0-9_-]{1,48}$", RegexOptions.CultureInvariant)]
	private static partial Regex PreferenceKey();
}

sealed class SampleCommands
{
	int _counter;

	public void Register(CommandRegistry registry)
	{
		var json = PlaygroundJsonContext.Default;
		registry.Register("sample.counter", json.EmptyArgs, json.CounterResult, (_, _) =>
			Task.FromResult(new CounterResult(Volatile.Read(ref _counter))));
		registry.Register("sample.echo", json.EchoArgs, json.EchoResult, async (args, op) =>
		{
			if (!double.IsFinite(args.Number) || args.Items.Length > 16 || args.Items.Any(item => item is null))
				throw new CommandException("invalid_arguments", "Supply a finite number and at most 16 non-null strings.");
			if (args.Text is not null)
				EssentialsProvider.ValidateText(args.Text);
			foreach (var item in args.Items)
				EssentialsProvider.ValidateText(item);
			await Task.Delay(80, op.Cancellation).ConfigureAwait(false);
			var count = Interlocked.Increment(ref _counter);
			op.Publish("service.changed", JsonSerializer.SerializeToElement(new CounterResult(count), json.CounterResult));
			return new EchoResult(args.Text, args.Number, args.Flag, args.Items, count);
		});
		registry.Register<EmptyArgs, Ack>("sample.fail", json.EmptyArgs, json.Ack, (_, _) =>
			throw new CommandException("intentional_error", "This is the deliberate native failure. The bridge is still usable."));
		registry.Register("sample.disabled", json.EmptyArgs, json.Ack, (_, _) => Task.FromResult(new Ack(true)), enabled: false);
		registry.Register("sample.work", json.WorkArgs, json.ProgressResult, async (args, op) =>
		{
			if (args.Steps is < 1 or > 50 || args.DelayMs is < 100 or > 500 || args.Steps * args.DelayMs > 15000)
				throw new CommandException("invalid_arguments", "Use 1–50 steps at 100–500 ms, with at most 15 seconds of work.");
			for (var step = 1; step <= args.Steps; step++)
			{
				await Task.Delay(args.DelayMs, op.Cancellation).ConfigureAwait(false);
				op.Publish("operation.progress", JsonSerializer.SerializeToElement(new ProgressResult(step, args.Steps), json.ProgressResult));
			}
			return new ProgressResult(args.Steps, args.Steps);
		});
	}
}
