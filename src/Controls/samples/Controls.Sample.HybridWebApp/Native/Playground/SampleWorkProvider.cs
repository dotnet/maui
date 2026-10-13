using System.Text.Json;
using Maui.HybridApp;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleWorkProvider : IHybridCommandProvider
{
	public void Register(CommandRegistry registry)
	{
		registry.RegisterTopic("operation.progress");
		var json = WorkJsonContext.Default;
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

sealed record WorkArgs(int Steps, int DelayMs);
sealed record ProgressResult(int Completed, int Total);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(WorkArgs))]
[JsonSerializable(typeof(ProgressResult))]
partial class WorkJsonContext : JsonSerializerContext;
