using System.Text.Json;
using Maui.HybridApp;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleEchoProvider(SampleCounter counter) : IHybridCommandProvider
{
	public void Register(CommandRegistry registry)
	{
		registry.RegisterTopic("service.changed");
		var json = EchoJsonContext.Default;
		registry.Register("sample.echo", json.EchoArgs, json.EchoResult, async (args, op) =>
		{
			if (!double.IsFinite(args.Number) || args.Items.Length > 16 || args.Items.Any(item => item is null))
				throw new CommandException("invalid_arguments", "Supply a finite number and at most 16 non-null strings.");
			if ((args.Text?.Length ?? 0) > SamplePolicy.MaximumTextLength || args.Items.Any(item => item.Length > SamplePolicy.MaximumTextLength))
				throw new CommandException("invalid_arguments", "Text is limited to 4096 UTF-16 code units.");
			await Task.Delay(80, op.Cancellation).ConfigureAwait(false);
			var count = counter.Increment();
			op.Publish("service.changed", JsonSerializer.SerializeToElement(new CounterResult(count), CounterJsonContext.Default.CounterResult));
			return new EchoResult(args.Text, args.Number, args.Flag, args.Items, count);
		});
	}
}

	sealed record EchoArgs(string? Text, double Number, bool Flag, string[] Items);
	sealed record EchoResult(string? Text, double Number, bool Flag, string[] Items, int Counter);

	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
		RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
	[JsonSerializable(typeof(EchoArgs))]
	[JsonSerializable(typeof(EchoResult))]
	partial class EchoJsonContext : JsonSerializerContext;
