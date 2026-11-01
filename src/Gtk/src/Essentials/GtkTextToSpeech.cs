using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="ITextToSpeech"/> over the platform speech engine: speech-dispatcher (<c>libspeechd.so.2</c>) on Linux, SAPI
/// (<c>SAPI.SpVoice</c>) on Windows. Without an engine every call fails with <see cref="FeatureNotSupportedException"/>.
/// </summary>
/// <remarks>
/// Like the other platforms, utterances are queued: a second <c>SpeakAsync</c> waits for the first. Cancelling the token
/// stops the running utterance (or removes a queued one) and cancels the task. Needs no GTK main loop.
/// </remarks>
sealed class GtkTextToSpeech : ITextToSpeech
{
	const float PitchMin = 0f, PitchMax = 2f, VolumeMin = 0f, VolumeMax = 1f, RateMin = 0.1f, RateMax = 2f;

	readonly Lazy<ISpeechBackend?> backend;
	readonly SemaphoreSlim queue = new(1, 1);

	/// <param name="factory">Creates the engine on first use (a fake in tests); <see langword="null"/> binds the platform engine.</param>
	internal GtkTextToSpeech(Func<ISpeechBackend?>? factory = null)
	{
		this.backend = new Lazy<ISpeechBackend?>(factory ?? SpeechBackends.TryCreate);
	}

	ISpeechBackend Backend =>
		backend.Value ?? throw new FeatureNotSupportedException("No speech engine (speech-dispatcher / SAPI) is available.");

	/// <summary>Whether a speech engine could be bound.</summary>
	internal bool IsSupported => backend.Value is not null;

	public Task<IEnumerable<Locale>> GetLocalesAsync()
	{
		ISpeechBackend engine;
		try
		{
			engine = Backend;
		}
		catch (Exception ex)
		{
			return Task.FromException<IEnumerable<Locale>>(ex);
		}

		return Task.Run(() => (IEnumerable<Locale>)engine.GetVoices()
			.Select(v => new Locale(v.Language, null!, v.Name, v.Id))
			.ToList());
	}

	public async Task SpeakAsync(string text, SpeechOptions? options = default, CancellationToken cancelToken = default)
	{
		if (string.IsNullOrEmpty(text))
			throw new ArgumentNullException(nameof(text), "Text cannot be null or empty string");

		if (options?.Volume is float volume && (volume < VolumeMin || volume > VolumeMax))
			throw new ArgumentOutOfRangeException(nameof(options), $"Volume must be >= {VolumeMin} and <= {VolumeMax}");

		if (options?.Pitch is float pitch && (pitch < PitchMin || pitch > PitchMax))
			throw new ArgumentOutOfRangeException(nameof(options), $"Pitch must be >= {PitchMin} and <= {PitchMax}");

		if (options?.Rate is float rate && (rate < RateMin || rate > RateMax))
			throw new ArgumentOutOfRangeException(nameof(options), $"Rate must be >= {RateMin} and <= {RateMax}");

		var engine = Backend;
		await queue.WaitAsync(cancelToken).ConfigureAwait(false);
		try
		{
			cancelToken.ThrowIfCancellationRequested();

			SpeechVoice? voice = null;
			string? language = null;
			if (options?.Locale is { } locale)
			{
				voice = SpeechScale.Match(engine.GetVoices(), locale.Id, locale.Language);
				if (voice is null && !string.IsNullOrEmpty(locale.Language))
					language = locale.Language;
			}

			await engine.SpeakAsync(new SpeechRequest(text, voice, language, options?.Rate, options?.Pitch, options?.Volume), cancelToken)
				.ConfigureAwait(false);
			cancelToken.ThrowIfCancellationRequested();
		}
		finally
		{
			queue.Release();
		}
	}
}

/// <summary>Picks the speech engine of the running OS.</summary>
static class SpeechBackends
{
	/// <summary>The engine of this OS, or <see langword="null"/> when none is installed.</summary>
	internal static ISpeechBackend? TryCreate()
	{
		if (OperatingSystem.IsLinux())
			return LibSpeechd.TryCreate();

		if (OperatingSystem.IsWindows())
			return SapiSpeech.TryCreate();

		return null;
	}
}
