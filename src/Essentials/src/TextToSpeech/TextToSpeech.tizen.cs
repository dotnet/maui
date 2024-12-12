using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tizen.Uix.Tts;

namespace Microsoft.Maui.Media
{
	partial class TextToSpeechImplementation : ITextToSpeech
	{
		TtsClient tts = null;
		TaskCompletionSource<bool> tcsInitialize = null;
		TaskCompletionSource<bool> tcsUtterances = null;

		async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default)
		{
			await Initialize();

			if (tcsUtterances?.Task != null)
				await tcsUtterances.Task;

			tcsUtterances = new TaskCompletionSource<bool>();

			cancelToken.Register(() =>
			{
				tts?.Stop();
				tcsUtterances?.TrySetResult(true);
			});

			var language = "en_US";
			var voiceType = Voice.Auto;
			if (options?.Locale.Language != null)
			{
				foreach (var voice in tts.GetSupportedVoices())
				{
					if (voice.Language == options.Locale.Language)
					{
						language = voice.Language;
						voiceType = voice.VoiceType;
					}
				}
			}

			var rate = 0;
			if (options?.Rate.HasValue ?? false)
				rate = (int)Math.Round(options.Rate.Value / RateMax * tts.GetSpeedRange().Max, MidpointRounding.AwayFromZero);

			tts.AddText(text, language, (int)voiceType, rate);
			tts.Play();

			await tcsUtterances.Task;
		}

		async Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
		{
			await Initialize();
			var list = new List<Locale>();
			foreach (var voice in tts?.GetSupportedVoices())
				list.Add(new Locale(voice.Language, null, null, null));
			return list;
		}

		Task<bool> Initialize()
		{
			if (tcsInitialize != null && tts != null)
				return tcsInitialize.Task;

			tcsInitialize = new TaskCompletionSource<bool>();
			tts = new TtsClient();

			tts.StateChanged += (s, e) =>
			{
				if (e.Current == State.Ready)
					tcsInitialize?.TrySetResult(true);
			};

			tts.UtteranceCompleted += (s, e) =>
			{
				tts?.Stop();
				tcsUtterances?.TrySetResult(true);
			};

			tts.Prepare();
			return tcsInitialize.Task;
		}
	}
}
