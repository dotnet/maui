#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// The TextToSpeech API enables an application to utilize the built-in text-to-speech engines to speak back text from the device and also to query available languages that the engine can support.
	/// </summary>
	public interface ITextToSpeech
	{
		/// <summary>
		/// Gets a list of languages supported by text-to-speech.
		/// </summary>
		/// <returns>A collection of <see cref="Locale"/> objects with languages supported by text-to-speech on this device.</returns>
		Task<IEnumerable<Locale>> GetLocalesAsync();

		/// <summary>
		/// Speaks the given text through the device's speech-to-text.
		/// </summary>
		/// <param name="text">The text to speak.</param>
		/// <param name="options">The options to use for speaking.</param>
		/// <param name="cancelToken">Optional cancellation token to stop speaking.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task SpeakAsync(string text, SpeechOptions? options = default, CancellationToken cancelToken = default);
	}

	/// <summary>
	/// The TextToSpeech API enables an application to utilize the built-in text-to-speech engines to speak back text from the device and also to query available languages that the engine can support.
	/// </summary>
	/// <remarks>When using this on Android targeting Android 11 (R API 30) you must update your Android Manifest with queries that are used with the new package visibility requirements. See the conceptual docs for more information.</remarks>
	public static partial class TextToSpeech
	{
		/// <summary>
		/// Gets a list of languages supported by text-to-speech.
		/// </summary>
		/// <returns>A collection of <see cref="Locale"/> objects with languages supported by text-to-speech on this device.</returns>
		public static Task<IEnumerable<Locale>> GetLocalesAsync() =>
			Default.GetLocalesAsync();

		/// <summary>
		/// Speaks the given text through the device's speech-to-text.
		/// </summary>
		/// <param name="text">The text to speak.</param>
		/// <param name="cancelToken">Optional cancellation token to stop speaking.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task SpeakAsync(string text, CancellationToken cancelToken = default) =>
			Default.SpeakAsync(text, default, cancelToken);

		/// <summary>
		/// Speaks the given text through the device's speech-to-text.
		/// </summary>
		/// <param name="text">The text to speak.</param>
		/// <param name="options">The options to use for speaking.</param>
		/// <param name="cancelToken">Optional cancellation token to stop speaking.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task SpeakAsync(string text, SpeechOptions? options, CancellationToken cancelToken = default) =>
			Default.SpeakAsync(text, options, cancelToken);

		static ITextToSpeech? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static ITextToSpeech Default =>
			defaultImplementation ??= new TextToSpeechImplementation();

		internal static void SetDefault(ITextToSpeech? implementation) =>
			defaultImplementation = implementation;

		internal static List<string> SplitSpeak(string text, int max)
		{
			var parts = new List<string>();
			if (text.Length <= max)
			{
				// no need to split
				parts.Add(text);
			}
			else
			{
				var positionbegin = 0;
				var positionend = max;
				var position = positionbegin;

				var p = string.Empty;
				while (position != text.Length)
				{
					while (positionend > positionbegin)
					{
						if (positionend >= text.Length)
						{
							// we just need the rest of it
							p = text.Substring(positionbegin, text.Length - positionbegin);
							parts.Add(p);
							return parts;
						}

						var ch = text[positionend];
						if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch))
						{
							p = text.Substring(positionbegin, positionend - positionbegin);
							break;
						}
						else if (positionend == positionbegin)
						{
							// no whitespace or punctuation found
							// grab the whole buffer (max)
							p = text.Substring(positionbegin, positionbegin + max);
							break;
						}

						positionend--;
					}

					Debug.WriteLine($"p             = {p}");
					Debug.WriteLine($"p.Length      = {p.Length}");
					Debug.WriteLine($"positionbegin = {positionbegin}");
					Debug.WriteLine($"positionend   = {positionend}");
					Debug.WriteLine($"position      = {position}");

					positionbegin = positionbegin + p.Length + 1;
					positionend = positionbegin + max;
					position = positionbegin;

					Debug.WriteLine($"------------------------------");
					Debug.WriteLine($"positionbegin = {positionbegin}");
					Debug.WriteLine($"positionend   = {positionend}");
					Debug.WriteLine($"position      = {position}");

					parts.Add(p);
				}
			}

			return parts;
		}
	}

	partial class TextToSpeechImplementation : ITextToSpeech
	{
		internal const float PitchMax = 2.0f;
		internal const float PitchDefault = 1.0f;
		internal const float PitchMin = 0.0f;

		internal const float VolumeMax = 1.0f;
		internal const float VolumeDefault = 0.5f;
		internal const float VolumeMin = 0.0f;

		internal const float RateMax = 2.0f;
		internal const float RateDefault = 1.0f;
		internal const float RateMin = 0.1f;

		SemaphoreSlim? semaphore;

		public Task<IEnumerable<Locale>> GetLocalesAsync() =>
			PlatformGetLocalesAsync();

		public async Task SpeakAsync(string text, SpeechOptions? options = default, CancellationToken cancelToken = default)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text), "Text cannot be null or empty string");

			if (options?.Volume.HasValue ?? false)
			{
				if (options.Volume.Value < VolumeMin || options.Volume.Value > VolumeMax)
					throw new ArgumentOutOfRangeException($"Volume must be >= {VolumeMin} and <= {VolumeMax}");
			}

			if (options?.Pitch.HasValue ?? false)
			{
				if (options.Pitch.Value < PitchMin || options.Pitch.Value > PitchMax)
					throw new ArgumentOutOfRangeException($"Pitch must be >= {PitchMin} and <= {PitchMin}");
			}

			if (options?.Rate.HasValue ?? false)
			{
				if (options.Rate.Value < RateMin || options.Rate.Value > RateMax)
					throw new ArgumentOutOfRangeException($"Rate must be >= {RateMin} and <= {RateMin}");
			}

			if (semaphore == null)
				semaphore = new SemaphoreSlim(1, 1);

			try
			{
				await semaphore.WaitAsync(cancelToken);
				await PlatformSpeakAsync(text, options, cancelToken);
			}
			finally
			{
				if (semaphore.CurrentCount == 0)
					semaphore.Release();
			}
		}
	}

	/// <summary>
	/// Represents a specific geographical, political, or cultural region.
	/// </summary>
	public class Locale
	{
		/// <summary>
		/// Gets the language name or code.
		/// </summary>
		/// <remarks>
		/// <para>This value may vary between platforms.</para>
		/// <para>
		/// For Android this used the ISO 639 alpha-2 or alpha-3 language code, or registered language subtags up to 8 alpha letters (for future enhancements).
		/// When a language has both an alpha-2 code and an alpha-3 code, the alpha-2 code must be used.
		/// </para>
		/// <para>For iOS and Windows this uses the BCP-47 language code.</para>
		/// </remarks>
		public string Language { get; }

		/// <summary>
		/// Gets the country name or code.
		/// </summary>
		/// <remarks>
		/// <para>This value may vary between platforms.</para>
		/// <para>For Android this used the ISO 3166 alpha-2 country code or UN M.49 numeric-3 area code.</para>
		/// <para>For iOS and Windows this field is not used and <see langword="null"/> .</para>
		/// </remarks>
		public string Country { get; }

		/// <summary>
		/// Gets the display name of the locale.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the unique identifier of the locale.
		/// </summary>
		public string Id { get; }

		internal Locale(string language, string country, string name, string id)
		{
			Language = language;
			Country = country;
			Name = name;
			Id = id;
		}
	}

	/// <summary>
	/// Represents options that can be used to influence the <see cref="ITextToSpeech"/> behavior.
	/// </summary>
	public class SpeechOptions
	{
		/// <summary>
		/// Gets or sets the locale to use with text-to-speech.
		/// </summary>
		/// <remarks>The <see cref="Locale.Language"/> property should match a <see cref="Locale.Language"/> value returned by <see cref="ITextToSpeech.GetLocalesAsync"/>.</remarks>
		public Locale? Locale { get; set; }

		/// <summary>
		/// The pitch to use when speaking.
		/// </summary>
		/// <remarks>This value should be between <c>0f</c> and <c>2.0f</c>.</remarks>
		public float? Pitch { get; set; }

		/// <summary>
		/// The volume to use when speaking.
		/// </summary>
		/// <remarks>This value should be between <c>0f</c> and <c>1.0f</c>.</remarks>
		public float? Volume { get; set; }

		/// <summary>
		/// The speech rate to use when speaking.
		/// </summary>
		/// <remarks>This value should be between <c>0.1f</c> and <c>2.0f</c>.</remarks>
		public float? Rate { get; set; }
	}
}
