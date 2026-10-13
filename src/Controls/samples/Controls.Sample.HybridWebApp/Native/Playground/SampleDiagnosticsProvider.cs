using System.Runtime.InteropServices;
using Maui.HybridApp;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleDiagnosticsProvider(SampleStartupIdentity startup) : IHybridCommandProvider
{
	public void Register(CommandRegistry registry) =>
		registry.Register("sample.diagnostics", DiagnosticsJsonContext.Default.EmptyArgs, DiagnosticsJsonContext.Default.DiagnosticsResult,
			(_, _) => Task.FromResult(new DiagnosticsResult(RuntimeInformation.ProcessArchitecture.ToString(),
				Environment.ProcessId, startup.StartupId, startup.StartedAt.ToString("O"))));
}

sealed record SampleStartupIdentity(string StartupId, DateTimeOffset StartedAt);
sealed record DiagnosticsResult(string Architecture, int Pid, string StartupId, string StartedAt);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(EmptyArgs))]
[JsonSerializable(typeof(DiagnosticsResult))]
partial class DiagnosticsJsonContext : JsonSerializerContext;
