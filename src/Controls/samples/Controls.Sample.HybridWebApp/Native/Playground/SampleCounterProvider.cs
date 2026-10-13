using Maui.HybridApp;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleCounterProvider(SampleCounter counter) : IHybridCommandProvider
{
	public void Register(CommandRegistry registry) =>
		registry.Register("sample.counter", CounterJsonContext.Default.EmptyArgs, CounterJsonContext.Default.CounterResult,
			(_, _) => Task.FromResult(new CounterResult(counter.Value)));
}

sealed record CounterResult(int Counter);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(EmptyArgs))]
[JsonSerializable(typeof(CounterResult))]
partial class CounterJsonContext : JsonSerializerContext;
