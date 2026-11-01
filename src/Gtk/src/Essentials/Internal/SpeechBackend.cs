using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>A synthesis voice reported by a speech engine; surfaces as a <c>Locale</c>.</summary>
/// <param name="Language">BCP-47 style language tag (<c>en-GB</c>), or empty when unknown.</param>
/// <param name="Name">Display name.</param>
/// <param name="Id">Engine-specific identifier that selects exactly this voice.</param>
readonly record struct SpeechVoice(string Language, string Name, string Id);

/// <summary>One utterance: the text, the chosen voice and the unconverted MAUI option values (<c>null</c> = engine default).</summary>
/// <param name="Text">Text to speak.</param>
/// <param name="Voice">Voice to use, or <see langword="null"/> for the engine default.</param>
/// <param name="Language">Requested language when no installed voice matched, or <see langword="null"/>.</param>
/// <param name="Rate">MAUI rate, 0.1 to 2 (1 = normal).</param>
/// <param name="Pitch">MAUI pitch, 0 to 2 (1 = normal).</param>
/// <param name="Volume">MAUI volume, 0 to 1.</param>
readonly record struct SpeechRequest(string Text, SpeechVoice? Voice, string? Language, float? Rate, float? Pitch, float? Volume);

/// <summary>A speech engine (speech-dispatcher on Linux, SAPI on Windows); abstracted so it can be faked.</summary>
interface ISpeechBackend
{
	/// <summary>The installed voices.</summary>
	IReadOnlyList<SpeechVoice> GetVoices();

	/// <summary>
	/// Speaks <paramref name="request"/>; completes when the engine finished. When <paramref name="cancellationToken"/> is
	/// signalled the engine is stopped and the task is cancelled.
	/// </summary>
	Task SpeakAsync(SpeechRequest request, CancellationToken cancellationToken);
}

/// <summary>Conversions from MAUI speech option ranges to the ranges of each engine, and voice matching.</summary>
static class SpeechScale
{
	/// <summary>Maps a MAUI rate (0.1 to 2, 1 normal) onto <c>-max..max</c>, 1 mapping to 0 and 0.1 to <c>-max</c>.</summary>
	internal static int Rate(float rate, int max)
	{
		var scaled = rate <= 1f ? (rate - 1f) / 0.9f : rate - 1f;
		return Clamp((int)Math.Round(scaled * max), max);
	}

	/// <summary>Maps a MAUI pitch (0 to 2, 1 normal) onto <c>-max..max</c>.</summary>
	internal static int Pitch(float pitch, int max) =>
		Clamp((int)Math.Round((pitch - 1f) * max), max);

	/// <summary>Maps a MAUI volume (0 to 1) onto speech-dispatcher's <c>-100..100</c> (0.5 = the engine's normal level).</summary>
	internal static int SsipVolume(float volume) =>
		Clamp((int)Math.Round((volume * 2f - 1f) * 100f), 100);

	/// <summary>Maps a MAUI volume (0 to 1) onto SAPI's <c>0..100</c>.</summary>
	internal static int SapiVolume(float volume) =>
		Math.Clamp((int)Math.Round(volume * 100f), 0, 100);

	static int Clamp(int value, int max) => Math.Clamp(value, -max, max);

	/// <summary>
	/// Picks the voice for a requested locale: an exact <see cref="SpeechVoice.Id"/> match first, then the same language tag,
	/// then the same primary language (<c>en</c> for <c>en-GB</c>). Returns <see langword="null"/> when none matches.
	/// </summary>
	internal static SpeechVoice? Match(IReadOnlyList<SpeechVoice> voices, string? id, string? language)
	{
		if (!string.IsNullOrEmpty(id))
		{
			foreach (var voice in voices)
			{
				if (string.Equals(voice.Id, id, StringComparison.OrdinalIgnoreCase))
					return voice;
			}
		}

		if (string.IsNullOrEmpty(language))
			return null;

		var tag = Normalize(language!);
		foreach (var voice in voices)
		{
			if (string.Equals(Normalize(voice.Language), tag, StringComparison.OrdinalIgnoreCase))
				return voice;
		}

		var primary = Primary(tag);
		foreach (var voice in voices)
		{
			if (string.Equals(Primary(Normalize(voice.Language)), primary, StringComparison.OrdinalIgnoreCase))
				return voice;
		}

		return null;
	}

	internal static string Normalize(string language) => language.Replace('_', '-');

	static string Primary(string tag)
	{
		var dash = tag.IndexOf('-', StringComparison.Ordinal);
		return dash < 0 ? tag : tag.Substring(0, dash);
	}
}
