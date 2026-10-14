using System.Text.Json.Serialization;
using Maui.HybridApp.UI;
using Microsoft.Maui.ApplicationModel;

namespace Maui.HybridApp.Essentials.Launcher;

/// <summary>Public HTTPS launch through the OS, never a guest, in-app browser, file or shell command.</summary>
public sealed class LauncherProvider(ILauncher launcher, NativeDispatcher dispatcher) : IHybridCommandProvider
{
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = LauncherJsonContext.Default;
		registry.Register("launcher.openUrl", json.LaunchUrlArgs, json.LaunchUrlResult, (args, op) =>
		{
			var uri = RemoteWebAddress.Validate(args.Url);
			return dispatcher.RunAsync(async () =>
			{
				if (!await launcher.OpenAsync(uri))
					throw new CommandException("launch_failed", "The native system did not accept the HTTPS URL.");
				return new LaunchUrlResult(true);
			}, op.Cancellation);
		}, interactive: true);
	}
}

sealed record LaunchUrlArgs(string Url);
sealed record LaunchUrlResult(bool Accepted);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(LaunchUrlArgs))]
[JsonSerializable(typeof(LaunchUrlResult))]
partial class LauncherJsonContext : JsonSerializerContext;
