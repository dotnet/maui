using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

interface IHybridAppTarget
{
	Task<string> Invoke(string request);
}

sealed record Request(int Version, string Session, string Id, string Command, JsonElement Args);
sealed record Response(int Version, string Session, string Id, string Status, JsonElement? Result, CommandError? Error);
sealed record CommandError(string Code, string Message);
sealed record NativeEvent(int Version, string Session, string Id, string Topic, JsonElement Data);
sealed record DocumentMessage(string Session, string Kind);
sealed record EmptyArgs;
sealed record ReadyResult(string[] Commands, string PreferenceNamespace, int MaximumTextLength);
sealed record TextArgs(string Text);
sealed record TextResult(string? Text);
sealed record PreferenceArgs(string Key);
sealed record SetPreferenceArgs(string Key, string Value);
sealed record PreferenceResult(string Key, string? Value);
sealed record Ack(bool Accepted);
sealed record TopicArgs(string Topic);
sealed record CancelArgs(string RequestId);
sealed record EchoArgs(string? Text, double Number, bool Flag, string[] Items);
sealed record EchoResult(string? Text, double Number, bool Flag, string[] Items, int Counter);
sealed record CounterResult(int Counter);
sealed record WorkArgs(int Steps, int DelayMs);
sealed record ProgressResult(int Completed, int Total);
sealed record IdentityResult(string AppName, string Version, string Build, string Platform, string OsVersion,
	string Model, string Architecture, int Pid, string StartupId, string StartedAt);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(NativeEvent))]
[JsonSerializable(typeof(DocumentMessage))]
[JsonSerializable(typeof(EmptyArgs))]
[JsonSerializable(typeof(ReadyResult))]
[JsonSerializable(typeof(TextArgs))]
[JsonSerializable(typeof(TextResult))]
[JsonSerializable(typeof(PreferenceArgs))]
[JsonSerializable(typeof(SetPreferenceArgs))]
[JsonSerializable(typeof(PreferenceResult))]
[JsonSerializable(typeof(Ack))]
[JsonSerializable(typeof(TopicArgs))]
[JsonSerializable(typeof(CancelArgs))]
[JsonSerializable(typeof(EchoArgs))]
[JsonSerializable(typeof(EchoResult))]
[JsonSerializable(typeof(CounterResult))]
[JsonSerializable(typeof(WorkArgs))]
[JsonSerializable(typeof(ProgressResult))]
[JsonSerializable(typeof(IdentityResult))]
partial class PlaygroundJsonContext : JsonSerializerContext;

sealed class CommandException(string code, string message) : Exception(message)
{
	public string Code { get; } = code;
}

interface IPlaygroundPlatform
{
	Task<IdentityResult> GetIdentityAsync(CancellationToken cancellation);
	Task<TextResult> GetClipboardAsync(CancellationToken cancellation);
	Task<Ack> SetClipboardAsync(string text, CancellationToken cancellation);
	Task<PreferenceResult> GetPreferenceAsync(string key, CancellationToken cancellation);
	Task<PreferenceResult> SetPreferenceAsync(string key, string? value, CancellationToken cancellation);
}
