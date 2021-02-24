using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Speech.Tts;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;
using Debug = System.Diagnostics.Debug;
using JavaLocale = Java.Util.Locale;

namespace Microsoft.Maui.Essentials
{
	public static partial class TextToSpeech
	{
		const int maxSpeechInputLengthDefault = 4000;

		static WeakReference<TextToSpeechImplementation> textToSpeechRef = null;

		static TextToSpeechImplementation GetTextToSpeech()
		{
			if (textToSpeechRef == null || !textToSpeechRef.TryGetTarget(out var tts))
			{
				tts = new TextToSpeechImplementation();
				textToSpeechRef = new WeakReference<TextToSpeechImplementation>(tts);
			}

			return tts;
		}

		internal static Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default)
		{
			var textToSpeech = GetTextToSpeech();

			if (textToSpeech == null)
				throw new PlatformNotSupportedException("Unable to start text-to-speech engine, not supported on device.");

			var max = maxSpeechInputLengthDefault;
			if (Platform.HasApiLevel(BuildVersionCodes.JellyBeanMr2))
				max = AndroidTextToSpeech.MaxSpeechInputLength;

			return textToSpeech.SpeakAsync(text, max, options, cancelToken);
		}

		internal static Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
		{
			var textToSpeech = GetTextToSpeech();

			if (textToSpeech == null)
				throw new PlatformNotSupportedException("Unable to start text-to-speech engine, not supported on device.");

			return textToSpeech.GetLocalesAsync();
		}
	}

	class TextToSpeechImplementation : Java.Lang.Object, AndroidTextToSpeech.IOnInitListener,
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
				tts = new AndroidTextToSpeech(Platform.AppContext, this);
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

			if (cancelToken != null)
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

			if (options?.Pitch.HasValue ?? false)
				tts.SetPitch(options.Pitch.Value);
			else
				tts.SetPitch(TextToSpeech.PitchDefault);

			tts.SetSpeechRate(1.0f);

			var parts = text.SplitSpeak(max);

			numExpectedUtterances = parts.Count;
			tcsUtterances = new TaskCompletionSource<bool>();

			var guid = Guid.NewGuid().ToString();

			for (var i = 0; i < parts.Count && !cancelToken.IsCancellationRequested; i++)
			{
				// We require the utterance id to be set if we want the completed listener to fire
				var map = new Dictionary<string, string>
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

			if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
			{
				try
				{
					return tts.AvailableLanguages.Select(a => new Locale(a.Language, a.Country, a.DisplayName, string.Empty));
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to query language on new API, attempting older api: " + ex);
				}
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
			if (Platform.HasApiLevel(BuildVersionCodes.JellyBeanMr2))
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
			else
			{
				if (tts.Language != null)
					tts.SetLanguage(tts.Language);
			}
		}
#pragma warning restore 0618
	}
}
