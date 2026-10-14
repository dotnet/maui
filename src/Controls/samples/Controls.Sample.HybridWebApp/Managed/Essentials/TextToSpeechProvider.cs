using System.Text.Json.Serialization;
using Microsoft.Maui.Media;

namespace Maui.HybridApp.Essentials.TextToSpeech;

/// <summary>Actual native speech/locales with host text budgets and per-operation native cancellation.</summary>
public sealed class TextToSpeechProvider(ITextToSpeech speech, NativeDispatcher dispatcher,
	HybridAppOptions policy) : IHybridCommandProvider
{
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = SpeechJsonContext.Default;
		registry.Register("speech.locales", json.SpeechEmpty, json.SpeechLocales, (_, op) =>
			dispatcher.RunAsync(async () => new SpeechLocales((await speech.GetLocalesAsync()).Select(locale =>
				new SpeechLocale(locale.Id, locale.Language, locale.Country, locale.Name)).ToArray()), op.Cancellation));
		registry.Register("speech.speak", json.SpeechArgs, json.SpeechCompleted, (args, op) =>
		{
			if (string.IsNullOrWhiteSpace(args.Text) || args.Text.Length > policy.MaximumTextLength)
				throw new CommandException("invalid_arguments", $"Speech requires nonblank text within the host's {policy.MaximumTextLength}-character UTF-16 budget.");
			var options = Options(args.Options);
			return dispatcher.RunAsync(async () =>
			{
				await speech.SpeakAsync(args.Text, options, op.Cancellation);
				return new SpeechCompleted(true);
			}, op.Cancellation);
		}, interactive: true);
	}

	static SpeechOptions? Options(SpeechOptionArgs? args)
	{
		if (args is null) return null;
		static void Range(float? value, float min, float max)
		{
			if (value is { } number && (!float.IsFinite(number) || number < min || number > max))
				throw new CommandException("invalid_arguments", "Speech pitch, volume or rate is outside its native range.");
		}
		Range(args.Pitch, 0, 2);
		Range(args.Volume, 0, 1);
		Range(args.Rate, 0.1f, 2);
		if (args.Locale is { } locale &&
			new[] { locale.Id, locale.Language, locale.Country, locale.Name }.Any(value => value.Length > 512))
			throw new CommandException("invalid_arguments", "Use bounded native speech locale values.");
		return new SpeechOptions
		{
			Pitch = args.Pitch, Volume = args.Volume, Rate = args.Rate,
			Locale = args.Locale is { } selected ? new Locale(selected.Language, selected.Country, selected.Name, selected.Id) : null,
		};
	}
}

sealed record SpeechEmpty;
sealed record SpeechLocale(string Id, string Language, string Country, string Name);
sealed record SpeechLocales(SpeechLocale[] Locales);
sealed record SpeechOptionArgs(SpeechLocale? Locale = null, float? Pitch = null, float? Volume = null, float? Rate = null);
sealed record SpeechArgs(string Text, SpeechOptionArgs? Options = null);
sealed record SpeechCompleted(bool Accepted);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(SpeechEmpty))]
[JsonSerializable(typeof(SpeechLocales))]
[JsonSerializable(typeof(SpeechArgs))]
[JsonSerializable(typeof(SpeechCompleted))]
partial class SpeechJsonContext : JsonSerializerContext;
