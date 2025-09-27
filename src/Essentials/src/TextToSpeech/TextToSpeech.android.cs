using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Speech.Tts;
using Microsoft.Maui.ApplicationModel;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;
using Debug = System.Diagnostics.Debug;
using JavaLocale = Java.Util.Locale;

namespace Microsoft.Maui.Media
{
	partial class TextToSpeechImplementation : ITextToSpeech
	{
		const int maxSpeechInputLengthDefault = 4000;

		WeakReference<TextToSpeechInternalImplementation> textToSpeechRef = null;

		TextToSpeechInternalImplementation GetTextToSpeech()
		{
			if (textToSpeechRef == null || !textToSpeechRef.TryGetTarget(out var tts))
			{
				tts = new TextToSpeechInternalImplementation();
				textToSpeechRef = new WeakReference<TextToSpeechInternalImplementation>(tts);
			}

			return tts;
		}

		Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken)
		{
			var textToSpeech = GetTextToSpeech();
			if (textToSpeech == null)
				throw new PlatformNotSupportedException("Unable to start text-to-speech engine, not supported on device.");

			var max = maxSpeechInputLengthDefault;
			if (OperatingSystem.IsAndroidVersionAtLeast(18))
				max = AndroidTextToSpeech.MaxSpeechInputLength;

			return textToSpeech.SpeakAsync(text, max, options, cancelToken);
		}

		Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
		{
			var textToSpeech = GetTextToSpeech();
			if (textToSpeech == null)
				throw new PlatformNotSupportedException("Unable to start text-to-speech engine, not supported on device.");

			return textToSpeech.GetLocalesAsync();
		}
	}

	class TextToSpeechInternalImplementation : Java.Lang.Object, AndroidTextToSpeech.IOnInitListener,
#pragma warning disable CS0618
		AndroidTextToSpeech.IOnUtteranceCompletedListener
#pragma warning restore CS0618
	{
		AndroidTextToSpeech tts;
		TaskCompletionSource<bool> tcsInitialize;
		TaskCompletionSource<bool> tcsUtterances;

		Task<bool> Initialize()
		{
			if (tcsInitialize != null && tts != null)
				return tcsInitialize.Task;

			tcsInitialize = new TaskCompletionSource<bool>();
			try
			{
				// set up the TextToSpeech object
				tts = new AndroidTextToSpeech(Application.Context, this);
#pragma warning disable CS0618
				tts.SetOnUtteranceCompletedListener(this);
#pragma warning restore CS0618

			}
			catch (Exception e)
			{
				tcsInitialize.TrySetException(e);
			}

			return tcsInitialize.Task;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				tts?.Stop();
				tts?.Shutdown();
				tts = null;
			}

			base.Dispose(disposing);
		}

		int numExpectedUtterances = 0;
		int numCompletedUtterances = 0;

		public async Task SpeakAsync(string text, int max, SpeechOptions options, CancellationToken cancelToken)
		{
			await Initialize();

			// Wait for any previous calls to finish up
			if (tcsUtterances?.Task != null)
				await tcsUtterances.Task;

			tcsUtterances = new TaskCompletionSource<bool>();

			if (cancelToken != default)
			{
				cancelToken.Register(() =>
				{
					try
					{
						tts?.Stop();

						tcsUtterances?.TrySetResult(true);
					}
					catch
					{
					}
				});
			}

			if (options?.Locale?.Language != null)
			{
				JavaLocale locale = null;
				if (!string.IsNullOrWhiteSpace(options?.Locale.Country))
					locale = new JavaLocale(options.Locale.Language, options.Locale.Country);
				else
					locale = new JavaLocale(options.Locale.Language);

				tts.SetLanguage(locale);
			}
			else
			{
				SetDefaultLanguage();
			}
			// Set the voice if specified, otherwise use the voice that based on the selected/default language
			if (!string.IsNullOrEmpty(options?.Locale?.Id) && tts?.Voices is not null)
			{
				var voice = tts.Voices.FirstOrDefault(v => v.Name == options.Locale.Id);
				if (voice is not null)
				{
					tts.SetVoice(voice);
				}
			}

			if (options?.Pitch.HasValue ?? false)
				tts.SetPitch(options.Pitch.Value);
			else
				tts.SetPitch(TextToSpeechImplementation.PitchDefault);

			if (options?.Rate.HasValue ?? false)
				tts.SetSpeechRate((float)options.Rate);
			else
				tts.SetSpeechRate(1.0f);

			var parts = TextToSpeech.SplitSpeak(text, max);

			numExpectedUtterances = parts.Count;

			var guid = Guid.NewGuid().ToString();

			for (var i = 0; i < parts.Count && !cancelToken.IsCancellationRequested; i++)
			{
				// We require the utterance id to be set if we want the completed listener to fire
				var map = new Dictionary<string, string>(StringComparer.Ordinal)
				{
					{ AndroidTextToSpeech.Engine.KeyParamUtteranceId, $"{guid}.{i}" }
				};

				if (options != null && options.Volume.HasValue)
					map.Add(AndroidTextToSpeech.Engine.KeyParamVolume, options.Volume.Value.ToString(CultureInfo.InvariantCulture));

				// We use an obsolete overload here so it works on older API levels at runtime
				// Flush on first entry and add (to not flush our own previous) subsequent entries
#pragma warning disable CS0618
				tts.Speak(parts[i], i == 0 ? QueueMode.Flush : QueueMode.Add, map);
#pragma warning restore CS0618
			}

			await tcsUtterances.Task;
		}

		public void OnInit(OperationResult status)
		{
			if (status == OperationResult.Success)
				tcsInitialize.TrySetResult(true);
			else
				tcsInitialize.TrySetException(new ArgumentException("Failed to initialize Text to Speech engine."));
		}

		public async Task<IEnumerable<Locale>> GetLocalesAsync()
		{
			await Initialize();

			try
			{
				// Attempt to use the new API to get the voices and their locales, a.Name is the voice's identifier
				return tts.Voices.Select(a => new Locale(a.Locale.Language, a.Locale.Country, a.Locale.DisplayName, a.Name));
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to query language on new API, attempting older api: " + ex);
			}

			return JavaLocale.GetAvailableLocales()
				.Where(IsLocaleAvailable)
				.Select(l => new Locale(l.Language, l.Country, l.DisplayName, string.Empty))
				.GroupBy(c => c.ToString())
				.Select(g => g.First());
		}

		bool IsLocaleAvailable(JavaLocale l)
		{
			try
			{
				var r = tts.IsLanguageAvailable(l);
				return
					r == LanguageAvailableResult.Available ||
					r == LanguageAvailableResult.CountryAvailable ||
					r == LanguageAvailableResult.CountryVarAvailable;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error checking language; " + l + " " + ex);
			}
			return false;
		}

		public void OnUtteranceCompleted(string utteranceId)
		{
			numCompletedUtterances++;
			if (numCompletedUtterances >= numExpectedUtterances)
				tcsUtterances?.TrySetResult(true);
		}

#pragma warning disable 0618
		void SetDefaultLanguage()
		{
			try
			{
				if (tts.DefaultLanguage == null && tts.Language != null)
					tts.SetLanguage(tts.Language);
				else if (tts.DefaultLanguage != null)
					tts.SetLanguage(tts.DefaultLanguage);
			}
			catch
			{
				if (tts.Language != null)
					tts.SetLanguage(tts.Language);
			}
		}
#pragma warning restore 0618
	}
}
