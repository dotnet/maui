using Microsoft.Maui.Media;

namespace Maui.HybridApp.Essentials.TextToSpeech;

/// <summary>Actual native speech/locales with host text budgets and per-operation native cancellation.</summary>
public sealed class TextToSpeechProvider(ITextToSpeech speech, HybridAppOptions policy)
{
	[HybridAction("speech.locales", MainThread = true)]
	internal async Task<SpeechLocales> Locales() =>
		new((await speech.GetLocalesAsync()).ToArray());

	[HybridAction("speech.speak", MainThread = true, Interactive = true)]
	internal async Task<SpeechCompleted> Speak(string text, CancellationToken cancellation, SpeechOptions? options = null)
	{
		if (string.IsNullOrWhiteSpace(text) || text.Length > policy.MaximumTextLength)
			throw new CommandException("invalid_arguments", $"Speech requires nonblank text within the host's {policy.MaximumTextLength}-character UTF-16 budget.");
		await speech.SpeakAsync(text, Options(options), cancellation);
		return new(true);
	}

	static SpeechOptions? Options(SpeechOptions? args)
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
		return args;
	}
}

sealed record SpeechLocales(Locale[] Locales);
sealed record SpeechCompleted(bool Accepted);
